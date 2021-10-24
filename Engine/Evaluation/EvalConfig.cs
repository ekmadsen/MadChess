// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine.Evaluation
{
    public sealed class EvalConfig
    {
        // ReSharper disable ConvertToConstant.Global
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        // ReSharper disable RedundantDefaultMemberInitializer
        // Material
        public int MgKnightMaterial = 300;
        public int EgKnightMaterial = 455;
        public int MgBishopMaterial = 330;
        public int EgBishopMaterial = 472;
        public int MgRookMaterial = 500;
        public int EgRookMaterial = 765;
        public int MgQueenMaterial = 975;
        public int EgQueenMaterial = 1388;
        // Incentivize engine to promote pawns.
        // Also incentivize engine to eliminate enemy's last pawn in K vrs KQ or KR endgames (to trigger simple endgame scoring that pushes enemy king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
        public int UnstoppablePassedPawn => EgQueenMaterial - (2 * Eval.PawnMaterial);
        public int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Pawn Location
        public int MgPawnAdvancement = 5;
        public int EgPawnAdvancement = 5;
        public int MgPawnCentrality = 0;
        public int EgPawnCentrality = -6;
        // Knight Location 
        public int MgKnightAdvancement = 3;
        public int EgKnightAdvancement = 7;
        public int MgKnightCentrality = 13;
        public int EgKnightCentrality = 19;
        public int MgKnightCorner = 0;
        public int EgKnightCorner = -25;
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
        public int MgRookCorner = -13;
        public int EgRookCorner = 0;
        // Queen Location
        public int MgQueenAdvancement = -22;
        public int EgQueenAdvancement = 25;
        public int MgQueenCentrality = 3;
        public int EgQueenCentrality = 7;
        public int MgQueenCorner = 0;
        public int EgQueenCorner = -6;
        // King Location
        public int MgKingAdvancement = 0;
        public int EgKingAdvancement = 12;
        public int MgKingCentrality = 0;
        public int EgKingCentrality = 20;
        public int MgKingCorner = 1;
        public int EgKingCorner = -2;
        // Passed Pawns
        public int PassedPawnPowerPer128 = 307;
        public int MgPassedPawnScalePer128 = 119;
        public int EgPassedPawnScalePer128 = 257;
        public int EgFreePassedPawnScalePer128 = 547;
        public int EgKingEscortedPassedPawn = 12;
        // Piece Mobility
        public int PieceMobilityPowerPer128 = 81;
        public int MgKnightMobilityScale = 1;
        public int EgKnightMobilityScale = 6;
        public int MgBishopMobilityScale = 1;
        public int EgBishopMobilityScale = 162;
        public int MgRookMobilityScale = 45;
        public int EgRookMobilityScale = 86;
        public int MgQueenMobilityScale = 72;
        public int EgQueenMobilityScale = 166;
        // King Safety
        public int MgKingSafetyPowerPer128 = 244;
        public int MgKingSafetyScalePer128 = 84;
        public int MgKingSafetyMinorAttackOuterRingPer8 = 8;
        public int MgKingSafetyMinorAttackInnerRingPer8 = 31;
        public int MgKingSafetyRookAttackOuterRingPer8 = 12;
        public int MgKingSafetyRookAttackInnerRingPer8 = 20;
        public int MgKingSafetyQueenAttackOuterRingPer8 = 15;
        public int MgKingSafetyQueenAttackInnerRingPer8 = 24;
        public int MgKingSafetySemiOpenFilePer8 = 30;
        public int MgKingSafetyPawnShieldPer8 = 8;
        // Threats
        public int MgPawnThreatenMinor = 54;
        public int MgPawnThreatenMajor = 74;
        public int MgMinorThreatenMajor = 60;
        // Minor Pieces
        public int MgBishopPair = 42;
        public int EgBishopPair = 85;
        // Endgame Scaling
        public int EgBishopAdvantagePer128 = 19;
        public int EgOppBishopsPerPassedPawn = 25;
        public int EgOppBishopsPer128 = 23;
        public int EgWinningPerPawn = 30;
        // Limit Strength
        public bool LimitedStrength = false;
        public int LsPieceLocationPer128 = 128;
        public int LsPassedPawnsPer128 = 128;
        public int LsPieceMobilityPer128 = 128;
        public int LsKingSafetyPer128 = 128;
        public int LsMinorPiecesPer128 = 128;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global


        public void Set(EvalConfig copyFromConfig)
        {
            // Copy material values.
            MgKnightMaterial = copyFromConfig.MgKnightMaterial;
            EgKnightMaterial = copyFromConfig.EgKnightMaterial;
            MgBishopMaterial = copyFromConfig.MgBishopMaterial;
            EgBishopMaterial = copyFromConfig.EgBishopMaterial;
            MgRookMaterial = copyFromConfig.MgRookMaterial;
            EgRookMaterial = copyFromConfig.EgRookMaterial;
            MgQueenMaterial = copyFromConfig.MgQueenMaterial;
            EgQueenMaterial = copyFromConfig.EgQueenMaterial;
            // Copy piece location values.
            MgPawnAdvancement = copyFromConfig.MgPawnAdvancement;
            EgPawnAdvancement = copyFromConfig.EgPawnAdvancement;
            MgPawnCentrality = copyFromConfig.MgPawnCentrality;
            EgPawnCentrality = copyFromConfig.EgPawnCentrality;
            MgKnightAdvancement = copyFromConfig.MgKnightAdvancement;
            EgKnightAdvancement = copyFromConfig.EgKnightAdvancement;
            MgKnightCentrality = copyFromConfig.MgKnightCentrality;
            EgKnightCentrality = copyFromConfig.EgKnightCentrality;
            MgKnightCorner = copyFromConfig.MgKnightCorner;
            EgKnightCorner = copyFromConfig.EgKnightCorner;
            MgBishopAdvancement = copyFromConfig.MgBishopAdvancement;
            EgBishopAdvancement = copyFromConfig.EgBishopAdvancement;
            MgBishopCentrality = copyFromConfig.MgBishopCentrality;
            EgBishopCentrality = copyFromConfig.EgBishopCentrality;
            MgBishopCorner = copyFromConfig.MgBishopCorner;
            EgBishopCorner = copyFromConfig.EgBishopCorner;
            MgRookAdvancement = copyFromConfig.MgRookAdvancement;
            EgRookAdvancement = copyFromConfig.EgRookAdvancement;
            MgRookCentrality = copyFromConfig.MgRookCentrality;
            EgRookCentrality = copyFromConfig.EgRookCentrality;
            MgRookCorner = copyFromConfig.MgRookCorner;
            EgRookCorner = copyFromConfig.EgRookCorner;
            MgQueenAdvancement = copyFromConfig.MgQueenAdvancement;
            EgQueenAdvancement = copyFromConfig.EgQueenAdvancement;
            MgQueenCentrality = copyFromConfig.MgQueenCentrality;
            EgQueenCentrality = copyFromConfig.EgQueenCentrality;
            MgQueenCorner = copyFromConfig.MgQueenCorner;
            EgQueenCorner = copyFromConfig.EgQueenCorner;
            MgKingAdvancement = copyFromConfig.MgKingAdvancement;
            EgKingAdvancement = copyFromConfig.EgKingAdvancement;
            MgKingCentrality = copyFromConfig.MgKingCentrality;
            EgKingCentrality = copyFromConfig.EgKingCentrality;
            MgKingCorner = copyFromConfig.MgKingCorner;
            EgKingCorner = copyFromConfig.EgKingCorner;
            // Copy passed pawn values.
            PassedPawnPowerPer128 = copyFromConfig.PassedPawnPowerPer128;
            MgPassedPawnScalePer128 = copyFromConfig.MgPassedPawnScalePer128;
            EgPassedPawnScalePer128 = copyFromConfig.EgPassedPawnScalePer128;
            EgFreePassedPawnScalePer128 = copyFromConfig.EgFreePassedPawnScalePer128;
            EgKingEscortedPassedPawn = copyFromConfig.EgKingEscortedPassedPawn;
            // Copy piece mobility values.
            PieceMobilityPowerPer128 = copyFromConfig.PieceMobilityPowerPer128;
            MgKnightMobilityScale = copyFromConfig.MgKnightMobilityScale;
            EgKnightMobilityScale = copyFromConfig.EgKnightMobilityScale;
            MgBishopMobilityScale = copyFromConfig.MgBishopMobilityScale;
            EgBishopMobilityScale = copyFromConfig.EgBishopMobilityScale;
            MgRookMobilityScale = copyFromConfig.MgRookMobilityScale;
            EgRookMobilityScale = copyFromConfig.EgRookMobilityScale;
            MgQueenMobilityScale = copyFromConfig.MgQueenMobilityScale;
            EgQueenMobilityScale = copyFromConfig.EgQueenMobilityScale;
            // Copy king safety values.
            MgKingSafetyPowerPer128 = copyFromConfig.MgKingSafetyPowerPer128;
            MgKingSafetyScalePer128 = copyFromConfig.MgKingSafetyScalePer128;
            MgKingSafetyMinorAttackOuterRingPer8 = copyFromConfig.MgKingSafetyMinorAttackOuterRingPer8;
            MgKingSafetyMinorAttackInnerRingPer8 = copyFromConfig.MgKingSafetyMinorAttackInnerRingPer8;
            MgKingSafetyRookAttackOuterRingPer8 = copyFromConfig.MgKingSafetyRookAttackOuterRingPer8;
            MgKingSafetyRookAttackInnerRingPer8 = copyFromConfig.MgKingSafetyRookAttackInnerRingPer8;
            MgKingSafetyQueenAttackOuterRingPer8 = copyFromConfig.MgKingSafetyQueenAttackOuterRingPer8;
            MgKingSafetyQueenAttackInnerRingPer8 = copyFromConfig.MgKingSafetyQueenAttackInnerRingPer8;
            MgKingSafetySemiOpenFilePer8 = copyFromConfig.MgKingSafetySemiOpenFilePer8;
            MgKingSafetyPawnShieldPer8 = copyFromConfig.MgKingSafetyPawnShieldPer8;
            // Copy threats value.
            MgPawnThreatenMinor = copyFromConfig.MgPawnThreatenMinor;
            MgPawnThreatenMajor = copyFromConfig.MgPawnThreatenMajor;
            MgMinorThreatenMajor = copyFromConfig.MgMinorThreatenMajor;
            // Copy minor values.
            MgBishopPair = copyFromConfig.MgBishopPair;
            EgBishopPair = copyFromConfig.EgBishopPair;
            // Copy endgame scaling values.
            EgBishopAdvantagePer128 = copyFromConfig.EgBishopAdvantagePer128;
            EgOppBishopsPerPassedPawn = copyFromConfig.EgOppBishopsPerPassedPawn;
            EgOppBishopsPer128 = copyFromConfig.EgOppBishopsPer128;
            EgWinningPerPawn = copyFromConfig.EgWinningPerPawn;
            // Copy limit strength values.
            LimitedStrength = copyFromConfig.LimitedStrength;
            LsPieceLocationPer128 = copyFromConfig.LsPieceLocationPer128;
            LsPassedPawnsPer128 = copyFromConfig.PassedPawnPowerPer128;
            LsPieceMobilityPer128 = copyFromConfig.LsPieceMobilityPer128;
            LsKingSafetyPer128 = copyFromConfig.LsKingSafetyPer128;
            LsMinorPiecesPer128 = copyFromConfig.LsMinorPiecesPer128;
        }
    }
}
