﻿// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Evaluation;
using ErikTheCoder.MadChess.Engine.Intelligence;
using ErikTheCoder.MadChess.Engine.Score;


namespace ErikTheCoder.MadChess.Engine.Tuning;


public sealed class Particle
{
    public readonly PgnGames PgnGames;
    public readonly Parameters Parameters;
    public readonly Parameters BestParameters;

    public double EvaluationError;
    public double BestEvaluationError;

    private const double _maxInitialVelocityFraction = 0.10;
    private const double _inertia = 0.75d;
    private const double _influence = 1.50d;

    private readonly double[] _velocities;


    public Particle(PgnGames pgnGames, Parameters parameters)
    {
        PgnGames = pgnGames;

        Parameters = parameters;
        BestParameters = parameters.DuplicateWithSameValues();

        EvaluationError = double.MaxValue;
        BestEvaluationError = double.MaxValue;

        // Initialize random velocities.
        _velocities = new double[parameters.Count];
        InitializeRandomVelocities();
    }


    private void InitializeRandomVelocities()
    {
        for (var index = 0; index < Parameters.Count; index++)
        {
            var parameter = Parameters[index];
            var maxVelocity = _maxInitialVelocityFraction * (parameter.MaxValue - parameter.MinValue);
            _velocities[index] = (SafeRandom.NextDouble() * maxVelocity * 2) - maxVelocity; // Permit positive or negative velocity.
        }
    }

    public void SetDefaultParameters()
    {
        var evalConfig = new EvalConfig();

        // Material
        Parameters[nameof(evalConfig.EgPawnMaterial)].Value = evalConfig.EgPawnMaterial;
        Parameters[nameof(evalConfig.MgKnightMaterial)].Value = evalConfig.MgKnightMaterial;
        Parameters[nameof(evalConfig.EgKnightMaterial)].Value = evalConfig.EgKnightMaterial;
        Parameters[nameof(evalConfig.MgBishopMaterial)].Value = evalConfig.MgBishopMaterial;
        Parameters[nameof(evalConfig.EgBishopMaterial)].Value = evalConfig.EgBishopMaterial;
        Parameters[nameof(evalConfig.MgRookMaterial)].Value = evalConfig.MgRookMaterial;
        Parameters[nameof(evalConfig.EgRookMaterial)].Value = evalConfig.EgRookMaterial;
        Parameters[nameof(evalConfig.MgQueenMaterial)].Value = evalConfig.MgQueenMaterial;
        Parameters[nameof(evalConfig.EgQueenMaterial)].Value = evalConfig.EgQueenMaterial;

        // Passed Pawns
        Parameters[nameof(evalConfig.PassedPawnPowerPer128)].Value = evalConfig.PassedPawnPowerPer128;
        Parameters[nameof(evalConfig.MgPassedPawnScalePer128)].Value = evalConfig.MgPassedPawnScalePer128;
        Parameters[nameof(evalConfig.EgPassedPawnScalePer128)].Value = evalConfig.EgPassedPawnScalePer128;
        Parameters[nameof(evalConfig.EgFreePassedPawnScalePer128)].Value = evalConfig.EgFreePassedPawnScalePer128;
        Parameters[nameof(evalConfig.EgConnectedPassedPawnScalePer128)].Value = evalConfig.EgConnectedPassedPawnScalePer128;
        Parameters[nameof(evalConfig.EgKingEscortedPassedPawn)].Value = evalConfig.EgKingEscortedPassedPawn;

        // King Safety
        Parameters[nameof(evalConfig.MgKingSafetyPowerPer128)].Value = evalConfig.MgKingSafetyPowerPer128;
        Parameters[nameof(evalConfig.MgKingSafetyScalePer128)].Value = evalConfig.MgKingSafetyScalePer128;
        Parameters[nameof(evalConfig.MgKingSafetyKnightAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyKnightAttackOuterRingPer8;
        Parameters[nameof(evalConfig.MgKingSafetyKnightAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyKnightAttackInnerRingPer8;
        Parameters[nameof(evalConfig.MgKingSafetyBishopAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyBishopAttackOuterRingPer8;
        Parameters[nameof(evalConfig.MgKingSafetyBishopAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyBishopAttackInnerRingPer8;
        Parameters[nameof(evalConfig.MgKingSafetyRookAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyRookAttackOuterRingPer8;
        Parameters[nameof(evalConfig.MgKingSafetyRookAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyRookAttackInnerRingPer8;
        Parameters[nameof(evalConfig.MgKingSafetyQueenAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyQueenAttackOuterRingPer8;
        Parameters[nameof(evalConfig.MgKingSafetyQueenAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyQueenAttackInnerRingPer8;
        Parameters[nameof(evalConfig.MgKingSafetySemiOpenFilePer8)].Value = evalConfig.MgKingSafetySemiOpenFilePer8;
        Parameters[nameof(evalConfig.MgKingSafetyPawnShieldPer8)].Value = evalConfig.MgKingSafetyPawnShieldPer8;
        Parameters[nameof(evalConfig.MgKingSafetyDefendingPiecesPer8)].Value = evalConfig.MgKingSafetyDefendingPiecesPer8;
        // Pawn Location
        Parameters[nameof(evalConfig.MgPawnAdvancement)].Value = evalConfig.MgPawnAdvancement;
        Parameters[nameof(evalConfig.EgPawnAdvancement)].Value = evalConfig.EgPawnAdvancement;
        Parameters[nameof(evalConfig.MgPawnCentrality)].Value = evalConfig.MgPawnCentrality;
        Parameters[nameof(evalConfig.EgPawnCentrality)].Value = evalConfig.EgPawnCentrality;

        // Knight Location
        Parameters[nameof(evalConfig.MgKnightAdvancement)].Value = evalConfig.MgKnightAdvancement;
        Parameters[nameof(evalConfig.EgKnightAdvancement)].Value = evalConfig.EgKnightAdvancement;
        Parameters[nameof(evalConfig.MgKnightCentrality)].Value = evalConfig.MgKnightCentrality;
        Parameters[nameof(evalConfig.EgKnightCentrality)].Value = evalConfig.EgKnightCentrality;
        Parameters[nameof(evalConfig.MgKnightCorner)].Value = evalConfig.MgKnightCorner;
        Parameters[nameof(evalConfig.EgKnightCorner)].Value = evalConfig.EgKnightCorner;

        // Bishop Location
        Parameters[nameof(evalConfig.MgBishopAdvancement)].Value = evalConfig.MgBishopAdvancement;
        Parameters[nameof(evalConfig.EgBishopAdvancement)].Value = evalConfig.EgBishopAdvancement;
        Parameters[nameof(evalConfig.MgBishopCentrality)].Value = evalConfig.MgBishopCentrality;
        Parameters[nameof(evalConfig.EgBishopCentrality)].Value = evalConfig.EgBishopCentrality;
        Parameters[nameof(evalConfig.MgBishopCorner)].Value = evalConfig.MgBishopCorner;
        Parameters[nameof(evalConfig.EgBishopCorner)].Value = evalConfig.EgBishopCorner;

        // Rook Location
        Parameters[nameof(evalConfig.MgRookAdvancement)].Value = evalConfig.MgRookAdvancement;
        Parameters[nameof(evalConfig.EgRookAdvancement)].Value = evalConfig.EgRookAdvancement;
        Parameters[nameof(evalConfig.MgRookCentrality)].Value = evalConfig.MgRookCentrality;
        Parameters[nameof(evalConfig.EgRookCentrality)].Value = evalConfig.EgRookCentrality;
        Parameters[nameof(evalConfig.MgRookCorner)].Value = evalConfig.MgRookCorner;
        Parameters[nameof(evalConfig.EgRookCorner)].Value = evalConfig.EgRookCorner;

        // Queen Location
        Parameters[nameof(evalConfig.MgQueenAdvancement)].Value = evalConfig.MgQueenAdvancement;
        Parameters[nameof(evalConfig.EgQueenAdvancement)].Value = evalConfig.EgQueenAdvancement;
        Parameters[nameof(evalConfig.MgQueenCentrality)].Value = evalConfig.MgQueenCentrality;
        Parameters[nameof(evalConfig.EgQueenCentrality)].Value = evalConfig.EgQueenCentrality;
        Parameters[nameof(evalConfig.MgQueenCorner)].Value = evalConfig.MgQueenCorner;
        Parameters[nameof(evalConfig.EgQueenCorner)].Value = evalConfig.EgQueenCorner;

        // King Location
        Parameters[nameof(evalConfig.MgKingAdvancement)].Value = evalConfig.MgKingAdvancement;
        Parameters[nameof(evalConfig.EgKingAdvancement)].Value = evalConfig.EgKingAdvancement;
        Parameters[nameof(evalConfig.MgKingCentrality)].Value = evalConfig.MgKingCentrality;
        Parameters[nameof(evalConfig.EgKingCentrality)].Value = evalConfig.EgKingCentrality;
        Parameters[nameof(evalConfig.MgKingCorner)].Value = evalConfig.MgKingCorner;
        Parameters[nameof(evalConfig.EgKingCorner)].Value = evalConfig.EgKingCorner;

        // Piece Mobility
        Parameters[nameof(evalConfig.PieceMobilityPowerPer128)].Value = evalConfig.PieceMobilityPowerPer128;
        Parameters[nameof(evalConfig.MgKnightMobilityScale)].Value = evalConfig.MgKnightMobilityScale;
        Parameters[nameof(evalConfig.EgKnightMobilityScale)].Value = evalConfig.EgKnightMobilityScale;
        Parameters[nameof(evalConfig.MgBishopMobilityScale)].Value = evalConfig.MgBishopMobilityScale;
        Parameters[nameof(evalConfig.EgBishopMobilityScale)].Value = evalConfig.EgBishopMobilityScale;
        Parameters[nameof(evalConfig.MgRookMobilityScale)].Value = evalConfig.MgRookMobilityScale;
        Parameters[nameof(evalConfig.EgRookMobilityScale)].Value = evalConfig.EgRookMobilityScale;
        Parameters[nameof(evalConfig.MgQueenMobilityScale)].Value = evalConfig.MgQueenMobilityScale;
        Parameters[nameof(evalConfig.EgQueenMobilityScale)].Value = evalConfig.EgQueenMobilityScale;

        // Pawn Structure
        Parameters[nameof(evalConfig.MgIsolatedPawn)].Value = evalConfig.MgIsolatedPawn;
        Parameters[nameof(evalConfig.EgIsolatedPawn)].Value = evalConfig.EgIsolatedPawn;
        Parameters[nameof(evalConfig.MgDoubledPawn)].Value = evalConfig.MgDoubledPawn;
        Parameters[nameof(evalConfig.EgDoubledPawn)].Value = evalConfig.EgDoubledPawn;

        // Threats
        Parameters[nameof(evalConfig.MgPawnThreatenMinor)].Value = evalConfig.MgPawnThreatenMinor;
        Parameters[nameof(evalConfig.EgPawnThreatenMinor)].Value = evalConfig.EgPawnThreatenMinor;
        Parameters[nameof(evalConfig.MgPawnThreatenMajor)].Value = evalConfig.MgPawnThreatenMajor;
        Parameters[nameof(evalConfig.EgPawnThreatenMajor)].Value = evalConfig.EgPawnThreatenMajor;
        Parameters[nameof(evalConfig.MgMinorThreatenMajor)].Value = evalConfig.MgMinorThreatenMajor;
        Parameters[nameof(evalConfig.EgMinorThreatenMajor)].Value = evalConfig.EgMinorThreatenMajor;

        // Minor Pieces
        Parameters[nameof(evalConfig.MgBishopPair)].Value = evalConfig.MgBishopPair;
        Parameters[nameof(evalConfig.EgBishopPair)].Value = evalConfig.EgBishopPair;
        Parameters[nameof(evalConfig.MgKnightOutpost)].Value = evalConfig.MgKnightOutpost;
        Parameters[nameof(evalConfig.EgKnightOutpost)].Value = evalConfig.EgKnightOutpost;
        Parameters[nameof(evalConfig.MgBishopOutpost)].Value = evalConfig.MgBishopOutpost;
        Parameters[nameof(evalConfig.EgBishopOutpost)].Value = evalConfig.EgBishopOutpost;

        // Major Pieces
        Parameters[nameof(evalConfig.MgRook7thRank)].Value = evalConfig.MgRook7thRank;
        Parameters[nameof(evalConfig.EgRook7thRank)].Value = evalConfig.EgRook7thRank;
    }


    public void ConfigureEvaluation(Eval eval)
    {
        // Material
        eval.Config.EgPawnMaterial = Parameters[nameof(EvalConfig.EgPawnMaterial)].Value;
        eval.Config.MgKnightMaterial = Parameters[nameof(EvalConfig.MgKnightMaterial)].Value;
        eval.Config.EgKnightMaterial = Parameters[nameof(EvalConfig.EgKnightMaterial)].Value;
        eval.Config.MgBishopMaterial = Parameters[nameof(EvalConfig.MgBishopMaterial)].Value;
        eval.Config.EgBishopMaterial = Parameters[nameof(EvalConfig.EgBishopMaterial)].Value;
        eval.Config.MgRookMaterial = Parameters[nameof(EvalConfig.MgRookMaterial)].Value;
        eval.Config.EgRookMaterial = Parameters[nameof(EvalConfig.EgRookMaterial)].Value;
        eval.Config.MgQueenMaterial = Parameters[nameof(EvalConfig.MgQueenMaterial)].Value;
        eval.Config.EgQueenMaterial = Parameters[nameof(EvalConfig.EgQueenMaterial)].Value;

        // Passed Pawns
        eval.Config.PassedPawnPowerPer128 = Parameters[nameof(EvalConfig.PassedPawnPowerPer128)].Value;
        eval.Config.MgPassedPawnScalePer128 = Parameters[nameof(EvalConfig.MgPassedPawnScalePer128)].Value;
        eval.Config.EgPassedPawnScalePer128 = Parameters[nameof(EvalConfig.EgPassedPawnScalePer128)].Value;
        eval.Config.EgFreePassedPawnScalePer128 = Parameters[nameof(EvalConfig.EgFreePassedPawnScalePer128)].Value;
        eval.Config.EgConnectedPassedPawnScalePer128 = Parameters[nameof(EvalConfig.EgConnectedPassedPawnScalePer128)].Value;
        eval.Config.EgKingEscortedPassedPawn = Parameters[nameof(EvalConfig.EgKingEscortedPassedPawn)].Value;

        // King Safety
        eval.Config.MgKingSafetyPowerPer128 = Parameters[nameof(EvalConfig.MgKingSafetyPowerPer128)].Value;
        eval.Config.MgKingSafetyScalePer128 = Parameters[nameof(EvalConfig.MgKingSafetyScalePer128)].Value;
        eval.Config.MgKingSafetyKnightAttackOuterRingPer8 = Parameters[nameof(EvalConfig.MgKingSafetyKnightAttackOuterRingPer8)].Value;
        eval.Config.MgKingSafetyKnightAttackInnerRingPer8 = Parameters[nameof(EvalConfig.MgKingSafetyKnightAttackInnerRingPer8)].Value;
        eval.Config.MgKingSafetyBishopAttackOuterRingPer8 = Parameters[nameof(EvalConfig.MgKingSafetyBishopAttackOuterRingPer8)].Value;
        eval.Config.MgKingSafetyBishopAttackInnerRingPer8 = Parameters[nameof(EvalConfig.MgKingSafetyBishopAttackInnerRingPer8)].Value;
        eval.Config.MgKingSafetyRookAttackOuterRingPer8 = Parameters[nameof(EvalConfig.MgKingSafetyRookAttackOuterRingPer8)].Value;
        eval.Config.MgKingSafetyRookAttackInnerRingPer8 = Parameters[nameof(EvalConfig.MgKingSafetyRookAttackInnerRingPer8)].Value;
        eval.Config.MgKingSafetyQueenAttackOuterRingPer8 = Parameters[nameof(EvalConfig.MgKingSafetyQueenAttackOuterRingPer8)].Value;
        eval.Config.MgKingSafetyQueenAttackInnerRingPer8 = Parameters[nameof(EvalConfig.MgKingSafetyQueenAttackInnerRingPer8)].Value;
        eval.Config.MgKingSafetySemiOpenFilePer8 = Parameters[nameof(EvalConfig.MgKingSafetySemiOpenFilePer8)].Value;
        eval.Config.MgKingSafetyPawnShieldPer8 = Parameters[nameof(EvalConfig.MgKingSafetyPawnShieldPer8)].Value;
        eval.Config.MgKingSafetyDefendingPiecesPer8 = Parameters[nameof(EvalConfig.MgKingSafetyDefendingPiecesPer8)].Value;

        // Pawn Location
        eval.Config.MgPawnAdvancement = Parameters[nameof(EvalConfig.MgPawnAdvancement)].Value;
        eval.Config.EgPawnAdvancement = Parameters[nameof(EvalConfig.EgPawnAdvancement)].Value;
        eval.Config.MgPawnCentrality = Parameters[nameof(EvalConfig.MgPawnCentrality)].Value;
        eval.Config.EgPawnCentrality = Parameters[nameof(EvalConfig.EgPawnCentrality)].Value;

        // Knight Location
        eval.Config.MgKnightAdvancement = Parameters[nameof(EvalConfig.MgKnightAdvancement)].Value;
        eval.Config.EgKnightAdvancement = Parameters[nameof(EvalConfig.EgKnightAdvancement)].Value;
        eval.Config.MgKnightCentrality = Parameters[nameof(EvalConfig.MgKnightCentrality)].Value;
        eval.Config.EgKnightCentrality = Parameters[nameof(EvalConfig.EgKnightCentrality)].Value;
        eval.Config.MgKnightCorner = Parameters[nameof(EvalConfig.MgKnightCorner)].Value;
        eval.Config.EgKnightCorner = Parameters[nameof(EvalConfig.EgKnightCorner)].Value;

        // Bishop Location
        eval.Config.MgBishopAdvancement = Parameters[nameof(EvalConfig.MgBishopAdvancement)].Value;
        eval.Config.EgBishopAdvancement = Parameters[nameof(EvalConfig.EgBishopAdvancement)].Value;
        eval.Config.MgBishopCentrality = Parameters[nameof(EvalConfig.MgBishopCentrality)].Value;
        eval.Config.EgBishopCentrality = Parameters[nameof(EvalConfig.EgBishopCentrality)].Value;
        eval.Config.MgBishopCorner = Parameters[nameof(EvalConfig.MgBishopCorner)].Value;
        eval.Config.EgBishopCorner = Parameters[nameof(EvalConfig.EgBishopCorner)].Value;

        // Rook Location
        eval.Config.MgRookAdvancement = Parameters[nameof(EvalConfig.MgRookAdvancement)].Value;
        eval.Config.EgRookAdvancement = Parameters[nameof(EvalConfig.EgRookAdvancement)].Value;
        eval.Config.MgRookCentrality = Parameters[nameof(EvalConfig.MgRookCentrality)].Value;
        eval.Config.EgRookCentrality = Parameters[nameof(EvalConfig.EgRookCentrality)].Value;
        eval.Config.MgRookCorner = Parameters[nameof(EvalConfig.MgRookCorner)].Value;
        eval.Config.EgRookCorner = Parameters[nameof(EvalConfig.EgRookCorner)].Value;

        // Queen Location
        eval.Config.MgQueenAdvancement = Parameters[nameof(EvalConfig.MgQueenAdvancement)].Value;
        eval.Config.EgQueenAdvancement = Parameters[nameof(EvalConfig.EgQueenAdvancement)].Value;
        eval.Config.MgQueenCentrality = Parameters[nameof(EvalConfig.MgQueenCentrality)].Value;
        eval.Config.EgQueenCentrality = Parameters[nameof(EvalConfig.EgQueenCentrality)].Value;
        eval.Config.MgQueenCorner = Parameters[nameof(EvalConfig.MgQueenCorner)].Value;
        eval.Config.EgQueenCorner = Parameters[nameof(EvalConfig.EgQueenCorner)].Value;

        // King Location
        eval.Config.MgKingAdvancement = Parameters[nameof(EvalConfig.MgKingAdvancement)].Value;
        eval.Config.EgKingAdvancement = Parameters[nameof(EvalConfig.EgKingAdvancement)].Value;
        eval.Config.MgKingCentrality = Parameters[nameof(EvalConfig.MgKingCentrality)].Value;
        eval.Config.EgKingCentrality = Parameters[nameof(EvalConfig.EgKingCentrality)].Value;
        eval.Config.MgKingCorner = Parameters[nameof(EvalConfig.MgKingCorner)].Value;
        eval.Config.EgKingCorner = Parameters[nameof(EvalConfig.EgKingCorner)].Value;

        // Piece Mobility
        eval.Config.PieceMobilityPowerPer128 = Parameters[nameof(EvalConfig.PieceMobilityPowerPer128)].Value;
        eval.Config.MgKnightMobilityScale = Parameters[nameof(EvalConfig.MgKnightMobilityScale)].Value;
        eval.Config.EgKnightMobilityScale = Parameters[nameof(EvalConfig.EgKnightMobilityScale)].Value;
        eval.Config.MgBishopMobilityScale = Parameters[nameof(EvalConfig.MgBishopMobilityScale)].Value;
        eval.Config.EgBishopMobilityScale = Parameters[nameof(EvalConfig.EgBishopMobilityScale)].Value;
        eval.Config.MgRookMobilityScale = Parameters[nameof(EvalConfig.MgRookMobilityScale)].Value;
        eval.Config.EgRookMobilityScale = Parameters[nameof(EvalConfig.EgRookMobilityScale)].Value;
        eval.Config.MgQueenMobilityScale = Parameters[nameof(EvalConfig.MgQueenMobilityScale)].Value;
        eval.Config.EgQueenMobilityScale = Parameters[nameof(EvalConfig.EgQueenMobilityScale)].Value;

        // Pawn Structure
        eval.Config.MgIsolatedPawn = Parameters[nameof(EvalConfig.MgIsolatedPawn)].Value;
        eval.Config.EgIsolatedPawn = Parameters[nameof(EvalConfig.EgIsolatedPawn)].Value;
        eval.Config.MgDoubledPawn = Parameters[nameof(EvalConfig.MgDoubledPawn)].Value;
        eval.Config.EgDoubledPawn = Parameters[nameof(EvalConfig.EgDoubledPawn)].Value;

        // Threats
        eval.Config.MgPawnThreatenMinor = Parameters[nameof(EvalConfig.MgPawnThreatenMinor)].Value;
        eval.Config.EgPawnThreatenMinor = Parameters[nameof(EvalConfig.EgPawnThreatenMinor)].Value;
        eval.Config.MgPawnThreatenMajor = Parameters[nameof(EvalConfig.MgPawnThreatenMajor)].Value;
        eval.Config.EgPawnThreatenMajor = Parameters[nameof(EvalConfig.EgPawnThreatenMajor)].Value;
        eval.Config.MgMinorThreatenMajor = Parameters[nameof(EvalConfig.MgMinorThreatenMajor)].Value;
        eval.Config.EgMinorThreatenMajor = Parameters[nameof(EvalConfig.EgMinorThreatenMajor)].Value;

        // Minor Pieces
        eval.Config.MgBishopPair = Parameters[nameof(EvalConfig.MgBishopPair)].Value;
        eval.Config.EgBishopPair = Parameters[nameof(EvalConfig.EgBishopPair)].Value;
        eval.Config.MgKnightOutpost = Parameters[nameof(EvalConfig.MgKnightOutpost)].Value;
        eval.Config.EgKnightOutpost = Parameters[nameof(EvalConfig.EgKnightOutpost)].Value;
        eval.Config.MgBishopOutpost = Parameters[nameof(EvalConfig.MgBishopOutpost)].Value;
        eval.Config.EgBishopOutpost = Parameters[nameof(EvalConfig.EgBishopOutpost)].Value;

        // Major Pieces
        eval.Config.MgRook7thRank = Parameters[nameof(EvalConfig.MgRook7thRank)].Value;
        eval.Config.EgRook7thRank = Parameters[nameof(EvalConfig.EgRook7thRank)].Value;

        // Calculate positional factors after updating evaluation config.
        eval.CalculatePositionalFactors();
    }


    // See http://talkchess.com/forum/viewtopic.php?t=50823&postdays=0&postorder=asc&highlight=texel+tuning&topic_view=flat&start=20.
    public void CalculateEvaluationError(Board board, Search search, int winScale)
    {
        search.NodesExamineTime = long.MaxValue;
        search.PvInfoUpdate = false;
        search.Continue = true;

        // Sum the square of evaluation error over (almost) all positions in (almost) all games.
        EvaluationError = 0;

        for (var gameIndex = 0; gameIndex < PgnGames.Count; gameIndex++)
        {
            var game = PgnGames[gameIndex];
            if (game.Result == GameResult.Unknown) continue; // Skip games with unknown results.

            board.SetPosition(Board.StartPositionFen);

            for (var moveIndex = 0; moveIndex < game.Moves.Count; moveIndex++)
            {
                var move = game.Moves[moveIndex];

                // Play move and get quiet score.
                board.PlayMove(move);
                var quietScore = search.GetQuietScore(board, 1, 1, -SpecialScore.Max, SpecialScore.Max);

                // Convert quiet score to win fraction and compare to game result.
                var winFraction = GetWinFraction(quietScore, winScale);

                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                var result = game.Result switch
                {
                    GameResult.WhiteWon => board.CurrentPosition.ColorToMove == Color.White ? 1d : 0,
                    GameResult.Draw => 0.5d,
                    GameResult.BlackWon => board.CurrentPosition.ColorToMove == Color.Black ? 1d : 0,
                    _ => throw new InvalidOperationException($"{game.Result} game result not supported.")
                };

                var evalError = winFraction - result;
                EvaluationError += evalError * evalError;
            }
        }

        if (EvaluationError < BestEvaluationError)
        {
            BestEvaluationError = EvaluationError;
            Parameters.CopyValuesTo(BestParameters);
        }
    }


    public void Move()
    {
        // Move particle in parameter space.
        for (var index = 0; index < Parameters.Count; index++)
        {
            var parameter = Parameters[index];
            parameter.Value += (int)_velocities[index];

            if (parameter.Value < parameter.MinValue)
            {
                parameter.Value = parameter.MinValue;
                _velocities[index] = 0;
            }

            if (parameter.Value > parameter.MaxValue)
            {
                parameter.Value = parameter.MaxValue;
                _velocities[index] = 0;
            }
        }
    }


    public void UpdateVelocity(Particle bestSwarmParticle, Particle globallyBestParticle)
    {
        for (var index = 0; index < Parameters.Count; index++)
        {
            var parameter = Parameters[index];

            var bestParameter = BestParameters[index];
            var bestSwarmParameter = bestSwarmParticle.BestParameters[index];
            var globallyBestParameter = globallyBestParticle.BestParameters[index];

            // Consider inertia.
            var velocity = _inertia * _velocities[index];
            var particleMagnitude = SafeRandom.NextDouble() * _influence;
            velocity += particleMagnitude * (bestParameter.Value - parameter.Value);

            // Consider attraction to swarm's best particle.
            var swarmMagnitude = SafeRandom.NextDouble() * ParticleSwarm.Influence;
            velocity += swarmMagnitude * (bestSwarmParameter.Value - parameter.Value);

            // Consider attraction to globally best particle.
            var allSwarmsMagnitude = SafeRandom.NextDouble() * ParticleSwarms.Influence;
            velocity += allSwarmsMagnitude * (globallyBestParameter.Value - parameter.Value);

            _velocities[index] = velocity;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double GetWinFraction(int score, int winScale) => 1d / (1d + Math.Pow(10d, -1d * score / winScale)); // Use a sigmoid function to map score to win fraction.
}