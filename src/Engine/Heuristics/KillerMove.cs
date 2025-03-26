// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;


namespace ErikTheCoder.MadChess.Engine.Heuristics;


public readonly struct KillerMove(Piece piece, Square toSquare) : IEquatable<KillerMove>
{
    public static readonly KillerMove Null;
    public readonly Piece Piece = piece;
    public readonly Square ToSquare = toSquare;


    static KillerMove()
    {
        Null = new KillerMove(Piece.None, Square.Illegal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(KillerMove killerMove1, KillerMove killerMove2) => (killerMove1.Piece == killerMove2.Piece) && (killerMove1.ToSquare == killerMove2.ToSquare);

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(KillerMove killerMove1, KillerMove killerMove2) => (killerMove1.Piece != killerMove2.Piece) || (killerMove1.ToSquare != killerMove2.ToSquare);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(KillerMove otherKillerMove) => otherKillerMove == this;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object other) => (other is KillerMove otherKillerMove) && otherKillerMove == this;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => HashCode.Combine((int)Piece, (int)ToSquare);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KillerMove Parse(ulong move) => new(Move.Piece(move), Move.To(move));
}