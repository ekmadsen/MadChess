// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine
{
    public sealed class EvaluationConfig
    {
        // ReSharper disable ConvertToConstant.Global
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        // ReSharper disable RedundantDefaultMemberInitializer

        // Game Phase
        // Select phase constants such that starting material = 256.
        // This improves integer division speed since x / 256 = x >> 8.
        public const int KnightPhase = 10; //   4 * 10 =  40
        public const int BishopPhase = 10; // + 4 * 10 =  80
        public const int RookPhase = 22; //   + 4 * 22 = 168
        public const int QueenPhase = 44; //  + 2 * 44 = 256
        public const int MiddlegamePhase = 4 * (KnightPhase + BishopPhase + RookPhase) + 2 * QueenPhase;
        // Material and Simple Endgame
        public const int KnightExchangeMaterial = 300;
        public const int BishopExchangeMaterial = 300;
        public const int RookExchangeMaterial = 500;
        public const int QueenExchangeMaterial = 900;
        public int KnightMaterial = 300;
        public int BishopMaterial = 330;
        public int RookMaterial = 500;
        public int QueenMaterial = 975;
        // Incentivize engine to promote pawn in king and pawn endgames.
        // Also incentivize engine to eliminate opponent's last pawn in KQkp endgame (to trigger simple endgame scoring that pushes opposing king to a corner).
        // Want to ensure simple endgame score > (queen material + position + mobility - opponent pawn material - opponent pawn position).
        public int SimpleEndgame => 2 * UnstoppablePassedPawn;
        // Pawn Location
        public int MgPawnAdvancement = 1;
        public int EgPawnAdvancement = 6;
        public int MgPawnCentrality = 2;
        public int EgPawnCentrality = -10;
        public int EgPawnConstant = 42;
        // Knight Location 
        public int MgKnightAdvancement = 3;
        public int EgKnightAdvancement = 20;
        public int MgKnightCentrality = 6;
        public int EgKnightCentrality = 15;
        public int MgKnightCorner = -2;
        public int EgKnightCorner = -20;
        public int EgKnightConstant = 95;
        // Bishop Location
        public int MgBishopAdvancement = 4;
        public int EgBishopAdvancement = 4;
        public int MgBishopCentrality = 6;
        public int EgBishopCentrality = 0;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = -11;
        public int EgBishopConstant = 162;
        // Rook Location
        public int MgRookAdvancement = 2;
        public int EgRookAdvancement = 16;
        public int MgRookCentrality = 3;
        public int EgRookCentrality = -3;
        public int MgRookCorner = -14;
        public int EgRookCorner = 0;
        public int EgRookConstant = 251;
        // Queen Location
        public int MgQueenAdvancement = -7;
        public int EgQueenAdvancement = 32;
        public int MgQueenCentrality = 0;
        public int EgQueenCentrality = 15;
        public int MgQueenCorner = -1;
        public int EgQueenCorner = -5;
        public int EgQueenConstant = 400;
        // King Location
        public int MgKingAdvancement = -28;
        public int EgKingAdvancement = 29;
        public int MgKingCentrality = -16;
        public int EgKingCentrality = 11;
        public int MgKingCorner = 12;
        public int EgKingCorner = 0;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 149;
        public int EgPassedPawnScalePercent = 421;
        public int EgFreePassedPawnScalePercent = 897;
        public int EgKingEscortedPassedPawn = 9;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        // Piece Mobility
        public int MgKnightMobilityScale = 17;
        public int EgKnightMobilityScale = 127;
        public int MgBishopMobilityScale = 36;
        public int EgBishopMobilityScale = 221;
        public int MgRookMobilityScale = 84;
        public int EgRookMobilityScale = 155;
        public int MgQueenMobilityScale = 96;
        public int EgQueenMobilityScale = 346;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global


        public void Set(EvaluationConfig CopyFromConfig)
        {
            // Copy piece location values.
            KnightMaterial = CopyFromConfig.KnightMaterial;
            BishopMaterial = CopyFromConfig.BishopMaterial;
            RookMaterial = CopyFromConfig.RookMaterial;
            QueenMaterial = CopyFromConfig.QueenMaterial;
            MgPawnAdvancement = CopyFromConfig.MgPawnAdvancement;
            EgPawnAdvancement = CopyFromConfig.EgPawnAdvancement;
            MgPawnCentrality = CopyFromConfig.MgPawnCentrality;
            EgPawnCentrality = CopyFromConfig.EgPawnCentrality;
            EgPawnConstant = CopyFromConfig.EgPawnConstant;
            MgKnightAdvancement = CopyFromConfig.MgKnightAdvancement;
            EgKnightAdvancement = CopyFromConfig.EgKnightAdvancement;
            MgKnightCentrality = CopyFromConfig.MgKnightCentrality;
            EgKnightCentrality = CopyFromConfig.EgKnightCentrality;
            MgKnightCorner = CopyFromConfig.MgKnightCorner;
            EgKnightCorner = CopyFromConfig.EgKnightCorner;
            EgKnightConstant = CopyFromConfig.EgKnightConstant;
            MgBishopAdvancement = CopyFromConfig.MgBishopAdvancement;
            EgBishopAdvancement = CopyFromConfig.EgBishopAdvancement;
            MgBishopCentrality = CopyFromConfig.MgBishopCentrality;
            EgBishopCentrality = CopyFromConfig.EgBishopCentrality;
            MgBishopCorner = CopyFromConfig.MgBishopCorner;
            EgBishopCorner = CopyFromConfig.EgBishopCorner;
            EgBishopConstant = CopyFromConfig.EgBishopConstant;
            MgRookAdvancement = CopyFromConfig.MgRookAdvancement;
            EgRookAdvancement = CopyFromConfig.EgRookAdvancement;
            MgRookCentrality = CopyFromConfig.MgRookCentrality;
            EgRookCentrality = CopyFromConfig.EgRookCentrality;
            MgRookCorner = CopyFromConfig.MgRookCorner;
            EgRookCorner = CopyFromConfig.EgRookCorner;
            EgRookConstant = CopyFromConfig.EgRookConstant;
            MgQueenAdvancement = CopyFromConfig.MgQueenAdvancement;
            EgQueenAdvancement = CopyFromConfig.EgQueenAdvancement;
            MgQueenCentrality = CopyFromConfig.MgQueenCentrality;
            EgQueenCentrality = CopyFromConfig.EgQueenCentrality;
            MgQueenCorner = CopyFromConfig.MgQueenCorner;
            EgQueenCorner = CopyFromConfig.EgQueenCorner;
            EgQueenConstant = CopyFromConfig.EgQueenConstant;
            MgKingAdvancement = CopyFromConfig.MgKingAdvancement;
            EgKingAdvancement = CopyFromConfig.EgKingAdvancement;
            MgKingCentrality = CopyFromConfig.MgKingCentrality;
            EgKingCentrality = CopyFromConfig.EgKingCentrality;
            MgKingCorner = CopyFromConfig.MgKingCorner;
            EgKingCorner = CopyFromConfig.EgKingCorner;
            // Copy passed pawn values.
            MgPassedPawnScalePercent = CopyFromConfig.MgPassedPawnScalePercent;
            EgPassedPawnScalePercent = CopyFromConfig.EgPassedPawnScalePercent;
            EgFreePassedPawnScalePercent = CopyFromConfig.EgFreePassedPawnScalePercent;
            EgKingEscortedPassedPawn = CopyFromConfig.EgKingEscortedPassedPawn;
            // Copy piece mobility values.
            MgKnightMobilityScale = CopyFromConfig.MgKnightMobilityScale;
            EgKnightMobilityScale = CopyFromConfig.EgKnightMobilityScale;
            MgBishopMobilityScale = CopyFromConfig.MgBishopMobilityScale;
            EgBishopMobilityScale = CopyFromConfig.EgBishopMobilityScale;
            MgRookMobilityScale = CopyFromConfig.MgRookMobilityScale;
            EgRookMobilityScale = CopyFromConfig.EgRookMobilityScale;
            MgQueenMobilityScale = CopyFromConfig.MgQueenMobilityScale;
            EgQueenMobilityScale = CopyFromConfig.EgQueenMobilityScale;
        }
    }
}
