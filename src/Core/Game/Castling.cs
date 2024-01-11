// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Core.Game;


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
        _shifts =
        [
            [
                2, 3
            ],
            [
                0, 1
            ]
        ];

        _masks =
        [
            [
                Bitwise.CreateUIntMask(2),
                Bitwise.CreateUIntMask(3)
            ],
            [
                Bitwise.CreateUIntMask(0),
                Bitwise.CreateUIntMask(1)
            ]
        ];

        _unmasks =
        [
            [
                Bitwise.CreateUIntUnmask(2),
                Bitwise.CreateUIntUnmask(3)
            ],
            [
                Bitwise.CreateUIntUnmask(0),
                Bitwise.CreateUIntUnmask(1)
            ]
        ];
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