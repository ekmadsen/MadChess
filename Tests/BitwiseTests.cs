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
    public sealed class BitwiseTests
    {
        [Test]
        public void TestCreateUIntMask()
        {
            var uciStream = new UciStream();
            var mask = Bitwise.CreateUIntMask(7, 11);
            uciStream.WriteMessageLine(Bitwise.ToString(mask));
            Assert.That(Bitwise.ToString(mask), Is.EqualTo("00000000_00000000_00001111_10000000"));
        }


        [Test]
        public void TestKingRingMasks()
        {
            // TODO: Assert in king ring mask tests.
            // King on e8.
            var uciStream = new UciStream();
            var square = "e8";
            var mask = Board.InnerRingMasks[(int)Board.GetSquare(square)];
            uciStream.WriteMessageLine($"King on {square} inner ring mask = ");
            uciStream.WriteMessageLine(Position.ToString(mask));
            mask = Board.OuterRingMasks[(int)Board.GetSquare(square)];
            uciStream.WriteMessageLine($"King on {square} outer ring mask = ");
            uciStream.WriteMessageLine(Position.ToString(mask));
            // King on c3.
            square = "c3";
            mask = Board.InnerRingMasks[(int)Board.GetSquare(square)];
            uciStream.WriteMessageLine($"King on {square} inner ring mask = ");
            uciStream.WriteMessageLine(Position.ToString(mask));
            mask = Board.OuterRingMasks[(int)Board.GetSquare(square)];
            uciStream.WriteMessageLine($"King on {square} outer ring mask = ");
            uciStream.WriteMessageLine(Position.ToString(mask));
        }
    }
}