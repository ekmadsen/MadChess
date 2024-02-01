// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
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
using ErikTheCoder.MadChess.Core;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Config;
using ErikTheCoder.MadChess.Engine.Hashtable;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Engine.Score;


namespace ErikTheCoder.MadChess.Engine.Intelligence;


public delegate (ulong Move, int MoveIndex) GetNextMove(ulong previousMove, Position position, ulong toSquareMask, int depth, ulong bestMove);


public sealed class Search : IDisposable
{
    public const int MaxHorizon = 64;
    public const int MaxQuietDepth = 8;
    public const int MaxPlyWithoutCaptureOrPawnMove = 100;

    public readonly AutoResetEvent Signal;
    public readonly List<ulong> CandidateMoves;

    public bool PvInfoUpdate;
    public bool AnalyzeMode;
    public int MultiPv;
    public long NodesExamineTime;
    public long NodesInfoUpdate;
    public int Count;
    public bool Continue;

    private const int _nullMoveReduction = 3;
    private const int _nullStaticScoreReduction = 180;
    private const int _nullStaticScoreMaxReduction = 3;
    private const int _iidReduction = 2;
    private const int _singularMoveMinToHorizon = 7;
    private const int _singularMoveMaxInsufficientDraft = 3;
    private const int _singularMoveReductionPer128 = 64;
    private const int _singularMoveMargin = 2;
    private const int _lmrMaxIndex = 64;
    private const int _lmrScalePer128 = 44;
    private const int _lmrConstPer128 = -96;
    private const int _recapturesOnlyMaxFromHorizon = 3;

    private readonly LimitStrengthSearchConfig _limitStrengthConfig;
    private readonly int[] _limitStrengthElos;
    private readonly Messenger _messenger; // Lifetime managed by caller.
    private readonly TimeManagement _timeManagement;
    private readonly Stats _stats;
    private readonly Cache _cache;
    private readonly KillerMoves _killerMoves;
    private readonly MoveHistory _moveHistory;
    private readonly Evaluation _evaluation;
    private readonly MovePriorityComparer _movePriorityComparer;
    private readonly ScoredMovePriorityComparer _scoredMovePriorityComparer;
    private readonly ScoredMoveComparer _scoredMoveComparer;
    private readonly GetNextMove _getNextMove;
    private readonly GetNextMove _getNextCapture;
    private readonly Stopwatch _stopwatch;
    private readonly int[] _futilityPruningMargins;
    private readonly int[] _lateMovePruningMargins;
    private readonly int[][] _lateMoveReductions; // [quietMoveNumber][toHorizon]
    private readonly ScoredMove[] _rootMoves;
    private readonly ScoredMove[] _bestMoves;
    private readonly ScoredMove[] _bestMovePlies;
    private readonly ScoredMove[] _multiPvMoves;
    private readonly ulong[][][] _principalVariations; // [rootMoveIndex][depth][pvMoveIndex]

    private int _originalHorizon;
    private int _selectiveHorizon;
    private ulong _rootMove;
    private int _rootMoveNumber;
    private bool _limitedStrength;
    private int _elo;
    private int? _nodesPerSecond;
    private int _phasedNodesPerSecond;
    private int _moveError;
    private int _blunderError;
    private int _blunderPer1024;


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


    public Search(LimitStrengthSearchConfig limitStrengthConfig, Messenger messenger, TimeManagement timeManagement, Stats stats, Cache cache, KillerMoves killerMoves, MoveHistory moveHistory, Evaluation evaluation)
    {
        _limitStrengthConfig = limitStrengthConfig;
        _messenger = messenger;
        _timeManagement = timeManagement;
        _stats = stats;
        _cache = cache;
        _killerMoves = killerMoves;
        _moveHistory = moveHistory;
        _evaluation = evaluation;

        _limitStrengthElos = [600, 800, 1000, 1200, 1400, 1600, 1800, 2000, 2200, 2300, 2400, 2600];

        _movePriorityComparer = new MovePriorityComparer();
        _scoredMovePriorityComparer = new ScoredMovePriorityComparer();
        _scoredMoveComparer = new ScoredMoveComparer();

        _getNextMove = GetNextMove;
        _getNextCapture = GetNextCapture;

        // Create synchronization and diagnostic objects.
        Signal = new AutoResetEvent(false);
        _stopwatch = new Stopwatch();

        // To Horizon =                   000  001  002  003  004  005  006  007
        _futilityPruningMargins = [050, 066, 114, 194, 306, 450, 626, 834]; // (16 * (toHorizon Pow 2)) + 50
        _lateMovePruningMargins = [999, 004, 007, 012, 019, 028, 039, 052]; // (01 * (toHorizon Pow 2)) + 03... quiet search excluded
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

        // Create candidate moves list and set default values.
        CandidateMoves = [];
        MultiPv = 1;
        AnalyzeMode = false;
        ConfigureFullStrength();
    }


    public void Dispose()
    {
        Signal?.Dispose();
    }


    private static int[][] GetLateMoveReductions()
    {
        var lateMoveReductions = new int[_lmrMaxIndex + 1][];
        const double constReduction = (double)_lmrConstPer128 / 128;
        for (var quietMoveNumber = 0; quietMoveNumber <= _lmrMaxIndex; quietMoveNumber++)
        {
            lateMoveReductions[quietMoveNumber] = new int[_lmrMaxIndex + 1];
            for (var toHorizon = 0; toHorizon <= _lmrMaxIndex; toHorizon++)
            {
                var logReduction = _lmrScalePer128 * Math.Log2(quietMoveNumber) * Math.Log2(toHorizon) / 128;
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

        // Limit search speed, enable errors on every move, and enable occasional blunders.
        (_nodesPerSecond, _moveError, _blunderError, _blunderPer1024) = CalculateLimitStrengthParams(_elo);

        if (_messenger.Debug)
        {
            _messenger.WriteLine($"info string LimitStrength = {LimitedStrength}, ELO = {Elo}.");
            _messenger.WriteLine($"info string NPS = {_nodesPerSecond}, MoveError = {_moveError}, BlunderError = {_blunderError}, BlunderPer1024 = {_blunderPer1024}.");
        }
    }


    private (int nodesPerSecond, int moveError, int blunderError, int blunderPer1024) CalculateLimitStrengthParams(int elo)
    {
        var ratingClass = elo / 200d;
        var nodesPerSecond = Formula.GetNonLinearBonus(ratingClass, _limitStrengthConfig.NpsScale, _limitStrengthConfig.NpsPower, _limitStrengthConfig.NpsConstant);

        ratingClass = (Intelligence.Elo.Max - elo) / 200d;
        var moveError = Formula.GetNonLinearBonus(ratingClass, _limitStrengthConfig.MoveErrorScale, _limitStrengthConfig.MoveErrorPower, _limitStrengthConfig.MoveErrorConstant);
        var blunderError = Formula.GetNonLinearBonus(ratingClass, _limitStrengthConfig.BlunderErrorScale, _limitStrengthConfig.BlunderErrorPower, _limitStrengthConfig.BlunderErrorConstant);
        var blunderPer1024 = Formula.GetNonLinearBonus(ratingClass, _limitStrengthConfig.BlunderPer1024Scale, _limitStrengthConfig.BlunderPer1024Power, _limitStrengthConfig.BlunderPer1024Constant);

        return (nodesPerSecond, moveError, blunderError, blunderPer1024);
    }


    private void ConfigureFullStrength()
    {
        _elo = Intelligence.Elo.Min;
        _limitedStrength = false;
        _nodesPerSecond = null;
        _phasedNodesPerSecond = 0;
        _moveError = 0;
        _blunderError = 0;
        _blunderPer1024 = 0;
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
        
        // Copy legal moves to root moves and principal variations.
        for (var moveIndex = 0; moveIndex < legalMoveIndex; moveIndex++)
        {
            var move = board.CurrentPosition.Moves[moveIndex];
            _rootMoves[moveIndex] = new ScoredMove(move, -StaticScore.Max);
            _principalVariations[moveIndex][0][0] = move;
        }

        // Determine score error and move time.
        var scoreError = ((_blunderError > 0) && (SafeRandom.NextInt(0, 1024) < _blunderPer1024))
            ? _blunderError // Blunder
            : 0;
        scoreError = FastMath.Max(scoreError, _moveError);
        _timeManagement.DetermineMoveTime(board.CurrentPosition, _stopwatch.Elapsed);
        NodesExamineTime = _nodesPerSecond.HasValue ? 1 : UciStream.NodesTimeInterval;

        // If strength is limited, slow search speed as game progresses towards the endgame.
        if (LimitedStrength) DeterminePhasedSearchSpeed(board.CurrentPosition);

        // Iteratively deepen search.
        _originalHorizon = 0;
        var bestMove = new ScoredMove(Move.Null, -StaticScore.Max);
        do
        {
            // Increment horizon, reset root move, and age move history.
            _originalHorizon++;
            _selectiveHorizon = 0;
            _rootMove = Move.Null;
            _rootMoveNumber = 1;
            _moveHistory.Age();

            // Reset move scores, then search moves.
            for (var moveIndex = 0; moveIndex < legalMoveIndex; moveIndex++)
                _rootMoves[moveIndex].Score = -StaticScore.Max;
            var score = GetDynamicScore(board, 0, _originalHorizon, false, -StaticScore.Max, StaticScore.Max);
            if (FastMath.Abs(score) == StaticScore.Interrupted) break; // Stop searching.

            // Find best move.
            SortMovesByScore(_rootMoves, legalMoveIndex - 1);
            for (var moveIndex = 0; moveIndex < legalMoveIndex; moveIndex++)
                _bestMoves[moveIndex] = _rootMoves[moveIndex];
            bestMove = _bestMoves[0];
            _bestMovePlies[_originalHorizon] = bestMove;

            // Update principal variation status and determine whether to keep searching.
            if (PvInfoUpdate) UpdateStatus(board, true);
            if (_timeManagement.MateInMoves.HasValue && (bestMove.Score >= StaticScore.Checkmate) && (Evaluation.GetMateMoveCount(bestMove.Score) <= _timeManagement.MateInMoves.Value)) break; // Found checkmate in correct number of moves.
            _timeManagement.AdjustMoveTime(_originalHorizon, _bestMovePlies);
            if (!_timeManagement.HaveTimeForNextHorizon(_stopwatch.Elapsed)) break; // Do not have time to search next ply.

        } while (Continue && (_originalHorizon < _timeManagement.HorizonLimit));

        // Search is complete.  Return best move.
        _stopwatch.Stop();
        if (_messenger.Debug) _messenger.WriteLine($"info string Stopping search at {_stopwatch.Elapsed.TotalMilliseconds:0} milliseconds.");
        if (PvInfoUpdate) UpdateStatus(board, true);
        return scoreError == 0 ? bestMove.Move : SelectInferiorMove(board, scoreError);
    }


    private void DeterminePhasedSearchSpeed(Position position)
    {
        if (!_nodesPerSecond.HasValue) throw new Exception("When engine strength is limited, NPS must be specified.");

        var phase = Evaluation.DetermineGamePhase(position);
        var minNodePerSecond = (_nodesPerSecond.Value * _limitStrengthConfig.NpsEndgamePer128) / 128;
        _phasedNodesPerSecond = Formula.GetLinearlyInterpolatedValue(minNodePerSecond, _nodesPerSecond.Value, phase, 0, Evaluation.MiddlegamePhase);
    }


    private bool ShouldSearchMove(ulong move)
    {
        if (CandidateMoves.Count == 0) return true; // Search all moves.
        // Search only candidate moves.
        for (var moveIndex = 0; moveIndex < CandidateMoves.Count; moveIndex++)
        {
            var candidateMove = CandidateMoves[moveIndex];
            if (Move.Equals(candidateMove, move)) return true;
        }
        return false;
    }


    private ulong SelectInferiorMove(Board board, int scoreError)
    {
        var bestMove = _bestMoves[0];
        var bestScore = bestMove.Score;
        if (bestScore >= StaticScore.SimpleEndgame) return bestMove.Move; // Ensure engine progresses towards checkmate.

        // Determine how many moves are within score error.
        var worstScore = bestScore - scoreError;
        var anyUnreasonableInferiorMoves = false;
        var inferiorMoves = 0;

        for (var moveIndex = 1; moveIndex < board.CurrentPosition.MoveIndex; moveIndex++)
        {
            var inferiorMove = _bestMoves[moveIndex];
            if (inferiorMove.Score < worstScore) break;
            if (IsInferiorMoveUnreasonable(board, inferiorMove.Move))
            {
                // Inferior move is unreasonable.
                _bestMoves[moveIndex].Score = -StaticScore.Max;
                anyUnreasonableInferiorMoves = true;
                continue;
            }
            inferiorMoves++;
        }

        if (anyUnreasonableInferiorMoves) SortMovesByScore(_bestMoves, board.CurrentPosition.MoveIndex - 1);  // Sort moves again so an unreasonable inferior move is not selected.

        // Randomly select a move within score error.
        return _bestMoves[SafeRandom.NextInt(0, inferiorMoves + 1)].Move;
    }


    private bool IsInferiorMoveUnreasonable(Board board, ulong move)
    {
        var fromSquare = Move.From(move);
        var kingMove = Move.IsKingMove(move);
        var rookMove = (Board.SquareMasks[(int)fromSquare] & board.CurrentPosition.GetRooks(board.CurrentPosition.ColorToMove)) > 0;
        var piece = board.CurrentPosition.GetPiece(fromSquare);

        if (Castling.Permitted(board.CurrentPosition.Castling) && (kingMove || rookMove))
        {
            var castlingMove = Move.IsCastling(move);
            if (Castling.Permitted(board.CurrentPosition.Castling, board.CurrentPosition.ColorToMove, BoardSide.Queen))
            {
                if (kingMove && !castlingMove) return true; // King move that forfeits queenside castling rights is unreasonable.
                if (rookMove && (piece == Piece.WhiteRook) && (fromSquare == Square.A1)) return true; // White rook move that forfeits queenside castling rights is unreasonable.
                if (rookMove && (piece == Piece.BlackRook) && (fromSquare == Square.A8)) return true; // Black rook move that forfeits queenside castling rights is unreasonable.
            }
            if (Castling.Permitted(board.CurrentPosition.Castling, board.CurrentPosition.ColorToMove, BoardSide.King))
            {
                if (kingMove && !castlingMove) return true; // King move that forfeits kingside castling rights is unreasonable.
                if (rookMove && (piece == Piece.WhiteRook) && (fromSquare == Square.H1)) return true; // White rook move that forfeits kingside castling rights is unreasonable.
                if (rookMove && (piece == Piece.BlackRook) && (fromSquare == Square.H8)) return true; // Black rook move that forfeits kingside castling rights is unreasonable.
            }
        }

        var toSquare = Move.To(move);
        var colorlessCaptureVictim = PieceHelper.GetColorlessPiece(Move.CaptureVictim(move));
        var pieceMove = (Board.SquareMasks[(int)fromSquare] & board.CurrentPosition.GetMajorAndMinorPieces(board.CurrentPosition.ColorToMove)) > 0;
        if ((colorlessCaptureVictim == ColorlessPiece.None) && pieceMove)
        {
            // Non-Capture Piece Move
            if ((Board.PawnAttackMasks[(int)board.CurrentPosition.ColorToMove][(int)toSquare] & board.CurrentPosition.GetPawns(board.CurrentPosition.ColorLastMoved)) > 0)
            {
                // Moving piece to square attacked by enemy pawn(s) is unreasonable.
                return true;
            }
        }
        
        var lastMoveColorlessCaptureVictim = PieceHelper.GetColorlessPiece(Move.CaptureVictim(board.PreviousPosition?.PlayedMove ?? Move.Null));
        if ((lastMoveColorlessCaptureVictim != ColorlessPiece.None) && (lastMoveColorlessCaptureVictim != ColorlessPiece.Pawn))
        {
            // Last move captured a minor or major piece.
            if (_evaluation.GetPieceMaterialScore(colorlessCaptureVictim, Evaluation.MiddlegamePhase) < _evaluation.GetPieceMaterialScore(lastMoveColorlessCaptureVictim, Evaluation.MiddlegamePhase))
            {
                // Move that fails to recapture equal or greater value piece is unreasonable.
                return true;
            }
        }

        for (var previousMoves = 2; previousMoves <= 6; previousMoves += 2)
        {
            var previousPosition = board.GetPreviousPosition(previousMoves);
            if (previousPosition == null) break;

            var previousMove = previousPosition.PlayedMove;
            var previousFromSquare = Move.From(previousMove);
            var previousToSquare = Move.To(previousMove);
            var previouslyMovedPiece = previousPosition.GetPiece(previousFromSquare);

            if ((previouslyMovedPiece == piece) && (previousToSquare == fromSquare) && (previousFromSquare == toSquare))
            {
                return true; // Shuffling piece between same two squares is unreasonable.
            }
        }

        var fromOwnBackRank = Board.Ranks[(int)board.CurrentPosition.ColorToMove][(int)fromSquare] == 0;
        var toOwnBackRank = Board.Ranks[(int)board.CurrentPosition.ColorToMove][(int)toSquare] == 0;
        return !kingMove && !fromOwnBackRank && toOwnBackRank; // Retreating minor or major piece to own back rank is unreasonable.
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetDynamicScore(Board board, int depth, int horizon, bool nullMovePermitted, int alpha, int beta)
    {
        return GetDynamicScore(board, depth, horizon, nullMovePermitted, alpha, beta, Move.Null);
    }


    private int GetDynamicScore(Board board, int depth, int horizon, bool nullMovePermitted, int alpha, int beta, ulong excludedMove)
    {
        _principalVariations[_rootMoveNumber - 1][depth][0] = Move.Null;

        // +---------------------------------------------------------------------------+
        // |                                                                           |
        // |                     Search Step 1: Terminate Search?                      |
        // |                                                                           |
        // +---------------------------------------------------------------------------+

        // Determine whether time allotted to play a move has elapsed.
        if ((board.Nodes > NodesExamineTime) || _nodesPerSecond.HasValue)
        {
            ExamineTimeAndNodes(board.Nodes);
            var intervals = (int)(board.Nodes / UciStream.NodesTimeInterval);
            NodesExamineTime = _nodesPerSecond.HasValue
                ? board.Nodes + 1
                : UciStream.NodesTimeInterval * (intervals + 1);
        }
        if (!Continue && (_bestMoves[0].Move != Move.Null)) return StaticScore.Interrupted; // Search was interrupted.

        // Mate Distance Pruning
        var (terminalDraw, repeatPosition) = _evaluation.IsTerminalDraw(board.CurrentPosition);
        if (depth > 0)
        {
            var lowestPossibleScore = Evaluation.GetMatedScore(depth);
            if (alpha < lowestPossibleScore)
            {
                alpha = lowestPossibleScore;
                if (lowestPossibleScore >= beta) return beta;
            }
            var highestPossibleScore = Evaluation.GetMatingScore(depth);
            if (beta > highestPossibleScore)
            {
                beta = highestPossibleScore;
                if (highestPossibleScore <= alpha) return alpha;
            }
            if (terminalDraw) return 0; // Game ends on this move.
        }

        // +---------------------------------------------------------------------------+
        // |                                                                           |
        // |                     Search Step 2: Cache Probe                            |
        // |                                                                           |
        // +---------------------------------------------------------------------------+

        // Determine if cached dynamic score causes a beta cutoff or score cutoff.
        var cachedPosition = _cache.GetPosition(board.CurrentPosition.Key, Count);
        var toHorizon = horizon - depth;
        var historyIncrement = toHorizon * toHorizon;
        var previousMove = board.PreviousPosition?.PlayedMove ?? Move.Null;
        ulong bestMove;

        if ((cachedPosition.Key != _cache.NullPosition.Key) && (depth > 0) && !repeatPosition)
        {
            // Position is cached and is not a root or repeat position.
            // Determine if dynamic score is cached.
            var cachedDynamicScore = GetCachedDynamicScore(cachedPosition.Data, depth, horizon, alpha, beta);
            if (cachedDynamicScore != StaticScore.NotCached)
            {
                // Dynamic score is cached.
                if (cachedDynamicScore >= beta)
                {
                    // Cached dynamic score causes a beta cutoff.
                    bestMove = _cache.GetBestMove(board.CurrentPosition, cachedPosition.Data);
                    if ((bestMove != Move.Null) && Move.IsQuiet(bestMove))
                    {
                        // Assume the quiet best move specified by the cached position would have caused a beta cutoff.
                        // Update history heuristic.
                        _moveHistory.UpdateValue(previousMove, bestMove, historyIncrement);
                    }
                }

                // Cached dynamic score causes a score cutoff.  No need to search position.
                _stats.CacheScoreCutoff++;
                return cachedDynamicScore;
            }
        }

        // +---------------------------------------------------------------------------+
        // |                                                                           |
        // |        Search Step 3: Static Evaluation & Futile Position Pruning         |
        // |                                                                           |
        // +---------------------------------------------------------------------------+

        if (toHorizon <= 0) return GetQuietScore(board, depth, depth, alpha, beta); // Search for a quiet position.

        // Evaluate static score.
        bool drawnEndgame;
        int phase;
        if (board.CurrentPosition.KingInCheck)
        {
            board.CurrentPosition.StaticScore = -StaticScore.Max; // Do not evaluate static score because no moves are futile when king is in check.
            drawnEndgame = false;
            phase = Evaluation.DetermineGamePhase(board.CurrentPosition);
        }
        else (board.CurrentPosition.StaticScore, drawnEndgame, phase) = _evaluation.GetStaticScore(board.CurrentPosition);

        // Even if endgame is drawn, search moves for a swindle (enemy mistake that makes drawn game winnable).
        if (IsPositionFutile(board.CurrentPosition, depth, horizon, drawnEndgame, alpha, beta))
        {
            // Position is futile.
            // Position is not the result of best play by both players.
            UpdateCache(board.CurrentPosition, depth, horizon, Move.Null, beta, alpha, beta);
            return beta;
        }

        // +---------------------------------------------------------------------------+
        // |                                                                           |
        // |          Search Step 4: Null Move & Internal Iterative Deepening          |
        // |                                                                           |
        // +---------------------------------------------------------------------------+

        if (nullMovePermitted && IsNullMovePermitted(board.CurrentPosition, beta))
        {
            // Null move is permitted.
            _stats.NullMoves++;
            if (DoesNullMoveCauseBetaCutoff(board, depth, horizon, beta))
            {
                // Enemy is unable to capitalize on position even if player forfeits right to move.
                // While forfeiting right to move is illegal, this indicates position is strong.
                // Position is not the result of best play by both players.
                UpdateCache(board.CurrentPosition, depth, horizon, Move.Null, beta, alpha, beta);
                _stats.NullMoveCutoffs++;
                return beta;
            }
        }

        var inPv = (beta - alpha) > 1;
        bestMove = _cache.GetBestMove(board.CurrentPosition, cachedPosition.Data);
        if ((bestMove == Move.Null) && inPv && (toHorizon > _iidReduction))
        {
            // Cached position in a principal variation does not specify best move.
            // Find best move via Internal Iterative Deepening.
            GetDynamicScore(board, depth, horizon - _iidReduction, false, alpha, beta);
            cachedPosition = _cache.GetPosition(board.CurrentPosition.Key, Count);
            bestMove = _cache.GetBestMove(board.CurrentPosition, cachedPosition.Data);
        }

        // +---------------------------------------------------------------------------+
        // |                                                                           |
        // |                     Search Step 5: Begin Move Loop                        |
        // |                                                                           |
        // +---------------------------------------------------------------------------+

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
                    PrioritizeMoves(previousMove, _rootMoves, lastMoveIndex, bestMove, depth);
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
                (move, moveIndex) = GetNextMove(previousMove, board.CurrentPosition, Board.AllSquaresMask, depth, bestMove);
                if (move == Move.Null) break; // All moves have been searched.
            }
            if (Move.Equals(move, excludedMove)) continue; // Do not search excluded (potentially singular) move.

            // +---------------------------------------------------------------------------+
            // |                                                                           |
            // |         Search Step 6: Futile Move Pruning & Late Move Reductions         |
            // |                                                                           |
            // +---------------------------------------------------------------------------+

            // Must call IsMoveInDynamicSearchFutile and GetSearchHorizon before board.PlayMove to avoid bugs related to incorrect KingInCheck and ColorToMove.
            if (Move.IsQuiet(move)) quietMoveNumber++;
            var futileMove = IsMoveInDynamicSearchFutile(board.CurrentPosition, depth, horizon, move, legalMoveNumber + 1, quietMoveNumber, drawnEndgame, phase, alpha, beta);
            var searchHorizon = GetSearchHorizon(board, depth, horizon, move, cachedPosition, legalMoveNumber + 1, quietMoveNumber, drawnEndgame);

            // Play move.
            var (legalMove, checkingMove) = board.PlayMove(move);
            if (!legalMove)
            {
                // Skip illegal move.
                if (Move.IsQuiet(move)) quietMoveNumber--;
                board.UndoMove();
                continue;
            }
            legalMoveNumber++;

            if (futileMove && !checkingMove)
            {
                // Skip futile move that doesn't check enemy king.
                board.UndoMove();
                continue;
            }
            if (checkingMove) searchHorizon = FastMath.Max(searchHorizon, horizon); // Do not reduce move that delivers check.

            // Search move.
            Move.SetPlayed(ref move, true);
            board.PreviousPosition.Moves[moveIndex] = move;
            var moveBeta = (legalMoveNumber == 1) || ((depth == 0) && (_limitedStrength || (MultiPv > 1)))
                ? beta // Search with full alpha / beta window.
                : bestScore + 1; // Search with zero alpha / beta window.
            var score = -GetDynamicScore(board, depth + 1, searchHorizon, true, -moveBeta, -alpha);
            if (FastMath.Abs(score) == StaticScore.Interrupted)
            {
                // Stop searching.
                board.UndoMove();
                return score;
            }

            if ((score > bestScore) || ((depth == 0) && (MultiPv > 1) && (score > alpha)))
            {
                // Move may be stronger than principal variation (or stronger than worst score among multiple principal variations).
                if ((moveBeta < beta) || (searchHorizon < horizon))
                {
                    // Search move at unreduced horizon with full alpha / beta window.
                    score = -GetDynamicScore(board, depth + 1, FastMath.Max(searchHorizon, horizon), true, -beta, -alpha);
                }
            }

            board.UndoMove();
            if (FastMath.Abs(score) == StaticScore.Interrupted) return score; // Stop searching.

            // +---------------------------------------------------------------------------+
            // |                                                                           |
            // |         Search Step 7: Beta Cutoff or New Principal Variation             |
            // |                                                                           |
            // +---------------------------------------------------------------------------+

            if (score >= beta)
            {
                // Position is not the result of best play by both players.
                if (Move.IsQuiet(move))
                {
                    // Update move heuristics.
                    _killerMoves.Update(depth, move);
                    _moveHistory.UpdateValue(previousMove, move, historyIncrement);

                    moveIndex--; // Decrement move index immediately so as not to include the quiet move that caused the beta cutoff.

                    while (moveIndex >= 0)
                    {
                        var priorMove = board.CurrentPosition.Moves[moveIndex];
                        if (Move.IsQuiet(priorMove) && Move.Played(priorMove))
                        {
                            // Update history of prior quiet move that failed to produce cutoff.
                            _moveHistory.UpdateValue(previousMove, priorMove, -historyIncrement);
                        }
                        moveIndex--;
                    }
                }

                // Update cache and stats.
                UpdateCache(board.CurrentPosition, depth, horizon, move, score, alpha, beta);
                if (excludedMove == Move.Null)
                {
                    _stats.MovesCausingBetaCutoff++;
                    _stats.BetaCutoffMoveNumber += legalMoveNumber;
                    if (legalMoveNumber == 1) _stats.BetaCutoffFirstMove++;
                }

                return beta;
            }

            if (score > alpha)
            {
                // Found new principal variation.
                if (depth == 0) _rootMoves[moveIndex].Score = score; // Update root move score.

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
                    UpdateCache(board.CurrentPosition, depth, horizon, move, score, alpha, beta);
                    // Raise alpha except when searching multiple principal variations or when limiting strength.
                    if ((depth > 0) || ((MultiPv == 1) && !LimitedStrength)) alpha = score;
                }
            }

            // +---------------------------------------------------------------------------+
            // |                                                                           |
            // |             Search Step 8: Principal Variations for Multi-PV              |
            // |                                                                           |
            // +---------------------------------------------------------------------------+

            if ((depth == 0) && (MultiPv > 1))
            {
                // Searching root moves for multiple principal variations.
                var multiPvIndex = legalMoveNumber - 1;
                _multiPvMoves[multiPvIndex] = _rootMoves[multiPvIndex];
                if (legalMoveNumber >= MultiPv)
                {
                    // Determine worst score among multiple principal variations.
                    // For example: If MultiPV = 4 and legalMoveNumber = 8, find the 4th best move among the 8 moves searched.
                    SortMovesByScore(_multiPvMoves, multiPvIndex);
                    var worstPvScore = _multiPvMoves[MultiPv - 1].Score;
                    // Raise alpha because MultiPV best moves have been found.
                    alpha = worstPvScore;
                }
            }

            // Update status.
            if ((_bestMoves[0].Move != Move.Null) && (board.Nodes >= NodesInfoUpdate)) UpdateStatus(board, false);

        } while (true);

        // +---------------------------------------------------------------------------+
        // |                                                                           |
        // |            Search Step 9: End Move Loop & Return Dynamic Score            |
        // |                                                                           |
        // +---------------------------------------------------------------------------+

        if (legalMoveNumber == 0)
        {
            // Checkmate or Stalemate
            bestScore = board.CurrentPosition.KingInCheck ? Evaluation.GetMatedScore(depth) : 0;
        }
        if (bestScore <= originalAlpha)
        {
            // Score failed low.
            UpdateCache(board.CurrentPosition, depth, horizon, Move.Null, bestScore, originalAlpha, beta);
        }
        return bestScore;
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetQuietScore(Board board, int depth, int horizon, int alpha, int beta) => GetQuietScore(board, depth, horizon, Board.AllSquaresMask, 0, alpha, beta);


    private int GetQuietScore(Board board, int depth, int horizon, ulong toSquareMask, int checksInQuietSearch, int alpha, int beta)
    {
        if ((board.Nodes > NodesExamineTime) || _nodesPerSecond.HasValue)
        {
            ExamineTimeAndNodes(board.Nodes);
            var intervals = (int)(board.Nodes / UciStream.NodesTimeInterval);
            NodesExamineTime = _nodesPerSecond.HasValue
                ? board.Nodes + 1
                : UciStream.NodesTimeInterval * (intervals + 1);
        }
        if (!Continue && (_bestMoves[0].Move != Move.Null)) return StaticScore.Interrupted; // Search was interrupted.

        // Mate Distance Pruning
        var (terminalDraw, _) = _evaluation.IsTerminalDraw(board.CurrentPosition);
        if (depth > 0)
        {
            var lowestPossibleScore = Evaluation.GetMatedScore(depth);
            if (alpha < lowestPossibleScore)
            {
                alpha = lowestPossibleScore;
                if (lowestPossibleScore >= beta) return beta;
            }
            var highestPossibleScore = Evaluation.GetMatingScore(depth);
            if (beta > highestPossibleScore)
            {
                beta = highestPossibleScore;
                if (highestPossibleScore <= alpha) return alpha;
            }
            if (terminalDraw) return 0; // Game ends on this move.
        }

        // Search for a quiet position where no captures are possible.
        _selectiveHorizon = FastMath.Max(depth, _selectiveHorizon);
        bool drawnEndgame;
        int phase;
        GetNextMove getNextMove;
        ulong moveGenerationToSquareMask;

        if (board.CurrentPosition.KingInCheck)
        {
            // King is in check.  Search all moves.
            checksInQuietSearch++;
            getNextMove = _getNextMove;
            moveGenerationToSquareMask = Board.AllSquaresMask;
            board.CurrentPosition.StaticScore = -StaticScore.Max; // Do not evaluate static score when king is in check.
            drawnEndgame = false;
            phase = Evaluation.DetermineGamePhase(board.CurrentPosition);
        }
        else
        {
            // King is not in check.  Search only captures.
            getNextMove = _getNextCapture;
            var fromHorizonExcludingChecks = depth - horizon - checksInQuietSearch;
            if ((fromHorizonExcludingChecks > _recapturesOnlyMaxFromHorizon) && !Move.IsKingMove(board.PreviousPosition.PlayedMove))
            {
                // Past max distance from horizon and last move was not by king.
                var lastMoveToSquare = Move.To(board.PreviousPosition.PlayedMove);
                moveGenerationToSquareMask = lastMoveToSquare == Square.Illegal
                    ? toSquareMask
                    : Board.SquareMasks[(int)lastMoveToSquare]; // Search only recaptures.
            }
            else moveGenerationToSquareMask = toSquareMask;
            (board.CurrentPosition.StaticScore, drawnEndgame, phase) = _evaluation.GetStaticScore(board.CurrentPosition);
            if (board.CurrentPosition.StaticScore >= beta) return beta; // Prevent worsening of position by making a bad capture.  Stand pat.
            alpha = FastMath.Max(board.CurrentPosition.StaticScore, alpha);
        }

        // Even if endgame is drawn, search moves for a swindle (enemy mistake that makes drawn game winnable).
        var legalMoveNumber = 0;
        var previousMove = board.PreviousPosition?.PlayedMove ?? Move.Null;
        board.CurrentPosition.PrepareMoveGeneration();

        do
        {
            // Do not retrieve (or update) best move from the cache.  Rely on MVV / LVA move order.
            var (move, moveIndex) = getNextMove(previousMove, board.CurrentPosition, moveGenerationToSquareMask, depth, Move.Null);
            if (move == Move.Null) break; // All moves have been searched.

            // Must call IsMoveInQuietSearchFutile before board.PlayMove to avoid bugs related to incorrect KingInCheck and ColorToMove.
            var futileMove = IsMoveInQuietSearchFutile(board.CurrentPosition, move, drawnEndgame, phase, alpha);

            // Play and search move.
            var (legalMove, checkingMove) = board.PlayMove(move);
            if (!legalMove)
            {
                // Skip illegal move.
                board.UndoMove();
                continue;
            }
            legalMoveNumber++;

            if (futileMove && !checkingMove)
            {
                // Skip futile move that doesn't check enemy king.
                board.UndoMove();
                continue;
            }

            Move.SetPlayed(ref move, true);
            // ReSharper disable once PossibleNullReferenceException
            board.PreviousPosition.Moves[moveIndex] = move;

            var score = -GetQuietScore(board, depth + 1, horizon, toSquareMask, checksInQuietSearch, -beta, -alpha);
            board.UndoMove();

            if (FastMath.Abs(score) == StaticScore.Interrupted) return score; // Stop searching.
            if (score >= beta) return beta; // Position is not the result of best play by both players.

            alpha = FastMath.Max(score, alpha);

        } while (true);

        if ((legalMoveNumber == 0) && board.CurrentPosition.KingInCheck) return Evaluation.GetMatedScore(depth); // Game ends on this move.

        // Return score of best move.
        return alpha;
    }


    private void ExamineTimeAndNodes(long nodes)
    {
        if (nodes >= _timeManagement.NodeLimit)
        {
            // Have passed node limit.
            Continue = false;
            return;
        }

        if (_nodesPerSecond.HasValue && (_originalHorizon > 1)) // Guarantee to search at least one ply.
        {
            double nps;
            do
            {
                // Slow search until it's less than phased nodes per second (NPS at game phase of root search position) or until soft time limit is exceeded.
                if (_stopwatch.Elapsed >= _timeManagement.MoveTimeSoftLimit)
                {
                    // No time is available to continue searching.
                    Continue = false;
                    return;
                }
                nps = nodes / _stopwatch.Elapsed.TotalSeconds;
            } while (nps > _phasedNodesPerSecond);
        }

        // Search at full speed until hard time limit is exceeded.
        Continue = _stopwatch.Elapsed < _timeManagement.MoveTimeHardLimit;
    }


    private bool IsPositionFutile(Position position, int depth, int horizon, bool isDrawnEndgame, int alpha, int beta)
    {
        var toHorizon = horizon - depth;
        if ((depth == 0) || (toHorizon >= _futilityPruningMargins.Length)) return false; // Root position or position far from search horizon is not futile.
        if (isDrawnEndgame || position.KingInCheck) return false; // Position in drawn endgame or when king is in check is not futile.
        if ((FastMath.Abs(alpha) >= StaticScore.Checkmate) || (FastMath.Abs(beta) >= StaticScore.Checkmate)) return false; // Position under threat of checkmate is not futile.

        // Position with lone king on board is not futile.
        if (Bitwise.CountSetBits(position.ColorOccupancy[(int)Color.White]) == 1) return false;
        if (Bitwise.CountSetBits(position.ColorOccupancy[(int)Color.Black]) == 1) return false;

        // Determine if any move can lower score under beta.
        return position.StaticScore - _futilityPruningMargins[toHorizon] >= beta;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNullMovePermitted(Position position, int beta)
    {
        if ((position.StaticScore < beta) || position.KingInCheck) return false; // Do not attempt null move if static score is weak, nor if king is in check.
        // Do not attempt null move in pawn endgames.  Side to move may be in zugzwang.
        var minorAndMajorPieces = Bitwise.CountSetBits(position.GetMajorAndMinorPieces(position.ColorToMove));
        return minorAndMajorPieces > 0;
    }


    private bool DoesNullMoveCauseBetaCutoff(Board board, int depth, int horizon, int beta)
    {
        var reduction = _nullMoveReduction + Math.Min((board.CurrentPosition.StaticScore - beta) / _nullStaticScoreReduction, _nullStaticScoreMaxReduction);

        board.PlayNullMove();
        var score = -GetDynamicScore(board, depth + 1, horizon - reduction, false, -beta, -beta + 1);  // Do not play two null moves consecutively.
        board.UndoMove();

        return score >= beta;
    }


    public (ulong Move, int MoveIndex) GetNextMove(ulong previousMove, Position position, ulong toSquareMask, int depth, ulong bestMove)
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
                    // Prioritize and sort captures.
                    lastMoveIndex = FastMath.Max(firstMoveIndex, position.MoveIndex - 1);
                    if (firstMoveIndex < lastMoveIndex)
                    {
                        PrioritizeMoves(previousMove, position.Moves, firstMoveIndex, lastMoveIndex, bestMove, depth);
                        SortMovesByPriority(position.Moves, firstMoveIndex, lastMoveIndex);
                    }
                    position.MoveGenerationStage++;
                    continue;

                case MoveGenerationStage.NonCaptures:
                    firstMoveIndex = position.MoveIndex;
                    position.GenerateMoves(MoveGeneration.OnlyNonCaptures, Board.AllSquaresMask, toSquareMask);
                    // Prioritize and sort non-captures.
                    lastMoveIndex = FastMath.Max(firstMoveIndex, position.MoveIndex - 1);
                    if (firstMoveIndex < lastMoveIndex)
                    {
                        PrioritizeMoves(previousMove, position.Moves, firstMoveIndex, lastMoveIndex, bestMove, depth);
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
    
    
    private (ulong Move, int MoveIndex) GetNextCapture(ulong previousMove, Position position, ulong toSquareMask, int depth, ulong bestMove)
    {
        while (true)
        {
            if (position.CurrentMoveIndex < position.MoveIndex)
            {
                var moveIndex = position.CurrentMoveIndex;
                var move = position.Moves[moveIndex];
                position.CurrentMoveIndex++;
                Debug.Assert(Move.CaptureVictim(move) != Piece.None);
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
                    // Prioritize and sort captures.
                    var lastMoveIndex = FastMath.Max(firstMoveIndex, position.MoveIndex - 1);
                    if (firstMoveIndex < lastMoveIndex)
                    {
                        PrioritizeMoves(previousMove, position.Moves, firstMoveIndex, lastMoveIndex, bestMove, depth);
                        SortMovesByPriority(position.Moves, firstMoveIndex, lastMoveIndex);
                    }
                    position.MoveGenerationStage = MoveGenerationStage.End; // Skip non-captures.
                    continue;
                case MoveGenerationStage.End:
                    return (Move.Null, position.CurrentMoveIndex);
            }
            break;
        }

        return (Move.Null, position.CurrentMoveIndex);
    }


    private bool IsMoveInDynamicSearchFutile(Position position, int depth, int horizon, ulong move, int legalMoveNumber, int quietMoveNumber, bool drawnEndgame, int phase, int alpha, int beta)
    {
        Debug.Assert(_futilityPruningMargins.Length == _lateMovePruningMargins.Length);
        var toHorizon = horizon - depth;

        if (legalMoveNumber == 1) return false; // First legal move is not futile.
        if (!Move.IsQuiet(move)) return false; // Tactical move is not futile.
        if ((depth == 0) || (toHorizon >= _futilityPruningMargins.Length)) return false; // Root move or move far from search horizon is not futile.
        if (drawnEndgame || position.KingInCheck) return false; // Move in drawn endgame or move when king is in check is not futile.
        if ((Move.Killer(move) > 0) || (Move.PromotedPiece(move) != Piece.None) || Move.IsCastling(move)) return false; // Killer move or castling is not futile.
        if ((FastMath.Abs(alpha) >= StaticScore.Checkmate) || (FastMath.Abs(beta) >= StaticScore.Checkmate)) return false; // Move under threat of checkmate is not futile.

        if (Move.IsPawnMove(move))
        {
            var rank = Board.Ranks[(int)position.ColorToMove][(int)Move.To(move)];
            if (rank >= 6) return false; // Pawn push to 7th rank is not futile.
        }

        // Move with lone king on board is not futile.
        if (Bitwise.CountSetBits(position.ColorOccupancy[(int)Color.White]) == 1) return false;
        if (Bitwise.CountSetBits(position.ColorOccupancy[(int)Color.Black]) == 1) return false;

        // Determine if quiet move is too late to be worth searching.
        if (quietMoveNumber >= _lateMovePruningMargins[toHorizon]) return true;

        // No material improvement is possible because captures and pawn promotions are not futile.
        // Determine if location improvement raises score to within futility margin of alpha.
        var locationImprovement = _evaluation.GetPieceLocationImprovement(move, phase);
        return (position.StaticScore + locationImprovement + _futilityPruningMargins[toHorizon]) < alpha;
    }


    private bool IsMoveInQuietSearchFutile(Position position, ulong move, bool drawnEndgame, int phase, int alpha)
    {
        if (drawnEndgame || position.KingInCheck) return false; // Move in drawn endgame or move when king is in check is not futile.

        // Determine if material and location improvements raise score to within futility margin of alpha.
        var captureVictim = Move.CaptureVictim(move);
        var materialImprovement = _evaluation.GetPieceMaterialScore(PieceHelper.GetColorlessPiece(captureVictim), phase);
        var locationImprovement = _evaluation.GetPieceLocationImprovement(move, phase);
        return (position.StaticScore + materialImprovement + locationImprovement + _futilityPruningMargins[0]) < alpha;
    }


    private int GetSearchHorizon(Board board, int depth, int horizon, ulong move, CachedPosition cachedPosition, int legalMoveNumber, int quietMoveNumber, bool drawnEndgame)
    {
        if (Move.IsBest(move) && IsBestMoveSingular(board, depth, horizon, move, cachedPosition))
        {
            // The best move from the cache is singular.  That is, it's the only good move in the position.
            // Evaluation of the current position relies on the accuracy of the singular move's score.
            // If the engine misjudges the singular move, the position could deteriorate because no alternative strong moves exist.
            // To increase confidence in the singular move's score, search it one ply deeper.
            return horizon + 1;
        }

        if (legalMoveNumber == 1) return horizon; // Do not reduce first legal move.
        if (!Move.IsQuiet(move)) return horizon; // Do not reduce tactical move.
        if (depth == 0)
        {
            if (MultiPv > 1) return horizon; // Do not reduce Multi-PV root move.
            if (_limitedStrength) return horizon; // Do not reduce limited strength root move.
        }
        if (drawnEndgame || board.CurrentPosition.KingInCheck) return horizon; // Do not reduce move in drawn endgame or move when king is in check.
        if ((Move.Killer(move) > 0) || Move.IsCastling(move)) return horizon; // Do not reduce killer move or castling.

        if (Move.IsPawnMove(move))
        {
            var rank = Board.Ranks[(int)board.CurrentPosition.ColorToMove][(int)Move.To(move)];
            if (rank >= 6) return horizon; // Do not reduce pawn push to 7th rank.
        }

        // Reduce search horizon of late move.
        var quietMoveIndex = FastMath.Min(quietMoveNumber, _lmrMaxIndex);
        var toHorizonIndex = FastMath.Min(horizon - depth, _lmrMaxIndex);
        var reduction = _lateMoveReductions[quietMoveIndex][toHorizonIndex];
        
        var previous2StaticScore = board.GetPreviousPosition(2)?.StaticScore ?? -StaticScore.Max;
        if (board.CurrentPosition.StaticScore < previous2StaticScore)
        {
            var previous4StaticScore = board.GetPreviousPosition(4)?.StaticScore ?? -StaticScore.Max;
            if (previous2StaticScore < previous4StaticScore)
            {
                // Reduce more when static evaluation score has worsened in each of previous two moves by same color.
                reduction++;
            }
        }

        return horizon - reduction;
    }

    
    // Singular move idea from Stockfish chess engine.
    private bool IsBestMoveSingular(Board board, int depth, int horizon, ulong bestMove, CachedPosition cachedPosition)
    {
        // Determine if best move that had failed high in recent searches is best by a significant margin.
        var toHorizon = horizon - depth;
        if ((depth == 0) || (toHorizon < _singularMoveMinToHorizon)) return false;

        var dynamicScore = CachedPositionData.DynamicScore(cachedPosition.Data);
        if ((dynamicScore == StaticScore.NotCached) || (FastMath.Abs(dynamicScore) >= StaticScore.Checkmate)) return false;

        if (CachedPositionData.ScorePrecision(cachedPosition.Data) != ScorePrecision.LowerBound) return false;
        if (CachedPositionData.ToHorizon(cachedPosition.Data) < (toHorizon - _singularMoveMaxInsufficientDraft)) return false;

        var beta = dynamicScore - (_singularMoveMargin * toHorizon);
        var searchHorizon = depth + ((toHorizon * _singularMoveReductionPer128) / 128);
        dynamicScore = GetDynamicScore(board, depth, searchHorizon, false, beta - 1, beta, bestMove); // Exclude best move from search.
        return dynamicScore < beta;
    }


    private int GetCachedDynamicScore(ulong cachedPositionData, int depth, int horizon, int alpha, int beta)
    {
        var dynamicScore = CachedPositionData.DynamicScore(cachedPositionData);
        if (dynamicScore == StaticScore.NotCached) return StaticScore.NotCached; // Score is not cached.

        var toHorizon = horizon - depth;
        var cachedToHorizon = CachedPositionData.ToHorizon(cachedPositionData);
        if (cachedToHorizon < toHorizon) return StaticScore.NotCached; // Cached position is shallower than current horizon. Do not use cached score.

        // Adjust checkmate score.
        if (dynamicScore >= StaticScore.Checkmate) dynamicScore -= depth;
        else if (dynamicScore <= -StaticScore.Checkmate) dynamicScore += depth;

        var scorePrecision = CachedPositionData.ScorePrecision(cachedPositionData);
        // ReSharper disable once SwitchStatementMissingSomeCases
        switch (scorePrecision)
        {
            case ScorePrecision.Exact:
                if (dynamicScore <= alpha) return alpha; // Score fails low.
                if (dynamicScore >= beta) return beta; // Score fails high.
                return AnalyzeMode
                    ? StaticScore.NotCached // Continue searching when analyzing so principal variation isn't truncated.
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

        return StaticScore.NotCached;
    }

    
    private void PrioritizeMoves(ulong previousMove, ScoredMove[] moves, int lastMoveIndex, ulong bestMove, int depth)
    {
        for (var moveIndex = 0; moveIndex <= lastMoveIndex; moveIndex++)
        {
            // Prioritize by best move, killer moves, then move history.
            ref var move = ref moves[moveIndex].Move;
            Move.SetIsBest(ref move, Move.Equals(move, bestMove));
            Move.SetKiller(ref move, _killerMoves.GetValue(depth, move));
            Move.SetHistory(ref move, _moveHistory.GetValue(previousMove, move));
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PrioritizeMoves(ulong previousMove, ulong[] moves, int lastMoveIndex, ulong bestMove, int depth) => PrioritizeMoves(previousMove, moves, 0, lastMoveIndex, bestMove, depth);

    
    private void PrioritizeMoves(ulong previousMove, ulong[] moves, int firstMoveIndex, int lastMoveIndex, ulong bestMove, int depth)
    {
        for (var moveIndex = firstMoveIndex; moveIndex <= lastMoveIndex; moveIndex++)
        {
            // Prioritize by best move, killer moves, then move history.
            ref var move = ref moves[moveIndex];
            Move.SetIsBest(ref move, Move.Equals(move, bestMove));
            Move.SetKiller(ref move, _killerMoves.GetValue(depth, move));
            Move.SetHistory(ref move, _moveHistory.GetValue(previousMove, move));
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SortMovesByPriority(ScoredMove[] moves, int lastMoveIndex) => Array.Sort(moves, 0, lastMoveIndex + 1, _scoredMovePriorityComparer);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SortMovesByPriority(ulong[] moves, int lastMoveIndex) => Array.Sort(moves, 0, lastMoveIndex + 1, _movePriorityComparer);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SortMovesByPriority(ulong[] moves, int firstMoveIndex, int lastMoveIndex) => Array.Sort(moves, firstMoveIndex, lastMoveIndex - firstMoveIndex + 1, _movePriorityComparer);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SortMovesByScore(ScoredMove[] moves, int lastMoveIndex) => Array.Sort(moves, 0, lastMoveIndex + 1, _scoredMoveComparer);


    private void UpdateCache(Position currentPosition, int depth, int horizon, ulong bestMove, int dynamicScore, int alpha, int beta)
    {
        if (FastMath.Abs(dynamicScore) == StaticScore.Interrupted) return;

        var cachedPosition = _cache.NullPosition;
        cachedPosition.Key = currentPosition.Key;
        CachedPositionData.SetLastAccessed(ref cachedPosition.Data, Count);
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
        if (adjustedDynamicScore >= StaticScore.Checkmate) adjustedDynamicScore += depth;
        else if (adjustedDynamicScore <= -StaticScore.Checkmate) adjustedDynamicScore -= depth;

        // Set score value and precision.
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

        // Update cache.
        _cache.SetPosition(cachedPosition);
    }

    
    private void UpdateStatus(Board board, bool includePrincipalVariations)
    {
        // Calculate search speed and hash population.
        var milliseconds = _stopwatch.Elapsed.TotalMilliseconds;
        var nodesPerSecond = board.Nodes / _stopwatch.Elapsed.TotalSeconds;
        var nodes = includePrincipalVariations ? board.Nodes : NodesInfoUpdate;
        var hashFull = (int)((1000L * _cache.Positions) / _cache.Capacity);

        if (includePrincipalVariations)
        {
            // Include principal variations.
            var legalPv = FastMath.Min(MultiPv, board.CurrentPosition.MoveIndex); // Less legal moves may exist than principal variations requested.
            for (var pv = 0; pv < legalPv; pv++)
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
                var scorePhrase = FastMath.Abs(score) >= StaticScore.Checkmate ? $"mate {Evaluation.GetMateMoveCount(score)}" : $"cp {score}";
                _messenger.WriteLine(
                    $"info {multiPvPhrase}depth {_originalHorizon} seldepth {FastMath.Max(_selectiveHorizon, _originalHorizon)} time {milliseconds:0} nodes {nodes} score {scorePhrase} nps {nodesPerSecond:0} {pvLongAlgebraic}");
            }
        }
        else
        {
            // Write message without principal variation(s).
            _messenger.WriteLine($"info depth {_originalHorizon} seldepth {FastMath.Max(_selectiveHorizon, _originalHorizon)} time {milliseconds:0} nodes {nodes} nps {nodesPerSecond:0}");
        }

        // Write message regarding hash and current move.
        _messenger.WriteLine($"info hashfull {hashFull:0} currmove {Move.ToLongAlgebraic(_rootMove)} currmovenumber {_rootMoveNumber}");
        if (_messenger.Debug) _messenger.WriteLine(_stats.ToString());

        // Calculate node count for next status update.
        var intervals = (int)(board.Nodes / UciStream.NodesInfoInterval);
        NodesInfoUpdate = UciStream.NodesInfoInterval * (intervals + 1);
    }


    public void Reset()
    {
        // Restart stopwatch and clear candidate moves.
        _stopwatch.Restart();
        CandidateMoves.Clear();

        // Reset best moves.
        for (var moveIndex = 0; moveIndex < _bestMoves.Length; moveIndex++)
            _bestMoves[moveIndex] = new ScoredMove(Move.Null, -StaticScore.Max);
        for (var depth = 0; depth < _bestMovePlies.Length; depth++)
            _bestMovePlies[depth] = new ScoredMove(Move.Null, -StaticScore.Max);

        // Prepare for next search.
        PvInfoUpdate = true;
        Count++;
        Continue = true;
    }


    public string ShowLimitStrengthParameters()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("Rating (Elo)  Search Speed (NPS)  Move Error  Blunder Error  Blunder Percent");
        stringBuilder.Append('=', 76);
        stringBuilder.AppendLine();

        for (var index = 0; index < _limitStrengthElos.Length; index++)
        {
            var elo = _limitStrengthElos[index];

            var (nodesPerSecond, moveError, blunderError, blunderPer1024) = CalculateLimitStrengthParams(elo);
            var blunderPercent = 100d * blunderPer1024 / 1024d;

            stringBuilder.AppendLine($"{elo,12}{nodesPerSecond.ToString("n0").PadLeft(20)}{moveError,12}{blunderError,15}{blunderPercent.ToString("0.0").PadLeft(17)}");
        }

        stringBuilder.AppendLine();
        stringBuilder.Append($"NpsEndgamePer128: {_limitStrengthConfig.NpsEndgamePer128}");
        return stringBuilder.ToString();
    }
}
