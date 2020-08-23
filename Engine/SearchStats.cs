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
    public sealed class SearchStats
    {
        public long NullMoves;
        public long NullMoveCutoffs;
        public long BetaCutoffs;
        public long BetaCutoffMoveNumber;
        public long BetaCutoffFirstMove;


        public void Reset()
        {
            NullMoves = 0;
            NullMoveCutoffs = 0;
            BetaCutoffs = 0;
            BetaCutoffMoveNumber = 0;
            BetaCutoffFirstMove = 0;
        }
    }
}
