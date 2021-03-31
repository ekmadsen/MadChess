// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
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
    // TODO: Refactor evaluation into color-agnostic methods using delegates.
    public sealed class Evaluation
    {
        private readonly Stats _stats;
        private readonly EvaluationConfig _defaultConfig;
        private readonly Delegates.IsRepeatPosition _isRepeatPosition;
        private readonly Delegates.Debug _debug;
        private readonly Delegates.WriteMessageLine _writeMessageLine;
        private readonly StaticScore _staticScore;
        // Game Phase (constants selected such that starting material = 256)
        public const int MiddlegamePhase = 4 * (_knightPhase + _bishopPhase + _rookPhase) + 2 * _queenPhase;
        private const int _knightPhase = 10; //   4 * 10 =  40
        private const int _bishopPhase = 10; // + 4 * 10 =  80
        private const int _rookPhase = 22; //   + 4 * 22 = 168
        private const int _queenPhase = 44; //  + 2 * 44 = 256
        // Material
        public const int MgPawnMaterial = 100;
        public readonly EvaluationConfig Config;
        public int DrawMoves;
        public bool UnderstandsPieceLocation;
        public bool UnderstandsPassedPawns;
        public bool UnderstandsMobility;
        public bool UnderstandsKingSafety;
        private const int _knightExchangeMaterial = 300;
        private const int _bishopExchangeMaterial = 300;
        private const int _rookExchangeMaterial = 500;
        private const int _queenExchangeMaterial = 900;
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
        // Piece Mobility
        private readonly int[] _mgKnightMobility;
        private readonly int[] _egKnightMobility;
        private readonly int[] _mgBishopMobility;
        private readonly int[] _egBishopMobility;
        private readonly int[] _mgRookMobility;
        private readonly int[] _egRookMobility;
        private readonly int[] _mgQueenMobility;
        private readonly int[] _egQueenMobility;
        // King Safety
        private readonly int[] _kingSafety;

        
        public Evaluation(Stats Stats, Delegates.IsRepeatPosition IsRepeatPosition, Delegates.Debug Debug, Delegates.WriteMessageLine WriteMessageLine)
        {
            _stats = Stats;
            _isRepeatPosition = IsRepeatPosition;
            _debug = Debug;
            _writeMessageLine = WriteMessageLine;
            _staticScore = new StaticScore();
            // Don't set Config and _defaultConfig to same object in memory (reference equality) to avoid ConfigureStrength method overwriting defaults.
            Config = new EvaluationConfig();
            _defaultConfig = new EvaluationConfig();
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
            _kingSafety = new int[64];
            // Calculate positional factors and set default positional understanding.
            CalculatePositionalFactors();
            SetDefaultPositionalUnderstanding();
        }


        public void CalculatePositionalFactors()
        {
            // Calculate piece location values.
            for (var square = 0; square < 64; square++)
            {
                var rank = Board.WhiteRanks[square];
                var file = Board.Files[square];
                var squareCentrality = 3 - Board.DistanceToCentralSquares[square];
                var fileCentrality = 3 - Math.Min(Math.Abs(3 - file), Math.Abs(4 - file));
                var nearCorner = 3 - Board.DistanceToNearestCorner[square];
                _mgPawnLocations[square] = rank * Config.MgPawnAdvancement + squareCentrality * Config.MgPawnCentrality;
                _egPawnLocations[square] = rank * Config.EgPawnAdvancement + squareCentrality * Config.EgPawnCentrality;
                _mgKnightLocations[square] = rank * Config.MgKnightAdvancement + squareCentrality * Config.MgKnightCentrality + nearCorner * Config.MgKnightCorner;
                _egKnightLocations[square] = rank * Config.EgKnightAdvancement + squareCentrality * Config.EgKnightCentrality + nearCorner * Config.EgKnightCorner;
                _mgBishopLocations[square] = rank * Config.MgBishopAdvancement + squareCentrality * Config.MgBishopCentrality + nearCorner * Config.MgBishopCorner;
                _egBishopLocations[square] = rank * Config.EgBishopAdvancement + squareCentrality * Config.EgBishopCentrality + nearCorner * Config.EgBishopCorner;
                _mgRookLocations[square] = rank * Config.MgRookAdvancement + fileCentrality * Config.MgRookCentrality + nearCorner * Config.MgRookCorner;
                _egRookLocations[square] = rank * Config.EgRookAdvancement + squareCentrality * Config.EgRookCentrality + nearCorner * Config.EgRookCorner;
                _mgQueenLocations[square] = rank * Config.MgQueenAdvancement + squareCentrality * Config.MgQueenCentrality + nearCorner * Config.MgQueenCorner;
                _egQueenLocations[square] = rank * Config.EgQueenAdvancement + squareCentrality * Config.EgQueenCentrality + nearCorner * Config.EgQueenCorner;
                _mgKingLocations[square] = rank * Config.MgKingAdvancement + squareCentrality * Config.MgKingCentrality + nearCorner * Config.MgKingCorner;
                _egKingLocations[square] = rank * Config.EgKingAdvancement + squareCentrality * Config.EgKingCentrality + nearCorner * Config.EgKingCorner;
            }
            // Calculate passed pawn values.
            var passedPawnPower = Config.PassedPawnPowerPer128 / 128d;
            var mgScale = Config.MgPassedPawnScalePer128 / 128d;
            var egScale = Config.EgPassedPawnScalePer128 / 128d;
            var egFreeScale = Config.EgFreePassedPawnScalePer128 / 128d;
            for (var rank = 1; rank < 7; rank++)
            {
                _mgPassedPawns[rank] = GetNonLinearBonus(rank, mgScale, passedPawnPower, 0);
                _egPassedPawns[rank] = GetNonLinearBonus(rank, egScale, passedPawnPower, 0);
                _egFreePassedPawns[rank] = GetNonLinearBonus(rank, egFreeScale, passedPawnPower, 0);
            }
            // Calculate piece mobility values.
            CalculatePieceMobility(_mgKnightMobility, _egKnightMobility, Config.MgKnightMobilityScale, Config.EgKnightMobilityScale);
            CalculatePieceMobility(_mgBishopMobility, _egBishopMobility, Config.MgBishopMobilityScale, Config.EgBishopMobilityScale);
            CalculatePieceMobility(_mgRookMobility, _egRookMobility, Config.MgRookMobilityScale, Config.EgRookMobilityScale);
            CalculatePieceMobility(_mgQueenMobility, _egQueenMobility, Config.MgQueenMobilityScale, Config.EgQueenMobilityScale);
            // Calculate king safety values.
            var kingSafetyPower = Config.KingSafetyPowerPer128 / 128d;
            for (var index = 0; index < _kingSafety.Length; index++)
            {
                var scale = -Config.KingSafetyScalePer128 / 128d;
                _kingSafety[index] = GetNonLinearBonus(index, scale, kingSafetyPower, 0);
            }
        }


        private void CalculatePieceMobility(int[] MgPieceMobility, int[] EgPieceMobility, int MgMobilityScale, int EgMobilityScale)
        {
            Debug.Assert(MgPieceMobility.Length == EgPieceMobility.Length);
            var maxMoves = MgPieceMobility.Length - 1;
            var pieceMobilityPower = Config.PieceMobilityPowerPer128 / 128d;
            for (var moves = 0; moves <= maxMoves; moves++)
            {
                var percentMaxMoves = (double)moves / maxMoves;
                MgPieceMobility[moves] = GetNonLinearBonus(percentMaxMoves, MgMobilityScale, pieceMobilityPower, -MgMobilityScale / 2);
                EgPieceMobility[moves] = GetNonLinearBonus(percentMaxMoves, EgMobilityScale, pieceMobilityPower, -EgMobilityScale / 2);
            }
            // Adjust constant so piece mobility bonus for average number of moves is zero.
            var averageMoves = maxMoves / 2;
            var averageMgBonus = MgPieceMobility[averageMoves];
            var averageEgBonus = EgPieceMobility[averageMoves];
            for (var moves = 0; moves <= maxMoves; moves++)
            {
                MgPieceMobility[moves] -= averageMgBonus;
                EgPieceMobility[moves] -= averageEgBonus;
            }
        }


        private void SetDefaultPositionalUnderstanding()
        {
            DrawMoves = 2;
            UnderstandsPieceLocation = true;
            UnderstandsPassedPawns = true;
            UnderstandsMobility = true;
            UnderstandsKingSafety = true;
        }


        public void ConfigureStrength(int Elo)
        {
            // TODO: Interpolate positional understanding within rating classes.
            // Set default positional understanding.
            SetDefaultPositionalUnderstanding();
            Config.Set(_defaultConfig);
            // Limit material and positional understanding.
            if (Elo < 800)
            {
                // Beginner
                // Undervalue rook and overvalue queen.
                Config.MgRookMaterial = 300;
                Config.EgRookMaterial = 300;
                Config.MgQueenMaterial = 1200;
                Config.EgQueenMaterial = 1200;
            }
            if (Elo < 1000)
            {
                // Novice
                // Value knight and bishop equally.
                Config.MgBishopMaterial = Config.MgKnightMaterial;
                Config.EgBishopMaterial = Config.EgKnightMaterial;
                // Misjudge danger of passed pawns.
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
                UnderstandsKingSafety = false;
                //UnderstandsThreats = false;
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
            if (_debug())
            {
                _writeMessageLine($"info string {nameof(MgPawnMaterial)} = {MgPawnMaterial}");
                _writeMessageLine($"info string {nameof(Config.EgPawnMaterial)} = {Config.EgPawnMaterial}");
                _writeMessageLine($"info string {nameof(Config.MgKnightMaterial)} = {Config.MgKnightMaterial}");
                _writeMessageLine($"info string {nameof(Config.EgKnightMaterial)} = {Config.EgKnightMaterial}");
                _writeMessageLine($"info string {nameof(Config.MgBishopMaterial)} = {Config.MgBishopMaterial}");
                _writeMessageLine($"info string {nameof(Config.EgBishopMaterial)} = {Config.EgBishopMaterial}");
                _writeMessageLine($"info string {nameof(Config.MgRookMaterial)} = {Config.MgRookMaterial}");
                _writeMessageLine($"info string {nameof(Config.EgRookMaterial)} = {Config.EgRookMaterial}");
                _writeMessageLine($"info string {nameof(Config.MgQueenMaterial)} = {Config.MgQueenMaterial}");
                _writeMessageLine($"info string {nameof(Config.EgQueenMaterial)} = {Config.EgQueenMaterial}");
                _writeMessageLine($"info string UnderstandsPieceLocation = {UnderstandsPieceLocation}");
                _writeMessageLine($"info string UnderstandsPassedPawns = {UnderstandsPassedPawns}");
                _writeMessageLine($"info string UnderstandsMobility = {UnderstandsMobility}");
                _writeMessageLine($"info string UnderstandsKingSafety = {UnderstandsKingSafety}");
                //_writeMessageLine($"info string UnderstandsThreats = {UnderstandsThreats}");
                //_writeMessageLine($"info string UnderstandsBishopPair = {UnderstandsBishopPair}");
                //_writeMessageLine($"info string UnderstandsOutposts = {UnderstandsOutposts}");
                //_writeMessageLine($"info string Understands7thRank = {Understands7thRank}");
                //_writeMessageLine($"info string UnderstandsTrades = {UnderstandsTrades}");
            }
        }


        public (bool TerminalDraw, bool RepeatPosition) IsTerminalDraw(Position Position)
        {
            // Only return true if position is drawn and no sequence of moves can make game winnable.
            if (_isRepeatPosition(DrawMoves)) return (true, true); // Draw by repetition of position.
            if (Position.PlySinceCaptureOrPawnMove >= StaticScore.MaxPlyWithoutCaptureOrPawnMove) return (true, false); // Draw by 50 moves (100 ply) without a capture or pawn move.
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
                        return (true, false);
                    }
                }
            }
            return (false, false);
        }


        public (int StaticScore, bool DrawnEndgame) GetStaticScore(Position Position)
        {
            Debug.Assert(!Position.KingInCheck);
            _stats.Evaluations++;
            _staticScore.Reset();
            if (EvaluateSimpleEndgame(Position))
            {
                // TODO: Return DrawnEndgame = True when _staticScore.WhiteEg == _staticScore.BlackEg.
                return Position.WhiteMove
                    ? (_staticScore.WhiteEg - _staticScore.BlackEg, false)
                    : (_staticScore.BlackEg - _staticScore.WhiteEg, false);
            }
            // Not a simple endgame.
            _staticScore.PlySinceCaptureOrPawnMove = Position.PlySinceCaptureOrPawnMove;
            EvaluateMaterial(Position);
            if (UnderstandsPieceLocation) EvaluatePieceLocation(Position);
            if (UnderstandsPassedPawns) EvaluatePawns(Position);
            EvaluatePieceMobilityKingSafety(Position);
            if (!UnderstandsMobility)
            {
                _staticScore.WhiteMgPieceMobility = 0;
                _staticScore.WhiteEgPieceMobility = 0;
                _staticScore.BlackMgPieceMobility = 0;
                _staticScore.BlackEgPieceMobility = 0;
            }
            if (!UnderstandsKingSafety)
            {
                _staticScore.WhiteMgKingSafety = 0;
                _staticScore.WhiteEgKingSafety = 0;
                _staticScore.BlackMgKingSafety = 0;
                _staticScore.BlackEgKingSafety = 0;
            }
            EvaluateMinorPieces(Position);
            DetermineEndgameScale(Position); // Scale down scores for difficult to win endgames.
            if (_staticScore.EgScalePer128 == 0) return (0, true); // Drawn Endgame
            var phase = DetermineGamePhase(Position);
            return Position.WhiteMove
                ? (_staticScore.GetTotalScore(phase), false)
                : (-_staticScore.GetTotalScore(phase), false);
        }


        private bool EvaluateSimpleEndgame(Position Position)
        {
            var whitePawns = Bitwise.CountSetBits(Position.WhitePawns);
            var whiteKnights = Bitwise.CountSetBits(Position.WhiteKnights);
            var whiteBishops = Bitwise.CountSetBits(Position.WhiteBishops);
            var whiteMinorPieces = whiteKnights + whiteBishops;
            var whiteMajorPieces = Bitwise.CountSetBits(Position.WhiteRooks | Position.WhiteQueens);
            var whitePawnsAndPieces = whitePawns + whiteMinorPieces + whiteMajorPieces;
            var blackPawns = Bitwise.CountSetBits(Position.BlackPawns);
            var blackKnights = Bitwise.CountSetBits(Position.BlackKnights);
            var blackBishops = Bitwise.CountSetBits(Position.BlackBishops);
            var blackMinorPieces = blackKnights + blackBishops;
            var blackMajorPieces = Bitwise.CountSetBits(Position.BlackRooks | Position.BlackQueens);
            var blackPawnsAndPieces = blackPawns + blackMinorPieces + blackMajorPieces;
            if ((whitePawnsAndPieces > 0) && (blackPawnsAndPieces > 0)) return false; // Position is not a simple endgame.
            var loneWhitePawn = (whitePawns == 1) && (whitePawnsAndPieces == 1) && (blackPawnsAndPieces == 0);
            var loneBlackPawn = (blackPawns == 1) && (blackPawnsAndPieces == 1) && (whitePawnsAndPieces == 0);
            var whiteKingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
            var blackKingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (whitePawnsAndPieces)
            {
                // Case 0 = Lone White King
                case 0 when loneBlackPawn:
                    return EvaluateKingVersusPawn(Position, false);
                // ReSharper disable once SwitchStatementMissingSomeCases
                case 0:
                    switch (blackPawns)
                    {
                        case 0 when (blackKnights == 1) && (blackBishops == 1) && (blackMajorPieces == 0):
                            // King Versus Knight and Bishop
                            var lightSquareBishop = Board.LightSquares[Bitwise.FindFirstSetBit(Position.BlackBishops)];
                            var distanceToCorrectColorCorner = lightSquareBishop
                                ? Board.DistanceToNearestLightCorner[whiteKingSquare]
                                : Board.DistanceToNearestDarkCorner[whiteKingSquare];
                            _staticScore.BlackEgSimple = Config.SimpleEndgame - distanceToCorrectColorCorner - Board.SquareDistances[whiteKingSquare][blackKingSquare];
                            return true;
                        case 0 when (blackMinorPieces == 0) && (blackMajorPieces >= 1):
                            // King Versus Major Pieces
                            _staticScore.BlackEgSimple = Config.SimpleEndgame - Board.DistanceToNearestCorner[whiteKingSquare] - Board.SquareDistances[whiteKingSquare][blackKingSquare];
                            return true;
                    }
                    break;
            }
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (blackPawnsAndPieces)
            {
                // Case 0 = Lone Black King
                case 0 when loneWhitePawn:
                    return EvaluateKingVersusPawn(Position, true);
                // ReSharper disable once SwitchStatementMissingSomeCases
                case 0:
                    switch (whitePawns)
                    {
                        case 0 when (whiteKnights == 1) && (whiteBishops == 1) && (whiteMajorPieces == 0):
                            // King Versus Knight and Bishop
                            var lightSquareBishop = Board.LightSquares[Bitwise.FindFirstSetBit(Position.WhiteBishops)];
                            var distanceToCorrectColorCorner = lightSquareBishop
                                ? Board.DistanceToNearestLightCorner[blackKingSquare]
                                : Board.DistanceToNearestDarkCorner[blackKingSquare];
                            _staticScore.WhiteEgSimple = Config.SimpleEndgame - distanceToCorrectColorCorner - Board.SquareDistances[whiteKingSquare][blackKingSquare];
                            return true;
                        case 0 when (whiteMinorPieces == 0) && (whiteMajorPieces >= 1):
                            // King Versus Major Pieces
                            _staticScore.WhiteEgSimple = Config.SimpleEndgame - Board.DistanceToNearestCorner[blackKingSquare] - Board.SquareDistances[whiteKingSquare][blackKingSquare];
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
                // White Winning
                var winningKingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
                winningKingRank = Board.WhiteRanks[winningKingSquare];
                winningKingFile = Board.Files[winningKingSquare];
                var defendingKingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
                defendingKingRank = Board.WhiteRanks[defendingKingSquare];
                defendingKingFile = Board.Files[defendingKingSquare];
                var pawnSquare = Bitwise.FindFirstSetBit(Position.WhitePawns);
                pawnRank = Board.WhiteRanks[pawnSquare];
                pawnFile = Board.Files[pawnSquare];
            }
            else
            {
                // Black Winning
                var winningKingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
                winningKingRank = Board.BlackRanks[winningKingSquare];
                winningKingFile = Board.Files[winningKingSquare];
                var defendingKingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
                defendingKingRank = Board.BlackRanks[defendingKingSquare];
                defendingKingFile = Board.Files[defendingKingSquare];
                var pawnSquare = Bitwise.FindFirstSetBit(Position.BlackPawns);
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
                var kingPawnRankDifference = winningKingRank - pawnRank;
                var kingPawnAbsoluteFileDifference = Math.Abs(winningKingFile - pawnFile);
                var winningKingOnKeySquare = pawnRank switch
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
                    if (LoneWhitePawn) _staticScore.WhiteEgSimple = Config.SimpleEndgame + pawnRank;
                    else _staticScore.BlackEgSimple = Config.SimpleEndgame + pawnRank;
                    return true;
                }
            }
            // Use regular evaluation.
            return false;
        }

        
        private void EvaluateMaterial(Position Position)
        {
            _staticScore.WhiteMgPawnMaterial = Bitwise.CountSetBits(Position.WhitePawns) * MgPawnMaterial;
            _staticScore.WhiteMgPieceMaterial = Bitwise.CountSetBits(Position.WhiteKnights) * Config.MgKnightMaterial + Bitwise.CountSetBits(Position.WhiteBishops) * Config.MgBishopMaterial +
                                                Bitwise.CountSetBits(Position.WhiteRooks) * Config.MgRookMaterial + Bitwise.CountSetBits(Position.WhiteQueens) * Config.MgQueenMaterial;
            _staticScore.WhiteEgPawnMaterial = Bitwise.CountSetBits(Position.WhitePawns) * Config.EgPawnMaterial;
            _staticScore.WhiteEgPieceMaterial = Bitwise.CountSetBits(Position.WhiteKnights) * Config.EgKnightMaterial + Bitwise.CountSetBits(Position.WhiteBishops) * Config.EgBishopMaterial +
                                                Bitwise.CountSetBits(Position.WhiteRooks) * Config.EgRookMaterial + Bitwise.CountSetBits(Position.WhiteQueens) * Config.EgQueenMaterial;

            _staticScore.BlackMgPawnMaterial = Bitwise.CountSetBits(Position.BlackPawns) * MgPawnMaterial;
            _staticScore.BlackMgPieceMaterial = Bitwise.CountSetBits(Position.BlackKnights) * Config.MgKnightMaterial + Bitwise.CountSetBits(Position.BlackBishops) * Config.MgBishopMaterial +
                                                Bitwise.CountSetBits(Position.BlackRooks) * Config.MgRookMaterial + Bitwise.CountSetBits(Position.BlackQueens) * Config.MgQueenMaterial;
            _staticScore.BlackEgPawnMaterial = Bitwise.CountSetBits(Position.BlackPawns) * Config.EgPawnMaterial;
            _staticScore.BlackEgPieceMaterial = Bitwise.CountSetBits(Position.BlackKnights) * Config.EgKnightMaterial + Bitwise.CountSetBits(Position.BlackBishops) * Config.EgBishopMaterial +
                                                Bitwise.CountSetBits(Position.BlackRooks) * Config.EgRookMaterial + Bitwise.CountSetBits(Position.BlackQueens) * Config.EgQueenMaterial;
        }


        public int GetMaterialScore(Position Position, int Piece)
        {
            int mgMaterial;
            int egMaterial;
            // Sequence cases in order of integer value to improve performance of switch statement.
            switch (Piece)
            {
                case Engine.Piece.None:
                    mgMaterial = 0;
                    egMaterial = 0;
                    break;
                case Engine.Piece.WhitePawn:
                    mgMaterial = MgPawnMaterial;
                    egMaterial = Config.EgPawnMaterial;
                    break;
                case Engine.Piece.WhiteKnight:
                    mgMaterial = Config.MgKnightMaterial;
                    egMaterial = Config.EgKnightMaterial;
                    break;
                case Engine.Piece.WhiteBishop:
                    mgMaterial = Config.MgBishopMaterial;
                    egMaterial = Config.EgBishopMaterial;
                    break;
                case Engine.Piece.WhiteRook:
                    mgMaterial = Config.MgRookMaterial;
                    egMaterial = Config.EgRookMaterial;
                    break;
                case Engine.Piece.WhiteQueen:
                    mgMaterial = Config.MgQueenMaterial;
                    egMaterial = Config.EgQueenMaterial;
                    break;
                case Engine.Piece.WhiteKing:
                    mgMaterial = 0;
                    egMaterial = 0;
                    break;
                case Engine.Piece.BlackPawn:
                    mgMaterial = MgPawnMaterial;
                    egMaterial = Config.EgPawnMaterial;
                    break;
                case Engine.Piece.BlackKnight:
                    mgMaterial = Config.MgKnightMaterial;
                    egMaterial = Config.EgKnightMaterial;
                    break;
                case Engine.Piece.BlackBishop:
                    mgMaterial = Config.MgBishopMaterial;
                    egMaterial = Config.EgBishopMaterial;
                    break;
                case Engine.Piece.BlackRook:
                    mgMaterial = Config.MgRookMaterial;
                    egMaterial = Config.EgRookMaterial;
                    break;
                case Engine.Piece.BlackQueen:
                    mgMaterial = Config.MgQueenMaterial;
                    egMaterial = Config.EgQueenMaterial;
                    break;
                case Engine.Piece.BlackKing:
                    mgMaterial = 0;
                    egMaterial = 0;
                    break;
                default:
                    throw new ArgumentException($"{Piece} piece not supported.");
            }
            var phase = DetermineGamePhase(Position);
            return StaticScore.GetTaperedScore(mgMaterial, egMaterial, phase);
        }


        public static (int StaticScore, bool DrawnEndgame) GetExchangeMaterialScore(Position Position)
        {
            var whiteScore = Bitwise.CountSetBits(Position.WhitePawns) * MgPawnMaterial +
                             Bitwise.CountSetBits(Position.WhiteKnights) * _knightExchangeMaterial + Bitwise.CountSetBits(Position.WhiteBishops) * _bishopExchangeMaterial +
                             Bitwise.CountSetBits(Position.WhiteRooks) * _rookExchangeMaterial + Bitwise.CountSetBits(Position.WhiteQueens) * _queenExchangeMaterial;
            var blackScore = Bitwise.CountSetBits(Position.BlackPawns) * MgPawnMaterial +
                             Bitwise.CountSetBits(Position.BlackKnights) * _knightExchangeMaterial + Bitwise.CountSetBits(Position.BlackBishops) * _bishopExchangeMaterial +
                             Bitwise.CountSetBits(Position.BlackRooks) * _rookExchangeMaterial + Bitwise.CountSetBits(Position.BlackQueens) * _queenExchangeMaterial;
            return Position.WhiteMove
                ? (whiteScore - blackScore, false)
                : (blackScore - whiteScore, false);
        }


        private void EvaluatePieceLocation(Position Position)
        {
            // Pawns
            int square;
            int blackSquare;
            var pieces = Position.WhitePawns;
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
            var pawns = Position.WhitePawns;
            var kingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
            var enemyKingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
            int pawnSquare;
            int rank;
            while ((pawnSquare = Bitwise.FindFirstSetBit(pawns)) != Square.Illegal)
            {
                if (IsPassedPawn(Position, pawnSquare, true))
                {
                    _staticScore.WhitePassedPawnCount++;
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
            // Black Pawns
            pawns = Position.BlackPawns;
            kingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
            enemyKingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
            while ((pawnSquare = Bitwise.FindFirstSetBit(pawns)) != Square.Illegal)
            {
                if (IsPassedPawn(Position, pawnSquare, false))
                {
                    _staticScore.BlackPassedPawnCount++;
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            var file = Board.Files[PawnSquare];
            int promotionSquare;
            int enemyPieces;
            if (White)
            {
                // White Pawn
                promotionSquare = Board.GetSquare(file, 7);
                enemyPieces = Bitwise.CountSetBits(Position.BlackKnights | Position.BlackBishops | Position.BlackRooks | Position.BlackQueens);
            }
            else
            {
                // Black Pawn
                promotionSquare = Board.GetSquare(file, 0);
                enemyPieces = Bitwise.CountSetBits(Position.WhiteKnights | Position.WhiteBishops | Position.WhiteRooks | Position.WhiteQueens);
            }
            if (enemyPieces == 0)
            {
                // Enemy has no minor or major pieces.
                var pawnDistanceToPromotionSquare = Board.SquareDistances[PawnSquare][promotionSquare];
                var kingDistanceToPromotionSquare = Board.SquareDistances[EnemyKingSquare][promotionSquare];
                if (White != Position.WhiteMove) kingDistanceToPromotionSquare--; // Enemy king can move one square closer to pawn.
                return kingDistanceToPromotionSquare > pawnDistanceToPromotionSquare; // Enemy king cannot stop pawn from promoting.
            }
            return false;
        }



        // TODO: Include stacked attacks on same square via x-rays.  For example, a rook behind a queen.
        private void EvaluatePieceMobilityKingSafety(Position Position)
        {
            var whiteKingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
            var whiteKingInnerRing = Board.InnerRingMasks[whiteKingSquare];
            var whiteKingOuterRing = Board.OuterRingMasks[whiteKingSquare];
            var blackKingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
            var blackKingInnerRing = Board.InnerRingMasks[blackKingSquare];
            var blackKingOuterRing = Board.OuterRingMasks[blackKingSquare];
            int square;
            ulong pieceDestinations;
            int mgPieceMobilityScore;
            int egPieceMobilityScore;
            int kingSafetyIndexIncrementPer8;
            var whiteMgKingSafetyIndexPer8 = 0;
            var whiteEgKingSafetyIndexPer8 = 0;
            var blackMgKingSafetyIndexPer8 = 0;
            var blackEgKingSafetyIndexPer8 = 0;
            // White Knights
            var pieces = Position.WhiteKnights;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetKnightDestinations(Position, square, true);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgKnightMobility, _egKnightMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, blackKingOuterRing, blackKingInnerRing, Config.KingSafetyMinorAttackOuterRingPer8, Config.KingSafetyMinorAttackInnerRingPer8);
                blackMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                blackEgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Black Knights
            pieces = Position.BlackKnights;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetKnightDestinations(Position, square, false);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgKnightMobility, _egKnightMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, whiteKingOuterRing, whiteKingInnerRing, Config.KingSafetyMinorAttackOuterRingPer8, Config.KingSafetyMinorAttackInnerRingPer8);
                whiteMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                whiteEgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // White Bishops
            pieces = Position.WhiteBishops;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetBishopDestinations(Position, square, true);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgBishopMobility, _egBishopMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, blackKingOuterRing, blackKingInnerRing, Config.KingSafetyMinorAttackOuterRingPer8, Config.KingSafetyMinorAttackInnerRingPer8);
                blackMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                blackEgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Black Bishops
            pieces = Position.BlackBishops;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetBishopDestinations(Position, square, false);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgBishopMobility, _egBishopMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, whiteKingOuterRing, whiteKingInnerRing, Config.KingSafetyMinorAttackOuterRingPer8, Config.KingSafetyMinorAttackInnerRingPer8);
                whiteMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                whiteEgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // White Rooks
            pieces = Position.WhiteRooks;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetRookDestinations(Position, square, true);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgRookMobility, _egRookMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, blackKingOuterRing, blackKingInnerRing, Config.KingSafetyRookAttackOuterRingPer8, Config.KingSafetyRookAttackInnerRingPer8);
                blackMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                blackEgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Black Rooks
            pieces = Position.BlackRooks;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetRookDestinations(Position, square, false);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgRookMobility, _egRookMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, whiteKingOuterRing, whiteKingInnerRing, Config.KingSafetyRookAttackOuterRingPer8, Config.KingSafetyRookAttackInnerRingPer8);
                whiteMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                whiteEgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // White Queens
            pieces = Position.WhiteQueens;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetQueenDestinations(Position, square, true);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgQueenMobility, _egQueenMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, blackKingOuterRing, blackKingInnerRing, Config.KingSafetyQueenAttackOuterRingPer8, Config.KingSafetyQueenAttackInnerRingPer8);
                blackMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                blackEgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Black Queens
            pieces = Position.BlackQueens;
            while ((square = Bitwise.FindFirstSetBit(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetQueenDestinations(Position, square, false);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgQueenMobility, _egQueenMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, whiteKingOuterRing, whiteKingInnerRing, Config.KingSafetyQueenAttackOuterRingPer8, Config.KingSafetyQueenAttackInnerRingPer8);
                whiteMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                whiteEgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Evaluate white king near semi-open file.
            var kingSquare = Bitwise.FindFirstSetBit(Position.WhiteKing);
            var kingFile = Board.Files[kingSquare];
            var leftFileMask = kingFile > 0 ? Board.FileMasks[kingFile - 1] : 0;
            var kingFileMask = Board.FileMasks[kingFile];
            var rightFileMask = kingFile < 7 ? Board.FileMasks[kingFile + 1] : 0;
            var leftFileSemiOpen = (leftFileMask > 0) && ((Position.WhitePawns & leftFileMask) == 0) ? 1 : 0;
            var kingFileSemiOpen = (Position.WhitePawns & kingFileMask) == 0 ? 1 : 0;
            var rightFileSemiOpen = (rightFileMask > 0) && ((Position.WhitePawns & rightFileMask) == 0) ? 1 : 0;
            var semiOpenFiles = leftFileSemiOpen + kingFileSemiOpen + rightFileSemiOpen;
            whiteMgKingSafetyIndexPer8 += semiOpenFiles * Config.MgKingSafetySemiOpenFilePer8;
            // Evaluate black king near semi-open file.
            kingSquare = Bitwise.FindFirstSetBit(Position.BlackKing);
            kingFile = Board.Files[kingSquare];
            rightFileMask = kingFile > 0 ? Board.FileMasks[kingFile - 1] : 0;
            kingFileMask = Board.FileMasks[kingFile];
            leftFileMask = kingFile < 7 ? Board.FileMasks[kingFile + 1] : 0;
            leftFileSemiOpen = (leftFileMask > 0) && ((Position.BlackPawns & leftFileMask) == 0) ? 1 : 0;
            kingFileSemiOpen = (Position.BlackPawns & kingFileMask) == 0 ? 1 : 0;
            rightFileSemiOpen = (rightFileMask > 0) && ((Position.BlackPawns & rightFileMask) == 0) ? 1 : 0;
            semiOpenFiles = leftFileSemiOpen + kingFileSemiOpen + rightFileSemiOpen;
            blackMgKingSafetyIndexPer8 += semiOpenFiles * Config.MgKingSafetySemiOpenFilePer8;
            // Lookup king safety score in array.
            var maxIndex = _kingSafety.Length - 1;
            _staticScore.WhiteMgKingSafety = _kingSafety[Math.Min(whiteMgKingSafetyIndexPer8 / 8, maxIndex)];
            _staticScore.WhiteEgKingSafety = _kingSafety[Math.Min(whiteEgKingSafetyIndexPer8 / 8, maxIndex)];
            _staticScore.BlackMgKingSafety = _kingSafety[Math.Min(blackMgKingSafetyIndexPer8 / 8, maxIndex)];
            _staticScore.BlackEgKingSafety = _kingSafety[Math.Min(blackEgKingSafetyIndexPer8 / 8, maxIndex)];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (int MiddlegameMobility, int EndgameMobility) GetPieceMobilityScore(ulong PieceDestinations, int[] MgPieceMobility, int[] EgPieceMobility)
        {
            var moves = Bitwise.CountSetBits(PieceDestinations);
            var mgMoveIndex = Math.Min(moves, MgPieceMobility.Length - 1);
            var egMoveIndex = Math.Min(moves, EgPieceMobility.Length - 1);
            return (MgPieceMobility[mgMoveIndex], EgPieceMobility[egMoveIndex]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetKingSafetyIndexIncrement(ulong PieceDestinations, ulong KingOuterRing, ulong KingInnerRing, int OuterRingAttackWeight, int InnerRingAttackWeight)
        {
            var attackedOuterRingSquares = Bitwise.CountSetBits(PieceDestinations & KingOuterRing);
            var attackedInnerRingSquares = Bitwise.CountSetBits(PieceDestinations & KingInnerRing);
            return (attackedOuterRingSquares * OuterRingAttackWeight) + (attackedInnerRingSquares * InnerRingAttackWeight);
        }


        private void EvaluateMinorPieces(Position Position)
        {
            // Bishop Pair
            var whiteBishops = Bitwise.CountSetBits(Position.WhiteBishops);
            if (whiteBishops >= 2)
            {
                _staticScore.WhiteMgBishopPair += Config.MgBishopPair;
                _staticScore.WhiteEgBishopPair += Config.EgBishopPair;
            }
            var blackBishops = Bitwise.CountSetBits(Position.BlackBishops);
            if (blackBishops >= 2)
            {
                _staticScore.BlackMgBishopPair += Config.MgBishopPair;
                _staticScore.BlackEgBishopPair += Config.EgBishopPair;
            }
        }


        // Ideas borrowed from Stockfish chess engine.
        // TODO: Extract constants to EvaluationConfig and tune.
        private void DetermineEndgameScale(Position Position)
        {
            // Use middlegame material values because those are constant (endgame material values are tuned).
            // Determine which color has a material advantage.
            int winningPawns;
            int winningPassedPawns;
            int winningPieces;
            int winningPieceMaterial;
            int losingPieceMaterial;
            if (_staticScore.WhiteEg >= _staticScore.BlackEg)
            {
                // White is winning the endgame.
                winningPawns = Bitwise.CountSetBits(Position.WhitePawns);
                winningPassedPawns = _staticScore.WhitePassedPawnCount;
                winningPieces = Bitwise.CountSetBits(Position.WhiteKnights | Position.WhiteBishops | Position.WhiteRooks | Position.WhiteQueens);
                winningPieceMaterial = _staticScore.WhiteMgPieceMaterial;
                losingPieceMaterial = _staticScore.BlackMgPieceMaterial;
            }
            else
            {
                // Black is winning the endgame.
                winningPawns = Bitwise.CountSetBits(Position.BlackPawns);
                winningPassedPawns = _staticScore.BlackPassedPawnCount;
                winningPieces = Bitwise.CountSetBits(Position.BlackKnights | Position.BlackBishops | Position.BlackRooks | Position.BlackQueens);
                winningPieceMaterial = _staticScore.BlackMgPieceMaterial;
                losingPieceMaterial = _staticScore.WhiteMgPieceMaterial;
            }
            // TODO: Should pawnsOnBothFlanks consider only the winning side?
            var pawns = Position.WhitePawns | Position.BlackPawns;
            var pawnsOnBothFlanks = ((pawns & Board.QueensideMask) > 0) && ((pawns & Board.KingsideMask) > 0);
            var pawnsOnSingleFlankScale = pawnsOnBothFlanks ? 0 : 8;
            var pieceMaterialDiff = winningPieceMaterial - losingPieceMaterial;
            if ((winningPawns == 0) && (pieceMaterialDiff <= Config.MgBishopMaterial))
            {
                // Winning side has no pawns and is up by a bishop or less.
                if (winningPieceMaterial < Config.MgRookMaterial) _staticScore.EgScalePer128 = 0;  // Winning side has less than a rook.
                else
                {
                    // Winning side has a rook or more.
                    _staticScore.EgScalePer128 = losingPieceMaterial <= Config.MgBishopMaterial
                        ? 8 // Losing side has a bishop or less.
                        : 28;  // Losing side has a rook or more.
                }
            }
            else if ((Bitwise.CountSetBits(Position.WhiteBishops) == 1) && (Bitwise.CountSetBits(Position.BlackBishops) == 1))
            {
                var whiteBishopLight = Board.LightSquares[Bitwise.FindFirstSetBit(Position.WhiteBishops)];
                var blackBishopLight = Board.LightSquares[Bitwise.FindFirstSetBit(Position.BlackBishops)];
                if (whiteBishopLight != blackBishopLight)
                {
                    // Opposite Colored Bishops
                    if ((winningPieceMaterial == Config.MgBishopMaterial) && (losingPieceMaterial == Config.MgBishopMaterial))
                    {
                        // Neither side has any other pieces.
                        _staticScore.EgScalePer128 = (winningPassedPawns * 8) + 36;
                    }
                    else
                    {
                        // Position includes pieces other than opposite colored bishops.
                        _staticScore.EgScalePer128 = (winningPieces * 6) + 44;
                    }
                }
            }
            else if (Bitwise.CountSetBits(Position.WhiteQueens | Position.BlackQueens) == 1)
            {
                // Queen versus no queen endgame.
                var minorPieces = Bitwise.CountSetBits(Position.WhiteQueens) == 1
                    ? Bitwise.CountSetBits(Position.BlackKnights | Position.BlackBishops)
                    : Bitwise.CountSetBits(Position.WhiteKnights | Position.WhiteBishops);
                _staticScore.EgScalePer128 = (minorPieces * 6) + 74;
            }
            else _staticScore.EgScalePer128 = (winningPawns * 14) + 72 - pawnsOnSingleFlankScale;
            _staticScore.EgScalePer128 -= pawnsOnSingleFlankScale;
            _staticScore.EgScalePer128 = Math.Max(Math.Min(_staticScore.EgScalePer128, 128), 0);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMateScore(int Depth) => -StaticScore.Max + Depth;
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMateMoveCount(int Score)
        {
            var plyCount = (Score > 0) ? StaticScore.Max - Score : -StaticScore.Max - Score;
            // Convert plies to full moves.
            var quotient = Math.DivRem(plyCount, 2, out var remainder);
            return quotient + remainder;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int DetermineGamePhase(Position Position)
        {
            var phase = _knightPhase * Bitwise.CountSetBits(Position.WhiteKnights | Position.BlackKnights) +
                        _bishopPhase * Bitwise.CountSetBits(Position.WhiteBishops | Position.BlackBishops) +
                        _rookPhase * Bitwise.CountSetBits(Position.WhiteRooks | Position.BlackRooks) +
                        _queenPhase * Bitwise.CountSetBits(Position.WhiteQueens | Position.BlackQueens);
            return Math.Min(phase, MiddlegamePhase);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNonLinearBonus(double Bonus, double Scale, double Power, int Constant) => (int)(Scale * Math.Pow(Bonus, Power)) + Constant;


        public string ShowParameters()
        {
            var stringBuilder = new StringBuilder();
            // Material
            stringBuilder.AppendLine("Material");
            stringBuilder.AppendLine("===========");
            stringBuilder.AppendLine($"MG Pawn:    {MgPawnMaterial}");
            stringBuilder.AppendLine($"EG Pawn:    {Config.EgPawnMaterial}");
            stringBuilder.AppendLine($"MG Knight:  {Config.MgKnightMaterial}");
            stringBuilder.AppendLine($"EG Knight:  {Config.EgKnightMaterial}");
            stringBuilder.AppendLine($"MG Bishop:  {Config.MgBishopMaterial}");
            stringBuilder.AppendLine($"EG Bishop:  {Config.EgBishopMaterial}");
            stringBuilder.AppendLine($"MG Rook:    {Config.MgRookMaterial}");
            stringBuilder.AppendLine($"EG Rook:    {Config.EgRookMaterial}");
            stringBuilder.AppendLine($"MG Queen:   {Config.MgQueenMaterial}");
            stringBuilder.AppendLine($"EG Queen:   {Config.EgQueenMaterial}");
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
            stringBuilder.AppendLine();
            // King Safety
            stringBuilder.AppendLine($"King Safety MgKingSafetySemiOpenFilePer8:        {Config.MgKingSafetySemiOpenFilePer8:000}");
            stringBuilder.AppendLine($"King Safety KingSafetyMinorAttackOuterRingPer8:  {Config.KingSafetyMinorAttackOuterRingPer8:000}");
            stringBuilder.AppendLine($"King Safety KingSafetyMinorAttackInnerRingPer8:  {Config.KingSafetyMinorAttackInnerRingPer8:000}");
            stringBuilder.AppendLine($"King Safety KingSafetyRookAttackOuterRingPer8:   {Config.KingSafetyRookAttackOuterRingPer8:000}");
            stringBuilder.AppendLine($"King Safety KingSafetyRookAttackInnerRingPer8:   {Config.KingSafetyRookAttackInnerRingPer8:000}");
            stringBuilder.AppendLine($"King Safety KingSafetyQueenAttackOuterRingPer8:  {Config.KingSafetyQueenAttackOuterRingPer8:000}");
            stringBuilder.AppendLine($"King Safety KingSafetyQueenAttackInnerRingPer8:  {Config.KingSafetyQueenAttackInnerRingPer8:000}");
            stringBuilder.AppendLine($"King Safety KingSafetyScalePer128:               {Config.KingSafetyScalePer128:000}");
            stringBuilder.AppendLine();
            stringBuilder.Append("King Safety:  ");
            ShowParameterArray(_kingSafety, stringBuilder);
            return stringBuilder.ToString();
        }


        private static void ShowParameterSquares(int[] Parameters, StringBuilder StringBuilder)
        {
            for (var rank = 7; rank >= 0; rank--)
            {
                for (var file = 0; file < 8; file++)
                {
                    var square = Board.GetSquare(file, rank);
                    StringBuilder.Append(Parameters[square].ToString("+000;-000").PadRight(6));
                }
                StringBuilder.AppendLine();
            }
        }


        private static void ShowParameterArray(int[] Parameters, StringBuilder StringBuilder)
        {
            for (var index = 0; index < Parameters.Length; index++) StringBuilder.Append(Parameters[index].ToString("+000;-000").PadRight(5));
            StringBuilder.AppendLine();
        }


        public string ToString(Position Position)
        {
            GetStaticScore(Position);
            var phase = DetermineGamePhase(Position);
            return _staticScore.ToString(phase);
        }
    }
}
