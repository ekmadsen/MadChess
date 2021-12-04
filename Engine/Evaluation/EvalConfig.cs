// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine.Evaluation;


public sealed class EvalConfig
{
    // ReSharper disable ConvertToConstant.Global
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    // ReSharper disable RedundantDefaultMemberInitializer
    // Material
    public int MgKnightMaterial = 300;
    public int EgKnightMaterial = 492;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 511;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 810;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 1493;
    // Incentivize engine to promote pawns.
    // Also incentivize engine to eliminate enemy's last pawn in K vrs KQ or KR endgames (to trigger simple endgame scoring that pushes enemy king to a corner).
    // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * Eval.PawnMaterial);
    public int SimpleEndgame => 2 * UnstoppablePassedPawn;
    // Pawn Location
    public int MgPawnAdvancement = 6;
    public int EgPawnAdvancement = 5;
    public int MgPawnCentrality = 0;
    public int EgPawnCentrality = -4;
    // Knight Location 
    public int MgKnightAdvancement = 4;
    public int EgKnightAdvancement = 8;
    public int MgKnightCentrality = 12;
    public int EgKnightCentrality = 17;
    public int MgKnightCorner = 0;
    public int EgKnightCorner = -22;
    // Bishop Location
    public int MgBishopAdvancement = -3;
    public int EgBishopAdvancement = 2;
    public int MgBishopCentrality = 17;
    public int EgBishopCentrality = 6;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = -1;
    // Rook Location
    public int MgRookAdvancement = 5;
    public int EgRookAdvancement = 12;
    public int MgRookCentrality = 2;
    public int EgRookCentrality = -3;
    public int MgRookCorner = -10;
    public int EgRookCorner = 2;
    // Queen Location
    public int MgQueenAdvancement = -24;
    public int EgQueenAdvancement = 24;
    public int MgQueenCentrality = 5;
    public int EgQueenCentrality = 1;
    public int MgQueenCorner = 0;
    public int EgQueenCorner = -9;
    // King Location
    public int MgKingAdvancement = -1;
    public int EgKingAdvancement = 12;
    public int MgKingCentrality = -3;
    public int EgKingCentrality = 11;
    public int MgKingCorner = 2;
    public int EgKingCorner = -3;
    // Passed Pawns
    public int PassedPawnPowerPer128 = 302;
    public int MgPassedPawnScalePer128 = 78;
    public int EgPassedPawnScalePer128 = 260;
    public int EgFreePassedPawnScalePer128 = 517;
    public int EgKingEscortedPassedPawn = 14;
    // Piece Mobility
    public int PieceMobilityPowerPer128 = 86;
    public int MgKnightMobilityScale = 3;
    public int EgKnightMobilityScale = 4;
    public int MgBishopMobilityScale = 12;
    public int EgBishopMobilityScale = 128;
    public int MgRookMobilityScale = 44;
    public int EgRookMobilityScale = 73;
    public int MgQueenMobilityScale = 72;
    public int EgQueenMobilityScale = 115;
    // King Safety
    public int MgKingSafetyPowerPer128 = 223;
    public int MgKingSafetyScalePer128 = 95;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 11;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 36;
    public int MgKingSafetyRookAttackOuterRingPer8 = 18;
    public int MgKingSafetyRookAttackInnerRingPer8 = 22;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 21;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 26;
    public int MgKingSafetySemiOpenFilePer8 = 33;
    public int MgKingSafetyPawnShieldPer8 = 16;
    // Threats
    public int MgPawnThreatenMinor = 41;
    public int EgPawnThreatenMinor = 61;
    public int MgPawnThreatenMajor = 53;
    public int EgPawnThreatenMajor = 30;
    public int MgMinorThreatenMajor = 67;
    public int EgMinorThreatenMajor = 23;
    // Minor Pieces
    public int MgBishopPair = 44;
    public int EgBishopPair = 75;
    // Endgame Scale
    public int EgScaleBishopAdvantagePer128 = 31;
    public int EgScaleOppBishopsPerPassedPawn = 40;
    public int EgScaleWinningPerPawn = 47;
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
        MgKingSafetyScalePer128 = copyFromConfig.MgKingSafetyScalePer128;
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
        // Copy endgame scale values.
        EgScaleBishopAdvantagePer128 = copyFromConfig.EgScaleBishopAdvantagePer128;
        EgScaleOppBishopsPerPassedPawn = copyFromConfig.EgScaleOppBishopsPerPassedPawn;
        EgScaleWinningPerPawn = copyFromConfig.EgScaleWinningPerPawn;
        // Copy limit strength values.
        LimitedStrength = copyFromConfig.LimitedStrength;
        LsPieceLocationPer128 = copyFromConfig.LsPieceLocationPer128;
        LsPassedPawnsPer128 = copyFromConfig.PassedPawnPowerPer128;
        LsPieceMobilityPer128 = copyFromConfig.LsPieceMobilityPer128;
        LsKingSafetyPer128 = copyFromConfig.LsKingSafetyPer128;
        LsMinorPiecesPer128 = copyFromConfig.LsMinorPiecesPer128;
    }
}