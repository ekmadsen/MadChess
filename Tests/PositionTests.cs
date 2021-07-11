// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
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
            var uciStream = new UciStream();
            var board = uciStream.Board;
            board.SetPosition(Board.StartPositionFen);
            uciStream.WriteMessageLine(board.ToString());
            // Validate integrity of board and occupancy of every square.
            board.AssertIntegrity();
            Assert.That(board.CurrentPosition.GetPiece(Square.A8), Is.EqualTo(Piece.BlackRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.B8), Is.EqualTo(Piece.BlackKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.C8), Is.EqualTo(Piece.BlackBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.D8), Is.EqualTo(Piece.BlackQueen));
            Assert.That(board.CurrentPosition.GetPiece(Square.E8), Is.EqualTo(Piece.BlackKing));
            Assert.That(board.CurrentPosition.GetPiece(Square.F8), Is.EqualTo(Piece.BlackBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.G8), Is.EqualTo(Piece.BlackKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.H8), Is.EqualTo(Piece.BlackRook));
            var square = Square.A7;
            do
            {
                Assert.That(board.CurrentPosition.GetPiece(square), Is.EqualTo(Piece.BlackPawn));
                square++;
            } while (square <= Square.H7);
            do
            {
                Assert.That(board.CurrentPosition.GetPiece(square), Is.EqualTo(Piece.None));
                square++;
            } while (square <= Square.H3);
            do
            {
                Assert.That(board.CurrentPosition.GetPiece(square), Is.EqualTo(Piece.WhitePawn));
                square++;
            } while (square <= Square.H2);
            Assert.That(board.CurrentPosition.GetPiece(Square.A1), Is.EqualTo(Piece.WhiteRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.B1), Is.EqualTo(Piece.WhiteKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.C1), Is.EqualTo(Piece.WhiteBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.D1), Is.EqualTo(Piece.WhiteQueen));
            Assert.That(board.CurrentPosition.GetPiece(Square.E1), Is.EqualTo(Piece.WhiteKing));
            Assert.That(board.CurrentPosition.GetPiece(Square.F1), Is.EqualTo(Piece.WhiteBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.G1), Is.EqualTo(Piece.WhiteKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.H1), Is.EqualTo(Piece.WhiteRook));
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
            var uciStream = new UciStream();
            var board = uciStream.Board;
            board.SetPosition("r1b1kb1r/3q1ppp/pBp1pn2/8/Np3P2/5B2/PPP3PP/R2Q1RK1 w kq -");
            uciStream.WriteMessageLine(board.ToString());
            // Validate integrity of board and occupancy of every square.
            board.AssertIntegrity();
            Assert.That(board.CurrentPosition.GetPiece(Square.A8), Is.EqualTo(Piece.BlackRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.B8), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.C8), Is.EqualTo(Piece.BlackBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.D8), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.E8), Is.EqualTo(Piece.BlackKing));
            Assert.That(board.CurrentPosition.GetPiece(Square.F8), Is.EqualTo(Piece.BlackBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.G8), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.H8), Is.EqualTo(Piece.BlackRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.A7), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.B7), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.C7), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.D7), Is.EqualTo(Piece.BlackQueen));
            Assert.That(board.CurrentPosition.GetPiece(Square.E7), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.F7), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.G7), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.H7), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.A6), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.B6), Is.EqualTo(Piece.WhiteBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.C6), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.D6), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.E6), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.F6), Is.EqualTo(Piece.BlackKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.G6), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.H6), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.A5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.B5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.C5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.D5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.E5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.F5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.G5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.H5), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.A4), Is.EqualTo(Piece.WhiteKnight));
            Assert.That(board.CurrentPosition.GetPiece(Square.B4), Is.EqualTo(Piece.BlackPawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.C4), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.D4), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.E4), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.F4), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.G4), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.H4), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.A3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.B3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.C3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.D3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.E3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.F3), Is.EqualTo(Piece.WhiteBishop));
            Assert.That(board.CurrentPosition.GetPiece(Square.G3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.H3), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.A2), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.B2), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.C2), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.D2), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.E2), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.F2), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.G2), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.H2), Is.EqualTo(Piece.WhitePawn));
            Assert.That(board.CurrentPosition.GetPiece(Square.A1), Is.EqualTo(Piece.WhiteRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.B1), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.C1), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.D1), Is.EqualTo(Piece.WhiteQueen));
            Assert.That(board.CurrentPosition.GetPiece(Square.E1), Is.EqualTo(Piece.None));
            Assert.That(board.CurrentPosition.GetPiece(Square.F1), Is.EqualTo(Piece.WhiteRook));
            Assert.That(board.CurrentPosition.GetPiece(Square.G1), Is.EqualTo(Piece.WhiteKing));
            Assert.That(board.CurrentPosition.GetPiece(Square.H1), Is.EqualTo(Piece.None));
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