// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Engine.Intelligence;


namespace ErikTheCoder.MadChess.Engine.Score;

public static class SpecialScore
{
    // CachedPositionData.StaticScore and CachedPositionData.DynamicScore is allotted 15 bits.
    // 2 Pow 15 = 32_768.
    // Value may be positive or negative, so max value is 32_768 / 2 = 16_384.
    // Account for zero value = 16_384 - 1 = 16_383.
    public const int Max = 16_383;

    public const int Checkmate = Max - Search.MaxHorizon;
    public const int Interrupted = Max - Search.MaxHorizon - 1;
    public const int NotCached = Max - Search.MaxHorizon - 2;
    public const int LargestNonMate = Max - Search.MaxHorizon - 3;
}