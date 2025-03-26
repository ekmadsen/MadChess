// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Intelligence;
using ErikTheCoder.MadChess.Engine.Score;


namespace ErikTheCoder.MadChess.Engine.Hashtable;


public static class CachedPositionData
{
    private static readonly int _toHorizonShift;
    private static readonly ulong _toHorizonMask;
    private static readonly ulong _toHorizonUnmask;

    private static readonly int _bestMovePromotedPieceShift;
    private static readonly ulong _bestMovePromotedPieceMask;
    private static readonly ulong _bestMovePromotedPieceUnmask;

    private static readonly int _bestMoveToShift;
    private static readonly ulong _bestMoveToMask;
    private static readonly ulong _bestMoveToUnmask;

    private static readonly int _bestMoveFromShift;
    private static readonly ulong _bestMoveFromMask;
    private static readonly ulong _bestMoveFromUnmask;

    private static readonly int _dynamicScoreShift;
    private static readonly ulong _dynamicScoreMask;
    private static readonly ulong _dynamicScoreUnmask;

    private static readonly int _scorePrecisionShift;
    private static readonly ulong _scorePrecisionMask;
    private static readonly ulong _scorePrecisionUnmask;

    private static readonly ulong _lastAccessedMask;
    private static readonly ulong _lastAccessedUnmask;

    // 6 6 6 6 5 5 5 5 5 5 5 5 5 5 4 4 4 4 4 4 4 4 4 4 3 3 3 3 3 3 3 3 3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0
    // 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
    // To Horizon   |BMPP   |Best To      |Best From    |Dynamic Score                                        |DSP|Last Accessed

    // BMPP =       Best Move Promoted Piece
    // Best To =    Best Move To (one extra bit for illegal square)
    // Best From =  Best Move From (one extra bit for illegal square)
    // DSP =        Dynamic Score Precision


    static CachedPositionData()
    {
        // Create bit shifts and masks.
        _toHorizonShift = 57;
        _toHorizonMask = Bitwise.CreateULongMask(57, 63);
        _toHorizonUnmask = Bitwise.CreateULongUnmask(57, 63);

        _bestMovePromotedPieceShift = 53;
        _bestMovePromotedPieceMask = Bitwise.CreateULongMask(53, 56);
        _bestMovePromotedPieceUnmask = Bitwise.CreateULongUnmask(53, 56);

        _bestMoveToShift = 46;
        _bestMoveToMask = Bitwise.CreateULongMask(46, 52);
        _bestMoveToUnmask = Bitwise.CreateULongUnmask(46, 52);

        _bestMoveFromShift = 39;
        _bestMoveFromMask = Bitwise.CreateULongMask(39, 45);
        _bestMoveFromUnmask = Bitwise.CreateULongUnmask(39, 45);

        _dynamicScoreShift = 12;
        _dynamicScoreMask = Bitwise.CreateULongMask(12, 38);
        _dynamicScoreUnmask = Bitwise.CreateULongUnmask(12, 38);

        _scorePrecisionShift = 10;
        _scorePrecisionMask = Bitwise.CreateULongMask(10, 11);
        _scorePrecisionUnmask = Bitwise.CreateULongUnmask(10, 11);

        _lastAccessedMask = Bitwise.CreateULongMask(0, 9);
        _lastAccessedUnmask = Bitwise.CreateULongUnmask(0, 9);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToHorizon(ulong cachedPositionData) => (int)((cachedPositionData & _toHorizonMask) >> _toHorizonShift);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetToHorizon(ref ulong cachedPositionData, int toHorizon)
    {
        // Clear
        cachedPositionData &= _toHorizonUnmask;
        // Set
        cachedPositionData |= ((ulong)toHorizon << _toHorizonShift) & _toHorizonMask;
        // Validate cached position.
        Debug.Assert(ToHorizon(cachedPositionData) == toHorizon);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square BestMoveFrom(ulong cachedPositionData) => (Square)((cachedPositionData & _bestMoveFromMask) >> _bestMoveFromShift);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBestMoveFrom(ref ulong cachedPositionData, Square bestMoveFrom)
    {
        // Clear
        cachedPositionData &= _bestMoveFromUnmask;
        // Set
        cachedPositionData |= ((ulong)bestMoveFrom << _bestMoveFromShift) & _bestMoveFromMask;
        // Validate cached position.
        Debug.Assert(BestMoveFrom(cachedPositionData) == bestMoveFrom);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square BestMoveTo(ulong cachedPositionData) => (Square)((cachedPositionData & _bestMoveToMask) >> _bestMoveToShift);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBestMoveTo(ref ulong cachedPositionData, Square bestMoveTo)
    {
        // Clear
        cachedPositionData &= _bestMoveToUnmask;
        // Set
        cachedPositionData |= ((ulong)bestMoveTo << _bestMoveToShift) & _bestMoveToMask;
        // Validate cached position.
        Debug.Assert(BestMoveTo(cachedPositionData) == bestMoveTo);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece BestMovePromotedPiece(ulong cachedPositionData) => (Piece)((cachedPositionData & _bestMovePromotedPieceMask) >> _bestMovePromotedPieceShift);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBestMovePromotedPiece(ref ulong cachedPositionData, Piece bestMovePromotedPiece)
    {
        // Clear
        cachedPositionData &= _bestMovePromotedPieceUnmask;
        // Set
        cachedPositionData |= ((ulong)bestMovePromotedPiece << _bestMovePromotedPieceShift) & _bestMovePromotedPieceMask;
        // Validate cached position.
        Debug.Assert(BestMovePromotedPiece(cachedPositionData) == bestMovePromotedPiece);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DynamicScore(ulong cachedPositionData) => (int)((cachedPositionData & _dynamicScoreMask) >> _dynamicScoreShift) - StaticScore.Max; // Cached score is a positive number.


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetDynamicScore(ref ulong cachedPositionData, int dynamicScore)
    {
        // Ensure cached score is a positive number.
        var positiveScore = dynamicScore + StaticScore.Max;
        // Clear
        cachedPositionData &= _dynamicScoreUnmask;
        // Set
        cachedPositionData |= ((ulong)positiveScore << _dynamicScoreShift) & _dynamicScoreMask;
        // Validate cached position.
        Debug.Assert(DynamicScore(cachedPositionData) == dynamicScore);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ScorePrecision ScorePrecision(ulong cachedPositionData) => (ScorePrecision)((cachedPositionData & _scorePrecisionMask) >> _scorePrecisionShift);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetScorePrecision(ref ulong cachedPositionData, ScorePrecision scorePrecision)
    {
        var value = (ulong)scorePrecision;
        // Clear
        cachedPositionData &= _scorePrecisionUnmask;
        // Set
        cachedPositionData |= (value << _scorePrecisionShift) & _scorePrecisionMask;
        // Validate cached position.
        Debug.Assert(ScorePrecision(cachedPositionData) == scorePrecision);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastAccessed(ulong cachedPositionData) => (int)(cachedPositionData & _lastAccessedMask);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetLastAccessed(ref ulong cachedPositionData, int lastAccessed)
    {
        // Clear
        cachedPositionData &= _lastAccessedUnmask;
        // Set
        cachedPositionData |= (ulong)lastAccessed & _lastAccessedMask;
        // Validate cached position.
        Debug.Assert(LastAccessed(cachedPositionData) == lastAccessed);
    }


    public static bool IsValid(ulong cachedPositionData)
    {
        Debug.Assert(ToHorizon(cachedPositionData) <= Search.MaxHorizon, $"ToHorizon(CachedPosition) = {ToHorizon(cachedPositionData)}, Search.MaxHorizon = {Search.MaxHorizon}{Environment.NewLine}{ToString(cachedPositionData)}");

        Debug.Assert(BestMoveFrom(cachedPositionData) <= Square.Illegal, $"BestMoveFrom(CachedPosition) = {BestMoveFrom(cachedPositionData)}, Square.Illegal = {Square.Illegal}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMoveTo(cachedPositionData) < Square.Illegal, $"BestMoveTo(CachedPosition) = {BestMoveTo(cachedPositionData)}, Square.Illegal = {Square.Illegal}{Environment.NewLine}{ToString(cachedPositionData)}");

        Debug.Assert(BestMovePromotedPiece(cachedPositionData) >= Piece.None, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(cachedPositionData)}, Piece.None = {Piece.None}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMovePromotedPiece(cachedPositionData) != Piece.WhitePawn, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(cachedPositionData)}, Piece.WhitePawn = {Piece.WhitePawn}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMovePromotedPiece(cachedPositionData) != Piece.WhiteKing, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(cachedPositionData)}, Piece.WhiteKing = {Piece.WhiteKing}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMovePromotedPiece(cachedPositionData) != Piece.BlackPawn, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(cachedPositionData)}, Piece.BlackPawn = {Piece.BlackPawn}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMovePromotedPiece(cachedPositionData) < Piece.BlackKing, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(cachedPositionData)}, Piece.BlackKing = {Piece.BlackKing}{Environment.NewLine}{ToString(cachedPositionData)}");

        Debug.Assert(DynamicScore(cachedPositionData) >= -StaticScore.Max, $"DynamicScore(CachedPosition) = {DynamicScore(cachedPositionData)}, -SpecialScore.Max = {-StaticScore.Max}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(DynamicScore(cachedPositionData) <= StaticScore.Max, $"DynamicScore(CachedPosition) = {DynamicScore(cachedPositionData)}, SpecialScore.Max = {StaticScore.Max}{Environment.NewLine}{ToString(cachedPositionData)}");

        return true;
    }


    private static string ToString(ulong cachedPositionData)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"To Horizon = {ToHorizon(cachedPositionData)}");
        stringBuilder.AppendLine($"Best Move From = {BestMoveFrom(cachedPositionData)}");
        stringBuilder.AppendLine($"Best Move To = {BestMoveTo(cachedPositionData)}");
        stringBuilder.AppendLine($"Best Move Promoted Piece = {BestMovePromotedPiece(cachedPositionData)}");
        stringBuilder.AppendLine($"Dynamic Score = {DynamicScore(cachedPositionData)}");
        stringBuilder.AppendLine($"Score Precision = {ScorePrecision(cachedPositionData)}");
        stringBuilder.AppendLine($"Last Accessed = {LastAccessed(cachedPositionData)}");

        return stringBuilder.ToString();
    }
}