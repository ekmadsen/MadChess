// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Engine.Heuristics;


public sealed class MoveHistory
{
    private const int _multiplier = 256;
    private const int _divisor = Move.HistoryMaxValue / _multiplier;
    private const int _moveHistoryWeight = 1; // _moveHistoryWeight + _counterMoveHistoryWeight = 32.  Divide by 64 == shift bits right 5 places.
    private const int _counterMoveHistoryWeight = 31; // Counter move history is more specific than move history, and consequently, is updated less often.  Therefore, weight it more heavily than move history.
    private const int _agePer256 = 245; // Reduces to half (49.52%) of root value after 16 ply.
    private readonly int[][] _moveHistory; // [piece][toSquare]
    private readonly int[][][][] _counterMoveHistory; // [previousPiece][previousToSquare][piece][toSquare]


    public MoveHistory()
    {
        Piece piece;
        Square toSquare;

        // Create move history array.
        _moveHistory = new int[(int)Piece.BlackKing + 1][];
        for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            _moveHistory[(int)piece] = new int[64];
            for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                _moveHistory[(int)piece][(int)toSquare] = 0;
        }

        // Create counter move history array.
        _counterMoveHistory = new int[(int)Piece.BlackKing + 1][][][];
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            _counterMoveHistory[(int)previousPiece] = new int[(int)Square.Illegal + 1][][];
            for (var previousToSquare = Square.A8; previousToSquare <= Square.Illegal; previousToSquare++)
            {
                _counterMoveHistory[(int)previousPiece][(int)previousToSquare] = new int[(int)Piece.BlackKing + 1][];
                for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
                {
                    _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece] = new int[(int)Square.Illegal];
                    for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                        _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = 0;
                }
            }
        }
    }

    
    public int GetValue(ulong previousMove, ulong move)
    {
        // Get move history.
        var piece = Move.Piece(move);
        var toSquare = Move.To(move);
        var moveHistory = _moveHistory[(int)piece][(int)toSquare];

        if (previousMove == Move.Null) return moveHistory;

        // Get counter move history.
        var previousPiece = Move.Piece(previousMove);
        var previousToSquare = Move.To(previousMove);
        var counterMoveHistory = _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];

        return ((moveHistory * _moveHistoryWeight) + (counterMoveHistory * _counterMoveHistoryWeight)) / (_moveHistoryWeight + _counterMoveHistoryWeight);
    }


    public void UpdateValue(ulong previousMove, ulong move, int increment)
    {
        // Update value with decay.  Idea from Ethereal chess engine.
        // This function approaches an asymptotic limit of +/- Move.HistoryMaxValue.

        // Update move history.
        var piece = Move.Piece(move);
        var toSquare = Move.To(move);
        var value = _moveHistory[(int)piece][(int)toSquare];
        value += (increment * _multiplier) - (value * FastMath.Abs(increment) / _divisor);
        _moveHistory[(int)piece][(int)toSquare] = FastMath.Clamp(value, -Move.HistoryMaxValue, Move.HistoryMaxValue);

        if (previousMove == Move.Null) return;

        // Update counter move history.
        var previousPiece = Move.Piece(previousMove);
        var previousToSquare = Move.To(previousMove);
        value = _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];
        value += (increment * _multiplier) - (value * FastMath.Abs(increment) / _divisor);
        _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = FastMath.Clamp(value, -Move.HistoryMaxValue, Move.HistoryMaxValue);
    }

    
    public void Age()
    {

        Piece piece;
        Square toSquare;

        // Age move history.
        for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
            {
                var value = _moveHistory[(int)piece][(int)toSquare];
                _moveHistory[(int)piece][(int)toSquare] = (_agePer256 * value) / 256;
            }
        }

        // Age counter move history.
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            for (var previousToSquare = Square.A8; previousToSquare <= Square.Illegal; previousToSquare++)
            {
                for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
                {
                    for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                    {
                        var value = _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];
                        _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = (_agePer256 * value) / 256;
                    }
                }
            }
        }
    }


    public void Reset()
    {
        Piece piece;
        Square toSquare;

        // Reset move history.
        for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                _moveHistory[(int)piece][(int)toSquare] = 0;
        }

        // Reset counter move history.
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            for (var previousToSquare = Square.A8; previousToSquare <= Square.Illegal; previousToSquare++)
            {
                for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
                {
                    for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                        _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = 0;
                }
            }
        }
    }
}
