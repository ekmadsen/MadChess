// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Runtime.CompilerServices;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Config;
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
        var evalConfig = new EvaluationConfig();

        //// Material
        //Parameters[nameof(evalConfig.EgPawnMaterial)].Value = evalConfig.EgPawnMaterial;
        //Parameters[nameof(evalConfig.MgKnightMaterial)].Value = evalConfig.MgKnightMaterial;
        //Parameters[nameof(evalConfig.EgKnightMaterial)].Value = evalConfig.EgKnightMaterial;
        //Parameters[nameof(evalConfig.MgBishopMaterial)].Value = evalConfig.MgBishopMaterial;
        //Parameters[nameof(evalConfig.EgBishopMaterial)].Value = evalConfig.EgBishopMaterial;
        //Parameters[nameof(evalConfig.MgRookMaterial)].Value = evalConfig.MgRookMaterial;
        //Parameters[nameof(evalConfig.EgRookMaterial)].Value = evalConfig.EgRookMaterial;
        //Parameters[nameof(evalConfig.MgQueenMaterial)].Value = evalConfig.MgQueenMaterial;
        //Parameters[nameof(evalConfig.EgQueenMaterial)].Value = evalConfig.EgQueenMaterial;

        // Passed Pawns
        //Parameters[nameof(evalConfig.PassedPawnPowerPer128)].Value = evalConfig.PassedPawnPowerPer128;
        Parameters[nameof(evalConfig.MgPassedPawnScalePer128)].Value = evalConfig.MgPassedPawnScalePer128;
        Parameters[nameof(evalConfig.EgPassedPawnScalePer128)].Value = evalConfig.EgPassedPawnScalePer128;
        Parameters[nameof(evalConfig.MgFreePassedPawnScalePer128)].Value = evalConfig.MgFreePassedPawnScalePer128;
        Parameters[nameof(evalConfig.EgFreePassedPawnScalePer128)].Value = evalConfig.EgFreePassedPawnScalePer128;
        Parameters[nameof(evalConfig.MgConnectedPassedPawnScalePer128)].Value = evalConfig.MgConnectedPassedPawnScalePer128;
        Parameters[nameof(evalConfig.EgConnectedPassedPawnScalePer128)].Value = evalConfig.EgConnectedPassedPawnScalePer128;
        Parameters[nameof(evalConfig.EgKingEscortedPassedPawn)].Value = evalConfig.EgKingEscortedPassedPawn;

        //// King Safety
        //Parameters[nameof(evalConfig.MgKingSafetyPowerPer128)].Value = evalConfig.MgKingSafetyPowerPer128;
        //Parameters[nameof(evalConfig.MgKingSafetyScalePer128)].Value = evalConfig.MgKingSafetyScalePer128;
        //Parameters[nameof(evalConfig.MgKingSafetyKnightAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyKnightAttackOuterRingPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyKnightAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyKnightAttackInnerRingPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyKnightProximityPer8)].Value = evalConfig.MgKingSafetyKnightProximityPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyBishopAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyBishopAttackOuterRingPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyBishopAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyBishopAttackInnerRingPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyBishopProximityPer8)].Value = evalConfig.MgKingSafetyBishopProximityPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyRookAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyRookAttackOuterRingPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyRookAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyRookAttackInnerRingPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyRookProximityPer8)].Value = evalConfig.MgKingSafetyRookProximityPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyQueenAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyQueenAttackOuterRingPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyQueenAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyQueenAttackInnerRingPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyQueenProximityPer8)].Value = evalConfig.MgKingSafetyQueenProximityPer8;
        //Parameters[nameof(evalConfig.MgKingSafetySemiOpenFilePer8)].Value = evalConfig.MgKingSafetySemiOpenFilePer8;
        //Parameters[nameof(evalConfig.MgKingSafetyPawnShieldPer8)].Value = evalConfig.MgKingSafetyPawnShieldPer8;
        //Parameters[nameof(evalConfig.MgKingSafetyDefendingPiecesPer8)].Value = evalConfig.MgKingSafetyDefendingPiecesPer8;

        //// Pawn Location
        //Parameters[nameof(evalConfig.MgPawnAdvancement)].Value = evalConfig.MgPawnAdvancement;
        //Parameters[nameof(evalConfig.EgPawnAdvancement)].Value = evalConfig.EgPawnAdvancement;
        //Parameters[nameof(evalConfig.MgPawnSquareCentrality)].Value = evalConfig.MgPawnSquareCentrality;
        //Parameters[nameof(evalConfig.EgPawnSquareCentrality)].Value = evalConfig.EgPawnSquareCentrality;
        //Parameters[nameof(evalConfig.MgPawnFileCentrality)].Value = evalConfig.MgPawnFileCentrality;
        //Parameters[nameof(evalConfig.EgPawnFileCentrality)].Value = evalConfig.EgPawnFileCentrality;
        //Parameters[nameof(evalConfig.MgPawnCorner)].Value = evalConfig.MgPawnCorner;
        //Parameters[nameof(evalConfig.EgPawnCorner)].Value = evalConfig.EgPawnCorner;

        //// Knight Location
        //Parameters[nameof(evalConfig.MgKnightAdvancement)].Value = evalConfig.MgKnightAdvancement;
        //Parameters[nameof(evalConfig.EgKnightAdvancement)].Value = evalConfig.EgKnightAdvancement;
        //Parameters[nameof(evalConfig.MgKnightSquareCentrality)].Value = evalConfig.MgKnightSquareCentrality;
        //Parameters[nameof(evalConfig.EgKnightSquareCentrality)].Value = evalConfig.EgKnightSquareCentrality;
        //Parameters[nameof(evalConfig.MgKnightFileCentrality)].Value = evalConfig.MgKnightFileCentrality;
        //Parameters[nameof(evalConfig.EgKnightFileCentrality)].Value = evalConfig.EgKnightFileCentrality;
        //Parameters[nameof(evalConfig.MgKnightCorner)].Value = evalConfig.MgKnightCorner;
        //Parameters[nameof(evalConfig.EgKnightCorner)].Value = evalConfig.EgKnightCorner;

        //// Bishop Location
        //Parameters[nameof(evalConfig.MgBishopAdvancement)].Value = evalConfig.MgBishopAdvancement;
        //Parameters[nameof(evalConfig.EgBishopAdvancement)].Value = evalConfig.EgBishopAdvancement;
        //Parameters[nameof(evalConfig.MgBishopSquareCentrality)].Value = evalConfig.MgBishopSquareCentrality;
        //Parameters[nameof(evalConfig.EgBishopSquareCentrality)].Value = evalConfig.EgBishopSquareCentrality;
        //Parameters[nameof(evalConfig.MgBishopFileCentrality)].Value = evalConfig.MgBishopFileCentrality;
        //Parameters[nameof(evalConfig.EgBishopFileCentrality)].Value = evalConfig.EgBishopFileCentrality;
        //Parameters[nameof(evalConfig.MgBishopCorner)].Value = evalConfig.MgBishopCorner;
        //Parameters[nameof(evalConfig.EgBishopCorner)].Value = evalConfig.EgBishopCorner;

        //// Rook Location
        //Parameters[nameof(evalConfig.MgRookAdvancement)].Value = evalConfig.MgRookAdvancement;
        //Parameters[nameof(evalConfig.EgRookAdvancement)].Value = evalConfig.EgRookAdvancement;
        //Parameters[nameof(evalConfig.MgRookSquareCentrality)].Value = evalConfig.MgRookSquareCentrality;
        //Parameters[nameof(evalConfig.EgRookSquareCentrality)].Value = evalConfig.EgRookSquareCentrality;
        //Parameters[nameof(evalConfig.MgRookFileCentrality)].Value = evalConfig.MgRookFileCentrality;
        //Parameters[nameof(evalConfig.EgRookFileCentrality)].Value = evalConfig.EgRookFileCentrality;
        //Parameters[nameof(evalConfig.MgRookCorner)].Value = evalConfig.MgRookCorner;
        //Parameters[nameof(evalConfig.EgRookCorner)].Value = evalConfig.EgRookCorner;

        //// Queen Location
        //Parameters[nameof(evalConfig.MgQueenAdvancement)].Value = evalConfig.MgQueenAdvancement;
        //Parameters[nameof(evalConfig.EgQueenAdvancement)].Value = evalConfig.EgQueenAdvancement;
        //Parameters[nameof(evalConfig.MgQueenSquareCentrality)].Value = evalConfig.MgQueenSquareCentrality;
        //Parameters[nameof(evalConfig.EgQueenSquareCentrality)].Value = evalConfig.EgQueenSquareCentrality;
        //Parameters[nameof(evalConfig.MgQueenFileCentrality)].Value = evalConfig.MgQueenFileCentrality;
        //Parameters[nameof(evalConfig.EgQueenFileCentrality)].Value = evalConfig.EgQueenFileCentrality;
        //Parameters[nameof(evalConfig.MgQueenCorner)].Value = evalConfig.MgQueenCorner;
        //Parameters[nameof(evalConfig.EgQueenCorner)].Value = evalConfig.EgQueenCorner;

        //// King Location
        //Parameters[nameof(evalConfig.MgKingAdvancement)].Value = evalConfig.MgKingAdvancement;
        //Parameters[nameof(evalConfig.EgKingAdvancement)].Value = evalConfig.EgKingAdvancement;
        //Parameters[nameof(evalConfig.MgKingSquareCentrality)].Value = evalConfig.MgKingSquareCentrality;
        //Parameters[nameof(evalConfig.EgKingSquareCentrality)].Value = evalConfig.EgKingSquareCentrality;
        //Parameters[nameof(evalConfig.MgKingFileCentrality)].Value = evalConfig.MgKingFileCentrality;
        //Parameters[nameof(evalConfig.EgKingFileCentrality)].Value = evalConfig.EgKingFileCentrality;
        //Parameters[nameof(evalConfig.MgKingCorner)].Value = evalConfig.MgKingCorner;
        //Parameters[nameof(evalConfig.EgKingCorner)].Value = evalConfig.EgKingCorner;

        //// Piece Mobility
        //Parameters[nameof(evalConfig.PieceMobilityPowerPer128)].Value = evalConfig.PieceMobilityPowerPer128;
        //Parameters[nameof(evalConfig.MgKnightMobilityScale)].Value = evalConfig.MgKnightMobilityScale;
        //Parameters[nameof(evalConfig.EgKnightMobilityScale)].Value = evalConfig.EgKnightMobilityScale;
        //Parameters[nameof(evalConfig.MgBishopMobilityScale)].Value = evalConfig.MgBishopMobilityScale;
        //Parameters[nameof(evalConfig.EgBishopMobilityScale)].Value = evalConfig.EgBishopMobilityScale;
        //Parameters[nameof(evalConfig.MgRookMobilityScale)].Value = evalConfig.MgRookMobilityScale;
        //Parameters[nameof(evalConfig.EgRookMobilityScale)].Value = evalConfig.EgRookMobilityScale;
        //Parameters[nameof(evalConfig.MgQueenMobilityScale)].Value = evalConfig.MgQueenMobilityScale;
        //Parameters[nameof(evalConfig.EgQueenMobilityScale)].Value = evalConfig.EgQueenMobilityScale;

        //// Pawn Structure
        //Parameters[nameof(evalConfig.MgIsolatedPawn)].Value = evalConfig.MgIsolatedPawn;
        //Parameters[nameof(evalConfig.EgIsolatedPawn)].Value = evalConfig.EgIsolatedPawn;
        //Parameters[nameof(evalConfig.MgDoubledPawn)].Value = evalConfig.MgDoubledPawn;
        //Parameters[nameof(evalConfig.EgDoubledPawn)].Value = evalConfig.EgDoubledPawn;

        //// Threats
        //Parameters[nameof(evalConfig.MgPawnThreatenMinor)].Value = evalConfig.MgPawnThreatenMinor;
        //Parameters[nameof(evalConfig.EgPawnThreatenMinor)].Value = evalConfig.EgPawnThreatenMinor;
        //Parameters[nameof(evalConfig.MgPawnThreatenMajor)].Value = evalConfig.MgPawnThreatenMajor;
        //Parameters[nameof(evalConfig.EgPawnThreatenMajor)].Value = evalConfig.EgPawnThreatenMajor;
        //Parameters[nameof(evalConfig.MgMinorThreatenMajor)].Value = evalConfig.MgMinorThreatenMajor;
        //Parameters[nameof(evalConfig.EgMinorThreatenMajor)].Value = evalConfig.EgMinorThreatenMajor;

        //// Minor Pieces
        //Parameters[nameof(evalConfig.MgBishopPair)].Value = evalConfig.MgBishopPair;
        //Parameters[nameof(evalConfig.EgBishopPair)].Value = evalConfig.EgBishopPair;
        //Parameters[nameof(evalConfig.MgKnightOutpost)].Value = evalConfig.MgKnightOutpost;
        //Parameters[nameof(evalConfig.EgKnightOutpost)].Value = evalConfig.EgKnightOutpost;
        //Parameters[nameof(evalConfig.MgBishopOutpost)].Value = evalConfig.MgBishopOutpost;
        //Parameters[nameof(evalConfig.EgBishopOutpost)].Value = evalConfig.EgBishopOutpost;

        //// Major Pieces
        //Parameters[nameof(evalConfig.MgRook7thRank)].Value = evalConfig.MgRook7thRank;
        //Parameters[nameof(evalConfig.EgRook7thRank)].Value = evalConfig.EgRook7thRank;
    }


    public void ConfigureEvaluation(Evaluation evaluation)
    {
        //// Material
        //evaluation.Config.EgPawnMaterial = Parameters[nameof(EvaluationConfig.EgPawnMaterial)].Value;
        //evaluation.Config.MgKnightMaterial = Parameters[nameof(EvaluationConfig.MgKnightMaterial)].Value;
        //evaluation.Config.EgKnightMaterial = Parameters[nameof(EvaluationConfig.EgKnightMaterial)].Value;
        //evaluation.Config.MgBishopMaterial = Parameters[nameof(EvaluationConfig.MgBishopMaterial)].Value;
        //evaluation.Config.EgBishopMaterial = Parameters[nameof(EvaluationConfig.EgBishopMaterial)].Value;
        //evaluation.Config.MgRookMaterial = Parameters[nameof(EvaluationConfig.MgRookMaterial)].Value;
        //evaluation.Config.EgRookMaterial = Parameters[nameof(EvaluationConfig.EgRookMaterial)].Value;
        //evaluation.Config.MgQueenMaterial = Parameters[nameof(EvaluationConfig.MgQueenMaterial)].Value;
        //evaluation.Config.EgQueenMaterial = Parameters[nameof(EvaluationConfig.EgQueenMaterial)].Value;

        // Passed Pawns
        //evaluation.Config.PassedPawnPowerPer128 = Parameters[nameof(EvaluationConfig.PassedPawnPowerPer128)].Value;
        evaluation.Config.MgPassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.MgPassedPawnScalePer128)].Value;
        evaluation.Config.EgPassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.EgPassedPawnScalePer128)].Value;
        evaluation.Config.MgFreePassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.MgFreePassedPawnScalePer128)].Value;
        evaluation.Config.EgFreePassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.EgFreePassedPawnScalePer128)].Value;
        evaluation.Config.MgConnectedPassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.MgConnectedPassedPawnScalePer128)].Value;
        evaluation.Config.EgConnectedPassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.EgConnectedPassedPawnScalePer128)].Value;
        evaluation.Config.EgKingEscortedPassedPawn = Parameters[nameof(EvaluationConfig.EgKingEscortedPassedPawn)].Value;

        //// King Safety
        //evaluation.Config.MgKingSafetyPowerPer128 = Parameters[nameof(EvaluationConfig.MgKingSafetyPowerPer128)].Value;
        //evaluation.Config.MgKingSafetyScalePer128 = Parameters[nameof(EvaluationConfig.MgKingSafetyScalePer128)].Value;
        //evaluation.Config.MgKingSafetyKnightAttackOuterRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyKnightAttackOuterRingPer8)].Value;
        //evaluation.Config.MgKingSafetyKnightAttackInnerRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyKnightAttackInnerRingPer8)].Value;
        //evaluation.Config.MgKingSafetyKnightProximityPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyKnightProximityPer8)].Value;
        //evaluation.Config.MgKingSafetyBishopAttackOuterRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyBishopAttackOuterRingPer8)].Value;
        //evaluation.Config.MgKingSafetyBishopAttackInnerRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyBishopAttackInnerRingPer8)].Value;
        //evaluation.Config.MgKingSafetyBishopProximityPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyBishopProximityPer8)].Value;
        //evaluation.Config.MgKingSafetyRookAttackOuterRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyRookAttackOuterRingPer8)].Value;
        //evaluation.Config.MgKingSafetyRookAttackInnerRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyRookAttackInnerRingPer8)].Value;
        //evaluation.Config.MgKingSafetyRookProximityPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyRookProximityPer8)].Value;
        //evaluation.Config.MgKingSafetyQueenAttackOuterRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyQueenAttackOuterRingPer8)].Value;
        //evaluation.Config.MgKingSafetyQueenAttackInnerRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyQueenAttackInnerRingPer8)].Value;
        //evaluation.Config.MgKingSafetyQueenProximityPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyQueenProximityPer8)].Value;
        //evaluation.Config.MgKingSafetySemiOpenFilePer8 = Parameters[nameof(EvaluationConfig.MgKingSafetySemiOpenFilePer8)].Value;
        //evaluation.Config.MgKingSafetyPawnShieldPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyPawnShieldPer8)].Value;
        //evaluation.Config.MgKingSafetyDefendingPiecesPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyDefendingPiecesPer8)].Value;

        //// Pawn Location
        //evaluation.Config.MgPawnAdvancement = Parameters[nameof(EvaluationConfig.MgPawnAdvancement)].Value;
        //evaluation.Config.EgPawnAdvancement = Parameters[nameof(EvaluationConfig.EgPawnAdvancement)].Value;
        //evaluation.Config.MgPawnSquareCentrality = Parameters[nameof(EvaluationConfig.MgPawnSquareCentrality)].Value;
        //evaluation.Config.EgPawnSquareCentrality = Parameters[nameof(EvaluationConfig.EgPawnSquareCentrality)].Value;
        //evaluation.Config.MgPawnFileCentrality = Parameters[nameof(EvaluationConfig.MgPawnFileCentrality)].Value;
        //evaluation.Config.EgPawnFileCentrality = Parameters[nameof(EvaluationConfig.EgPawnFileCentrality)].Value;
        //evaluation.Config.MgPawnCorner = Parameters[nameof(EvaluationConfig.MgPawnCorner)].Value;
        //evaluation.Config.EgPawnCorner = Parameters[nameof(EvaluationConfig.EgPawnCorner)].Value;

        //// Knight Location
        //evaluation.Config.MgKnightAdvancement = Parameters[nameof(EvaluationConfig.MgKnightAdvancement)].Value;
        //evaluation.Config.EgKnightAdvancement = Parameters[nameof(EvaluationConfig.EgKnightAdvancement)].Value;
        //evaluation.Config.MgKnightSquareCentrality = Parameters[nameof(EvaluationConfig.MgKnightSquareCentrality)].Value;
        //evaluation.Config.EgKnightSquareCentrality = Parameters[nameof(EvaluationConfig.EgKnightSquareCentrality)].Value;
        //evaluation.Config.MgKnightFileCentrality = Parameters[nameof(EvaluationConfig.MgKnightFileCentrality)].Value;
        //evaluation.Config.EgKnightFileCentrality = Parameters[nameof(EvaluationConfig.EgKnightFileCentrality)].Value;
        //evaluation.Config.MgKnightCorner = Parameters[nameof(EvaluationConfig.MgKnightCorner)].Value;
        //evaluation.Config.EgKnightCorner = Parameters[nameof(EvaluationConfig.EgKnightCorner)].Value;

        //// Bishop Location
        //evaluation.Config.MgBishopAdvancement = Parameters[nameof(EvaluationConfig.MgBishopAdvancement)].Value;
        //evaluation.Config.EgBishopAdvancement = Parameters[nameof(EvaluationConfig.EgBishopAdvancement)].Value;
        //evaluation.Config.MgBishopSquareCentrality = Parameters[nameof(EvaluationConfig.MgBishopSquareCentrality)].Value;
        //evaluation.Config.EgBishopSquareCentrality = Parameters[nameof(EvaluationConfig.EgBishopSquareCentrality)].Value;
        //evaluation.Config.MgBishopFileCentrality = Parameters[nameof(EvaluationConfig.MgBishopFileCentrality)].Value;
        //evaluation.Config.EgBishopFileCentrality = Parameters[nameof(EvaluationConfig.EgBishopFileCentrality)].Value;
        //evaluation.Config.MgBishopCorner = Parameters[nameof(EvaluationConfig.MgBishopCorner)].Value;
        //evaluation.Config.EgBishopCorner = Parameters[nameof(EvaluationConfig.EgBishopCorner)].Value;

        //// Rook Location
        //evaluation.Config.MgRookAdvancement = Parameters[nameof(EvaluationConfig.MgRookAdvancement)].Value;
        //evaluation.Config.EgRookAdvancement = Parameters[nameof(EvaluationConfig.EgRookAdvancement)].Value;
        //evaluation.Config.MgRookSquareCentrality = Parameters[nameof(EvaluationConfig.MgRookSquareCentrality)].Value;
        //evaluation.Config.EgRookSquareCentrality = Parameters[nameof(EvaluationConfig.EgRookSquareCentrality)].Value;
        //evaluation.Config.MgRookFileCentrality = Parameters[nameof(EvaluationConfig.MgRookFileCentrality)].Value;
        //evaluation.Config.EgRookFileCentrality = Parameters[nameof(EvaluationConfig.EgRookFileCentrality)].Value;
        //evaluation.Config.MgRookCorner = Parameters[nameof(EvaluationConfig.MgRookCorner)].Value;
        //evaluation.Config.EgRookCorner = Parameters[nameof(EvaluationConfig.EgRookCorner)].Value;

        //// Queen Location
        //evaluation.Config.MgQueenAdvancement = Parameters[nameof(EvaluationConfig.MgQueenAdvancement)].Value;
        //evaluation.Config.EgQueenAdvancement = Parameters[nameof(EvaluationConfig.EgQueenAdvancement)].Value;
        //evaluation.Config.MgQueenSquareCentrality = Parameters[nameof(EvaluationConfig.MgQueenSquareCentrality)].Value;
        //evaluation.Config.EgQueenSquareCentrality = Parameters[nameof(EvaluationConfig.EgQueenSquareCentrality)].Value;
        //evaluation.Config.MgQueenFileCentrality = Parameters[nameof(EvaluationConfig.MgQueenFileCentrality)].Value;
        //evaluation.Config.EgQueenFileCentrality = Parameters[nameof(EvaluationConfig.EgQueenFileCentrality)].Value;
        //evaluation.Config.MgQueenCorner = Parameters[nameof(EvaluationConfig.MgQueenCorner)].Value;
        //evaluation.Config.EgQueenCorner = Parameters[nameof(EvaluationConfig.EgQueenCorner)].Value;

        //// King Location
        //evaluation.Config.MgKingAdvancement = Parameters[nameof(EvaluationConfig.MgKingAdvancement)].Value;
        //evaluation.Config.EgKingAdvancement = Parameters[nameof(EvaluationConfig.EgKingAdvancement)].Value;
        //evaluation.Config.MgKingSquareCentrality = Parameters[nameof(EvaluationConfig.MgKingSquareCentrality)].Value;
        //evaluation.Config.EgKingSquareCentrality = Parameters[nameof(EvaluationConfig.EgKingSquareCentrality)].Value;
        //evaluation.Config.MgKingFileCentrality = Parameters[nameof(EvaluationConfig.MgKingFileCentrality)].Value;
        //evaluation.Config.EgKingFileCentrality = Parameters[nameof(EvaluationConfig.EgKingFileCentrality)].Value;
        //evaluation.Config.MgKingCorner = Parameters[nameof(EvaluationConfig.MgKingCorner)].Value;
        //evaluation.Config.EgKingCorner = Parameters[nameof(EvaluationConfig.EgKingCorner)].Value;

        //// Piece Mobility
        //evaluation.Config.PieceMobilityPowerPer128 = Parameters[nameof(EvaluationConfig.PieceMobilityPowerPer128)].Value;
        //evaluation.Config.MgKnightMobilityScale = Parameters[nameof(EvaluationConfig.MgKnightMobilityScale)].Value;
        //evaluation.Config.EgKnightMobilityScale = Parameters[nameof(EvaluationConfig.EgKnightMobilityScale)].Value;
        //evaluation.Config.MgBishopMobilityScale = Parameters[nameof(EvaluationConfig.MgBishopMobilityScale)].Value;
        //evaluation.Config.EgBishopMobilityScale = Parameters[nameof(EvaluationConfig.EgBishopMobilityScale)].Value;
        //evaluation.Config.MgRookMobilityScale = Parameters[nameof(EvaluationConfig.MgRookMobilityScale)].Value;
        //evaluation.Config.EgRookMobilityScale = Parameters[nameof(EvaluationConfig.EgRookMobilityScale)].Value;
        //evaluation.Config.MgQueenMobilityScale = Parameters[nameof(EvaluationConfig.MgQueenMobilityScale)].Value;
        //evaluation.Config.EgQueenMobilityScale = Parameters[nameof(EvaluationConfig.EgQueenMobilityScale)].Value;

        //// Pawn Structure
        //evaluation.Config.MgIsolatedPawn = Parameters[nameof(EvaluationConfig.MgIsolatedPawn)].Value;
        //evaluation.Config.EgIsolatedPawn = Parameters[nameof(EvaluationConfig.EgIsolatedPawn)].Value;
        //evaluation.Config.MgDoubledPawn = Parameters[nameof(EvaluationConfig.MgDoubledPawn)].Value;
        //evaluation.Config.EgDoubledPawn = Parameters[nameof(EvaluationConfig.EgDoubledPawn)].Value;

        //// Threats
        //evaluation.Config.MgPawnThreatenMinor = Parameters[nameof(EvaluationConfig.MgPawnThreatenMinor)].Value;
        //evaluation.Config.EgPawnThreatenMinor = Parameters[nameof(EvaluationConfig.EgPawnThreatenMinor)].Value;
        //evaluation.Config.MgPawnThreatenMajor = Parameters[nameof(EvaluationConfig.MgPawnThreatenMajor)].Value;
        //evaluation.Config.EgPawnThreatenMajor = Parameters[nameof(EvaluationConfig.EgPawnThreatenMajor)].Value;
        //evaluation.Config.MgMinorThreatenMajor = Parameters[nameof(EvaluationConfig.MgMinorThreatenMajor)].Value;
        //evaluation.Config.EgMinorThreatenMajor = Parameters[nameof(EvaluationConfig.EgMinorThreatenMajor)].Value;

        //// Minor Pieces
        //evaluation.Config.MgBishopPair = Parameters[nameof(EvaluationConfig.MgBishopPair)].Value;
        //evaluation.Config.EgBishopPair = Parameters[nameof(EvaluationConfig.EgBishopPair)].Value;
        //evaluation.Config.MgKnightOutpost = Parameters[nameof(EvaluationConfig.MgKnightOutpost)].Value;
        //evaluation.Config.EgKnightOutpost = Parameters[nameof(EvaluationConfig.EgKnightOutpost)].Value;
        //evaluation.Config.MgBishopOutpost = Parameters[nameof(EvaluationConfig.MgBishopOutpost)].Value;
        //evaluation.Config.EgBishopOutpost = Parameters[nameof(EvaluationConfig.EgBishopOutpost)].Value;

        //// Major Pieces
        //evaluation.Config.MgRook7thRank = Parameters[nameof(EvaluationConfig.MgRook7thRank)].Value;
        //evaluation.Config.EgRook7thRank = Parameters[nameof(EvaluationConfig.EgRook7thRank)].Value;

        // Calculate positional factors after updating evaluation config.
        evaluation.CalculatePositionalFactors();
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
                var quietScore = search.GetQuietScore(board, 1, 1, -StaticScore.Max, StaticScore.Max);

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

            // Calculate inertia.
            var velocity = _inertia * _velocities[index];
            var particleMagnitude = SafeRandom.NextDouble() * _influence;
            velocity += particleMagnitude * (bestParameter.Value - parameter.Value);

            // Calculate attraction to swarm's best particle.
            var swarmMagnitude = SafeRandom.NextDouble() * ParticleSwarm.Influence;
            velocity += swarmMagnitude * (bestSwarmParameter.Value - parameter.Value);

            // Calculate attraction to globally best particle.
            var allSwarmsMagnitude = SafeRandom.NextDouble() * ParticleSwarms.Influence;
            velocity += allSwarmsMagnitude * (globallyBestParameter.Value - parameter.Value);

            _velocities[index] = velocity;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double GetWinFraction(int score, int winScale) => 1d / (1d + Math.Pow(10d, -1d * score / winScale)); // Use a sigmoid function to map score to win fraction.
}