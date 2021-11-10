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


namespace ErikTheCoder.MadChess.Tests;


[TestFixture]
public sealed class KnightMoveTests : TestBase
{
    [Test]
    public void TestKnightOnC5Moves()
    {
        var board = new Board(WriteMessageLine, long.MaxValue);
        board.SetPosition("4k3/pppppppp/8/2N5/8/3P4/PPP1PPPP/4K3 w - - 0 1");

        var knightDestinations = Board.KnightMoveMasks[(int)Square.C5] & ~board.CurrentPosition.ColorOccupancy[(int)Color.White];
        WriteMessageLine("Knight destinations = ");
        WriteMessageLine(Position.ToString(knightDestinations));
        WriteMessageLine();
        var expectedKnightDestinations = 0ul;
        Bitwise.SetBit(ref expectedKnightDestinations, Square.B7);
        Bitwise.SetBit(ref expectedKnightDestinations, Square.D7);
        Bitwise.SetBit(ref expectedKnightDestinations, Square.E6);
        Bitwise.SetBit(ref expectedKnightDestinations, Square.E4);
        Bitwise.SetBit(ref expectedKnightDestinations, Square.B3);
        Bitwise.SetBit(ref expectedKnightDestinations, Square.A4);
        Bitwise.SetBit(ref expectedKnightDestinations, Square.A6);
        WriteMessageLine("Expected knight destinations = ");
        WriteMessageLine(Position.ToString(expectedKnightDestinations));
        WriteMessageLine();
        Assert.That(knightDestinations, Is.EqualTo(expectedKnightDestinations));
    }
}