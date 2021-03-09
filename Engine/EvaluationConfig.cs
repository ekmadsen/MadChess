// +------------------------------------------------------------------------------+
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
        public int EgPawnMaterial = 134;
        public int MgKnightMaterial = 300;
        public int EgKnightMaterial = 405;
        public int MgBishopMaterial = 330;
        public int EgBishopMaterial = 507;
        public int MgRookMaterial = 500;
        public int EgRookMaterial = 789;
        public int MgQueenMaterial = 975;
        public int EgQueenMaterial = 1442;
        // Incentivize engine to promote pawns.
        // Also incentivize engine to eliminate enemy's last pawn in KQkp endgame (to trigger simple endgame scoring that pushes enemy king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
        public int UnstoppablePassedPawn => EgQueenMaterial - (2 * Evaluation.MgPawnMaterial);
        public int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Pawn Location
        public int MgPawnAdvancement = 0;
        public int EgPawnAdvancement = 8;
        public int MgPawnCentrality = 3;
        public int EgPawnCentrality = -11;
        // Knight Location 
        public int MgKnightAdvancement = 1;
        public int EgKnightAdvancement = 21;
        public int MgKnightCentrality = 8;
        public int EgKnightCentrality = 15;
        public int MgKnightCorner = -2;
        public int EgKnightCorner = -17;
        // Bishop Location
        public int MgBishopAdvancement = 3;
        public int EgBishopAdvancement = 4;
        public int MgBishopCentrality = 8;
        public int EgBishopCentrality = 0;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = -9;
        // Rook Location
        public int MgRookAdvancement = -1;
        public int EgRookAdvancement = 14;
        public int MgRookCentrality = 2;
        public int EgRookCentrality = -4;
        public int MgRookCorner = -15;
        public int EgRookCorner = 0;
        // Queen Location
        public int MgQueenAdvancement = -14;
        public int EgQueenAdvancement = 29;
        public int MgQueenCentrality = 2;
        public int EgQueenCentrality = 13;
        public int MgQueenCorner = -2;
        public int EgQueenCorner = -14;
        // King Location
        public int MgKingAdvancement = -17;
        public int EgKingAdvancement = 26;
        public int MgKingCentrality = 0;
        public int EgKingCentrality = 18;
        public int MgKingCorner = 3;
        public int EgKingCorner = -9;
        // Passed Pawns
        public int PassedPawnPowerPer128 = 270;
        public int MgPassedPawnScalePer128 = 176;
        public int EgPassedPawnScalePer128 = 534;
        public int EgFreePassedPawnScalePer128 = 1096;
        public int EgKingEscortedPassedPawn = 10;
        // Piece Mobility
        public int PieceMobilityPowerPer128 = 72;
        public int MgKnightMobilityScale = 15;
        public int EgKnightMobilityScale = 87;
        public int MgBishopMobilityScale = 33;
        public int EgBishopMobilityScale = 177;
        public int MgRookMobilityScale = 92;
        public int EgRookMobilityScale = 121;
        public int MgQueenMobilityScale = 102;
        public int EgQueenMobilityScale = 209;
        // King Safety
        public int KingSafetyPowerPer128 = 233;
        public int MgKingSafetySemiOpenFilePer8 = 64;
        public int KingSafetyMinorAttackOuterRingPer8 = 9;
        public int KingSafetyMinorAttackInnerRingPer8 = 23;
        public int KingSafetyRookAttackOuterRingPer8 = 11;
        public int KingSafetyRookAttackInnerRingPer8 = 15;
        public int KingSafetyQueenAttackOuterRingPer8 = 16;
        public int KingSafetyQueenAttackInnerRingPer8 = 31;
        public int KingSafetyScalePer128 = 44;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global


        public void Set(EvaluationConfig CopyFromConfig)
        {
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
            MgPassedPawnScalePer128 = CopyFromConfig.MgPassedPawnScalePer128;
            EgPassedPawnScalePer128 = CopyFromConfig.EgPassedPawnScalePer128;
            EgFreePassedPawnScalePer128 = CopyFromConfig.EgFreePassedPawnScalePer128;
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
            // Copy king safety values.
            MgKingSafetySemiOpenFilePer8 = CopyFromConfig.MgKingSafetySemiOpenFilePer8;
            KingSafetyMinorAttackOuterRingPer8 = CopyFromConfig.KingSafetyMinorAttackOuterRingPer8;
            KingSafetyMinorAttackInnerRingPer8 = CopyFromConfig.KingSafetyMinorAttackInnerRingPer8;
            KingSafetyRookAttackOuterRingPer8 = CopyFromConfig.KingSafetyRookAttackOuterRingPer8;
            KingSafetyRookAttackInnerRingPer8 = CopyFromConfig.KingSafetyRookAttackInnerRingPer8;
            KingSafetyQueenAttackOuterRingPer8 = CopyFromConfig.KingSafetyQueenAttackOuterRingPer8;
            KingSafetyQueenAttackInnerRingPer8 = CopyFromConfig.KingSafetyQueenAttackInnerRingPer8;
            KingSafetyScalePer128 = CopyFromConfig.KingSafetyScalePer128;
        }
    }
}
