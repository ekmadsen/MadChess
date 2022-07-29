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
    public int EgPawnMaterial = 130;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 512;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 510;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 850;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 1563;
    // Incentivize engine to promote pawns.
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial);
    // Pawn Location
    public int MgPawnAdvancement = 5;
    public int EgPawnAdvancement = 4;
    public int MgPawnCentrality = 8;
    public int EgPawnCentrality = -4;
    // Knight Location 
    public int MgKnightAdvancement = 3;
    public int EgKnightAdvancement = 6;
    public int MgKnightCentrality = 20;
    public int EgKnightCentrality = 16;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -22;
    // Bishop Location
    public int MgBishopAdvancement = -4;
    public int EgBishopAdvancement = 5;
    public int MgBishopCentrality = 21;
    public int EgBishopCentrality = 3;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = -1;
    // Rook Location
    public int MgRookAdvancement = -1;
    public int EgRookAdvancement = 11;
    public int MgRookCentrality = 16;
    public int EgRookCentrality = 3;
    public int MgRookCorner = 0;
    public int EgRookCorner = 3;
    // Queen Location
    public int MgQueenAdvancement = -18;
    public int EgQueenAdvancement = 25;
    public int MgQueenCentrality = 3;
    public int EgQueenCentrality = 0;
    public int MgQueenCorner = -1;
    public int EgQueenCorner = -9;
    // King Location
    public int MgKingAdvancement = -1;
    public int EgKingAdvancement = 15;
    public int MgKingCentrality = 0;
    public int EgKingCentrality = 11;
    public int MgKingCorner = 1;
    public int EgKingCorner = -1;
    // Pawn Structure
    public int MgIsolatedPawn = 20;
    public int EgIsolatedPawn = 27;
    public int MgDoubledPawn = 30;
    public int EgDoubledPawn = 16;
    // Passed Pawns
    public int PassedPawnPowerPer128 = 311;
    public int MgPassedPawnScalePer128 = 181;
    public int EgPassedPawnScalePer128 = 213;
    public int EgFreePassedPawnScalePer128 = 626;
    public int EgKingEscortedPassedPawn = 10;
    // Piece Mobility
    public int PieceMobilityPowerPer128 = 88;
    public int MgKnightMobilityScale = 0;
    public int EgKnightMobilityScale = 30;
    public int MgBishopMobilityScale = 37;
    public int EgBishopMobilityScale = 109;
    public int MgRookMobilityScale = 48;
    public int EgRookMobilityScale = 89;
    public int MgQueenMobilityScale = 136;
    public int EgQueenMobilityScale = 27;
    // King Safety
    public int MgKingSafetyPowerPer128 = 240;
    public int MgKingSafetyScalePer128 = 96;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 8;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 31;
    public int MgKingSafetyRookAttackOuterRingPer8 = 10;
    public int MgKingSafetyRookAttackInnerRingPer8 = 16;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 14;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 22;
    public int MgKingSafetySemiOpenFilePer8 = 16;
    public int MgKingSafetyPawnShieldPer8 = 24;
    // Threats
    public int MgPawnThreatenMinor = 43;
    public int EgPawnThreatenMinor = 49;
    public int MgPawnThreatenMajor = 71;
    public int EgPawnThreatenMajor = 33;
    public int MgMinorThreatenMajor = 33;
    public int EgMinorThreatenMajor = 26;
    // Minor Pieces
    public int MgBishopPair = 27;
    public int EgBishopPair = 67;
    public int MgKnightOutpost = 112;
    public int EgKnightOutpost = 0;
    public int MgBishopOutpost = 7;
    public int EgBishopOutpost = 16;
    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 59;
    public int EgRook7thRank = 19;
    public int MgQueen7thRank = 0;
    public int EgQueen7thRank = 3;
    // ReSharper restore InconsistentNaming
    // Endgame Scale
    public int EgScaleMinorAdvantage = 21;
    public int EgScaleOppBishopsPerPassedPawn = 85;
    public int EgScalePerPawn = 52;
    public int EgScale = 128;
    // Limit Strength
    public bool LimitedStrength = false;
    public int LsPieceLocationPer128 = 128;
    public int LsPassedPawnsPer128 = 128;
    public int LsPieceMobilityPer128 = 128;
    public int LsKingSafetyPer128 = 128;
    public int LsMinorPiecesPer128 = 128;
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
        // Copy pawn structure values.
        MgIsolatedPawn = copyFromConfig.MgIsolatedPawn;
        EgIsolatedPawn = copyFromConfig.EgIsolatedPawn;
        MgDoubledPawn = copyFromConfig.MgDoubledPawn;
        EgDoubledPawn = copyFromConfig.EgDoubledPawn;
        // Copy passed pawn values.
        PassedPawnPowerPer128 = copyFromConfig.PassedPawnPowerPer128;
        MgPassedPawnScalePer128 = copyFromConfig.MgPassedPawnScalePer128;
        EgPassedPawnScalePer128 = copyFromConfig.EgPassedPawnScalePer128;
        EgFreePassedPawnScalePer128 = copyFromConfig.EgFreePassedPawnScalePer128;
        EgKingEscortedPassedPawn = copyFromConfig.EgKingEscortedPassedPawn;
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
        // Copy king safety values.
        MgKingSafetyPowerPer128 = copyFromConfig.MgKingSafetyPowerPer128;
        MgKingSafetyMinorAttackOuterRingPer8 = copyFromConfig.MgKingSafetyMinorAttackOuterRingPer8;
        MgKingSafetyMinorAttackInnerRingPer8 = copyFromConfig.MgKingSafetyMinorAttackInnerRingPer8;
        MgKingSafetyRookAttackOuterRingPer8 = copyFromConfig.MgKingSafetyRookAttackOuterRingPer8;
        MgKingSafetyRookAttackInnerRingPer8 = copyFromConfig.MgKingSafetyRookAttackInnerRingPer8;
        MgKingSafetyQueenAttackOuterRingPer8 = copyFromConfig.MgKingSafetyQueenAttackOuterRingPer8;
        MgKingSafetyQueenAttackInnerRingPer8 = copyFromConfig.MgKingSafetyQueenAttackInnerRingPer8;
        MgKingSafetySemiOpenFilePer8 = copyFromConfig.MgKingSafetySemiOpenFilePer8;
        MgKingSafetyPawnShieldPer8 = copyFromConfig.MgKingSafetyPawnShieldPer8;
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
        MgQueen7thRank = copyFromConfig.MgQueen7thRank;
        EgQueen7thRank = copyFromConfig.EgQueen7thRank;
        // Copy endgame scale values.
        EgScaleMinorAdvantage = copyFromConfig.EgScaleMinorAdvantage;
        EgScaleOppBishopsPerPassedPawn = copyFromConfig.EgScaleOppBishopsPerPassedPawn;
        EgScalePerPawn = copyFromConfig.EgScalePerPawn;
        EgScale = copyFromConfig.EgScale;
        // Copy limit strength values.
        LimitedStrength = copyFromConfig.LimitedStrength;
        LsPieceLocationPer128 = copyFromConfig.LsPieceLocationPer128;
        LsPassedPawnsPer128 = copyFromConfig.LsPassedPawnsPer128;
        LsPieceMobilityPer128 = copyFromConfig.LsPieceMobilityPer128;
        LsKingSafetyPer128 = copyFromConfig.LsKingSafetyPer128;
        LsMinorPiecesPer128 = copyFromConfig.LsMinorPiecesPer128;
    }
}