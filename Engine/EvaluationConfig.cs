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
        // Also incentivize engine to eliminate opponent's last pawn in KQkp endgame (to trigger simple endgame scoring that pushes opposing king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - opponent pawn material - opponent pawn position).
        public static int UnstoppablePassedPawn => Evaluation.QueenMaterial - (2 * Evaluation.PawnMaterial);
        public static int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Pawn Location
        public int MgPawnAdvancement = 0;
        public int EgPawnAdvancement = 7;
        public int MgPawnCentrality = 2;
        public int EgPawnCentrality = -12;
        public int EgPawnConstant = 38;
        // Knight Location 
        public int MgKnightAdvancement = 0;
        public int EgKnightAdvancement = 20;
        public int MgKnightCentrality = 8;
        public int EgKnightCentrality = 14;
        public int MgKnightCorner = -2;
        public int EgKnightCorner = -18;
        public int EgKnightConstant = 97;
        // Bishop Location
        public int MgBishopAdvancement = 3;
        public int EgBishopAdvancement = 3;
        public int MgBishopCentrality = 5;
        public int EgBishopCentrality = 0;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = -8;
        public int EgBishopConstant = 172;
        // Rook Location
        public int MgRookAdvancement = 0;
        public int EgRookAdvancement = 15;
        public int MgRookCentrality = 3;
        public int EgRookCentrality = -5;
        public int MgRookCorner = -13;
        public int EgRookCorner = 2;
        public int EgRookConstant = 263;
        // Queen Location
        public int MgQueenAdvancement = -13;
        public int EgQueenAdvancement = 29;
        public int MgQueenCentrality = 2;
        public int EgQueenCentrality = 13;
        public int MgQueenCorner = -2;
        public int EgQueenCorner = -14;
        public int EgQueenConstant = 399;
        // King Location
        public int MgKingAdvancement = -19;
        public int EgKingAdvancement = 27;
        public int MgKingCentrality = 0;
        public int EgKingCentrality = 12;
        public int MgKingCorner = 9;
        public int EgKingCorner = -12;
        // Passed Pawns
        public int PassedPawnPowerPer16 = 33;
        public int MgPassedPawnScalePer128 = 179;
        public int EgPassedPawnScalePer128 = 527;
        public int EgFreePassedPawnScalePer128 = 1100;
        public int EgKingEscortedPassedPawn = 9;
        // Piece Mobility
        public int PieceMobilityPowerPer16 = 9;
        public int MgKnightMobilityScale = 19;
        public int EgKnightMobilityScale = 85;
        public int MgBishopMobilityScale = 27;
        public int EgBishopMobilityScale = 197;
        public int MgRookMobilityScale = 94;
        public int EgRookMobilityScale = 102;
        public int MgQueenMobilityScale = 96;
        public int EgQueenMobilityScale = 258;
        // King Safety
        public int KingSafetyPowerPer16 = 29;
        public int MgKingSafetySemiOpenFilePer8 = 62;
        public int KingSafetyMinorAttackOuterRingPer8 = 8;
        public int KingSafetyMinorAttackInnerRingPer8 = 21;
        public int KingSafetyRookAttackOuterRingPer8 = 7;
        public int KingSafetyRookAttackInnerRingPer8 = 18;
        public int KingSafetyQueenAttackOuterRingPer8 = 14;
        public int KingSafetyQueenAttackInnerRingPer8 = 33;
        public int KingSafetyScalePer128 = 43;
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
