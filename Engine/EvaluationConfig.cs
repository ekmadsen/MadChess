﻿// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
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
        // Material
        public int EgPawnMaterial = 137;
        public int MgKnightMaterial = 300;
        public int EgKnightMaterial = 407;
        public int MgBishopMaterial = 330;
        public int EgBishopMaterial = 497;
        public int MgRookMaterial = 500;
        public int EgRookMaterial = 818;
        public int MgQueenMaterial = 975;
        public int EgQueenMaterial = 1455;
        // Incentivize engine to promote pawns.
        // Also incentivize engine to eliminate enemy's last pawn in KQkp endgame (to trigger simple endgame scoring that pushes enemy king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
        public int UnstoppablePassedPawn => EgQueenMaterial - (2 * Evaluation.MgPawnMaterial); // TODO: Should this be - (2 * Evaluation.EgPawnMaterial)?
        public int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Pawn Location
        public int MgPawnAdvancement = 0;
        public int EgPawnAdvancement = 7;
        public int MgPawnCentrality = 2;
        public int EgPawnCentrality = -11;
        // Knight Location 
        public int MgKnightAdvancement = 1;
        public int EgKnightAdvancement = 22;
        public int MgKnightCentrality = 10;
        public int EgKnightCentrality = 22;
        public int MgKnightCorner = -3;
        public int EgKnightCorner = -18;
        // Bishop Location
        public int MgBishopAdvancement = 1;
        public int EgBishopAdvancement = 4;
        public int MgBishopCentrality = 7;
        public int EgBishopCentrality = 0;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = -11;
        // Rook Location
        public int MgRookAdvancement = -3;
        public int EgRookAdvancement = 14;
        public int MgRookCentrality = 3;
        public int EgRookCentrality = -2;
        public int MgRookCorner = -17;
        public int EgRookCorner = 3;
        // Queen Location
        public int MgQueenAdvancement = -16;
        public int EgQueenAdvancement = 29;
        public int MgQueenCentrality = 3;
        public int EgQueenCentrality = 11;
        public int MgQueenCorner = -3;
        public int EgQueenCorner = -13;
        // King Location
        public int MgKingAdvancement = -17;
        public int EgKingAdvancement = 23;
        public int MgKingCentrality = 0;
        public int EgKingCentrality = 19;
        public int MgKingCorner = 3;
        public int EgKingCorner = -9;
        // Passed Pawns
        public int PassedPawnPowerPer128 = 280;
        public int MgPassedPawnScalePer128 = 150;
        public int EgPassedPawnScalePer128 = 527;
        public int EgFreePassedPawnScalePer128 = 1040;
        public int EgKingEscortedPassedPawn = 10;
        // Piece Mobility
        public int PieceMobilityPowerPer128 = 76;
        public int MgKnightMobilityScale = 15;
        public int EgKnightMobilityScale = 79;
        public int MgBishopMobilityScale = 39;
        public int EgBishopMobilityScale = 192;
        public int MgRookMobilityScale = 91;
        public int EgRookMobilityScale = 121;
        public int MgQueenMobilityScale = 102;
        public int EgQueenMobilityScale = 208;
        // King Safety
        public int KingSafetyPowerPer128 = 230;
        public int MgKingSafetySemiOpenFilePer8 = 64;
        public int KingSafetyMinorAttackOuterRingPer8 = 10;
        public int KingSafetyMinorAttackInnerRingPer8 = 27;
        public int KingSafetyRookAttackOuterRingPer8 = 11;
        public int KingSafetyRookAttackInnerRingPer8 = 15;
        public int KingSafetyQueenAttackOuterRingPer8 = 17;
        public int KingSafetyQueenAttackInnerRingPer8 = 32;
        public int KingSafetyScalePer128 = 38;
        // Minor Pieces
        public int MgBishopPair = 21;
        public int EgBishopPair = 98;
        // Endgame Scaling
        public int EgBishopAdvantagePer128 = 14;
        public int EgOppBishopsPerPassedPawn = 24;
        public int EgOppBishopsPer128 = 42;
        public int EgWinningPerPawn = 7;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global


        public void Set(EvaluationConfig CopyFromConfig)
        {
            // Copy material values.
            EgPawnMaterial = CopyFromConfig.EgPawnMaterial;
            MgKnightMaterial = CopyFromConfig.MgKnightMaterial;
            EgKnightMaterial = CopyFromConfig.EgKnightMaterial;
            MgBishopMaterial = CopyFromConfig.MgBishopMaterial;
            EgBishopMaterial = CopyFromConfig.EgBishopMaterial;
            MgRookMaterial = CopyFromConfig.MgRookMaterial;
            EgRookMaterial = CopyFromConfig.EgRookMaterial;
            MgQueenMaterial = CopyFromConfig.MgQueenMaterial;
            EgQueenMaterial = CopyFromConfig.EgQueenMaterial;
            // Copy piece location values.
            MgPawnAdvancement = CopyFromConfig.MgPawnAdvancement;
            EgPawnAdvancement = CopyFromConfig.EgPawnAdvancement;
            MgPawnCentrality = CopyFromConfig.MgPawnCentrality;
            EgPawnCentrality = CopyFromConfig.EgPawnCentrality;
            MgKnightAdvancement = CopyFromConfig.MgKnightAdvancement;
            EgKnightAdvancement = CopyFromConfig.EgKnightAdvancement;
            MgKnightCentrality = CopyFromConfig.MgKnightCentrality;
            EgKnightCentrality = CopyFromConfig.EgKnightCentrality;
            MgKnightCorner = CopyFromConfig.MgKnightCorner;
            EgKnightCorner = CopyFromConfig.EgKnightCorner;
            MgBishopAdvancement = CopyFromConfig.MgBishopAdvancement;
            EgBishopAdvancement = CopyFromConfig.EgBishopAdvancement;
            MgBishopCentrality = CopyFromConfig.MgBishopCentrality;
            EgBishopCentrality = CopyFromConfig.EgBishopCentrality;
            MgBishopCorner = CopyFromConfig.MgBishopCorner;
            EgBishopCorner = CopyFromConfig.EgBishopCorner;
            MgRookAdvancement = CopyFromConfig.MgRookAdvancement;
            EgRookAdvancement = CopyFromConfig.EgRookAdvancement;
            MgRookCentrality = CopyFromConfig.MgRookCentrality;
            EgRookCentrality = CopyFromConfig.EgRookCentrality;
            MgRookCorner = CopyFromConfig.MgRookCorner;
            EgRookCorner = CopyFromConfig.EgRookCorner;
            MgQueenAdvancement = CopyFromConfig.MgQueenAdvancement;
            EgQueenAdvancement = CopyFromConfig.EgQueenAdvancement;
            MgQueenCentrality = CopyFromConfig.MgQueenCentrality;
            EgQueenCentrality = CopyFromConfig.EgQueenCentrality;
            MgQueenCorner = CopyFromConfig.MgQueenCorner;
            EgQueenCorner = CopyFromConfig.EgQueenCorner;
            MgKingAdvancement = CopyFromConfig.MgKingAdvancement;
            EgKingAdvancement = CopyFromConfig.EgKingAdvancement;
            MgKingCentrality = CopyFromConfig.MgKingCentrality;
            EgKingCentrality = CopyFromConfig.EgKingCentrality;
            MgKingCorner = CopyFromConfig.MgKingCorner;
            EgKingCorner = CopyFromConfig.EgKingCorner;
            // Copy passed pawn values.
            PassedPawnPowerPer128 = CopyFromConfig.PassedPawnPowerPer128;
            MgPassedPawnScalePer128 = CopyFromConfig.MgPassedPawnScalePer128;
            EgPassedPawnScalePer128 = CopyFromConfig.EgPassedPawnScalePer128;
            EgFreePassedPawnScalePer128 = CopyFromConfig.EgFreePassedPawnScalePer128;
            EgKingEscortedPassedPawn = CopyFromConfig.EgKingEscortedPassedPawn;
            // Copy piece mobility values.
            PieceMobilityPowerPer128 = CopyFromConfig.PieceMobilityPowerPer128;
            MgKnightMobilityScale = CopyFromConfig.MgKnightMobilityScale;
            EgKnightMobilityScale = CopyFromConfig.EgKnightMobilityScale;
            MgBishopMobilityScale = CopyFromConfig.MgBishopMobilityScale;
            EgBishopMobilityScale = CopyFromConfig.EgBishopMobilityScale;
            MgRookMobilityScale = CopyFromConfig.MgRookMobilityScale;
            EgRookMobilityScale = CopyFromConfig.EgRookMobilityScale;
            MgQueenMobilityScale = CopyFromConfig.MgQueenMobilityScale;
            EgQueenMobilityScale = CopyFromConfig.EgQueenMobilityScale;
            // Copy king safety values.
            KingSafetyPowerPer128 = CopyFromConfig.KingSafetyPowerPer128;
            MgKingSafetySemiOpenFilePer8 = CopyFromConfig.MgKingSafetySemiOpenFilePer8;
            KingSafetyMinorAttackOuterRingPer8 = CopyFromConfig.KingSafetyMinorAttackOuterRingPer8;
            KingSafetyMinorAttackInnerRingPer8 = CopyFromConfig.KingSafetyMinorAttackInnerRingPer8;
            KingSafetyRookAttackOuterRingPer8 = CopyFromConfig.KingSafetyRookAttackOuterRingPer8;
            KingSafetyRookAttackInnerRingPer8 = CopyFromConfig.KingSafetyRookAttackInnerRingPer8;
            KingSafetyQueenAttackOuterRingPer8 = CopyFromConfig.KingSafetyQueenAttackOuterRingPer8;
            KingSafetyQueenAttackInnerRingPer8 = CopyFromConfig.KingSafetyQueenAttackInnerRingPer8;
            KingSafetyScalePer128 = CopyFromConfig.KingSafetyScalePer128;
            // Copy minor values.
            MgBishopPair = CopyFromConfig.MgBishopPair;
            EgBishopPair = CopyFromConfig.EgBishopPair;
            // Copy endgame scaling values.
            EgBishopAdvantagePer128 = CopyFromConfig.EgBishopAdvantagePer128;
            EgOppBishopsPerPassedPawn = CopyFromConfig.EgOppBishopsPerPassedPawn;
            EgOppBishopsPer128 = CopyFromConfig.EgOppBishopsPer128;
            EgWinningPerPawn = CopyFromConfig.EgWinningPerPawn;
        }
    }
}
