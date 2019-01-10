// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;


namespace ErikTheCoder.MadChess.Engine
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


        // Castling bits

        // 3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0
        // 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
        //                                                         K|Q|k|q

        // K = White castle kingside
        // Q = White castle queenside
        // k = Black castle kingside
        // q = Black castle queenside


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
        public static bool IsPossible(uint Castling) => Castling > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WhiteKingside(uint Castling) => (Castling & _whiteKingsideMask) > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWhiteKingside(ref uint Castling, bool WhiteKingside)
        {
            uint whiteKingSide = WhiteKingside ? 1u : 0;
            // Clear.
            Castling &= _whiteKingsideUnmask;
            // Set.
            Castling |= whiteKingSide << _whiteKingsideShift;
            // Validate move.
            Debug.Assert(Engine.Castling.WhiteKingside(Castling) == WhiteKingside);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WhiteQueenside(uint Castling) => (Castling & _whiteQueensideMask) > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWhiteQueenside(ref uint Castling, bool WhiteQueenside)
        {
            uint whiteQueenside = WhiteQueenside ? 1u : 0;
            // Clear.
            Castling &= _whiteQueensideUnmask;
            // Set.
            Castling |= whiteQueenside << _whiteQueensideShift;
            // Validate move.
            Debug.Assert(Engine.Castling.WhiteQueenside(Castling) == WhiteQueenside);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BlackKingside(uint Castling) => (Castling & _blackKingsideMask) > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBlackKingside(ref uint Castling, bool BlackKingside)
        {
            uint blackKingSide = BlackKingside ? 1u : 0;
            // Clear.
            Castling &= _blackKingsideUnmask;
            // Set.
            Castling |= blackKingSide << _blackKingsideShift;
            // Validate move.
            Debug.Assert(Engine.Castling.BlackKingside(Castling) == BlackKingside);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BlackQueenside(uint Castling) => (Castling & _blackQueensideMask) > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBlackQueenside(ref uint Castling, bool BlackQueenside)
        {
            uint blackQueenside = BlackQueenside ? 1u : 0;
            // Clear.
            Castling &= _blackQueensideUnmask;
            // Set.
            Castling |= blackQueenside;
            // Validate move.
            Debug.Assert(Engine.Castling.BlackQueenside(Castling) == BlackQueenside);
        }


        public static string ToString(uint Castling)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool anyCastlingRights = false;
            if (WhiteKingside(Castling))
            {
                stringBuilder.Append("K");
                anyCastlingRights = true;
            }
            if (WhiteQueenside(Castling))
            {
                stringBuilder.Append("Q");
                anyCastlingRights = true;
            }
            if (BlackKingside(Castling))
            {
                stringBuilder.Append("k");
                anyCastlingRights = true;
            }
            if (BlackQueenside(Castling))
            {
                stringBuilder.Append("q");
                anyCastlingRights = true;
            }
            if (!anyCastlingRights) stringBuilder.Append("-");
            return stringBuilder.ToString();
        }
    }
}