// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ErikTheCoder.MadChess.Engine.Tuning;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class UciStream : IDisposable
    {
        public const long NodesInfoInterval = 1000000;
        public const long NodesTimeInterval = 5000;
        public Board Board;
        public Cache Cache;
        public KillerMoves KillerMoves;
        public MoveHistory MoveHistory;
        public Evaluation Evaluation;
        public Search Search;
        public bool Debug;
        private string[] _defaultHalfAndFullMove;
        private const int _cacheSizeMegabytes = 128;
        private const int _minWinPercentScale = 400;
        private const int _maxWinPercentScale = 800;
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
                        string file = $"MadChess-{Guid.NewGuid()}.log";
                        FileStream fileStream = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.Read);
                        _logWriter = new StreamWriter(fileStream) {AutoFlush = true};
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
            Board = new Board(WriteMessageLine);
            Board.PrecalculatedMoves = new PrecalculatedMoves(Board.BishopMoveMasks, Board.RookMoveMasks, Board.CreateMoveDestinationsMask, WriteMessageLine);
            Cache = new Cache(_cacheSizeMegabytes * Cache.CapacityPerMegabyte, Board.ValidateMove);
            KillerMoves = new KillerMoves(Search.MaxHorizon);
            MoveHistory = new MoveHistory();
            Evaluation = new Evaluation(new EvaluationConfig(), Board.GetPositionCount, Board.IsPassedPawn, Board.IsFreePawn);
            Search = new Search(Cache, KillerMoves, MoveHistory, Evaluation, () => Debug, WriteMessageLine);
            _defaultHalfAndFullMove = new[] { "0", "1" };
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


        private void Dispose(bool Disposing)
        {
            if (_disposed) return;
            if (Disposing)
            {
                // Release managed resources.
                Board = null;
                Cache = null;
                KillerMoves = null;
                MoveHistory = null;
                Evaluation = null;
                _defaultHalfAndFullMove = null;
                lock (_messageLock) { _stopwatch = null; }
                _commandStopwatch = null;
                lock (_asyncLock) { _asyncQueue = null; }
                _asyncThread = null;
                _asyncLock = null;
                _messageLock = null;
            }
            // Release unmanaged resources.
            Search?.Dispose();
            Search = null;
            _logWriter?.Dispose();
            _logWriter = null;
            _asyncSignal?.Dispose();
            _asyncSignal = null;
            _disposed = true;
        }


        public void Run()
        {
            // Create async thread.
            _asyncThread = new Thread(MonitorQueue) {Name = "UCI Asynchronous", IsBackground = true};
            _asyncThread.Start();
            // Monitor input stream.
            Thread.CurrentThread.Name = "UCI Synchronous";
            MonitorInputStream();
        }


        public void WriteMessageLine()
        {
            lock (_messageLock)
            {
                Console.WriteLine();
                if (Log) WriteMessageLine(null, CommandDirection.Out);
            }
        }


        public void WriteMessageLine(string Message)
        {
            lock (_messageLock)
            {
                Console.WriteLine(Message);
                if (Log) WriteMessageLine(Message, CommandDirection.Out);
            }
        }


        public void HandleException(Exception Exception)
        {
            Log = true;
            StringBuilder stringBuilder = new StringBuilder();
            Exception exception = Exception;
            do
            {
                // Display message and write to log.
                stringBuilder.AppendLine($"Exception Message = {exception.Message}");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"Exception Type = {exception.GetType().FullName}.");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"Exception StackTrace = {exception.StackTrace}");
                stringBuilder.AppendLine();
                exception = exception.InnerException;
            } while (exception != null);
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


        private void DispatchCommand(string Command)
        {
            if (Command == null) return;
            // Parse command into tokens.
            List<string> tokens = Tokens.Parse(Command, ' ', '"');
            // Do not convert to lowercase because this invalidates FEN strings (where case differentiates white and black pieces).
            if (tokens.Count == 0) return;
            // Determine whether to dispatch command on main thread or async thread.
            switch (tokens[0].ToLowerInvariant())
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


        private void DispatchOnMainThread(List<string> Tokens)
        {
            bool writeMessageLine = true;
            switch (Tokens[0].ToLowerInvariant())
            {
                // Standard commands
                case "uci":
                    Uci();
                    break;
                case "isready":
                    WriteMessageLine("readyok");
                    break;
                case "debug":
                    Debug = Tokens[1].Equals("on", StringComparison.CurrentCultureIgnoreCase);
                    break;
                case "setoption":
                    SetOption(Tokens);
                    break;
                case "ucinewgame":
                    UciNewGame();
                    break;
                case "position":
                    Position(Tokens);
                    break;
                case "go":
                    GoSync(Tokens);
                    writeMessageLine = false;
                    break;
                case "stop":
                    Search.Continue = false;
                    // Wait for search to complete.
                    Search.Signal.WaitOne();
                    break;
                case "quit":
                    Quit(0);
                    break;
                // Extended commands
                case "showboard":
                    WriteMessageLine(Board.ToString());
                    break;
                case "findmagics":
                    FindMagicMultipliers();
                    break;
                case "countmoves":
                    CountMoves(Tokens);
                    break;
                case "dividemoves":
                    DivideMoves(Tokens);
                    break;
                case "listmoves":
                    ListMoves();
                    break;
                case "shiftkillermoves":
                    KillerMoves.Shift(int.Parse(Tokens[1]));
                    break;
                case "showevalparams":
                    WriteMessageLine(Evaluation.ShowParameters());
                    break;
                case "staticscore":
                    WriteMessageLine(Evaluation.ToString(ref Board.CurrentPosition));
                    break;
                case "swapoffscore":
                    SwapOffScore(Tokens);
                    break;
                case "testpositions":
                    TestPositions(Tokens);
                    break;
                case "analyzepositions":
                    AnalyzePositions(Tokens);
                    break;
                case "tune":
                    Tune(Tokens);
                    break;
                case "tunewinpercentscale":
                    TuneWinPercentScale(Tokens);
                    break;
                case "?":
                case "help":
                    Help();
                    break;
                default:
                    WriteMessageLine(Tokens[0] + " command not supported.");
                    break;
            }
            if (writeMessageLine) WriteMessageLine();
        }


        private void DispatchOnAsyncThread(List<string> Tokens)
        {
            lock (_asyncLock)
            {
                // Queue command.
                _asyncQueue.Enqueue(Tokens);
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
                        switch (tokens[0].ToLowerInvariant())
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


        // Standard commands
        private void Uci()
        {
            // Display engine name and author.
            WriteMessageLine("id name MadChess 3.0");
            WriteMessageLine("id author Erik Madsen");
            // TODO: Display engine options.
            WriteMessageLine("option name UCI_EngineAbout type string default MadChess by Erik Madsen.  See http://www.madchess.net.");
            WriteMessageLine("uciok");
        }


        private void SetOption(List<string> Tokens)
        {
            string optionName = Tokens[2];
            string optionValue = Tokens.Count > 4 ? Tokens[4] : string.Empty;
            switch (optionName.ToLowerInvariant())
            {
                case "debug":
                    Debug = optionValue.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    break;
                case "log":
                    Log = optionValue.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    break;
                case "hash":
                    int cacheSizeMegabytes = int.Parse(optionValue);
                    Cache.Capacity = cacheSizeMegabytes * Cache.CapacityPerMegabyte;
                    break;
                case "clearhash":
                    // Reset cache and move heuristics.
                    Cache.Reset();
                    KillerMoves.Reset();
                    MoveHistory.Reset();
                    break;
                case "uci_analysemode":
                case "analyze":
                    bool analysisMode = optionValue.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    if (analysisMode)
                    {
                        Search.TruncatePrincipalVariation = false;
                        Evaluation.DrawMoves = 3;
                    }
                    else
                    {
                        Search.TruncatePrincipalVariation = true;
                        Evaluation.DrawMoves = 2;
                    }
                    break;
                case "multipv":
                    Search.MultiPv = int.Parse(optionValue);
                    break;
                case "nps":
                    if (optionValue == "0") Search.NodesPerSecond = null;
                    else Search.NodesPerSecond = int.Parse(optionValue);
                    break;
                case "moveerror":
                    Search.MoveError = int.Parse(optionValue);
                    break;
                case "blundererror":
                    Search.BlunderError = int.Parse(optionValue);
                    break;
                case "blunderpercent":
                    Search.BlunderPercent = int.Parse(optionValue);
                    break;
                case "uci_limitstrength":
                case "limitstrength":
                    Search.LimitStrength = optionValue.Equals("true", StringComparison.CurrentCultureIgnoreCase);
                    break;
                case "uci_elo":
                case "elo":
                    Search.Elo = int.Parse(optionValue);
                    break;
                default:
                    WriteMessageLine(optionName + " option not supported.");
                    break;
            }
        }


        private void UciNewGame(bool PreserveMoveCount = false)
        {
            // Reset cache and move heuristics.
            Cache.Reset();
            KillerMoves.Reset();
            MoveHistory.Reset();
            // Set up start position.
            Board.SetPosition(Board.StartPositionFen, PreserveMoveCount);
        }


        private void Position(List<string> Tokens)
        {
            // ParseLongAlgebraic FEN.
            // Determine if position specifies moves.
            bool specifiesMoves = false;
            int moveIndex = Tokens.Count;
            for (int index = 2; index < Tokens.Count; index++)
            {
                if (Tokens[index].ToLowerInvariant() == "moves")
                {
                    // Position specifies moves.
                    specifiesMoves = true;
                    if (!char.IsNumber(Tokens[index - 1][0]))
                    {
                        // Position does not specify half or full move number.
                        Tokens.InsertRange(index, _defaultHalfAndFullMove);
                        index += 2;
                    }
                    if (index == Tokens.Count - 1) Tokens.RemoveAt(Tokens.Count - 1);
                    moveIndex = index + 1;
                    break;
                }
            }
            if (!specifiesMoves)
            {
                if (!char.IsNumber(Tokens[Tokens.Count - 1][0]))
                {
                    // Position does not specify half or full move number.
                    Tokens.AddRange(_defaultHalfAndFullMove);
                    moveIndex += 2;
                }
            }
            // Must convert tokens to array to prevent joining class name (System.Collections.Generic.List) instead of string value.
            // This is because the IEnumerable<T> overload does not accept a StartIndex and Count so those parameters are interpreted as params object[].
            string fen = Tokens[1] == "startpos"
                ? Board.StartPositionFen
                : string.Join(" ",Tokens.ToArray(), 2, Tokens.Count - 2);
            // Setup position and play moves if specified.
            Board.SetPosition(fen);
            while (moveIndex < Tokens.Count)
            {
                ulong move = Move.ParseLongAlgebraic(Tokens[moveIndex], Board.CurrentPosition.WhiteMove);
                bool validMove = Board.ValidateMove(ref move);
                if (!validMove || !Board.IsMoveLegal(ref move)) throw new Exception($"Move {Move.ToLongAlgebraic(move)} is illegal in position {Board.CurrentPosition.ToFen()}.");
                Board.PlayMove(move);
                moveIndex++;
            }
        }


        private void GoSync(List<string> Tokens)
        {
            _commandStopwatch.Restart();
            // Reset search and evaluation.  Shift killer moves.
            Search.Reset(false);
            Evaluation.Reset(false);
            KillerMoves.Shift(2);
            for (int tokenIndex = 1; tokenIndex < Tokens.Count; tokenIndex++)
            {
                string token = Tokens[tokenIndex];
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (token.ToLowerInvariant())
                {
                    case "wtime":
                        Search.WhiteTimeRemaining = TimeSpan.FromMilliseconds(int.Parse(Tokens[tokenIndex + 1]));
                        break;
                    case "btime":
                        Search.BlackTimeRemaining = TimeSpan.FromMilliseconds(int.Parse(Tokens[tokenIndex + 1]));
                        break;
                    case "winc":
                        Search.WhiteTimeIncrement = TimeSpan.FromMilliseconds(int.Parse(Tokens[tokenIndex + 1]));
                        break;
                    case "binc":
                        Search.BlackTimeIncrement = TimeSpan.FromMilliseconds(int.Parse(Tokens[tokenIndex + 1]));
                        break;
                    case "movestogo":
                        Search.MovesToTimeControl = int.Parse(Tokens[tokenIndex + 1]);
                        break;
                    case "depth":
                        Search.HorizonLimit = Math.Min(int.Parse(Tokens[tokenIndex + 1]), Search.MaxHorizon);
                        break;
                    case "nodes":
                        Search.NodeLimit = long.Parse(Tokens[tokenIndex + 1]);
                        break;
                    case "mate":
                        Search.MateInMoves = int.Parse(Tokens[tokenIndex + 1]);
                        Search.MoveTimeHardLimit = TimeSpan.MaxValue;
                        Search.WhiteTimeRemaining = TimeSpan.MaxValue;
                        Search.BlackTimeRemaining = TimeSpan.MaxValue;
                        break;
                    case "movetime":
                        Search.MoveTimeHardLimit = TimeSpan.FromMilliseconds(int.Parse(Tokens[tokenIndex + 1]));
                        break;
                    case "infinite":
                        Search.MoveTimeHardLimit = TimeSpan.MaxValue;
                        Search.WhiteTimeRemaining = TimeSpan.MaxValue;
                        Search.BlackTimeRemaining = TimeSpan.MaxValue;
                        break;
                }
            }
        }


        private void GoAsync()
        {
            // Find best move and respond.
            ulong bestMove = Search.FindBestMove(Board);
            WriteMessageLine($"bestmove {Move.ToLongAlgebraic(bestMove)}");
            // Signal search has stopped.
            _commandStopwatch.Stop();
            Search.Signal.Set();
            // Collect memory from unreferenced objects in generation 0 and 1.
            // Do not collect memory from generation 2 which contains the large object heap, since it's mostly arrays whose lifetime is the duration of the application.
            GC.Collect(1, GCCollectionMode.Forced, true, true);
        }


        private void Quit(int ExitCode)
        {
            Dispose(true);
            Environment.Exit(ExitCode);
        }


        // Extended commands
        private void FindMagicMultipliers()
        {
            WriteMessageLine("Square   Piece  Shift  Unique Occupancies  Unique Moves  Magic Multiplier");
            WriteMessageLine("======  ======  =====  ==================  ============  ================");
            // Find magic multipliers for bishop and rook moves.
            // No need to find magic multipliers for queen moves since the queen combines bishop and rook moves.
            Board.PrecalculatedMoves.FindMagicMultipliers(Piece.WhiteBishop, true);
            Board.PrecalculatedMoves.FindMagicMultipliers(Piece.WhiteRook, true);
        }


        private void CountMoves(List<string> Tokens)
        {
            int horizon = int.Parse(Tokens[1].Trim());
            if (horizon <= 0) throw new ArgumentException();
            Board.Nodes = 0;
            Board.NodesInfoUpdate = NodesInfoInterval;
            _commandStopwatch.Restart();
            long moves = CountMoves(0, horizon);
            _commandStopwatch.Stop();
            WriteMessageLine($"Counted {moves:n0} moves in {_commandStopwatch.Elapsed.TotalSeconds:0.000} seconds.");
        }


        private long CountMoves(int Depth, int Horizon)
        {
            if ((Depth < 0) || (Horizon < 0)) throw new ArgumentException();
            if (Board.Nodes >= Board.NodesInfoUpdate)
            {
                // Update move count.
                double nodesPerSecond = Board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
                WriteMessageLine($"Counted {Board.NodesInfoUpdate:n0} nodes ({nodesPerSecond:n0} nodes per second).");
                int intervals = (int) (Board.Nodes / NodesInfoInterval);
                Board.NodesInfoUpdate = NodesInfoInterval * (intervals + 1);
            }
            int toHorizon = Horizon - Depth;
            Board.CurrentPosition.GenerateMoves();
            long moves = 0;
            for (int moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++)
            {
                ulong move = Board.CurrentPosition.Moves[moveIndex];
                if (!Board.IsMoveLegal(ref move)) continue; // Skip illegal move.
                if (toHorizon > 1)
                {
                    Board.PlayMove(move);
                    moves += CountMoves(Depth + 1, Horizon);
                    Board.UndoMove();
                }
                else
                {
                    moves++;
                    Board.Nodes++;
                }
            }
            return moves;
        }


        private void DivideMoves(List<string> Tokens)
        {
            int horizon = int.Parse(Tokens[1].Trim());
            if (horizon < 1) throw new ArgumentException();
            Board.Nodes = 0;
            Board.NodesInfoUpdate = NodesInfoInterval;
            _commandStopwatch.Restart();
            Board.CurrentPosition.GenerateMoves();
            // Count moves for each root move.
            long[] rootMoves = new long[Board.CurrentPosition.MoveIndex];
            for (int moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++)
            {
                ulong move = Board.CurrentPosition.Moves[moveIndex];
                if (!Board.IsMoveLegal(ref move)) continue; // Skip illegal move.
                Board.PlayMove(move);
                rootMoves[moveIndex] = horizon == 1 ? 1 : CountMoves(1, horizon);
                Board.UndoMove();
            }
            _commandStopwatch.Stop();
            // Display move count for each root move.
            int legalMoves = 0;
            WriteMessageLine("Root Move    Moves");
            WriteMessageLine("=========  =======");
            for (int moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++)
            {
                ulong move = Board.CurrentPosition.Moves[moveIndex];
                if (!Board.IsMoveLegal(ref move)) continue; // Skip illegal move.
                legalMoves++;
                WriteMessageLine($"{Move.ToLongAlgebraic(move).PadRight(9)}  {rootMoves[moveIndex].ToString().PadLeft(7)}");
            }
            WriteMessageLine();
            WriteMessageLine(legalMoves + " legal root moves");
        }


        private void ListMoves()
        {
            _commandStopwatch.Restart();
            // Get cached position.
            ref CachedPosition cachedPosition = ref Cache.GetPosition(Board.CurrentPosition.Key);
            ulong bestMove = Cache.GetBestMove(cachedPosition);
            // Generate and sort moves.
            Board.CurrentPosition.GenerateMoves();
            int lastMoveIndex = Board.CurrentPosition.MoveIndex - 1;
            Search.PrioritizeMoves(ref Board.CurrentPosition, Board.CurrentPosition.Moves, lastMoveIndex, bestMove, 0);
            Search.SortMovesByPriority(Board.CurrentPosition.Moves, lastMoveIndex);
            WriteMessageLine("Rank   Move  Best  Cap Victim  Cap Attacker  Promo  Killer  History              Priority");
            WriteMessageLine("====  =====  ====  ==========  ============  =====  ======  =======  ====================");
            StringBuilder stringBuilder = new StringBuilder();
            int legalMoveNumber = 0;
            for (int moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++)
            {
                ulong move = Board.CurrentPosition.Moves[moveIndex];
                if (!Board.IsMoveLegal(ref move)) continue; // Skip illegal move.
                legalMoveNumber++;
                stringBuilder.Clear();
                stringBuilder.Append(legalMoveNumber.ToString("00").PadLeft(4));
                stringBuilder.Append(Move.ToLongAlgebraic(move).PadLeft(7));
                stringBuilder.Append((Move.IsBest(move) ? "True" : string.Empty).PadLeft(6));
                stringBuilder.Append(Piece.GetName(Move.CaptureVictim(move)).PadLeft(12));
                stringBuilder.Append(Piece.GetName(Move.CaptureAttacker(move)).PadLeft(14));
                string promotedPiece = Move.PromotedPiece(move) == Piece.None ? string.Empty : Piece.GetName(Move.PromotedPiece(move));
                stringBuilder.Append(promotedPiece.PadLeft(7));
                stringBuilder.Append(Move.Killer(move).ToString().PadLeft(8));
                stringBuilder.Append(Move.History(move).ToString().PadLeft(9));
                stringBuilder.Append(move.ToString().PadLeft(22));
                WriteMessageLine(stringBuilder.ToString());
            }
            WriteMessageLine();
            WriteMessageLine(legalMoveNumber + " legal moves");
            _commandStopwatch.Stop();
        }


        private void SwapOffScore(List<string> Tokens)
        {
            ulong move = Move.ParseLongAlgebraic(Tokens[1].Trim(), Board.CurrentPosition.WhiteMove);
            int staticScore = Evaluation.GetStaticScore(ref Board.CurrentPosition);
            Search.Reset(false);
            Evaluation.Reset(false);
            int swapOffScore = Search.GetSwapOffScore(Board, 0, 0, move, staticScore, -StaticScore.Max, StaticScore.Max);
            WriteMessageLine(swapOffScore.ToString());
        }
        

        private void TestPositions(List<string> Tokens)
        {
            string file = Tokens[1].Trim();
            WriteMessageLine("Number                                                                     Position  Depth     Expected        Moves  Correct    Pct");
            WriteMessageLine("======  ===========================================================================  =====  ===========  ===========  =======  =====");
            Board.Nodes = 0;
            Board.NodesInfoUpdate = NodesInfoInterval;
            int positions = 0;
            int correctPositions = 0;
            _commandStopwatch.Restart();
            // Verify move counts of test positions.
            using (StreamReader reader = File.OpenText(file))
            {
                while (!reader.EndOfStream)
                {
                    // Load position, horizon, and correct move count.
                    string line = reader.ReadLine();
                    if (line == null) continue;
                    positions++;
                    List<string> tokens = Engine.Tokens.Parse(line, '|', '"');
                    string fen = tokens[0];
                    int horizon = int.Parse(tokens[1]);
                    long expectedMoves = long.Parse(tokens[2]);
                    // Setup position.  Preserve move count.
                    Board.SetPosition(fen, true);
                    // Count nodes.  Do not update node count.
                    Board.NodesInfoUpdate = long.MaxValue;
                    long moves = CountMoves(0, horizon);
                    bool correct = moves == expectedMoves;
                    if (correct) correctPositions++;
                    double percent = 100d * correctPositions / positions;
                    WriteMessageLine($"{positions.ToString().PadLeft(6)}  {fen.PadLeft(75)}  {horizon.ToString("0").PadLeft(5)}  {expectedMoves.ToString("n0").PadLeft(11)}  " +
                        $"{moves.ToString("n0").PadLeft(11)}  {correct.ToString().PadLeft(7)}  {percent.ToString("0.0").PadLeft(5)}");
                }
            }
            _commandStopwatch.Stop();
            // Update node count.
            double nodesPerSecond = Board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
            WriteMessageLine();
            WriteMessageLine($"Counted {Board.Nodes:n0} nodes ({nodesPerSecond:n0} nodes per second).");
        }


        private void AnalyzePositions(IList<string> Tokens)
        {
            string file = Tokens[1].Trim();
            int moveTimeMilliseconds = int.Parse(Tokens[2].Trim());
            int positions = 0;
            int correctPositions = 0;
            using (StreamReader reader = File.OpenText(file))
            {
                WriteMessageLine("Number                                                                     Position  Solution    Expected Moves   Move  Correct    Pct");
                WriteMessageLine("======  ===========================================================================  ========  ================  =====  =======  =====");
                Board.Nodes = 0L;
                Board.NodesInfoUpdate = NodesInfoInterval;
                _commandStopwatch.Restart();
                while (!reader.EndOfStream)
                {
                    // Load position, horizon, and correct move count.
                    string line = reader.ReadLine();
                    if (line == null) continue;
                    positions++;
                    List<string> tokens = Engine.Tokens.Parse(line, ' ', '"');
                    PositionSolution positionSolution = PositionSolution.Unknown;
                    const int illegalIndex = -1;
                    int solutionIndex = illegalIndex;
                    int expectedMovesIndex = illegalIndex;
                    for (int index = 0; index < tokens.Count; index++)
                    {
                        string token = tokens[index].Trim().ToLower();
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (token)
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
                        if (token.EndsWith(";"))
                        {
                            expectedMovesIndex = index;
                            break;
                        }
                    }
                    if (solutionIndex == illegalIndex) throw new Exception("Position does not specify a best moves or avoid moves solution.");
                    if (expectedMovesIndex == illegalIndex) throw new Exception("Position does not terminate the expected moves with a semicolon.");
                    int correctMoves = expectedMovesIndex - solutionIndex;
                    // Must convert tokens to array to prevent joining class name (System.Collections.Generic.List) instead of string value.
                    // This is because the IEnumerable<T> overload does not accept a StartIndex and Count so those parameters are interpreted as params object[].
                    string fen = string.Join(" ", tokens.ToArray(), 0, solutionIndex).Trim();
                    string expectedMovesListStandardAlgebraic = string.Join(" ", tokens.ToArray(), solutionIndex + 1, correctMoves).Trim().TrimEnd(";".ToCharArray());
                    string[] expectedMovesStandardAlgebraic = expectedMovesListStandardAlgebraic.Split(" ".ToCharArray());
                    ulong[] expectedMoves = new ulong[expectedMovesStandardAlgebraic.Length];
                    string[] expectedMovesLongAlgebraic = new string[expectedMovesStandardAlgebraic.Length];
                    // Setup position and reset search.
                    UciNewGame(true);
                    Board.SetPosition(fen, true);
                    for (int moveIndex = 0; moveIndex < expectedMovesStandardAlgebraic.Length; moveIndex++)
                    {
                        string expectedMoveStandardAlgebraic = expectedMovesStandardAlgebraic[moveIndex];
                        ulong expectedMove = Move.ParseStandardAlgebraic(Board, expectedMoveStandardAlgebraic);
                        expectedMoves[moveIndex] = expectedMove;
                        expectedMovesLongAlgebraic[moveIndex] = Move.ToLongAlgebraic(expectedMove);
                    }
                    Search.Reset(true);
                    // Find best move.  Do not update node count or PV.
                    Board.NodesInfoUpdate = long.MaxValue;
                    Search.PvInfoUpdate = false;
                    Search.MoveTimeSoftLimit = TimeSpan.MaxValue;
                    Search.MoveTimeHardLimit = TimeSpan.FromMilliseconds(moveTimeMilliseconds);
                    ulong bestMove = Search.FindBestMove(Board);
                    Search.Signal.Set();
                    // Determine if search found correct move.
                    bool correct;
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (positionSolution)
                    {
                        case PositionSolution.BestMoves:
                            correct = false;
                            for (int moveIndex = 0; moveIndex < expectedMoves.Length; moveIndex++)
                            {
                                ulong expectedMove = expectedMoves[moveIndex];
                                if (Move.Equals(bestMove, expectedMove))
                                {
                                    correct = true;
                                    break;
                                }
                            }
                            break;
                        case PositionSolution.AvoidMoves:
                            correct = true;
                            for (int moveIndex = 0; moveIndex < expectedMoves.Length; moveIndex++)
                            {
                                ulong expectedMove = expectedMoves[moveIndex];
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
                    double percent = 100d * correctPositions / positions;
                    string solution = positionSolution == PositionSolution.BestMoves ? "Best" : "Avoid";
                    WriteMessageLine($"{positions.ToString().PadLeft(6)}  {fen.PadLeft(75)}  {solution.PadLeft(8)}  {string.Join(" ", expectedMovesLongAlgebraic).PadLeft(16)}  " +
                        $"{Move.ToLongAlgebraic(bestMove).PadLeft(5)}  {correct.ToString().PadLeft(7)}  {percent.ToString("0.0").PadLeft(5)}");
                }
            }
            _commandStopwatch.Stop();
            // Update score.
            WriteMessageLine();
            WriteMessageLine($"Solved {correctPositions} of {positions} positions in {_commandStopwatch.Elapsed.TotalSeconds:0} seconds.");
            // Update node count.
            double nodesPerSecond = Board.Nodes / _commandStopwatch.Elapsed.TotalSeconds;
            WriteMessageLine($"Counted {Board.Nodes:n0} nodes ({nodesPerSecond:n0} nodes per second).");
            // Update stats.
            double nullMoveCutoffPercent = 100d * Search.Stats.NullMoveCutoffs / Search.Stats.NullMoves;
            double betaCutoffMoveNumber = (double)Search.Stats.BetaCutoffMoveNumber / Search.Stats.BetaCutoffs;
            double betaCutoffFirstMovePercent = 100d * Search.Stats.BetaCutoffFirstMove / Search.Stats.BetaCutoffs;
            WriteMessageLine();
            WriteMessageLine($"Null Move Cutoffs = {nullMoveCutoffPercent:0.00}% Beta Cutoff Move Number = {betaCutoffMoveNumber:0.00} Beta Cutoff First Move = {betaCutoffFirstMovePercent:0.00}%");
        }


        private void Tune(IList<string> Tokens)
        {
            string pgnFilename = Tokens[1].Trim();
            int particleSwarmsCount = int.Parse(Tokens[2].Trim());
            int particlesPerSwarm = int.Parse(Tokens[3].Trim());
            int winPercentScale = int.Parse(Tokens[4].Trim()); // Use 580 for Gm2600EloGoodGames.pgn.
            int iterations = int.Parse(Tokens[5].Trim());
            _commandStopwatch.Restart();
            ParticleSwarms particleSwarms = new ParticleSwarms(pgnFilename, particleSwarmsCount, particlesPerSwarm, winPercentScale, WriteMessageLine);
            particleSwarms.Optimize(iterations);
            _commandStopwatch.Stop();
        }


        private void TuneWinPercentScale(IList<string> Tokens)
        {
            string pgnFilename = Tokens[1].Trim();
            // Load games.
            _commandStopwatch.Restart();
            WriteMessageLine("Loading games.");
            PgnGames pgnGames = new PgnGames();
            pgnGames.Load(Board, pgnFilename);
            // Count positions.
            long positions = 0;
            for (int index = 0; index < pgnGames.Count; index++)
            {
                PgnGame pgnGame = pgnGames[index];
                positions += pgnGame.Moves.Count;
            }
            int positionsPerSecond = (int)(positions / _commandStopwatch.Elapsed.TotalSeconds);
            WriteMessageLine($"Loaded {pgnGames.Count:n0} games with {positions:n0} positions in {_commandStopwatch.Elapsed.TotalSeconds:0.000} seconds ({positionsPerSecond:n0} positions per second).");
            WriteMessageLine("Tuning win percent scale.");
            WriteMessageLine();
            // Calculate evaluation error of all win percent scales.
            Parameters parameters = ParticleSwarms.CreateParameters();
            const int scales = _maxWinPercentScale - _minWinPercentScale + 1;
            Task[] tasks = new Task[scales];
            double[] evaluationErrors = new double[scales];
            for (int index = 0; index < scales; index++)
            {
                int winPercentScale = _minWinPercentScale + index;
                Particle particle = new Particle(pgnGames, parameters);
                Board board = new Board(WriteMessageLine);
                board.PrecalculatedMoves = new PrecalculatedMoves(board.BishopMoveMasks, board.RookMoveMasks, board.CreateMoveDestinationsMask, WriteMessageLine);
                Cache cache = new Cache(1, board.ValidateMove);
                KillerMoves killerMoves = new KillerMoves(Search.MaxHorizon);
                MoveHistory moveHistory = new MoveHistory();
                Evaluation evaluation = new Evaluation(new EvaluationConfig(), board.GetPositionCount, board.IsPassedPawn, board.IsFreePawn);
                Search search = new Search(cache, killerMoves, moveHistory, evaluation, () => false, WriteMessageLine);
                tasks[index] = Task.Run(() => CalculateEvaluationError(particle, board, search, evaluationErrors, winPercentScale));
            }
            // Wait for particles to calculate evaluation error of all win percent scales.
            Task.WaitAll(tasks);
            // Find best win percent scale.
            int bestWinPercentScale = _minWinPercentScale;
            double bestEvaluationError = double.MaxValue;
            for (int index = 0; index < scales; index++)
            {
                int winPercentScale = _minWinPercentScale + index;
                double evaluationError = evaluationErrors[index];
                if (evaluationError < bestEvaluationError)
                {
                    bestWinPercentScale = winPercentScale;
                    bestEvaluationError = evaluationError;
                }
            }
            WriteMessageLine();
            WriteMessageLine($"Best win percent scale = {bestWinPercentScale}.");
            _commandStopwatch.Stop();
            WriteMessageLine($"Completed tuning of win percent scale in {_commandStopwatch.Elapsed.TotalSeconds:0} seconds.");
        }
        

        private void CalculateEvaluationError(Particle Particle, Board ParticleBoard, Search ParticleSearch, double[] EvaluationErrors, int WinPercentScale)
        {
            int index = WinPercentScale - _minWinPercentScale;
            Particle.CalculateEvaluationError(ParticleBoard, ParticleSearch, WinPercentScale);
            WriteMessageLine($"Win Percent Scale = {WinPercentScale:0000}, Evaluation Error = {Particle.EvaluationError:0.000}");
            EvaluationErrors[index] = Particle.EvaluationError;
        }   


        private void Help()
        {
            WriteMessageLine("MadChess by Erik Madsen.  See http://www.madchess.net.");
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
            WriteMessageLine("listmoves                             List moves in order of priority.  Display heuristics for each move.");
            WriteMessageLine();
            WriteMessageLine("shiftkillermoves [depth]              Shift killer moves deeper by given depth.");
            WriteMessageLine("                                      Useful after go command followed by a position command that includes moves.");
            WriteMessageLine("                                      Without shifting killer moves, the listmoves command will display incorrect killer values.");
            WriteMessageLine();
            WriteMessageLine("showevalparams                        Display evaluation parameters used to calculate static score for a position.");
            WriteMessageLine();
            WriteMessageLine("staticscore                           Display evaluation details of current position.");
            WriteMessageLine();
            WriteMessageLine("swapoffscore [move]                   Display static score if pieces are traded on the destination square of the given move.");
            WriteMessageLine("                                      Move must be specified in long algebraic notation.");
            WriteMessageLine();
            WriteMessageLine("testpositions [filename]              Calculate legal moves for positions in given file and compare to expected results.");
            WriteMessageLine("                                      Each line of file must be formatted as [FEN]|[Depth]|[Legal Move Count].");
            WriteMessageLine();
            WriteMessageLine("analyzepositions [filename] [msec]    Search for best move for positions in given file and compare to expected results.");
            WriteMessageLine("                                      File must be in EPD format.  Search of each move is limited to given time in milliseconds.");
            WriteMessageLine();
            WriteMessageLine("tune [pgn] [ps] [pps] [wps] [i]       Tune evaluation parameters using a particle swarm algorithm.");
            WriteMessageLine("                                      pgn = PGN filename, ps = Particle Swarms, pps = Particles Per Swarm.");
            WriteMessageLine("                                      wps = Win Percent Scale, i = Iterations.");
            WriteMessageLine();
            WriteMessageLine("tunewinpercentscale [pgn]             Compute a scaling constant used in the sigmoid function of the tuning algorithm.");
            WriteMessageLine("                                      The sigmoid function maps evaluation score to expected winning percentage.");
        }


        private void WriteMessageLine(string Message, CommandDirection Direction)
        {
            lock (_messageLock)
            {
                TimeSpan elapsed = _stopwatch.Elapsed;
                _logWriter.Write($"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}  ");
                _logWriter.Write(Direction == CommandDirection.In ? " In   " : " Out  ");
                _logWriter.WriteLine(Message);
            }
        }
    }
}