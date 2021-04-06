// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Runtime.CompilerServices;
using System.Text;


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class StaticScore
    {
        public const int Max = 9999;
        public const int Checkmate = Max - Search.MaxHorizon;
        public const int Interrupted = Max - Search.MaxHorizon - 1;
        public const int NotCached = Max - Search.MaxHorizon - 2;
        public const int MaxPlyWithoutCaptureOrPawnMove = 100;
        public int WhiteEgSimple;
        public int WhiteMgPawnMaterial;
        public int WhiteEgPawnMaterial;
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
        public int WhiteEgKingSafety;
        public int WhiteMgBishopPair;
        public int WhiteEgBishopPair;
        public int BlackEgSimple;
        public int BlackMgPawnMaterial;
        public int BlackEgPawnMaterial;
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
        public int BlackEgKingSafety;
        public int BlackMgBishopPair;
        public int BlackEgBishopPair;
        public int PlySinceCaptureOrPawnMove;
        public int EgScalePer128;


        private int WhiteMgMaterial => WhiteMgPawnMaterial + WhiteMgPieceMaterial;


        private int WhiteMg => WhiteMgMaterial + WhiteMgPieceLocation + WhiteMgPassedPawns + WhiteUnstoppablePassedPawns + WhiteMgPieceMobility + WhiteMgKingSafety + WhiteMgBishopPair;


        private int WhiteEgMaterial => WhiteEgPawnMaterial + WhiteEgPieceMaterial;


        public int WhiteEg => WhiteEgSimple + WhiteEgMaterial + WhiteEgPieceLocation + WhiteEgPassedPawns + WhiteEgFreePassedPawns + WhiteEgKingEscortedPassedPawns + WhiteUnstoppablePassedPawns +
                               WhiteEgPieceMobility + WhiteEgKingSafety + WhiteEgBishopPair;


        private int WhiteEgScaled => (EgScalePer128 * WhiteEg) / 128;


        private int BlackMgMaterial => BlackMgPawnMaterial + BlackMgPieceMaterial;


        private int BlackMg => BlackMgMaterial + BlackMgPieceLocation + BlackMgPassedPawns + BlackUnstoppablePassedPawns + BlackMgPieceMobility + BlackMgKingSafety + BlackMgBishopPair;


        private int BlackEgMaterial => BlackEgPawnMaterial + BlackEgPieceMaterial;


        public int BlackEg => BlackEgSimple + BlackEgMaterial + BlackEgPieceLocation + BlackEgPassedPawns + BlackEgFreePassedPawns + BlackEgKingEscortedPassedPawns + BlackUnstoppablePassedPawns +
                               BlackEgPieceMobility + BlackEgKingSafety + BlackEgBishopPair;


        private int BlackEgScaled => (EgScalePer128 * BlackEg) / 128;
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetTaperedScore(int Phase) => GetTaperedScore(WhiteMg - BlackMg, WhiteEgScaled - BlackEgScaled, Phase);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetTaperedScore(int MiddlegameScore, int EndgameScore, int Phase) => ((MiddlegameScore * Phase) + (EndgameScore * (Evaluation.MiddlegamePhase - Phase))) / Evaluation.MiddlegamePhase;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetTotalScore(int Phase)
        {
            var taperedScore = GetTaperedScore(Phase);
            // Scale score as position approaches draw by 50 moves (100 ply) without a capture or pawn move.
            return (taperedScore * (MaxPlyWithoutCaptureOrPawnMove - PlySinceCaptureOrPawnMove)) / MaxPlyWithoutCaptureOrPawnMove;
        }


        public void Reset()
        {
            WhiteEgSimple = 0;
            WhiteMgPawnMaterial = 0;
            WhiteEgPawnMaterial = 0;
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
            WhiteEgKingSafety = 0;
            WhiteMgBishopPair = 0;
            WhiteEgBishopPair = 0;
            BlackEgSimple = 0;
            BlackMgPawnMaterial = 0;
            BlackEgPawnMaterial = 0;
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
            BlackEgKingSafety = 0;
            BlackMgBishopPair = 0;
            BlackEgBishopPair = 0;
            PlySinceCaptureOrPawnMove = 0;
            EgScalePer128 = 128;
        }


        public string ToString(int Phase)
        {
            var egScale = (100 * EgScalePer128) / 128;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("                             |         Middlegame        |          Endgame          |           Total           |");
            stringBuilder.AppendLine("Evaluation Term              |  White    Black     Diff  |  White    Black     Diff  |  White    Black     Diff  |");
            stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");
            AppendStaticScoreLine(stringBuilder, "Simple Endgame", WhiteEgSimple, BlackEgSimple, WhiteEgSimple, BlackEgSimple, Phase);
            AppendStaticScoreLine(stringBuilder, "Material", WhiteMgMaterial, BlackMgMaterial, WhiteEgMaterial, BlackEgMaterial, Phase);
            AppendStaticScoreLine(stringBuilder, "Piece Location", WhiteMgPieceLocation, BlackMgPieceLocation, WhiteEgPieceLocation, BlackEgPieceLocation, Phase);
            AppendStaticScoreLine(stringBuilder, "Passed Pawns", WhiteMgPassedPawns, BlackMgPassedPawns, WhiteEgPassedPawns, BlackEgPassedPawns, Phase);
            AppendStaticScoreLine(stringBuilder, "Free Passed Pawns", 0, 0, WhiteEgFreePassedPawns, BlackEgFreePassedPawns, Phase);
            AppendStaticScoreLine(stringBuilder, "King Escorted Passed Pawns", 0, 0, WhiteEgKingEscortedPassedPawns, BlackEgKingEscortedPassedPawns, Phase);
            AppendStaticScoreLine(stringBuilder, "Unstoppable Passed Pawns", WhiteUnstoppablePassedPawns, BlackUnstoppablePassedPawns, WhiteUnstoppablePassedPawns, BlackUnstoppablePassedPawns, Phase);
            AppendStaticScoreLine(stringBuilder, "Piece Mobility", WhiteMgPieceMobility, BlackMgPieceMobility, WhiteEgPieceMobility, BlackEgPieceMobility, Phase);
            AppendStaticScoreLine(stringBuilder, "King Safety", WhiteMgKingSafety, BlackMgKingSafety, WhiteEgKingSafety, BlackEgKingSafety, Phase);
            AppendStaticScoreLine(stringBuilder, "Bishop Pair", WhiteMgBishopPair, BlackMgBishopPair, WhiteEgBishopPair, BlackEgBishopPair, Phase);
            stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");
            AppendStaticScoreLine(stringBuilder, "Subtotal", WhiteMg, BlackMg, WhiteEg, BlackEg, Phase);
            AppendStaticScoreLine(stringBuilder, "Scale", 100, 100, egScale, egScale, Phase);
            AppendStaticScoreLine(stringBuilder, "Total", WhiteMg, BlackMg, WhiteEgScaled, BlackEgScaled, Phase);
            stringBuilder.AppendLine();
            var middlegamePercent = (100 * Phase) / Evaluation.MiddlegamePhase;
            var totalScore = GetTotalScore(Phase) / 100d;
            stringBuilder.AppendLine($"Middlegame   = {Phase} of {Evaluation.MiddlegamePhase} ({middlegamePercent}%)");
            stringBuilder.AppendLine($"50 Move Rule = {MaxPlyWithoutCaptureOrPawnMove - PlySinceCaptureOrPawnMove}%");
            stringBuilder.AppendLine($"Total Score  = {totalScore:0.00}");
            return stringBuilder.ToString();
        }


        private static void AppendStaticScoreLine(StringBuilder StringBuilder, string EvaluationTerm, int WhiteMg, int BlackMg, int WhiteEg, int BlackEg, int Phase)
        {
            var evaluationTerm = EvaluationTerm.PadRight(27);
            var mgWhite = (WhiteMg / 100d).ToString("0.00").PadLeft(7);
            var mgBlack = (BlackMg / 100d).ToString("0.00").PadLeft(7);
            var mgDiff = ((WhiteMg - BlackMg) / 100d).ToString("0.00").PadLeft(7);
            var egWhite = (WhiteEg / 100d).ToString("0.00").PadLeft(7);
            var egBlack = (BlackEg / 100d).ToString("0.00").PadLeft(7);
            var egDiff = ((WhiteEg - BlackEg) / 100d).ToString("0.00").PadLeft(7);
            var white = GetTaperedScore(WhiteMg, WhiteEg, Phase);
            var black = GetTaperedScore(BlackMg, BlackEg, Phase);
            var totalWhite = (white / 100d).ToString("0.00").PadLeft(7);
            var totalBlack = (black / 100d).ToString("0.00").PadLeft(7);
            var totalDiff = ((white - black) / 100d).ToString("0.00").PadLeft(7);
            StringBuilder.AppendLine($"{evaluationTerm}   {mgWhite}  {mgBlack}  {mgDiff}   {egWhite}  {egBlack}  {egDiff}   {totalWhite}  {totalBlack}  {totalDiff}");
        }
    }
}
