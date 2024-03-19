// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


// using JetBrains.Annotations;


namespace ErikTheCoder.MadChess.Core.Game;


public enum Direction
{
    Unknown,

    // Sliding Piece Moves
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest,

    // Knight Moves
    North2East1,
    East2North1,
    East2South1,
    South2East1,
    South2West1,
    West2South1,
    West2North1,
    North2West1
}