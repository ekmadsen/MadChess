// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Game;


namespace ErikTheCoder.MadChess.Core;


public static class Delegates
{
    // TODO: Replace Core delegates with injected services (if circular dependencies can be avoided).
    public delegate bool ValidateMove(ref ulong move);
    public delegate bool Debug();
    public delegate void WriteMessageLine(string message);
    public delegate ulong GetPieceMovesMask(Square fromSquare, ulong occupancy);
    public delegate ulong GetPieceXrayMovesMask(Square fromSquare, Color color, Position position);
}