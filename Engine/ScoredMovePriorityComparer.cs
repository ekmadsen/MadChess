// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class ScoredMovePriorityComparer : IComparer<ScoredMove>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ScoredMove Move1, ScoredMove Move2)
        {
            // Sort moves by priority descending.
            if (Move2.Score > Move1.Score) return 1;
            return Move2.Score < Move1.Score ? -1 : 0;
        }
    }
}