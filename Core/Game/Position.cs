﻿// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Core.Game
{
    public sealed class Position
    {
        public const int MaxMoves = 128;
        public readonly ulong[] PieceBitboards;
        public readonly ulong[] ColorOccupancy;
        public readonly bool[][] Castling;
        public readonly ulong[] Moves;
        public ulong Occupancy;
        public ulong PinnedPieces;
        public Color ColorToMove;
        public Square EnPassantSquare;
        public int PlySinceCaptureOrPawnMove;
        public int FullMoveNumber;
        public bool KingInCheck;
        public int CurrentMoveIndex;
        public int MoveIndex;
        public MoveGenerationStage MoveGenerationStage;
        public ulong PiecesSquaresKey;
        public ulong CastlingKey;
        public ulong Key;
        public int StaticScore;
        public ulong PlayedMove;
        private readonly Board _board;


        public ulong WhitePawns => PieceBitboards[(int) Piece.WhitePawn];
        public ulong WhiteKnights => PieceBitboards[(int) Piece.WhiteKnight];
        public ulong WhiteBishops => PieceBitboards[(int) Piece.WhiteBishop];
        public ulong WhiteRooks => PieceBitboards[(int) Piece.WhiteRook];
        public ulong WhiteQueens => PieceBitboards[(int) Piece.WhiteQueen];
        public ulong WhiteKing => PieceBitboards[(int) Piece.WhiteKing];
        public ulong WhiteOccupancy => ColorOccupancy[(int) Color.White];

        
        public ulong BlackPawns => PieceBitboards[(int) Piece.BlackPawn];
        public ulong BlackKnights => PieceBitboards[(int) Piece.BlackKnight];
        public ulong BlackBishops => PieceBitboards[(int) Piece.BlackBishop];
        public ulong BlackRooks => PieceBitboards[(int) Piece.BlackRook];
        public ulong BlackQueens => PieceBitboards[(int) Piece.BlackQueen];
        public ulong BlackKing => PieceBitboards[(int) Piece.BlackKing];
        public ulong BlackOccupancy => ColorOccupancy[(int) Color.Black];


        public Color ColorLastMoved => 1 - ColorToMove;


        public Position(Board board)
        {
            _board = board;
            PieceBitboards = new ulong[(int) Piece.BlackKing + 1];
            ColorOccupancy = new ulong[2];
            Castling = new[]
            {
                new[] { false, false },
                new[] { false, false }
            };
            Moves = new ulong[MaxMoves];
            Reset();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetPawns(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhitePawn];
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetKnights(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhiteKnight];
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetBishops(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhiteBishop];
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetRooks(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhiteRook];
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetQueens(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhiteQueen];
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetKing(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhiteKing];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetMajorAndMinorPieces(Color color)
        {
            var pieces = 0ul;
            var firstPiece = (Piece)((int)color * (int)Piece.WhiteKing) + (int)Piece.WhiteKnight;
            var lastPiece = firstPiece + 3;
            for (var piece = firstPiece; piece <= lastPiece; piece++) pieces |= PieceBitboards[(int)piece];
            return pieces;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetPiece(Square square)
        {
            var squareMask = Board.SquareMasks[(int) square];
            if ((Occupancy & squareMask) == 0) return Piece.None;
            if ((WhiteOccupancy & squareMask) > 0)
            {
                // Locate white piece.
                for (var piece = Piece.WhitePawn; piece <= Piece.WhiteKing; piece++)
                {
                    if ((PieceBitboards[(int) piece] & squareMask) > 0) return piece;
                }
                throw new Exception($"White piece not found at {Board.SquareLocations[(int) square]}.");
            }
            // Locate black piece.
            for (var piece = Piece.BlackPawn; piece <= Piece.BlackKing; piece++)
            {
                if ((PieceBitboards[(int) piece] & squareMask) > 0) return piece;
            }
            throw new Exception($"Black piece not found at {Board.SquareLocations[(int) square]}.");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(Position copyFromPosition)
        {
            // Copy bitboards.
            for (var piece = Piece.WhitePawn; piece <= Piece.BlackKing; piece++) PieceBitboards[(int) piece] = copyFromPosition.PieceBitboards[(int) piece];
            ColorOccupancy[(int) Color.White] = copyFromPosition.ColorOccupancy[(int) Color.White];
            ColorOccupancy[(int) Color.Black] = copyFromPosition.ColorOccupancy[(int) Color.Black];
            Occupancy = copyFromPosition.Occupancy;
            // Copy board state.  Do not copy values that will be set after moves are generated or played.
            ColorToMove = copyFromPosition.ColorToMove;
            Castling[(int)Color.White][(int)BoardSide.Queen] = copyFromPosition.Castling[(int)Color.White][(int)BoardSide.Queen];
            Castling[(int)Color.White][(int)BoardSide.King] = copyFromPosition.Castling[(int)Color.White][(int)BoardSide.King];
            Castling[(int)Color.Black][(int)BoardSide.Queen] = copyFromPosition.Castling[(int)Color.Black][(int)BoardSide.Queen];
            Castling[(int)Color.Black][(int)BoardSide.King] = copyFromPosition.Castling[(int)Color.Black][(int)BoardSide.King];
            EnPassantSquare = copyFromPosition.EnPassantSquare;
            PlySinceCaptureOrPawnMove = copyFromPosition.PlySinceCaptureOrPawnMove;
            FullMoveNumber = copyFromPosition.FullMoveNumber;
            PiecesSquaresKey = copyFromPosition.PiecesSquaresKey;
            CastlingKey = copyFromPosition.CastlingKey;
        }


        public void GenerateMoves()
        {
            PrepareMoveGeneration();
            FindPinnedPieces();
            GenerateMoves(MoveGeneration.AllMoves, Board.AllSquaresMask, Board.AllSquaresMask);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GenerateMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
        {
            GeneratePawnMoves(moveGeneration, fromSquareMask, toSquareMask);
            GeneratePieceMoves(moveGeneration, fromSquareMask, toSquareMask);
            GenerateKingMoves(moveGeneration, fromSquareMask, toSquareMask);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrepareMoveGeneration()
        {
            CurrentMoveIndex = 0;
            MoveIndex = 0;
            MoveGenerationStage = MoveGenerationStage.BestMove;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GeneratePawnMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
        {
            var pawns = GetPawns(ColorToMove) & fromSquareMask;
            if (pawns == 0) return;
            var pawnMoveMasks = Board.PawnMoveMasks[(int)ColorToMove];
            var pawnDoubleMoveMasks = Board.PawnDoubleMoveMasks[(int)ColorToMove];
            var pawnAttackMasks = Board.PawnAttackMasks[(int)ColorToMove];
            var enemyOccupiedSquares = ColorOccupancy[(int)ColorLastMoved];
            var unoccupiedSquares = ~Occupancy;
            var ranks = Board.Ranks[(int)ColorToMove];
            var attacker = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, ColorToMove);
            var queen = PieceHelper.GetPieceOfColor(ColorlessPiece.Queen, ColorToMove);
            var knight = PieceHelper.GetPieceOfColor(ColorlessPiece.Knight, ColorToMove);
            var enPassantVictim = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, ColorLastMoved);
            Square fromSquare;
            ulong move;
            if ((EnPassantSquare != Square.Illegal) && ((Board.SquareMasks[(int)EnPassantSquare] & toSquareMask) > 0) && (moveGeneration != MoveGeneration.OnlyNonCaptures))
            {
                var enPassantAttackers = Board.EnPassantAttackerMasks[(int)EnPassantSquare] & pawns;
                while ((fromSquare = Bitwise.FirstSetSquare(enPassantAttackers)) != Square.Illegal)
                {
                    // Capture pawn en passant.
                    move = Move.Null;
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, EnPassantSquare);
                    Move.SetCaptureAttacker(ref move, attacker);
                    Move.SetCaptureVictim(ref move, enPassantVictim);
                    Move.SetIsEnPassantCapture(ref move, true);
                    Move.SetIsPawnMove(ref move, true);
                    Moves[MoveIndex] = move;
                    MoveIndex++;
                    Bitwise.ClearBit(ref enPassantAttackers, fromSquare);
                }
            }
            while ((fromSquare = Bitwise.FirstSetSquare(pawns)) != Square.Illegal)
            {
                ulong pawnDestinations;
                Square toSquare;
                int toSquareRank;
                if (moveGeneration != MoveGeneration.OnlyCaptures)
                {
                    // Pawns may move forward one square (or two if on initial square) if forward squares are unoccupied.
                    pawnDestinations = pawnMoveMasks[(int)fromSquare] & unoccupiedSquares & toSquareMask;
                    while ((toSquare = Bitwise.FirstSetSquare(pawnDestinations)) != Square.Illegal)
                    {
                        var doubleMove = Board.SquareDistances[(int)fromSquare][(int)toSquare] == 2;
                        if (doubleMove && ((Occupancy & pawnDoubleMoveMasks[(int)fromSquare]) > 0))
                        {
                            // Double move is blocked.
                            Bitwise.ClearBit(ref pawnDestinations, toSquare);
                            continue;
                        }
                        toSquareRank = ranks[(int)toSquare];
                        if (toSquareRank < 7)
                        {
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetIsDoublePawnMove(ref move, doubleMove);
                            Move.SetIsPawnMove(ref move, true);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                        }
                        else
                        {
                            for (var promotedPiece = queen; promotedPiece >= knight; promotedPiece--)
                            {
                                // Promote pawn.
                                move = Move.Null;
                                Move.SetFrom(ref move, fromSquare);
                                Move.SetTo(ref move, toSquare);
                                Move.SetPromotedPiece(ref move, promotedPiece);
                                Move.SetIsPawnMove(ref move, true);
                                Moves[MoveIndex] = move;
                                MoveIndex++;
                            }
                        }
                        Bitwise.ClearBit(ref pawnDestinations, toSquare);
                    }
                }
                if (moveGeneration != MoveGeneration.OnlyNonCaptures)
                {
                    // Pawns may attack diagonally forward one square if occupied by enemy.
                    pawnDestinations = pawnAttackMasks[(int)fromSquare] & enemyOccupiedSquares & toSquareMask;
                    while ((toSquare = Bitwise.FirstSetSquare(pawnDestinations)) != Square.Illegal)
                    {
                        toSquareRank = ranks[(int)toSquare];
                        var victim = GetPiece(toSquare);
                        if (toSquareRank < 7)
                        {
                            move = Move.Null;
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetCaptureAttacker(ref move, attacker);
                            Move.SetCaptureVictim(ref move, victim);
                            Move.SetIsPawnMove(ref move, true);
                            Moves[MoveIndex] = move;
                            MoveIndex++;
                        }
                        else
                        {
                            for (var promotedPiece = queen; promotedPiece >= knight; promotedPiece--)
                            {
                                // Promote pawn.
                                move = Move.Null;
                                Move.SetFrom(ref move, fromSquare);
                                Move.SetTo(ref move, toSquare);
                                Move.SetCaptureAttacker(ref move, attacker);
                                Move.SetPromotedPiece(ref move, promotedPiece);
                                Move.SetCaptureVictim(ref move, victim);
                                Move.SetIsPawnMove(ref move, true);
                                Moves[MoveIndex] = move;
                                MoveIndex++;
                            }
                        }
                        Bitwise.ClearBit(ref pawnDestinations, toSquare);
                    }
                }
                Bitwise.ClearBit(ref pawns, fromSquare);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GeneratePieceMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
        {
            for (var colorlessPiece = ColorlessPiece.Knight; colorlessPiece <= ColorlessPiece.Queen; colorlessPiece++)
            {
                var attacker = PieceHelper.GetPieceOfColor(colorlessPiece, ColorToMove);
                var pieces = PieceBitboards[(int)attacker] & fromSquareMask;
                if (pieces == 0) continue;
                var getPieceMovesMask = Board.PieceMoveMaskDelegates[(int)colorlessPiece];
                var unOrEnemyOccupiedSquares = ~ColorOccupancy[(int)ColorToMove];
                var enemyOccupiedSquares = ColorOccupancy[(int)ColorLastMoved];
                Square fromSquare;
                while ((fromSquare = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
                {
                    var occupancy = Board.PieceMoveMasks[(int)colorlessPiece][(int)fromSquare] & Occupancy;
                    var pieceDestinations = moveGeneration switch
                    {
                        MoveGeneration.AllMoves => getPieceMovesMask(fromSquare, occupancy) & unOrEnemyOccupiedSquares & toSquareMask,
                        MoveGeneration.OnlyCaptures => getPieceMovesMask(fromSquare, occupancy) & enemyOccupiedSquares & toSquareMask,
                        MoveGeneration.OnlyNonCaptures => getPieceMovesMask(fromSquare, occupancy) & ~Occupancy & toSquareMask,
                        _ => throw new Exception($"{moveGeneration} move generation not supported.")
                    };
                    Square toSquare;
                    while ((toSquare = Bitwise.FirstSetSquare(pieceDestinations)) != Square.Illegal)
                    {
                        var victim = GetPiece(toSquare);
                        var move = Move.Null;
                        Move.SetFrom(ref move, fromSquare);
                        Move.SetTo(ref move, toSquare);
                        if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                        Move.SetCaptureVictim(ref move, victim);
                        Moves[MoveIndex] = move;
                        MoveIndex++;
                        Bitwise.ClearBit(ref pieceDestinations, toSquare);
                    }
                    Bitwise.ClearBit(ref pieces, fromSquare);
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GenerateKingMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
        {
            var king = GetKing(ColorToMove) & fromSquareMask;
            if (king == 0) return;
            var unOrEnemyOccupiedSquares = ~ColorOccupancy[(int)ColorToMove];
            var enemyOccupiedSquares = ColorOccupancy[(int)ColorLastMoved];
            var attacker = PieceHelper.GetPieceOfColor(ColorlessPiece.King, ColorToMove);
            ulong move;
            var fromSquare = Bitwise.FirstSetSquare(king);
            if (fromSquare == Square.Illegal) return;
            var kingDestinations = moveGeneration switch
            {
                MoveGeneration.AllMoves => Board.KingMoveMasks[(int)fromSquare] & unOrEnemyOccupiedSquares & toSquareMask,
                MoveGeneration.OnlyCaptures => Board.KingMoveMasks[(int)fromSquare] & enemyOccupiedSquares & toSquareMask,
                MoveGeneration.OnlyNonCaptures => Board.KingMoveMasks[(int)fromSquare] & ~Occupancy & toSquareMask,
                _ => throw new Exception($"{moveGeneration} move generation not supported.")
            };
            Square toSquare;
            while ((toSquare = Bitwise.FirstSetSquare(kingDestinations)) != Square.Illegal)
            {
                var victim = GetPiece(toSquare);
                move = Move.Null;
                Move.SetFrom(ref move, fromSquare);
                Move.SetTo(ref move, toSquare);
                if (victim != Piece.None) Move.SetCaptureAttacker(ref move, attacker);
                Move.SetCaptureVictim(ref move, victim);
                Move.SetIsKingMove(ref move, true);
                Moves[MoveIndex] = move;
                MoveIndex++;
                Bitwise.ClearBit(ref kingDestinations, toSquare);
            }
            if (moveGeneration != MoveGeneration.OnlyCaptures)
            {
                if (Castling[(int)ColorToMove][(int)BoardSide.Queen] && ((Occupancy & Board.CastleEmptySquaresMask[(int)ColorToMove][(int)BoardSide.Queen]) == 0))
                {
                    // Castle Queenside
                    toSquare = Board.CastleToSquares[(int)ColorToMove][(int)BoardSide.Queen];
                    if ((Board.SquareMasks[(int)toSquare] & toSquareMask) > 0)
                    {
                        move = Move.Null;
                        Move.SetFrom(ref move, fromSquare);
                        Move.SetTo(ref move, toSquare);
                        Move.SetIsCastling(ref move, true);
                        Move.SetIsKingMove(ref move, true);
                        Moves[MoveIndex] = move;
                        MoveIndex++;
                    }
                }
                if (Castling[(int)ColorToMove][(int)BoardSide.King] && ((Occupancy & Board.CastleEmptySquaresMask[(int)ColorToMove][(int)BoardSide.King]) == 0))
                {
                    // Castle Kingside
                    toSquare = Board.CastleToSquares[(int)ColorToMove][(int)BoardSide.King];
                    if ((Board.SquareMasks[(int)toSquare] & toSquareMask) > 0)
                    {
                        move = Move.Null;
                        Move.SetFrom(ref move, fromSquare);
                        Move.SetTo(ref move, toSquare);
                        Move.SetIsCastling(ref move, true);
                        Move.SetIsKingMove(ref move, true);
                        Moves[MoveIndex] = move;
                        MoveIndex++;
                    }
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void FindPinnedPieces()
        {
            var ownKingSquare = Bitwise.FirstSetSquare(GetKing(ColorToMove));
            var ownPieces = ColorOccupancy[(int)ColorToMove];
            var enemyRankFileAttackers = GetRooks(ColorLastMoved) | GetQueens(ColorLastMoved);
            var enemyDiagonalAttackers = GetBishops(ColorLastMoved) | GetQueens(ColorLastMoved);
            var enemyPieces = ColorOccupancy[(int)ColorLastMoved];
            // Find pieces pinned to own king by enemy rank / file attackers.
            PinnedPieces = 0;
            Square attackerSquare;
            while ((attackerSquare = Bitwise.FirstSetSquare(enemyRankFileAttackers)) != Square.Illegal)
            {
                var betweenSquares = Board.RankFileBetweenSquares[(int)attackerSquare][(int)ownKingSquare];
                if (betweenSquares == 0)
                {
                    Bitwise.ClearBit(ref enemyRankFileAttackers, attackerSquare);
                    continue;
                }
                if ((betweenSquares & enemyPieces) == 0)
                {
                    // No enemy pieces between enemy attacker and own king.
                    var pinnedPieces = betweenSquares & ownPieces;
                    if (Bitwise.CountSetBits(pinnedPieces) == 1)
                    {
                        // Exactly one own piece between enemy attacker and own king.
                        // Piece is pinned to own king.
                        PinnedPieces |= pinnedPieces;
                    }
                }
                Bitwise.ClearBit(ref enemyRankFileAttackers, attackerSquare);
            }
            // Find pieces pinned to own king by enemy diagonal attackers.
            while ((attackerSquare = Bitwise.FirstSetSquare(enemyDiagonalAttackers)) != Square.Illegal)
            {
                var betweenSquares = Board.DiagonalBetweenSquares[(int)attackerSquare][(int)ownKingSquare];
                if (betweenSquares == 0)
                {
                    Bitwise.ClearBit(ref enemyDiagonalAttackers, attackerSquare);
                    continue;
                }
                if ((betweenSquares & enemyPieces) == 0)
                {
                    // No enemy pieces between enemy attacker and own king.
                    var pinnedPieces = betweenSquares & ownPieces;
                    if (Bitwise.CountSetBits(pinnedPieces) == 1)
                    {
                        // Exactly one own piece between enemy attacker and own king.
                        // Piece is pinned to own king.
                        PinnedPieces |= pinnedPieces;
                    }
                }
                Bitwise.ClearBit(ref enemyDiagonalAttackers, attackerSquare);
            }
        }


        public void Reset()
        {
            for (var piece = Piece.WhitePawn; piece <= Piece.BlackKing; piece++) PieceBitboards[(int) piece] = 0;
            ColorOccupancy[(int) Color.White] = 0;
            ColorOccupancy[(int) Color.Black] = 0;
            Occupancy = 0;
            PinnedPieces = 0;
            ColorToMove = Color.White;
            Castling[(int)Color.White][(int)BoardSide.Queen] = false;
            Castling[(int)Color.White][(int)BoardSide.King] = false;
            Castling[(int)Color.Black][(int)BoardSide.Queen] = false;
            Castling[(int)Color.Black][(int)BoardSide.King] = false;
            EnPassantSquare = Square.Illegal;
            PlySinceCaptureOrPawnMove = 0;
            FullMoveNumber = 0;
            KingInCheck = false;
            CurrentMoveIndex = 0;
            MoveIndex = 0;
            MoveGenerationStage = MoveGenerationStage.BestMove;
            PiecesSquaresKey = 0;
            CastlingKey = 0;
            Key = 0;
            StaticScore = 0;
            PlayedMove = Move.Null;
        }


        public string ToFen()
        {
            var stringBuilder = new StringBuilder();
            // Position
            var x = 0;
            var unoccupiedSquares = 0;
            for (var square = Square.A8; square < Square.Illegal; square++)
            {
                x++;
                if (x == 9)
                {
                    stringBuilder.Append("/");
                    x = 1;
                }
                var piece = GetPiece(square);
                if (piece == Piece.None)
                {
                    // Unoccupied Square
                    unoccupiedSquares++;
                    if (x == 8)
                    {
                        // Last File
                        // Display count of unoccupied squares.
                        stringBuilder.Append(unoccupiedSquares);
                        unoccupiedSquares = 0;
                    }
                }
                else
                {
                    // Occupied Square
                    if (unoccupiedSquares > 0)
                    {
                        // Display count of unoccupied squares.
                        stringBuilder.Append(unoccupiedSquares);
                        unoccupiedSquares = 0;
                    }
                    stringBuilder.Append(PieceHelper.GetChar(piece));
                }
            }
            // Display side to move, castling rights, en passant square, ply, and full move number.
            stringBuilder.Append(" ");
            stringBuilder.Append(ColorToMove == Color.White ? "w" : "b");
            stringBuilder.Append(" ");
            stringBuilder.Append(CastlingHelper.ToString(Castling));
            stringBuilder.Append(" ");
            stringBuilder.Append(EnPassantSquare == Square.Illegal ? "-" : Board.SquareLocations[(int)EnPassantSquare]);
            stringBuilder.Append(" ");
            stringBuilder.Append(PlySinceCaptureOrPawnMove);
            stringBuilder.Append(" ");
            stringBuilder.Append(FullMoveNumber);
            return stringBuilder.ToString();
        }


        public static string ToString(ulong occupancy)
        {
            var stringBuilder = new StringBuilder();
            for (var rank = 7; rank >= 0; rank--)
            {
                for (var file = 0; file < 8; file++)
                {
                    var square = Board.GetSquare(file, rank);
                    stringBuilder.Append(Bitwise.IsBitSet(occupancy, square) ? " 1 " : " . ");
                }
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }


        public override string ToString()
        {
            // Iterate over the piece array to construct an 8 x 8 text display of the chessboard.
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("  +---+---+---+---+---+---+---+---+");
            var square = Square.A8;
            for (var y = 8; y > 0; y--)
            {
                // Add rank.
                stringBuilder.Append(y);
                stringBuilder.Append(" ");
                for (var x = 1; x <= 8; x++)
                {
                    stringBuilder.Append("| ");
                    var piece = GetPiece(square);
                    if (piece == Piece.None) stringBuilder.Append(" ");
                    else stringBuilder.Append(PieceHelper.GetChar(piece));
                    stringBuilder.Append(" ");
                    square++;
                }
                stringBuilder.AppendLine("|");
                stringBuilder.AppendLine("  +---+---+---+---+---+---+---+---+");
            }
            stringBuilder.Append("  ");
            for (var x = 1; x <= 8; x++)
            {
                // Add file.
                var chrFile = (char)(x + 96);
                stringBuilder.Append($"  {chrFile} ");
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            // Display FEN, key, position count, and check.
            stringBuilder.AppendLine($"FEN:             {ToFen()}");
            stringBuilder.AppendLine($"Key:             {Key:X16}");
            stringBuilder.AppendLine($"Position Count:  {_board.GetPositionCount()}");
            stringBuilder.AppendLine($"King in Check:   {(KingInCheck ? "Yes" : "No")}");
            stringBuilder.AppendLine($"Static Score:    {StaticScore}");
            stringBuilder.AppendLine($"Played Move:     {Move.ToString(PlayedMove)}");
            return stringBuilder.ToString();
        }
    }
}
