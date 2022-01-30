// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Engine.Tuning;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Evaluation;
using ErikTheCoder.MadChess.Engine.Hashtable;
using ErikTheCoder.MadChess.Engine.Intelligence;
using ErikTheCoder.MadChess.Engine.Score;


namespace ErikTheCoder.MadChess.Engine.Uci;


public sealed class UciStream : IDisposable
{
    public const long NodesInfoInterval = 1_000_000;
    public const long NodesTimeInterval = 1_000;
    private string[] _defaultPlyAndFullMove;
    private const int _cacheSizeMegabytes = 128;
    private const int _minWinScale = 400;
    private const int _maxWinScale = 800;
    private readonly TimeSpan _maxStopTime = TimeSpan.FromMilliseconds(500);
    private Board _board;
    private Stats _stats;
    private Cache _cache;
    private KillerMoves _killerMoves;
    private MoveHistory _moveHistory;
    private Eval _eval;
    private Search _search;
    private bool _debug;
    private Stopwatch _stopwatch;
    private Stopwatch _commandStopwatch;
    private Queue<List<string>> _asyncQueue;
    private Thread _asyncThread;
    private AutoResetEvent _asyncSignal;
    private object _asyncLock;
    private StreamWriter _logWriter;
    private object _messageLock;
    private bool _disposed;


    private bool Log
    {
        get => _logWriter != null;
        set
        {
            if (value)
            {
                // Start logging.
                if (_logWriter == null)
                {
                    // Create or append to log file.
                    // Include GUID in log filename to avoid multiple engine instances interleaving lines in a single log file.
                    var file = $"MadChess-{Guid.NewGuid()}.log";
                    var fileStream = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.Read);
                    _logWriter = new StreamWriter(fileStream) { AutoFlush = true };
                }
            }
            else
            {
                // Stop logging.
                _logWriter?.Close();
                _logWriter?.Dispose();
                _logWriter = null;
            }
        }
    }

    public UciStream()
    {
        // Create diagnostic and synchronization objects.
        _stopwatch = Stopwatch.StartNew();
        _commandStopwatch = new Stopwatch();
        _asyncQueue = new Queue<List<string>>();
        _asyncSignal = new AutoResetEvent(false);
        _asyncLock = new object();
        _messageLock = new object();
        // Create game objects.
        // Cannot use object initializer because it changes order of object construction (to PreCalculatedMoves first, Board second, which causes null reference in PrecalculatedMove.FindMagicMultipliers).
        // ReSharper disable once UseObjectOrCollectionInitializer
        _board = new Board(WriteMessageLine, NodesInfoInterval);
        _stats = new Stats();
        _cache = new Cache(_cacheSizeMegabytes, _stats, _board.ValidateMove);
        _killerMoves = new KillerMoves(Search.MaxHorizon + Search.MaxQuietDepth);
        _moveHistory = new MoveHistory();
        _eval = new Eval(_stats, _board.IsRepeatPosition, () => _debug, WriteMessageLine);
        _search = new Search(_stats, _cache, _killerMoves, _moveHistory, _eval, () => _debug, DisplayStats, WriteMessageLine);
        _defaultPlyAndFullMove = new[] { "0", "1" };
        _board.SetPosition(Board.StartPositionFen);
    }


    ~UciStream()
    {
        Dispose(false);
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            // Release managed resources.
            _board = null;
            _stats = null;
            _cache = null;
            _killerMoves = null;
            _moveHistory = null;
            _eval = null;
            _defaultPlyAndFullMove = null;
            lock (_messageLock) { _stopwatch = null; }
            _commandStopwatch = null;
            lock (_asyncLock) { _asyncQueue = null; }
            _asyncThread = null;
            _asyncLock = null;
            _messageLock = null;
        }
        // Release unmanaged resources.
        _search?.Dispose();
        _search = null;
        _logWriter?.Dispose();
        _logWriter = null;
        _asyncSignal?.Dispose();
        _asyncSignal = null;
        _disposed = true;
    }


    public void Run()
    {
        // Create async thread.
        _asyncThread = new Thread(MonitorQueue) { Name = "UCI Asynchronous", IsBackground = true };
        _asyncThread.Start();
        // Monitor input stream.
        Thread.CurrentThread.Name = "UCI Synchronous";
        MonitorInputStream();
    }


    public void HandleException(Exception exception)
    {
        Log = true;
        var stringBuilder = new StringBuilder();
        var ex = exception;
        do
        {
            // Display message and write to log.
            stringBuilder.AppendLine($"Exception Message = {ex.Message}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Exception Type = {ex.GetType().FullName}.");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Exception StackTrace = {ex.StackTrace}");
            stringBuilder.AppendLine();
            ex = ex.InnerException;
        } while (ex != null);
        WriteMessageLine(stringBuilder.ToString());
        Quit(-1);
    }


    private void MonitorInputStream()
    {
        try
        {
            string command;
            do
            {
                // Read command.
                command = Console.ReadLine();
                if (Log) WriteMessageLine(command, CommandDirection.In);
                // Dispatch command.
                DispatchCommand(command);
            } while (command != null);
        }
        catch (Exception exception)
        {
            HandleException(exception);
        }
    }


    private void DispatchCommand(string command)
    {
        if (command == null) return;
        // Parse command into tokens.
        var tokens = Tokens.Parse(command, ' ', '"');
        // Do not convert to lowercase because this invalidates FEN strings (where case differentiates white and black pieces).
        if (tokens.Count == 0) return;
        // Determine whether to dispatch command on main thread or async thread.
        switch (tokens[0].ToLower())
        {
            case "go":
                DispatchOnMainThread(tokens);
                DispatchOnAsyncThread(tokens);
                break;
            default:
                DispatchOnMainThread(tokens);
                break;
        }
    }


    // TODO: Add "resetstats" UCI command.
    private void DispatchOnMainThread(List<string> tokens)
    {
        var writeMessageLine = true;
        switch (tokens[0].ToLower())
        {
            // Standard Commands
            case "uci":
                Uci();
                break;
            case "isready":
                WriteMessageLine("readyok");
                break;
            case "debug":
                _debug = tokens[1].Equals("on", StringComparison.OrdinalIgnoreCase);
                break;
            case "setoption":
                SetOption(tokens);
                break;
            case "ucinewgame":
                UciNewGame();
                break;
            case "position":
                Position(tokens);
                break;
            case "go":
                GoSync(tokens);
                writeMessageLine = false;
                break;
            case "stop":
                Stop();
                break;
            case "quit":
                Quit(0);
                break;
            // Extended Commands
            case "showboard":
                WriteMessageLine(_board.ToString());
                break;
            case "findmagics":
                FindMagicMultipliers();
                break;
            case "countmoves":
                CountMoves(tokens);
                break;
            case "dividemoves":
                DivideMoves(tokens);
                break;
            case "listmoves":
                ListMoves();
                break;
            case "shiftkillermoves":
                _killerMoves.Shift(int.Parse(tokens[1]));
                break;
            case "showevalparams":
                WriteMessageLine(_eval.ShowParameters());
                break;
            case "staticscore":
                WriteMessageLine(_eval.ToString(_board.CurrentPosition));
                break;
            case "exchangescore":
                ExchangeScore(tokens);
                break;
            case "testpositions":
                TestPositions(tokens);
                break;
            case "analyzepositions":
                AnalyzePositions(tokens);
                break;
            case "analyzeexchangepositions":
                AnalyzeExchangePositions(tokens);
                break;
            case "exportquietpositions":
                ExportQuietPositions(tokens);
                break;
            case "tune":
                Tune(tokens);
                break;
            case "tunewinscale":
                TuneWinScale(tokens);
                break;
            case "?":
            case "help":
                Help();
                break;
            default:
                WriteMessageLine(tokens[0] + " command not supported.");
                break;
        }
        if (writeMessageLine) WriteMessageLine();
    }


    private void DispatchOnAsyncThread(List<string> tokens)
    {
        lock (_asyncLock)
        {
            // Queue command.
            _asyncQueue.Enqueue(tokens);
            // Signal async queue.
            _asyncSignal.Set();
        }
    }


    private void MonitorQueue()
    {
        try
        {
            do
            {
                // Wait for signal.
                _asyncSignal.WaitOne();
                List<string> tokens = null;
                lock (_asyncLock)
                {
                    if (_asyncQueue.Count > 0) tokens = _asyncQueue.Dequeue();
                }
                if ((tokens != null) && (tokens.Count > 0))
                {
                    // Process command.
                    switch (tokens[0].ToLower())
                    {
                        case "go":
                            GoAsync();
                            break;
                        default:
                            throw new Exception($"Cannot process {tokens[0]} command on asynchronous thread.");
                    }
                    WriteMessageLine();
                }
            } while (true);
        }
        catch (Exception exception)
        {
            HandleException(exception);
        }
    }


    private void Uci()
    {
        // Display engine name, author, and standard commands.
        // ReSharper disable once ConvertToConstant.Local
        var version = "3.1";
#if CPU64
        version = $"{version} x64";
#else
            version = $"{version} x86";
#endif
        WriteMessageLine($"id name MadChess {version}");
        WriteMessageLine("id author Erik Madsen");
        WriteMessageLine("option name UCI_EngineAbout type string default MadChess by Erik Madsen.  See https://www.madchess.net.");
        WriteMessageLine("option name Debug type check default false");
        WriteMessageLine("option name Log type check default false");
        WriteMessageLine("option name Hash type spin default 128 min 0 max 2048");
        WriteMessageLine("option name ClearHash type button");
        WriteMessageLine("option name UCI_AnalyseMode type check default false");
        WriteMessageLine($"option name MultiPV type spin default 1 min 1 max {Core.Game.Position.MaxMoves}");
        WriteMessageLine("option name UCI_LimitStrength type check default false");
        WriteMessageLine($"option name UCI_Elo type spin default {Search.MinElo} min {Search.MinElo} max {Search.MaxElo}");
        WriteMessageLine("uciok");
    }


    private void SetOption(List<string> tokens)
    {
        var optionName = tokens[2];
        var optionValue = tokens.Count > 4 ? tokens[4] : string.Empty;
        switch (optionName.ToLower())
        {
            case "debug":
                _debug = optionValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case "log":
                Log = optionValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case "hash":
                var cacheSizeMegabytes = int.Parse(optionValue);
                _cache.Capacity = cacheSizeMegabytes * Cache.CapacityPerMegabyte;
                break;
            case "clearhash":
                // Reset cache and move heuristics.
                _cache.Reset();
                _killerMoves.Reset();
                _moveHistory.Reset();
                break;
            case "uci_analysemode":
            case "analyze":
                var analysisMode = optionValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                if (analysisMode)
                {
                    _search.AllowedToTruncatePv = false;
                    _eval.DrawMoves = 3;
                }
                else
                {
                    _search.AllowedToTruncatePv = true;
                    _eval.DrawMoves = 2;
                }
                break;
            case "multipv":
                Stop();
                _search.MultiPv = int.Parse(optionValue);
                break;
            case "uci_limitstrength":
                _search.LimitedStrength = optionValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case "uci_elo":
                _search.Elo = int.Parse(optionValue);
                break;
            default:
                WriteMessageLine(optionName + " option not supported.");
                break;
        }
    }


    private void UciNewGame(bool preserveMoveCount = false)
    {
        // Reset cache and move heuristics.
        _cache.Reset();
        _killerMoves.Reset();
        _moveHistory.Reset();
        // Set up start position.
        _board.SetPosition(Board.StartPositionFen, preserveMoveCount);
    }


    private void Position(List<string> tokens)
    {
        // ParseLongAlgebraic FEN.
        // Determine if position specifies moves.
        var specifiesMoves = false;
        var moveIndex = tokens.Count;
        for (var index = 2; index < tokens.Count; index++)
        {
            if (tokens[index].ToLower() == "moves")
            {
                // Position specifies moves.
                specifiesMoves = true;
                if (!char.IsNumber(tokens[index - 1][0]))
                {
                    // Position does not specify ply or full move number.
                    tokens.InsertRange(index, _defaultPlyAndFullMove);
                    index += 2;
                }
                if (index == tokens.Count - 1) tokens.RemoveAt(tokens.Count - 1);
                moveIndex = index + 1;
                break;
            }
        }
        if (!specifiesMoves)
        {
            if (!char.IsNumber(tokens[tokens.Count - 1][0]))
            {
                // Position does not specify ply or full move number.
                tokens.AddRange(_defaultPlyAndFullMove);
                moveIndex += 2;
            }
        }
        // Must convert tokens to array to prevent joining class name (System.Collections.Generic.List) instead of string value.
        // This is because the IEnumerable<T> overload does not accept a StartIndex and Count so those parameters are interpreted as params object[].
        var fen = tokens[1] == "startpos"
            ? Board.StartPositionFen
            : string.Join(" ", tokens.ToArray(), 2, tokens.Count - 2);
        // Setup position and play moves if specified.
        _board.SetPosition(fen);
        while (moveIndex < tokens.Count)
        {
            var move = Move.ParseLongAlgebraic(tokens[moveIndex], _board.CurrentPosition.ColorToMove);
            var validMove = _board.ValidateMove(ref move);
            if (!validMove || !_board.IsMoveLegal(ref move)) throw new Exception($"Move {Move.ToLongAlgebraic(move)} is illegal in position {_board.CurrentPosition.ToFen()}.");
            _board.PlayMove(move);
            moveIndex++;
        }
    }


    private void GoSync(List<string> tokens)
    {
        _commandStopwatch.Restart();
        // Reset stats and search and shift killer moves.
        _stats.Reset();
        _search.Reset();
        _killerMoves.Shift(2);
        for (var tokenIndex = 1; tokenIndex < tokens.Count; tokenIndex++)
        {
            var token = tokens[tokenIndex];
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (token.ToLower())
            {
                case "searchmoves":
                    // Assume all remaining tokens are moves.
                    for (var moveIndex = tokenIndex + 1; moveIndex < tokens.Count; moveIndex++)
                    {
                        var move = Move.ParseLongAlgebraic(tokens[moveIndex], _board.CurrentPosition.ColorToMove);
                        _search.SpecifiedMoves.Add(move);
                    }
                    break;
                case "wtime":
                    _search.TimeRemaining[(int)Color.White] = TimeSpan.FromMilliseconds(int.Parse(tokens[tokenIndex + 1]));
                    break;
                case "btime":
                    _search.TimeRemaining[(int)Color.Black] = TimeSpan.FromMilliseconds(int.Parse(tokens[tokenIndex + 1]));
                    break;
                case "winc":
                    _search.TimeIncrement[(int)Color.White] = TimeSpan.FromMilliseconds(int.Parse(tokens[tokenIndex + 1]));
                    break;
                case "binc":
                    _search.TimeIncrement[(int)Color.Black] = TimeSpan.FromMilliseconds(int.Parse(tokens[tokenIndex + 1]));
                    break;
                case "movestogo":
                    _search.MovesToTimeControl = int.Parse(tokens[tokenIndex + 1]);
                    break;
                case "depth":
                    _search.HorizonLimit = Math.Min(int.Parse(tokens[tokenIndex + 1]), Search.MaxHorizon);
                    _search.CanAdjustMoveTime = false;
                    break;
                case "nodes":
                    _search.NodeLimit = long.Parse(tokens[tokenIndex + 1]);
                    _search.CanAdjustMoveTime = false;
                    break;
                case "mate":
                    _search.MateInMoves = int.Parse(tokens[tokenIndex + 1]);
                    _search.MoveTimeHardLimit = TimeSpan.MaxValue;
                    _search.TimeRemaining[(int)Color.White] = TimeSpan.MaxValue;
                    _search.TimeRemaining[(int)Color.Black] = TimeSpan.MaxValue;
                    _search.CanAdjustMoveTime = false;
                    break;
                case "movetime":
                    _search.MoveTimeHardLimit = TimeSpan.FromMilliseconds(int.Parse(tokens[tokenIndex + 1]));
                    _search.CanAdjustMoveTime = false;
                    break;
                case "infinite":
                    _search.MoveTimeHardLimit = TimeSpan.MaxValue;
                    _search.TimeRemaining[(int)Color.White] = TimeSpan.MaxValue;
                    _search.TimeRemaining[(int)Color.Black] = TimeSpan.MaxValue;
                    _search.CanAdjustMoveTime = false;
                    break;
            }
        }
    }


    private void GoAsync()
    {
        // Find best move and respond.
        var bestMove = _search.FindBestMove(_board);
        WriteMessageLine($"bestmove {Move.ToLongAlgebraic(bestMove)}");
        // Signal search has stopped.
        _commandStopwatch.Stop();
        _search.Signal.Set();
        // Collect memory from unreferenced objects in generations 0 and 1.
        // Do not collect memory from generation 2 (that contains the large object heap) since it's mostly arrays whose lifetime is the duration of the application.
        GC.Collect(1, GCCollectionMode.Forced, true, true);
    }


    private void Stop()
    {
        if (_search.Continue)
        {
            _search.Continue = false;
            // Wait for search to complete.
            _search.Signal.WaitOne(_maxStopTime);
        }
    }


    private void Quit(int exitCode)
    {
        Dispose(true);
        Environment.Exit(exitCode);
    }


    // Extended Commands
    private void FindMagicMultipliers()
    {
        WriteMessageLine("Square   Piece  Shift  Unique Occupancies  Unique Moves  Magic Multiplier");
        WriteMessageLine("======  ======  =====  ==================  ============  ================");
        // Find magic multipliers for bishop and rook moves.
        // No need to find magic multipliers for queen moves since the queen combines bishop and rook moves.
        Board.PrecalculatedMoves.FindMagicMultipliers(ColorlessPiece.Bishop, WriteMessageLine);
        Board.PrecalculatedMoves.FindMagicMultipliers(ColorlessPiece.Rook, WriteMessageLine);
    }


    private void CountMoves(List<string> tokens)
    {
        _commandStopwatch.Restart();
        var horizon = int.Parse(tokens[1].Trim());
        if (horizon <= 0) throw new ArgumentException("Horizon must be > 0.", nameof(tokens));
        _board.Nodes = 0;
        _board.NodesInfoUpdate = NodesInfoInterval;
        var moves = CountMoves(0, horizon);
        _commandStopwatch.Stop();
        WriteMessageLine($"Counted {moves:n0} moves in {_commandStopwatch.Elapsed.TotalSeconds:0.000} seconds.");
    }


    private long CountMoves(int depth, int horizon)
    {
        if (depth < 0) throw new ArgumentException($"{nameof(depth)} must be >= 0.", nameof(depth));
        if (horizon < 0) throw new ArgumentException($"{nameof(horizon)} must be >= 0.", nameof(horizon));
        if (_board.Nodes >= _board.NodesInfoUpdate)
        {
            // Update move count.
            var nodesPerSecond = _board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
            WriteMessageLine($"Counted {_board.NodesInfoUpdate:n0} nodes ({nodesPerSecond:n0} nodes per second).");
            var intervals = (int)(_board.Nodes / NodesInfoInterval);
            _board.NodesInfoUpdate = NodesInfoInterval * (intervals + 1);
        }
        var toHorizon = horizon - depth;
        // Count moves using staged moved generation (as is done when searching moves).
        _board.CurrentPosition.PrepareMoveGeneration();
        long moves = 0;
        while (true)
        {
            var (move, moveIndex) = _search.GetNextMove(_board.CurrentPosition, Board.AllSquaresMask, depth, Move.Null);
            if (move == Move.Null) break; // All moves have been searched.
            if (!_board.IsMoveLegal(ref move)) continue; // Skip illegal move.
            Move.SetPlayed(ref move, true);
            _board.CurrentPosition.Moves[moveIndex] = move;
            _board.PlayMove(move);
            if (toHorizon > 1) moves += CountMoves(depth + 1, horizon);
            else moves++;
            _board.UndoMove();
        }
        return moves;
    }


    private void DivideMoves(List<string> tokens)
    {
        _commandStopwatch.Restart();
        var horizon = int.Parse(tokens[1].Trim());
        if (horizon < 1) throw new ArgumentException("Horizon must be >= 1.", nameof(tokens));
        _board.Nodes = 0;
        _board.NodesInfoUpdate = NodesInfoInterval;
        // Ensure all root moves are legal.
        _board.CurrentPosition.GenerateMoves();
        var legalMoveIndex = 0;
        for (var moveIndex = 0; moveIndex < _board.CurrentPosition.MoveIndex; moveIndex++)
        {
            var move = _board.CurrentPosition.Moves[moveIndex];
            if (_board.IsMoveLegal(ref move))
            {
                // Move is legal.
                Move.SetPlayed(ref move, true); // All root moves will be played so set this in advance.
                _board.CurrentPosition.Moves[legalMoveIndex] = move;
                legalMoveIndex++;
            }
        }
        _board.CurrentPosition.MoveIndex = legalMoveIndex;
        // Count moves for each root move.
        var rootMoves = new long[_board.CurrentPosition.MoveIndex];
        for (var moveIndex = 0; moveIndex < _board.CurrentPosition.MoveIndex; moveIndex++)
        {
            var move = _board.CurrentPosition.Moves[moveIndex];
            _board.PlayMove(move);
            rootMoves[moveIndex] = horizon == 1 ? 1 : CountMoves(1, horizon);
            _board.UndoMove();
        }
        _commandStopwatch.Stop();
        // Display move count for each root move.
        WriteMessageLine("Root Move    Moves");
        WriteMessageLine("=========  =======");
        for (var moveIndex = 0; moveIndex < _board.CurrentPosition.MoveIndex; moveIndex++)
        {
            var move = _board.CurrentPosition.Moves[moveIndex];
            WriteMessageLine($"{Move.ToLongAlgebraic(move),9}  {rootMoves[moveIndex],7}");
        }
        WriteMessageLine();
        WriteMessageLine($"{legalMoveIndex} legal root moves.");
    }


    private void ListMoves()
    {
        // Get cached position.
        var cachedPosition = _cache.GetPosition(_board.CurrentPosition.Key);
        var bestMove = _cache.GetBestMove(cachedPosition.Data);
        // Generate and sort moves.
        _board.CurrentPosition.GenerateMoves();
        var lastMoveIndex = _board.CurrentPosition.MoveIndex - 1;
        _search.PrioritizeMoves(_board.CurrentPosition, _board.CurrentPosition.Moves, lastMoveIndex, bestMove, 0);
        Search.SortMovesByPriority(_board.CurrentPosition.Moves, lastMoveIndex);
        WriteMessageLine("Rank   Move  Best  Cap Victim  Cap Attacker  Promo  Killer   History              Priority");
        WriteMessageLine("====  =====  ====  ==========  ============  =====  ======  ========  ====================");
        var stringBuilder = new StringBuilder();
        var legalMoveNumber = 0;
        for (var moveIndex = 0; moveIndex < _board.CurrentPosition.MoveIndex; moveIndex++)
        {
            var move = _board.CurrentPosition.Moves[moveIndex];
            if (!_board.IsMoveLegal(ref move)) continue; // Skip illegal move.
            legalMoveNumber++;
            stringBuilder.Clear();
            stringBuilder.Append(legalMoveNumber.ToString("00").PadLeft(4));
            stringBuilder.Append(Move.ToLongAlgebraic(move).PadLeft(7));
            stringBuilder.Append((Move.IsBest(move) ? "True" : string.Empty).PadLeft(6));
            stringBuilder.Append(PieceHelper.GetName(Move.CaptureVictim(move)).PadLeft(12));
            stringBuilder.Append(PieceHelper.GetName(Move.CaptureAttacker(move)).PadLeft(14));
            var promotedPiece = Move.PromotedPiece(move) == Piece.None ? string.Empty : PieceHelper.GetName(Move.PromotedPiece(move));
            stringBuilder.Append(promotedPiece.PadLeft(7));
            stringBuilder.Append(Move.Killer(move).ToString().PadLeft(8));
            stringBuilder.Append(Move.History(move).ToString().PadLeft(10));
            stringBuilder.Append(move.ToString().PadLeft(22));
            WriteMessageLine(stringBuilder.ToString());
        }
        WriteMessageLine();
        WriteMessageLine($"{legalMoveNumber} legal moves.");
    }


    private void ExchangeScore(List<string> tokens)
    {
        var move = Move.ParseLongAlgebraic(tokens[1].Trim(), _board.CurrentPosition.ColorToMove);
        var validMove = _board.ValidateMove(ref move);
        if (!validMove || !_board.IsMoveLegal(ref move)) throw new Exception($"Move {Move.ToLongAlgebraic(move)} is illegal in position {_board.CurrentPosition.ToFen()}.");
        var exchangeScore = _search.GetExchangeScore(_board, move);
        WriteMessageLine(exchangeScore.ToString());
    }


    private void TestPositions(List<string> tokens)
    {
        _commandStopwatch.Restart();
        var file = tokens[1].Trim();
        WriteMessageLine("Number                                                                     Position  Depth     Expected        Moves  Correct    Pct");
        WriteMessageLine("======  ===========================================================================  =====  ===========  ===========  =======  =====");
        _board.Nodes = 0;
        _board.NodesInfoUpdate = NodesInfoInterval;
        var positions = 0;
        var correctPositions = 0;
        // Verify move counts of test positions.
        using (var reader = File.OpenText(file))
        {
            while (!reader.EndOfStream)
            {
                // Load position, horizon, and correct move count.
                var line = reader.ReadLine();
                if (line == null) continue;
                positions++;
                var parsedTokens = Tokens.Parse(line, '|', '"');
                var fen = parsedTokens[0];
                var horizon = int.Parse(parsedTokens[1]);
                var expectedMoves = long.Parse(parsedTokens[2]);
                // Setup position.  Preserve move count.
                _board.SetPosition(fen, true);
                // Count nodes.  Do not update node count.
                _board.NodesInfoUpdate = long.MaxValue;
                var moves = CountMoves(0, horizon);
                var correct = moves == expectedMoves;
                if (correct) correctPositions++;
                var correctFraction = (100d * correctPositions) / positions;
                WriteMessageLine($"{positions,6}  {fen,75}  {horizon,5:0}  {expectedMoves,11:n0}  {moves,11:n0}  {correct,7}  {correctFraction,5:0.0}");
            }
        }
        _commandStopwatch.Stop();
        WriteMessageLine();
        WriteMessageLine($"Test completed in {_commandStopwatch.Elapsed.TotalSeconds:0} seconds.");
        // Update node count.
        var nodesPerSecond = _board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
        WriteMessageLine($"Counted {_board.Nodes:n0} nodes ({nodesPerSecond:n0} nodes per second).");
    }


    private void AnalyzePositions(IList<string> tokens)
    {
        _commandStopwatch.Restart();
        var file = tokens[1].Trim();
        var moveTimeMilliseconds = int.Parse(tokens[2].Trim());
        var positions = 0;
        var correctPositions = 0;
        _board.Nodes = 0;
        _board.NodesInfoUpdate = NodesInfoInterval;
        _stats.Reset();
        using (var reader = File.OpenText(file))
        {
            WriteMessageLine("Number                                                                     Position  Solution    Expected Moves   Move  Correct    Pct");
            WriteMessageLine("======  ===========================================================================  ========  ================  =====  =======  =====");
            while (!reader.EndOfStream)
            {
                // Load position and solution.
                var line = reader.ReadLine();
                if (line == null) continue;
                positions++;
                var parsedTokens = Tokens.Parse(line, ' ', '"');
                var positionSolution = PositionSolution.Unknown;
                const int illegalIndex = -1;
                var solutionIndex = illegalIndex;
                var expectedMovesIndex = illegalIndex;
                for (var index = 0; index < parsedTokens.Count; index++)
                {
                    var parsedToken = parsedTokens[index].Trim().ToLower();
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (parsedToken)
                    {
                        case "bm":
                            positionSolution = PositionSolution.BestMoves;
                            solutionIndex = index;
                            break;
                        case "am":
                            positionSolution = PositionSolution.AvoidMoves;
                            solutionIndex = index;
                            break;
                    }
                    if (parsedToken.EndsWith(";"))
                    {
                        expectedMovesIndex = index;
                        break;
                    }
                }
                if (solutionIndex == illegalIndex) throw new Exception("Position does not specify a best moves or avoid moves solution.");
                if (expectedMovesIndex == illegalIndex) throw new Exception("Position does not terminate the expected moves with a semicolon.");
                var correctMoves = expectedMovesIndex - solutionIndex;
                // Must convert tokens to array to prevent joining class name (System.Collections.Generic.List) instead of string value.
                // This is because the IEnumerable<T> overload does not accept a StartIndex and Count so those parameters are interpreted as params object[].
                var fen = string.Join(" ", parsedTokens.ToArray(), 0, solutionIndex).Trim();
                var expectedMovesListStandardAlgebraic = string.Join(" ", parsedTokens.ToArray(), solutionIndex + 1, correctMoves).Trim().TrimEnd(";".ToCharArray());
                var expectedMovesStandardAlgebraic = expectedMovesListStandardAlgebraic.Split(" ".ToCharArray());
                var expectedMoves = new ulong[expectedMovesStandardAlgebraic.Length];
                var expectedMovesLongAlgebraic = new string[expectedMovesStandardAlgebraic.Length];
                // Setup position and reset search and move heuristics.
                UciNewGame(true);
                _board.SetPosition(fen, true);
                for (var moveIndex = 0; moveIndex < expectedMovesStandardAlgebraic.Length; moveIndex++)
                {
                    var expectedMoveStandardAlgebraic = expectedMovesStandardAlgebraic[moveIndex];
                    var expectedMove = Move.ParseStandardAlgebraic(_board, expectedMoveStandardAlgebraic);
                    expectedMoves[moveIndex] = expectedMove;
                    expectedMovesLongAlgebraic[moveIndex] = Move.ToLongAlgebraic(expectedMove);
                }
                _cache.Reset();
                _killerMoves.Reset();
                _moveHistory.Reset();
                _search.Reset();
                // Find best move.  Do not update node count or PV.
                _board.NodesInfoUpdate = long.MaxValue;
                _search.PvInfoUpdate = false;
                _search.MoveTimeSoftLimit = TimeSpan.MaxValue;
                _search.MoveTimeHardLimit = TimeSpan.FromMilliseconds(moveTimeMilliseconds);
                _search.CanAdjustMoveTime = false;
                var bestMove = _search.FindBestMove(_board);
                _search.Signal.Set();
                // Determine if search found correct move.
                bool correct;
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (positionSolution)
                {
                    case PositionSolution.BestMoves:
                        correct = false;
                        for (var moveIndex = 0; moveIndex < expectedMoves.Length; moveIndex++)
                        {
                            var expectedMove = expectedMoves[moveIndex];
                            if (Move.Equals(bestMove, expectedMove))
                            {
                                correct = true;
                                break;
                            }
                        }
                        break;
                    case PositionSolution.AvoidMoves:
                        correct = true;
                        for (var moveIndex = 0; moveIndex < expectedMoves.Length; moveIndex++)
                        {
                            var expectedMove = expectedMoves[moveIndex];
                            if (Move.Equals(bestMove, expectedMove))
                            {
                                correct = false;
                                break;
                            }
                        }
                        break;
                    default:
                        throw new Exception(positionSolution + " position solution not supported.");
                }
                if (correct) correctPositions++;
                var correctFraction = (100d * correctPositions) / positions;
                var solution = positionSolution == PositionSolution.BestMoves ? "Best" : "Avoid";
                WriteMessageLine($"{positions,6}  {fen,75}  {solution,8}  {string.Join(" ", expectedMovesLongAlgebraic),16}  {Move.ToLongAlgebraic(bestMove),5}  {correct,7}  {correctFraction,5:0.0}");
            }
        }
        _commandStopwatch.Stop();
        // Display score, node count, and stats.
        WriteMessageLine();
        WriteMessageLine($"Solved {correctPositions} of {positions} positions in {_commandStopwatch.Elapsed.TotalSeconds:0} seconds.");
        var nodesPerSecond = _board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
        WriteMessageLine($"Counted {_board.Nodes:n0} nodes ({nodesPerSecond:n0} nodes per second).");
        WriteMessageLine();
        DisplayStats();
    }


    private void DisplayStats()
    {
        var nullMoveCutoffFraction = (100d * _stats.NullMoveCutoffs) / _stats.NullMoves;
        var betaCutoffMoveNumber = (double) _stats.BetaCutoffMoveNumber / _stats.MovesCausingBetaCutoff;
        var betaCutoffFirstMoveFraction = (100d * _stats.BetaCutoffFirstMove) / _stats.MovesCausingBetaCutoff;
        var cacheHitFraction = (100d * _stats.CacheHits) / _stats.CacheProbes;
        var scoreCutoffFraction = (100d * _stats.CacheScoreCutoff) / _stats.CacheHits;
        var bestMoveHitFraction = (100d * _stats.CacheValidBestMove) / _stats.CacheBestMoveProbes;
        WriteMessageLine($"info string Cache Hit = {cacheHitFraction:0.00}% Score Cutoff = {scoreCutoffFraction:0.00}% Best Move Hit = {bestMoveHitFraction:0.00}% Invalid Best Moves = {_stats.CacheInvalidBestMove:n0}");
        WriteMessageLine($"info string Null Move Cutoffs = {nullMoveCutoffFraction:0.00}% Beta Cutoff Move Number = {betaCutoffMoveNumber:0.00} Beta Cutoff First Move = {betaCutoffFirstMoveFraction:0.00}%");
        WriteMessageLine($"info string Evals = {_stats.Evaluations:n0}");
    }


    private void AnalyzeExchangePositions(IList<string> tokens)
    {
        _commandStopwatch.Restart();
        var file = tokens[1].Trim();
        var positions = 0;
        var correctPositions = 0;
        _stats.Reset();
        using (var reader = File.OpenText(file))
        {
            WriteMessageLine("Number                                                                     Position   Move  Expected Score  Score  Correct    Pct");
            WriteMessageLine("======  ===========================================================================  =====  ==============  =====  =======  =====");
            _board.Nodes = 0;
            while (!reader.EndOfStream)
            {
                // Load position and correct score.
                var line = reader.ReadLine();
                if (line == null) continue;
                positions++;
                var parsedTokens = Tokens.Parse(line, ',', '"');
                var fen = parsedTokens[0].Trim();
                var moveStandardAlgebraic = parsedTokens[1].Trim();
                var expectedScore = int.Parse(parsedTokens[2].Trim());
                // Setup position and reset search.
                _board.SetPosition(fen, true);
                _search.Reset();
                var move = Move.ParseStandardAlgebraic(_board, moveStandardAlgebraic);
                var score = _search.GetExchangeScore(_board, move);
                var correct = score == expectedScore;
                if (correct) correctPositions++;
                var correctFraction = (100d * correctPositions) / positions;
                WriteMessageLine($"{positions,6}  {fen,75}  {Move.ToLongAlgebraic(move),5}  {expectedScore,14}  {score,5}  {correct,7}  {correctFraction,5:0.0}");
            }
        }
        _commandStopwatch.Stop();
        // Update score.
        WriteMessageLine();
        WriteMessageLine($"Solved {correctPositions} of {positions} positions in {_commandStopwatch.Elapsed.TotalMilliseconds:000} milliseconds.");
        // Update node count.
        var nodesPerSecond = _board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
        WriteMessageLine($"Counted {_board.Nodes:n0} nodes ({nodesPerSecond:n0} nodes per second).");
    }


    private void ExportQuietPositions(IList<string> tokens)
    {
        var pgnFilename = tokens[1].Trim();
        var directory = Path.GetDirectoryName(pgnFilename) ?? string.Empty;
        var filenameNoExtension = Path.GetFileNameWithoutExtension(pgnFilename);
        var quietFilename = Path.Combine(directory, $"{filenameNoExtension}Quiet.txt");
        var margin = int.Parse(tokens[2].Trim());
        // Load games.
        WriteMessageLine("Loading games.");
        var stopwatch = Stopwatch.StartNew();
        var board = new Board(WriteMessageLine, NodesInfoInterval);
        var pgnGames = new PgnGames();
        pgnGames.Load(board, pgnFilename, WriteMessageLine);
        stopwatch.Stop();
        // Count positions.
        var positions = 0;
        for (var gameIndex = 0; gameIndex < pgnGames.Count; gameIndex++)
        {
            var pgnGame = pgnGames[gameIndex];
            positions += pgnGame.Moves.Count;
        }
        var positionsPerSecond = (int)(positions / stopwatch.Elapsed.TotalSeconds);
        WriteMessageLine($"Loaded {pgnGames.Count:n0} games with {positions:n0} positions in {stopwatch.Elapsed.TotalSeconds:0.000} seconds ({positionsPerSecond:n0} positions per second).");
        // Create game objects.
        var stats = new Stats();
        var cache = new Cache(1, stats, board.ValidateMove);
        var killerMoves = new KillerMoves(Search.MaxHorizon);
        var moveHistory = new MoveHistory();
        var eval = new Eval(stats, board.IsRepeatPosition, () => false, WriteMessageLine);
        var search = new Search(stats, cache, killerMoves, moveHistory, eval, () => false, DisplayStats, WriteMessageLine);
        board.NodesExamineTime = long.MaxValue;
        search.PvInfoUpdate = false;
        search.Continue = true;
        var quietPositions = 0;
        // Create or overwrite output file of quiet positions.
        using (var fileStream = File.Open(quietFilename, FileMode.Create, FileAccess.Write, FileShare.Read))
        using (var streamWriter = new StreamWriter(fileStream) { AutoFlush = true })
        {
            for (var gameIndex = 0; gameIndex < pgnGames.Count; gameIndex++)
            {
                var game = pgnGames[gameIndex];
                if (game.Result == GameResult.Unknown) continue; // Skip games with unknown results.
                board.SetPosition(Board.StartPositionFen, true);
                for (var moveIndex = 0; moveIndex < game.Moves.Count; moveIndex++)
                {
                    var move = game.Moves[moveIndex];
                    // Play move.
                    board.PlayMove(move);
                    if (board.CurrentPosition.KingInCheck) continue; // Do not evaluate positions with king in check.
                    // Get static and quiet scores.
                    var (staticScore, _) = eval.GetStaticScore(board.CurrentPosition);
                    var quietScore = search.GetQuietScore(board, 1, 1, -SpecialScore.Max, SpecialScore.Max);
                    if (FastMath.Abs(staticScore - quietScore) <= margin)
                    {
                        // Write quiet position to output file.
                        quietPositions++;
                        var result = game.Result switch
                        {
                            GameResult.WhiteWon => "1-0",
                            GameResult.BlackWon => "0-1",
                            _ => "1/2-1/2"
                        };
                        streamWriter.WriteLine($"{board.CurrentPosition.ToFen()}|{result}");
                        if ((quietPositions % 10_000) == 0) WriteMessageLine($"Exported {quietPositions:n0} quiet positions.");
                    }
                }
            }
            WriteMessageLine($"Exported {quietPositions:n0} quiet positions.");
        }
    }


    private void Tune(IList<string> tokens)
    {
        _commandStopwatch.Restart();
        var quietFilename = tokens[1].Trim();
        var particleSwarmsCount = int.Parse(tokens[2].Trim());
        var particlesPerSwarm = int.Parse(tokens[3].Trim());
        var winScale = int.Parse(tokens[4].Trim()); // Use 774 for MadChessBulletCompetitorsQuiet.txt.
        var iterations = int.Parse(tokens[5].Trim());
        var particleSwarms = new ParticleSwarms(quietFilename, particleSwarmsCount, particlesPerSwarm, winScale, WriteMessageLine);
        particleSwarms.Optimize(iterations);
        _commandStopwatch.Stop();
        WriteMessageLine();
        WriteMessageLine("Tuning complete.");
    }


    private void TuneWinScale(IList<string> tokens)
    {
        var quietFilename = tokens[1].Trim();
        var threads = int.Parse(tokens[2]);
        // Load quiet positions.
        WriteMessageLine("Loading quiet positions.");
        var stopwatch = Stopwatch.StartNew();
        var quietPositions = new QuietPositions();
        using (var streamReader = File.OpenText(quietFilename))
        {
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if (line == null) continue;
                var lineSegments = line.Split('|');
                if (lineSegments.Length < 2) continue;
                var fen = lineSegments[0];
                var resultText = lineSegments[1];
                var result = resultText switch
                {
                    "1-0" => GameResult.WhiteWon,
                    "1/2-1/2" => GameResult.Draw,
                    "0-1" => GameResult.BlackWon,
                    _ => GameResult.Unknown
                };
                var quietPosition = new QuietPosition(fen, result);
                quietPositions.Add(quietPosition);
                if ((quietPositions.Count % 10_000) == 0) WriteMessageLine($"Loaded {quietPositions.Count:n0} quiet positions.");
            }
            WriteMessageLine($"Loaded {quietPositions.Count:n0} quiet positions.");
        }
        var positionsPerSecond = (int)(quietPositions.Count / stopwatch.Elapsed.TotalSeconds);
        WriteMessageLine($"Loaded {quietPositions.Count:n0} quiet positions in {stopwatch.Elapsed.TotalSeconds:0.000} seconds ({positionsPerSecond:n0} positions per second).");
        stopwatch.Restart();
        // Create game objects.
        var parameters = ParticleSwarms.CreateParameters();
        var gameObjects = new (Particle Particle, Board Board, Eval Eval)[threads];
        int thread;
        for (thread = 0; thread < threads; thread++)
        {
            var particle = new Particle(quietPositions, parameters);
            var board = new Board(WriteMessageLine, NodesInfoInterval);
            var stats = new Stats();
            var eval = new Eval(stats, board.IsRepeatPosition, () => false, WriteMessageLine);
            gameObjects[thread] = (particle, board, eval);
        }
        // Calculate evaluation error of all win scales.
        WriteMessageLine("Tuning win scale.");
        WriteMessageLine();
        var winScales = new Stack<int>(_maxWinScale - _minWinScale + 1);
        for (var winScale = _minWinScale; winScale <= _maxWinScale; winScale++) winScales.Push(winScale);
        var tasks = new Task<int>[threads];
        var bestWinScale = _minWinScale;
        var bestEvaluationError = double.MaxValue;
        do
        {
            thread = 0;
            while ((winScales.Count > 0) && (thread < threads))
            {
                var (particle, board, eval) = gameObjects[thread];
                var winScale = winScales.Pop();
                tasks[thread] = Task.Run(() =>
                {
                    particle.CalculateEvaluationError(board, eval, winScale);
                    return winScale;
                });
                thread++;
            }
            // ReSharper disable once CoVariantArrayConversion
            Task.WaitAll(tasks);
            // Find best win scale.
            for (thread = 0; thread < threads; thread++)
            {
                var winScale = tasks[thread].Result;
                var evaluationError = gameObjects[thread].Particle.EvaluationError;
                WriteMessageLine($"Win Scale = {winScale:0000}, Evaluation Error = {evaluationError:0.000}");
                if (evaluationError < bestEvaluationError)
                {
                    bestWinScale = winScale;
                    bestEvaluationError = evaluationError;
                }
            }
            if (winScales.Count == 0) break;
        } while (true);
        WriteMessageLine();
        WriteMessageLine($"Best win scale = {bestWinScale}.");
        _commandStopwatch.Stop();
        WriteMessageLine($"Completed tuning of win scale in {_commandStopwatch.Elapsed.TotalSeconds:0} seconds.");
    }

    
    private void Help()
    {
        WriteMessageLine("MadChess by Erik Madsen.  See https://www.madchess.net/.");
        WriteMessageLine();
        WriteMessageLine("In addition to standard UCI commands, MadChess supports the following custom commands.");
        WriteMessageLine();
        WriteMessageLine("showboard                             Display current position.");
        WriteMessageLine();
        WriteMessageLine("findmagics                            Find magic multipliers not already hard-coded into engine.  Not useful without first");
        WriteMessageLine("                                      removing hard-coded magic multipliers from source code, then recompiling.");
        WriteMessageLine();
        WriteMessageLine("countmoves [depth]                    Count legal moves at given depth.   Count only leaf nodes, not internal nodes.");
        WriteMessageLine("                                      Known by chess programmers as perft.");
        WriteMessageLine();
        WriteMessageLine("dividemoves [depth]                   Count legal moves following each legal root move.  Count only leaf nodes.");
        WriteMessageLine();
        WriteMessageLine("listmoves                             List moves in order of priority.  Display history heuristics for each move.");
        WriteMessageLine();
        WriteMessageLine("shiftkillermoves [depth]              Shift killer moves deeper by given depth.");
        WriteMessageLine("                                      Useful after go command followed by a position command that includes moves.");
        WriteMessageLine("                                      Without shifting killer moves, the listmoves command will display incorrect killer values.");
        WriteMessageLine();
        WriteMessageLine("showevalparams                        Display evaluation parameters used to calculate static score for a position.");
        WriteMessageLine();
        WriteMessageLine("staticscore                           Display evaluation details of current position.");
        WriteMessageLine();
        WriteMessageLine("exchangescore [move]                  Display static score if pieces are traded on the destination square of the given move.");
        WriteMessageLine("                                      Move must be specified in long algebraic notation.");
        WriteMessageLine();
        WriteMessageLine("testpositions [filename]              Calculate legal moves for positions in given file and compare to expected results.");
        WriteMessageLine("                                      Each line of file must be formatted as [FEN]|[Depth]|[Legal Move Count].");
        WriteMessageLine();
        WriteMessageLine("analyzepositions [filename] [msec]    Search for best move for positions in given file and compare to expected results.");
        WriteMessageLine("                                      File must be in EPD format.  Search of each move is limited to given time in milliseconds.");
        WriteMessageLine();
        WriteMessageLine("analyzeexchangepositions [filename]   Determine material score after exchanging pieces on destination square of given move.");
        WriteMessageLine("                                      Pawn = 100, Knight and Bishop = 300, Rook = 500, Queen = 900.");
        WriteMessageLine();
        WriteMessageLine("exportquietpositions [pgn] [margin]   Export quiet positions from games in given PDF file.  A quiet position is one where the");
        WriteMessageLine("                                      static score differs from the quiet score by [margin] centipawns or less. ");
        WriteMessageLine();
        WriteMessageLine("tune [quiet] [ps] [pps] [ws] [i]      Tune evaluation parameters using a particle swarm algorithm.");
        WriteMessageLine("                                      quiet = quiet positions filename, ps = Particle Swarms, pps = Particles Per Swarm.");
        WriteMessageLine("                                      ws = Win Scale, i = Iterations.");
        WriteMessageLine();
        WriteMessageLine("tunewinscale [filename] [threads]     Compute a scale constant used in the sigmoid function of the tuning algorithm.");
        WriteMessageLine("                                      The sigmoid function maps evaluation score to expected win fraction.");
        WriteMessageLine("                                      Each line of file must be formatted as [FEN]|[GameResult].");
    }


    private void WriteMessageLine()
    {
        lock (_messageLock)
        {
            Console.WriteLine();
            if (Log) WriteMessageLine(null, CommandDirection.Out);
        }
    }


    private void WriteMessageLine(string message)
    {
        lock (_messageLock)
        {
            Console.WriteLine(message);
            if (Log) WriteMessageLine(message, CommandDirection.Out);
        }
    }


    private void WriteMessageLine(string message, CommandDirection direction)
    {
        lock (_messageLock)
        {
            var elapsed = _stopwatch.Elapsed;
            _logWriter.Write($"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}  ");
            _logWriter.Write(direction == CommandDirection.In ? " In   " : " Out  ");
            _logWriter.WriteLine(message);
        }
    }
}