// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


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