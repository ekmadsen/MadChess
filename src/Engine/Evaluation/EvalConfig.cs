﻿// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
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
    public int EgPawnMaterial = 171;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 605;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 652;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 1091;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 2085;

    // Passed Pawns
    public int PassedPawnPowerPer128 = 322;
    public int MgPassedPawnScalePer128 = 88;
    public int EgPassedPawnScalePer128 = 331;
    public int EgFreePassedPawnScalePer128 = 692;
    public int EgConnectedPassedPawnScalePer128 = 103;
    public int EgKingEscortedPassedPawn = 15;
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial); // Incentivize engine to promote pawns.

    // King Safety
    public int MgKingSafetyPowerPer128 = 232;
    public int MgKingSafetyScalePer128 = 61;
    public int MgKingSafetyKnightAttackOuterRingPer8 = 32;
    public int MgKingSafetyKnightAttackInnerRingPer8 = 28;
    public int MgKingSafetyBishopAttackOuterRingPer8 = 12;
    public int MgKingSafetyBishopAttackInnerRingPer8 = 32;
    public int MgKingSafetyRookAttackOuterRingPer8 = 11;
    public int MgKingSafetyRookAttackInnerRingPer8 = 17;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 23;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 24;
    public int MgKingSafetySemiOpenFilePer8 = 11;
    public int MgKingSafetyPawnShieldPer8 = 14;
    public int MgKingSafetyDefendingPiecesPer8 = 16;

    // Pawn Location
    public int MgPawnAdvancement = 0;
    public int EgPawnAdvancement = 10;
    public int MgPawnCentrality = 0;
    public int EgPawnCentrality = -9;

    // Knight Location 
    public int MgKnightAdvancement = 5;
    public int EgKnightAdvancement = 10;
    public int MgKnightCentrality = 7;
    public int EgKnightCentrality = 22;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -27;

    // Bishop Location
    public int MgBishopAdvancement = -3;
    public int EgBishopAdvancement = 6;
    public int MgBishopCentrality = 10;
    public int EgBishopCentrality = 1;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = -4;

    // Rook Location
    public int MgRookAdvancement = 5;
    public int EgRookAdvancement = 14;
    public int MgRookCentrality = 4;
    public int EgRookCentrality = -1;
    public int MgRookCorner = -4;
    public int EgRookCorner = -2;

    // Queen Location
    public int MgQueenAdvancement = -10;
    public int EgQueenAdvancement = 32;
    public int MgQueenCentrality = 3;
    public int EgQueenCentrality = 13;
    public int MgQueenCorner = -2;
    public int EgQueenCorner = -9;

    // King Location
    public int MgKingAdvancement = 0;
    public int EgKingAdvancement = 8;
    public int MgKingCentrality = 0;
    public int EgKingCentrality = 17;
    public int MgKingCorner = 10;
    public int EgKingCorner = -8;

    // Piece Mobility
    public int PieceMobilityPowerPer128 = 111;
    public int MgKnightMobilityScale = 47;
    public int EgKnightMobilityScale = 17;
    public int MgBishopMobilityScale = 62;
    public int EgBishopMobilityScale = 156;
    public int MgRookMobilityScale = 74;
    public int EgRookMobilityScale = 163;
    public int MgQueenMobilityScale = 81;
    public int EgQueenMobilityScale = 73;

    // Pawn Structure
    public int MgIsolatedPawn = 13;
    public int EgIsolatedPawn = 41;
    public int MgDoubledPawn = 11;
    public int EgDoubledPawn = 30;

    // Threats
    public int MgPawnThreatenMinor = 48;
    public int EgPawnThreatenMinor = 40;
    public int MgPawnThreatenMajor = 69;
    public int EgPawnThreatenMajor = 50;
    public int MgMinorThreatenMajor = 29;
    public int EgMinorThreatenMajor = 37;

    // Minor Pieces
    public int MgBishopPair = 44;
    public int EgBishopPair = 119;
    public int MgKnightOutpost = 43;
    public int EgKnightOutpost = 28;
    public int MgBishopOutpost = 25;
    public int EgBishopOutpost = 16;

    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 17;
    public int EgRook7thRank = 57;
    // ReSharper restore InconsistentNaming

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
        EgConnectedPassedPawnScalePer128 = copyFromConfig.EgConnectedPassedPawnScalePer128;
        EgKingEscortedPassedPawn = copyFromConfig.EgKingEscortedPassedPawn;

        // Copy king safety values.
        MgKingSafetyPowerPer128 = copyFromConfig.MgKingSafetyPowerPer128;
        MgKingSafetyScalePer128 = copyFromConfig.MgKingSafetyScalePer128;
        MgKingSafetyKnightAttackOuterRingPer8 = copyFromConfig.MgKingSafetyKnightAttackOuterRingPer8;
        MgKingSafetyKnightAttackInnerRingPer8 = copyFromConfig.MgKingSafetyKnightAttackInnerRingPer8;
        MgKingSafetyBishopAttackOuterRingPer8 = copyFromConfig.MgKingSafetyBishopAttackOuterRingPer8;
        MgKingSafetyBishopAttackInnerRingPer8 = copyFromConfig.MgKingSafetyBishopAttackInnerRingPer8;
        MgKingSafetyRookAttackOuterRingPer8 = copyFromConfig.MgKingSafetyRookAttackOuterRingPer8;
        MgKingSafetyRookAttackInnerRingPer8 = copyFromConfig.MgKingSafetyRookAttackInnerRingPer8;
        MgKingSafetyQueenAttackOuterRingPer8 = copyFromConfig.MgKingSafetyQueenAttackOuterRingPer8;
        MgKingSafetyQueenAttackInnerRingPer8 = copyFromConfig.MgKingSafetyQueenAttackInnerRingPer8;
        MgKingSafetySemiOpenFilePer8 = copyFromConfig.MgKingSafetySemiOpenFilePer8;
        MgKingSafetyPawnShieldPer8 = copyFromConfig.MgKingSafetyPawnShieldPer8;
        MgKingSafetyDefendingPiecesPer8 = copyFromConfig.MgKingSafetyDefendingPiecesPer8;

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
    }
}