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
    public int EgKnightMaterial = 483;
    public int MgBishopMaterial = 330;
    public int EgBishopMaterial = 484;
    public int MgRookMaterial = 500;
    public int EgRookMaterial = 782;
    public int MgQueenMaterial = 975;
    public int EgQueenMaterial = 1455;
    // Incentivize engine to promote pawns.
    // Also incentivize engine to eliminate enemy's last pawn in K vrs KQ or KR endgames (to trigger simple endgame scoring that pushes enemy king to a corner).
    // Want to ensure simple endgame score > (queen material + position + mobility - enemy pawn material - enemy pawn position).
    public int UnstoppablePassedPawn => EgQueenMaterial - (2 * Eval.PawnMaterial);
    public int SimpleEndgame => 2 * UnstoppablePassedPawn;
    // Pawn Location
    public int MgPawnAdvancement = 5;
    public int EgPawnAdvancement = 3;
    public int MgPawnCentrality = 3;
    public int EgPawnCentrality = -3;
    // Knight Location 
    public int MgKnightAdvancement = 5;
    public int EgKnightAdvancement = 6;
    public int MgKnightCentrality = 15;
    public int EgKnightCentrality = 18;
    public int MgKnightCorner = -1;
    public int EgKnightCorner = -24;
    // Bishop Location
    public int MgBishopAdvancement = -4;
    public int EgBishopAdvancement = 5;
    public int MgBishopCentrality = 18;
    public int EgBishopCentrality = 6;
    public int MgBishopCorner = 0;
    public int EgBishopCorner = 0;
    // Rook Location
    public int MgRookAdvancement = 1;
    public int EgRookAdvancement = 11;
    public int MgRookCentrality = 8;
    public int EgRookCentrality = -2;
    public int MgRookCorner = -13;
    public int EgRookCorner = 0;
    // Queen Location
    public int MgQueenAdvancement = -23;
    public int EgQueenAdvancement = 25;
    public int MgQueenCentrality = 6;
    public int EgQueenCentrality = 9;
    public int MgQueenCorner = 0;
    public int EgQueenCorner = -6;
    // King Location
    public int MgKingAdvancement = 0;
    public int EgKingAdvancement = 12;
    public int MgKingCentrality = 0;
    public int EgKingCentrality = 13;
    public int MgKingCorner = 0;
    public int EgKingCorner = -2;
    // Passed Pawns
    public int PassedPawnPowerPer128 = 301;
    public int MgPassedPawnScalePer128 = 164;
    public int EgPassedPawnScalePer128 = 269;
    public int EgFreePassedPawnScalePer128 = 548;
    public int EgKingEscortedPassedPawn = 12;
    // Piece Mobility
    public int PieceMobilityPowerPer128 = 87;
    public int MgKnightMobilityScale = 0;
    public int EgKnightMobilityScale = 30;
    public int MgBishopMobilityScale = 0;
    public int EgBishopMobilityScale = 112;
    public int MgRookMobilityScale = 29;
    public int EgRookMobilityScale = 91;
    public int MgQueenMobilityScale = 75;
    public int EgQueenMobilityScale = 66;
    // King Safety
    public int MgKingSafetyPowerPer128 = 236;
    public int MgKingSafetyScalePer128 = 84;
    public int MgKingSafetyMinorAttackOuterRingPer8 = 6;
    public int MgKingSafetyMinorAttackInnerRingPer8 = 30;
    public int MgKingSafetyRookAttackOuterRingPer8 = 12;
    public int MgKingSafetyRookAttackInnerRingPer8 = 25;
    public int MgKingSafetyQueenAttackOuterRingPer8 = 14;
    public int MgKingSafetyQueenAttackInnerRingPer8 = 19;
    public int MgKingSafetySemiOpenFilePer8 = 22;
    public int MgKingSafetyPawnShieldPer8 = 21;
    // Threats
    public int MgPawnThreatenMinor = 37;
    public int EgPawnThreatenMinor = 62;
    public int MgPawnThreatenMajor = 41;
    public int EgPawnThreatenMajor = 32;
    public int MgMinorThreatenMajor = 75;
    public int EgMinorThreatenMajor = 24;
    // Minor Pieces
    public int MgBishopPair = 39;
    public int EgBishopPair = 66;
    // Endgame Scale
    public int EgScaleBishopAdvantagePer128 = 34;
    public int EgScaleOppBishopsPerPassedPawn = 46;
    public int EgScaleWinningPerPawn = 52;
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