// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Engine.Intelligence;


namespace ErikTheCoder.MadChess.Engine.Score;

public static class SpecialScore
{
    public const int Max = 30_000;
    public const int Checkmate = Max - Search.MaxHorizon;
    public const int Interrupted = Max - Search.MaxHorizon - 1;
    public const int NotCached = Max - Search.MaxHorizon - 2;
    public const int LargestNonMate = Max - Search.MaxHorizon - 3;
    public const int SimpleEndgame = 20_000;
}