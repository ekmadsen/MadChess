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
    public sealed class MoveScoreComparer : IComparer<int>
    {
        public int Compare(int Score1, int Score2)
        {
            // Sort moves by score descending.
            if (Score2 > Score1) return 1;
            return Score2 < Score1 ? -1 : 0;
        }
    }
}
