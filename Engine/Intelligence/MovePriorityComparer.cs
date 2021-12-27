// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine.Intelligence;


public sealed class MovePriorityComparer : IComparer<ulong>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(ulong move1, ulong move2)
    {
        // Sort moves by priority descending.
        if (move2 > move1) return 1;
        return move2 < move1 ? -1 : 0;
    }
}