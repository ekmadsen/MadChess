// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine.Heuristics;


public sealed class Stats
{
    public long NullMoves;
    public long NullMoveCutoffs;

    public long MovesCausingBetaCutoff;
    public long BetaCutoffMoveNumber;
    public long BetaCutoffFirstMove;

    public long Evaluations;

    public long CacheProbes;
    public long CacheHits;
    public long CacheScoreCutoff;
    public long CacheBestMoveProbes;
    public long CacheValidBestMove;
    public long CacheInvalidBestMove;


    public void Reset()
    {
        NullMoves = 0;
        NullMoveCutoffs = 0;

        MovesCausingBetaCutoff = 0;
        BetaCutoffMoveNumber = 0;
        BetaCutoffFirstMove = 0;

        Evaluations = 0;

        CacheProbes = 0;
        CacheHits = 0;
        CacheScoreCutoff = 0;
        CacheBestMoveProbes = 0;
        CacheValidBestMove = 0;
        CacheInvalidBestMove = 0;
    }

    public override string ToString()
    {
        var nullMoveCutoffFraction = (100d * NullMoveCutoffs) / NullMoves;
        var betaCutoffMoveNumber = (double)BetaCutoffMoveNumber / MovesCausingBetaCutoff;
        var betaCutoffFirstMoveFraction = (100d * BetaCutoffFirstMove) / MovesCausingBetaCutoff;
        var cacheHitFraction = (100d * CacheHits) / CacheProbes;
        var scoreCutoffFraction = (100d * CacheScoreCutoff) / CacheHits;
        var bestMoveHitFraction = (100d * CacheValidBestMove) / CacheBestMoveProbes;

        return $"""
            info string Cache Hit = {cacheHitFraction:0.00}% Score Cutoff = {scoreCutoffFraction:0.00}% Best Move Hit = {bestMoveHitFraction:0.00}% Invalid Best Moves = {CacheInvalidBestMove:n0}
            info string Null Move Cutoffs = {nullMoveCutoffFraction:0.00}% Beta Cutoff Move Number = {betaCutoffMoveNumber:0.00} Beta Cutoff First Move = {betaCutoffFirstMoveFraction:0.00}%
            info string Evals = {Evaluations:n0}
            """;
    }
}