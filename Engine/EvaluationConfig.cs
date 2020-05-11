// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class EvaluationConfig
    {
        // ReSharper disable ConvertToConstant.Global
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        // ReSharper disable RedundantDefaultMemberInitializer
        // Material and Simple Endgame
        public int KnightMaterial = 300;
        public int KnightExchangeMaterial = 300;
        public int BishopMaterial = 330;
        public int BishopExchangeMaterial = 300;
        public int RookMaterial = 500;
        public int RookExchangeMaterial = 500;
        public int QueenMaterial = 975;
        public int QueenExchangeMaterial = 900;
        public int SimpleEndgame => UnstoppablePassedPawn;  // Incentivize engine to promote pawn in king and pawn endgames.
        // Pawn Location
        public int MgPawnAdvancement = 2;
        public int EgPawnAdvancement = 4;
        public int MgPawnCentrality = 2;
        public int EgPawnCentrality = -10;
        public int EgPawnConstant = 32;
        // Knight Location 
        public int MgKnightAdvancement = 4;
        public int EgKnightAdvancement = 22;
        public int MgKnightCentrality = 6;
        public int EgKnightCentrality = 23;
        public int MgKnightCorner = 0;
        public int EgKnightCorner = -23;
        public int EgKnightConstant = -5;
        // Bishop Location
        public int MgBishopAdvancement = 5;
        public int EgBishopAdvancement = 6;
        public int MgBishopCentrality = 6;
        public int EgBishopCentrality = 3;
        public int MgBishopCorner = 4;
        public int EgBishopCorner = -11;
        public int EgBishopConstant = 64;
        // Rook Location
        public int MgRookAdvancement = 1;
        public int EgRookAdvancement = 17;
        public int MgRookCentrality = 0;
        public int EgRookCentrality = 0;
        public int MgRookCorner = -17;
        public int EgRookCorner = 1;
        public int EgRookConstant = 100;
        // Queen Location
        public int MgQueenAdvancement = -5;
        public int EgQueenAdvancement = 25;
        public int MgQueenCentrality = -3;
        public int EgQueenCentrality = 19;
        public int MgQueenCorner = -2;
        public int EgQueenCorner = 3;
        public int EgQueenConstant = 100;
        // King Location
        public int MgKingAdvancement = -30;
        public int EgKingAdvancement = 30;
        public int MgKingCentrality = -4;
        public int EgKingCentrality = 5;
        public int MgKingCorner = 13;
        public int EgKingCorner = 0;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 88;
        public int EgPassedPawnScalePercent = 485;
        public int EgFreePassedPawnScalePercent = 886;
        public int EgKingEscortedPassedPawn = 8;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        // Piece Mobility
        public int MgKnightMobilityScale = 17;
        public int EgKnightMobilityScale = 80;
        public int MgBishopMobilityScale = 36;
        public int EgBishopMobilityScale = 221;
        public int MgRookMobilityScale = 71;
        public int EgRookMobilityScale = 181;
        public int MgQueenMobilityScale = 84;
        public int EgQueenMobilityScale = 357;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global


        public void Set(EvaluationConfig CopyFromConfig)
        {
            // Copy piece location values.
            KnightMaterial = CopyFromConfig.KnightMaterial;
            BishopMaterial = CopyFromConfig.BishopMaterial;
            RookMaterial = CopyFromConfig.RookMaterial;
            QueenMaterial = CopyFromConfig.QueenMaterial;
            MgPawnAdvancement = CopyFromConfig.MgPawnAdvancement;
            EgPawnAdvancement = CopyFromConfig.EgPawnAdvancement;
            MgPawnCentrality = CopyFromConfig.MgPawnCentrality;
            EgPawnCentrality = CopyFromConfig.EgPawnCentrality;
            EgPawnConstant = CopyFromConfig.EgPawnConstant;
            MgKnightAdvancement = CopyFromConfig.MgKnightAdvancement;
            EgKnightAdvancement = CopyFromConfig.EgKnightAdvancement;
            MgKnightCentrality = CopyFromConfig.MgKnightCentrality;
            EgKnightCentrality = CopyFromConfig.EgKnightCentrality;
            MgKnightCorner = CopyFromConfig.MgKnightCorner;
            EgKnightCorner = CopyFromConfig.EgKnightCorner;
            EgKnightConstant = CopyFromConfig.EgKnightConstant;
            MgBishopAdvancement = CopyFromConfig.MgBishopAdvancement;
            EgBishopAdvancement = CopyFromConfig.EgBishopAdvancement;
            MgBishopCentrality = CopyFromConfig.MgBishopCentrality;
            EgBishopCentrality = CopyFromConfig.EgBishopCentrality;
            MgBishopCorner = CopyFromConfig.MgBishopCorner;
            EgBishopCorner = CopyFromConfig.EgBishopCorner;
            EgBishopConstant = CopyFromConfig.EgBishopConstant;
            MgRookAdvancement = CopyFromConfig.MgRookAdvancement;
            EgRookAdvancement = CopyFromConfig.EgRookAdvancement;
            MgRookCentrality = CopyFromConfig.MgRookCentrality;
            EgRookCentrality = CopyFromConfig.EgRookCentrality;
            MgRookCorner = CopyFromConfig.MgRookCorner;
            EgRookCorner = CopyFromConfig.EgRookCorner;
            EgRookConstant = CopyFromConfig.EgRookConstant;
            MgQueenAdvancement = CopyFromConfig.MgQueenAdvancement;
            EgQueenAdvancement = CopyFromConfig.EgQueenAdvancement;
            MgQueenCentrality = CopyFromConfig.MgQueenCentrality;
            EgQueenCentrality = CopyFromConfig.EgQueenCentrality;
            MgQueenCorner = CopyFromConfig.MgQueenCorner;
            EgQueenCorner = CopyFromConfig.EgQueenCorner;
            EgQueenConstant = CopyFromConfig.EgQueenConstant;
            MgKingAdvancement = CopyFromConfig.MgKingAdvancement;
            EgKingAdvancement = CopyFromConfig.EgKingAdvancement;
            MgKingCentrality = CopyFromConfig.MgKingCentrality;
            EgKingCentrality = CopyFromConfig.EgKingCentrality;
            MgKingCorner = CopyFromConfig.MgKingCorner;
            EgKingCorner = CopyFromConfig.EgKingCorner;
            // Copy passed pawn values.
            MgPassedPawnScalePercent = CopyFromConfig.MgPassedPawnScalePercent;
            EgPassedPawnScalePercent = CopyFromConfig.EgPassedPawnScalePercent;
            EgFreePassedPawnScalePercent = CopyFromConfig.EgFreePassedPawnScalePercent;
            EgKingEscortedPassedPawn = CopyFromConfig.EgKingEscortedPassedPawn;
            // Copy piece mobility values.
            MgKnightMobilityScale = CopyFromConfig.MgKnightMobilityScale;
            EgKnightMobilityScale = CopyFromConfig.EgKnightMobilityScale;
            MgBishopMobilityScale = CopyFromConfig.MgBishopMobilityScale;
            EgBishopMobilityScale = CopyFromConfig.EgBishopMobilityScale;
            MgRookMobilityScale = CopyFromConfig.MgRookMobilityScale;
            EgRookMobilityScale = CopyFromConfig.EgRookMobilityScale;
            MgQueenMobilityScale = CopyFromConfig.MgQueenMobilityScale;
            EgQueenMobilityScale = CopyFromConfig.EgQueenMobilityScale;
        }
    }
}
