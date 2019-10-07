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
using System.Text;
using System.Threading;


namespace ErikTheCoder.MadChess.Engine
{
    // TODO: Replace all divisions by multiples of 2 (to enable faster bit-shift operations).
    public sealed class Search : IDisposable
    {
        public const int MaxHorizon = 64;
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
        public bool TruncatePrincipalVariation;
        public int MultiPv;
        public int? NodesPerSecond;
        public int MoveError;
        public int BlunderError;
        public int BlunderPercent;
        public bool Continue;
        private const double _millisecondsReserved = 100d;
        private const int _minMovesRemaining = 8;
        private const int _piecesMovesPer128 = 160; // This improves integer division speed since x / 128 = x >> 7.
        private const int _materialAdvantageMovesPer1024 = 25; // This improves integer division speed since x / 1024 = x >> 10.
        private const int _moveTimeHardLimitPer128 = 512; // This improves integer division speed since x / 128 = x >> 7.
        private const int _haveTimeNextHorizonPer128 = 70; // This improves integer division speed since x / 128 = x >> 7.
        private const int _nullMoveReduction = 3;
        private const int _estimateBestMoveReduction = 2;
        private const int _pvsMinToHorizon = 3;
        private const int _historyPriorMovePer128 = 64;
        private const int _quietSearchMaxFromHorizon = 3;
        private static MovePriorityComparer _movePriorityComparer;
        private static MoveScoreComparer _moveScoreComparer;
        private int[] _singlePvAspirationWindows;
        private int[] _multiPvAspirationWindows;
        private int[] _scoreErrorAspirationWindows;
        private int[] _futilityMargins;
        private int[] _lateMoveReductions;
        private ulong[] _rootMoves;
        private int[] _rootScores;
        private ulong[] _bestMoves;
        private int[] _bestScores;
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
        private int _originalHorizon;
        private int _selectiveHorizon;
        private ulong _rootMove;
        private int _rootMoveNumber;
        private int _lastAspirationWindowIndex;
        private int _scoreError;
        private bool _limitStrength;
        private int _elo;
        private bool _disposed;


        public bool LimitStrength
        {
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
            _moveScoreComparer = new MoveScoreComparer();
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
            Stats = new SearchStats();
            // Create synchronization and diagnostic objects.
            Signal = new AutoResetEvent(false);
            _stopwatch = new Stopwatch();
            // Create search parameters.
            _singlePvAspirationWindows = new[] {100, 200, 500};
            _multiPvAspirationWindows = new[] {100, 125, 150, 175, 200, 225, 250, 300, 350, 400, 450, 500, 600, 700, 800, 900, 1000};
            _scoreErrorAspirationWindows = new int[1];
            // To Horizon =              000  001  002  003  004  005  006  007  008  009  010  011  012  013  014  015  016  017
            _futilityMargins = new[]    {050, 100, 175, 275, 400, 550};
            // Quiet Move Number =       000  001  002  003  004  005  006  007  008  009  010  011  012  013  014  015  016  017  018  019  020  021  022  023  024  025  026  027  028  029  030  031
            _lateMoveReductions = new[] {000, 000, 000, 001, 001, 001, 001, 002, 002, 002, 002, 002, 002, 002, 002, 003, 003, 003, 003, 003, 003, 003, 003, 003, 003, 003, 003, 003, 003, 003, 003, 004};
            // Create move and score arrays.
            _rootMoves = new ulong[Position.MaxMoves];
            _rootScores = new int[Position.MaxMoves];
            _bestMoves = new ulong[Position.MaxMoves];
            _bestScores = new int[Position.MaxMoves];
            // Create possible and principal variations.
            _possibleVariations = new ulong[MaxHorizon + 1][];
            for (int depth = 0; depth < _possibleVariations.Length; depth++) _possibleVariations[depth] = new ulong[MaxHorizon - depth];
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
                _moveScoreComparer = null;
                _singlePvAspirationWindows = null;
                _multiPvAspirationWindows = null;
                _scoreErrorAspirationWindows = null;
                _futilityMargins = null;
                _lateMoveReductions = null;
                _rootMoves = null;
                _rootScores = null;
                _bestMoves = null;
                _bestScores = null;
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
            double scale = 16d;
            int power = 4;
            int constant = 100;
            double ratingClass = (double) (_elo - MinElo) / 200;
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
            if (_debug()) _writeMessageLine($"info string NPS = {NodesPerSecond} MoveError = {MoveError} BlunderError = {BlunderError} BlunderPercent = {BlunderPercent}.");
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
            int legalMoveIndex = 0;
            for (int moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++)
            {
                ulong move = Board.CurrentPosition.Moves[moveIndex];
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
            Array.Copy(Board.CurrentPosition.Moves, _rootMoves, legalMoveIndex);
            for (int moveIndex = 0; moveIndex < legalMoveIndex; moveIndex++)
            {
                ulong[] principalVariation = new ulong[Position.MaxMoves];
                principalVariation[0] = Move.Null;
                _principalVariations.Add(Move.ToLongAlgebraic(Board.CurrentPosition.Moves[moveIndex]), principalVariation);
            }
            int principalVariations = Math.Min(MultiPv, legalMoveIndex);
            // Determine score error.
            _scoreError = 0;
            if ((BlunderError > 0) && (SafeRandom.NextInt(0, 101) <= BlunderPercent)) _scoreError = BlunderError; // Blunder
            _scoreError = Math.Max(_scoreError, MoveError);
            // Determine move time.
            GetMoveTime(Board.CurrentPosition);
            Board.NodesExamineTime = UciStream.NodesTimeInterval;
            // Iteratively deepen search.
            _originalHorizon = 0;
            ulong bestMove = Move.Null;
            do
            {
                // Update horizon.
                _originalHorizon++;
                _selectiveHorizon = 0;
                // Clear principal variations and age move history.
                using (Dictionary<string, ulong[]>.Enumerator pvEnumerator = _principalVariations.GetEnumerator())
                {
                    while (pvEnumerator.MoveNext()) pvEnumerator.Current.Value[0] = Move.Null;
                }
                _moveHistory.Age(Board.CurrentPosition.WhiteMove);
                // Get score within aspiration window.
                int score = GetScoreWithinAspirationWindow(Board, principalVariations);
                if (Math.Abs(score) == StaticScore.Interrupted) break; // Stop searching.
                // Find best moves.
                SortMovesByScore(_rootMoves, _rootScores, Board.CurrentPosition.MoveIndex - 1);
                int bestMoves = _scoreError == 0 ? principalVariations : legalMoveIndex;
                for (int moveIndex = 0; moveIndex < bestMoves; moveIndex++)
                {
                    _bestMoves[moveIndex] = _rootMoves[moveIndex];
                    _bestScores[moveIndex] = _rootScores[moveIndex];
                }
                if (PvInfoUpdate) UpdateInfo(Board, principalVariations, true);
                bestMove = _bestMoves[0];
                int bestScore = _bestScores[0];
                if (MateInMoves.HasValue && (Math.Abs(bestScore) >= StaticScore.Checkmate) && (Evaluation.GetMateDistance(bestScore) <= MateInMoves.Value)) break; // Found checkmate in correct number of moves.
                if (!HaveTimeForNextHorizon()) break; // Do not have time to search next depth.
            } while (Continue && (_originalHorizon < HorizonLimit));
            _stopwatch.Stop();
            if (_debug()) _writeMessageLine($"info string Stopping search at {_stopwatch.Elapsed.TotalMilliseconds:0} milliseconds.");
            return _scoreError == 0 ? bestMove : GetInferiorMove(Board.CurrentPosition);
        }


        private void GetMoveTime(Position Position)
        {
            // Determine if move time, horizon limit, or infinite move time is specified.
            if ((MoveTimeHardLimit != TimeSpan.MaxValue) || (HorizonLimit != MaxHorizon) || (WhiteTimeRemaining == TimeSpan.MaxValue) || (BlackTimeRemaining == TimeSpan.MaxValue)) return;
            if (!WhiteTimeRemaining.HasValue) throw new Exception("WhiteTimeRemaining is null.");
            if (!BlackTimeRemaining.HasValue) throw new Exception("BlackTimeRemaining is null.");
            // Retrieve time remaining and time increment.
            TimeSpan timeRemaining;
            TimeSpan timeIncrement;
            if (Position.WhiteMove)
            {
                timeRemaining = WhiteTimeRemaining.Value;
                timeIncrement = WhiteTimeIncrement ?? TimeSpan.Zero;
            }
            else
            {
                timeRemaining = BlackTimeRemaining.Value;
                timeIncrement = BlackTimeIncrement ?? TimeSpan.Zero;
            }
            int piecesMovesRemaining = 0;
            int materialAdvantageMovesRemaining = 0;
            int movesRemaining;
            if (MovesToTimeControl.HasValue) movesRemaining = MovesToTimeControl.Value;
            else
            {
                // Estimate moves remaining.
                int pieces = Bitwise.CountSetBits(Position.Occupancy) - 2; // Don't include kings.
                piecesMovesRemaining = (pieces * _piecesMovesPer128) / 128;
                int materialAdvantage = Math.Abs(_evaluation.GetMaterialScore(Position));
                materialAdvantageMovesRemaining = (materialAdvantage * _materialAdvantageMovesPer1024) / 1024;
                movesRemaining = Math.Max(piecesMovesRemaining - materialAdvantageMovesRemaining, _minMovesRemaining);
            }
            if (_debug())
            {
                _writeMessageLine($"Moves remaining = {piecesMovesRemaining} pieces moves - {materialAdvantageMovesRemaining} material advantage moves = {movesRemaining}.");
                _writeMessageLine($"Min moves remaining = {_minMovesRemaining}.");
            }
            // Calculate move time.
            double millisecondsRemaining = timeRemaining.TotalMilliseconds + (movesRemaining * timeIncrement.TotalMilliseconds);
            double milliseconds = millisecondsRemaining / movesRemaining;
            MoveTimeSoftLimit = TimeSpan.FromMilliseconds(milliseconds);
            MoveTimeHardLimit = TimeSpan.FromMilliseconds((milliseconds * _moveTimeHardLimitPer128) / 128);
            if (MoveTimeHardLimit.TotalMilliseconds > (timeRemaining.TotalMilliseconds - _millisecondsReserved))
            {
                // Prevent loss on time.
                MoveTimeSoftLimit = TimeSpan.FromMilliseconds(timeRemaining.TotalMilliseconds / _minMovesRemaining);
                MoveTimeHardLimit = MoveTimeSoftLimit;
            }
            if (_debug()) _writeMessageLine($"info string MoveTimeSoftLimit = {MoveTimeSoftLimit.TotalMilliseconds:0} MoveTimeHardLimit = {MoveTimeHardLimit.TotalMilliseconds:0}");
        }


        private ulong GetInferiorMove(Position Position)
        {
            // Determine how many moves are within score error.
            int bestScore = _bestScores[0];
            int worstScore = bestScore - _scoreError;
            int inferiorMoves = 0;
            for (int moveIndex = 0; moveIndex < Position.MoveIndex; moveIndex++)
            {
                if (_bestScores[moveIndex] < worstScore) break;
                inferiorMoves++;
            }
            // Randomly select a move within score error.
            return _bestMoves[SafeRandom.NextInt(0, inferiorMoves)];
        }


        private int GetScoreWithinAspirationWindow(Board Board, int PrincipalVariations)
        {
            int bestScore = _bestScores[0];
            if ((_originalHorizon == 1) || (Math.Abs(bestScore) >= StaticScore.Checkmate))
            {
                // Reset move scores.
                for (int moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++) _rootScores[moveIndex] = -StaticScore.Max;
                // Search moves with infinite aspiration window.
                return GetDynamicScore(Board, 0, _originalHorizon, false, -StaticScore.Max, StaticScore.Max);
            }
            int[] aspirationWindows;
            int aspirationStartingIndex;
            switch (_scoreError)
            {
                case 0 when PrincipalVariations == 1:
                    // Use single PV aspiration windows.
                    aspirationWindows = _singlePvAspirationWindows;
                    aspirationStartingIndex = 0;
                    break;
                case 0:
                    // Use multi PV aspiration windows.
                    aspirationWindows = _multiPvAspirationWindows;
                    aspirationStartingIndex = _originalHorizon == 1 ? 0 : _lastAspirationWindowIndex;
                    break;
                default:
                    _scoreErrorAspirationWindows[0] = (_scoreError + 1) * 2;
                    aspirationWindows = _scoreErrorAspirationWindows;
                    aspirationStartingIndex = 0;
                    break;
            }
            if (_debug()) _writeMessageLine($"info string Initial Aspiration Window = {aspirationWindows[aspirationStartingIndex]}");
            int alpha = 0;
            int beta = 0;
            ScorePrecision scorePrecision = ScorePrecision.Exact;
            for (int aspirationWindowIndex = 0; aspirationWindowIndex < aspirationWindows.Length; aspirationWindowIndex++)
            {
                int aspirationWindow = aspirationWindows[aspirationWindowIndex];
                if (_debug()) _writeMessageLine($"info string Aspiration Window = {aspirationWindow}");
                // Reset move scores.
                for (int moveIndex = 0; moveIndex < Board.CurrentPosition.MoveIndex; moveIndex++) _rootScores[moveIndex] = -StaticScore.Max;
                // Adjust alpha / beta window.
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (scorePrecision)
                {
                    case ScorePrecision.LowerBound:
                        // Fail high;
                        alpha = beta - 1;
                        beta += aspirationWindow;
                        break;
                    case ScorePrecision.UpperBound:
                        // Fail low
                        beta = alpha + 1;
                        alpha -= aspirationWindow;
                        break;
                    case ScorePrecision.Exact:
                        // Initial aspiration window
                        if (PrincipalVariations == 1)
                        {
                            // Center aspiration around best score from prior iteration.
                            alpha = bestScore - aspirationWindow / 2;
                            beta = bestScore + aspirationWindow / 2;
                        }
                        else
                        {
                            // Set aspiration window large enough to include all principal variations.
                            aspirationWindowIndex = _lastAspirationWindowIndex;
                            aspirationWindow = aspirationWindows[aspirationWindowIndex];
                            alpha = bestScore - aspirationWindow;
                            beta = bestScore + _multiPvAspirationWindows[0];
                        }
                        break;
                    default:
                        throw new Exception(scorePrecision + " score precision not supported.");
                }
                // Search moves with aspiration window.
                int score = GetDynamicScore(Board, 0, _originalHorizon, false, alpha, beta);
                if (Math.Abs(score) == StaticScore.Interrupted) return score; // Stop searching.
                if (score >= beta)
                {
                    // Search failed high.
                    scorePrecision = ScorePrecision.LowerBound;
                    if (PvInfoUpdate) UpdateInfoScoreOutsideAspirationWindow(Board.Nodes, score, true);
                    continue;
                }
                // Find lowest score.
                int lowestScore = PrincipalVariations == 1 ? score : GetBestScore(Board.CurrentPosition, PrincipalVariations);
                if (lowestScore <= alpha)
                {
                    // Search failed low.
                    scorePrecision = ScorePrecision.UpperBound;
                    if (PvInfoUpdate) UpdateInfoScoreOutsideAspirationWindow(Board.Nodes, score, false);
                    continue;
                }
                // Score within aspiration window.
                _lastAspirationWindowIndex = aspirationWindowIndex;
                return score;
            }
            // Search moves with infinite aspiration window.
            return GetDynamicScore(Board, 0, _originalHorizon, false, -StaticScore.Max, StaticScore.Max);
        }


        private int GetBestScore(Position Position, int Rank)
        {
            Debug.Assert(Rank > 0);
            if (Rank == 1)
            {
                int bestScore = -StaticScore.Max;
                for (int moveIndex = 0; moveIndex < Position.MoveIndex; moveIndex++)
                {
                    int score = _rootScores[moveIndex];
                    if (score > bestScore) bestScore = score;
                }
                return bestScore;
            }
            // Sort moves and return Rank best move (1 based index).
            Array.Sort(_rootScores, _rootMoves, 0, Position.MoveIndex, _moveScoreComparer);
            return _rootScores[Rank - 1];
        }


        private bool HaveTimeForNextHorizon()
        {
            if (MoveTimeSoftLimit == TimeSpan.MaxValue) return true;
            int moveTimePer128 = (int) ((128 * _stopwatch.Elapsed.TotalMilliseconds) / MoveTimeSoftLimit.TotalMilliseconds);
            return moveTimePer128 <= _haveTimeNextHorizonPer128;
        }


        private int GetDynamicScore(Board Board, int Depth, int Horizon, bool IsNullMoveAllowed, int Alpha, int Beta)
        {
            if ((Board.Nodes > Board.NodesExamineTime) || NodesPerSecond.HasValue)
            {
                // Examine time.
                ExamineTime(Board.Nodes);
                int intervals = (int) (Board.Nodes / UciStream.NodesTimeInterval);
                Board.NodesExamineTime = UciStream.NodesTimeInterval * (intervals + 1);
            }
            if (!Continue && (_bestMoves[0] != Move.Null)) return StaticScore.Interrupted; // Search was interrupted.
            (bool terminalDraw, int positionCount) = _evaluation.IsTerminalDraw(Board.CurrentPosition);
            if ((Depth > 0) && terminalDraw) return 0; // Terminal node (games ends on this move)
            // Get cached position.
            int toHorizon = Horizon - Depth;
            int historyIncrement = toHorizon * toHorizon;
            CachedPosition cachedPosition = _cache.GetPosition(Board.CurrentPosition.Key);
            ulong bestMove;
            if ((cachedPosition.Key != 0) && (Depth > 0) && (positionCount < 2))
            {
                // Not a root or repeat position.
                // Determine if score is cached.
                int cachedScore = GetCachedScore(cachedPosition.Data, Depth, Horizon, Alpha, Beta);
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
            if (toHorizon <= 0) return GetQuietScore(Board, Depth, Depth, Board.AllSquaresMask, Alpha, Beta); // Search for a quiet position.
            bool drawnEndgame = Evaluation.IsDrawnEndgame(Board.CurrentPosition);
            int staticScore = drawnEndgame ? 0 : _evaluation.GetStaticScore(Board.CurrentPosition);
            if (IsPositionFutile(Board.CurrentPosition, Depth, Horizon, staticScore, drawnEndgame, Beta))
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
            if ((bestMove == Move.Null) && (toHorizon > _estimateBestMoveReduction) && ((Beta - Alpha) > 1))
            {
                // Cached position in a principal variation does not specify a best move.
                // Estimate best move by searching at reduced depth.
                GetDynamicScore(Board, Depth, Horizon - _estimateBestMoveReduction, false, Alpha, Beta);
                cachedPosition = _cache.GetPosition(Board.CurrentPosition.Key);
                bestMove = _cache.GetBestMove(cachedPosition);
            }
            int originalAlpha = Alpha;
            int bestScore = Alpha;
            int legalMoveNumber = 0;
            int quietMoveNumber = 0;
            int moveIndex = -1;
            int lastMoveIndex = Board.CurrentPosition.MoveIndex - 1;
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
                        SortMovesByPriority(_rootMoves, _rootScores, lastMoveIndex);
                    }
                    if (moveIndex > lastMoveIndex) break;
                    move = _rootMoves[moveIndex];
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
                if (IsMoveFutile(Board.CurrentPosition, Depth, Horizon, move, legalMoveNumber, staticScore, drawnEndgame, Alpha, Beta)) continue; // Move is futile.  Skip move.
                if (Move.IsQuiet(move)) quietMoveNumber++;
                int searchHorizon = GetSearchHorizon(Board.CurrentPosition, Depth, Horizon, move, quietMoveNumber, drawnEndgame);
                int moveBeta;
                if ((legalMoveNumber == 1) || (toHorizon < _pvsMinToHorizon)) moveBeta = Beta; // Search with full alpha / beta window.
                else moveBeta = bestScore + 1; // Search with zero alpha / beta window.
                // Play and search move.
                Move.SetPlayed(ref move, true);
                Board.CurrentPosition.Moves[moveIndex] = move;
                Board.PlayMove(move);
                int score = -GetDynamicScore(Board, Depth + 1, searchHorizon, true, -moveBeta, -Alpha);
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
                if ((score > Alpha) && (score < Beta) && (Depth == 0)) _rootScores[moveIndex] = score; // Update root move score.
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
                            ulong priorMove = Board.CurrentPosition.Moves[moveIndex];
                            if (Move.IsQuiet(priorMove) && Move.Played(priorMove))
                            {
                                // Update history of prior quiet move that failed to produce cutoff.
                                _moveHistory.UpdateValue(Board.CurrentPosition, priorMove, (-historyIncrement * _historyPriorMovePer128) / 128 );
                            }
                            moveIndex--;
                        }
                    }
                    UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, move, score, Alpha, Beta);
                    return Beta;
                }
                bool withinAspirationWindow = (Depth == 0) && (score > Alpha);
                if (withinAspirationWindow || (score > bestScore))
                {
                    // Update possible variation.
                    _possibleVariations[Depth][0] = move;
                    int possibleVariationLength = _possibleVariationLength[Depth + 1];
                    Array.Copy(_possibleVariations[Depth + 1], 0, _possibleVariations[Depth], 1, possibleVariationLength);
                    _possibleVariationLength[Depth] = possibleVariationLength + 1;
                    if (Depth == 0)
                    {
                        // Update principal variation.
                        ulong[] principalVariation = _principalVariations[Move.ToLongAlgebraic(move)];
                        Array.Copy(_possibleVariations[0], 0, principalVariation, 0, _possibleVariationLength[0]);
                        principalVariation[_possibleVariationLength[0]] = Move.Null; // Mark last move of principal variation.
                    }
                }
                if (score > bestScore)
                {
                    // New principal variation 
                    bestScore = score;
                    UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, move, score, Alpha, Beta);
                    if ((Depth > 0) || ((MultiPv == 1) && (_scoreError == 0))) Alpha = score;
                }
                if ((_bestMoves[0] != Move.Null) && (Board.Nodes >= Board.NodesInfoUpdate))
                {
                    // Update info.
                    UpdateInfo(Board, 1, false);
                    int intervals = (int) (Board.Nodes / UciStream.NodesInfoInterval);
                    Board.NodesInfoUpdate = UciStream.NodesInfoInterval * (intervals + 1);
                }
            } while (true);
            if (legalMoveNumber == 0) bestScore = Board.CurrentPosition.KingInCheck ? Evaluation.GetMateScore(Depth) : 0; // Checkmate or stalemate
            if ((bestScore <= originalAlpha) || (bestScore >= Beta)) UpdateBestMoveCache(Board.CurrentPosition, Depth, Horizon, Move.Null, bestScore, originalAlpha, Beta); // Score fails low or high.
            return bestScore;
        }


        public int GetSwapOffScore(Board Board, ulong Move, int StaticScore)
        {
            // TODO: Calculate swap off score without playing any moves.
            ulong toSquareMask = Board.SquareMasks[Engine.Move.To(Move)];
            // Play and search move.
            Board.PlayMove(Move);
            int score = -GetQuietScore(Board, 1, 1, toSquareMask, -Engine.StaticScore.Max, Engine.StaticScore.Max);
            Board.UndoMove();
            return score - StaticScore;
        }


        public int GetQuietScore(Board Board, int Depth, int Horizon, ulong ToSquareMask, int Alpha, int Beta)
        {
            if ((Board.Nodes > Board.NodesExamineTime) || NodesPerSecond.HasValue)
            {
                // Examine time.
                ExamineTime(Board.Nodes);
                long intervals = Board.Nodes / UciStream.NodesTimeInterval;
                Board.NodesExamineTime = UciStream.NodesTimeInterval * (intervals + 1);
            }
            if (!Continue && (_bestMoves[0] != Move.Null)) return StaticScore.Interrupted; // Search was interrupted.
            (bool terminalDraw, _) = _evaluation.IsTerminalDraw(Board.CurrentPosition);
            if ((Depth > 0) && terminalDraw) return 0; // Terminal node (games ends on this move)
            // Search for a quiet position where no captures are possible.
            int fromHorizon = Depth - Horizon;
            _selectiveHorizon = Math.Max(Depth, _selectiveHorizon);
            bool drawnEndgame = Evaluation.IsDrawnEndgame(Board.CurrentPosition);
            Delegates.GetNextMove getNextMove;
            int staticScore;
            ulong moveGenerationToSquareMask;
            if (Board.CurrentPosition.KingInCheck)
            {
                // King is in check.  Search all moves.
                getNextMove = _getNextMove;
                moveGenerationToSquareMask = Board.AllSquaresMask;
                staticScore = 0; // Don't evaluate static score since moves when king is in check are not futile.
            }
            else
            {
                // King is not in check.  Search only captures.
                getNextMove = _getNextCapture;
                if (fromHorizon > _quietSearchMaxFromHorizon)
                {
                    int lastMoveToSquare = Move.To(Board.PreviousPosition.PlayedMove);
                    moveGenerationToSquareMask = lastMoveToSquare == Square.Illegal
                        ? ToSquareMask
                        : Board.SquareMasks[lastMoveToSquare]; // Search only recaptures.
                }
                else moveGenerationToSquareMask = ToSquareMask;
                staticScore = drawnEndgame ? 0 : _evaluation.GetStaticScore(Board.CurrentPosition);
                if (staticScore >= Beta) return Beta; // Prevent worsening of position by making a bad capture.  Stand pat.
                Alpha = Math.Max(staticScore, Alpha);
            }
            int legalMoveNumber = 0;
            Board.CurrentPosition.PrepareMoveGeneration();
            do
            {
                (ulong move, _) = getNextMove(Board.CurrentPosition, moveGenerationToSquareMask, Depth, Move.Null); // Don't retrieve (or update) best move from the cache.  Rely on MVV / LVA move order.
                if (move == Move.Null) break;
                if (Board.IsMoveLegal(ref move)) legalMoveNumber++; // Move is legal.
                else continue; // Skip illegal move.
                if (IsMoveFutile(Board.CurrentPosition, Depth, Horizon, move, legalMoveNumber, staticScore, drawnEndgame, Alpha, Beta)) continue; // Move is futile.  Skip move.
                // Play and search move.
                Board.PlayMove(move);
                int score = -GetQuietScore(Board, Depth + 1, Horizon, ToSquareMask, -Beta, -Alpha);
                Board.UndoMove();
                if (Math.Abs(score) == StaticScore.Interrupted) return score; // Stop searching.
                if (score >= Beta) return Beta; // Position is not the result of best play by both players.
                Alpha = Math.Max(score, Alpha);
            } while (true);
            if ((legalMoveNumber == 0) && Board.CurrentPosition.KingInCheck) return Evaluation.GetMateScore(Depth); // Terminal node (games ends on this move)
            // Return score of best move.
            return Alpha;
        }


        private void ExamineTime(long Nodes)
        {
            if (Nodes > NodeLimit) Continue = false; // Have passed node limit.
            if (NodesPerSecond.HasValue && (_originalHorizon > 1)) // Guarantee to search at least one ply.
            {
                // Slow search until it's less than specified nodes per second or until soft time limit is exceeded.
                int nodesPerSecond = int.MaxValue;
                while (nodesPerSecond > NodesPerSecond)
                {
                    // Delay search.
                    nodesPerSecond = (int)(Nodes / _stopwatch.Elapsed.TotalSeconds);
                    if (_stopwatch.Elapsed >= MoveTimeSoftLimit)
                    {
                        // No time is available to continue searching.
                        Continue = false;
                        return;
                    }
                }
            }
            else
            {
                // Search at full speed until hard time limit is exceeded.
                if (_stopwatch.Elapsed >= MoveTimeHardLimit) Continue = false; // No time is available to continue searching.
            }
        }


        private bool IsPositionFutile(Position Position, int Depth, int Horizon, int StaticScore, bool IsDrawnEndgame, int Beta)
        {
            if ((Depth == 0) || Position.KingInCheck || IsDrawnEndgame) return false; // Root, king in check, and drawn endgame positions are not futile.
            int toHorizon = Horizon - Depth;
            if (toHorizon >= _futilityMargins.Length) return false; // Position far from search horizon is not futile.
            // Determine if any move can lower score to beta.
            int futilityMargin = toHorizon <= 0 ? _futilityMargins[0] : _futilityMargins[toHorizon];
            return StaticScore - futilityMargin > Beta;
        }


        private static bool IsNullMoveAllowed(Position Position, int StaticScore, int Beta)
        {
            if ((StaticScore < Beta) || Position.KingInCheck) return false;
            // Do not attempt null move in pawn endgames.  Side to move may be in zugzwang.
            int minorAndMajorPieces = Position.WhiteMove
                ? Bitwise.CountSetBits(Position.WhiteKnights) + Bitwise.CountSetBits(Position.WhiteBishops) + Bitwise.CountSetBits(Position.WhiteRooks) + Bitwise.CountSetBits(Position.WhiteQueens)
                : Bitwise.CountSetBits(Position.BlackKnights) + Bitwise.CountSetBits(Position.BlackBishops) + Bitwise.CountSetBits(Position.BlackRooks) + Bitwise.CountSetBits(Position.BlackQueens);
            return minorAndMajorPieces > 0;
        }


        private bool DoesNullMoveCauseBetaCutoff(Board Board, int Depth, int Horizon, int Beta)
        {
            // Play and search null move.
            Board.PlayNullMove();
            // Do not play two null moves consecutively.  Search with zero alpha / beta window.
            int score = -GetDynamicScore(Board, Depth + 1, Horizon - _nullMoveReduction, false, -Beta, -Beta + 1);
            Board.UndoMove();
            return score >= Beta;
        }


        public (ulong Move, int MoveIndex) GetNextMove(Position Position, ulong ToSquareMask, int Depth, ulong BestMove)
        {
            while (true)
            {
                int firstMoveIndex;
                int lastMoveIndex;
                if (Position.CurrentMoveIndex < Position.MoveIndex)
                {
                    ulong move = Position.Moves[Position.CurrentMoveIndex];
                    if (Move.Played(move))
                    {
                        // Don't play best move twice.
                        Position.CurrentMoveIndex++;
                        continue;
                    }
                    (ulong Move, int MoveIndex) nextMove = (move, Position.CurrentMoveIndex);
                    Position.CurrentMoveIndex++;
                    return nextMove;
                }
                switch (Position.MoveGenerationStage)
                {
                    case MoveGenerationStage.BestMove:
                        Position.FindPotentiallyPinnedPieces();
                        if (BestMove != Move.Null)
                        {
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


        private static (ulong Move, int MoveIndex) GetNextCapture(Position Position, ulong ToSquareMask, int Depth, ulong BestMove)
        {
            while (true)
            {
                if (Position.CurrentMoveIndex < Position.MoveIndex)
                {
                    (ulong Move, int MoveIndex) nextMove = (Position.Moves[Position.CurrentMoveIndex], Position.CurrentMoveIndex);
                    if (Move.CaptureVictim(nextMove.Move) == Piece.None) continue;
                    Position.CurrentMoveIndex++;
                    return nextMove;
                }
                switch (Position.MoveGenerationStage)
                {
                    case MoveGenerationStage.BestMove:
                    case MoveGenerationStage.Captures:
                        Position.FindPotentiallyPinnedPieces();
                        int firstMoveIndex = Position.MoveIndex;
                        Position.GenerateMoves(MoveGeneration.OnlyCaptures, Board.AllSquaresMask, ToSquareMask);
                        int lastMoveIndex = Math.Max(firstMoveIndex, Position.MoveIndex - 1);
                        if (lastMoveIndex > firstMoveIndex) SortMovesByPriority(Position.Moves, firstMoveIndex, lastMoveIndex); // Don't prioritize moves.  MVV / LVA is good enough when ordering captures.
                        Position.MoveGenerationStage = MoveGenerationStage.End;
                        continue;
                    case MoveGenerationStage.End:
                        return (Move.Null, Position.CurrentMoveIndex);
                }
                break;
            }
            return (Move.Null, Position.CurrentMoveIndex);
        }


        private bool IsMoveFutile(Position Position, int Depth, int Horizon, ulong Move, int LegalMoveNumber, int StaticScore, bool IsDrawnEndgame, int Alpha, int Beta)
        {
            if ((Depth == 0) || (LegalMoveNumber == 1)) return false; // Root moves and first moves are not futile.
            int toHorizon = Horizon - Depth;
            if (toHorizon >= _futilityMargins.Length) return false; // Move far from search horizon is not futile.
            int captureVictim = Engine.Move.CaptureVictim(Move);
            bool capture = captureVictim != Piece.None;
            if (capture && (toHorizon > 0)) return false; // Capture in main search is not futile.
            if ((Engine.Move.Killer(Move) > 0) || (Engine.Move.PromotedPiece(Move) != Piece.None) || Engine.Move.IsCastling(Move)) return false; // Killer moves, pawn promotions, and castling are not futile.
            if (Engine.Move.IsPawnMove(Move))
            {
                int rank = Position.WhiteMove ? Board.WhiteRanks[Engine.Move.From(Move)] : Board.BlackRanks[Engine.Move.From(Move)];
                if (rank >= 5) return false; // Pawn pushes are not futile.
            }
            if (Engine.Move.IsCheck(Move) || Position.KingInCheck) return false; // Checking moves and moves when king is in check are not futile.
            if (IsDrawnEndgame || (Math.Abs(Alpha) >= Engine.StaticScore.Checkmate) || (Math.Abs(Beta) >= Engine.StaticScore.Checkmate)) return false; // Move in drawn endgame or under threat of checkmate is not futile.
            // Count pawns and pieces (but don't include kings).
            int whitePawnsAndPieces = Bitwise.CountSetBits(Position.OccupancyWhite) - 1;
            int blackPawnsAndPieces = Bitwise.CountSetBits(Position.OccupancyBlack) - 1;
            if ((whitePawnsAndPieces == 0) || (blackPawnsAndPieces == 0)) return false; // Move with lone king on board is not futile.
            // Determine if move can raise score to alpha.
            int futilityMargin = toHorizon <= 0 ? _futilityMargins[0] : _futilityMargins[toHorizon];
            return StaticScore + _evaluation.GetMaterialScore(captureVictim) + futilityMargin < Alpha;
        }


        private int GetSearchHorizon(Position Position, int Depth, int Horizon, ulong Move, int QuietMoveNumber, bool IsDrawnEndgame)
        {
            if ((Depth == 0) && ((MultiPv > 1) || (_scoreError > 0))) return Horizon; // Do not reduce root moves when MultiPV is enabled or engine playing strength is reduced.
            if (IsDrawnEndgame || (Engine.Move.CaptureVictim(Move) != Piece.None)) return Horizon; // Do not reduce search horizon of drawn endgames or captures.
            if ((Engine.Move.Killer(Move) > 0) || (Engine.Move.PromotedPiece(Move) != Piece.None) || Engine.Move.IsCastling(Move)) return Horizon; // Do not reduce search horizon of killer moves, pawn promotions, or castling.
            if (Engine.Move.IsPawnMove(Move))
            {
                int rank = Position.WhiteMove ? Board.WhiteRanks[Engine.Move.From(Move)] : Board.BlackRanks[Engine.Move.From(Move)];
                if (rank >= 5) return Horizon; // Do not reduce search horizon of pawn pushes.
            }
            if (Engine.Move.IsCheck(Move) || Position.KingInCheck) return Horizon;  // Do not reduce search horizon of checking moves or when king is in check.
            // Count pawns and pieces (but don't include kings).
            int whitePawnsAndPieces = Bitwise.CountSetBits(Position.OccupancyWhite) - 1;
            int blackPawnsAndPieces = Bitwise.CountSetBits(Position.OccupancyBlack) - 1;
            if ((whitePawnsAndPieces == 0) || (blackPawnsAndPieces == 0)) return Horizon; // Do not reduce search horizon of moves with lone king on board.
            // Reduce search horizon based on quiet move number.
            return Horizon - _lateMoveReductions[Math.Min(QuietMoveNumber, _lateMoveReductions.Length - 1)];
        }

        
        private int GetCachedScore(ulong PositionData, int Depth, int Horizon, int Alpha, int Beta)
        {
            int score = CachedPositionData.Score(PositionData);
            if (score == StaticScore.NotCached) return StaticScore.NotCached; // Score is not cached.
            int toHorizon = Horizon - Depth;
            int cachedToHorizon = CachedPositionData.ToHorizon(PositionData);
            if (cachedToHorizon < toHorizon) return StaticScore.NotCached; // Cached position is shallower than current horizon. Do not use cached score.
            if (Math.Abs(score) >= StaticScore.Checkmate)
            {
                // Adjust checkmate score.
                if (score > 0) score -= Depth;
                else score += Depth;
            }
            ScorePrecision scorePrecision = CachedPositionData.ScorePrecision(PositionData);
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


        public void PrioritizeMoves(Position Position, ulong[] Moves, int LastMoveIndex, ulong BestMove, int Depth) => PrioritizeMoves(Position, Moves, 0, LastMoveIndex, BestMove, Depth);


        private void PrioritizeMoves(Position Position, ulong[] Moves, int FirstMoveIndex, int LastMoveIndex, ulong BestMove, int Depth)
        {
            for (int moveIndex = FirstMoveIndex; moveIndex <= LastMoveIndex; moveIndex++)
            {
                ulong move = Moves[moveIndex];
                // Prioritize best move.
                Move.SetIsBest(ref move, Move.Equals(move, BestMove));
                // Prioritize killer moves.
                Move.SetKiller(ref move, _killerMoves.GetValue(Position, Depth, move));
                // Prioritize by move history.
                Move.SetHistory(ref move, _moveHistory.GetValue(Position, move));
                Moves[moveIndex] = move;
            }
        }


        public static void SortMovesByPriority(ulong[] Moves, int LastMoveIndex) => Array.Sort(Moves, 0, LastMoveIndex + 1, _movePriorityComparer);


        private static void SortMovesByPriority(ulong[] Moves, int FirstMoveIndex, int LastMoveIndex) => Array.Sort(Moves, FirstMoveIndex, LastMoveIndex - FirstMoveIndex + 1, _movePriorityComparer);


        private static void SortMovesByPriority(ulong[] Moves, int[] Scores, int LastMoveIndex) => Array.Sort(Moves, Scores, 0, LastMoveIndex + 1, _movePriorityComparer);


        private static void SortMovesByScore(ulong[] Moves, int[] Scores, int LastMoveIndex) => Array.Sort(Scores, Moves, 0, LastMoveIndex + 1, _moveScoreComparer);


        private void UpdateBestMoveCache(Position CurrentPosition, int Depth, int Horizon, ulong BestMove, int Score, int Alpha, int Beta)
        {
            if (Math.Abs(Score) == StaticScore.Interrupted) return;
            CachedPosition cachedPosition = _cache.NullPosition;
            cachedPosition.Key = CurrentPosition.Key;
            CachedPositionData.SetToHorizon(ref cachedPosition.Data, Horizon - Depth);
            if (BestMove != Move.Null)
            {
                // Set best move.
                CachedPositionData.SetBestMoveFrom(ref cachedPosition.Data, Move.From(BestMove));
                CachedPositionData.SetBestMoveTo(ref cachedPosition.Data, Move.To(BestMove));
                CachedPositionData.SetBestMovePromotedPiece(ref cachedPosition.Data, Move.PromotedPiece(BestMove));
            }
            int score = Score;
            if (Math.Abs(score) >= StaticScore.Checkmate)
            {
                // Adjust checkmate score.
                if (score > 0) score += Depth;
                else score -= Depth;
            }
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


        private void UpdateInfo(Board Board, int PrincipalVariations, bool IncludePrincipalVariation)
        {
            double milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            double nodesPerSecond = Board.Nodes / _stopwatch.Elapsed.TotalSeconds;
            long nodes = IncludePrincipalVariation ? Board.Nodes : Board.NodesInfoUpdate;
            for (int pv = 0; pv < PrincipalVariations; pv++)
            {
                string pvLongAlgebraic;
                if (IncludePrincipalVariation)
                {
                    ulong[] principalVariation = _principalVariations[Move.ToLongAlgebraic(_bestMoves[pv])];
                    // TODO: Review if long algebraic principal variation can be created without allocating a StringBuilder.
                    StringBuilder stringBuilder = new StringBuilder("pv");
                    for (int pvIndex = 0; pvIndex < principalVariation.Length; pvIndex++)
                    {
                        ulong move = principalVariation[pvIndex];
                        if (move == Move.Null) break;  // Null move marks the last move of the principal variation.
                        stringBuilder.Append(' ');
                        stringBuilder.Append(Move.ToLongAlgebraic(move));
                    }
                    pvLongAlgebraic = stringBuilder.ToString();
                }
                else pvLongAlgebraic = null;
                int score = _bestScores[pv];
                string scorePhrase = Math.Abs(score) >= StaticScore.Checkmate ? $"mate {Evaluation.GetMateDistance(score)}" : $"cp {score}";
                _writeMessageLine($"info multipv {(pv + 1)} depth {_originalHorizon} seldepth {Math.Max(_selectiveHorizon, _originalHorizon)} " +
                                  $"time {milliseconds:0} nodes {nodes} score {scorePhrase} nps {nodesPerSecond:0} {pvLongAlgebraic}");
            }
            int hashFull = (int) (1000L * _cache.Positions / _cache.Capacity);
            _writeMessageLine($"info hashfull {hashFull:0} currmove {Move.ToLongAlgebraic(_rootMove)} currmovenumber {_rootMoveNumber}");
            if (_debug())
            {
                // Update stats.
                double nullMoveCutoffPercent = 100d * Stats.NullMoveCutoffs / Stats.NullMoves;
                double betaCutoffMoveNumber = (double)Stats.BetaCutoffMoveNumber / Stats.BetaCutoffs;
                double betaCutoffFirstMovePercent = 100d * Stats.BetaCutoffFirstMove / Stats.BetaCutoffs;
                _writeMessageLine($"info string Null Move Cutoffs = {nullMoveCutoffPercent:0.00}% Beta Cutoff Move Number = {betaCutoffMoveNumber:0.00} Beta Cutoff First Move = {betaCutoffFirstMovePercent: 0.00}%");
                _writeMessageLine($"info string Evals = {_evaluation.Stats.Evaluations}");
            }
            int intervals = (int) (Board.Nodes / UciStream.NodesInfoInterval);
            Board.NodesInfoUpdate = UciStream.NodesInfoInterval * (intervals + 1);
        }


        private void UpdateInfoScoreOutsideAspirationWindow(long Nodes, int Score, bool FailHigh)
        {
            double milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
            double nodesPerSecond = Nodes / _stopwatch.Elapsed.TotalSeconds;
            string boundary = FailHigh ? "lowerbound " : "upperbound ";
            _writeMessageLine($"info multipv 1 depth {_originalHorizon} seldepth {_selectiveHorizon} score {boundary} {Score} time {milliseconds:0} nodes {Nodes} nps {nodesPerSecond:0}");
        }


        public void Reset(bool PreserveStats)
        {
            // Reset stopwatch.
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
            // Reset score error, best moves, possible and principal variations, and last aspiration window.
            _scoreError = 0;
            for (int moveIndex = 0; moveIndex < MultiPv; moveIndex++)
            {
                _bestMoves[moveIndex] = Move.Null;
                _bestScores[moveIndex] = -StaticScore.Max;
            }
            for (int depth = 0; depth < _possibleVariationLength.Length; depth++) _possibleVariationLength[depth] = 0;
            _principalVariations.Clear();
            _lastAspirationWindowIndex = 0;
            if (!PreserveStats) Stats.Reset();
            // Enable PV update, increment search counter, and continue search.
            PvInfoUpdate = true;
            _cache.Searches++;
            Continue = true;
        }
    }
}