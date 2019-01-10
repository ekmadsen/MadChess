// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Engine;
using NUnit.Framework;


namespace ErikTheCoder.MadChess.Tests
{
    [TestFixture]
    public sealed class SquareTests
    {
        [Test]
        public void TestA1()
        {
            int square = Board.GetSquare("a1");
            Assert.That(square, Is.EqualTo(56));
        }

        [Test]
        public void TestA8()
        {
            int square = Board.GetSquare("a8");
            Assert.That(square, Is.EqualTo(0));
        }


        [Test]
        public void TestC5()
        {
            int square = Board.GetSquare("c5");
            Assert.That(square, Is.EqualTo(26));
        }


        [Test]
        public void TestF3()
        {
            int square = Board.GetSquare("f3");
            Assert.That(square, Is.EqualTo(45));
        }


        [Test]
        public void TestH1()
        {
            int square = Board.GetSquare("h1");
            Assert.That(square, Is.EqualTo(63));
        }


        [Test]
        public void TestH8()
        {
            int square = Board.GetSquare("h8");
            Assert.That(square, Is.EqualTo(7));
        }
    }
}