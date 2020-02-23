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
        // Allocate delegates at program startup so they aren't allocated repeatedly via lambda syntax inside search loop.
        public delegate bool ValidateMove(ref ulong Move);
        public delegate bool Debug();
        public delegate (ulong Move, int MoveIndex) GetNextMove(Position Position, int Depth, int Horizon, ulong BestMove);
        public delegate int GetPositionCount();
        public delegate void WriteMessageLine(string Message);
        public delegate ulong GetPieceDestinations(Position Position, int FromSquare, bool White);
        public delegate void AddPiece(int Piece, int Square);
        public delegate int RemovePiece(int Square);
    }
}
