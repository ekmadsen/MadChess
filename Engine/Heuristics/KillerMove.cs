// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Game;


namespace ErikTheCoder.MadChess.Engine.Heuristics;


public sealed class KillerMove
{
    public Piece Piece;
    public Square ToSquare;


    public KillerMove(Piece piece, Square toSquare)
    {
        Piece = piece;
        ToSquare = toSquare;
    }
}