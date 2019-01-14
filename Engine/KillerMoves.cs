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
    public sealed class KillerMoves
    {
        private readonly KillerMove[][] _killerMoves;


        public KillerMoves(int MaxDepth)
        {
            _killerMoves = new KillerMove[MaxDepth + 1][];
            for (int depth = 0; depth <= MaxDepth; depth++) _killerMoves[depth] = new[] {new KillerMove(Piece.None, Square.Illegal), new KillerMove(Piece.None, Square.Illegal)};
            Reset();
        }


        public int GetValue(Position Position, int Depth, ulong Move)
        {
            if (Equals(Position, _killerMoves[Depth][0], Move)) return 2;
            return Equals(Position, _killerMoves[Depth][1], Move) ? 1 : 0;
        }


        public void UpdateValue(Position Position, int Depth, ulong Move)
        {
            if (Equals(Position, _killerMoves[Depth][0], Move)) return; // Move already is the best killer move.
            // Shift killer move.
            _killerMoves[Depth][1].Piece = _killerMoves[Depth][0].Piece;
            _killerMoves[Depth][1].ToSquare = _killerMoves[Depth][0].ToSquare;
            // Update killer move.
            _killerMoves[Depth][0].Piece = Position.GetPiece(Engine.Move.From(Move));
            _killerMoves[Depth][0].ToSquare = Engine.Move.To(Move);
        }


        public void Shift(int Depth)
        {
            // Shift killer moves closer to root position.
            int lastDepth = _killerMoves.Length - Depth - 1;
            for (int depth = 0; depth <= lastDepth; depth++)
            {
                _killerMoves[depth][0].Piece = _killerMoves[depth + Depth][0].Piece;
                _killerMoves[depth][0].ToSquare = _killerMoves[depth + Depth][0].ToSquare;
                _killerMoves[depth][1].Piece = _killerMoves[depth + Depth][1].Piece;
                _killerMoves[depth][1].ToSquare = _killerMoves[depth + Depth][1].ToSquare;
            }
            // Reset killer moves far from root position.
            for (int depth = lastDepth + 1; depth < _killerMoves.Length; depth++)
            {
                _killerMoves[depth][0].Piece = Piece.None;
                _killerMoves[depth][0].ToSquare = Square.Illegal;
                _killerMoves[depth][1].Piece = Piece.None;
                _killerMoves[depth][1].ToSquare = Square.Illegal;
            }
        }


        public void Reset()
        {
            for (int index = 0; index < _killerMoves.Length; index++)
            {
                KillerMove[] killerMoves = _killerMoves[index];
                killerMoves[0].Piece = Piece.None;
                killerMoves[0].ToSquare = Square.Illegal;
                killerMoves[1].Piece = Piece.None;
                killerMoves[1].ToSquare = Square.Illegal;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Equals(Position Position, KillerMove KillerMove, ulong Move) => (KillerMove.Piece == Position.GetPiece(Engine.Move.From(Move))) && (KillerMove.ToSquare == Engine.Move.To(Move));
    }
}
