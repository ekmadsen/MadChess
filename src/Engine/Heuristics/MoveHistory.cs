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
    private const int _moveHistoryWeight = 1; // _moveHistoryWeight + _counterMoveHistoryWeight = a multiple of 2.  Divide by a multiple of 2 == shift bits right a few places.
    private const int _quietCounterMoveHistoryWeight = 127;  // Counter move history is more specific than move history, and consequently, is updated less often.
    private const int _captureCounterMoveHistoryWeight = 31; // Therefore, weight it more heavily than move history.
    private const int _agePer1024 = 1004; // Reduces history value by half in 36 iterations.
    private readonly int[][][] _moveHistory; // [piece][toSquare][victim]
    private readonly int[][][][] _quietCounterMoveHistory; // [previousPiece][previousToSquare][piece][toSquare]
    private readonly int[][][][] _captureCounterMoveHistory; // [previousPiece][previousToSquare][piece][toSquare]
    private readonly int[] _victimBonusPer128; // [colorlessPiece]


    public MoveHistory()
    {
        Piece piece;
        Square toSquare;

        // Create history array.
        _moveHistory = new int[(int)Piece.BlackKing + 1][][];
        for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
        {
            _moveHistory[(int)piece] = new int[(int)Square.Illegal][];
            for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
            {
                _moveHistory[(int)piece][(int)toSquare] = new int[(int)Piece.BlackKing + 1];
                for (var victim = Piece.None; victim <= Piece.BlackKing; victim++)
                    _moveHistory[(int)piece][(int)toSquare][(int)victim] = 0;
            }
        }

        // Create quiet counter move history array.
        _quietCounterMoveHistory = new int[(int)Piece.BlackKing + 1][][][];
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            _quietCounterMoveHistory[(int)previousPiece] = new int[(int)Square.Illegal][][];
            for (var previousToSquare = Square.A8; previousToSquare < Square.Illegal; previousToSquare++)
            {
                _quietCounterMoveHistory[(int)previousPiece][(int)previousToSquare] = new int[(int)Piece.BlackKing + 1][];
                for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
                {
                    _quietCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece] = new int[(int)Square.Illegal];
                    for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                        _quietCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = 0;
                }
            }
        }

        // Create capture counter move history array.
        _captureCounterMoveHistory = new int[(int)Square.Illegal][][][];
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            _captureCounterMoveHistory[(int)previousPiece] = new int[(int)Square.Illegal][][];
            for (var previousToSquare = Square.A8; previousToSquare < Square.Illegal; previousToSquare++)
            {
                _captureCounterMoveHistory[(int)previousPiece][(int)previousToSquare] = new int[(int)Piece.BlackKing + 1][];
                for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
                {
                    _captureCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece] = new int[(int)Square.Illegal];
                    for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                        _captureCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = 0;
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
        // Get move history.
        var piece = Move.Piece(move);
        var toSquare = Move.To(move);
        var victim = Move.CaptureVictim(move);
        var moveHistory = _moveHistory[(int)piece][(int)toSquare][(int)victim];

        // Get bonus based on victim material value to improve ordering of capture moves.
        var victimBonus = includeVictimBonus
            ? (_victimBonusPer128[(int)PieceHelper.GetColorlessPiece(victim)] * Move.HistoryMaxValue) / 128
            : 0;

        if (previousMove == Move.Null) return FastMath.Clamp(moveHistory + victimBonus, -Move.HistoryMaxValue, Move.HistoryMaxValue);

        var previousPiece = Move.Piece(previousMove);
        var previousToSquare = Move.To(previousMove);

        // Get counter move history.
        int counterMoveHistory;
        int counterMoveWeight;

        if (Move.IsQuiet(move))
        {
            counterMoveHistory = _quietCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];
            counterMoveWeight = _quietCounterMoveHistoryWeight;
        }
        else if (Move.IsCapture(move))
        {
            counterMoveHistory = _captureCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];
            counterMoveWeight = _captureCounterMoveHistoryWeight;
        }
        else
        {
            counterMoveHistory = moveHistory;
            counterMoveWeight = 1;
        }

        var value = ((moveHistory * _moveHistoryWeight) + (counterMoveHistory * counterMoveWeight)) / (_moveHistoryWeight + counterMoveWeight);
        return FastMath.Clamp(value + victimBonus, -Move.HistoryMaxValue, Move.HistoryMaxValue);
    }


    public void UpdateValue(ulong previousMove, ulong move, int increment)
    {
        // Update value with decay.  Idea from Ethereal chess engine.
        // This function approaches an asymptotic limit of +/- Move.HistoryMaxValue.

        // Update move history.
        var absIncrement = FastMath.Abs(increment);
        var piece = Move.Piece(move);
        var toSquare = Move.To(move);
        var victim = Move.CaptureVictim(move);
        var value = _moveHistory[(int)piece][(int)toSquare][(int)victim];
        value += (increment * _multiplier) - (value * absIncrement / _divisor);
        _moveHistory[(int)piece][(int)toSquare][(int)victim] = FastMath.Clamp(value, -Move.HistoryMaxValue, Move.HistoryMaxValue);

        if (previousMove == Move.Null) return;

        // Update counter move history.
        var previousPiece = Move.Piece(previousMove);
        var previousToSquare = Move.To(previousMove);

        if (Move.IsQuiet(move))
        {
            value = _quietCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];
            value += (increment * _multiplier) - (value * absIncrement / _divisor);
            _quietCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = FastMath.Clamp(value, -Move.HistoryMaxValue, Move.HistoryMaxValue);
        }
        else if (Move.IsCapture(move))
        {
            value = _captureCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];
            value += (increment * _multiplier) - (value * absIncrement / _divisor);
            _captureCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = FastMath.Clamp(value, -Move.HistoryMaxValue, Move.HistoryMaxValue);
        }
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

        // Age quiet counter move history.
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            for (var previousToSquare = Square.A8; previousToSquare < Square.Illegal; previousToSquare++)
            {
                for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
                {
                    for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                    {
                        var value = _quietCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];
                        _quietCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = (_agePer1024 * value) / 1024;
                    }
                }
            }
        }

        // Age capture counter move history.
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            for (var previousToSquare = Square.A8; previousToSquare < Square.Illegal; previousToSquare++)
            {
                for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
                {
                    for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                    {
                        var value = _captureCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare];
                        _captureCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = (_agePer1024 * value) / 1024;
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

        // Reset quiet counter move history.
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            for (var previousToSquare = Square.A8; previousToSquare < Square.Illegal; previousToSquare++)
            {
                for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
                {
                    for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                        _quietCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = 0;
                }
            }
        }

        // Reset capture counter move history.
        for (var previousPiece = Piece.None; previousPiece <= Piece.BlackKing; previousPiece++)
        {
            for (var previousToSquare = Square.A8; previousToSquare < Square.Illegal; previousToSquare++)
            {
                for (piece = Piece.None; piece <= Piece.BlackKing; piece++)
                {
                    for (toSquare = Square.A8; toSquare < Square.Illegal; toSquare++)
                        _captureCounterMoveHistory[(int)previousPiece][(int)previousToSquare][(int)piece][(int)toSquare] = 0;
                }
            }
        }   
    }
}