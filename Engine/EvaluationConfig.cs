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
        // Material and Simple Endgame
        public int KnightMaterial = 300;
        public int BishopMaterial = 330;
        public int RookMaterial = 500;
        public int QueenMaterial = 975;
        public int SimpleEndgame => UnstoppablePassedPawn;  // Incentivize engine to promote pawn in king and pawn endgames.
        // Pawn Location
        public int MgPawnAdvancement = 2;
        public int EgPawnAdvancement = 0;
        public int MgPawnCentrality = 10;
        public int EgPawnCentrality = 0;
        public int EgPawnConstant = 11;
        // Knight Location 
        public int MgKnightAdvancement = 10;
        public int EgKnightAdvancement = 2;
        public int MgKnightCentrality = 3;
        public int EgKnightCentrality = 7;
        public int MgKnightCorner = -6;
        public int EgKnightCorner = -2;
        public int EgKnightConstant = 18;
        // Bishop Location
        public int MgBishopAdvancement = 4;
        public int EgBishopAdvancement = 10;
        public int MgBishopCentrality = 2;
        public int EgBishopCentrality = 0;
        public int MgBishopCorner = 0;
        public int EgBishopCorner = 0;
        public int EgBishopConstant = 33;
        // Rook Location
        public int MgRookAdvancement = 3;
        public int EgRookAdvancement = 11;
        public int MgRookCentrality = 3;
        public int EgRookCentrality = 1;
        public int MgRookCorner = -10;
        public int EgRookCorner = -1;
        public int EgRookConstant = 100;
        // Queen Location
        public int MgQueenAdvancement = 2;
        public int EgQueenAdvancement = 25;
        public int MgQueenCentrality = 1;
        public int EgQueenCentrality = 5;
        public int MgQueenCorner = -6;
        public int EgQueenCorner = -3;
        public int EgQueenConstant = 100;
        // King Location
        public int MgKingAdvancement = -3;
        public int EgKingAdvancement = 14;
        public int MgKingCentrality = -24;
        public int EgKingCentrality = 14;
        public int MgKingCorner = 24;
        public int EgKingCorner = -15;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 120;
        public int EgPassedPawnScalePercent = 502;
        public int EgFreePassedPawnScalePercent = 972;
        public int EgKingEscortedPassedPawn = 9;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        // Piece Mobility
        public int MgKnightMobilityScale = 2;
        public int EgKnightMobilityScale = 47;
        public int MgBishopMobilityScale = 9;
        public int EgBishopMobilityScale = 17;
        public int MgRookMobilityScale = 42;
        public int EgRookMobilityScale = 3;
        public int MgQueenMobilityScale = 3;
        public int EgQueenMobilityScale = 48;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global
    }
}