// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class Stats
    {
        public long NullMoves;
        public long NullMoveCutoffs;
        public long BetaCutoffs;
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
            BetaCutoffs = 0;
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
    }
}
