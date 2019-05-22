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
        public int BishopMaterial = 330;
        public int RookMaterial = 500;
        public int QueenMaterial = 975;
        public int SimpleEndgame => UnstoppablePassedPawn;  // Incentivize engine to promote pawn in king and pawn endgames.
        // Pawn Location
        public int MgPawnAdvancement = 2;
        public int EgPawnAdvancement = 0;
        public int MgPawnCentrality = 9;
        public int EgPawnCentrality = 0;
        public int EgPawnConstant = 15;
        // Knight Location 
        public int MgKnightAdvancement = 11;
        public int EgKnightAdvancement = 4;
        public int MgKnightCentrality = 3;
        public int EgKnightCentrality = 5;
        public int MgKnightCorner = -2;
        public int EgKnightCorner = -3;
        public int EgKnightConstant = 30;
        // Bishop Location
        public int MgBishopAdvancement = 7;
        public int EgBishopAdvancement = 7;
        public int MgBishopCentrality = 3;
        public int EgBishopCentrality = 0;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = 0;
        public int EgBishopConstant = 69;
        // Rook Location
        public int MgRookAdvancement = 5;
        public int EgRookAdvancement = 16;
        public int MgRookCentrality = 2;
        public int EgRookCentrality = 0;
        public int MgRookCorner = -9;
        public int EgRookCorner = -1;
        public int EgRookConstant = 100;
        // Queen Location
        public int MgQueenAdvancement = 3;
        public int EgQueenAdvancement = 25;
        public int MgQueenCentrality = 1;
        public int EgQueenCentrality = 4;
        public int MgQueenCorner = -5;
        public int EgQueenCorner = 0;
        public int EgQueenConstant = 100;
        // King Location
        public int MgKingAdvancement = -2;
        public int EgKingAdvancement = 16;
        public int MgKingCentrality = -24;
        public int EgKingCentrality = 14;
        public int MgKingCorner = 23;
        public int EgKingCorner = -13;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 167;
        public int EgPassedPawnScalePercent = 543;
        public int EgFreePassedPawnScalePercent = 951;
        public int EgKingEscortedPassedPawn = 2;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        // Piece Mobility
        public int MgKnightMobilityScale = 3;
        public int EgKnightMobilityScale = 48;
        public int MgBishopMobilityScale = 25;
        public int EgBishopMobilityScale = 15;
        public int MgRookMobilityScale = 46;
        public int EgRookMobilityScale = 6;
        public int MgQueenMobilityScale = 2;
        public int EgQueenMobilityScale = 46;
        // ReSharper restore RedundantDefaultMemberInitializer
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