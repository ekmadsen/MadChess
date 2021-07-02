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
        public int EgKnightMaterial = 454;
        public int MgBishopMaterial = 330;
        public int EgBishopMaterial = 470;
        public int MgRookMaterial = 500;
        public int EgRookMaterial = 764;
        public int MgQueenMaterial = 975;
        public int EgQueenMaterial = 1384;
        // Incentivize engine to promote pawns.
        // Also incentivize engine to eliminate enemy's last pawn in K vrs KQ or KR endgames (to trigger simple endgame scoring that pushes enemy king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
        public int UnstoppablePassedPawn => EgQueenMaterial - (2 * Evaluation.PawnMaterial);
        public int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Pawn Location
        public int MgPawnAdvancement = 4;
        public int EgPawnAdvancement = 5;
        public int MgPawnCentrality = 0;
        public int EgPawnCentrality = -6;
        // Knight Location 
        public int MgKnightAdvancement = 3;
        public int EgKnightAdvancement = 7;
        public int MgKnightCentrality = 13;
        public int EgKnightCentrality = 19;
        public int MgKnightCorner = 0;
        public int EgKnightCorner = -24;
        // Bishop Location
        public int MgBishopAdvancement = -4;
        public int EgBishopAdvancement = 3;
        public int MgBishopCentrality = 17;
        public int EgBishopCentrality = 5;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = 0;
        // Rook Location
        public int MgRookAdvancement = 3;
        public int EgRookAdvancement = 13;
        public int MgRookCentrality = 0;
        public int EgRookCentrality = -5;
        public int MgRookCorner = -11;
        public int EgRookCorner = 0;
        // Queen Location
        public int MgQueenAdvancement = -22;
        public int EgQueenAdvancement = 25;
        public int MgQueenCentrality = 2;
        public int EgQueenCentrality = 7;
        public int MgQueenCorner = 0;
        public int EgQueenCorner = -6;
        // King Location
        public int MgKingAdvancement = -1;
        public int EgKingAdvancement = 12;
        public int MgKingCentrality = 0;
        public int EgKingCentrality = 21;
        public int MgKingCorner = 2;
        public int EgKingCorner = -2;
        // Passed Pawns
        public int PassedPawnPowerPer128 = 307;
        public int MgPassedPawnScalePer128 = 119;
        public int EgPassedPawnScalePer128 = 257;
        public int EgFreePassedPawnScalePer128 = 547;
        public int EgKingEscortedPassedPawn = 12;
        // Piece Mobility
        public int PieceMobilityPowerPer128 = 82;
        public int MgKnightMobilityScale = 1;
        public int EgKnightMobilityScale = 6;
        public int MgBishopMobilityScale = 2;
        public int EgBishopMobilityScale = 161;
        public int MgRookMobilityScale = 44;
        public int EgRookMobilityScale = 85;
        public int MgQueenMobilityScale = 72;
        public int EgQueenMobilityScale = 159;
        // King Safety
        public int MgKingSafetyPowerPer128 = 244;
        public int MgKingSafetyScalePer128 = 84;
        public int MgKingSafetyMinorAttackOuterRingPer8 = 8;
        public int MgKingSafetyMinorAttackInnerRingPer8 = 31;
        public int MgKingSafetyRookAttackOuterRingPer8 = 12;
        public int MgKingSafetyRookAttackInnerRingPer8 = 20;
        public int MgKingSafetyQueenAttackOuterRingPer8 = 14;
        public int MgKingSafetyQueenAttackInnerRingPer8 = 25;
        public int MgKingSafetySemiOpenFilePer8 = 30;
        public int MgKingSafetyPawnShieldPer8 = 8;
        // Minor Pieces
        public int MgBishopPair = 42;
        public int EgBishopPair = 85;
        // Endgame Scaling
        public int EgBishopAdvantagePer128 = 19;
        public int EgOppBishopsPerPassedPawn = 25;
        public int EgOppBishopsPer128 = 22;
        public int EgWinningPerPawn = 29;
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
            MgKingSafetyPowerPer128 = CopyFromConfig.MgKingSafetyPowerPer128;
            MgKingSafetyScalePer128 = CopyFromConfig.MgKingSafetyScalePer128;
            MgKingSafetyMinorAttackOuterRingPer8 = CopyFromConfig.MgKingSafetyMinorAttackOuterRingPer8;
            MgKingSafetyMinorAttackInnerRingPer8 = CopyFromConfig.MgKingSafetyMinorAttackInnerRingPer8;
            MgKingSafetyRookAttackOuterRingPer8 = CopyFromConfig.MgKingSafetyRookAttackOuterRingPer8;
            MgKingSafetyRookAttackInnerRingPer8 = CopyFromConfig.MgKingSafetyRookAttackInnerRingPer8;
            MgKingSafetyQueenAttackOuterRingPer8 = CopyFromConfig.MgKingSafetyQueenAttackOuterRingPer8;
            MgKingSafetyQueenAttackInnerRingPer8 = CopyFromConfig.MgKingSafetyQueenAttackInnerRingPer8;
            MgKingSafetySemiOpenFilePer8 = CopyFromConfig.MgKingSafetySemiOpenFilePer8;
            MgKingSafetyPawnShieldPer8 = CopyFromConfig.MgKingSafetyPawnShieldPer8;
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
