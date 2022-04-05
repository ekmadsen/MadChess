// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Evaluation;
using ErikTheCoder.MadChess.Engine.Intelligence;


namespace ErikTheCoder.MadChess.Engine.Score;


public sealed class StaticScore
{
    public readonly int[] EgSimple;
    public readonly int[] MgPawnMaterial;
    public readonly int[] EgPawnMaterial;
    public readonly int[] MgPieceMaterial;
    public readonly int[] EgPieceMaterial;
    public readonly int[] MgPieceLocation;
    public readonly int[] EgPieceLocation;
    public readonly int[] MgPawnStructure;
    public readonly int[] EgPawnStructure;
    public readonly int[] MgPassedPawns;
    public readonly int[] EgPassedPawns;
    public readonly int[] EgFreePassedPawns;
    public readonly int[] EgKingEscortedPassedPawns;
    public readonly int[] UnstoppablePassedPawns;
    public readonly int[] MgPieceMobility;
    public readonly int[] EgPieceMobility;
    public readonly int[] MgKingSafety;
    public readonly int[] MgThreats;
    public readonly int[] EgThreats;
    public readonly int[] MgBishopPair;
    public readonly int[] EgBishopPair;
    public readonly int[] MgOutposts;
    public readonly int[] EgOutposts;
    public int PlySinceCaptureOrPawnMove;
    public int EgScalePer128;


    public StaticScore()
    {
        EgSimple = new int[2];
        MgPawnMaterial = new int[2];
        EgPawnMaterial = new int[2];
        MgPieceMaterial = new int[2];
        EgPieceMaterial = new int[2];
        MgPieceLocation = new int[2];
        EgPieceLocation = new int[2];
        MgPawnStructure = new int[2];
        EgPawnStructure = new int[2];
        MgPassedPawns = new int[2];
        EgPassedPawns = new int[2];
        EgFreePassedPawns = new int[2];
        EgKingEscortedPassedPawns = new int[2];
        UnstoppablePassedPawns = new int[2];
        MgPieceMobility = new int[2];
        EgPieceMobility = new int[2];
        MgKingSafety = new int[2];
        MgThreats = new int[2];
        EgThreats = new int[2];
        MgBishopPair = new int[2];
        EgBishopPair = new int[2];
        MgOutposts = new int[2];
        EgOutposts = new int[2];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetMgMaterial(Color color) => MgPawnMaterial[(int)color] + MgPieceMaterial[(int)color];


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetMg(Color color) => GetMgMaterial(color) + MgPieceLocation[(int)color] + MgPawnStructure[(int)color] + MgPassedPawns[(int)color] + UnstoppablePassedPawns[(int)color] +
                                      MgPieceMobility[(int)color] + MgKingSafety[(int)color] + MgThreats[(int)color] + MgBishopPair[(int)color] + MgOutposts[(int)color];


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEgMaterial(Color color) => EgPawnMaterial[(int)color] + EgPieceMaterial[(int)color];


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetEg(Color color) => EgSimple[(int)color] + GetEgMaterial(color) + EgPieceLocation[(int)color] + EgPawnStructure[(int)color] + EgPassedPawns[(int)color] + EgFreePassedPawns[(int)color] +
                                     EgKingEscortedPassedPawns[(int)color] + UnstoppablePassedPawns[(int)color] + EgPieceMobility[(int)color] + EgThreats[(int)color] + EgBishopPair[(int)color] + EgOutposts[(int)color];


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetEgScaled(Color color) => (EgScalePer128 * GetEg(color)) / 128;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetTaperedScore(Color color, int phase)
    {
        var enemyColor = 1 - color;
        var mgScore = GetMg(color);
        var mgEnemyScore = GetMg(enemyColor);
        var egScaledScore = GetEgScaled(color);
        var egEnemyScaledScore = GetEgScaled(enemyColor);
        return GetTaperedScore(mgScore - mgEnemyScore, egScaledScore - egEnemyScaledScore, phase);
    }


    // Linearly interpolate between middlegame and endgame scores.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetTaperedScore(int middlegameScore, int endgameScore, int phase) => ((middlegameScore * phase) + (endgameScore * (Eval.MiddlegamePhase - phase))) / Eval.MiddlegamePhase;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTotalScore(Color color, int phase)
    {
        var taperedScore = GetTaperedScore(color, phase);
        // Scale score as position approaches draw by 50 moves (100 ply) without a capture or pawn move.
        var scaledTaperedScore = (taperedScore * (Search.MaxPlyWithoutCaptureOrPawnMove - PlySinceCaptureOrPawnMove)) / Search.MaxPlyWithoutCaptureOrPawnMove;
        // Evaluation never scores checkmate positions.  Search identifies checkmates.
        return FastMath.Constrain(scaledTaperedScore, -SpecialScore.LargestNonMate, SpecialScore.LargestNonMate);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        MgPieceLocation[(int)Color.White] = 0;
        MgPieceLocation[(int)Color.Black] = 0;
        EgPieceLocation[(int)Color.White] = 0;
        EgPieceLocation[(int)Color.Black] = 0;
        MgPawnStructure[(int)Color.White] = 0;
        MgPawnStructure[(int)Color.Black] = 0;
        EgPawnStructure[(int)Color.White] = 0;
        EgPawnStructure[(int)Color.Black] = 0;
        MgPassedPawns[(int)Color.White] = 0;
        MgPassedPawns[(int)Color.Black] = 0;
        EgPassedPawns[(int)Color.White] = 0;
        EgPassedPawns[(int)Color.Black] = 0;
        EgFreePassedPawns[(int)Color.White] = 0;
        EgFreePassedPawns[(int)Color.Black] = 0;
        EgKingEscortedPassedPawns[(int)Color.White] = 0;
        EgKingEscortedPassedPawns[(int)Color.Black] = 0;
        UnstoppablePassedPawns[(int)Color.White] = 0;
        UnstoppablePassedPawns[(int)Color.Black] = 0;
        MgPieceMobility[(int)Color.White] = 0;
        MgPieceMobility[(int)Color.Black] = 0;
        EgPieceMobility[(int)Color.White] = 0;
        EgPieceMobility[(int)Color.Black] = 0;
        MgKingSafety[(int)Color.White] = 0;
        MgKingSafety[(int)Color.Black] = 0;
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
        PlySinceCaptureOrPawnMove = 0;
        EgScalePer128 = 128;
    }


    public string ToString(int phase)
    {
        var egScale = (100 * EgScalePer128) / 128;
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("                             |         Middlegame        |          Endgame          |           Total           |");
        stringBuilder.AppendLine("Evaluation Param             |  White    Black     Diff  |  White    Black     Diff  |  White    Black     Diff  |");
        stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");
        AppendStaticScoreLine(stringBuilder, "Simple Endgame", EgSimple[(int)Color.White], EgSimple[(int)Color.Black], EgSimple[(int)Color.White], EgSimple[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Material", GetMgMaterial(Color.White), GetMgMaterial(Color.Black), GetEgMaterial(Color.White), GetEgMaterial(Color.Black), phase);
        AppendStaticScoreLine(stringBuilder, "Piece Location", MgPieceLocation[(int)Color.White], MgPieceLocation[(int)Color.Black], EgPieceLocation[(int)Color.White], EgPieceLocation[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Pawn Structure", MgPawnStructure[(int)Color.White], MgPawnStructure[(int)Color.Black], EgPawnStructure[(int)Color.White], EgPawnStructure[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Passed Pawns", MgPassedPawns[(int)Color.White], MgPassedPawns[(int)Color.Black], EgPassedPawns[(int)Color.White], EgPassedPawns[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Free Passed Pawns", 0, 0, EgFreePassedPawns[(int)Color.White], EgFreePassedPawns[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "King Escorted Passed Pawns", 0, 0, EgKingEscortedPassedPawns[(int)Color.White], EgKingEscortedPassedPawns[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Unstoppable Passed Pawns", UnstoppablePassedPawns[(int)Color.White], UnstoppablePassedPawns[(int)Color.Black], UnstoppablePassedPawns[(int)Color.White], UnstoppablePassedPawns[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Piece Mobility", MgPieceMobility[(int)Color.White], MgPieceMobility[(int)Color.Black], EgPieceMobility[(int)Color.White], EgPieceMobility[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "King Safety", MgKingSafety[(int)Color.White], MgKingSafety[(int)Color.Black], 0, 0, phase);
        AppendStaticScoreLine(stringBuilder, "Threats", MgThreats[(int)Color.White], MgThreats[(int)Color.Black], EgThreats[(int)Color.White], EgThreats[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Bishop Pair", MgBishopPair[(int)Color.White], MgBishopPair[(int)Color.Black], EgBishopPair[(int)Color.White], EgBishopPair[(int)Color.Black], phase);
        AppendStaticScoreLine(stringBuilder, "Outposts", MgOutposts[(int)Color.White], MgOutposts[(int)Color.Black], EgOutposts[(int)Color.White], EgOutposts[(int)Color.Black], phase);
        stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");
        var mgWhite = GetMg(Color.White);
        var mgBlack = GetMg(Color.Black);
        AppendStaticScoreLine(stringBuilder, "Subtotal", mgWhite, mgBlack, GetEg(Color.White), GetEg(Color.Black), phase);
        AppendStaticScoreLine(stringBuilder, "Scale", 100, 100, egScale, egScale, phase);
        AppendStaticScoreLine(stringBuilder, "Total", mgWhite, mgBlack, GetEgScaled(Color.White), GetEgScaled(Color.Black), phase);
        stringBuilder.AppendLine();
        var phaseFraction = (100 * phase) / Eval.MiddlegamePhase;
        var totalScore = GetTotalScore(Color.White, phase) / 100d;
        stringBuilder.AppendLine($"Middlegame   = {phase} of {Eval.MiddlegamePhase} ({phaseFraction}%)");
        stringBuilder.AppendLine($"50 Move Rule = {PlySinceCaptureOrPawnMove} ({Search.MaxPlyWithoutCaptureOrPawnMove - PlySinceCaptureOrPawnMove}%)");
        stringBuilder.AppendLine($"Total Score  = {totalScore:0.00}");
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