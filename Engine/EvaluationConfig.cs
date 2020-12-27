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

        // Game Phase
        // Select phase constants such that starting material = 256.
        public const int KnightPhase = 10; //   4 * 10 =  40
        public const int BishopPhase = 10; // + 4 * 10 =  80
        public const int RookPhase = 22; //   + 4 * 22 = 168
        public const int QueenPhase = 44; //  + 2 * 44 = 256
        public const int MiddlegamePhase = 4 * (KnightPhase + BishopPhase + RookPhase) + 2 * QueenPhase;
        // Incentivize engine to promote pawns.
        // Also incentivize engine to eliminate enemy's last pawn in KQkp endgame (to trigger simple endgame scoring that pushes enemy king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
        public static int UnstoppablePassedPawn => Evaluation.QueenMaterial - (2 * Evaluation.PawnMaterial);
        public static int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Pawn Location
        public int MgPawnAdvancement = 0;
        public int EgPawnAdvancement = 6;
        public int MgPawnCentrality = 1;
        public int EgPawnCentrality = -12;
        public int EgPawnConstant = 38;
        // Knight Location 
        public int MgKnightAdvancement = 0;
        public int EgKnightAdvancement = 20;
        public int MgKnightCentrality = 5;
        public int EgKnightCentrality = 12;
        public int MgKnightCorner = -2;
        public int EgKnightCorner = -19;
        public int EgKnightConstant = 100;
        // Bishop Location
        public int MgBishopAdvancement = 3;
        public int EgBishopAdvancement = 3;
        public int MgBishopCentrality = 7;
        public int EgBishopCentrality = 0;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = -10;
        public int EgBishopConstant = 179;
        // Rook Location
        public int MgRookAdvancement = 0;
        public int EgRookAdvancement = 15;
        public int MgRookCentrality = 4;
        public int EgRookCentrality = -2;
        public int MgRookCorner = -15;
        public int EgRookCorner = 1;
        public int EgRookConstant = 272;
        // Queen Location
        public int MgQueenAdvancement = -11;
        public int EgQueenAdvancement = 30;
        public int MgQueenCentrality = 1;
        public int EgQueenCentrality = 12;
        public int MgQueenCorner = 0;
        public int EgQueenCorner = -13;
        public int EgQueenConstant = 398;
        // King Location
        public int MgKingAdvancement = -22;
        public int EgKingAdvancement = 29;
        public int MgKingCentrality = 0;
        public int EgKingCentrality = 12;
        public int MgKingCorner = 7;
        public int EgKingCorner = -12;
        // Passed Pawns
        public int PassedPawnPowerPer16 = 35;
        public int MgPassedPawnScalePer128 = 173;
        public int EgPassedPawnScalePer128 = 415;
        public int EgFreePassedPawnScalePer128 = 1030;
        public int EgKingEscortedPassedPawn = 9;
        // Piece Mobility
        public int PieceMobilityPowerPer16 = 10;
        public int MgKnightMobilityScale = 54;
        public int EgKnightMobilityScale = 176;
        public int MgBishopMobilityScale = 59;
        public int EgBishopMobilityScale = 140;
        public int MgRookMobilityScale = 115;
        public int EgRookMobilityScale = 103;
        public int MgQueenMobilityScale = 92;
        public int EgQueenMobilityScale = 333;
        // King Safety
        public int KingSafetyPowerPer16 = 28;
        public int MgKingSafetySemiOpenFilePer8 = 58;
        public int KingSafetyMinorAttackOuterRingPer8 = 17;
        public int KingSafetyMinorAttackInnerRingPer8 = 25;
        public int KingSafetyRookAttackOuterRingPer8 = 10;
        public int KingSafetyRookAttackInnerRingPer8 = 19;
        public int KingSafetyQueenAttackOuterRingPer8 = 23;
        public int KingSafetyQueenAttackInnerRingPer8 = 27;
        public int KingSafetyScalePer128 = 41;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global


        public void Set(EvaluationConfig CopyFromConfig)
        {
            // Copy piece location values.
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
            PassedPawnPowerPer16 = CopyFromConfig.PassedPawnPowerPer16;
            MgPassedPawnScalePer128 = CopyFromConfig.MgPassedPawnScalePer128;
            EgPassedPawnScalePer128 = CopyFromConfig.EgPassedPawnScalePer128;
            EgFreePassedPawnScalePer128 = CopyFromConfig.EgFreePassedPawnScalePer128;
            EgKingEscortedPassedPawn = CopyFromConfig.EgKingEscortedPassedPawn;
            // Copy piece mobility values.
            PieceMobilityPowerPer16 = CopyFromConfig.PieceMobilityPowerPer16;
            MgKnightMobilityScale = CopyFromConfig.MgKnightMobilityScale;
            EgKnightMobilityScale = CopyFromConfig.EgKnightMobilityScale;
            MgBishopMobilityScale = CopyFromConfig.MgBishopMobilityScale;
            EgBishopMobilityScale = CopyFromConfig.EgBishopMobilityScale;
            MgRookMobilityScale = CopyFromConfig.MgRookMobilityScale;
            EgRookMobilityScale = CopyFromConfig.EgRookMobilityScale;
            MgQueenMobilityScale = CopyFromConfig.MgQueenMobilityScale;
            EgQueenMobilityScale = CopyFromConfig.EgQueenMobilityScale;
            // Copy king safety values.
            KingSafetyPowerPer16 = CopyFromConfig.KingSafetyPowerPer16;
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
