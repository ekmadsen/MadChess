// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine
{
    public static class PieceHelper
    {
        public static char GetChar(Piece piece)
        {
            // Sequence cases in order of enum value to improve performance of switch statement.
            return piece switch
            {
                Piece.None => ' ',
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


        public static string GetName(Piece piece)
        {
            // Sequence cases in order of integer value to improve performance of switch statement.
            return piece switch
            {
                Piece.None => string.Empty,
                Piece.WhitePawn => "Pawn",
                Piece.WhiteKnight => "Knight",
                Piece.WhiteBishop => "Bishop",
                Piece.WhiteRook => "Rook",
                Piece.WhiteQueen => "Queen",
                Piece.WhiteKing => "King",
                Piece.BlackPawn => "Pawn",
                Piece.BlackKnight => "Knight",
                Piece.BlackBishop => "Bishop",
                Piece.BlackRook => "Rook",
                Piece.BlackQueen => "Queen",
                Piece.BlackKing => "King",
                _ => throw new ArgumentException($"{piece} piece not supported.")
            };
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhite(Piece piece) => piece <= Piece.WhiteKing;


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
}
