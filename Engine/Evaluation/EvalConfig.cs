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
    public int EgPawnMaterial = 100;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 489;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 499;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 791;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 1468;
    // Incentivize engine to promote pawns.
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial);
    // Pawn Location
    public int MgPawnAdvancement = 8;
    public int EgPawnAdvancement = 4;
    public int MgPawnCentrality = 4;
    public int EgPawnCentrality = -4;
    // Knight Location 
    public int MgKnightAdvancement = 1;
    public int EgKnightAdvancement = 6;
    public int MgKnightCentrality = 16;
    public int EgKnightCentrality = 17;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -23;
    // Bishop Location
    public int MgBishopAdvancement = -7;
    public int EgBishopAdvancement = 4;
    public int MgBishopCentrality = 19;
    public int EgBishopCentrality = 5;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = 0;
    // Rook Location
    public int MgRookAdvancement = -1;
    public int EgRookAdvancement = 11;
    public int MgRookCentrality = 8;
    public int EgRookCentrality = -2;
    public int MgRookCorner = -12;
    public int EgRookCorner = -1;
    // Queen Location
    public int MgQueenAdvancement = -24;
    public int EgQueenAdvancement = 26;
    public int MgQueenCentrality = 4;
    public int EgQueenCentrality = 12;
    public int MgQueenCorner = 0;
    public int EgQueenCorner = -6;
    // King Location
    public int MgKingAdvancement = 0;
    public int EgKingAdvancement = 14;
    public int MgKingCentrality = 0;
    public int EgKingCentrality = 9;
    public int MgKingCorner = 0;
    public int EgKingCorner = -2;
    // Pawn Structure
    public int MgIsolatedPawn = 22;
    public int EgIsolatedPawn = 22;
    public int MgDoubledPawn = 20;
    public int EgDoubledPawn = 23;
    // Passed Pawns
    public int PassedPawnPowerPer128 = 304;
    public int MgPassedPawnScalePer128 = 175;
    public int EgPassedPawnScalePer128 = 295;
    public int EgFreePassedPawnScalePer128 = 555;
    public int EgKingEscortedPassedPawn = 13;
    // Piece Mobility
    public int PieceMobilityPowerPer128 = 89;
    public int MgKnightMobilityScale = 3;
    public int EgKnightMobilityScale = 33;
    public int MgBishopMobilityScale = 5;
    public int EgBishopMobilityScale = 102;
    public int MgRookMobilityScale = 44;
    public int EgRookMobilityScale = 90;
    public int MgQueenMobilityScale = 62;
    public int EgQueenMobilityScale = 46;
    // King Safety
    public int MgKingSafetyPowerPer128 = 237;
    public int MgKingSafetyScalePer128 = 84;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 7;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 29;
    public int MgKingSafetyRookAttackOuterRingPer8 = 10;
    public int MgKingSafetyRookAttackInnerRingPer8 = 24;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 12;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 16;
    public int MgKingSafetySemiOpenFilePer8 = 24;
    public int MgKingSafetyPawnShieldPer8 = 25;
    // Threats
    public int MgPawnThreatenMinor = 36;
    public int EgPawnThreatenMinor = 51;
    public int MgPawnThreatenMajor = 72;
    public int EgPawnThreatenMajor = 26;
    public int MgMinorThreatenMajor = 78;
    public int EgMinorThreatenMajor = 28;
    // Minor Pieces
    public int MgBishopPair = 40;
    public int EgBishopPair = 56;
    // Endgame Scale
    public int EgScaleBishopAdvantagePer128 = 43;
    public int EgScaleOppBishopsPerPassedPawn = 48;
    public int EgScaleWinningPerPawn = 58;
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