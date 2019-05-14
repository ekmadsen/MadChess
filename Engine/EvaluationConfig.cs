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
        public int BishopMaterial = 300;
        public int RookMaterial = 500;
        public int QueenMaterial = 975;
        public int SimpleEndgame => UnstoppablePassedPawn;  // Incentivize engine to promote pawn in king and pawn endgames.
        // Pawn Location
        public int MgPawnAdvancement = 4;
        public int EgPawnAdvancement = 3;
        public int MgPawnCentrality = 2;
        public int EgPawnCentrality = 0;
        public int EgPawnConstant = 14;
        // Knight Location 
        public int MgKnightAdvancement = 11;
        public int EgKnightAdvancement = 22;
        public int MgKnightCentrality = 9;
        public int EgKnightCentrality = 25;
        public int MgKnightCorner = 0;
        public int EgKnightCorner = -23;
        public int MgKnightConstant = -100;
        public int EgKnightConstant = 57;
        // Bishop Location
        public int MgBishopAdvancement = 9;
        public int EgBishopAdvancement = 11;
        public int MgBishopCentrality = 6;
        public int EgBishopCentrality = 25;
        public int MgBishopCorner = 1;
        public int EgBishopCorner = 0;
        public int MgBishopConstant = -46;
        public int EgBishopConstant = 100;
        // Rook Location
        public int MgRookAdvancement = 13;
        public int EgRookAdvancement = 15;
        public int MgRookCentrality = 13;
        public int EgRookCentrality = 0;
        public int MgRookCorner = -3;
        public int EgRookCorner = -3;
        public int MgRookConstant = -221;
        public int EgRookConstant = 250;
        // Queen Location
        public int MgQueenAdvancement = 2;
        public int EgQueenAdvancement = 25;
        public int MgQueenCentrality = 3;
        public int EgQueenCentrality = 20;
        public int MgQueenCorner = 0;
        public int EgQueenCorner = -10;
        public int MgQueenConstant = -279;
        public int EgQueenConstant = 395;
        // King Location
        public int MgKingAdvancement = -20;
        public int EgKingAdvancement = 25;
        public int MgKingCentrality = -19;
        public int EgKingCentrality = 10;
        public int MgKingCorner = 12;
        public int EgKingCorner = 0;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 44;
        public int EgPassedPawnScalePercent = 499;
        public int EgFreePassedPawnScalePercent = 922;
        public int EgKingEscortedPassedPawn = 9;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        // Piece Mobility
        public int MgKnightMobilityScale = 5;
        public int EgKnightMobilityScale = 8;
        public int MgBishopMobilityScale = 15;
        public int EgBishopMobilityScale = 25;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global
    }
}