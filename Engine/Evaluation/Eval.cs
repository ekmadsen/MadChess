// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2021.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Core.Utilities;


namespace ErikTheCoder.MadChess.Engine.Evaluation
{
    // TODO: Refactor evaluation into color-agnostic methods using delegates.
    public sealed class Eval
    {
        private const int _beginnerElo = 800;
        private const int _noviceElo = 1000;
        private const int _socialElo = 1200;
        private const int _strongSocialElo = 1400;
        private const int _clubElo = 1600;
        private const int _strongClubElo = 1800;
        private readonly Stats _stats;
        private readonly EvalConfig _defaultConfig;
        private readonly Delegates.IsRepeatPosition _isRepeatPosition;
        private readonly Core.Delegates.Debug _debug;
        private readonly Core.Delegates.WriteMessageLine _writeMessageLine;
        private readonly StaticScore _staticScore;
        // Game Phase (constants selected such that starting material = 256)
        public const int MiddlegamePhase = 4 * (_knightPhase + _bishopPhase + _rookPhase) + (2 * _queenPhase);
        private const int _knightPhase = 10; //   4 * 10 =  40
        private const int _bishopPhase = 10; // + 4 * 10 =  80
        private const int _rookPhase = 22; //   + 4 * 22 = 168
        private const int _queenPhase = 44; //  + 2 * 44 = 256
        // Draw by Repetition
        public int DrawMoves;
        // Material
        public const int PawnMaterial = 100;
        public readonly EvalConfig Config;
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
        private readonly int[] _mgKingSafety;


        public Eval(Stats stats, Delegates.IsRepeatPosition isRepeatPosition, Core.Delegates.Debug debug, Core.Delegates.WriteMessageLine writeMessageLine)
        {
            _stats = stats;
            _isRepeatPosition = isRepeatPosition;
            _debug = debug;
            _writeMessageLine = writeMessageLine;
            _staticScore = new StaticScore();
            // Don't set Config and _defaultConfig to same object in memory (reference equality) to avoid ConfigureLimitedStrength method overwriting defaults.
            Config = new EvalConfig();
            _defaultConfig = new EvalConfig();
            // Create arrays for quick lookup of positional factors, then calculate positional factors.
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
            _egKingLocations = new int[64];// Create arrays for quick lookup of positional factors, then calculate positional factors.
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
            _mgKingSafety = new int[64];
            // Set number of repetitions considered a draw, calculate positional factors, and set evaluation strength.
            DrawMoves = 2;
            CalculatePositionalFactors();
            ConfigureFullStrength();
        }


        public void CalculatePositionalFactors()
        {
            // Calculate piece location values.
            for (var square = Square.A8; square < Square.Illegal; square++)
            {
                var rank = Board.Ranks[(int)Color.White][(int)square];
                var file = Board.Files[(int)square];
                var squareCentrality = 3 - Board.DistanceToCentralSquares[(int)square];
                var fileCentrality = 3 - Math.Min(Math.Abs(3 - file), Math.Abs(4 - file));
                var nearCorner = 3 - Board.DistanceToNearestCorner[(int)square];
                _mgPawnLocations[(int)square] = rank * Config.MgPawnAdvancement + squareCentrality * Config.MgPawnCentrality;
                _egPawnLocations[(int)square] = rank * Config.EgPawnAdvancement + squareCentrality * Config.EgPawnCentrality;
                _mgKnightLocations[(int)square] = rank * Config.MgKnightAdvancement + squareCentrality * Config.MgKnightCentrality + nearCorner * Config.MgKnightCorner;
                _egKnightLocations[(int)square] = rank * Config.EgKnightAdvancement + squareCentrality * Config.EgKnightCentrality + nearCorner * Config.EgKnightCorner;
                _mgBishopLocations[(int)square] = rank * Config.MgBishopAdvancement + squareCentrality * Config.MgBishopCentrality + nearCorner * Config.MgBishopCorner;
                _egBishopLocations[(int)square] = rank * Config.EgBishopAdvancement + squareCentrality * Config.EgBishopCentrality + nearCorner * Config.EgBishopCorner;
                _mgRookLocations[(int)square] = rank * Config.MgRookAdvancement + fileCentrality * Config.MgRookCentrality + nearCorner * Config.MgRookCorner;
                _egRookLocations[(int)square] = rank * Config.EgRookAdvancement + squareCentrality * Config.EgRookCentrality + nearCorner * Config.EgRookCorner;
                _mgQueenLocations[(int)square] = rank * Config.MgQueenAdvancement + squareCentrality * Config.MgQueenCentrality + nearCorner * Config.MgQueenCorner;
                _egQueenLocations[(int)square] = rank * Config.EgQueenAdvancement + squareCentrality * Config.EgQueenCentrality + nearCorner * Config.EgQueenCorner;
                _mgKingLocations[(int)square] = rank * Config.MgKingAdvancement + squareCentrality * Config.MgKingCentrality + nearCorner * Config.MgKingCorner;
                _egKingLocations[(int)square] = rank * Config.EgKingAdvancement + squareCentrality * Config.EgKingCentrality + nearCorner * Config.EgKingCorner;
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
            var kingSafetyPower = Config.MgKingSafetyPowerPer128 / 128d;
            var scale = -Config.MgKingSafetyScalePer128 / 128d;
            for (var index = 0; index < _mgKingSafety.Length; index++) _mgKingSafety[index] = GetNonLinearBonus(index, scale, kingSafetyPower, 0);
        }


        private void CalculatePieceMobility(int[] mgPieceMobility, int[] egPieceMobility, int mgMobilityScale, int egMobilityScale)
        {
            Debug.Assert(mgPieceMobility.Length == egPieceMobility.Length);
            var maxMoves = mgPieceMobility.Length - 1;
            var pieceMobilityPower = Config.PieceMobilityPowerPer128 / 128d;
            for (var moves = 0; moves <= maxMoves; moves++)
            {
                var fractionOfMaxMoves = (double) moves / maxMoves;
                mgPieceMobility[moves] = GetNonLinearBonus(fractionOfMaxMoves, mgMobilityScale, pieceMobilityPower, -mgMobilityScale / 2);
                egPieceMobility[moves] = GetNonLinearBonus(fractionOfMaxMoves, egMobilityScale, pieceMobilityPower, -egMobilityScale / 2);
            }
            // Adjust constant so piece mobility bonus for average number of moves is zero.
            var averageMoves = maxMoves / 2;
            var averageMgBonus = mgPieceMobility[averageMoves];
            var averageEgBonus = egPieceMobility[averageMoves];
            for (var moves = 0; moves <= maxMoves; moves++)
            {
                mgPieceMobility[moves] -= averageMgBonus;
                egPieceMobility[moves] -= averageEgBonus;
            }
        }


        public void ConfigureLimitedStrength(int elo)
        {
            // Reset to full strength, then limit positional understanding.
            ConfigureFullStrength();
            Config.LimitedStrength = true;
            if (elo < _beginnerElo)
            {
                // Undervalue rook and overvalue queen.
                Config.MgRookMaterial = _defaultConfig.MgRookMaterial - 200;
                Config.EgRookMaterial = _defaultConfig.EgRookMaterial - 200;
                Config.MgQueenMaterial = _defaultConfig.MgQueenMaterial + 300;
                Config.EgQueenMaterial = _defaultConfig.EgQueenMaterial + 300;
                // Value knight and bishop equally.
                Config.MgBishopMaterial = Config.MgKnightMaterial;
                Config.EgBishopMaterial = Config.EgKnightMaterial;
            }
            if (elo < _noviceElo)
            {
                // Misplace pieces.
                Config.LsPieceLocationPer128 = GetLinearlyInterpolatedValue(0, 128, elo, _beginnerElo, _noviceElo);
            }
            if (elo < _socialElo)
            {
                // Misjudge danger of passed pawns.
                Config.LsPassedPawnsPer128 = GetLinearlyInterpolatedValue(0, 128, elo, _noviceElo, _socialElo);
            }
            if (elo < _strongSocialElo)
            {
                // Oblivious to attacking potential of mobile pieces.
                Config.LsPieceMobilityPer128 = GetLinearlyInterpolatedValue(0, 128, elo, _socialElo, _strongSocialElo);
            }
            if (elo < _clubElo)
            {
                // Inattentive to defense of king.
                Config.LsKingSafetyPer128 = GetLinearlyInterpolatedValue(0, 128, elo, _strongSocialElo, _clubElo);
            }
            if (elo < _strongClubElo)
            {
                // Inexpert use of minor pieces.
                Config.LsMinorPiecesPer128 = GetLinearlyInterpolatedValue(0, 128, elo, _clubElo, _strongClubElo);
            }
            if (_debug())
            {
                _writeMessageLine($"info string {nameof(PawnMaterial)} = {PawnMaterial}");
                _writeMessageLine($"info string {nameof(Config.MgKnightMaterial)} = {Config.MgKnightMaterial}");
                _writeMessageLine($"info string {nameof(Config.EgKnightMaterial)} = {Config.EgKnightMaterial}");
                _writeMessageLine($"info string {nameof(Config.MgBishopMaterial)} = {Config.MgBishopMaterial}");
                _writeMessageLine($"info string {nameof(Config.EgBishopMaterial)} = {Config.EgBishopMaterial}");
                _writeMessageLine($"info string {nameof(Config.MgRookMaterial)} = {Config.MgRookMaterial}");
                _writeMessageLine($"info string {nameof(Config.EgRookMaterial)} = {Config.EgRookMaterial}");
                _writeMessageLine($"info string {nameof(Config.MgQueenMaterial)} = {Config.MgQueenMaterial}");
                _writeMessageLine($"info string {nameof(Config.EgQueenMaterial)} = {Config.EgQueenMaterial}");
                _writeMessageLine($"info string {nameof(Config.LsPieceLocationPer128)} = {Config.LsPieceLocationPer128}");
                _writeMessageLine($"info string {nameof(Config.LsPassedPawnsPer128)} = {Config.LsPassedPawnsPer128}");
                _writeMessageLine($"info string {nameof(Config.LsPieceMobilityPer128)} = {Config.LsPieceMobilityPer128}");
                _writeMessageLine($"info string {nameof(Config.LsKingSafetyPer128)} = {Config.LsKingSafetyPer128}");
                _writeMessageLine($"info string {nameof(Config.LsMinorPiecesPer128)} = {Config.LsMinorPiecesPer128}");
            }
        }


        private static int GetLinearlyInterpolatedValue(int minValue, int maxValue, int correlatedValue, int minCorrelatedValue, int maxCorrelatedValue)
        {
            var correlatedRange = maxCorrelatedValue - minCorrelatedValue;
            var fraction = (double) (Math.Max(correlatedValue, minCorrelatedValue) - minCorrelatedValue) / correlatedRange;
            var valueRange = maxValue - minValue;
            return (int) ((fraction * valueRange) + minValue);
        }


        public void ConfigureFullStrength() => Config.Set(_defaultConfig);


        public (bool TerminalDraw, bool RepeatPosition) IsTerminalDraw(Position position)
        {
            // Only return true if position is drawn and no sequence of moves can make game winnable.
            if (_isRepeatPosition(DrawMoves)) return (true, true); // Draw by repetition of position.
            if (position.PlySinceCaptureOrPawnMove >= StaticScore.MaxPlyWithoutCaptureOrPawnMove) return (true, false); // Draw by 50 moves (100 ply) without a capture or pawn move.
            // Determine if insufficient material remains for checkmate.
            if (Bitwise.CountSetBits(position.WhitePawns | position.BlackPawns) == 0)
            {
                // Neither side has any pawns.
                if (Bitwise.CountSetBits(position.WhiteRooks | position.WhiteQueens | position.BlackRooks | position.BlackQueens) == 0)
                {
                    // Neither side has any major pieces.
                    if ((Bitwise.CountSetBits(position.WhiteKnights | position.WhiteBishops) <= 1) && (Bitwise.CountSetBits(position.BlackKnights | position.BlackBishops) <= 1))
                    {
                        // Each side has one or zero minor pieces.  Draw by insufficient material.
                        return (true, false);
                    }
                }
            }
            return (false, false);
        }


        public (int StaticScore, bool DrawnEndgame) GetStaticScore(Position position)
        {
            // TODO: Handicap knowledge of checkmates and endgames when in limited strength mode.
            Debug.Assert(!position.KingInCheck);
            _stats.Evaluations++;
            _staticScore.Reset();
            if (EvaluateSimpleEndgame(position))
            {
                // Position is a simple endgame.
                if (_staticScore.EgScalePer128 == 0) return (0, true); // Drawn Endgame
                return position.WhiteMove
                    ? (_staticScore.WhiteEg - _staticScore.BlackEg, false)
                    : (_staticScore.BlackEg - _staticScore.WhiteEg, false);
            }
            // Position is not a simple endgame.
            _staticScore.PlySinceCaptureOrPawnMove = position.PlySinceCaptureOrPawnMove;
            EvaluateMaterial(position);
            EvaluatePieceLocation(position);
            EvaluatePawns(position);
            EvaluatePieceMobilityKingSafety(position);
            EvaluateMinorPieces(position);
            if (Config.LimitedStrength) LimitStrength();
            DetermineEndgameScale(position); // Scale down scores for difficult to win endgames.
            if (_staticScore.EgScalePer128 == 0) return (0, true); // Drawn Endgame
            var phase = DetermineGamePhase(position);
            return position.WhiteMove
                ? (_staticScore.GetTotalScore(phase), false)
                : (-_staticScore.GetTotalScore(phase), false);
        }


        private bool EvaluateSimpleEndgame(Position position)
        {
            var whitePawns = Bitwise.CountSetBits(position.WhitePawns);
            var blackPawns = Bitwise.CountSetBits(position.BlackPawns);
            if ((whitePawns == 0) && (blackPawns == 0) && IsPawnlessDraw(position))
            {
                // Game is pawnless draw.
                _staticScore.EgScalePer128 = 0;
                return true;
            }
            var whiteKnights = Bitwise.CountSetBits(position.WhiteKnights);
            var whiteBishops = Bitwise.CountSetBits(position.WhiteBishops);
            var whiteMinorPieces = whiteKnights + whiteBishops;
            var whiteMajorPieces = Bitwise.CountSetBits(position.WhiteRooks | position.WhiteQueens);
            var whitePawnsAndPieces = whitePawns + whiteMinorPieces + whiteMajorPieces;
            var blackKnights = Bitwise.CountSetBits(position.BlackKnights);
            var blackBishops = Bitwise.CountSetBits(position.BlackBishops);
            var blackMinorPieces = blackKnights + blackBishops;
            var blackMajorPieces = Bitwise.CountSetBits(position.BlackRooks | position.BlackQueens);
            var blackPawnsAndPieces = blackPawns + blackMinorPieces + blackMajorPieces;
            if ((whitePawnsAndPieces > 0) && (blackPawnsAndPieces > 0)) return false; // Position is not a simple endgame.
            var loneWhitePawn = (whitePawns == 1) && (whitePawnsAndPieces == 1);
            var loneBlackPawn = (blackPawns == 1) && (blackPawnsAndPieces == 1);
            var whiteKingSquare = Bitwise.FirstSetSquare(position.WhiteKing);
            var blackKingSquare = Bitwise.FirstSetSquare(position.BlackKing);
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (whitePawnsAndPieces)
            {
                // Case 0 = Lone White King
                case 0 when loneBlackPawn:
                    EvaluateKingVersusPawn(position, Color.Black);
                    return true;
                // ReSharper disable once SwitchStatementMissingSomeCases
                case 0:
                    switch (blackPawns)
                    {
                        case 0 when (blackBishops == 1) && (blackKnights == 1) && (blackMajorPieces == 0):
                            // K vrs KBN
                            var lightSquareBishop = Board.LightSquares[(int)Bitwise.FirstSetSquare(position.BlackBishops)];
                            var distanceToCorrectColorCorner = lightSquareBishop
                                ? Board.DistanceToNearestLightCorner[(int)whiteKingSquare]
                                : Board.DistanceToNearestDarkCorner[(int)whiteKingSquare];
                            _staticScore.BlackEgSimple = Config.SimpleEndgame - distanceToCorrectColorCorner - Board.SquareDistances[(int)whiteKingSquare][(int)blackKingSquare];
                            return true;
                        case 0 when (blackMajorPieces == 1) && (blackMinorPieces == 0):
                            // K vrs KQ or KR
                            _staticScore.BlackEgSimple = Config.SimpleEndgame - Board.DistanceToNearestCorner[(int)whiteKingSquare] - Board.SquareDistances[(int)whiteKingSquare][(int)blackKingSquare];
                            return true;
                    }
                    break;
            }
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (blackPawnsAndPieces)
            {
                // Case 0 = Lone Black King
                case 0 when loneWhitePawn:
                    EvaluateKingVersusPawn(position, Color.White);
                    return true;
                // ReSharper disable once SwitchStatementMissingSomeCases
                case 0:
                    switch (whitePawns)
                    {
                        case 0 when (whiteBishops == 1) && (whiteKnights == 1) && (whiteMajorPieces == 0):
                            // K vrs KBN
                            var lightSquareBishop = Board.LightSquares[(int)Bitwise.FirstSetSquare(position.WhiteBishops)];
                            var distanceToCorrectColorCorner = lightSquareBishop
                                ? Board.DistanceToNearestLightCorner[(int)blackKingSquare]
                                : Board.DistanceToNearestDarkCorner[(int)blackKingSquare];
                            _staticScore.WhiteEgSimple = Config.SimpleEndgame - distanceToCorrectColorCorner - Board.SquareDistances[(int)whiteKingSquare][(int)blackKingSquare];
                            return true;
                        case 0 when (whiteMajorPieces == 1) && (whiteMinorPieces == 0):
                            // K vrs KQ or KR
                            _staticScore.WhiteEgSimple = Config.SimpleEndgame - Board.DistanceToNearestCorner[(int)blackKingSquare] - Board.SquareDistances[(int)whiteKingSquare][(int)blackKingSquare];
                            return true;
                    }
                    break;
            }
            // Use regular evaluation.
            return false;
        }


        private static bool IsPawnlessDraw(Position position)
        {
            var whiteKnights = Bitwise.CountSetBits(position.WhiteKnights);
            var whiteBishops = Bitwise.CountSetBits(position.WhiteBishops);
            var whiteRooks = Bitwise.CountSetBits(position.WhiteRooks);
            var whiteQueens = Bitwise.CountSetBits(position.WhiteQueens);
            var whiteMinorPieces = whiteKnights + whiteBishops;
            var whiteMajorPieces = whiteRooks + whiteQueens;
            var blackKnights = Bitwise.CountSetBits(position.BlackKnights);
            var blackBishops = Bitwise.CountSetBits(position.BlackBishops);
            var blackRooks = Bitwise.CountSetBits(position.BlackRooks);
            var blackQueens = Bitwise.CountSetBits(position.BlackQueens);
            var blackMinorPieces = blackKnights + blackBishops;
            var blackMajorPieces = blackRooks + blackQueens;
            var totalMajorPieces = whiteMajorPieces + blackMajorPieces;
            switch (totalMajorPieces)
            {
                case 0:
                    if ((whiteKnights == 2) && (whiteMinorPieces == 2) && (blackMinorPieces <= 1)) return true; // 2N vrs <= 1 Minor
                    if ((blackKnights == 2) && (blackMinorPieces == 2) && (whiteMinorPieces <= 1)) return true; // 2N vrs <= 1 Minor
                    break;
                case 1:
                    if ((whiteQueens == 1) && (whiteMinorPieces == 0))
                    {
                        if ((blackBishops == 2) && (blackMinorPieces == 2)) return true; // Q vrs 2B
                        if ((blackKnights == 2) && (blackMinorPieces == 2)) return true; // Q vrs 2N
                    }
                    if ((blackQueens == 1) && (blackMinorPieces == 0))
                    {
                        if ((whiteBishops == 2) && (whiteMinorPieces == 2)) return true; // Q vrs 2B
                        if ((whiteKnights == 2) && (whiteMinorPieces == 2)) return true; // Q vrs 2N
                    }
                    // Considering R vrs <= 2 Minors a draw increases evaluation error and causes engine to play weaker.
                    //if ((whiteRooks == 1) && (whiteMinorPieces == 0) && (blackMinorPieces <= 2)) return true; // R vrs <= 2 Minors
                    //if ((blackRooks == 1) && (blackMinorPieces == 0) && (whiteMinorPieces <= 2)) return true; // R vrs <= 2 Minors
                    break;
                case 2:
                    if ((whiteQueens == 1) && (whiteMinorPieces == 0))
                    {
                        if ((blackQueens == 1) && (blackMinorPieces == 0)) return true; // Q vrs Q
                        if ((blackRooks == 1) && (blackMinorPieces == 1)) return true; // Q vrs R + Minor
                    }
                    if ((blackQueens == 1) && (blackMinorPieces == 0) && (whiteRooks == 1) && (whiteMinorPieces == 1)) return true; // Q vrs R + Minor
                    if ((whiteRooks == 1) && (whiteMinorPieces == 0) && (blackRooks == 1) && (blackMinorPieces <= 1)) return true; // R vrs R + <= 1 Minor
                    if ((blackRooks == 1) && (blackMinorPieces == 0) && (whiteRooks == 1) && (whiteMinorPieces <= 1)) return true; // R vrs R + <= 1 Minor
                    break;
                case 3:
                    if ((whiteQueens == 1) && (whiteMinorPieces == 0) && (blackRooks == 2) && (blackMinorPieces == 0)) return true; // Q vrs 2R
                    if ((blackQueens == 1) && (blackMinorPieces == 0) && (whiteRooks == 2) && (whiteMinorPieces == 0)) return true; // Q vrs 2R
                    if ((whiteRooks == 2) & (whiteMinorPieces == 0) && (blackRooks == 1) && (blackMinorPieces == 1)) return true; // 2R vrs R + Minor
                    if ((blackRooks == 2) & (blackMinorPieces == 0) && (whiteRooks == 1) && (whiteMinorPieces == 1)) return true; // 2R vrs R + Minor
                    break;
                case 4:
                    if ((whiteRooks == 2) && (whiteMinorPieces == 0) && (blackRooks == 2) && (blackMinorPieces == 0)) return true; // 2R vrs 2R
                    break;
            }
            return false;
        }


        private void EvaluateKingVersusPawn(Position position, Color lonePawnColor)
        {
            var winningKingIndex = ((int) lonePawnColor + 1) * (int) Piece.WhiteKing;
            var winningKingSquare = Bitwise.FirstSetSquare(position.PieceBitboards[winningKingIndex]);
            var winningKingRank = Board.Ranks[(int)lonePawnColor][(int)winningKingSquare];
            var winningKingFile = Board.Files[(int)winningKingSquare];
            var defendingKingColor = 1 - lonePawnColor;
            var defendingKingIndex = ((int)defendingKingColor + 1) * (int)Piece.WhiteKing;
            var defendingKingSquare = Bitwise.FirstSetSquare(position.PieceBitboards[defendingKingIndex]);
            var defendingKingRank = Board.Ranks[(int)defendingKingColor][(int)defendingKingSquare];
            var defendingKingFile = Board.Files[(int)defendingKingSquare];
            var pawnIndex = ((int) lonePawnColor * (int) Piece.WhiteKing) + (int) Piece.WhitePawn;
            var pawnSquare = Bitwise.FirstSetSquare(position.PieceBitboards[pawnIndex]);
            var pawnRank = Board.Ranks[(int)lonePawnColor][(int)pawnSquare];
            var pawnFile = Board.Files[(int)pawnSquare];
            if ((pawnFile == 0) || (pawnFile == 7))
            {
                // Pawn is on rook file.
                if ((defendingKingFile == pawnFile) && (defendingKingRank > pawnRank))
                {
                    // Defending king is in front of pawn and on same file.
                    // Game is drawn.
                    _staticScore.EgScalePer128 = 0;
                    return;
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
                    if (lonePawnColor == Color.White) _staticScore.WhiteEgSimple = Config.SimpleEndgame + pawnRank;
                    else _staticScore.BlackEgSimple = Config.SimpleEndgame + pawnRank;
                    return;
                }
            }
            // Pawn does not promote.
            // Game is drawn.
            _staticScore.EgScalePer128 = 0;
        }

        
        private void EvaluateMaterial(Position position)
        {
            _staticScore.WhitePawnMaterial = Bitwise.CountSetBits(position.WhitePawns) * PawnMaterial;
            _staticScore.WhiteMgPieceMaterial = Bitwise.CountSetBits(position.WhiteKnights) * Config.MgKnightMaterial + Bitwise.CountSetBits(position.WhiteBishops) * Config.MgBishopMaterial +
                                                Bitwise.CountSetBits(position.WhiteRooks) * Config.MgRookMaterial + Bitwise.CountSetBits(position.WhiteQueens) * Config.MgQueenMaterial;
            _staticScore.WhiteEgPieceMaterial = Bitwise.CountSetBits(position.WhiteKnights) * Config.EgKnightMaterial + Bitwise.CountSetBits(position.WhiteBishops) * Config.EgBishopMaterial +
                                                Bitwise.CountSetBits(position.WhiteRooks) * Config.EgRookMaterial + Bitwise.CountSetBits(position.WhiteQueens) * Config.EgQueenMaterial;

            _staticScore.BlackPawnMaterial = Bitwise.CountSetBits(position.BlackPawns) * PawnMaterial;
            _staticScore.BlackMgPieceMaterial = Bitwise.CountSetBits(position.BlackKnights) * Config.MgKnightMaterial + Bitwise.CountSetBits(position.BlackBishops) * Config.MgBishopMaterial +
                                                Bitwise.CountSetBits(position.BlackRooks) * Config.MgRookMaterial + Bitwise.CountSetBits(position.BlackQueens) * Config.MgQueenMaterial;
            _staticScore.BlackEgPieceMaterial = Bitwise.CountSetBits(position.BlackKnights) * Config.EgKnightMaterial + Bitwise.CountSetBits(position.BlackBishops) * Config.EgBishopMaterial +
                                                Bitwise.CountSetBits(position.BlackRooks) * Config.EgRookMaterial + Bitwise.CountSetBits(position.BlackQueens) * Config.EgQueenMaterial;
        }


        public int GetMaterialScore(Position position, Piece piece)
        {
            int mgMaterial;
            int egMaterial;
            // Sequence cases in order of integer value to improve performance of switch statement.
            switch (piece)
            {
                case Piece.None:
                    mgMaterial = 0;
                    egMaterial = 0;
                    break;
                case Piece.WhitePawn:
                    return PawnMaterial;
                case Piece.WhiteKnight:
                    mgMaterial = Config.MgKnightMaterial;
                    egMaterial = Config.EgKnightMaterial;
                    break;
                case Piece.WhiteBishop:
                    mgMaterial = Config.MgBishopMaterial;
                    egMaterial = Config.EgBishopMaterial;
                    break;
                case Piece.WhiteRook:
                    mgMaterial = Config.MgRookMaterial;
                    egMaterial = Config.EgRookMaterial;
                    break;
                case Piece.WhiteQueen:
                    mgMaterial = Config.MgQueenMaterial;
                    egMaterial = Config.EgQueenMaterial;
                    break;
                case Piece.WhiteKing:
                    mgMaterial = 0;
                    egMaterial = 0;
                    break;
                case Piece.BlackPawn:
                    return PawnMaterial;
                case Piece.BlackKnight:
                    mgMaterial = Config.MgKnightMaterial;
                    egMaterial = Config.EgKnightMaterial;
                    break;
                case Piece.BlackBishop:
                    mgMaterial = Config.MgBishopMaterial;
                    egMaterial = Config.EgBishopMaterial;
                    break;
                case Piece.BlackRook:
                    mgMaterial = Config.MgRookMaterial;
                    egMaterial = Config.EgRookMaterial;
                    break;
                case Piece.BlackQueen:
                    mgMaterial = Config.MgQueenMaterial;
                    egMaterial = Config.EgQueenMaterial;
                    break;
                case Piece.BlackKing:
                    mgMaterial = 0;
                    egMaterial = 0;
                    break;
                default:
                    throw new ArgumentException($"{piece} piece not supported.");
            }
            var phase = DetermineGamePhase(position);
            return StaticScore.GetTaperedScore(mgMaterial, egMaterial, phase);
        }


        public static (int StaticScore, bool DrawnEndgame) GetExchangeMaterialScore(Position position)
        {
            var whiteScore = Bitwise.CountSetBits(position.WhitePawns) * PawnMaterial +
                             Bitwise.CountSetBits(position.WhiteKnights) * _knightExchangeMaterial + Bitwise.CountSetBits(position.WhiteBishops) * _bishopExchangeMaterial +
                             Bitwise.CountSetBits(position.WhiteRooks) * _rookExchangeMaterial + Bitwise.CountSetBits(position.WhiteQueens) * _queenExchangeMaterial;
            var blackScore = Bitwise.CountSetBits(position.BlackPawns) * PawnMaterial +
                             Bitwise.CountSetBits(position.BlackKnights) * _knightExchangeMaterial + Bitwise.CountSetBits(position.BlackBishops) * _bishopExchangeMaterial +
                             Bitwise.CountSetBits(position.BlackRooks) * _rookExchangeMaterial + Bitwise.CountSetBits(position.BlackQueens) * _queenExchangeMaterial;
            return position.WhiteMove
                ? (whiteScore - blackScore, false)
                : (blackScore - whiteScore, false);
        }


        private void EvaluatePieceLocation(Position position)
        {
            // Pawns
            Square square;
            Square blackSquare;
            var pieces = position.WhitePawns;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgPawnLocations[(int)square];
                _staticScore.WhiteEgPieceLocation += _egPawnLocations[(int)square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = position.BlackPawns;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgPawnLocations[(int)blackSquare];
                _staticScore.BlackEgPieceLocation += _egPawnLocations[(int)blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Knights
            pieces = position.WhiteKnights;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgKnightLocations[(int)square];
                _staticScore.WhiteEgPieceLocation += _egKnightLocations[(int)square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = position.BlackKnights;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgKnightLocations[(int)blackSquare];
                _staticScore.BlackEgPieceLocation += _egKnightLocations[(int)blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Bishops
            pieces = position.WhiteBishops;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgBishopLocations[(int)square];
                _staticScore.WhiteEgPieceLocation += _egBishopLocations[(int)square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = position.BlackBishops;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgBishopLocations[(int)blackSquare];
                _staticScore.BlackEgPieceLocation += _egBishopLocations[(int)blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Rooks
            pieces = position.WhiteRooks;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgRookLocations[(int)square];
                _staticScore.WhiteEgPieceLocation += _egRookLocations[(int)square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = position.BlackRooks;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgRookLocations[(int)blackSquare];
                _staticScore.BlackEgPieceLocation += _egRookLocations[(int)blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Queens
            pieces = position.WhiteQueens;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                _staticScore.WhiteMgPieceLocation += _mgQueenLocations[(int)square];
                _staticScore.WhiteEgPieceLocation += _egQueenLocations[(int)square];
                Bitwise.ClearBit(ref pieces, square);
            }
            pieces = position.BlackQueens;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                blackSquare = Board.GetBlackSquare(square);
                _staticScore.BlackMgPieceLocation += _mgQueenLocations[(int)blackSquare];
                _staticScore.BlackEgPieceLocation += _egQueenLocations[(int)blackSquare];
                Bitwise.ClearBit(ref pieces, square);
            }
            // Kings
            square = Bitwise.FirstSetSquare(position.WhiteKing);
            _staticScore.WhiteMgPieceLocation += _mgKingLocations[(int)square];
            _staticScore.WhiteEgPieceLocation += _egKingLocations[(int)square];
            blackSquare = Board.GetBlackSquare(Bitwise.FirstSetSquare(position.BlackKing));
            _staticScore.BlackMgPieceLocation += _mgKingLocations[(int)blackSquare];
            _staticScore.BlackEgPieceLocation += _egKingLocations[(int)blackSquare];
        }


        private void EvaluatePawns(Position position)
        {
            // White pawns
            var pawns = position.WhitePawns;
            var kingSquare = Bitwise.FirstSetSquare(position.WhiteKing);
            var enemyKingSquare = Bitwise.FirstSetSquare(position.BlackKing);
            Square pawnSquare;
            int rank;
            while ((pawnSquare = Bitwise.FirstSetSquare(pawns)) != Square.Illegal)
            {
                if (IsPassedPawn(position, pawnSquare, true))
                {
                    _staticScore.WhitePassedPawnCount++;
                    rank = Board.Ranks[(int)Color.White][(int)pawnSquare];
                    _staticScore.WhiteEgKingEscortedPassedPawns += (Board.SquareDistances[(int)pawnSquare][(int)enemyKingSquare] - Board.SquareDistances[(int)pawnSquare][(int)kingSquare]) * Config.EgKingEscortedPassedPawn;
                    if (IsFreePawn(position, pawnSquare, true))
                    {
                        // Pawn can advance safely.
                        if (IsUnstoppablePawn(position, pawnSquare, enemyKingSquare, true)) _staticScore.WhiteUnstoppablePassedPawns += Config.UnstoppablePassedPawn; // Pawn is unstoppable.
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
            pawns = position.BlackPawns;
            kingSquare = Bitwise.FirstSetSquare(position.BlackKing);
            enemyKingSquare = Bitwise.FirstSetSquare(position.WhiteKing);
            while ((pawnSquare = Bitwise.FirstSetSquare(pawns)) != Square.Illegal)
            {
                if (IsPassedPawn(position, pawnSquare, false))
                {
                    _staticScore.BlackPassedPawnCount++;
                    rank = Board.Ranks[(int)Color.Black][(int)pawnSquare];
                    _staticScore.BlackEgKingEscortedPassedPawns += (Board.SquareDistances[(int)pawnSquare][(int)enemyKingSquare] - Board.SquareDistances[(int)pawnSquare][(int)kingSquare]) * Config.EgKingEscortedPassedPawn;
                    if (IsFreePawn(position, pawnSquare, false))
                    {
                        // Pawn can advance safely.
                        if (IsUnstoppablePawn(position, pawnSquare, enemyKingSquare, false)) _staticScore.BlackUnstoppablePassedPawns += Config.UnstoppablePassedPawn; // Pawn is unstoppable.
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
        private static bool IsPassedPawn(Position position, Square square, bool white)
        {
            Debug.Assert(position.GetPiece(square) == (white ? Piece.WhitePawn : Piece.BlackPawn));
            return white
                ? (Board.PassedPawnMasks[(int)Color.White][(int)square] & position.BlackPawns) == 0
                : (Board.PassedPawnMasks[(int)Color.Black][(int)square] & position.WhitePawns) == 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFreePawn(Position position, Square square, bool white)
        {
            Debug.Assert(position.GetPiece(square) == (white ? Piece.WhitePawn : Piece.BlackPawn));
            // Determine if pawn can advance.
            return white
                ? (Board.FreePawnMasks[(int)Color.White][(int)square] & position.Occupancy) == 0
                : (Board.FreePawnMasks[(int)Color.Black][(int)square] & position.Occupancy) == 0;
        }


        private static bool IsUnstoppablePawn(Position position, Square pawnSquare, Square enemyKingSquare, bool white)
        {
            // Pawn is free to advance to promotion square.
            var file = Board.Files[(int)pawnSquare];
            Square promotionSquare;
            int enemyPieces;
            if (white)
            {
                // White Pawn
                promotionSquare = Board.GetSquare(file, 7);
                enemyPieces = Bitwise.CountSetBits(position.BlackKnights | position.BlackBishops | position.BlackRooks | position.BlackQueens);
            }
            else
            {
                // Black Pawn
                promotionSquare = Board.GetSquare(file, 0);
                enemyPieces = Bitwise.CountSetBits(position.WhiteKnights | position.WhiteBishops | position.WhiteRooks | position.WhiteQueens);
            }
            if (enemyPieces == 0)
            {
                // Enemy has no minor or major pieces.
                var pawnDistanceToPromotionSquare = Board.SquareDistances[(int)pawnSquare][(int)promotionSquare];
                var kingDistanceToPromotionSquare = Board.SquareDistances[(int)enemyKingSquare][(int)promotionSquare];
                if (white != position.WhiteMove) kingDistanceToPromotionSquare--; // Enemy king can move one square closer to pawn.
                return kingDistanceToPromotionSquare > pawnDistanceToPromotionSquare; // Enemy king cannot stop pawn from promoting.
            }
            return false;
        }



        // TODO: Include stacked attacks on same square via x-rays.  For example, a rook behind a queen.
        private void EvaluatePieceMobilityKingSafety(Position position)
        {
            var whiteKingSquare = Bitwise.FirstSetSquare(position.WhiteKing);
            var whiteKingInnerRing = Board.InnerRingMasks[(int)whiteKingSquare];
            var whiteKingOuterRing = Board.OuterRingMasks[(int)whiteKingSquare];
            var blackKingSquare = Bitwise.FirstSetSquare(position.BlackKing);
            var blackKingInnerRing = Board.InnerRingMasks[(int)blackKingSquare];
            var blackKingOuterRing = Board.OuterRingMasks[(int)blackKingSquare];
            Square square;
            ulong pieceDestinations;
            int mgPieceMobilityScore;
            int egPieceMobilityScore;
            int kingSafetyIndexIncrementPer8;
            var whiteMgKingSafetyIndexPer8 = 0;
            var blackMgKingSafetyIndexPer8 = 0;
            // White Knights
            var pieces = position.WhiteKnights;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetKnightDestinations(position, square, true);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgKnightMobility, _egKnightMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, blackKingOuterRing, blackKingInnerRing, Config.MgKingSafetyMinorAttackOuterRingPer8, Config.MgKingSafetyMinorAttackInnerRingPer8);
                blackMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Black Knights
            pieces = position.BlackKnights;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetKnightDestinations(position, square, false);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgKnightMobility, _egKnightMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, whiteKingOuterRing, whiteKingInnerRing, Config.MgKingSafetyMinorAttackOuterRingPer8, Config.MgKingSafetyMinorAttackInnerRingPer8);
                whiteMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // White Bishops
            pieces = position.WhiteBishops;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetBishopDestinations(position, square, true);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgBishopMobility, _egBishopMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, blackKingOuterRing, blackKingInnerRing, Config.MgKingSafetyMinorAttackOuterRingPer8, Config.MgKingSafetyMinorAttackInnerRingPer8);
                blackMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Black Bishops
            pieces = position.BlackBishops;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetBishopDestinations(position, square, false);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgBishopMobility, _egBishopMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, whiteKingOuterRing, whiteKingInnerRing, Config.MgKingSafetyMinorAttackOuterRingPer8, Config.MgKingSafetyMinorAttackInnerRingPer8);
                whiteMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // White Rooks
            pieces = position.WhiteRooks;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetRookDestinations(position, square, true);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgRookMobility, _egRookMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, blackKingOuterRing, blackKingInnerRing, Config.MgKingSafetyRookAttackOuterRingPer8, Config.MgKingSafetyRookAttackInnerRingPer8);
                blackMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Black Rooks
            pieces = position.BlackRooks;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetRookDestinations(position, square, false);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgRookMobility, _egRookMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, whiteKingOuterRing, whiteKingInnerRing, Config.MgKingSafetyRookAttackOuterRingPer8, Config.MgKingSafetyRookAttackInnerRingPer8);
                whiteMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // White Queens
            pieces = position.WhiteQueens;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetQueenDestinations(position, square, true);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgQueenMobility, _egQueenMobility);
                _staticScore.WhiteMgPieceMobility += mgPieceMobilityScore;
                _staticScore.WhiteEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, blackKingOuterRing, blackKingInnerRing, Config.MgKingSafetyQueenAttackOuterRingPer8, Config.MgKingSafetyQueenAttackInnerRingPer8);
                blackMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Black Queens
            pieces = position.BlackQueens;
            while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
            {
                pieceDestinations = Board.GetQueenDestinations(position, square, false);
                (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgQueenMobility, _egQueenMobility);
                _staticScore.BlackMgPieceMobility += mgPieceMobilityScore;
                _staticScore.BlackEgPieceMobility += egPieceMobilityScore;
                kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, whiteKingOuterRing, whiteKingInnerRing, Config.MgKingSafetyQueenAttackOuterRingPer8, Config.MgKingSafetyQueenAttackInnerRingPer8);
                whiteMgKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                Bitwise.ClearBit(ref pieces, square);
            }
            // Evaluate white king near semi-open file.
            var kingFile = Board.Files[(int)whiteKingSquare];
            var leftFileMask = kingFile > 0 ? Board.FileMasks[kingFile - 1] : 0;
            var kingFileMask = Board.FileMasks[kingFile];
            var rightFileMask = kingFile < 7 ? Board.FileMasks[kingFile + 1] : 0;
            var leftFileSemiOpen = (leftFileMask > 0) && ((position.WhitePawns & leftFileMask) == 0) ? 1 : 0;
            var kingFileSemiOpen = (position.WhitePawns & kingFileMask) == 0 ? 1 : 0;
            var rightFileSemiOpen = (rightFileMask > 0) && ((position.WhitePawns & rightFileMask) == 0) ? 1 : 0;
            var semiOpenFiles = leftFileSemiOpen + kingFileSemiOpen + rightFileSemiOpen;
            whiteMgKingSafetyIndexPer8 += semiOpenFiles * Config.MgKingSafetySemiOpenFilePer8;
            // Evaluate black king near semi-open file.
            kingFile = Board.Files[(int)blackKingSquare];
            rightFileMask = kingFile > 0 ? Board.FileMasks[kingFile - 1] : 0;
            kingFileMask = Board.FileMasks[kingFile];
            leftFileMask = kingFile < 7 ? Board.FileMasks[kingFile + 1] : 0;
            leftFileSemiOpen = (leftFileMask > 0) && ((position.BlackPawns & leftFileMask) == 0) ? 1 : 0;
            kingFileSemiOpen = (position.BlackPawns & kingFileMask) == 0 ? 1 : 0;
            rightFileSemiOpen = (rightFileMask > 0) && ((position.BlackPawns & rightFileMask) == 0) ? 1 : 0;
            semiOpenFiles = leftFileSemiOpen + kingFileSemiOpen + rightFileSemiOpen;
            blackMgKingSafetyIndexPer8 += semiOpenFiles * Config.MgKingSafetySemiOpenFilePer8;
            // Evaluate white pawn shield.
            const int maxPawnsInShield = 3;
            var missingPawns = maxPawnsInShield - Bitwise.CountSetBits(position.WhitePawns & Board.WhitePawnShieldMasks[(int)whiteKingSquare]);
            whiteMgKingSafetyIndexPer8 += missingPawns * Config.MgKingSafetyPawnShieldPer8;
            // Evaluate black pawn shield.
            missingPawns = maxPawnsInShield - Bitwise.CountSetBits(position.BlackPawns & Board.BlackPawnShieldMasks[(int)blackKingSquare]);
            blackMgKingSafetyIndexPer8 += missingPawns * Config.MgKingSafetyPawnShieldPer8;
            // Lookup king safety score in array.
            var maxIndex = _mgKingSafety.Length - 1;
            _staticScore.WhiteMgKingSafety = _mgKingSafety[Math.Min(whiteMgKingSafetyIndexPer8 / 8, maxIndex)];
            _staticScore.BlackMgKingSafety = _mgKingSafety[Math.Min(blackMgKingSafetyIndexPer8 / 8, maxIndex)];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (int MiddlegameMobility, int EndgameMobility) GetPieceMobilityScore(ulong pieceDestinations, int[] mgPieceMobility, int[] egPieceMobility)
        {
            var moves = Bitwise.CountSetBits(pieceDestinations);
            var mgMoveIndex = Math.Min(moves, mgPieceMobility.Length - 1);
            var egMoveIndex = Math.Min(moves, egPieceMobility.Length - 1);
            return (mgPieceMobility[mgMoveIndex], egPieceMobility[egMoveIndex]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetKingSafetyIndexIncrement(ulong pieceDestinations, ulong kingOuterRing, ulong kingInnerRing, int outerRingAttackWeight, int innerRingAttackWeight)
        {
            var attackedOuterRingSquares = Bitwise.CountSetBits(pieceDestinations & kingOuterRing);
            var attackedInnerRingSquares = Bitwise.CountSetBits(pieceDestinations & kingInnerRing);
            return (attackedOuterRingSquares * outerRingAttackWeight) + (attackedInnerRingSquares * innerRingAttackWeight);
        }


        private void EvaluateMinorPieces(Position position)
        {
            // Bishop Pair
            var whiteBishops = Bitwise.CountSetBits(position.WhiteBishops);
            if (whiteBishops >= 2)
            {
                _staticScore.WhiteMgBishopPair += Config.MgBishopPair;
                _staticScore.WhiteEgBishopPair += Config.EgBishopPair;
            }
            var blackBishops = Bitwise.CountSetBits(position.BlackBishops);
            if (blackBishops >= 2)
            {
                _staticScore.BlackMgBishopPair += Config.MgBishopPair;
                _staticScore.BlackEgBishopPair += Config.EgBishopPair;
            }
        }


        private void LimitStrength()
        {
            // Limit understanding of piece location.
            _staticScore.WhiteMgPieceLocation = (_staticScore.WhiteMgPieceLocation * Config.LsPieceLocationPer128) / 128;
            _staticScore.WhiteEgPieceLocation = (_staticScore.WhiteEgPieceLocation * Config.LsPieceLocationPer128) / 128;
            _staticScore.BlackMgPieceLocation = (_staticScore.BlackMgPieceLocation * Config.LsPieceLocationPer128) / 128;
            _staticScore.BlackEgPieceLocation = (_staticScore.BlackEgPieceLocation * Config.LsPieceLocationPer128) / 128;
            // Limit understanding of passed pawns.
            _staticScore.WhiteMgPassedPawns = (_staticScore.WhiteMgPassedPawns * Config.LsPassedPawnsPer128) / 128;
            _staticScore.WhiteEgPassedPawns = (_staticScore.WhiteEgPassedPawns * Config.LsPassedPawnsPer128) / 128;
            _staticScore.WhiteEgFreePassedPawns = (_staticScore.WhiteEgFreePassedPawns * Config.LsPassedPawnsPer128) / 128;
            _staticScore.WhiteEgKingEscortedPassedPawns = (_staticScore.WhiteEgKingEscortedPassedPawns * Config.LsPassedPawnsPer128) / 128;
            _staticScore.WhiteUnstoppablePassedPawns = (_staticScore.WhiteUnstoppablePassedPawns * Config.LsPassedPawnsPer128) / 128;
            _staticScore.BlackMgPassedPawns = (_staticScore.BlackMgPassedPawns * Config.LsPassedPawnsPer128) / 128;
            _staticScore.BlackEgPassedPawns = (_staticScore.BlackEgPassedPawns * Config.LsPassedPawnsPer128) / 128;
            _staticScore.BlackEgFreePassedPawns = (_staticScore.BlackEgFreePassedPawns * Config.LsPassedPawnsPer128) / 128;
            _staticScore.BlackEgKingEscortedPassedPawns = (_staticScore.BlackEgKingEscortedPassedPawns * Config.LsPassedPawnsPer128) / 128;
            _staticScore.BlackUnstoppablePassedPawns = (_staticScore.BlackUnstoppablePassedPawns * Config.LsPassedPawnsPer128) / 128;
            // Limit understanding of piece mobility.
            _staticScore.WhiteMgPieceMobility = (_staticScore.WhiteMgPieceMobility * Config.LsPieceMobilityPer128) / 128;
            _staticScore.WhiteEgPieceMobility = (_staticScore.WhiteEgPieceMobility * Config.LsPieceMobilityPer128) / 128;
            _staticScore.BlackMgPieceMobility = (_staticScore.BlackMgPieceMobility * Config.LsPieceMobilityPer128) / 128;
            _staticScore.BlackEgPieceMobility = (_staticScore.BlackEgPieceMobility * Config.LsPieceMobilityPer128) / 128;
            // Limit understanding of king safety.
            _staticScore.WhiteMgKingSafety = (_staticScore.WhiteMgKingSafety * Config.LsKingSafetyPer128) / 128;
            _staticScore.BlackMgKingSafety = (_staticScore.BlackMgKingSafety * Config.LsKingSafetyPer128) / 128;
            // Limit understanding of minor pieces.
            _staticScore.WhiteMgBishopPair = (_staticScore.WhiteMgBishopPair * Config.LsMinorPiecesPer128) / 128;
            _staticScore.WhiteEgBishopPair = (_staticScore.WhiteEgBishopPair * Config.LsMinorPiecesPer128) / 128;
            _staticScore.BlackMgBishopPair = (_staticScore.BlackMgBishopPair * Config.LsMinorPiecesPer128) / 128;
            _staticScore.BlackEgBishopPair = (_staticScore.BlackEgBishopPair * Config.LsMinorPiecesPer128) / 128;
        }


        private void DetermineEndgameScale(Position position)
        {
            // Use middlegame material values because they are constant (endgame material values are tuned).
            // Determine which color has an advantage.
            int winningPawnCount;
            int winningPassedPawns;
            int winningPieceMaterial;
            int losingPieceMaterial;
            if (_staticScore.WhiteEg >= _staticScore.BlackEg)
            {
                // White is winning the endgame.
                winningPawnCount = Bitwise.CountSetBits(position.WhitePawns);
                winningPassedPawns = _staticScore.WhitePassedPawnCount;
                winningPieceMaterial = _staticScore.WhiteMgPieceMaterial;
                losingPieceMaterial = _staticScore.BlackMgPieceMaterial;
            }
            else
            {
                // Black is winning the endgame.
                winningPawnCount = Bitwise.CountSetBits(position.BlackPawns);
                winningPassedPawns = _staticScore.BlackPassedPawnCount;
                winningPieceMaterial = _staticScore.BlackMgPieceMaterial;
                losingPieceMaterial = _staticScore.WhiteMgPieceMaterial;
            }
            var oppositeColoredBishops = (Bitwise.CountSetBits(position.WhiteBishops) == 1) && (Bitwise.CountSetBits(position.BlackBishops) == 1) &&
                                         (Board.LightSquares[(int)Bitwise.FirstSetSquare(position.WhiteBishops)] != Board.LightSquares[(int)Bitwise.FirstSetSquare(position.BlackBishops)]);
            var pieceMaterialDiff = winningPieceMaterial - losingPieceMaterial;
            if ((winningPawnCount == 0) && (pieceMaterialDiff <= Config.MgBishopMaterial))
            {
                // Winning side has no pawns and is up by a bishop or less.
                _staticScore.EgScalePer128 = winningPieceMaterial >= Config.MgRookMaterial
                    ? Config.EgBishopAdvantagePer128 // Winning side has a rook or more.
                    : 0; // Winning side has less than a rook.
            }
            else if (oppositeColoredBishops && (winningPieceMaterial == Config.MgBishopMaterial) && (losingPieceMaterial == Config.MgBishopMaterial))
            {
                // Sides have opposite colored bishops and no other pieces.
                _staticScore.EgScalePer128 = (winningPassedPawns * Config.EgOppBishopsPerPassedPawn) + Config.EgOppBishopsPer128;
            }
            else
            {
                // All Other Endgames
                _staticScore.EgScalePer128 = (winningPawnCount * Config.EgWinningPerPawn) + 128;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMateScore(int depth) => -StaticScore.Max + depth;
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMateMoveCount(int score)
        {
            var plyCount = (score > 0) ? StaticScore.Max - score : -StaticScore.Max - score;
            // Convert plies to full moves.
            var quotient = Math.DivRem(plyCount, 2, out var remainder);
            return quotient + remainder;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int DetermineGamePhase(Position position)
        {
            var phase = _knightPhase * Bitwise.CountSetBits(position.WhiteKnights | position.BlackKnights) +
                        _bishopPhase * Bitwise.CountSetBits(position.WhiteBishops | position.BlackBishops) +
                        _rookPhase * Bitwise.CountSetBits(position.WhiteRooks | position.BlackRooks) +
                        _queenPhase * Bitwise.CountSetBits(position.WhiteQueens | position.BlackQueens);
            return Math.Min(phase, MiddlegamePhase);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNonLinearBonus(double bonus, double scale, double power, int constant) => (int)(scale * Math.Pow(bonus, power)) + constant;


        public string ShowParameters()
        {
            var stringBuilder = new StringBuilder();
            // Material
            stringBuilder.AppendLine("Material");
            stringBuilder.AppendLine("===========");
            stringBuilder.AppendLine($"Pawn:       {PawnMaterial}");
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
            stringBuilder.AppendLine($"King Safety MinorAttackOuterRingPer8:  {Config.MgKingSafetyMinorAttackOuterRingPer8:000}");
            stringBuilder.AppendLine($"King Safety MinorAttackInnerRingPer8:  {Config.MgKingSafetyMinorAttackInnerRingPer8:000}");
            stringBuilder.AppendLine($"King Safety RookAttackOuterRingPer8:   {Config.MgKingSafetyRookAttackOuterRingPer8:000}");
            stringBuilder.AppendLine($"King Safety RookAttackInnerRingPer8:   {Config.MgKingSafetyRookAttackInnerRingPer8:000}");
            stringBuilder.AppendLine($"King Safety QueenAttackOuterRingPer8:  {Config.MgKingSafetyQueenAttackOuterRingPer8:000}");
            stringBuilder.AppendLine($"King Safety QueenAttackInnerRingPer8:  {Config.MgKingSafetyQueenAttackInnerRingPer8:000}");
            stringBuilder.AppendLine($"King Safety SemiOpenFilePer8:          {Config.MgKingSafetySemiOpenFilePer8:000}");
            stringBuilder.AppendLine($"King Safety PawnShieldPer8:            {Config.MgKingSafetyPawnShieldPer8:000}");
            stringBuilder.AppendLine();
            stringBuilder.Append("Middlegame King Safety:  ");
            ShowParameterArray(_mgKingSafety, stringBuilder);
            return stringBuilder.ToString();
        }


        private static void ShowParameterSquares(int[] parameters, StringBuilder stringBuilder)
        {
            for (var rank = 7; rank >= 0; rank--)
            {
                for (var file = 0; file < 8; file++)
                {
                    var square = Board.GetSquare(file, rank);
                    stringBuilder.Append(parameters[(int)square].ToString("+000;-000").PadRight(6));
                }
                stringBuilder.AppendLine();
            }
        }


        private static void ShowParameterArray(int[] parameters, StringBuilder stringBuilder)
        {
            for (var index = 0; index < parameters.Length; index++) stringBuilder.Append(parameters[index].ToString("+000;-000").PadRight(5));
            stringBuilder.AppendLine();
        }


        public string ToString(Position position)
        {
            GetStaticScore(position);
            var phase = DetermineGamePhase(position);
            return _staticScore.ToString(phase);
        }
    }
}
