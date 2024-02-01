// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine.Score;


public sealed class ScoredMovePriorityComparer : IComparer<ScoredMove>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(ScoredMove move1, ScoredMove move2)
    {
        // Sort moves by priority descending.
        if (move2.Move > move1.Move) return 1;
        return move2.Move < move1.Move ? -1 : 0;
    }
}