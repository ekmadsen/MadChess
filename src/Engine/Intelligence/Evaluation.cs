// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using ErikTheCoder.MadChess.Core;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Moves;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Config;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Engine.Score;


namespace ErikTheCoder.MadChess.Engine.Intelligence;


public sealed class Evaluation
{
    private const int _egKingCornerFactor = 50;
    private const int _egKbnPenalty = 300;
    private readonly LimitStrengthEvalConfig _limitStrengthConfig;
    private readonly int[] _limitStrengthElos;
    private readonly Messenger _messenger;
    private readonly Stats _stats;
    private readonly EvaluationConfig _defaultConfig;
    private readonly StaticScore _staticScore;

    public readonly EvaluationConfig Config;

    // Game Phase (constants selected such that starting material = 128)
    public const int MiddlegamePhase = 4 * (_knightPhaseWeight + _bishopPhaseWeight + _rookPhaseWeight) + (2 * _queenPhaseWeight);
    private const int _knightPhaseWeight = 5; //   4 *  5 =  20
    private const int _bishopPhaseWeight = 5; // + 4 *  5 =  40
    private const int _rookPhaseWeight =  11; // + 4 * 11 =  84
    private const int _queenPhaseWeight = 22; // + 2 * 22 = 128

    // Draw by Repetition
    public int DrawMoves;

    // Material
    private readonly int[] _mgMaterialScores; // [colorlessPiece]
    private readonly int[] _egMaterialScores; // [colorlessPiece]

    // Passed Pawns
    private readonly ulong[] _passedPawns; // [color]
    private readonly int[] _mgPassedPawns; // [rank]
    private readonly int[] _egPassedPawns; // [rank]
    private readonly int[] _egFreePassedPawns; // [rank]
    private readonly int[] _egConnectedPassedPawns; // [rank]

    // King Safety
    private readonly int[][] _mgKingSafetyAttackWeights; // [colorlessPiece][kingRing]
    private readonly int[] _mgKingSafetyPieceProximityWeights; // [colorlessPiece]
    private readonly int[] _mgKingSafety; // [attacks]

    // Piece Location
    private readonly int[][] _mgPieceLocations; // [colorlessPiece][square]
    private readonly int[][] _egPieceLocations; // [colorlessPiece][square]

    // Piece Mobility
    private readonly int[][] _mgPieceMobility; // [colorlessPiece][moves]
    private readonly int[][] _egPieceMobility; // [colorlessPiece][moves]


    public Evaluation(LimitStrengthEvalConfig limitStrengthConfig, Messenger messenger, Stats stats)
    {
        _limitStrengthConfig = limitStrengthConfig;
        _messenger = messenger;
        _stats = stats;
        _staticScore = new StaticScore();

        // Do not set Config and _defaultConfig to same object in memory (reference equality) to avoid ConfigureLimitedStrength method overwriting defaults.
        Config = new EvaluationConfig();
        _defaultConfig = new EvaluationConfig();
        _limitStrengthElos = new[] { 600, 800, 1000, 1200, 1400, 1600, 1800, 2000, 2200, 2300, 2400 };

        // Create arrays for quick lookup of positional factors, then calculate positional factors.

        // Material
        _mgMaterialScores = new int[(int)ColorlessPiece.King + 1];
        _egMaterialScores = new int[(int)ColorlessPiece.King + 1];

        // Passed Pawns
        _passedPawns = new ulong[2];
        _mgPassedPawns = new int[8];
        _egPassedPawns = new int[8];
        _egFreePassedPawns = new int[8];
        _egConnectedPassedPawns = new int[8];

        // King Safety
        _mgKingSafetyAttackWeights = new[]
        {
            Array.Empty<int>(), // None
            Array.Empty<int>(), // Pawn
            new int[2],         // Knight
            new int[2],         // Bishop
            new int[2],         // Rook
            new int[2]          // Queen
        };
        _mgKingSafetyPieceProximityWeights = new int[(int)ColorlessPiece.King];
        _mgKingSafety = new int[64];

        // Piece Location
        _mgPieceLocations = new int[(int)ColorlessPiece.King + 1][];
        _egPieceLocations = new int[(int)ColorlessPiece.King + 1][];
        for (var colorlessPiece = ColorlessPiece.None; colorlessPiece <= ColorlessPiece.King; colorlessPiece++)
        {
            _mgPieceLocations[(int)colorlessPiece] = new int[64];
            _egPieceLocations[(int)colorlessPiece] = new int[64];
        }

        // Piece Mobility
        _mgPieceMobility = new[]
        {
            Array.Empty<int>(),  // None
            Array.Empty<int>(),  // Pawn
            new int[9],          // Knight
            new int[14],         // Bishop
            new int[15],         // Rook
            new int[28]          // Queen
        };

        _egPieceMobility = new[]
        {
            Array.Empty<int>(),  // None
            Array.Empty<int>(),  // Pawn
            new int[9],          // Knight
            new int[14],         // Bishop
            new int[15],         // Rook
            new int[28]          // Queen
        };

        // Set number of repetitions considered a draw, calculate positional factors, and set evaluation strength.
        DrawMoves = 2;
        CalculatePositionalFactors();
        ConfigureFullStrength();
    }


    public void CalculatePositionalFactors()
    {
        // Update material score array.
        _mgMaterialScores[(int)ColorlessPiece.Pawn] = Config.MgPawnMaterial;
        _mgMaterialScores[(int)ColorlessPiece.Knight] = Config.MgKnightMaterial;
        _mgMaterialScores[(int)ColorlessPiece.Bishop] = Config.MgBishopMaterial;
        _mgMaterialScores[(int)ColorlessPiece.Rook] = Config.MgRookMaterial;
        _mgMaterialScores[(int)ColorlessPiece.Queen] = Config.MgQueenMaterial;
        _mgMaterialScores[(int)ColorlessPiece.King] = 0;

        _egMaterialScores[(int)ColorlessPiece.Pawn] = Config.EgPawnMaterial;
        _egMaterialScores[(int)ColorlessPiece.Knight] = Config.EgKnightMaterial;
        _egMaterialScores[(int)ColorlessPiece.Bishop] = Config.EgBishopMaterial;
        _egMaterialScores[(int)ColorlessPiece.Rook] = Config.EgRookMaterial;
        _egMaterialScores[(int)ColorlessPiece.Queen] = Config.EgQueenMaterial;
        _egMaterialScores[(int)ColorlessPiece.King] = 0;

        // Calculate passed pawn values.
        var passedPawnPower = Config.PassedPawnPowerPer128 / 128d;
        var mgScale = Config.MgPassedPawnScalePer128 / 128d;
        var egScale = Config.EgPassedPawnScalePer128 / 128d;
        var egFreeScale = Config.EgFreePassedPawnScalePer128 / 128d;
        var egConnectedScale = Config.EgConnectedPassedPawnScalePer128 / 128d;

        for (var rank = 1; rank < 7; rank++)
        {
            _mgPassedPawns[rank] = Formula.GetNonLinearBonus(rank, mgScale, passedPawnPower, 0);
            _egPassedPawns[rank] = Formula.GetNonLinearBonus(rank, egScale, passedPawnPower, 0);
            _egFreePassedPawns[rank] = Formula.GetNonLinearBonus(rank, egFreeScale, passedPawnPower, 0);
            _egConnectedPassedPawns[rank] = Formula.GetNonLinearBonus(rank, egConnectedScale, passedPawnPower, 0);
        }

        // Calculate king safety values.
        _mgKingSafetyAttackWeights[(int)ColorlessPiece.Knight][(int)KingRing.Outer] = Config.MgKingSafetyKnightAttackOuterRingPer8;
        _mgKingSafetyAttackWeights[(int)ColorlessPiece.Knight][(int)KingRing.Inner] = Config.MgKingSafetyKnightAttackInnerRingPer8;
        _mgKingSafetyAttackWeights[(int)ColorlessPiece.Bishop][(int)KingRing.Outer] = Config.MgKingSafetyBishopAttackOuterRingPer8;
        _mgKingSafetyAttackWeights[(int)ColorlessPiece.Bishop][(int)KingRing.Inner] = Config.MgKingSafetyBishopAttackInnerRingPer8;
        _mgKingSafetyAttackWeights[(int)ColorlessPiece.Rook][(int)KingRing.Outer] = Config.MgKingSafetyRookAttackOuterRingPer8;
        _mgKingSafetyAttackWeights[(int)ColorlessPiece.Rook][(int)KingRing.Inner] = Config.MgKingSafetyRookAttackInnerRingPer8;
        _mgKingSafetyAttackWeights[(int)ColorlessPiece.Queen][(int)KingRing.Outer] = Config.MgKingSafetyQueenAttackOuterRingPer8;
        _mgKingSafetyAttackWeights[(int)ColorlessPiece.Queen][(int)KingRing.Inner] = Config.MgKingSafetyQueenAttackInnerRingPer8;
        _mgKingSafetyPieceProximityWeights[(int)ColorlessPiece.Knight] = Config.MgKingSafetyKnightProximityPer8;
        _mgKingSafetyPieceProximityWeights[(int)ColorlessPiece.Bishop] = Config.MgKingSafetyBishopProximityPer8;
        _mgKingSafetyPieceProximityWeights[(int)ColorlessPiece.Rook] = Config.MgKingSafetyRookProximityPer8;
        _mgKingSafetyPieceProximityWeights[(int)ColorlessPiece.Queen] = Config.MgKingSafetyQueenProximityPer8;

        var kingSafetyPower = Config.MgKingSafetyPowerPer128 / 128d;
        var kingSafetyScale = Config.MgKingSafetyScalePer128 / 128d;
        for (var index = 0; index < _mgKingSafety.Length; index++)
            _mgKingSafety[index] = Formula.GetNonLinearBonus(index, kingSafetyScale, kingSafetyPower, 0);

        // Calculate piece location values.
        for (var colorlessPiece = ColorlessPiece.Pawn; colorlessPiece <= ColorlessPiece.King; colorlessPiece++)
        {
            for (var square = Square.A8; square < Square.Illegal; square++)
            {
                var rank = Board.Ranks[(int)Color.White][(int)square];
                var file = Board.Files[(int)square];

                var squareCentrality = 3 - Board.DistanceToCentralSquares[(int)square];
                var fileCentrality = 3 - FastMath.Min(FastMath.Abs(3 - file), FastMath.Abs(4 - file));
                var mgCentralityMetric = squareCentrality;

                var nearestCorner = 3 - Board.DistanceToNearestCorner[(int)square];

                int mgAdvancement;
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
                        mgCentrality = Config.MgPawnCentrality;
                        mgCorner = 0;
                        egAdvancement = Config.EgPawnAdvancement;
                        egCentrality = Config.EgPawnCentrality;
                        egCorner = 0;
                        break;

                    case ColorlessPiece.Knight:
                        mgAdvancement = Config.MgKnightAdvancement;
                        mgCentrality = Config.MgKnightCentrality;
                        mgCorner = Config.MgKnightCorner;
                        egAdvancement = Config.EgKnightAdvancement;
                        egCentrality = Config.EgKnightCentrality;
                        egCorner = Config.EgKnightCorner;
                        break;

                    case ColorlessPiece.Bishop:
                        mgAdvancement = Config.MgBishopAdvancement;
                        mgCentrality = Config.MgBishopCentrality;
                        mgCorner = Config.MgBishopCorner;
                        egAdvancement = Config.EgBishopAdvancement;
                        egCentrality = Config.EgBishopCentrality;
                        egCorner = Config.EgBishopCorner;
                        break;

                    case ColorlessPiece.Rook:
                        mgAdvancement = Config.MgRookAdvancement;
                        mgCentralityMetric = fileCentrality;
                        mgCentrality = Config.MgRookCentrality;
                        mgCorner = Config.MgRookCorner;
                        egAdvancement = Config.EgRookAdvancement;
                        egCentrality = Config.EgRookCentrality;
                        egCorner = Config.EgRookCorner;
                        break;

                    case ColorlessPiece.Queen:
                        mgAdvancement = Config.MgQueenAdvancement;
                        mgCentrality = Config.MgQueenCentrality;
                        mgCorner = Config.MgQueenCorner;
                        egAdvancement = Config.EgQueenAdvancement;
                        egCentrality = Config.EgQueenCentrality;
                        egCorner = Config.EgQueenCorner;
                        break;

                    case ColorlessPiece.King:
                        mgAdvancement = Config.MgKingAdvancement;
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
                _egPieceLocations[(int)colorlessPiece][(int)square] = (rank * egAdvancement) + (squareCentrality * egCentrality) + (nearestCorner * egCorner);
            }
        }

        // Calculate piece mobility values.
        CalculatePieceMobility(_mgPieceMobility[(int)ColorlessPiece.Knight], _egPieceMobility[(int)ColorlessPiece.Knight], Config.MgKnightMobilityScale, Config.EgKnightMobilityScale);
        CalculatePieceMobility(_mgPieceMobility[(int)ColorlessPiece.Bishop], _egPieceMobility[(int)ColorlessPiece.Bishop], Config.MgBishopMobilityScale, Config.EgBishopMobilityScale);
        CalculatePieceMobility(_mgPieceMobility[(int)ColorlessPiece.Rook], _egPieceMobility[(int)ColorlessPiece.Rook], Config.MgRookMobilityScale, Config.EgRookMobilityScale);
        CalculatePieceMobility(_mgPieceMobility[(int)ColorlessPiece.Queen], _egPieceMobility[(int)ColorlessPiece.Queen], Config.MgQueenMobilityScale, Config.EgQueenMobilityScale);
    }


    private void CalculatePieceMobility(int[] mgPieceMobility, int[] egPieceMobility, int mgMobilityScale, int egMobilityScale)
    {
        Debug.Assert(mgPieceMobility.Length == egPieceMobility.Length);

        var maxMoves = mgPieceMobility.Length - 1;
        var pieceMobilityPower = Config.PieceMobilityPowerPer128 / 128d;

        for (var moves = 0; moves <= maxMoves; moves++)
        {
            var fractionOfMaxMoves = (double) moves / maxMoves;
            mgPieceMobility[moves] = Formula.GetNonLinearBonus(fractionOfMaxMoves, mgMobilityScale, pieceMobilityPower, -mgMobilityScale / 2);
            egPieceMobility[moves] = Formula.GetNonLinearBonus(fractionOfMaxMoves, egMobilityScale, pieceMobilityPower, -egMobilityScale / 2);
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

        // Undervalue pawns.
        Config.MgPawnMaterial = Formula.GetLinearlyInterpolatedValue(50, _defaultConfig.MgPawnMaterial, elo, Elo.Min, _limitStrengthConfig.UndervaluePawnsMaxElo);
        Config.EgPawnMaterial = Formula.GetLinearlyInterpolatedValue(50, _defaultConfig.EgPawnMaterial, elo, Elo.Min, _limitStrengthConfig.UndervaluePawnsMaxElo);

        // Value knight and bishop equally.
        if (_defaultConfig.MgBishopMaterial > _defaultConfig.MgKnightMaterial)
        {
            // Bishop worth more than knight in middlegame.
            Config.MgBishopMaterial = Formula.GetLinearlyInterpolatedValue(_defaultConfig.MgKnightMaterial, _defaultConfig.MgBishopMaterial, elo, Elo.Min, _limitStrengthConfig.ValueKnightBishopEquallyMaxElo);
        }
        else
        {
            // Knight worth more than bishop in middlegame.
            Config.MgKnightMaterial = Formula.GetLinearlyInterpolatedValue(_defaultConfig.MgBishopMaterial, _defaultConfig.MgKnightMaterial, elo, Elo.Min, _limitStrengthConfig.ValueKnightBishopEquallyMaxElo);
        }

        if (_defaultConfig.EgBishopMaterial > _defaultConfig.EgKnightMaterial)
        {
            // Bishop worth more than knight in endgame.
            Config.EgBishopMaterial = Formula.GetLinearlyInterpolatedValue(_defaultConfig.EgKnightMaterial, _defaultConfig.EgBishopMaterial, elo, Elo.Min, _limitStrengthConfig.ValueKnightBishopEquallyMaxElo);
        }
        else
        {
            // Knight worth more than bishop in endgame.
            Config.EgKnightMaterial = Formula.GetLinearlyInterpolatedValue(_defaultConfig.EgBishopMaterial, _defaultConfig.EgKnightMaterial, elo, Elo.Min, _limitStrengthConfig.ValueKnightBishopEquallyMaxElo);
        }

        // Undervalue rook and overvalue queen.
        Config.MgRookMaterial = Formula.GetLinearlyInterpolatedValue((_defaultConfig.MgRookMaterial * 2) / 3, _defaultConfig.MgRookMaterial, elo, Elo.Min, _limitStrengthConfig.UndervalueRookOvervalueQueenMaxElo);
        Config.EgRookMaterial = Formula.GetLinearlyInterpolatedValue((_defaultConfig.EgRookMaterial * 2) / 3, _defaultConfig.EgRookMaterial, elo, Elo.Min, _limitStrengthConfig.UndervalueRookOvervalueQueenMaxElo);
        var queenMaterialAccuracyPer128 = Formula.GetLinearlyInterpolatedValue(0, 128, elo, Elo.Min, _limitStrengthConfig.UndervalueRookOvervalueQueenMaxElo);
        Config.MgQueenMaterial = _defaultConfig.MgQueenMaterial + ((128 - queenMaterialAccuracyPer128) * _defaultConfig.MgQueenMaterial) / (128 * 3);
        Config.EgQueenMaterial = _defaultConfig.EgQueenMaterial + ((128 - queenMaterialAccuracyPer128) * _defaultConfig.EgQueenMaterial) / (128 * 3);

        // Misjudge danger of passed pawns.
        Config.LsPassedPawnsPer128 = Formula.GetLinearlyInterpolatedValue(0, 128, elo, Elo.Min, _limitStrengthConfig.MisjudgePassedPawnsMaxElo);

        // Inattentive to defense of king.
        Config.LsKingSafetyPer128 = Formula.GetLinearlyInterpolatedValue(0, 128, elo, Elo.Min, _limitStrengthConfig.InattentiveKingDefenseMaxElo);

        // Misplace pieces.
        Config.LsPieceLocationPer128 = Formula.GetLinearlyInterpolatedValue(0, 128, elo, Elo.Min, _limitStrengthConfig.MisplacePiecesMaxElo);

        // Underestimate attacking potential of mobile pieces.
        Config.LsPieceMobilityPer128 = Formula.GetLinearlyInterpolatedValue(0, 128, elo, Elo.Min, _limitStrengthConfig.UnderestimateMobilePiecesMaxElo);

        // Allow pawn structure to be damaged.
        Config.LsPawnStructurePer128 = Formula.GetLinearlyInterpolatedValue(0, 128, elo, Elo.Min, _limitStrengthConfig.AllowPawnStructureDamageMaxElo);

        // Underestimate threats (lesser-value pieces attacking greater-value pieces).
        Config.LsThreatsPer128 = Formula.GetLinearlyInterpolatedValue(0, 128, elo, Elo.Min, _limitStrengthConfig.UnderestimateThreatsMaxElo);

        // Poor maneuvering of minor pieces.
        Config.LsMinorPiecesPer128 = Formula.GetLinearlyInterpolatedValue(0, 128, elo, Elo.Min, _limitStrengthConfig.PoorManeuveringMinorPiecesMaxElo);

        // Poor maneuvering of major pieces.
        Config.LsMajorPiecesPer128 = Formula.GetLinearlyInterpolatedValue(0, 128, elo, Elo.Min, _limitStrengthConfig.PoorManeuveringMajorPiecesMaxElo);

        if (_messenger.Debug)
        {
            _messenger.WriteLine($"info string {nameof(Config.MgPawnMaterial)} = {Config.MgPawnMaterial}");
            _messenger.WriteLine($"info string {nameof(Config.EgPawnMaterial)} = {Config.EgPawnMaterial}");

            _messenger.WriteLine($"info string {nameof(Config.MgKnightMaterial)} = {Config.MgKnightMaterial}");
            _messenger.WriteLine($"info string {nameof(Config.EgKnightMaterial)} = {Config.EgKnightMaterial}");

            _messenger.WriteLine($"info string {nameof(Config.MgBishopMaterial)} = {Config.MgBishopMaterial}");
            _messenger.WriteLine($"info string {nameof(Config.EgBishopMaterial)} = {Config.EgBishopMaterial}");

            _messenger.WriteLine($"info string {nameof(Config.MgRookMaterial)} = {Config.MgRookMaterial}");
            _messenger.WriteLine($"info string {nameof(Config.EgRookMaterial)} = {Config.EgRookMaterial}");

            _messenger.WriteLine($"info string {nameof(Config.MgQueenMaterial)} = {Config.MgQueenMaterial}");
            _messenger.WriteLine($"info string {nameof(Config.EgQueenMaterial)} = {Config.EgQueenMaterial}");

            _messenger.WriteLine($"info string {nameof(Config.LsPassedPawnsPer128)} = {Config.LsPassedPawnsPer128}");
            _messenger.WriteLine($"info string {nameof(Config.LsKingSafetyPer128)} = {Config.LsKingSafetyPer128}");
            _messenger.WriteLine($"info string {nameof(Config.LsPieceLocationPer128)} = {Config.LsPieceLocationPer128}");
            _messenger.WriteLine($"info string {nameof(Config.LsPieceMobilityPer128)} = {Config.LsPieceMobilityPer128}");
            _messenger.WriteLine($"info string {nameof(Config.LsPawnStructurePer128)} = {Config.LsPawnStructurePer128}");
            _messenger.WriteLine($"info string {nameof(Config.LsThreatsPer128)} = {Config.LsThreatsPer128}");
            _messenger.WriteLine($"info string {nameof(Config.LsMinorPiecesPer128)} = {Config.LsMinorPiecesPer128}");
            _messenger.WriteLine($"info string {nameof(Config.LsMajorPiecesPer128)} = {Config.LsMajorPiecesPer128}");
        }
    }


    public void ConfigureFullStrength() => Config.Set(_defaultConfig);


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public (bool TerminalDraw, bool RepeatPosition) IsTerminalDraw(Position position)
    {
        // Only return true if position is drawn and no sequence of moves can make game winnable.
        if (position.PlySinceCaptureOrPawnMove >= Search.MaxPlyWithoutCaptureOrPawnMove) return (true, false); // Draw by 50 moves (100 ply) without a capture or pawn move.

        // Determine if insufficient material remains for checkmate.
        if (Bitwise.CountSetBits(position.GetPawns(Color.White) | position.GetPawns(Color.Black)) == 0)
        {
            // Neither side has any pawns.
            if (Bitwise.CountSetBits(position.GetMajorPieces(Color.White) | position.GetMajorPieces(Color.Black)) == 0)
            {
                // Neither side has any major pieces.
                if ((Bitwise.CountSetBits(position.GetMinorPieces(Color.White)) <= 1) && (Bitwise.CountSetBits(position.GetMinorPieces(Color.Black)) <= 1))
                {
                    // Each side has one or zero minor pieces.  Draw by insufficient checkmating material.
                    return (true, false);
                }
            }
        }

        return position.IsRepeatPosition(DrawMoves) ? (true, true) : (false, false);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPieceMaterialScore(ColorlessPiece colorlessPiece, int phase) => StaticScore.GetTaperedScore(_mgMaterialScores[(int)colorlessPiece], _egMaterialScores[(int)colorlessPiece], phase);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPieceLocationImprovement(ulong move, int phase)
    {
        var piece = Move.Piece(move);
        var color = PieceHelper.GetColor(piece);
        var colorlessPiece = PieceHelper.GetColorlessPiece(piece);

        var fromSquare = Move.From(move);
        var fromSquareWhitePerspective = Board.GetSquareFromWhitePerspective(fromSquare, color);
        var toSquare = Move.To(move);
        var toSquareWhitePerspective = Board.GetSquareFromWhitePerspective(toSquare, color);
        
        var mgImprovement = _mgPieceLocations[(int)colorlessPiece][(int)toSquareWhitePerspective] - _mgPieceLocations[(int)colorlessPiece][(int)fromSquareWhitePerspective];
        var egImprovement = _egPieceLocations[(int)colorlessPiece][(int)toSquareWhitePerspective] - _egPieceLocations[(int)colorlessPiece][(int)fromSquareWhitePerspective];

        return StaticScore.GetTaperedScore(mgImprovement, egImprovement, phase);
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public (int StaticScore, bool DrawnEndgame, int Phase) GetStaticScore(Position position)
    {
        // TODO: Evaluate space.  See https://www.chess.com/forum/view/general/gm-larry-evans-method-of-static-analysis#comment-31672572.
        // TODO: Evaluate rooks on open files.
        Debug.Assert(!position.KingInCheck);

        _stats.Evaluations++;
        Reset();

        _staticScore.PlySinceCaptureOrPawnMove = position.PlySinceCaptureOrPawnMove;
        var phase = DetermineGamePhase(position);

        // Determine if position is a pawnless draw or simple endgame.
        if (IsPawnlessDraw(position, Color.White) || IsPawnlessDraw(position, Color.Black)) return (0, true, phase);
        if (EvaluateSimpleEndgame(position, Color.White) || EvaluateSimpleEndgame(position, Color.Black))
        {
            // Simple Endgame
            var staticScore = _staticScore.GetEg(position.ColorToMove) - _staticScore.GetEg(position.ColorLastMoved);
            var drawnEndgame = staticScore == 0;
            return (staticScore, drawnEndgame, phase);
        }

        // Position is not a pawnless draw or simple endgame.
        // Explicit array lookups are faster than looping through colors.
        EvaluateMaterial(position, Color.White);
        EvaluateMaterial(position, Color.Black);

        EvaluatePieceLocation(position, Color.White);
        EvaluatePieceLocation(position, Color.Black);

        EvaluatePawns(position, Color.White);
        EvaluatePawns(position, Color.Black);

        EvaluateMobilityKingSafetyThreats(position, Color.White);
        EvaluateMobilityKingSafetyThreats(position, Color.Black);

        EvaluateMinorPieces(position, Color.White);
        EvaluateMinorPieces(position, Color.Black);

        EvaluateMajorPieces(position, Color.White);
        EvaluateMajorPieces(position, Color.Black);

        // Limit strength if necessary and return total score.
        if (Config.LimitedStrength) LimitStrength();

        return (_staticScore.GetTotalScore(position.ColorToMove, phase), false, phase);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DetermineGamePhase(Position position)
    {
        var phase = (Bitwise.CountSetBits(position.GetPieces(ColorlessPiece.Knight)) * _knightPhaseWeight) +
                    (Bitwise.CountSetBits(position.GetPieces(ColorlessPiece.Bishop)) * _bishopPhaseWeight) +
                    (Bitwise.CountSetBits(position.GetPieces(ColorlessPiece.Rook)) * _rookPhaseWeight) +
                    (Bitwise.CountSetBits(position.GetPieces(ColorlessPiece.Queen)) * _queenPhaseWeight);

        return FastMath.Min(phase, MiddlegamePhase);
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static bool IsPawnlessDraw(Position position, Color color)
    {
        var enemyColor = 1 - color;
        var pawnCount = Bitwise.CountSetBits(position.GetPawns(color));
        var enemyPawnCount = Bitwise.CountSetBits(position.GetPawns(enemyColor));
        if ((pawnCount + enemyPawnCount) > 0) return false;

        // Consider material combinations excluding pawns.
        var knightCount = Bitwise.CountSetBits(position.GetKnights(color));
        var bishopCount = Bitwise.CountSetBits(position.GetBishops(color));
        var rookCount = Bitwise.CountSetBits(position.GetRooks(color));
        var queenCount = Bitwise.CountSetBits(position.GetQueens(color));

        var minorPieceCount = knightCount + bishopCount;
        var majorPieceCount = rookCount + queenCount;

        var enemyKnightCount = Bitwise.CountSetBits(position.GetKnights(enemyColor));
        var enemyBishopCount = Bitwise.CountSetBits(position.GetBishops(enemyColor));
        var enemyRookCount = Bitwise.CountSetBits(position.GetRooks(enemyColor));
        var enemyQueenCount = Bitwise.CountSetBits(position.GetQueens(enemyColor));

        var enemyMinorPieceCount = enemyKnightCount + enemyBishopCount;
        var enemyMajorPieceCount = enemyRookCount + enemyQueenCount;

        var totalMajorPieces = majorPieceCount + enemyMajorPieceCount;

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

                if ((rookCount == 1) && (minorPieceCount == 0) && (enemyMinorPieceCount == 1)) return true; // R vrs Minor
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
    private bool EvaluateSimpleEndgame(Position position, Color color)
    {
        // TODO: Add detection of unwinnable KBPk endgame, where enemy king prevents pawn from promoting, and bishop is wrong color.

        var enemyColor = 1 - color;
        var pawnCount = Bitwise.CountSetBits(position.GetPawns(color));
        var enemyPawnCount = Bitwise.CountSetBits(position.GetPawns(enemyColor));

        var minorPieceCount = Bitwise.CountSetBits(position.GetMinorPieces(color));
        var majorPieceCount = Bitwise.CountSetBits(position.GetMajorPieces(color));

        var pawnsAndPiecesCount = pawnCount + minorPieceCount + majorPieceCount;

        var enemyKnightCount = Bitwise.CountSetBits(position.GetKnights(enemyColor));

        var enemyBishops = position.GetBishops(enemyColor);
        var enemyBishopCount = Bitwise.CountSetBits(enemyBishops);

        var enemyMinorPieceCount = enemyKnightCount + enemyBishopCount;
        var enemyMajorPieceCount = Bitwise.CountSetBits(position.GetMajorPieces(enemyColor));

        var enemyPawnsAndPiecesCount = enemyPawnCount + enemyMinorPieceCount + enemyMajorPieceCount;

        var kingSquare = Bitwise.FirstSetSquare(position.GetKing(color));
        var enemyKingSquare = Bitwise.FirstSetSquare(position.GetKing(enemyColor));

        switch (pawnsAndPiecesCount)
        {
            // Case 0 = Lone King
            case 0 when (enemyPawnCount == 1) && (enemyPawnsAndPiecesCount == 1):
                // K vrs KP
                EvaluateKingVersusPawn(position, enemyColor);
                return true;

            // TODO: Implement Bahr's rule to evaluate a blocked rook pawn and passed pawn.
            // See https://macechess.blogspot.com/2013/08/bahrs-rule.html and http://www.fraserheightschess.com/Documents/Bahrs_Rule.pdf.

            case 0 when (enemyPawnCount == 0) && (enemyBishopCount == 1) && (enemyKnightCount == 1) && (enemyMajorPieceCount == 0):
                // K vrs KBN
                // Push lone king to corner with same color square as occupied by bishop.  Push winning king close to lone king.
                var enemyBishopSquareColor = (Board.SquareColors[(int)Color.White] & enemyBishops) > 0 ? Color.White : Color.Black;
                var distanceToCorrectColorCorner = Board.DistanceToNearestCornerOfColor[(int)enemyBishopSquareColor][(int)kingSquare];
                // Penalize K vrs KBN so engine, with a knight, prefers to promote pawn to Q for K vrs KQN instead of promoting pawn to B for K vrs KBN.
                _staticScore.EgSimple[(int)enemyColor] = SpecialScore.SimpleEndgame - distanceToCorrectColorCorner -
                                                         Board.SquareDistances[(int)kingSquare][(int)enemyKingSquare] - _egKbnPenalty;
                return true;

            case 0 when enemyMajorPieceCount > 0:
                // K vrs K + Major Piece
                // Push lone king to corner.  Push winning king close to lone king.
                // Multiply king distances by a factor to overcome piece location values.
                EvaluatePawns(position, enemyColor); // Incentivize engine to promote its passed pawns.
                _staticScore.EgSimple[(int)enemyColor] = SpecialScore.SimpleEndgame - (_egKingCornerFactor * (Board.DistanceToNearestCorner[(int)kingSquare] + Board.SquareDistances[(int)kingSquare][(int)enemyKingSquare]));
                return true;
        }

        // Use regular evaluation.
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
            }
        }

        else
        {
            // Pawn is not on rook file.
            var kingPawnRankDifference = winningKingRank - pawnRank;
            var kingPawnAbsoluteFileDifference = FastMath.Abs(winningKingFile - pawnFile);

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
                _staticScore.EgSimple[(int)lonePawnColor] = SpecialScore.SimpleEndgame + pawnRank;
            }
        }

        // Pawn does not promote.
        // Game is drawn.
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void EvaluateMaterial(Position position, Color color)
    {
        // Explicit piece evaluation is faster than looping through pieces due to avoiding CPU stalls and enabling out-of-order execution.
        // See https://stackoverflow.com/a/2349265/8992299.

        // Pawns
        var pawn = PieceHelper.GetPieceOfColor(ColorlessPiece.Pawn, color);
        var pawnCount = Bitwise.CountSetBits(position.PieceBitboards[(int)pawn]);
        _staticScore.MgPawnMaterial[(int)color] = pawnCount * _mgMaterialScores[(int)ColorlessPiece.Pawn];
        _staticScore.EgPawnMaterial[(int)color] = pawnCount * _egMaterialScores[(int)ColorlessPiece.Pawn];

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


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void EvaluatePieceLocation(Position position, Color color)
    {
        for (var colorlessPiece = ColorlessPiece.Pawn; colorlessPiece <= ColorlessPiece.King; colorlessPiece++)
        {
            var piece = PieceHelper.GetPieceOfColor(colorlessPiece, color);
            var pieces = position.PieceBitboards[(int)piece];

            Square square;
            while ((square = Bitwise.PopFirstSetSquare(ref pieces)) != Square.Illegal)
            {
                var squareFromWhitePerspective = Board.GetSquareFromWhitePerspective(square, color);
                _staticScore.MgPieceLocation[(int)color] += _mgPieceLocations[(int)colorlessPiece][(int)squareFromWhitePerspective];
                _staticScore.EgPieceLocation[(int)color] += _egPieceLocations[(int)colorlessPiece][(int)squareFromWhitePerspective];
            }
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void EvaluatePawns(Position position, Color color)
    {
        var pawns = position.GetPawns(color);
        if (pawns == 0) return;

        var enemyColor = 1 - color;

        var kingSquare = Bitwise.FirstSetSquare(position.GetKing(color));

        var enemyKingSquare = Bitwise.FirstSetSquare(position.GetKing(enemyColor));
        var enemyMinorPieces = position.GetMinorPieces(enemyColor);
        var enemyMajorPieces = position.GetMajorPieces(enemyColor);

        Square square;
        while ((square = Bitwise.PopFirstSetSquare(ref pawns)) != Square.Illegal)
        {
            var file = Board.Files[(int)square];

            // Evaluate pawn structure.
            var pawnsOccupyLeftFile = (file > 0) && ((Board.FileMasks[file - 1] & pawns) > 0);
            var pawnsOccupyRightFile = (file < 7) && ((Board.FileMasks[file + 1] & pawns) > 0);

            if (!pawnsOccupyLeftFile && !pawnsOccupyRightFile)
            {
                // Pawn is isolated.
                _staticScore.MgPawnStructure[(int)color] -= Config.MgIsolatedPawn;
                _staticScore.EgPawnStructure[(int)color] -= Config.EgIsolatedPawn;
            }

            // Evaluate passed pawns.
            if (IsPassedPawn(position, square, color))
            {
                _passedPawns[(int)color] |= Board.SquareMasks[(int)square];
                _staticScore.EgKingEscortedPassedPawns[(int)color] += (Board.SquareDistances[(int)square][(int)enemyKingSquare] - Board.SquareDistances[(int)square][(int)kingSquare]) * Config.EgKingEscortedPassedPawn;

                var rank = Board.Ranks[(int)color][(int)square];

                if (IsFreePawn(position, square, color))
                {
                    // Pawn is free to advance.
                    if (IsUnstoppablePawn(position, square, enemyKingSquare, color)) _staticScore.UnstoppablePassedPawns[(int)color] += Config.UnstoppablePassedPawn; // Pawn is unstoppable.
                    else _staticScore.EgFreePassedPawns[(int)color] += _egFreePassedPawns[rank]; // Pawn is passed and free.
                }
                else
                {
                    // Pawn is passed.
                    _staticScore.MgPassedPawns[(int)color] += _mgPassedPawns[rank];
                    _staticScore.EgPassedPawns[(int)color] += _egPassedPawns[rank];
                }
            }

            // Evaluate threats.
            var pawnAttacks = Board.PawnAttackMasks[(int)color][(int)square];
            var minorPiecesAttacked = Bitwise.CountSetBits(pawnAttacks & enemyMinorPieces);
            var majorPiecesAttacked = Bitwise.CountSetBits(pawnAttacks & enemyMajorPieces);

            _staticScore.MgThreats[(int)color] += minorPiecesAttacked * Config.MgPawnThreatenMinor;
            _staticScore.EgThreats[(int)color] += minorPiecesAttacked * Config.EgPawnThreatenMinor;
            _staticScore.MgThreats[(int)color] += majorPiecesAttacked * Config.MgPawnThreatenMajor;
            _staticScore.EgThreats[(int)color] += majorPiecesAttacked * Config.EgPawnThreatenMajor;
        }

        // Evaluate doubled pawns.
        for (var file = 0; file < 8; file++)
        {
            var pawnCount = Bitwise.CountSetBits(Board.FileMasks[file] & pawns);
            if (pawnCount > 1)
            {
                // File has double (or more) pawns.
                var extraPawnCount = pawnCount - 1;
                _staticScore.MgPawnStructure[(int)color] -= extraPawnCount * Config.MgDoubledPawn;
                _staticScore.EgPawnStructure[(int)color] -= extraPawnCount * Config.EgDoubledPawn;
            }
        }

        // Evaluate connected passed pawns.
        var allPassedPawns = _passedPawns[(int)color];
        var passedPawns = allPassedPawns; // Store into second variable because while loop will pop all pawn squares.
        while ((square = Bitwise.PopFirstSetSquare(ref passedPawns)) != Square.Illegal)
        {
            var rank = Board.Ranks[(int)color][(int)square];
            var file = Board.Files[(int)square];
            if ((file > 0) && ((Board.FileMasks[file - 1] & allPassedPawns) > 0)) _staticScore.EgConnectedPassedPawns[(int)color] += _egConnectedPassedPawns[rank]; // File Left of Passed Pawn
            if ((file < 7) && ((Board.FileMasks[file + 1] & allPassedPawns) > 0)) _staticScore.EgConnectedPassedPawns[(int)color] += _egConnectedPassedPawns[rank]; // File Right of Passed Pawn
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsUnstoppablePawn(Position position, Square pawnSquare, Square enemyKingSquare, Color color)
    {
        var enemyColor = 1 - color;

        var enemyPieceCount = Bitwise.CountSetBits(position.GetMajorAndMinorPieces(enemyColor));
        if (enemyPieceCount > 0) return false;

        // Enemy has no minor or major pieces.
        var file = Board.Files[(int)pawnSquare];

        var rankOfPromotionSquare = (int)Piece.BlackPawn - ((int)color * (int)Piece.BlackPawn);
        var promotionSquare = Board.GetSquare(file, rankOfPromotionSquare);

        var pawnDistanceToPromotionSquare = Board.SquareDistances[(int)pawnSquare][(int)promotionSquare];
        var kingDistanceToPromotionSquare = Board.SquareDistances[(int)enemyKingSquare][(int)promotionSquare];

        if (color != position.ColorToMove) kingDistanceToPromotionSquare--; // Enemy king can move one square closer to pawn.

        return kingDistanceToPromotionSquare > pawnDistanceToPromotionSquare; // Enemy king cannot stop pawn from promoting.
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void EvaluateMobilityKingSafetyThreats(Position position, Color color)
    {
        var enemyColor = 1 - color;
        var enemyKingSquare = Bitwise.FirstSetSquare(position.GetKing(enemyColor));
        var enemyKingOuterRing = Board.OuterRingMasks[(int)enemyKingSquare];
        var enemyKingInnerRing = Board.InnerRingMasks[(int)enemyKingSquare];
        var enemyKingFile = Board.Files[(int)enemyKingSquare];

        var enemyMajorPieces = position.GetMajorPieces(enemyColor);
        var enemyPawns = position.GetPawns(enemyColor);
        var unOrEnemyOccupiedSquares = ~position.ColorOccupancy[(int)color];

        var mgThreatsToEnemyKingSafety = 0;

        // Determine safe squares (not attacked by enemy pawns).
        Square square;
        var squaresAttackedByEnemyPawns = 0ul;
        while ((square = Bitwise.PopFirstSetSquare(ref enemyPawns)) != Square.Illegal)
            squaresAttackedByEnemyPawns |= Board.PawnAttackMasks[(int)enemyColor][(int)square];
        var safeSquares = ~squaresAttackedByEnemyPawns;
        
        enemyPawns = position.GetPawns(enemyColor); // Repopulate after above while loop popped all enemy pawn squares.

        // Evaluate mobility of individual pieces.
        for (var colorlessPiece = ColorlessPiece.Knight; colorlessPiece <= ColorlessPiece.Queen; colorlessPiece++)
        {
            var piece = PieceHelper.GetPieceOfColor(colorlessPiece, color);
            var pieces = position.PieceBitboards[(int)piece];

            var getPieceMovesMask = Board.PieceMoveMaskDelegates[(int)colorlessPiece];
            var getPieceXrayMovesMask = Board.PieceXrayMoveMaskDelegates[(int)colorlessPiece];

            while ((square = Bitwise.PopFirstSetSquare(ref pieces)) != Square.Illegal)
            {
                // Evaluate piece proximity to enemy king.
                var distanceToEnemyKing = Board.SquareDistances[(int)square][(int)enemyKingSquare];
                mgThreatsToEnemyKingSafety += (7 - distanceToEnemyKing) * _mgKingSafetyPieceProximityWeights[(int)colorlessPiece];

                // Evaluate piece mobility using safe moves.
                var pieceMovesMask = getPieceMovesMask(square, position.Occupancy);
                var moves = pieceMovesMask & unOrEnemyOccupiedSquares & safeSquares;
                var (mgPieceMobilityScore, egPieceMobilityScore) = GetPieceMobilityScore(moves, _mgPieceMobility[(int)colorlessPiece], _egPieceMobility[(int)colorlessPiece]);
                _staticScore.MgPieceMobility[(int)color] += mgPieceMobilityScore;
                _staticScore.EgPieceMobility[(int)color] += egPieceMobilityScore;

                // Evaluate king safety using safe xray moves.
                var pieceXrayMovesMask = getPieceXrayMovesMask(square, color, position);
                moves = pieceXrayMovesMask & unOrEnemyOccupiedSquares & safeSquares;
                var outerRingAttackWeight = _mgKingSafetyAttackWeights[(int)colorlessPiece][(int)KingRing.Outer];
                var innerRingAttackWeight = _mgKingSafetyAttackWeights[(int)colorlessPiece][(int)KingRing.Inner];
                mgThreatsToEnemyKingSafety += GetKingSafetyIndexIncrement(moves, enemyKingOuterRing, enemyKingInnerRing, outerRingAttackWeight, innerRingAttackWeight);

                if (colorlessPiece < ColorlessPiece.Rook)
                {
                    // Evaluate threats using safe moves.
                    moves = pieceMovesMask & enemyMajorPieces & safeSquares;
                    var majorPiecesAttacked = Bitwise.CountSetBits(moves);
                    _staticScore.MgThreats[(int)color] += majorPiecesAttacked * Config.MgMinorThreatenMajor;
                    _staticScore.EgThreats[(int)color] += majorPiecesAttacked * Config.EgMinorThreatenMajor;
                }
            }
        }

        // Evaluate enemy king near semi-open files.
        var semiOpenFilesNearEnemyKing = 0;
        if ((enemyKingFile > 0) && ((Board.FileMasks[enemyKingFile - 1] & enemyPawns) == 0)) semiOpenFilesNearEnemyKing++; // File Left of Enemy King
        if ((Board.FileMasks[enemyKingFile] & enemyPawns) == 0) semiOpenFilesNearEnemyKing++; // Enemy King File
        if ((enemyKingFile < 7) && ((Board.FileMasks[enemyKingFile + 1] & enemyPawns) == 0)) semiOpenFilesNearEnemyKing++; // File Right of Enemy King
        mgThreatsToEnemyKingSafety += semiOpenFilesNearEnemyKing * Config.MgKingSafetySemiOpenFilePer8;

        // Evaluate enemy king pawn shield.
        var pawnsMissingFromShield = 3 - Bitwise.CountSetBits(enemyPawns & Board.PawnShieldMasks[(int)enemyColor][(int)enemyKingSquare]);
        mgThreatsToEnemyKingSafety += pawnsMissingFromShield * Config.MgKingSafetyPawnShieldPer8;

        // Evaluate average distance of defending pieces from enemy king.
        var defendingPieces = position.GetMajorAndMinorPieces(enemyColor);
        var defendingPieceCount = Bitwise.CountSetBits(defendingPieces);
        if (defendingPieceCount > 0)
        {
            var defendingPieceDistance = 0;
            while ((square = Bitwise.PopFirstSetSquare(ref defendingPieces)) != Square.Illegal)
            {
                defendingPieceDistance += Board.SquareDistances[(int)square][(int)enemyKingSquare];
            }
            mgThreatsToEnemyKingSafety += (defendingPieceDistance * Config.MgKingSafetyDefendingPiecesPer8) / defendingPieceCount;
        }

        // Lookup king safety score in array.
        var maxIndex = _mgKingSafety.Length - 1;
        _staticScore.MgKingSafety[(int)enemyColor] = -_mgKingSafety[FastMath.Min(mgThreatsToEnemyKingSafety / 8, maxIndex)]; // Negative value because more threats == less safety.
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int MiddlegameMobility, int EndgameMobility) GetPieceMobilityScore(ulong pieceDestinations, int[] mgPieceMobility, int[] egPieceMobility)
    {
        var moves = Bitwise.CountSetBits(pieceDestinations);
        var mgMoveIndex = FastMath.Min(moves, mgPieceMobility.Length - 1);
        var egMoveIndex = FastMath.Min(moves, egPieceMobility.Length - 1);
        return (mgPieceMobility[mgMoveIndex], egPieceMobility[egMoveIndex]);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetKingSafetyIndexIncrement(ulong pieceDestinations, ulong kingOuterRing, ulong kingInnerRing, int outerRingAttackWeight, int innerRingAttackWeight)
    {
        var attackedOuterRingSquares = Bitwise.CountSetBits(pieceDestinations & kingOuterRing);
        var attackedInnerRingSquares = Bitwise.CountSetBits(pieceDestinations & kingInnerRing);
        return (attackedOuterRingSquares * outerRingAttackWeight) + (attackedInnerRingSquares * innerRingAttackWeight);
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void EvaluateMinorPieces(Position position, Color color)
    {
        var enemyColor = 1 - color;
        var enemyPawns = position.GetPawns(enemyColor);

        var knights = position.GetKnights(color);
        var bishops = position.GetBishops(color);
        var pawns = position.GetPawns(color);
        
        // Bishop Pair
        var bishopOnWhiteSquare = (bishops & Board.SquareColors[(int)Color.White]) > 0;
        var bishopOnBlackSquare = (bishops & Board.SquareColors[(int)Color.Black]) > 0;
        if (bishopOnWhiteSquare && bishopOnBlackSquare)
        {
            _staticScore.MgBishopPair[(int)color] += Config.MgBishopPair;
            _staticScore.EgBishopPair[(int)color] += Config.EgBishopPair;
        }

        // Outposts
        ulong supportingPawnsMask;
        ulong potentialAttackMask;
        Square square;
        var outpostMask = Board.OutpostMasks[(int)color];

        // Knight Outposts
        while ((square = Bitwise.PopFirstSetSquare(ref knights)) != Square.Illegal)
        {
            if ((Board.SquareMasks[(int)square] & outpostMask) == 0) continue; // Knight is not in enemy outpost territory.
            supportingPawnsMask = Board.PawnAttackMasks[(int)enemyColor][(int)square]; // Attacked by white pawn masks = black pawn attack masks and vice-versa.
            if ((pawns & supportingPawnsMask) == 0) continue; // Knight is unsupported by own pawns.
            potentialAttackMask = Board.PassedPawnMasks[(int)color][(int)square] & ~Board.FreePawnMasks[(int)color][(int)square];
            if ((enemyPawns & potentialAttackMask) > 0) continue; // Knight can be attacked by enemy pawns.
            
            // Knight is positioned safely in enemy territory on an outpost square.
            _staticScore.MgOutposts[(int)color] += Config.MgKnightOutpost;
            _staticScore.EgOutposts[(int)color] += Config.EgKnightOutpost;
        }

        // Bishop Outposts
        while ((square = Bitwise.PopFirstSetSquare(ref bishops)) != Square.Illegal)
        {
            if ((Board.SquareMasks[(int)square] & outpostMask) == 0) continue; // Bishop is not in enemy outpost territory.
            supportingPawnsMask = Board.PawnAttackMasks[(int)enemyColor][(int)square]; // Attacked by white pawn masks = black pawn attack masks and vice-versa.
            if ((pawns & supportingPawnsMask) == 0) continue; // Bishop is unsupported by own pawns.
            potentialAttackMask = Board.PassedPawnMasks[(int)color][(int)square] & ~Board.FreePawnMasks[(int)color][(int)square];
            if ((enemyPawns & potentialAttackMask) > 0) continue; // Bishop can be attacked by enemy pawns.
            
            // Bishop is positioned safely in enemy territory on an outpost square.
            _staticScore.MgOutposts[(int)color] += Config.MgBishopOutpost;
            _staticScore.EgOutposts[(int)color] += Config.EgBishopOutpost;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void EvaluateMajorPieces(Position position, Color color)
    {
        var enemyColor = 1 - color;
        var enemyKingSquare = Bitwise.FirstSetSquare(position.GetKing(enemyColor));

        if (Board.Ranks[(int)enemyColor][(int)enemyKingSquare] != 0) return; // Enemy king is not on back rank.

        // Rooks on 7th Rank
        var rooks = position.GetRooks(color);
        Square square;

        while ((square = Bitwise.PopFirstSetSquare(ref rooks)) != Square.Illegal)
        {
            if (Board.Ranks[(int)color][(int)square] == 6)
            {
                _staticScore.MgRookOn7thRank[(int)color] += Config.MgRook7thRank;
                _staticScore.EgRookOn7thRank[(int)color] += Config.EgRook7thRank;
            }
        }
    }


    private void LimitStrength()
    {
        for (var color = Color.White; color <= Color.Black; color++)
        {
            // Limit understanding of passed pawns.
            _staticScore.MgPassedPawns[(int)color] = (_staticScore.MgPassedPawns[(int)color] * Config.LsPassedPawnsPer128) / 128;
            _staticScore.EgPassedPawns[(int)color] = (_staticScore.EgPassedPawns[(int)color] * Config.LsPassedPawnsPer128) / 128;
            _staticScore.EgFreePassedPawns[(int)color] = (_staticScore.EgFreePassedPawns[(int)color] * Config.LsPassedPawnsPer128) / 128;
            _staticScore.EgKingEscortedPassedPawns[(int)color] = (_staticScore.EgKingEscortedPassedPawns[(int)color] * Config.LsPassedPawnsPer128) / 128;
            _staticScore.UnstoppablePassedPawns[(int)color] = (_staticScore.UnstoppablePassedPawns[(int)color] * Config.LsPassedPawnsPer128) / 128;

            // Limit understanding of king safety.
            _staticScore.MgKingSafety[(int)color] = (_staticScore.MgKingSafety[(int)color] * Config.LsKingSafetyPer128) / 128;

            // Limit understanding of piece location.
            _staticScore.MgPieceLocation[(int)color] = (_staticScore.MgPieceLocation[(int)color] * Config.LsPieceLocationPer128) / 128;
            _staticScore.EgPieceLocation[(int)color] = (_staticScore.EgPieceLocation[(int)color] * Config.LsPieceLocationPer128) / 128;

            // Limit understanding of piece mobility.
            _staticScore.MgPieceMobility[(int)color] = (_staticScore.MgPieceMobility[(int)color] * Config.LsPieceMobilityPer128) / 128;
            _staticScore.EgPieceMobility[(int)color] = (_staticScore.EgPieceMobility[(int)color] * Config.LsPieceMobilityPer128) / 128;

            // Limit understanding of pawn structure.
            _staticScore.MgPawnStructure[(int)color] = (_staticScore.MgPawnStructure[(int)color] * Config.LsPawnStructurePer128) / 128;
            _staticScore.EgPawnStructure[(int)color] = (_staticScore.EgPawnStructure[(int)color] * Config.LsPawnStructurePer128) / 128;

            // Limit understanding of threats.
            _staticScore.MgThreats[(int)color] = (_staticScore.MgThreats[(int)color] * Config.LsThreatsPer128) / 128;
            _staticScore.EgThreats[(int)color] = (_staticScore.EgThreats[(int)color] * Config.LsThreatsPer128) / 128;

            // Limit understanding of minor pieces.
            _staticScore.MgBishopPair[(int)color] = (_staticScore.MgBishopPair[(int)color] * Config.LsMinorPiecesPer128) / 128;
            _staticScore.EgBishopPair[(int)color] = (_staticScore.EgBishopPair[(int)color] * Config.LsMinorPiecesPer128) / 128;
            _staticScore.MgOutposts[(int)color] = (_staticScore.MgOutposts[(int)color] * Config.LsMinorPiecesPer128) / 128;
            _staticScore.EgOutposts[(int)color] = (_staticScore.EgOutposts[(int)color] * Config.LsMinorPiecesPer128) / 128;

            // Limit understanding of major pieces.
            _staticScore.MgRookOn7thRank[(int)color] = (_staticScore.MgRookOn7thRank[(int)color] * Config.LsMajorPiecesPer128) / 128;
            _staticScore.EgRookOn7thRank[(int)color] = (_staticScore.EgRookOn7thRank[(int)color] * Config.LsMajorPiecesPer128) / 128;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetMatedScore(int depth) => -SpecialScore.Max + depth;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetMatingScore(int depth) => SpecialScore.Max - depth;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetMateMoveCount(int score)
    {
        var plyCount = (score > 0) ? SpecialScore.Max - score : -SpecialScore.Max - score;
        // Convert plies to full moves.
        var (quotient, remainder) = Math.DivRem(plyCount, 2);
        return quotient + remainder;
    }


    public string ShowParameters()
    {
        var stringBuilder = new StringBuilder();

        // Material
        stringBuilder.AppendLine("Material");
        stringBuilder.AppendLine("===========");
        stringBuilder.AppendLine($"MG Pawn:    {Config.MgPawnMaterial}");
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

        stringBuilder.Append("Endgame Connected Passed Pawns:     ");
        ShowParameterArray(_egConnectedPassedPawns, stringBuilder);
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
        stringBuilder.AppendLine($"King Safety KnightAttackOuterRingPer8:  {Config.MgKingSafetyKnightAttackOuterRingPer8:000}");
        stringBuilder.AppendLine($"King Safety KnightAttackInnerRingPer8:  {Config.MgKingSafetyKnightAttackInnerRingPer8:000}");
        stringBuilder.AppendLine($"King Safety BishopAttackOuterRingPer8:  {Config.MgKingSafetyBishopAttackOuterRingPer8:000}");
        stringBuilder.AppendLine($"King Safety BishopAttackInnerRingPer8:  {Config.MgKingSafetyBishopAttackInnerRingPer8:000}");
        stringBuilder.AppendLine($"King Safety RookAttackOuterRingPer8:    {Config.MgKingSafetyRookAttackOuterRingPer8:000}");
        stringBuilder.AppendLine($"King Safety RookAttackInnerRingPer8:    {Config.MgKingSafetyRookAttackInnerRingPer8:000}");
        stringBuilder.AppendLine($"King Safety QueenAttackOuterRingPer8:   {Config.MgKingSafetyQueenAttackOuterRingPer8:000}");
        stringBuilder.AppendLine($"King Safety QueenAttackInnerRingPer8:   {Config.MgKingSafetyQueenAttackInnerRingPer8:000}");
        stringBuilder.AppendLine($"King Safety SemiOpenFilePer8:           {Config.MgKingSafetySemiOpenFilePer8:000}");
        stringBuilder.AppendLine($"King Safety PawnShieldPer8:             {Config.MgKingSafetyPawnShieldPer8:000}");
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
        for (var index = 0; index < parameters.Length; index++)
            stringBuilder.Append(parameters[index].ToString("+000;-000").PadRight(5));
    }


    public string ShowLimitStrengthParameters()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("Evaluation Term                    Knowledge Percent");
        stringBuilder.AppendLine();
        
        stringBuilder.Append("Rating (Elo)                    ");
        for (var index = 0; index < _limitStrengthElos.Length; index++)
        {
            var elo = _limitStrengthElos[index];
            stringBuilder.Append($"{elo,6}");
        }
        stringBuilder.AppendLine();
        stringBuilder.Append('=', 98);
        stringBuilder.AppendLine();

        // Pawn Material Value
        ShowPositionalKnowledgeGain("Pawn Material Value", _limitStrengthConfig.UndervaluePawnsMaxElo, stringBuilder);
        stringBuilder.AppendLine();

        // Knight and Bishop Material Value
        ShowPositionalKnowledgeGain("Knight and Bishop Material Value", _limitStrengthConfig.ValueKnightBishopEquallyMaxElo, stringBuilder);
        stringBuilder.AppendLine();

        // Rook and Queen Material Value
        ShowPositionalKnowledgeGain("Rook and Queen Material Value", _limitStrengthConfig.UndervalueRookOvervalueQueenMaxElo, stringBuilder);
        stringBuilder.AppendLine();

        // Passed Pawns
        ShowPositionalKnowledgeGain("Passed Pawns", _limitStrengthConfig.MisjudgePassedPawnsMaxElo, stringBuilder);
        stringBuilder.AppendLine();

        // King Safety
        ShowPositionalKnowledgeGain("King Safety", _limitStrengthConfig.InattentiveKingDefenseMaxElo, stringBuilder);
        stringBuilder.AppendLine();

        // Piece Location
        ShowPositionalKnowledgeGain("Piece Location", _limitStrengthConfig.MisplacePiecesMaxElo, stringBuilder);
        stringBuilder.AppendLine();

        // Piece Mobility
        ShowPositionalKnowledgeGain("Piece Mobility", _limitStrengthConfig.UnderestimateMobilePiecesMaxElo, stringBuilder);
        stringBuilder.AppendLine();

        // Pawn Structure
        ShowPositionalKnowledgeGain("Pawn Structure", _limitStrengthConfig.AllowPawnStructureDamageMaxElo, stringBuilder);
        stringBuilder.AppendLine();

        // Threats
        ShowPositionalKnowledgeGain("Threats", _limitStrengthConfig.UnderestimateThreatsMaxElo, stringBuilder);
        stringBuilder.AppendLine();

        // Minor Pieces
        ShowPositionalKnowledgeGain("Minor Pieces", _limitStrengthConfig.PoorManeuveringMinorPiecesMaxElo, stringBuilder);
        stringBuilder.AppendLine();

        // Major Pieces
        ShowPositionalKnowledgeGain("Major Pieces", _limitStrengthConfig.PoorManeuveringMajorPiecesMaxElo, stringBuilder);

        return stringBuilder.ToString();
    }


    private void ShowPositionalKnowledgeGain(string evaluationTerm, int maxElo, StringBuilder stringBuilder)
    {
        stringBuilder.Append(evaluationTerm.PadRight(32));

        for (var index = 0; index < _limitStrengthElos.Length; index++)
        {
            var elo = _limitStrengthElos[index];
            var knowledgePercent = Formula.GetLinearlyInterpolatedValue(0, 100, elo, Elo.Min, maxElo);
            stringBuilder.Append($"{knowledgePercent}%".PadLeft(6));
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Reset()
    {
        _staticScore.Reset();
        _passedPawns[(int)Color.White] = 0;
        _passedPawns[(int)Color.Black] = 0;
    }


    public string ToString(Position position)
    {
        var (_, _, phase) = GetStaticScore(position);
        return _staticScore.ToString(phase);
    }
}
