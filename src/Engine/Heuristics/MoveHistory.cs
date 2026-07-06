// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2026.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;
#pragma warning disable IDE0047


namespace ErikTheCoder.MadChess.Engine.Heuristics;


public sealed class MoveHistory
{
    private const int _multiplier = 256;
    private const int _divisor = Move.HistoryMaxValue / _multiplier;
    private const int _agePer1024 = 995; // Reduces history value by half in 24 iterations.
    private readonly int[][][] _moveHistory; // [piece][toSquare][victim]


    public MoveHistory()
    {
        _moveHistory = new int[(int)Piece.BlackKing + 1][][];
        for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            _moveHistory[(int)piece] = new int[64][];
            for (var toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
            {
                _moveHistory[(int)piece][(int)toSquare] = new int[(int)Piece.BlackKing + 1];
                for (var victim = Piece.None; victim <= Piece.BlackKing; victim++)
                    _moveHistory[(int)piece][(int)toSquare][(int)victim] = 0;
            }
        }
    }

    
    public int GetValue(ulong move)
    {
        var piece = Move.Piece(move);
        var toSquare = Move.To(move);
        var victim = Move.CaptureVictim(move);

        return _moveHistory[(int)piece][(int)toSquare][(int)victim];
    }


    public void UpdateValue(ulong move, int increment)
    {
        // Update value with decay.  Idea from Ethereal chess engine.
        // This function approaches an asymptotic limit of +/- Move.HistoryMaxValue.

        var piece = Move.Piece(move);
        var toSquare = Move.To(move);
        var victim = Move.CaptureVictim(move);
        var value = _moveHistory[(int)piece][(int)toSquare][(int)victim];
        value += (increment * _multiplier) - (value * FastMath.Abs(increment) / _divisor);

        _moveHistory[(int)piece][(int)toSquare][(int)victim] = FastMath.Clamp(value, -Move.HistoryMaxValue, Move.HistoryMaxValue);
    }

    
    public void Age()
    {
        for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            for (var toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
            {
                for (var victim = Piece.None; victim <= Piece.BlackKing; victim++)
                {
                    var value = _moveHistory[(int)piece][(int)toSquare][(int)victim];
                    _moveHistory[(int)piece][(int)toSquare][(int)victim] = (_agePer1024 * value) / 1024;
                }
            }
        }
    }


    public void Reset()
    {
        // Reset move history.
        for (var piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            for (var toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
            {
                for (var victim = Piece.None; victim <= Piece.BlackKing; victim++)
                    _moveHistory[(int)piece][(int)toSquare][(int)victim] = 0;
            }
        }
    }
}