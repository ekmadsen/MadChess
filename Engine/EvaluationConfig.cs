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
        public int EgPawnMaterial = 140;
        public int MgKnightMaterial = 300;
        public int EgKnightMaterial = 300;
        public int MgBishopMaterial = 330;
        public int EgBishopMaterial = 380;
        public int MgRookMaterial = 500;
        public int EgRookMaterial = 600;
        public int MgQueenMaterial = 975;
        public int EgQueenMaterial = 1100;
        // Incentivize engine to promote pawns.
        // Also incentivize engine to eliminate enemy's last pawn in KQkp endgame (to trigger simple endgame scoring that pushes enemy king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
        public int UnstoppablePassedPawn => EgQueenMaterial - (2 * Evaluation.MgPawnMaterial);
        public int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Pawn Location
        public int MgPawnAdvancement = 2;
        public int EgPawnAdvancement = 5;
        public int MgPawnCentrality = 1;
        public int EgPawnCentrality = -13;
        // Knight Location 
        public int MgKnightAdvancement = 0;
        public int EgKnightAdvancement = 18;
        public int MgKnightCentrality = 8;
        public int EgKnightCentrality = 22;
        public int MgKnightCorner = -3;
        public int EgKnightCorner = -14;
        // Bishop Location
        public int MgBishopAdvancement = 3;
        public int EgBishopAdvancement = 6;
        public int MgBishopCentrality = 7;
        public int EgBishopCentrality = 6;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = -5;
        // Rook Location
        public int MgRookAdvancement = 1;
        public int EgRookAdvancement = 15;
        public int MgRookCentrality = 6;
        public int EgRookCentrality = 0;
        public int MgRookCorner = -13;
        public int EgRookCorner = 7;
        // Queen Location
        public int MgQueenAdvancement = -13;
        public int EgQueenAdvancement = 30;
        public int MgQueenCentrality = 6;
        public int EgQueenCentrality = 10;
        public int MgQueenCorner = -2;
        public int EgQueenCorner = -5;
        // King Location
        public int MgKingAdvancement = -11;
        public int EgKingAdvancement = 22;
        public int MgKingCentrality = 0;
        public int EgKingCentrality = 23;
        public int MgKingCorner = 5;
        public int EgKingCorner = -11;
        // Passed Pawns
        public int MgPassedPawnScalePer128 = 156;
        public int EgPassedPawnScalePer128 = 507;
        public int EgFreePassedPawnScalePer128 = 1179;
        public int EgKingEscortedPassedPawn = 5;
        // Piece Mobility
        public int MgKnightMobilityScale = 30;
        public int EgKnightMobilityScale = 53;
        public int MgBishopMobilityScale = 39;
        public int EgBishopMobilityScale = 163;
        public int MgRookMobilityScale = 80;
        public int EgRookMobilityScale = 145;
        public int MgQueenMobilityScale = 96;
        public int EgQueenMobilityScale = 293;
        // King Safety
        public int MgKingSafetySemiOpenFilePer8 = 40;
        public int KingSafetyMinorAttackOuterRingPer8 = 7;
        public int KingSafetyMinorAttackInnerRingPer8 = 16;
        public int KingSafetyRookAttackOuterRingPer8 = 7;
        public int KingSafetyRookAttackInnerRingPer8 = 11;
        public int KingSafetyQueenAttackOuterRingPer8 = 9;
        public int KingSafetyQueenAttackInnerRingPer8 = 19;
        public int KingSafetyScalePer128 = 57;
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
