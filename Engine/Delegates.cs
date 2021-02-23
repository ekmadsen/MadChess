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
        public delegate bool ValidateMove(ref ulong Move);
        public delegate bool Debug();
        public delegate (ulong Move, int MoveIndex) GetNextMove(Position Position, ulong ToSquareMask, int Depth, ulong BestMove);
        public delegate bool IsRepeatPosition(int Repeats);
        public delegate void DisplayStats();
        public delegate void WriteMessageLine(string Message);
        public delegate int GetStaticScore(Position Position);
    }
}
