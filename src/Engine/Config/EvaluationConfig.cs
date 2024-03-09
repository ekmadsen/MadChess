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
    public int EgPawnMaterial = 179;
    public int MgKnightMaterial = 493;
    public int EgKnightMaterial = 550;
    public int MgBishopMaterial = 517;
    public int EgBishopMaterial = 591;
    public int MgRookMaterial = 593;
    public int EgRookMaterial = 1088;
    public int MgQueenMaterial = 1537;
    public int EgQueenMaterial = 1844;

    // Passed Pawns
    public int PassedPawnPowerPer128 = 322;
    public int MgPassedPawnScalePer128 = 78;
    public int EgPassedPawnScalePer128 = 378;
    public int EgFreePassedPawnScalePer128 = 685;
    public int EgConnectedPassedPawnScalePer128 = 66;
    public int EgKingEscortedPassedPawn = 13;
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial); // Incentivize engine to promote pawns.

    // King Safety
    public int MgKingSafetyPowerPer128 = 253;
    public int MgKingSafetyScalePer128 = 43;
    public int MgKingSafetyKnightAttackOuterRingPer8 = 31;
    public int MgKingSafetyKnightAttackInnerRingPer8 = 23;
    public int MgKingSafetyKnightProximityPer8 = 0;
    public int MgKingSafetyBishopAttackOuterRingPer8 = 14;
    public int MgKingSafetyBishopAttackInnerRingPer8 = 30;
    public int MgKingSafetyBishopProximityPer8 = 0;
    public int MgKingSafetyRookAttackOuterRingPer8 = 15;
    public int MgKingSafetyRookAttackInnerRingPer8 = 17;
    public int MgKingSafetyRookProximityPer8 = 0;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 16;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 19;
    public int MgKingSafetyQueenProximityPer8 = 7;
    public int MgKingSafetySemiOpenFilePer8 = 8;
    public int MgKingSafetyPawnShieldPer8 = 17;
    public int MgKingSafetyDefendingPiecesPer8 = 10;

    // Pawn Location
    public int MgPawnAdvancement = 1;
    public int EgPawnAdvancement = 11;
    public int MgPawnCentrality = 0;
    public int EgPawnCentrality = -9;

    // Knight Location 
    public int MgKnightAdvancement = 4;
    public int EgKnightAdvancement = 8;
    public int MgKnightCentrality = 5;
    public int EgKnightCentrality = 20;
    public int MgKnightCorner = -2;
    public int EgKnightCorner = -31;

    // Bishop Location
    public int MgBishopAdvancement = -2;
    public int EgBishopAdvancement = 4;
    public int MgBishopCentrality = 10;
    public int EgBishopCentrality = 6;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = -7;

    // Rook Location
    public int MgRookAdvancement = 6;
    public int EgRookAdvancement = 13;
    public int MgRookCentrality = 14;
    public int EgRookCentrality = -2;
    public int MgRookCorner = -1;
    public int EgRookCorner = -2;

    // Queen Location
    public int MgQueenAdvancement = -7;
    public int EgQueenAdvancement = 32;
    public int MgQueenCentrality = 3;
    public int EgQueenCentrality = 24;
    public int MgQueenCorner = -4;
    public int EgQueenCorner = -11;

    // King Location
    public int MgKingAdvancement = -3;
    public int EgKingAdvancement = 15;
    public int MgKingCentrality = 0;
    public int EgKingCentrality = 23;
    public int MgKingCorner = 1;
    public int EgKingCorner = -4;

    // Piece Mobility
    public int PieceMobilityPowerPer128 = 100;
    public int MgKnightMobilityScale = 77;
    public int EgKnightMobilityScale = 39;
    public int MgBishopMobilityScale = 62;
    public int EgBishopMobilityScale = 185;
    public int MgRookMobilityScale = 25;  // Orig: 85, AW 8.3.: 25
    public int EgRookMobilityScale = 154;
    public int MgQueenMobilityScale = 87;
    public int EgQueenMobilityScale = 78;

    // Pawn Structure
    public int MgIsolatedPawn = 12;
    public int EgIsolatedPawn = 36;
    public int MgDoubledPawn = 10;
    public int EgDoubledPawn = 27;

    // Threats
    public int MgPawnThreatenMinor = 50;
    public int EgPawnThreatenMinor = 44;
    public int MgPawnThreatenMajor = 59;
    public int EgPawnThreatenMajor = 62;
    public int MgMinorThreatenMajor = 31;
    public int EgMinorThreatenMajor = 35;

    // Minor Pieces
    public int MgBishopPair = 35;
    public int EgBishopPair = 86;
    public int MgKnightOutpost = 59;
    public int EgKnightOutpost = 36;
    public int MgBishopOutpost = 26;
    public int EgBishopOutpost = 22;

    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 20;
    public int EgRook7thRank = 53;
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
    public int LikesClosedPositionsPer128 = 0;
    public int LikesEndgamesPer128 = 0;
    public int NumberOfPieceSquareTable = 0;

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
        EgFreePassedPawnScalePer128 = copyFromConfig.EgFreePassedPawnScalePer128;
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
        LikesClosedPositionsPer128 = copyFromConfig.LikesClosedPositionsPer128;
        LikesEndgamesPer128 = copyFromConfig.LikesEndgamesPer128;
        NumberOfPieceSquareTable = copyFromConfig.NumberOfPieceSquareTable;
    }
}