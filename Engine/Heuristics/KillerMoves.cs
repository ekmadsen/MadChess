// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;


namespace ErikTheCoder.MadChess.Engine.Heuristics;


public sealed class KillerMoves
{
    private readonly KillerMove[][] _killerMoves;


    public KillerMoves(int maxDepth)
    {
        _killerMoves = new KillerMove[maxDepth + 1][];
        for (var depth = 0; depth <= maxDepth; depth++) _killerMoves[depth] = new[] {new KillerMove(Piece.None, Square.Illegal), new KillerMove(Piece.None, Square.Illegal)};
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetValue(Position position, int depth, ulong move)
    {
        if (Equals(position, _killerMoves[depth][0], move)) return 2;
        return Equals(position, _killerMoves[depth][1], move) ? 1 : 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateValue(Position position, int depth, ulong move)
    {
        if (Equals(position, _killerMoves[depth][0], move)) return; // Move already is the best killer move.
        // Shift killer move.
        _killerMoves[depth][1].Piece = _killerMoves[depth][0].Piece;
        _killerMoves[depth][1].ToSquare = _killerMoves[depth][0].ToSquare;
        // Update killer move.
        _killerMoves[depth][0].Piece = position.GetPiece(Move.From(move));
        _killerMoves[depth][0].ToSquare = Move.To(move);
    }


    public void Shift(int depth)
    {
        // Shift killer moves closer to root position.
        var lastDepth = _killerMoves.Length - depth - 1;
        for (var depthIndex = 0; depthIndex <= lastDepth; depthIndex++)
        {
            _killerMoves[depthIndex][0].Piece = _killerMoves[depthIndex + depth][0].Piece;
            _killerMoves[depthIndex][0].ToSquare = _killerMoves[depthIndex + depth][0].ToSquare;
            _killerMoves[depthIndex][1].Piece = _killerMoves[depthIndex + depth][1].Piece;
            _killerMoves[depthIndex][1].ToSquare = _killerMoves[depthIndex + depth][1].ToSquare;
        }
        // Reset killer moves far from root position.
        for (var depthIndex = lastDepth + 1; depthIndex < _killerMoves.Length; depthIndex++)
        {
            _killerMoves[depthIndex][0].Piece = Piece.None;
            _killerMoves[depthIndex][0].ToSquare = Square.Illegal;
            _killerMoves[depthIndex][1].Piece = Piece.None;
            _killerMoves[depthIndex][1].ToSquare = Square.Illegal;
        }
    }


    public void Reset()
    {
        for (var depth = 0; depth < _killerMoves.Length; depth++)
        {
            var killerMoves = _killerMoves[depth];
            killerMoves[0].Piece = Piece.None;
            killerMoves[0].ToSquare = Square.Illegal;
            killerMoves[1].Piece = Piece.None;
            killerMoves[1].ToSquare = Square.Illegal;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Equals(Position position, KillerMove killerMove, ulong move) => (killerMove.Piece == position.GetPiece(Move.From(move))) && (killerMove.ToSquare == Move.To(move));
}