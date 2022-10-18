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
    public int EgPawnMaterial = 127;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 519;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 543;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 856;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 1673;
    // Passed Pawns
    public int PassedPawnPowerPer128 = 326;
    public int MgPassedPawnScalePer128 = 140;
    public int EgPassedPawnScalePer128 = 226;
    public int EgFreePassedPawnScalePer128 = 498;
    public int EgKingEscortedPassedPawn = 11;
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial); // Incentivize engine to promote pawns.
    // King Safety
    public int MgKingSafetyPowerPer128 = 267;
    public int MgKingSafetyScalePer128 = 43;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 19;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 19;
    public int MgKingSafetyRookAttackOuterRingPer8 = 10;
    public int MgKingSafetyRookAttackInnerRingPer8 = 15;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 14;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 19;
    public int MgKingSafetySemiOpenFilePer8 = 5;
    public int MgKingSafetyPawnShieldPer8 = 16;
    public int MgKingSafetyDefendingPiecesPer8 = 11;
    public int MgKingSafetyAttackingPiecesPer8 = 4;
    // Pawn Location
    public int MgPawnAdvancement = 0;
    public int EgPawnAdvancement = 4;
    public int MgPawnCentrality = 1;
    public int EgPawnCentrality = -5;
    // Knight Location 
    public int MgKnightAdvancement = 5;
    public int EgKnightAdvancement = 5;
    public int MgKnightCentrality = 10;
    public int EgKnightCentrality = 11;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -15;
    // Bishop Location
    public int MgBishopAdvancement = -5;
    public int EgBishopAdvancement = 4;
    public int MgBishopCentrality = 14;
    public int EgBishopCentrality = 1;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = 0;
    // Rook Location
    public int MgRookAdvancement = -2;
    public int EgRookAdvancement = 14;
    public int MgRookCentrality = 11;
    public int EgRookCentrality = -2;
    public int MgRookCorner = 0;
    public int EgRookCorner = -2;
    // Queen Location
    public int MgQueenAdvancement = -13;
    public int EgQueenAdvancement = 20;
    public int MgQueenCentrality = 4;
    public int EgQueenCentrality = 8;
    public int MgQueenCorner = -1;
    public int EgQueenCorner = -3;
    // King Location
    public int MgKingAdvancement = -50;
    public int EgKingAdvancement = 20;
    public int MgKingCentrality = 0;
    public int EgKingCentrality = 12;
    public int MgKingCorner = 8;
    public int EgKingCorner = -5;
    // Piece Mobility
    public int PieceMobilityPowerPer128 = 120;
    public int MgKnightMobilityScale = 38;
    public int EgKnightMobilityScale = 50;
    public int MgBishopMobilityScale = 43;
    public int EgBishopMobilityScale = 146;
    public int MgRookMobilityScale = 106;
    public int EgRookMobilityScale = 84;
    public int MgQueenMobilityScale = 95;
    public int EgQueenMobilityScale = 89;
    // Pawn Structure
    public int MgIsolatedPawn = 9;
    public int EgIsolatedPawn = 30;
    public int MgDoubledPawn = 26;
    public int EgDoubledPawn = 10;
    // Threats
    public int MgPawnThreatenMinor = 50;
    public int EgPawnThreatenMinor = 48;
    public int MgPawnThreatenMajor = 75;
    public int EgPawnThreatenMajor = 43;
    public int MgMinorThreatenMajor = 41;
    public int EgMinorThreatenMajor = 24;
    // Minor Pieces
    public int MgBishopPair = 56;
    public int EgBishopPair = 64;
    public int MgKnightOutpost = 34;
    public int EgKnightOutpost = 34;
    public int MgBishopOutpost = 34;
    public int EgBishopOutpost = 1;
    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 58;
    public int EgRook7thRank = 21;
    // ReSharper restore InconsistentNaming
    // Endgame Scale
    public int EgScaleMinorAdvantage = 32;
    public int EgScaleOppBishopsPerPassedPawn = 62;
    public int EgScalePerPawn = 40;
    // Limit Strength
    public bool LimitedStrength = false;
    public int LsPassedPawnsPer128 = 128;
    public int LsKingSafetyPer128 = 128;
    public int LsPieceLocationPer128 = 128;
    public int LsPieceMobilityPer128 = 128;
    public int LsPawnStructurePer128 = 128;
    public int LsThreatsPer128 = 128;
    public int LsMinorPiecesPer128 = 128;
    public int LsMajorPiecesPer128 = 128;
    public int LsEndgameScalePer128 = 128;
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
        // Copy passed pawn values.
        PassedPawnPowerPer128 = copyFromConfig.PassedPawnPowerPer128;
        MgPassedPawnScalePer128 = copyFromConfig.MgPassedPawnScalePer128;
        EgPassedPawnScalePer128 = copyFromConfig.EgPassedPawnScalePer128;
        EgFreePassedPawnScalePer128 = copyFromConfig.EgFreePassedPawnScalePer128;
        EgKingEscortedPassedPawn = copyFromConfig.EgKingEscortedPassedPawn;
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
        MgKingSafetyDefendingPiecesPer8 = copyFromConfig.MgKingSafetyDefendingPiecesPer8;
        MgKingSafetyAttackingPiecesPer8 = copyFromConfig.MgKingSafetyAttackingPiecesPer8;
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
        // Copy pawn structure values.
        MgIsolatedPawn = copyFromConfig.MgIsolatedPawn;
        EgIsolatedPawn = copyFromConfig.EgIsolatedPawn;
        MgDoubledPawn = copyFromConfig.MgDoubledPawn;
        EgDoubledPawn = copyFromConfig.EgDoubledPawn;
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
        MgKnightOutpost = copyFromConfig.MgKnightOutpost;
        EgKnightOutpost = copyFromConfig.EgKnightOutpost;
        MgBishopOutpost = copyFromConfig.MgBishopOutpost;
        EgBishopOutpost = copyFromConfig.EgBishopOutpost;
        // Copy major piece values.
        MgRook7thRank = copyFromConfig.MgRook7thRank;
        EgRook7thRank = copyFromConfig.EgRook7thRank;
        // Copy endgame scale values.
        EgScaleMinorAdvantage = copyFromConfig.EgScaleMinorAdvantage;
        EgScaleOppBishopsPerPassedPawn = copyFromConfig.EgScaleOppBishopsPerPassedPawn;
        EgScalePerPawn = copyFromConfig.EgScalePerPawn;
        // Copy limit strength values.
        LimitedStrength = copyFromConfig.LimitedStrength;
        LsPassedPawnsPer128 = copyFromConfig.LsPassedPawnsPer128;
        LsKingSafetyPer128 = copyFromConfig.LsKingSafetyPer128;
        LsPieceLocationPer128 = copyFromConfig.LsPieceLocationPer128;
        LsPieceMobilityPer128 = copyFromConfig.LsPieceMobilityPer128;
        LsPawnStructurePer128 = copyFromConfig.LsPawnStructurePer128;
        LsThreatsPer128 = copyFromConfig.LsThreatsPer128;
        LsMinorPiecesPer128 = copyFromConfig.LsMinorPiecesPer128;
        LsMajorPiecesPer128 = copyFromConfig.LsMajorPiecesPer128;
        LsEndgameScalePer128 = copyFromConfig.LsEndgameScalePer128;
    }
}