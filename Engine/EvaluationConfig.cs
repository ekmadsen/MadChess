﻿// +------------------------------------------------------------------------------+
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
        public int MgPawnAdvancement = 3;
        public int EgPawnAdvancement = 0;
        public int MgPawnCentrality = 0;
        public int EgPawnCentrality = 1;
        public int EgPawnConstant = 33;
        // Knight Location 
        public int MgKnightAdvancement = 5;
        public int EgKnightAdvancement = 19;
        public int MgKnightCentrality = 12;
        public int EgKnightCentrality = 16;
        public int MgKnightCorner = -4;
        public int EgKnightCorner = -6;
        public int EgKnightConstant = 17;
        // Bishop Location
        public int MgBishopAdvancement = 6;
        public int EgBishopAdvancement = 2;
        public int MgBishopCentrality = 7;
        public int EgBishopCentrality = 12;
        public int MgBishopCorner = 4;
        public int EgBishopCorner = 1;
        public int EgBishopConstant = 36;
        // Rook Location
        public int MgRookAdvancement = 2;
        public int EgRookAdvancement = 16;
        public int MgRookCentrality = 0;
        public int EgRookCentrality = 8;
        public int MgRookCorner = -13;
        public int EgRookCorner = -3;
        public int EgRookConstant = 50;
        // Queen Location
        public int MgQueenAdvancement = 1;
        public int EgQueenAdvancement = 20;
        public int MgQueenCentrality = 7;
        public int EgQueenCentrality = 8;
        public int MgQueenCorner = -4;
        public int EgQueenCorner = -1;
        public int EgQueenConstant = 91;
        // King Location
        public int MgKingAdvancement = -19;
        public int EgKingAdvancement = 13;
        public int MgKingCentrality = -3;
        public int EgKingCentrality = 17;
        public int MgKingCorner = 19;
        public int EgKingCorner = -2;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 72;
        public int EgPassedPawnScalePercent = 437;
        public int EgFreePassedPawnScalePercent = 789;
        public int EgKingEscortedPassedPawn = 10;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        public int MgKnightMobilityScale = 31;
        public int EgKnightMobilityScale = 25;
        public int MgBishopMobilityScale = 45;
        public int EgBishopMobilityScale = 113;
        public int MgRookMobilityScale = 14;
        public int EgRookMobilityScale = 106;
        public int MgQueenMobilityScale = 76;
        public int EgQueenMobilityScale = 44;
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