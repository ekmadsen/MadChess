// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Core.Moves;


public sealed class PrecalculatedMoves
{
    private readonly ulong[] _bishopRelevantOccupancyMasks;
    private readonly ulong[] _bishopMagicMultipliers;
    private readonly int[] _bishopShifts;
    private readonly ulong[][] _bishopMoveMasks; // [square][magicIndex]
    private readonly ulong[] _rookRelevantOccupancyMasks;
    private readonly ulong[] _rookMagicMultipliers;
    private readonly int[] _rookShifts;
    private readonly ulong[][] _rookMoveMasks; // [square][magicIndex]


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
        for (var square = Square.A8; square < Square.Illegal; square++) _bishopRelevantOccupancyMasks[(int)square] = Board.BishopMoveMasks[(int)square] & GetRelevantOccupancy(square, false);
        for (var square = Square.A8; square < Square.Illegal; square++) _rookRelevantOccupancyMasks[(int)square] = Board.RookMoveMasks[(int)square] & GetRelevantOccupancy(square, true);

        // Find magic multipliers if not already known.
        _bishopMagicMultipliers[(int)Square.A8] = 0x7099C1ECF439F7FEul;
        _bishopMagicMultipliers[(int)Square.B8] = 0x2C1B54D809792D91ul;
        _bishopMagicMultipliers[(int)Square.C8] = 0x674106258B047ABBul;
        _bishopMagicMultipliers[(int)Square.D8] = 0xB79809EAB3293D1Bul;
        _bishopMagicMultipliers[(int)Square.E8] = 0x10BC0C2012AC9C54ul;
        _bishopMagicMultipliers[(int)Square.F8] = 0xEFA461D8FF02AF1Cul;
        _bishopMagicMultipliers[(int)Square.G8] = 0x7559634FFBFFFA41ul;
        _bishopMagicMultipliers[(int)Square.H8] = 0x3D8CE431A8BFD419ul;

        _bishopMagicMultipliers[(int)Square.A7] = 0x9802DEA895925FF2ul;
        _bishopMagicMultipliers[(int)Square.B7] = 0x20A158E7A90FE483ul;
        _bishopMagicMultipliers[(int)Square.C7] = 0x5AD7103586074F2Cul;
        _bishopMagicMultipliers[(int)Square.D7] = 0x27B3020A0A006F94ul;
        _bishopMagicMultipliers[(int)Square.E7] = 0xD8741110C08A882Cul;
        _bishopMagicMultipliers[(int)Square.F7] = 0x8A3AFD03A0102940ul;
        _bishopMagicMultipliers[(int)Square.G7] = 0x901E6C0E41547FD2ul;
        _bishopMagicMultipliers[(int)Square.H7] = 0x145F967BFFA814D8ul;

        _bishopMagicMultipliers[(int)Square.A6] = 0x8BB4547657BBBFE1ul;
        _bishopMagicMultipliers[(int)Square.B6] = 0xFDF4217D3F7998C4ul;
        _bishopMagicMultipliers[(int)Square.C6] = 0x15F80B500C2040D0ul;
        _bishopMagicMultipliers[(int)Square.D6] = 0x3EE402380243680Eul;
        _bishopMagicMultipliers[(int)Square.E6] = 0x637501882008018Eul;
        _bishopMagicMultipliers[(int)Square.F6] = 0x853F004602869424ul;
        _bishopMagicMultipliers[(int)Square.G6] = 0xBB3D4C0E1736FF8Dul;
        _bishopMagicMultipliers[(int)Square.H6] = 0xCC2704C6029DFFA4ul;

        _bishopMagicMultipliers[(int)Square.A5] = 0xC620391B514F5F7Ful;
        _bishopMagicMultipliers[(int)Square.B5] = 0x1D6C141726500412ul;
        _bishopMagicMultipliers[(int)Square.C5] = 0x0FCA281DF0008021ul;
        _bishopMagicMultipliers[(int)Square.D5] = 0x004006000C009010ul;
        _bishopMagicMultipliers[(int)Square.E5] = 0x194B00B063004000ul;
        _bishopMagicMultipliers[(int)Square.F5] = 0x719004829700898Dul;
        _bishopMagicMultipliers[(int)Square.G5] = 0x32EA2E0807C11029ul;
        _bishopMagicMultipliers[(int)Square.H5] = 0x51628F6D9F9403CFul;

        _bishopMagicMultipliers[(int)Square.A4] = 0x95BB524004AA45E6ul;
        _bishopMagicMultipliers[(int)Square.B4] = 0x77C8D018D3AB480Cul;
        _bishopMagicMultipliers[(int)Square.C4] = 0x4DA646B802700335ul;
        _bishopMagicMultipliers[(int)Square.D4] = 0xC396F00821040400ul;
        _bishopMagicMultipliers[(int)Square.E4] = 0x5E380F6C00094100ul;
        _bishopMagicMultipliers[(int)Square.F4] = 0xC7F1070E006B01C7ul;
        _bishopMagicMultipliers[(int)Square.G4] = 0x23D624090351E804ul;
        _bishopMagicMultipliers[(int)Square.H4] = 0x612C009C81A26417ul;

        _bishopMagicMultipliers[(int)Square.A3] = 0x908FD6588E66B93Bul;
        _bishopMagicMultipliers[(int)Square.B3] = 0x5FF64718287A206Aul;
        _bishopMagicMultipliers[(int)Square.C3] = 0xB5CFFE3603027A08ul;
        _bishopMagicMultipliers[(int)Square.D3] = 0x650499E013066802ul;
        _bishopMagicMultipliers[(int)Square.E3] = 0xCFB54C270C017E00ul;
        _bishopMagicMultipliers[(int)Square.F3] = 0x4D200F8502C2FA00ul;
        _bishopMagicMultipliers[(int)Square.G3] = 0xE2FF7E66968AFF45ul;
        _bishopMagicMultipliers[(int)Square.H3] = 0xFEF81209CA03ED44ul;

        _bishopMagicMultipliers[(int)Square.A2] = 0x3797F367D90EC167ul;
        _bishopMagicMultipliers[(int)Square.B2] = 0x508BFCF242F40AB4ul;
        _bishopMagicMultipliers[(int)Square.C2] = 0xAF9A420642383663ul;
        _bishopMagicMultipliers[(int)Square.D2] = 0xE56EE992E1881FBDul;
        _bishopMagicMultipliers[(int)Square.E2] = 0x4DC8E4F05F37B3B2ul;
        _bishopMagicMultipliers[(int)Square.F2] = 0xB4A1A13E8E72035Aul;
        _bishopMagicMultipliers[(int)Square.G2] = 0x2FC1B20C04078D0Eul;
        _bishopMagicMultipliers[(int)Square.H2] = 0xB4BFBB9B79729264ul;

        _bishopMagicMultipliers[(int)Square.A1] = 0x8FDFFFCF3CA21D69ul;
        _bishopMagicMultipliers[(int)Square.B1] = 0x2EA5976CA801EFB9ul;
        _bishopMagicMultipliers[(int)Square.C1] = 0x89AC2287F5F3500Cul;
        _bishopMagicMultipliers[(int)Square.D1] = 0x7EA6599134840435ul;
        _bishopMagicMultipliers[(int)Square.E1] = 0x9F49970A3206660Aul;
        _bishopMagicMultipliers[(int)Square.F1] = 0x22F11FFF06906D03ul;
        _bishopMagicMultipliers[(int)Square.G1] = 0xEBC4FFAC8FD9EE0Ful;
        _bishopMagicMultipliers[(int)Square.H1] = 0x267FA4B9D2C59BDCul;

        _rookMagicMultipliers[(int)Square.A8] = 0xD9800180B3400524ul;
        _rookMagicMultipliers[(int)Square.B8] = 0X3FD80075FFEBFFFFul;
        _rookMagicMultipliers[(int)Square.C8] = 0X4010000DF6F6FFFEul;
        _rookMagicMultipliers[(int)Square.D8] = 0X0050001FAFFAFFFFul;
        _rookMagicMultipliers[(int)Square.E8] = 0X0050028004FFFFB0ul;
        _rookMagicMultipliers[(int)Square.F8] = 0X7F600280089FFFF1ul;
        _rookMagicMultipliers[(int)Square.G8] = 0X7F5000B0029FFFFCul;
        _rookMagicMultipliers[(int)Square.H8] = 0X5B58004848A7FFFAul;

        _rookMagicMultipliers[(int)Square.A7] = 0xFD0F800289C00061ul;
        _rookMagicMultipliers[(int)Square.B7] = 0x000050007F13FFFFul;
        _rookMagicMultipliers[(int)Square.C7] = 0x007FA0006013FFFFul;
        _rookMagicMultipliers[(int)Square.D7] = 0x0022004128102200ul;
        _rookMagicMultipliers[(int)Square.E7] = 0x000200081201200Cul;
        _rookMagicMultipliers[(int)Square.F7] = 0x202A001048460004ul;
        _rookMagicMultipliers[(int)Square.G7] = 0x0081000100420004ul;
        _rookMagicMultipliers[(int)Square.H7] = 0x4000800380004500ul;

        _rookMagicMultipliers[(int)Square.A6] = 0x0000208002904001ul;
        _rookMagicMultipliers[(int)Square.B6] = 0x0090004040026008ul;
        _rookMagicMultipliers[(int)Square.C6] = 0x0208808010002001ul;
        _rookMagicMultipliers[(int)Square.D6] = 0x2002020020704940ul;
        _rookMagicMultipliers[(int)Square.E6] = 0x8048010008110005ul;
        _rookMagicMultipliers[(int)Square.F6] = 0x6820808004002200ul;
        _rookMagicMultipliers[(int)Square.G6] = 0x0A80040008023011ul;
        _rookMagicMultipliers[(int)Square.H6] = 0x00B1460000811044ul;

        _rookMagicMultipliers[(int)Square.A5] = 0x4204400080008EA0ul;
        _rookMagicMultipliers[(int)Square.B5] = 0xB002400180200184ul;
        _rookMagicMultipliers[(int)Square.C5] = 0x2020200080100380ul;
        _rookMagicMultipliers[(int)Square.D5] = 0x0010080080100080ul;
        _rookMagicMultipliers[(int)Square.E5] = 0x2204080080800400ul;
        _rookMagicMultipliers[(int)Square.F5] = 0x0000A40080360080ul;
        _rookMagicMultipliers[(int)Square.G5] = 0x02040604002810B1ul;
        _rookMagicMultipliers[(int)Square.H5] = 0x008C218600004104ul;

        _rookMagicMultipliers[(int)Square.A4] = 0x8180004000402000ul;
        _rookMagicMultipliers[(int)Square.B4] = 0x488C402000401001ul;
        _rookMagicMultipliers[(int)Square.C4] = 0x4018A00080801004ul;
        _rookMagicMultipliers[(int)Square.D4] = 0x1230002105001008ul;
        _rookMagicMultipliers[(int)Square.E4] = 0x8904800800800400ul;
        _rookMagicMultipliers[(int)Square.F4] = 0x0042000C42003810ul;
        _rookMagicMultipliers[(int)Square.G4] = 0x008408110400B012ul;
        _rookMagicMultipliers[(int)Square.H4] = 0x0018086182000401ul;

        _rookMagicMultipliers[(int)Square.A3] = 0x2240088020C28000ul;
        _rookMagicMultipliers[(int)Square.B3] = 0x001001201040C004ul;
        _rookMagicMultipliers[(int)Square.C3] = 0x0A02008010420020ul;
        _rookMagicMultipliers[(int)Square.D3] = 0x0010003009010060ul;
        _rookMagicMultipliers[(int)Square.E3] = 0x0004008008008014ul;
        _rookMagicMultipliers[(int)Square.F3] = 0x0080020004008080ul;
        _rookMagicMultipliers[(int)Square.G3] = 0x0282020001008080ul;
        _rookMagicMultipliers[(int)Square.H3] = 0x50000181204A0004ul;

        _rookMagicMultipliers[(int)Square.A2] = 0x48FFFE99FECFAA00ul;
        _rookMagicMultipliers[(int)Square.B2] = 0x48FFFE99FECFAA00ul;
        _rookMagicMultipliers[(int)Square.C2] = 0x497FFFADFF9C2E00ul;
        _rookMagicMultipliers[(int)Square.D2] = 0x613FFFDDFFCE9200ul;
        _rookMagicMultipliers[(int)Square.E2] = 0xFFFFFFE9FFE7CE00ul;
        _rookMagicMultipliers[(int)Square.F2] = 0xFFFFFFF5FFF3E600ul;
        _rookMagicMultipliers[(int)Square.G2] = 0x0010301802830400ul;
        _rookMagicMultipliers[(int)Square.H2] = 0x510FFFF5F63C96A0ul;

        _rookMagicMultipliers[(int)Square.A1] = 0xEBFFFFB9FF9FC526ul;
        _rookMagicMultipliers[(int)Square.B1] = 0x61FFFEDDFEEDAEAEul;
        _rookMagicMultipliers[(int)Square.C1] = 0x53BFFFEDFFDEB1A2ul;
        _rookMagicMultipliers[(int)Square.D1] = 0x127FFFB9FFDFB5F6ul;
        _rookMagicMultipliers[(int)Square.E1] = 0x411FFFDDFFDBF4D6ul;
        _rookMagicMultipliers[(int)Square.F1] = 0x0801000804000603ul;
        _rookMagicMultipliers[(int)Square.G1] = 0x0003FFEF27EEBE74ul;
        _rookMagicMultipliers[(int)Square.H1] = 0x7645FFFECBFEA79Eul;

        FindMagicMultipliers(ColorlessPiece.Bishop);
        FindMagicMultipliers(ColorlessPiece.Rook);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetBishopMovesMask(Square square, ulong occupancy)
    {
        var relevantOccupancy = occupancy & _bishopRelevantOccupancyMasks[(int)square];
        var magicIndex = GetMagicIndex(relevantOccupancy, _bishopMagicMultipliers[(int)square], _bishopShifts[(int)square]);
        return _bishopMoveMasks[(int)square][magicIndex];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetRookMovesMask(Square square, ulong occupancy)
    {
        var relevantOccupancy = occupancy & _rookRelevantOccupancyMasks[(int)square];
        var magicIndex = GetMagicIndex(relevantOccupancy, _rookMagicMultipliers[(int)square], _rookShifts[(int)square]);
        return _rookMoveMasks[(int)square][magicIndex];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetMagicIndex(ulong occupancy, ulong magicMultiplier, int shift) => (int)((occupancy * magicMultiplier) >> shift);


    public void FindMagicMultipliers(ColorlessPiece colorlessPiece, Delegates.WriteMessageLine writeMessageLine = null)
    {
        Direction[] directions;
        ulong[] unoccupiedMoveMasks;
        ulong[] relevantOccupancyMasks;
        ulong[] magicMultipliers;
        int[] shifts;
        ulong[][] moveMasks;
        // ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (colorlessPiece)
        {
            case ColorlessPiece.Bishop:
                directions = new[] {Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest};
                unoccupiedMoveMasks = Board.BishopMoveMasks;
                relevantOccupancyMasks = _bishopRelevantOccupancyMasks;
                magicMultipliers = _bishopMagicMultipliers;
                shifts = _bishopShifts;
                moveMasks = _bishopMoveMasks;
                break;
            case ColorlessPiece.Rook:
                directions = new[] {Direction.North, Direction.East, Direction.South, Direction.West};
                unoccupiedMoveMasks = Board.RookMoveMasks;
                relevantOccupancyMasks = _rookRelevantOccupancyMasks;
                magicMultipliers = _rookMagicMultipliers;
                shifts = _rookShifts;
                moveMasks = _rookMoveMasks;
                break;
            default:
                throw new ArgumentException($"{colorlessPiece} piece not supported.");
        }
        // ReSharper restore SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        // Generate moves mask on each square.
        var occupancyToMovesMask = new Dictionary<ulong, ulong>();
        var uniqueMovesMasks = new HashSet<ulong>();
        for (var square = Square.A8; square < Square.Illegal; square++)
        {
            occupancyToMovesMask.Clear();
            uniqueMovesMasks.Clear();
            var moveDestinations = unoccupiedMoveMasks[(int)square];
            var relevantMoveDestinations = moveDestinations & relevantOccupancyMasks[(int)square];
            var uniqueOccupancies = (int)Math.Pow(2, Bitwise.CountSetBits(relevantMoveDestinations));
            occupancyToMovesMask.EnsureCapacity(uniqueOccupancies);
            // Generate moves mask for every permutation of occupancy of the relevant destination squares.
            var occupancyPermutations = Bitwise.GetAllPermutations(relevantMoveDestinations);
            foreach (var occupancy in occupancyPermutations)
            {
                var movesMask = Board.CreateMoveDestinationsMask(square, occupancy, directions);
                occupancyToMovesMask.Add(occupancy, movesMask);
                if (!uniqueMovesMasks.Contains(movesMask)) uniqueMovesMasks.Add(movesMask);
            }
            Debug.Assert(occupancyToMovesMask.Count == uniqueOccupancies);
            // Determine bit shift that produces number >= unique occupancies.
            // A stricter condition is number >= unique moves but this requires more computing time to find magic multipliers.
            var shift = 64 - (int)Math.Ceiling(Math.Log(uniqueOccupancies, 2d));
            shifts[(int)square] = shift;
            var magicMultiplier = magicMultipliers[(int)square];
            var knownMagicMultiplier = magicMultiplier == 0 ? null : (ulong?) magicMultiplier;
            (magicMultipliers[(int)square], moveMasks[(int)square]) = FindMagicMultiplier(occupancyToMovesMask, shift, knownMagicMultiplier);
            writeMessageLine?.Invoke($"{Board.SquareLocations[(int)square],6}  {PieceHelper.GetName(colorlessPiece),6}  {shift,5}  {occupancyToMovesMask.Count,18}  {uniqueMovesMasks.Count,12}  {magicMultipliers[(int)square],16:X16}");
        }
    }


    public static ulong GetRelevantOccupancy(Square square, bool fileRankSlidingPiece)
    {
        if ((Board.SquareMasks[(int)square] & Board.EdgeSquaresMask) == 0) return ~Board.EdgeSquaresMask;
        // Square is on edge of board.
        if (!fileRankSlidingPiece) return ~Board.EdgeSquaresMask;
        // Piece can slide along file or rank.
        ulong occupancy;
        var file = Board.Files[(int)square];
        var rank = Board.Ranks[(int)Color.White][(int)square];
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
                        return ~Board.SquareMasks[(int)Square.A8] & ~Board.SquareMasks[(int)Square.H1];
                    case 7:
                        // Piece is on A8 square.
                        // Occupancy of most distant squares does not affect pseudo-legal moves.
                        return ~Board.SquareMasks[(int)Square.A1] & ~Board.SquareMasks[(int)Square.H8];
                    default:
                        // Piece is not on edge rank.
                        // Occupancy of edge ranks and opposite edge file does not affect pseudo-legal moves.
                        return ~Board.WhiteRankMasks[0] & ~Board.WhiteRankMasks[7] & ~Board.FileMasks[7];
                }
            case 7:
                // Piece is on Easternmost edge file.
                switch (rank)
                {
                    case 0:
                        // Piece is on H1 square.
                        // Occupancy of most distant squares does not affect pseudo-legal moves.
                        return ~Board.SquareMasks[(int)Square.A1] & ~Board.SquareMasks[(int)Square.H8];
                    case 7:
                        // Piece is on H8 square.
                        // Occupancy of most distant squares does not affect pseudo-legal moves.
                        return ~Board.SquareMasks[(int)Square.A8] & ~Board.SquareMasks[(int)Square.H1];
                    default:
                        // Piece is not on edge rank.
                        // Occupancy of edge ranks and opposite edge file does not affect pseudo-legal moves.
                        return ~Board.WhiteRankMasks[0] & ~Board.WhiteRankMasks[7] & ~Board.FileMasks[0];
                }
            default:
                // Piece is not on edge file.
                // Occupancy of edge files does not affect pseudo-legal moves.
                occupancy = ~Board.FileMasks[0] & ~Board.FileMasks[7];
                break;
        }
        // Piece is not on a corner square (handled in above code).
        switch (rank)
        {
            case 0:
                // Piece is on Southernmost edge rank.
                // Occupancy of opposite rank does not affect pseudo-legal moves.
                return occupancy & ~Board.WhiteRankMasks[7];
            case 7:
                // Piece is on Northernmost edge rank.
                // Occupancy of opposite rank does not affect pseudo-legal moves.
                return occupancy & ~Board.WhiteRankMasks[0];
            default:
                // Piece is not on edge rank.
                // Occupancy of edge ranks does not affect pseudo-legal moves.
                return occupancy & ~Board.WhiteRankMasks[0] & ~Board.WhiteRankMasks[7];
        }
        // ReSharper restore ConvertSwitchStatementToSwitchExpression
    }


    private static (ulong MagicMultiplier, ulong[] MovesMasks) FindMagicMultiplier(Dictionary<ulong, ulong> occupancyToMovesMask, int shift, ulong? knownMagicMultiplier)
    {
        var indexBits = 64 - shift;
        var indexLength = (int)Math.Pow(2d, indexBits);
        var movesMasks = new ulong[indexLength];
        var occupancies = new List<ulong>(occupancyToMovesMask.Keys);
        NextMagicMultiplier:
        var magicMultiplier = knownMagicMultiplier ?? SafeRandom.NextULong();
        // Clear moves masks.
        for (var maskIndex = 0; maskIndex < movesMasks.Length; maskIndex++) movesMasks[maskIndex] = 0;
        for (var occupancyIndex = 0; occupancyIndex < occupancies.Count; occupancyIndex++)
        {
            var occupancy = occupancies[occupancyIndex];
            var magicIndex = GetMagicIndex(occupancy, magicMultiplier, shift);
            var movesMask = movesMasks[magicIndex];
            if (movesMask == 0) movesMasks[magicIndex] = occupancyToMovesMask[occupancy]; // Moves mask not yet added to unique moves array.
            else if (movesMask != occupancyToMovesMask[occupancy]) goto NextMagicMultiplier; // Moves mask already added to unique moves array but mask is incorrect.
        }
        // Found magic multiplier that maps to correct moves index for all occupancies.
        return (magicMultiplier, movesMasks);
    }
}