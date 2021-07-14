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
            var square = (int)Board.GetSquare("a1");
            Assert.That(square, Is.EqualTo(56));
        }

        [Test]
        public void TestA8()
        {
            var square = (int)Board.GetSquare("a8");
            Assert.That(square, Is.EqualTo(0));
        }


        [Test]
        public void TestC5()
        {
            var square = (int)Board.GetSquare("c5");
            Assert.That(square, Is.EqualTo(26));
        }


        [Test]
        public void TestF3()
        {
            var square = (int)Board.GetSquare("f3");
            Assert.That(square, Is.EqualTo(45));
        }


        [Test]
        public void TestH1()
        {
            var square = (int)Board.GetSquare("h1");
            Assert.That(square, Is.EqualTo(63));
        }


        [Test]
        public void TestH8()
        {
            var square = (int)Board.GetSquare("h8");
            Assert.That(square, Is.EqualTo(7));
        }
    }
}