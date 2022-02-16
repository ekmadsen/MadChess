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
    public int MgPawnMaterial = 100;
    public int EgPawnMaterial = 109;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 530;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 538;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 844;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 1545;
    // Incentivize engine to promote pawns.
    // Also incentivize engine to eliminate enemy's last pieces and pawns (to trigger simple endgame scoring that pushes enemy king to a corner).
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial);
    public int SimpleEndgame => 4 * EgQueenMaterial; // Winning side is unlikely to acquire this much of a material advantage before delivering checkmate.
    // Pawn Location
    public int MgPawnAdvancement = 7;
    public int EgPawnAdvancement = 5;
    public int MgPawnCentrality = 0;
    public int EgPawnCentrality = -3;
    // Knight Location 
    public int MgKnightAdvancement = 4;
    public int EgKnightAdvancement = 7;
    public int MgKnightCentrality = 11;
    public int EgKnightCentrality = 17;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -26;
    // Bishop Location
    public int MgBishopAdvancement = -3;
    public int EgBishopAdvancement = 4;
    public int MgBishopCentrality = 10;
    public int EgBishopCentrality = 4;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = -1;
    // Rook Location
    public int MgRookAdvancement = 5;
    public int EgRookAdvancement = 14;
    public int MgRookCentrality = 0;
    public int EgRookCentrality = -3;
    public int MgRookCorner = -8;
    public int EgRookCorner = 4;
    // Queen Location
    public int MgQueenAdvancement = -23;
    public int EgQueenAdvancement = 31;
    public int MgQueenCentrality = 3;
    public int EgQueenCentrality = 8;
    public int MgQueenCorner = 0;
    public int EgQueenCorner = -2;
    // King Location
    public int MgKingAdvancement = 0;
    public int EgKingAdvancement = 14;
    public int MgKingCentrality = 0;
    public int EgKingCentrality = 12;
    public int MgKingCorner = 2;
    public int EgKingCorner = 0;
    // Pawn Structure
    public int MgIsolatedPawn = 19;
    public int EgIsolatedPawn = 30;
    public int MgDoubledPawn = 21;
    public int EgDoubledPawn = 32;
    // Passed Pawns
    public int PassedPawnPowerPer128 = 315;
    public int MgPassedPawnScalePer128 = 181;
    public int EgPassedPawnScalePer128 = 232;
    public int EgFreePassedPawnScalePer128 = 487;
    public int EgKingEscortedPassedPawn = 16;
    // Piece Mobility
    public int PieceMobilityPowerPer128 = 94;
    public int MgKnightMobilityScale = 0;
    public int EgKnightMobilityScale = 0;
    public int MgBishopMobilityScale = 5;
    public int EgBishopMobilityScale = 122;
    public int MgRookMobilityScale = 35;
    public int EgRookMobilityScale = 75;
    public int MgQueenMobilityScale = 84;
    public int EgQueenMobilityScale = 51;
    // King Safety
    public int MgKingSafetyPowerPer128 = 238;
    public int MgKingSafetyScalePer128 = 79;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 7;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 32;
    public int MgKingSafetyRookAttackOuterRingPer8 = 12;
    public int MgKingSafetyRookAttackInnerRingPer8 = 18;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 16;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 23;
    public int MgKingSafetySemiOpenFilePer8 = 23;
    public int MgKingSafetyPawnShieldPer8 = 16;
    // Threats
    public int MgPawnThreatenMinor = 35;
    public int EgPawnThreatenMinor = 63;
    public int MgPawnThreatenMajor = 57;
    public int EgPawnThreatenMajor = 26;
    public int MgMinorThreatenMajor = 46;
    public int EgMinorThreatenMajor = 31;
    // Minor Pieces
    public int MgBishopPair = 44;
    public int EgBishopPair = 73;
    // Endgame Scale
    public int EgScaleBishopAdvantagePer128 = 14;
    public int EgScaleOppBishopsPerPassedPawn = 33;
    public int EgScaleWinningPerPawn = 27;
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
        MgPawnMaterial = copyFromConfig.MgPawnMaterial;
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