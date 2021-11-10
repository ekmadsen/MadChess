// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Core.Game;


public sealed class Board
{
    public const string StartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public static readonly int[] Files; // [square]
    public static readonly int[][] Ranks; // [color][square]
    public static readonly Color[] SquareColors; // [square]
    public static readonly int[][] SquareDistances; // [square1][square2]
    public static readonly int[] DistanceToCentralSquares; // [square]
    public static readonly int[] DistanceToNearestCorner; // [square]
    public static readonly int[][] DistanceToNearestCornerOfColor; // [color][square]
    public static readonly string[] SquareLocations; // [square]
    public static readonly ulong[] SquareMasks; // [square]
    public static readonly ulong[] FileMasks; // [file]
    public static readonly ulong[] WhiteRankMasks; // [rank]
    public static readonly ulong AllSquaresMask;
    public static readonly ulong EdgeSquaresMask;
    public static readonly ulong[][] CastleEmptySquaresMask; // [color][boardSide]
    public static readonly Square[] CastleFromSquares; // [color]
    public static readonly Square[][] CastleToSquares; // [color][boardSide]
    public static readonly ulong[][] PawnMoveMasks; // [color][square]
    public static readonly ulong[][] PawnDoubleMoveMasks; // [color][square]
    public static readonly ulong[][] PawnAttackMasks; // [color][square]
    public static readonly ulong[] KnightMoveMasks; // [square]
    public static readonly ulong[] BishopMoveMasks; // [square]
    public static readonly ulong[] RookMoveMasks; // [square]
    public static readonly ulong[] KingMoveMasks; // [square]
    public static readonly Delegates.GetPieceMovesMask[] PieceMoveMaskDelegates; // [colorlessPiece]
    public static readonly ulong[] EnPassantAttackerMasks; // [square]
    public static readonly ulong[][] PassedPawnMasks; // [color][square]
    public static readonly ulong[][] FreePawnMasks; // [color][square]
    public static readonly ulong[] InnerRingMasks; // [square]
    public static readonly ulong[] OuterRingMasks; // [square]
    public static readonly ulong[][] PawnShieldMasks; // [color][square]
    public static readonly PrecalculatedMoves PrecalculatedMoves;
    public static readonly ulong[][] RankFileBetweenSquares; // [square1][square2]
    public static readonly ulong[][] DiagonalBetweenSquares; // [square1][square2]
    public long Nodes;
    public long NodesInfoUpdate;
    public long NodesExamineTime;
    private const int _maxPositions = 1024;
    private static readonly int[] _squarePerspectiveFactors; // [color]
    private static readonly ulong[] _squareUnmasks; // [square]
    private static readonly ulong[][] _castleAttackedSquareMasks; // [color][boardSide]
    private static readonly int[][] _neighborSquares; // [square][direction]
    private static readonly Square[] _enPassantTargetSquares; // [square]
    private static readonly Square[] _enPassantVictimSquares; // [square]
    private readonly ulong[][] _pieceSquareKeys; // [piece][square]
    private readonly ulong[] _sideToMoveKeys; // [color]
    private readonly ulong[] _castlingKeys; // [castlingRights]
    private readonly ulong[] _enPassantKeys; // [square]
    private readonly Position[] _positions; // [distanceFromRoot]
    private readonly Delegates.WriteMessageLine _writeMessageLine;
    private readonly long _nodesInfoInterval;
    private readonly ulong _piecesSquaresInitialKey;
    private int _positionIndex;


    public Position PreviousPosition => _positionIndex > 0 ? _positions[_positionIndex - 1] : null;


    public Position CurrentPosition => _positions[_positionIndex];


    private Position NextPosition => _positions[_positionIndex + 1];


    // TODO: Split the Board class static constructor into many methods to reduce its length.
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
        Ranks = new[]
        {
            new[]
            {
                7, 7, 7, 7, 7, 7, 7, 7,
                6, 6, 6, 6, 6, 6, 6, 6,
                5, 5, 5, 5, 5, 5, 5, 5,
                4, 4, 4, 4, 4, 4, 4, 4,
                3, 3, 3, 3, 3, 3, 3, 3,
                2, 2, 2, 2, 2, 2, 2, 2,
                1, 1, 1, 1, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 0, 0
            },
            new[]
            {
                0, 0, 0, 0, 0, 0, 0, 0,
                1, 1, 1, 1, 1, 1, 1, 1,
                2, 2, 2, 2, 2, 2, 2, 2,
                3, 3, 3, 3, 3, 3, 3, 3,
                4, 4, 4, 4, 4, 4, 4, 4,
                5, 5, 5, 5, 5, 5, 5, 5,
                6, 6, 6, 6, 6, 6, 6, 6,
                7, 7, 7, 7, 7, 7, 7, 7
            }
        };
        Square[] centralSquares = { Square.D5, Square.E5, Square.D4, Square.E4 };
        Square[] cornerSquares = { Square.A8, Square.H8, Square.A1, Square.H1 };
        Square[][] cornerSquaresOfColor =
        {
            new[] {Square.A8, Square.H1},
            new[] {Square.H8, Square.A1}
        };
        SquareColors = new[]
        {
            Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black,
            Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White,
            Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black,
            Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White,
            Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black,
            Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White,
            Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black,
            Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White
        };
        // Determine distances between squares.
        SquareDistances = new int[64][];
        for (var square1 = 0; square1 < 64; square1++)
        {
            SquareDistances[square1] = new int[64];
            for (var square2 = 0; square2 < 64; square2++)
            {
                var fileDistance = Math.Abs(Files[square1] - Files[square2]);
                var rankDistance = Math.Abs(Ranks[(int)Color.White][square1] - Ranks[(int)Color.White][square2]);
                SquareDistances[square1][square2] = Math.Max(fileDistance, rankDistance);
            }
        }
        // Determine distances to central and nearest corner squares.
        DistanceToCentralSquares = new int[64];
        DistanceToNearestCorner = new int[64];
        for (var square = Square.A8; square < Square.Illegal; square++)
        {
            DistanceToCentralSquares[(int)square] = GetShortestDistance(square, centralSquares);
            DistanceToNearestCorner[(int)square] = GetShortestDistance(square, cornerSquares);
        }
        DistanceToNearestCornerOfColor = new int[2][];
        for (var color = Color.White; color <= Color.Black; color++)
        {
            DistanceToNearestCornerOfColor[(int)color] = new int[64];
            for (var square = Square.A8; square < Square.Illegal; square++)
            {
                DistanceToNearestCornerOfColor[(int)color][(int)square] = GetShortestDistance(square, cornerSquaresOfColor[(int)color]);
            }
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
        // Square perspective factors are used to determine square from white's perspective (black's g6 = white's b3).
        _squarePerspectiveFactors = new[] { -1, 1 };
        // Create square, file, rank, diagonal, and edge masks.
        SquareMasks = new ulong[64];
        _squareUnmasks = new ulong[64];
        for (var square = Square.A8; square < Square.Illegal; square++)
        {
            SquareMasks[(int)square] = Bitwise.CreateULongMask(square);
            _squareUnmasks[(int)square] = Bitwise.CreateULongUnmask(square);
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
        WhiteRankMasks = new ulong[8];
        for (var rank = 0; rank < 8; rank++)
        {
            WhiteRankMasks[rank] = 0;
            for (var file = 0; file < 8; file++)
            {
                var square = GetSquare(file, rank);
                WhiteRankMasks[rank] |= Bitwise.CreateULongMask(square);
            }
        }
        AllSquaresMask = Bitwise.CreateULongMask(0, 63);
        EdgeSquaresMask = FileMasks[0] | WhiteRankMasks[7] | FileMasks[7] | WhiteRankMasks[0];
        // Create castling masks.
        CastleEmptySquaresMask = new[]
        {
            new[]
            {
                Bitwise.CreateULongMask(new[] { Square.B1, Square.C1, Square.D1 }),
                Bitwise.CreateULongMask(new[] { Square.F1, Square.G1 })
            },
            new[]
            {
                Bitwise.CreateULongMask(new[] { Square.B8, Square.C8, Square.D8 }),
                Bitwise.CreateULongMask(new[] { Square.F8, Square.G8 })
            }
        };
        _castleAttackedSquareMasks = new[]
        {
            new[]
            {
                Bitwise.CreateULongMask(Square.D1),
                Bitwise.CreateULongMask(Square.F1)
            },
            new[]
            {
                Bitwise.CreateULongMask(Square.D8),
                Bitwise.CreateULongMask(Square.F8)
            }
        };
        CastleFromSquares = new[] { Square.E1, Square.E8 };
        CastleToSquares = new[]
        {
            new[]
            {
                Square.C1,
                Square.G1
            },
            new[]
            {
                Square.C8,
                Square.G8
            }
        };

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
        PawnMoveMasks = CreatePawnMoveMasks();
        PawnDoubleMoveMasks = CreatePawnDoubleMoveMasks();
        PawnAttackMasks = CreatePawnAttackMasks();
        KnightMoveMasks = CreateKnightMoveMasks();
        BishopMoveMasks = CreateBishopMoveMasks();
        RookMoveMasks = CreateRookMoveMasks();
        KingMoveMasks = CreateKingMoveMasks();
        PieceMoveMaskDelegates = new Delegates.GetPieceMovesMask[(int)ColorlessPiece.Queen + 1];
        PieceMoveMaskDelegates[(int)ColorlessPiece.Knight] = GetKnightDestinations;
        PieceMoveMaskDelegates[(int)ColorlessPiece.Bishop] = GetBishopDestinations;
        PieceMoveMaskDelegates[(int)ColorlessPiece.Rook] = GetRookDestinations;
        PieceMoveMaskDelegates[(int) ColorlessPiece.Queen] = GetQueenDestinations;
        PrecalculatedMoves = new PrecalculatedMoves();
        // Determine squares in a rank / file direction between two squares.
        RankFileBetweenSquares = new ulong[64][];
        for (var square1 = Square.A8; square1 < Square.Illegal; square1++)
        {
            RankFileBetweenSquares[(int)square1] = new ulong[64];
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
                    square2 = (Square)_neighborSquares[(int)square2][(int)direction];
                    if (square2 == Square.Illegal) break;
                    if (distance > 1)
                    {
                        betweenSquares |= SquareMasks[(int)previousSquare2];
                        RankFileBetweenSquares[(int)square1][(int)square2] = betweenSquares;
                    }
                    previousSquare2 = square2;
                    distance++;
                } while (true);
            }
        }
        // Determine squares in a diagonal direction between two squares.
        DiagonalBetweenSquares = new ulong[64][];
        for (var square1 = Square.A8; square1 < Square.Illegal; square1++)
        {
            DiagonalBetweenSquares[(int)square1] = new ulong[64];
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
                    square2 = (Square)_neighborSquares[(int)square2][(int)direction];
                    if (square2 == Square.Illegal) break;
                    if (distance > 1)
                    {
                        betweenSquares |= SquareMasks[(int)previousSquare2];
                        DiagonalBetweenSquares[(int)square1][(int)square2] = betweenSquares;
                    }
                    previousSquare2 = square2;
                    distance++;
                } while (true);
            }
        }
        // Create en passant, passed pawn, and free pawn masks.
        (_enPassantTargetSquares, _enPassantVictimSquares, EnPassantAttackerMasks) = CreateEnPassantAttackerMasks();
        PassedPawnMasks = CreatePassedPawnMasks();
        FreePawnMasks = CreateFreePawnMasks();
        // Create ring and pawn shield masks.
        (InnerRingMasks, OuterRingMasks) = CreateRingMasks();
        PawnShieldMasks = CreatePawnShieldMasks();
    }


    public Board(Delegates.WriteMessageLine writeMessageLine, long nodesInfoInterval)
    {
        _writeMessageLine = writeMessageLine;
        _nodesInfoInterval = nodesInfoInterval;
        // Create positions and precalculated moves.
        _positions = new Position[_maxPositions];
        for (var positionIndex = 0; positionIndex < _maxPositions; positionIndex++) _positions[positionIndex] = new Position(this);
        // Create Zobrist position keys.
        _piecesSquaresInitialKey = SafeRandom.NextULong();
        _pieceSquareKeys = new ulong[13][];
        for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            _pieceSquareKeys[(int)piece] = new ulong[64];
            for (var square = Square.A8; square < Square.Illegal; square++) _pieceSquareKeys[(int)piece][(int)square] = SafeRandom.NextULong();
        }
        _sideToMoveKeys = new[] { SafeRandom.NextULong(), SafeRandom.NextULong() };
        _castlingKeys = new ulong[16]; // 2 Pow 4 = 16 combinations of castling rights.
        {
            for (var castlingRights = 0; castlingRights < 16; castlingRights++) _castlingKeys[castlingRights] = SafeRandom.NextULong();
        }
        _enPassantKeys = new ulong[(int)Square.Illegal + 1];
        for (var square = Square.A8; square <= Square.Illegal; square++) _enPassantKeys[(int)square] = SafeRandom.NextULong();
        _piecesSquaresInitialKey = SafeRandom.NextULong();
        // Set nodes.
        Nodes = 0;
        NodesInfoUpdate = nodesInfoInterval;
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
            else squareIndices1212To88[square1212] = (int)Square.Illegal;
            square1212++;
        }
        return squareIndices1212To88;
    }


    private static int[][] CreateNeighborSquares(int[] directionOffsets1212, int[] squareIndices1212To88)
    {
        var neighborSquares = new int[64][];
        Square square88;
        for (square88 = Square.A8; square88 < Square.Illegal; square88++) neighborSquares[(int)square88] = new int[(int)Direction.North2West1 + 1];
        for (var square1212 = 0; square1212 < 144; square1212++)
        {
            square88 = (Square)squareIndices1212To88[square1212];
            if (square88 != Square.Illegal)
                for (var direction = 1; direction <= (int)Direction.North2West1; direction++)
                {
                    var directionOffset1212 = directionOffsets1212[direction];
                    neighborSquares[(int)square88][direction] = squareIndices1212To88[square1212 + directionOffset1212];
                }
        }
        return neighborSquares;
    }


    private static ulong[][] CreatePawnMoveMasks()
    {
        var masks = new ulong[2][];
        var directions = new[] { Direction.North, Direction.South };
        for (var color = Color.White; color <= Color.Black; color++)
        {
            masks[(int)color] = new ulong[64];
            var direction = directions[(int)color];
            for (var square = Square.A8; square < Square.Illegal; square++)
            {
                var mask = 0ul;
                var otherSquare = (Square)_neighborSquares[(int)square][(int)direction];
                if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                if (Ranks[(int)color][(int)square] == 1)
                {
                    otherSquare = (Square)_neighborSquares[(int)otherSquare][(int)direction];
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                masks[(int)color][(int)square] = mask;
            }
        }
        return masks;
    }


    private static ulong[][] CreatePawnDoubleMoveMasks()
    {
        var masks = new ulong[2][];
        var directions = new[] { Direction.North, Direction.South };
        for (var color = Color.White; color <= Color.Black; color++)
        {
            masks[(int)color] = new ulong[64];
            var direction = directions[(int) color];
            for (var square = Square.A8; square < Square.Illegal; square++)
            {
                var mask = 0ul;
                if (Ranks[(int)color][(int)square] == 1)
                {
                    var otherSquare = (Square) _neighborSquares[(int)square][(int)direction];
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                masks[(int)color][(int)square] = mask;
            }
        }
        return masks;
    }


    private static ulong[][] CreatePawnAttackMasks()
    {
        var masks = new ulong[2][];
        var colorDirections = new[]
        {
            new[] { Direction.NorthWest, Direction.NorthEast },
            new[] { Direction.SouthWest, Direction.SouthEast }
        };
        for (var color = Color.White; color <= Color.Black; color++)
        {
            masks[(int)color] = new ulong[64];
            var directions = colorDirections[(int)color];
            for (var square = Square.A8; square < Square.Illegal; square++)
            {
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var otherSquare = (Square)_neighborSquares[(int)square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                masks[(int)color][(int)square] = mask;
            }
        }
        return masks;
    }


    private static (Square[] EnPassantTargetSquares, Square[] EnPassantVictimSquares, ulong[] EnPassantAttackerMasks) CreateEnPassantAttackerMasks()
    {
        var enPassantTargetSquares = new Square[64];
        var enPassantVictimSquares = new Square[64];
        var enPassantAttackerMasks = new ulong[64];
        for (var file = 0; file < 8; file++)
        {
            // White takes black pawn en passant.
            var toSquare = GetSquare(file, 4);
            var targetSquare = (Square)_neighborSquares[(int)toSquare][(int)Direction.North];
            enPassantVictimSquares[(int)targetSquare] = (Square)_neighborSquares[(int)targetSquare][(int)Direction.South];
            var westAttackerSquare = (Square)_neighborSquares[(int)targetSquare][(int)Direction.SouthWest];
            var eastAttackerSquare = (Square)_neighborSquares[(int)targetSquare][(int)Direction.SouthEast];
            enPassantTargetSquares[(int)toSquare] = targetSquare;
            var attackerMask = 0ul;
            if (westAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(westAttackerSquare);
            if (eastAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(eastAttackerSquare);
            enPassantAttackerMasks[(int)targetSquare] = attackerMask;
            // Black takes white pawn en passant.
            toSquare = GetSquare(file, 3);
            targetSquare = (Square)_neighborSquares[(int)toSquare][(int)Direction.South];
            enPassantVictimSquares[(int)targetSquare] = (Square)_neighborSquares[(int)targetSquare][(int)Direction.North];
            westAttackerSquare = (Square)_neighborSquares[(int)targetSquare][(int)Direction.NorthWest];
            eastAttackerSquare = (Square)_neighborSquares[(int)targetSquare][(int)Direction.NorthEast];
            enPassantTargetSquares[(int)toSquare] = targetSquare;
            attackerMask = 0;
            if (westAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(westAttackerSquare);
            if (eastAttackerSquare != Square.Illegal) attackerMask |= Bitwise.CreateULongMask(eastAttackerSquare);
            enPassantAttackerMasks[(int)targetSquare] = attackerMask;
        }
        return (enPassantTargetSquares, enPassantVictimSquares, enPassantAttackerMasks);
    }


    private static ulong[][] CreatePassedPawnMasks()
    {
        var masks = new ulong[2][];
        var directions = new[] { Direction.North, Direction.South };
        for (var color = Color.White; color <= Color.Black; color++)
        {
            masks[(int)color] = new ulong[64];
            var direction = directions[(int)color];
            for (var square = Square.A8; square < Square.Illegal; square++)
            {
                var mask = 0ul;
                Square[] startingSquares =
                {
                    (Square)_neighborSquares[(int)square][(int)Direction.West],
                    square,
                    (Square)_neighborSquares[(int)square][(int)Direction.East]
                };
                for (var index = 0; index < startingSquares.Length; index++)
                {
                    var otherSquare = startingSquares[index];
                    while (otherSquare != Square.Illegal)
                    {
                        otherSquare = (Square)_neighborSquares[(int)otherSquare][(int)direction];
                        if (otherSquare == Square.Illegal) break;
                        Bitwise.SetBit(ref mask, otherSquare);
                    }
                }
                masks[(int)color][(int)square] = mask;
            }
        }
        return masks;
    }


    private static ulong[][] CreateFreePawnMasks()
    {
        var masks = new ulong[2][];
        var directions = new[] { Direction.North, Direction.South };
        for (var color = Color.White; color <= Color.Black; color++)
        {
            masks[(int)color] = new ulong[64];
            var direction = directions[(int)color];
            for (var square = Square.A8; square < Square.Illegal; square++)
            {
                var mask = 0ul;
                var otherSquare = square;
                while (true)
                {
                    otherSquare = (Square)_neighborSquares[(int)otherSquare][(int)direction];
                    if (otherSquare == Square.Illegal) break;
                    Bitwise.SetBit(ref mask, otherSquare);
                }
                masks[(int)color][(int)square] = mask;
            }
        }
        return masks;
    }


    private static (ulong[] InnerRingMasks, ulong[] OuterRingMasks) CreateRingMasks()
    {
        var innerRingMasks = new ulong[64];
        var outerRingMasks = new ulong[64];
        Direction[] innerRingDirections = { Direction.North, Direction.NorthEast, Direction.East, Direction.SouthEast, Direction.South, Direction.SouthWest, Direction.West, Direction.NorthWest };
        Direction[] outerRingDirections = { Direction.North2East1, Direction.East2North1, Direction.East2South1, Direction.South2East1, Direction.South2West1, Direction.West2South1, Direction.West2North1, Direction.North2West1 };
        for (var square = Square.A8; square < Square.Illegal; square++)
        {
            // Create inner ring mask.
            Direction direction;
            var mask = 0ul;
            for (var directionIndex = 0; directionIndex < innerRingDirections.Length; directionIndex++)
            {
                direction = innerRingDirections[directionIndex];
                var otherSquare = (Square)_neighborSquares[(int)square][(int)direction];
                if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
            }
            innerRingMasks[(int)square] = mask;
            // Create outer ring mask from the inner ring directions (distance = 2) plus the outer ring directions (knight moves).
            mask = 0;
            for (var directionIndex = 0; directionIndex < innerRingDirections.Length; directionIndex++)
            {
                direction = innerRingDirections[directionIndex];
                var otherSquare = (Square)_neighborSquares[(int)square][(int)direction];
                if (otherSquare != Square.Illegal)
                {
                    otherSquare = (Square)_neighborSquares[(int)otherSquare][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
            }
            for (var directionIndex = 0; directionIndex < outerRingDirections.Length; directionIndex++)
            {
                direction = outerRingDirections[directionIndex];
                var otherSquare = (Square)_neighborSquares[(int)square][(int)direction];
                if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
            }
            outerRingMasks[(int)square] = mask;
        }
        return (innerRingMasks, outerRingMasks);
    }


    private static ulong[][] CreatePawnShieldMasks()
    {
        var masks = new ulong[2][];
        var colorDirections = new[]
        {
            new[] { Direction.NorthWest, Direction.North, Direction.NorthEast },
            new[] { Direction.SouthWest, Direction.South, Direction.SouthEast}
        };
        for (var color = Color.White; color <= Color.Black; color++)
        {
            masks[(int)color] = new ulong[64];
            var directions = colorDirections[(int)color];
            for (var square = Square.A8; square < Square.Illegal; square++)
            {
                var mask = 0ul;
                for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
                {
                    var direction = directions[directionIndex];
                    var otherSquare = (Square)_neighborSquares[(int)square][(int)direction];
                    if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
                }
                masks[(int)color][(int)square] = mask;
            }
        }
        return masks;
    }


    private static ulong[] CreateKnightMoveMasks()
    {
        var masks = new ulong[64];
        Direction[] directions = { Direction.North2East1, Direction.East2North1, Direction.East2South1, Direction.South2East1, Direction.South2West1, Direction.West2South1, Direction.West2North1, Direction.North2West1 };
        for (var square = Square.A8; square < Square.Illegal; square++)
        {
            var mask = 0ul;
            for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
            {
                var direction = directions[directionIndex];
                var otherSquare = (Square)_neighborSquares[(int)square][(int)direction];
                if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
            }
            masks[(int)square] = mask;
        }
        return masks;
    }


    private static ulong[] CreateBishopMoveMasks()
    {
        var masks = new ulong[64];
        Direction[] directions = { Direction.NorthEast, Direction.SouthEast, Direction.SouthWest, Direction.NorthWest };
        for (var square = Square.A8; square < Square.Illegal; square++)
        {
            var mask = 0ul;
            for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
            {
                var direction = directions[directionIndex];
                var otherSquare = square;
                while (true)
                {
                    otherSquare = (Square)_neighborSquares[(int)otherSquare][(int)direction];
                    if (otherSquare == Square.Illegal) break;
                    Bitwise.SetBit(ref mask, otherSquare);
                }
            }
            masks[(int)square] = mask;
        }
        return masks;
    }


    private static ulong[] CreateRookMoveMasks()
    {
        var masks = new ulong[64];
        Direction[] directions = { Direction.North, Direction.East, Direction.South, Direction.West };
        for (var square = Square.A8; square < Square.Illegal; square++)
        {
            var mask = 0ul;
            for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
            {
                var direction = directions[directionIndex];
                var otherSquare = square;
                while (true)
                {
                    otherSquare = (Square)_neighborSquares[(int)otherSquare][(int)direction];
                    if (otherSquare == Square.Illegal) break;
                    Bitwise.SetBit(ref mask, otherSquare);
                }
            }
            masks[(int)square] = mask;
        }
        return masks;
    }


    private static ulong[] CreateKingMoveMasks()
    {
        var masks = new ulong[64];
        Direction[] directions = { Direction.North, Direction.NorthEast, Direction.East, Direction.SouthEast, Direction.South, Direction.SouthWest, Direction.West, Direction.NorthWest };
        for (var square = Square.A8; square < Square.Illegal; square++)
        {
            var mask = 0ul;
            for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
            {
                var direction = directions[directionIndex];
                var otherSquare = (Square)_neighborSquares[(int)square][(int)direction];
                if (otherSquare != Square.Illegal) Bitwise.SetBit(ref mask, otherSquare);
            }
            masks[(int)square] = mask;
        }
        return masks;
    }


    public static ulong CreateMoveDestinationsMask(Square square, ulong occupancy, Direction[] directions)
    {
        var moveDestinations = 0ul;
        for (var directionIndex = 0; directionIndex < directions.Length; directionIndex++)
        {
            var direction = directions[directionIndex];
            var otherSquare = square;
            while (true)
            {
                otherSquare = (Square)_neighborSquares[(int)otherSquare][(int)direction];
                if (otherSquare == Square.Illegal) break;
                Bitwise.SetBit(ref moveDestinations, otherSquare);
                if (Bitwise.IsBitSet(occupancy, otherSquare)) break; // Square is occupied.
            }
        }
        return moveDestinations;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square GetSquare(int file, int rank)
    {
        Debug.Assert(file >= 0 && file < 8);
        Debug.Assert(rank >= 0 && rank < 8);
        return (Square)(file + (7 - rank) * 8);
    }


    public static Square GetSquare(string square)
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square GetSquareFromWhitePerspective(Square square, Color color) => (Square)((int)color * 63) - ((int)square * _squarePerspectiveFactors[(int)color]);


    private static int GetShortestDistance(Square square, Square[] otherSquares)
    {
        var shortestDistance = int.MaxValue;
        for (var index = 0; index < otherSquares.Length; index++)
        {
            var otherSquare = otherSquares[index];
            var distance = SquareDistances[(int)square][(int)otherSquare];
            if (distance < shortestDistance) shortestDistance = distance;
        }
        return shortestDistance;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)] private static ulong GetKnightDestinations(Square fromSquare, ulong occupancy) => KnightMoveMasks[(int)fromSquare];
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private static ulong GetBishopDestinations(Square fromSquare, ulong occupancy) => PrecalculatedMoves.GetBishopMovesMask(fromSquare, occupancy);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private static ulong GetRookDestinations(Square fromSquare, ulong occupancy) => PrecalculatedMoves.GetRookMovesMask(fromSquare, occupancy);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private static ulong GetQueenDestinations(Square fromSquare, ulong occupancy) => PrecalculatedMoves.GetBishopMovesMask(fromSquare, occupancy) | PrecalculatedMoves.GetRookMovesMask(fromSquare, occupancy);


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void SetPosition(string fen, bool preserveMoveCount = false)
    {
        var fenTokens = Tokens.Parse(fen, ' ', '"');
        if (fenTokens.Count < 4) throw new ArgumentException($"FEN has only {fenTokens.Count}  fields.");
        Reset(preserveMoveCount);
        // Place pieces on board.
        var fenPosition = Tokens.Parse(fenTokens[0], '/', '"');
        var square = Square.A8;
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
                else AddPiece(PieceHelper.ParseChar(piece), square);
                square++;
            }
        }
        // Set side to move, castling rights, en passant square, ply, and full move number.
        CurrentPosition.ColorToMove = fenTokens[1].Equals("w") ? Color.White : Color.Black;
        Castling.Set(ref CurrentPosition.Castling, Color.White, BoardSide.King, fenTokens[2].IndexOf("K") > -1);
        Castling.Set(ref CurrentPosition.Castling, Color.White, BoardSide.Queen, fenTokens[2].IndexOf("Q") > -1);
        Castling.Set(ref CurrentPosition.Castling, Color.Black, BoardSide.King, fenTokens[2].IndexOf("k") > -1);
        Castling.Set(ref CurrentPosition.Castling, Color.Black, BoardSide.Queen, fenTokens[2].IndexOf("q") > -1);
        CurrentPosition.EnPassantSquare = fenTokens[3] == "-" ? Square.Illegal : GetSquare(fenTokens[3]);
        CurrentPosition.PlySinceCaptureOrPawnMove = fenTokens.Count == 6 ? int.Parse(fenTokens[4]) : 0;
        CurrentPosition.FullMoveNumber = fenTokens.Count == 6 ? int.Parse(fenTokens[5]) : 1;
        // Determine if king is in check and set position key.
        PlayNullMove();
        var kingSquare = Bitwise.FirstSetSquare(CurrentPosition.GetKing(CurrentPosition.ColorLastMoved));
        var kingInCheck = IsSquareAttacked(kingSquare);
        UndoMove();
        CurrentPosition.KingInCheck = kingInCheck;
        CurrentPosition.Key = GetCurrentPositionKey();
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public bool ValidateMove(ref ulong move)
    {
        // Don't trust move that wasn't generated by engine (from cache, game notation, input by user, etc).
        // Validate main aspects of the move.  Don't test for every impossibility.
        // Goal is to prevent engine crashes, not ensure a perfectly legal search tree.
        var fromSquare = Move.From(move);
        var toSquare = Move.To(move);
        var attacker = CurrentPosition.GetPiece(fromSquare);
        if (attacker == Piece.None) return false; // No piece on from square.
        var attackerColor = PieceHelper.GetColor(attacker);
        if (CurrentPosition.ColorToMove != attackerColor) return false; // Piece is wrong color.
        var victim = CurrentPosition.GetPiece(toSquare);
        if ((victim != Piece.None) && (attackerColor == PieceHelper.GetColor(victim))) return false; // Piece cannot attack its own color.
        if ((victim == Piece.WhiteKing) || (victim == Piece.BlackKing)) return false;  // Piece cannot attack king.
        var promotedPiece = Move.PromotedPiece(move);
        if ((promotedPiece != Piece.None) && (CurrentPosition.ColorToMove != PieceHelper.GetColor(promotedPiece))) return false; // Promoted piece is wrong color.
        var distance = SquareDistances[(int)fromSquare][(int)toSquare];
        if (distance > 1)
        {
            // For sliding pieces, validate to square is reachable and not blocked.
            ulong betweenSquares;
            // ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
            switch (attacker)
            {
                case Piece.WhiteBishop:
                    betweenSquares = DiagonalBetweenSquares[(int)fromSquare][(int)toSquare];
                    if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                    break;
                case Piece.WhiteRook:
                    betweenSquares = RankFileBetweenSquares[(int)fromSquare][(int)toSquare];
                    if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                    break;
                case Piece.WhiteQueen:
                    betweenSquares = DiagonalBetweenSquares[(int)fromSquare][(int)toSquare];
                    if (betweenSquares == 0) betweenSquares = RankFileBetweenSquares[(int)fromSquare][(int)toSquare];
                    if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                    break;
                case Piece.BlackBishop:
                    betweenSquares = DiagonalBetweenSquares[(int)fromSquare][(int)toSquare];
                    if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                    break;
                case Piece.BlackRook:
                    betweenSquares = RankFileBetweenSquares[(int)fromSquare][(int)toSquare];
                    if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                    break;
                case Piece.BlackQueen:
                    betweenSquares = DiagonalBetweenSquares[(int)fromSquare][(int)toSquare];
                    if (betweenSquares == 0) betweenSquares = RankFileBetweenSquares[(int)fromSquare][(int)toSquare];
                    if ((betweenSquares == 0) || ((CurrentPosition.Occupancy & betweenSquares) > 0)) return false;
                    break;
            }
            // ReSharper restore SwitchStatementMissingSomeEnumCasesNoDefault
        }
        var pawn = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, CurrentPosition.ColorToMove);
        var king = PieceHelper.GetPieceOfColor(ColorlessPiece.King, CurrentPosition.ColorToMove);
        var enPassantVictim = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, CurrentPosition.ColorLastMoved);
        if ((promotedPiece != Piece.None) && (attacker != pawn)) return false; // Only pawns can promote.
        if ((promotedPiece == pawn) || (promotedPiece == king)) return false; // Cannot promote pawn to pawn or king.
        var castling = (attacker == king) && (distance == 2);
        if (castling)
        {
            // ReSharper disable ConvertIfStatementToSwitchStatement
            if (CurrentPosition.ColorToMove == Color.White)
            {
                // White Castling
                if ((toSquare != Square.C1) && (toSquare != Square.G1)) return false; // Castle destination square invalid.
                if (toSquare == Square.C1)
                {
                    // Castle Queenside
                    if (!Castling.Permitted(CurrentPosition.Castling, Color.White, BoardSide.Queen)) return false; // Castle not possible.
                    if ((CurrentPosition.Occupancy & CastleEmptySquaresMask[(int)Color.White][(int)BoardSide.Queen]) > 0) return false; // Castle squares occupied.
                }
                else
                {
                    // Castle Kingside
                    if (!Castling.Permitted(CurrentPosition.Castling, Color.White, BoardSide.King)) return false; // Castle not possible.
                    if ((CurrentPosition.Occupancy & CastleEmptySquaresMask[(int)Color.White][(int)BoardSide.King]) > 0) return false; // Castle squares occupied.
                }
            }
            else
            {
                // Black Castling
                if ((toSquare != Square.C8) && (toSquare != Square.G8)) return false; // Castle destination square invalid.
                if (toSquare == Square.C8)
                {
                    // Castle Queenside
                    if (!Castling.Permitted(CurrentPosition.Castling, Color.Black, BoardSide.Queen)) return false; // Castle not possible.
                    if ((CurrentPosition.Occupancy & CastleEmptySquaresMask[(int)Color.Black][(int)BoardSide.Queen]) > 0) return false; // Castle squares occupied.
                }
                else
                {
                    // Castle Kingside
                    if (!Castling.Permitted(CurrentPosition.Castling, Color.Black, BoardSide.King)) return false; // Castle not possible.
                    if ((CurrentPosition.Occupancy & CastleEmptySquaresMask[(int)Color.Black][(int)BoardSide.King]) > 0) return false; // Castle squares occupied.
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


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public bool IsMoveLegal(ref ulong move)
    {
        var fromSquare = Move.From(move);
        if (!CurrentPosition.KingInCheck && !Move.IsKingMove(move) && !Move.IsEnPassantCapture(move))
        {
            if ((SquareMasks[(int)fromSquare] & CurrentPosition.PinnedPieces) == 0)
            {
                // Move cannot expose king to check.
                PlayMove(move);
                goto ChecksEnemyKing;
            }
        }
        // Determine if moving piece exposes king to check.
        PlayMove(move);
        var kingSquare = Bitwise.FirstSetSquare(CurrentPosition.GetKing(CurrentPosition.ColorLastMoved));
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
        CurrentPosition.ColorToMove = CurrentPosition.ColorLastMoved;
        CurrentPosition.KingInCheck = false;
        CurrentPosition.EnPassantSquare = Square.Illegal;
        // Determine if move checks enemy king.
        kingSquare = Bitwise.FirstSetSquare(CurrentPosition.GetKing(CurrentPosition.ColorLastMoved));
        var check = IsSquareAttacked(kingSquare);
        // Revert side to move.
        CurrentPosition.ColorToMove = CurrentPosition.ColorLastMoved;
        CurrentPosition.KingInCheck = kingInCheck;
        CurrentPosition.EnPassantSquare = enPassantSquare;
        // Set check property and undo move.
        Move.SetIsCheck(ref move, check);
        UndoMove();
        return true;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private bool IsCastlePathAttacked(ulong move)
    {
        var toSquare = Move.To(move);
        ulong attackedSquaresMask;
        if (CurrentPosition.ColorToMove == Color.White)
        {
            // Black castled, now white move.
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            attackedSquaresMask = toSquare switch
            {
                Square.C8 => _castleAttackedSquareMasks[(int)Color.Black][(int)BoardSide.Queen],
                Square.G8 => _castleAttackedSquareMasks[(int)Color.Black][(int)BoardSide.King],
                _ => throw new Exception($"Black king cannot castle to {SquareLocations[(int)toSquare]}.")
            };
        }
        else
        {
            // White castled, now black move.
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            attackedSquaresMask = toSquare switch
            {
                Square.C1 => _castleAttackedSquareMasks[(int)Color.White][(int)BoardSide.Queen],
                Square.G1 => _castleAttackedSquareMasks[(int)Color.White][(int)BoardSide.King],
                _ => throw new Exception($"White king cannot castle to {SquareLocations[(int)toSquare]}.")
            };
        }
        while ((toSquare = Bitwise.FirstSetSquare(attackedSquaresMask)) != Square.Illegal)
        {
            if (IsSquareAttacked(toSquare)) return true;
            Bitwise.ClearBit(ref attackedSquaresMask, toSquare);
        }
        return false;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private bool IsSquareAttacked(Square square)
    {
        var pawnAttackMask = PawnAttackMasks[(int)CurrentPosition.ColorLastMoved][(int)square]; // Attacked by white pawn masks = black pawn attack masks and vice-versa.
        var pawns = CurrentPosition.GetPawns(CurrentPosition.ColorToMove);
        // Determine if square is attacked by pawns.
        if ((pawnAttackMask & pawns) > 0) return true;
        // Determine if square is attacked by knights.
        var knights = CurrentPosition.GetKnights(CurrentPosition.ColorToMove);
        if ((KnightMoveMasks[(int)square] & knights) > 0) return true;
        // Determine if square is attacked by diagonal sliding piece.
        var bishopDestinations = PrecalculatedMoves.GetBishopMovesMask(square, CurrentPosition.Occupancy);
        var bishops = CurrentPosition.GetBishops(CurrentPosition.ColorToMove);
        var queens = CurrentPosition.GetQueens(CurrentPosition.ColorToMove);
        if ((bishopDestinations & (bishops | queens)) > 0) return true;
        // Determine if square is attacked by file / rank sliding pieces.
        var rookDestinations = PrecalculatedMoves.GetRookMovesMask(square, CurrentPosition.Occupancy);
        var rooks = CurrentPosition.GetRooks(CurrentPosition.ColorToMove);
        if ((rookDestinations & (rooks | queens)) > 0) return true;
        // Determine if square is attacked by king.
        var king = CurrentPosition.GetKing(CurrentPosition.ColorToMove);
        return (KingMoveMasks[(int)square] & king) > 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
        Piece captureVictim;
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
            Debug.Assert((captureVictim != Piece.WhiteKing) && (captureVictim != Piece.BlackKing));
            RemovePiece(fromSquare);
            var promotedPiece = Move.PromotedPiece(move);
            AddPiece(promotedPiece == Piece.None ? piece : promotedPiece, toSquare);
        }
        if (Castling.Permitted(CurrentPosition.Castling))
        {
            // Update castling rights.
            // ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
            switch (fromSquare)
            {
                case Square.A8:
                    Castling.Set(ref CurrentPosition.Castling, Color.Black, BoardSide.Queen, false);
                    break;
                case Square.E8:
                    Castling.Set(ref CurrentPosition.Castling, Color.Black, BoardSide.Queen, false);
                    Castling.Set(ref CurrentPosition.Castling, Color.Black, BoardSide.King, false);
                    break;
                case Square.H8:
                    Castling.Set(ref CurrentPosition.Castling, Color.Black, BoardSide.King, false);
                    break;
                case Square.A1:
                    Castling.Set(ref CurrentPosition.Castling, Color.White, BoardSide.Queen, false);
                    break;
                case Square.E1:
                    Castling.Set(ref CurrentPosition.Castling, Color.White, BoardSide.Queen, false);
                    Castling.Set(ref CurrentPosition.Castling, Color.White, BoardSide.King, false);
                    break;
                case Square.H1:
                    Castling.Set(ref CurrentPosition.Castling, Color.White, BoardSide.King, false);
                    break;
            }
            switch (toSquare)
            {
                case Square.A8:
                    Castling.Set(ref CurrentPosition.Castling, Color.Black, BoardSide.Queen, false);
                    break;
                case Square.H8:
                    Castling.Set(ref CurrentPosition.Castling, Color.Black, BoardSide.King, false);
                    break;
                case Square.A1:
                    Castling.Set(ref CurrentPosition.Castling, Color.White, BoardSide.Queen, false);
                    break;
                case Square.H1:
                    Castling.Set(ref CurrentPosition.Castling, Color.White, BoardSide.King, false);
                    break;
            }
            // ReSharper restore SwitchStatementMissingSomeEnumCasesNoDefault
        }
        // ReSharper restore ConvertIfStatementToSwitchStatement
        // Update current position.
        CurrentPosition.EnPassantSquare = Move.IsDoublePawnMove(move) ? _enPassantTargetSquares[(int)toSquare] : Square.Illegal;
        if ((captureVictim != Piece.None) || Move.IsPawnMove(move)) CurrentPosition.PlySinceCaptureOrPawnMove = 0;
        else CurrentPosition.PlySinceCaptureOrPawnMove++;
        CurrentPosition.FullMoveNumber += (int)CurrentPosition.ColorToMove;
        CurrentPosition.ColorToMove = CurrentPosition.ColorLastMoved;
        CurrentPosition.KingInCheck = Move.IsCheck(move);
        CurrentPosition.Key = GetCurrentPositionKey();
        Nodes++;
        Debug.Assert(AssertIntegrity());
    }


    private bool AssertMoveIntegrity(ulong move)
    {
        var fromSquare = Move.From(move);
        var toSquare = Move.To(move);
        var piece = CurrentPosition.GetPiece(fromSquare);
        var pawn = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, CurrentPosition.ColorToMove);
        var king = PieceHelper.GetPieceOfColor(ColorlessPiece.King, CurrentPosition.ColorToMove);
        var toRank = Ranks[(int)CurrentPosition.ColorToMove][(int) toSquare];
        // EnPassantVictim variable only used in Debug builds.
        // ReSharper disable RedundantAssignment
        var enPassantVictim = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, CurrentPosition.ColorLastMoved);
        var captureVictim = CurrentPosition.GetPiece(toSquare);
        var enPassantCapture = (CurrentPosition.EnPassantSquare != Square.Illegal) && (piece == pawn) && (toSquare == CurrentPosition.EnPassantSquare);
        Debug.Assert(Move.IsEnPassantCapture(move) == enPassantCapture, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
        if (enPassantCapture) Debug.Assert(Move.CaptureVictim(move) == enPassantVictim, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
        else Debug.Assert(Move.CaptureVictim(move) == captureVictim, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
        var pawnPromotion = (piece == pawn) && (toRank == 7);
        if (pawnPromotion) Debug.Assert(Move.PromotedPiece(move) != Piece.None, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
        else Debug.Assert(Move.PromotedPiece(move) == Piece.None, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
        var castling = (piece == king) && (SquareDistances[(int)fromSquare][(int)toSquare] == 2);
        Debug.Assert(Move.IsCastling(move) == castling, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
        var kingMove = piece == king;
        Debug.Assert(Move.IsKingMove(move) == kingMove, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
        var doublePawnMove = (piece == pawn) && (SquareDistances[(int)fromSquare][(int)toSquare] == 2);
        Debug.Assert(Move.IsDoublePawnMove(move) == doublePawnMove, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
        var pawnMove = piece == pawn;
        Debug.Assert(Move.IsPawnMove(move) == pawnMove, $"{CurrentPosition.ToFen()}{Environment.NewLine}Move = {Move.ToString(move)}{Environment.NewLine}{CurrentPosition}");
        // ReSharper restore RedundantAssignment
        return true;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void Castle(Piece piece, Square toSquare)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (piece == Piece.WhiteKing)
            // ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (toSquare)
            {
                case Square.C1:
                    // White Castle Queenside
                    RemovePiece(Square.E1);
                    AddPiece(Piece.WhiteKing, Square.C1);
                    RemovePiece(Square.A1);
                    AddPiece(Piece.WhiteRook, Square.D1);
                    break;
                case Square.G1:
                    // White Castle Kingside
                    RemovePiece(Square.E1);
                    AddPiece(Piece.WhiteKing, Square.G1);
                    RemovePiece(Square.H1);
                    AddPiece(Piece.WhiteRook, Square.F1);
                    break;
                default:
                    throw new Exception($"White king cannot castle to {SquareLocations[(int)toSquare]}.");
            }
        else if (piece == Piece.BlackKing)
            switch (toSquare)
            {
                case Square.C8:
                    // Black Castle Queenside
                    RemovePiece(Square.E8);
                    AddPiece(Piece.BlackKing, Square.C8);
                    RemovePiece(Square.A8);
                    AddPiece(Piece.BlackRook, Square.D8);
                    break;
                case Square.G8:
                    // Black Castle Kingside
                    RemovePiece(Square.E8);
                    AddPiece(Piece.BlackKing, Square.G8);
                    RemovePiece(Square.H8);
                    AddPiece(Piece.BlackRook, Square.F8);
                    break;
                default:
                    throw new Exception($"Black king cannot castle to {SquareLocations[(int)toSquare]}.");
            }
        // ReSharper restore SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        else
            throw new Exception($"{piece} piece cannot castle.");
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnPassantCapture(Piece piece, Square fromSquare)
    {
        // Move pawn and remove captured pawn.
        RemovePiece(_enPassantVictimSquares[(int)CurrentPosition.EnPassantSquare]);
        RemovePiece(fromSquare);
        AddPiece(piece, CurrentPosition.EnPassantSquare);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PlayNullMove()
    {
        CurrentPosition.PlayedMove = Move.Null;
        // Advance position index.
        NextPosition.Set(CurrentPosition);
        _positionIndex++;
        // King cannot be in check, nor is en passant capture possible after null move.
        CurrentPosition.KingInCheck = false;
        CurrentPosition.EnPassantSquare = Square.Illegal;
        CurrentPosition.ColorToMove = CurrentPosition.ColorLastMoved;
        CurrentPosition.Key = GetCurrentPositionKey();
        Nodes++;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UndoMove()
    {
        Debug.Assert(_positionIndex > 0);
        _positionIndex--;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
        for (var color = Color.White; color <= Color.Black; color++)
        {
            // ReSharper disable once RedundantAssignment
            var occupancy = CurrentPosition.GetPawns(color) | CurrentPosition.GetKnights(color) | CurrentPosition.GetBishops(color) |
                            CurrentPosition.GetRooks(color) | CurrentPosition.GetQueens(color) | CurrentPosition.GetKing(color);
            Debug.Assert(occupancy == CurrentPosition.ColorOccupancy[(int)color]);
            Debug.Assert(Bitwise.CountSetBits(CurrentPosition.GetKing(color)) == 1);
        }
        Debug.Assert((CurrentPosition.ColorOccupancy[(int)Color.White] | CurrentPosition.ColorOccupancy[(int)Color.Black]) == CurrentPosition.Occupancy);
        return true;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddPiece(Piece piece, Square square)
    {
        Debug.Assert(piece != Piece.None);
        var squareMask = SquareMasks[(int)square];
        var pieceColor = PieceHelper.GetColor(piece);
        // Update bitboards and Zobrist key.
        CurrentPosition.PieceBitboards[(int)piece] |= squareMask;
        CurrentPosition.ColorOccupancy[(int)pieceColor] |= squareMask;
        CurrentPosition.Occupancy |= squareMask;
        UpdatePiecesSquaresKey(piece, square);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Piece RemovePiece(Square square)
    {
        var squareUnmask = _squareUnmasks[(int)square];
        var piece = CurrentPosition.GetPiece(square);
        if (piece == Piece.None) return piece;
        var pieceColor = PieceHelper.GetColor(piece);
        // Update bitboards and Zobrist key.
        CurrentPosition.PieceBitboards[(int)piece] &= squareUnmask;
        CurrentPosition.ColorOccupancy[(int)pieceColor] &= squareUnmask;
        CurrentPosition.Occupancy &= squareUnmask;
        UpdatePiecesSquaresKey(piece, square);
        return piece;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdatePiecesSquaresKey(Piece piece, Square square)
    {
        CurrentPosition.PiecesSquaresKey ^= _pieceSquareKeys[(int)piece][(int)square];
        Debug.Assert(AssertPiecesSquaresKeyIntegrity());
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong GetCurrentPositionKey()
    {
        var sideToMoveKey = _sideToMoveKeys[(int)CurrentPosition.ColorToMove];
        var castlingKey = _castlingKeys[CurrentPosition.Castling];
        var enPassantKey = _enPassantKeys[(int)CurrentPosition.EnPassantSquare];
        return CurrentPosition.PiecesSquaresKey ^ sideToMoveKey ^ castlingKey ^ enPassantKey;
    }


    private bool AssertPiecesSquaresKeyIntegrity()
    {
        // Verify incrementally updated pieces squares key matches fully updated pieces squares key.
        var fullyUpdatedPiecesSquaresKey = _piecesSquaresInitialKey;
        for (var square = Square.A8; square < Square.Illegal; square++)
        {
            var piece = CurrentPosition.GetPiece(square);
            if (piece != Piece.None) fullyUpdatedPiecesSquaresKey ^= _pieceSquareKeys[(int)piece][(int)square];
        }
        var matches = fullyUpdatedPiecesSquaresKey == CurrentPosition.PiecesSquaresKey;
        if (!matches) _writeMessageLine(ToString(true));
        return matches;
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
            NodesInfoUpdate = _nodesInfoInterval;
        }
    }


    public override string ToString() => CurrentPosition.ToString();


    private string ToString(bool allPositions)
    {
        if (!allPositions) return ToString();
        var stringBuilder = new StringBuilder();
        for (var positionIndex = 0; positionIndex <= _positionIndex; positionIndex++) stringBuilder.AppendLine(_positions[positionIndex].ToString());
        return stringBuilder.ToString();
    }
}