// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Game;


namespace ErikTheCoder.MadChess.Engine
{
    public static class Delegates
    {
        public delegate (ulong Move, int MoveIndex) GetNextMove(Position position, ulong toSquareMask, int depth, ulong bestMove);
        public delegate bool IsRepeatPosition(int repetitions);
        public delegate void DisplayStats();
        public delegate (int StaticScore, bool DrawnEndgame) GetStaticScore(Position position);
    }
}
