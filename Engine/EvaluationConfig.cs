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
        public int MgPawnAdvancement = 1;
        public int EgPawnAdvancement = 4;
        public int MgPawnCentrality = 3;
        public int EgPawnCentrality = 0;
        public int EgPawnConstant = 12;
        // Knight Location 
        public int MgKnightAdvancement = 11;
        public int EgKnightAdvancement = 23;
        public int MgKnightCentrality = 11;
        public int EgKnightCentrality = 25;
        public int MgKnightCorner = 0;
        public int EgKnightCorner = -24;
        public int MgKnightConstant = -99;
        public int EgKnightConstant = 53;
        // Bishop Location
        public int MgBishopAdvancement = 9;
        public int EgBishopAdvancement = 11;
        public int MgBishopCentrality = 8;
        public int EgBishopCentrality = 25;
        public int MgBishopCorner = 2;
        public int EgBishopCorner = 0;
        public int MgBishopConstant = -43;
        public int EgBishopConstant = 100;
        // Rook Location
        public int MgRookAdvancement = 10;
        public int EgRookAdvancement = 16;
        public int MgRookCentrality = 10;
        public int EgRookCentrality = 0;
        public int MgRookCorner = -9;
        public int EgRookCorner = 0;
        public int MgRookConstant = -198;
        public int EgRookConstant = 250;
        // Queen Location
        public int MgQueenAdvancement = 2;
        public int EgQueenAdvancement = 25;
        public int MgQueenCentrality = 0;
        public int EgQueenCentrality = 22;
        public int MgQueenCorner = 0;
        public int EgQueenCorner = -3;
        public int MgQueenConstant = -175;
        public int EgQueenConstant = 359;
        // King Location
        public int MgKingAdvancement = -3;
        public int EgKingAdvancement = 9;
        public int MgKingCentrality = -3;
        public int EgKingCentrality = 8;
        public int MgKingCorner = 0;
        public int EgKingCorner = -1;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 88;
        public int EgPassedPawnScalePercent = 516;
        public int EgFreePassedPawnScalePercent = 985;
        public int EgKingEscortedPassedPawn = 9;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global
    }
}