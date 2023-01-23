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
    public int EgPawnMaterial = 153;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 620;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 655;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 1069;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 2058;

    // Passed Pawns
    public int PassedPawnPowerPer128 = 313;
    public int MgPassedPawnScalePer128 = 87;
    public int EgPassedPawnScalePer128 = 325;
    public int EgFreePassedPawnScalePer128 = 793;
    public int EgKingEscortedPassedPawn = 15;
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial); // Incentivize engine to promote pawns.

    // King Safety
    public int MgKingSafetyPowerPer128 = 253;
    public int MgKingSafetyScalePer128 = 49;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 17;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 21;
    public int MgKingSafetyRookAttackOuterRingPer8 = 8;
    public int MgKingSafetyRookAttackInnerRingPer8 = 14;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 14;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 21;
    public int MgKingSafetySemiOpenFilePer8 = 3;
    public int MgKingSafetyPawnShieldPer8 = 13;
    public int MgKingSafetyDefendingPiecesPer8 = 9;
    public int MgKingSafetyAttackingPiecesPer8 = 6;

    // Pawn Location
    public int MgPawnAdvancement = 0;
    public int EgPawnAdvancement = 9;
    public int MgPawnCentrality = 0;
    public int EgPawnCentrality = -6;

    // Knight Location 
    public int MgKnightAdvancement = 4;
    public int EgKnightAdvancement = 10;
    public int MgKnightCentrality = 6;
    public int EgKnightCentrality = 13;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -18;

    // Bishop Location
    public int MgBishopAdvancement = -6;
    public int EgBishopAdvancement = 9;
    public int MgBishopCentrality = 11;
    public int EgBishopCentrality = 3;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = 0;

    // Rook Location
    public int MgRookAdvancement = 1;
    public int EgRookAdvancement = 17;
    public int MgRookCentrality = 5;
    public int EgRookCentrality = -1;
    public int MgRookCorner = -3;
    public int EgRookCorner = -2;

    // Queen Location
    public int MgQueenAdvancement = -8;
    public int EgQueenAdvancement = 30;
    public int MgQueenCentrality = 0;
    public int EgQueenCentrality = 15;
    public int MgQueenCorner = -1;
    public int EgQueenCorner = -7;

    // King Location
    public int MgKingAdvancement = 0;
    public int EgKingAdvancement = 12;
    public int MgKingCentrality = 0;
    public int EgKingCentrality = 15;
    public int MgKingCorner = 8;
    public int EgKingCorner = -11;

    // Piece Mobility
    public int PieceMobilityPowerPer128 = 116;
    public int MgKnightMobilityScale = 54;
    public int EgKnightMobilityScale = 106;
    public int MgBishopMobilityScale = 58;
    public int EgBishopMobilityScale = 181;
    public int MgRookMobilityScale = 84;
    public int EgRookMobilityScale = 157;
    public int MgQueenMobilityScale = 88;
    public int EgQueenMobilityScale = 84;

    // Pawn Structure
    public int MgIsolatedPawn = 12;
    public int EgIsolatedPawn = 44;
    public int MgDoubledPawn = 31;
    public int EgDoubledPawn = 15;

    // Threats
    public int MgPawnThreatenMinor = 46;
    public int EgPawnThreatenMinor = 51;
    public int MgPawnThreatenMajor = 65;
    public int EgPawnThreatenMajor = 45;
    public int MgMinorThreatenMajor = 40;
    public int EgMinorThreatenMajor = 17;

    // Minor Pieces
    public int MgBishopPair = 40;
    public int EgBishopPair = 112;
    public int MgKnightOutpost = 45;
    public int EgKnightOutpost = 33;
    public int MgBishopOutpost = 39;
    public int EgBishopOutpost = 1;

    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 50;
    public int EgRook7thRank = 16;
    // ReSharper restore InconsistentNaming

    // Endgame Scale
    public int EgScaleMinorAdvantage = 3;
    public int EgScaleOppBishopsPerPassedPawn = 35;
    public int EgScalePerPawnAdvantage = 10;

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