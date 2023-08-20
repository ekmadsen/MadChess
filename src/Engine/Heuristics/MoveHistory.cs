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
    private const int _increments = 524_288;
    private const int _agePer128 = 116;
    private const int _scalePer128 = 32;
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

        // Scale increment so it's larger when current move history value is near zero.
        var value = _moveHistory[(int)piece][(int)toSquare];
        var scaledIncrement = ((Move.HistoryMaxValue - FastMath.Abs(value)) * increment) / _increments;
        value = FastMath.Clamp(value + scaledIncrement, -Move.HistoryMaxValue, Move.HistoryMaxValue);

        _moveHistory[(int)piece][(int)toSquare] = value;
        if (FastMath.Abs(value) == Move.HistoryMaxValue) Scale(_scalePer128);
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