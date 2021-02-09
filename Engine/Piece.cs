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
    public static class Piece
    {
        public const int None = 0;
        public const int WhitePawn = 1;
        public const int WhiteKnight = 2;
        public const int WhiteBishop = 3;
        public const int WhiteRook = 4;
        public const int WhiteQueen = 5;
        public const int WhiteKing = 6;
        public const int BlackPawn = 7;
        public const int BlackKnight = 8;
        public const int BlackBishop = 9;
        public const int BlackRook = 10;
        public const int BlackQueen = 11;
        public const int BlackKing = 12;

        
        public static char GetChar(int Piece)
        {
            // Sequence cases in order of integer value to improve performance of switch statement.
            return Piece switch
            {
                None => ' ',
                WhitePawn => 'P',
                WhiteKnight => 'N',
                WhiteBishop => 'B',
                WhiteRook => 'R',
                WhiteQueen => 'Q',
                WhiteKing => 'K',
                BlackPawn => 'p',
                BlackKnight => 'n',
                BlackBishop => 'b',
                BlackRook => 'r',
                BlackQueen => 'q',
                BlackKing => 'k',
                _ => throw new ArgumentException($"{Piece} piece not supported.")
            };
        }


        public static string GetName(int Piece)
        {
            // Sequence cases in order of integer value to improve performance of switch statement.
            return Piece switch
            {
                None => string.Empty,
                WhitePawn => "Pawn",
                WhiteKnight => "Knight",
                WhiteBishop => "Bishop",
                WhiteRook => "Rook",
                WhiteQueen => "Queen",
                WhiteKing => "King",
                BlackPawn => "Pawn",
                BlackKnight => "Knight",
                BlackBishop => "Bishop",
                BlackRook => "Rook",
                BlackQueen => "Queen",
                BlackKing => "King",
                _ => throw new ArgumentException($"{Piece} piece not supported.")
            };
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhite(int Piece) => Piece <= WhiteKing;


        public static int ParseChar(char Character)
        {
            return Character switch
            {
                'P' => WhitePawn,
                'N' => WhiteKnight,
                'B' => WhiteBishop,
                'R' => WhiteRook,
                'Q' => WhiteQueen,
                'K' => WhiteKing,
                'p' => BlackPawn,
                'n' => BlackKnight,
                'b' => BlackBishop,
                'r' => BlackRook,
                'q' => BlackQueen,
                'k' => BlackKing,
                _ => throw new ArgumentException($"{Character} character not supported.")
            };
        }
    }
}