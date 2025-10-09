// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine.Config;


public sealed class EvaluationConfig
{
    // ReSharper disable ConvertToConstant.Global
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    // ReSharper disable RedundantDefaultMemberInitializer

    // Material
    public int MgPawnMaterial = 100;
    public int EgPawnMaterial = 198;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 680;
    public int MgBishopMaterial = 325;
    public int EgBishopMaterial = 702;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 1189;
    public int MgQueenMaterial = 950;
    public int EgQueenMaterial = 2247;

    // Passed Pawns
    public int PassedPawnPowerPer128 = 327;
    public int MgPassedPawnScalePer128 = 162;
    public int EgPassedPawnScalePer128 = 300;
    public int MgFreePassedPawnScalePer128 = 207;
    public int EgFreePassedPawnScalePer128 = 607;
    public int MgConnectedPassedPawnScalePer128 = 5;
    public int EgConnectedPassedPawnScalePer128 = 120;
    public int EgKingEscortedPassedPawn = 19;
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial); // Incentivize engine to promote pawns.

    // King Safety
    public int MgKingSafetyPowerPer128 = 260;
    public int MgKingSafetyScalePer128 = 36;
    public int MgKingSafetyKnightAttackOuterRingPer8 = 28;
    public int MgKingSafetyKnightAttackInnerRingPer8 = 18;
    public int MgKingSafetyKnightProximityPer8 = 0;
    public int MgKingSafetyBishopAttackOuterRingPer8 = 12;
    public int MgKingSafetyBishopAttackInnerRingPer8 = 26;
    public int MgKingSafetyBishopProximityPer8 = 0;
    public int MgKingSafetyRookAttackOuterRingPer8 = 14;
    public int MgKingSafetyRookAttackInnerRingPer8 = 18;
    public int MgKingSafetyRookProximityPer8 = 3;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 19;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 14;
    public int MgKingSafetyQueenProximityPer8 = 12;
    public int MgKingSafetySemiOpenFilePer8 = 21;
    public int MgKingSafetyPawnShieldPer8 = 15;
    public int MgKingSafetyDefendingPiecesPer8 = 13;

    // Pawn Location
    public int MgPawnAdvancement = -3;
    public int EgPawnAdvancement = 16;
    public int MgPawnSquareCentrality = 12;
    public int EgPawnSquareCentrality = -35;
    public int MgPawnFileCentrality = -13;
    public int EgPawnFileCentrality = 22;
    public int MgPawnCorner = -12;
    public int EgPawnCorner = 13;

    // Knight Location 
    public int MgKnightAdvancement = 6;
    public int EgKnightAdvancement = 12;
    public int MgKnightSquareCentrality = -3;
    public int EgKnightSquareCentrality = 10;
    public int MgKnightFileCentrality = 7;
    public int EgKnightFileCentrality = 2;
    public int MgKnightCorner = 4;
    public int EgKnightCorner = -24;

    // Bishop Location
    public int MgBishopAdvancement = 1;
    public int EgBishopAdvancement = 7;
    public int MgBishopSquareCentrality = 9;
    public int EgBishopSquareCentrality = 1;
    public int MgBishopFileCentrality = -2;
    public int EgBishopFileCentrality = 7;
    public int MgBishopCorner = 10;
    public int EgBishopCorner = -2;

    // Rook Location
    public int MgRookAdvancement = 6;
    public int EgRookAdvancement = 14;
    public int MgRookSquareCentrality = -20;
    public int EgRookSquareCentrality = 10;
    public int MgRookFileCentrality = 7;
    public int EgRookFileCentrality = -6;
    public int MgRookCorner = -11;
    public int EgRookCorner = -2;

    // Queen Location
    public int MgQueenAdvancement = -14;
    public int EgQueenAdvancement = 30;
    public int MgQueenSquareCentrality = -5;
    public int EgQueenSquareCentrality = 17;
    public int MgQueenFileCentrality = 0;
    public int EgQueenFileCentrality = 5;
    public int MgQueenCorner = 2;
    public int EgQueenCorner = -1;

    // King Location
    public int MgKingAdvancement = 16;
    public int EgKingAdvancement = 21;
    public int MgKingSquareCentrality = 12;
    public int EgKingSquareCentrality = 12;
    public int MgKingFileCentrality = 7;
    public int EgKingFileCentrality = 6;
    public int MgKingCorner = 8;
    public int EgKingCorner = -4;

    // Piece Mobility
    public int PieceMobilityPowerPer128 = 96;
    public int MgKnightMobilityScale = 64;
    public int EgKnightMobilityScale = 124;
    public int MgBishopMobilityScale = 68;
    public int EgBishopMobilityScale = 175;
    public int MgRookMobilityScale = 97;
    public int EgRookMobilityScale = 131;
    public int MgQueenMobilityScale = 95;
    public int EgQueenMobilityScale = 126;

    // Pawn Structure
    public int MgIsolatedPawn = 25;
    public int EgIsolatedPawn = 51;
    public int MgDoubledPawn = 56;
    public int EgDoubledPawn = 9;

    // Threats
    public int MgPawnThreatenMinor = 33;
    public int EgPawnThreatenMinor = 64;
    public int MgPawnThreatenMajor = 49;
    public int EgPawnThreatenMajor = 69;
    public int MgMinorThreatenMajor = 45;
    public int EgMinorThreatenMajor = 39;

    // Minor Pieces
    public int MgBishopPair = 32;
    public int EgBishopPair = 145;
    public int MgKnightOutpost = 31;
    public int EgKnightOutpost = 43;
    public int MgBishopOutpost = 48;
    public int EgBishopOutpost = 15;

    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 3;
    public int EgRook7thRank = 14;
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


    public void Set(EvaluationConfig copyFromConfig)
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
        MgFreePassedPawnScalePer128 = copyFromConfig.MgFreePassedPawnScalePer128;
        EgFreePassedPawnScalePer128 = copyFromConfig.EgFreePassedPawnScalePer128;
        MgConnectedPassedPawnScalePer128 = copyFromConfig.MgConnectedPassedPawnScalePer128;
        EgConnectedPassedPawnScalePer128 = copyFromConfig.EgConnectedPassedPawnScalePer128;
        EgKingEscortedPassedPawn = copyFromConfig.EgKingEscortedPassedPawn;

        // Copy king safety values.
        MgKingSafetyScalePer128 = copyFromConfig.MgKingSafetyScalePer128;
        MgKingSafetyKnightAttackOuterRingPer8 = copyFromConfig.MgKingSafetyKnightAttackOuterRingPer8;
        MgKingSafetyKnightAttackInnerRingPer8 = copyFromConfig.MgKingSafetyKnightAttackInnerRingPer8;
        MgKingSafetyKnightProximityPer8 = copyFromConfig.MgKingSafetyKnightProximityPer8;
        MgKingSafetyBishopAttackOuterRingPer8 = copyFromConfig.MgKingSafetyBishopAttackOuterRingPer8;
        MgKingSafetyBishopAttackInnerRingPer8 = copyFromConfig.MgKingSafetyBishopAttackInnerRingPer8;
        MgKingSafetyBishopProximityPer8 = copyFromConfig.MgKingSafetyBishopProximityPer8;
        MgKingSafetyRookAttackOuterRingPer8 = copyFromConfig.MgKingSafetyRookAttackOuterRingPer8;
        MgKingSafetyRookAttackInnerRingPer8 = copyFromConfig.MgKingSafetyRookAttackInnerRingPer8;
        MgKingSafetyRookProximityPer8 = copyFromConfig.MgKingSafetyRookProximityPer8;
        MgKingSafetyQueenAttackOuterRingPer8 = copyFromConfig.MgKingSafetyQueenAttackOuterRingPer8;
        MgKingSafetyQueenAttackInnerRingPer8 = copyFromConfig.MgKingSafetyQueenAttackInnerRingPer8;
        MgKingSafetyQueenProximityPer8 = copyFromConfig.MgKingSafetyQueenProximityPer8;
        MgKingSafetySemiOpenFilePer8 = copyFromConfig.MgKingSafetySemiOpenFilePer8;
        MgKingSafetyPawnShieldPer8 = copyFromConfig.MgKingSafetyPawnShieldPer8;
        MgKingSafetyDefendingPiecesPer8 = copyFromConfig.MgKingSafetyDefendingPiecesPer8;

        // Copy piece location values.
        MgPawnAdvancement = copyFromConfig.MgPawnAdvancement;
        EgPawnAdvancement = copyFromConfig.EgPawnAdvancement;
        MgPawnSquareCentrality = copyFromConfig.MgPawnSquareCentrality;
        EgPawnSquareCentrality = copyFromConfig.EgPawnSquareCentrality;
        MgPawnFileCentrality = copyFromConfig.MgPawnFileCentrality;
        EgPawnFileCentrality = copyFromConfig.EgPawnFileCentrality;
        MgPawnCorner = copyFromConfig.MgPawnCorner;
        EgPawnCorner = copyFromConfig.EgPawnCorner;

        MgKnightAdvancement = copyFromConfig.MgKnightAdvancement;
        EgKnightAdvancement = copyFromConfig.EgKnightAdvancement;
        MgKnightSquareCentrality = copyFromConfig.MgKnightSquareCentrality;
        EgKnightSquareCentrality = copyFromConfig.EgKnightSquareCentrality;
        MgKnightFileCentrality = copyFromConfig.MgKnightFileCentrality;
        EgKnightFileCentrality = copyFromConfig.EgKnightFileCentrality;
        MgKnightCorner = copyFromConfig.MgKnightCorner;
        EgKnightCorner = copyFromConfig.EgKnightCorner;

        MgBishopAdvancement = copyFromConfig.MgBishopAdvancement;
        EgBishopAdvancement = copyFromConfig.EgBishopAdvancement;
        MgBishopSquareCentrality = copyFromConfig.MgBishopSquareCentrality;
        EgBishopSquareCentrality = copyFromConfig.EgBishopSquareCentrality;
        MgBishopFileCentrality = copyFromConfig.MgBishopFileCentrality;
        EgBishopFileCentrality = copyFromConfig.EgBishopFileCentrality;
        MgBishopCorner = copyFromConfig.MgBishopCorner;
        EgBishopCorner = copyFromConfig.EgBishopCorner;

        MgRookAdvancement = copyFromConfig.MgRookAdvancement;
        EgRookAdvancement = copyFromConfig.EgRookAdvancement;
        MgRookSquareCentrality = copyFromConfig.MgRookSquareCentrality;
        EgRookSquareCentrality = copyFromConfig.EgRookSquareCentrality;
        MgRookFileCentrality = copyFromConfig.MgRookFileCentrality;
        EgRookFileCentrality = copyFromConfig.EgRookFileCentrality;
        MgRookCorner = copyFromConfig.MgRookCorner;
        EgRookCorner = copyFromConfig.EgRookCorner;

        MgQueenAdvancement = copyFromConfig.MgQueenAdvancement;
        EgQueenAdvancement = copyFromConfig.EgQueenAdvancement;
        MgQueenSquareCentrality = copyFromConfig.MgQueenSquareCentrality;
        EgQueenSquareCentrality = copyFromConfig.EgQueenSquareCentrality;
        MgQueenFileCentrality = copyFromConfig.MgQueenFileCentrality;
        EgQueenFileCentrality = copyFromConfig.EgQueenFileCentrality;
        MgQueenCorner = copyFromConfig.MgQueenCorner;
        EgQueenCorner = copyFromConfig.EgQueenCorner;

        MgKingAdvancement = copyFromConfig.MgKingAdvancement;
        EgKingAdvancement = copyFromConfig.EgKingAdvancement;
        MgKingSquareCentrality = copyFromConfig.MgKingSquareCentrality;
        EgKingSquareCentrality = copyFromConfig.EgKingSquareCentrality;
        MgKingFileCentrality = copyFromConfig.MgKingFileCentrality;
        EgKingFileCentrality = copyFromConfig.EgKingFileCentrality;
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