// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class PrecalculatedMoves
    {
        private readonly ulong[] _bishopRelevantOccupancyMasks;
        private readonly ulong[] _bishopMagicMultipliers;
        private readonly int[] _bishopShifts;
        private readonly ulong[][] _bishopMoveMasks; // [Square][Index]
        private readonly ulong[] _rookRelevantOccupancyMasks;
        private readonly ulong[] _rookMagicMultipliers;
        private readonly int[] _rookShifts;
        private readonly ulong[][] _rookMoveMasks; // [Square][Index]


        public PrecalculatedMoves()
        {
            _bishopRelevantOccupancyMasks = new ulong[64];
            _bishopMagicMultipliers = new ulong[64];
            _bishopShifts = new int[64];
            _bishopMoveMasks = new ulong[64][];
            _rookRelevantOccupancyMasks = new ulong[64];
            _rookMagicMultipliers = new ulong[64];
            _rookShifts = new int[64];
            _rookMoveMasks = new ulong[64][];
            // Calculate relevant occupancy masks.
            for (var square = 0; square < 64; square++) _bishopRelevantOccupancyMasks[square] = Board.BishopMoveMasks[square] & GetRelevantOccupancy(square, false);
            for (var square = 0; square < 64; square++) _rookRelevantOccupancyMasks[square] = Board.RookMoveMasks[square] & GetRelevantOccupancy(square, true);

            // Find magic multipliers if not already known.
            _bishopMagicMultipliers[Square.a8] = 0x7099C1ECF439F7FEul;
            _bishopMagicMultipliers[Square.b8] = 0x2C1B54D809792D91ul;
            _bishopMagicMultipliers[Square.c8] = 0x674106258B047ABBul;
            _bishopMagicMultipliers[Square.d8] = 0xB79809EAB3293D1Bul;
            _bishopMagicMultipliers[Square.e8] = 0x10BC0C2012AC9C54ul;
            _bishopMagicMultipliers[Square.f8] = 0xEFA461D8FF02AF1Cul;
            _bishopMagicMultipliers[Square.g8] = 0x7559634FFBFFFA41ul;
            _bishopMagicMultipliers[Square.h8] = 0x3D8CE431A8BFD419ul;

            _bishopMagicMultipliers[Square.a7] = 0x9802DEA895925FF2ul;
            _bishopMagicMultipliers[Square.b7] = 0x20A158E7A90FE483ul;
            _bishopMagicMultipliers[Square.c7] = 0x5AD7103586074F2Cul;
            _bishopMagicMultipliers[Square.d7] = 0x27B3020A0A006F94ul;
            _bishopMagicMultipliers[Square.e7] = 0xD8741110C08A882Cul;
            _bishopMagicMultipliers[Square.f7] = 0x8A3AFD03A0102940ul;
            _bishopMagicMultipliers[Square.g7] = 0x901E6C0E41547FD2ul;
            _bishopMagicMultipliers[Square.h7] = 0x145F967BFFA814D8ul;

            _bishopMagicMultipliers[Square.a6] = 0x8BB4547657BBBFE1ul;
            _bishopMagicMultipliers[Square.b6] = 0xFDF4217D3F7998C4ul;
            _bishopMagicMultipliers[Square.c6] = 0x15F80B500C2040D0ul;
            _bishopMagicMultipliers[Square.d6] = 0x3EE402380243680Eul;
            _bishopMagicMultipliers[Square.e6] = 0x637501882008018Eul;
            _bishopMagicMultipliers[Square.f6] = 0x853F004602869424ul;
            _bishopMagicMultipliers[Square.g6] = 0xBB3D4C0E1736FF8Dul;
            _bishopMagicMultipliers[Square.h6] = 0xCC2704C6029DFFA4ul;

            _bishopMagicMultipliers[Square.a5] = 0xC620391B514F5F7Ful;
            _bishopMagicMultipliers[Square.b5] = 0x1D6C141726500412ul;
            _bishopMagicMultipliers[Square.c5] = 0x0FCA281DF0008021ul;
            _bishopMagicMultipliers[Square.d5] = 0x004006000C009010ul;
            _bishopMagicMultipliers[Square.e5] = 0x194B00B063004000ul;
            _bishopMagicMultipliers[Square.f5] = 0x719004829700898Dul;
            _bishopMagicMultipliers[Square.g5] = 0x32EA2E0807C11029ul;
            _bishopMagicMultipliers[Square.h5] = 0x51628F6D9F9403CFul;

            _bishopMagicMultipliers[Square.a4] = 0x95BB524004AA45E6ul;
            _bishopMagicMultipliers[Square.b4] = 0x77C8D018D3AB480Cul;
            _bishopMagicMultipliers[Square.c4] = 0x4DA646B802700335ul;
            _bishopMagicMultipliers[Square.d4] = 0xC396F00821040400ul;
            _bishopMagicMultipliers[Square.e4] = 0x5E380F6C00094100ul;
            _bishopMagicMultipliers[Square.f4] = 0xC7F1070E006B01C7ul;
            _bishopMagicMultipliers[Square.g4] = 0x23D624090351E804ul;
            _bishopMagicMultipliers[Square.h4] = 0x612C009C81A26417ul;

            _bishopMagicMultipliers[Square.a3] = 0x908FD6588E66B93Bul;
            _bishopMagicMultipliers[Square.b3] = 0x5FF64718287A206Aul;
            _bishopMagicMultipliers[Square.c3] = 0xB5CFFE3603027A08ul;
            _bishopMagicMultipliers[Square.d3] = 0x650499E013066802ul;
            _bishopMagicMultipliers[Square.e3] = 0xCFB54C270C017E00ul;
            _bishopMagicMultipliers[Square.f3] = 0x4D200F8502C2FA00ul;
            _bishopMagicMultipliers[Square.g3] = 0xE2FF7E66968AFF45ul;
            _bishopMagicMultipliers[Square.h3] = 0xFEF81209CA03ED44ul;

            _bishopMagicMultipliers[Square.a2] = 0x3797F367D90EC167ul;
            _bishopMagicMultipliers[Square.b2] = 0x508BFCF242F40AB4ul;
            _bishopMagicMultipliers[Square.c2] = 0xAF9A420642383663ul;
            _bishopMagicMultipliers[Square.d2] = 0xE56EE992E1881FBDul;
            _bishopMagicMultipliers[Square.e2] = 0x4DC8E4F05F37B3B2ul;
            _bishopMagicMultipliers[Square.f2] = 0xB4A1A13E8E72035Aul;
            _bishopMagicMultipliers[Square.g2] = 0x2FC1B20C04078D0Eul;
            _bishopMagicMultipliers[Square.h2] = 0xB4BFBB9B79729264ul;

            _bishopMagicMultipliers[Square.a1] = 0x8FDFFFCF3CA21D69ul;
            _bishopMagicMultipliers[Square.b1] = 0x2EA5976CA801EFB9ul;
            _bishopMagicMultipliers[Square.c1] = 0x89AC2287F5F3500Cul;
            _bishopMagicMultipliers[Square.d1] = 0x7EA6599134840435ul;
            _bishopMagicMultipliers[Square.e1] = 0x9F49970A3206660Aul;
            _bishopMagicMultipliers[Square.f1] = 0x22F11FFF06906D03ul;
            _bishopMagicMultipliers[Square.g1] = 0xEBC4FFAC8FD9EE0Ful;
            _bishopMagicMultipliers[Square.h1] = 0x267FA4B9D2C59BDCul;

            _rookMagicMultipliers[Square.a8] = 0xD9800180B3400524ul;
            _rookMagicMultipliers[Square.b8] = 0X3FD80075FFEBFFFFul;
            _rookMagicMultipliers[Square.c8] = 0X4010000DF6F6FFFEul;
            _rookMagicMultipliers[Square.d8] = 0X0050001FAFFAFFFFul;
            _rookMagicMultipliers[Square.e8] = 0X0050028004FFFFB0ul;
            _rookMagicMultipliers[Square.f8] = 0X7F600280089FFFF1ul;
            _rookMagicMultipliers[Square.g8] = 0X7F5000B0029FFFFCul;
            _rookMagicMultipliers[Square.h8] = 0X5B58004848A7FFFAul;

            _rookMagicMultipliers[Square.a7] = 0xFD0F800289C00061ul;
            _rookMagicMultipliers[Square.b7] = 0x000050007F13FFFFul;
            _rookMagicMultipliers[Square.c7] = 0x007FA0006013FFFFul;
            _rookMagicMultipliers[Square.d7] = 0x0022004128102200ul;
            _rookMagicMultipliers[Square.e7] = 0x000200081201200Cul;
            _rookMagicMultipliers[Square.f7] = 0x202A001048460004ul;
            _rookMagicMultipliers[Square.g7] = 0x0081000100420004ul;
            _rookMagicMultipliers[Square.h7] = 0x4000800380004500ul;

            _rookMagicMultipliers[Square.a6] = 0x0000208002904001ul;
            _rookMagicMultipliers[Square.b6] = 0x0090004040026008ul;
            _rookMagicMultipliers[Square.c6] = 0x0208808010002001ul;
            _rookMagicMultipliers[Square.d6] = 0x2002020020704940ul;
            _rookMagicMultipliers[Square.e6] = 0x8048010008110005ul;
            _rookMagicMultipliers[Square.f6] = 0x6820808004002200ul;
            _rookMagicMultipliers[Square.g6] = 0x0A80040008023011ul;
            _rookMagicMultipliers[Square.h6] = 0x00B1460000811044ul;

            _rookMagicMultipliers[Square.a5] = 0x4204400080008EA0ul;
            _rookMagicMultipliers[Square.b5] = 0xB002400180200184ul;
            _rookMagicMultipliers[Square.c5] = 0x2020200080100380ul;
            _rookMagicMultipliers[Square.d5] = 0x0010080080100080ul;
            _rookMagicMultipliers[Square.e5] = 0x2204080080800400ul;
            _rookMagicMultipliers[Square.f5] = 0x0000A40080360080ul;
            _rookMagicMultipliers[Square.g5] = 0x02040604002810B1ul;
            _rookMagicMultipliers[Square.h5] = 0x008C218600004104ul;

            _rookMagicMultipliers[Square.a4] = 0x8180004000402000ul;
            _rookMagicMultipliers[Square.b4] = 0x488C402000401001ul;
            _rookMagicMultipliers[Square.c4] = 0x4018A00080801004ul;
            _rookMagicMultipliers[Square.d4] = 0x1230002105001008ul;
            _rookMagicMultipliers[Square.e4] = 0x8904800800800400ul;
            _rookMagicMultipliers[Square.f4] = 0x0042000C42003810ul;
            _rookMagicMultipliers[Square.g4] = 0x008408110400B012ul;
            _rookMagicMultipliers[Square.h4] = 0x0018086182000401ul;

            _rookMagicMultipliers[Square.a3] = 0x2240088020C28000ul;
            _rookMagicMultipliers[Square.b3] = 0x001001201040C004ul;
            _rookMagicMultipliers[Square.c3] = 0x0A02008010420020ul;
            _rookMagicMultipliers[Square.d3] = 0x0010003009010060ul;
            _rookMagicMultipliers[Square.e3] = 0x0004008008008014ul;
            _rookMagicMultipliers[Square.f3] = 0x0080020004008080ul;
            _rookMagicMultipliers[Square.g3] = 0x0282020001008080ul;
            _rookMagicMultipliers[Square.h3] = 0x50000181204A0004ul;

            _rookMagicMultipliers[Square.a2] = 0x48FFFE99FECFAA00ul;
            _rookMagicMultipliers[Square.b2] = 0x48FFFE99FECFAA00ul;
            _rookMagicMultipliers[Square.c2] = 0x497FFFADFF9C2E00ul;
            _rookMagicMultipliers[Square.d2] = 0x613FFFDDFFCE9200ul;
            _rookMagicMultipliers[Square.e2] = 0xFFFFFFE9FFE7CE00ul;
            _rookMagicMultipliers[Square.f2] = 0xFFFFFFF5FFF3E600ul;
            _rookMagicMultipliers[Square.g2] = 0x0010301802830400ul;
            _rookMagicMultipliers[Square.h2] = 0x510FFFF5F63C96A0ul;

            _rookMagicMultipliers[Square.a1] = 0xEBFFFFB9FF9FC526ul;
            _rookMagicMultipliers[Square.b1] = 0x61FFFEDDFEEDAEAEul;
            _rookMagicMultipliers[Square.c1] = 0x53BFFFEDFFDEB1A2ul;
            _rookMagicMultipliers[Square.d1] = 0x127FFFB9FFDFB5F6ul;
            _rookMagicMultipliers[Square.e1] = 0x411FFFDDFFDBF4D6ul;
            _rookMagicMultipliers[Square.f1] = 0x0801000804000603ul;
            _rookMagicMultipliers[Square.g1] = 0x0003FFEF27EEBE74ul;
            _rookMagicMultipliers[Square.h1] = 0x7645FFFECBFEA79Eul;

            FindMagicMultipliers(Piece.WhiteBishop);
            FindMagicMultipliers(Piece.WhiteRook);
        }


        public ulong GetBishopMovesMask(int Square, ulong Occupancy)
        {
            var occupancy = Occupancy & _bishopRelevantOccupancyMasks[Square];
            var index = GetIndex(occupancy, _bishopMagicMultipliers[Square], _bishopShifts[Square]);
            return _bishopMoveMasks[Square][index];
        }


        public ulong GetRookMovesMask(int Square, ulong Occupancy)
        {
            var occupancy = Occupancy & _rookRelevantOccupancyMasks[Square];
            var index = GetIndex(occupancy, _rookMagicMultipliers[Square], _rookShifts[Square]);
            return _rookMoveMasks[Square][index];
        }


        public void FindMagicMultipliers(int Piece, Delegates.WriteMessageLine WriteMessageLine = null)
        {
            Direction[] directions;
            ulong[] unoccupiedMoveMasks;
            ulong[] relevantOccupancyMasks;
            ulong[] magicMultipliers;
            int[] shifts;
            ulong[][] moveMasks;
            switch (Piece)
            {
                case Engine.Piece.WhiteBishop:
                case Engine.Piece.BlackBishop:
                    directions = new[] {Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest};
                    unoccupiedMoveMasks = Board.BishopMoveMasks;
                    relevantOccupancyMasks = _bishopRelevantOccupancyMasks;
                    magicMultipliers = _bishopMagicMultipliers;
                    shifts = _bishopShifts;
                    moveMasks = _bishopMoveMasks;
                    break;
                case Engine.Piece.WhiteRook:
                case Engine.Piece.BlackRook:
                    directions = new[] {Direction.North, Direction.East, Direction.South, Direction.West};
                    unoccupiedMoveMasks = Board.RookMoveMasks;
                    relevantOccupancyMasks = _rookRelevantOccupancyMasks;
                    magicMultipliers = _rookMagicMultipliers;
                    shifts = _rookShifts;
                    moveMasks = _rookMoveMasks;
                    break;
                default:
                    throw new ArgumentException($"{Piece} piece not supported.");
            }
            // Generate moves mask on each square.
            var occupancyToMovesMask = new Dictionary<ulong, ulong>();
            var uniqueMovesMasks = new HashSet<ulong>();
            for (var square = 0; square < 64; square++)
            {
                occupancyToMovesMask.Clear();
                uniqueMovesMasks.Clear();
                var moveDestinations = unoccupiedMoveMasks[square];
                var relevantMoveDestinations = moveDestinations & relevantOccupancyMasks[square];
                var uniqueOccupancies = (int) Math.Pow(2, Bitwise.CountSetBits(relevantMoveDestinations));
                occupancyToMovesMask.EnsureCapacity(uniqueOccupancies);
                // Generate moves mask for every permutation of relevant occupancy bits.
                using (var occupancyPermutations = Bitwise.GetAllPermutations(relevantMoveDestinations).GetEnumerator())
                {
                    while (occupancyPermutations.MoveNext())
                    {
                        var occupancy = occupancyPermutations.Current;
                        if (!occupancyToMovesMask.ContainsKey(occupancy))
                        {
                            // Have not yet generated moves for this occupancy mask.
                            var movesMask = Board.CreateMoveDestinationsMask(square, occupancy, directions);
                            occupancyToMovesMask.Add(occupancy, movesMask);
                            if (!uniqueMovesMasks.Contains(movesMask)) uniqueMovesMasks.Add(movesMask);
                        }
                    }
                }
                // Validate enumerator found all permutations of relevant occupancy bits.
                Debug.Assert(occupancyToMovesMask.Count == uniqueOccupancies);
                // Determine bit shift that produces number >= unique occupancies.
                // A stricter condition is number >= unique moves but this requires more computing time to find magic multipliers.
                var shift = 64 - (int) Math.Ceiling(Math.Log(uniqueOccupancies, 2d));
                shifts[square] = shift;
                var magicMultiplier = magicMultipliers[square];
                if (magicMultiplier == 0) (magicMultipliers[square], moveMasks[square]) = FindMagicMultiplier(occupancyToMovesMask, shift, null);
                else (magicMultipliers[square], moveMasks[square]) = FindMagicMultiplier(occupancyToMovesMask, shift, magicMultiplier);
                WriteMessageLine?.Invoke($"{Board.SquareLocations[square],6}  {Engine.Piece.GetName(Piece),6}  {shift,5}  {occupancyToMovesMask.Count,18}  {uniqueMovesMasks.Count,12}  {magicMultipliers[square],16:X16}");
            }
        }


        public static ulong GetRelevantOccupancy(int Square, bool FileRankSlidingPiece)
        {
            if ((Board.SquareMasks[Square] & Board.EdgeSquareMask) == 0) return ~Board.EdgeSquareMask;
            // Square is on edge of board.
            if (!FileRankSlidingPiece) return ~Board.EdgeSquareMask;
            // Piece can slide along file or rank.
            ulong occupancy;
            var file = Board.Files[Square];
            var rank = Board.WhiteRanks[Square];
            // ReSharper disable ConvertSwitchStatementToSwitchExpression
            switch (file)
            {
                case 0:
                    // Piece is on Westernmost edge file.
                    switch (rank)
                    {
                        case 0:
                            // Piece is on A1 square.
                            // Occupancy of most distant squares does not affect pseudo-legal moves.
                            return ~Board.SquareMasks[Engine.Square.a8] & ~Board.SquareMasks[Engine.Square.h1];
                        case 7:
                            // Piece is on A8 square.
                            // Occupancy of most distant squares does not affect pseudo-legal moves.
                            return ~Board.SquareMasks[Engine.Square.a1] & ~Board.SquareMasks[Engine.Square.h8];
                        default:
                            // Piece is not on edge rank.
                            // Occupancy of edge ranks and opposite edge file does not affect pseudo-legal moves.
                            return ~Board.RankMasks[0] & ~Board.RankMasks[7] & ~Board.FileMasks[7];
                    }
                case 7:
                    // Piece is on Easternmost edge file.
                    switch (rank)
                    {
                        case 0:
                            // Piece is on H1 square.
                            // Occupancy of most distant squares does not affect pseudo-legal moves.
                            return ~Board.SquareMasks[Engine.Square.a1] & ~Board.SquareMasks[Engine.Square.h8];
                        case 7:
                            // Piece is on H8 square.
                            // Occupancy of most distant squares does not affect pseudo-legal moves.
                            return ~Board.SquareMasks[Engine.Square.a8] & ~Board.SquareMasks[Engine.Square.h1];
                        default:
                            // Piece is not on edge rank.
                            // Occupancy of edge ranks and opposite edge file does not affect pseudo-legal moves.
                            return ~Board.RankMasks[0] & ~Board.RankMasks[7] & ~Board.FileMasks[0];
                    }
                default:
                    // Piece is not on edge file.
                    // Occupancy of edge files does not affect pseudo-legal moves.
                    occupancy = ~Board.FileMasks[0] & ~Board.FileMasks[7];
                    break;
            }
            // Piece is not on a corner square (handled in code above).
            switch (rank)
            {
                case 0:
                    // Piece is on Southernmost edge rank.
                    // Occupancy of opposite rank does not affect pseudo-legal moves.
                    return occupancy & ~Board.RankMasks[7];
                case 7:
                    // Piece is on Northernmost edge rank.
                    // Occupancy of opposite rank does not affect pseudo-legal moves.
                    return occupancy & ~Board.RankMasks[0];
                default:
                    // Piece is not on edge rank.
                    // Occupancy of edge ranks does not affect pseudo-legal moves.
                    return occupancy & ~Board.RankMasks[0] & ~Board.RankMasks[7];
            }
            // ReSharper restore ConvertSwitchStatementToSwitchExpression
        }


        private static int GetIndex(ulong Occupancy, ulong MagicMultiplier, int Shift) => (int) ((Occupancy * MagicMultiplier) >> Shift);


        private static (ulong MagicMultiplier, ulong[] MovesMasks) FindMagicMultiplier(Dictionary<ulong, ulong> OccupancyToMovesMask, int Shift, ulong? KnownMagicMultiplier)
        {
            var indexBits = 64 - Shift;
            var indexLength = (int) Math.Pow(2d, indexBits);
            var movesMasks = new ulong[indexLength];
            var occupancies = new List<ulong>(OccupancyToMovesMask.Keys);
            NextMagicMultiplier:
            var magicMultiplier = KnownMagicMultiplier ?? SafeRandom.NextULong();
            // Clear moves masks.
            for (var maskIndex = 0; maskIndex < movesMasks.Length; maskIndex++) movesMasks[maskIndex] = 0;
            for (var occupancyIndex = 0; occupancyIndex < occupancies.Count; occupancyIndex++)
            {
                var occupancy = occupancies[occupancyIndex];
                var index = GetIndex(occupancy, magicMultiplier, Shift);
                var movesMask = movesMasks[index];
                if (movesMask == 0) movesMasks[index] = OccupancyToMovesMask[occupancy]; // Moves mask not yet added to unique moves array.
                else if (movesMask != OccupancyToMovesMask[occupancy]) goto NextMagicMultiplier; // Moves mask already added to unique moves array but mask is incorrect.
            }
            // Found magic multiplier that maps to correct moves index for all occupancies.
            return (magicMultiplier, movesMasks);
        }
    }
}