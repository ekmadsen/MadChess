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
    public int EgPawnMaterial = 196;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 667;
    public int MgBishopMaterial = 325;
    public int EgBishopMaterial = 693;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 1173;
    public int MgQueenMaterial = 950;
    public int EgQueenMaterial = 2200;

    // Passed Pawns
    public int MgPassedPawnScalePer128 = 156;
    public int EgPassedPawnScalePer128 = 328;
    public int MgFreePassedPawnScalePer128 = 197;
    public int EgFreePassedPawnScalePer128 = 675;
    public int MgConnectedPassedPawnScalePer128 = 15;
    public int EgConnectedPassedPawnScalePer128 = 113;
    public int EgKingEscortedPassedPawn = 18;
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial); // Incentivize engine to promote pawns.

    // King Safety
    public int MgKingSafetyScalePer128 = 29;
    public int MgKingSafetyKnightAttackOuterRingPer8 = 28;
    public int MgKingSafetyKnightAttackInnerRingPer8 = 18;
    public int MgKingSafetyKnightProximityPer8 = 4;
    public int MgKingSafetyBishopAttackOuterRingPer8 = 10;
    public int MgKingSafetyBishopAttackInnerRingPer8 = 26;
    public int MgKingSafetyBishopProximityPer8 = 4;
    public int MgKingSafetyRookAttackOuterRingPer8 = 15;
    public int MgKingSafetyRookAttackInnerRingPer8 = 18;
    public int MgKingSafetyRookProximityPer8 = 6;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 23;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 16;
    public int MgKingSafetyQueenProximityPer8 = 11;
    public int MgKingSafetySemiOpenFilePer8 = 25;
    public int MgKingSafetyPawnShieldPer8 = 14;
    public int MgKingSafetyDefendingPiecesPer8 = 18;

    // Pawn Location
    public int MgPawnAdvancement = 0;
    public int EgPawnAdvancement = 13;
    public int MgPawnSquareCentrality = 7;
    public int EgPawnSquareCentrality = -33;
    public int MgPawnFileCentrality = -10;
    public int EgPawnFileCentrality = 22;
    public int MgPawnCorner = -8;
    public int EgPawnCorner = 10;

    // Knight Location 
    public int MgKnightAdvancement = 5;
    public int EgKnightAdvancement = 12;
    public int MgKnightSquareCentrality = -3;
    public int EgKnightSquareCentrality = 7;
    public int MgKnightFileCentrality = 8;
    public int EgKnightFileCentrality = 2;
    public int MgKnightCorner = 7;
    public int EgKnightCorner = -24;

    // Bishop Location
    public int MgBishopAdvancement = 0;
    public int EgBishopAdvancement = 7;
    public int MgBishopSquareCentrality = 8;
    public int EgBishopSquareCentrality = 1;
    public int MgBishopFileCentrality = -2;
    public int EgBishopFileCentrality = 7;
    public int MgBishopCorner = 11;
    public int EgBishopCorner = -1;

    // Rook Location
    public int MgRookAdvancement = 6;
    public int EgRookAdvancement = 14;
    public int MgRookSquareCentrality = -20;
    public int EgRookSquareCentrality = 6;
    public int MgRookFileCentrality = 9;
    public int EgRookFileCentrality = -6;
    public int MgRookCorner = -7;
    public int EgRookCorner = -2;

    // Queen Location
    public int MgQueenAdvancement = -11;
    public int EgQueenAdvancement = 30;
    public int MgQueenSquareCentrality = -6;
    public int EgQueenSquareCentrality = 18;
    public int MgQueenFileCentrality = 1;
    public int EgQueenFileCentrality = 5;
    public int MgQueenCorner = 2;
    public int EgQueenCorner = 2;

    // King Location
    public int MgKingAdvancement = 19;
    public int EgKingAdvancement = 17;
    public int MgKingSquareCentrality = 19;
    public int EgKingSquareCentrality = 12;
    public int MgKingFileCentrality = 4;
    public int EgKingFileCentrality = 3;
    public int MgKingCorner = 7;
    public int EgKingCorner = -4;

    // Piece Mobility
    public int MgKnightMobilityScale = 54;
    public int EgKnightMobilityScale = 186;
    public int MgBishopMobilityScale = 54;
    public int EgBishopMobilityScale = 213;
    public int MgRookMobilityScale = 78;
    public int EgRookMobilityScale = 179;
    public int MgQueenMobilityScale = 78;
    public int EgQueenMobilityScale = 127;

    // Pawn Structure
    public int MgIsolatedPawn = 25;
    public int EgIsolatedPawn = 49;
    public int MgDoubledPawn = 55;
    public int EgDoubledPawn = 9;

    // Threats
    public int MgPawnThreatenMinor = 36;
    public int EgPawnThreatenMinor = 64;
    public int MgPawnThreatenMajor = 53;
    public int EgPawnThreatenMajor = 69;
    public int MgMinorThreatenMajor = 44;
    public int EgMinorThreatenMajor = 26;

    // Minor Pieces
    public int MgBishopPair = 34;
    public int EgBishopPair = 139;
    public int MgKnightOutpost = 31;
    public int EgKnightOutpost = 40;
    public int MgBishopOutpost = 48;
    public int EgBishopOutpost = 14;

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