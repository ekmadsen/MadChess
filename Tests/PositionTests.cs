// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Engine;
using NUnit.Framework;


namespace ErikTheCoder.MadChess.Tests
{
    [TestFixture]
    public sealed class PositionTests
    {
        [Test]
        public void TestStartPosition()
        {
            UciStream uciStream = new UciStream();
            Board board = uciStream.Board;
            board.SetPosition(Board.StartPositionFen);
            uciStream.WriteMessageLine(board.ToString());
            // Validate integrity of board and occupancy of every square.
            board.AssertIntegrity();
            Assert.That(board.CurrentPosition.GetPiece(Square.a8), Is.EqualTo(Piece.BlackRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.b8), Is.EqualTo(Piece.BlackKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.c8), Is.EqualTo(Piece.BlackBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.d8), Is.EqualTo(Piece.BlackQueen));
            Assert.That(board.CurrentPosition.GetPiece(Square.e8), Is.EqualTo(Piece.BlackKing));
            Assert.That(board.CurrentPosition.GetPiece(Square.f8), Is.EqualTo(Piece.BlackBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.g8), Is.EqualTo(Piece.BlackKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.h8), Is.EqualTo(Piece.BlackRook));
            int square = Square.a7;
            do
            {
                Assert.That(board.CurrentPosition.GetPiece(square), Is.EqualTo(Piece.BlackPawn));
                square++;
            } while (square <= Square.h7);
            do
            {
                Assert.That(board.CurrentPosition.GetPiece(square), Is.EqualTo(Piece.None));
                square++;
            } while (square <= Square.h3);
            do
            {
                Assert.That(board.CurrentPosition.GetPiece(square), Is.EqualTo(Piece.WhitePawn));
                square++;
            } while (square <= Square.h2);
            Assert.That(board.CurrentPosition.GetPiece(Square.a1), Is.EqualTo(Piece.WhiteRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.b1), Is.EqualTo(Piece.WhiteKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.c1), Is.EqualTo(Piece.WhiteBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.d1), Is.EqualTo(Piece.WhiteQueen));
            Assert.That(board.CurrentPosition.GetPiece(Square.e1), Is.EqualTo(Piece.WhiteKing));
            Assert.That(board.CurrentPosition.GetPiece(Square.f1), Is.EqualTo(Piece.WhiteBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.g1), Is.EqualTo(Piece.WhiteKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.h1), Is.EqualTo(Piece.WhiteRook));
            // Validate piece counts.
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhitePawns), Is.EqualTo(8));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhiteKnights), Is.EqualTo(2));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhiteBishops), Is.EqualTo(2));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhiteRooks), Is.EqualTo(2));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhiteQueens), Is.EqualTo(1));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhiteKing), Is.EqualTo(1));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackPawns), Is.EqualTo(8));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackKnights), Is.EqualTo(2));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackBishops), Is.EqualTo(2));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackRooks), Is.EqualTo(2));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackQueens), Is.EqualTo(1));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackKing), Is.EqualTo(1));
        }


        [Test]
        public void TestWac11Position()
        {
            UciStream uciStream = new UciStream();
            Board board = uciStream.Board;
            board.SetPosition("r1b1kb1r/3q1ppp/pBp1pn2/8/Np3P2/5B2/PPP3PP/R2Q1RK1 w kq -");
            uciStream.WriteMessageLine(board.ToString());
            // Validate integrity of board and occupancy of every square.
            board.AssertIntegrity();
            Assert.That(board.CurrentPosition.GetPiece(Square.a8), Is.EqualTo(Piece.BlackRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.b8), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.c8), Is.EqualTo(Piece.BlackBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.d8), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.e8), Is.EqualTo(Piece.BlackKing));
            Assert.That(board.CurrentPosition.GetPiece(Square.f8), Is.EqualTo(Piece.BlackBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.g8), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.h8), Is.EqualTo(Piece.BlackRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.a7), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.b7), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.c7), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.d7), Is.EqualTo(Piece.BlackQueen));
            Assert.That(board.CurrentPosition.GetPiece(Square.e7), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.f7), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.g7), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.h7), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.a6), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.b6), Is.EqualTo(Piece.WhiteBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.c6), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.d6), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.e6), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.f6), Is.EqualTo(Piece.BlackKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.g6), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.h6), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.a5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.b5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.c5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.d5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.e5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.f5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.g5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.h5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.a4), Is.EqualTo(Piece.WhiteKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.b4), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.c4), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.d4), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.e4), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.f4), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.g4), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.h4), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.a3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.b3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.c3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.d3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.e3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.f3), Is.EqualTo(Piece.WhiteBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.g3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.h3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.a2), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.b2), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.c2), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.d2), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.e2), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.f2), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.g2), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.h2), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.a1), Is.EqualTo(Piece.WhiteRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.b1), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.c1), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.d1), Is.EqualTo(Piece.WhiteQueen));
            Assert.That(board.CurrentPosition.GetPiece(Square.e1), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.f1), Is.EqualTo(Piece.WhiteRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.g1), Is.EqualTo(Piece.WhiteKing));
            Assert.That(board.CurrentPosition.GetPiece(Square.h1), Is.EqualTo(Piece.None));
            // Validate piece counts.
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhitePawns), Is.EqualTo(6));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhiteKnights), Is.EqualTo(1));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhiteBishops), Is.EqualTo(2));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhiteRooks), Is.EqualTo(2));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhiteQueens), Is.EqualTo(1));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.WhiteKing), Is.EqualTo(1));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackPawns), Is.EqualTo(7));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackKnights), Is.EqualTo(1));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackBishops), Is.EqualTo(2));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackRooks), Is.EqualTo(2));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackQueens), Is.EqualTo(1));
            Assert.That(Bitwise.CountSetBits(board.CurrentPosition.BlackKing), Is.EqualTo(1));
        }
    }
}