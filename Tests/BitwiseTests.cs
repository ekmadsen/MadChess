// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Utilities;
using NUnit.Framework;


namespace ErikTheCoder.MadChess.Tests
{
    [TestFixture]
    public sealed class BitwiseTests : TestBase
    {
        [Test]
        public void TestKingRingMasks()
        {
            // King on e8.
            // Inner Ring
            var square = "e8";
            var mask = Board.InnerRingMasks[(int)Board.GetSquare(square)];
            WriteMessageLine($"King on {square} inner ring mask = ");
            WriteMessageLine(Position.ToString(mask));
            Assert.That(Bitwise.IsBitSet(mask, Square.D8), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.D7), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.E7), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.F7), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.F8), Is.EqualTo(true));
            mask &= Bitwise.CreateULongUnmask(Square.D8) & Bitwise.CreateULongUnmask(Square.D7) & Bitwise.CreateULongUnmask(Square.E7) & Bitwise.CreateULongUnmask(Square.F7) & Bitwise.CreateULongUnmask(Square.F8);
            Assert.That(mask & Bitwise.CreateULongMask((int) Square.A8, (int) Square.H1), Is.EqualTo(0));
            // Outer Ring
            mask = Board.OuterRingMasks[(int)Board.GetSquare(square)];
            WriteMessageLine($"King on {square} outer ring mask = ");
            WriteMessageLine(Position.ToString(mask));
            Assert.That(Bitwise.IsBitSet(mask, Square.C8), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.C7), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.C6), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.D6), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.E6), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.F6), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.G6), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.G7), Is.EqualTo(true));
            Assert.That(Bitwise.IsBitSet(mask, Square.G8), Is.EqualTo(true));
            mask &= Bitwise.CreateULongUnmask(Square.C8) & Bitwise.CreateULongUnmask(Square.C7) & Bitwise.CreateULongUnmask(Square.C6) & Bitwise.CreateULongUnmask(Square.D6) & Bitwise.CreateULongUnmask(Square.E6) &
                    Bitwise.CreateULongUnmask(Square.F6) & Bitwise.CreateULongUnmask(Square.G6) & Bitwise.CreateULongUnmask(Square.G7) & Bitwise.CreateULongUnmask(Square.G8);
            Assert.That(mask & Bitwise.CreateULongMask((int)Square.A8, (int)Square.H1), Is.EqualTo(0));
            // King on c3.
            square = "c3";
            mask = Board.InnerRingMasks[(int)Board.GetSquare(square)];
            WriteMessageLine($"King on {square} inner ring mask = ");
            WriteMessageLine(Position.ToString(mask));
            mask = Board.OuterRingMasks[(int)Board.GetSquare(square)];
            WriteMessageLine($"King on {square} outer ring mask = ");
            WriteMessageLine(Position.ToString(mask));
        }
    }
}