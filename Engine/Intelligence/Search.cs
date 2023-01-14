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


namespace ErikTheCoder.MadChess.Engine.Intelligence;


public sealed class Search : IDisposable
{
    public const int MaxHorizon = 64;
    public const int MaxQuietDepth = 8;
    public const int MaxPlyWithoutCaptureOrPawnMove = 100;
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
    public bool AnalyzeMode;
    public int MultiPv;
    public bool Continue;
    private const int _minMovesRemaining = 8;
    private const int _piecesMovesPer128 = 160;
    private const int _moveTimeHardLimitPer128 = 512;
    private const int _adjustMoveTimeMinDepth = 9;
    private const int _adjustMoveTimeMinScoreDecrease = 33;
    private const int _adjustMoveTimePer128 = 32;
    private const int _haveTimeSearchNextPlyPer128 = 70;
    private const int _nullMoveReduction = 3;
    private const int _nullStaticScoreReduction = 200;
    private const int _nullStaticScoreMaxReduction = 3;
    private const int _iidReduction = 2;
    private const int _singularMoveMinToHorizon = 7;
    private const int _singularMoveMaxInsufficientDraft = 3;
    private const int _singularMoveReductionPer128 = 64;
    private const int _singularMoveMargin = 2;
    private const int _lmrMaxIndex = 64;
    private const int _lmrScalePer128 = 40;
    private const int _lmrConstPer128 = -96;
    private const int _quietSearchMaxFromHorizon = 3;
    private static MovePriorityComparer _movePriorityComparer;
    private static ScoredMovePriorityComparer _scoredMovePriorityComparer;
    private static MoveScoreComparer _moveScoreComparer;
    private static int[] _futilityPruningMargins;
    private readonly TimeSpan _moveTimeReserved = TimeSpan.FromMilliseconds(100);
    private int[] _lateMovePruningMargins;
    private int[][] _lateMoveReductions; // [quietMoveNumber][toHorizon]
    private ScoredMove[] _rootMoves;
    private ScoredMove[] _bestMoves;
    private ScoredMove[] _bestMovePlies;
    private ScoredMove[] _multiPvMoves;
    private ulong[][][] _principalVariations; // [rootMoveIndex][depth][pvMoveIndex]
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

    static Search()
    {
        _movePriorityComparer = new MovePriorityComparer();
        _scoredMovePriorityComparer = new ScoredMovePriorityComparer();
        _moveScoreComparer = new MoveScoreComparer();
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
        // Create synchronization and diagnostic objects.
        Signal = new AutoResetEvent(false);
        _stopwatch = new Stopwatch();
        // Create search parameters.
        SpecifiedMoves = new List<ulong>();
        TimeRemaining = new TimeSpan?[2];
        TimeIncrement = new TimeSpan?[2];
        // To Horizon =                   000  001  002  003  004  005  006  007
        _futilityPruningMargins = new[] { 050, 066, 114, 194, 306, 450, 626, 834 }; // (16 * (toHorizon Pow 2)) + 50
        _lateMovePruningMargins = new[] { 999, 004, 007, 012, 019, 028, 039, 052 }; // (01 * (toHorizon Pow 2)) + 03... quiet search excluded
        Debug.Assert(_futilityPruningMargins.Length == _lateMovePruningMargins.Length);
        _lateMoveReductions = GetLateMoveReductions();
        // Create scored move and principal variation arrays.
        _rootMoves = new ScoredMove[Position.MaxMoves];
        _bestMoves = new ScoredMove[Position.MaxMoves];
        _bestMovePlies = new ScoredMove[MaxHorizon + 1];
        _multiPvMoves = new ScoredMove[Position.MaxMoves];
        _principalVariations = new ulong[Position.MaxMoves][][];
        for (var rootMoveIndex = 0; rootMoveIndex < Position.MaxMoves; rootMoveIndex++)
        {
            _principalVariations[rootMoveIndex] = new ulong[MaxHorizon + 2][]; // Guarantees var pvNextDepth = _principalVariations[rootMoveIndex][depth + 1] is in bounds.
            for (var depth = 0; depth <= MaxHorizon; depth++)
            {
                var remainingDepth = MaxHorizon + 2 - depth;
                _principalVariations[rootMoveIndex][depth] = new ulong[remainingDepth];
                for (var pvMoveIndex = 0; pvMoveIndex < remainingDepth; pvMoveIndex++)
                    _principalVariations[rootMoveIndex][depth][pvMoveIndex] = Move.Null;
            }
        }
        // Set Multi PV, analyze mode, and search strength.
        MultiPv = 1;
        AnalyzeMode = false;
        ConfigureFullStrength();
        _disposed = false;
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
            _futilityPruningMargins = null;
            _lateMovePruningMargins = null;
            _lateMoveReductions = null;
            _rootMoves = null;
            _bestMoves = null;
            _bestMovePlies = null;
            _multiPvMoves = null;
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
            _stopwatch = null;
        }
        // Release unmanaged resources.
        Signal?.Dispose();
        Signal = null;
        _disposed = true;
    }


    private static int[][] GetLateMoveReductions()
    {
        var lateMoveReductions = new int[_lmrMaxIndex + 1][];
        const double constReduction =  (double)_lmrConstPer128 / 128;
        for (var quietMoveNumber = 0; quietMoveNumber <= _lmrMaxIndex; quietMoveNumber++)
        {
            lateMoveReductions[quietMoveNumber] = new int[_lmrMaxIndex + 1];
            for (var toHorizon = 0; toHorizon <= _lmrMaxIndex; toHorizon++)
            {
                var logReduction = (double)_lmrScalePer128 / 128 * Math.Log2(quietMoveNumber) * Math.Log2(toHorizon);
                lateMoveReductions[quietMoveNumber][toHorizon] = (int)Math.Max(logReduction + constReduction, 0);
            }
        }
        return lateMoveReductions;
    }
    

    private void ConfigureLimitedStrength()
    {
        // Reset to full strength, then limit search capabilities.
        var elo = _elo;
        ConfigureFullStrength();
        _limitedStrength = true;
        _elo = elo;

        // See https://www.madchess.net/the-madchess-uci_limitstrength-algorithm/ for chart with NPS, Move Error, Blunder Error, and Blunder Percent values.
        var scale = 192d;
        var power = 4d; 
        var constant = 32;
        var ratingClass = (double)(_elo - Intelligence.Elo.Min) / 200;
        _nodesPerSecond = Eval.GetNonLinearBonus(ratingClass, scale, power, constant);
        
        // Enable errors on every move.
        scale = 0.5d;
        power = 2d;
        constant = 5;
        ratingClass = (double)(Intelligence.Elo.Max - _elo) / 200;
        _moveError = Eval.GetNonLinearBonus(ratingClass, scale, power, constant);
        
        // Enable occasional blunders.
        scale = 1d;
        power = 2.5d;
        constant = 25;
        _blunderError = Eval.GetNonLinearBonus(ratingClass, scale, power, constant);
        scale = 0.16d;
        power = 2d;
        constant = 5;
        _blunderPer128 = Eval.GetNonLinearBonus(ratingClass, scale, power, constant);

        if (_debug())
        {
            _writeMessageLine($"info string LimitStrength = {LimitedStrength}, ELO = {Elo}.");
            _writeMessageLine($"info string NPS = {_nodesPerSecond}, MoveError = {_moveError}, BlunderError = {_blunderError}, BlunderPer128 = {_blunderPer128}.");
        }
    }


    private void ConfigureFullStrength()
    {
        _elo = Intelligence.Elo.Min;
        _limitedStrength = false;
        _nodesPerSecond = null;
        _moveError = 0;
        _blunderError = 0;
        _blunderPer128 = 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ulong FindBestMove(Board board)
    {
        // Ensure all root moves are legal.
        board.CurrentPosition.GenerateMoves();
        var legalMoveIndex = 0;
        for (var moveIndex = 0; moveIndex < board.CurrentPosition.MoveIndex; moveIndex++)
        {
            var move = board.CurrentPosition.Moves[moveIndex];
            if (!ShouldSearchMove(move)) continue;
            var (legalMove, _) = board.PlayMove(move);
            board.UndoMove();
            if (legalMove)
            {
                // Move is legal.
                Move.SetPlayed(ref move, true); // All root moves will be played so set this in advance.
                board.CurrentPosition.Moves[legalMoveIndex] = move;
                legalMoveIndex++;
            }
        }
        board.CurrentPosition.MoveIndex = legalMoveIndex;
        _rootMoveNumber = 1;
        if ((legalMoveIndex == 1) && (SpecifiedMoves.Count == 0) && !AnalyzeMode)
        {
            // Only one legal move found.  Play it immediately.
            _stopwatch.Stop();
            return board.CurrentPosition.Moves[0];
        }
        // Copy legal moves to root moves and principal variations.
        for (var moveIndex = 0; moveIndex < legalMoveIndex; moveIndex++)
        {
            var move = board.CurrentPosition.Moves[moveIndex];
            _rootMoves[moveIndex] = new ScoredMove(move, -SpecialScore.Max);
            _principalVariations[moveIndex][0][0] = move;
        }
        // Determine score error.
        var scoreError = ((_blunderError > 0) && (SafeRandom.NextInt(0, 128) < _blunderPer128))
            ? _blunderError // Blunder
            : 0;
        scoreError = FastMath.Max(scoreError, _moveError);
        // Determine move time.
        GetMoveTime(board.CurrentPosition);
        board.NodesExamineTime = _nodesPerSecond.HasValue ? 1 : UciStream.NodesTimeInterval;
        // Iteratively deepen search.
        _originalHorizon = 0;
        var bestMove = new ScoredMove(Move.Null, -SpecialScore.Max);
        do
        {
            // Increment horizon and age move history.
            _originalHorizon++;
            _selectiveHorizon = 0;
            _moveHistory.Age();
            // Reset move scores, then search moves.
            for (var moveIndex = 0; moveIndex < legalMoveIndex; moveIndex++)
                _rootMoves[moveIndex].Score = -SpecialScore.Max;
            var score = GetDynamicScore(board, 0, _originalHorizon, -SpecialScore.Max, SpecialScore.Max);
            if (FastMath.Abs(score) == SpecialScore.Interrupted) break; // Stop searching.
            // Find best move.
            SortMovesByScore(_rootMoves, legalMoveIndex - 1);
            for (var moveIndex = 0; moveIndex < legalMoveIndex; moveIndex++)
                _bestMoves[moveIndex] = _rootMoves[moveIndex];
            bestMove = _bestMoves[0];
            _bestMovePlies[_originalHorizon] = bestMove;
            // Update principal variation status and determine whether to keep searching.
            if (PvInfoUpdate) UpdateStatus(board, true);
            if (MateInMoves.HasValue && (bestMove.Score >= SpecialScore.Checkmate) && (Eval.GetMateMoveCount(bestMove.Score) <= MateInMoves.Value)) break; // Found checkmate in correct number of moves.
            AdjustMoveTime();
            if (!HaveTimeForNextHorizon()) break; // Do not have time to search next ply.
        } while (Continue && (_originalHorizon < HorizonLimit));
        // Search is complete.  Return best move.
        _stopwatch.Stop();
        if (_debug()) _writeMessageLine($"info string Stopping search at {_stopwatch.Elapsed.TotalMilliseconds:0} milliseconds.");
        return scoreError == 0 ? bestMove.Move : GetInferiorMove(board.CurrentPosition, scoreError);
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
            var pieces = Bitwise.CountSetBits(position.Occupancy) - 2; // Do not include kings.
            movesRemaining = (pieces * _piecesMovesPer128) / 128;
        }
        movesRemaining = FastMath.Max(movesRemaining, _minMovesRemaining);
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AdjustMoveTime()
    {
        if (!CanAdjustMoveTime || (_originalHorizon < _adjustMoveTimeMinDepth) || (MoveTimeSoftLimit == MoveTimeHardLimit)) return;
        if (_bestMovePlies[_originalHorizon].Score >= (_bestMovePlies[_originalHorizon - 1].Score - _adjustMoveTimeMinScoreDecrease)) return;
        // Score has decreased significantly from last ply.
        if (_debug()) _writeMessageLine("Adjusting move time because score has decreased significantly from previous ply.");
        MoveTimeSoftLimit += TimeSpan.FromMilliseconds((MoveTimeSoftLimit.TotalMilliseconds * _adjustMoveTimePer128) / 128);
        if (MoveTimeSoftLimit > MoveTimeHardLimit) MoveTimeSoftLimit = MoveTimeHardLimit;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    private int GetDynamicScore(Board board, int depth, int horizon, int alpha, int beta, ulong excludedMove = 0)
    {
        _principalVariations[_rootMoveNumber - 1][depth][0] = Move.Null;
        if ((board.Nodes > board.NodesExamineTime) || _nodesPerSecond.HasValue)
        {
            ExamineTimeAndNodes(board.Nodes);
            var intervals = (int)(board.Nodes / UciStream.NodesTimeInterval);
            board.NodesExamineTime = _nodesPerSecond.HasValue
                ? board.Nodes + 1
                : UciStream.NodesTimeInterval * (intervals + 1);
        }
        if (!Continue && (_bestMoves[0].Move != Move.Null)) return SpecialScore.Interrupted; // Search was interrupted.
        var (terminalDraw, repeatPosition) = _eval.IsTerminalDraw(board.CurrentPosition);
        if (depth > 0)
        {
            // Mate Distance Pruning
            var lowestPossibleScore = Eval.GetMatedScore(depth);
            if (alpha < lowestPossibleScore)
            {
                alpha = lowestPossibleScore;
                if (lowestPossibleScore >= beta) return beta;
            }
            var highestPossibleScore = Eval.GetMatingScore(depth);
            if (beta > highestPossibleScore)
            {
                beta = highestPossibleScore;
                if (highestPossibleScore <= alpha) return alpha;
            }
            if (terminalDraw) return 0; // Game ends on this move.
        }
        // Get cached position.
        var toHorizon = horizon - depth;
        var historyIncrement = toHorizon * toHorizon;
        var cachedPosition = _cache[board.CurrentPosition.Key];
        ulong bestMove;
        if ((cachedPosition.Key != _cache.NullPosition.Key) && (depth > 0) && !repeatPosition)
        {
            // Position is cached and is not a root or repeat position.
            // Determine if dynamic score is cached.
            var cachedDynamicScore = GetCachedDynamicScore(cachedPosition.Data, depth, horizon, alpha, beta);
            if (cachedDynamicScore != SpecialScore.NotCached)
            {
                // Dynamic score is cached.
                if (cachedDynamicScore >= beta)
                {
                    bestMove = _cache.GetBestMove(cachedPosition.Data);
                    if ((bestMove != Move.Null) && Move.IsQuiet(bestMove))
                    {
                        // Assume the quiet best move specified by the cached position would have caused a beta cutoff.
                        // Update history heuristic.
                        _moveHistory.UpdateValue(bestMove, historyIncrement);
                    }
                }
                _stats.CacheScoreCutoff++;
                return cachedDynamicScore;
            }
        }
        if (toHorizon <= 0) return GetQuietScore(board, depth, depth, alpha, beta); // Search for a quiet position.
        // Evaluate static score.
        bool drawnEndgame;
        int phase;
        if (board.CurrentPosition.KingInCheck)
        {
            board.CurrentPosition.StaticScore = -SpecialScore.Max; // Do not evaluate static score because no moves are futile when king is in check.
            drawnEndgame = false;
            phase = Eval.DetermineGamePhase(board.CurrentPosition);
        }
        else if (board.PreviousPosition?.PlayedMove == Move.Null)
        {
            board.CurrentPosition.StaticScore = -board.PreviousPosition.StaticScore;
            drawnEndgame = false;
            phase = Eval.DetermineGamePhase(board.CurrentPosition);
        }
        else (board.CurrentPosition.StaticScore, drawnEndgame, phase) = _eval.GetStaticScore(board.CurrentPosition);
        // Even if endgame is drawn, search moves for a swindle (enemy mistake that makes drawn game winnable).
        if (IsPositionFutile(board.CurrentPosition, depth, horizon, drawnEndgame, alpha, beta))
        {
            // Position is futile.
            // Position is not the result of best play by both players.
            UpdateBestMoveCache(board.CurrentPosition, depth, horizon, Move.Null, beta, alpha, beta);
            return beta;
        }
        if (IsNullMovePermitted(board, beta))
        {
            // Null move is permitted.
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
            GetDynamicScore(board, depth, horizon - _iidReduction, alpha, beta);
            cachedPosition = _cache[board.CurrentPosition.Key];
            bestMove = _cache.GetBestMove(cachedPosition.Data);
        }
        // Search moves.
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
                    PrioritizeMoves(_rootMoves, lastMoveIndex, bestMove, depth);
                    SortMovesByPriority(_rootMoves, lastMoveIndex);
                }
                if (moveIndex > lastMoveIndex) break;
                move = _rootMoves[moveIndex].Move;
                _rootMove = move;
                _rootMoveNumber = legalMoveNumber + 1; // All root moves are legal.
            }
            else
            {
                // Search moves at current position.
                (move, moveIndex) = GetNextMove(board.CurrentPosition, Board.AllSquaresMask, depth, bestMove);
                if (move == Move.Null) break; // All moves have been searched.
            }
            if (Move.Equals(move, excludedMove)) continue; // Don't search excluded (potentially singular) move.
            if (Move.IsQuiet(move)) quietMoveNumber++;
            var futileMove = IsMoveFutile(board.CurrentPosition, depth, horizon, move, quietMoveNumber, drawnEndgame, phase, alpha, beta);
            var searchHorizon = GetSearchHorizon(board, depth, horizon, move, cachedPosition, quietMoveNumber, drawnEndgame);
            // Play and search move.
            var (legalMove, checkingMove) = board.PlayMove(move);
            if (!legalMove)
            {
                // Skip illegal move.
                if (Move.IsQuiet(move)) quietMoveNumber--;
                board.UndoMove();
                continue;
            }
            legalMoveNumber++;
            if ((legalMoveNumber > 1) && futileMove && !checkingMove)
            {
                // Skip futile move that doesn't deliver check.
                board.UndoMove();
                continue;
            }
            if (checkingMove) searchHorizon = horizon; // Do not reduce move that delivers check.
            Move.SetPlayed(ref move, true);
            board.PreviousPosition.Moves[moveIndex] = move;
            var moveBeta = (legalMoveNumber == 1) || ((MultiPv > 1) && (depth == 0))
                ? beta // Search with full alpha / beta window.
                : bestScore + 1; // Search with zero alpha / beta window.
            var score = -GetDynamicScore(board, depth + 1, searchHorizon, -moveBeta, -alpha);
            if (FastMath.Abs(score) == SpecialScore.Interrupted)
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
                    score = -GetDynamicScore(board, depth + 1, horizon, -beta, -alpha);
                }
            }
            board.UndoMove();
            if (FastMath.Abs(score) == SpecialScore.Interrupted) return score; // Stop searching.
            if ((score > alpha) && (score < beta) && (depth == 0)) _rootMoves[moveIndex].Score = score; // Update root move score.
            if (score >= beta)
            {
                // Position is not the result of best play by both players.
                if (Move.IsQuiet(move))
                {
                    // Update move heuristics.
                    _killerMoves.Update(depth, move);
                    _moveHistory.UpdateValue(move, historyIncrement);
                    // Decrement move index immediately so as not to include the quiet move that caused the beta cutoff.
                    moveIndex--;
                    while (moveIndex >= 0)
                    {
                        var priorMove = board.CurrentPosition.Moves[moveIndex];
                        if (Move.IsQuiet(priorMove) && Move.Played(priorMove))
                        {
                            // Update history of prior quiet move that failed to produce cutoff.
                            _moveHistory.UpdateValue(priorMove, -historyIncrement);
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
            if (score > alpha)
            {
                // Found new principal variation.
                var rootMoveIndex = _rootMoveNumber - 1;
                var pvThisDepth = _principalVariations[rootMoveIndex][depth];
                pvThisDepth[0] = move;
                var pvNextDepth = _principalVariations[rootMoveIndex][depth + 1];
                for (var pvMoveIndex = 0; pvMoveIndex < pvNextDepth.Length; pvMoveIndex++)
                {
                    var pvMove = pvNextDepth[pvMoveIndex];
                    pvThisDepth[pvMoveIndex + 1] = pvMove;
                    if (pvMove == Move.Null) break;
                }
                if (score > bestScore)
                {
                    // Found new best move.
                    bestScore = score;
                    UpdateBestMoveCache(board.CurrentPosition, depth, horizon, move, score, alpha, beta);
                    if ((depth > 0) || ((MultiPv == 1) && !LimitedStrength)) alpha = score; // Else leave alpha / beta window open when limiting strength.
                }
            }
            if (depth == 0)
            {
                // Searching root moves.
                var principalVariations = FastMath.Min(MultiPv, board.CurrentPosition.MoveIndex);
                if (principalVariations > 1)
                {
                    // MultiPV Search
                    var multiPvIndex = legalMoveNumber - 1;
                    _multiPvMoves[multiPvIndex] = _rootMoves[multiPvIndex];
                    if (legalMoveNumber >= principalVariations)
                    {
                        // Determine worst score among principal variations.
                        // For example: If MultiPV = 4 and legalMoveNumber = 8, find the 4th best move among the 8 moves searched.
                        SortMovesByScore(_multiPvMoves, multiPvIndex);
                        var worstPvScore = _multiPvMoves[principalVariations - 1].Score;
                        // Raise alpha because MultiPV best moves have been found.
                        alpha = worstPvScore;
                    }
                }
            }
            if ((_bestMoves[0].Move != Move.Null) && (board.Nodes >= board.NodesInfoUpdate)) UpdateStatus(board, false); // Update status.
        } while (true);
        if (legalMoveNumber == 0)
        {
            // Checkmate or Stalemate
            bestScore = board.CurrentPosition.KingInCheck ? Eval.GetMatedScore(depth) : 0;
        }
        if (bestScore <= originalAlpha) UpdateBestMoveCache(board.CurrentPosition, depth, horizon, Move.Null, bestScore, originalAlpha, beta); // Score failed low.
        return bestScore;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetQuietScore(Board board, int depth, int horizon, int alpha, int beta) => GetQuietScore(board, depth, horizon, Board.AllSquaresMask, alpha, beta);


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private int GetQuietScore(Board board, int depth, int horizon, ulong toSquareMask, int alpha, int beta)
    {
        if ((board.Nodes > board.NodesExamineTime) || _nodesPerSecond.HasValue)
        {
            ExamineTimeAndNodes(board.Nodes);
            var intervals = board.Nodes / UciStream.NodesTimeInterval; board.NodesExamineTime = _nodesPerSecond.HasValue
                ? board.Nodes + 1
                : UciStream.NodesTimeInterval * (intervals + 1);
        }
        if (!Continue && (_bestMoves[0].Move != Move.Null)) return SpecialScore.Interrupted; // Search was interrupted.
        var (terminalDraw, _) = _eval.IsTerminalDraw(board.CurrentPosition);
        if (depth > 0)
        {
            // Mate Distance Pruning
            var lowestPossibleScore = Eval.GetMatedScore(depth);
            if (alpha < lowestPossibleScore)
            {
                alpha = lowestPossibleScore;
                if (lowestPossibleScore >= beta) return beta;
            }
            var highestPossibleScore = Eval.GetMatingScore(depth);
            if (beta > highestPossibleScore)
            {
                beta = highestPossibleScore;
                if (highestPossibleScore <= alpha) return alpha;
            }
            if (terminalDraw) return 0; // Game ends on this move.
        }
        // Search for a quiet position where no captures are possible.
        var fromHorizon = depth - horizon;
        _selectiveHorizon = FastMath.Max(depth, _selectiveHorizon);
        bool drawnEndgame;
        int phase;
        Delegates.GetNextMove getNextMove;
        ulong moveGenerationToSquareMask;
        if (board.CurrentPosition.KingInCheck)
        {
            // King is in check.  Search all moves.
            getNextMove = _getNextMove;
            moveGenerationToSquareMask = Board.AllSquaresMask;
            board.CurrentPosition.StaticScore = -SpecialScore.Max; // Do not evaluate static score because no moves are futile when king is in check.
            drawnEndgame = false;
            phase = Eval.DetermineGamePhase(board.CurrentPosition);
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
            (board.CurrentPosition.StaticScore, drawnEndgame, phase) = _eval.GetStaticScore(board.CurrentPosition);
            if (board.CurrentPosition.StaticScore >= beta) return beta; // Prevent worsening of position by making a bad capture.  Stand pat.
            alpha = FastMath.Max(board.CurrentPosition.StaticScore, alpha);
        }
        // Even if endgame is drawn, search moves for a swindle (enemy mistake that makes drawn game winnable).
        var legalMoveNumber = 0;
        board.CurrentPosition.PrepareMoveGeneration();
        do
        {
            // Do not retrieve (or update) best move from the cache.  Rely on MVV / LVA move order.
            var (move, moveIndex) = getNextMove(board.CurrentPosition, moveGenerationToSquareMask, depth, Move.Null);
            if (move == Move.Null) break; // All moves have been searched.
            var futileMove = IsMoveFutile(board.CurrentPosition, depth, horizon, move, 0, drawnEndgame, phase, alpha, beta);
            // Play and search move.
            var (legalMove, checkingMove) = board.PlayMove(move);
            if (!legalMove)
            {
                // Skip illegal move.
                board.UndoMove();
                continue;
            }
            legalMoveNumber++;
            if ((legalMoveNumber > 1) && futileMove && !checkingMove)
            {
                // Skip futile move that doesn't deliver check.
                board.UndoMove();
                continue;
            }
            Move.SetPlayed(ref move, true);
            board.PreviousPosition.Moves[moveIndex] = move;
            var score = -GetQuietScore(board, depth + 1, horizon, toSquareMask, -beta, -alpha);
            board.UndoMove();
            if (FastMath.Abs(score) == SpecialScore.Interrupted) return score; // Stop searching.
            if (score >= beta) return beta; // Position is not the result of best play by both players.
            alpha = FastMath.Max(score, alpha);
        } while (true);
        if ((legalMoveNumber == 0) && board.CurrentPosition.KingInCheck) return Eval.GetMatedScore(depth); // Game ends on this move.
        // Return score of best move.
        return alpha;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
        if ((depth == 0) || (toHorizon >= _futilityPruningMargins.Length)) return false; // Root position or position far from search horizon is not futile.
        if (isDrawnEndgame || position.KingInCheck) return false; // Position in drawn endgame or when king is in check is not futile.
        if ((FastMath.Abs(alpha) >= SpecialScore.Checkmate) || (FastMath.Abs(beta) >= SpecialScore.Checkmate)) return false; // Position under threat of checkmate is not futile.
        // Position with lone king on board is not futile.
        if (Bitwise.CountSetBits(position.ColorOccupancy[(int)Color.White]) == 1) return false;
        if (Bitwise.CountSetBits(position.ColorOccupancy[(int)Color.Black]) == 1) return false;
        // Determine if any move can lower score to beta.
        var futilityPruningMargin = toHorizon <= 0 ? _futilityPruningMargins[0] : _futilityPruningMargins[toHorizon];
        return position.StaticScore - futilityPruningMargin > beta;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNullMovePermitted(Board board, int beta)
    {
        if (board.PreviousPosition?.PlayedMove == Move.Null) return false;
        if ((board.CurrentPosition.StaticScore < beta) || board.CurrentPosition.KingInCheck) return false; // Do attempt null move if static score is weak, nor if king is in check.
        // Do not attempt null move in pawn endgames.  Side to move may be in zugzwang.
        var minorAndMajorPieces = Bitwise.CountSetBits(board.CurrentPosition.GetMajorAndMinorPieces(board.CurrentPosition.ColorToMove));
        return minorAndMajorPieces > 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool DoesNullMoveCauseBetaCutoff(Board board, int depth, int horizon, int beta)
    {
        var reduction = _nullMoveReduction + Math.Min((board.CurrentPosition.StaticScore - beta) / _nullStaticScoreReduction, _nullStaticScoreMaxReduction);
        board.PlayNullMove();
        // Do not play two null moves consecutively.  Search with zero alpha / beta window.
        var score = -GetDynamicScore(board, depth + 1, horizon - reduction, -beta, -beta + 1);
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
                if (Move.Played(move) || ((moveIndex > 0) && Move.Equals(move, bestMove))) continue; // Do not play move twice.
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
                    lastMoveIndex = FastMath.Max(firstMoveIndex, position.MoveIndex - 1);
                    if (firstMoveIndex < lastMoveIndex) SortMovesByPriority(position.Moves, firstMoveIndex, lastMoveIndex); // Do not prioritize moves before sorting.  MVV / LVA is good enough when ordering captures.
                    position.MoveGenerationStage++;
                    continue;
                case MoveGenerationStage.NonCaptures:
                    firstMoveIndex = position.MoveIndex;
                    position.GenerateMoves(MoveGeneration.OnlyNonCaptures, Board.AllSquaresMask, toSquareMask);
                    // Prioritize and sort non-captures.
                    lastMoveIndex = FastMath.Max(firstMoveIndex, position.MoveIndex - 1);
                    if (firstMoveIndex < lastMoveIndex)
                    {
                        PrioritizeMoves(position.Moves, firstMoveIndex, lastMoveIndex, bestMove, depth);
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


    // Pass bestMove parameter even though it isn't referenced to satisfy GetNextMove delegate signature.
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
                    var lastMoveIndex = FastMath.Max(firstMoveIndex, position.MoveIndex - 1);
                    if (firstMoveIndex < lastMoveIndex) SortMovesByPriority(position.Moves, firstMoveIndex, lastMoveIndex); // Do not prioritize moves before sorting.  MVV / LVA is good enough when ordering captures.
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
    private bool IsMoveFutile(Position position, int depth, int horizon, ulong move, int quietMoveNumber, bool drawnEndgame, int phase, int alpha, int beta)
    {
        Debug.Assert(_futilityPruningMargins.Length == _lateMovePruningMargins.Length);
        var toHorizon = horizon - depth;
        if ((depth == 0) || (toHorizon >= _futilityPruningMargins.Length)) return false; // Root move or move far from search horizon is not futile.
        if (drawnEndgame || position.KingInCheck) return false; // Move in drawn endgame or move when king is in check is not futile.
        if ((FastMath.Abs(alpha) >= SpecialScore.Checkmate) || (FastMath.Abs(beta) >= SpecialScore.Checkmate)) return false; // Move under threat of checkmate is not futile.
        var captureVictim = Move.CaptureVictim(move);
        if ((captureVictim != Piece.None) && (toHorizon > 0)) return false; // Capture in main search is not futile.
        if ((Move.Killer(move) > 0) || (Move.PromotedPiece(move) != Piece.None) || Move.IsCastling(move)) return false; // Killer move, pawn promotion, or castling is not futile.
        if (Move.IsPawnMove(move))
        {
            var rank = Board.Ranks[(int)position.ColorToMove][(int)Move.To(move)];
            if (rank >= 6) return false; // Pawn push to 7th rank is not futile.
        }
        // Move with lone king on board is not futile.
        if (Bitwise.CountSetBits(position.ColorOccupancy[(int)Color.White]) == 1) return false;
        if (Bitwise.CountSetBits(position.ColorOccupancy[(int)Color.Black]) == 1) return false;
        var lateMoveNumber = toHorizon <= 0 ? _lateMovePruningMargins[0] : _lateMovePruningMargins[toHorizon];
        if (Move.IsQuiet(move) && (quietMoveNumber >= lateMoveNumber)) return true; // Quiet move is too late to be worth searching.
        // Determine if move can raise score to alpha.
        var materialImprovement = captureVictim == Piece.None
            ? 0
            : _eval.GetPieceMaterialScore(PieceHelper.GetColorlessPiece(captureVictim), phase);
        var locationImprovement = _eval.GetPieceLocationImprovement(move, phase);
        var futilityPruningMargin = toHorizon <= 0 ? _futilityPruningMargins[0] : _futilityPruningMargins[toHorizon];
        return position.StaticScore + materialImprovement + locationImprovement + futilityPruningMargin < alpha;
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
        if (Move.CaptureVictim(move) != Piece.None) return horizon; // Do not reduce capture.
        if (drawnEndgame || board.CurrentPosition.KingInCheck) return horizon; // Do not reduce move in drawn endgame or move when king is in check.
        if ((Move.Killer(move) > 0) || (Move.PromotedPiece(move) != Piece.None) || Move.IsCastling(move)) return horizon; // Do not reduce killer move, pawn promotion, or castling.
        if (Move.IsPawnMove(move))
        {
            var rank = Board.Ranks[(int)board.CurrentPosition.ColorToMove][(int)Move.To(move)];
            if (rank >= 6) return horizon; // Do not reduce pawn push to 7th rank.
        }
        if (!Move.IsQuiet(move)) return horizon; // Do not reduce tactical move.
        // Reduce search horizon of late move.
        var quietMoveIndex = FastMath.Min(quietMoveNumber, _lmrMaxIndex);
        var toHorizonIndex = FastMath.Min(horizon - depth, _lmrMaxIndex);
        return horizon - _lateMoveReductions[quietMoveIndex][toHorizonIndex];
    }


    // Singular move idea from Stockfish chess engine.
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private bool IsBestMoveSingular(Board board, int depth, int horizon, ulong move, CachedPosition cachedPosition)
    {
        // Determine if best move that had failed high in recent searches is best by a significant margin.
        var toHorizon = horizon - depth;
        if ((depth == 0) || (toHorizon < _singularMoveMinToHorizon)) return false;
        var dynamicScore = CachedPositionData.DynamicScore(cachedPosition.Data);
        if ((dynamicScore == SpecialScore.NotCached) || (FastMath.Abs(dynamicScore) >= SpecialScore.Checkmate)) return false;
        if (CachedPositionData.ScorePrecision(cachedPosition.Data) != ScorePrecision.LowerBound) return false;
        if (CachedPositionData.ToHorizon(cachedPosition.Data) < (toHorizon - _singularMoveMaxInsufficientDraft)) return false;
        var beta = dynamicScore - (_singularMoveMargin * toHorizon);
        var searchHorizon = depth + ((toHorizon * _singularMoveReductionPer128) / 128);
        dynamicScore = GetDynamicScore(board, depth, searchHorizon, beta - 1, beta, move); // Exclude best move from search.
        return dynamicScore < beta;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private int GetCachedDynamicScore(ulong cachedPositionData, int depth, int horizon, int alpha, int beta)
    {
        var dynamicScore = CachedPositionData.DynamicScore(cachedPositionData);
        if (dynamicScore == SpecialScore.NotCached) return SpecialScore.NotCached; // Score is not cached.
        var toHorizon = horizon - depth;
        var cachedToHorizon = CachedPositionData.ToHorizon(cachedPositionData);
        if (cachedToHorizon < toHorizon) return SpecialScore.NotCached; // Cached position is shallower than current horizon. Do not use cached score.
        // Adjust checkmate score.
        if (dynamicScore >= SpecialScore.Checkmate) dynamicScore -= depth;
        else if (dynamicScore <= -SpecialScore.Checkmate) dynamicScore += depth;
        var scorePrecision = CachedPositionData.ScorePrecision(cachedPositionData);
        // ReSharper disable once SwitchStatementMissingSomeCases
        switch (scorePrecision)
        {
            case ScorePrecision.Exact:
                if (dynamicScore <= alpha) return alpha; // Score fails low.
                if (dynamicScore >= beta) return beta; // Score fails high.
                return AnalyzeMode
                    ? SpecialScore.NotCached // Force continuing search of principal variation.
                    : dynamicScore;
            case ScorePrecision.UpperBound:
                if (dynamicScore <= alpha) return alpha; // Score fails low.
                break;
            case ScorePrecision.LowerBound:
                if (dynamicScore >= beta) return beta; // Score fails high.
                break;
            default:
                throw new Exception($"{scorePrecision} score precision not supported.");
        }
        return SpecialScore.NotCached;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void PrioritizeMoves(ScoredMove[] moves, int lastMoveIndex, ulong bestMove, int depth)
    {
        for (var moveIndex = 0; moveIndex <= lastMoveIndex; moveIndex++)
        {
            var move = moves[moveIndex].Move;
            // Prioritize best move.
            Move.SetIsBest(ref move, Move.Equals(move, bestMove));
            // Prioritize killer moves.
            Move.SetKiller(ref move, _killerMoves.GetValue(depth, move));
            // Prioritize by move history.
            Move.SetHistory(ref move, _moveHistory.GetValue(move));
            moves[moveIndex].Move = move;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PrioritizeMoves(ulong[] moves, int lastMoveIndex, ulong bestMove, int depth) => PrioritizeMoves(moves, 0, lastMoveIndex, bestMove, depth);


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void PrioritizeMoves(ulong[] moves, int firstMoveIndex, int lastMoveIndex, ulong bestMove, int depth)
    {
        for (var moveIndex = firstMoveIndex; moveIndex <= lastMoveIndex; moveIndex++)
        {
            var move = moves[moveIndex];
            // Prioritize best move.
            Move.SetIsBest(ref move, Move.Equals(move, bestMove));
            // Prioritize killer moves.
            Move.SetKiller(ref move, _killerMoves.GetValue(depth, move));
            // Prioritize by move history.
            Move.SetHistory(ref move, _moveHistory.GetValue(move));
            moves[moveIndex] = move;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SortMovesByPriority(ScoredMove[] moves, int lastMoveIndex) => Array.Sort(moves, 0, lastMoveIndex + 1, _scoredMovePriorityComparer);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortMovesByPriority(ulong[] moves, int lastMoveIndex) => Array.Sort(moves, 0, lastMoveIndex + 1, _movePriorityComparer);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SortMovesByPriority(ulong[] moves, int firstMoveIndex, int lastMoveIndex) => Array.Sort(moves, firstMoveIndex, lastMoveIndex - firstMoveIndex + 1, _movePriorityComparer);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SortMovesByScore(ScoredMove[] moves, int lastMoveIndex) => Array.Sort(moves, 0, lastMoveIndex + 1, _moveScoreComparer);


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void UpdateBestMoveCache(Position currentPosition, int depth, int horizon, ulong bestMove, int dynamicScore, int alpha, int beta)
    {
        if (FastMath.Abs(dynamicScore) == SpecialScore.Interrupted) return;
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
        // Adjust checkmate score.
        var adjustedDynamicScore = dynamicScore;
        if (adjustedDynamicScore >= SpecialScore.Checkmate) adjustedDynamicScore += depth;
        else if (adjustedDynamicScore <= -SpecialScore.Checkmate) adjustedDynamicScore -= depth;
        // Update score.
        if (adjustedDynamicScore <= alpha)
        {
            CachedPositionData.SetScorePrecision(ref cachedPosition.Data, ScorePrecision.UpperBound);
            CachedPositionData.SetDynamicScore(ref cachedPosition.Data, alpha);
        }
        else if (adjustedDynamicScore >= beta)
        {
            CachedPositionData.SetScorePrecision(ref cachedPosition.Data, ScorePrecision.LowerBound);
            CachedPositionData.SetDynamicScore(ref cachedPosition.Data, beta);
        }
        else
        {
            CachedPositionData.SetScorePrecision(ref cachedPosition.Data, ScorePrecision.Exact);
            CachedPositionData.SetDynamicScore(ref cachedPosition.Data, adjustedDynamicScore);
        }
        _cache[cachedPosition.Key] = cachedPosition;
    }

    
    private void UpdateStatus(Board board, bool includePrincipalVariations)
    {
        var milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
        var nodesPerSecond = board.Nodes / _stopwatch.Elapsed.TotalSeconds;
        var nodes = includePrincipalVariations ? board.Nodes : board.NodesInfoUpdate;
        var hashFull = (int)((1000L * _cache.Positions) / _cache.Capacity);
        if (includePrincipalVariations)
        {
            // Include principal variations.
            var principalVariations = FastMath.Min(MultiPv, board.CurrentPosition.MoveIndex);
            for (var pv = 0; pv < principalVariations; pv++)
            {
                var stringBuilder = new StringBuilder("pv");
                var bestMove = _bestMoves[pv];
                var move = bestMove.Move;
                for (var rootMoveIndex = 0; rootMoveIndex < Position.MaxMoves; rootMoveIndex++)
                {
                    if (Move.Equals(_principalVariations[rootMoveIndex][0][0], move))
                    {
                        for (var pvMoveIndex = 0; pvMoveIndex < MaxHorizon; pvMoveIndex++)
                        {
                            var pvMove = _principalVariations[rootMoveIndex][0][pvMoveIndex];
                            if (pvMove == Move.Null) goto writePv;
                            stringBuilder.Append($" {Move.ToLongAlgebraic(pvMove)}");
                        }
                    }
                }
                writePv:
                // Write message with principal variation(s).
                var pvLongAlgebraic = stringBuilder.ToString();
                var score = bestMove.Score;
                var multiPvPhrase = MultiPv > 1 ? $"multipv {pv + 1} " : null;
                var scorePhrase = FastMath.Abs(score) >= SpecialScore.Checkmate ? $"mate {Eval.GetMateMoveCount(score)}" : $"cp {score}";
                _writeMessageLine($"info {multiPvPhrase}depth {_originalHorizon} seldepth {FastMath.Max(_selectiveHorizon, _originalHorizon)} " +
                                  $"time {milliseconds:0} nodes {nodes} score {scorePhrase} nps {nodesPerSecond:0} {pvLongAlgebraic}");
            }
        }
        else
        {
            // Write message without principal variation(s).
            _writeMessageLine($"info depth {_originalHorizon} seldepth {FastMath.Max(_selectiveHorizon, _originalHorizon)} time {milliseconds:0} nodes {nodes} nps {nodesPerSecond:0}");
        }
        _writeMessageLine($"info hashfull {hashFull:0} currmove {Move.ToLongAlgebraic(_rootMove)} currmovenumber {_rootMoveNumber}");
        if (_debug()) _displayStats();
        var intervals = (int)(board.Nodes / UciStream.NodesInfoInterval);
        board.NodesInfoUpdate = UciStream.NodesInfoInterval * (intervals + 1);
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
        // Reset best moves.
        for (var moveIndex = 0; moveIndex < _bestMoves.Length; moveIndex++)
            _bestMoves[moveIndex] = new ScoredMove(Move.Null, -SpecialScore.Max);
        for (var depth = 0; depth < _bestMovePlies.Length; depth++)
            _bestMovePlies[depth] = new ScoredMove(Move.Null, -SpecialScore.Max);
        // Enable PV update, increment search counter, and continue search.
        PvInfoUpdate = true;
        _cache.Searches++;
        Continue = true;
    }
}
