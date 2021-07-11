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
    public sealed class SlidingMoveTests
    {
        [Test]
        public void TestBishopOnE7Moves()
        {
            var uciStream = new UciStream();
            var board = uciStream.Board;
            board.SetPosition("rnbq1bnr/ppppkppp/4p3/8/8/BP6/P1PPPPPP/RN1QKBNR b KQ - 0 3");

            var unoccupiedMovesMask = Board.BishopMoveMasks[(int)Square.E7];
            var expectedUnoccupiedMovesMask = 0ul;
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.D8);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.F8);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.D6);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.F6);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.C5);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.G5);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.B4);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.H4);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.A3);
            var relevantMoveDestinations = unoccupiedMovesMask & PrecalculatedMoves.GetRelevantOccupancy(Square.E7, false);
            uciStream.WriteMessageLine("Relevant move destinations = ");
            uciStream.WriteMessageLine(Position.ToString(relevantMoveDestinations));
            uciStream.WriteMessageLine();
            ulong expectedRelevantMoveDestinations = 0;
            Bitwise.SetBit(ref expectedRelevantMoveDestinations, Square.D6);
            Bitwise.SetBit(ref expectedRelevantMoveDestinations, Square.F6);
            Bitwise.SetBit(ref expectedRelevantMoveDestinations, Square.C5);
            Bitwise.SetBit(ref expectedRelevantMoveDestinations, Square.G5);
            Bitwise.SetBit(ref expectedRelevantMoveDestinations, Square.B4);
            uciStream.WriteMessageLine("Expected relevant move destinations = ");
            uciStream.WriteMessageLine(Position.ToString(relevantMoveDestinations));
            uciStream.WriteMessageLine();
            Assert.That(relevantMoveDestinations, Is.EqualTo(expectedRelevantMoveDestinations));

            Direction[] bishopDirections = { Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest };
            var bishopDestinations = Board.CreateMoveDestinationsMask(Square.E7, board.CurrentPosition.Occupancy, bishopDirections);
            uciStream.WriteMessageLine("Bishop destinations = ");
            uciStream.WriteMessageLine(Position.ToString(bishopDestinations));
            uciStream.WriteMessageLine();
            var expectedBishopDestinations = 0ul;
            Bitwise.SetBit(ref expectedBishopDestinations, Square.D8);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.F8);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.D6);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.F6);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.C5);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.G5);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.B4);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.H4);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.A3);
            uciStream.WriteMessageLine("Expected bishop destinations = ");
            uciStream.WriteMessageLine(Position.ToString(expectedBishopDestinations));
            uciStream.WriteMessageLine();
            Assert.That(bishopDestinations, Is.EqualTo(expectedBishopDestinations));

            var precalculatedBishopDestinations = Board.PrecalculatedMoves.GetBishopMovesMask(Square.E7, board.CurrentPosition.Occupancy);
            uciStream.WriteMessageLine("Precalculated bishop destinations = ");
            uciStream.WriteMessageLine(Position.ToString(precalculatedBishopDestinations));
            uciStream.WriteMessageLine();
            Assert.That(precalculatedBishopDestinations, Is.EqualTo(expectedBishopDestinations));
        }
    }
}
