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
        public int EgPawnAdvancement = 5;
        public int MgPawnCentrality = 5;
        public int EgPawnCentrality = -2;
        public int EgPawnConstant = 15;
        // Knight Location 
        public int MgKnightAdvancement = 5;
        public int EgKnightAdvancement = 10;
        public int MgKnightCentrality = 10;
        public int EgKnightCentrality = 10;
        public int MgKnightCorner = -5;
        public int EgKnightCorner = -10;
        public int EgKnightConstant = 0;
        // Bishop Location
        public int MgBishopAdvancement = 2;
        public int EgBishopAdvancement = 8;
        public int MgBishopCentrality = 2;
        public int EgBishopCentrality = 8;
        public int MgBishopCorner = -4;
        public int EgBishopCorner = -8;
        public int EgBishopConstant = 0;
        // Rook Location
        public int MgRookAdvancement = 0;
        public int EgRookAdvancement = 5;
        public int MgRookCentrality = 10;
        public int EgRookCentrality = 0;
        public int MgRookCorner = -5;
        public int EgRookCorner = 0;
        public int EgRookConstant = 100;
        // Queen Location
        public int MgQueenAdvancement = 0;
        public int EgQueenAdvancement = 15;
        public int MgQueenCentrality = 5;
        public int EgQueenCentrality = 20;
        public int MgQueenCorner = -5;
        public int EgQueenCorner = -10;
        public int EgQueenConstant = 0;
        // King Location
        public int MgKingAdvancement = -20;
        public int EgKingAdvancement = 10;
        public int MgKingCentrality = -20;
        public int EgKingCentrality = 20;
        public int MgKingCorner = 20;
        public int EgKingCorner = -20;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 50;
        public int EgPassedPawnScalePercent = 500;
        public int EgFreePassedPawnScalePercent = 900;
        public int EgKingEscortedPassedPawn = 10;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        // Piece Mobility
        public int MgKnightMobilityScale = 5;
        public int EgKnightMobilityScale = 10;
        public int MgBishopMobilityScale = 10;
        public int EgBishopMobilityScale = 30;
        public int MgRookMobilityScale = 5;
        public int EgRookMobilityScale = 25;
        public int MgQueenMobilityScale = 5;
        public int EgQueenMobilityScale = 35;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global
    }
}