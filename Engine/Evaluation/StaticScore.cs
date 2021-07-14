// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Engine.Intelligence;


namespace ErikTheCoder.MadChess.Engine.Evaluation
{
    public sealed class StaticScore
    {
        public const int Max = 999_999;
        public const int Checkmate = Max - Search.MaxHorizon;
        public const int Interrupted = Max - Search.MaxHorizon - 1;
        public const int NotCached = Max - Search.MaxHorizon - 2;
        public const int MaxPlyWithoutCaptureOrPawnMove = 100;
        public int WhiteEgSimple;
        public int WhitePawnMaterial;
        public int WhiteMgPieceMaterial;
        public int WhiteEgPieceMaterial;
        public int WhiteMgPieceLocation;
        public int WhiteEgPieceLocation;
        public int WhitePassedPawnCount;
        public int WhiteMgPassedPawns;
        public int WhiteEgPassedPawns;
        public int WhiteEgFreePassedPawns;
        public int WhiteEgKingEscortedPassedPawns;
        public int WhiteUnstoppablePassedPawns;
        public int WhiteMgPieceMobility;
        public int WhiteEgPieceMobility;
        public int WhiteMgKingSafety;
        public int WhiteMgBishopPair;
        public int WhiteEgBishopPair;
        public int BlackEgSimple;
        public int BlackPawnMaterial;
        public int BlackMgPieceMaterial;
        public int BlackEgPieceMaterial;
        public int BlackMgPieceLocation;
        public int BlackEgPieceLocation;
        public int BlackPassedPawnCount;
        public int BlackMgPassedPawns;
        public int BlackEgPassedPawns;
        public int BlackEgFreePassedPawns;
        public int BlackEgKingEscortedPassedPawns;
        public int BlackUnstoppablePassedPawns;
        public int BlackMgPieceMobility;
        public int BlackEgPieceMobility;
        public int BlackMgKingSafety;
        public int BlackMgBishopPair;
        public int BlackEgBishopPair;
        public int PlySinceCaptureOrPawnMove;
        public int EgScalePer128;


        private int WhiteMgMaterial => WhitePawnMaterial + WhiteMgPieceMaterial;


        private int WhiteMg => WhiteMgMaterial + WhiteMgPieceLocation + WhiteMgPassedPawns + WhiteUnstoppablePassedPawns + WhiteMgPieceMobility + WhiteMgKingSafety + WhiteMgBishopPair;


        private int WhiteEgMaterial => WhitePawnMaterial + WhiteEgPieceMaterial;


        public int WhiteEg => WhiteEgSimple + WhiteEgMaterial + WhiteEgPieceLocation + WhiteEgPassedPawns + WhiteEgFreePassedPawns + WhiteEgKingEscortedPassedPawns + WhiteUnstoppablePassedPawns +
                              WhiteEgPieceMobility + WhiteEgBishopPair;


        private int WhiteEgScaled => (EgScalePer128 * WhiteEg) / 128;


        private int BlackMgMaterial => BlackPawnMaterial + BlackMgPieceMaterial;


        private int BlackMg => BlackMgMaterial + BlackMgPieceLocation + BlackMgPassedPawns + BlackUnstoppablePassedPawns + BlackMgPieceMobility + BlackMgKingSafety + BlackMgBishopPair;


        private int BlackEgMaterial => BlackPawnMaterial + BlackEgPieceMaterial;


        public int BlackEg => BlackEgSimple + BlackEgMaterial + BlackEgPieceLocation + BlackEgPassedPawns + BlackEgFreePassedPawns + BlackEgKingEscortedPassedPawns + BlackUnstoppablePassedPawns +
                              BlackEgPieceMobility + BlackEgBishopPair;


        private int BlackEgScaled => (EgScalePer128 * BlackEg) / 128;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetTaperedScore(int phase) => GetTaperedScore(WhiteMg - BlackMg, WhiteEgScaled - BlackEgScaled, phase);


        // Linearly interpolate between middlegame and endgame scores.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTaperedScore(int middlegameScore, int endgameScore, int phase) => ((middlegameScore * phase) + (endgameScore * (Eval.MiddlegamePhase - phase))) / Eval.MiddlegamePhase;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetTotalScore(int phase)
        {
            var taperedScore = GetTaperedScore(phase);
            // Scale score as position approaches draw by 50 moves (100 ply) without a capture or pawn move.
            return (taperedScore * (MaxPlyWithoutCaptureOrPawnMove - PlySinceCaptureOrPawnMove)) / MaxPlyWithoutCaptureOrPawnMove;
        }


        public void Reset()
        {
            WhiteEgSimple = 0;
            WhitePawnMaterial = 0;
            WhiteMgPieceMaterial = 0;
            WhiteEgPieceMaterial = 0;
            WhiteMgPieceLocation = 0;
            WhiteEgPieceLocation = 0;
            WhitePassedPawnCount = 0;
            WhiteMgPassedPawns = 0;
            WhiteEgPassedPawns = 0;
            WhiteEgFreePassedPawns = 0;
            WhiteEgKingEscortedPassedPawns = 0;
            WhiteUnstoppablePassedPawns = 0;
            WhiteMgPieceMobility = 0;
            WhiteEgPieceMobility = 0;
            WhiteMgKingSafety = 0;
            WhiteMgBishopPair = 0;
            WhiteEgBishopPair = 0;
            BlackEgSimple = 0;
            BlackPawnMaterial = 0;
            BlackMgPieceMaterial = 0;
            BlackEgPieceMaterial = 0;
            BlackMgPieceLocation = 0;
            BlackEgPieceLocation = 0;
            BlackPassedPawnCount = 0;
            BlackMgPassedPawns = 0;
            BlackEgPassedPawns = 0;
            BlackEgFreePassedPawns = 0;
            BlackEgKingEscortedPassedPawns = 0;
            BlackUnstoppablePassedPawns = 0;
            BlackMgPieceMobility = 0;
            BlackEgPieceMobility = 0;
            BlackMgKingSafety = 0;
            BlackMgBishopPair = 0;
            BlackEgBishopPair = 0;
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
            AppendStaticScoreLine(stringBuilder, "Simple Endgame", WhiteEgSimple, BlackEgSimple, WhiteEgSimple, BlackEgSimple, phase);
            AppendStaticScoreLine(stringBuilder, "Material", WhiteMgMaterial, BlackMgMaterial, WhiteEgMaterial, BlackEgMaterial, phase);
            AppendStaticScoreLine(stringBuilder, "Piece Location", WhiteMgPieceLocation, BlackMgPieceLocation, WhiteEgPieceLocation, BlackEgPieceLocation, phase);
            AppendStaticScoreLine(stringBuilder, "Passed Pawns", WhiteMgPassedPawns, BlackMgPassedPawns, WhiteEgPassedPawns, BlackEgPassedPawns, phase);
            AppendStaticScoreLine(stringBuilder, "Free Passed Pawns", 0, 0, WhiteEgFreePassedPawns, BlackEgFreePassedPawns, phase);
            AppendStaticScoreLine(stringBuilder, "King Escorted Passed Pawns", 0, 0, WhiteEgKingEscortedPassedPawns, BlackEgKingEscortedPassedPawns, phase);
            AppendStaticScoreLine(stringBuilder, "Unstoppable Passed Pawns", WhiteUnstoppablePassedPawns, BlackUnstoppablePassedPawns, WhiteUnstoppablePassedPawns, BlackUnstoppablePassedPawns, phase);
            AppendStaticScoreLine(stringBuilder, "Piece Mobility", WhiteMgPieceMobility, BlackMgPieceMobility, WhiteEgPieceMobility, BlackEgPieceMobility, phase);
            AppendStaticScoreLine(stringBuilder, "King Safety", WhiteMgKingSafety, BlackMgKingSafety, 0, 0, phase);
            AppendStaticScoreLine(stringBuilder, "Bishop Pair", WhiteMgBishopPair, BlackMgBishopPair, WhiteEgBishopPair, BlackEgBishopPair, phase);
            stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");
            AppendStaticScoreLine(stringBuilder, "Subtotal", WhiteMg, BlackMg, WhiteEg, BlackEg, phase);
            AppendStaticScoreLine(stringBuilder, "Scale", 100, 100, egScale, egScale, phase);
            AppendStaticScoreLine(stringBuilder, "Total", WhiteMg, BlackMg, WhiteEgScaled, BlackEgScaled, phase);
            stringBuilder.AppendLine();
            var phaseFraction = (100 * phase) / Eval.MiddlegamePhase;
            var totalScore = GetTotalScore(phase) / 100d;
            stringBuilder.AppendLine($"Middlegame   = {phase} of {Eval.MiddlegamePhase} ({phaseFraction}%)");
            stringBuilder.AppendLine($"50 Move Rule = {PlySinceCaptureOrPawnMove} ({MaxPlyWithoutCaptureOrPawnMove - PlySinceCaptureOrPawnMove}%)");
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
}
