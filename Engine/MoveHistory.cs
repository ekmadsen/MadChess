// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class MoveHistory
    {
        public const int MaxValue = 67_108_863; // History has 48 - 22 + 1 = 27 bits.  2 Pow 27 = 134_217_728.  Value may be positive or negative.
        private const int _agePer256 = 244;
        private readonly int[][] _moveHistory;


        public MoveHistory()
        {
            _moveHistory = new int[Piece.BlackKing + 1][];
            for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
            {
                _moveHistory[piece] = new int[64];
                for (var toSquare = 0; toSquare < 64; toSquare++) _moveHistory[piece][toSquare] = 0;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(Position Position, ulong Move)
        {
            var piece = Position.GetPiece(Engine.Move.From(Move));
            var toSquare = Engine.Move.To(Move);
            return _moveHistory[piece][toSquare];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateValue(Position Position, ulong Move, int Increment)
        {
            var piece = Position.GetPiece(Engine.Move.From(Move));
            var toSquare = Engine.Move.To(Move);
            var value = _moveHistory[piece][toSquare] + Increment;
            _moveHistory[piece][toSquare] = Math.Max(Math.Min(value, MaxValue), -MaxValue);
        }


        public void Age()
        {
            for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
            {
                for (var toSquare = 0; toSquare < 64; toSquare++) _moveHistory[piece][toSquare] = (_agePer256 * _moveHistory[piece][toSquare]) / 256;
            }
        }


        public void Reset()
        {
            for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
            {
                for (var toSquare = 0; toSquare < 64; toSquare++) _moveHistory[piece][toSquare] = 0;
            }
        }
    }
}
