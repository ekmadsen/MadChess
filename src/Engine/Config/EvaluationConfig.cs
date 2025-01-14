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
    public int EgPawnMaterial = 192;
    public int MgKnightMaterial = 460;
    public int EgKnightMaterial = 582;
    public int MgBishopMaterial = 488;
    public int EgBishopMaterial = 600;
    public int MgRookMaterial = 585;
    public int EgRookMaterial = 1113;
    public int MgQueenMaterial = 1405;
    public int EgQueenMaterial = 1939;

    // Passed Pawns
    public int PassedPawnPowerPer128 = 327;
    public int MgPassedPawnScalePer128 = 162;
    public int EgPassedPawnScalePer128 = 301;
    public int MgFreePassedPawnScalePer128 = 207;
    public int EgFreePassedPawnScalePer128 = 610;
    public int MgConnectedPassedPawnScalePer128 = 4;
    public int EgConnectedPassedPawnScalePer128 = 119;
    public int EgKingEscortedPassedPawn = 19;
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial); // Incentivize engine to promote pawns.

    // King Safety
    public int MgKingSafetyPowerPer128 = 260;
    public int MgKingSafetyScalePer128 = 37;
    public int MgKingSafetyKnightAttackOuterRingPer8 = 28;
    public int MgKingSafetyKnightAttackInnerRingPer8 = 22;
    public int MgKingSafetyKnightProximityPer8 = 0;
    public int MgKingSafetyBishopAttackOuterRingPer8 = 12;
    public int MgKingSafetyBishopAttackInnerRingPer8 = 26;
    public int MgKingSafetyBishopProximityPer8 = 0;
    public int MgKingSafetyRookAttackOuterRingPer8 = 14;
    public int MgKingSafetyRookAttackInnerRingPer8 = 18;
    public int MgKingSafetyRookProximityPer8 = 3;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 19;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 16;
    public int MgKingSafetyQueenProximityPer8 = 12;
    public int MgKingSafetySemiOpenFilePer8 = 21;
    public int MgKingSafetyPawnShieldPer8 = 15;
    public int MgKingSafetyDefendingPiecesPer8 = 13;

    // Pawn Location
    public int MgPawnAdvancement = 4;
    public int EgPawnAdvancement = 12;
    public int MgPawnSquareCentrality = 2;
    public int EgPawnSquareCentrality = -18;
    public int MgPawnFileCentrality = -4;
    public int EgPawnFileCentrality = 9;
    public int MgPawnCorner = -4;
    public int EgPawnCorner = 9;

    // Knight Location 
    public int MgKnightAdvancement = 1;
    public int EgKnightAdvancement = 11;
    public int MgKnightSquareCentrality = 2;
    public int EgKnightSquareCentrality = 20;
    public int MgKnightFileCentrality = 0;
    public int EgKnightFileCentrality = -4;
    public int MgKnightCorner = -2;
    public int EgKnightCorner = -31;

    // Bishop Location
    public int MgBishopAdvancement = -1;
    public int EgBishopAdvancement = 7;
    public int MgBishopSquareCentrality = 10;
    public int EgBishopSquareCentrality = 3;
    public int MgBishopFileCentrality = -7;
    public int EgBishopFileCentrality = 6;
    public int MgBishopCorner = 5;
    public int EgBishopCorner = -5;

    // Rook Location
    public int MgRookAdvancement = 9;
    public int EgRookAdvancement = 10;
    public int MgRookSquareCentrality = -11;
    public int EgRookSquareCentrality = 2;
    public int MgRookFileCentrality = 14;
    public int EgRookFileCentrality = -9;
    public int MgRookCorner = -3;
    public int EgRookCorner = -8;

    // Queen Location
    public int MgQueenAdvancement = -16;
    public int EgQueenAdvancement = 29;
    public int MgQueenSquareCentrality = -3;
    public int EgQueenSquareCentrality = 25;
    public int MgQueenFileCentrality = 1;
    public int EgQueenFileCentrality = -8;
    public int MgQueenCorner = 0;
    public int EgQueenCorner = -6;

    // King Location
    public int MgKingAdvancement = 4;
    public int EgKingAdvancement = 20;
    public int MgKingSquareCentrality = 12;
    public int EgKingSquareCentrality = 18;
    public int MgKingFileCentrality = 4;
    public int EgKingFileCentrality = 3;
    public int MgKingCorner = 3;
    public int EgKingCorner = -2;

    // Piece Mobility
    public int PieceMobilityPowerPer128 = 96;
    public int MgKnightMobilityScale = 64;
    public int EgKnightMobilityScale = 96;
    public int MgBishopMobilityScale = 68;
    public int EgBishopMobilityScale = 175;
    public int MgRookMobilityScale = 96;
    public int EgRookMobilityScale = 125;
    public int MgQueenMobilityScale = 95;
    public int EgQueenMobilityScale = 126;

    // Pawn Structure
    public int MgIsolatedPawn = 24;
    public int EgIsolatedPawn = 49;
    public int MgDoubledPawn = 18;
    public int EgDoubledPawn = 32;

    // Threats
    public int MgPawnThreatenMinor = 42;
    public int EgPawnThreatenMinor = 46;
    public int MgPawnThreatenMajor = 68;
    public int EgPawnThreatenMajor = 62;
    public int MgMinorThreatenMajor = 48;
    public int EgMinorThreatenMajor = 46;

    // Minor Pieces
    public int MgBishopPair = 33;
    public int EgBishopPair = 134;
    public int MgKnightOutpost = 40;
    public int EgKnightOutpost = 35;
    public int MgBishopOutpost = 41;
    public int EgBishopOutpost = 23;

    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 19;
    public int EgRook7thRank = 11;
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
        MgKingSafetyPowerPer128 = copyFromConfig.MgKingSafetyPowerPer128;
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