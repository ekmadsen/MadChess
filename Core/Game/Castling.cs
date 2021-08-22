// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
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
        private static readonly int[][] _shifts;
        private static readonly uint[][] _masks;
        private static readonly uint[][] _unmasks;


        // Castling Bits

        // 3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0
        // 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
        //                                                         K|Q|k|q

        // K = White Castle Kingside
        // Q = White Castle Queenside
        // k = Black Castle Kingside
        // q = Black Castle Queenside


        static Castling()
        {
            // Create bit shifts and masks.
            _shifts = new[]
            {
                new[]
                {
                    2, 3
                },
                new[]
                {
                    0, 1
                }
            };
            _masks = new[]
            {
                new[]
                {
                    Bitwise.CreateUIntMask(2),
                    Bitwise.CreateUIntMask(3)
                },
                new[]
                {
                    Bitwise.CreateUIntMask(0),
                    Bitwise.CreateUIntMask(1)
                }
            };
            _unmasks = new[]
            {
                new[]
                {
                    Bitwise.CreateUIntUnmask(2),
                    Bitwise.CreateUIntUnmask(3)
                },
                new[]
                {
                    Bitwise.CreateUIntUnmask(0),
                    Bitwise.CreateUIntUnmask(1)
                }
            };
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Permitted(uint castling) => castling > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Permitted(uint castling, Color color, BoardSide boardSide) => (castling & _masks[(int)color][(int)boardSide]) > 0;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(ref uint castling, Color color, BoardSide boardSide, bool permitted)
        {
            var value = permitted ? 1u : 0;
            // Clear.
            castling &= _unmasks[(int)color][(int)boardSide];
            // Set.
            castling |= (value << _shifts[(int)color][(int)boardSide]) & _masks[(int)color][(int)boardSide];
            // Validate move.
            Debug.Assert(Permitted(castling, color, boardSide) == permitted);
        }


        public static string ToString(uint castling)
        {
            var stringBuilder = new StringBuilder();
            var anyCastlingRights = false;
            if (Permitted(castling, Color.White, BoardSide.King))
            {
                stringBuilder.Append('K');
                anyCastlingRights = true;
            }
            if (Permitted(castling, Color.White, BoardSide.Queen))
            {
                stringBuilder.Append('Q');
                anyCastlingRights = true;
            }
            if (Permitted(castling, Color.Black, BoardSide.King))
            {
                stringBuilder.Append('k');
                anyCastlingRights = true;
            }
            if(Permitted(castling, Color.Black, BoardSide.Queen))
            {
                stringBuilder.Append('q');
                anyCastlingRights = true;
            }
            return anyCastlingRights ? stringBuilder.ToString() : "-";
        }
    }
}