// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Engine.Intelligence;


namespace ErikTheCoder.MadChess.Engine.Score;


public sealed class StaticScore
{
    public const int Max = 30_000;
    public const int Checkmate = Max - Search.MaxHorizon;
    public const int Interrupted = Max - Search.MaxHorizon - 1;
    public const int NotCached = Max - Search.MaxHorizon - 2;
    public const int SimpleEndgame = 20_000;
    public const int SimpleMajorPieceEndgame = 25_000;

    public readonly int[] EgSimple; // [color]

    public readonly int[] MgPawnMaterial; // [color]
    public readonly int[] EgPawnMaterial; // [color]

    public readonly int[] MgPieceMaterial; // [color]
    public readonly int[] EgPieceMaterial; // [color]

    public readonly int[] MgPassedPawns; // [color]
    public readonly int[] EgPassedPawns; // [color]
    public readonly int[] EgFreePassedPawns; // [color]
    public readonly int[] EgConnectedPassedPawns; // [color]
    public readonly int[] EgKingEscortedPassedPawns; // [color]
    public readonly int[] UnstoppablePassedPawns; // [color]

    public readonly int[] MgKingSafety; // [color]

    public readonly int[] MgPieceLocation; // [color]
    public readonly int[] EgPieceLocation; // [color]

    public readonly int[] MgPieceMobility; // [color]
    public readonly int[] EgPieceMobility; // [color]

    public readonly int[] MgPawnStructure; // [color]
    public readonly int[] EgPawnStructure; // [color]

    public readonly int[] MgThreats; // [color]
    public readonly int[] EgThreats; // [color]

    public readonly int[] MgBishopPair; // [color]
    public readonly int[] EgBishopPair; // [color]

    public readonly int[] MgOutposts; // [color]
    public readonly int[] EgOutposts; // [color]

    // ReSharper disable InconsistentNaming
    public readonly int[] MgRookOn7thRank; // [color]
    public readonly int[] EgRookOn7thRank; // [color]
    // ReSharper restore InconsistentNaming

    public readonly int[] Closedness; // [color]


    public int PlySinceCaptureOrPawnMove;

    public StaticScore()
    {
        EgSimple = new int[2];

        MgPawnMaterial = new int[2];
        EgPawnMaterial = new int[2];

        MgPieceMaterial = new int[2];
        EgPieceMaterial = new int[2];

        MgPassedPawns = new int[2];
        EgPassedPawns = new int[2];
        EgFreePassedPawns = new int[2];
        EgConnectedPassedPawns = new int[2];
        EgKingEscortedPassedPawns = new int[2];
        UnstoppablePassedPawns = new int[2];

        MgKingSafety = new int[2];

        MgPieceLocation = new int[2];
        EgPieceLocation = new int[2];

        MgPieceMobility = new int[2];
        EgPieceMobility = new int[2];

        MgPawnStructure = new int[2];
        EgPawnStructure = new int[2];

        MgThreats = new int[2];
        EgThreats = new int[2];

        MgBishopPair = new int[2];
        EgBishopPair = new int[2];

        MgOutposts = new int[2];
        EgOutposts = new int[2];

        MgRookOn7thRank = new int[2];
        EgRookOn7thRank = new int[2];

        Closedness = new int[2];
    }


    private int GetMgMaterial(Color color) => MgPawnMaterial[(int)color] + MgPieceMaterial[(int)color];


    private int GetMg(Color color) => GetMgMaterial(color) +
                                      MgPassedPawns[(int)color] + UnstoppablePassedPawns[(int)color] +
                                      MgKingSafety[(int)color] +
                                      MgPieceLocation[(int)color] + MgPieceMobility[(int)color] + MgPawnStructure[(int)color] +
                                      MgThreats[(int)color] + MgBishopPair[(int)color] + MgOutposts[(int)color] + MgRookOn7thRank[(int)color];


    private int GetEgMaterial(Color color) => EgPawnMaterial[(int)color] + EgPieceMaterial[(int)color];


    public int GetEg(Color color) => EgSimple[(int)color] + GetEgMaterial(color) +
                                     EgPassedPawns[(int)color] + EgFreePassedPawns[(int)color] + EgConnectedPassedPawns[(int)color] + EgKingEscortedPassedPawns[(int)color] + UnstoppablePassedPawns[(int)color] +
                                     EgPieceLocation[(int)color] + EgPieceMobility[(int)color] + EgPawnStructure[(int)color] +
                                     EgThreats[(int)color] + EgBishopPair[(int)color] + EgOutposts[(int)color] + EgRookOn7thRank[(int)color];


    public int GetTotalScore(Color color, int phase)
    {
        var taperedScore = GetTaperedScore(color, phase);
        // Scale score as position approaches draw by 50 moves (100 ply) without a capture or pawn move.
        return (taperedScore * (Search.MaxPlyWithoutCaptureOrPawnMove - PlySinceCaptureOrPawnMove)) / Search.MaxPlyWithoutCaptureOrPawnMove;
    }


    private int GetTaperedScore(Color color, int phase)
    {
        var enemyColor = 1 - color;

        var mgScore = GetMg(color);
        var mgEnemyScore = GetMg(enemyColor);

        var egScore = GetEg(color);
        var egEnemyScore = GetEg(enemyColor);

        var closedness = Closedness[(int)color] + Closedness[(int)enemyColor];

        return GetTaperedScore(closedness + mgScore - mgEnemyScore, (closedness + egScore - egEnemyScore) / egScoreReductionDivisor, phase);
    }

    // Factor introduced by AW, to make scores more resemble real centipawns in endgame.  
    // Also this prevents degradation of UCI_Elo-degradation in endgame. 
    const int egScoreReductionDivisor = 2;

    // Linearly interpolate between middlegame and endgame scores.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetTaperedScore(int middlegameScore, int endgameScore, int phase) 
        => ((middlegameScore * phase) + (endgameScore * (Evaluation.MiddlegamePhase - phase))) / Evaluation.MiddlegamePhase;


    public void Reset()
    {
        // Explicit array lookups are faster than looping through colors.
        EgSimple[(int)Color.White] = 0;
        EgSimple[(int)Color.Black] = 0;

        MgPawnMaterial[(int)Color.White] = 0;
        MgPawnMaterial[(int)Color.Black] = 0;
        EgPawnMaterial[(int)Color.White] = 0;
        EgPawnMaterial[(int)Color.Black] = 0;

        MgPieceMaterial[(int)Color.White] = 0;
        MgPieceMaterial[(int)Color.Black] = 0;
        EgPieceMaterial[(int)Color.White] = 0;
        EgPieceMaterial[(int)Color.Black] = 0;

        MgPassedPawns[(int)Color.White] = 0;
        MgPassedPawns[(int)Color.Black] = 0;
        EgPassedPawns[(int)Color.White] = 0;
        EgPassedPawns[(int)Color.Black] = 0;
        EgFreePassedPawns[(int)Color.White] = 0;
        EgFreePassedPawns[(int)Color.Black] = 0;
        EgConnectedPassedPawns[(int)Color.White] = 0;
        EgConnectedPassedPawns[(int)Color.Black] = 0;
        EgKingEscortedPassedPawns[(int)Color.White] = 0;
        EgKingEscortedPassedPawns[(int)Color.Black] = 0;
        UnstoppablePassedPawns[(int)Color.White] = 0;
        UnstoppablePassedPawns[(int)Color.Black] = 0;

        MgKingSafety[(int)Color.White] = 0;
        MgKingSafety[(int)Color.Black] = 0;

        MgPieceLocation[(int)Color.White] = 0;
        MgPieceLocation[(int)Color.Black] = 0;
        EgPieceLocation[(int)Color.White] = 0;
        EgPieceLocation[(int)Color.Black] = 0;

        MgPieceMobility[(int)Color.White] = 0;
        MgPieceMobility[(int)Color.Black] = 0;
        EgPieceMobility[(int)Color.White] = 0;
        EgPieceMobility[(int)Color.Black] = 0;

        MgPawnStructure[(int)Color.White] = 0;
        MgPawnStructure[(int)Color.Black] = 0;
        EgPawnStructure[(int)Color.White] = 0;
        EgPawnStructure[(int)Color.Black] = 0;

        MgThreats[(int)Color.White] = 0;
        MgThreats[(int)Color.Black] = 0;
        EgThreats[(int)Color.White] = 0;
        EgThreats[(int)Color.Black] = 0;

        MgBishopPair[(int)Color.White] = 0;
        MgBishopPair[(int)Color.Black] = 0;
        EgBishopPair[(int)Color.White] = 0;
        EgBishopPair[(int)Color.Black] = 0;

        MgOutposts[(int)Color.White] = 0;
        MgOutposts[(int)Color.Black] = 0;
        EgOutposts[(int)Color.White] = 0;
        EgOutposts[(int)Color.Black] = 0;

        MgRookOn7thRank[(int)Color.White] = 0;
        MgRookOn7thRank[(int)Color.Black] = 0;
        EgRookOn7thRank[(int)Color.White] = 0;
        EgRookOn7thRank[(int)Color.Black] = 0;

        Closedness[(int)Color.White] = 0;
        Closedness[(int)Color.Black] = 0;

        PlySinceCaptureOrPawnMove = 0;
    }


    public string ToString(int phase)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("                             |         Middlegame        |          Endgame          |           Total           |");
        stringBuilder.AppendLine("Evaluation Param             |  White    Black     Diff  |  White    Black     Diff  |  White    Black     Diff  |");
        stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");

        AppendStaticScoreLine(stringBuilder, "Simple Endgame", EgSimple[(int)Color.White], EgSimple[(int)Color.Black], EgSimple[(int)Color.White], EgSimple[(int)Color.Black], phase);

        AppendStaticScoreLine(stringBuilder, "Material", GetMgMaterial(Color.White), GetMgMaterial(Color.Black), GetEgMaterial(Color.White), GetEgMaterial(Color.Black), phase);

        AppendStaticScoreLine(stringBuilder, "Passed Pawns", MgPassedPawns[(int)Color.White], MgPassedPawns[(int)Color.Black], EgPassedPawns[(int)Color.White], EgPassedPawns[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Free Passed Pawns", 0, 0, EgFreePassedPawns[(int)Color.White], EgFreePassedPawns[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Connected Passed Pawns", 0, 0, EgConnectedPassedPawns[(int)Color.White], EgConnectedPassedPawns[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "King Escorted Passed Pawns", 0, 0, EgKingEscortedPassedPawns[(int)Color.White], EgKingEscortedPassedPawns[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Unstoppable Passed Pawns", UnstoppablePassedPawns[(int)Color.White], UnstoppablePassedPawns[(int)Color.Black], UnstoppablePassedPawns[(int)Color.White], UnstoppablePassedPawns[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "King Safety", MgKingSafety[(int)Color.White], MgKingSafety[(int)Color.Black], 0, 0, phase);

        AppendStaticScoreLine(stringBuilder, "Piece Location", MgPieceLocation[(int)Color.White], MgPieceLocation[(int)Color.Black], EgPieceLocation[(int)Color.White], EgPieceLocation[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Piece Mobility", MgPieceMobility[(int)Color.White], MgPieceMobility[(int)Color.Black], EgPieceMobility[(int)Color.White], EgPieceMobility[(int)Color.Black], phase);

        AppendStaticScoreLine(stringBuilder, "Pawn Structure", MgPawnStructure[(int)Color.White], MgPawnStructure[(int)Color.Black], EgPawnStructure[(int)Color.White], EgPawnStructure[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Threats", MgThreats[(int)Color.White], MgThreats[(int)Color.Black], EgThreats[(int)Color.White], EgThreats[(int)Color.Black], phase);

        AppendStaticScoreLine(stringBuilder, "Bishop Pair", MgBishopPair[(int)Color.White], MgBishopPair[(int)Color.Black], EgBishopPair[(int)Color.White], EgBishopPair[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Outposts", MgOutposts[(int)Color.White], MgOutposts[(int)Color.Black], EgOutposts[(int)Color.White], EgOutposts[(int)Color.Black], phase);

        AppendStaticScoreLine(stringBuilder, "Rook on 7th Rank", MgRookOn7thRank[(int)Color.White], MgRookOn7thRank[(int)Color.Black], EgRookOn7thRank[(int)Color.White], EgRookOn7thRank[(int)Color.Black], phase);
        stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");

        var mgWhite = GetMg(Color.White);
        var mgBlack = GetMg(Color.Black);

        AppendStaticScoreLine(stringBuilder, "Total", mgWhite, mgBlack, GetEg(Color.White), GetEg(Color.Black), phase);
        stringBuilder.AppendLine();

        var phaseFraction = (100 * phase) / Evaluation.MiddlegamePhase;
        var totalScore = GetTotalScore(Color.White, phase) / 100d;

        stringBuilder.AppendLine($"Middlegame   = {phase} of {Evaluation.MiddlegamePhase} ({phaseFraction}%)");
        stringBuilder.AppendLine($"50 Move Rule = {PlySinceCaptureOrPawnMove} ({Search.MaxPlyWithoutCaptureOrPawnMove - PlySinceCaptureOrPawnMove}%)");
        stringBuilder.Append($"Total Score  = {totalScore:0.00}");

        return stringBuilder.ToString();
    }


    private static void AppendStaticScoreLine(StringBuilder stringBuilder, string evalParam, int whiteMg, int blackMg, int whiteEg, int blackEg, int phase)
    {
        var paddedEvalParam = evalParam.PadRight(27);

        var mgWhite = (whiteMg / 100d).ToString("0.00").PadLeft(7);
        var mgBlack = (blackMg / 100d).ToString("0.00").PadLeft(7);
        var mgDiff = ((whiteMg - blackMg) / 100d).ToString("0.00").PadLeft(7);

        var egWhite = (whiteEg / 100d).ToString("0.00").PadLeft(7);
        var egBlack = (blackEg / 100d).ToString("0.00").PadLeft(7);
        var egDiff = ((whiteEg - blackEg) / 100d).ToString("0.00").PadLeft(7);

        var white = GetTaperedScore(whiteMg, whiteEg, phase);
        var black = GetTaperedScore(blackMg, blackEg, phase);

        var totalWhite = (white / 100d).ToString("0.00").PadLeft(7);
        var totalBlack = (black / 100d).ToString("0.00").PadLeft(7);
        var totalDiff = ((white - black) / 100d).ToString("0.00").PadLeft(7);

        stringBuilder.AppendLine($"{paddedEvalParam}   {mgWhite}  {mgBlack}  {mgDiff}   {egWhite}  {egBlack}  {egDiff}   {totalWhite}  {totalBlack}  {totalDiff}");
    }
}