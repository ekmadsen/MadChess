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
        // History has 48 - 22 + 1 = 27 bits.
        // Eliminate one bit to prevent overflow caused by zero (adds one distinct value to range).
        // 2 Pow 26 = 67_108_864.
        // Value may be positive or negative, so max value is 67_108_864 / 2.
        public const int MaxValue = 33_554_432;
        private const int _multiplier = 1024;
        private const int _divisor = MaxValue / _multiplier;
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
            // Update value with decay.  Idea from Ethereal chess engine.
            // This function approaches an asymptotic limit of +/- MaxValue.
            var piece = Position.GetPiece(Engine.Move.From(Move));
            var toSquare = Engine.Move.To(Move);
            var value = _moveHistory[piece][toSquare];
            value += (Increment * _multiplier) - ((value * Math.Abs(Increment)) / _divisor);
            _moveHistory[piece][toSquare] = value;
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
