// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Evaluation;
using ErikTheCoder.MadChess.Engine.Hashtable;
using ErikTheCoder.MadChess.Engine.Score;
using ErikTheCoder.MadChess.Engine.Uci;


namespace ErikTheCoder.MadChess.Engine.Intelligence
{
    public sealed class Search : IDisposable
    {
        public const int MaxHorizon = 64;
        public const int MaxQuietDepth = 32;
        public const int MinElo = 600;
        public const int MaxElo = 2400;
        public AutoResetEvent Signal;
        public bool PvInfoUpdate;
        public List<ulong> SpecifiedMoves;
        public TimeSpan?[] TimeRemaining;
        public TimeSpan?[] TimeIncrement;
        public int? MovesToTimeControl;
        public int? MateInMoves;
        public int HorizonLimit;
        public long NodeLimit;
        public TimeSpan MoveTimeSoftLimit;
        public TimeSpan MoveTimeHardLimit;
        public bool CanAdjustMoveTime;
        public bool TruncatePrincipalVariation;
        public int MultiPv;
        public bool Continue;
        private const int _minMovesRemaining = 8;
        private const int _piecesMovesPer128 = 160;
        private const int _moveTimeHardLimitPer128 = 512;
        private const int _adjustMoveTimeMinDepth = 9;
        private const int _adjustMoveTimeMinScoreDecrease = 33;
        private const int _adjustMoveTimePer128 = 32;
        private const int _haveTimeSearchNextPlyPer128 = 70;
        private const int _aspirationMinHorizon = 5;
        private const int _aspirationWindow = 100;
        private const int _nullMoveReduction = 3;
        private const int _nullStaticScoreReduction = 200;
        private const int _nullStaticScoreMaxReduction = 3;
        private const int _iidReduction = 2;
        private const int _singularMoveMinToHorizon = 7;
        private const int _singularMoveMaxInsufficientDraft = 3;
        private const int _singularMoveReductionPer128 = 64;
        private const int _singularMoveMargin = 2;
        private const int _quietSearchMaxFromHorizon = 3;
        private static MovePriorityComparer _movePriorityComparer;
        private static ScoredMovePriorityComparer _scoredMovePriorityComparer;
        private static MoveScoreComparer _moveScoreComparer;
        private static Delegates.GetStaticScore _getExchangeMaterialScore;
        private readonly TimeSpan _moveTimeReserved = TimeSpan.FromMilliseconds(100);
        private static int[] _futilityMargins;
        private int[] _lateMovePruning;
        private int[] _lateMoveReductions;
        private ScoredMove[] _rootMoves;
        private ScoredMove[] _bestMoves;
        private ScoredMove[] _bestMovePlies;
        private ulong[][] _possibleVariations;
        private int[] _possibleVariationLength;
        private Dictionary<string, ulong[]> _principalVariations;
        private Stats _stats;
        private Cache _cache;
        private KillerMoves _killerMoves;
        private MoveHistory _moveHistory;
        private Eval _eval;
        private Core.Delegates.Debug _debug;
        private Delegates.DisplayStats _displayStats;
        private Core.Delegates.WriteMessageLine _writeMessageLine;
        private Delegates.GetNextMove _getNextMove;
        private Delegates.GetNextMove _getNextCapture;
        private Delegates.GetStaticScore _getStaticScore;
        private Stopwatch _stopwatch;
        private int _originalHorizon;
        private int _selectiveHorizon;
        private ulong _rootMove;
        private int _rootMoveNumber;
        private bool _limitedStrength;
        private int _elo;
        private int? _nodesPerSecond;
        private int _moveError;
        private int _blunderError;
        private int _blunderPer128;
        private bool _disposed;


        public bool LimitedStrength
        {
            get => _limitedStrength;
            set
            {
                _limitedStrength = value;
                if (_limitedStrength)
                {
                    ConfigureLimitedStrength();
                    _eval.ConfigureLimitedStrength(_elo);
                }
                else
                {
                    ConfigureFullStrength();
                    _eval.ConfigureFullStrength();
                }
            }
        }


        public int Elo
        {
            get => _elo;
            set
            {
                _elo = value;
                if (_limitedStrength)
                {
                    ConfigureLimitedStrength();
                    _eval.ConfigureLimitedStrength(_elo);
                }
            }
        }


        private bool CompetitivePlay => !LimitedStrength && (MultiPv == 1);


        static Search()
        {
            _movePriorityComparer = new MovePriorityComparer();
            _scoredMovePriorityComparer = new ScoredMovePriorityComparer();
            _moveScoreComparer = new MoveScoreComparer();
            _getExchangeMaterialScore = Eval.GetExchangeMaterialScore;
        }


        public Search(Stats stats, Cache cache, KillerMoves killerMoves, MoveHistory moveHistory, Eval eval,
            Core.Delegates.Debug debug, Delegates.DisplayStats displayStats, Core.Delegates.WriteMessageLine writeMessageLine)
        {
            _stats = stats;
            _cache = cache;
            _killerMoves = killerMoves;
            _moveHistory = moveHistory;
            _eval = eval;
            _debug = debug;
            _displayStats = displayStats;
            _writeMessageLine = writeMessageLine;
            _getNextMove = GetNextMove;
            _getNextCapture = GetNextCapture;
            _getStaticScore = _eval.GetStaticScore;
            // Create synchronization and diagnostic objects.
            Signal = new AutoResetEvent(false);
            _stopwatch = new Stopwatch();
            // Create search parameters.
            SpecifiedMoves = new List<ulong>();
            TimeRemaining = new TimeSpan?[2];
            TimeIncrement = new TimeSpan?[2];
            // To Horizon =            000  001  002  003  004  005
            _futilityMargins = new[] { 050, 100, 175, 275, 400, 550 };
            _lateMovePruning = new[] { 999, 003, 007, 013, 021, 031 };
            Debug.Assert(_futilityMargins.Length == _lateMovePruning.Length);
            // Quiet Move Number =        000  001  002  003  004  005  006  007  008  009  010  011  012  013  014  015  016  017  018  019  020  021  022  023  024  025  026  027  028  029  030  031
            _lateMoveReductions = new[] { 000, 000, 000, 001, 001, 001, 001, 002, 002, 002, 002, 002, 002, 003, 003, 003, 003, 003, 003, 003, 003, 004, 004, 004, 004, 004, 004, 004, 004, 004, 004, 005 };
            // Create scored move arrays.
            _rootMoves = new ScoredMove[Position.MaxMoves];
            _bestMoves = new ScoredMove[Position.MaxMoves];
            _bestMovePlies = new ScoredMove[MaxHorizon + 1];
            // Create possible and principal variations.
            _possibleVariations = new ulong[MaxHorizon + 1][];
            for (var depth = 0; depth < _possibleVariations.Length; depth++) _possibleVariations[depth] = new ulong[_possibleVariations.Length - depth];
            _possibleVariationLength = new int[MaxHorizon + 1];
            _principalVariations = new Dictionary<string, ulong[]>();
            _disposed = false;
            // Set Multi PV, PV truncation, and search strength.
            MultiPv = 1;
            TruncatePrincipalVariation = true;
            ConfigureFullStrength();
        }


        ~Search()
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
                SpecifiedMoves = null;
                TimeRemaining = null;
                TimeIncrement = null;
                MovesToTimeControl = null;
                MateInMoves = null;
                _movePriorityComparer = null;
                _scoredMovePriorityComparer = null;
                _moveScoreComparer = null;
                _movePriorityComparer = null;
                _scoredMovePriorityComparer = null;
                _moveScoreComparer = null;
                _getExchangeMaterialScore = null;
                _futilityMargins = null;
                _lateMovePruning = null;
                _lateMoveReductions = null;
                _rootMoves = null;
                _bestMoves = null;
                _bestMovePlies = null;
                _movePriorityComparer = null;
                _scoredMovePriorityComparer = null;
                _moveScoreComparer = null;
                _possibleVariations = null;
                _possibleVariationLength = null;
                _principalVariations = null;
                _stats = null;
                _cache = null;
                _killerMoves = null;
                _moveHistory = null;
                _eval = null;
                _debug = null;
                _displayStats = null;
                _writeMessageLine = null;
                _getNextMove = null;
                _getNextCapture = null;
                _getStaticScore = null;
                _stopwatch = null;
            }
            // Release unmanaged resources.
            Signal?.Dispose();
            Signal = null;
            _disposed = true;
        }


        private void ConfigureLimitedStrength()
        {
            // Reset to full strength, then limit search capabilities.
            var elo = _elo;
            ConfigureFullStrength();
            _limitedStrength = true;
            _elo = elo;

            // Limit search speed.  --------------------------------------------------------------------------+
            var scale = 512d; //                                                                              |
            var power = 4d; //                                                                                |
            var constant = 100; //                                                                            |
            var ratingClass = (double) (_elo - MinElo) / 200; //                                              |
            _nodesPerSecond = Eval.GetNonLinearBonus(ratingClass, scale, power, constant); //                 |
            // Rating               600  800  1000   1200    1400    1600    1800     2000     2200     2400  |
            // Nodes Per Second     100  612  8292  41572  131172  320100  663652  1229412  2097252  3359332  |
            //                                                                                                |
            // -----------------------------------------------------------------------------------------------+

            // Enable errors on every move.  -------------------------------------------------------+
            scale = 2d; //                                                                          |
            power = 2d; //                                                                          |
            constant = 10; //                                                                       |
            ratingClass = (double) (MaxElo - _elo) / 200; //                                        |
            _moveError = Eval.GetNonLinearBonus(ratingClass, scale, power, constant); //            |
            // Rating          600  800  1000  1200  1400  1600  1800  2000  2200  2400             |
            // Move Error      172  138   108    82    60    42    28    18    12    10             |
            //                                                                                      |
            // -------------------------------------------------------------------------------------+

            // Enable occasional blunders.  --------------------------------------------------------+
            scale = 8d; //                                                                          |
            power = 2d; //                                                                          |
            constant = 25; //                                                                       |
            _blunderError = Eval.GetNonLinearBonus(ratingClass, scale, power, constant); //         |
            scale = 0.33d; //                                                                       |
            power = 2; //                                                                           |
            constant = 5; //                                                                        |
            _blunderPer128 = Eval.GetNonLinearBonus(ratingClass, scale, power, constant); //        |
            // Rating          600  800  1000  1200  1400  1600  1800  2000  2200  2400             |    
            // Blunder Error   673  537   417   313   225   153    57    41    33    25             |
            // Blunder Per128   31   26    21    16    13    10     6     6     5     5             |
            //                                                                                      |
            // -------------------------------------------------------------------------------------+

            if (_debug())
            {
                _writeMessageLine($"info string LimitStrength = {LimitedStrength}, ELO = {Elo}.");
                _writeMessageLine($"info string NPS = {_nodesPerSecond}, MoveError = {_moveError}, BlunderError = {_blunderError}, BlunderPer128 = {_blunderPer128}.");
            }
        }


        private void ConfigureFullStrength()
        {
            _elo = MinElo;
            _limitedStrength = false;
            _nodesPerSecond = null;
            _moveError = 0;
            _blunderError = 0;
            _blunderPer128 = 0;
        }


        public ulong FindBestMove(Board board)
        {
            // Ensure all root moves are legal.
            board.CurrentPosition.GenerateMoves();
            var legalMoveIndex = 0;
            for (var moveIndex = 0; moveIndex < board.CurrentPosition.MoveIndex; moveIndex++)
            {
                var move = board.CurrentPosition.Moves[moveIndex];
                if (!ShouldSearchMove(move)) continue;
                if (board.IsMoveLegal(ref move))
                {
                    // Move is legal.
                    Move.SetPlayed(ref move, true); // All root moves will be played so set this in advance.
                    board.CurrentPosition.Moves[legalMoveIndex] = move;
                    legalMoveIndex++;
                }
            }
            board.CurrentPosition.MoveIndex = legalMoveIndex;
            if ((legalMoveIndex == 1) && (SpecifiedMoves.Count == 0))
            {
                // Only one legal move found.
                _stopwatch.Stop();
                return board.CurrentPosition.Moves[0];
            }
            // Copy legal moves to root moves and principal variations.
            for (var moveIndex = 0; moveIndex < legalMoveIndex; moveIndex++)
            {
                var move = board.CurrentPosition.Moves[moveIndex];
                _rootMoves[moveIndex] = new ScoredMove(move, -StaticScore.Max);
                var principalVariation = new ulong[Position.MaxMoves];
                principalVariation[0] = Move.Null;
                _principalVariations.Add(Move.ToLongAlgebraic(move), principalVariation);
            }
            var principalVariations = Math.Min(MultiPv, legalMoveIndex);
            // Determine score error.
            var scoreError = ((_blunderError > 0) && (SafeRandom.NextInt(0, 128) < _blunderPer128))
                ? _blunderError // Blunder
                : 0;
            scoreError = Math.Max(scoreError, _moveError);
            // Determine move time.
            GetMoveTime(board.CurrentPosition);
            board.NodesExamineTime = _nodesPerSecond.HasValue ? 1 : UciStream.NodesTimeInterval;
            // Iteratively deepen search.
            _originalHorizon = 0;
            var bestMove = new ScoredMove(Move.Null, -StaticScore.Max);
            do
            {
                _originalHorizon++;
                _selectiveHorizon = 0;
                // Clear principal variations and age move history.
                // The Dictionary enumerator allocates memory which is not desirable when searching positions.
                // However, this occurs only once per ply.
                using (var pvEnumerator = _principalVariations.GetEnumerator())
                {
                    while (pvEnumerator.MoveNext()) pvEnumerator.Current.Value[0] = Move.Null;
                }
                int alpha;
                int beta;
                if (CompetitivePlay || (_originalHorizon < _aspirationMinHorizon))
                {
                    // Search with full alpha / beta window.
                    alpha = -StaticScore.Max;
                    beta = StaticScore.Max;
                }
                else
                {
                    // Search with aspiration window.
                    // This speeds up Multi-PV searches (for analysis and UCI_LimitStrength) but slows down Single-PV searches (competitive play).
                    if (LimitedStrength)
                    {
                        alpha = _rootMoves[0].Score - scoreError - 1;
                        beta = _rootMoves[0].Score + _aspirationWindow;
                    }
                    else
                    {
                        alpha = _rootMoves[principalVariations - 1].Score - _aspirationWindow;
                        beta = _rootMoves[0].Score + _aspirationWindow;
                    }
                    alpha = Math.Max(alpha, -StaticScore.Max);
                    beta = Math.Min(beta, StaticScore.Max);
                }
                // Reset move scores then search moves.
                for (var moveIndex = 0; moveIndex < board.CurrentPosition.MoveIndex; moveIndex++) _rootMoves[moveIndex].Score = -StaticScore.Max;
                var score = GetDynamicScore(board, 0, _originalHorizon, false, alpha, beta);
                if (Math.Abs(score) == StaticScore.Interrupted) break; // Stop searching.
                SortMovesByScore(_rootMoves, board.CurrentPosition.MoveIndex - 1);
                var failHigh = _rootMoves[0].Score >= beta;
                var failHighScore = score;
                var failLow = !failHigh && (_rootMoves[principalVariations - 1].Score <= alpha);
                var failLowScore = MultiPv > 1 ? _rootMoves[principalVariations - 1].Score : score;
                if (failHigh || failLow)
                {
                    if (PvInfoUpdate)
                    {
                        if (failHigh) UpdateStatusFailHigh(board.Nodes, failHighScore);
                        if (failLow) UpdateStatusFailLow(board.Nodes, failLowScore);
                    }
                    // Reset move scores then search moves within infinite alpha / beta window.
                    for (var moveIndex = 0; moveIndex < board.CurrentPosition.MoveIndex; moveIndex++) _rootMoves[moveIndex].Score = -StaticScore.Max;
                    score = GetDynamicScore(board, 0, _originalHorizon, false, -StaticScore.Max, StaticScore.Max);
                    if (Math.Abs(score) == StaticScore.Interrupted) break; // Stop searching.
                    SortMovesByScore(_rootMoves, board.CurrentPosition.MoveIndex - 1);
                }
                // Find best move.
                for (var moveIndex = 0; moveIndex < board.CurrentPosition.MoveIndex; moveIndex++) _bestMoves[moveIndex] = _rootMoves[moveIndex];
                bestMove = _bestMoves[0];
                _bestMovePlies[_originalHorizon] = bestMove;
                if (PvInfoUpdate) UpdateStatus(board, true);
                if (MateInMoves.HasValue && (bestMove.Score >= StaticScore.Checkmate) && (Eval.GetMateMoveCount(bestMove.Score) <= MateInMoves.Value)) break; // Found checkmate in correct number of moves.
                AdjustMoveTime();
                if (!HaveTimeForNextHorizon()) break; // Do not have time to search next ply.
            } while (Continue && (_originalHorizon < HorizonLimit));
            _stopwatch.Stop();
            if (_debug()) _writeMessageLine($"info string Stopping search at {_stopwatch.Elapsed.TotalMilliseconds:0} milliseconds.");
            return scoreError == 0 ? bestMove.Move : GetInferiorMove(board.CurrentPosition, scoreError);
        }


        private bool ShouldSearchMove(ulong move)
        {
            if (SpecifiedMoves.Count == 0) return true; // Search all moves.
            // Search only specified moves.
            for (var moveIndex = 0; moveIndex < SpecifiedMoves.Count; moveIndex++)
            {
                var specifiedMove = SpecifiedMoves[moveIndex];
                if (Move.Equals(move, specifiedMove)) return true;
            }
            return false;
        }
        
        
        private void GetMoveTime(Position position)
        {
            // No need to calculate move time if go command specified move time, horizon limit, or nodes..
            if ((MoveTimeHardLimit != TimeSpan.MaxValue) || (HorizonLimit != MaxHorizon) || (NodeLimit != long.MaxValue)) return;
            // Retrieve time remaining and increment.
            if (!TimeRemaining[(int)position.ColorToMove].HasValue) throw new Exception($"{nameof(TimeRemaining)} for {position.ColorToMove} is null.");
            // ReSharper disable once PossibleInvalidOperationException
            var timeRemaining = TimeRemaining[(int)position.ColorToMove].Value;
            var timeIncrement = TimeIncrement[(int)position.ColorToMove] ?? TimeSpan.Zero;
            if (timeRemaining == TimeSpan.MaxValue) return; // No need to calculate move time if go command specified infinite search.
            timeRemaining -= _stopwatch.Elapsed; // Account for lag between receiving go command and now.
            int movesRemaining;
            if (MovesToTimeControl.HasValue) movesRemaining = MovesToTimeControl.Value;
            else
            {
                // Estimate moves remaining.
                var pieces = Bitwise.CountSetBits(position.Occupancy) - 2; // Don't include kings.
                movesRemaining = (pieces * _piecesMovesPer128) / 128;
            }
            movesRemaining = Math.Max(movesRemaining, _minMovesRemaining);
            // Calculate move time.
            var millisecondsRemaining = timeRemaining.TotalMilliseconds + (movesRemaining * timeIncrement.TotalMilliseconds);
            var milliseconds = millisecondsRemaining / movesRemaining;
            MoveTimeSoftLimit = TimeSpan.FromMilliseconds(milliseconds);
            MoveTimeHardLimit = TimeSpan.FromMilliseconds((milliseconds * _moveTimeHardLimitPer128) / 128);
            if (MoveTimeHardLimit > (timeRemaining - _moveTimeReserved))
            {
                // Prevent loss on time.
                MoveTimeSoftLimit = TimeSpan.FromMilliseconds(timeRemaining.TotalMilliseconds / movesRemaining);
                MoveTimeHardLimit = MoveTimeSoftLimit;
                if (_debug()) _writeMessageLine($"info string Preventing loss on time.  Moves Remaining = {movesRemaining}");
            }
            if (_debug()) _writeMessageLine($"info string MoveTimeSoftLimit = {MoveTimeSoftLimit.TotalMilliseconds:0} MoveTimeHardLimit = {MoveTimeHardLimit.TotalMilliseconds:0}");
        }


        private void AdjustMoveTime()
        {
            if (!CanAdjustMoveTime || (_originalHorizon < _adjustMoveTimeMinDepth) || (MoveTimeSoftLimit == MoveTimeHardLimit)) return;
            if (_bestMovePlies[_originalHorizon].Score >= (_bestMovePlies[_originalHorizon - 1].Score - _adjustMoveTimeMinScoreDecrease)) return;
            // Score has decreased significantly from last ply.
            if (_debug()) _writeMessageLine("Adjusting move time because score has decreased significantly from previous ply.");
            MoveTimeSoftLimit += TimeSpan.FromMilliseconds((MoveTimeSoftLimit.TotalMilliseconds * _adjustMoveTimePer128) / 128);
            if (MoveTimeSoftLimit > MoveTimeHardLimit) MoveTimeSoftLimit = MoveTimeHardLimit;
        }


        private bool HaveTimeForNextHorizon()
        {
            if (MoveTimeSoftLimit == TimeSpan.MaxValue) return true;
            var moveTimePer128 = (int)((128 * _stopwatch.Elapsed.TotalMilliseconds) / MoveTimeSoftLimit.TotalMilliseconds);
            return moveTimePer128 <= _haveTimeSearchNextPlyPer128;
        }


        private ulong GetInferiorMove(Position position, int scoreError)
        {
            // Determine how many moves are within score error.
            var bestScore = _bestMoves[0].Score;
            var worstScore = bestScore - scoreError;
            var inferiorMoves = 0;
            for (var moveIndex = 1; moveIndex < position.MoveIndex; moveIndex++)
            {
                if (_bestMoves[moveIndex].Score < worstScore) break;
                inferiorMoves++;
            }
            // Randomly select a move within score error.
            return _bestMoves[SafeRandom.NextInt(0, inferiorMoves + 1)].Move;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetDynamicScore(Board board, int depth, int horizon, bool isNullMoveAllowed, int alpha, int beta, ulong excludedMove = 0)
        {
            if ((board.Nodes > board.NodesExamineTime) || _nodesPerSecond.HasValue)
            {
                ExamineTimeAndNodes(board.Nodes);
                var intervals = (int) (board.Nodes / UciStream.NodesTimeInterval);
                board.NodesExamineTime = _nodesPerSecond.HasValue
                    ? board.Nodes + 1
                    : UciStream.NodesTimeInterval * (intervals + 1);
            }
            if (!Continue && (_bestMoves[0].Move != Move.Null)) return StaticScore.Interrupted; // Search was interrupted.
            var (terminalDraw, repeatPosition) = _eval.IsTerminalDraw(board.CurrentPosition);
            if ((depth > 0) && terminalDraw) return 0; // Game ends on this move.
            // Get cached position.
            var toHorizon = horizon - depth;
            var historyIncrement = toHorizon * toHorizon;
            var cachedPosition = _cache.GetPosition(board.CurrentPosition.Key);
            ulong bestMove;
            if ((cachedPosition.Key != _cache.NullPosition.Key) && (depth > 0) && !repeatPosition)
            {
                // Not a root or repeat position.
                // Determine if score is cached.
                var cachedScore = GetCachedScore(cachedPosition.Data, depth, horizon, alpha, beta);
                if (cachedScore != StaticScore.NotCached)
                {
                    // Score is cached.
                    if (cachedScore >= beta)
                    {
                        bestMove = _cache.GetBestMove(cachedPosition.Data);
                        if ((bestMove != Move.Null) && Move.IsQuiet(bestMove))
                        {
                            // Assume the quiet best move specified by the cached position would have caused a beta cutoff.
                            // Update history heuristic.
                            _moveHistory.UpdateValue(board.CurrentPosition, bestMove, historyIncrement);
                        }
                    }
                    _stats.CacheScoreCutoff++;
                    return cachedScore;
                }
            }
            if (toHorizon <= 0) return GetQuietScore(board, depth, depth, Board.AllSquaresMask, alpha, beta, _getStaticScore, true); // Search for a quiet position.
            var drawnEndgame = false;
            if (board.CurrentPosition.KingInCheck) board.CurrentPosition.StaticScore = -StaticScore.Max;
            // ReSharper disable once PossibleNullReferenceException
            else if (board.PreviousPosition?.PlayedMove == Move.Null) board.CurrentPosition.StaticScore = -board.PreviousPosition.StaticScore;
            else
            {
                // Even if endgame is drawn, search moves for a swindle (enemy mistake that makes drawn game winnable).
                (board.CurrentPosition.StaticScore, drawnEndgame) = _eval.GetStaticScore(board.CurrentPosition);
            }
            // ReSharper restore PossibleNullReferenceException
            if (IsPositionFutile(board.CurrentPosition, depth, horizon, drawnEndgame, alpha, beta))
            {
                // Position is futile.
                // Position is not the result of best play by both players.
                UpdateBestMoveCache(board.CurrentPosition, depth, horizon, Move.Null, beta, alpha, beta);
                return beta;
            }
            if (isNullMoveAllowed && IsNullMoveAllowed(board.CurrentPosition, beta))
            {
                // Null move is allowed.
                _stats.NullMoves++;
                if (DoesNullMoveCauseBetaCutoff(board, depth, horizon, beta))
                {
                    // Enemy is unable to capitalize on position even if player forfeits right to move.
                    // While forfeiting right to move is illegal, this indicates position is strong.
                    // Position is not the result of best play by both players.
                    UpdateBestMoveCache(board.CurrentPosition, depth, horizon, Move.Null, beta, alpha, beta);
                    _stats.NullMoveCutoffs++;
                    return beta;
                }
            }
            // Get best move.
            bestMove = _cache.GetBestMove(cachedPosition.Data);
            if ((bestMove == Move.Null) && ((beta - alpha) > 1) && (toHorizon > _iidReduction))
            {
                // Cached position in a principal variation does not specify a best move.
                // Find best move via Internal Iterative Deepening.
                GetDynamicScore(board, depth, horizon - _iidReduction, false, alpha, beta);
                cachedPosition = _cache.GetPosition(board.CurrentPosition.Key);
                bestMove = _cache.GetBestMove(cachedPosition.Data);
            }
            var originalAlpha = alpha;
            var bestScore = alpha;
            var legalMoveNumber = 0;
            var quietMoveNumber = 0;
            var moveIndex = -1;
            var lastMoveIndex = board.CurrentPosition.MoveIndex - 1;
            if (depth > 0) board.CurrentPosition.PrepareMoveGeneration();
            do
            {
                ulong move;
                if (depth == 0)
                {
                    // Search root moves.
                    moveIndex++;
                    if (moveIndex == 0)
                    {
                        PrioritizeMoves(board.CurrentPosition, _rootMoves, lastMoveIndex, bestMove, depth);
                        SortMovesByPriority(_rootMoves, lastMoveIndex);
                    }
                    if (moveIndex > lastMoveIndex) break;
                    move = _rootMoves[moveIndex].Move;
                    legalMoveNumber++;
                    _rootMove = move;
                    _rootMoveNumber = legalMoveNumber;
                }
                else
                {
                    // Search moves at current position.
                    (move, moveIndex) = GetNextMove(board.CurrentPosition, Board.AllSquaresMask, depth, bestMove);
                    if (move == Move.Null) break;
                    if (board.IsMoveLegal(ref move)) legalMoveNumber++;
                    else continue; // Skip illegal move.
                    board.CurrentPosition.Moves[moveIndex] = move;
                }
                if (Move.Equals(move, excludedMove)) continue;
                if (IsMoveFutile(board, depth, horizon, move, legalMoveNumber, quietMoveNumber, drawnEndgame, alpha, beta)) continue; // Move is futile.  Skip move.
                if (Move.IsQuiet(move)) quietMoveNumber++;
                var searchHorizon = GetSearchHorizon(board, depth, horizon, move, cachedPosition, quietMoveNumber, drawnEndgame);
                var moveBeta = (legalMoveNumber == 1) || ((MultiPv > 1) && (depth == 0))
                    ? beta // Search with full alpha / beta window.
                    : bestScore + 1; // Search with zero alpha / beta window.
                // Play and search move.
                Move.SetPlayed(ref move, true);
                board.CurrentPosition.Moves[moveIndex] = move;
                board.PlayMove(move);
                var score = -GetDynamicScore(board, depth + 1, searchHorizon, true, -moveBeta, -alpha);
                if (Math.Abs(score) == StaticScore.Interrupted)
                {
                    // Stop searching.
                    board.UndoMove();
                    return score;
                }
                if (score > bestScore)
                {
                    // Move may be stronger than principal variation.
                    if ((moveBeta < beta) || (searchHorizon < horizon))
                    {
                        // Search move at unreduced horizon with full alpha / beta window.
                        score = -GetDynamicScore(board, depth + 1, horizon, true, -beta, -alpha);
                    }
                }
                board.UndoMove();
                if (Math.Abs(score) == StaticScore.Interrupted) return score; // Stop searching.
                if ((score > alpha) && (score < beta) && (depth == 0)) _rootMoves[moveIndex].Score = score; // Update root move score.
                if (score >= beta)
                {
                    // Position is not the result of best play by both players.
                    if (Move.IsQuiet(move))
                    {
                        // Update move heuristics.
                        _killerMoves.UpdateValue(board.CurrentPosition, depth, move);
                        _moveHistory.UpdateValue(board.CurrentPosition, move, historyIncrement);
                        // Decrement move index immediately so as not to include the quiet move that caused the beta cutoff.
                        moveIndex--;
                        while (moveIndex >= 0)
                        {
                            var priorMove = board.CurrentPosition.Moves[moveIndex];
                            if (Move.IsQuiet(priorMove) && Move.Played(priorMove))
                            {
                                // Update history of prior quiet move that failed to produce cutoff.
                                _moveHistory.UpdateValue(board.CurrentPosition, priorMove, -historyIncrement);
                            }
                            moveIndex--;
                        }
                    }
                    UpdateBestMoveCache(board.CurrentPosition, depth, horizon, move, score, alpha, beta);
                    _stats.MovesCausingBetaCutoff++;
                    _stats.BetaCutoffMoveNumber += legalMoveNumber;
                    if (legalMoveNumber == 1) _stats.BetaCutoffFirstMove++;
                    return beta;
                }
                var rootMoveWithinWindow = (depth == 0) && (score > alpha) && (score < beta);
                if (rootMoveWithinWindow || (score > bestScore))
                {
                    // Update possible variation.
                    _possibleVariations[depth][0] = move;
                    var possibleVariationLength = _possibleVariationLength[depth + 1];
                    Array.Copy(_possibleVariations[depth + 1], 0, _possibleVariations[depth], 1, possibleVariationLength);
                    _possibleVariationLength[depth] = possibleVariationLength + 1;
                    if (depth == 0)
                    {
                        // Update principal variation.
                        var principalVariation = _principalVariations[Move.ToLongAlgebraic(move)];
                        Array.Copy(_possibleVariations[0], 0, principalVariation, 0, _possibleVariationLength[0]);
                        principalVariation[_possibleVariationLength[0]] = Move.Null; // Mark last move of principal variation.
                    }
                }
                if (score > bestScore)
                {
                    // Found new principal variation.
                    bestScore = score;
                    UpdateBestMoveCache(board.CurrentPosition, depth, horizon, move, score, alpha, beta);
                    if ((depth > 0) || CompetitivePlay) alpha = score;
                }
                if ((_bestMoves[0].Move != Move.Null) && (board.Nodes >= board.NodesInfoUpdate))
                {
                    // Update status.
                    UpdateStatus(board, false);
                    var intervals = (int) (board.Nodes / UciStream.NodesInfoInterval);
                    board.NodesInfoUpdate = UciStream.NodesInfoInterval * (intervals + 1);
                }
            } while (true);
            if (legalMoveNumber == 0)
            {
                // Checkmate or Stalemate
                bestScore = board.CurrentPosition.KingInCheck ? Eval.GetMateScore(depth) : 0;
                _possibleVariationLength[depth] = 0;
            }
            if (bestScore <= originalAlpha) UpdateBestMoveCache(board.CurrentPosition, depth, horizon, Move.Null, bestScore, originalAlpha, beta); // Score fails low.
            return bestScore;
        }


        public int GetExchangeScore(Board board, ulong move)
        {
            var (scoreBeforeMove, _) = _getExchangeMaterialScore(board.CurrentPosition);
            board.PlayMove(move);
            var scoreAfterMove = -GetQuietScore(board, 0, 0, Board.SquareMasks[(int)Move.To(move)], -StaticScore.Max, StaticScore.Max, _getExchangeMaterialScore, false);
            board.UndoMove();
            return scoreAfterMove - scoreBeforeMove;
        }
        
        
        public int GetQuietScore(Board board, int depth, int horizon, int alpha, int beta) => GetQuietScore(board, depth, horizon, Board.AllSquaresMask, alpha, beta, _getStaticScore, true);


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetQuietScore(Board board, int depth, int horizon, ulong toSquareMask, int alpha, int beta, Delegates.GetStaticScore getStaticScore, bool considerFutility)
        {
            if ((board.Nodes > board.NodesExamineTime) || _nodesPerSecond.HasValue)
            {
                ExamineTimeAndNodes(board.Nodes);
                var intervals = board.Nodes / UciStream.NodesTimeInterval; board.NodesExamineTime = _nodesPerSecond.HasValue
                    ? board.Nodes + 1
                    : UciStream.NodesTimeInterval * (intervals + 1);
            }
            if (!Continue && (_bestMoves[0].Move != Move.Null)) return StaticScore.Interrupted; // Search was interrupted.
            var (terminalDraw, _) = _eval.IsTerminalDraw(board.CurrentPosition);
            if ((depth > 0) && terminalDraw) return 0; // Game ends on this move.
            // Search for a quiet position where no captures are possible.
            var fromHorizon = depth - horizon;
            _selectiveHorizon = Math.Max(depth, _selectiveHorizon);
            var drawnEndgame = false;
            Delegates.GetNextMove getNextMove;
            ulong moveGenerationToSquareMask;
            if (board.CurrentPosition.KingInCheck)
            {
                // King is in check.  Search all moves.
                getNextMove = _getNextMove;
                moveGenerationToSquareMask = Board.AllSquaresMask;
                board.CurrentPosition.StaticScore = -StaticScore.Max; // Don't evaluate static score since moves when king is in check are not futile.
            }
            else
            {
                // King is not in check.  Search only captures.
                getNextMove = _getNextCapture;
                if ((fromHorizon > _quietSearchMaxFromHorizon) && !board.PreviousPosition.KingInCheck)
                {
                    var lastMoveToSquare = Move.To(board.PreviousPosition.PlayedMove);
                    moveGenerationToSquareMask = lastMoveToSquare == Square.Illegal
                        ? toSquareMask
                        : Board.SquareMasks[(int)lastMoveToSquare]; // Search only recaptures.
                }
                else moveGenerationToSquareMask = toSquareMask;
                // ReSharper disable PossibleNullReferenceException
                if (board.PreviousPosition?.PlayedMove == Move.Null) board.CurrentPosition.StaticScore = -board.PreviousPosition.StaticScore;
                else
                {
                    // Even if endgame is drawn, search moves for a swindle (enemy mistake that makes drawn game winnable).
                    (board.CurrentPosition.StaticScore, drawnEndgame) = getStaticScore(board.CurrentPosition);
                }
                // ReSharper restore PossibleNullReferenceException
                if (board.CurrentPosition.StaticScore >= beta) return beta; // Prevent worsening of position by making a bad capture.  Stand pat.
                alpha = Math.Max(board.CurrentPosition.StaticScore, alpha);
            }
            var legalMoveNumber = 0;
            board.CurrentPosition.PrepareMoveGeneration();
            do
            {
                var (move, _) = getNextMove(board.CurrentPosition, moveGenerationToSquareMask, depth, Move.Null); // Don't retrieve (or update) best move from the cache.  Rely on MVV / LVA move order.
                if (move == Move.Null) break;
                if (board.IsMoveLegal(ref move)) legalMoveNumber++; // Move is legal.
                else continue; // Skip illegal move.
                if (considerFutility && IsMoveFutile(board, depth, horizon, move, legalMoveNumber, 0, drawnEndgame, alpha, beta)) continue; // Move is futile.  Skip move.
                // Play and search move.
                board.PlayMove(move);
                var score = -GetQuietScore(board, depth + 1, horizon, toSquareMask, -beta, -alpha, getStaticScore, considerFutility);
                board.UndoMove();
                if (Math.Abs(score) == StaticScore.Interrupted) return score; // Stop searching.
                if (score >= beta) return beta; // Position is not the result of best play by both players.
                alpha = Math.Max(score, alpha);
            } while (true);
            if ((legalMoveNumber == 0) && board.CurrentPosition.KingInCheck) return Eval.GetMateScore(depth); // Game ends on this move.
            // Return score of best move.
            return alpha;
        }


        private void ExamineTimeAndNodes(long nodes)
        {
            if (nodes >= NodeLimit)
            {
                // Have passed node limit.
                Continue = false;
                return;
            }
            if (_nodesPerSecond.HasValue && (_originalHorizon > 1)) // Guarantee to search at least one ply.
            {
                // Slow search until it's less than specified nodes per second or until soft time limit is exceeded.
                var nodesPerSecond = int.MaxValue;
                while (nodesPerSecond > _nodesPerSecond)
                {
                    // Delay search but keep CPU busy to simulate "thinking".
                    nodesPerSecond = (int)(nodes / _stopwatch.Elapsed.TotalSeconds);
                    if (_stopwatch.Elapsed >= MoveTimeSoftLimit)
                    {
                        // No time is available to continue searching.
                        Continue = false;
                        return;
                    }
                }
            }
            // Search at full speed until hard time limit is exceeded.
            Continue = _stopwatch.Elapsed < MoveTimeHardLimit;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPositionFutile(Position position, int depth, int horizon, bool isDrawnEndgame, int alpha, int beta)
        {
            var toHorizon = horizon - depth;
            if (toHorizon >= _futilityMargins.Length) return false; // Position far from search horizon is not futile.
            if (isDrawnEndgame || (depth == 0) || position.KingInCheck) return false; // Position in drawn endgame, at root, or when king is in check is not futile.
            if ((Math.Abs(alpha) >= StaticScore.Checkmate) || (Math.Abs(beta) >= StaticScore.Checkmate)) return false; // Position under threat of checkmate is not futile.
            // Count pawns and pieces (but don't include kings).
            var whitePawnsAndPieces = Bitwise.CountSetBits(position.WhiteOccupancy) - 1;
            var blackPawnsAndPieces = Bitwise.CountSetBits(position.BlackOccupancy) - 1;
            if ((whitePawnsAndPieces == 0) || (blackPawnsAndPieces == 0)) return false; // Position with lone king on board is not futile.
            // Determine if any move can lower score to beta.
            var futilityMargin = toHorizon <= 0 ? _futilityMargins[0] : _futilityMargins[toHorizon];
            return position.StaticScore - futilityMargin > beta;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNullMoveAllowed(Position position, int beta)
        {
            if ((position.StaticScore < beta) || position.KingInCheck) return false;
            // Do not attempt null move in pawn endgames.  Side to move may be in zugzwang.
            var minorAndMajorPieces = Bitwise.CountSetBits(position.GetMajorAndMinorPieces(position.ColorToMove));
            return minorAndMajorPieces > 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DoesNullMoveCauseBetaCutoff(Board board, int depth, int horizon, int beta)
        {
            var reduction = _nullMoveReduction + Math.Min((board.CurrentPosition.StaticScore - beta) / _nullStaticScoreReduction, _nullStaticScoreMaxReduction);
            board.PlayNullMove();
            // Do not play two null moves consecutively.  Search with zero alpha / beta window.
            var score = -GetDynamicScore(board, depth + 1, horizon - reduction, false, -beta, -beta + 1);
            board.UndoMove();
            return score >= beta;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public (ulong Move, int MoveIndex) GetNextMove(Position position, ulong toSquareMask, int depth, ulong bestMove)
        {
            while (true)
            {
                int firstMoveIndex;
                int lastMoveIndex;
                if (position.CurrentMoveIndex < position.MoveIndex)
                {
                    var moveIndex = position.CurrentMoveIndex;
                    var move = position.Moves[moveIndex];
                    position.CurrentMoveIndex++;
                    var generatedBestMove = (moveIndex > 0) && Move.Equals(move, bestMove);
                    if (Move.Played(move) || generatedBestMove) continue; // Don't play move twice.
                    return (move, moveIndex);
                }
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (position.MoveGenerationStage)
                {
                    case MoveGenerationStage.BestMove:
                        // Find pinned pieces and set best move.
                        position.FindPinnedPieces();
                        if (bestMove != Move.Null)
                        {
                            Move.SetIsBest(ref bestMove, true);
                            position.Moves[position.MoveIndex] = bestMove;
                            position.MoveIndex++;
                        }
                        position.MoveGenerationStage++;
                        continue;
                    case MoveGenerationStage.Captures:
                        firstMoveIndex = position.MoveIndex;
                        position.GenerateMoves(MoveGeneration.OnlyCaptures, Board.AllSquaresMask, toSquareMask);
                        // Prioritize and sort captures.
                        lastMoveIndex = Math.Max(firstMoveIndex, position.MoveIndex - 1);
                        if (lastMoveIndex > firstMoveIndex)
                        {
                            PrioritizeMoves(position, position.Moves, firstMoveIndex, lastMoveIndex, bestMove, depth);
                            SortMovesByPriority(position.Moves, firstMoveIndex, lastMoveIndex);
                        }
                        position.MoveGenerationStage++;
                        continue;
                    case MoveGenerationStage.NonCaptures:
                        firstMoveIndex = position.MoveIndex;
                        position.GenerateMoves(MoveGeneration.OnlyNonCaptures, Board.AllSquaresMask, toSquareMask);
                        // Prioritize and sort non-captures.
                        lastMoveIndex = Math.Max(firstMoveIndex, position.MoveIndex - 1);
                        if (lastMoveIndex > firstMoveIndex)
                        {
                            PrioritizeMoves(position, position.Moves, firstMoveIndex, lastMoveIndex, bestMove, depth);
                            SortMovesByPriority(position.Moves, firstMoveIndex, lastMoveIndex);
                        }
                        position.MoveGenerationStage++;
                        continue;
                    case MoveGenerationStage.End:
                        return (Move.Null, position.CurrentMoveIndex);
                }
                break;
            }
            return (Move.Null, position.CurrentMoveIndex);
        }


        // Pass BestMove parameter even though it isn't referenced to satisfy GetNextMove delegate signature.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static (ulong Move, int MoveIndex) GetNextCapture(Position position, ulong toSquareMask, int depth, ulong bestMove)
        {
            while (true)
            {
                if (position.CurrentMoveIndex < position.MoveIndex)
                {
                    var moveIndex = position.CurrentMoveIndex;
                    var move = position.Moves[moveIndex];
                    position.CurrentMoveIndex++;
                    if (Move.CaptureVictim(move) == Piece.None) continue;
                    return (move, moveIndex);
                }
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (position.MoveGenerationStage)
                {
                    case MoveGenerationStage.BestMove:
                    case MoveGenerationStage.Captures:
                        position.FindPinnedPieces();
                        var firstMoveIndex = position.MoveIndex;
                        position.GenerateMoves(MoveGeneration.OnlyCaptures, Board.AllSquaresMask, toSquareMask);
                        var lastMoveIndex = Math.Max(firstMoveIndex, position.MoveIndex - 1);
                        if (lastMoveIndex > firstMoveIndex) SortMovesByPriority(position.Moves, firstMoveIndex, lastMoveIndex); // Don't prioritize moves before sorting.  MVV / LVA is good enough when ordering captures.
                        position.MoveGenerationStage = MoveGenerationStage.End; // Skip non-captures.
                        continue;
                    case MoveGenerationStage.End:
                        return (Move.Null, position.CurrentMoveIndex);
                }
                break;
            }
            return (Move.Null, position.CurrentMoveIndex);
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool IsMoveFutile(Board board, int depth, int horizon, ulong move, int legalMoveNumber, int quietMoveNumber, bool drawnEndgame, int alpha, int beta)
        {
            Debug.Assert(_futilityMargins.Length == _lateMovePruning.Length);
            var toHorizon = horizon - depth;
            if (toHorizon >= _futilityMargins.Length) return false; // Move far from search horizon is not futile.
            if ((depth == 0) || (legalMoveNumber == 1)) return false; // Root move or first move is not futile.
            if (drawnEndgame || Move.IsCheck(move) || board.CurrentPosition.KingInCheck) return false; // Move in drawn endgame, checking move, or move when king is in check is not futile.
            if ((Math.Abs(alpha) >= StaticScore.Checkmate) || (Math.Abs(beta) >= StaticScore.Checkmate)) return false; // Move under threat of checkmate is not futile.
            var captureVictim = Move.CaptureVictim(move);
            var capture = captureVictim != Piece.None;
            if (capture && (toHorizon > 0)) return false; // Capture in main search is not futile.
            if ((Move.Killer(move) > 0) || (Move.PromotedPiece(move) != Piece.None) || Move.IsCastling(move)) return false; // Killer move, pawn promotion, or castling is not futile.
            if (Move.IsPawnMove(move))
            {
                var rank = Board.Ranks[(int)board.CurrentPosition.ColorToMove][(int)Move.To(move)];
                if (rank >= 6) return false; // Pawn push to 7th rank is not futile.
            }
            // Count pawns and pieces (but don't include kings).
            var whitePawnsAndPieces = Bitwise.CountSetBits(board.CurrentPosition.WhiteOccupancy) - 1;
            var blackPawnsAndPieces = Bitwise.CountSetBits(board.CurrentPosition.BlackOccupancy) - 1;
            if ((whitePawnsAndPieces == 0) || (blackPawnsAndPieces == 0)) return false; // Move with lone king on board is not futile.
            var lateMoveNumber = toHorizon <= 0 ? _lateMovePruning[0] : _lateMovePruning[toHorizon];
            if (Move.IsQuiet(move) && (quietMoveNumber >= lateMoveNumber)) return true; // Quiet move is too late to be worth searching.
            // Determine if move can raise score to alpha.
            var futilityMargin = toHorizon <= 0 ? _futilityMargins[0] : _futilityMargins[toHorizon];
            return board.CurrentPosition.StaticScore + _eval.GetMaterialScore(board.CurrentPosition, captureVictim) + futilityMargin < alpha;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetSearchHorizon(Board board, int depth, int horizon, ulong move, CachedPosition cachedPosition, int quietMoveNumber, bool drawnEndgame)
        {
            if (Move.IsBest(move) && IsBestMoveSingular(board, depth, horizon, move, cachedPosition))
            {
                // The best move (from the cache) is singular.  That is, it's the only good move in the position.
                // Evaluation of the current position relies on the accuracy of the singular move's score.
                // If the engine misjudges the singular move, the position could deteriorate because no alternative strong moves exist.
                // To increase confidence in the singular move's score, search it one ply deeper.
                return horizon + 1;
            }
            if ((depth == 0) && !CompetitivePlay) return horizon; // Do not reduce root move in Multi-PV searches or when engine playing strength is reduced.
            var capture = Move.CaptureVictim(move) != Piece.None;
            if (capture) return horizon; // Do not reduce capture.
            if (drawnEndgame || Move.IsCheck(move) || board.CurrentPosition.KingInCheck) return horizon; // Do not reduce move in drawn endgame, checking move, or move when king is in check.
            if ((Move.Killer(move) > 0) || (Move.PromotedPiece(move) != Piece.None) || Move.IsCastling(move)) return horizon; // Do not reduce killer move, pawn promotion, or castling.
            if (Move.IsPawnMove(move))
            {
                var rank = Board.Ranks[(int)board.CurrentPosition.ColorToMove][(int)Move.To(move)];
                if (rank >= 6) return horizon; // Do not reduce pawn push to 7th rank.
            }
            // Count pawns and pieces (but don't include kings).
            var whitePawnsAndPieces = Bitwise.CountSetBits(board.CurrentPosition.WhiteOccupancy) - 1;
            var blackPawnsAndPieces = Bitwise.CountSetBits(board.CurrentPosition.BlackOccupancy) - 1;
            if ((whitePawnsAndPieces == 0) || (blackPawnsAndPieces == 0)) return horizon; // Do not reduce move with lone king on board.
            if (!Move.IsQuiet(move)) return horizon;
            // Reduce search horizon of late move.
            var quietMoveIndex = Math.Min(quietMoveNumber, _lateMoveReductions.Length - 1);
            return horizon - _lateMoveReductions[quietMoveIndex];
        }


        // Idea from Stockfish chess engine.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool IsBestMoveSingular(Board board, int depth, int horizon, ulong move, CachedPosition cachedPosition)
        {
            // Determine if best move that had failed high in recent searches is best by a significant margin.
            var toHorizon = horizon - depth;
            if ((depth == 0) || (toHorizon < _singularMoveMinToHorizon)) return false;
            var score = CachedPositionData.Score(cachedPosition.Data);
            if ((score == StaticScore.NotCached) || (Math.Abs(score) >= StaticScore.Checkmate)) return false;
            if (CachedPositionData.ScorePrecision(cachedPosition.Data) != ScorePrecision.LowerBound) return false;
            if (CachedPositionData.ToHorizon(cachedPosition.Data) < (toHorizon - _singularMoveMaxInsufficientDraft)) return false;
            var beta = score - (_singularMoveMargin * toHorizon);
            var searchHorizon = depth + ((toHorizon * _singularMoveReductionPer128) / 128);
            score = GetDynamicScore(board, depth, searchHorizon, false, beta - 1, beta, move);
            return score < beta;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetCachedScore(ulong cachedPositionData, int depth, int horizon, int alpha, int beta)
        {
            var score = CachedPositionData.Score(cachedPositionData);
            if (score == StaticScore.NotCached) return StaticScore.NotCached; // Score is not cached.
            var toHorizon = horizon - depth;
            var cachedToHorizon = CachedPositionData.ToHorizon(cachedPositionData);
            if (cachedToHorizon < toHorizon) return StaticScore.NotCached; // Cached position is shallower than current horizon. Do not use cached score.
            if (Math.Abs(score) >= StaticScore.Checkmate)
            {
                // Adjust checkmate score.
                if (score > 0) score -= depth;
                else score += depth;
            }
            var scorePrecision = CachedPositionData.ScorePrecision(cachedPositionData);
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (scorePrecision)
            {
                case ScorePrecision.Exact:
                    // Score is exact.
                    if (score <= alpha) return alpha; // Score fails low.
                    if (score >= beta) return beta; // Score fails high.
                    // If necessary, avoid truncating the principal variation by returning a cached score.
                    return TruncatePrincipalVariation ? score : StaticScore.NotCached;
                case ScorePrecision.UpperBound:
                    // Score is upper bound.
                    if (score <= alpha) return alpha; // Score fails low.
                    break;
                case ScorePrecision.LowerBound:
                    // Score is lower bound.
                    if (score >= beta) return beta; // Score fails high.
                    break;
                default:
                    throw new Exception($"{scorePrecision} score precision not supported.");
            }
            return StaticScore.NotCached;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrioritizeMoves(Position position, ScoredMove[] moves, int lastMoveIndex, ulong bestMove, int depth)
        {
            for (var moveIndex = 0; moveIndex <= lastMoveIndex; moveIndex++)
            {
                var move = moves[moveIndex].Move;
                // Prioritize best move.
                Move.SetIsBest(ref move, Move.Equals(move, bestMove));
                // Prioritize killer moves.
                Move.SetKiller(ref move, _killerMoves.GetValue(position, depth, move));
                // Prioritize by move history.
                Move.SetHistory(ref move, _moveHistory.GetValue(position, move));
                moves[moveIndex].Move = move;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrioritizeMoves(Position position, ulong[] moves, int lastMoveIndex, ulong bestMove, int depth) => PrioritizeMoves(position, moves, 0, lastMoveIndex, bestMove, depth);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrioritizeMoves(Position position, ulong[] moves, int firstMoveIndex, int lastMoveIndex, ulong bestMove, int depth)
        {
            for (var moveIndex = firstMoveIndex; moveIndex <= lastMoveIndex; moveIndex++)
            {
                var move = moves[moveIndex];
                // Prioritize best move.
                Move.SetIsBest(ref move, Move.Equals(move, bestMove));
                // Prioritize killer moves.
                Move.SetKiller(ref move, _killerMoves.GetValue(position, depth, move));
                // Prioritize by move history.
                Move.SetHistory(ref move, _moveHistory.GetValue(position, move));
                moves[moveIndex] = move;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SortMovesByPriority(ScoredMove[] moves, int lastMoveIndex) => Array.Sort(moves, 0, lastMoveIndex + 1, _scoredMovePriorityComparer);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SortMovesByScore(ScoredMove[] moves, int lastMoveIndex) => Array.Sort(moves, 0, lastMoveIndex + 1, _moveScoreComparer);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SortMovesByPriority(ulong[] moves, int lastMoveIndex) => Array.Sort(moves, 0, lastMoveIndex + 1, _movePriorityComparer);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SortMovesByPriority(ulong[] moves, int firstMoveIndex, int lastMoveIndex) => Array.Sort(moves, firstMoveIndex, lastMoveIndex - firstMoveIndex + 1, _movePriorityComparer);


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void UpdateBestMoveCache(Position currentPosition, int depth, int horizon, ulong bestMove, int score, int alpha, int beta)
        {
            if (Math.Abs(score) == StaticScore.Interrupted) return;
            var cachedPosition = _cache.NullPosition;
            cachedPosition.Key = currentPosition.Key;
            CachedPositionData.SetToHorizon(ref cachedPosition.Data, horizon - depth);
            if (bestMove != Move.Null)
            {
                // Set best move.
                CachedPositionData.SetBestMoveFrom(ref cachedPosition.Data, Move.From(bestMove));
                CachedPositionData.SetBestMoveTo(ref cachedPosition.Data, Move.To(bestMove));
                CachedPositionData.SetBestMovePromotedPiece(ref cachedPosition.Data, Move.PromotedPiece(bestMove));
            }
            var adjustedScore = score;
            if (Math.Abs(adjustedScore) >= StaticScore.Checkmate) adjustedScore += adjustedScore > 0 ? depth : -depth; // Adjust checkmate score.
            // Update score.
            if (adjustedScore <= alpha)
            {
                CachedPositionData.SetScorePrecision(ref cachedPosition.Data, ScorePrecision.UpperBound);
                CachedPositionData.SetScore(ref cachedPosition.Data, alpha);
            }
            else if (adjustedScore >= beta)
            {
                CachedPositionData.SetScorePrecision(ref cachedPosition.Data, ScorePrecision.LowerBound);
                CachedPositionData.SetScore(ref cachedPosition.Data, beta);
            }
            else
            {
                CachedPositionData.SetScorePrecision(ref cachedPosition.Data, ScorePrecision.Exact);
                CachedPositionData.SetScore(ref cachedPosition.Data, adjustedScore);
            }
            _cache.SetPosition(cachedPosition);
        }


        // TODO: Resolve issue involving illegal PVs.  See http://talkchess.com/forum3/viewtopic.php?p=892120#p892120.
        private void UpdateStatus(Board board, bool includePrincipalVariation)
        {
            var milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            var nodesPerSecond = board.Nodes / _stopwatch.Elapsed.TotalSeconds;
            var nodes = includePrincipalVariation ? board.Nodes : board.NodesInfoUpdate;
            var hashFull = (int)((1000L * _cache.Positions) / _cache.Capacity);
            if (includePrincipalVariation)
            {
                var principalVariations = Math.Min(MultiPv, board.CurrentPosition.MoveIndex);
                for (var pv = 0; pv < principalVariations; pv++)
                {
                    var principalVariation = _principalVariations[Move.ToLongAlgebraic(_bestMoves[pv].Move)];
                    // TODO: Determine if PV can be constructed without allocating memory via StringBuilder instance.
                    var stringBuilder = new StringBuilder("pv");
                    for (var moveIndex = 0; moveIndex < principalVariation.Length; moveIndex++)
                    {
                        var move = principalVariation[moveIndex];
                        if (move == Move.Null) break;  // Null move marks the last move of the principal variation.
                        stringBuilder.Append(' ');
                        stringBuilder.Append(Move.ToLongAlgebraic(move));
                    }
                    var pvLongAlgebraic = stringBuilder.ToString();
                    var score = _bestMoves[pv].Score;
                    var multiPvPhrase = MultiPv > 1 ? $"multipv {pv + 1} " : null;
                    var scorePhrase = Math.Abs(score) >= StaticScore.Checkmate ? $"mate {Eval.GetMateMoveCount(score)}" : $"cp {score}";
                    _writeMessageLine($"info {multiPvPhrase}depth {_originalHorizon} seldepth {Math.Max(_selectiveHorizon, _originalHorizon)} " +
                                      $"time {milliseconds:0} nodes {nodes} score {scorePhrase} nps {nodesPerSecond:0} {pvLongAlgebraic}");
                }
            }
            else
            {
                _writeMessageLine($"info depth {_originalHorizon} seldepth {Math.Max(_selectiveHorizon, _originalHorizon)} time {milliseconds:0} nodes {nodes} nps {nodesPerSecond:0}");
            }
            _writeMessageLine($"info hashfull {hashFull:0} currmove {Move.ToLongAlgebraic(_rootMove)} currmovenumber {_rootMoveNumber}");
            if (_debug()) _displayStats();
            var intervals = (int) (board.Nodes / UciStream.NodesInfoInterval);
            board.NodesInfoUpdate = UciStream.NodesInfoInterval * (intervals + 1);
        }


        private void UpdateStatusFailHigh(long nodes, int score)
        {
            var milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            var nodesPerSecond = nodes / _stopwatch.Elapsed.TotalSeconds;
            _writeMessageLine($"info depth {_originalHorizon} seldepth {_selectiveHorizon} score lowerbound {score} time {milliseconds:0} nodes {nodes} nps {nodesPerSecond:0}");
        }


        private void UpdateStatusFailLow(long nodes, int score)
        {
            var milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            var nodesPerSecond = nodes / _stopwatch.Elapsed.TotalSeconds;
            _writeMessageLine($"info depth {_originalHorizon} seldepth {_selectiveHorizon} score upperbound {score} time {milliseconds:0} nodes {nodes} nps {nodesPerSecond:0}");
        }


        public void Reset()
        {
            _stopwatch.Restart();
            SpecifiedMoves.Clear();
            // Reset move times and limits.
            TimeRemaining[(int)Color.White] = null;
            TimeRemaining[(int)Color.Black] = null;
            MovesToTimeControl = null;
            MateInMoves = null;
            HorizonLimit = MaxHorizon;
            NodeLimit = long.MaxValue;
            MoveTimeSoftLimit = TimeSpan.MaxValue;
            MoveTimeHardLimit = TimeSpan.MaxValue;
            CanAdjustMoveTime = true;
            // Reset best moves, possible and principal variations, and stats.
            for (var moveIndex = 0; moveIndex < _bestMoves.Length; moveIndex++) _bestMoves[moveIndex] = new ScoredMove(Move.Null, -StaticScore.Max);
            for (var depth = 0; depth < _bestMovePlies.Length; depth++) _bestMovePlies[depth] = new ScoredMove(Move.Null, -StaticScore.Max);
            for (var depth = 0; depth < _possibleVariationLength.Length; depth++) _possibleVariationLength[depth] = 0;
            _principalVariations.Clear();
            // Enable PV update, increment search counter, and continue search.
            PvInfoUpdate = true;
            _cache.Searches++;
            Continue = true;
        }
    }
}