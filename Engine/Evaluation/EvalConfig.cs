// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine.Evaluation;


public sealed class EvalConfig
{
    // ReSharper disable ConvertToConstant.Global
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    // ReSharper disable RedundantDefaultMemberInitializer
    // Material
    public const int MgPawnMaterial = 100;
    public int EgPawnMaterial = 119;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 546;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 557;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 875;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 1592;
    // Incentivize engine to promote pawns.
    // Also incentivize engine to eliminate enemy's last pieces and pawns (to trigger simple endgame scoring that pushes enemy king to a corner).
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial);
    public int SimpleEndgame => 4 * EgQueenMaterial; // Winning side is unlikely to acquire this much of a material advantage before delivering checkmate.
    // Pawn Location
    public int MgPawnAdvancement = 8;
    public int EgPawnAdvancement = 4;
    public int MgPawnCentrality = 0;
    public int EgPawnCentrality = -4;
    // Knight Location 
    public int MgKnightAdvancement = 3;
    public int EgKnightAdvancement = 8;
    public int MgKnightCentrality = 11;
    public int EgKnightCentrality = 14;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -23;
    // Bishop Location
    public int MgBishopAdvancement = -4;
    public int EgBishopAdvancement = 5;
    public int MgBishopCentrality = 10;
    public int EgBishopCentrality = 5;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = -2;
    // Rook Location
    public int MgRookAdvancement = 5;
    public int EgRookAdvancement = 14;
    public int MgRookCentrality = 0;
    public int EgRookCentrality = -3;
    public int MgRookCorner = -7;
    public int EgRookCorner = 3;
    // Queen Location
    public int MgQueenAdvancement = -24;
    public int EgQueenAdvancement = 36;
    public int MgQueenCentrality = 3;
    public int EgQueenCentrality = 11;
    public int MgQueenCorner = 0;
    public int EgQueenCorner = -2;
    // King Location
    public int MgKingAdvancement = -1;
    public int EgKingAdvancement = 17;
    public int MgKingCentrality = -1;
    public int EgKingCentrality = 12;
    public int MgKingCorner = 1;
    public int EgKingCorner = 0;
    // Pawn Structure
    public int MgIsolatedPawn = 23;
    public int EgIsolatedPawn = 32;
    public int MgDoubledPawn = 21;
    public int EgDoubledPawn = 31;
    // Passed Pawns
    public int PassedPawnPowerPer128 = 310;
    public int MgPassedPawnScalePer128 = 189;
    public int EgPassedPawnScalePer128 = 239;
    public int EgFreePassedPawnScalePer128 = 518;
    public int EgKingEscortedPassedPawn = 14;
    // Piece Mobility
    public int PieceMobilityPowerPer128 = 100;
    public int MgKnightMobilityScale = 3;
    public int EgKnightMobilityScale = 31;
    public int MgBishopMobilityScale = 2;
    public int EgBishopMobilityScale = 133;
    public int MgRookMobilityScale = 38;
    public int EgRookMobilityScale = 80;
    public int MgQueenMobilityScale = 87;
    public int EgQueenMobilityScale = 14;
    // King Safety
    public int MgKingSafetyPowerPer128 = 232;
    public int MgKingSafetyScalePer128 = 89;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 7;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 30;
    public int MgKingSafetyRookAttackOuterRingPer8 = 11;
    public int MgKingSafetyRookAttackInnerRingPer8 = 19;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 17;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 20;
    public int MgKingSafetySemiOpenFilePer8 = 21;
    public int MgKingSafetyPawnShieldPer8 = 16;
    // Threats
    public int MgPawnThreatenMinor = 35;
    public int EgPawnThreatenMinor = 58;
    public int MgPawnThreatenMajor = 57;
    public int EgPawnThreatenMajor = 19;
    public int MgMinorThreatenMajor = 60;
    public int EgMinorThreatenMajor = 26;
    // Minor Pieces
    public int MgBishopPair = 46;
    public int EgBishopPair = 63;
    // Endgame Scale
    public int EgScaleBishopAdvantagePer128 = 17;
    public int EgScaleOppBishopsPerPassedPawn = 33;
    public int EgScaleWinningPerPawn = 19;
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
        EgPawnMaterial = copyFromConfig.EgPawnMaterial;
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
        // Copy pawn structure values.
        MgIsolatedPawn = copyFromConfig.MgIsolatedPawn;
        EgIsolatedPawn = copyFromConfig.EgIsolatedPawn;
        MgDoubledPawn = copyFromConfig.MgDoubledPawn;
        EgDoubledPawn = copyFromConfig.EgDoubledPawn;
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
        // Copy threat values.
        MgPawnThreatenMinor = copyFromConfig.MgPawnThreatenMinor;
        EgPawnThreatenMinor = copyFromConfig.EgPawnThreatenMinor;
        MgPawnThreatenMajor = copyFromConfig.MgPawnThreatenMajor;
        EgPawnThreatenMajor = copyFromConfig.EgPawnThreatenMajor;
        MgMinorThreatenMajor = copyFromConfig.MgMinorThreatenMajor;
        EgMinorThreatenMajor = copyFromConfig.EgMinorThreatenMajor;
        // Copy minor piece values.
        MgBishopPair = copyFromConfig.MgBishopPair;
        EgBishopPair = copyFromConfig.EgBishopPair;
        // Copy endgame scale values.
        EgScaleBishopAdvantagePer128 = copyFromConfig.EgScaleBishopAdvantagePer128;
        EgScaleOppBishopsPerPassedPawn = copyFromConfig.EgScaleOppBishopsPerPassedPawn;
        EgScaleWinningPerPawn = copyFromConfig.EgScaleWinningPerPawn;
        // Copy limit strength values.
        LimitedStrength = copyFromConfig.LimitedStrength;
        LsPieceLocationPer128 = copyFromConfig.LsPieceLocationPer128;
        LsPassedPawnsPer128 = copyFromConfig.PassedPawnPowerPer128;
        LsPieceMobilityPer128 = copyFromConfig.LsPieceMobilityPer128;
        LsKingSafetyPer128 = copyFromConfig.LsKingSafetyPer128;
        LsMinorPiecesPer128 = copyFromConfig.LsMinorPiecesPer128;
    }
}