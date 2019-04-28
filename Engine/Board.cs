// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class Board
    {
        public const string StartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        
        public static readonly int[] Files;
        public static readonly int[] WhiteRanks;
        public static readonly int[] BlackRanks;
        public static readonly int[] UpDiagonals;
        public static readonly int[] DownDiagonals;
        public static readonly int[] CentralSquares;
        public static readonly int[] CornerSquares;
        public static readonly bool[] LightSquares;
        public static readonly int[] LightCornerSquares;
        public static readonly int[] DarkCornerSquares;
        public static readonly int[][] SquareDistances;
        public static readonly int[] DistanceToCentralSquares;
        public static readonly int[] DistanceToNearestCorner;
        public static readonly int[] DistanceToNearestLightCorner;
        public static readonly int[] DistanceToNearestDarkCorner;
        public static readonly string[] SquareLocations;
        public static readonly ulong[] SquareMasks;
        public static readonly ulong[] SquareUnmasks;
        public static readonly ulong[] FileMasks;
        public static readonly ulong[] RankMasks;
        public static readonly ulong[] UpDiagonalMasks;
        public static readonly ulong[] DownDiagonalMasks;
        public static readonly ulong EdgeSquareMask;
        public static readonly ulong WhiteCastleQEmptySquaresMask;
        public static readonly ulong WhiteCastleQAttackedSquareMask;
        public static readonly ulong WhiteCastleKEmptySquaresMask;
        public static readonly ulong WhiteCastleKAttackedSquareMask;
        public static readonly ulong BlackCastleQEmptySquaresMask;
        public static readonly ulong BlackCastleQAttackedSquareMask;
        public static readonly ulong BlackCastleKEmptySquaresMask;
        public static readonly ulong BlackCastleKAttackedSquareMask;
        public static readonly ulong AllSquaresMask;
        public readonly ulong[] WhitePawnMoveMasks;
        public readonly ulong[] WhitePawnDoubleMoveMasks;
        public readonly ulong[] WhitePawnAttackMasks;
        public readonly ulong[] BlackPawnMoveMasks;
        public readonly ulong[] BlackPawnDoubleMoveMasks;
        public readonly ulong[] BlackPawnAttackMasks;
        public readonly ulong[] KnightMoveMasks;
        public readonly ulong[] BishopMoveMasks;
        public readonly ulong[] RookMoveMasks;
        public readonly ulong[] KingMoveMasks;
        public readonly int[] EnPassantTargetSquares;
        public readonly int[] EnPassantVictimSquares;
        public readonly ulong[] EnPassantAttackerMasks;
        public PrecalculatedMoves PrecalculatedMoves;
        public long Nodes;
        public long NodesInfoUpdate;
        public long NodesExamineTime;
        private const int _maxPositions = 1024;
        private readonly int[][] _neighborSquares;
        private readonly ulong[] _whitePassedPawnMasks;
        private readonly ulong[] _whiteFreePawnMasks;
        private readonly ulong[] _blackPassedPawnMasks;
        private readonly ulong[] _blackFreePawnMasks;
        private readonly ulong _piecesSquaresInitialKey;
        private readonly ulong[][] _pieceSquareKeys;
        private readonly ulong[] _sideToMoveKeys;
        private readonly ulong[] _castlingKeys;
        private readonly ulong[] _enPassantKeys;
        private readonly Position[] _positions;
        private readonly Delegates.WriteMessageLine _writeMessageLine;
        private int _positionIndex;


        public Position PreviousPosition => _positionIndex > 0 ? _positions[_positionIndex - 1] : null;


        public Position CurrentPosition => _positions[_positionIndex];


        public Position NextPosition => _positions[_positionIndex + 1];


        static Board()
        {
            // The chessboard is represented as an array of 64 squares, shown here as an 8 x 8 grid of square indices.
            // A1 = square index 56.  A8 = square index 00.  H8 = square index 07.  H1 = square index 63.

            //    A  B  C  D  E  F  G  H
            // 7  00 01 02 03 04 05 06 07  7
            // 6  08 09 10 11 12 13 14 15  6
            // 5  16 17 18 19 20 21 22 23  5
            // 4  24 25 26 27 28 29 30 31  4
            // 3  32 33 34 35 36 37 38 39  3
            // 2  40 41 42 43 44 45 46 47  2
            // 1  48 49 50 51 52 53 54 55  1
            // 0  56 57 58 59 60 61 62 63  0
            //    A  B  C  D  E  F  G  H

            // Files are indexed West to East from 0 to 7 (white's perspective).
            Files = new[]
            {
                0, 1, 2, 3, 4, 5, 6, 7,
                0, 1, 2, 3, 4, 5, 6, 7,
                0, 1, 2, 3, 4, 5, 6, 7,
                0, 1, 2, 3, 4, 5, 6, 7,
                0, 1, 2, 3, 4, 5, 6, 7,
                0, 1, 2, 3, 4, 5, 6, 7,
                0, 1, 2, 3, 4, 5, 6, 7,
                0, 1, 2, 3, 4, 5, 6, 7
            };
            // Ranks are indexed South to North from 0 to 7 (white's perspective).
            WhiteRanks = new[]
            {
                7, 7, 7, 7, 7, 7, 7, 7,
                6, 6, 6, 6, 6, 6, 6, 6,
                5, 5, 5, 5, 5, 5, 5, 5,
                4, 4, 4, 4, 4, 4, 4, 4,
                3, 3, 3, 3, 3, 3, 3, 3,
                2, 2, 2, 2, 2, 2, 2, 2,
                1, 1, 1, 1, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 0, 0
            };
            BlackRanks = new[]
            {
                0, 0, 0, 0, 0, 0, 0, 0,
                1, 1, 1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2, 2,
                3, 3, 3, 3, 3, 3, 3, 3,
                4, 4, 4, 4, 4, 4, 4, 4,
                5, 5, 5, 5, 5, 5, 5, 5,
                6, 6, 6, 6, 6, 6, 6, 6,
                7, 7, 7, 7, 7, 7, 7, 7
            };
            // Up diagonals are indexed Northwest to Southeast from 0 to 14.
            UpDiagonals = new[]
            {
                00, 01, 02, 03, 04, 05, 06, 07,
                01, 02, 03, 04, 05, 06, 07, 08,
                02, 03, 04, 05, 06, 07, 08, 09,
                03, 04, 05, 06, 07, 08, 09, 10,
                04, 05, 06, 07, 08, 09, 10, 11,
                05, 06, 07, 08, 09, 10, 11, 12,
                06, 07, 08, 09, 10, 11, 12, 13,
                07, 08, 09, 10, 11, 12, 13, 14
            };
            // Down diagonals are indexed SouthWest to Northeast from 0 to 14.
            DownDiagonals = new[]
            {
                07, 08, 09, 10, 11, 12, 13, 14,
                06, 07, 08, 09, 10, 11, 12, 13,
                05, 06, 07, 08, 09, 10, 11, 12,
                04, 05, 06, 07, 08, 09, 10, 11,
                03, 04, 05, 06, 07, 08, 09, 10,
                02, 03, 04, 05, 06, 07, 08, 09,
                01, 02, 03, 04, 05, 06, 07, 08,
                00, 01, 02, 03, 04, 05, 06, 07
            };
            CentralSquares = new[] { Square.d4, Square.e4, Square.d5, Square.e5 };
            CornerSquares = new[] { Square.a8, Square.h8, Square.a1, Square.h1 };
            LightSquares = new[]
            {
                true, false, true, false, true, false, true, false,
                false, true, false, true, false, true, false, true,
                true, false, true, false, true, false, true, false,
                false, true, false, true, false, true, false, true,
                true, false, true, false, true, false, true, false,
                false, true, false, true, false, true, false, true,
                true, false, true, false, true, false, true, false,
                false, true, false, true, false, true, false, true
            };
            LightCornerSquares = new[] { Square.a8, Square.h1 };
            DarkCornerSquares = new[] { Square.a1, Square.h8 };
            // Determine distances between squares.
            SquareDistances = new int[64][];
            for (int square1 = 0; square1 < 64; square1++)
            {
                SquareDistances[square1] = new int[64];
                for (int square2 = 0; square2 < 64; square2++)
                {
                    int fileDistance = Math.Abs(Files[square1] - Files[square2]);
                    int rankDistance = Math.Abs(WhiteRanks[square1] - WhiteRanks[square2]);
                    SquareDistances[square1][square2] = Math.Max(fileDistance, rankDistance);
                }
            }
            // Determine distances to central and nearest corner squares.
            DistanceToCentralSquares = new int[64];
            DistanceToNearestCorner = new int[64];
            DistanceToNearestLightCorner = new int[64];
            DistanceToNearestDarkCorner = new int[64];
            for (int square = 0; square < 64; square++)
            {
                DistanceToCentralSquares[square] = GetShortestDistance(square, CentralSquares);
                DistanceToNearestCorner[square] = GetShortestDistance(square, CornerSquares);
                DistanceToNearestLightCorner[square] = GetShortestDistance(square, LightCornerSquares);
                DistanceToNearestDarkCorner[square] = GetShortestDistance(square, DarkCornerSquares);
            }
            SquareLocations = new[]
            {
                "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8",
                "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
                "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
                "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
                "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
                "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
                "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
                "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1"
            };
            
            // Create square, file, rank, diagonal, and edge masks.
            SquareMasks = new ulong[64];
            SquareUnmasks = new ulong[64];
            for (int square = 0; square < 64; square++)
            {
                SquareMasks[square] = Bitwise.CreateULongMask(square);
                SquareUnmasks[square] = Bitwise.CreateULongUnmask(square);
            }
            FileMasks = new ulong[8];
            for (int file = 0; file < 8; file++)
            {
                FileMasks[file] = 0;
                for (int rank = 0; rank < 8; rank++)
                {
                    int square = GetSquare(file, rank);
                    FileMasks[file] |= Bitwise.CreateULongMask(square);
                }
            }
            RankMasks = new ulong[8];
            for (int rank = 0; rank < 8; rank++)
            {
                RankMasks[rank] = 0;
                for (int file = 0; file < 8; file++)
                {
                    int square = GetSquare(file, rank);
                    RankMasks[rank] |= Bitwise.CreateULongMask(square);
                }
            }
            UpDiagonalMasks = new ulong[15];
            UpDiagonalMasks[00] = Bitwise.CreateULongMask(new[] {00});
            UpDiagonalMasks[01] = Bitwise.CreateULongMask(new[] {08, 01});
            UpDiagonalMasks[02] = Bitwise.CreateULongMask(new[] {16, 09, 02});
            UpDiagonalMasks[03] = Bitwise.CreateULongMask(new[] {24, 17, 10, 03});
            UpDiagonalMasks[04] = Bitwise.CreateULongMask(new[] {32, 25, 18, 11, 04});
            UpDiagonalMasks[05] = Bitwise.CreateULongMask(new[] {40, 33, 26, 19, 12, 05});
            UpDiagonalMasks[06] = Bitwise.CreateULongMask(new[] {48, 41, 34, 27, 20, 13, 06});
            UpDiagonalMasks[07] = Bitwise.CreateULongMask(new[] {56, 49, 42, 35, 28, 21, 14, 07});
            UpDiagonalMasks[08] = Bitwise.CreateULongMask(new[] {57, 50, 43, 36, 29, 22, 15});
            UpDiagonalMasks[09] = Bitwise.CreateULongMask(new[] {58, 51, 44, 37, 30, 23});
            UpDiagonalMasks[10] = Bitwise.CreateULongMask(new[] {59, 52, 45, 38, 31});
            UpDiagonalMasks[11] = Bitwise.CreateULongMask(new[] {60, 53, 46, 39});
            UpDiagonalMasks[12] = Bitwise.CreateULongMask(new[] {61, 54, 47});
            UpDiagonalMasks[13] = Bitwise.CreateULongMask(new[] {62, 55});
            UpDiagonalMasks[14] = Bitwise.CreateULongMask(new[] {63});
            DownDiagonalMasks = new ulong[15];
            DownDiagonalMasks[00] = Bitwise.CreateULongMask(new[] {56});
            DownDiagonalMasks[01] = Bitwise.CreateULongMask(new[] {48, 57});
            DownDiagonalMasks[02] = Bitwise.CreateULongMask(new[] {40, 49, 58});
            DownDiagonalMasks[03] = Bitwise.CreateULongMask(new[] {32, 41, 50, 59});
            DownDiagonalMasks[04] = Bitwise.CreateULongMask(new[] {24, 33, 42, 51, 60});
            DownDiagonalMasks[05] = Bitwise.CreateULongMask(new[] {16, 25, 34, 43, 52, 61});
            DownDiagonalMasks[06] = Bitwise.CreateULongMask(new[] {08, 17, 26, 35, 44, 53, 62});
            DownDiagonalMasks[07] = Bitwise.CreateULongMask(new[] {00, 09, 18, 27, 36, 45, 54, 63});
            DownDiagonalMasks[08] = Bitwise.CreateULongMask(new[] {01, 10, 19, 28, 37, 46, 55});
            DownDiagonalMasks[09] = Bitwise.CreateULongMask(new[] {02, 11, 20, 29, 38, 47});
            DownDiagonalMasks[10] = Bitwise.CreateULongMask(new[] {03, 12, 21, 30, 39});
            DownDiagonalMasks[11] = Bitwise.CreateULongMask(new[] {04, 13, 22, 31});
            DownDiagonalMasks[12] = Bitwise.CreateULongMask(new[] {05, 14, 23});
            DownDiagonalMasks[13] = Bitwise.CreateULongMask(new[] {06, 15});
            DownDiagonalMasks[14] = Bitwise.CreateULongMask(new[] {07});
            EdgeSquareMask = FileMasks[0] | RankMasks[7] | FileMasks[7] | RankMasks[0];
            AllSquaresMask = Bitwise.CreateULongMask(0, 63);
            // Create castling masks.
            WhiteCastleQEmptySquaresMask = Bitwise.CreateULongMask(new[] {Square.b1, Square.c1, Square.d1});
            WhiteCastleQAttackedSquareMask = Bitwise.CreateULongMask(Square.d1);
            WhiteCastleKEmptySquaresMask = Bitwise.CreateULongMask(new[] { Square.f1, Square.g1 });
            WhiteCastleKAttackedSquareMask = Bitwise.CreateULongMask(Square.f1);
            BlackCastleQEmptySquaresMask = Bitwise.CreateULongMask(new[] { Square.b8, Square.c8, Square.d8 });
            BlackCastleQAttackedSquareMask = Bitwise.CreateULongMask(Square.d8);
            BlackCastleKEmptySquaresMask = Bitwise.CreateULongMask(new[] { Square.f8, Square.g8 });
            BlackCastleKAttackedSquareMask = Bitwise.CreateULongMask(Square.f8);
        }


        public Board(Delegates.WriteMessageLine WriteMessageLine)
        {
            _writeMessageLine = WriteMessageLine;

            // Use a 12 x 12 grid of square indices to calculate square legality.

            //  000, 001,   002, 003, 004, 005, 006, 007, 008, 009,   010, 011,
            //  012, 013,   014, 015, 016, 017, 018, 019, 020, 021,   022, 023,

            //  024, 025,   026, 027, 028, 029, 030, 031, 032, 033,   034, 035,
            //  036, 037,   038, 039, 040, 041, 042, 043, 044, 045,   046, 047,
            //  048, 049,   050, 051, 052, 053, 054, 055, 056, 057,   058, 059,
            //  060, 061,   062, 063, 064, 065, 066, 067, 068, 069,   070, 071,
            //  072, 073,   074, 075, 076, 077, 078, 079, 080, 081,   082, 083,
            //  084, 085,   086, 087, 088, 089, 090, 091, 092, 093,   094, 095,
            //  096, 097,   098, 099, 100, 101, 102, 103, 104, 105,   106, 107,
            //  108, 109,   110, 111, 112, 113, 114, 115, 116, 117,   118, 119,

            //  120, 121,   122, 123, 124, 125, 126, 127, 128, 129,   130, 131,
            //  132, 133,   134, 135, 136, 137, 138, 139, 140, 141,   142, 143 

            // Create direction offsets for 12 x 12 grid.
            int[] directionOffsets1212 = CreateDirectionOffsets1212();
            // Map 12 x 12 square indices to 8 x 8 square indices.
            int[] squareIndices1212To88 = MapSquareIndices1212To88();
            // Create neighbor squares.
            _neighborSquares = CreateNeighborSquares(directionOffsets1212, squareIndices1212To88);
            // Create move masks.
            WhitePawnMoveMasks = CreateWhitePawnMoveMasks();
            WhitePawnDoubleMoveMasks = CreateWhitePawnDoubleMoveMasks();
            WhitePawnAttackMasks = CreateWhitePawnAttackMasks();
            BlackPawnMoveMasks = CreateBlackPawnMoveMasks();
            BlackPawnDoubleMoveMasks = CreateBlackPawnDoubleMoveMasks();
            BlackPawnAttackMasks = CreateBlackPawnAttackMasks();
            KnightMoveMasks = CreateKnightMoveMasks();
            BishopMoveMasks = CreateBishopMoveMasks();
            RookMoveMasks = CreateRookMoveMasks();
            KingMoveMasks = CreateKingMoveMasks();
            EnPassantTargetSquares = new int[64];
            EnPassantVictimSquares = new int[64];
            EnPassantAttackerMasks = new ulong[64];
            for (int file = 0; file < 8; file++)
            {
                // White takes black pawn en passant
                int toSquare = GetSquare(file, 4);
                int targetSquare = _neighborSquares[toSquare][(int)Direction.North];
                EnPassantVictimSquares[targetSquare] = _neighborSquares[targetSquare][(int)Direction.South];
                int westAttackerSquare = _neighborSquares[targetSquare][(int)Direction.SouthWest];
                int eastAttackerSquare = _neighborSquares[targetSquare][(int)Direction.SouthEast];
                EnPassantTargetSquares[toSquare] = targetSquare;
                ulong attackerMask = 0;
                if (westAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(westAttackerSquare);
                if (eastAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(eastAttackerSquare);
                EnPassantAttackerMasks[targetSquare] = attackerMask;
                // Black takes white pawn en passant
                toSquare = GetSquare(file, 3);
                targetSquare = _neighborSquares[toSquare][(int)Direction.South];
                EnPassantVictimSquares[targetSquare] = _neighborSquares[targetSquare][(int)Direction.North];
                westAttackerSquare = _neighborSquares[targetSquare][(int)Direction.NorthWest];
                eastAttackerSquare = _neighborSquares[targetSquare][(int)Direction.NorthEast];
                EnPassantTargetSquares[toSquare] = targetSquare;
                attackerMask = 0;
                if (westAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(westAttackerSquare);
                if (eastAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(eastAttackerSquare);
                EnPassantAttackerMasks[targetSquare] = attackerMask;
            }
            // Create passed pawn and free pawn masks.
            _whitePassedPawnMasks = CreateWhitePassedPawnMasks();
            _whiteFreePawnMasks = CreateWhiteFreePawnMasks();
            _blackPassedPawnMasks = CreateBlackPassedPawnMasks();
            _blackFreePawnMasks = CreateBlackFreePawnMasks();
            // Create positions.
            _positions = new Position[_maxPositions];
            for (int positionIndex = 0; positionIndex < _maxPositions; positionIndex++) _positions[positionIndex] = new Position(this);
            // Create Zobrist position keys.
            _piecesSquaresInitialKey = SafeRandom.NextULong();
            _pieceSquareKeys = new ulong[13][];
            for (int piece = 0; piece < 13; piece++)
            {
                _pieceSquareKeys[piece] = new ulong[64];
                for (int square = 0; square < 64; square++) _pieceSquareKeys[piece][square] = SafeRandom.NextULong();
            }
            _sideToMoveKeys = new[] { SafeRandom.NextULong(), SafeRandom.NextULong() };
            _castlingKeys = new ulong[16];
            {
                for (int castlingRights = 0; castlingRights < 16; castlingRights++) _castlingKeys[castlingRights] = SafeRandom.NextULong();
            }
            _enPassantKeys = new ulong[64];
            for (int square = 0; square < 64; square++) _enPassantKeys[square] = SafeRandom.NextULong();
            _piecesSquaresInitialKey = SafeRandom.NextULong();
            // Set nodes.
            Nodes = 0;
            NodesInfoUpdate = UciStream.NodesInfoInterval;
        }


        private static int[] CreateDirectionOffsets1212()
        {
            int[] directionOffsets1212 = new int[17];
            directionOffsets1212[(int)Direction.North] = -12;
            directionOffsets1212[(int)Direction.NorthEast] = -11;
            directionOffsets1212[(int)Direction.East] = 1;
            directionOffsets1212[(int)Direction.SouthEast] = 13;
            directionOffsets1212[(int)Direction.South] = 12;
            directionOffsets1212[(int)Direction.SouthWest] = 11;
            directionOffsets1212[(int)Direction.West] = -1;
            directionOffsets1212[(int)Direction.NorthWest] = -13;
            directionOffsets1212[(int)Direction.North2East1] = -23;
            directionOffsets1212[(int)Direction.East2North1] = -10;
            directionOffsets1212[(int)Direction.East2South1] = 14;
            directionOffsets1212[(int)Direction.South2East1] = 25;
            directionOffsets1212[(int)Direction.South2West1] = 23;
            directionOffsets1212[(int)Direction.West2South1] = 10;
            directionOffsets1212[(int)Direction.West2North1] = -14;
            directionOffsets1212[(int)Direction.North2West1] = -25;
            return directionOffsets1212;
        }


        private static int[] MapSquareIndices1212To88()
        {
            int[] squareIndices1212To88 = new int[144];
            int square1212 = 0;
            int square88 = 0;
            for (int file = -2; file <= 9; file++)
                for (int rank = -2; rank <= 9; rank++)
                {
                    if (file >= 0 && file <= 7 && rank >= 0 && rank <= 7)
                    {
                        // Legal square
                        squareIndices1212To88[square1212] = square88;
                        square88++;
                    }
                    else squareIndices1212To88[square1212] = Square.Illegal; // Illegal square
                    square1212++;
                }
            return squareIndices1212To88;
        }


        private static int[][] CreateNeighborSquares(int[] DirectionOffsets1212, int[] SquareIndices1212To88)
        {
            int[][] neighborSquares = new int[64][];
            int square88;
            for (square88 = 0; square88 < 64; square88++) neighborSquares[square88] = new int[17];
            for (int square1212 = 0; square1212 < 144; square1212++)
            {
                square88 = SquareIndices1212To88[square1212];
                if (square88 != Square.Illegal)
                    for (int directionIndex = 1; directionIndex < 17; directionIndex++)
                    {
                        int directionOffset1212 = DirectionOffsets1212[directionIndex];
                        neighborSquares[square88][directionIndex] = SquareIndices1212To88[square1212 + directionOffset1212];
                    }
            }
            return neighborSquares;
        }


        private ulong[] CreateWhitePawnMoveMasks()
        {
            ulong[] moveMasks = new ulong[64];
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                int otherSquare = _neighborSquares[square][(int)Direction.North];
                if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                if (WhiteRanks[square] == 1)
                {
                    otherSquare = _neighborSquares[otherSquare][(int)Direction.North];
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private ulong[] CreateWhitePawnDoubleMoveMasks()
        {
            ulong[] moveMasks = new ulong[64];
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                if (WhiteRanks[square] == 1)
                {
                    int otherSquare = _neighborSquares[square][(int)Direction.North];
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private ulong[] CreateWhitePawnAttackMasks()
        {
            ulong[] moveMasks = new ulong[64];
            Direction[] directions = { Direction.NorthWest, Direction.NorthEast };
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    Direction direction = directions[directionIndex];
                    int otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private ulong[] CreateBlackPawnMoveMasks()
        {
            ulong[] moveMasks = new ulong[64];
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                int otherSquare = _neighborSquares[square][(int)Direction.South];
                if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                if (BlackRanks[square] == 1)
                {
                    otherSquare = _neighborSquares[otherSquare][(int)Direction.South];
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private ulong[] CreateBlackPawnDoubleMoveMasks()
        {
            ulong[] moveMasks = new ulong[64];
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                if (BlackRanks[square] == 1)
                {
                    int otherSquare = _neighborSquares[square][(int)Direction.South];
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private ulong[] CreateBlackPawnAttackMasks()
        {
            ulong[] moveMasks = new ulong[64];
            Direction[] directions = { Direction.SouthWest, Direction.SouthEast };
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    Direction direction = directions[directionIndex];
                    int otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private ulong[] CreateWhitePassedPawnMasks()
        {
            ulong[] masks = new ulong[64];
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                int[] startingSquares =
                {
                    _neighborSquares[square][(int) Direction.West],
                    square,
                    _neighborSquares[square][(int) Direction.East]
                };
                for (int index = 0; index < startingSquares.Length; index++)
                {
                    int otherSquare = startingSquares[index];
                    while (otherSquare != Square.Illegal)
                    {
                        otherSquare = _neighborSquares[otherSquare][(int)Direction.North];
                        if (otherSquare == Square.Illegal) break;
                        Bitwise.SetBit(ref mask, otherSquare);
                    }
                }
                masks[square] = mask;
            }
            return masks;
        }


        private ulong[] CreateWhiteFreePawnMasks()
        {
            ulong[] masks = new ulong[64];
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                int otherSquare = square;
                while (true)
                {
                    otherSquare = _neighborSquares[otherSquare][(int)Direction.North];
                    if (otherSquare == Square.Illegal) break;
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                masks[square] = mask;
            }
            return masks;
        }


        private ulong[] CreateBlackPassedPawnMasks()
        {
            ulong[] masks = new ulong[64];
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                int[] startingSquares =
                {
                    _neighborSquares[square][(int) Direction.West],
                    square,
                    _neighborSquares[square][(int) Direction.East]
                };
                for (int index = 0; index < startingSquares.Length; index++)
                {
                    int otherSquare = startingSquares[index];
                    while (otherSquare != Square.Illegal)
                    {
                        otherSquare = _neighborSquares[otherSquare][(int)Direction.South];
                        if (otherSquare == Square.Illegal) break;
                        Bitwise.SetBit(ref mask, otherSquare);
                    }
                }
                masks[square] = mask;
            }
            return masks;
        }


        private ulong[] CreateBlackFreePawnMasks()
        {
            ulong[] masks = new ulong[64];
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                int otherSquare = square;
                while (true)
                {
                    otherSquare = _neighborSquares[otherSquare][(int)Direction.South];
                    if (otherSquare == Square.Illegal) break;
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                masks[square] = mask;
            }
            return masks;
        }


        private ulong[] CreateKnightMoveMasks()
        {
            ulong[] attackMasks = new ulong[64];
            Direction[] directions = { Direction.North2East1, Direction.East2North1, Direction.East2South1, Direction.South2East1, Direction.South2West1, Direction.West2South1, Direction.West2North1, Direction.North2West1 };
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    Direction direction = directions[directionIndex];
                    int otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                attackMasks[square] = mask;
            }
            return attackMasks;
        }


        private ulong[] CreateBishopMoveMasks()
        {
            ulong[] moveMasks = new ulong[64];
            Direction[] directions = { Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest };
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    Direction direction = directions[directionIndex];
                    int otherSquare = square;
                    while (true)
                    {
                        otherSquare = _neighborSquares[otherSquare][(int)direction];
                        if (otherSquare == Square.Illegal) break;
                        Bitwise.SetBit(ref mask, otherSquare);
                    }
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private ulong[] CreateRookMoveMasks()
        {
            ulong[] moveMasks = new ulong[64];
            Direction[] directions = { Direction.North, Direction.East, Direction.South, Direction.West };
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    Direction direction = directions[directionIndex];
                    int otherSquare = square;
                    while (true)
                    {
                        otherSquare = _neighborSquares[otherSquare][(int)direction];
                        if (otherSquare == Square.Illegal) break;
                        Bitwise.SetBit(ref mask, otherSquare);
                    }
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private ulong[] CreateKingMoveMasks()
        {
            ulong[] attackMasks = new ulong[64];
            Direction[] directions = { Direction.North, Direction.NorthEast, Direction.East, Direction.SouthEast, Direction.South, Direction.SouthWest, Direction.West, Direction.NorthWest };
            for (int square = 0; square < 64; square++)
            {
                ulong mask = 0;
                for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    Direction direction = directions[directionIndex];
                    int otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                attackMasks[square] = mask;
            }
            return attackMasks;
        }


        public ulong CreateMoveDestinationsMask(int Square, ulong Occupancy, Direction[] Directions)
        {
            ulong moveDestinations = 0;
            for (int directionIndex = 0; directionIndex < Directions.Length; directionIndex++)
            {
                Direction direction = Directions[directionIndex];
                int otherSquare = Square;
                while (true)
                {
                    otherSquare = _neighborSquares[otherSquare][(int) direction];
                    if (otherSquare == Engine.Square.Illegal) break;
                    Bitwise.SetBit(ref moveDestinations, otherSquare);
                    if (Bitwise.IsBitSet(Occupancy, otherSquare)) break; // Square is occupied.
                }
            }
            return moveDestinations;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSquare(int File, int Rank)
        {
            Debug.Assert(File >= 0 && File < 8);
            Debug.Assert(Rank >= 0 && Rank < 8);
            return File + (7 - Rank) * 8;
        }


        public static int GetSquare(string Square)
        {
            Debug.Assert(Square.Length == 2, $"Square = {Square}");
            char fileChar = Square[0];
            char rankChar = Square[1];
            int file = fileChar - 97;
            int rank = rankChar - 49;
            Debug.Assert(file >= 0 && file < 8);
            Debug.Assert(rank >= 0 && rank < 8);
            return GetSquare(file, rank);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBlackSquare(int Square) => 63 - Square;


        private static int GetShortestDistance(int Square, int[] OtherSquares)
        {
            int shortestDistance = int.MaxValue;
            for (int index = 0; index < OtherSquares.Length; index++)
            {
                int otherSquare = OtherSquares[index];
                int distance = SquareDistances[Square][otherSquare];
                if (distance < shortestDistance) shortestDistance = distance;
            }
            return shortestDistance;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPassedPawn(int Square, bool White)
        {
            Debug.Assert(CurrentPosition.GetPiece(Square) == (White ? Piece.WhitePawn : Piece.BlackPawn));
            return White
                ? (_whitePassedPawnMasks[Square] & CurrentPosition.BlackPawns) == 0
                : (_blackPassedPawnMasks[Square] & CurrentPosition.WhitePawns) == 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFreePawn(int Square, bool White)
        {
            Debug.Assert(CurrentPosition.GetPiece(Square) == (White ? Piece.WhitePawn : Piece.BlackPawn));
            // TODO: Verify swap off score is positive.
            return White
                ? (_whiteFreePawnMasks[Square] & CurrentPosition.Occupancy) == 0
                : (_blackFreePawnMasks[Square] & CurrentPosition.Occupancy) == 0;
        }


        public void SetPosition(string Fen, bool PreserveMoveCount = false)
        {
            List<string> fen = Tokens.Parse(Fen, ' ', '"');
            if (fen.Count < 4) throw new ArgumentException($"FEN has only {fen.Count}  fields.");
            Reset(PreserveMoveCount);
            // Place pieces on board.
            List<string> fenPosition = Tokens.Parse(fen[0], '/', '"');
            int square = Square.a8;
            for (int fenPositionIndex = 0; fenPositionIndex < fenPosition.Count; fenPositionIndex++)
            {
                string rank = fenPosition[fenPositionIndex];
                for (int rankIndex = 0; rankIndex < rank.Length; rankIndex++)
                {
                    char piece = rank[rankIndex];
                    if (char.IsNumber(piece))
                    {
                        // Empty squares
                        int emptySquares = int.Parse(piece.ToString());
                        square += emptySquares - 1;
                    }
                    else AddPiece(Piece.ParseChar(piece), square);
                    square++;
                }
            }
            // Set side to move, castling rights, en passant square, half and full move counts.
            CurrentPosition.WhiteMove = fen[1].Equals("w");
            Castling.SetWhiteKingside(ref CurrentPosition.Castling, fen[2].IndexOf("K") > -1);
            Castling.SetWhiteQueenside(ref CurrentPosition.Castling, fen[2].IndexOf("Q") > -1);
            Castling.SetBlackKingside(ref CurrentPosition.Castling, fen[2].IndexOf("k") > -1);
            Castling.SetBlackQueenside(ref CurrentPosition.Castling, fen[2].IndexOf("q") > -1);
            CurrentPosition.EnPassantSquare = fen[3] == "-" ? Square.Illegal : GetSquare(fen[3]);
            CurrentPosition.HalfMoveNumber = fen.Count == 6 ? int.Parse(fen[4]) : 0;
            CurrentPosition.FullMoveNumber = fen.Count == 6 ? int.Parse(fen[5]) : 1;
            // Determine if king is in check and set position key.
            PlayNullMove();
            int kingSquare = CurrentPosition.WhiteMove
                ? Bitwise.FindFirstSetBit(CurrentPosition.BlackKing)
                : Bitwise.FindFirstSetBit(CurrentPosition.WhiteKing);
            bool kingInCheck = IsSquareAttacked(kingSquare);
            UndoMove();
            CurrentPosition.KingInCheck = kingInCheck;
            CurrentPosition.Key = GetPositionKey();
        }


        public bool ValidateMove(ref ulong Move)
        {
            int fromSquare = Engine.Move.From(Move);
            int toSquare = Engine.Move.To(Move);
            int attacker = CurrentPosition.GetPiece(fromSquare);
            if (attacker == Piece.None) return false; // No piece on from square.
            bool attackerWhite = Piece.IsWhite(attacker);
            if (CurrentPosition.WhiteMove != attackerWhite) return false; // Piece is wrong color.
            int victim = CurrentPosition.GetPiece(toSquare);
            if ((victim != Piece.None) && (attackerWhite == Piece.IsWhite(victim))) return false; // Piece cannot attack its own color.
            int promotedPiece = Engine.Move.PromotedPiece(Move);
            if ((promotedPiece != Piece.None) && (CurrentPosition.WhiteMove != Piece.IsWhite(promotedPiece))) return false; // Promoted piece is wrong color.
            int pawn;
            int king;
            int enPassantVictim;
            if (CurrentPosition.WhiteMove)
            {
                // White move
                pawn = Piece.WhitePawn;
                king = Piece.WhiteKing;
                enPassantVictim = Piece.BlackPawn;
            }
            else
            {
                // Black move
                pawn = Piece.BlackPawn;
                king = Piece.BlackKing;
                enPassantVictim = Piece.WhitePawn;
            }
            bool capture = victim != Piece.None;
            if (capture) Engine.Move.SetCaptureAttacker(ref Move, attacker);
            bool castling = (attacker == king) && (SquareDistances[fromSquare][toSquare] == 2);
            Engine.Move.SetIsCastling(ref Move, castling);
            Engine.Move.SetIsKingMove(ref Move, attacker == king);
            bool enPassantCapture = (attacker == pawn) && (toSquare == CurrentPosition.EnPassantSquare);
            Engine.Move.SetIsEnPassantCapture(ref Move, enPassantCapture);
            if (enPassantCapture)
            {
                capture = true;
                Engine.Move.SetCaptureVictim(ref Move, enPassantVictim);
            }
            else Engine.Move.SetCaptureVictim(ref Move, victim);
            Engine.Move.SetIsDoublePawnMove(ref Move, (attacker == pawn) && (SquareDistances[fromSquare][toSquare] == 2));
            Engine.Move.SetIsPawnMove(ref Move, attacker == pawn);
            bool pawnPromotion = Engine.Move.PromotedPiece(Move) != Piece.None;
            Engine.Move.SetIsQuiet(ref Move, !capture && !pawnPromotion && !castling);
            return true;
        }


        public bool IsMoveLegal(ref ulong Move)
        {
            if (Engine.Move.IsCastling(Move) && CurrentPosition.KingInCheck) return false;
            if (!CurrentPosition.KingInCheck && !Engine.Move.IsKingMove(Move) && !Engine.Move.IsEnPassantCapture(Move))
            {
                int fromSquare = Engine.Move.From(Move);
                if ((SquareMasks[fromSquare] & CurrentPosition.PotentiallyPinnedPieces) == 0)
                {
                    // Move cannot expose king to check.
                    PlayMove(Move);
                    goto ChecksEnemyKing;
                }
            }
            // Determine if moving piece exposes king to check.
            PlayMove(Move);
            int kingSquare = CurrentPosition.WhiteMove
                ? Bitwise.FindFirstSetBit(CurrentPosition.BlackKing)
                : Bitwise.FindFirstSetBit(CurrentPosition.WhiteKing);
            if (IsSquareAttacked(kingSquare))
            {
                UndoMove();
                return false;
            }
            if (Engine.Move.IsCastling(Move) && IsCastlePathAttacked(Move))
            {
                UndoMove();
                return false;
            }
            ChecksEnemyKing:
            // Move is legal.
            // Determine if move checks enemy king.
            PlayNullMove();
            kingSquare = CurrentPosition.WhiteMove
                ? Bitwise.FindFirstSetBit(CurrentPosition.BlackKing)
                : Bitwise.FindFirstSetBit(CurrentPosition.WhiteKing);
            bool check = IsSquareAttacked(kingSquare);
            UndoMove();
            Engine.Move.SetIsCheck(ref Move, check);
            if (check) Engine.Move.SetIsQuiet(ref Move, false);
            UndoMove();
            return true;
        }


        private bool IsCastlePathAttacked(ulong Move)
        {
            int toSquare = Engine.Move.To(Move);
            ulong attackedSquaresMask;
            if (CurrentPosition.WhiteMove)
            {
                // Black castled, now white move.
                switch (toSquare)
                {
                    case Square.c8:
                        attackedSquaresMask = BlackCastleQAttackedSquareMask;
                        break;
                    case Square.g8:
                        attackedSquaresMask = BlackCastleKAttackedSquareMask;
                        break;
                    default:
                        throw new Exception($"Black king cannot castle to {SquareLocations[toSquare]}.");
                }
            }
            else
            {
                // White castled, now black move.
                switch (toSquare)
                {
                    case Square.c1:
                        attackedSquaresMask = WhiteCastleQAttackedSquareMask;
                        break;
                    case Square.g1:
                        attackedSquaresMask = WhiteCastleKAttackedSquareMask;
                        break;
                    default:
                        throw new Exception($"White king cannot castle to {SquareLocations[toSquare]}.");
                }
            }
            while ((toSquare = Bitwise.FindFirstSetBit(attackedSquaresMask)) != Square.Illegal)
            {
                if (IsSquareAttacked(toSquare)) return true;
                Bitwise.ClearBit(ref attackedSquaresMask, toSquare);
            }
            return false;
        }


        private bool IsSquareAttacked(int Square)
        {
            ulong pawns;
            ulong pawnAttackMask;
            ulong knights;
            ulong bishops;
            ulong rooks;
            ulong queens;
            ulong king;
            if (CurrentPosition.WhiteMove)
            {
                // White move
                pawns = CurrentPosition.WhitePawns;
                pawnAttackMask = BlackPawnAttackMasks[Square]; // Attacked by white pawn masks = black pawn attack masks
                knights = CurrentPosition.WhiteKnights;
                bishops = CurrentPosition.WhiteBishops;
                rooks = CurrentPosition.WhiteRooks;
                queens = CurrentPosition.WhiteQueens;
                king = CurrentPosition.WhiteKing;
            }
            else
            {
                // Black move
                pawns = CurrentPosition.BlackPawns;
                pawnAttackMask = WhitePawnAttackMasks[Square];  // Attacked by black pawn masks = white pawn attack masks
                knights = CurrentPosition.BlackKnights;
                bishops = CurrentPosition.BlackBishops;
                rooks = CurrentPosition.BlackRooks;
                queens = CurrentPosition.BlackQueens;
                king = CurrentPosition.BlackKing;
            }
            // Determine if square is attacked by pawns or knights.
            if ((pawns & pawnAttackMask) > 0) return true;
            if ((knights & KnightMoveMasks[Square]) > 0) return true;
            // Determine if square is attacked by diagonal sliding piece.
            // Rather than examining every move of every enemy sliding piece, examine move of sliding piece on given Square.
            ulong bishopDestinations = PrecalculatedMoves.GetBishopMovesMask(Square, CurrentPosition.Occupancy);
            int toSquare;
            while ((toSquare = Bitwise.FindFirstSetBit(bishopDestinations)) != Engine.Square.Illegal)
            {
                if ((SquareMasks[toSquare] & (bishops | queens)) > 0) return true;
                Bitwise.ClearBit(ref bishopDestinations, toSquare);
            }
            // Determine if square is attacked by file / rank sliding pieces.
            // Rather than examining every move of every enemy sliding piece, examine move of sliding piece on given Square.
            ulong rookDestinations = PrecalculatedMoves.GetRookMovesMask(Square, CurrentPosition.Occupancy);
            while ((toSquare = Bitwise.FindFirstSetBit(rookDestinations)) != Engine.Square.Illegal)
            {
                if ((SquareMasks[toSquare] & (rooks | queens)) > 0) return true;
                Bitwise.ClearBit(ref rookDestinations, toSquare);
            }
            // Determine if square is attacked by king.
            return (king & KingMoveMasks[Square]) > 0;
        }


        public void PlayMove(ulong Move)
        {
            Debug.Assert(AssertMoveIntegrity(Move));
            CurrentPosition.PlayedMove = Move;
            // Advance position index.
            NextPosition.Set(CurrentPosition);
            _positionIndex++;
            int fromSquare = Engine.Move.From(Move);
            int toSquare = Engine.Move.To(Move);
            int piece = CurrentPosition.GetPiece(fromSquare);
            int captureVictim;
            if (Engine.Move.IsCastling(Move))
            {
                // Castle
                captureVictim = Piece.None;
                Castle(piece, toSquare);
            }
            else if (Engine.Move.IsEnPassantCapture(Move))
            {
                // En passant capture
                captureVictim = Engine.Move.CaptureVictim(Move);
                EnPassantCapture(piece, fromSquare);
            }
            else
            {
                // Move piece and remove capture victim (none if destination square is unoccupied).
                captureVictim = RemovePiece(toSquare);
                Debug.Assert(AssertKingIsNotCaptured(captureVictim, Move));
                RemovePiece(fromSquare);
                int promotedPiece = Engine.Move.PromotedPiece(Move);
                AddPiece(promotedPiece == Piece.None ? piece : promotedPiece, toSquare);
                if (Engine.Move.IsDoublePawnMove(Move)) CurrentPosition.EnPassantSquare = EnPassantTargetSquares[toSquare];
            }
            if (Castling.IsPossible(CurrentPosition.Castling))
            {
                // Update castling rights.
                switch (fromSquare)
                {
                    case Square.a8:
                        Castling.SetBlackQueenside(ref CurrentPosition.Castling, false);
                        break;
                    case Square.e8:
                        Castling.SetBlackQueenside(ref CurrentPosition.Castling, false);
                        Castling.SetBlackKingside(ref CurrentPosition.Castling, false);
                        break;
                    case Square.h8:
                        Castling.SetBlackKingside(ref CurrentPosition.Castling, false);
                        break;
                    case Square.a1:
                        Castling.SetWhiteQueenside(ref CurrentPosition.Castling, false);
                        break;
                    case Square.e1:
                        Castling.SetWhiteQueenside(ref CurrentPosition.Castling, false);
                        Castling.SetWhiteKingside(ref CurrentPosition.Castling, false);
                        break;
                    case Square.h1:
                        Castling.SetWhiteKingside(ref CurrentPosition.Castling, false);
                        break;
                }
                switch (toSquare)
                {
                    case Square.a8:
                        Castling.SetBlackQueenside(ref CurrentPosition.Castling, false);
                        break;
                    case Square.h8:
                        Castling.SetBlackKingside(ref CurrentPosition.Castling, false);
                        break;
                    case Square.a1:
                        Castling.SetWhiteQueenside(ref CurrentPosition.Castling, false);
                        break;
                    case Square.h1:
                        Castling.SetWhiteKingside(ref CurrentPosition.Castling, false);
                        break;
                }
            }
            // Update en passant capture square, move counts, side to move, position key, and nodes.
            CurrentPosition.EnPassantSquare = Engine.Move.IsDoublePawnMove(Move) ? EnPassantTargetSquares[toSquare] : Square.Illegal;
            if (captureVictim != Piece.None || Engine.Move.IsPawnMove(Move)) CurrentPosition.HalfMoveNumber = 0;
            else CurrentPosition.HalfMoveNumber++;
            if (!CurrentPosition.WhiteMove) CurrentPosition.FullMoveNumber++;
            CurrentPosition.WhiteMove = !CurrentPosition.WhiteMove;
            CurrentPosition.KingInCheck = Engine.Move.IsCheck(Move);
            CurrentPosition.Key = GetPositionKey();
            Nodes++;
            Debug.Assert(AssertIntegrity());
        }


        private bool AssertMoveIntegrity(ulong Move)
        {
            int fromSquare = Engine.Move.From(Move);
            int toSquare = Engine.Move.To(Move);
            int piece = CurrentPosition.GetPiece(fromSquare);
            int pawn;
            int king;
            int toRank;
            // EnPassantVictim variable only used in Debug builds.
            // ReSharper disable RedundantAssignment
            int enPassantVictim;
            if (CurrentPosition.WhiteMove)
            {
                // White move
                pawn = Piece.WhitePawn;
                king = Piece.WhiteKing;
                toRank = WhiteRanks[toSquare];
                enPassantVictim = Piece.BlackPawn;
            }
            else
            {
                // Black move
                pawn = Piece.BlackPawn;
                king = Piece.BlackKing;
                toRank = BlackRanks[toSquare];
                enPassantVictim = Piece.WhitePawn;
            }
            int captureVictim = CurrentPosition.GetPiece(toSquare);
            bool enPassantCapture = (CurrentPosition.EnPassantSquare != Square.Illegal) && (piece == pawn) && (toSquare == CurrentPosition.EnPassantSquare);
            Debug.Assert(Engine.Move.IsEnPassantCapture(Move) == enPassantCapture, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
            if (enPassantCapture) Debug.Assert(Engine.Move.CaptureVictim(Move) == enPassantVictim, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
            else Debug.Assert(Engine.Move.CaptureVictim(Move) == captureVictim, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
            bool pawnPromotion = (piece == pawn) && (toRank == 7);
            if (pawnPromotion) Debug.Assert(Engine.Move.PromotedPiece(Move) != Piece.None, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
            else Debug.Assert(Engine.Move.PromotedPiece(Move) == Piece.None, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
            bool castling = (piece == king) && (SquareDistances[fromSquare][toSquare] == 2);
            Debug.Assert(Engine.Move.IsCastling(Move) == castling, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
            bool kingMove = piece == king;
            Debug.Assert(Engine.Move.IsKingMove(Move) == kingMove, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
            bool doublePawnMove = (piece == pawn) && (SquareDistances[fromSquare][toSquare] == 2);
            Debug.Assert(Engine.Move.IsDoublePawnMove(Move) == doublePawnMove, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
            bool pawnMove = piece == pawn;
            Debug.Assert(Engine.Move.IsPawnMove(Move) == pawnMove, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
            // ReSharper restore RedundantAssignment
            return true;
        }


        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private bool AssertKingIsNotCaptured(int CaptureVictim, ulong Move)
        {
            if ((CaptureVictim == Piece.WhiteKing) || (CaptureVictim == Piece.BlackKing))
            {
                _positionIndex--;
                _writeMessageLine($"Previous position = {PreviousPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(PreviousPosition.PlayedMove)}{Environment.NewLine}{PreviousPosition}");
                _writeMessageLine(PreviousPosition.ToString());
                _writeMessageLine(null);
                Debug.Assert(CaptureVictim != Piece.WhiteKing, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
                Debug.Assert(CaptureVictim != Piece.BlackKing, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Engine.Move.ToString(Move)}{Environment.NewLine}{CurrentPosition}");
            }
            return true;
        }


        private void Castle(int Piece, int ToSquare)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (Piece == Engine.Piece.WhiteKing)
                switch (ToSquare)
                {
                    case Square.c1:
                        // White castle queenside
                        RemovePiece(Square.e1);
                        AddPiece(Engine.Piece.WhiteKing, Square.c1);
                        RemovePiece(Square.a1);
                        AddPiece(Engine.Piece.WhiteRook, Square.d1);
                        break;
                    case Square.g1:
                        // White castle kingside
                        RemovePiece(Square.e1);
                        AddPiece(Engine.Piece.WhiteKing, Square.g1);
                        RemovePiece(Square.h1);
                        AddPiece(Engine.Piece.WhiteRook, Square.f1);
                        break;
                    default:
                        throw new Exception($"White king cannot castle to {SquareLocations[ToSquare]}.");
                }
            else if (Piece == Engine.Piece.BlackKing)
                switch (ToSquare)
                {
                    case Square.c8:
                        // Black castle queenside
                        RemovePiece(Square.e8);
                        AddPiece(Engine.Piece.BlackKing, Square.c8);
                        RemovePiece(Square.a8);
                        AddPiece(Engine.Piece.BlackRook, Square.d8);
                        break;
                    case Square.g8:
                        // Black castle kingside
                        RemovePiece(Square.e8);
                        AddPiece(Engine.Piece.BlackKing, Square.g8);
                        RemovePiece(Square.h8);
                        AddPiece(Engine.Piece.BlackRook, Square.f8);
                        break;
                    default:
                        throw new Exception($"Black king cannot castle to {SquareLocations[ToSquare]}.");
                }
            else
                throw new Exception($"{Piece} piece cannot castle.");
        }


        private void EnPassantCapture(int Piece, int FromSquare)
        {
            // Move pawn and remove captured pawn.
            RemovePiece(EnPassantVictimSquares[CurrentPosition.EnPassantSquare]);
            RemovePiece(FromSquare);
            AddPiece(Piece, CurrentPosition.EnPassantSquare);
        }


        public void PlayNullMove()
        {
            CurrentPosition.PlayedMove = Move.Null;
            // Advance position index.
            NextPosition.Set(CurrentPosition);
            _positionIndex++;
            // King cannot be in check, nor is en passant capture possible after null move.
            CurrentPosition.KingInCheck = false;
            CurrentPosition.EnPassantSquare = Square.Illegal;
            CurrentPosition.WhiteMove = !CurrentPosition.WhiteMove;
            CurrentPosition.Key = GetPositionKey();
            Nodes++;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoMove()
        {
            Debug.Assert(_positionIndex > 0);
            _positionIndex--;
        }


        public int GetPositionCount()
        {
            ulong currentPositionKey = CurrentPosition.Key;
            int positionCount = 0;
            // Examine positions since the last capture or pawn move.
            int firstMove = Math.Max(_positionIndex - CurrentPosition.HalfMoveNumber, 0);
            for (int positionIndex = firstMove; positionIndex <= _positionIndex; positionIndex++) if (_positions[positionIndex].Key == currentPositionKey) positionCount++;
            return positionCount;
        }


        public bool AssertIntegrity()
        {
            // Validate occupancy.
            Debug.Assert((CurrentPosition.WhitePawns | CurrentPosition.WhiteKnights | CurrentPosition.WhiteBishops | CurrentPosition.WhiteRooks | CurrentPosition.WhiteQueens | CurrentPosition.WhiteKing) == CurrentPosition.OccupancyWhite);
            Debug.Assert((CurrentPosition.BlackPawns | CurrentPosition.BlackKnights | CurrentPosition.BlackBishops | CurrentPosition.BlackRooks | CurrentPosition.BlackQueens | CurrentPosition.BlackKing) == CurrentPosition.OccupancyBlack);
            Debug.Assert((CurrentPosition.OccupancyWhite | CurrentPosition.OccupancyBlack) == CurrentPosition.Occupancy);
            // Validate one king of each color is on the board.
            Debug.Assert(Bitwise.CountSetBits(CurrentPosition.WhiteKing) == 1);
            Debug.Assert(Bitwise.CountSetBits(CurrentPosition.BlackKing) == 1);
            return true;
        }


        private void AddPiece(int Piece, int Square)
        {
            Debug.Assert(Piece != Engine.Piece.None);
            // Update piece, color, and both color bitboards.
            ulong squareMask = SquareMasks[Square];
            switch (Piece)
            {
                case Engine.Piece.WhitePawn:
                    CurrentPosition.WhitePawns |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Engine.Piece.WhiteKnight:
                    CurrentPosition.WhiteKnights |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Engine.Piece.WhiteBishop:
                    CurrentPosition.WhiteBishops |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Engine.Piece.WhiteRook:
                    CurrentPosition.WhiteRooks |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Engine.Piece.WhiteQueen:
                    CurrentPosition.WhiteQueens |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Engine.Piece.WhiteKing:
                    CurrentPosition.WhiteKing |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Engine.Piece.BlackPawn:
                    CurrentPosition.BlackPawns |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
                case Engine.Piece.BlackKnight:
                    CurrentPosition.BlackKnights |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
                case Engine.Piece.BlackBishop:
                    CurrentPosition.BlackBishops |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
                case Engine.Piece.BlackRook:
                    CurrentPosition.BlackRooks |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
                case Engine.Piece.BlackQueen:
                    CurrentPosition.BlackQueens |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
                case Engine.Piece.BlackKing:
                    CurrentPosition.BlackKing |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
            }
            CurrentPosition.Occupancy |= squareMask;
            UpdatePiecesSquaresKey(Piece, Square);
        }


        private int RemovePiece(int Square)
        {
            ulong squareUnmask = SquareUnmasks[Square];
            int piece = CurrentPosition.GetPiece(Square);
            // Update piece, color, and both color bitboards.
            switch (piece)
            {
                case Piece.None:
                    return piece;
                case Piece.WhitePawn:
                    CurrentPosition.WhitePawns &= squareUnmask;
                    CurrentPosition.OccupancyWhite &= squareUnmask;
                    break;
                case Piece.WhiteKnight:
                    CurrentPosition.WhiteKnights &= squareUnmask;
                    CurrentPosition.OccupancyWhite &= squareUnmask;
                    break;
                case Piece.WhiteBishop:
                    CurrentPosition.WhiteBishops &= squareUnmask;
                    CurrentPosition.OccupancyWhite &= squareUnmask;
                    break;
                case Piece.WhiteRook:
                    CurrentPosition.WhiteRooks &= squareUnmask;
                    CurrentPosition.OccupancyWhite &= squareUnmask;
                    break;
                case Piece.WhiteQueen:
                    CurrentPosition.WhiteQueens &= squareUnmask;
                    CurrentPosition.OccupancyWhite &= squareUnmask;
                    break;
                case Piece.WhiteKing:
                    CurrentPosition.WhiteKing &= squareUnmask;
                    CurrentPosition.OccupancyWhite &= squareUnmask;
                    break;
                case Piece.BlackPawn:
                    CurrentPosition.BlackPawns &= squareUnmask;
                    CurrentPosition.OccupancyBlack &= squareUnmask;
                    break;
                case Piece.BlackKnight:
                    CurrentPosition.BlackKnights &= squareUnmask;
                    CurrentPosition.OccupancyBlack &= squareUnmask;
                    break;
                case Piece.BlackBishop:
                    CurrentPosition.BlackBishops &= squareUnmask;
                    CurrentPosition.OccupancyBlack &= squareUnmask;
                    break;
                case Piece.BlackRook:
                    CurrentPosition.BlackRooks &= squareUnmask;
                    CurrentPosition.OccupancyBlack &= squareUnmask;
                    break;
                case Piece.BlackQueen:
                    CurrentPosition.BlackQueens &= squareUnmask;
                    CurrentPosition.OccupancyBlack &= squareUnmask;
                    break;
                case Piece.BlackKing:
                    CurrentPosition.BlackKing &= squareUnmask;
                    CurrentPosition.OccupancyBlack &= squareUnmask;
                    break;
            }
            CurrentPosition.Occupancy &= squareUnmask;
            UpdatePiecesSquaresKey(piece, Square);
            return piece;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePiecesSquaresKey(int Piece, int Square)
        {
            CurrentPosition.PiecesSquaresKey ^= _pieceSquareKeys[Piece][Square];
            Debug.Assert(AssertPiecesSquaresKeyIntegrity());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong GetPositionKey()
        {
            ulong sideToMoveKey = CurrentPosition.WhiteMove ? _sideToMoveKeys[0] : _sideToMoveKeys[1];
            ulong castlingKey = _castlingKeys[CurrentPosition.Castling];
            ulong enPassantKey = CurrentPosition.EnPassantSquare == Square.Illegal ? _enPassantKeys[0] : _enPassantKeys[CurrentPosition.EnPassantSquare];
            return CurrentPosition.PiecesSquaresKey ^ sideToMoveKey ^ castlingKey ^ enPassantKey;
        }


        private bool AssertPiecesSquaresKeyIntegrity()
        {
            // Verify incrementally updated pieces squares key matches fully updated pieces squares key.
            ulong fullyUpdatedPiecesSquaresKey = _piecesSquaresInitialKey;
            for (int square = 0; square < 64; square++)
            {
                int piece = CurrentPosition.GetPiece(square);
                if (piece != Piece.None) fullyUpdatedPiecesSquaresKey ^= _pieceSquareKeys[piece][square];
            }
            Debug.Assert(fullyUpdatedPiecesSquaresKey == CurrentPosition.PiecesSquaresKey);
            return true;
        }

        private void Reset(bool PreserveMoveCount)
        {
            // Reset position index, position, key, and stats.
            _positionIndex = 0;
            CurrentPosition.Reset();
            CurrentPosition.PiecesSquaresKey = _piecesSquaresInitialKey;
            if (!PreserveMoveCount)
            {
                // Reset nodes.
                Nodes = 0;
                NodesInfoUpdate = UciStream.NodesInfoInterval;
            }
        }


        public static string ToString(ulong Occupancy)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    int square = GetSquare(file, rank);
                    stringBuilder.Append(Bitwise.IsBitSet(Occupancy, square) ? " 1 " : " . ");
                }
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }


        public override string ToString() => CurrentPosition.ToString();
    }
}