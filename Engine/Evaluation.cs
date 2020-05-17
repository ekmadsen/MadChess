// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;


namespace ErikTheCoder.MadChess.Engine
{
    // TODO: Capitalize first letter of each word in comment if the comment is not a complete sentence.
    // TODO: Refactor evaluation into color-agnostic methods using delegates.
    public sealed class Evaluation
    {
        public const int PawnMaterial = 100;
        public readonly EvaluationConfig Config;
        public readonly EvaluationStats Stats;
        public int DrawMoves;
        public bool UnderstandsPieceLocation;
        public bool UnderstandsPassedPawns;
        public bool UnderstandsMobility;
        private const double _passedPawnPower = 2d;
        private const double _pieceMobilityPower = 0.5d;
        private readonly EvaluationConfig _defaultConfig;
        private readonly EvaluationDelegates _delegates;
        private readonly StaticScore _staticScore;
        // Piece Location
        private readonly int[] _mgPawnLocations;
        private readonly int[] _egPawnLocations;
        private readonly int[] _mgKnightLocations;
        private readonly int[] _egKnightLocations;
        private readonly int[] _mgBishopLocations;
        private readonly int[] _egBishopLocations;
        private readonly int[] _mgRookLocations;
        private readonly int[] _egRookLocations;
        private readonly int[] _mgQueenLocations;
        private readonly int[] _egQueenLocations;
        private readonly int[] _mgKingLocations;
        private readonly int[] _egKingLocations;
        // Passed Pawns
        private readonly int[] _mgPassedPawns;
        private readonly int[] _egPassedPawns;
        private readonly int[] _egFreePassedPawns;
        // Piece mobility
        private readonly int[] _mgKnightMobility;
        private readonly int[] _egKnightMobility;
        private readonly int[] _mgBishopMobility;
        private readonly int[] _egBishopMobility;
        private readonly int[] _mgRookMobility;
        private readonly int[] _egRookMobility;
        private readonly int[] _mgQueenMobility;
        private readonly int[] _egQueenMobility;

        
        public Evaluation(EvaluationDelegates Delegates)
        {
            // Don't set Config and _defaultConfig to same object in memory (reference equality) to avoid ConfigureStrength method overwriting defaults.
            Config = new EvaluationConfig();
            _defaultConfig = new EvaluationConfig();
            _delegates = Delegates;
            _staticScore = new StaticScore();
            // Create arrays for quick lookup of positional factors.
            _mgPawnLocations = new int[64];
            _egPawnLocations = new int[64];
            _mgKnightLocations = new int[64];
            _egKnightLocations = new int[64];
            _mgBishopLocations = new int[64];
            _egBishopLocations = new int[64];
            _mgRookLocations = new int[64];
            _egRookLocations = new int[64];
            _mgQueenLocations = new int[64];
            _egQueenLocations = new int[64];
            _mgKingLocations = new int[64];
            _egKingLocations = new int[64];
            _mgPassedPawns = new int[8];
            _egPassedPawns = new int[8];
            _egFreePassedPawns = new int[8];
            _mgKnightMobility = new int[9];
            _egKnightMobility = new int[9];
            _mgBishopMobility = new int[14];
            _egBishopMobility = new int[14];
            _mgRookMobility = new int[15];
            _egRookMobility = new int[15];
            _mgQueenMobility = new int[28];
            _egQueenMobility = new int[28];
            // Calculate positional factor values.
            Configure();
            // Set default values.
            Stats = new EvaluationStats();
            DrawMoves = 2;
            UnderstandsPieceLocation = true;
            UnderstandsPassedPawns = true;
            UnderstandsMobility = true;
        }


        public void Configure()
        {
            // Calculate piece location values.
            for (int square = 0; square < 64; square++)
            {
                int rank = Board.WhiteRanks[square];
                int file = Board.Files[square];
                int squareCentrality = 3 - Board.DistanceToCentralSquares[square];
                int fileCentrality = 3 - Math.Min(Math.Abs(3 - file), Math.Abs(4 - file));
                int nearCorner = 3 - Board.DistanceToNearestCorner[square];
                _mgPawnLocations[square] = rank * Config.MgPawnAdvancement + squareCentrality * Config.MgPawnCentrality;
                _egPawnLocations[square] = rank * Config.EgPawnAdvancement + squareCentrality * Config.EgPawnCentrality + Config.EgPawnConstant;
                _mgKnightLocations[square] = rank * Config.MgKnightAdvancement + squareCentrality * Config.MgKnightCentrality + nearCorner * Config.MgKnightCorner;
                _egKnightLocations[square] = rank * Config.EgKnightAdvancement + squareCentrality * Config.EgKnightCentrality + nearCorner * Config.EgKnightCorner + Config.EgKnightConstant;
                _mgBishopLocations[square] = rank * Config.MgBishopAdvancement + squareCentrality * Config.MgBishopCentrality + nearCorner * Config.MgBishopCorner;
                _egBishopLocations[square] = rank * Config.EgBishopAdvancement + squareCentrality * Config.EgBishopCentrality + nearCorner * Config.EgBishopCorner + Config.EgBishopConstant;
                _mgRookLocations[square] = rank * Config.MgRookAdvancement + fileCentrality * Config.MgRookCentrality + nearCorner * Config.MgRookCorner;
                _egRookLocations[square] = rank * Config.EgRookAdvancement + squareCentrality * Config.EgRookCentrality + nearCorner * Config.EgRookCorner + Config.EgRookConstant;
                _mgQueenLocations[square] = rank * Config.MgQueenAdvancement + squareCentrality * Config.MgQueenCentrality + nearCorner * Config.MgQueenCorner;
                _egQueenLocations[square] = rank * Config.EgQueenAdvancement + squareCentrality * Config.EgQueenCentrality + nearCorner * Config.EgQueenCorner + Config.EgQueenConstant;
                _mgKingLocations[square] = rank * Config.MgKingAdvancement + squareCentrality * Config.MgKingCentrality + nearCorner * Config.MgKingCorner;
                _egKingLocations[square] = rank * Config.EgKingAdvancement + squareCentrality * Config.EgKingCentrality + nearCorner * Config.EgKingCorner;
            }
            // Calculate passed pawn values.
            double mgScale = Config.MgPassedPawnScalePercent / 100d;
            double egScale = Config.EgPassedPawnScalePercent / 100d;
            double egFreeScale = Config.EgFreePassedPawnScalePercent / 100d;
            for (int rank = 1; rank < 7; rank++)
            {
                _mgPassedPawns[rank] = GetNonLinearBonus(rank, mgScale, _passedPawnPower, 0);
                _egPassedPawns[rank] = GetNonLinearBonus(rank, egScale, _passedPawnPower, 0);
                _egFreePassedPawns[rank] = GetNonLinearBonus(rank, egFreeScale, _passedPawnPower, 0);
            }
            // Calculate piece mobility values.
            CalculatePieceMobility(_mgKnightMobility, _egKnightMobility, Config.MgKnightMobilityScale, Config.EgKnightMobilityScale);
            CalculatePieceMobility(_mgBishopMobility, _egBishopMobility, Config.MgBishopMobilityScale, Config.EgBishopMobilityScale);
            CalculatePieceMobility(_mgRookMobility, _egRookMobility, Config.MgRookMobilityScale, Config.EgRookMobilityScale);
            CalculatePieceMobility(_mgQueenMobility, _egQueenMobility, Config.MgQueenMobilityScale, Config.EgQueenMobilityScale);
        }


        public void ConfigureStrength(int Elo)
        {
            // Set default parameters.
            Config.Set(_defaultConfig);
            UnderstandsPieceLocation = true;
            UnderstandsPassedPawns = true;
            UnderstandsMobility = true;
            // Limit material and positional understanding.
            if (Elo < 800)
            {
                // Beginner
                // Undervalue rook and overvalue queen.
                Config.RookMaterial = 300;
                Config.QueenMaterial = 1200;
            }
            if (Elo < 1000)
            {
                // Novice
                // Value knight and bishop equally.
                Config.KnightMaterial = 300;
                Config.BishopMaterial = 300;
                // Misjudge the danger of passed pawns.
                UnderstandsPassedPawns = false;
                // Misplace pieces.
                UnderstandsPieceLocation = false;
            }
            if (Elo < 1200)
            {
                // Social
                UnderstandsMobility = false;
            }
            if (Elo < 1400)
            {
                // Strong Social
                //UnderstandsThreats = false;
                //UnderstandsKingSafety = false;
            }
            if (Elo < 1600)
            {
                // Club
                //UnderstandsBishopPair = false;
                //UnderstandsOutposts = false;
            }
            if (Elo < 1800)
            {
                // Strong Club
                //Understands7thRank = false;
                //UnderstandsTrades = false;
            }
            if (_delegates.Debug())
            {
                _delegates.WriteMessageLine($"info string PawnMaterialScore = {PawnMaterial}");
                _delegates.WriteMessageLine($"info string KnightMaterialScore = {Config.KnightMaterial}");
                _delegates.WriteMessageLine($"info string BishopMaterialScore = {Config.BishopMaterial}");
                _delegates.WriteMessageLine($"info string RookMaterialScore = {Config.RookMaterial}");
                _delegates.WriteMessageLine($"info string QueenMaterialScore = {Config.QueenMaterial}");
                _delegates.WriteMessageLine($"info string UnderstandsPieceLocation = {UnderstandsPieceLocation}");
                _delegates.WriteMessageLine($"info string UnderstandsPassedPawns = {UnderstandsPassedPawns}");
                _delegates.WriteMessageLine($"info string UnderstandsMobility = {UnderstandsMobility}");
                //_delegates.WriteMessageLine($"info string UnderstandsThreats = {UnderstandsThreats}");
                //_delegates.WriteMessageLine($"info string UnderstandsKingSafety = {UnderstandsKingSafety}");
                //_delegates.WriteMessageLine($"info string UnderstandsBishopPair = {UnderstandsBishopPair}");
                //_delegates.WriteMessageLine($"info string UnderstandsOutposts = {UnderstandsOutposts}");
                //_delegates.WriteMessageLine($"info string Understands7thRank = {Understands7thRank}");
                //_delegates.WriteMessageLine($"info string UnderstandsTrades = {UnderstandsTrades}");
            }
        }


        private static void CalculatePieceMobility(int[] MgPieceMobility, int[] EgPieceMobility, int MgMobilityScale, int EgMobilityScale)
        {
            Debug.Assert(MgPieceMobility.Length == EgPieceMobility.Length);
            int maxMoves = MgPieceMobility.Length - 1;
            for (int moves = 0; moves <= maxMoves; moves++)
            {
                double percentMaxMoves = (double)moves / maxMoves;
                MgPieceMobility[moves] = GetNonLinearBonus(percentMaxMoves, MgMobilityScale, _pieceMobilityPower, -MgMobilityScale / 2);
                EgPieceMobility[moves] = GetNonLinearBonus(percentMaxMoves, EgMobilityScale, _pieceMobilityPower, -EgMobilityScale / 2);
            }
            // Adjust constant so piece mobility bonus for average number of moves is zero.
            int averageMoves = maxMoves / 2;
            int averageMgBonus = MgPieceMobility[averageMoves];
            int averageEgBonus = EgPieceMobility[averageMoves];
            for (int moves = 0; moves <= maxMoves; moves++)
            {
                MgPieceMobility[moves] -= averageMgBonus;
                EgPieceMobility[moves] -= averageEgBonus;
            }
        }


        public (bool TerminalDraw, int PositionCount) IsTerminalDraw(Position Position)
        {
            // Only return true if position is drawn and no sequence of moves can make game winnable.
            int positionCount = _delegates.GetPositionCount();
            if (positionCount >= DrawMoves) return (true, positionCount); // Draw by repetition of position
            if (Position.HalfMoveNumber >= 99) return (true, positionCount); // Draw by fifty moves without a capture or pawn move
            // Determine if insufficient material remains for checkmate.
            if (Bitwise.CountSetBits(Position.WhitePawns | Position.BlackPawns) == 0)
            {
                // Neither side has any pawns.
                if (Bitwise.CountSetBits(Position.WhiteRooks | Position.WhiteQueens | Position.BlackRooks | Position.BlackQueens) == 0)
                {
                    // Neither side has any major pieces.
                    if ((Bitwise.CountSetBits(Position.WhiteKnights | Position.WhiteBishops) <= 1) && (Bitwise.CountSetBits(Position.BlackKnights | Position.BlackBishops) <= 1))
                    {
                        // Each side has one or zero minor pieces.  Draw by insufficient material.
                        return (true, positionCount); 
                    }
                }
            }
            return (false, positionCount);
        }


        public static bool IsDrawnEndgame(Position Position)
        {
            if (Bitwise.CountSetBits(Position.WhitePawns | Position.BlackPawns) > 0) return false; // At least one pawn on board.
            return IsDrawnEndgame(Position, true) || IsDrawnEndgame(Position, false);
        }


        private static bool IsDrawnEndgame(Position Position, bool WhiteIsSide1)
        {
            // Return true if position is drawn.
            // Do not terminate search based on this method because a sequence of moves could make the game winnable.
            int side1Knights;
            int side1Bishops;
            int side1Rooks;
            int side1Queens;
            int side2Knights;
            int side2Bishops;
            int side2Rooks;
            int side2Queens;
            if (WhiteIsSide1)
            {
                // White is Side 1
                side1Knights = Bitwise.CountSetBits(Position.WhiteKnights);
                side1Bishops = Bitwise.CountSetBits(Position.WhiteBishops);
                side1Rooks = Bitwise.CountSetBits(Position.WhiteRooks);
                side1Queens = Bitwise.CountSetBits(Position.WhiteQueens);
                side2Knights = Bitwise.CountSetBits(Position.BlackKnights);
                side2Bishops = Bitwise.CountSetBits(Position.BlackBishops);
                side2Rooks = Bitwise.CountSetBits(Position.BlackRooks);
                side2Queens = Bitwise.CountSetBits(Position.BlackQueens);
            }
            else
            {
                // Black is Side 1
                side1Knights = Bitwise.CountSetBits(Position.BlackKnights);
                side1Bishops = Bitwise.CountSetBits(Position.BlackBishops);
                side1Rooks = Bitwise.CountSetBits(Position.BlackRooks);
                side1Queens = Bitwise.CountSetBits(Position.BlackQueens);
                side2Knights = Bitwise.CountSetBits(Position.WhiteKnights);
                side2Bishops = Bitwise.CountSetBits(Position.WhiteBishops);
                side2Rooks = Bitwise.CountSetBits(Position.WhiteRooks);
                side2Queens = Bitwise.CountSetBits(Position.WhiteQueens);
            }
            int side1MinorPieces = side1Knights + side1Bishops;
            int side1MajorPieces = side1Rooks + side1Queens;
            int side2MinorPieces = side2Knights + side2Bishops;
            int side2MajorPieces = side2Rooks + side2Queens;
            bool side1QueenOr2Rooks = ((side1Queens == 1) && (side1MajorPieces == 1) && (side1MinorPieces == 0)) || ((side1Rooks == 2) && (side1MajorPieces == 2) && (side1MinorPieces == 0));
            bool side2QueenOr2Rooks = ((side2Queens == 1) && (side2MajorPieces == 1) && (side2MinorPieces == 0)) || ((side2Rooks == 2) && (side2MajorPieces == 2) && (side2MinorPieces == 0));
            if (side1QueenOr2Rooks && side2QueenOr2Rooks) return true; // Both sides have queen or two rooks.
            if (side1QueenOr2Rooks)
            {
                // Side 1 has queen or two rooks.
                if ((side2Rooks == 1) && (side2MajorPieces == 1))
                {
                    if ((side2Bishops == 1) && (side2Knights == 1)) return true; // Side 2 has rook, bishop, and knight.
                    if (side2MinorPieces == 1) return true; // Side 2 has rook and minor piece.
                }
            }
            if ((side1Rooks == 1) && (side1MajorPieces == 1) && (side1MinorPieces == 1))
            {
                // Side 1 has rook and minor piece.
                switch (side2Rooks)
                {
                    case 1 when (side2MajorPieces == 1) && (side2MinorPieces == 1): // Side 2 has rook and minor piece.
                    case 1 when (side2MajorPieces == 1) && (side2MinorPieces == 0): // Side 2 has rook.
                        return true;
                }
            }
            if ((side1Bishops == 1) && (side1Knights == 1) && (side1MajorPieces == 0))
            {
                // Side 1 has bishop and knight.
                if ((side2Rooks == 1) && (side2MajorPieces == 1) && (side2MinorPieces == 0)) return true; // Side 2 has rook.
            }
            if ((side1Rooks == 1) && (side1MajorPieces == 1) && (side1MinorPieces == 0))
            {
                // Side 1 has rook.
                if ((side2Rooks == 1) && (side2MajorPieces == 1) && (side2MinorPieces == 0)) return true; // Side 2 has rook.
                if ((side2MinorPieces == 1) && (side2MajorPieces == 0)) return true; // Side 2 has minor piece.
            }
            if ((side1Knights == 2) && (side1MinorPieces == 2) && (side1MajorPieces == 0))
            {
                // Side 1 has two knights.
                if ((side2MinorPieces <= 1) && (side2MajorPieces == 0)) return true; // Side 2 has one or zero minor pieces.
            }
            return false;
        }


        public int GetStaticScore(Position Position)
        {
            Debug.Assert(!Position.KingInCheck);
            Stats.Evaluations++;
            _staticScore.Reset();
            if (!EvaluateSimpleEndgame(Position))
            {
                // Not a simple endgame.
                GetMaterialScore(Position);
                if (UnderstandsPieceLocation) EvaluatePieceLocation(Position);
                if (UnderstandsPassedPawns) EvaluatePawns(Position);
                if (UnderstandsMobility) EvaluatePieceMobility(Position);
            }
            int phase = DetermineGamePhase(Position);
            return Position.WhiteMove ? _staticScore.TotalScore(phase) : -_staticScore.TotalScore(phase);
        }


        private bool EvaluateSimpleEndgame(Position Position)
        {
            int whitePawns = Bitwise.CountSetBits(Position.WhitePawns);
            int whiteKnights = Bitwise.CountSetBits(Position.WhiteKnights);
            int whiteBishops = Bitwise.CountSetBits(Position.WhiteBishops);
            int whiteMinorPieces = whiteKnights + whiteBishops;
            int whiteMajorPieces = Bitwise.CountSetBits(Position.WhiteRooks | Position.WhiteQueens);
            int whitePawnsAndPieces = whitePawns + whiteMinorPieces + whiteMajorPieces;
            int blackPawns = Bitwise.CountSetBits(Position.BlackPawns);
            int blackKnights = Bitwise.CountSetBits(Position.BlackKnights);
            int blackBishops = Bitwise.CountSetBits(Position.BlackBishops);
            int blackMinorPieces = blackKnights + blackBishops;
            int blackMajorPieces = Bitwise.CountSetBits(Position.BlackRooks | Position.BlackQueens);
            int blackPawnsAndPieces = blackPawns + blackMinorPieces + blackMajorPieces;
            if ((whitePawnsAndPieces > 0) && (blackPawnsAndPieces > 0)) return false; // Position is not a simple endgame.
            bool loneWhitePawn = (whitePawns == 1) && (whitePawnsAndPieces == 1) && (blackPawnsAndPieces == 0);
            bool loneBlackPawn = (blackPawns == 1) && (blackPawnsAndPieces == 1) && (whitePawnsAndPieces == 0);
            int whiteKingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
            int blackKingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (whitePawnsAndPieces)
            {
                // Case 0 = Lone white king
                case 0 when loneBlackPawn:
                    return EvaluateKingVersusPawn(Position, false);
                // ReSharper disable once SwitchStatementMissingSomeCases
                case 0:
                    switch (blackPawns)
                    {
                        case 0 when (blackKnights == 1) && (blackBishops == 1) && (blackMajorPieces == 0):
                            // King versus knight and bishop
                            bool lightSquareBishop = Board.LightSquares[Bitwise.FindFirstSetBit(Position.BlackBishops)];
                            int distanceToCorrectColorCorner = lightSquareBishop
                                ? Board.DistanceToNearestLightCorner[whiteKingSquare]
                                : Board.DistanceToNearestDarkCorner[whiteKingSquare];
                            _staticScore.BlackSimpleEndgame = Config.SimpleEndgame - distanceToCorrectColorCorner - Board.SquareDistances[whiteKingSquare][blackKingSquare];
                            return true;
                        case 0 when (blackMinorPieces == 0) && (blackMajorPieces >= 1):
                            // King versus major pieces
                            _staticScore.BlackSimpleEndgame = Config.SimpleEndgame - Board.DistanceToNearestCorner[whiteKingSquare] - Board.SquareDistances[whiteKingSquare][blackKingSquare];
                            return true;
                    }
                    break;
            }
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (blackPawnsAndPieces)
            {
                // Case 0 = Lone black king
                case 0 when loneWhitePawn:
                    return EvaluateKingVersusPawn(Position, true);
                // ReSharper disable once SwitchStatementMissingSomeCases
                case 0:
                    switch (whitePawns)
                    {
                        case 0 when (whiteKnights == 1) && (whiteBishops == 1) && (whiteMajorPieces == 0):
                            // King versus knight and bishop
                            bool lightSquareBishop = Board.LightSquares[Bitwise.FindFirstSetBit(Position.WhiteBishops)];
                            int distanceToCorrectColorCorner = lightSquareBishop
                                ? Board.DistanceToNearestLightCorner[blackKingSquare]
                                : Board.DistanceToNearestDarkCorner[blackKingSquare];
                            _staticScore.WhiteSimpleEndgame = Config.SimpleEndgame - distanceToCorrectColorCorner - Board.SquareDistances[whiteKingSquare][blackKingSquare];
                            return true;
                        case 0 when (whiteMinorPieces == 0) && (whiteMajorPieces >= 1):
                            // King versus major pieces
                            _staticScore.WhiteSimpleEndgame = Config.SimpleEndgame - Board.DistanceToNearestCorner[blackKingSquare] - Board.SquareDistances[whiteKingSquare][blackKingSquare];
                            return true;
                    }
                    break;
            }
            // Use regular evaluation.
            return false;
        }


        private bool EvaluateKingVersusPawn(Position Position, bool LoneWhitePawn)
        {
            int winningKingRank;
            int winningKingFile;
            int defendingKingRank;
            int defendingKingFile;
            int pawnRank;
            int pawnFile;
            // Get rank and file of all pieces.
            if (LoneWhitePawn)
            {
                // White winning
                int winningKingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
                winningKingRank = Board.WhiteRanks[winningKingSquare];
                winningKingFile = Board.Files[winningKingSquare];
                int defendingKingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
                defendingKingRank = Board.WhiteRanks[defendingKingSquare];
                defendingKingFile = Board.Files[defendingKingSquare];
                int pawnSquare = Bitwise.FindFirstSetBit(Position.WhitePawns);
                pawnRank = Board.WhiteRanks[pawnSquare];
                pawnFile = Board.Files[pawnSquare];
            }
            else
            {
                // Black winning
                int winningKingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
                winningKingRank = Board.BlackRanks[winningKingSquare];
                winningKingFile = Board.Files[winningKingSquare];
                int defendingKingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
                defendingKingRank = Board.BlackRanks[defendingKingSquare];
                defendingKingFile = Board.Files[defendingKingSquare];
                int pawnSquare = Bitwise.FindFirstSetBit(Position.BlackPawns);
                pawnRank = Board.BlackRanks[pawnSquare];
                pawnFile = Board.Files[pawnSquare];
            }
            if ((pawnFile == 0) || (pawnFile == 7))
            {
                // Pawn is on rook file.
                if ((defendingKingFile == pawnFile) && (defendingKingRank > pawnRank))
                {
                    // Defending king is in front of pawn and on same file.
                    // Game is drawn.
                    return true;
                }
            }
            else
            {
                // Pawn is not on rook file.
                int kingPawnRankDifference = winningKingRank - pawnRank;
                int kingPawnAbsoluteFileDifference = Math.Abs(winningKingFile - pawnFile);
                bool winningKingOnKeySquare = pawnRank switch
                {
                    1 => ((winningKingRank == pawnRank + 2) && (kingPawnAbsoluteFileDifference <= 1)),
                    2 => ((winningKingRank == pawnRank + 2) && (kingPawnAbsoluteFileDifference <= 1)),
                    3 => ((winningKingRank == pawnRank + 2) && (kingPawnAbsoluteFileDifference <= 1)),
                    4 => ((kingPawnRankDifference > 0) && (kingPawnRankDifference <= 2) && (kingPawnAbsoluteFileDifference <= 1)),
                    5 => ((kingPawnRankDifference > 0) && (kingPawnRankDifference <= 2) && (kingPawnAbsoluteFileDifference <= 1)),
                    6 => ((kingPawnRankDifference >= 0) && (kingPawnRankDifference <= 1) && (kingPawnAbsoluteFileDifference <= 1)),
                    _ => false
                };
                if (winningKingOnKeySquare)
                {
                    // Pawn promotes.
                    if (LoneWhitePawn) _staticScore.WhiteSimpleEndgame = Config.SimpleEndgame + pawnRank;
                    else _staticScore.BlackSimpleEndgame = Config.SimpleEndgame + pawnRank;
                    return true;
                }
            }
            // Use regular evaluation.
            return false;
        }


        public int GetMaterialScore(Position Position)
        {
            _staticScore.WhiteMaterial = Bitwise.CountSetBits(Position.WhitePawns) * PawnMaterial +
                                         Bitwise.CountSetBits(Position.WhiteKnights) * Config.KnightMaterial + Bitwise.CountSetBits(Position.WhiteBishops) * Config.BishopMaterial +
                                         Bitwise.CountSetBits(Position.WhiteRooks) * Config.RookMaterial + Bitwise.CountSetBits(Position.WhiteQueens) * Config.QueenMaterial;
            _staticScore.BlackMaterial = Bitwise.CountSetBits(Position.BlackPawns) * PawnMaterial +
                                         Bitwise.CountSetBits(Position.BlackKnights) * Config.KnightMaterial + Bitwise.CountSetBits(Position.BlackBishops) * Config.BishopMaterial +
                                         Bitwise.CountSetBits(Position.BlackRooks) * Config.RookMaterial + Bitwise.CountSetBits(Position.BlackQueens) * Config.QueenMaterial;
            return Position.WhiteMove
                ? _staticScore.WhiteMaterial - _staticScore.BlackMaterial
                : _staticScore.BlackMaterial - _staticScore.WhiteMaterial;
        }


        public int GetMaterialScore(int Piece)
        {
            // Sequence cases in order of enum integer value to improve performance of switch statement.
            return Piece switch
            {
                Engine.Piece.None => 0,
                Engine.Piece.WhitePawn => PawnMaterial,
                Engine.Piece.WhiteKnight => Config.KnightMaterial,
                Engine.Piece.WhiteBishop => Config.BishopMaterial,
                Engine.Piece.WhiteRook => Config.RookMaterial,
                Engine.Piece.WhiteQueen => Config.QueenMaterial,
                Engine.Piece.WhiteKing => 0,
                Engine.Piece.BlackPawn => PawnMaterial,
                Engine.Piece.BlackKnight => Config.KnightMaterial,
                Engine.Piece.BlackBishop => Config.BishopMaterial,
                Engine.Piece.BlackRook => Config.RookMaterial,
                Engine.Piece.BlackQueen => Config.QueenMaterial,
                Engine.Piece.BlackKing => 0,
                _ => throw new ArgumentException($"{Piece} piece not supported.")
            };
        }


        public static int GetExchangeMaterialScore(Position Position)
        {
            int whiteScore = Bitwise.CountSetBits(Position.WhitePawns) * PawnMaterial +
                             Bitwise.CountSetBits(Position.WhiteKnights) * EvaluationConfig.KnightExchangeMaterial + Bitwise.CountSetBits(Position.WhiteBishops) * EvaluationConfig.BishopExchangeMaterial +
                             Bitwise.CountSetBits(Position.WhiteRooks) * EvaluationConfig.RookExchangeMaterial + Bitwise.CountSetBits(Position.WhiteQueens) * EvaluationConfig.QueenExchangeMaterial;
            int blackScore = Bitwise.CountSetBits(Position.BlackPawns) * PawnMaterial +
                             Bitwise.CountSetBits(Position.BlackKnights) * EvaluationConfig.KnightExchangeMaterial + Bitwise.CountSetBits(Position.BlackBishops) * EvaluationConfig.BishopExchangeMaterial +
                             Bitwise.CountSetBits(Position.BlackRooks) * EvaluationConfig.RookExchangeMaterial + Bitwise.CountSetBits(Position.BlackQueens) * EvaluationConfig.QueenExchangeMaterial;
            return Position.WhiteMove
                ? whiteScore - blackScore
                : blackScore - whiteScore;
        }


        private void EvaluatePieceLocation(Position Position)
        {
            // Pawns
            int square;
            int blackSquare;
            ulong pieces = Position.WhitePawns;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgPawnLocations[square];
                _staticScore.WhiteEgPieceLocation += _egPawnLocations[square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackPawns;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgPawnLocations[blackSquare];
                _staticScore.BlackEgPieceLocation += _egPawnLocations[blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Knights
            pieces = Position.WhiteKnights;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgKnightLocations[square];
                _staticScore.WhiteEgPieceLocation += _egKnightLocations[square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackKnights;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgKnightLocations[blackSquare];
                _staticScore.BlackEgPieceLocation += _egKnightLocations[blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Bishops
            pieces = Position.WhiteBishops;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgBishopLocations[square];
                _staticScore.WhiteEgPieceLocation += _egBishopLocations[square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackBishops;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgBishopLocations[blackSquare];
                _staticScore.BlackEgPieceLocation += _egBishopLocations[blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Rooks
            pieces = Position.WhiteRooks;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgRookLocations[square];
                _staticScore.WhiteEgPieceLocation += _egRookLocations[square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackRooks;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgRookLocations[blackSquare];
                _staticScore.BlackEgPieceLocation += _egRookLocations[blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Queens
            pieces = Position.WhiteQueens;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgQueenLocations[square];
                _staticScore.WhiteEgPieceLocation += _egQueenLocations[square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackQueens;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgQueenLocations[blackSquare];
                _staticScore.BlackEgPieceLocation += _egQueenLocations[blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Kings
            square = Bitwise.FindFirstSetBit(Position.WhiteKing);
            _staticScore.WhiteMgPieceLocation += _mgKingLocations[square];
            _staticScore.WhiteEgPieceLocation += _egKingLocations[square];
            blackSquare = Board.GetBlackSquare(Bitwise.FindFirstSetBit(Position.BlackKing));
            _staticScore.BlackMgPieceLocation += _mgKingLocations[blackSquare];
            _staticScore.BlackEgPieceLocation += _egKingLocations[blackSquare];
        }


        private void EvaluatePawns(Position Position)
        {
            // White pawns
            ulong pawns = Position.WhitePawns;
            int kingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
            int enemyKingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
            int pawnSquare;
            int rank;
            while ((pawnSquare = Bitwise.FindFirstSetBit(pawns)) != Square.Illegal)
            {
                if (IsPassedPawn(Position, pawnSquare, true))
                {
                    rank = Board.WhiteRanks[pawnSquare];
                    _staticScore.WhiteEgKingEscortedPassedPawns += (Board.SquareDistances[pawnSquare][enemyKingSquare] - Board.SquareDistances[pawnSquare][kingSquare]) * Config.EgKingEscortedPassedPawn;
                    if (IsFreePawn(Position, pawnSquare, true))
                    {
                        // Pawn can advance safely.
                        if (IsUnstoppablePawn(Position, pawnSquare, enemyKingSquare, true, true)) _staticScore.WhiteUnstoppablePassedPawns += Config.UnstoppablePassedPawn; // Pawn is unstoppable.
                        else _staticScore.WhiteEgFreePassedPawns += _egFreePassedPawns[rank]; // Pawn is passed and free.
                    }
                    else
                    {
                        // Pawn is passed.
                        _staticScore.WhiteMgPassedPawns += _mgPassedPawns[rank];
                        _staticScore.WhiteEgPassedPawns += _egPassedPawns[rank];
                    }
                }
                Bitwise.ClearBit(ref pawns, pawnSquare);
            }
            // Black pawns
            pawns = Position.BlackPawns;
            kingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
            enemyKingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
            while ((pawnSquare = Bitwise.FindFirstSetBit(pawns)) != Square.Illegal)
            {
                if (IsPassedPawn(Position, pawnSquare, false))
                {
                    rank = Board.BlackRanks[pawnSquare];
                    _staticScore.BlackEgKingEscortedPassedPawns += (Board.SquareDistances[pawnSquare][enemyKingSquare] - Board.SquareDistances[pawnSquare][kingSquare]) * Config.EgKingEscortedPassedPawn;
                    if (IsFreePawn(Position, pawnSquare, false))
                    {
                        // Pawn can advance safely.
                        if (IsUnstoppablePawn(Position, pawnSquare, enemyKingSquare, false, true)) _staticScore.BlackUnstoppablePassedPawns += Config.UnstoppablePassedPawn; // Pawn is unstoppable.
                        else _staticScore.BlackEgFreePassedPawns += _egFreePassedPawns[rank]; // Pawn is passed and free.
                    }
                    else
                    {
                        // Pawn is passed.
                        _staticScore.BlackMgPassedPawns += _mgPassedPawns[rank];
                        _staticScore.BlackEgPassedPawns += _egPassedPawns[rank];
                    }
                }
                Bitwise.ClearBit(ref pawns, pawnSquare);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPassedPawn(Position Position, int Square, bool White)
        {
            Debug.Assert(Position.GetPiece(Square) == (White ? Piece.WhitePawn : Piece.BlackPawn));
            return White
                ? (Board.WhitePassedPawnMasks[Square] & Position.BlackPawns) == 0
                : (Board.BlackPassedPawnMasks[Square] & Position.WhitePawns) == 0;
        }


        private static bool IsFreePawn(Position Position, int Square, bool White)
        {
            Debug.Assert(Position.GetPiece(Square) == (White ? Piece.WhitePawn : Piece.BlackPawn));
            // Determine if pawn can advance.
            return White
                ? (Board.WhiteFreePawnMasks[Square] & Position.Occupancy) == 0
                : (Board.BlackFreePawnMasks[Square] & Position.Occupancy) == 0;
        }


        private static bool IsUnstoppablePawn(Position Position, int PawnSquare, int EnemyKingSquare, bool White, bool IsFree)
        {
            if (!IsFree) return false;
            // Pawn is free to advance to promotion square.
            int file = Board.Files[PawnSquare];
            int promotionSquare;
            int enemyPieces;
            if (White)
            {
                // White Pawn
                promotionSquare = Board.GetSquare(file, 7);
                enemyPieces = Bitwise.CountSetBits(Position.BlackKnights) + Bitwise.CountSetBits(Position.BlackBishops) + Bitwise.CountSetBits(Position.BlackRooks) + Bitwise.CountSetBits(Position.BlackQueens);
            }
            else
            {
                // Black pawn
                promotionSquare = Board.GetSquare(file, 0);
                enemyPieces = Bitwise.CountSetBits(Position.WhiteKnights) + Bitwise.CountSetBits(Position.WhiteBishops) + Bitwise.CountSetBits(Position.WhiteRooks) + Bitwise.CountSetBits(Position.WhiteQueens);
            }
            if (enemyPieces == 0)
            {
                // Enemy has no minor or major pieces.
                int pawnDistanceToPromotionSquare = Board.SquareDistances[PawnSquare][promotionSquare];
                int kingDistanceToPromotionSquare = Board.SquareDistances[EnemyKingSquare][promotionSquare];
                if (White != Position.WhiteMove) kingDistanceToPromotionSquare--; // Enemy king can move one square closer to pawn.
                return kingDistanceToPromotionSquare > pawnDistanceToPromotionSquare; // Enemy king cannot stop pawn from promoting.
            }
            return false;
        }


        private void EvaluatePieceMobility(Position Position)
        {
            int square;
            int mgPieceMobilityScore;
            int egPieceMobilityScore;
            // Knights
            ulong pieces = Position.WhiteKnights;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(Position, square, true, _delegates.GetKnightDestinations, _mgKnightMobility, _egKnightMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackKnights;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(Position, square, false, _delegates.GetKnightDestinations, _mgKnightMobility, _egKnightMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Bishops
            pieces = Position.WhiteBishops;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(Position, square, true, _delegates.GetBishopDestinations, _mgBishopMobility, _egBishopMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackBishops;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(Position, square, false, _delegates.GetBishopDestinations, _mgBishopMobility, _egBishopMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Rooks
            pieces = Position.WhiteRooks;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(Position, square, true, _delegates.GetRookDestinations, _mgRookMobility, _egRookMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackRooks;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(Position, square, false, _delegates.GetRookDestinations, _mgRookMobility, _egRookMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Queens
            pieces = Position.WhiteQueens;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(Position, square, true, _delegates.GetQueenDestinations, _mgQueenMobility, _egQueenMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = Position.BlackQueens;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(Position, square, false, _delegates.GetQueenDestinations, _mgQueenMobility, _egQueenMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                Bitwise.ClearBit(ref pieces, square);
            }
        }


        private static (int MiddlegameMobility, int EndgameMobility) GetPieceMobilityScore(Position Position, int FromSquare, bool White, Delegates.GetPieceDestinations GetPieceUnoccupiedDestinations, int[] MgPieceMobility, int[] EgPieceMobility)
        {
            ulong pieceDestinations = GetPieceUnoccupiedDestinations(Position, FromSquare, White);
            int moves = Bitwise.CountSetBits(pieceDestinations);
            int mgMoveIndex = Math.Min(moves, MgPieceMobility.Length - 1);
            int egMoveIndex = Math.Min(moves, EgPieceMobility.Length - 1);
            return (MgPieceMobility[mgMoveIndex], EgPieceMobility[egMoveIndex]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMateScore(int Depth) => -StaticScore.Checkmate - StaticScore.LongestCheckmate + Depth;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMateDistance(int Score)
        {
            int mateDistance = (Score > 0)
                ? StaticScore.Checkmate + StaticScore.LongestCheckmate - Score
                : -StaticScore.Checkmate - StaticScore.LongestCheckmate - Score;
            // Convert plies to full moves.
            int quotient = Math.DivRem(mateDistance, 2, out int remainder);
            return quotient + remainder;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int DetermineGamePhase(Position Position)
        {
            int phase = EvaluationConfig.KnightPhase * Bitwise.CountSetBits(Position.WhiteKnights | Position.BlackKnights) +
                        EvaluationConfig.BishopPhase * Bitwise.CountSetBits(Position.WhiteBishops | Position.BlackBishops) +
                        EvaluationConfig.RookPhase * Bitwise.CountSetBits(Position.WhiteRooks | Position.BlackRooks) +
                        EvaluationConfig.QueenPhase * Bitwise.CountSetBits(Position.WhiteQueens | Position.BlackQueens);
            return Math.Min(phase, EvaluationConfig.MiddlegamePhase);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNonLinearBonus(double Bonus, double Scale, double Power, int Constant) => (int)(Scale * Math.Pow(Bonus, Power)) + Constant;


        public string ShowParameters()
        {
            StringBuilder stringBuilder = new StringBuilder();
            // Material
            stringBuilder.AppendLine("Material");
            stringBuilder.AppendLine("===========");
            stringBuilder.AppendLine($"Pawn:    {PawnMaterial}");
            stringBuilder.AppendLine($"Knight:  {Config.KnightMaterial}");
            stringBuilder.AppendLine($"Bishop:  {Config.BishopMaterial}");
            stringBuilder.AppendLine($"Rook:    {Config.RookMaterial}");
            stringBuilder.AppendLine($"Queen:   {Config.QueenMaterial}");
            stringBuilder.AppendLine();
            // Piece Location
            stringBuilder.AppendLine("Middlegame Pawn Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_mgPawnLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame Pawn Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_egPawnLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Middlegame Knight Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_mgKnightLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame Knight Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_egKnightLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Middlegame Bishop Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_mgBishopLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame Bishop Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_egBishopLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Middlegame Rook Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_mgRookLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame Rook Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_egRookLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Middlegame Queen Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_mgQueenLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame Queen Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_egQueenLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Middlegame King Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_mgKingLocations, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Endgame King Location");
            stringBuilder.AppendLine("==============================================");
            ShowParameterSquares(_egKingLocations, stringBuilder);
            stringBuilder.AppendLine();
            // Passed Pawns
            stringBuilder.Append("Middlegame Passed Pawns:            ");
            ShowParameterArray(_mgPassedPawns, stringBuilder);
            stringBuilder.Append("Endgame Passed Pawns:               ");
            ShowParameterArray(_egPassedPawns, stringBuilder);
            stringBuilder.Append("Endgame Free Passed Pawns:          ");
            ShowParameterArray(_egFreePassedPawns, stringBuilder);
            stringBuilder.AppendLine($"Endgame King Escorted Passed Pawn:  {Config.EgKingEscortedPassedPawn}");
            stringBuilder.AppendLine($"Unstoppable Passed Pawn:            {Config.UnstoppablePassedPawn}");
            stringBuilder.AppendLine();
            // Knight Mobility
            stringBuilder.Append("Middlegame Knight Mobility:  ");
            ShowParameterArray(_mgKnightMobility, stringBuilder);
            stringBuilder.Append("   Endgame Knight Mobility:  ");
            ShowParameterArray(_egKnightMobility, stringBuilder);
            stringBuilder.AppendLine();
            // Bishop Mobility
            stringBuilder.Append("Middlegame Bishop Mobility:  ");
            ShowParameterArray(_mgBishopMobility, stringBuilder);
            stringBuilder.Append("   Endgame Bishop Mobility:  ");
            ShowParameterArray(_egBishopMobility, stringBuilder);
            stringBuilder.AppendLine();
            // Rook Mobility
            stringBuilder.Append("Middlegame Rook Mobility:    ");
            ShowParameterArray(_mgRookMobility, stringBuilder);
            stringBuilder.Append("   Endgame Rook Mobility:    ");
            ShowParameterArray(_egRookMobility, stringBuilder);
            stringBuilder.AppendLine();
            // Queen Mobility
            stringBuilder.Append("Middlegame Queen Mobility:   ");
            ShowParameterArray(_mgQueenMobility, stringBuilder);
            stringBuilder.Append("   Endgame Queen Mobility:   ");
            ShowParameterArray(_egQueenMobility, stringBuilder);
            return stringBuilder.ToString();
        }


        private static void ShowParameterSquares(int[] Parameters, StringBuilder StringBuilder)
        {
            for (int rank = 7; rank >= 0; rank--)
            {
                for (int file = 0; file < 8; file++)
                {
                    int square = Board.GetSquare(file, rank);
                    StringBuilder.Append(Parameters[square].ToString("+000;-000").PadRight(6));
                }
                StringBuilder.AppendLine();
            }
        }


        private static void ShowParameterArray(int[] Parameters, StringBuilder StringBuilder)
        {
            for (int index = 0; index < Parameters.Length; index++) StringBuilder.Append(Parameters[index].ToString("+000;-000").PadRight(5));
            StringBuilder.AppendLine();
        }


        public void Reset(bool PreserveStats)
        {
            if (!PreserveStats) Stats.Reset();
        }


        public string ToString(Position Position)
        {
            GetStaticScore(Position);
            int phase = DetermineGamePhase(Position);
            return _staticScore.ToString(phase);
        }
    }
}
