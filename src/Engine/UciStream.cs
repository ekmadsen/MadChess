// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
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
using ErikTheCoder.MadChess.Core;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Engine.Tuning;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Evaluation;
using ErikTheCoder.MadChess.Engine.Hashtable;
using ErikTheCoder.MadChess.Engine.Intelligence;


namespace ErikTheCoder.MadChess.Engine;


public sealed class UciStream : IDisposable
{
    public const long NodesInfoInterval = 1_000_000;
    public const long NodesTimeInterval = 1_000;
    private string[] _defaultPlyAndFullMove;
    private const int _cacheSizeMegabytes = 128;
    private const int _minWinScale = 400;
    private const int _maxWinScale = 800;
    private readonly TimeSpan _maxStopTime = TimeSpan.FromMilliseconds(100);
    private readonly Messenger _messenger; // Lifetime managed by caller.
    private Board _board;
    private Stats _stats;
    private Cache _cache;
    private KillerMoves _killerMoves;
    private MoveHistory _moveHistory;
    private Eval _eval;
    private Search _search;
    private Stopwatch _commandStopwatch;
    private Queue<List<string>> _asyncQueue;
    private Thread _asyncThread;
    private AutoResetEvent _asyncSignal;
    private object _queueLock;
    private bool _disposed;


    public UciStream(Messenger messenger)
    {
        _messenger = messenger;

        // Create diagnostic and synchronization objects.
        _commandStopwatch = new Stopwatch();
        _asyncQueue = new Queue<List<string>>();
        _asyncSignal = new AutoResetEvent(false);
        _queueLock = new object();

        // Create game objects.
        _board = new Board(_messenger);
        _stats = new Stats();
        _cache = new Cache(_stats, _cacheSizeMegabytes);
        _killerMoves = new KillerMoves();
        _moveHistory = new MoveHistory();
        _eval = new Eval(_messenger, _stats);
        _search = new Search(_messenger, _stats, _cache, _killerMoves, _moveHistory, _eval);
        
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
            _commandStopwatch = null;
            lock (_queueLock) { _asyncQueue = null; }
            _asyncThread = null;
            _queueLock = null;
        }

        // Release unmanaged resources.
        _search?.Dispose();
        _search = null;
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
        _messenger.Log = true;
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

        _messenger.WriteMessageLine(stringBuilder.ToString());

        Quit(-1);
    }


    private void MonitorInputStream()
    {
        try
        {
            string command;
            do
            {
                // Read and dispatch command.
                command = _messenger.ReadLine();
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

        // Parse command into tokens.  Do not convert to lowercase because this invalidates FEN strings (where case differentiates white and black pieces).
        var tokens = Tokens.Parse(command, ' ', '"');
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
                _messenger.WriteMessageLine("readyok");
                break;

            case "debug":
                _messenger.Debug = tokens[1].Equals("on", StringComparison.OrdinalIgnoreCase);
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
                _messenger.WriteMessageLine(_board.ToString());
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
                ListMoves(_board.CurrentPosition);
                break;

            case "shiftkillermoves":
                _killerMoves.Shift(int.Parse(tokens[1]));
                break;

            case "resetstats":
                _stats.Reset();
                break;

            case "showevalparams":
                _messenger.WriteMessageLine(_eval.ShowParameters());
                break;

            case "staticscore":
                _messenger.WriteMessageLine(_eval.ToString(_board.CurrentPosition));
                break;

            case "testpositions":
                TestPositions(tokens);
                break;

            case "analyzepositions":
                AnalyzePositions(tokens);
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
                _messenger.WriteMessageLine(tokens[0] + " command not supported.");
                break;
        }

        if (writeMessageLine) _messenger.WriteMessageLine();
    }


    private void DispatchOnAsyncThread(List<string> tokens)
    {
        lock (_queueLock)
        {
            // Queue command then signal async queue.
            _asyncQueue.Enqueue(tokens);
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
                lock (_queueLock)
                {
                    if (_asyncQueue.Count > 0) tokens = _asyncQueue.Dequeue();
                }

                if (!tokens.IsNullOrEmpty())
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

                    _messenger.WriteMessageLine();
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
        // Display engine name.
        // ReSharper disable once ConvertToConstant.Local
        var version = "3.2 Beta";
#if CPU64
        version = $"{version} x64";
#else
            version = $"{version} x86";
#endif
        _messenger.WriteMessageLine($"id name MadChess {version}");

        // Display author.
        _messenger.WriteMessageLine("id author Erik Madsen");

        // Display engine options.
        _messenger.WriteMessageLine("option name UCI_EngineAbout type string default MadChess by Erik Madsen.  See https://www.madchess.net.");
        _messenger.WriteMessageLine("option name Log type check default false");
        _messenger.WriteMessageLine("option name Hash type spin default 128 min 0 max 2048");
        _messenger.WriteMessageLine("option name ClearHash type button");
        _messenger.WriteMessageLine("option name UCI_AnalyseMode type check default false");
        _messenger.WriteMessageLine($"option name MultiPV type spin default 1 min 1 max {Core.Game.Position.MaxMoves}");
        _messenger.WriteMessageLine("option name UCI_LimitStrength type check default false");
        _messenger.WriteMessageLine($"option name UCI_Elo type spin default {Elo.Min} min {Elo.Min} max {Elo.Max}");

        _messenger.WriteMessageLine("uciok");
    }


    private void SetOption(List<string> tokens)
    {
        var optionName = tokens[2];
        var optionValue = tokens.Count > 4 ? tokens[4] : string.Empty;

        switch (optionName.ToLower())
        {
            case "log":
                _messenger.Log = optionValue.Equals("true", StringComparison.OrdinalIgnoreCase);
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
                var analyzeMode = optionValue.Equals("true", StringComparison.OrdinalIgnoreCase);

                if (analyzeMode)
                {
                    _search.AnalyzeMode = true;
                    _eval.DrawMoves = 3;
                }
                else
                {
                    _search.AnalyzeMode = false;
                    _eval.DrawMoves = 2;
                }
                
                _eval.DrawMoves = analyzeMode ? 3 : 2;
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
                _messenger.WriteMessageLine(optionName + " option not supported.");
                break;
        }
    }


    private void UciNewGame(bool preserveMoveCount = false)
    {
        // Reset search count, cache, and move heuristics.
        _search.Count = 0;
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
            if (!char.IsNumber(tokens[^1][0]))
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

        // Setup position and play specified moves.
        _board.SetPosition(fen);

        while (moveIndex < tokens.Count)
        {
            var move = Move.ParseLongAlgebraic(tokens[moveIndex], _board.CurrentPosition.ColorToMove);

            // Verify move is valid.
            var validMove = _board.CurrentPosition.ValidateMove(ref move);
            if (!validMove) throw new Exception($"Move {Move.ToLongAlgebraic(move)} is invalid in position {_board.CurrentPosition.ToFen()}.");

            // Verify move is legal.
            var (legalMove, _) = _board.PlayMove(move);
            if (!legalMove) throw new Exception($"Move {Move.ToLongAlgebraic(move)} is illegal in position {_board.PreviousPosition.ToFen()}.");

            moveIndex++;
        }
    }


    private void GoSync(List<string> tokens)
    {
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
                    // Restrict search to candidate moves.
                    // Assume all remaining tokens are candidate moves.
                    for (var moveIndex = tokenIndex + 1; moveIndex < tokens.Count; moveIndex++)
                    {
                        var move = Move.ParseLongAlgebraic(tokens[moveIndex], _board.CurrentPosition.ColorToMove);
                        _search.CandidateMoves.Add(move);
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
                    _search.HorizonLimit = FastMath.Min(int.Parse(tokens[tokenIndex + 1]), Search.MaxHorizon);
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
        _messenger.WriteMessageLine($"bestmove {Move.ToLongAlgebraic(bestMove)}");

        // Signal search has stopped.
        _search.Signal.Set();

        // Collect memory from unreferenced objects in generations 0 and 1.
        // Do not collect memory from generation 2 (that contains the large object heap) because it's mostly arrays whose lifetime is the duration of the application.
        GC.Collect(1, GCCollectionMode.Forced, true, true);
    }


    private void Stop()
    {
        _search.Continue = false;

        // Wait for search to complete.
        _search.Signal.WaitOne(_maxStopTime);

        _search.Continue = false;
        _board.Nodes = 0; // Prevent incorrect NPS calculation by GUI when number of Multi-PV lines is modified.
    }


    private void Quit(int exitCode)
    {
        Dispose(true);
        Environment.Exit(exitCode);
    }


    // Extended Commands
    private void FindMagicMultipliers()
    {
        _messenger.WriteMessageLine("Square   Piece  Shift  Unique Occupancies  Unique Moves  Magic Multiplier");
        _messenger.WriteMessageLine("======  ======  =====  ==================  ============  ================");

        // Find magic multipliers for bishop and rook moves.
        // No need to find magic multipliers for queen moves because the queen combines bishop and rook moves.
        Board.PrecalculatedMoves.FindMagicMultipliers(ColorlessPiece.Bishop, _messenger);
        Board.PrecalculatedMoves.FindMagicMultipliers(ColorlessPiece.Rook, _messenger);
    }


    private void CountMoves(List<string> tokens)
    {
        _commandStopwatch.Restart();

        var horizon = int.Parse(tokens[1].Trim());
        if (horizon <= 0) throw new ArgumentException("Horizon must be > 0.", nameof(tokens));

        _board.Nodes = 0;

        var moves = CountMoves(0, horizon);

        _commandStopwatch.Stop();
        _messenger.WriteMessageLine($"Counted {moves:n0} moves in {_commandStopwatch.Elapsed.TotalSeconds:0.000} seconds.");
    }


    private long CountMoves(int depth, int horizon)
    {
        if (depth < 0) throw new ArgumentException($"{nameof(depth)} must be >= 0.", nameof(depth));
        if (horizon < 0) throw new ArgumentException($"{nameof(horizon)} must be >= 0.", nameof(horizon));

        if (_board.Nodes >= _search.NodesInfoUpdate)
        {
            // Display move count.
            var nodesPerSecond = _board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
            _messenger.WriteMessageLine($"Counted {_search.NodesInfoUpdate:n0} nodes ({nodesPerSecond:n0} nodes per second).");

            var intervals = (int)(_board.Nodes / NodesInfoInterval);
            _search.NodesInfoUpdate = NodesInfoInterval * (intervals + 1);
        }

        // Count moves using staged moved generation (as is done when searching moves).
        var toHorizon = horizon - depth;
        _board.CurrentPosition.PrepareMoveGeneration();
        long moves = 0;

        while (true)
        {
            var (move, moveIndex) = _search.GetNextMove(_board.CurrentPosition, Board.AllSquaresMask, depth, Move.Null);
            if (move == Move.Null) break; // All moves have been searched.

            var (legalMove, _) = _board.PlayMove(move);
            if (!legalMove)
            {
                // Skip illegal move.
                _board.UndoMove();
                continue;
            }

            Move.SetPlayed(ref move, true);
            _board.PreviousPosition.Moves[moveIndex] = move;

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
        _search.NodesInfoUpdate = NodesInfoInterval;

        // Ensure all root moves are legal.
        _board.CurrentPosition.GenerateMoves();
        var legalMoveIndex = 0;

        for (var moveIndex = 0; moveIndex < _board.CurrentPosition.MoveIndex; moveIndex++)
        {
            var move = _board.CurrentPosition.Moves[moveIndex];
            var (legalMove, _) = _board.PlayMove(move);
            _board.UndoMove();

            if (legalMove)
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

        // Display move count for each root move.
        _messenger.WriteMessageLine("Root Move    Moves");
        _messenger.WriteMessageLine("=========  =======");

        for (var moveIndex = 0; moveIndex < _board.CurrentPosition.MoveIndex; moveIndex++)
        {
            var move = _board.CurrentPosition.Moves[moveIndex];
            _messenger.WriteMessageLine($"{Move.ToLongAlgebraic(move),9}  {rootMoves[moveIndex],7}");
        }

        _messenger.WriteMessageLine();
        _messenger.WriteMessageLine($"{legalMoveIndex} legal root moves.");
    }


    private void ListMoves(Position position)
    {
        // Get cached position.
        var cachedPosition = _cache.GetPosition(_board.CurrentPosition.Key, _search.Count);
        var bestMove = _cache.GetBestMove(position, cachedPosition.Data);

        // Generate and sort moves.
        _board.CurrentPosition.GenerateMoves();
        var lastMoveIndex = _board.CurrentPosition.MoveIndex - 1;
        _search.PrioritizeMoves(_board.CurrentPosition.Moves, lastMoveIndex, bestMove, 0);
        Search.SortMovesByPriority(_board.CurrentPosition.Moves, lastMoveIndex);

        _messenger.WriteMessageLine("Rank   Move  Best  Cap Victim  Cap Attacker  Promo  Killer   History              Priority");
        _messenger.WriteMessageLine("====  =====  ====  ==========  ============  =====  ======  ========  ====================");

        var stringBuilder = new StringBuilder();
        var legalMoveNumber = 0;

        for (var moveIndex = 0; moveIndex < _board.CurrentPosition.MoveIndex; moveIndex++)
        {
            var move = _board.CurrentPosition.Moves[moveIndex];
            var (legalMove, _) = _board.PlayMove(move);
            _board.UndoMove();

            if (!legalMove) continue; // Skip illegal move.

            legalMoveNumber++;
            
            stringBuilder.Clear();
            stringBuilder.Append(legalMoveNumber.ToString("00").PadLeft(4));

            stringBuilder.Append(Move.ToLongAlgebraic(move).PadLeft(7));
            stringBuilder.Append((Move.IsBest(move) ? "True" : string.Empty).PadLeft(6));

            stringBuilder.Append(PieceHelper.GetName(Move.CaptureVictim(move)).PadLeft(12));
            stringBuilder.Append(PieceHelper.GetName(Move.CaptureAttacker(move)).PadLeft(14));

            var promotedPiece = PieceHelper.GetName(Move.PromotedPiece(move));
            stringBuilder.Append(promotedPiece.PadLeft(7));

            stringBuilder.Append(Move.Killer(move).ToString().PadLeft(8));
            stringBuilder.Append(Move.History(move).ToString().PadLeft(10));

            stringBuilder.Append(move.ToString().PadLeft(22));

            _messenger.WriteMessageLine(stringBuilder.ToString());
        }

        _messenger.WriteMessageLine();
        _messenger.WriteMessageLine($"{legalMoveNumber} legal moves.");
    }


    private void TestPositions(List<string> tokens)
    {
        _commandStopwatch.Restart();

        var file = tokens[1].Trim();

        _messenger.WriteMessageLine("Number                                                                     Position  Depth     Expected        Moves  Correct    Pct");
        _messenger.WriteMessageLine("======  ===========================================================================  =====  ===========  ===========  =======  =====");

        _board.Nodes = 0;
        _search.NodesInfoUpdate = NodesInfoInterval;
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

                // Count nodes.  Do not display node count.
                _search.NodesInfoUpdate = long.MaxValue;
                var moves = CountMoves(0, horizon);

                var correct = moves == expectedMoves;
                if (correct) correctPositions++;
                var correctFraction = (100d * correctPositions) / positions;

                _messenger.WriteMessageLine($"{positions,6}  {fen,75}  {horizon,5:0}  {expectedMoves,11:n0}  {moves,11:n0}  {correct,7}  {correctFraction,5:0.0}");
            }
        }

        _commandStopwatch.Stop();

        _messenger.WriteMessageLine();
        _messenger.WriteMessageLine($"Test completed in {_commandStopwatch.Elapsed.TotalSeconds:0} seconds.");

        // Display node count.
        var nodesPerSecond = _board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
        _messenger.WriteMessageLine($"Counted {_board.Nodes:n0} nodes ({nodesPerSecond:n0} nodes per second).");
    }


    private void AnalyzePositions(IList<string> tokens)
    {
        _commandStopwatch.Restart();

        var file = tokens[1].Trim();
        var moveTimeMilliseconds = int.Parse(tokens[2].Trim());
        
        var positions = 0;
        var correctPositions = 0;
        _board.Nodes = 0;
        _search.NodesInfoUpdate = NodesInfoInterval;
        
        _stats.Reset();

        using (var reader = File.OpenText(file))
        {
            _messenger.WriteMessageLine("Number                                                                     Position  Solution    Expected Moves   Move  Correct    Pct");
            _messenger.WriteMessageLine("======  ===========================================================================  ========  ================  =====  =======  =====");

            while (!reader.EndOfStream)
            {
                // Load position and solution.
                var line = reader.ReadLine();
                if (line == null) continue;

                positions++;

                const int illegalIndex = -1;
                var solutionIndex = illegalIndex;
                var expectedMovesIndex = illegalIndex;
                var parsedTokens = Tokens.Parse(line, ' ', '"');
                var positionSolution = PositionSolution.Unknown;

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
                
                // Must convert tokens to array to prevent joining class name (System.Collections.Generic.List) instead of string value.
                // This is because the IEnumerable<T> overload does not accept a StartIndex and Count so those parameters are interpreted as params object[].
                var fen = string.Join(" ", parsedTokens.ToArray(), 0, solutionIndex).Trim();

                // Setup position and reset search and move heuristics.
                UciNewGame(true);
                _board.SetPosition(fen, true);
                _cache.Reset();
                _killerMoves.Reset();
                _moveHistory.Reset();
                _search.Reset();

                // Determine expected moves.
                var correctMoves = expectedMovesIndex - solutionIndex;
                var expectedMovesListStandardAlgebraic = string.Join(" ", parsedTokens.ToArray(), solutionIndex + 1, correctMoves).Trim().TrimEnd(";".ToCharArray());
                var expectedMovesStandardAlgebraic = expectedMovesListStandardAlgebraic.Split(" ".ToCharArray());
                var expectedMoves = new ulong[expectedMovesStandardAlgebraic.Length];
                var expectedMovesLongAlgebraic = new string[expectedMovesStandardAlgebraic.Length];

                for (var moveIndex = 0; moveIndex < expectedMovesStandardAlgebraic.Length; moveIndex++)
                {
                    var expectedMoveStandardAlgebraic = expectedMovesStandardAlgebraic[moveIndex];
                    var expectedMove = Move.ParseStandardAlgebraic(_board, expectedMoveStandardAlgebraic);
                    expectedMoves[moveIndex] = expectedMove;
                    expectedMovesLongAlgebraic[moveIndex] = Move.ToLongAlgebraic(expectedMove);
                }

                // Find best move.  Do not display node count or PV.
                _search.NodesInfoUpdate = long.MaxValue;
                _search.PvInfoUpdate = false;
                _search.MoveTimeSoftLimit = TimeSpan.MaxValue;
                _search.MoveTimeHardLimit = TimeSpan.FromMilliseconds(moveTimeMilliseconds);
                _search.CanAdjustMoveTime = false;

                var bestMove = _search.FindBestMove(_board);

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

                _messenger.WriteMessageLine($"{positions,6}  {fen,75}  {solution,8}  {string.Join(" ", expectedMovesLongAlgebraic),16}  {Move.ToLongAlgebraic(bestMove),5}  {correct,7}  {correctFraction,5:0.0}");
            }
        }

        _commandStopwatch.Stop();

        // Display score, node count, and stats.
        _messenger.WriteMessageLine();
        _messenger.WriteMessageLine($"Solved {correctPositions} of {positions} positions in {_commandStopwatch.Elapsed.TotalSeconds:0} seconds.");

        var nodesPerSecond = _board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
        _messenger.WriteMessageLine($"Counted {_board.Nodes:n0} nodes ({nodesPerSecond:n0} nodes per second).");

        _messenger.WriteMessageLine();
        _messenger.WriteMessageLine(_stats.ToString());
    }


    private void Tune(IList<string> tokens)
    {
        var pgnFilename = tokens[1].Trim();
        var particleSwarmsCount = int.Parse(tokens[2].Trim());
        var particlesPerSwarm = int.Parse(tokens[3].Trim());
        var winScale = int.Parse(tokens[4].Trim()); // Use 637 for MadChessGauntletsRecent.pgn.
        var iterations = int.Parse(tokens[5].Trim());

        var particleSwarms = new ParticleSwarms(_messenger, pgnFilename, particleSwarmsCount, particlesPerSwarm, winScale);
        particleSwarms.Optimize(iterations);

        _messenger.WriteMessageLine();
        _messenger.WriteMessageLine("Tuning complete.");
    }


    private void TuneWinScale(IList<string> tokens)
    {
        var pgnFilename = tokens[1].Trim();
        var threads = int.Parse(tokens[2]);

        // Load games.
        _commandStopwatch.Restart();
        _messenger.WriteMessageLine("Loading games.");
        var pgnGames = new PgnGames(_messenger);
        pgnGames.Load(_board, pgnFilename);

        // Count positions.
        long positions = 0;
        for (var index = 0; index < pgnGames.Count; index++)
        {
            var pgnGame = pgnGames[index];
            positions += pgnGame.Moves.Count;
        }

        // Display game and position counts.
        var positionsPerSecond = (int)(positions / _commandStopwatch.Elapsed.TotalSeconds);
        _messenger.WriteMessageLine($"Loaded {pgnGames.Count:n0} games with {positions:n0} positions in {_commandStopwatch.Elapsed.TotalSeconds:0.000} seconds ({positionsPerSecond:n0} positions per second).");
        _messenger.WriteMessageLine("Tuning win scale.");
        _messenger.WriteMessageLine();

        // Create game objects.
        var parameters = ParticleSwarms.CreateParameters();
        var gameObjects = new (Particle Particle, Board Board, Search Search)[threads];
        int thread;

        for (thread = 0; thread < threads; thread++)
        {
            var particle = new Particle(pgnGames, parameters);

            var board = new Board(_messenger);
            var stats = new Stats();
            var cache = new Cache(stats, 1);
            var killerMoves = new KillerMoves();
            var moveHistory = new MoveHistory();
            var eval = new Eval(_messenger, stats);
            var search = new Search(_messenger, stats, cache, killerMoves, moveHistory, eval);

            gameObjects[thread] = (particle, board, search);
        }

        // Calculate evaluation error of all win scales.
        var winScales = new Stack<int>(_maxWinScale - _minWinScale + 1);
        for (var winScale = _minWinScale; winScale <= _maxWinScale; winScale++)
            winScales.Push(winScale);

        var tasks = new Task<int>[threads];
        var bestWinScale = _minWinScale;
        var bestEvaluationError = double.MaxValue;

        do
        {
            thread = 0;

            while ((winScales.Count > 0) && (thread < threads))
            {
                var (particle, board, search) = gameObjects[thread];
                var winScale = winScales.Pop();

                tasks[thread] = Task.Run(() =>
                {
                    particle.CalculateEvaluationError(board, search, winScale);
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

                _messenger.WriteMessageLine($"Win Scale = {winScale:0000}, Evaluation Error = {evaluationError:0.000}");

                if (evaluationError < bestEvaluationError)
                {
                    bestWinScale = winScale;
                    bestEvaluationError = evaluationError;
                }
            }

            if (winScales.Count == 0) break;

        } while (true);

        _messenger.WriteMessageLine();
        _messenger.WriteMessageLine($"Best win scale = {bestWinScale}.");

        _commandStopwatch.Stop();
        _messenger.WriteMessageLine($"Completed tuning of win scale in {_commandStopwatch.Elapsed.TotalSeconds:0} seconds.");
    }


    private void Help()
    {
        _messenger.WriteMessageLine("MadChess by Erik Madsen.  See https://www.madchess.net/.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("In addition to standard UCI commands, MadChess supports the following custom commands.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("showboard                             Display current position.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("findmagics                            Find magic multipliers not already hard-coded into engine.  Not useful without first");
        _messenger.WriteMessageLine("                                      removing hard-coded magic multipliers from source code, then recompiling.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("countmoves [depth]                    Count legal moves at depth.   Count only leaf nodes, not internal nodes.");
        _messenger.WriteMessageLine("                                      Known by chess programmers as perft.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("dividemoves [depth]                   Count legal moves following each legal root move.  Count only leaf nodes.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("listmoves                             List moves in order of priority.  Display history heuristics for each move.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("shiftkillermoves [depth]              Shift killer moves deeper by depth.");
        _messenger.WriteMessageLine("                                      Useful after go command followed by a position command that includes moves.");
        _messenger.WriteMessageLine("                                      Without shifting killer moves, the listmoves command will display incorrect killer values.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("resetstats                            Set NullMoves, NullMoveCutoffs, MovesCausingBetaCutoff, etc to 0.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("showevalparams                        Display evaluation parameters used to calculate static score for a position.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("staticscore                           Display evaluation details of current position.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("testpositions [filename]              Calculate legal moves for positions in file and compare to expected results.");
        _messenger.WriteMessageLine("                                      Each line of file must be formatted as [FEN]|[Depth]|[Legal Move Count].");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("analyzepositions [filename] [msec]    Search for best move for positions in file and compare to expected results.");
        _messenger.WriteMessageLine("                                      File must be in EPD format.  Search of each move is limited to time in milliseconds.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("tune [pgn] [ps] [pps] [ws] [i]        Tune evaluation parameters using a particle swarm algorithm.");
        _messenger.WriteMessageLine("                                      pgn = PGN filename, ps = Particle Swarms, pps = Particles Per Swarm.");
        _messenger.WriteMessageLine("                                      ws = Win Scale, i = Iterations.");
        _messenger.WriteMessageLine();

        _messenger.WriteMessageLine("tunewinscale [pgn] [threads]          Compute a scale constant used in the sigmoid function of the tuning algorithm.");
        _messenger.WriteMessageLine("                                      The sigmoid function maps evaluation score to expected win fraction.");
    }
}