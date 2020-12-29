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
        public const int MinElo = 400;
        public const int MaxElo = 2200;
        public SearchStats Stats;
        public AutoResetEvent Signal;
        public bool PvInfoUpdate;
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
        public int? NodesPerSecond;
        public int MoveError;
        public int BlunderError;
        public int BlunderPercent;
        public bool Continue;
        private const int _minMovesRemaining = 8;
        private const int _piecesMovesPer128 = 160;
        private const int _materialAdvantageMovesPer1024 = 25;
        private const int _moveTimeHardLimitPer128 = 512;
        private const int _adjustMoveTimeMinDepth = 9;
        private const int _adjustMoveTimeMinScoreDecrease = 33;
        private const int _adjustMoveTimePer128 = 32;
        private const int _haveTimeSearchNextPlyPer128 = 70;
        private const int _multiPvAspirationMinHorizon = 5;
        private const int _multiPvAspirationWindow = 100;
        private const int _nullMoveReduction = 3;
        private const int _estimateBestMoveReduction = 2;
        private const int _historyPriorMovePer128 = 256;
        private const int _quietSearchMaxFromHorizon = 3;
        private static MovePriorityComparer _movePriorityComparer;
        private static ScoredMovePriorityComparer _scoredMovePriorityComparer;
        private static MoveScoreComparer _moveScoreComparer;
        private static Delegates.GetStaticScore _getExchangeMaterialScore;
        private static int[] _futilityMargins;
        private static int[] _lateMovePruning;
        private readonly TimeSpan _moveTimeReserved = TimeSpan.FromMilliseconds(100);
        private int[] _lateMoveReductions;
        private ScoredMove[] _rootMoves;
        private ScoredMove[] _bestMoves;
        private ScoredMove[] _bestMovePlies;
        private ulong[][] _possibleVariations;
        private int[] _possibleVariationLength;
        private Dictionary<string, ulong[]> _principalVariations;
        private Cache _cache;
        private KillerMoves _killerMoves;
        private MoveHistory _moveHistory;
        private Evaluation _evaluation;
        private Delegates.Debug _debug;
        private Delegates.WriteMessageLine _writeMessageLine;
        private Stopwatch _stopwatch;
        private Delegates.GetNextMove _getNextMove;
        private Delegates.GetNextMove _getNextCapture;
        private Delegates.GetStaticScore _getStaticScore;
        private int _originalHorizon;
        private int _selectiveHorizon;
        private ulong _rootMove;
        private int _rootMoveNumber;
        private int _scoreError;
        private bool _limitStrength;
        private int _elo;
        private bool _disposed;


        public bool LimitStrength
        {
            get => _limitStrength;
            set
            {
                _limitStrength = value;
                if (_limitStrength && (_elo >= MinElo) && (_elo <= MaxElo))
                {
                    _evaluation.ConfigureStrength(_elo);
                    ConfigureStrength();
                }
            }
        }


        public int Elo
        {
            get => _elo;
            set
            {
                if ((value >= MinElo) && (value <= MaxElo))
                {
                    _elo = value;
                    if (_limitStrength)
                    {
                        _evaluation.ConfigureStrength(_elo);
                        ConfigureStrength();
                    }
                }
            }
        }


        static Search()
        {
            _movePriorityComparer = new MovePriorityComparer();
            _scoredMovePriorityComparer = new ScoredMovePriorityComparer();
            _moveScoreComparer = new MoveScoreComparer();
            _getExchangeMaterialScore = Evaluation.GetExchangeMaterialScore;
            // To Horizon =            000  001  002  003  004  005
            _futilityMargins = new[] { 050, 100, 175, 275, 400, 550 };
            _lateMovePruning = new[] { 999, 003, 006, 010, 015, 021 };
        }


        public Search(Cache Cache, KillerMoves KillerMoves, MoveHistory MoveHistory, Evaluation Evaluation, Delegates.Debug Debug, Delegates.WriteMessageLine WriteMessageLine)
        {
            _cache = Cache;
            _killerMoves = KillerMoves;
            _moveHistory = MoveHistory;
            _evaluation = Evaluation;
            _debug = Debug;
            _writeMessageLine = WriteMessageLine;
            _getNextMove = GetNextMove;
            _getNextCapture = GetNextCapture;
            _getStaticScore = _evaluation.GetStaticScore;
            Stats = new SearchStats();
            // Create synchronization and diagnostic objects.
            Signal = new AutoResetEvent(false);
            _stopwatch = new Stopwatch();
            // Create search parameters.
            // Quiet Move Number =       000  001  002  003  004  005  006  007  008  009  010  011  012  013  014  015  016  017  018  019  020  021  022  023  024  025  026  027  028  029  030  031
            _lateMoveReductions = new[] {000, 000, 000, 001, 001, 001, 001, 002, 002, 002, 002, 002, 002, 003, 003, 003, 003, 003, 003, 003, 003, 004, 004, 004, 004, 004, 004, 004, 004, 004, 004, 005};
            // Create scored move arrays.
            _rootMoves = new ScoredMove[Position.MaxMoves];
            _bestMoves = new ScoredMove[Position.MaxMoves];
            _bestMovePlies = new ScoredMove[Position.MaxMoves];
            // Create possible and principal variations.
            _possibleVariations = new ulong[MaxHorizon + 1][];
            for (var depth = 0; depth < _possibleVariations.Length; depth++) _possibleVariations[depth] = new ulong[MaxHorizon - depth];
            _possibleVariationLength = new int[MaxHorizon + 1];
            _principalVariations = new Dictionary<string, ulong[]>();
            _disposed = false;
            // Set default parameters.
            SetDefaultParameters();
            MultiPv = 1;
            TruncatePrincipalVariation = true;
            LimitStrength = false;
            Elo = MinElo;
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
                _evaluation = null;
                Stats = null;
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
                _lateMovePruning = null;
                _rootMoves = null;
                _bestMoves = null;
                _bestMovePlies = null;
                _movePriorityComparer = null;
                _scoredMovePriorityComparer = null;
                _moveScoreComparer = null;
                _possibleVariations = null;
                _possibleVariationLength = null;
                _principalVariations = null;
                _cache = null;
                _killerMoves = null;
                _moveHistory = null;
                _debug = null;
                _writeMessageLine = null;
                _stopwatch = null;
                _getNextMove = null;
                _getNextCapture = null;
                _getStaticScore = null;
            }
            // Release unmanaged resources.
            Signal?.Dispose();
            Signal = null;
            _disposed = true;
        }


        private void ConfigureStrength()
        {
            // Set default parameters.
            SetDefaultParameters();
            // Limit search speed.
            // Rating               400  600  800  1000  1200  1400   1600   1800    2000    2200
            // Nodes Per Second     100  116  356  1396  4196 10100  20836  38516   65636  105076
            var scale = 16d;
            var power = 4;
            var constant = 100;
            var ratingClass = (double) (_elo - MinElo) / 200;
            NodesPerSecond = Evaluation.GetNonLinearBonus(ratingClass, scale, power, constant);
            // Allow errors on every move.
            // Rating      400  600  800 1000 1200 1400 1600 1800 2000 2200
            // Move Error   81   64   49   36   25   16    9    4    1    0
            scale = 1d;
            power = 2;
            constant = 0;
            ratingClass = (double) (MaxElo - _elo) / 200;
            MoveError = Evaluation.GetNonLinearBonus(ratingClass, scale, power, constant);
            // Allow occasional blunders.
            // Rating         400  600  800  1000  1200  1400  1600  1800  2000  2200
            // Blunder Error  835  665  515   385   275   185   115    65    35    25
            // Blunder Pct     25   21   17    14    11     9     7     6     5     5
            scale = 10d;
            power = 2;
            constant = 25;
            BlunderError = Evaluation.GetNonLinearBonus(ratingClass, scale, power, constant);
            scale = 0.25d;
            power = 2;
            constant = 5;
            BlunderPercent = Evaluation.GetNonLinearBonus(ratingClass, scale, power, constant);
            if (_debug())
            {
                _writeMessageLine($"info string LimitStrength = {LimitStrength}, ELO = {Elo}.");
                _writeMessageLine($"info string NPS = {NodesPerSecond}, MoveError = {MoveError}, BlunderError = {BlunderError}, BlunderPercent = {BlunderPercent}.");
            }
        }


        private void SetDefaultParameters()
        {
            NodesPerSecond = null;
            MoveError = 0;
            BlunderError = 0;
            BlunderPercent = 0;
        }


        public ulong FindBestMove(Board Board)
        {
            // Ensure all root moves are legal.
            Board.CurrentPosition.GenerateMoves();
            var legalMoveIndex = 0;
            for (var moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++)
            {
                var move = Board.CurrentPosition.Moves[moveIndex];
                if (Board.IsMoveLegal(ref move))
                {
                    // Move is legal.
                    Move.SetPlayed(ref move, true); // All root moves will be played so set this in advance.
                    Board.CurrentPosition.Moves[legalMoveIndex] = move;
                    legalMoveIndex++;
                }
            }
            Board.CurrentPosition.MoveIndex = legalMoveIndex;
            if (legalMoveIndex == 1)
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
            _scoreError = ((BlunderError > 0) && (SafeRandom.NextInt(1, 101) <= BlunderPercent))
                ? BlunderError // Blunder
                : 0;
            _scoreError = Math.Max(_scoreError, MoveError);
            // Determine move time.
            GetMoveTime(Board.CurrentPosition);
            Board.NodesExamineTime = UciStream.NodesTimeInterval;
            // Iteratively deepen search.
            _originalHorizon = 0;
            var bestMove = new ScoredMove(Move.Null, -StaticScore.Max);
            var multiPvMaxScore = StaticScore.Max;
            var multiPvMinScore = -StaticScore.Max;
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
                _moveHistory.Age(Board.CurrentPosition.WhiteMove);
                int alpha;
                int beta;
                
                if ((MultiPv == 1) || (_originalHorizon < _multiPvAspirationMinHorizon))
                {
                    // Search with full alpha / beta window.
                    alpha = -StaticScore.Max;
                    beta = StaticScore.Max;
                }
                else
                {
                    // Search with aspiration window.
                    // This speeds up Multi-PV searches (for analysis) but slows down Single-PV searches (weakening engine when playing timed games).
                    alpha = Math.Max(multiPvMinScore - _multiPvAspirationWindow, -StaticScore.Max);
                    beta = Math.Min(multiPvMaxScore + _multiPvAspirationWindow, StaticScore.Max);
                }
                // Reset move scores then search moves.
                for (var moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++) _rootMoves[moveIndex].Score = -StaticScore.Max;
                var score = GetDynamicScore(Board, 0, _originalHorizon, false, alpha, beta);
                if (Math.Abs(score) == StaticScore.Interrupted) break; // Stop searching.
                SortMovesByScore(_rootMoves, Board.CurrentPosition.MoveIndex - 1);
                multiPvMaxScore = _rootMoves[0].Score;
                multiPvMinScore = _rootMoves[principalVariations - 1].Score;
                var failHigh = multiPvMaxScore >= beta;
                var failLow = !failHigh && (multiPvMinScore <= alpha);
                if (failHigh || failLow)
                {
                    if (PvInfoUpdate)
                    {
                        if (failHigh) UpdateInfoFailHigh(Board.Nodes, multiPvMaxScore);
                        if (failLow) UpdateInfoFailLow(Board.Nodes, multiPvMinScore);
                    }
                    // Reset move scores then search moves within infinite alpha / beta window.
                    for (var moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++) _rootMoves[moveIndex].Score = -StaticScore.Max;
                    score = GetDynamicScore(Board, 0, _originalHorizon, false, -StaticScore.Max, StaticScore.Max);
                    if (Math.Abs(score) == StaticScore.Interrupted) break; // Stop searching.
                    SortMovesByScore(_rootMoves, Board.CurrentPosition.MoveIndex - 1);
                    multiPvMaxScore = _rootMoves[0].Score;
                    multiPvMinScore = _rootMoves[principalVariations - 1].Score;
                }
                // Find best move.
                for (var moveIndex = 0; moveIndex < principalVariations; moveIndex++) _bestMoves[moveIndex] = _rootMoves[moveIndex];
                bestMove = _bestMoves[0];
                if (PvInfoUpdate) UpdateInfo(Board, true);
                _bestMovePlies[_originalHorizon] = bestMove;
                if (MateInMoves.HasValue && (Math.Abs(bestMove.Score) >= StaticScore.Checkmate) && (Evaluation.GetMateDistance(bestMove.Score) <= MateInMoves.Value)) break; // Found checkmate in correct number of moves.
                AdjustMoveTime();
                if (!HaveTimeForNextHorizon()) break; // Do not have time to search next ply.
            } while (Continue && (_originalHorizon < HorizonLimit));
            _stopwatch.Stop();
            if (_debug()) _writeMessageLine($"info string Stopping search at {_stopwatch.Elapsed.TotalMilliseconds:0} milliseconds.");
            return _scoreError == 0 ? bestMove.Move : GetInferiorMove(Board.CurrentPosition);
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
                var piecesMovesRemaining = (pieces * _piecesMovesPer128) / 128;
                var materialAdvantage = Math.Abs(_evaluation.GetMaterialScore(Position));
                var materialAdvantageMovesRemaining = (materialAdvantage * _materialAdvantageMovesPer1024) / 1024;
                movesRemaining = piecesMovesRemaining - materialAdvantageMovesRemaining;
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


        // TODO: Test score error.
        private ulong GetInferiorMove(Position Position)
        {
            // Determine how many moves are within score error.
            var bestScore = _bestMoves[0].Score;
            var worstScore = bestScore - _scoreError;
            var inferiorMoves = 0;
            for (var moveIndex = 1; moveIndex < Position.MoveIndex; moveIndex++)
            {
                if (_bestMoves[moveIndex].Score < worstScore) break;
                inferiorMoves++;
            }
            // Randomly select a move within score error.
            return _bestMoves[SafeRandom.NextInt(0, inferiorMoves)].Move;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetDynamicScore(Board Board, int Depth, int Horizon, bool IsNullMoveAllowed, int Alpha, int Beta)
        {
            if ((Board.Nodes > Board.NodesExamineTime) || NodesPerSecond.HasValue)
            {
                ExamineTimeAndNodes(Board.Nodes);
                var intervals = (int) (Board.Nodes / UciStream.NodesTimeInterval);
                Board.NodesExamineTime = UciStream.NodesTimeInterval * (intervals + 1);
            }
            if (!Continue && (_bestMoves[0].Move != Move.Null)) return StaticScore.Interrupted; // Search was interrupted.
            var (terminalDraw, repeatPosition) = _evaluation.IsTerminalDraw(Board.CurrentPosition);
            if ((Depth > 0) && terminalDraw) return 0; // Game ends on this move.
            // Get cached position.
            var toHorizon = Horizon - Depth;
            var historyIncrement = toHorizon * toHorizon;
            var cachedPosition = _cache.GetPosition(Board.CurrentPosition.Key);
            Debug.Assert(CachedPositionData.IsValid(cachedPosition.Data));
            ulong bestMove;
            if ((cachedPosition != _cache.NullPosition) && (Depth > 0) && !repeatPosition)
            {
                // Not a root or repeat position.
                // Determine if score is cached.
                var cachedScore = GetCachedScore(cachedPosition.Data, Depth, Horizon, Alpha, Beta);
                if (cachedScore != StaticScore.NotCached)
                {
                    // Score is cached.
                    if (cachedScore >= Beta)
                    {
                        bestMove = _cache.GetBestMove(cachedPosition);
                        if ((bestMove != Move.Null) && Move.IsQuiet(bestMove))
                        {
                            // Assume the quiet best move specified by the cached position would have caused a beta cutoff.
                            // Update history heuristic.
                            _moveHistory.UpdateValue(Board.CurrentPosition, bestMove, historyIncrement);
                        }
                    }
                    return cachedScore;
                }
            }
            if (toHorizon <= 0) return GetQuietScore(Board, Depth, Depth, Board.AllSquaresMask, Alpha, Beta, _getStaticScore, true); // Search for a quiet position.
            var drawnEndgame = Evaluation.IsDrawnEndgame(Board.CurrentPosition);
            var staticScore = Board.CurrentPosition.KingInCheck
                ? -StaticScore.Max
                : drawnEndgame ? 0 : _evaluation.GetStaticScore(Board.CurrentPosition);
            if (IsPositionFutile(Board.CurrentPosition, Depth, Horizon, staticScore, drawnEndgame, Alpha, Beta))
            {
                // Position is futile.
                // Position is not the result of best play by both players.
                UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, Move.Null, Beta, Alpha, Beta);
                return Beta;
            }
            if (IsNullMoveAllowed && Search.IsNullMoveAllowed(Board.CurrentPosition, staticScore, Beta))
            {
                // Null move is allowed.
                Stats.NullMoves++;
                if (DoesNullMoveCauseBetaCutoff(Board, Depth, Horizon, Beta))
                {
                    // Enemy is unable to capitalize on position even if player forfeits right to move.
                    // While forfeiting right to move is illegal, this indicates position is strong.
                    // Position is not the result of best play by both players.
                    Stats.NullMoveCutoffs++;
                    UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, Move.Null, Beta, Alpha, Beta);
                    return Beta;
                }
            }
            // Get best move.
            bestMove = _cache.GetBestMove(cachedPosition);
            if ((bestMove == Move.Null) && ((Beta - Alpha) > 1) && (toHorizon > _estimateBestMoveReduction))
            {
                // Cached position in a principal variation does not specify a best move.
                // Estimate best move by searching at reduced depth.
                GetDynamicScore(Board, Depth, Horizon - _estimateBestMoveReduction, false, Alpha, Beta);
                cachedPosition = _cache.GetPosition(Board.CurrentPosition.Key);
                bestMove = _cache.GetBestMove(cachedPosition);
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
                if (IsMoveFutile(Board, Depth, Horizon, move, legalMoveNumber, quietMoveNumber, staticScore, drawnEndgame, Alpha, Beta)) continue; // Move is futile.  Skip move.
                if (Move.IsQuiet(move)) quietMoveNumber++;
                var searchHorizon = GetSearchHorizon(Board, Depth, Horizon, move, quietMoveNumber, drawnEndgame);
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
                    if (moveBeta < Beta)
                    {
                        // Search move at reduced horizon with full alpha / beta window.
                        score = -GetDynamicScore(Board, Depth + 1, searchHorizon, true, -Beta, -Alpha);
                        if ((score > bestScore) && (searchHorizon < Horizon)) score = -GetDynamicScore(Board, Depth + 1, Horizon, true, -Beta, -Alpha); // Search move at unreduced horizon with full alpha / beta window.
                    }
                    else if (searchHorizon < Horizon) score = -GetDynamicScore(Board, Depth + 1, Horizon, true, -Beta, -Alpha); // Search move at unreduced horizon with full alpha / beta window.
                }
                Board.UndoMove();
                if (Math.Abs(score) == StaticScore.Interrupted) return score; // Stop searching.
                if (Depth == 0) _rootMoves[moveIndex].Score = score; // Update root move score.
                if (score >= Beta)
                {
                    // Position is not the result of best play by both players.
                    Stats.BetaCutoffs++;
                    if (legalMoveNumber == 1) Stats.BetaCutoffFirstMove++;
                    Stats.BetaCutoffMoveNumber += legalMoveNumber;
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
                                _moveHistory.UpdateValue(Board.CurrentPosition, priorMove, (-historyIncrement * _historyPriorMovePer128) / 128);
                            }
                            moveIndex--;
                        }
                    }
                    UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, move, score, Alpha, Beta);
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
                    if ((Depth > 0) || ((MultiPv == 1) && (_scoreError == 0))) Alpha = score;
                }
                if ((_bestMoves[0].Move != Move.Null) && (Board.Nodes >= Board.NodesInfoUpdate))
                {
                    // Update info.
                    UpdateInfo(Board, false);
                    var intervals = (int) (Board.Nodes / UciStream.NodesInfoInterval);
                    Board.NodesInfoUpdate = UciStream.NodesInfoInterval * (intervals + 1);
                }
            } while (true);
            if (legalMoveNumber == 0) bestScore = Board.CurrentPosition.KingInCheck ? Evaluation.GetMateScore(Depth) : 0; // Checkmate or Stalemate
            if (bestScore <= originalAlpha) UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, Move.Null, bestScore, originalAlpha, Beta); // Score fails low.
            return bestScore;
        }


        public int GetExchangeScore(Board Board, ulong Move)
        {
            var scoreBeforeMove = _getExchangeMaterialScore(Board.CurrentPosition);
            Board.PlayMove(Move);
            var scoreAfterMove = -GetQuietScore(Board, 0, 0, Board.SquareMasks[Engine.Move.To(Move)], -StaticScore.Max, StaticScore.Max, _getExchangeMaterialScore, false);
            Board.UndoMove();
            return scoreAfterMove - scoreBeforeMove;
        }
        

        public int GetQuietScore(Board Board, int Depth, int Horizon, ulong ToSquareMask, int Alpha, int Beta) => GetQuietScore(Board, Depth, Horizon, ToSquareMask, Alpha, Beta, _getStaticScore, true);


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetQuietScore(Board Board, int Depth, int Horizon, ulong ToSquareMask, int Alpha, int Beta, Delegates.GetStaticScore GetStaticScore, bool ConsiderFutility)
        {
            if ((Board.Nodes > Board.NodesExamineTime) || NodesPerSecond.HasValue)
            {
                ExamineTimeAndNodes(Board.Nodes);
                var intervals = Board.Nodes / UciStream.NodesTimeInterval;
                Board.NodesExamineTime = UciStream.NodesTimeInterval * (intervals + 1);
            }
            if (!Continue && (_bestMoves[0].Move != Move.Null)) return StaticScore.Interrupted; // Search was interrupted.
            var (terminalDraw, _) = _evaluation.IsTerminalDraw(Board.CurrentPosition);
            if ((Depth > 0) && terminalDraw) return 0; // Game ends on this move.
            // Search for a quiet position where no captures are possible.
            var fromHorizon = Depth - Horizon;
            _selectiveHorizon = Math.Max(Depth, _selectiveHorizon);
            var drawnEndgame = Evaluation.IsDrawnEndgame(Board.CurrentPosition);
            Delegates.GetNextMove getNextMove;
            int staticScore;
            ulong moveGenerationToSquareMask;
            if (Board.CurrentPosition.KingInCheck)
            {
                // King is in check.  Search all moves.
                getNextMove = _getNextMove;
                moveGenerationToSquareMask = Board.AllSquaresMask;
                staticScore = -StaticScore.Max; // Don't evaluate static score since moves when king is in check are not futile.
            }
            else
            {
                // King is not in check.  Search only captures.
                getNextMove = _getNextCapture;
                if (fromHorizon > _quietSearchMaxFromHorizon)
                {
                    var lastMoveToSquare = Move.To(Board.PreviousPosition.PlayedMove);
                    moveGenerationToSquareMask = lastMoveToSquare == Square.Illegal
                        ? ToSquareMask
                        : Board.SquareMasks[lastMoveToSquare]; // Search only recaptures.
                }
                else moveGenerationToSquareMask = ToSquareMask;
                staticScore = drawnEndgame ? 0 : GetStaticScore(Board.CurrentPosition);
                if (staticScore >= Beta) return Beta; // Prevent worsening of position by making a bad capture.  Stand pat.
                Alpha = Math.Max(staticScore, Alpha);
            }
            var legalMoveNumber = 0;
            Board.CurrentPosition.PrepareMoveGeneration();
            do
            {
                var (move, _) = getNextMove(Board.CurrentPosition, moveGenerationToSquareMask, Depth, Move.Null); // Don't retrieve (or update) best move from the cache.  Rely on MVV / LVA move order.
                if (move == Move.Null) break;
                if (Board.IsMoveLegal(ref move)) legalMoveNumber++; // Move is legal.
                else continue; // Skip illegal move.
                if (ConsiderFutility && IsMoveFutile(Board, Depth, Horizon, move, legalMoveNumber, 0, staticScore, drawnEndgame, Alpha, Beta)) continue; // Move is futile.  Skip move.
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
            if (NodesPerSecond.HasValue && (_originalHorizon > 1)) // Guarantee to search at least one ply.
            {
                // Slow search until it's less than specified nodes per second or until soft time limit is exceeded.
                var nodesPerSecond = int.MaxValue;
                while (nodesPerSecond > NodesPerSecond)
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
        private static bool IsPositionFutile(Position Position, int Depth, int Horizon, int StaticScore, bool IsDrawnEndgame, int Alpha, int Beta)
        {
            var toHorizon = Horizon - Depth;
            if (toHorizon >= _futilityMargins.Length) return false; // Position far from search horizon is not futile.
            if (IsDrawnEndgame || (Depth == 0) || Position.KingInCheck) return false; // Position in drawn endgame, at root, or when king is in check is not futile.
            if ((Math.Abs(Alpha) >= Engine.StaticScore.Checkmate) || (Math.Abs(Beta) >= Engine.StaticScore.Checkmate)) return false; // Position under threat of checkmate is not futile.
            // Count pawns and pieces (but don't include kings).
            var whitePawnsAndPieces = Bitwise.CountSetBits(Position.OccupancyWhite) - 1;
            var blackPawnsAndPieces = Bitwise.CountSetBits(Position.OccupancyBlack) - 1;
            if ((whitePawnsAndPieces == 0) || (blackPawnsAndPieces == 0)) return false; // Position with lone king on board is not futile.
            // Determine if any move can lower score to beta.
            var futilityMargin = toHorizon <= 0 ? _futilityMargins[0] : _futilityMargins[toHorizon];
            return StaticScore - futilityMargin > Beta;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNullMoveAllowed(Position Position, int StaticScore, int Beta)
        {
            if ((StaticScore < Beta) || Position.KingInCheck) return false;
            // Do not attempt null move in pawn endgames.  Side to move may be in zugzwang.
            var minorAndMajorPieces = Position.WhiteMove
                ? Bitwise.CountSetBits(Position.WhiteKnights | Position.WhiteBishops | Position.WhiteRooks | Position.WhiteQueens)
                : Bitwise.CountSetBits(Position.BlackKnights | Position.BlackBishops | Position.BlackRooks | Position.BlackQueens);
            return minorAndMajorPieces > 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DoesNullMoveCauseBetaCutoff(Board Board, int Depth, int Horizon, int Beta)
        {
            // Do not play two null moves consecutively.  Search with zero alpha / beta window.
            Board.PlayNullMove();
            var score = -GetDynamicScore(Board, Depth + 1, Horizon - _nullMoveReduction, false, -Beta, -Beta + 1);
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
                        Position.FindPotentiallyPinnedPieces();
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
                        Position.FindPotentiallyPinnedPieces();
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
        private static bool IsMoveFutile(Board Board, int Depth, int Horizon, ulong Move, int LegalMoveNumber, int QuietMoveNumber, int StaticScore, bool IsDrawnEndgame, int Alpha, int Beta)
        {
            var toHorizon = Horizon - Depth;
            if (toHorizon >= _futilityMargins.Length) return false; // Move far from search horizon is not futile.
            if ((Depth == 0) || (LegalMoveNumber == 1)) return false; // Root move or first move is not futile.
            if (IsDrawnEndgame || Engine.Move.IsCheck(Move) || Board.CurrentPosition.KingInCheck) return false; // Move in drawn endgame, checking move, or move when king is in check is not futile.
            if ((Math.Abs(Alpha) >= Engine.StaticScore.Checkmate) || (Math.Abs(Beta) >= Engine.StaticScore.Checkmate)) return false; // Move under threat of checkmate is not futile.
            var captureVictim = Engine.Move.CaptureVictim(Move);
            var capture = captureVictim != Piece.None;
            if (capture && (toHorizon > 0)) return false; // Capture in main search is not futile.
            if ((Engine.Move.Killer(Move) > 0) || (Engine.Move.PromotedPiece(Move) != Piece.None) || Engine.Move.IsCastling(Move)) return false; // Killer move, pawn promotion, or castling is not futile.
            if (Engine.Move.IsPawnMove(Move))
            {
                var rank = Board.CurrentPosition.WhiteMove ? Board.WhiteRanks[Engine.Move.From(Move)] : Board.BlackRanks[Engine.Move.From(Move)];
                if (rank >= 5) return false; // Pawn push is not futile.
            }
            // Count pawns and pieces (but don't include kings).
            var whitePawnsAndPieces = Bitwise.CountSetBits(Board.CurrentPosition.OccupancyWhite) - 1;
            var blackPawnsAndPieces = Bitwise.CountSetBits(Board.CurrentPosition.OccupancyBlack) - 1;
            if ((whitePawnsAndPieces == 0) || (blackPawnsAndPieces == 0)) return false; // Move with lone king on board is not futile.
            var lateMoveNumber = toHorizon <= 0 ? _lateMovePruning[0] : _lateMovePruning[toHorizon];
            if (Engine.Move.IsQuiet(Move) && (QuietMoveNumber >= lateMoveNumber)) return true; // Quiet move is too late to be worth searching.
            // Determine if move can raise score to alpha.
            var futilityMargin = toHorizon <= 0 ? _futilityMargins[0] : _futilityMargins[toHorizon];
            return StaticScore + Evaluation.GetMaterialScore(captureVictim) + futilityMargin < Alpha;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetSearchHorizon(Board Board, int Depth, int Horizon, ulong Move, int QuietMoveNumber, bool IsDrawnEndgame)
        {
            if ((Depth == 0) && ((MultiPv > 1) || (_scoreError > 0))) return Horizon; // Do not reduce root move in Multi-PV searches or when engine playing strength is reduced.
            var capture = Engine.Move.CaptureVictim(Move) != Piece.None;
            if (capture) return Horizon; // Do not reduce capture.
            if (IsDrawnEndgame || Engine.Move.IsCheck(Move) || Board.CurrentPosition.KingInCheck) return Horizon; // Do not reduce move in drawn endgame, checking move, or move when king is in check.
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
            // Reduce search horizon based on quiet move number.
            var reduction = Engine.Move.IsQuiet(Move) ? _lateMoveReductions[Math.Min(QuietMoveNumber, _lateMoveReductions.Length - 1)] : 0;
            return Horizon - reduction;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int GetCachedScore(ulong PositionData, int Depth, int Horizon, int Alpha, int Beta)
        {
            var score = CachedPositionData.Score(PositionData);
            if (score == StaticScore.NotCached) return StaticScore.NotCached; // Score is not cached.
            var toHorizon = Horizon - Depth;
            var cachedToHorizon = CachedPositionData.ToHorizon(PositionData);
            if (cachedToHorizon < toHorizon) return StaticScore.NotCached; // Cached position is shallower than current horizon. Do not use cached score.
            if (Math.Abs(score) >= StaticScore.Checkmate)
            {
                // Adjust checkmate score.
                if (score > 0) score -= Depth;
                else score += Depth;
            }
            var scorePrecision = CachedPositionData.ScorePrecision(PositionData);
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
        

        // TODO: Rename UpdateInfo to something more descriptive.
        // TODO: Determine if info can be split across more lines.  Test in Hiarcs, Shredder, Cute Chess, and Fritz GUIs.
        private void UpdateInfo(Board Board, bool IncludePrincipalVariation)
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
                    var scorePhrase = Math.Abs(score) >= StaticScore.Checkmate ? $"mate {Evaluation.GetMateDistance(score)}" : $"cp {score}";
                    _writeMessageLine($"info multipv {pv + 1} depth {_originalHorizon} seldepth {Math.Max(_selectiveHorizon, _originalHorizon)} " +
                                      $"time {milliseconds:0} nodes {nodes} score {scorePhrase} nps {nodesPerSecond:0} {pvLongAlgebraic}");
                }
            }
            else
            {
                _writeMessageLine($"info depth {_originalHorizon} seldepth {Math.Max(_selectiveHorizon, _originalHorizon)} " +
                                  $"time {milliseconds:0} nodes {nodes} nps {nodesPerSecond:0}");
            }
            _writeMessageLine($"info hashfull {hashFull:0} currmove {Move.ToLongAlgebraic(_rootMove)} currmovenumber {_rootMoveNumber}");
            if (_debug())
            {
                // Update stats.
                var nullMoveCutoffPercent = (100d * Stats.NullMoveCutoffs) / Stats.NullMoves;
                var betaCutoffMoveNumber = (double)Stats.BetaCutoffMoveNumber / Stats.BetaCutoffs;
                var betaCutoffFirstMovePercent = (100d * Stats.BetaCutoffFirstMove) / Stats.BetaCutoffs;
                _writeMessageLine($"info string Null Move Cutoffs = {nullMoveCutoffPercent:0.00}% Beta Cutoff Move Number = {betaCutoffMoveNumber:0.00} Beta Cutoff First Move = {betaCutoffFirstMovePercent: 0.00}%");
                _writeMessageLine($"info string Evals = {_evaluation.Stats.Evaluations}");
            }
            var intervals = (int) (Board.Nodes / UciStream.NodesInfoInterval);
            Board.NodesInfoUpdate = UciStream.NodesInfoInterval * (intervals + 1);
        }


        private void UpdateInfoFailHigh(long Nodes, int Score)
        {
            var milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            var nodesPerSecond = Nodes / _stopwatch.Elapsed.TotalSeconds;
            _writeMessageLine($"info depth {_originalHorizon} seldepth {_selectiveHorizon} score lowerbound {Score} time {milliseconds:0} nodes {Nodes} nps {nodesPerSecond:0}");
        }


        private void UpdateInfoFailLow(long Nodes, int Score)
        {
            var milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            var nodesPerSecond = Nodes / _stopwatch.Elapsed.TotalSeconds;
            _writeMessageLine($"info depth {_originalHorizon} seldepth {_selectiveHorizon} score upperbound {Score} time {milliseconds:0} nodes {Nodes} nps {nodesPerSecond:0}");
        }


        public void Reset(bool PreserveStats)
        {
            _stopwatch.Restart();
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
            // Reset score error, best moves, possible and principal variations, last alpha, and stats.
            _scoreError = 0;
            for (var moveIndex = 0; moveIndex < MultiPv; moveIndex++) _bestMoves[moveIndex] = new ScoredMove(Move.Null, -StaticScore.Max);
            for (var depth = 0; depth < _possibleVariationLength.Length; depth++) _possibleVariationLength[depth] = 0;
            _principalVariations.Clear();
            if (!PreserveStats) Stats.Reset();
            // Enable PV update, increment search counter, and continue search.
            PvInfoUpdate = true;
            _cache.Searches++;
            Continue = true;
        }
    }
}