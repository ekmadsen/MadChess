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
        public int EgKnightMaterial = 400;
        public int MgBishopMaterial = 330;
        public int EgBishopMaterial = 500;
        public int MgRookMaterial = 500;
        public int EgRookMaterial = 750;
        public int MgQueenMaterial = 975;
        public int EgQueenMaterial = 1375;
        // Incentivize engine to promote pawns.
        // Also incentivize engine to eliminate enemy's last pawn in KQkp endgame (to trigger simple endgame scoring that pushes enemy king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
        public int UnstoppablePassedPawn => EgQueenMaterial - (2 * Evaluation.MgPawnMaterial);
        public int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Pawn Location
        public int MgPawnAdvancement = 0;
        public int EgPawnAdvancement = 7;
        public int MgPawnCentrality = 2;
        public int EgPawnCentrality = -9;
        // Knight Location 
        public int MgKnightAdvancement = 1;
        public int EgKnightAdvancement = 20;
        public int MgKnightCentrality = 7;
        public int EgKnightCentrality = 13;
        public int MgKnightCorner = -2;
        public int EgKnightCorner = -19;
        // Bishop Location
        public int MgBishopAdvancement = 6;
        public int EgBishopAdvancement = 0;
        public int MgBishopCentrality = 4;
        public int EgBishopCentrality = 1;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = -9;
        // Rook Location
        public int MgRookAdvancement = 1;
        public int EgRookAdvancement = 18;
        public int MgRookCentrality = 3;
        public int EgRookCentrality = -3;
        public int MgRookCorner = -12;
        public int EgRookCorner = 2;
        // Queen Location
        public int MgQueenAdvancement = -14;
        public int EgQueenAdvancement = 29;
        public int MgQueenCentrality = 4;
        public int EgQueenCentrality = 11;
        public int MgQueenCorner = -2;
        public int EgQueenCorner = -13;
        // King Location
        public int MgKingAdvancement = -18;
        public int EgKingAdvancement = 25;
        public int MgKingCentrality = 0;
        public int EgKingCentrality = 17;
        public int MgKingCorner = 9;
        public int EgKingCorner = -13;
        // Passed Pawns
        public int MgPassedPawnScalePer128 = 184;
        public int EgPassedPawnScalePer128 = 550;
        public int EgFreePassedPawnScalePer128 = 1013;
        public int EgKingEscortedPassedPawn = 9;
        // Piece Mobility
        public int MgKnightMobilityScale = 23;
        public int EgKnightMobilityScale = 59;
        public int MgBishopMobilityScale = 28;
        public int EgBishopMobilityScale = 133;
        public int MgRookMobilityScale = 70;
        public int EgRookMobilityScale = 80;
        public int MgQueenMobilityScale = 100;
        public int EgQueenMobilityScale = 223;
        // King Safety
        public int MgKingSafetySemiOpenFilePer8 = 60;
        public int KingSafetyMinorAttackOuterRingPer8 = 8;
        public int KingSafetyMinorAttackInnerRingPer8 = 25;
        public int KingSafetyRookAttackOuterRingPer8 = 2;
        public int KingSafetyRookAttackInnerRingPer8 = 17;
        public int KingSafetyQueenAttackOuterRingPer8 = 12;
        public int KingSafetyQueenAttackInnerRingPer8 = 33;
        public int KingSafetyScalePer128 = 53;
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
