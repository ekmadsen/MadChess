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
        // History has 48 - 21 + 1 = 28 bits.
        // Eliminate one bit to prevent overflow caused by zero (adds one distinct value to range).
        // 2 Pow 27 = 134_217_728.
        // Value may be positive or negative, so max value is 134_217_728 / 2.
        public const int MaxValue = 67_108_864;
        private const int _multiplier = 1024;
        private const int _divisor = MaxValue / _multiplier;
        private readonly int[][] _moveHistory;


        public MoveHistory()
        {
            _moveHistory = new int[(int)Piece.BlackKing + 1][];
            for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
            {
                _moveHistory[(int)piece] = new int[64];
                for (var toSquare = 0; toSquare < 64; toSquare++) _moveHistory[(int)piece][toSquare] = 0;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(Position position, ulong move)
        {
            var piece = position.GetPiece(Move.From(move));
            var toSquare = Move.To(move);
            return _moveHistory[(int)piece][toSquare];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateValue(Position position, ulong move, int increment)
        {
            // Update value with decay.  Idea from Ethereal chess engine.
            // This function approaches an asymptotic limit of +/- MaxValue.
            var piece = position.GetPiece(Move.From(move));
            var toSquare = Move.To(move);
            var value = _moveHistory[(int)piece][toSquare];
            value += (increment * _multiplier) - ((value * Math.Abs(increment)) / _divisor);
            _moveHistory[(int)piece][toSquare] = value;
        }


        public void Reset()
        {
            for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
            {
                for (var toSquare = 0; toSquare < 64; toSquare++) _moveHistory[(int)piece][toSquare] = 0;
            }
        }
    }
}
