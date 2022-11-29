﻿// +---------------------------------------------------------------------------+
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
    public int EgPawnMaterial = 152;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 609;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 650;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 1068;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 2068;
    // Passed Pawns
    public int PassedPawnPowerPer128 = 314;
    public int MgPassedPawnScalePer128 = 126;
    public int EgPassedPawnScalePer128 = 349;
    public int EgFreePassedPawnScalePer128 = 806;
    public int EgKingEscortedPassedPawn = 16;
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial); // Incentivize engine to promote pawns.
    // King Safety
    public int MgKingSafetyPowerPer128 = 255;
    public int MgKingSafetyScalePer128 = 52;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 17;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 17;
    public int MgKingSafetyRookAttackOuterRingPer8 = 12;
    public int MgKingSafetyRookAttackInnerRingPer8 = 12;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 14;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 21;
    public int MgKingSafetySemiOpenFilePer8 = 3;
    public int MgKingSafetyPawnShieldPer8 = 12;
    public int MgKingSafetyDefendingPiecesPer8 = 9;
    public int MgKingSafetyAttackingPiecesPer8 = 5;
    // Pawn Location
    public int MgPawnAdvancement = 0;
    public int EgPawnAdvancement = 8;
    public int MgPawnCentrality = 0;
    public int EgPawnCentrality = -6;
    // Knight Location 
    public int MgKnightAdvancement = 4;
    public int EgKnightAdvancement = 10;
    public int MgKnightCentrality = 8;
    public int EgKnightCentrality = 12;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -17;
    // Bishop Location
    public int MgBishopAdvancement = -5;
    public int EgBishopAdvancement = 11;
    public int MgBishopCentrality = 11;
    public int EgBishopCentrality = 3;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = 0;
    // Rook Location
    public int MgRookAdvancement = 2;
    public int EgRookAdvancement = 18;
    public int MgRookCentrality = 9;
    public int EgRookCentrality = -2;
    public int MgRookCorner = 0;
    public int EgRookCorner = -1;
    // Queen Location
    public int MgQueenAdvancement = -9;
    public int EgQueenAdvancement = 30;
    public int MgQueenCentrality = 3;
    public int EgQueenCentrality = 15;
    public int MgQueenCorner = 0;
    public int EgQueenCorner = -7;
    // King Location
    public int MgKingAdvancement = -40;
    public int EgKingAdvancement = 28;
    public int MgKingCentrality = 0;
    public int EgKingCentrality = 15;
    public int MgKingCorner = 7;
    public int EgKingCorner = -13;
    // Piece Mobility
    public int PieceMobilityPowerPer128 = 116;
    public int MgKnightMobilityScale = 54;
    public int EgKnightMobilityScale = 108;
    public int MgBishopMobilityScale = 58;
    public int EgBishopMobilityScale = 181;
    public int MgRookMobilityScale = 87;
    public int EgRookMobilityScale = 157;
    public int MgQueenMobilityScale = 89;
    public int EgQueenMobilityScale = 90;
    // Pawn Structure
    public int MgIsolatedPawn = 12;
    public int EgIsolatedPawn = 43;
    public int MgDoubledPawn = 30;
    public int EgDoubledPawn = 15;
    // Threats
    public int MgPawnThreatenMinor = 46;
    public int EgPawnThreatenMinor = 51;
    public int MgPawnThreatenMajor = 64;
    public int EgPawnThreatenMajor = 40;
    public int MgMinorThreatenMajor = 42;
    public int EgMinorThreatenMajor = 17;
    // Minor Pieces
    public int MgBishopPair = 44;
    public int EgBishopPair = 62;
    public int MgKnightOutpost = 45;
    public int EgKnightOutpost = 48;
    public int MgBishopOutpost = 39;
    public int EgBishopOutpost = 1;
    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 50;
    public int EgRook7thRank = 5;
    // ReSharper restore InconsistentNaming
    // Endgame Scale
    public int EgScaleMinorAdvantage = 11;
    public int EgScaleOppBishopsPerPassedPawn = 34;
    public int EgScalePerPawnAdvantage = 11;
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
        EgScalePerPawnAdvantage = copyFromConfig.EgScalePerPawnAdvantage;
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