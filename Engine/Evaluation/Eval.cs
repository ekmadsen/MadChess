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
using ErikTheCoder.MadChess.Engine.Score;


namespace ErikTheCoder.MadChess.Engine.Evaluation
{
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
        public const int MiddlegamePhase = 4 * (_knightPhaseWeight + _bishopPhaseWeight + _rookPhaseWeight) + (2 * _queenPhaseWeight);
        private const int _knightPhaseWeight = 10; //   4 * 10 =  40
        private const int _bishopPhaseWeight = 10; // + 4 * 10 =  80
        private const int _rookPhaseWeight = 22; //   + 4 * 22 = 168
        private const int _queenPhaseWeight = 44; //  + 2 * 44 = 256
        // Draw by Repetition
        public int DrawMoves;
        // Material
        public const int PawnMaterial = 100;
        public readonly EvalConfig Config;
        private readonly int[] _mgMaterialScores; // [(int)colorlessPiece]
        private readonly int[] _egMaterialScores; // [(int)colorlessPiece]
        private static readonly int[] _exchangeMaterialScores =
        {
            0,   // None
            100, // Pawn
            300, // Knight
            300, // Bishop
            500, // Rook
            900  // Queen
        };
        // Piece Location
        private readonly int[][] _mgPieceLocations; // [(int)colorlessPiece][(int)square)]
        private readonly int[][] _egPieceLocations; // [(int)colorlessPiece][(int)square)]
        // Passed Pawns
        private readonly int[] _mgPassedPawns;
        private readonly int[] _egPassedPawns;
        private readonly int[] _egFreePassedPawns;
        // Piece Mobility
        private readonly int[][] _mgPieceMobility; // [(int)colorlessPiece][moveCount]
        private readonly int[][] _egPieceMobility; // [(int)colorlessPiece][moveCount]
        // King Safety
        private readonly int[][] _mgKingSafetyAttackWeights; // [(int)colorlessPiece][(int)kingRing]
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
            _mgMaterialScores = new int[(int)ColorlessPiece.King + 1];
            _egMaterialScores = new int[(int)ColorlessPiece.King + 1];
            _mgPieceLocations = new int[(int)ColorlessPiece.King + 1][];
            _egPieceLocations = new int[(int)ColorlessPiece.King + 1][];
            for (var colorlessPiece = ColorlessPiece.None; colorlessPiece <= ColorlessPiece.King; colorlessPiece++)
            {
                _mgPieceLocations[(int)colorlessPiece] = new int[64];
                _egPieceLocations[(int)colorlessPiece] = new int[64];
            }
            _mgPieceMobility = new[]
            {
                new int[0],  // None
                new int[0],  // Pawn
                new int[9],  // Knight
                new int[14], // Bishop
                new int[15], // Rook
                new int[28]  // Queen
            };
            _egPieceMobility = new[]
            {
                new int[0],  // None
                new int[0],  // Pawn
                new int[9],  // Knight
                new int[14], // Bishop
                new int[15], // Rook
                new int[28]  // Queen
            };
            _mgPassedPawns = new int[8];
            _egPassedPawns = new int[8];
            _egFreePassedPawns = new int[8];
            _mgKingSafetyAttackWeights = new[]
            {
                new int[0], // None
                new int[0], // Pawn
                new int[2], // Knight
                new int[2], // Bishop
                new int[2], // Rook
                new int[2]  // Queen
            };
            _mgKingSafety = new int[64];
            // Set number of repetitions considered a draw, calculate positional factors, and set evaluation strength.
            DrawMoves = 2;
            CalculatePositionalFactors();
            ConfigureFullStrength();
        }


        public void CalculatePositionalFactors()
        {
            // Update material score array.
            _mgMaterialScores[(int)ColorlessPiece.Pawn] = PawnMaterial;
            _mgMaterialScores[(int)ColorlessPiece.Knight] = Config.MgKnightMaterial;
            _mgMaterialScores[(int)ColorlessPiece.Bishop] = Config.MgBishopMaterial;
            _mgMaterialScores[(int)ColorlessPiece.Rook] = Config.MgRookMaterial;
            _mgMaterialScores[(int)ColorlessPiece.Queen] = Config.MgQueenMaterial;
            _egMaterialScores[(int)ColorlessPiece.Pawn] = PawnMaterial;
            _egMaterialScores[(int)ColorlessPiece.Knight] = Config.EgKnightMaterial;
            _egMaterialScores[(int)ColorlessPiece.Bishop] = Config.EgBishopMaterial;
            _egMaterialScores[(int)ColorlessPiece.Rook] = Config.EgRookMaterial;
            _egMaterialScores[(int)ColorlessPiece.Queen] = Config.EgQueenMaterial;
            // Calculate piece location values.
            for (var colorlessPiece = ColorlessPiece.Pawn; colorlessPiece <= ColorlessPiece.King; colorlessPiece++)
            {
                for (var square = Square.A8; square < Square.Illegal; square++)
                {
                    var rank = Board.Ranks[(int)Color.White][(int)square];
                    var file = Board.Files[(int)square];
                    var squareCentrality = 3 - Board.DistanceToCentralSquares[(int)square];
                    var fileCentrality = 3 - Math.Min(Math.Abs(3 - file), Math.Abs(4 - file));
                    var nearestCorner = 3 - Board.DistanceToNearestCorner[(int)square];
                    int mgAdvancement;
                    int mgCentralityMetric;
                    int egCentralityMetric;
                    int mgCentrality;
                    int mgCorner;
                    int egAdvancement;
                    int egCentrality;
                    int egCorner;
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (colorlessPiece)
                    {
                        case ColorlessPiece.Pawn:
                            mgAdvancement = Config.MgPawnAdvancement;
                            mgCentralityMetric = squareCentrality;
                            egCentralityMetric = squareCentrality;
                            mgCentrality = Config.MgPawnCentrality;
                            mgCorner = 0;
                            egAdvancement = Config.EgPawnAdvancement;
                            egCentrality = Config.EgPawnCentrality;
                            egCorner = 0;
                            break;
                        case ColorlessPiece.Knight:
                            mgAdvancement = Config.MgKnightAdvancement;
                            mgCentralityMetric = squareCentrality;
                            egCentralityMetric = squareCentrality;
                            mgCentrality = Config.MgKnightCentrality;
                            mgCorner = Config.MgKnightCorner;
                            egAdvancement = Config.EgKnightAdvancement;
                            egCentrality = Config.EgKnightCentrality;
                            egCorner = Config.EgKnightCorner;
                            break;
                        case ColorlessPiece.Bishop:
                            mgAdvancement = Config.MgBishopAdvancement;
                            mgCentralityMetric = squareCentrality;
                            egCentralityMetric = squareCentrality;
                            mgCentrality = Config.MgBishopCentrality;
                            mgCorner = Config.MgBishopCorner;
                            egAdvancement = Config.EgBishopAdvancement;
                            egCentrality = Config.EgBishopCentrality;
                            egCorner = Config.EgBishopCorner;
                            break;
                        case ColorlessPiece.Rook:
                            mgAdvancement = Config.MgRookAdvancement;
                            mgCentralityMetric = fileCentrality;
                            egCentralityMetric = squareCentrality;
                            mgCentrality = Config.MgRookCentrality;
                            mgCorner = Config.MgRookCorner;
                            egAdvancement = Config.EgRookAdvancement;
                            egCentrality = Config.EgRookCentrality;
                            egCorner = Config.EgRookCorner;
                            break;
                        case ColorlessPiece.Queen:
                            mgAdvancement = Config.MgQueenAdvancement;
                            mgCentralityMetric = squareCentrality;
                            egCentralityMetric = squareCentrality;
                            mgCentrality = Config.MgQueenCentrality;
                            mgCorner = Config.MgQueenCorner;
                            egAdvancement = Config.EgQueenAdvancement;
                            egCentrality = Config.EgQueenCentrality;
                            egCorner = Config.EgQueenCorner;
                            break;
                        case ColorlessPiece.King:
                            mgAdvancement = Config.MgKingAdvancement;
                            mgCentralityMetric = squareCentrality;
                            egCentralityMetric = squareCentrality;
                            mgCentrality = Config.MgKingCentrality;
                            mgCorner = Config.MgKingCorner;
                            egAdvancement = Config.EgKingAdvancement;
                            egCentrality = Config.EgKingCentrality;
                            egCorner = Config.EgKingCorner;
                            break;
                        default:
                            throw new Exception($"{colorlessPiece} colorless piece not supported.");
                    }
                    _mgPieceLocations[(int)colorlessPiece][(int)square] = (rank * mgAdvancement) + (mgCentralityMetric * mgCentrality) + (nearestCorner * mgCorner);
                    _egPieceLocations[(int)colorlessPiece][(int)square] = (rank * egAdvancement) + (egCentralityMetric * egCentrality) + (nearestCorner * egCorner);
                }
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
            CalculatePieceMobility(_mgPieceMobility[(int)ColorlessPiece.Knight], _egPieceMobility[(int)ColorlessPiece.Knight], Config.MgKnightMobilityScale, Config.EgKnightMobilityScale);
            CalculatePieceMobility(_mgPieceMobility[(int)ColorlessPiece.Bishop], _egPieceMobility[(int)ColorlessPiece.Bishop], Config.MgBishopMobilityScale, Config.EgBishopMobilityScale);
            CalculatePieceMobility(_mgPieceMobility[(int)ColorlessPiece.Rook], _egPieceMobility[(int)ColorlessPiece.Rook], Config.MgRookMobilityScale, Config.EgRookMobilityScale);
            CalculatePieceMobility(_mgPieceMobility[(int)ColorlessPiece.Queen], _egPieceMobility[(int)ColorlessPiece.Queen], Config.MgQueenMobilityScale, Config.EgQueenMobilityScale);
            // Calculate king safety values.
            _mgKingSafetyAttackWeights[(int)ColorlessPiece.Knight][(int)KingRing.Outer] = Config.MgKingSafetyMinorAttackOuterRingPer8;
            _mgKingSafetyAttackWeights[(int)ColorlessPiece.Knight][(int)KingRing.Inner] = Config.MgKingSafetyMinorAttackInnerRingPer8;
            _mgKingSafetyAttackWeights[(int)ColorlessPiece.Bishop][(int)KingRing.Outer] = Config.MgKingSafetyMinorAttackOuterRingPer8;
            _mgKingSafetyAttackWeights[(int)ColorlessPiece.Bishop][(int)KingRing.Inner] = Config.MgKingSafetyMinorAttackInnerRingPer8;
            _mgKingSafetyAttackWeights[(int)ColorlessPiece.Rook][(int)KingRing.Outer] = Config.MgKingSafetyRookAttackOuterRingPer8;
            _mgKingSafetyAttackWeights[(int)ColorlessPiece.Rook][(int)KingRing.Inner] = Config.MgKingSafetyRookAttackInnerRingPer8;
            _mgKingSafetyAttackWeights[(int)ColorlessPiece.Queen][(int)KingRing.Outer] = Config.MgKingSafetyQueenAttackOuterRingPer8;
            _mgKingSafetyAttackWeights[(int)ColorlessPiece.Queen][(int)KingRing.Inner] = Config.MgKingSafetyQueenAttackInnerRingPer8;
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


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public (bool TerminalDraw, bool RepeatPosition) IsTerminalDraw(Position position)
        {
            // Only return true if position is drawn and no sequence of moves can make game winnable.
            if (_isRepeatPosition(DrawMoves)) return (true, true); // Draw by repetition of position.
            if (position.PlySinceCaptureOrPawnMove >= StaticScore.MaxPlyWithoutCaptureOrPawnMove) return (true, false); // Draw by 50 moves (100 ply) without a capture or pawn move.
            // Determine if insufficient material remains for checkmate.
            if (Bitwise.CountSetBits(position.GetPawns(Color.White) | position.GetPawns(Color.Black)) == 0)
            {
                // Neither side has any pawns.
                if (Bitwise.CountSetBits(position.GetMajorPieces(Color.White) | position.GetMajorPieces(Color.Black)) == 0)
                {
                    // Neither side has any major pieces.
                    if ((Bitwise.CountSetBits(position.GetMinorPieces(Color.White)) <= 1) && (Bitwise.CountSetBits(position.GetMinorPieces(Color.Black)) <= 1))
                    {
                        // Each side has one or zero minor pieces.  Draw by insufficient material.
                        return (true, false);
                    }
                }
            }
            return (false, false);
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public (int StaticScore, bool DrawnEndgame) GetStaticScore(Position position)
        {
            // TODO: Handicap knowledge of checkmates and endgames when in limited strength mode.
            Debug.Assert(!position.KingInCheck);
            _stats.Evaluations++;
            _staticScore.Reset();
            if (EvaluateSimpleEndgame(position, Color.White) || EvaluateSimpleEndgame(position, Color.Black))
            {
                // Position is a simple endgame.
                return _staticScore.EgScalePer128 == 0
                    ? (0, true) // Drawn Endgame
                    : (_staticScore.GetEg(position.ColorToMove) - _staticScore.GetEg(position.ColorLastMoved), false);
            }
            // Position is not a simple endgame.
            _staticScore.PlySinceCaptureOrPawnMove = position.PlySinceCaptureOrPawnMove;
            // Explicit array lookups are faster than looping through colors.
            EvaluateMaterial(position, Color.White);
            EvaluateMaterial(position, Color.Black);
            EvaluatePieceLocation(position, Color.White);
            EvaluatePieceLocation(position, Color.Black);
            EvaluatePawns(position, Color.White);
            EvaluatePawns(position, Color.Black);
            EvaluatePieceMobilityKingSafety(position, Color.White);
            EvaluatePieceMobilityKingSafety(position, Color.Black);
            EvaluateMinorPieces(position, Color.White);
            EvaluateMinorPieces(position, Color.Black);
            // Limit strength, determine endgame scale, phase, and total score.
            if (Config.LimitedStrength) LimitStrength();
            DetermineEndgameScale(position); // Scale down scores for difficult to win endgames.
            if (_staticScore.EgScalePer128 == 0) return (0, true); // Drawn Endgame
            var phase = DetermineGamePhase(position);
            return (_staticScore.GetTotalScore(position.ColorToMove, phase), false);
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool EvaluateSimpleEndgame(Position position, Color color)
        {
            var enemyColor = 1 - color;
            var pawnCount = Bitwise.CountSetBits(position.GetPawns(color));
            var enemyPawnCount = Bitwise.CountSetBits(position.GetPawns(enemyColor));
            if ((pawnCount == 0) && (enemyPawnCount == 0) && IsPawnlessDraw(position))
            {
                // Game is pawnless draw.
                _staticScore.EgScalePer128 = 0;
                return true;
            }
            var knightCount = Bitwise.CountSetBits(position.GetKnights(color));
            var bishopCount = Bitwise.CountSetBits(position.GetBishops(color));
            var minorPieceCount = knightCount + bishopCount;
            var majorPieceCount = Bitwise.CountSetBits(position.GetRooks(color) | position.GetQueens(color));
            var pawnsAndPiecesCount = pawnCount + minorPieceCount + majorPieceCount;
            var enemyKnightCount = Bitwise.CountSetBits(position.GetKnights(enemyColor));
            var enemyBishops = position.GetBishops(enemyColor);
            var enemyBishopCount = Bitwise.CountSetBits(enemyBishops);
            var enemyMinorPieceCount = enemyKnightCount + enemyBishopCount;
            var enemyMajorPieceCount = Bitwise.CountSetBits(position.GetRooks(enemyColor) | position.GetQueens(enemyColor));
            var enemyPawnsAndPiecesCount = enemyPawnCount + enemyMinorPieceCount + enemyMajorPieceCount;
            if ((pawnsAndPiecesCount > 0) && (enemyPawnsAndPiecesCount > 0)) return false; // Position is not a simple endgame.
            var loneEnemyPawn = (enemyPawnCount == 1) && (enemyPawnsAndPiecesCount == 1);
            var kingSquare = Bitwise.FirstSetSquare(position.GetKing(color));
            var enemyKingSquare = Bitwise.FirstSetSquare(position.GetKing(enemyColor));
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (pawnsAndPiecesCount)
            {
                // Case 0 = Lone King
                case 0 when loneEnemyPawn:
                    EvaluateKingVersusPawn(position, enemyColor);
                    return true;
                // ReSharper disable once SwitchStatementMissingSomeCases
                case 0:
                    switch (enemyPawnCount)
                    {
                        case 0 when (enemyBishopCount == 1) && (enemyKnightCount == 1) && (enemyMajorPieceCount == 0):
                            // K vrs KBN
                            var enemyBishopColor = Board.SquareColors[(int)Bitwise.FirstSetSquare(enemyBishops)];
                            var distanceToCorrectColorCorner = Board.DistanceToNearestCornerOfColor[(int)enemyBishopColor][(int)kingSquare];
                            _staticScore.EgSimple[(int)enemyColor]  = Config.SimpleEndgame - distanceToCorrectColorCorner - Board.SquareDistances[(int)kingSquare][(int)enemyKingSquare];
                            return true;
                        case 0 when (enemyMajorPieceCount == 1) && (enemyMinorPieceCount == 0):
                            // K vrs KQ or KR
                            _staticScore.EgSimple[(int)enemyColor] = Config.SimpleEndgame - Board.DistanceToNearestCorner[(int)kingSquare] - Board.SquareDistances[(int)kingSquare][(int)enemyKingSquare];
                            return true;
                    }
                    break;
            }
            // Use regular evaluation.
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPawnlessDraw(Position position) => IsPawnlessDraw(position, Color.White) || IsPawnlessDraw(position, Color.Black);


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool IsPawnlessDraw(Position position, Color color)
        {
            var knightCount = Bitwise.CountSetBits(position.GetKnights(color));
            var bishopCount = Bitwise.CountSetBits(position.GetBishops(color));
            var rookCount = Bitwise.CountSetBits(position.GetRooks(color));
            var queenCount = Bitwise.CountSetBits(position.GetQueens(color));
            var minorPieceCount = knightCount + bishopCount;
            var majorPieceCount = rookCount + queenCount;
            var enemyColor = 1 - color;
            var enemyKnightCount = Bitwise.CountSetBits(position.GetKnights(enemyColor));
            var enemyBishopCount = Bitwise.CountSetBits(position.GetBishops(enemyColor));
            var enemyRookCount = Bitwise.CountSetBits(position.GetRooks(enemyColor));
            var enemyQueenCount = Bitwise.CountSetBits(position.GetQueens(enemyColor));
            var enemyMinorPieceCount = enemyKnightCount + enemyBishopCount;
            var enemyMajorPieceCount = enemyRookCount + enemyQueenCount;
            var totalMajorPieces = majorPieceCount + enemyMajorPieceCount;
            // TODO: Add rook versus bishop as a draw?
            switch (totalMajorPieces)
            {
                case 0:
                    if ((knightCount == 2) && (minorPieceCount == 2) && (enemyMinorPieceCount <= 1)) return true; // 2N vrs <= 1 Minor
                    break;
                case 1:
                    if ((queenCount == 1) && (minorPieceCount == 0))
                    {
                        if ((enemyBishopCount == 2) && (enemyMinorPieceCount == 2)) return true; // Q vrs 2B
                        if ((enemyKnightCount == 2) && (enemyMinorPieceCount == 2)) return true; // Q vrs 2N
                    }
                    // Considering R vrs <= 2 Minors a draw increases evaluation error and causes engine to play weaker.
                    //if ((rookCount == 1) && (minorPieceCount == 0) && (enemyMinorPieceCount <= 2)) return true; // R vrs <= 2 Minors
                    break;
                case 2:
                    if ((queenCount == 1) && (minorPieceCount == 0))
                    {
                        if ((enemyQueenCount == 1) && (enemyMinorPieceCount == 0)) return true; // Q vrs Q
                        if ((enemyRookCount == 1) && (enemyMinorPieceCount == 1)) return true; // Q vrs R + Minor
                    }
                    if ((rookCount == 1) && (minorPieceCount == 0) && (enemyRookCount == 1) && (enemyMinorPieceCount <= 1)) return true; // R vrs R + <= 1 Minor
                    break;
                case 3:
                    if ((queenCount == 1) && (minorPieceCount == 0) && (enemyRookCount == 2) && (enemyMinorPieceCount == 0)) return true; // Q vrs 2R
                    if ((rookCount == 2) & (minorPieceCount == 0) && (enemyRookCount == 1) && (enemyMinorPieceCount == 1)) return true; // 2R vrs R + Minor
                    break;
                case 4:
                    if ((rookCount == 2) && (minorPieceCount == 0) && (enemyRookCount == 2) && (enemyMinorPieceCount == 0)) return true; // 2R vrs 2R
                    break;
            }
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void EvaluateKingVersusPawn(Position position, Color lonePawnColor)
        {
            var winningKingSquare = Bitwise.FirstSetSquare(position.GetKing(lonePawnColor));
            var winningKingRank = Board.Ranks[(int)lonePawnColor][(int)winningKingSquare];
            var winningKingFile = Board.Files[(int)winningKingSquare];
            var defendingKingColor = 1 - lonePawnColor;
            var defendingKingSquare = Bitwise.FirstSetSquare(position.GetKing(defendingKingColor));
            var defendingKingRank = Board.Ranks[(int)defendingKingColor][(int)defendingKingSquare];
            var defendingKingFile = Board.Files[(int)defendingKingSquare];
            var pawnSquare = Bitwise.FirstSetSquare(position.GetPawns(lonePawnColor));
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
                    _staticScore.EgSimple[(int)lonePawnColor] = Config.SimpleEndgame + pawnRank;
                    return;
                }
            }
            // Pawn does not promote.
            // Game is drawn.
            _staticScore.EgScalePer128 = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void EvaluateMaterial(Position position, Color color)
        {
            // Explicit piece evaluation is faster than looping through pieces due to avoiding CPU stalls and enabling out-of-order execution.
            // See https://stackoverflow.com/a/2349265/8992299.
            // Pawns
            _staticScore.PawnMaterial[(int)color] = Bitwise.CountSetBits(position.GetPawns(color)) * PawnMaterial;
            // Knights
            var knight = PieceHelper.GetPieceOfColor(ColorlessPiece.Knight, color);
            var knightCount = Bitwise.CountSetBits(position.PieceBitboards[(int)knight]);
            var mgKnightMaterial = knightCount * _mgMaterialScores[(int)ColorlessPiece.Knight];
            var egKnightMaterial = knightCount * _egMaterialScores[(int)ColorlessPiece.Knight];
            // Bishops
            var bishop = PieceHelper.GetPieceOfColor(ColorlessPiece.Bishop, color);
            var bishopCount = Bitwise.CountSetBits(position.PieceBitboards[(int)bishop]);
            var mgBishopMaterial = bishopCount * _mgMaterialScores[(int)ColorlessPiece.Bishop];
            var egBishopMaterial = bishopCount * _egMaterialScores[(int)ColorlessPiece.Bishop];
            // Rooks
            var rook = PieceHelper.GetPieceOfColor(ColorlessPiece.Rook, color);
            var rookCount = Bitwise.CountSetBits(position.PieceBitboards[(int)rook]);
            var mgRookMaterial = rookCount * _mgMaterialScores[(int)ColorlessPiece.Rook];
            var egRookMaterial = rookCount * _egMaterialScores[(int)ColorlessPiece.Rook];
            // Queens
            var queen = PieceHelper.GetPieceOfColor(ColorlessPiece.Queen, color);
            var queenCount = Bitwise.CountSetBits(position.PieceBitboards[(int)queen]);
            var mgQueenMaterial = queenCount * _mgMaterialScores[(int)ColorlessPiece.Queen];
            var egQueenMaterial = queenCount * _egMaterialScores[(int)ColorlessPiece.Queen];
            // Total
            _staticScore.MgPieceMaterial[(int)color] = mgKnightMaterial + mgBishopMaterial + mgRookMaterial + mgQueenMaterial;
            _staticScore.EgPieceMaterial[(int)color] = egKnightMaterial + egBishopMaterial + egRookMaterial + egQueenMaterial;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMaterialScore(Position position, ColorlessPiece colorlessPiece)
        {
            var mgMaterial = _mgMaterialScores[(int)colorlessPiece];
            var egMaterial = _egMaterialScores[(int)colorlessPiece];
            var phase = DetermineGamePhase(position);
            return StaticScore.GetTaperedScore(mgMaterial, egMaterial, phase);
        }


        public static (int StaticScore, bool DrawnEndgame) GetExchangeMaterialScore(Position position)
        {
            var score = 0;
            for (var colorlessPiece = ColorlessPiece.Pawn; colorlessPiece <= ColorlessPiece.Queen; colorlessPiece++)
            {
                var piece = PieceHelper.GetPieceOfColor(colorlessPiece, position.ColorToMove);
                var enemyPiece = PieceHelper.GetPieceOfColor(colorlessPiece, position.ColorLastMoved);
                var pieceCount = Bitwise.CountSetBits(position.PieceBitboards[(int)piece]);
                var enemyPieceCount = Bitwise.CountSetBits(position.PieceBitboards[(int)enemyPiece]);
                score += (pieceCount - enemyPieceCount) * _exchangeMaterialScores[(int)colorlessPiece];
            }
            return (score, false);
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void EvaluatePieceLocation(Position position, Color color)
        {
            for (var colorlessPiece = ColorlessPiece.Pawn; colorlessPiece <= ColorlessPiece.King; colorlessPiece++)
            {
                var piece = PieceHelper.GetPieceOfColor(colorlessPiece, color);
                var pieces = position.PieceBitboards[(int)piece];
                Square square;
                while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
                {
                    var squareFromWhitePerspective = Board.GetSquareFromWhitePerspective(square, color);
                    _staticScore.MgPieceLocation[(int)color] += _mgPieceLocations[(int)colorlessPiece][(int)squareFromWhitePerspective];
                    _staticScore.EgPieceLocation[(int)color] += _egPieceLocations[(int)colorlessPiece][(int)squareFromWhitePerspective];
                    Bitwise.ClearBit(ref pieces, square);
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void EvaluatePawns(Position position, Color color)
        {
            var enemyColor = 1 - color;
            var pawns = position.GetPawns(color);
            if (pawns == 0) return;
            var kingSquare = Bitwise.FirstSetSquare(position.GetKing(color));
            var enemyKingSquare = Bitwise.FirstSetSquare(position.GetKing(enemyColor));
            Square pawnSquare;
            while ((pawnSquare = Bitwise.FirstSetSquare(pawns)) != Square.Illegal)
            {
                if (IsPassedPawn(position, pawnSquare, color))
                {
                    _staticScore.PassedPawnCount[(int)color]++;
                    var rank = Board.Ranks[(int)color][(int)pawnSquare];
                    _staticScore.EgKingEscortedPassedPawns[(int)color] += (Board.SquareDistances[(int)pawnSquare][(int)enemyKingSquare] - Board.SquareDistances[(int)pawnSquare][(int)kingSquare]) * Config.EgKingEscortedPassedPawn;
                    if (IsFreePawn(position, pawnSquare, color))
                    {
                        // Pawn is free to advance.
                        if (IsUnstoppablePawn(position, pawnSquare, enemyKingSquare, color)) _staticScore.UnstoppablePassedPawns[(int)color] += Config.UnstoppablePassedPawn; // Pawn is unstoppable.
                        else _staticScore.EgFreePassedPawns[(int)color] += _egFreePassedPawns[rank]; // Pawn is passed and free.
                    }
                    else
                    {
                        // Pawn is passed.
                        _staticScore.MgPassedPawns[(int)color] += _mgPassedPawns[rank];
                        _staticScore.EgPassedPawns[(int)color] += _egPassedPawns[rank];
                    }
                }
                Bitwise.ClearBit(ref pawns, pawnSquare);
            }
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPassedPawn(Position position, Square square, Color color)
        {
            Debug.Assert(PieceHelper.GetColorlessPiece(position.GetPiece(square)) == ColorlessPiece.Pawn);
            Debug.Assert(PieceHelper.GetColor(position.GetPiece(square)) == color);
            var enemyColor = 1 - color;
            // Determine if pawn can be blocked or attacked by enemy pawns as it advances to promotion square.
            return (Board.PassedPawnMasks[(int)color][(int)square] & position.GetPawns(enemyColor)) == 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFreePawn(Position position, Square square, Color color)
        {
            Debug.Assert(PieceHelper.GetColorlessPiece(position.GetPiece(square)) == ColorlessPiece.Pawn);
            Debug.Assert(PieceHelper.GetColor(position.GetPiece(square)) == color);
            // Determine if pawn can advance.
            return (Board.FreePawnMasks[(int)color][(int)square] & position.Occupancy) == 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool IsUnstoppablePawn(Position position, Square pawnSquare, Square enemyKingSquare, Color color)
        {
            // Pawn is free to advance to promotion square.
            var enemyColor = 1 - color;
            var file = Board.Files[(int)pawnSquare];
            var rankOfPromotionSquare = (int)Piece.BlackPawn - ((int)color * (int)Piece.BlackPawn);
            var promotionSquare = Board.GetSquare(file, rankOfPromotionSquare);
            var enemyPieceCount = Bitwise.CountSetBits(position.GetMajorAndMinorPieces(enemyColor));
            if (enemyPieceCount == 0)
            {
                // Enemy has no minor or major pieces.
                var pawnDistanceToPromotionSquare = Board.SquareDistances[(int)pawnSquare][(int)promotionSquare];
                var kingDistanceToPromotionSquare = Board.SquareDistances[(int)enemyKingSquare][(int)promotionSquare];
                if (color != position.ColorToMove) kingDistanceToPromotionSquare--; // Enemy king can move one square closer to pawn.
                return kingDistanceToPromotionSquare > pawnDistanceToPromotionSquare; // Enemy king cannot stop pawn from promoting.
            }
            return false;
        }



        // TODO: Include stacked attacks on same square via x-rays.  For example, a rook behind a queen.
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void EvaluatePieceMobilityKingSafety(Position position, Color color)
        {
            var enemyColor = 1 - color;
            var enemyKingSquare = Bitwise.FirstSetSquare(position.GetKing(enemyColor));
            var enemyKingInnerRing = Board.InnerRingMasks[(int)enemyKingSquare];
            var enemyKingOuterRing = Board.OuterRingMasks[(int)enemyKingSquare];
            var mgEnemyKingSafetyIndexPer8 = 0;
            for (var colorlessPiece = ColorlessPiece.Knight; colorlessPiece <= ColorlessPiece.Queen; colorlessPiece++)
            {
                var piece = PieceHelper.GetPieceOfColor(colorlessPiece, color);
                var pieces = position.PieceBitboards[(int)piece];
                Square square;
                while ((square = Bitwise.FirstSetSquare(pieces)) != Square.Illegal)
                {
                    var getPieceMovesMask = Board.PieceMoveMaskDelegates[(int)colorlessPiece];
                    var unOrEnemyOccupiedSquares = ~position.ColorOccupancy[(int)color];
                    var pieceDestinations = getPieceMovesMask(square, position.Occupancy) & unOrEnemyOccupiedSquares;
                    var (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(pieceDestinations, _mgPieceMobility[(int)colorlessPiece], _egPieceMobility[(int)colorlessPiece]);
                    _staticScore.MgPieceMobility[(int)color] += mgPieceMobilityScore;
                    _staticScore.EgPieceMobility[(int)color] += egPieceMobilityScore;
                    var outerRingAttackWeight = _mgKingSafetyAttackWeights[(int)colorlessPiece][(int)KingRing.Outer];
                    var innerRingAttackWeight = _mgKingSafetyAttackWeights[(int)colorlessPiece][(int)KingRing.Inner];
                    var kingSafetyIndexIncrementPer8 = GetKingSafetyIndexIncrement(pieceDestinations, enemyKingOuterRing, enemyKingInnerRing, outerRingAttackWeight, innerRingAttackWeight);
                    mgEnemyKingSafetyIndexPer8 += kingSafetyIndexIncrementPer8;
                    Bitwise.ClearBit(ref pieces, square);
                }
            }
            // Evaluate enemy king near semi-open file.
            var enemyKingFile = Board.Files[(int)enemyKingSquare];
            var leftFileMask = enemyKingFile > 0 ? Board.FileMasks[enemyKingFile - 1] : 0;
            var enemyKingFileMask = Board.FileMasks[enemyKingFile];
            var rightFileMask = enemyKingFile < 7 ? Board.FileMasks[enemyKingFile + 1] : 0;
            var leftFileSemiOpen = (leftFileMask > 0) && ((position.GetPawns(enemyColor) & leftFileMask) == 0) ? 1 : 0;
            var kingFileSemiOpen = (position.GetPawns(enemyColor) & enemyKingFileMask) == 0 ? 1 : 0;
            var rightFileSemiOpen = (rightFileMask > 0) && ((position.GetPawns(enemyColor) & rightFileMask) == 0) ? 1 : 0;
            var semiOpenFiles = leftFileSemiOpen + kingFileSemiOpen + rightFileSemiOpen;
            mgEnemyKingSafetyIndexPer8 += semiOpenFiles * Config.MgKingSafetySemiOpenFilePer8;
            // Evaluate enemy king pawn shield.
            const int maxPawnsInShield = 3;
            var missingPawns = maxPawnsInShield - Bitwise.CountSetBits(position.GetPawns(enemyColor) & Board.PawnShieldMasks[(int)enemyColor][(int)enemyKingSquare]);
            mgEnemyKingSafetyIndexPer8 += missingPawns * Config.MgKingSafetyPawnShieldPer8;
            // Lookup king safety score in array.
            var maxIndex = _mgKingSafety.Length - 1;
            _staticScore.MgKingSafety[(int)enemyColor] = _mgKingSafety[Math.Min(mgEnemyKingSafetyIndexPer8 / 8, maxIndex)];
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EvaluateMinorPieces(Position position, Color color)
        {
            // Bishop Pair
            var bishopCount = Bitwise.CountSetBits(position.GetBishops(color));
            if (bishopCount >= 2)
            {
                _staticScore.MgBishopPair[(int)color] += Config.MgBishopPair;
                _staticScore.EgBishopPair[(int)color] += Config.EgBishopPair;
            }
        }


        private void LimitStrength()
        {
            for (var color = Color.White; color <= Color.Black; color++)
            {
                // Limit understanding of piece location.
                _staticScore.MgPieceLocation[(int)color] = (_staticScore.MgPieceLocation[(int)color] * Config.LsPieceLocationPer128) / 128;
                _staticScore.EgPieceLocation[(int)color] = (_staticScore.EgPieceLocation[(int)color] * Config.LsPieceLocationPer128) / 128;
                // Limit understanding of passed pawns.
                _staticScore.MgPassedPawns[(int)color] = (_staticScore.MgPassedPawns[(int)color] * Config.LsPassedPawnsPer128) / 128;
                _staticScore.EgPassedPawns[(int)color] = (_staticScore.EgPassedPawns[(int)color] * Config.LsPassedPawnsPer128) / 128;
                _staticScore.EgFreePassedPawns[(int)color] = (_staticScore.EgFreePassedPawns[(int)color] * Config.LsPassedPawnsPer128) / 128;
                _staticScore.EgKingEscortedPassedPawns[(int)color] = (_staticScore.EgKingEscortedPassedPawns[(int)color] * Config.LsPassedPawnsPer128) / 128;
                _staticScore.UnstoppablePassedPawns[(int)color] = (_staticScore.UnstoppablePassedPawns[(int)color] * Config.LsPassedPawnsPer128) / 128;
                // Limit understanding of piece mobility.
                _staticScore.MgPieceMobility[(int)color] = (_staticScore.MgPieceMobility[(int)color] * Config.LsPieceMobilityPer128) / 128;
                _staticScore.EgPieceMobility[(int)color] = (_staticScore.EgPieceMobility[(int)color] * Config.LsPieceMobilityPer128) / 128;
                // Limit understanding of king safety.
                _staticScore.MgKingSafety[(int)color] = (_staticScore.MgKingSafety[(int)color] * Config.LsKingSafetyPer128) / 128;
                // Limit understanding of minor pieces.
                _staticScore.MgBishopPair[(int)color] = (_staticScore.MgBishopPair[(int)color] * Config.LsMinorPiecesPer128) / 128;
                _staticScore.EgBishopPair[(int)color] = (_staticScore.EgBishopPair[(int)color] * Config.LsMinorPiecesPer128) / 128;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DetermineEndgameScale(Position position)
        {
            // Use middlegame material values because they are constant (endgame material values are tuned).
            // Determine which color is winning the endgame.
            var winningColor = _staticScore.GetEg(Color.White) >= _staticScore.GetEg(Color.Black) ? Color.White : Color.Black;
            var losingColor = 1 - winningColor;
            var winningPawnCount = Bitwise.CountSetBits(position.GetPawns(winningColor));
            var winningPassedPawns = _staticScore.PassedPawnCount[(int)winningColor];
            var winningPieceMaterial = _staticScore.MgPieceMaterial[(int)winningColor];
            var losingPieceMaterial = _staticScore.MgPieceMaterial[(int)losingColor];
            var whiteBishops = position.GetBishops(Color.White);
            var blackBishops = position.GetBishops(Color.Black);
            var oppositeColoredBishops = (Bitwise.CountSetBits(whiteBishops) == 1) && (Bitwise.CountSetBits(blackBishops) == 1) &&
                                         (Board.SquareColors[(int)Bitwise.FirstSetSquare(whiteBishops)] != Board.SquareColors[(int)Bitwise.FirstSetSquare(blackBishops)]);
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
            var phase = (Bitwise.CountSetBits(position.GetPieces(ColorlessPiece.Knight)) * _knightPhaseWeight) +
                        (Bitwise.CountSetBits(position.GetPieces(ColorlessPiece.Bishop)) * _bishopPhaseWeight) +
                        (Bitwise.CountSetBits(position.GetPieces(ColorlessPiece.Rook)) * _rookPhaseWeight) +
                        (Bitwise.CountSetBits(position.GetPieces(ColorlessPiece.Queen)) * _queenPhaseWeight);
            return Math.Min(phase, MiddlegamePhase);
        }


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
            for (var colorlessPiece = ColorlessPiece.Pawn; colorlessPiece <= ColorlessPiece.King; colorlessPiece++)
            {
                // Middlegame
                stringBuilder.AppendLine($"Middlegame {PieceHelper.GetName(colorlessPiece)} Location");
                stringBuilder.AppendLine("==============================================");
                ShowParameterSquares(_mgPieceLocations[(int)colorlessPiece], stringBuilder);
                stringBuilder.AppendLine();
                // Endgame
                stringBuilder.AppendLine($"Endgame {PieceHelper.GetName(colorlessPiece)} Location");
                stringBuilder.AppendLine("==============================================");
                ShowParameterSquares(_egPieceLocations[(int)colorlessPiece], stringBuilder);
                stringBuilder.AppendLine();
            }
            // Passed Pawns
            stringBuilder.Append("Middlegame Passed Pawns:            ");
            ShowParameterArray(_mgPassedPawns, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.Append("Endgame Passed Pawns:               ");
            ShowParameterArray(_egPassedPawns, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.Append("Endgame Free Passed Pawns:          ");
            ShowParameterArray(_egFreePassedPawns, stringBuilder);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Endgame King Escorted Passed Pawn:  {Config.EgKingEscortedPassedPawn}");
            stringBuilder.AppendLine($"Unstoppable Passed Pawn:            {Config.UnstoppablePassedPawn}");
            stringBuilder.AppendLine();
            // Mobility
            for (var colorlessPiece = ColorlessPiece.Knight; colorlessPiece <= ColorlessPiece.Queen; colorlessPiece++)
            {
                // Middlegame
                stringBuilder.Append($"Middlegame {PieceHelper.GetName(colorlessPiece)} Mobility:  ".PadLeft(29));
                ShowParameterArray(_mgPieceMobility[(int)colorlessPiece], stringBuilder);
                stringBuilder.AppendLine();
                // Endgame
                stringBuilder.Append($"   Endgame {PieceHelper.GetName(colorlessPiece)} Mobility:  ".PadLeft(29));
                ShowParameterArray(_egPieceMobility[(int)colorlessPiece], stringBuilder);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
            }
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
            stringBuilder.AppendLine();
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
        }


        public string ToString(Position position)
        {
            GetStaticScore(position);
            var phase = DetermineGamePhase(position);
            return _staticScore.ToString(phase);
        }
    }
}