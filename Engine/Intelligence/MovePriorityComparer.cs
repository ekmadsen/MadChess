// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace ErikTheCoder.MadChess.Engine.Intelligence
{
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
}

