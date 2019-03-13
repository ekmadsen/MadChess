// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine
{
    public static class Delegates
    {
        public delegate ulong CreateMoveDestinationsMask(int Square, ulong Occupancy, Direction[] Directions);
        public delegate bool ValidateMove(ref ulong Move);
        public delegate (ulong Move, int MoveIndex) GetNextMove(ref Position Position, ulong ToSquareMask, int Depth, ulong BestMove);
        public delegate int GetPositionCount();
        public delegate bool IsPassedPawn(int Square, bool White);
        public delegate bool IsFreePawn(int Square, bool White);
        public delegate void WriteMessageLine(string Message);
    }
}
