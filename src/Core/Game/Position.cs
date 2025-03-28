﻿// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Core.Game;


public sealed class Position
{
    public const int MaxMoves = 128;
    public readonly ulong[] PieceBitboards;
    public readonly ulong[] ColorOccupancy;
    public readonly ulong[] Moves;
    public ulong Occupancy;
    public ulong PinnedPieces;
    public Color ColorToMove;
    public uint Castling;
    public Square EnPassantSquare;
    public int PlySinceCaptureOrPawnMove;
    public int FullMoveNumber;
    public bool KingInCheck;
    public int CurrentMoveIndex;
    public int MoveIndex;
    public MoveGenerationStage MoveGenerationStage;
    public ulong PiecesSquaresKey;
    public ulong Key;
    public int StaticScore;
    public ulong PlayedMove;


    public Color ColorPreviouslyMoved => 1 - ColorToMove;


    public Position()
    {
        PieceBitboards = new ulong[(int) Piece.BlackKing + 1];
        ColorOccupancy = new ulong[2];
        Moves = new ulong[MaxMoves];

        Reset();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetPawns(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhitePawn];
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetKnights(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhiteKnight];
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetBishops(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhiteBishop];
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetRooks(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhiteRook];
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetQueens(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhiteQueen];
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetKing(Color color) => PieceBitboards[((int) color * (int) Piece.WhiteKing) + (int) Piece.WhiteKing];
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public ulong GetPieces(ColorlessPiece piece) => PieceBitboards[(int)piece] | PieceBitboards[(int)piece + (int)Piece.WhiteKing];


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetMajorPieces(Color color)
    {
        var firstPiece = (Piece)((int)color * (int)Piece.WhiteKing) + (int)Piece.WhiteRook;
        return PieceBitboards[(int)firstPiece] | PieceBitboards[(int)firstPiece + 1];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetMinorPieces(Color color)
    {
        var firstPiece = (Piece)((int)color * (int)Piece.WhiteKing) + (int)Piece.WhiteKnight;
        return PieceBitboards[(int)firstPiece] | PieceBitboards[(int)firstPiece + 1];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetMajorAndMinorPieces(Color color)
    {
        // Explicit array lookups are faster than looping through pieces.
        return color == Color.White
            ? PieceBitboards[(int)Piece.WhiteKnight] | PieceBitboards[(int)Piece.WhiteBishop] | PieceBitboards[(int)Piece.WhiteRook] | PieceBitboards[(int)Piece.WhiteQueen]
            : PieceBitboards[(int)Piece.BlackKnight] | PieceBitboards[(int)Piece.BlackBishop] | PieceBitboards[(int)Piece.BlackRook] | PieceBitboards[(int)Piece.BlackQueen];
    }


    public Piece GetPiece(Square square)
    {
        var squareMask = Board.SquareMasks[(int) square];
        if ((Occupancy & squareMask) == 0) return Piece.None;

        if ((ColorOccupancy[(int)Color.White] & squareMask) > 0)
        {
            // Locate white piece.  // Explicit array lookups are faster than looping through pieces.
            if ((PieceBitboards[(int)Piece.WhitePawn] & squareMask) > 0) return Piece.WhitePawn;
            if ((PieceBitboards[(int)Piece.WhiteKnight] & squareMask) > 0) return Piece.WhiteKnight;
            if ((PieceBitboards[(int)Piece.WhiteBishop] & squareMask) > 0) return Piece.WhiteBishop;
            if ((PieceBitboards[(int)Piece.WhiteRook] & squareMask) > 0) return Piece.WhiteRook;
            if ((PieceBitboards[(int)Piece.WhiteQueen] & squareMask) > 0) return Piece.WhiteQueen;
            if ((PieceBitboards[(int)Piece.WhiteKing] & squareMask) > 0) return Piece.WhiteKing;

            throw new Exception($"White piece not found at {Board.SquareLocations[(int) square]}.");
        }

        // Locate black piece.  // Explicit array lookups are faster than looping through pieces.
        if ((PieceBitboards[(int)Piece.BlackPawn] & squareMask) > 0) return Piece.BlackPawn;
        if ((PieceBitboards[(int)Piece.BlackKnight] & squareMask) > 0) return Piece.BlackKnight;
        if ((PieceBitboards[(int)Piece.BlackBishop] & squareMask) > 0) return Piece.BlackBishop;
        if ((PieceBitboards[(int)Piece.BlackRook] & squareMask) > 0) return Piece.BlackRook;
        if ((PieceBitboards[(int)Piece.BlackQueen] & squareMask) > 0) return Piece.BlackQueen;
        if ((PieceBitboards[(int)Piece.BlackKing] & squareMask) > 0) return Piece.BlackKing;

        throw new Exception($"Black piece not found at {Board.SquareLocations[(int) square]}.");
    }


    public bool IsSquareAttacked(Square square, Color color)
    {
        var enemyColor = 1 - color;

        // Determine if square is attacked by pawns.
        // Attacked by white pawn masks = black pawn attack masks and vice-versa.
        var pawns = GetPawns(color);
        if ((pawns & Board.PawnAttackMasks[(int)enemyColor][(int)square]) > 0) return true;

        // Determine if square is attacked by knights.
        var knights = GetKnights(color);
        if ((knights & Board.KnightMoveMasks[(int)square]) > 0) return true;

        // Determine if square is attacked by diagonal sliding piece.
        var bishops = GetBishops(color);
        var queens = GetQueens(color);
        if (((bishops | queens) & Board.PrecalculatedMoves.GetBishopMovesMask(square, Occupancy)) > 0) return true;

        // Determine if square is attacked by rank / file sliding pieces.
        var rooks = GetRooks(color);
        if (((rooks | queens) & Board.PrecalculatedMoves.GetRookMovesMask(square, Occupancy)) > 0) return true;

        // Determine if square is attacked by king.
        var king = GetKing(color);
        return (king & Board.KingMoveMasks[(int)square]) > 0;
    }


    public ulong GetSquareAttackers(Square square, ulong modifiedOccupancy)
    {
        // This position's occupancy has been slightly modified (a move has been simulated but not actually played).
        // Use given occupancy to determine attacks by sliding pieces.
        var attackers = 0ul;

        // Add pawn attackers.
        // Attacked by white pawn masks = black pawn attack masks and vice-versa.
        attackers |= GetPawns(Color.White) & Board.PawnAttackMasks[(int)Color.Black][(int)square];
        attackers |= GetPawns(Color.Black) & Board.PawnAttackMasks[(int)Color.White][(int)square];

        // Add knight attackers.
        attackers |= GetPieces(ColorlessPiece.Knight) & Board.KnightMoveMasks[(int)square];

        // Add diagonal sliding piece attackers.
        var bishops = GetPieces(ColorlessPiece.Bishop);
        var queens = GetPieces(ColorlessPiece.Queen);
        attackers |= (bishops | queens) & Board.PrecalculatedMoves.GetBishopMovesMask(square, modifiedOccupancy);

        // Add rank / file sliding piece attackers.
        var rooks = GetPieces(ColorlessPiece.Rook);
        attackers |= (rooks | queens) & Board.PrecalculatedMoves.GetRookMovesMask(square, modifiedOccupancy);

        // Add king attacker.
        attackers |= GetPieces(ColorlessPiece.King) & Board.KingMoveMasks[(int)square];

        return attackers & modifiedOccupancy;
    }


    public void Set(Position copyFromPosition)
    {
        // Defining a position via colored piece bitboards + color occupancy + occupancy is redundant.
        // But the engine plays slightly stronger than with colorless piece bitboards + color occupancy.

        // Copy bitboards.  Explicit array lookups are faster than looping through pieces.
        PieceBitboards[(int)Piece.WhitePawn] = copyFromPosition.PieceBitboards[(int)Piece.WhitePawn];
        PieceBitboards[(int)Piece.WhiteKnight] = copyFromPosition.PieceBitboards[(int)Piece.WhiteKnight];
        PieceBitboards[(int)Piece.WhiteBishop] = copyFromPosition.PieceBitboards[(int)Piece.WhiteBishop];
        PieceBitboards[(int)Piece.WhiteRook] = copyFromPosition.PieceBitboards[(int)Piece.WhiteRook];
        PieceBitboards[(int)Piece.WhiteQueen] = copyFromPosition.PieceBitboards[(int)Piece.WhiteQueen];
        PieceBitboards[(int)Piece.WhiteKing] = copyFromPosition.PieceBitboards[(int)Piece.WhiteKing];

        PieceBitboards[(int)Piece.BlackPawn] = copyFromPosition.PieceBitboards[(int)Piece.BlackPawn];
        PieceBitboards[(int)Piece.BlackKnight] = copyFromPosition.PieceBitboards[(int)Piece.BlackKnight];
        PieceBitboards[(int)Piece.BlackBishop] = copyFromPosition.PieceBitboards[(int)Piece.BlackBishop];
        PieceBitboards[(int)Piece.BlackRook] = copyFromPosition.PieceBitboards[(int)Piece.BlackRook];
        PieceBitboards[(int)Piece.BlackQueen] = copyFromPosition.PieceBitboards[(int)Piece.BlackQueen];
        PieceBitboards[(int)Piece.BlackKing] = copyFromPosition.PieceBitboards[(int)Piece.BlackKing];

        ColorOccupancy[(int) Color.White] = copyFromPosition.ColorOccupancy[(int) Color.White];
        ColorOccupancy[(int) Color.Black] = copyFromPosition.ColorOccupancy[(int) Color.Black];
        Occupancy = copyFromPosition.Occupancy;

        // Copy board state.  Do not copy values that will be set after moves are generated or played.
        ColorToMove = copyFromPosition.ColorToMove;
        Castling = copyFromPosition.Castling;
        EnPassantSquare = copyFromPosition.EnPassantSquare;
        PlySinceCaptureOrPawnMove = copyFromPosition.PlySinceCaptureOrPawnMove;
        FullMoveNumber = copyFromPosition.FullMoveNumber;
        PiecesSquaresKey = copyFromPosition.PiecesSquaresKey;
    }


    public void GenerateMoves()
    {
        PrepareMoveGeneration();
        FindPinnedPieces();
        GenerateMoves(MoveGeneration.AllMoves, Board.AllSquaresMask, Board.AllSquaresMask);
        MoveGenerationStage = MoveGenerationStage.Completed;
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
        MoveGenerationStage = MoveGenerationStage.Prepared;
    }


    private void GeneratePawnMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
    {
        var pawns = GetPawns(ColorToMove) & fromSquareMask;
        if (pawns == 0) return;

        var pawnMoveMasks = Board.PawnMoveMasks[(int)ColorToMove];
        var pawnDoubleMoveMasks = Board.PawnDoubleMoveMasks[(int)ColorToMove];
        var pawnAttackMasks = Board.PawnAttackMasks[(int)ColorToMove];

        var enemyOccupiedSquares = ColorOccupancy[(int)ColorPreviouslyMoved];
        var unoccupiedSquares = ~Occupancy;

        var ranks = Board.Ranks[(int)ColorToMove];

        var attacker = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, ColorToMove);
        var queen = PieceHelper.GetPieceOfColor(ColorlessPiece.Queen, ColorToMove);
        var knight = PieceHelper.GetPieceOfColor(ColorlessPiece.Knight, ColorToMove);
        var enPassantVictim = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, ColorPreviouslyMoved);

        Square fromSquare;
        ulong move;

        if ((EnPassantSquare != Square.Illegal) && ((Board.SquareMasks[(int)EnPassantSquare] & toSquareMask) > 0) && (moveGeneration != MoveGeneration.OnlyNonCaptures))
        {
            var enPassantAttackers = Board.EnPassantAttackerMasks[(int)EnPassantSquare] & pawns;

            while ((fromSquare = Bitwise.PopFirstSetSquare(ref enPassantAttackers)) != Square.Illegal)
            {
                // Capture pawn en passant.
                move = Move.Null;

                Move.SetPiece(ref move, attacker);
                Move.SetFrom(ref move, fromSquare);
                Move.SetTo(ref move, EnPassantSquare);
                Move.SetCaptureAttacker(ref move, attacker);
                Move.SetCaptureVictim(ref move, enPassantVictim);
                Move.SetIsEnPassantCapture(ref move, true);
                Move.SetIsPawnMove(ref move, true);

                Moves[MoveIndex] = move;
                MoveIndex++;
            }
        }

        while ((fromSquare = Bitwise.PopFirstSetSquare(ref pawns)) != Square.Illegal)
        {
            ulong pawnDestinations;
            Square toSquare;
            int toSquareRank;

            if (moveGeneration != MoveGeneration.OnlyCaptures)
            {
                // Pawns may move forward one square (or two if on initial square) if forward squares are unoccupied.
                pawnDestinations = pawnMoveMasks[(int)fromSquare] & unoccupiedSquares & toSquareMask;

                while ((toSquare = Bitwise.PopFirstSetSquare(ref pawnDestinations)) != Square.Illegal)
                {
                    var doubleMove = Board.SquareDistances[(int)fromSquare][(int)toSquare] == 2;
                    if (doubleMove && ((Occupancy & pawnDoubleMoveMasks[(int)fromSquare]) > 0)) continue; // Double move is blocked.
                    
                    toSquareRank = ranks[(int)toSquare];

                    if (toSquareRank < 7)
                    {
                        move = Move.Null;

                        Move.SetPiece(ref move, attacker);
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

                            Move.SetPiece(ref move, attacker);
                            Move.SetFrom(ref move, fromSquare);
                            Move.SetTo(ref move, toSquare);
                            Move.SetPromotedPiece(ref move, promotedPiece);
                            Move.SetIsPawnMove(ref move, true);

                            Moves[MoveIndex] = move;
                            MoveIndex++;
                        }
                    }
                }
            }

            if (moveGeneration != MoveGeneration.OnlyNonCaptures)
            {
                // Pawns may attack diagonally forward one square if occupied by enemy.
                pawnDestinations = pawnAttackMasks[(int)fromSquare] & enemyOccupiedSquares & toSquareMask;

                while ((toSquare = Bitwise.PopFirstSetSquare(ref pawnDestinations)) != Square.Illegal)
                {
                    toSquareRank = ranks[(int)toSquare];
                    var victim = GetPiece(toSquare);

                    if (toSquareRank < 7)
                    {
                        move = Move.Null;

                        Move.SetPiece(ref move, attacker);
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

                            Move.SetPiece(ref move, attacker);
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
                }
            }
        }
    }


    private void GeneratePieceMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
    {
        for (var colorlessPiece = ColorlessPiece.Knight; colorlessPiece <= ColorlessPiece.Queen; colorlessPiece++)
        {
            var attacker = PieceHelper.GetPieceOfColor(colorlessPiece, ColorToMove);
            var pieces = PieceBitboards[(int)attacker] & fromSquareMask;
            if (pieces == 0) continue;

            var getPieceMovesMask = Board.PieceMoveMaskDelegates[(int)colorlessPiece];
            
            var unOrEnemyOccupiedSquares = ~ColorOccupancy[(int)ColorToMove];
            var enemyOccupiedSquares = ColorOccupancy[(int)ColorPreviouslyMoved];

            Square fromSquare;
            while ((fromSquare = Bitwise.PopFirstSetSquare(ref pieces)) != Square.Illegal)
            {
                var pieceDestinations = moveGeneration switch
                {
                    MoveGeneration.AllMoves => getPieceMovesMask(fromSquare, Occupancy) & unOrEnemyOccupiedSquares & toSquareMask,
                    MoveGeneration.OnlyCaptures => getPieceMovesMask(fromSquare, Occupancy) & enemyOccupiedSquares & toSquareMask,
                    MoveGeneration.OnlyNonCaptures => getPieceMovesMask(fromSquare, Occupancy) & ~Occupancy & toSquareMask,
                    _ => throw new Exception($"{moveGeneration} move generation not supported.")
                };

                Square toSquare;
                while ((toSquare = Bitwise.PopFirstSetSquare(ref pieceDestinations)) != Square.Illegal)
                {
                    var victim = GetPiece(toSquare);
                    var move = Move.Null;

                    Move.SetPiece(ref move, attacker);
                    Move.SetFrom(ref move, fromSquare);
                    Move.SetTo(ref move, toSquare);
                    if (victim != Piece.None)
                    {
                        Move.SetCaptureAttacker(ref move, attacker);
                        Move.SetCaptureVictim(ref move, victim);
                    }

                    Moves[MoveIndex] = move;
                    MoveIndex++;
                }
            }
        }
    }


    private void GenerateKingMoves(MoveGeneration moveGeneration, ulong fromSquareMask, ulong toSquareMask)
    {
        var king = GetKing(ColorToMove) & fromSquareMask;
        if (king == 0) return;

        var unOrEnemyOccupiedSquares = ~ColorOccupancy[(int)ColorToMove];
        var enemyOccupiedSquares = ColorOccupancy[(int)ColorPreviouslyMoved];

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
        while ((toSquare = Bitwise.PopFirstSetSquare(ref kingDestinations)) != Square.Illegal)
        {
            var victim = GetPiece(toSquare);
            move = Move.Null;

            Move.SetPiece(ref move, attacker);
            Move.SetFrom(ref move, fromSquare);
            Move.SetTo(ref move, toSquare);
            if (victim != Piece.None)
            {
                Move.SetCaptureAttacker(ref move, attacker);
                Move.SetCaptureVictim(ref move, victim);
            }
            Move.SetIsKingMove(ref move, true);

            Moves[MoveIndex] = move;
            MoveIndex++;
        }

        if (moveGeneration != MoveGeneration.OnlyCaptures)
        {
            for (var boardSide = BoardSide.Queen; boardSide <= BoardSide.King; boardSide++)
            {
                var castleEmptySquaresMask = Board.CastleEmptySquaresMask[(int)ColorToMove][(int)boardSide];

                if (!KingInCheck && Game.Castling.Permitted(Castling, ColorToMove, boardSide) && ((Occupancy & castleEmptySquaresMask) == 0))
                {
                    // Castle
                    toSquare = Board.CastleToSquares[(int)ColorToMove][(int)boardSide];

                    if ((Board.SquareMasks[(int)toSquare] & toSquareMask) > 0)
                    {
                        move = Move.Null;

                        Move.SetPiece(ref move, attacker);
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
    }


    public void FindPinnedPieces()
    {
        var ownKingSquare = Bitwise.FirstSetSquare(GetKing(ColorToMove));
        var ownPieces = ColorOccupancy[(int)ColorToMove];

        var enemyRankFileAttackers = GetRooks(ColorPreviouslyMoved) | GetQueens(ColorPreviouslyMoved);
        var enemyDiagonalAttackers = GetBishops(ColorPreviouslyMoved) | GetQueens(ColorPreviouslyMoved);
        var enemyPieces = ColorOccupancy[(int)ColorPreviouslyMoved];

        // Find pieces pinned to own king by enemy rank / file attackers.
        PinnedPieces = 0;
        Square attackerSquare;

        while ((attackerSquare = Bitwise.PopFirstSetSquare(ref enemyRankFileAttackers)) != Square.Illegal)
        {
            var betweenSquares = Board.RankFileBetweenSquares[(int)attackerSquare][(int)ownKingSquare];
            if (betweenSquares == 0) continue;

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
        }

        // Find pieces pinned to own king by enemy diagonal attackers.
        while ((attackerSquare = Bitwise.PopFirstSetSquare(ref enemyDiagonalAttackers)) != Square.Illegal)
        {
            var betweenSquares = Board.DiagonalBetweenSquares[(int)attackerSquare][(int)ownKingSquare];
            if (betweenSquares == 0) continue;

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
        }
    }


    public bool ValidateMove(ref ulong move)
    {
        // Do not trust move that wasn't generated by engine (from cache, game notation, input by user, etc).
        // Validate main aspects of the move.  Do not test for every impossibility.
        // Goal is to prevent engine crashes, not ensure a perfectly legal search tree.
        var fromSquare = Move.From(move);
        var toSquare = Move.To(move);

        var attacker = GetPiece(fromSquare);
        if (attacker == Piece.None) return false; // No piece on from square.

        var attackerColor = PieceHelper.GetColor(attacker);
        if (attackerColor != ColorToMove) return false; // Piece is wrong color.

        var colorlessAttacker = PieceHelper.GetColorlessPiece(attacker);
        var victim = GetPiece(toSquare);
        if ((victim != Piece.None) && (PieceHelper.GetColor(victim) == attackerColor)) return false; // Piece cannot attack its own color.
        if ((victim == Piece.WhiteKing) || (victim == Piece.BlackKing)) return false;  // Piece cannot attack king.

        var promotedPiece = Move.PromotedPiece(move);
        if ((promotedPiece != Piece.None) && (ColorToMove != PieceHelper.GetColor(promotedPiece))) return false; // Promoted piece is wrong color.

        var distance = Board.SquareDistances[(int)fromSquare][(int)toSquare];
        if (distance > 1)
        {
            // For sliding pieces, validate to square is reachable and not blocked.
            ulong betweenSquares;
            // ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
            switch (colorlessAttacker)
            {
                case ColorlessPiece.Bishop:
                    betweenSquares = Board.DiagonalBetweenSquares[(int)fromSquare][(int)toSquare];
                    if ((betweenSquares == 0) || ((Occupancy & betweenSquares) > 0)) return false;
                    break;

                case ColorlessPiece.Rook:
                    betweenSquares = Board.RankFileBetweenSquares[(int)fromSquare][(int)toSquare];
                    if ((betweenSquares == 0) || ((Occupancy & betweenSquares) > 0)) return false;
                    break;

                case ColorlessPiece.Queen:
                    betweenSquares = Board.DiagonalBetweenSquares[(int)fromSquare][(int)toSquare];
                    if (betweenSquares == 0) betweenSquares = Board.RankFileBetweenSquares[(int)fromSquare][(int)toSquare];
                    if ((betweenSquares == 0) || ((Occupancy & betweenSquares) > 0)) return false;
                    break;
            }
            // ReSharper restore SwitchStatementMissingSomeEnumCasesNoDefault
        }

        var pawn = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, ColorToMove);
        var king = PieceHelper.GetPieceOfColor(ColorlessPiece.King, ColorToMove);
        var toRank = Board.Ranks[(int)ColorToMove][(int)toSquare];

        if ((attacker == pawn) && (toRank == 7) && (promotedPiece == Piece.None)) return false; // Pawn cannot move to eighth rank without promoting.
        if ((promotedPiece != Piece.None) && (attacker != pawn)) return false; // Only pawns can promote.
        if ((promotedPiece == pawn) || (promotedPiece == king)) return false; // Cannot promote pawn to pawn or king.

        var castling = (attacker == king) && (distance == 2);
        if (castling)
        {
            var boardSide = Board.Files[(int)toSquare] < 4 ? BoardSide.Queen : BoardSide.King;
            if (!Game.Castling.Permitted(Castling, ColorToMove, boardSide)) return false; // Castle not possible.
            if (toSquare != Board.CastleToSquares[(int)ColorToMove][(int)boardSide]) return false; // Castle destination square invalid.
            if ((Occupancy & Board.CastleEmptySquaresMask[(int)ColorToMove][(int)boardSide]) > 0) return false; // Castle squares occupied.
        }

        // Set move properties.
        Move.SetPiece(ref move, attacker);
        Move.SetIsPawnMove(ref move, attacker == pawn);
        Move.SetIsDoublePawnMove(ref move, (attacker == pawn) && (distance == 2));
        Move.SetIsKingMove(ref move, attacker == king);
        Move.SetIsCastling(ref move, castling);
        if ((attacker == pawn) && (toSquare == EnPassantSquare))
        {
            // En Passant Capture
            Move.SetIsEnPassantCapture(ref move, true);
            Move.SetCaptureAttacker(ref move, attacker);
            Move.SetCaptureVictim(ref move, PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, ColorPreviouslyMoved));
        }
        else if (victim != Piece.None)
        {
            Move.SetCaptureAttacker(ref move, attacker);
            Move.SetCaptureVictim(ref move, victim);
        }

        return true;
    }


    public void Reset()
    {
        for (var piece = Piece.WhitePawn; piece <= Piece.BlackKing; piece++)
            PieceBitboards[(int) piece] = 0;

        ColorOccupancy[(int) Color.White] = 0;
        ColorOccupancy[(int) Color.Black] = 0;
        Occupancy = 0;
        PinnedPieces = 0;

        ColorToMove = Color.White;

        Castling = 0;
        EnPassantSquare = Square.Illegal;

        PlySinceCaptureOrPawnMove = 0;
        FullMoveNumber = 0;
        KingInCheck = false;

        CurrentMoveIndex = 0;
        MoveIndex = 0;
        MoveGenerationStage = MoveGenerationStage.Reset;

        PiecesSquaresKey = 0;
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
                stringBuilder.Append('/');
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
        stringBuilder.Append(' ');
        stringBuilder.Append(ColorToMove == Color.White ? "w" : "b");
        stringBuilder.Append(' ');

        stringBuilder.Append(Game.Castling.ToString(Castling));
        stringBuilder.Append(' ');

        stringBuilder.Append(EnPassantSquare == Square.Illegal ? "-" : Board.SquareLocations[(int)EnPassantSquare]);
        stringBuilder.Append(' ');

        stringBuilder.Append(PlySinceCaptureOrPawnMove);
        stringBuilder.Append(' ');

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


    public string ToString(int positionCount)
    {
        // Iterate over the piece array to construct an 8 x 8 text display of the chessboard.
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("  +---+---+---+---+---+---+---+---+");

        var square = Square.A8;
        for (var y = 8; y > 0; y--)
        {
            // Add rank.
            stringBuilder.Append(y);
            stringBuilder.Append(' ');

            for (var x = 1; x <= 8; x++)
            {
                stringBuilder.Append("| ");

                var piece = GetPiece(square);
                if (piece == Piece.None) stringBuilder.Append(' ');
                else stringBuilder.Append(PieceHelper.GetChar(piece));

                stringBuilder.Append(' ');
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

        // Display position properties.
        stringBuilder.AppendLine($"FEN:             {ToFen()}");
        stringBuilder.AppendLine($"Key:             {Key:X16}");
        stringBuilder.AppendLine($"Position Count:  {positionCount}");
        stringBuilder.AppendLine($"King in Check:   {(KingInCheck ? "Yes" : "No")}");
        stringBuilder.AppendLine($"Static Score:    {StaticScore}");
        stringBuilder.AppendLine("Played Move:");
        stringBuilder.AppendLine();
        stringBuilder.Append(Move.ToString(PlayedMove));

        return stringBuilder.ToString();
    }
}