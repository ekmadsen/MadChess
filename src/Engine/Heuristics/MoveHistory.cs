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
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Engine.Heuristics;


public sealed class MoveHistory
{
    private const int _multiplier = 256;
    private const int _divisor = Move.HistoryMaxValue / _multiplier;
    private const int _agePer128 = 118;
    private readonly int[][] _moveHistory; // [piece][toSquare]


    public MoveHistory()
    {
        _moveHistory = new int[(int)Piece.BlackKing + 1][];

        for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            _moveHistory[(int)piece] = new int[64];

            for (var toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                _moveHistory[(int)piece][(int)toSquare] = 0;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetValue(ulong move)
    {
        var piece = Move.Piece(move);
        var toSquare = Move.To(move);
        return _moveHistory[(int)piece][(int)toSquare];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateValue(ulong move, int increment)
    {
        // Update value with decay.  Idea from Ethereal chess engine.
        // This function approaches an asymptotic limit of +/- Move.HistoryMaxValue.
        var piece = Move.Piece(move);
        var toSquare = Move.To(move);

        var value = _moveHistory[(int)piece][(int)toSquare];
        value += (increment * _multiplier) - (value * FastMath.Abs(increment) / _divisor);

        _moveHistory[(int)piece][(int)toSquare] = FastMath.Clamp(value, -Move.HistoryMaxValue, Move.HistoryMaxValue);
    }

    
    public void Age()
    {
        for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            for (var toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                _moveHistory[(int)piece][(int)toSquare] = (_agePer128 * _moveHistory[(int)piece][(int)toSquare]) / 128;
        }
    }


    public void Reset()
    {
        for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            for (var toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                _moveHistory[(int)piece][(int)toSquare] = 0;
        }
    }
}
