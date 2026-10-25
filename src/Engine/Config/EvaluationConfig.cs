// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2026.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


#pragma warning disable IDE0047
namespace ErikTheCoder.MadChess.Engine.Config;


public sealed class EvaluationConfig
{
    // ReSharper disable ConvertToConstant.Global
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    // ReSharper disable RedundantDefaultMemberInitializer

    // Material
    public int MgPawnMaterial = 100;
    public int EgPawnMaterial = 198;
    public int MgKnightMaterial = 374;
    public int EgKnightMaterial = 745;
    public int MgBishopMaterial = 397;
    public int EgBishopMaterial = 759;
    public int MgRookMaterial = 488;
    public int EgRookMaterial = 1371;
    public int MgQueenMaterial = 1217;
    public int EgQueenMaterial = 2384;
    public int EgMaterialDisadvantagePawns = 124;
    public int EgMaterialDisadvantageAtLeastOnePawn = 96;

    // Passed Pawns
    public int PassedPawnPowerPer128 = 332;
    public int MgPassedPawnScalePer128 = 181;
    public int EgPassedPawnScalePer128 = 275;
    public int MgFreePassedPawnScalePer128 = 204;
    public int EgFreePassedPawnScalePer128 = 604;
    public int MgConnectedPassedPawnScalePer128 = 4;
    public int EgConnectedPassedPawnScalePer128 = 99;
    public int EgKingEscortedPassedPawn = 20;
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial); // Incentivize engine to promote pawns.

    // King Safety
    public int MgKingSafetyPowerPer128 = 260;
    public int MgKingSafetyScalePer128 = 37;
    public int MgKingSafetyKnightAttackOuterRingPer8 = 28;
    public int MgKingSafetyKnightAttackInnerRingPer8 = 23;
    public int MgKingSafetyKnightProximityPer8 = 0;
    public int MgKingSafetyBishopAttackOuterRingPer8 = 10;
    public int MgKingSafetyBishopAttackInnerRingPer8 = 25;
    public int MgKingSafetyBishopProximityPer8 = 1;
    public int MgKingSafetyRookAttackOuterRingPer8 = 12;
    public int MgKingSafetyRookAttackInnerRingPer8 = 18;
    public int MgKingSafetyRookProximityPer8 = 4;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 19;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 15;
    public int MgKingSafetyQueenProximityPer8 = 13;
    public int MgKingSafetySemiOpenFilePer8 = 18;
    public int MgKingSafetyPawnShieldPer8 = 15;
    public int MgKingSafetyDefendingPiecesPer8 = 14;

    // Pawn Location
    public int MgPawnAdvancement = -3;
    public int EgPawnAdvancement = 18;
    public int MgPawnSquareCentrality = 9;
    public int EgPawnSquareCentrality = -35;
    public int MgPawnFileCentrality = -10;
    public int EgPawnFileCentrality = 25;
    public int MgPawnCorner = -6;
    public int EgPawnCorner = 12;

    // Knight Location 
    public int MgKnightAdvancement = 1;
    public int EgKnightAdvancement = 15;
    public int MgKnightSquareCentrality = 4;
    public int EgKnightSquareCentrality = 18;
    public int MgKnightFileCentrality = 0;
    public int EgKnightFileCentrality = -4;
    public int MgKnightCorner = -3;
    public int EgKnightCorner = -28;

    // Bishop Location
    public int MgBishopAdvancement = -3;
    public int EgBishopAdvancement = 7;
    public int MgBishopSquareCentrality = 15;
    public int EgBishopSquareCentrality = 1;
    public int MgBishopFileCentrality = -9;
    public int EgBishopFileCentrality = 8;
    public int MgBishopCorner = 3;
    public int EgBishopCorner = -4;

    // Rook Location
    public int MgRookAdvancement = 7;
    public int EgRookAdvancement = 12;
    public int MgRookSquareCentrality = -17;
    public int EgRookSquareCentrality = 4;
    public int MgRookFileCentrality = 15;
    public int EgRookFileCentrality = -10;
    public int MgRookCorner = -3;
    public int EgRookCorner = -4;

    // Queen Location
    public int MgQueenAdvancement = -18;
    public int EgQueenAdvancement = 31;
    public int MgQueenSquareCentrality = -3;
    public int EgQueenSquareCentrality = 16;
    public int MgQueenFileCentrality = -3;
    public int EgQueenFileCentrality = -4;
    public int MgQueenCorner = 2;
    public int EgQueenCorner = -8;

    // King Location
    public int MgKingAdvancement = 8;
    public int EgKingAdvancement = 23;
    public int MgKingSquareCentrality = 10;
    public int EgKingSquareCentrality = 11;
    public int MgKingFileCentrality = 2;
    public int EgKingFileCentrality = 7;
    public int MgKingCorner = 1;
    public int EgKingCorner = 0;

    // Piece Mobility
    public int PieceMobilityPowerPer128 = 94;
    public int MgKnightMobilityScale = 55;
    public int EgKnightMobilityScale = 118;
    public int MgBishopMobilityScale = 65;
    public int EgBishopMobilityScale = 184;
    public int MgRookMobilityScale = 100;
    public int EgRookMobilityScale = 136;
    public int MgQueenMobilityScale = 81;
    public int EgQueenMobilityScale = 219;

    // Pawn Structure
    public int MgIsolatedPawn = 14;
    public int EgIsolatedPawn = 40;
    public int MgDoubledPawn = 20;
    public int EgDoubledPawn = 22;

    // Threats
    public int MgPawnThreatenMinor = 38;
    public int EgPawnThreatenMinor = 47;
    public int MgPawnThreatenMajor = 59;
    public int EgPawnThreatenMajor = 32;
    public int MgMinorThreatenMajor = 50;
    public int EgMinorThreatenMajor = 48;

    // Minor Pieces
    public int MgBishopPair = 19;
    public int EgBishopPair = 184;
    public int MgKnightOutpost = 36;
    public int EgKnightOutpost = 33;
    public int MgBishopOutpost = 46;
    public int EgBishopOutpost = 19;

    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 8;
    public int EgRook7thRank = 23;
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
        EgMaterialDisadvantagePawns = copyFromConfig.EgMaterialDisadvantagePawns;
        EgMaterialDisadvantageAtLeastOnePawn = copyFromConfig.EgMaterialDisadvantageAtLeastOnePawn;

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