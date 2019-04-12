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
        public int MgPawnAdvancement = 3;
        public int EgPawnAdvancement = 4;
        public int MgPawnCentrality = 2;
        public int EgPawnCentrality = 0;
        public int EgPawnConstant = 10;
        // Knight Location 
        public int MgKnightAdvancement = 10;
        public int EgKnightAdvancement = 22;
        public int MgKnightCentrality = 10;
        public int EgKnightCentrality = 25;
        public int MgKnightCorner = 0;
        public int EgKnightCorner = -24;
        public int MgKnightConstant = -100;
        public int EgKnightConstant = 54;
        // Bishop Location
        public int MgBishopAdvancement = 8;
        public int EgBishopAdvancement = 10;
        public int MgBishopCentrality = 8;
        public int EgBishopCentrality = 25;
        public int MgBishopCorner = 1;
        public int EgBishopCorner = 0;
        public int MgBishopConstant = -47;
        public int EgBishopConstant = 99;
        // Rook Location
        public int MgRookAdvancement = 10;
        public int EgRookAdvancement = 16;
        public int MgRookCentrality = 9;
        public int EgRookCentrality = 0;
        public int MgRookCorner = -9;
        public int EgRookCorner = 0;
        public int MgRookConstant = -217;
        public int EgRookConstant = 250;
        // Queen Location
        public int MgQueenAdvancement = 3;
        public int EgQueenAdvancement = 25;
        public int MgQueenCentrality = 0;
        public int EgQueenCentrality = 22;
        public int MgQueenCorner = 0;
        public int EgQueenCorner = -2;
        public int MgQueenConstant = -242;
        public int EgQueenConstant = 372;
        // King Location
        public int MgKingAdvancement = -3;
        public int EgKingAdvancement = 18;
        public int MgKingCentrality = -7;
        public int EgKingCentrality = 7;
        public int MgKingCorner = 11;
        public int EgKingCorner = 0;
        // Passed Pawns
        public int MgPassedPawnScalePercent = 98;
        public int EgPassedPawnScalePercent = 511;
        public int EgFreePassedPawnScalePercent = 963;
        public int EgKingEscortedPassedPawn = 9;
        public int UnstoppablePassedPawn => QueenMaterial - (2 * Evaluation.PawnMaterial);  // Incentivize engine to promote pawn.
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global
    }
}
