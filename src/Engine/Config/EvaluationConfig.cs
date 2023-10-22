// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
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
    public int EgPawnMaterial = 168;
    public int MgKnightMaterial = 449;
    public int EgKnightMaterial = 577;
    public int MgBishopMaterial = 470;
    public int EgBishopMaterial = 614;
    public int MgRookMaterial = 547;
    public int EgRookMaterial = 1112;
    public int MgQueenMaterial = 1368;
    public int EgQueenMaterial = 1958;

    // Passed Pawns
    public int PassedPawnPowerPer128 = 322;
    public int MgPassedPawnScalePer128 = 85;
    public int EgPassedPawnScalePer128 = 379;
    public int EgFreePassedPawnScalePer128 = 698;
    public int EgConnectedPassedPawnScalePer128 = 81;
    public int EgKingEscortedPassedPawn = 14;
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial); // Incentivize engine to promote pawns.

    // King Safety
    public int MgKingSafetyPowerPer128 = 241;
    public int MgKingSafetyScalePer128 = 56;
    public int MgKingSafetyKnightAttackOuterRingPer8 = 32;
    public int MgKingSafetyKnightAttackInnerRingPer8 = 25;
    public int MgKingSafetyBishopAttackOuterRingPer8 = 12;
    public int MgKingSafetyBishopAttackInnerRingPer8 = 30;
    public int MgKingSafetyRookAttackOuterRingPer8 = 13;
    public int MgKingSafetyRookAttackInnerRingPer8 = 17;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 21;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 22;
    public int MgKingSafetySemiOpenFilePer8 = 9;
    public int MgKingSafetyPawnShieldPer8 = 15;
    public int MgKingSafetyDefendingPiecesPer8 = 16;

    // Pawn Location
    public int MgPawnAdvancement = 2;
    public int EgPawnAdvancement = 11;
    public int MgPawnCentrality = 1;
    public int EgPawnCentrality = -7;

    // Knight Location 
    public int MgKnightAdvancement = 2;
    public int EgKnightAdvancement = 7;
    public int MgKnightCentrality = 6;
    public int EgKnightCentrality = 17;
    public int MgKnightCorner = -1;
    public int EgKnightCorner = -25;

    // Bishop Location
    public int MgBishopAdvancement = -2;
    public int EgBishopAdvancement = 5;
    public int MgBishopCentrality = 11;
    public int EgBishopCentrality = 5;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = -5;

    // Rook Location
    public int MgRookAdvancement = 5;
    public int EgRookAdvancement = 14;
    public int MgRookCentrality = 9;
    public int EgRookCentrality = -2;
    public int MgRookCorner = -1;
    public int EgRookCorner = -4;

    // Queen Location
    public int MgQueenAdvancement = -8;
    public int EgQueenAdvancement = 31;
    public int MgQueenCentrality = 2;
    public int EgQueenCentrality = 16;
    public int MgQueenCorner = -4;
    public int EgQueenCorner = -13;

    // King Location
    public int MgKingAdvancement = -9;
    public int EgKingAdvancement = 16;
    public int MgKingCentrality = -2;
    public int EgKingCentrality = 20;
    public int MgKingCorner = 10;
    public int EgKingCorner = -3;

    // Piece Mobility
    public int PieceMobilityPowerPer128 = 109;
    public int MgKnightMobilityScale = 59;
    public int EgKnightMobilityScale = 39;
    public int MgBishopMobilityScale = 61;
    public int EgBishopMobilityScale = 161;
    public int MgRookMobilityScale = 89;
    public int EgRookMobilityScale = 153;
    public int MgQueenMobilityScale = 86;
    public int EgQueenMobilityScale = 75;

    // Pawn Structure
    public int MgIsolatedPawn = 11;
    public int EgIsolatedPawn = 36;
    public int MgDoubledPawn = 12;
    public int EgDoubledPawn = 25;

    // Threats
    public int MgPawnThreatenMinor = 50;
    public int EgPawnThreatenMinor = 42;
    public int MgPawnThreatenMajor = 72;
    public int EgPawnThreatenMajor = 67;
    public int MgMinorThreatenMajor = 29;
    public int EgMinorThreatenMajor = 37;

    // Minor Pieces
    public int MgBishopPair = 33;
    public int EgBishopPair = 97;
    public int MgKnightOutpost = 56;
    public int EgKnightOutpost = 34;
    public int MgBishopOutpost = 30;
    public int EgBishopOutpost = 20;

    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 20;
    public int EgRook7thRank = 52;
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