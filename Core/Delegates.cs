// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Game;


namespace ErikTheCoder.MadChess.Core;


public static class Delegates
{
    public delegate bool ValidateMove(ref ulong move);
    public delegate bool Debug();
    public delegate void WriteMessageLine(string message);
    public delegate ulong GetPieceMovesMask(Square fromSquare, ulong occupancy);
}