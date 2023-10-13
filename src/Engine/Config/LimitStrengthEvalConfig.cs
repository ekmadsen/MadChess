// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using JetBrains.Annotations;


namespace ErikTheCoder.MadChess.Engine.Config;


[UsedImplicitly]
public sealed class LimitStrengthEvalConfig
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public int UndervaluePawnsMaxElo { get; set; }
    public int UndervalueRookOvervalueQueenMaxElo { get; set; }
    public int ValueKnightBishopEquallyMaxElo { get; set; }
    public int MisjudgePassedPawnsMaxElo { get; set; }
    public int InattentiveKingDefenseMaxElo { get; set; }
    public int MisplacePiecesMaxElo { get; set; }
    public int UnderestimateMobilePiecesMaxElo { get; set; }
    public int AllowPawnStructureDamageMaxElo { get; set; }
    public int UnderestimateThreatsMaxElo { get; set; }
    public int PoorManeuveringMinorPiecesMaxElo { get; set; }
    public int PoorManeuveringMajorPiecesMaxElo { get; set; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}