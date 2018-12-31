// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using JetBrains.Annotations;


namespace MadChess.Engine
{
    public enum CommandDirection
    {
        [UsedImplicitly] Unknown,
        In,
        Out
    }
}
