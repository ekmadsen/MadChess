// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Engine.Heuristics
{
    public sealed class MoveHistory
    {
        private const int _multiplier = 1024;
        private const int _divisor = Move.HistoryMaxValue / _multiplier;
        private readonly int[][] _moveHistory;


        public MoveHistory()
        {
            _moveHistory = new int[(int)Piece.BlackKing + 1][];
            for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
            {
                _moveHistory[(int)piece] = new int[64];
                for (var toSquare = Square.A8; toSquare < Square.Illegal; toSquare++) _moveHistory[(int)piece][(int)toSquare] = 0;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(Position position, ulong move)
        {
            var piece = position.GetPiece(Move.From(move));
            var toSquare = Move.To(move);
            return _moveHistory[(int)piece][(int)toSquare];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateValue(Position position, ulong move, int increment)
        {
            // Update value with decay.  Idea from Ethereal chess engine.
            // This function approaches an asymptotic limit of +/- MaxValue.
            var piece = position.GetPiece(Move.From(move));
            var toSquare = Move.To(move);
            var value = _moveHistory[(int)piece][(int)toSquare];
            value += (increment * _multiplier) - ((value * FastMath.Abs(increment)) / _divisor);
            _moveHistory[(int)piece][(int)toSquare] = value;
        }


        public void Reset()
        {
            for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
            {
                for (var toSquare = Square.A8; toSquare < Square.Illegal; toSquare++) _moveHistory[(int)piece][(int)toSquare] = 0;
            }
        }
    }
}
