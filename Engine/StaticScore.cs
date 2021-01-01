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
        public const int Checkmate = Max - Search.MaxHorizon - 1;
        public const int Interrupted = Max - Search.MaxHorizon - 2;
        public const int NotCached = Max - Search.MaxHorizon - 3;
        public int WhiteSimpleEndgame;
        public int WhiteMaterial;
        public int WhiteMgPieceLocation;
        public int WhiteEgPieceLocation;
        public int WhiteMgPassedPawns;
        public int WhiteEgPassedPawns;
        public int WhiteEgFreePassedPawns;
        public int WhiteEgKingEscortedPassedPawns;
        public int WhiteUnstoppablePassedPawns;
        public int WhiteMgPieceMobility;
        public int WhiteEgPieceMobility;
        public int WhiteMgKingSafety;
        public int WhiteEgKingSafety;
        public int BlackSimpleEndgame;
        public int BlackMaterial;
        public int BlackMgPieceLocation;
        public int BlackEgPieceLocation;
        public int BlackMgPassedPawns;
        public int BlackEgPassedPawns;
        public int BlackEgFreePassedPawns;
        public int BlackEgKingEscortedPassedPawns;
        public int BlackUnstoppablePassedPawns;
        public int BlackMgPieceMobility;
        public int BlackEgPieceMobility;
        public int BlackMgKingSafety;
        public int BlackEgKingSafety;


        private int MiddlegameWhite => WhiteSimpleEndgame + WhiteMaterial + WhiteMgPieceLocation + WhiteMgPassedPawns + WhiteUnstoppablePassedPawns + WhiteMgPieceMobility + WhiteMgKingSafety;


        private int MiddlegameBlack => BlackSimpleEndgame + BlackMaterial + BlackMgPieceLocation + BlackMgPassedPawns + BlackUnstoppablePassedPawns + BlackMgPieceMobility + BlackMgKingSafety;


        private int EndgameWhite => WhiteSimpleEndgame + WhiteMaterial + WhiteEgPieceLocation + WhiteEgPassedPawns + WhiteEgFreePassedPawns + WhiteEgKingEscortedPassedPawns + WhiteUnstoppablePassedPawns +
                                    WhiteEgPieceMobility + WhiteEgKingSafety;


        private int EndgameBlack => BlackSimpleEndgame + BlackMaterial + BlackEgPieceLocation + BlackEgPassedPawns + BlackEgFreePassedPawns + BlackEgKingEscortedPassedPawns + BlackUnstoppablePassedPawns +
                                    BlackEgPieceMobility + BlackEgKingSafety;


        public int TotalScore(int Phase) => GetTaperedScore(MiddlegameWhite - MiddlegameBlack, EndgameWhite - EndgameBlack, Phase);


        public void Reset()
        {
            WhiteSimpleEndgame = 0;
            WhiteMaterial = 0;
            WhiteMgPieceLocation = 0;
            WhiteEgPieceLocation = 0;
            WhiteMgPassedPawns = 0;
            WhiteEgPassedPawns = 0;
            WhiteEgFreePassedPawns = 0;
            WhiteEgKingEscortedPassedPawns = 0;
            WhiteUnstoppablePassedPawns = 0;
            WhiteMgPieceMobility = 0;
            WhiteEgPieceMobility = 0;
            WhiteMgKingSafety = 0;
            WhiteEgKingSafety = 0;
            BlackSimpleEndgame = 0;
            BlackMaterial = 0;
            BlackMgPieceLocation = 0;
            BlackEgPieceLocation = 0;
            BlackMgPassedPawns = 0;
            BlackEgPassedPawns = 0;
            BlackEgFreePassedPawns = 0;
            BlackEgKingEscortedPassedPawns = 0;
            BlackUnstoppablePassedPawns = 0;
            BlackMgPieceMobility = 0;
            BlackEgPieceMobility = 0;
            BlackMgKingSafety = 0;
            BlackEgKingSafety = 0;
        }


        public string ToString(int Phase)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("                             |         Middlegame        |          Endgame          |           Total           |");
            stringBuilder.AppendLine("Evaluation Term              |  White    Black     Diff  |  White    Black     Diff  |  White    Black     Diff  |");
            stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");
            AppendLine(stringBuilder, "Simple Endgame", WhiteSimpleEndgame, BlackSimpleEndgame, WhiteSimpleEndgame, BlackSimpleEndgame, Phase);
            AppendLine(stringBuilder, "Material", WhiteMaterial, BlackMaterial, WhiteMaterial, BlackMaterial, Phase);
            AppendLine(stringBuilder, "Piece Location", WhiteMgPieceLocation, BlackMgPieceLocation, WhiteEgPieceLocation, BlackEgPieceLocation, Phase);
            AppendLine(stringBuilder, "Passed Pawns", WhiteMgPassedPawns, BlackMgPassedPawns, WhiteEgPassedPawns, BlackEgPassedPawns, Phase);
            AppendLine(stringBuilder, "Free Passed Pawns", 0, 0, WhiteEgFreePassedPawns, BlackEgFreePassedPawns, Phase);
            AppendLine(stringBuilder, "King Escorted Passed Pawns", 0, 0, WhiteEgKingEscortedPassedPawns, BlackEgKingEscortedPassedPawns, Phase);
            AppendLine(stringBuilder, "Unstoppable Passed Pawns", WhiteUnstoppablePassedPawns, BlackUnstoppablePassedPawns, WhiteUnstoppablePassedPawns, BlackUnstoppablePassedPawns, Phase);
            AppendLine(stringBuilder, "Piece Mobility", WhiteMgPieceMobility, BlackMgPieceMobility, WhiteEgPieceMobility, BlackEgPieceMobility, Phase);
            AppendLine(stringBuilder, "King Safety", WhiteMgKingSafety, BlackMgKingSafety, WhiteEgKingSafety, BlackEgKingSafety, Phase);
            stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");
            AppendLine(stringBuilder, "Total", MiddlegameWhite, MiddlegameBlack, EndgameWhite, EndgameBlack, Phase);
            stringBuilder.AppendLine();
            var middlegamePercent = (100 * Phase) / Evaluation.MiddlegamePhase;
            stringBuilder.AppendLine($"Middlegame  = {Phase} of {Evaluation.MiddlegamePhase} ({middlegamePercent}%)");
            return stringBuilder.ToString();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetTaperedScore(int MiddlegameScore, int EndgameScore, int Phase) => ((MiddlegameScore * Phase) + (EndgameScore * (Evaluation.MiddlegamePhase - Phase))) / Evaluation.MiddlegamePhase;


        private static void AppendLine(StringBuilder StringBuilder, string EvaluationTerm, int WhiteMg, int BlackMg, int WhiteEg, int BlackEg, int Phase)
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
