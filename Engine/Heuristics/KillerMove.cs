﻿// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;


namespace ErikTheCoder.MadChess.Engine.Heuristics;


public readonly struct KillerMove
{
    private readonly Piece _piece;
    private readonly Square _toSquare;


    public KillerMove(Piece piece, Square toSquare)
    {
        _piece = piece;
        _toSquare = toSquare;
    }


    public static bool operator ==(KillerMove killerMove1, KillerMove killerMove2) => (killerMove1._piece == killerMove2._piece) && (killerMove1._toSquare == killerMove2._toSquare);
    public static bool operator !=(KillerMove killerMove1, KillerMove killerMove2) => (killerMove1._piece != killerMove2._piece) || (killerMove1._toSquare != killerMove2._toSquare);
    // ReSharper disable once MemberCanBePrivate.Global
    public bool Equals(KillerMove otherKillerMove) => (_piece == otherKillerMove._piece) && (_toSquare == otherKillerMove._toSquare);
    public override bool Equals(object other) => (other is KillerMove otherKillerMove) && Equals(otherKillerMove);
    public override int GetHashCode() => HashCode.Combine((int)_piece, (int)_toSquare);
    public static KillerMove Parse(Position position, ulong move) => new(position.GetPiece(Move.From(move)), Move.To(move));
}