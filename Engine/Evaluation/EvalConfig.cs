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
    public int EgPawnMaterial = 131;
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 526;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 547;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 853;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 1665;
    // Incentivize engine to promote pawns.
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * EgPawnMaterial);
    // Pawn Location
    public int MgPawnAdvancement = 0;
    public int EgPawnAdvancement = 4;
    public int MgPawnCentrality = 7;
    public int EgPawnCentrality = -4;
    // Knight Location 
    public int MgKnightAdvancement = 2;
    public int EgKnightAdvancement = 7;
    public int MgKnightCentrality = 21;
    public int EgKnightCentrality = 11;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -16;
    // Bishop Location
    public int MgBishopAdvancement = -2;
    public int EgBishopAdvancement = 3;
    public int MgBishopCentrality = 12;
    public int EgBishopCentrality = 1;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = 0;
    // Rook Location
    public int MgRookAdvancement = -6;
    public int EgRookAdvancement = 14;
    public int MgRookCentrality = 13;
    public int EgRookCentrality = -3;
    public int MgRookCorner = 0;
    public int EgRookCorner = 0;
    // Queen Location
    public int MgQueenAdvancement = -17;
    public int EgQueenAdvancement = 19;
    public int MgQueenCentrality = 1;
    public int EgQueenCentrality = 6;
    public int MgQueenCorner = 0;
    public int EgQueenCorner = -2;
    // King Location
    public int MgKingAdvancement = -49;
    public int EgKingAdvancement = 18;
    public int MgKingCentrality = -2;
    public int EgKingCentrality = 15;
    public int MgKingCorner = 1;
    public int EgKingCorner = -7;
    // Pawn Structure
    public int MgIsolatedPawn = 14;
    public int EgIsolatedPawn = 31;
    public int MgDoubledPawn = 28;
    public int EgDoubledPawn = 11;
    // Passed Pawns
    public int PassedPawnPowerPer128 = 327;
    public int MgPassedPawnScalePer128 = 146;
    public int EgPassedPawnScalePer128 = 237;
    public int EgFreePassedPawnScalePer128 = 493;
    public int EgKingEscortedPassedPawn = 11;
    // Piece Mobility
    public int PieceMobilityPowerPer128 = 117;
    public int MgKnightMobilityScale = 0;
    public int EgKnightMobilityScale = 33;
    public int MgBishopMobilityScale = 29;
    public int EgBishopMobilityScale = 144;
    public int MgRookMobilityScale = 65;
    public int EgRookMobilityScale = 94;
    public int MgQueenMobilityScale = 123;
    public int EgQueenMobilityScale = 66;
    // King Safety
    public int MgKingSafetyPowerPer128 = 236;
    public int MgKingSafetyScalePer128 = 89;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 7;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 24;
    public int MgKingSafetyRookAttackOuterRingPer8 = 9;
    public int MgKingSafetyRookAttackInnerRingPer8 = 16;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 15;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 20;
    public int MgKingSafetySemiOpenFilePer8 = 10;
    public int MgKingSafetyPawnShieldPer8 = 28;
    // Threats
    public int MgPawnThreatenMinor = 41;
    public int EgPawnThreatenMinor = 45;
    public int MgPawnThreatenMajor = 73;
    public int EgPawnThreatenMajor = 41;
    public int MgMinorThreatenMajor = 39;
    public int EgMinorThreatenMajor = 26;
    // Minor Pieces
    public int MgBishopPair = 55;
    public int EgBishopPair = 62;
    public int MgKnightOutpost = 26;
    public int EgKnightOutpost = 33;
    public int MgBishopOutpost = 31;
    public int EgBishopOutpost = 1;
    // Major Pieces
    // ReSharper disable InconsistentNaming
    public int MgRook7thRank = 51;
    public int EgRook7thRank = 21;
    public int MgQueen7thRank = 2;
    public int EgQueen7thRank = 0;
    // ReSharper restore InconsistentNaming
    // Endgame Scale
    public int EgScaleMinorAdvantage = 36;
    public int EgScaleOppBishopsPerPassedPawn = 56;
    public int EgScalePerPawn = 44;
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