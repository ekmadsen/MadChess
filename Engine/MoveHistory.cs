// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class MoveHistory
    {
        private const int _agePer256 = 244;  // This improves integer division speed since x / 256 = x >> 8.
        private readonly int[][] _moveHistory;


        public MoveHistory()
        {
            _moveHistory = new int[Piece.BlackKing + 1][];
            for (int piece = Piece.None; piece <= Piece.BlackKing; piece++)
            {
                _moveHistory[piece] = new int[64];
                for (int toSquare = 0; toSquare < 64; toSquare++) _moveHistory[piece][toSquare] = 0;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(ref Position Position, ulong Move)
        {
            int piece = Position.GetPiece(Engine.Move.From(Move));
            int toSquare = Engine.Move.To(Move);
            return _moveHistory[piece][toSquare];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateValue(ref Position Position, ulong Move, int Increment)
        {
            // Don't bother checking for overflow values as it's not likely to happen with 27 bits.
            int piece = Position.GetPiece(Engine.Move.From(Move));
            int toSquare = Engine.Move.To(Move);
            _moveHistory[piece][toSquare] += Increment;
        }


        public void Age(bool WhiteMove)
        {
            int minPiece;
            int maxPiece;
            if (WhiteMove)
            {
                // White move
                minPiece = Piece.WhitePawn;
                maxPiece = Piece.WhiteKing;
            }
            else
            {
                // Black move
                minPiece = Piece.BlackPawn;
                maxPiece = Piece.BlackKing;
            }
            for (int piece = minPiece; piece <= maxPiece; piece++)
            {
                for (int toSquare = 0; toSquare < 64; toSquare++) _moveHistory[piece][toSquare] = (_agePer256 * _moveHistory[piece][toSquare]) / 256;
            }
        }


        public void Reset()
        {
            for (int piece = Piece.None; piece <= Piece.BlackKing; piece++)
            {
                for (int toSquare = 0; toSquare < 64; toSquare++) _moveHistory[piece][toSquare] = 0;
            }
        }
    }
}
