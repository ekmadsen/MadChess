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
    private const int _multiplier = 4;
    private const int _divisor = Move.HistoryMaxValue / _multiplier;
    private const int _moveHistoryWeight = 1; // _moveHistoryWeight + _counterMoveHistoryWeight = 128.  Divide by 128 == shift bits right 7 places.
    private const int _counterMoveHistoryWeight = 127; // Counter move history is more specific than move history, and consequently, is updated less often.  Therefore, weight it more heavily than move history.
    private const int _agePer1024 = 1004; // Reduces history value by half in 36 iterations.
    private readonly int[][][] _moveHistory; // [piece][toSquare][victim]
    private readonly int[][][][] _counterMoveHistory; // [previousPiece][previousToSquare][piece][toSquare]
    private readonly int[] _victimBonusPer128; // [piece]


    public MoveHistory()
    {
        Piece piece;
        Square toSquare;

        // Create move history array.
        _moveHistory = new int[(int)Piece.BlackKing + 1][][];
        for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            _moveHistory[(int)piece] = new int[64][];
            for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
            {
                _moveHistory[(int)piece][(int)toSquare] = new int[(int)Piece.BlackKing + 1];
                for (var victim = Piece.None; victim <= Piece.BlackKing; victim++)
                    _moveHistory[(int)piece][(int)toSquare][(int)victim] = 0;
            }
        }

        // Create counter move history array.
        _counterMoveHistory = new int[(int)Piece.BlackKing + 1][][][];
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            _counterMoveHistory[(int)previousPiece] = new int[(int)Square.Illegal][][];
            for (var previousToSquare = Square.A8; previousToSquare < Square.Illegal; previousToSquare++)
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

        // Create victim bonus array.
        _victimBonusPer128 = new int[(int)ColorlessPiece.King + 1];
        _victimBonusPer128[(int)ColorlessPiece.None] = 0;
        _victimBonusPer128[(int)ColorlessPiece.Pawn] = 0;
        _victimBonusPer128[(int)ColorlessPiece.Knight] = 27;
        _victimBonusPer128[(int)ColorlessPiece.Bishop] = 27;
        _victimBonusPer128[(int)ColorlessPiece.Rook] = 45;
        _victimBonusPer128[(int)ColorlessPiece.Queen] = 81;
        _victimBonusPer128[(int)ColorlessPiece.King] = 0;
    }


    public int GetValue(ulong previousMove, ulong move, bool includeVictimBonus = true)
    {
        // Get move history (for quiet and tactical moves).
        var piece = Move.Piece(move);
        var toSquare = Move.To(move);
        var victim = Move.CaptureVictim(move);
        var moveHistory = _moveHistory[(int)piece][(int)toSquare][(int)victim];

        // Get bonus based on victim material value to improve ordering of capture moves.
        var victimBonus = includeVictimBonus
            ? (_victimBonusPer128[(int)PieceHelper.GetColorlessPiece(victim)] * Move.HistoryMaxValue) / 128
            : 0;

        if ((previousMove == Move.Null) || !Move.IsQuiet(move)) return FastMath.Clamp(moveHistory + victimBonus, -Move.HistoryMaxValue, Move.HistoryMaxValue);

        // Get counter move history (for quiet moves).
        var previousPiece = Move.Piece(previousMove);
        var previousToSquare = Move.To(previousMove);
        var counterMoveHistory = _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];

        var value = ((moveHistory * _moveHistoryWeight) + (counterMoveHistory * _counterMoveHistoryWeight)) / (_moveHistoryWeight + _counterMoveHistoryWeight);
        return FastMath.Clamp(value + victimBonus, -Move.HistoryMaxValue, Move.HistoryMaxValue);
    }


    public void UpdateValue(ulong previousMove, ulong move, int increment)
    {
        // Update value with decay.  Idea from Ethereal chess engine.
        // This function approaches an asymptotic limit of +/- Move.HistoryMaxValue.

        // Update move history (for quiet and tactical moves).
        var absIncrement = FastMath.Abs(increment);
        var piece = Move.Piece(move);
        var toSquare = Move.To(move);
        var victim = Move.CaptureVictim(move);
        var value = _moveHistory[(int)piece][(int)toSquare][(int)victim];
        value += (increment * _multiplier) - (value * absIncrement / _divisor);
        _moveHistory[(int)piece][(int)toSquare][(int)victim] = FastMath.Clamp(value, -Move.HistoryMaxValue, Move.HistoryMaxValue);

        if ((previousMove == Move.Null) || !Move.IsQuiet(move)) return;

        // Update counter move history (for quiet moves).
        var previousPiece = Move.Piece(previousMove);
        var previousToSquare = Move.To(previousMove);
        value = _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];
        value += (increment * _multiplier) - (value * absIncrement / _divisor);
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
                for (var victim = Piece.None; victim <= Piece.BlackKing; victim++)
                {
                    var value = _moveHistory[(int)piece][(int)toSquare][(int)victim];
                    _moveHistory[(int)piece][(int)toSquare][(int)victim] = (_agePer1024 * value) / 1024;
                }
            }
        }

        // Age counter move history.
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            for (var previousToSquare = Square.A8; previousToSquare < Square.Illegal; previousToSquare++)
            {
                for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
                {
                    for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                    {
                        var value = _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];
                        _counterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = (_agePer1024 * value) / 1024;
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
            {
                for (var victim = Piece.None; victim <= Piece.BlackKing; victim++)
                    _moveHistory[(int)piece][(int)toSquare][(int)victim] = 0;
            }
        }

        // Reset counter move history.
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            for (var previousToSquare = Square.A8; previousToSquare < Square.Illegal; previousToSquare++)
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