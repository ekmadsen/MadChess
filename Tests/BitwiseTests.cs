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
    public sealed class BitwiseTests
    {
        [Test]
        public void TestCreateUIntMask()
        {
            UciStream uciStream = new UciStream();
            uint mask = Bitwise.CreateUIntMask(7, 11);
            uciStream.WriteMessageLine(Bitwise.ToString(mask));
            Assert.That(Bitwise.ToString(mask), Is.EqualTo("00000000_00000000_00001111_10000000"));
        }


        [Test]
        public void TestKingRingMasks()
        {
            // TODO: Assert in king ring mask tests.
            // King on e8.
            UciStream uciStream = new UciStream();
            string square = "e8";
            ulong mask = Board.InnerRingMasks[Board.GetSquare(square)];
            uciStream.WriteMessageLine($"King on {square} inner ring mask = ");
            uciStream.WriteMessageLine(Position.ToString(mask));
            mask = Board.OuterRingMasks[Board.GetSquare(square)];
            uciStream.WriteMessageLine($"King on {square} outer ring mask = ");
            uciStream.WriteMessageLine(Position.ToString(mask));
            // King on c3.
            square = "c3";
            mask = Board.InnerRingMasks[Board.GetSquare(square)];
            uciStream.WriteMessageLine($"King on {square} inner ring mask = ");
            uciStream.WriteMessageLine(Position.ToString(mask));
            mask = Board.OuterRingMasks[Board.GetSquare(square)];
            uciStream.WriteMessageLine($"King on {square} outer ring mask = ");
            uciStream.WriteMessageLine(Position.ToString(mask));
        }
    }
}