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
        public int SimpleEndgame => QueenMaterial - Evaluation.PawnMaterial;
        // Pawn Location
        public int MgPawnAdvancement = 2;
        public int EgPawnAdvancement = 5;
        public int MgPawnCentrality = 2;
        public int EgPawnCentrality = 0;
        public int EgPawnConstant = 11;
        // Knight Location 
        public int MgKnightAdvancement = 11;
        public int EgKnightAdvancement = 22;
        public int MgKnightCentrality = 8;
        public int EgKnightCentrality = 25;
        public int MgKnightCorner = 0;
        public int EgKnightCorner = -22;
        public int MgKnightConstant = -87;
        public int EgKnightConstant = 36;
        // Bishop Location
        public int MgBishopAdvancement = 9;
        public int EgBishopAdvancement = 9;
        public int MgBishopCentrality = 6;
        public int EgBishopCentrality = 24;
        public int MgBishopCorner = 3;
        public int EgBishopCorner = 0;
        public int MgBishopConstant = -43;
        public int EgBishopConstant = 88;
        // Rook Location
        public int MgRookAdvancement = 10;
        public int EgRookAdvancement = 16;
        public int MgRookCentrality = 11;
        public int EgRookCentrality = 0;
        public int MgRookCorner = -8;
        public int EgRookCorner = 0;
        public int MgRookConstant = -177;
        public int EgRookConstant = 200;
        // Queen Location
        public int MgQueenAdvancement = 2;
        public int EgQueenAdvancement = 25;
        public int MgQueenCentrality = 3;
        public int EgQueenCentrality = 19;
        public int MgQueenCorner = 0;
        public int EgQueenCorner = -8;
        public int MgQueenConstant = -61;
        public int EgQueenConstant = 199;
        // King Location
        public int MgKingAdvancement = -4;
        public int EgKingAdvancement = 3;
        public int MgKingCentrality = -2;
        public int EgKingCentrality = 16;
        public int MgKingCorner = 0;
        public int EgKingCorner = -21;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 97;
        public int EgPassedPawnScalePercent = 473;
        public int EgFreePassedPawnScalePercent = 866;
        public int EgKingEscortedPassedPawn = 11;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global
    }
}
