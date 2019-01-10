// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine
{
    // Piece must be a primitive type (not an enum) to use Buffer.BlockCopy in Position.Set method.
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
            // Sequence cases in order of enum integer value to improve performance of switch statement.
            switch (Piece)
            {
                case None:
                    return ' ';
                case WhitePawn:
                    return 'P';
                case WhiteKnight:
                    return 'N';
                case WhiteBishop:
                    return 'B';
                case WhiteRook:
                    return 'R';
                case WhiteQueen:
                    return 'Q';
                case WhiteKing:
                    return 'K';
                case BlackPawn:
                    return 'p';
                case BlackKnight:
                    return 'n';
                case BlackBishop:
                    return 'b';
                case BlackRook:
                    return 'r';
                case BlackQueen:
                    return 'q';
                case BlackKing:
                    return 'k';
                default:
                    throw new ArgumentException($"{Piece} piece not supported.");
            }
        }


        public static string GetName(int Piece)
        {
            // Sequence cases in order of enum integer value to improve performance of switch statement.
            switch (Piece)
            {
                case None:
                    return string.Empty;
                case WhitePawn:
                    return "Pawn";
                case WhiteKnight:
                    return "Knight";
                case WhiteBishop:
                    return "Bishop";
                case WhiteRook:
                    return "Rook";
                case WhiteQueen:
                    return "Queen";
                case WhiteKing:
                    return "King";
                case BlackPawn:
                    return "Pawn";
                case BlackKnight:
                    return "Knight";
                case BlackBishop:
                    return "Bishop";
                case BlackRook:
                    return "Rook";
                case BlackQueen:
                    return "Queen";
                case BlackKing:
                    return "King";
                default:
                    throw new ArgumentException($"{Piece} piece not supported.");
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhite(int Piece) => Piece <= WhiteKing;


        public static int ParseChar(char Character)
        {
            switch (Character)
            {
                case 'P':
                    return WhitePawn;
                case 'N':
                    return WhiteKnight;
                case 'B':
                    return WhiteBishop;
                case 'R':
                    return WhiteRook;
                case 'Q':
                    return WhiteQueen;
                case 'K':
                    return WhiteKing;
                case 'p':
                    return BlackPawn;
                case 'n':
                    return BlackKnight;
                case 'b':
                    return BlackBishop;
                case 'r':
                    return BlackRook;
                case 'q':
                    return BlackQueen;
                case 'k':
                    return BlackKing;
                default:
                    throw new ArgumentException($"{Character} character not supported.");
            }
        }
    }
}