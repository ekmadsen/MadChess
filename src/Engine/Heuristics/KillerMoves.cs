// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Engine.Intelligence;


namespace ErikTheCoder.MadChess.Engine.Heuristics;


public sealed class KillerMoves
{
    private const int _maxDepth = Search.MaxHorizon + Search.MaxQuietDepth;
    private readonly KillerMove[][] _killerMoves; // [depth][slot]


    public KillerMoves()
    {
        _killerMoves = new KillerMove[_maxDepth + 1][];

        for (var depth = 0; depth <= _maxDepth; depth++)
        {
            _killerMoves[depth] = new[]
            {
                new KillerMove(Piece.None, Square.Illegal),
                new KillerMove(Piece.None, Square.Illegal)
            };
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetValue(int depth, ulong move)
    {
        if (depth > _maxDepth) return 0;

        var killerMove = KillerMove.Parse(move);
        if (killerMove == _killerMoves[depth][0]) return 2;
        return killerMove == _killerMoves[depth][1] ? 1 : 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(int depth, ulong move)
    {
        if (depth > _maxDepth) return;

        var killerMove = KillerMove.Parse(move);
        if (killerMove == _killerMoves[depth][0]) return; // Move already is the best killer move.

        if (killerMove == _killerMoves[depth][1])
        {
            // Avoid storing same killer move in both slots.  Swap slots.
            (_killerMoves[depth][0], _killerMoves[depth][1]) = (_killerMoves[depth][1], _killerMoves[depth][0]);
            return;
        }

        // Shift and update killer move.
        _killerMoves[depth][1] = _killerMoves[depth][0];
        _killerMoves[depth][0] = killerMove;
    }


    public void Shift(int depth)
    {
        // Shift killer moves closer to root position.
        var lastDepth = _maxDepth - depth;

        for (var depthIndex = 0; depthIndex <= lastDepth; depthIndex++)
        {
            _killerMoves[depthIndex][0] = _killerMoves[depthIndex + depth][0];
            _killerMoves[depthIndex][1] = _killerMoves[depthIndex + depth][1];
        }

        // Reset killer moves far from root position.
        for (var depthIndex = lastDepth + 1; depthIndex <= _maxDepth; depthIndex++)
        {
            _killerMoves[depthIndex][0] = new KillerMove(Piece.None, Square.Illegal);
            _killerMoves[depthIndex][1] = new KillerMove(Piece.None, Square.Illegal);
        }
    }


    public void Reset()
    {
        for (var depth = 0; depth <= _maxDepth; depth++)
        {
            var killerMoves = _killerMoves[depth];
            killerMoves[0] = new KillerMove(Piece.None, Square.Illegal);
            killerMoves[1] = new KillerMove(Piece.None, Square.Illegal);
        }
    }
}