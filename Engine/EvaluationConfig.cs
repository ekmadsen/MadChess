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
        public int EgPawnMaterial = 121;
        public int MgKnightMaterial = 300;
        public int EgKnightMaterial = 435;
        public int MgBishopMaterial = 330;
        public int EgBishopMaterial = 465;
        public int MgRookMaterial = 500;
        public int EgRookMaterial = 739;
        public int MgQueenMaterial = 975;
        public int EgQueenMaterial = 1524;
        // Incentivize engine to promote pawns.
        // Also incentivize engine to eliminate enemy's last pawn in K vrs KQ or KR endgames (to trigger simple endgame scoring that pushes enemy king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
        public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial);
        public int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Passed Pawns
        public int PassedPawnPowerPer128 = 273;
        public int MgPassedPawnScalePer128 = 122;
        public int EgPassedPawnScalePer128 = 500;
        public int EgFreePassedPawnScalePer128 = 1018;
        public int EgKingEscortedPassedPawn = 17;
        // Piece Mobility
        public int PieceMobilityPowerPer128 = 73;
        public int MgKnightMobilityScale = 0;
        public int EgKnightMobilityScale = 92;
        public int MgBishopMobilityScale = 22;
        public int EgBishopMobilityScale = 167;
        public int MgRookMobilityScale = 44;
        public int EgRookMobilityScale = 110;
        public int MgQueenMobilityScale = 96;
        public int EgQueenMobilityScale = 160;
        // King Safety
        public int KingSafetyPowerPer128 = 246;
        public int MgKingSafetySemiOpenFilePer8 = 59;
        public int KingSafetyMinorAttackOuterRingPer8 = 4;
        public int KingSafetyMinorAttackInnerRingPer8 = 34;
        public int KingSafetyRookAttackOuterRingPer8 = 11;
        public int KingSafetyRookAttackInnerRingPer8 = 12;
        public int KingSafetyQueenAttackOuterRingPer8 = 17;
        public int KingSafetyQueenAttackInnerRingPer8 = 32;
        public int KingSafetyScalePer128 = 45;
        // Minor Pieces
        public int MgBishopPair = 19;
        public int EgBishopPair = 90;
        // Endgame Scaling
        public int EgBishopAdvantagePer128 = 11;
        public int EgOppBishopsPerPassedPawn = 18;
        public int EgOppBishopsPer128 = 38;
        public int EgWinningPerPawn = 11;
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
            EgPawnMaterial = CopyFromConfig.EgPawnMaterial;
            MgKnightMaterial = CopyFromConfig.MgKnightMaterial;
            EgKnightMaterial = CopyFromConfig.EgKnightMaterial;
            MgBishopMaterial = CopyFromConfig.MgBishopMaterial;
            EgBishopMaterial = CopyFromConfig.EgBishopMaterial;
            MgRookMaterial = CopyFromConfig.MgRookMaterial;
            EgRookMaterial = CopyFromConfig.EgRookMaterial;
            MgQueenMaterial = CopyFromConfig.MgQueenMaterial;
            EgQueenMaterial = CopyFromConfig.EgQueenMaterial;
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
