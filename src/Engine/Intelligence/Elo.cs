// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine.Intelligence;


public static class Elo
{
    public const int Min = 600;

    // ReSharper disable UnusedMember.Global
    public const int Beginner = 600;
    public const int Patzer = 800;
    public const int Novice = 1000;
    public const int Social = 1200;
    public const int StrongSocial = 1400;
    public const int Club = 1600;
    public const int StrongClub = 1800;
    public const int Expert = 2000;
    public const int CandidateMaster = 2200;
    public const int Master = 2300;
    public const int InternationalMaster = 2400;
    public const int Grandmaster = 2500;
    // ReSharper restore UnusedMember.Global

    public const int Max = 2600;
}