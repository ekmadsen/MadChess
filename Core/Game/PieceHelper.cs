// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Core.Game;


public static class PieceHelper
{
    public static char? GetChar(Piece piece)
    {
        // Sequence cases in order of enum value to improve performance of switch statement.
        return piece switch
        {
            Piece.None => null,
            Piece.WhitePawn => 'P',
            Piece.WhiteKnight => 'N',
            Piece.WhiteBishop => 'B',
            Piece.WhiteRook => 'R',
            Piece.WhiteQueen => 'Q',
            Piece.WhiteKing => 'K',
            Piece.BlackPawn => 'p',
            Piece.BlackKnight => 'n',
            Piece.BlackBishop => 'b',
            Piece.BlackRook => 'r',
            Piece.BlackQueen => 'q',
            Piece.BlackKing => 'k',
            _ => throw new ArgumentException($"{piece} piece not supported.")
        };
    }


    public static string GetName(Piece piece) => GetName(GetColorlessPiece(piece));


    public static string GetName(ColorlessPiece colorlessPiece)
    {
        // Sequence cases in order of enum value to improve performance of switch statement.
        return colorlessPiece switch
        {
            ColorlessPiece.None => string.Empty,
            ColorlessPiece.Pawn => "Pawn",
            ColorlessPiece.Knight => "Knight",
            ColorlessPiece.Bishop => "Bishop",
            ColorlessPiece.Rook => "Rook",
            ColorlessPiece.Queen => "Queen",
            ColorlessPiece.King => "King",
            _ => throw new ArgumentException($"{colorlessPiece} piece not supported.")
        };
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ColorlessPiece GetColorlessPiece(Piece piece) => piece <= Piece.WhiteKing ? (ColorlessPiece)piece : (ColorlessPiece)(piece - Piece.WhiteKing);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece GetPieceOfColor(ColorlessPiece colorlessPiece, Color color) => ((int) color * (int) Piece.WhiteKing) + (Piece) colorlessPiece;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color GetColor(Piece piece) => piece <= Piece.WhiteKing ? Color.White : Color.Black;


    public static Piece ParseChar(char character)
    {
        return character switch
        {
            'P' => Piece.WhitePawn,
            'N' => Piece.WhiteKnight,
            'B' => Piece.WhiteBishop,
            'R' => Piece.WhiteRook,
            'Q' => Piece.WhiteQueen,
            'K' => Piece.WhiteKing,
            'p' => Piece.BlackPawn,
            'n' => Piece.BlackKnight,
            'b' => Piece.BlackBishop,
            'r' => Piece.BlackRook,
            'q' => Piece.BlackQueen,
            'k' => Piece.BlackKing,
            _ => throw new ArgumentException($"{character} character not supported.")
        };
    }
}