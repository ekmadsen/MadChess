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
using ErikTheCoder.MadChess.Engine.Evaluation;
using ErikTheCoder.MadChess.Engine.Hashtable;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Engine.Intelligence;


namespace ErikTheCoder.MadChess.Engine.Tuning;


public sealed class ParticleSwarms : List<ParticleSwarm>
{
    public const double Influence = 0.375d;
    private readonly Messenger _messenger; // Lifetime managed by caller.
    private readonly double _originalEvaluationError;
    private int _iterations;


    public ParticleSwarms(Messenger messenger, string pgnFilename, int particleSwarms, int particlesPerSwarm, int winScale)
    {
        _messenger = messenger;

        // Load games.
        messenger.WriteMessageLine("Loading games.");
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
        messenger.WriteMessageLine($"Loaded {pgnGames.Count:n0} games with {positions:n0} positions in {stopwatch.Elapsed.TotalSeconds:0.000} seconds ({positionsPerSecond:n0} positions per second).");
        
        stopwatch.Restart();
        messenger.WriteMessageLine("Creating data structures.");

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
        var eval = new Eval(messenger, stats);
        var search = new Search(messenger, stats, cache, killerMoves, moveHistory, eval);
        
        // Set default parameters for one particle and determine original evaluation error.
        var firstParticleInFirstSwarm = this[0].Particles[0];
        firstParticleInFirstSwarm.SetDefaultParameters();
        firstParticleInFirstSwarm.ConfigureEvaluation(eval);
        firstParticleInFirstSwarm.CalculateEvaluationError(board, search, winScale);

        _originalEvaluationError = firstParticleInFirstSwarm.EvaluationError;
        
        stopwatch.Stop();
        messenger.WriteMessageLine($"Created data structures in {stopwatch.Elapsed.TotalSeconds:0.000} seconds.");
    }


    public static Parameters CreateParameters()
    {
        return new Parameters
        {
            // Endgame Material
            new(nameof(EvalConfig.EgPawnMaterial), 50, 200),
            new(nameof(EvalConfig.EgKnightMaterial), 200, 900),
            new(nameof(EvalConfig.EgBishopMaterial), 200, 900),
            new(nameof(EvalConfig.EgRookMaterial), 400, 2000),
            new(nameof(EvalConfig.EgQueenMaterial), 800, 4000),

            // Passed Pawns
            new(nameof(EvalConfig.PassedPawnPowerPer128), 128, 512),
            new(nameof(EvalConfig.MgPassedPawnScalePer128), 0, 256),
            new(nameof(EvalConfig.EgPassedPawnScalePer128), 64, 512),
            new(nameof(EvalConfig.EgFreePassedPawnScalePer128), 128, 1024),
            new(nameof(EvalConfig.EgConnectedPassedPawnScalePer128), 64, 512),
            new(nameof(EvalConfig.EgKingEscortedPassedPawn), 0, 32),

            // King Safety
            new(nameof(EvalConfig.MgKingSafetyPowerPer128), 128, 512),
            new(nameof(EvalConfig.MgKingSafetyScalePer128), 0, 128),
            new(nameof(EvalConfig.MgKingSafetyKnightAttackOuterRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyKnightAttackInnerRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyBishopAttackOuterRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyBishopAttackInnerRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyRookAttackOuterRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyRookAttackInnerRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyQueenAttackOuterRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyQueenAttackInnerRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetySemiOpenFilePer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyPawnShieldPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyDefendingPiecesPer8), 0, 32),

            // Pawn Location
            new(nameof(EvalConfig.MgPawnAdvancement), 0, 32),
            new(nameof(EvalConfig.EgPawnAdvancement), 0, 32),
            new(nameof(EvalConfig.MgPawnCentrality), 0, 64),
            new(nameof(EvalConfig.EgPawnCentrality), -32, 32),

            // Knight Location
            new(nameof(EvalConfig.MgKnightAdvancement), -32, 32),
            new(nameof(EvalConfig.EgKnightAdvancement), 0, 32),
            new(nameof(EvalConfig.MgKnightCentrality), 0, 32),
            new(nameof(EvalConfig.EgKnightCentrality), 0, 32),
            new(nameof(EvalConfig.MgKnightCorner), -32, 0),
            new(nameof(EvalConfig.EgKnightCorner), -32, 0),

            // Bishop Location
            new(nameof(EvalConfig.MgBishopAdvancement), -32, 32),
            new(nameof(EvalConfig.EgBishopAdvancement), 0, 32),
            new(nameof(EvalConfig.MgBishopCentrality), 0, 32),
            new(nameof(EvalConfig.EgBishopCentrality), 0, 32),
            new(nameof(EvalConfig.MgBishopCorner), -32, 0),
            new(nameof(EvalConfig.EgBishopCorner), -32, 0),

            // Rook Location
            new(nameof(EvalConfig.MgRookAdvancement), -32, 32),
            new(nameof(EvalConfig.EgRookAdvancement), 0, 32),
            new(nameof(EvalConfig.MgRookCentrality), 0, 32),
            new(nameof(EvalConfig.EgRookCentrality), -32, 32),
            new(nameof(EvalConfig.MgRookCorner), -32, 0),
            new(nameof(EvalConfig.EgRookCorner), -32, 0),

            // Queen Location
            new(nameof(EvalConfig.MgQueenAdvancement), -32, 0),
            new(nameof(EvalConfig.EgQueenAdvancement), 0, 32),
            new(nameof(EvalConfig.MgQueenCentrality), 0, 32),
            new(nameof(EvalConfig.EgQueenCentrality), -32, 32),
            new(nameof(EvalConfig.MgQueenCorner), -32, 0),
            new(nameof(EvalConfig.EgQueenCorner), -32, 0),

            // King Location
            new(nameof(EvalConfig.MgKingAdvancement), -64, 0),
            new(nameof(EvalConfig.EgKingAdvancement), 0, 64),
            new(nameof(EvalConfig.MgKingCentrality), -32, 0),
            new(nameof(EvalConfig.EgKingCentrality), 0, 32),
            new(nameof(EvalConfig.MgKingCorner), 0, 32),
            new(nameof(EvalConfig.EgKingCorner), -32, 0),

            // Piece Mobility
            new(nameof(EvalConfig.PieceMobilityPowerPer128), 0, 256),
            new(nameof(EvalConfig.MgKnightMobilityScale), 0, 128),
            new(nameof(EvalConfig.EgKnightMobilityScale), 0, 256),
            new(nameof(EvalConfig.MgBishopMobilityScale), 0, 128),
            new(nameof(EvalConfig.EgBishopMobilityScale), 0, 256),
            new(nameof(EvalConfig.MgRookMobilityScale), 0, 256),
            new(nameof(EvalConfig.EgRookMobilityScale), 0, 256),
            new(nameof(EvalConfig.MgQueenMobilityScale), 0, 128),
            new(nameof(EvalConfig.EgQueenMobilityScale), 0, 128),

            // Pawn Structure
            new(nameof(EvalConfig.MgIsolatedPawn), 0, 64),
            new(nameof(EvalConfig.EgIsolatedPawn), 0, 64),
            new(nameof(EvalConfig.MgDoubledPawn), 0, 64),
            new(nameof(EvalConfig.EgDoubledPawn), 0, 64),

            // Threats
            new(nameof(EvalConfig.MgPawnThreatenMinor), 0, 64),
            new(nameof(EvalConfig.EgPawnThreatenMinor), 0, 64),
            new(nameof(EvalConfig.MgPawnThreatenMajor), 0, 128),
            new(nameof(EvalConfig.EgPawnThreatenMajor), 0, 128),
            new(nameof(EvalConfig.MgMinorThreatenMajor), 0, 64),
            new(nameof(EvalConfig.EgMinorThreatenMajor), 0, 64),

            // Minor Pieces
            new(nameof(EvalConfig.MgBishopPair), 0, 128),
            new(nameof(EvalConfig.EgBishopPair), 0, 256),
            new(nameof(EvalConfig.MgKnightOutpost), 0, 128),
            new(nameof(EvalConfig.EgKnightOutpost), 0, 128),
            new(nameof(EvalConfig.MgBishopOutpost), 0, 128),
            new(nameof(EvalConfig.EgBishopOutpost), 0, 64),

            // Major Pieces
            new(nameof(EvalConfig.MgRook7thRank), 0, 128),
            new(nameof(EvalConfig.EgRook7thRank), 0, 64)
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

        _messenger.WriteMessageLine($"Optimizing {firstParticleInFirstSwarm.Parameters.Count} parameters in a space of {parameterSpace:e2} discrete parameter combinations.");

        // Create game objects for each particle swarm.
        var boards = new Board[Count];
        var searches = new Search[Count];
        var evals = new Eval[Count];

        for (var index = 0; index < Count; index++)
        {
            var board = new Board(_messenger);
            boards[index] = board;

            var stats = new Stats();
            var cache = new Cache(stats, 1);
            var killerMoves = new KillerMoves();
            var moveHistory = new MoveHistory();

            var eval = new Eval(_messenger, stats);
            evals[index] = eval;

            searches[index] = new Search(_messenger, stats, cache, killerMoves, moveHistory, eval);
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
        _messenger.WriteMessageLine(null);
        _messenger.WriteMessageLine($"{"Iterations",-padding} = {_iterations,6:000}    ");
        _messenger.WriteMessageLine($"{"Original Evaluation Error",-padding} = {_originalEvaluationError,10:0.000}");

        // Display globally best evaluation error.
        var bestParticle = GetBestParticle();
        _messenger.WriteMessageLine($"{"Best Evaluation Error",-padding} = {bestParticle.BestEvaluationError,10:0.000}");
        _messenger.WriteMessageLine(null);

        // Display evaluation error of every particle in every particle swarm.
        for (var swarmIndex = 0; swarmIndex < Count; swarmIndex++)
        {
            // Display evaluation error of best particle in swarm.
            var particleSwarm = this[swarmIndex];
            var bestSwarmParticle = particleSwarm.GetBestParticle();
            _messenger.WriteMessageLine($"Particle Swarm {swarmIndex:00} Best Evaluation Error = {bestSwarmParticle.BestEvaluationError,10:0.000}");

            // Display evaluation error of all particles in swarm.
            for (var particleIndex = 0; particleIndex < particleSwarm.Particles.Count; particleIndex++)
            {
                var particle = particleSwarm.Particles[particleIndex];
                _messenger.WriteMessageLine($"  Particle {particleIndex:00} Evaluation Error          = {particle.EvaluationError,10:0.000}");
            }
        }

        _messenger.WriteMessageLine(null);

        // Display globally best parameter values.
        for (var parameterIndex = 0; parameterIndex < bestParticle.BestParameters.Count; parameterIndex++)
        {
            var parameter = bestParticle.BestParameters[parameterIndex];
            _messenger.WriteMessageLine($"{parameter.Name,-padding} = {parameter.Value,6}");
        }
    }
}