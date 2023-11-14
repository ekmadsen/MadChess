// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
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
        for (var particleSwarmsIndex = 0; particleSwarmsIndex < particleSwarms; particleSwarmsIndex++)
        {
            var particleSwarm = new ParticleSwarm(pgnGames, parameters, particlesPerSwarm, winScale);
            Add(particleSwarm);
        }

        var stats = new Stats();
        var cache = new Cache(stats, 1);
        var killerMoves = new KillerMoves();
        var moveHistory = new MoveHistory();
        var evaluation = new Evaluation(_advancedConfig.LimitStrength.Evaluation, messenger, stats);
        var search = new Search(_advancedConfig.LimitStrength.Search, messenger, stats, cache, killerMoves, moveHistory, evaluation);



        foreach (var particleSwarm in this)
        {
            foreach (var particle in particleSwarm.Particles)
            {
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


    public static Parameters CreateParameters()
    {
        return new Parameters
        {
            // Material
            //new(nameof(EvaluationConfig.EgPawnMaterial), 50, 200),
            //new(nameof(EvaluationConfig.MgKnightMaterial), 200, 900),
            //new(nameof(EvaluationConfig.EgKnightMaterial), 200, 900),
            //new(nameof(EvaluationConfig.MgBishopMaterial), 200, 900),
            //new(nameof(EvaluationConfig.EgBishopMaterial), 200, 900),
            //new(nameof(EvaluationConfig.MgRookMaterial), 400, 2000),
            //new(nameof(EvaluationConfig.EgRookMaterial), 400, 2000),
            //new(nameof(EvaluationConfig.MgQueenMaterial), 800, 4000),
            //new(nameof(EvaluationConfig.EgQueenMaterial), 800, 4000),

            //// Passed Pawns
            //new(nameof(EvaluationConfig.PassedPawnPowerPer128), 128, 512),
            //new(nameof(EvaluationConfig.MgPassedPawnScalePer128), 0, 256),
            //new(nameof(EvaluationConfig.EgPassedPawnScalePer128), 64, 512),
            //new(nameof(EvaluationConfig.EgFreePassedPawnScalePer128), 128, 1024),
            //new(nameof(EvaluationConfig.EgConnectedPassedPawnScalePer128), 64, 512),
            //new(nameof(EvaluationConfig.EgKingEscortedPassedPawn), 0, 32),

            // King Safety
            new(nameof(EvaluationConfig.MgKingSafetyPowerPer128), 128, 512),
            new(nameof(EvaluationConfig.MgKingSafetyScalePer128), 0, 128),
            new(nameof(EvaluationConfig.MgKingSafetyKnightAttackOuterRingPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyKnightAttackInnerRingPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyKnightProximityPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyBishopAttackOuterRingPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyBishopAttackInnerRingPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyBishopProximityPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyRookAttackOuterRingPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyRookAttackInnerRingPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyRookProximityPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyQueenAttackOuterRingPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyQueenAttackInnerRingPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyQueenProximityPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetySemiOpenFilePer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyPawnShieldPer8), 0, 32),
            new(nameof(EvaluationConfig.MgKingSafetyDefendingPiecesPer8), 0, 32),

            // Pawn Location
            //new(nameof(EvaluationConfig.MgPawnAdvancement), 0, 32),
            //new(nameof(EvaluationConfig.EgPawnAdvancement), 0, 32),
            //new(nameof(EvaluationConfig.MgPawnCentrality), 0, 64),
            //new(nameof(EvaluationConfig.EgPawnCentrality), -32, 32),

            //// Knight Location
            //new(nameof(EvaluationConfig.MgKnightAdvancement), -32, 32),
            //new(nameof(EvaluationConfig.EgKnightAdvancement), 0, 32),
            //new(nameof(EvaluationConfig.MgKnightCentrality), 0, 32),
            //new(nameof(EvaluationConfig.EgKnightCentrality), 0, 32),
            //new(nameof(EvaluationConfig.MgKnightCorner), -32, 0),
            //new(nameof(EvaluationConfig.EgKnightCorner), -32, 0),

            //// Bishop Location
            //new(nameof(EvaluationConfig.MgBishopAdvancement), -32, 32),
            //new(nameof(EvaluationConfig.EgBishopAdvancement), 0, 32),
            //new(nameof(EvaluationConfig.MgBishopCentrality), 0, 32),
            //new(nameof(EvaluationConfig.EgBishopCentrality), 0, 32),
            //new(nameof(EvaluationConfig.MgBishopCorner), -32, 0),
            //new(nameof(EvaluationConfig.EgBishopCorner), -32, 0),

            //// Rook Location
            //new(nameof(EvaluationConfig.MgRookAdvancement), -32, 32),
            //new(nameof(EvaluationConfig.EgRookAdvancement), 0, 32),
            //new(nameof(EvaluationConfig.MgRookCentrality), 0, 32),
            //new(nameof(EvaluationConfig.EgRookCentrality), -32, 32),
            //new(nameof(EvaluationConfig.MgRookCorner), -32, 0),
            //new(nameof(EvaluationConfig.EgRookCorner), -32, 0),

            //// Queen Location
            //new(nameof(EvaluationConfig.MgQueenAdvancement), -32, 0),
            //new(nameof(EvaluationConfig.EgQueenAdvancement), 0, 32),
            //new(nameof(EvaluationConfig.MgQueenCentrality), 0, 32),
            //new(nameof(EvaluationConfig.EgQueenCentrality), -32, 32),
            //new(nameof(EvaluationConfig.MgQueenCorner), -32, 0),
            //new(nameof(EvaluationConfig.EgQueenCorner), -32, 0),

            //// King Location
            //new(nameof(EvaluationConfig.MgKingAdvancement), -64, 0),
            //new(nameof(EvaluationConfig.EgKingAdvancement), 0, 64),
            //new(nameof(EvaluationConfig.MgKingCentrality), -32, 0),
            //new(nameof(EvaluationConfig.EgKingCentrality), 0, 32),
            //new(nameof(EvaluationConfig.MgKingCorner), 0, 32),
            //new(nameof(EvaluationConfig.EgKingCorner), -32, 0),

            //// Piece Mobility
            //new(nameof(EvaluationConfig.PieceMobilityPowerPer128), 0, 256),
            //new(nameof(EvaluationConfig.MgKnightMobilityScale), 0, 128),
            //new(nameof(EvaluationConfig.EgKnightMobilityScale), 0, 256),
            //new(nameof(EvaluationConfig.MgBishopMobilityScale), 0, 128),
            //new(nameof(EvaluationConfig.EgBishopMobilityScale), 0, 256),
            //new(nameof(EvaluationConfig.MgRookMobilityScale), 0, 256),
            //new(nameof(EvaluationConfig.EgRookMobilityScale), 0, 256),
            //new(nameof(EvaluationConfig.MgQueenMobilityScale), 0, 128),
            //new(nameof(EvaluationConfig.EgQueenMobilityScale), 0, 128),

            //// Pawn Structure
            //new(nameof(EvaluationConfig.MgIsolatedPawn), 0, 64),
            //new(nameof(EvaluationConfig.EgIsolatedPawn), 0, 64),
            //new(nameof(EvaluationConfig.MgDoubledPawn), 0, 64),
            //new(nameof(EvaluationConfig.EgDoubledPawn), 0, 64),

            //// Threats
            //new(nameof(EvaluationConfig.MgPawnThreatenMinor), 0, 64),
            //new(nameof(EvaluationConfig.EgPawnThreatenMinor), 0, 64),
            //new(nameof(EvaluationConfig.MgPawnThreatenMajor), 0, 128),
            //new(nameof(EvaluationConfig.EgPawnThreatenMajor), 0, 128),
            //new(nameof(EvaluationConfig.MgMinorThreatenMajor), 0, 64),
            //new(nameof(EvaluationConfig.EgMinorThreatenMajor), 0, 64),

            //// Minor Pieces
            //new(nameof(EvaluationConfig.MgBishopPair), 0, 128),
            //new(nameof(EvaluationConfig.EgBishopPair), 0, 256),
            //new(nameof(EvaluationConfig.MgKnightOutpost), 0, 128),
            //new(nameof(EvaluationConfig.EgKnightOutpost), 0, 128),
            //new(nameof(EvaluationConfig.MgBishopOutpost), 0, 128),
            //new(nameof(EvaluationConfig.EgBishopOutpost), 0, 64),

            //// Major Pieces
            //new(nameof(EvaluationConfig.MgRook7thRank), 0, 128),
            //new(nameof(EvaluationConfig.EgRook7thRank), 0, 64)
        };
    }


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

            var stats = new Stats();
            var cache = new Cache(stats, 1);
            var killerMoves = new KillerMoves();
            var moveHistory = new MoveHistory();

            var evaluation = new Evaluation(_advancedConfig.LimitStrength.Evaluation, _messenger, stats);
            evals[index] = evaluation;

            searches[index] = new Search(_advancedConfig.LimitStrength.Search, _messenger, stats, cache, killerMoves, moveHistory, evaluation);
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