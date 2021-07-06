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
    public static class Delegates
    {
        // Allocate delegates at program startup so they aren't allocated repeatedly via lambda syntax inside search loop.
        public delegate bool ValidateMove(ref ulong move);
        public delegate bool Debug();
        public delegate (ulong Move, int MoveIndex) GetNextMove(Position position, ulong toSquareMask, int depth, ulong bestMove);
        public delegate bool IsRepeatPosition(int repetitions);
        public delegate void DisplayStats();
        public delegate void WriteMessageLine(string message);
        public delegate (int StaticScore, bool DrawnEndgame) GetStaticScore(Position position);
    }
}
