﻿// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using JetBrains.Annotations;


namespace ErikTheCoder.MadChess.Engine.Uci;


public enum CommandDirection
{
    [UsedImplicitly] Unknown,
    In,
    Out
}