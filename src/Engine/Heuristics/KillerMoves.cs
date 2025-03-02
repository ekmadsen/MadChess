// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Runtime.CompilerServices;
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
            _killerMoves[depth] = [KillerMove.Null, KillerMove.Null];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (KillerMove KillerMove1, KillerMove KillerMove2) Get(int depth) => (_killerMoves[depth][0], _killerMoves[depth][1]);


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

        // Shift killer move slots and update the best killer move.
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
            _killerMoves[depthIndex][0] = KillerMove.Null;
            _killerMoves[depthIndex][1] = KillerMove.Null;
        }
    }


    public void Reset()
    {
        for (var depth = 0; depth <= _maxDepth; depth++)
        {
            _killerMoves[depth][0] = KillerMove.Null;
            _killerMoves[depth][1] = KillerMove.Null;
        }
    }
}