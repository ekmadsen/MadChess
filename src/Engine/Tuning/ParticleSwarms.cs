// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ErikTheCoder.MadChess.Core;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Engine.Config;
using ErikTheCoder.MadChess.Engine.Hashtable;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Engine.Intelligence;


namespace ErikTheCoder.MadChess.Engine.Tuning;


public sealed class ParticleSwarms : List<ParticleSwarm>
{
    public const double Influence = 0.375d;
    private readonly AdvancedConfig _advancedConfig;
    private readonly Messenger _messenger;
    private readonly double _originalEvaluationError;
    private int _iterations;


    public ParticleSwarms(AdvancedConfig advancedConfig, Messenger messenger, string pgnFilename, int particleSwarms, int particlesPerSwarm, int winScale)
    {
        _advancedConfig = advancedConfig;
        _messenger = messenger;

        // Load games.
        messenger.WriteLine("Loading games.");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var board = new Board(messenger);
        var pgnGames = new PgnGames(messenger);
        pgnGames.Load(board, pgnFilename);
        stopwatch.Stop();

        // Count positions.
        long positions = 0;
        for (var gameIndex = 0; gameIndex < pgnGames.Count; gameIndex++)
        {
            var pgnGame = pgnGames[gameIndex];
            positions += pgnGame.Moves.Count;
        }

        var positionsPerSecond = (int)(positions / stopwatch.Elapsed.TotalSeconds);
        messenger.WriteLine($"Loaded {pgnGames.Count:n0} games with {positions:n0} positions in {stopwatch.Elapsed.TotalSeconds:0.000} seconds ({positionsPerSecond:n0} positions per second).");

        stopwatch.Restart();
        messenger.WriteLine("Creating data structures.");

        // Create parameters and particle swarms.
        var parameters = CreateParameters();
        for (var particleSwarmIndex = 0; particleSwarmIndex < particleSwarms; particleSwarmIndex++)
        {
            var particleSwarm = new ParticleSwarm(pgnGames, parameters, particlesPerSwarm, winScale);
            Add(particleSwarm);
        }

        var timeManagement = new TimeManagement(messenger);
        var stats = new Stats();
        var cache = new Cache(stats, 1);
        var killerMoves = new KillerMoves();
        var moveHistory = new MoveHistory();
        var evaluation = new Evaluation(_advancedConfig.LimitStrength.Evaluation, messenger, stats);
        var search = new Search(_advancedConfig.LimitStrength.Search, messenger, timeManagement, stats, cache, killerMoves, moveHistory, evaluation);

        // Set default parameters for all particles.
        for (var particleSwarmIndex = 0; particleSwarmIndex < Count; particleSwarmIndex++)
        {
            var particleSwarm = this[particleSwarmIndex];
            for (var particleIndex = 0; particleIndex < particleSwarm.Particles.Count; particleIndex++)
            {
                var particle = particleSwarm.Particles[particleIndex];
                particle.SetDefaultParameters();
                particle.ConfigureEvaluation(evaluation);
            }
        }

        // Set default parameters for one particle and determine original evaluation error.
        var firstParticleInFirstSwarm = this[0].Particles[0];
        //firstParticleInFirstSwarm.SetDefaultParameters();
        //firstParticleInFirstSwarm.ConfigureEvaluation(evaluation);
        firstParticleInFirstSwarm.CalculateEvaluationError(board, search, winScale);
        _originalEvaluationError = firstParticleInFirstSwarm.EvaluationError;

        stopwatch.Stop();
        messenger.WriteLine($"Created data structures in {stopwatch.Elapsed.TotalSeconds:0.000} seconds.");
    }


    public static Parameters CreateParameters() =>
    [
        // Material
        new Parameter(nameof(EvaluationConfig.EgPawnMaterial), 50, 200),
        new Parameter(nameof(EvaluationConfig.MgKnightMaterial), 200, 900),
        new Parameter(nameof(EvaluationConfig.EgKnightMaterial), 200, 900),
        new Parameter(nameof(EvaluationConfig.MgBishopMaterial), 200, 900),
        new Parameter(nameof(EvaluationConfig.EgBishopMaterial), 200, 900),
        new Parameter(nameof(EvaluationConfig.MgRookMaterial), 400, 2000),
        new Parameter(nameof(EvaluationConfig.EgRookMaterial), 400, 2000),
        new Parameter(nameof(EvaluationConfig.MgQueenMaterial), 800, 4000),
        new Parameter(nameof(EvaluationConfig.EgQueenMaterial), 800, 4000),

        // Passed Pawns
        new Parameter(nameof(EvaluationConfig.PassedPawnPowerPer128), 128, 512),
        new Parameter(nameof(EvaluationConfig.MgPassedPawnScalePer128), 0, 256),
        new Parameter(nameof(EvaluationConfig.EgPassedPawnScalePer128), 64, 512),
        new Parameter(nameof(EvaluationConfig.EgFreePassedPawnScalePer128), 128, 1024),
        new Parameter(nameof(EvaluationConfig.EgConnectedPassedPawnScalePer128), 64, 512),
        new Parameter(nameof(EvaluationConfig.EgKingEscortedPassedPawn), 0, 32),

        // King Safety
        new Parameter(nameof(EvaluationConfig.MgKingSafetyPowerPer128), 128, 512),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyScalePer128), 0, 128),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyKnightAttackOuterRingPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyKnightAttackInnerRingPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyKnightProximityPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyBishopAttackOuterRingPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyBishopAttackInnerRingPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyBishopProximityPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyRookAttackOuterRingPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyRookAttackInnerRingPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyRookProximityPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyQueenAttackOuterRingPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyQueenAttackInnerRingPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyQueenProximityPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetySemiOpenFilePer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyPawnShieldPer8), 0, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSafetyDefendingPiecesPer8), 0, 32),

        // Pawn Location
        new Parameter(nameof(EvaluationConfig.MgPawnAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgPawnAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgPawnSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgPawnSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgPawnFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgPawnFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgPawnCorner), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgPawnCorner), -32, 32),

        // Knight Location
        new Parameter(nameof(EvaluationConfig.MgKnightAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgKnightAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgKnightSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgKnightSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgKnightFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgKnightFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgKnightCorner), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgKnightCorner), -32, 32),

        // Bishop Location
        new Parameter(nameof(EvaluationConfig.MgBishopAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgBishopAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgBishopSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgBishopSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgBishopFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgBishopFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgBishopCorner), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgBishopCorner), -32, 32),

        // Rook Location
        new Parameter(nameof(EvaluationConfig.MgRookAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgRookAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgRookSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgRookSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgRookFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgRookFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgRookCorner), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgRookCorner), -32, 32),

        // Queen Location
        new Parameter(nameof(EvaluationConfig.MgQueenAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgQueenAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgQueenSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgQueenSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgQueenFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgQueenFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgQueenCorner), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgQueenCorner), -32, 32),

        // King Location
        new Parameter(nameof(EvaluationConfig.MgKingAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgKingAdvancement), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgKingSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgKingSquareCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgKingFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgKingFileCentrality), -32, 32),
        new Parameter(nameof(EvaluationConfig.MgKingCorner), -32, 32),
        new Parameter(nameof(EvaluationConfig.EgKingCorner), -32, 32),

        // Piece Mobility
        new Parameter(nameof(EvaluationConfig.PieceMobilityPowerPer128), 0, 256),
        new Parameter(nameof(EvaluationConfig.MgKnightMobilityScale), 0, 128),
        new Parameter(nameof(EvaluationConfig.EgKnightMobilityScale), 0, 256),
        new Parameter(nameof(EvaluationConfig.MgBishopMobilityScale), 0, 128),
        new Parameter(nameof(EvaluationConfig.EgBishopMobilityScale), 0, 256),
        new Parameter(nameof(EvaluationConfig.MgRookMobilityScale), 0, 256),
        new Parameter(nameof(EvaluationConfig.EgRookMobilityScale), 0, 256),
        new Parameter(nameof(EvaluationConfig.MgQueenMobilityScale), 0, 128),
        new Parameter(nameof(EvaluationConfig.EgQueenMobilityScale), 0, 128),

        // Pawn Structure
        new Parameter(nameof(EvaluationConfig.MgIsolatedPawn), 0, 64),
        new Parameter(nameof(EvaluationConfig.EgIsolatedPawn), 0, 64),
        new Parameter(nameof(EvaluationConfig.MgDoubledPawn), 0, 64),
        new Parameter(nameof(EvaluationConfig.EgDoubledPawn), 0, 64),

        // Threats
        new Parameter(nameof(EvaluationConfig.MgPawnThreatenMinor), 0, 64),
        new Parameter(nameof(EvaluationConfig.EgPawnThreatenMinor), 0, 64),
        new Parameter(nameof(EvaluationConfig.MgPawnThreatenMajor), 0, 128),
        new Parameter(nameof(EvaluationConfig.EgPawnThreatenMajor), 0, 128),
        new Parameter(nameof(EvaluationConfig.MgMinorThreatenMajor), 0, 64),
        new Parameter(nameof(EvaluationConfig.EgMinorThreatenMajor), 0, 64),

        // Minor Pieces
        new Parameter(nameof(EvaluationConfig.MgBishopPair), 0, 128),
        new Parameter(nameof(EvaluationConfig.EgBishopPair), 0, 256),
        new Parameter(nameof(EvaluationConfig.MgKnightOutpost), 0, 128),
        new Parameter(nameof(EvaluationConfig.EgKnightOutpost), 0, 128),
        new Parameter(nameof(EvaluationConfig.MgBishopOutpost), 0, 128),
        new Parameter(nameof(EvaluationConfig.EgBishopOutpost), 0, 64),

        // Major Pieces
        new Parameter(nameof(EvaluationConfig.MgRook7thRank), 0, 128),
        new Parameter(nameof(EvaluationConfig.EgRook7thRank), 0, 64)
    ];


    public void Optimize(int iterations)
    {
        // Determine size of parameter space.
        var parameterSpace = 1d;
        var firstParticleInFirstSwarm = this[0].Particles[0];
        for (var index = 0; index < firstParticleInFirstSwarm.Parameters.Count; index++)
        {
            var parameter = firstParticleInFirstSwarm.Parameters[index];
            parameterSpace *= parameter.MaxValue - parameter.MinValue + 1;
        }

        _messenger.WriteLine($"Optimizing {firstParticleInFirstSwarm.Parameters.Count} parameters in a space of {parameterSpace:e2} discrete parameter combinations.");

        // Create game objects for each particle swarm.
        var boards = new Board[Count];
        var searches = new Search[Count];
        var evals = new Evaluation[Count];

        for (var index = 0; index < Count; index++)
        {
            var board = new Board(_messenger);
            boards[index] = board;

            var timeManagement = new TimeManagement(_messenger);
            var stats = new Stats();
            var cache = new Cache(stats, 1);
            var killerMoves = new KillerMoves();
            var moveHistory = new MoveHistory();

            var evaluation = new Evaluation(_advancedConfig.LimitStrength.Evaluation, _messenger, stats);
            evals[index] = evaluation;

            searches[index] = new Search(_advancedConfig.LimitStrength.Search, _messenger, timeManagement, stats, cache, killerMoves, moveHistory, evaluation);
        }

        var tasks = new Task[Count];
        var bestEvaluationError = double.MaxValue;

        for (var iteration = 1; iteration <= iterations; iteration++)
        {
            // Run iteration tasks on threadpool.
            _iterations = iteration;

            for (var index = 0; index < Count; index++)
            {
                var particleSwarm = this[index];

                var board = boards[index];
                var search = searches[index];
                var evaluation = evals[index];

                tasks[index] = Task.Run(() => particleSwarm.Iterate(board, search, evaluation));
            }

            // Wait for all particle swarms to complete an iteration.
            Task.WaitAll(tasks);

            // Determine if particle swarms found a new best particle.
            var bestParticle = GetBestParticle();
            if (bestParticle.EvaluationError < bestEvaluationError) bestEvaluationError = bestParticle.BestEvaluationError;

            UpdateVelocity();
            UpdateStatus();
        }
    }


    private void UpdateVelocity()
    {
        var bestParticle = GetBestParticle();

        for (var index = 0; index < Count; index++)
        {
            var particleSwarm = this[index];
            particleSwarm.UpdateVelocity(bestParticle);
        }
    }


    private Particle GetBestParticle()
    {
        var bestParticle = this[0].GetBestParticle();

        for (var index = 1; index < Count; index++)
        {
            var particle = this[index].GetBestParticle();
            if (particle.BestEvaluationError < bestParticle.BestEvaluationError) bestParticle = particle;
        }

        return bestParticle;
    }


    private void UpdateStatus()
    {
        const int padding = 39;

        // Display iterations and original evaluation error.
        _messenger.WriteLine(null);
        _messenger.WriteLine($"{"Iterations",-padding} = {_iterations,6:000}    ");
        _messenger.WriteLine($"{"Original Evaluation Error",-padding} = {_originalEvaluationError,10:0.000}");

        // Display globally best evaluation error.
        var bestParticle = GetBestParticle();
        _messenger.WriteLine($"{"Best Evaluation Error",-padding} = {bestParticle.BestEvaluationError,10:0.000}");
        _messenger.WriteLine(null);

        // Display evaluation error of every particle in every particle swarm.
        for (var swarmIndex = 0; swarmIndex < Count; swarmIndex++)
        {
            // Display evaluation error of best particle in swarm.
            var particleSwarm = this[swarmIndex];
            var bestSwarmParticle = particleSwarm.GetBestParticle();
            _messenger.WriteLine($"Particle Swarm {swarmIndex:00} Best Evaluation Error = {bestSwarmParticle.BestEvaluationError,10:0.000}");

            // Display evaluation error of all particles in swarm.
            for (var particleIndex = 0; particleIndex < particleSwarm.Particles.Count; particleIndex++)
            {
                var particle = particleSwarm.Particles[particleIndex];
                _messenger.WriteLine($"  Particle {particleIndex:00} Evaluation Error          = {particle.EvaluationError,10:0.000}");
            }
        }

        _messenger.WriteLine(null);

        // Display globally best parameter values.
        for (var parameterIndex = 0; parameterIndex < bestParticle.BestParameters.Count; parameterIndex++)
        {
            var parameter = bestParticle.BestParameters[parameterIndex];
            _messenger.WriteLine($"{parameter.Name,-padding} = {parameter.Value,6}");
        }
    }
}