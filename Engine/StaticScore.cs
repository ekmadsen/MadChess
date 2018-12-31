// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2018.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See License.txt for details.          |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Runtime.CompilerServices;
using System.Text;


namespace MadChess.Engine
{
    public sealed class StaticScore
    {
        public const int Max = 9999;
        public const int LongestCheckmate = 100;
        public const int Checkmate = Max - LongestCheckmate;
        public const int Interrupted = Max - LongestCheckmate - 1;
        public const int NotCached = Max - LongestCheckmate - 2;
        private readonly int _middlegamePhase;
        public int WhiteMaterial;
        public int WhiteMgPieceLocation;
        public int WhiteEgPieceLocation;
        public int WhiteMgPassedPawns;
        public int WhiteEgPassedPawns;
        public int WhiteEgFreePassedPawns;
        public int WhiteEgKingEscortedPassedPawns;
        public int WhiteUnstoppablePassedPawns;
        public int BlackMaterial;
        public int BlackMgPieceLocation;
        public int BlackEgPieceLocation;
        public int BlackMgPassedPawns;
        public int BlackEgPassedPawns;
        public int BlackEgFreePassedPawns;
        public int BlackEgKingEscortedPassedPawns;
        public int BlackUnstoppablePassedPawns;


        public int MaterialScore => WhiteMaterial - BlackMaterial;


        private int MiddlegameWhite => WhiteMaterial + WhiteMgPieceLocation + WhiteMgPassedPawns + WhiteUnstoppablePassedPawns;


        private int MiddlegameBlack => BlackMaterial + BlackMgPieceLocation + BlackMgPassedPawns + BlackUnstoppablePassedPawns;


        private int EndgameWhite => WhiteMaterial + WhiteEgPieceLocation + WhiteEgPassedPawns + WhiteEgFreePassedPawns + WhiteEgKingEscortedPassedPawns + WhiteUnstoppablePassedPawns;


        private int EndgameBlack => BlackMaterial + BlackEgPieceLocation + BlackEgPassedPawns + BlackEgFreePassedPawns + BlackEgKingEscortedPassedPawns + BlackUnstoppablePassedPawns;


        public StaticScore(int MiddlegamePhase)
        {
            _middlegamePhase = MiddlegamePhase;
        }


        public int TotalScore(int Phase) => GetTaperedScore(MiddlegameWhite - MiddlegameBlack, EndgameWhite - EndgameBlack, Phase);


        public void Reset()
        {
            WhiteMaterial = 0;
            WhiteMgPieceLocation = 0;
            WhiteEgPieceLocation = 0;
            WhiteMgPassedPawns = 0;
            WhiteEgPassedPawns = 0;
            WhiteEgFreePassedPawns = 0;
            WhiteEgKingEscortedPassedPawns = 0;
            WhiteUnstoppablePassedPawns = 0;
            BlackMaterial = 0;
            BlackMgPieceLocation = 0;
            BlackEgPieceLocation = 0;
            BlackMgPassedPawns = 0;
            BlackEgPassedPawns = 0;
            BlackEgFreePassedPawns = 0;
            BlackEgKingEscortedPassedPawns = 0;
            BlackUnstoppablePassedPawns = 0;
        }


        public string ToString(int Phase)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("                             |         Middlegame        |          Endgame          |           Total           |");
            stringBuilder.AppendLine("Evaluation Term              |  White    Black     Diff  |  White    Black     Diff  |  White    Black     Diff  |");
            stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");
            AppendLine(stringBuilder, "Material", WhiteMaterial, BlackMaterial, WhiteMaterial, BlackMaterial, Phase);
            AppendLine(stringBuilder, "Piece Location", WhiteMgPieceLocation, BlackMgPieceLocation, WhiteEgPieceLocation, BlackEgPieceLocation, Phase);
            AppendLine(stringBuilder, "Passed Pawns", WhiteMgPassedPawns, BlackMgPassedPawns, WhiteEgPassedPawns, BlackEgPassedPawns, Phase);
            AppendLine(stringBuilder, "Free Passed Pawns", 0, 0, WhiteEgFreePassedPawns, BlackEgFreePassedPawns, Phase);
            AppendLine(stringBuilder, "King Escorted Passed Pawns", 0, 0, WhiteEgKingEscortedPassedPawns, BlackEgKingEscortedPassedPawns, Phase);
            AppendLine(stringBuilder, "Unstoppable Passed Pawns", WhiteUnstoppablePassedPawns, BlackUnstoppablePassedPawns, WhiteUnstoppablePassedPawns, BlackUnstoppablePassedPawns, Phase);
            stringBuilder.AppendLine("=============================+===========================+===========================+===========================+");
            AppendLine(stringBuilder, "Total", MiddlegameWhite, MiddlegameBlack, EndgameWhite, EndgameBlack, Phase);
            stringBuilder.AppendLine();
            int middlegamePercent = (100 * Phase) / _middlegamePhase;
            stringBuilder.AppendLine($"Middlegame  = {Phase} of {_middlegamePhase} ({middlegamePercent}%)");
            return stringBuilder.ToString();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetTaperedScore(int MiddlegameScore, int EndgameScore, int Phase) => ((MiddlegameScore * Phase) + (EndgameScore * (_middlegamePhase - Phase))) / _middlegamePhase;


        private void AppendLine(StringBuilder StringBuilder, string EvaluationTerm, int WhiteMg, int BlackMg, int WhiteEg, int BlackEg, int Phase)
        {
            string evaluationTerm = EvaluationTerm.PadRight(27);
            string mgWhite = (WhiteMg / 100d).ToString("0.00").PadLeft(7);
            string mgBlack = (BlackMg / 100d).ToString("0.00").PadLeft(7);
            string mgDiff = ((WhiteMg - BlackMg) / 100d).ToString("0.00").PadLeft(7);
            string egWhite = (WhiteEg / 100d).ToString("0.00").PadLeft(7);
            string egBlack = (BlackEg / 100d).ToString("0.00").PadLeft(7);
            string egDiff = ((WhiteEg - BlackEg) / 100d).ToString("0.00").PadLeft(7);
            int white = GetTaperedScore(WhiteMg, WhiteEg, Phase);
            int black = GetTaperedScore(BlackMg, BlackEg, Phase);
            string totalWhite = (white / 100d).ToString("0.00").PadLeft(7);
            string totalBlack = (black / 100d).ToString("0.00").PadLeft(7);
            string totalDiff = ((white - black) / 100d).ToString("0.00").PadLeft(7);
            StringBuilder.AppendLine($"{evaluationTerm}   {mgWhite}  {mgBlack}  {mgDiff}   {egWhite}  {egBlack}  {egDiff}   {totalWhite}  {totalBlack}  {totalDiff}");
        }
    }
}
