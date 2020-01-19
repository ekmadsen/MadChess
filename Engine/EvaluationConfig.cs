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
        public int BishopMaterial = 300;
        public int RookMaterial = 500;
        public int QueenMaterial = 975;
        public int SimpleEndgame => UnstoppablePassedPawn;  // Incentivize engine to promote pawn in king and pawn endgames.
        // Pawn Location
        public int MgPawnAdvancement = 2;
        public int EgPawnAdvancement = 5;
        public int MgPawnCentrality = 2;
        public int EgPawnCentrality = 0;
        public int EgPawnConstant = 11;
        // Knight Location 
        public int MgKnightAdvancement = 11;
        public int EgKnightAdvancement = 22;
        public int MgKnightCentrality = 8;
        public int EgKnightCentrality = 25;
        public int MgKnightCorner = 0;
        public int EgKnightCorner = -22;
        public int EgKnightConstant = 62;
        // Bishop Location
        public int MgBishopAdvancement = 9;
        public int EgBishopAdvancement = 9;
        public int MgBishopCentrality = 6;
        public int EgBishopCentrality = 24;
        public int MgBishopCorner = 3;
        public int EgBishopCorner = 0;
        public int EgBishopConstant = 66;
        // Rook Location
        public int MgRookAdvancement = 10;
        public int EgRookAdvancement = 16;
        public int MgRookCentrality = 11;
        public int EgRookCentrality = 0;
        public int MgRookCorner = -8;
        public int EgRookCorner = 0;
        public int EgRookConstant = 189;
        // Queen Location
        public int MgQueenAdvancement = 2;
        public int EgQueenAdvancement = 25;
        public int MgQueenCentrality = 3;
        public int EgQueenCentrality = 19;
        public int MgQueenCorner = 0;
        public int EgQueenCorner = -8;
        public int EgQueenConstant = 130;
        // King Location
        public int MgKingAdvancement = -4;
        public int EgKingAdvancement = 3;
        public int MgKingCentrality = -2;
        public int EgKingCentrality = 16;
        public int MgKingCorner = 0;
        public int EgKingCorner = -21;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 97;
        public int EgPassedPawnScalePercent = 473;
        public int EgFreePassedPawnScalePercent = 866;
        public int EgKingEscortedPassedPawn = 11;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        // Piece Location
        public int MgKnightMobilityScale = 5;
        public int EgKnightMobilityScale = 5;
        public int MgBishopMobilityScale = 15;
        public int EgBishopMobilityScale = 100;
        public int MgRookMobilityScale = 15;
        public int EgRookMobilityScale = 80;
        public int MgQueenMobilityScale = 25;
        public int EgQueenMobilityScale = 150;
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