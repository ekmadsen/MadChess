// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class Board
    {
        public const string StartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public static readonly int[] Files;
        public static readonly int[] WhiteRanks;
        public static readonly int[] BlackRanks;
        public static readonly bool[] LightSquares;
        public static readonly int[][] SquareDistances;
        public static readonly int[] DistanceToCentralSquares;
        public static readonly int[] DistanceToNearestCorner;
        public static readonly int[] DistanceToNearestLightCorner;
        public static readonly int[] DistanceToNearestDarkCorner;
        public static readonly string[] SquareLocations;
        public static readonly ulong[] SquareMasks;
        public static readonly ulong[] FileMasks;
        public static readonly ulong[] RankMasks;
        public static readonly ulong AllSquaresMask;
        public static readonly ulong EdgeSquaresMask;
        public static readonly ulong WhiteCastleQEmptySquaresMask;
        public static readonly ulong WhiteCastleKEmptySquaresMask;
        public static readonly ulong BlackCastleQEmptySquaresMask;
        public static readonly ulong BlackCastleKEmptySquaresMask;
        public static readonly ulong[] KnightMoveMasks;
        public static readonly ulong[] BishopMoveMasks;
        public static readonly ulong[] RookMoveMasks;
        public static readonly ulong[] KingMoveMasks;
        public static readonly ulong[] EnPassantAttackerMasks;
        public static readonly ulong[] WhitePassedPawnMasks;
        public static readonly ulong[] WhiteFreePawnMasks;
        public static readonly ulong[] BlackPassedPawnMasks;
        public static readonly ulong[] BlackFreePawnMasks;
        public static readonly ulong[] WhitePawnMoveMasks;
        public static readonly ulong[] WhitePawnDoubleMoveMasks;
        public static readonly ulong[] WhitePawnAttackMasks;
        public static readonly ulong[] BlackPawnMoveMasks;
        public static readonly ulong[] BlackPawnDoubleMoveMasks;
        public static readonly ulong[] BlackPawnAttackMasks;
        public static readonly ulong[] InnerRingMasks;
        public static readonly ulong[] OuterRingMasks;
        public static readonly ulong[] WhitePawnShieldMasks;
        public static readonly ulong[] BlackPawnShieldMasks;
        public static readonly PrecalculatedMoves PrecalculatedMoves;
        public static readonly ulong[][] RankFileBetweenSquares;
        public static readonly ulong[][] DiagonalBetweenSquares;
        public long Nodes;
        public long NodesInfoUpdate;
        public long NodesExamineTime;
        private const int _maxPositions = 1024;
        private static readonly ulong[] _squareUnmasks;
        private static readonly ulong _whiteCastleQAttackedSquareMask;
        private static readonly ulong _whiteCastleKAttackedSquareMask;
        private static readonly ulong _blackCastleQAttackedSquareMask;
        private static readonly ulong _blackCastleKAttackedSquareMask;
        private static readonly int[][] _neighborSquares;
        private static readonly int[] _enPassantTargetSquares;
        private static readonly int[] _enPassantVictimSquares;
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


        private Position NextPosition => _positions[_positionIndex + 1];


        static Board()
        {
            // The chessboard is represented as an array of 64 squares, shown here as an 8 x 8 grid of square indices.
            // Note this code uses zero-based indices, while chess literature uses one-based indices.
            // A1 in chess literature = square index 56.  A8 = square index 00.  H8 = square index 07.  H1 = square index 63.

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
            int[] centralSquares = { Square.d4, Square.e4, Square.d5, Square.e5 };
            int[] cornerSquares = { Square.a8, Square.h8, Square.a1, Square.h1 };
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
            int[] lightCornerSquares = { Square.a8, Square.h1 };
            int[] darkCornerSquares = { Square.a1, Square.h8 };
            // Determine distances between squares.
            SquareDistances = new int[64][];
            for (var square1 = 0; square1 < 64; square1++)
            {
                SquareDistances[square1] = new int[64];
                for (var square2 = 0; square2 < 64; square2++)
                {
                    var fileDistance = Math.Abs(Files[square1] - Files[square2]);
                    var rankDistance = Math.Abs(WhiteRanks[square1] - WhiteRanks[square2]);
                    SquareDistances[square1][square2] = Math.Max(fileDistance, rankDistance);
                }
            }
            // Determine distances to central and nearest corner squares.
            DistanceToCentralSquares = new int[64];
            DistanceToNearestCorner = new int[64];
            DistanceToNearestLightCorner = new int[64];
            DistanceToNearestDarkCorner = new int[64];
            for (var square = 0; square < 64; square++)
            {
                DistanceToCentralSquares[square] = GetShortestDistance(square, centralSquares);
                DistanceToNearestCorner[square] = GetShortestDistance(square, cornerSquares);
                DistanceToNearestLightCorner[square] = GetShortestDistance(square, lightCornerSquares);
                DistanceToNearestDarkCorner[square] = GetShortestDistance(square, darkCornerSquares);
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
            _squareUnmasks = new ulong[64];
            for (var square = 0; square < 64; square++)
            {
                SquareMasks[square] = Bitwise.CreateULongMask(square);
                _squareUnmasks[square] = Bitwise.CreateULongUnmask(square);
            }
            FileMasks = new ulong[8];
            for (var file = 0; file < 8; file++)
            {
                FileMasks[file] = 0;
                for (var rank = 0; rank < 8; rank++)
                {
                    var square = GetSquare(file, rank);
                    FileMasks[file] |= Bitwise.CreateULongMask(square);
                }
            }
            RankMasks = new ulong[8];
            for (var rank = 0; rank < 8; rank++)
            {
                RankMasks[rank] = 0;
                for (var file = 0; file < 8; file++)
                {
                    var square = GetSquare(file, rank);
                    RankMasks[rank] |= Bitwise.CreateULongMask(square);
                }
            }
            AllSquaresMask = Bitwise.CreateULongMask(0, 63);
            EdgeSquaresMask = FileMasks[0] | RankMasks[7] | FileMasks[7] | RankMasks[0];
            // Create castling masks.
            WhiteCastleQEmptySquaresMask = Bitwise.CreateULongMask(new[] {Square.b1, Square.c1, Square.d1});
            _whiteCastleQAttackedSquareMask = Bitwise.CreateULongMask(Square.d1);
            WhiteCastleKEmptySquaresMask = Bitwise.CreateULongMask(new[] { Square.f1, Square.g1 });
            _whiteCastleKAttackedSquareMask = Bitwise.CreateULongMask(Square.f1);
            BlackCastleQEmptySquaresMask = Bitwise.CreateULongMask(new[] { Square.b8, Square.c8, Square.d8 });
            _blackCastleQAttackedSquareMask = Bitwise.CreateULongMask(Square.d8);
            BlackCastleKEmptySquaresMask = Bitwise.CreateULongMask(new[] { Square.f8, Square.g8 });
            _blackCastleKAttackedSquareMask = Bitwise.CreateULongMask(Square.f8);

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

            var directionOffsets1212 = CreateDirectionOffsets1212();
            var squareIndices1212To88 = MapSquareIndices1212To88();
            _neighborSquares = CreateNeighborSquares(directionOffsets1212, squareIndices1212To88);
            // Create move masks and precalculated moves.
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
            PrecalculatedMoves = new PrecalculatedMoves();
            // Determine squares in a rank / file direction between two squares.
            RankFileBetweenSquares = new ulong[64][];
            for (var square1 = 0; square1 < 64; square1++)
            {
                RankFileBetweenSquares[square1] = new ulong[64];
                var directions = new[] {Direction.North, Direction.East, Direction.South, Direction.West};
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var distance = 1;
                    var square2 = square1;
                    var previousSquare2 = Square.Illegal;
                    var betweenSquares = 0ul;
                    do
                    {
                        square2 = _neighborSquares[square2][(int)direction];
                        if (square2 == Square.Illegal) break;
                        if (distance > 1)
                        {
                            betweenSquares |= SquareMasks[previousSquare2];
                            RankFileBetweenSquares[square1][square2] = betweenSquares;
                        }
                        previousSquare2 = square2;
                        distance++;
                    } while (true);
                }
            }
            // Determine squares in a diagonal direction between two squares.
            DiagonalBetweenSquares = new ulong[64][];
            for (var square1 = 0; square1 < 64; square1++)
            {
                DiagonalBetweenSquares[square1] = new ulong[64];
                var directions = new[] { Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest };
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var distance = 1;
                    var square2 = square1;
                    var previousSquare2 = Square.Illegal;
                    var betweenSquares = 0ul;
                    do
                    {
                        square2 = _neighborSquares[square2][(int)direction];
                        if (square2 == Square.Illegal) break;
                        if (distance > 1)
                        {
                            betweenSquares |= SquareMasks[previousSquare2];
                            DiagonalBetweenSquares[square1][square2] = betweenSquares;
                        }
                        previousSquare2 = square2;
                        distance++;
                    } while (true);
                }
            }
            // Create en passant, passed pawn, and free pawn masks.
            (_enPassantTargetSquares, _enPassantVictimSquares, EnPassantAttackerMasks) = CreateEnPassantAttackerMasks();
            WhitePassedPawnMasks = CreateWhitePassedPawnMasks();
            WhiteFreePawnMasks = CreateWhiteFreePawnMasks();
            BlackPassedPawnMasks = CreateBlackPassedPawnMasks();
            BlackFreePawnMasks = CreateBlackFreePawnMasks();
            // Create ring and pawn shield masks.
            (InnerRingMasks, OuterRingMasks) = CreateRingMasks();
            WhitePawnShieldMasks = CreateWhitePawnShieldMasks();
            BlackPawnShieldMasks = CreateBlackPawnShieldMasks();
        }


        public Board(Delegates.WriteMessageLine writeMessageLine)
        {
            _writeMessageLine = writeMessageLine;
            // Create positions and precalculated moves.
            _positions = new Position[_maxPositions];
            for (var positionIndex = 0; positionIndex < _maxPositions; positionIndex++) _positions[positionIndex] = new Position(this);
            // Create Zobrist position keys.
            _piecesSquaresInitialKey = SafeRandom.NextULong();
            _pieceSquareKeys = new ulong[13][];
            for (var piece = 0; piece < 13; piece++)
            {
                _pieceSquareKeys[piece] = new ulong[64];
                for (var square = 0; square < 64; square++) _pieceSquareKeys[piece][square] = SafeRandom.NextULong();
            }
            _sideToMoveKeys = new[] { SafeRandom.NextULong(), SafeRandom.NextULong() };
            _castlingKeys = new ulong[16];
            {
                for (var castlingRights = 0; castlingRights < 16; castlingRights++) _castlingKeys[castlingRights] = SafeRandom.NextULong();
            }
            _enPassantKeys = new ulong[64];
            for (var square = 0; square < 64; square++) _enPassantKeys[square] = SafeRandom.NextULong();
            _piecesSquaresInitialKey = SafeRandom.NextULong();
            // Set nodes.
            Nodes = 0;
            NodesInfoUpdate = UciStream.NodesInfoInterval;
        }


        private static int[] CreateDirectionOffsets1212()
        {
            var directionOffsets1212 = new int[17];
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
            var squareIndices1212To88 = new int[144];
            var square1212 = 0;
            var square88 = 0;
            for (var file = -2; file <= 9; file++)
                for (var rank = -2; rank <= 9; rank++)
                {
                    if (file >= 0 && file <= 7 && rank >= 0 && rank <= 7)
                    {
                        // Legal Square
                        squareIndices1212To88[square1212] = square88;
                        square88++;
                    }
                    else squareIndices1212To88[square1212] = Square.Illegal; // Illegal Square
                    square1212++;
                }
            return squareIndices1212To88;
        }


        private static int[][] CreateNeighborSquares(int[] directionOffsets1212, int[] squareIndices1212To88)
        {
            var neighborSquares = new int[64][];
            int square88;
            for (square88 = 0; square88 < 64; square88++) neighborSquares[square88] = new int[(int)Direction.North2West1 + 1];
            for (var square1212 = 0; square1212 < 144; square1212++)
            {
                square88 = squareIndices1212To88[square1212];
                if (square88 != Square.Illegal)
                    for (var direction = 1; direction <= (int)Direction.North2West1; direction++)
                    {
                        var directionOffset1212 = directionOffsets1212[direction];
                        neighborSquares[square88][direction] = squareIndices1212To88[square1212 + directionOffset1212];
                    }
            }
            return neighborSquares;
        }


        private static ulong[] CreateWhitePawnMoveMasks()
        {
            var moveMasks = new ulong[64];
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                var otherSquare = _neighborSquares[square][(int)Direction.North];
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


        private static ulong[] CreateWhitePawnDoubleMoveMasks()
        {
            var moveMasks = new ulong[64];
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                if (WhiteRanks[square] == 1)
                {
                    var otherSquare = _neighborSquares[square][(int)Direction.North];
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private static ulong[] CreateWhitePawnAttackMasks()
        {
            var moveMasks = new ulong[64];
            Direction[] directions = { Direction.NorthWest, Direction.NorthEast };
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private static ulong[] CreateBlackPawnMoveMasks()
        {
            var moveMasks = new ulong[64];
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                var otherSquare = _neighborSquares[square][(int)Direction.South];
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


        private static ulong[] CreateBlackPawnDoubleMoveMasks()
        {
            var moveMasks = new ulong[64];
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                if (BlackRanks[square] == 1)
                {
                    var otherSquare = _neighborSquares[square][(int)Direction.South];
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private static ulong[] CreateBlackPawnAttackMasks()
        {
            var moveMasks = new ulong[64];
            Direction[] directions = { Direction.SouthWest, Direction.SouthEast };
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                moveMasks[square] = mask;
            }
            return moveMasks;
        }


        private static (int[] EnPassantTargetSquares, int[] EnPassantVictimSquares, ulong[] EnPassantAttackerMasks) CreateEnPassantAttackerMasks()
        {
            var enPassantTargetSquares = new int[64];
            var enPassantVictimSquares = new int[64];
            var enPassantAttackerMasks = new ulong[64];
            for (var file = 0; file < 8; file++)
            {
                // White takes black pawn en passant.
                var toSquare = GetSquare(file, 4);
                var targetSquare = _neighborSquares[toSquare][(int)Direction.North];
                enPassantVictimSquares[targetSquare] = _neighborSquares[targetSquare][(int)Direction.South];
                var westAttackerSquare = _neighborSquares[targetSquare][(int)Direction.SouthWest];
                var eastAttackerSquare = _neighborSquares[targetSquare][(int)Direction.SouthEast];
                enPassantTargetSquares[toSquare] = targetSquare;
                var attackerMask = 0ul;
                if (westAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(westAttackerSquare);
                if (eastAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(eastAttackerSquare);
                enPassantAttackerMasks[targetSquare] = attackerMask;
                // Black takes white pawn en passant.
                toSquare = GetSquare(file, 3);
                targetSquare = _neighborSquares[toSquare][(int)Direction.South];
                enPassantVictimSquares[targetSquare] = _neighborSquares[targetSquare][(int)Direction.North];
                westAttackerSquare = _neighborSquares[targetSquare][(int)Direction.NorthWest];
                eastAttackerSquare = _neighborSquares[targetSquare][(int)Direction.NorthEast];
                enPassantTargetSquares[toSquare] = targetSquare;
                attackerMask = 0;
                if (westAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(westAttackerSquare);
                if (eastAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(eastAttackerSquare);
                enPassantAttackerMasks[targetSquare] = attackerMask;
            }
            return (enPassantTargetSquares, enPassantVictimSquares, enPassantAttackerMasks);
        }


        private static ulong[] CreateWhitePassedPawnMasks()
        {
            var masks = new ulong[64];
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                int[] startingSquares =
                {
                    _neighborSquares[square][(int) Direction.West],
                    square,
                    _neighborSquares[square][(int) Direction.East]
                };
                for (var index = 0; index < startingSquares.Length; index++)
                {
                    var otherSquare = startingSquares[index];
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


        private static ulong[] CreateWhiteFreePawnMasks()
        {
            var masks = new ulong[64];
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                var otherSquare = square;
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


        private static ulong[] CreateBlackPassedPawnMasks()
        {
            var masks = new ulong[64];
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                int[] startingSquares =
                {
                    _neighborSquares[square][(int) Direction.West],
                    square,
                    _neighborSquares[square][(int) Direction.East]
                };
                for (var index = 0; index < startingSquares.Length; index++)
                {
                    var otherSquare = startingSquares[index];
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


        private static ulong[] CreateBlackFreePawnMasks()
        {
            var masks = new ulong[64];
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                var otherSquare = square;
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


        private static (ulong[] InnerRingMasks, ulong[] OuterRingMasks) CreateRingMasks()
        {
            var innerRingMasks = new ulong[64];
            var outerRingMasks = new ulong[64];
            Direction[] innerRingDirections = { Direction.North, Direction.NorthEast, Direction.East, Direction.SouthEast, Direction.South, Direction.SouthWest, Direction.West, Direction.NorthWest };
            Direction[] outerRingDirections = { Direction.North2East1, Direction.East2North1, Direction.East2South1, Direction.South2East1, Direction.South2West1, Direction.West2South1, Direction.West2North1, Direction.North2West1 };
            for (var square = 0; square < 64; square++)
            {
                // Create inner ring mask.
                Direction direction;
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < innerRingDirections.Length; directionIndex++)
                {
                    direction = innerRingDirections[directionIndex];
                    var otherSquare = _neighborSquares[square][(int) direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                innerRingMasks[square] = mask;
                // Create outer ring mask from the inner ring directions (distance = 2) plus the outer ring directions (knight moves).
                mask = 0;
                for (var directionIndex = 0; directionIndex < innerRingDirections.Length; directionIndex++)
                {
                    direction = innerRingDirections[directionIndex];
                    var otherSquare = _neighborSquares[square][(int) direction];
                    if (otherSquare != Square.Illegal)
                    {
                        otherSquare = _neighborSquares[otherSquare][(int) direction];
                        if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                    }
                }
                for (var directionIndex = 0; directionIndex < outerRingDirections.Length; directionIndex++)
                {
                    direction = outerRingDirections[directionIndex];
                    var otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                outerRingMasks[square] = mask;
            }
            return (innerRingMasks, outerRingMasks);
        }


        private static ulong[] CreateWhitePawnShieldMasks()
        {
            var pawnShieldMasks = new ulong[64];
            Direction[] directions = { Direction.NorthWest, Direction.North, Direction.NorthEast };
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                pawnShieldMasks[square] = mask;
            }
            return pawnShieldMasks;
        }


        private static ulong[] CreateBlackPawnShieldMasks()
        {
            var pawnShieldMasks = new ulong[64];
            Direction[] directions = { Direction.SouthWest, Direction.South, Direction.SouthEast };
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                pawnShieldMasks[square] = mask;
            }
            return pawnShieldMasks;
        }


        private static ulong[] CreateKnightMoveMasks()
        {
            var attackMasks = new ulong[64];
            Direction[] directions = { Direction.North2East1, Direction.East2North1, Direction.East2South1, Direction.South2East1, Direction.South2West1, Direction.West2South1, Direction.West2North1, Direction.North2West1 };
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                attackMasks[square] = mask;
            }
            return attackMasks;
        }


        private static ulong[] CreateBishopMoveMasks()
        {
            var moveMasks = new ulong[64];
            Direction[] directions = { Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest };
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var otherSquare = square;
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


        private static ulong[] CreateRookMoveMasks()
        {
            var moveMasks = new ulong[64];
            Direction[] directions = { Direction.North, Direction.East, Direction.South, Direction.West };
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var otherSquare = square;
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


        private static ulong[] CreateKingMoveMasks()
        {
            var attackMasks = new ulong[64];
            Direction[] directions = { Direction.North, Direction.NorthEast, Direction.East, Direction.SouthEast, Direction.South, Direction.SouthWest, Direction.West, Direction.NorthWest };
            for (var square = 0; square < 64; square++)
            {
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var otherSquare = _neighborSquares[square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                attackMasks[square] = mask;
            }
            return attackMasks;
        }


        public static ulong CreateMoveDestinationsMask(int square, ulong occupancy, Direction[] directions)
        {
            var moveDestinations = 0ul;
            for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
            {
                var direction = directions[directionIndex];
                var otherSquare = square;
                while (true)
                {
                    otherSquare = _neighborSquares[otherSquare][(int)direction];
                    if (otherSquare == Square.Illegal) break;
                    Bitwise.SetBit(ref moveDestinations, otherSquare);
                    if (Bitwise.IsBitSet(occupancy, otherSquare)) break; // Square is occupied.
                }
            }
            return moveDestinations;
        }


        public static int GetSquare(int file, int rank)
        {
            Debug.Assert(file >= 0 && file < 8);
            Debug.Assert(rank >= 0 && rank < 8);
            return file + (7 - rank) * 8;
        }


        public static int GetSquare(string square)
        {
            Debug.Assert(square.Length == 2, $"Square = {square}");
            var fileChar = square[0];
            var rankChar = square[1];
            var file = fileChar - 97;
            var rank = rankChar - 49;
            Debug.Assert(file >= 0 && file < 8);
            Debug.Assert(rank >= 0 && rank < 8);
            return GetSquare(file, rank);
        }


        public static int GetBlackSquare(int square) => 63 - square;


        private static int GetShortestDistance(int square, int[] otherSquares)
        {
            var shortestDistance = int.MaxValue;
            for (var index = 0; index < otherSquares.Length; index++)
            {
                var otherSquare = otherSquares[index];
                var distance = SquareDistances[square][otherSquare];
                if (distance < shortestDistance) shortestDistance = distance;
            }
            return shortestDistance;
        }


        public static ulong GetKnightDestinations(Position position, int fromSquare, bool white)
        {
            var unOrEnemyOccupiedSquares = white
                ? ~position.OccupancyWhite
                : ~position.OccupancyBlack;
            return KnightMoveMasks[fromSquare] & unOrEnemyOccupiedSquares;
        }


        public static ulong GetBishopDestinations(Position position, int fromSquare, bool white)
        {
            var unOrEnemyOccupiedSquares = white
                ? ~position.OccupancyWhite
                : ~position.OccupancyBlack;
            var occupancy = BishopMoveMasks[fromSquare] & position.Occupancy;
            return PrecalculatedMoves.GetBishopMovesMask(fromSquare, occupancy) & unOrEnemyOccupiedSquares;
        }


        public static ulong GetRookDestinations(Position position, int fromSquare, bool white)
        {
            var unOrEnemyOccupiedSquares = white
                ? ~position.OccupancyWhite
                : ~position.OccupancyBlack;
            var occupancy = RookMoveMasks[fromSquare] & position.Occupancy;
            return PrecalculatedMoves.GetRookMovesMask(fromSquare, occupancy) & unOrEnemyOccupiedSquares;
        }


        public static ulong GetQueenDestinations(Position position, int fromSquare, bool white)
        {
            var unOrEnemyOccupiedSquares = white
                ? ~position.OccupancyWhite
                : ~position.OccupancyBlack;
            var bishopOccupancy = BishopMoveMasks[fromSquare] & position.Occupancy;
            var rookOccupancy = RookMoveMasks[fromSquare] & position.Occupancy;
            return (PrecalculatedMoves.GetBishopMovesMask(fromSquare, bishopOccupancy) | PrecalculatedMoves.GetRookMovesMask(fromSquare, rookOccupancy)) & unOrEnemyOccupiedSquares;
        }


        public void SetPosition(string fen, bool preserveMoveCount = false)
        {
            var fenTokens = Tokens.Parse(fen, ' ', '"');
            if (fenTokens.Count < 4) throw new ArgumentException($"FEN has only {fenTokens.Count}  fields.");
            Reset(preserveMoveCount);
            // Place pieces on board.
            var fenPosition = Tokens.Parse(fenTokens[0], '/', '"');
            var square = Square.a8;
            for (var fenPositionIndex = 0; fenPositionIndex < fenPosition.Count; fenPositionIndex++)
            {
                var rank = fenPosition[fenPositionIndex];
                for (var rankIndex = 0; rankIndex < rank.Length; rankIndex++)
                {
                    var piece = rank[rankIndex];
                    if (char.IsNumber(piece))
                    {
                        // Empty Squares
                        var emptySquares = int.Parse(piece.ToString());
                        square += emptySquares - 1;
                    }
                    else AddPiece(Piece.ParseChar(piece), square);
                    square++;
                }
            }
            // Set side to move, castling rights, en passant square, ply, and full move number.
            CurrentPosition.WhiteMove = fenTokens[1].Equals("w");
            Castling.SetWhiteKingside(ref CurrentPosition.Castling, fenTokens[2].IndexOf("K") > -1);
            Castling.SetWhiteQueenside(ref CurrentPosition.Castling, fenTokens[2].IndexOf("Q") > -1);
            Castling.SetBlackKingside(ref CurrentPosition.Castling, fenTokens[2].IndexOf("k") > -1);
            Castling.SetBlackQueenside(ref CurrentPosition.Castling, fenTokens[2].IndexOf("q") > -1);
            CurrentPosition.EnPassantSquare = fenTokens[3] == "-" ? Square.Illegal : GetSquare(fenTokens[3]);
            CurrentPosition.PlySinceCaptureOrPawnMove = fenTokens.Count == 6 ? int.Parse(fenTokens[4]) : 0;
            CurrentPosition.FullMoveNumber = fenTokens.Count == 6 ? int.Parse(fenTokens[5]) : 1;
            // Determine if king is in check and set position key.
            PlayNullMove();
            var kingSquare = CurrentPosition.WhiteMove
                ? Bitwise.FindFirstSetBit(CurrentPosition.BlackKing)
                : Bitwise.FindFirstSetBit(CurrentPosition.WhiteKing);
            var kingInCheck = IsSquareAttacked(kingSquare);
            UndoMove();
            CurrentPosition.KingInCheck = kingInCheck;
            CurrentPosition.Key = GetPositionKey();
        }


        public bool ValidateMove(ref ulong move)
        {
            // Don't trust move that wasn't generated by engine (from cache, game notation, input by user, etc).
            // Validate main aspects of the move.  Don't test for every impossibility.
            // Goal is to prevent engine crashes, not ensure a perfectly legal search tree.
            var fromSquare = Move.From(move);
            var toSquare = Move.To(move);
            var attacker = CurrentPosition.GetPiece(fromSquare);
            if (attacker == Piece.None) return false; // No piece on from square.
            var attackerWhite = Piece.IsWhite(attacker);
            if (CurrentPosition.WhiteMove != attackerWhite) return false; // Piece is wrong color.
            var victim = CurrentPosition.GetPiece(toSquare);
            if ((victim != Piece.None) && (attackerWhite == Piece.IsWhite(victim))) return false; // Piece cannot attack its own color.
            if ((victim == Piece.WhiteKing) || (victim == Piece.BlackKing)) return false;  // Piece cannot attack king.
            var promotedPiece = Move.PromotedPiece(move);
            if ((promotedPiece != Piece.None) && (CurrentPosition.WhiteMove != Piece.IsWhite(promotedPiece))) return false; // Promoted piece is wrong color.
            var distance = SquareDistances[fromSquare][toSquare];
            if (distance > 1)
            {
                // For sliding pieces, validate to square is reachable and not blocked.
                ulong betweenSquares;
                switch (attacker)
                {
                    case Piece.WhiteBishop:
                        betweenSquares = DiagonalBetweenSquares[fromSquare][toSquare];
                        if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                        break;
                    case Piece.WhiteRook:
                        betweenSquares = RankFileBetweenSquares[fromSquare][toSquare];
                        if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                        break;
                    case Piece.WhiteQueen:
                        betweenSquares = DiagonalBetweenSquares[fromSquare][toSquare];
                        if (betweenSquares == 0) betweenSquares = RankFileBetweenSquares[fromSquare][toSquare];
                        if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                        break;
                    case Piece.BlackBishop:
                        betweenSquares = DiagonalBetweenSquares[fromSquare][toSquare];
                        if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                        break;
                    case Piece.BlackRook:
                        betweenSquares = RankFileBetweenSquares[fromSquare][toSquare];
                        if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                        break;
                    case Piece.BlackQueen:
                        betweenSquares = DiagonalBetweenSquares[fromSquare][toSquare];
                        if (betweenSquares == 0) betweenSquares = RankFileBetweenSquares[fromSquare][toSquare];
                        if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                        break;
                }
            }
            int pawn;
            int king;
            int enPassantVictim;
            if (CurrentPosition.WhiteMove)
            {
                // White Move
                pawn = Piece.WhitePawn;
                king = Piece.WhiteKing;
                enPassantVictim = Piece.BlackPawn;
            }
            else
            {
                // Black Move
                pawn = Piece.BlackPawn;
                king = Piece.BlackKing;
                enPassantVictim = Piece.WhitePawn;
            }
            if ((promotedPiece != Piece.None) && (attacker != pawn)) return false; // Only pawns can promote.
            if ((promotedPiece == pawn) || (promotedPiece == king)) return false; // Cannot promote pawn to pawn or king.
            var castling = (attacker == king) && (distance == 2);
            if (castling)
            {
                // ReSharper disable ConvertIfStatementToSwitchStatement
                if (CurrentPosition.WhiteMove)
                {
                    // White Castling
                    if ((toSquare != Square.c1) && (toSquare != Square.g1)) return false; // Castle destination square invalid.
                    if (toSquare == Square.c1)
                    {
                        // Castle Queenside
                        if (!Castling.WhiteQueenside(CurrentPosition.Castling)) return false; // Castle not possible.
                        if ((CurrentPosition.Occupancy & WhiteCastleQEmptySquaresMask) > 0) return false; // Castle squares occupied.
                    }
                    else
                    {
                        // Castle Kingside
                        if (!Castling.WhiteKingside(CurrentPosition.Castling)) return false; // Castle not possible.
                        if ((CurrentPosition.Occupancy & WhiteCastleKEmptySquaresMask) > 0) return false; // Castle squares occupied.
                    }
                }
                else
                {
                    // Black Castling
                    if ((toSquare != Square.c8) && (toSquare != Square.g8)) return false; // Castle destination square invalid.
                    if (toSquare == Square.c8)
                    {
                        // Castle Queenside
                        if (!Castling.BlackQueenside(CurrentPosition.Castling)) return false; // Castle not possible.
                        if ((CurrentPosition.Occupancy & BlackCastleQEmptySquaresMask) > 0) return false; // Castle squares occupied.
                    }
                    else
                    {
                        // Castle Kingside
                        if (!Castling.BlackKingside(CurrentPosition.Castling)) return false; // Castle not possible.
                        if ((CurrentPosition.Occupancy & BlackCastleKEmptySquaresMask) > 0) return false; // Castle squares occupied.
                    }
                }
                // ReSharper restore ConvertIfStatementToSwitchStatement
            }
            // Set move properties.
            var capture = victim != Piece.None;
            if (capture) Move.SetCaptureAttacker(ref move, attacker);
            Move.SetIsCastling(ref move, castling);
            Move.SetIsKingMove(ref move, attacker == king);
            var enPassantCapture = (attacker == pawn) && (toSquare == CurrentPosition.EnPassantSquare);
            Move.SetIsEnPassantCapture(ref move, enPassantCapture);
            if (enPassantCapture) Move.SetCaptureVictim(ref move, enPassantVictim);
            else Move.SetCaptureVictim(ref move, victim);
            Move.SetIsDoublePawnMove(ref move, (attacker == pawn) && (distance == 2));
            Move.SetIsPawnMove(ref move, attacker == pawn);
            return true;
        }


        public bool IsMoveLegal(ref ulong move)
        {
            if (Move.IsCastling(move) && CurrentPosition.KingInCheck) return false;
            var fromSquare = Move.From(move);
            if (!CurrentPosition.KingInCheck && !Move.IsKingMove(move) && !Move.IsEnPassantCapture(move))
            {
                if ((SquareMasks[fromSquare] & CurrentPosition.PinnedPieces) == 0)
                {
                    // Move cannot expose king to check.
                    PlayMove(move);
                    goto ChecksEnemyKing;
                }
            }
            // Determine if moving piece exposes king to check.
            PlayMove(move);
            var kingSquare = CurrentPosition.WhiteMove
                ? Bitwise.FindFirstSetBit(CurrentPosition.BlackKing)
                : Bitwise.FindFirstSetBit(CurrentPosition.WhiteKing);
            if (IsSquareAttacked(kingSquare))
            {
                UndoMove();
                return false;
            }
            if (Move.IsCastling(move) && IsCastlePathAttacked(move))
            {
                UndoMove();
                return false;
            }
            ChecksEnemyKing:
            // Move is legal.
            // Change side to move.
            var kingInCheck = CurrentPosition.KingInCheck;
            var enPassantSquare = CurrentPosition.EnPassantSquare;
            CurrentPosition.WhiteMove = !CurrentPosition.WhiteMove;
            CurrentPosition.KingInCheck = false;
            CurrentPosition.EnPassantSquare = Square.Illegal;
            // Determine if move checks enemy king.
            kingSquare = CurrentPosition.WhiteMove
                ? Bitwise.FindFirstSetBit(CurrentPosition.BlackKing)
                : Bitwise.FindFirstSetBit(CurrentPosition.WhiteKing);
            var check = IsSquareAttacked(kingSquare);
            // Revert side to move.
            CurrentPosition.WhiteMove = !CurrentPosition.WhiteMove;
            CurrentPosition.KingInCheck = kingInCheck;
            CurrentPosition.EnPassantSquare = enPassantSquare;
            // Set check property and undo move.
            Move.SetIsCheck(ref move, check);
            UndoMove();
            return true;
        }


        private bool IsCastlePathAttacked(ulong move)
        {
            var toSquare = Move.To(move);
            ulong attackedSquaresMask;
            if (CurrentPosition.WhiteMove)
            {
                // Black castled, now white move.
                attackedSquaresMask = toSquare switch
                {
                    Square.c8 => _blackCastleQAttackedSquareMask,
                    Square.g8 => _blackCastleKAttackedSquareMask,
                    _ => throw new Exception($"Black king cannot castle to {SquareLocations[toSquare]}.")
                };
            }
            else
            {
                // White castled, now black move.
                attackedSquaresMask = toSquare switch
                {
                    Square.c1 => _whiteCastleQAttackedSquareMask,
                    Square.g1 => _whiteCastleKAttackedSquareMask,
                    _ => throw new Exception($"White king cannot castle to {SquareLocations[toSquare]}.")
                };
            }
            while ((toSquare = Bitwise.FindFirstSetBit(attackedSquaresMask)) != Square.Illegal)
            {
                if (IsSquareAttacked(toSquare)) return true;
                Bitwise.ClearBit(ref attackedSquaresMask, toSquare);
            }
            return false;
        }


        private bool IsSquareAttacked(int square)
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
                // White Move
                pawns = CurrentPosition.WhitePawns;
                pawnAttackMask = BlackPawnAttackMasks[square]; // Attacked by white pawn masks = black pawn attack masks.
                knights = CurrentPosition.WhiteKnights;
                bishops = CurrentPosition.WhiteBishops;
                rooks = CurrentPosition.WhiteRooks;
                queens = CurrentPosition.WhiteQueens;
                king = CurrentPosition.WhiteKing;
            }
            else
            {
                // Black Move
                pawns = CurrentPosition.BlackPawns;
                pawnAttackMask = WhitePawnAttackMasks[square];  // Attacked by black pawn masks = white pawn attack masks.
                knights = CurrentPosition.BlackKnights;
                bishops = CurrentPosition.BlackBishops;
                rooks = CurrentPosition.BlackRooks;
                queens = CurrentPosition.BlackQueens;
                king = CurrentPosition.BlackKing;
            }
            // Determine if square is attacked by pawns or knights.
            if ((pawnAttackMask & pawns) > 0) return true;
            if ((KnightMoveMasks[square] & knights) > 0) return true;
            // Determine if square is attacked by diagonal sliding piece.
            var bishopDestinations = PrecalculatedMoves.GetBishopMovesMask(square, CurrentPosition.Occupancy);
            if ((bishopDestinations & (bishops | queens)) > 0) return true;
            // Determine if square is attacked by file / rank sliding pieces.
            var rookDestinations = PrecalculatedMoves.GetRookMovesMask(square, CurrentPosition.Occupancy);
            if ((rookDestinations & (rooks | queens)) > 0) return true;
            // Determine if square is attacked by king.
            return (KingMoveMasks[square] & king) > 0;
        }


        public void PlayMove(ulong move)
        {
            Debug.Assert(Move.IsValid(move));
            Debug.Assert(AssertMoveIntegrity(move));
            CurrentPosition.PlayedMove = move;
            // Advance position index.
            NextPosition.Set(CurrentPosition);
            _positionIndex++;
            var fromSquare = Move.From(move);
            var toSquare = Move.To(move);
            var piece = CurrentPosition.GetPiece(fromSquare);
            int captureVictim;
            if (Move.IsCastling(move))
            {
                // Castle
                captureVictim = Piece.None;
                Castle(piece, toSquare);
            }
            else if (Move.IsEnPassantCapture(move))
            {
                // En Passant Capture
                captureVictim = Move.CaptureVictim(move);
                EnPassantCapture(piece, fromSquare);
            }
            else
            {
                // Remove capture victim (none if destination square is unoccupied) and move piece.
                captureVictim = RemovePiece(toSquare);
                Debug.Assert(AssertKingIsNotCaptured(captureVictim, move));
                RemovePiece(fromSquare);
                var promotedPiece = Move.PromotedPiece(move);
                AddPiece(promotedPiece == Piece.None ? piece : promotedPiece, toSquare);
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
            CurrentPosition.EnPassantSquare = Move.IsDoublePawnMove(move) ? _enPassantTargetSquares[toSquare] : Square.Illegal;
            if ((captureVictim != Piece.None) || Move.IsPawnMove(move)) CurrentPosition.PlySinceCaptureOrPawnMove = 0;
            else CurrentPosition.PlySinceCaptureOrPawnMove++;
            if (!CurrentPosition.WhiteMove) CurrentPosition.FullMoveNumber++;
            CurrentPosition.WhiteMove = !CurrentPosition.WhiteMove;
            CurrentPosition.KingInCheck = Move.IsCheck(move);
            CurrentPosition.Key = GetPositionKey();
            Nodes++;
            Debug.Assert(AssertIntegrity());
        }


        private bool AssertMoveIntegrity(ulong move)
        {
            var fromSquare = Move.From(move);
            var toSquare = Move.To(move);
            var piece = CurrentPosition.GetPiece(fromSquare);
            int pawn;
            int king;
            int toRank;
            // EnPassantVictim variable only used in Debug builds.
            // ReSharper disable RedundantAssignment
            int enPassantVictim;
            if (CurrentPosition.WhiteMove)
            {
                // White Move
                pawn = Piece.WhitePawn;
                king = Piece.WhiteKing;
                toRank = WhiteRanks[toSquare];
                enPassantVictim = Piece.BlackPawn;
            }
            else
            {
                // Black Move
                pawn = Piece.BlackPawn;
                king = Piece.BlackKing;
                toRank = BlackRanks[toSquare];
                enPassantVictim = Piece.WhitePawn;
            }
            var captureVictim = CurrentPosition.GetPiece(toSquare);
            var enPassantCapture = (CurrentPosition.EnPassantSquare != Square.Illegal) && (piece == pawn) && (toSquare == CurrentPosition.EnPassantSquare);
            Debug.Assert(Move.IsEnPassantCapture(move) == enPassantCapture, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
            if (enPassantCapture) Debug.Assert(Move.CaptureVictim(move) == enPassantVictim, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
            else Debug.Assert(Move.CaptureVictim(move) == captureVictim, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
            var pawnPromotion = (piece == pawn) && (toRank == 7);
            if (pawnPromotion) Debug.Assert(Move.PromotedPiece(move) != Piece.None, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
            else Debug.Assert(Move.PromotedPiece(move) == Piece.None, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
            var castling = (piece == king) && (SquareDistances[fromSquare][toSquare] == 2);
            Debug.Assert(Move.IsCastling(move) == castling, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
            var kingMove = piece == king;
            Debug.Assert(Move.IsKingMove(move) == kingMove, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
            var doublePawnMove = (piece == pawn) && (SquareDistances[fromSquare][toSquare] == 2);
            Debug.Assert(Move.IsDoublePawnMove(move) == doublePawnMove, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
            var pawnMove = piece == pawn;
            Debug.Assert(Move.IsPawnMove(move) == pawnMove, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
            // ReSharper restore RedundantAssignment
            return true;
        }


        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private bool AssertKingIsNotCaptured(int captureVictim, ulong move)
        {
            if ((captureVictim == Piece.WhiteKing) || (captureVictim == Piece.BlackKing))
            {
                _positionIndex--;
                _writeMessageLine($"Previous position = {PreviousPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(PreviousPosition.PlayedMove)}{Environment.NewLine}{PreviousPosition}");
                _writeMessageLine(PreviousPosition.ToString());
                _writeMessageLine(null);
                Debug.Assert(captureVictim != Piece.WhiteKing, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
                Debug.Assert(captureVictim != Piece.BlackKing, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
            }
            return true;
        }


        private void Castle(int piece, int toSquare)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (piece == Piece.WhiteKing)
                switch (toSquare)
                {
                    case Square.c1:
                        // White Castle Queenside
                        RemovePiece(Square.e1);
                        AddPiece(Piece.WhiteKing, Square.c1);
                        RemovePiece(Square.a1);
                        AddPiece(Piece.WhiteRook, Square.d1);
                        break;
                    case Square.g1:
                        // White Castle Kingside
                        RemovePiece(Square.e1);
                        AddPiece(Piece.WhiteKing, Square.g1);
                        RemovePiece(Square.h1);
                        AddPiece(Piece.WhiteRook, Square.f1);
                        break;
                    default:
                        throw new Exception($"White king cannot castle to {SquareLocations[toSquare]}.");
                }
            else if (piece == Piece.BlackKing)
                switch (toSquare)
                {
                    case Square.c8:
                        // Black Castle Queenside
                        RemovePiece(Square.e8);
                        AddPiece(Piece.BlackKing, Square.c8);
                        RemovePiece(Square.a8);
                        AddPiece(Piece.BlackRook, Square.d8);
                        break;
                    case Square.g8:
                        // Black Castle Kingside
                        RemovePiece(Square.e8);
                        AddPiece(Piece.BlackKing, Square.g8);
                        RemovePiece(Square.h8);
                        AddPiece(Piece.BlackRook, Square.f8);
                        break;
                    default:
                        throw new Exception($"Black king cannot castle to {SquareLocations[toSquare]}.");
                }
            else
                throw new Exception($"{piece} piece cannot castle.");
        }


        private void EnPassantCapture(int piece, int fromSquare)
        {
            // Move pawn and remove captured pawn.
            RemovePiece(_enPassantVictimSquares[CurrentPosition.EnPassantSquare]);
            RemovePiece(fromSquare);
            AddPiece(piece, CurrentPosition.EnPassantSquare);
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


        public bool IsRepeatPosition(int repeats)
        {
            var currentPositionKey = CurrentPosition.Key;
            var positionCount = 0;
            // Examine positions since the last capture or pawn move.
            var firstMove = Math.Max(_positionIndex - CurrentPosition.PlySinceCaptureOrPawnMove, 0);
            for (var positionIndex = _positionIndex; positionIndex >= firstMove; positionIndex -= 2) // Advance by two ply to retain same side to move.
            {
                if (_positions[positionIndex].Key == currentPositionKey) positionCount++;
                if (positionCount >= repeats) return true;
            }
            return false;
        }


        public int GetPositionCount()
        {
            var currentPositionKey = CurrentPosition.Key;
            var positionCount = 0;
            // Examine positions since the last capture or pawn move.
            var firstMove = Math.Max(_positionIndex - CurrentPosition.PlySinceCaptureOrPawnMove, 0);
            for (var positionIndex = firstMove; positionIndex <= _positionIndex; positionIndex += 2) // Advance by two ply to retain same side to move.
            {
                if (_positions[positionIndex].Key == currentPositionKey) positionCount++;
            }
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


        private void AddPiece(int piece, int square)
        {
            Debug.Assert(piece != Piece.None);
            // Update piece, color, and both color bitboards.
            var squareMask = SquareMasks[square];
            switch (piece)
            {
                case Piece.WhitePawn:
                    CurrentPosition.WhitePawns |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Piece.WhiteKnight:
                    CurrentPosition.WhiteKnights |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Piece.WhiteBishop:
                    CurrentPosition.WhiteBishops |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Piece.WhiteRook:
                    CurrentPosition.WhiteRooks |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Piece.WhiteQueen:
                    CurrentPosition.WhiteQueens |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Piece.WhiteKing:
                    CurrentPosition.WhiteKing |= squareMask;
                    CurrentPosition.OccupancyWhite |= squareMask;
                    break;
                case Piece.BlackPawn:
                    CurrentPosition.BlackPawns |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
                case Piece.BlackKnight:
                    CurrentPosition.BlackKnights |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
                case Piece.BlackBishop:
                    CurrentPosition.BlackBishops |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
                case Piece.BlackRook:
                    CurrentPosition.BlackRooks |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
                case Piece.BlackQueen:
                    CurrentPosition.BlackQueens |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
                case Piece.BlackKing:
                    CurrentPosition.BlackKing |= squareMask;
                    CurrentPosition.OccupancyBlack |= squareMask;
                    break;
            }
            CurrentPosition.Occupancy |= squareMask;
            UpdatePiecesSquaresKey(piece, square);
        }


        private int RemovePiece(int square)
        {
            var squareUnmask = _squareUnmasks[square];
            var piece = CurrentPosition.GetPiece(square);
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
            UpdatePiecesSquaresKey(piece, square);
            return piece;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePiecesSquaresKey(int piece, int square)
        {
            CurrentPosition.PiecesSquaresKey ^= _pieceSquareKeys[piece][square];
            Debug.Assert(AssertPiecesSquaresKeyIntegrity());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong GetPositionKey()
        {
            var sideToMoveKey = CurrentPosition.WhiteMove ? _sideToMoveKeys[0] : _sideToMoveKeys[1];
            var castlingKey = _castlingKeys[CurrentPosition.Castling];
            var enPassantKey = CurrentPosition.EnPassantSquare == Square.Illegal ? _enPassantKeys[0] : _enPassantKeys[CurrentPosition.EnPassantSquare];
            return CurrentPosition.PiecesSquaresKey ^ sideToMoveKey ^ castlingKey ^ enPassantKey;
        }


        private bool AssertPiecesSquaresKeyIntegrity()
        {
            // Verify incrementally updated pieces squares key matches fully updated pieces squares key.
            var fullyUpdatedPiecesSquaresKey = _piecesSquaresInitialKey;
            for (var square = 0; square < 64; square++)
            {
                var piece = CurrentPosition.GetPiece(square);
                if (piece != Piece.None) fullyUpdatedPiecesSquaresKey ^= _pieceSquareKeys[piece][square];
            }
            return fullyUpdatedPiecesSquaresKey == CurrentPosition.PiecesSquaresKey;
        }


        private void Reset(bool preserveMoveCount)
        {
            // Reset position index, position, key, and stats.
            _positionIndex = 0;
            CurrentPosition.Reset();
            CurrentPosition.PiecesSquaresKey = _piecesSquaresInitialKey;
            if (!preserveMoveCount)
            {
                // Reset nodes.
                Nodes = 0;
                NodesInfoUpdate = UciStream.NodesInfoInterval;
            }
        }


        public override string ToString() => CurrentPosition.ToString();
    }
}