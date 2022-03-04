// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
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
    private static readonly int _bestMoveFromShift;
    private static readonly ulong _bestMoveFromMask;
    private static readonly ulong _bestMoveFromUnmask;
    private static readonly int _bestMoveToShift;
    private static readonly ulong _bestMoveToMask;
    private static readonly ulong _bestMoveToUnmask;
    private static readonly int _bestMovePromotedPieceShift;
    private static readonly ulong _bestMovePromotedPieceMask;
    private static readonly ulong _bestMovePromotedPieceUnmask;
    private static readonly int _staticScoreShift;
    private static readonly ulong _staticScoreMask;
    private static readonly ulong _staticScoreUnmask;
    private static readonly int _drawnEndgameShift;
    private static readonly ulong _drawnEndgameMask;
    private static readonly ulong _drawnEndgameUnmask;
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
    // To Horizon |Best From    |Best To      |BMP    |Static Score                   |D|Dynamic Score                  |SP |Last

    // Best From = Best Move From (one extra bit for illegal square)
    // Best To =   Best Move To   (one extra bit for illegal square)
    // BMP =       Best Move Promoted Piece
    // D =         Static Score Drawn Endgame
    // SP =        Dynamic Score Precision
    // Last =      Last Accessed


    static CachedPositionData()
    {
        // Create bit shifts and masks.
        _toHorizonShift = 58;
        _toHorizonMask = Bitwise.CreateULongMask(58, 63);
        _toHorizonUnmask = Bitwise.CreateULongUnmask(58, 63);
        _bestMoveFromShift = 51;
        _bestMoveFromMask = Bitwise.CreateULongMask(51, 57);
        _bestMoveFromUnmask = Bitwise.CreateULongUnmask(51, 57);
        _bestMoveToShift = 44;
        _bestMoveToMask = Bitwise.CreateULongMask(44, 50);
        _bestMoveToUnmask = Bitwise.CreateULongUnmask(44, 50);
        _bestMovePromotedPieceShift = 40;
        _bestMovePromotedPieceMask = Bitwise.CreateULongMask(40, 43);
        _bestMovePromotedPieceUnmask = Bitwise.CreateULongUnmask(40, 43);
        _staticScoreShift = 24;
        _staticScoreMask = Bitwise.CreateULongMask(24, 39);
        _staticScoreUnmask = Bitwise.CreateULongUnmask(24, 39);
        _drawnEndgameShift = 23;
        _drawnEndgameMask = Bitwise.CreateULongMask(23);
        _drawnEndgameUnmask = Bitwise.CreateULongUnmask(23);
        _dynamicScoreShift = 7;
        _dynamicScoreMask = Bitwise.CreateULongMask(7, 22);
        _dynamicScoreUnmask = Bitwise.CreateULongUnmask(7, 22);
        _scorePrecisionShift = 5;
        _scorePrecisionMask = Bitwise.CreateULongMask(5, 6);
        _scorePrecisionUnmask = Bitwise.CreateULongUnmask(5, 6);
        _lastAccessedMask = Bitwise.CreateULongMask(0, 4);
        _lastAccessedUnmask = Bitwise.CreateULongUnmask(0, 4);
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
    public static int StaticScore(ulong cachedPositionData) => (int)((cachedPositionData & _staticScoreMask) >> _staticScoreShift) - SpecialScore.Max; // Cached score is a positive number.


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetStaticScore(ref ulong cachedPositionData, int staticScore)
    {
        // Ensure cached score is a positive number.
        var positiveScore = staticScore + SpecialScore.Max;
        // Clear
        cachedPositionData &= _staticScoreUnmask;
        // Set
        cachedPositionData |= ((ulong)positiveScore << _staticScoreShift) & _staticScoreMask;
        // Validate cached position.
        Debug.Assert(StaticScore(cachedPositionData) == staticScore);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsStaticDrawnEndgame(ulong cachedPositionData) => (cachedPositionData & _drawnEndgameMask) >> _drawnEndgameShift > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetIsStaticDrawnEndgame(ref ulong cachedPositionData, bool isStaticDrawnEndgame)
    {
        var value = isStaticDrawnEndgame ? 1ul : 0;
        // Clear
        cachedPositionData &= _drawnEndgameUnmask;
        // Set
        cachedPositionData |= (value << _drawnEndgameShift) & _drawnEndgameMask;
        // Validate move.
        Debug.Assert(IsStaticDrawnEndgame(cachedPositionData) == isStaticDrawnEndgame);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DynamicScore(ulong cachedPositionData) => (int)((cachedPositionData & _dynamicScoreMask) >> _dynamicScoreShift) - SpecialScore.Max; // Cached score is a positive number.


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetDynamicScore(ref ulong cachedPositionData, int dynamicScore)
    {
        // Ensure cached score is a positive number.
        var positiveScore = dynamicScore + SpecialScore.Max;
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
    public static byte LastAccessed(ulong cachedPositionData) => (byte)(cachedPositionData & _lastAccessedMask);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetLastAccessed(ref ulong cachedPositionData, byte lastAccessed)
    {
        // Clear
        cachedPositionData &= _lastAccessedUnmask;
        // Set
        cachedPositionData |= lastAccessed & _lastAccessedMask;
        // Validate cached position.
        Debug.Assert(LastAccessed(cachedPositionData) == lastAccessed);
    }


    public static bool IsValid(ulong cachedPositionData)
    {
        Debug.Assert(ToHorizon(cachedPositionData) <= Search.MaxHorizon, $"ToHorizon(CachedPosition) = {ToHorizon(cachedPositionData)}, Search.MaxHorizon = {Search.MaxHorizon}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMoveFrom(cachedPositionData) <= Square.Illegal, $"BestMoveFrom(CachedPosition) = {BestMoveFrom(cachedPositionData)}, Square.Illegal = {Square.Illegal}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMoveTo(cachedPositionData) <= Square.Illegal, $"BestMoveTo(CachedPosition) = {BestMoveTo(cachedPositionData)}, Square.Illegal = {Square.Illegal}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMovePromotedPiece(cachedPositionData) >= Piece.None, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(cachedPositionData)}, Piece.None = {Piece.None}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMovePromotedPiece(cachedPositionData) != Piece.WhitePawn, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(cachedPositionData)}, Piece.WhitePawn = {Piece.WhitePawn}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMovePromotedPiece(cachedPositionData) != Piece.WhiteKing, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(cachedPositionData)}, Piece.WhiteKing = {Piece.WhiteKing}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMovePromotedPiece(cachedPositionData) != Piece.BlackPawn, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(cachedPositionData)}, Piece.BlackPawn = {Piece.BlackPawn}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(BestMovePromotedPiece(cachedPositionData) < Piece.BlackKing, $"BestMovePromotedPiece(CachedPosition) = {BestMovePromotedPiece(cachedPositionData)}, Piece.BlackKing = {Piece.BlackKing}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(StaticScore(cachedPositionData) >= -SpecialScore.Max, $"StaticScore(CachedPosition) = {StaticScore(cachedPositionData)}, -SpecialScore.Max = {-SpecialScore.Max}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(StaticScore(cachedPositionData) <= SpecialScore.Max, $"StaticScore(CachedPosition) = {StaticScore(cachedPositionData)}, SpecialScore.Max = {SpecialScore.Max}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(DynamicScore(cachedPositionData) >= -SpecialScore.Max, $"DynamicScore(CachedPosition) = {DynamicScore(cachedPositionData)}, -SpecialScore.Max = {-SpecialScore.Max}{Environment.NewLine}{ToString(cachedPositionData)}");
        Debug.Assert(DynamicScore(cachedPositionData) <= SpecialScore.Max, $"DynamicScore(CachedPosition) = {DynamicScore(cachedPositionData)}, SpecialScore.Max = {SpecialScore.Max}{Environment.NewLine}{ToString(cachedPositionData)}");
        return true;
    }


    private static string ToString(ulong cachedPositionData)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"To Horizon = {ToHorizon(cachedPositionData)}");
        stringBuilder.AppendLine($"Best Move From = {BestMoveFrom(cachedPositionData)}");
        stringBuilder.AppendLine($"Best Move To = {BestMoveTo(cachedPositionData)}");
        stringBuilder.AppendLine($"Best Move Promoted Piece = {BestMovePromotedPiece(cachedPositionData)}");
        stringBuilder.AppendLine($"Static Score = {StaticScore(cachedPositionData)}");
        stringBuilder.AppendLine($"IsStaticDrawnEndgame = {IsStaticDrawnEndgame(cachedPositionData)}");
        stringBuilder.AppendLine($"Dynamic Score = {DynamicScore(cachedPositionData)}");
        stringBuilder.AppendLine($"Score Precision = {ScorePrecision(cachedPositionData)}");
        stringBuilder.AppendLine($"Last Accessed = {LastAccessed(cachedPositionData)}");
        return stringBuilder.ToString();
    }
}