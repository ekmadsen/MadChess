﻿// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using JetBrains.Annotations;


namespace MadChess.Engine
{
    public enum Direction
    {
        [UsedImplicitly] Unknown,
        // Sliding piece moves
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
        // Knight moves
        North2East1,
        East2North1,
        East2South1,
        South2East1,
        South2West1,
        West2South1,
        West2North1,
        North2West1
    }
}