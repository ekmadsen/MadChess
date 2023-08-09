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
    private const int _agePer128 = 125;
    private const int _scalePer128 = 64;
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
        var piece = Move.Piece(move);
        var toSquare = Move.To(move);

        var value = _moveHistory[(int)piece][(int)toSquare] + increment;
        if (FastMath.Abs(value) > Move.HistoryMaxValue)
        {
            Scale(_scalePer128);
            value = _moveHistory[(int)piece][(int)toSquare] + increment;
            _moveHistory[(int)piece][(int)toSquare] = FastMath.Clamp(value, -Move.HistoryMaxValue, Move.HistoryMaxValue);
        }
        else _moveHistory[(int)piece][(int)toSquare] = value;
    }


    public void Age() => Scale(_agePer128);


    private void Scale(int scalePer128)
    {
        for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            for (var toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                _moveHistory[(int)piece][(int)toSquare] = (scalePer128 * _moveHistory[(int)piece][(int)toSquare]) / 128;
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