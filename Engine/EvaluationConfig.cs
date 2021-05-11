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
        public int MgKnightMaterial = 300;
        public int EgKnightMaterial = 443;
        public int MgBishopMaterial = 330;
        public int EgBishopMaterial = 468;
        public int MgRookMaterial = 500;
        public int EgRookMaterial = 741;
        public int MgQueenMaterial = 975;
        public int EgQueenMaterial = 1356;
        // Incentivize engine to promote pawns.
        // Also incentivize engine to eliminate enemy's last pawn in K vrs KQ or KR endgames (to trigger simple endgame scoring that pushes enemy king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
        public int UnstoppablePassedPawn => EgQueenMaterial - (2 * Evaluation.PawnMaterial);
        public int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Pawn Location
        public int MgPawnAdvancement = 3;
        public int EgPawnAdvancement = 5;
        public int MgPawnCentrality = 1;
        public int EgPawnCentrality = -6;
        // Knight Location 
        public int MgKnightAdvancement = 3;
        public int EgKnightAdvancement = 8;
        public int MgKnightCentrality = 12;
        public int EgKnightCentrality = 19;
        public int MgKnightCorner = 0;
        public int EgKnightCorner = -20;
        // Bishop Location
        public int MgBishopAdvancement = -3;
        public int EgBishopAdvancement = 2;
        public int MgBishopCentrality = 16;
        public int EgBishopCentrality = 4;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = -1;
        // Rook Location
        public int MgRookAdvancement = 3;
        public int EgRookAdvancement = 12;
        public int MgRookCentrality = 4;
        public int EgRookCentrality = -3;
        public int MgRookCorner = -10;
        public int EgRookCorner = 3;
        // Queen Location
        public int MgQueenAdvancement = -19;
        public int EgQueenAdvancement = 22;
        public int MgQueenCentrality = 0;
        public int EgQueenCentrality = 5;
        public int MgQueenCorner = 0;
        public int EgQueenCorner = -9;
        // King Location
        public int MgKingAdvancement = -9;
        public int EgKingAdvancement = 16;
        public int MgKingCentrality = -2;
        public int EgKingCentrality = 20;
        public int MgKingCorner = 4;
        public int EgKingCorner = -1;
        // Passed Pawns
        public int PassedPawnPowerPer128 = 301;
        public int MgPassedPawnScalePer128 = 126;
        public int EgPassedPawnScalePer128 = 262;
        public int EgFreePassedPawnScalePer128 = 565;
        public int EgKingEscortedPassedPawn = 11;
        // Piece Mobility
        public int PieceMobilityPowerPer128 = 82;
        public int MgKnightMobilityScale = 4;
        public int EgKnightMobilityScale = 11;
        public int MgBishopMobilityScale = 1;
        public int EgBishopMobilityScale = 158;
        public int MgRookMobilityScale = 58;
        public int EgRookMobilityScale = 77;
        public int MgQueenMobilityScale = 74;
        public int EgQueenMobilityScale = 147;
        // King Safety
        public int KingSafetyPowerPer128 = 227;
        public int MgKingSafetySemiOpenFilePer8 = 64;
        public int KingSafetyMinorAttackOuterRingPer8 = 7;
        public int KingSafetyMinorAttackInnerRingPer8 = 24;
        public int KingSafetyRookAttackOuterRingPer8 = 10;
        public int KingSafetyRookAttackInnerRingPer8 = 16;
        public int KingSafetyQueenAttackOuterRingPer8 = 17;
        public int KingSafetyQueenAttackInnerRingPer8 = 24;
        public int KingSafetyScalePer128 = 57;
        // Minor Pieces
        public int MgBishopPair = 37;
        public int EgBishopPair = 80;
        // Endgame Scaling
        public int EgBishopAdvantagePer128 = 16;
        public int EgOppBishopsPerPassedPawn = 23;
        public int EgOppBishopsPer128 = 28;
        public int EgWinningPerPawn = 27;
        // Limit Strength
        public bool LimitedStrength = false;
        public int LsPieceLocationPer128 = 128;
        public int LsPassedPawnsPer128 = 128;
        public int LsPieceMobilityPer128 = 128;
        public int LsKingSafetyPer128 = 128;
        public int LsMinorPiecesPer128 = 128;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global


        public void Set(EvaluationConfig CopyFromConfig)
        {
            // Copy material values.
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
            // Copy limit strength values.
            LimitedStrength = CopyFromConfig.LimitedStrength;
            LsPieceLocationPer128 = CopyFromConfig.LsPieceLocationPer128;
            LsPassedPawnsPer128 = CopyFromConfig.PassedPawnPowerPer128;
            LsPieceMobilityPer128 = CopyFromConfig.LsPieceMobilityPer128;
            LsKingSafetyPer128 = CopyFromConfig.LsKingSafetyPer128;
            LsMinorPiecesPer128 = CopyFromConfig.LsMinorPiecesPer128;
        }
    }
}
