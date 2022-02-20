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
    public int EgPawnMaterial = 95;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 420;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 420;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 728;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 1414;
    // Incentivize engine to promote pawns.
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial);
    // Pawn Location
    public int MgPawnAdvancement = 11;
    public int EgPawnAdvancement = 4;
    public int MgPawnCentrality = 19;
    public int EgPawnCentrality = -6;
    // Knight Location 
    public int MgKnightAdvancement = 11;
    public int EgKnightAdvancement = 0;
    public int MgKnightCentrality = 19;
    public int EgKnightCentrality = 16;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -24;
    // Bishop Location
    public int MgBishopAdvancement = 3;
    public int EgBishopAdvancement = 9;
    public int MgBishopCentrality = 22;
    public int EgBishopCentrality = 4;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = -1;
    // Rook Location
    public int MgRookAdvancement = 0;
    public int EgRookAdvancement = 10;
    public int MgRookCentrality = 14;
    public int EgRookCentrality = 6;
    public int MgRookCorner = -22;
    public int EgRookCorner = 0;
    // Queen Location
    public int MgQueenAdvancement = -23;
    public int EgQueenAdvancement = 27;
    public int MgQueenCentrality = 10;
    public int EgQueenCentrality = 12;
    public int MgQueenCorner = 0;
    public int EgQueenCorner = -7;
    // King Location
    public int MgKingAdvancement = 0;
    public int EgKingAdvancement = 8;
    public int MgKingCentrality = 0;
    public int EgKingCentrality = 15;
    public int MgKingCorner = 2;
    public int EgKingCorner = -3;
    // Pawn Structure
    public int MgIsolatedPawn = 41;
    public int EgIsolatedPawn = 39;
    public int MgDoubledPawn = 20;
    public int EgDoubledPawn = 22;
    // Passed Pawns
    public int PassedPawnPowerPer128 = 309;
    public int MgPassedPawnScalePer128 = 44;
    public int EgPassedPawnScalePer128 = 286;
    public int EgFreePassedPawnScalePer128 = 506;
    public int EgKingEscortedPassedPawn = 12;
    // Piece Mobility
    public int PieceMobilityPowerPer128 = 38;
    public int MgKnightMobilityScale = 7;
    public int EgKnightMobilityScale = 113;
    public int MgBishopMobilityScale = 121;
    public int EgBishopMobilityScale = 121;
    public int MgRookMobilityScale = 114;
    public int EgRookMobilityScale = 117;
    public int MgQueenMobilityScale = 255;
    public int EgQueenMobilityScale = 6;
    // King Safety
    public int MgKingSafetyPowerPer128 = 218;
    public int MgKingSafetyScalePer128 = 78;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 9;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 35;
    public int MgKingSafetyRookAttackOuterRingPer8 = 4;
    public int MgKingSafetyRookAttackInnerRingPer8 = 19;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 24;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 16;
    public int MgKingSafetySemiOpenFilePer8 = 29;
    public int MgKingSafetyPawnShieldPer8 = 30;
    // Threats
    public int MgPawnThreatenMinor = 27;
    public int EgPawnThreatenMinor = 42;
    public int MgPawnThreatenMajor = 67;
    public int EgPawnThreatenMajor = 13;
    public int MgMinorThreatenMajor = 62;
    public int EgMinorThreatenMajor = 34;
    // Minor Pieces
    public int MgBishopPair = 39;
    public int EgBishopPair = 60;
    // Endgame Scale
    public int EgScaleBishopAdvantagePer128 = 55;
    public int EgScaleOppBishopsPerPassedPawn = 38;
    public int EgScaleWinningPerPawn = 25;
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