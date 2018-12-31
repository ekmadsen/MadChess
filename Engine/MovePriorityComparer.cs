// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Collections.Generic;


namespace MadChess.Engine
{
    public sealed class MovePriorityComparer : IComparer<ulong>
    {
        public int Compare(ulong Move1, ulong Move2)
        {
            // Sort moves by priority descending.
            if (Move2 > Move1) return 1;
            return Move2 < Move1 ? -1 : 0;
        }
    }
}

