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
    public sealed class SlidingMoveTests
    {
        [Test]
        public void TestBishopOnE7Moves()
        {
            UciStream uciStream = new UciStream();
            Board board = uciStream.Board;
            board.SetPosition("rnbq1bnr/ppppkppp/4p3/8/8/BP6/P1PPPPPP/RN1QKBNR b KQ - 0 3");

            ulong unoccupiedMovesMask = board.BishopMoveMasks[Square.e7];
            ulong expectedUnoccupiedMovesMask = 0;
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.d8);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.f8);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.d6);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.f6);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.c5);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.g5);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.b4);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.h4);
            Bitwise.SetBit(ref expectedUnoccupiedMovesMask, Square.a3);
            ulong relevantMoveDestinations = unoccupiedMovesMask & PrecalculatedMoves.GetRelevantOccupancy(Square.e7, false);
            uciStream.WriteMessageLine("Relevant move destinations = ");
            uciStream.WriteMessageLine(Board.ToString(relevantMoveDestinations));
            uciStream.WriteMessageLine();
            ulong expectedRelevantMoveDestinations = 0;
            Bitwise.SetBit(ref expectedRelevantMoveDestinations, Square.d6);
            Bitwise.SetBit(ref expectedRelevantMoveDestinations, Square.f6);
            Bitwise.SetBit(ref expectedRelevantMoveDestinations, Square.c5);
            Bitwise.SetBit(ref expectedRelevantMoveDestinations, Square.g5);
            Bitwise.SetBit(ref expectedRelevantMoveDestinations, Square.b4);
            uciStream.WriteMessageLine("Expected relevant move destinations = ");
            uciStream.WriteMessageLine(Board.ToString(relevantMoveDestinations));
            uciStream.WriteMessageLine();
            Assert.That(relevantMoveDestinations, Is.EqualTo(expectedRelevantMoveDestinations));

            Direction[] bishopDirections = { Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest };
            ulong bishopDestinations = board.CreateMoveDestinationsMask(Square.e7, board.CurrentPosition.Occupancy, bishopDirections);
            uciStream.WriteMessageLine("Bishop destinations = ");
            uciStream.WriteMessageLine(Board.ToString(bishopDestinations));
            uciStream.WriteMessageLine();
            ulong expectedBishopDestinations = 0;
            Bitwise.SetBit(ref expectedBishopDestinations, Square.d8);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.f8);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.d6);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.f6);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.c5);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.g5);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.b4);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.h4);
            Bitwise.SetBit(ref expectedBishopDestinations, Square.a3);
            uciStream.WriteMessageLine("Expected bishop destinations = ");
            uciStream.WriteMessageLine(Board.ToString(expectedBishopDestinations));
            uciStream.WriteMessageLine();
            Assert.That(bishopDestinations, Is.EqualTo(expectedBishopDestinations));

            ulong precalculatedBishopDestinations = board.PrecalculatedMoves.GetBishopMovesMask(Square.e7, board.CurrentPosition.Occupancy);
            uciStream.WriteMessageLine("Precalculated bishop destinations = ");
            uciStream.WriteMessageLine(Board.ToString(precalculatedBishopDestinations));
            uciStream.WriteMessageLine();
            Assert.That(precalculatedBishopDestinations, Is.EqualTo(expectedBishopDestinations));
        }
    }
}
