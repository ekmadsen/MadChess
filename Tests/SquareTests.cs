// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Game;
using NUnit.Framework;


namespace ErikTheCoder.MadChess.Tests
{
    [TestFixture]
    public sealed class SquareTests : TestBase
    {
        [Test]
        public void TestA1()
        {
            var square = Board.GetSquare("a1");
            Assert.That(square, Is.EqualTo(Square.A1));
            Assert.That((int)square, Is.EqualTo(56));
        }

        [Test]
        public void TestA8()
        {
            var square = Board.GetSquare("a8");
            Assert.That(square, Is.EqualTo(Square.A8));
            Assert.That((int)square, Is.EqualTo(0));
        }


        [Test]
        public void TestC5()
        {
            var square = Board.GetSquare("c5");
            Assert.That(square, Is.EqualTo(Square.C5));
            Assert.That((int)square, Is.EqualTo(26));
        }


        [Test]
        public void TestF3()
        {
            var square = Board.GetSquare("f3");
            Assert.That(square, Is.EqualTo(Square.F3));
            Assert.That((int)square, Is.EqualTo(45));
        }


        [Test]
        public void TestH1()
        {
            var square = Board.GetSquare("h1");
            Assert.That(square, Is.EqualTo(Square.H1));
            Assert.That((int)square, Is.EqualTo(63));
        }


        [Test]
        public void TestH8()
        {
            var square = Board.GetSquare("h8");
            Assert.That(square, Is.EqualTo(Square.H8));
            Assert.That((int)square, Is.EqualTo(7));
        }


        [Test]
        public void TestF6FromWhitePerspective()
        {
            var f6Square = Board.GetSquare("f6");
            var whiteToMoveSquare = Board.GetSquareFromWhitePerspective(f6Square, Color.White);
            var blackToMoveSquare = Board.GetSquareFromWhitePerspective(f6Square, Color.Black);
            Assert.That(whiteToMoveSquare, Is.EqualTo(Square.F6));
            Assert.That(blackToMoveSquare, Is.EqualTo(Square.C3));
        }
    }
}