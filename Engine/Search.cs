// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
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


namespace ErikTheCoder.MadChess.Engine
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
        public TimeSpan? WhiteTimeRemaining;
        public TimeSpan? BlackTimeRemaining;
        public TimeSpan? WhiteTimeIncrement;
        public TimeSpan? BlackTimeIncrement;
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
        private const int _iidMinToHorizon = 6;
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
        private Evaluation _evaluation;
        private Delegates.Debug _debug;
        private Delegates.DisplayStats _displayStats;
        private Delegates.WriteMessageLine _writeMessageLine;
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
                    _evaluation.ConfigureLimitedStrength(_elo);
                }
                else
                {
                    ConfigureFullStrength();
                    _evaluation.ConfigureFullStrength();
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
                    _evaluation.ConfigureLimitedStrength(_elo);
                }
            }
        }


        private bool CompetitivePlay => !LimitedStrength && (MultiPv == 1);


        static Search()
        {
            _movePriorityComparer = new MovePriorityComparer();
            _scoredMovePriorityComparer = new ScoredMovePriorityComparer();
            _moveScoreComparer = new MoveScoreComparer();
            _getExchangeMaterialScore = Evaluation.GetExchangeMaterialScore;
        }


        public Search(Stats Stats, Cache Cache, KillerMoves KillerMoves, MoveHistory MoveHistory, Evaluation Evaluation,
            Delegates.Debug Debug, Delegates.DisplayStats DisplayStats, Delegates.WriteMessageLine WriteMessageLine)
        {
            _stats = Stats;
            _cache = Cache;
            _killerMoves = KillerMoves;
            _moveHistory = MoveHistory;
            _evaluation = Evaluation;
            _debug = Debug;
            _displayStats = DisplayStats;
            _writeMessageLine = WriteMessageLine;
            _getNextMove = GetNextMove;
            _getNextCapture = GetNextCapture;
            _getStaticScore = _evaluation.GetStaticScore;
            // Create synchronization and diagnostic objects.
            Signal = new AutoResetEvent(false);
            _stopwatch = new Stopwatch();
            // Create search parameters.
            SpecifiedMoves = new List<ulong>();
            // To Horizon =            000  001  002  003  004  005
            _futilityMargins = new[] { 050, 100, 175, 275, 400, 550 };
            _lateMovePruning = new[] { 999, 003, 007, 013, 021, 031 };
            System.Diagnostics.Debug.Assert(_futilityMargins.Length == _lateMovePruning.Length);
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


        private void Dispose(bool Disposing)
        {
            if (_disposed) return;
            if (Disposing)
            {
                // Release managed resources.
                SpecifiedMoves = null;
                WhiteTimeRemaining = null;
                BlackTimeRemaining = null;
                WhiteTimeIncrement = null;
                BlackTimeIncrement = null;
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
                _evaluation = null;
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
            _nodesPerSecond = Evaluation.GetNonLinearBonus(ratingClass, scale, power, constant); //           |
            // Rating               600  800  1000   1200    1400    1600    1800     2000     2200     2400  |
            // Nodes Per Second     100  612  8292  41572  131172  320100  663652  1229412  2097252  3359332  |
            //                                                                                                |
            // -----------------------------------------------------------------------------------------------+

            // Enable errors on every move.  -------------------------------------------------------+
            scale = 2d; //                                                                          |
            power = 2d; //                                                                          |
            constant = 10; //                                                                       |
            ratingClass = (double) (MaxElo - _elo) / 200; //                                        |
            _moveError = Evaluation.GetNonLinearBonus(ratingClass, scale, power, constant); //      |
            // Rating          600  800  1000  1200  1400  1600  1800  2000  2200  2400             |
            // Move Error      172  138   108    82    60    42    28    18    12    10             |
            //                                                                                      |
            // -------------------------------------------------------------------------------------+

            // Enable occasional blunders.  --------------------------------------------------------+
            scale = 8d; //                                                                          |
            power = 2d; //                                                                          |
            constant = 25; //                                                                       |
            _blunderError = Evaluation.GetNonLinearBonus(ratingClass, scale, power, constant); //   |
            scale = 0.33d; //                                                                       |
            power = 2; //                                                                           |
            constant = 5; //                                                                        |
            _blunderPer128 = Evaluation.GetNonLinearBonus(ratingClass, scale, power, constant); //  |
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


        public ulong FindBestMove(Board Board)
        {
            // Ensure all root moves are legal.
            Board.CurrentPosition.GenerateMoves();
            var legalMoveIndex = 0;
            for (var moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++)
            {
                var move = Board.CurrentPosition.Moves[moveIndex];
                if (!ShouldSearchMove(move)) continue;
                if (Board.IsMoveLegal(ref move))
                {
                    // Move is legal.
                    Move.SetPlayed(ref move, true); // All root moves will be played so set this in advance.
                    Board.CurrentPosition.Moves[legalMoveIndex] = move;
                    legalMoveIndex++;
                }
            }
            Board.CurrentPosition.MoveIndex = legalMoveIndex;
            if ((legalMoveIndex == 1) && (SpecifiedMoves.Count == 0))
            {
                // Only one legal move found.
                _stopwatch.Stop();
                return Board.CurrentPosition.Moves[0];
            }
            // Copy legal moves to root moves and principal variations.
            for (var moveIndex = 0; moveIndex < legalMoveIndex; moveIndex++)
            {
                var move = Board.CurrentPosition.Moves[moveIndex];
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
            GetMoveTime(Board.CurrentPosition);
            Board.NodesExamineTime = _nodesPerSecond.HasValue ? 1 : UciStream.NodesTimeInterval;
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
                for (var moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++) _rootMoves[moveIndex].Score = -StaticScore.Max;
                var score = GetDynamicScore(Board, 0, _originalHorizon, false, alpha, beta);
                if (Math.Abs(score) == StaticScore.Interrupted) break; // Stop searching.
                SortMovesByScore(_rootMoves, Board.CurrentPosition.MoveIndex - 1);
                var failHigh = _rootMoves[0].Score >= beta;
                var failHighScore = score;
                var failLow = !failHigh && (_rootMoves[principalVariations - 1].Score <= alpha);
                var failLowScore = MultiPv > 1 ? _rootMoves[principalVariations - 1].Score : score;
                if (failHigh || failLow)
                {
                    if (PvInfoUpdate)
                    {
                        if (failHigh) UpdateStatusFailHigh(Board.Nodes, failHighScore);
                        if (failLow) UpdateStatusFailLow(Board.Nodes, failLowScore);
                    }
                    // Reset move scores then search moves within infinite alpha / beta window.
                    for (var moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++) _rootMoves[moveIndex].Score = -StaticScore.Max;
                    score = GetDynamicScore(Board, 0, _originalHorizon, false, -StaticScore.Max, StaticScore.Max);
                    if (Math.Abs(score) == StaticScore.Interrupted) break; // Stop searching.
                    SortMovesByScore(_rootMoves, Board.CurrentPosition.MoveIndex - 1);
                }
                // Find best move.
                for (var moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++) _bestMoves[moveIndex] = _rootMoves[moveIndex];
                bestMove = _bestMoves[0];
                _bestMovePlies[_originalHorizon] = bestMove;
                if (PvInfoUpdate) UpdateStatus(Board, true);
                if (MateInMoves.HasValue && (bestMove.Score >= StaticScore.Checkmate) && (Evaluation.GetMateMoveCount(bestMove.Score) <= MateInMoves.Value)) break; // Found checkmate in correct number of moves.
                AdjustMoveTime();
                if (!HaveTimeForNextHorizon()) break; // Do not have time to search next ply.
            } while (Continue && (_originalHorizon < HorizonLimit));
            _stopwatch.Stop();
            if (_debug()) _writeMessageLine($"info string Stopping search at {_stopwatch.Elapsed.TotalMilliseconds:0} milliseconds.");
            return scoreError == 0 ? bestMove.Move : GetInferiorMove(Board.CurrentPosition, scoreError);
        }


        private bool ShouldSearchMove(ulong Move)
        {
            if (SpecifiedMoves.Count == 0) return true; // Search all moves.
            // Search only specified moves.
            for (var moveIndex = 0; moveIndex < SpecifiedMoves.Count; moveIndex++)
            {
                var specifiedMove = SpecifiedMoves[moveIndex];
                if (Engine.Move.Equals(Move, specifiedMove)) return true;
            }
            return false;
        }
        
        
        private void GetMoveTime(Position Position)
        {
            // No need to calculate move time if go command specified move time or horizon limit.
            if ((MoveTimeHardLimit != TimeSpan.MaxValue) || (HorizonLimit != MaxHorizon)) return;
            // Retrieve time remaining increment.
            TimeSpan timeRemaining;
            TimeSpan timeIncrement;
            if (Position.WhiteMove)
            {
                // White Move
                if (!WhiteTimeRemaining.HasValue) throw new Exception($"{nameof(WhiteTimeRemaining)} is null.");
                timeRemaining = WhiteTimeRemaining.Value;
                timeIncrement = WhiteTimeIncrement ?? TimeSpan.Zero;
            }
            else
            {
                // Black Move
                if (!BlackTimeRemaining.HasValue) throw new Exception($"{nameof(BlackTimeRemaining)} is null.");
                timeRemaining = BlackTimeRemaining.Value;
                timeIncrement = BlackTimeIncrement ?? TimeSpan.Zero;
            }
            if (timeRemaining == TimeSpan.MaxValue) return; // No need to calculate move time if go command specified infinite search.
            timeRemaining -= _stopwatch.Elapsed; // Account for lag between receiving go command and now.
            int movesRemaining;
            if (MovesToTimeControl.HasValue) movesRemaining = MovesToTimeControl.Value;
            else
            {
                // Estimate moves remaining.
                var pieces = Bitwise.CountSetBits(Position.Occupancy) - 2; // Don't include kings.
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


        private ulong GetInferiorMove(Position Position, int ScoreError)
        {
            // Determine how many moves are within score error.
            var bestScore = _bestMoves[0].Score;
            var worstScore = bestScore - ScoreError;
            var inferiorMoves = 0;
            for (var moveIndex = 1; moveIndex < Position.MoveIndex; moveIndex++)
            {
                if (_bestMoves[moveIndex].Score < worstScore) break;
                inferiorMoves++;
            }
            // Randomly select a move within score error.
            return _bestMoves[SafeRandom.NextInt(0, inferiorMoves + 1)].Move;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetDynamicScore(Board Board, int Depth, int Horizon, bool IsNullMoveAllowed, int Alpha, int Beta, ulong ExcludedMove = 0)
        {
            if ((Board.Nodes > Board.NodesExamineTime) || _nodesPerSecond.HasValue)
            {
                ExamineTimeAndNodes(Board.Nodes);
                var intervals = (int) (Board.Nodes / UciStream.NodesTimeInterval);
                Board.NodesExamineTime = _nodesPerSecond.HasValue
                    ? Board.Nodes + 1
                    : UciStream.NodesTimeInterval * (intervals + 1);
            }
            if (!Continue && (_bestMoves[0].Move != Move.Null)) return StaticScore.Interrupted; // Search was interrupted.
            var (terminalDraw, repeatPosition) = _evaluation.IsTerminalDraw(Board.CurrentPosition);
            if ((Depth > 0) && terminalDraw) return 0; // Game ends on this move.
            // Get cached position.
            var toHorizon = Horizon - Depth;
            var historyIncrement = toHorizon * toHorizon;
            var cachedPosition = _cache.GetPosition(Board.CurrentPosition.Key);
            ulong bestMove;
            if ((cachedPosition.Key != _cache.NullPosition.Key) && (Depth > 0) && !repeatPosition)
            {
                // Not a root or repeat position.
                // Determine if score is cached.
                var cachedScore = GetCachedScore(cachedPosition.Data, Depth, Horizon, Alpha, Beta);
                if (cachedScore != StaticScore.NotCached)
                {
                    // Score is cached.
                    if (cachedScore >= Beta)
                    {
                        bestMove = _cache.GetBestMove(cachedPosition.Data);
                        if ((bestMove != Move.Null) && Move.IsQuiet(bestMove))
                        {
                            // Assume the quiet best move specified by the cached position would have caused a beta cutoff.
                            // Update history heuristic.
                            _moveHistory.UpdateValue(Board.CurrentPosition, bestMove, historyIncrement);
                        }
                    }
                    _stats.CacheScoreCutoff++;
                    return cachedScore;
                }
            }
            if (toHorizon <= 0) return GetQuietScore(Board, Depth, Depth, Board.AllSquaresMask, Alpha, Beta, _getStaticScore, true); // Search for a quiet position.
            var drawnEndgame = false;
            if (Board.CurrentPosition.KingInCheck) Board.CurrentPosition.StaticScore = -StaticScore.Max;
            // ReSharper disable once PossibleNullReferenceException
            else if (Board.PreviousPosition?.PlayedMove == Move.Null) Board.CurrentPosition.StaticScore = -Board.PreviousPosition.StaticScore;
            else
            {
                // Even if endgame is drawn, search moves for a swindle (enemy mistake that makes drawn game winnable).
                (Board.CurrentPosition.StaticScore, drawnEndgame) = _evaluation.GetStaticScore(Board.CurrentPosition);
            }
            // ReSharper restore PossibleNullReferenceException
            if (IsPositionFutile(Board.CurrentPosition, Depth, Horizon, drawnEndgame, Alpha, Beta))
            {
                // Position is futile.
                // Position is not the result of best play by both players.
                UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, Move.Null, Beta, Alpha, Beta);
                return Beta;
            }
            if (IsNullMoveAllowed && Search.IsNullMoveAllowed(Board.CurrentPosition, Beta))
            {
                // Null move is allowed.
                _stats.NullMoves++;
                if (DoesNullMoveCauseBetaCutoff(Board, Depth, Horizon, Beta))
                {
                    // Enemy is unable to capitalize on position even if player forfeits right to move.
                    // While forfeiting right to move is illegal, this indicates position is strong.
                    // Position is not the result of best play by both players.
                    UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, Move.Null, Beta, Alpha, Beta);
                    _stats.NullMoveCutoffs++;
                    return Beta;
                }
            }
            // Get best move.
            bestMove = _cache.GetBestMove(cachedPosition.Data);
            if ((bestMove == Move.Null) && ((Beta - Alpha) > 1) && (toHorizon >= _iidMinToHorizon))
            {
                // Cached position in a principal variation does not specify a best move.
                // Find best move via Internal Iterative Deepening.
                GetDynamicScore(Board, Depth, Horizon - 1, false, Alpha, Beta);
                cachedPosition = _cache.GetPosition(Board.CurrentPosition.Key);
                bestMove = _cache.GetBestMove(cachedPosition.Data);
            }
            var originalAlpha = Alpha;
            var bestScore = Alpha;
            var legalMoveNumber = 0;
            var quietMoveNumber = 0;
            var moveIndex = -1;
            var lastMoveIndex = Board.CurrentPosition.MoveIndex - 1;
            if (Depth > 0) Board.CurrentPosition.PrepareMoveGeneration();
            do
            {
                ulong move;
                if (Depth == 0)
                {
                    // Search root moves.
                    moveIndex++;
                    if (moveIndex == 0)
                    {
                        PrioritizeMoves(Board.CurrentPosition, _rootMoves, lastMoveIndex, bestMove, Depth);
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
                    (move, moveIndex) = GetNextMove(Board.CurrentPosition, Board.AllSquaresMask, Depth, bestMove);
                    if (move == Move.Null) break;
                    if (Board.IsMoveLegal(ref move)) legalMoveNumber++;
                    else continue; // Skip illegal move.
                    Board.CurrentPosition.Moves[moveIndex] = move;
                }
                if (move == ExcludedMove) continue;
                if (IsMoveFutile(Board, Depth, Horizon, move, legalMoveNumber, quietMoveNumber, drawnEndgame, Alpha, Beta)) continue; // Move is futile.  Skip move.
                if (Move.IsQuiet(move)) quietMoveNumber++;
                var searchHorizon = GetSearchHorizon(Board, Depth, Horizon, move, cachedPosition, quietMoveNumber, drawnEndgame);
                var moveBeta = (legalMoveNumber == 1) || ((MultiPv > 1) && (Depth == 0))
                    ? Beta // Search with full alpha / beta window.
                    : bestScore + 1; // Search with zero alpha / beta window.
                // Play and search move.
                Move.SetPlayed(ref move, true);
                Board.CurrentPosition.Moves[moveIndex] = move;
                Board.PlayMove(move);
                var score = -GetDynamicScore(Board, Depth + 1, searchHorizon, true, -moveBeta, -Alpha);
                if (Math.Abs(score) == StaticScore.Interrupted)
                {
                    // Stop searching.
                    Board.UndoMove();
                    return score;
                }
                if (score > bestScore)
                {
                    // Move may be stronger than principal variation.
                    if ((moveBeta < Beta) || (searchHorizon < Horizon))
                    {
                        // Search move at unreduced horizon with full alpha / beta window.
                        score = -GetDynamicScore(Board, Depth + 1, Horizon, true, -Beta, -Alpha);
                    }
                }
                Board.UndoMove();
                if (Math.Abs(score) == StaticScore.Interrupted) return score; // Stop searching.
                if ((score > Alpha) && (score < Beta) && (Depth == 0)) _rootMoves[moveIndex].Score = score; // Update root move score.
                if (score >= Beta)
                {
                    // Position is not the result of best play by both players.
                    if (Move.IsQuiet(move))
                    {
                        // Update move heuristics.
                        _killerMoves.UpdateValue(Board.CurrentPosition, Depth, move);
                        _moveHistory.UpdateValue(Board.CurrentPosition, move, historyIncrement);
                        // Decrement move index immediately so as not to include the quiet move that caused the beta cutoff.
                        moveIndex--;
                        while (moveIndex >= 0)
                        {
                            var priorMove = Board.CurrentPosition.Moves[moveIndex];
                            if (Move.IsQuiet(priorMove) && Move.Played(priorMove))
                            {
                                // Update history of prior quiet move that failed to produce cutoff.
                                _moveHistory.UpdateValue(Board.CurrentPosition, priorMove, -historyIncrement);
                            }
                            moveIndex--;
                        }
                    }
                    UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, move, score, Alpha, Beta);
                    _stats.MovesCausingBetaCutoff++;
                    _stats.BetaCutoffMoveNumber += legalMoveNumber;
                    if (legalMoveNumber == 1) _stats.BetaCutoffFirstMove++;
                    return Beta;
                }
                var rootMoveWithinWindow = (Depth == 0) && (score > Alpha) && (score < Beta);
                if (rootMoveWithinWindow || (score > bestScore))
                {
                    // Update possible variation.
                    _possibleVariations[Depth][0] = move;
                    var possibleVariationLength = _possibleVariationLength[Depth + 1];
                    Array.Copy(_possibleVariations[Depth + 1], 0, _possibleVariations[Depth], 1, possibleVariationLength);
                    _possibleVariationLength[Depth] = possibleVariationLength + 1;
                    if (Depth == 0)
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
                    UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, move, score, Alpha, Beta);
                    if ((Depth > 0) || CompetitivePlay) Alpha = score;
                }
                if ((_bestMoves[0].Move != Move.Null) && (Board.Nodes >= Board.NodesInfoUpdate))
                {
                    // Update status.
                    UpdateStatus(Board, false);
                    var intervals = (int) (Board.Nodes / UciStream.NodesInfoInterval);
                    Board.NodesInfoUpdate = UciStream.NodesInfoInterval * (intervals + 1);
                }
            } while (true);
            if (legalMoveNumber == 0)
            {
                // Checkmate or Stalemate
                bestScore = Board.CurrentPosition.KingInCheck ? Evaluation.GetMateScore(Depth) : 0;
                _possibleVariationLength[Depth] = 0;
            }
            if (bestScore <= originalAlpha) UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, Move.Null, bestScore, originalAlpha, Beta); // Score fails low.
            return bestScore;
        }


        public int GetExchangeScore(Board Board, ulong Move)
        {
            var (scoreBeforeMove, _) = _getExchangeMaterialScore(Board.CurrentPosition);
            Board.PlayMove(Move);
            var scoreAfterMove = -GetQuietScore(Board, 0, 0, Board.SquareMasks[Engine.Move.To(Move)], -StaticScore.Max, StaticScore.Max, _getExchangeMaterialScore, false);
            Board.UndoMove();
            return scoreAfterMove - scoreBeforeMove;
        }
        
        
        public int GetQuietScore(Board Board, int Depth, int Horizon, int Alpha, int Beta) => GetQuietScore(Board, Depth, Horizon, Board.AllSquaresMask, Alpha, Beta, _getStaticScore, true);


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetQuietScore(Board Board, int Depth, int Horizon, ulong ToSquareMask, int Alpha, int Beta, Delegates.GetStaticScore GetStaticScore, bool ConsiderFutility)
        {
            if ((Board.Nodes > Board.NodesExamineTime) || _nodesPerSecond.HasValue)
            {
                ExamineTimeAndNodes(Board.Nodes);
                var intervals = Board.Nodes / UciStream.NodesTimeInterval; Board.NodesExamineTime = _nodesPerSecond.HasValue
                    ? Board.Nodes + 1
                    : UciStream.NodesTimeInterval * (intervals + 1);
            }
            if (!Continue && (_bestMoves[0].Move != Move.Null)) return StaticScore.Interrupted; // Search was interrupted.
            var (terminalDraw, _) = _evaluation.IsTerminalDraw(Board.CurrentPosition);
            if ((Depth > 0) && terminalDraw) return 0; // Game ends on this move.
            // Search for a quiet position where no captures are possible.
            var fromHorizon = Depth - Horizon;
            _selectiveHorizon = Math.Max(Depth, _selectiveHorizon);
            var drawnEndgame = false;
            Delegates.GetNextMove getNextMove;
            ulong moveGenerationToSquareMask;
            if (Board.CurrentPosition.KingInCheck)
            {
                // King is in check.  Search all moves.
                getNextMove = _getNextMove;
                moveGenerationToSquareMask = Board.AllSquaresMask;
                Board.CurrentPosition.StaticScore = -StaticScore.Max; // Don't evaluate static score since moves when king is in check are not futile.
            }
            else
            {
                // King is not in check.  Search only captures.
                getNextMove = _getNextCapture;
                if ((fromHorizon > _quietSearchMaxFromHorizon) && !Board.PreviousPosition.KingInCheck)
                {
                    var lastMoveToSquare = Move.To(Board.PreviousPosition.PlayedMove);
                    moveGenerationToSquareMask = lastMoveToSquare == Square.Illegal
                        ? ToSquareMask
                        : Board.SquareMasks[lastMoveToSquare]; // Search only recaptures.
                }
                else moveGenerationToSquareMask = ToSquareMask;
                // ReSharper disable PossibleNullReferenceException
                if (Board.PreviousPosition?.PlayedMove == Move.Null) Board.CurrentPosition.StaticScore = -Board.PreviousPosition.StaticScore;
                else
                {
                    // Even if endgame is drawn, search moves for a swindle (enemy mistake that makes drawn game winnable).
                    (Board.CurrentPosition.StaticScore, drawnEndgame) = GetStaticScore(Board.CurrentPosition);
                }
                // ReSharper restore PossibleNullReferenceException
                if (Board.CurrentPosition.StaticScore >= Beta) return Beta; // Prevent worsening of position by making a bad capture.  Stand pat.
                Alpha = Math.Max(Board.CurrentPosition.StaticScore, Alpha);
            }
            var legalMoveNumber = 0;
            Board.CurrentPosition.PrepareMoveGeneration();
            do
            {
                var (move, _) = getNextMove(Board.CurrentPosition, moveGenerationToSquareMask, Depth, Move.Null); // Don't retrieve (or update) best move from the cache.  Rely on MVV / LVA move order.
                if (move == Move.Null) break;
                if (Board.IsMoveLegal(ref move)) legalMoveNumber++; // Move is legal.
                else continue; // Skip illegal move.
                if (ConsiderFutility && IsMoveFutile(Board, Depth, Horizon, move, legalMoveNumber, 0, drawnEndgame, Alpha, Beta)) continue; // Move is futile.  Skip move.
                // Play and search move.
                Board.PlayMove(move);
                var score = -GetQuietScore(Board, Depth + 1, Horizon, ToSquareMask, -Beta, -Alpha, GetStaticScore, ConsiderFutility);
                Board.UndoMove();
                if (Math.Abs(score) == StaticScore.Interrupted) return score; // Stop searching.
                if (score >= Beta) return Beta; // Position is not the result of best play by both players.
                Alpha = Math.Max(score, Alpha);
            } while (true);
            if ((legalMoveNumber == 0) && Board.CurrentPosition.KingInCheck) return Evaluation.GetMateScore(Depth); // Game ends on this move.
            // Return score of best move.
            return Alpha;
        }


        private void ExamineTimeAndNodes(long Nodes)
        {
            if (Nodes >= NodeLimit) Continue = false; // Have passed node limit.
            if (_nodesPerSecond.HasValue && (_originalHorizon > 1)) // Guarantee to search at least one ply.
            {
                // Slow search until it's less than specified nodes per second or until soft time limit is exceeded.
                var nodesPerSecond = int.MaxValue;
                while (nodesPerSecond > _nodesPerSecond)
                {
                    // Delay search but keep CPU busy to simulate "thinking".
                    nodesPerSecond = (int)(Nodes / _stopwatch.Elapsed.TotalSeconds);
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
        private static bool IsPositionFutile(Position Position, int Depth, int Horizon, bool IsDrawnEndgame, int Alpha, int Beta)
        {
            var toHorizon = Horizon - Depth;
            if (toHorizon >= _futilityMargins.Length) return false; // Position far from search horizon is not futile.
            if (IsDrawnEndgame || (Depth == 0) || Position.KingInCheck) return false; // Position in drawn endgame, at root, or when king is in check is not futile.
            if ((Math.Abs(Alpha) >= StaticScore.Checkmate) || (Math.Abs(Beta) >= StaticScore.Checkmate)) return false; // Position under threat of checkmate is not futile.
            // Count pawns and pieces (but don't include kings).
            var whitePawnsAndPieces = Bitwise.CountSetBits(Position.OccupancyWhite) - 1;
            var blackPawnsAndPieces = Bitwise.CountSetBits(Position.OccupancyBlack) - 1;
            if ((whitePawnsAndPieces == 0) || (blackPawnsAndPieces == 0)) return false; // Position with lone king on board is not futile.
            // Determine if any move can lower score to beta.
            var futilityMargin = toHorizon <= 0 ? _futilityMargins[0] : _futilityMargins[toHorizon];
            return Position.StaticScore - futilityMargin > Beta;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNullMoveAllowed(Position Position, int Beta)
        {
            if ((Position.StaticScore < Beta) || Position.KingInCheck) return false;
            // Do not attempt null move in pawn endgames.  Side to move may be in zugzwang.
            var minorAndMajorPieces = Position.WhiteMove
                ? Bitwise.CountSetBits(Position.WhiteKnights | Position.WhiteBishops | Position.WhiteRooks | Position.WhiteQueens)
                : Bitwise.CountSetBits(Position.BlackKnights | Position.BlackBishops | Position.BlackRooks | Position.BlackQueens);
            return minorAndMajorPieces > 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DoesNullMoveCauseBetaCutoff(Board Board, int Depth, int Horizon, int Beta)
        {
            var reduction = _nullMoveReduction + Math.Min((Board.CurrentPosition.StaticScore - Beta) / _nullStaticScoreReduction, _nullStaticScoreMaxReduction);
            Board.PlayNullMove();
            // Do not play two null moves consecutively.  Search with zero alpha / beta window.
            var score = -GetDynamicScore(Board, Depth + 1, Horizon - reduction, false, -Beta, -Beta + 1);
            Board.UndoMove();
            return score >= Beta;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public (ulong Move, int MoveIndex) GetNextMove(Position Position, ulong ToSquareMask, int Depth, ulong BestMove)
        {
            while (true)
            {
                int firstMoveIndex;
                int lastMoveIndex;
                if (Position.CurrentMoveIndex < Position.MoveIndex)
                {
                    var moveIndex = Position.CurrentMoveIndex;
                    var move = Position.Moves[moveIndex];
                    Position.CurrentMoveIndex++;
                    var generatedBestMove = (moveIndex > 0) && Move.Equals(move, BestMove);
                    if (Move.Played(move) || generatedBestMove) continue; // Don't play move twice.
                    return (move, moveIndex);
                }
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (Position.MoveGenerationStage)
                {
                    case MoveGenerationStage.BestMove:
                        // Find pinned pieces and set best move.
                        Position.FindPinnedPieces();
                        if (BestMove != Move.Null)
                        {
                            Move.SetIsBest(ref BestMove, true);
                            Position.Moves[Position.MoveIndex] = BestMove;
                            Position.MoveIndex++;
                        }
                        Position.MoveGenerationStage++;
                        continue;
                    case MoveGenerationStage.Captures:
                        firstMoveIndex = Position.MoveIndex;
                        Position.GenerateMoves(MoveGeneration.OnlyCaptures, Board.AllSquaresMask, ToSquareMask);
                        // Prioritize and sort captures.
                        lastMoveIndex = Math.Max(firstMoveIndex, Position.MoveIndex - 1);
                        if (lastMoveIndex > firstMoveIndex)
                        {
                            PrioritizeMoves(Position, Position.Moves, firstMoveIndex, lastMoveIndex, BestMove, Depth);
                            SortMovesByPriority(Position.Moves, firstMoveIndex, lastMoveIndex);
                        }
                        Position.MoveGenerationStage++;
                        continue;
                    case MoveGenerationStage.NonCaptures:
                        firstMoveIndex = Position.MoveIndex;
                        Position.GenerateMoves(MoveGeneration.OnlyNonCaptures, Board.AllSquaresMask, ToSquareMask);
                        // Prioritize and sort non-captures.
                        lastMoveIndex = Math.Max(firstMoveIndex, Position.MoveIndex - 1);
                        if (lastMoveIndex > firstMoveIndex)
                        {
                            PrioritizeMoves(Position, Position.Moves, firstMoveIndex, lastMoveIndex, BestMove, Depth);
                            SortMovesByPriority(Position.Moves, firstMoveIndex, lastMoveIndex);
                        }
                        Position.MoveGenerationStage++;
                        continue;
                    case MoveGenerationStage.End:
                        return (Move.Null, Position.CurrentMoveIndex);
                }
                break;
            }
            return (Move.Null, Position.CurrentMoveIndex);
        }


        // Pass BestMove parameter even though it isn't referenced to satisfy GetNextMove delegate signature.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static (ulong Move, int MoveIndex) GetNextCapture(Position Position, ulong ToSquareMask, int Depth, ulong BestMove)
        {
            while (true)
            {
                if (Position.CurrentMoveIndex < Position.MoveIndex)
                {
                    var moveIndex = Position.CurrentMoveIndex;
                    var move = Position.Moves[moveIndex];
                    Position.CurrentMoveIndex++;
                    if (Move.CaptureVictim(move) == Piece.None) continue;
                    return (move, moveIndex);
                }
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (Position.MoveGenerationStage)
                {
                    case MoveGenerationStage.BestMove:
                    case MoveGenerationStage.Captures:
                        Position.FindPinnedPieces();
                        var firstMoveIndex = Position.MoveIndex;
                        Position.GenerateMoves(MoveGeneration.OnlyCaptures, Board.AllSquaresMask, ToSquareMask);
                        var lastMoveIndex = Math.Max(firstMoveIndex, Position.MoveIndex - 1);
                        if (lastMoveIndex > firstMoveIndex) SortMovesByPriority(Position.Moves, firstMoveIndex, lastMoveIndex); // Don't prioritize moves before sorting.  MVV / LVA is good enough when ordering captures.
                        Position.MoveGenerationStage = MoveGenerationStage.End; // Skip non-captures.
                        continue;
                    case MoveGenerationStage.End:
                        return (Move.Null, Position.CurrentMoveIndex);
                }
                break;
            }
            return (Move.Null, Position.CurrentMoveIndex);
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool IsMoveFutile(Board Board, int Depth, int Horizon, ulong Move, int LegalMoveNumber, int QuietMoveNumber, bool DrawnEndgame, int Alpha, int Beta)
        {
            Debug.Assert(_futilityMargins.Length == _lateMovePruning.Length);
            var toHorizon = Horizon - Depth;
            if (toHorizon >= _futilityMargins.Length) return false; // Move far from search horizon is not futile.
            if ((Depth == 0) || (LegalMoveNumber == 1)) return false; // Root move or first move is not futile.
            if (DrawnEndgame || Engine.Move.IsCheck(Move) || Board.CurrentPosition.KingInCheck) return false; // Move in drawn endgame, checking move, or move when king is in check is not futile.
            if ((Math.Abs(Alpha) >= StaticScore.Checkmate) || (Math.Abs(Beta) >= StaticScore.Checkmate)) return false; // Move under threat of checkmate is not futile.
            var captureVictim = Engine.Move.CaptureVictim(Move);
            var capture = captureVictim != Piece.None;
            if (capture && (toHorizon > 0)) return false; // Capture in main search is not futile.
            if ((Engine.Move.Killer(Move) > 0) || (Engine.Move.PromotedPiece(Move) != Piece.None) || Engine.Move.IsCastling(Move)) return false; // Killer move, pawn promotion, or castling is not futile.
            if (Engine.Move.IsPawnMove(Move))
            {
                var rank = Board.CurrentPosition.WhiteMove ? Board.WhiteRanks[Engine.Move.To(Move)] : Board.BlackRanks[Engine.Move.To(Move)];
                if (rank >= 6) return false; // Pawn push is not futile.
            }
            // Count pawns and pieces (but don't include kings).
            var whitePawnsAndPieces = Bitwise.CountSetBits(Board.CurrentPosition.OccupancyWhite) - 1;
            var blackPawnsAndPieces = Bitwise.CountSetBits(Board.CurrentPosition.OccupancyBlack) - 1;
            if ((whitePawnsAndPieces == 0) || (blackPawnsAndPieces == 0)) return false; // Move with lone king on board is not futile.
            var lateMoveNumber = toHorizon <= 0 ? _lateMovePruning[0] : _lateMovePruning[toHorizon];
            if (Engine.Move.IsQuiet(Move) && (QuietMoveNumber >= lateMoveNumber)) return true; // Quiet move is too late to be worth searching.
            // Determine if move can raise score to alpha.
            var futilityMargin = toHorizon <= 0 ? _futilityMargins[0] : _futilityMargins[toHorizon];
            return Board.CurrentPosition.StaticScore + _evaluation.GetMaterialScore(Board.CurrentPosition, captureVictim) + futilityMargin < Alpha;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetSearchHorizon(Board Board, int Depth, int Horizon, ulong Move, CachedPosition CachedPosition, int QuietMoveNumber, bool DrawnEndgame)
        {
            if (Engine.Move.IsBest(Move) && IsBestMoveSingular(Board, Depth, Horizon, Move, CachedPosition)) return Horizon + 1; // Extend singular move.
            if ((Depth == 0) && !CompetitivePlay) return Horizon; // Do not reduce root move in Multi-PV searches or when engine playing strength is reduced.
            var capture = Engine.Move.CaptureVictim(Move) != Piece.None;
            if (capture) return Horizon; // Do not reduce capture.
            if (DrawnEndgame || Engine.Move.IsCheck(Move) || Board.CurrentPosition.KingInCheck) return Horizon; // Do not reduce move in drawn endgame, checking move, or move when king is in check.
            if ((Engine.Move.Killer(Move) > 0) || (Engine.Move.PromotedPiece(Move) != Piece.None) || Engine.Move.IsCastling(Move)) return Horizon; // Do not reduce killer move, pawn promotion, or castling.
            if (Engine.Move.IsPawnMove(Move))
            {
                var rank = Board.CurrentPosition.WhiteMove ? Board.WhiteRanks[Engine.Move.To(Move)] : Board.BlackRanks[Engine.Move.To(Move)];
                if (rank >= 6) return Horizon; // Do not reduce pawn push.
            }
            // Count pawns and pieces (but don't include kings).
            var whitePawnsAndPieces = Bitwise.CountSetBits(Board.CurrentPosition.OccupancyWhite) - 1;
            var blackPawnsAndPieces = Bitwise.CountSetBits(Board.CurrentPosition.OccupancyBlack) - 1;
            if ((whitePawnsAndPieces == 0) || (blackPawnsAndPieces == 0)) return Horizon; // Do not reduce move with lone king on board.
            if (!Engine.Move.IsQuiet(Move)) return Horizon;
            // Reduce search horizon of late move.
            var quietMoveIndex = Math.Min(QuietMoveNumber, _lateMoveReductions.Length - 1);
            return Horizon - _lateMoveReductions[quietMoveIndex];
        }


        // Idea from Stockfish chess engine.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool IsBestMoveSingular(Board Board, int Depth, int Horizon, ulong Move, CachedPosition CachedPosition)
        {
            var toHorizon = Horizon - Depth;
            if ((Depth == 0) || (toHorizon < _singularMoveMinToHorizon)) return false;
            var score = CachedPositionData.Score(CachedPosition.Data);
            if ((score == StaticScore.NotCached) || (Math.Abs(score) >= StaticScore.Checkmate)) return false;
            var scorePrecision = CachedPositionData.ScorePrecision(CachedPosition.Data);
            if (CachedPositionData.ScorePrecision(CachedPosition.Data) != ScorePrecision.LowerBound) return false;
            if (CachedPositionData.ToHorizon(CachedPosition.Data) < (toHorizon - _singularMoveMaxInsufficientDraft)) return false;
            var beta = score - (_singularMoveMargin * toHorizon);
            var searchHorizon = Depth + ((toHorizon * _singularMoveReductionPer128) / 128);
            score = GetDynamicScore(Board, Depth, searchHorizon, false, beta - 1, beta, Move);
            return score < beta;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetCachedScore(ulong CachedPositionData, int Depth, int Horizon, int Alpha, int Beta)
        {
            var score = Engine.CachedPositionData.Score(CachedPositionData);
            if (score == StaticScore.NotCached) return StaticScore.NotCached; // Score is not cached.
            var toHorizon = Horizon - Depth;
            var cachedToHorizon = Engine.CachedPositionData.ToHorizon(CachedPositionData);
            if (cachedToHorizon < toHorizon) return StaticScore.NotCached; // Cached position is shallower than current horizon. Do not use cached score.
            if (Math.Abs(score) >= StaticScore.Checkmate)
            {
                // Adjust checkmate score.
                if (score > 0) score -= Depth;
                else score += Depth;
            }
            var scorePrecision = Engine.CachedPositionData.ScorePrecision(CachedPositionData);
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (scorePrecision)
            {
                case ScorePrecision.Exact:
                    // Score is exact.
                    if (score <= Alpha) return Alpha; // Score fails low.
                    if (score >= Beta) return Beta; // Score fails high.
                    // If necessary, avoid truncating the principal variation by returning a cached score.
                    return TruncatePrincipalVariation ? score : StaticScore.NotCached;
                case ScorePrecision.UpperBound:
                    // Score is upper bound.
                    if (score <= Alpha) return Alpha; // Score fails low.
                    break;
                case ScorePrecision.LowerBound:
                    // Score is lower bound.
                    if (score >= Beta) return Beta; // Score fails high.
                    break;
                default:
                    throw new Exception($"{scorePrecision} score precision not supported.");
            }
            return StaticScore.NotCached;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrioritizeMoves(Position Position, ScoredMove[] Moves, int LastMoveIndex, ulong BestMove, int Depth)
        {
            for (var moveIndex = 0; moveIndex <= LastMoveIndex; moveIndex++)
            {
                var move = Moves[moveIndex].Move;
                // Prioritize best move.
                Move.SetIsBest(ref move, Move.Equals(move, BestMove));
                // Prioritize killer moves.
                Move.SetKiller(ref move, _killerMoves.GetValue(Position, Depth, move));
                // Prioritize by move history.
                Move.SetHistory(ref move, _moveHistory.GetValue(Position, move));
                Moves[moveIndex].Move = move;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrioritizeMoves(Position Position, ulong[] Moves, int LastMoveIndex, ulong BestMove, int Depth) => PrioritizeMoves(Position, Moves, 0, LastMoveIndex, BestMove, Depth);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PrioritizeMoves(Position Position, ulong[] Moves, int FirstMoveIndex, int LastMoveIndex, ulong BestMove, int Depth)
        {
            for (var moveIndex = FirstMoveIndex; moveIndex <= LastMoveIndex; moveIndex++)
            {
                var move = Moves[moveIndex];
                // Prioritize best move.
                Move.SetIsBest(ref move, Move.Equals(move, BestMove));
                // Prioritize killer moves.
                Move.SetKiller(ref move, _killerMoves.GetValue(Position, Depth, move));
                // Prioritize by move history.
                Move.SetHistory(ref move, _moveHistory.GetValue(Position, move));
                Moves[moveIndex] = move;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SortMovesByPriority(ScoredMove[] Moves, int LastMoveIndex) => Array.Sort(Moves, 0, LastMoveIndex + 1, _scoredMovePriorityComparer);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SortMovesByScore(ScoredMove[] Moves, int LastMoveIndex) => Array.Sort(Moves, 0, LastMoveIndex + 1, _moveScoreComparer);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SortMovesByPriority(ulong[] Moves, int LastMoveIndex) => Array.Sort(Moves, 0, LastMoveIndex + 1, _movePriorityComparer);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SortMovesByPriority(ulong[] Moves, int FirstMoveIndex, int LastMoveIndex) => Array.Sort(Moves, FirstMoveIndex, LastMoveIndex - FirstMoveIndex + 1, _movePriorityComparer);


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void UpdateBestMoveCache(Position CurrentPosition, int Depth, int Horizon, ulong BestMove, int Score, int Alpha, int Beta)
        {
            if (Math.Abs(Score) == StaticScore.Interrupted) return;
            var cachedPosition = _cache.NullPosition;
            cachedPosition.Key = CurrentPosition.Key;
            CachedPositionData.SetToHorizon(ref cachedPosition.Data, Horizon - Depth);
            if (BestMove != Move.Null)
            {
                // Set best move.
                CachedPositionData.SetBestMoveFrom(ref cachedPosition.Data, Move.From(BestMove));
                CachedPositionData.SetBestMoveTo(ref cachedPosition.Data, Move.To(BestMove));
                CachedPositionData.SetBestMovePromotedPiece(ref cachedPosition.Data, Move.PromotedPiece(BestMove));
            }
            var score = Score;
            if (Math.Abs(score) >= StaticScore.Checkmate) score += score > 0 ? Depth : -Depth; // Adjust checkmate score.
            // Update score.
            if (score <= Alpha)
            {
                CachedPositionData.SetScorePrecision(ref cachedPosition.Data, ScorePrecision.UpperBound);
                CachedPositionData.SetScore(ref cachedPosition.Data, Alpha);
            }
            else if (score >= Beta)
            {
                CachedPositionData.SetScorePrecision(ref cachedPosition.Data, ScorePrecision.LowerBound);
                CachedPositionData.SetScore(ref cachedPosition.Data, Beta);
            }
            else
            {
                CachedPositionData.SetScorePrecision(ref cachedPosition.Data, ScorePrecision.Exact);
                CachedPositionData.SetScore(ref cachedPosition.Data, score);
            }
            _cache.SetPosition(cachedPosition);
        }


        // TODO: Resolve issue involving illegal PVs.  See http://talkchess.com/forum3/viewtopic.php?p=892120#p892120.
        private void UpdateStatus(Board Board, bool IncludePrincipalVariation)
        {
            var milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            var nodesPerSecond = Board.Nodes / _stopwatch.Elapsed.TotalSeconds;
            var nodes = IncludePrincipalVariation ? Board.Nodes : Board.NodesInfoUpdate;
            var hashFull = (int)((1000L * _cache.Positions) / _cache.Capacity);
            if (IncludePrincipalVariation)
            {
                var principalVariations = Math.Min(MultiPv, Board.CurrentPosition.MoveIndex);
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
                    var scorePhrase = Math.Abs(score) >= StaticScore.Checkmate ? $"mate {Evaluation.GetMateMoveCount(score)}" : $"cp {score}";
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
            var intervals = (int) (Board.Nodes / UciStream.NodesInfoInterval);
            Board.NodesInfoUpdate = UciStream.NodesInfoInterval * (intervals + 1);
        }


        private void UpdateStatusFailHigh(long Nodes, int Score)
        {
            var milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            var nodesPerSecond = Nodes / _stopwatch.Elapsed.TotalSeconds;
            _writeMessageLine($"info depth {_originalHorizon} seldepth {_selectiveHorizon} score lowerbound {Score} time {milliseconds:0} nodes {Nodes} nps {nodesPerSecond:0}");
        }


        private void UpdateStatusFailLow(long Nodes, int Score)
        {
            var milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            var nodesPerSecond = Nodes / _stopwatch.Elapsed.TotalSeconds;
            _writeMessageLine($"info depth {_originalHorizon} seldepth {_selectiveHorizon} score upperbound {Score} time {milliseconds:0} nodes {Nodes} nps {nodesPerSecond:0}");
        }


        public void Reset()
        {
            _stopwatch.Restart();
            SpecifiedMoves.Clear();
            // Reset move times and limits.
            WhiteTimeRemaining = null;
            BlackTimeRemaining = null;
            WhiteTimeIncrement = null;
            BlackTimeIncrement = null;
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