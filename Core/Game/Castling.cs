// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Core.Game
{
    public static class Castling
    {
        private static readonly int _whiteKingsideShift;
        private static readonly uint _whiteKingsideMask;
        private static readonly uint _whiteKingsideUnmask;
        private static readonly int _whiteQueensideShift;
        private static readonly uint _whiteQueensideMask;
        private static readonly uint _whiteQueensideUnmask;
        private static readonly int _blackKingsideShift;
        private static readonly uint _blackKingsideMask;
        private static readonly uint _blackKingsideUnmask;
        private static readonly uint _blackQueensideMask;
        private static readonly uint _blackQueensideUnmask;


        // Castling Bits

        // 3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0
        // 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
        //                                                         K|Q|k|q

        // K = White Castle Kingside
        // Q = White Castle Queenside
        // k = Black Castle Kingside
        // q = Black Castle Queenside


        // TODO: Represent castling rights via byte instead of uint.
        static Castling()
        {
            // Create bit shifts and masks.
            _whiteKingsideShift = 3;
            _whiteKingsideMask = Bitwise.CreateUIntMask(3, 3);
            _whiteKingsideUnmask = Bitwise.CreateUIntUnmask(3, 3);
            _whiteQueensideShift = 2;
            _whiteQueensideMask = Bitwise.CreateUIntMask(2, 2);
            _whiteQueensideUnmask = Bitwise.CreateUIntUnmask(2, 2);
            _blackKingsideShift = 1;
            _blackKingsideMask = Bitwise.CreateUIntMask(1, 1);
            _blackKingsideUnmask = Bitwise.CreateUIntUnmask(1, 1);
            _blackQueensideMask = Bitwise.CreateUIntMask(0, 0);
            _blackQueensideUnmask = Bitwise.CreateUIntUnmask(0, 0);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPossible(uint castling) => castling > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WhiteKingside(uint castling) => (castling & _whiteKingsideMask) > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWhiteKingside(ref uint castling, bool whiteKingside)
        {
            var whiteKingSide = whiteKingside ? 1u : 0;
            // Clear.
            castling &= _whiteKingsideUnmask;
            // Set.
            castling |= (whiteKingSide << _whiteKingsideShift) & _whiteKingsideMask;
            // Validate move.
            Debug.Assert(WhiteKingside(castling) == whiteKingside);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WhiteQueenside(uint castling) => (castling & _whiteQueensideMask) > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWhiteQueenside(ref uint castling, bool whiteQueenside)
        {
            var value = whiteQueenside ? 1u : 0;
            // Clear.
            castling &= _whiteQueensideUnmask;
            // Set.
            castling |= (value << _whiteQueensideShift) & _whiteQueensideMask;
            // Validate move.
            Debug.Assert(WhiteQueenside(castling) == whiteQueenside);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BlackKingside(uint castling) => (castling & _blackKingsideMask) > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBlackKingside(ref uint castling, bool blackKingside)
        {
            var blackKingSide = blackKingside ? 1u : 0;
            // Clear.
            castling &= _blackKingsideUnmask;
            // Set.
            castling |= (blackKingSide << _blackKingsideShift) & _blackKingsideMask;
            // Validate move.
            Debug.Assert(BlackKingside(castling) == blackKingside);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BlackQueenside(uint castling) => (castling & _blackQueensideMask) > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBlackQueenside(ref uint castling, bool blackQueenside)
        {
            var value = blackQueenside ? 1u : 0;
            // Clear.
            castling &= _blackQueensideUnmask;
            // Set.
            castling |= value & _blackQueensideMask;
            // Validate move.
            Debug.Assert(BlackQueenside(castling) == blackQueenside);
        }


        public static string ToString(uint castling)
        {
            var stringBuilder = new StringBuilder();
            var anyCastlingRights = false;
            if (WhiteKingside(castling))
            {
                stringBuilder.Append('K');
                anyCastlingRights = true;
            }
            if (WhiteQueenside(castling))
            {
                stringBuilder.Append('Q');
                anyCastlingRights = true;
            }
            if (BlackKingside(castling))
            {
                stringBuilder.Append('k');
                anyCastlingRights = true;
            }
            if (BlackQueenside(castling))
            {
                stringBuilder.Append('q');
                anyCastlingRights = true;
            }
            if (!anyCastlingRights) stringBuilder.Append("-");
            return stringBuilder.ToString();
        }
    }
}