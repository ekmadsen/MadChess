// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Engine.Evaluation;
using ErikTheCoder.MadChess.Engine.Hashtable;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Engine.Intelligence;
using ErikTheCoder.MadChess.Engine.Uci;


namespace ErikTheCoder.MadChess.Engine.Tuning;


public sealed class ParticleSwarms : List<ParticleSwarm>
{
    public const double Influence = 0.375d;
    private readonly Delegates.DisplayStats _displayStats;
    private readonly Core.Delegates.WriteMessageLine _writeMessageLine;
    private readonly double _originalEvaluationError;
    private int _iterations;


    public ParticleSwarms(string pgnFilename, int particleSwarms, int particlesPerSwarm, int winScale, Delegates.DisplayStats displayStats, Core.Delegates.WriteMessageLine writeMessageLine)
    {
        _displayStats = displayStats;
        _writeMessageLine = writeMessageLine;

        // Load games.
        writeMessageLine("Loading games.");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var board = new Board(writeMessageLine, UciStream.NodesInfoInterval);
        var pgnGames = new PgnGames();
        pgnGames.Load(board, pgnFilename, writeMessageLine);
        stopwatch.Stop();

        // Count positions.
        long positions = 0;
        for (var gameIndex = 0; gameIndex < pgnGames.Count; gameIndex++)
        {
            var pgnGame = pgnGames[gameIndex];
            positions += pgnGame.Moves.Count;
        }

        var positionsPerSecond = (int)(positions / stopwatch.Elapsed.TotalSeconds);
        writeMessageLine($"Loaded {pgnGames.Count:n0} games with {positions:n0} positions in {stopwatch.Elapsed.TotalSeconds:0.000} seconds ({positionsPerSecond:n0} positions per second).");
        
        stopwatch.Restart();
        writeMessageLine("Creating data structures.");

        // Create parameters and particle swarms.
        var parameters = CreateParameters();
        for (var particleSwarmsIndex = 0; particleSwarmsIndex < particleSwarms; particleSwarmsIndex++)
        {
            var particleSwarm = new ParticleSwarm(pgnGames, parameters, particlesPerSwarm, winScale);
            Add(particleSwarm);
        }

        var stats = new Stats();
        var cache = new Cache(1, stats, board.ValidateMove);
        var killerMoves = new KillerMoves();
        var moveHistory = new MoveHistory();
        var eval = new Eval(stats, board.IsRepeatPosition, () => false, writeMessageLine);
        var search = new Search(stats, cache, killerMoves, moveHistory, eval, () => false, displayStats, writeMessageLine);
        
        // Set parameters of first particle in first swarm to known best.
        var firstParticleInFirstSwarm = this[0].Particles[0];
        SetDefaultParameters(firstParticleInFirstSwarm.Parameters);
        firstParticleInFirstSwarm.ConfigureEvaluation(eval);
        firstParticleInFirstSwarm.CalculateEvaluationError(board, search, winScale);

        _originalEvaluationError = firstParticleInFirstSwarm.EvaluationError;
        
        stopwatch.Stop();
        writeMessageLine($"Created data structures in {stopwatch.Elapsed.TotalSeconds:0.000} seconds.");
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
            new(nameof(EvalConfig.EgKingEscortedPassedPawn), 0, 32),

            // King Safety
            new(nameof(EvalConfig.MgKingSafetyPowerPer128), 128, 512),
            new(nameof(EvalConfig.MgKingSafetyScalePer128), 0, 128),
            new(nameof(EvalConfig.MgKingSafetyMinorAttackOuterRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyMinorAttackInnerRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyRookAttackOuterRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyRookAttackInnerRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyQueenAttackOuterRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyQueenAttackInnerRingPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetySemiOpenFilePer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyPawnShieldPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyDefendingPiecesPer8), 0, 32),
            new(nameof(EvalConfig.MgKingSafetyAttackingPiecesPer8), 0, 32),

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
            new(nameof(EvalConfig.EgBishopPair), 0, 128),
            new(nameof(EvalConfig.MgKnightOutpost), 0, 128),
            new(nameof(EvalConfig.EgKnightOutpost), 0, 128),
            new(nameof(EvalConfig.MgBishopOutpost), 0, 128),
            new(nameof(EvalConfig.EgBishopOutpost), 0, 64),

            // Major Pieces
            new(nameof(EvalConfig.MgRook7thRank), 0, 128),
            new(nameof(EvalConfig.EgRook7thRank), 0, 64),

            // Endgame Scale
            new(nameof(EvalConfig.EgScaleMinorAdvantage), 0, 128),
            new(nameof(EvalConfig.EgScaleOppBishopsPerPassedPawn), 0, 128),
            new(nameof(EvalConfig.EgScalePerPawnAdvantage), 0, 64)
        };
    }


    private static void SetDefaultParameters(Parameters parameters)
    {
        var evalConfig = new EvalConfig();

        // Endgame Material
        parameters[nameof(evalConfig.EgPawnMaterial)].Value = evalConfig.EgPawnMaterial;
        parameters[nameof(evalConfig.EgKnightMaterial)].Value = evalConfig.EgKnightMaterial;
        parameters[nameof(evalConfig.EgBishopMaterial)].Value = evalConfig.EgBishopMaterial;
        parameters[nameof(evalConfig.EgRookMaterial)].Value = evalConfig.EgRookMaterial;
        parameters[nameof(evalConfig.EgQueenMaterial)].Value = evalConfig.EgQueenMaterial;

        // Passed Pawns
        parameters[nameof(evalConfig.PassedPawnPowerPer128)].Value = evalConfig.PassedPawnPowerPer128;
        parameters[nameof(evalConfig.MgPassedPawnScalePer128)].Value = evalConfig.MgPassedPawnScalePer128;
        parameters[nameof(evalConfig.EgPassedPawnScalePer128)].Value = evalConfig.EgPassedPawnScalePer128;
        parameters[nameof(evalConfig.EgFreePassedPawnScalePer128)].Value = evalConfig.EgFreePassedPawnScalePer128;
        parameters[nameof(evalConfig.EgKingEscortedPassedPawn)].Value = evalConfig.EgKingEscortedPassedPawn;

        // King Safety
        parameters[nameof(evalConfig.MgKingSafetyPowerPer128)].Value = evalConfig.MgKingSafetyPowerPer128;
        parameters[nameof(evalConfig.MgKingSafetyScalePer128)].Value = evalConfig.MgKingSafetyScalePer128;
        parameters[nameof(evalConfig.MgKingSafetyMinorAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyMinorAttackOuterRingPer8;
        parameters[nameof(evalConfig.MgKingSafetyMinorAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyMinorAttackInnerRingPer8;
        parameters[nameof(evalConfig.MgKingSafetyRookAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyRookAttackOuterRingPer8;
        parameters[nameof(evalConfig.MgKingSafetyRookAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyRookAttackInnerRingPer8;
        parameters[nameof(evalConfig.MgKingSafetyQueenAttackOuterRingPer8)].Value = evalConfig.MgKingSafetyQueenAttackOuterRingPer8;
        parameters[nameof(evalConfig.MgKingSafetyQueenAttackInnerRingPer8)].Value = evalConfig.MgKingSafetyQueenAttackInnerRingPer8;
        parameters[nameof(evalConfig.MgKingSafetySemiOpenFilePer8)].Value = evalConfig.MgKingSafetySemiOpenFilePer8;
        parameters[nameof(evalConfig.MgKingSafetyPawnShieldPer8)].Value = evalConfig.MgKingSafetyPawnShieldPer8;
        parameters[nameof(evalConfig.MgKingSafetyDefendingPiecesPer8)].Value = evalConfig.MgKingSafetyDefendingPiecesPer8;
        parameters[nameof(evalConfig.MgKingSafetyAttackingPiecesPer8)].Value = evalConfig.MgKingSafetyAttackingPiecesPer8;

        // Pawn Location
        parameters[nameof(evalConfig.MgPawnAdvancement)].Value = evalConfig.MgPawnAdvancement;
        parameters[nameof(evalConfig.EgPawnAdvancement)].Value = evalConfig.EgPawnAdvancement;
        parameters[nameof(evalConfig.MgPawnCentrality)].Value = evalConfig.MgPawnCentrality;
        parameters[nameof(evalConfig.EgPawnCentrality)].Value = evalConfig.EgPawnCentrality;

        // Knight Location
        parameters[nameof(evalConfig.MgKnightAdvancement)].Value = evalConfig.MgKnightAdvancement;
        parameters[nameof(evalConfig.EgKnightAdvancement)].Value = evalConfig.EgKnightAdvancement;
        parameters[nameof(evalConfig.MgKnightCentrality)].Value = evalConfig.MgKnightCentrality;
        parameters[nameof(evalConfig.EgKnightCentrality)].Value = evalConfig.EgKnightCentrality;
        parameters[nameof(evalConfig.MgKnightCorner)].Value = evalConfig.MgKnightCorner;
        parameters[nameof(evalConfig.EgKnightCorner)].Value = evalConfig.EgKnightCorner;

        // Bishop Location
        parameters[nameof(evalConfig.MgBishopAdvancement)].Value = evalConfig.MgBishopAdvancement;
        parameters[nameof(evalConfig.EgBishopAdvancement)].Value = evalConfig.EgBishopAdvancement;
        parameters[nameof(evalConfig.MgBishopCentrality)].Value = evalConfig.MgBishopCentrality;
        parameters[nameof(evalConfig.EgBishopCentrality)].Value = evalConfig.EgBishopCentrality;
        parameters[nameof(evalConfig.MgBishopCorner)].Value = evalConfig.MgBishopCorner;
        parameters[nameof(evalConfig.EgBishopCorner)].Value = evalConfig.EgBishopCorner;

        // Rook Location
        parameters[nameof(evalConfig.MgRookAdvancement)].Value = evalConfig.MgRookAdvancement;
        parameters[nameof(evalConfig.EgRookAdvancement)].Value = evalConfig.EgRookAdvancement;
        parameters[nameof(evalConfig.MgRookCentrality)].Value = evalConfig.MgRookCentrality;
        parameters[nameof(evalConfig.EgRookCentrality)].Value = evalConfig.EgRookCentrality;
        parameters[nameof(evalConfig.MgRookCorner)].Value = evalConfig.MgRookCorner;
        parameters[nameof(evalConfig.EgRookCorner)].Value = evalConfig.EgRookCorner;

        // Queen Location
        parameters[nameof(evalConfig.MgQueenAdvancement)].Value = evalConfig.MgQueenAdvancement;
        parameters[nameof(evalConfig.EgQueenAdvancement)].Value = evalConfig.EgQueenAdvancement;
        parameters[nameof(evalConfig.MgQueenCentrality)].Value = evalConfig.MgQueenCentrality;
        parameters[nameof(evalConfig.EgQueenCentrality)].Value = evalConfig.EgQueenCentrality;
        parameters[nameof(evalConfig.MgQueenCorner)].Value = evalConfig.MgQueenCorner;
        parameters[nameof(evalConfig.EgQueenCorner)].Value = evalConfig.EgQueenCorner;

        // King Location
        parameters[nameof(evalConfig.MgKingAdvancement)].Value = evalConfig.MgKingAdvancement;
        parameters[nameof(evalConfig.EgKingAdvancement)].Value = evalConfig.EgKingAdvancement;
        parameters[nameof(evalConfig.MgKingCentrality)].Value = evalConfig.MgKingCentrality;
        parameters[nameof(evalConfig.EgKingCentrality)].Value = evalConfig.EgKingCentrality;
        parameters[nameof(evalConfig.MgKingCorner)].Value = evalConfig.MgKingCorner;
        parameters[nameof(evalConfig.EgKingCorner)].Value = evalConfig.EgKingCorner;

        // Piece Mobility
        parameters[nameof(evalConfig.PieceMobilityPowerPer128)].Value = evalConfig.PieceMobilityPowerPer128;
        parameters[nameof(evalConfig.MgKnightMobilityScale)].Value = evalConfig.MgKnightMobilityScale;
        parameters[nameof(evalConfig.EgKnightMobilityScale)].Value = evalConfig.EgKnightMobilityScale;
        parameters[nameof(evalConfig.MgBishopMobilityScale)].Value = evalConfig.MgBishopMobilityScale;
        parameters[nameof(evalConfig.EgBishopMobilityScale)].Value = evalConfig.EgBishopMobilityScale;
        parameters[nameof(evalConfig.MgRookMobilityScale)].Value = evalConfig.MgRookMobilityScale;
        parameters[nameof(evalConfig.EgRookMobilityScale)].Value = evalConfig.EgRookMobilityScale;
        parameters[nameof(evalConfig.MgQueenMobilityScale)].Value = evalConfig.MgQueenMobilityScale;
        parameters[nameof(evalConfig.EgQueenMobilityScale)].Value = evalConfig.EgQueenMobilityScale;

        // Pawn Structure
        parameters[nameof(evalConfig.MgIsolatedPawn)].Value = evalConfig.MgIsolatedPawn;
        parameters[nameof(evalConfig.EgIsolatedPawn)].Value = evalConfig.EgIsolatedPawn;
        parameters[nameof(evalConfig.MgDoubledPawn)].Value = evalConfig.MgDoubledPawn;
        parameters[nameof(evalConfig.EgDoubledPawn)].Value = evalConfig.EgDoubledPawn;

        // Threats
        parameters[nameof(evalConfig.MgPawnThreatenMinor)].Value = evalConfig.MgPawnThreatenMinor;
        parameters[nameof(evalConfig.EgPawnThreatenMinor)].Value = evalConfig.EgPawnThreatenMinor;
        parameters[nameof(evalConfig.MgPawnThreatenMajor)].Value = evalConfig.MgPawnThreatenMajor;
        parameters[nameof(evalConfig.EgPawnThreatenMajor)].Value = evalConfig.EgPawnThreatenMajor;
        parameters[nameof(evalConfig.MgMinorThreatenMajor)].Value = evalConfig.MgMinorThreatenMajor;
        parameters[nameof(evalConfig.EgMinorThreatenMajor)].Value = evalConfig.EgMinorThreatenMajor;

        // Minor Pieces
        parameters[nameof(evalConfig.MgBishopPair)].Value = evalConfig.MgBishopPair;
        parameters[nameof(evalConfig.EgBishopPair)].Value = evalConfig.EgBishopPair;
        parameters[nameof(evalConfig.MgKnightOutpost)].Value = evalConfig.MgKnightOutpost;
        parameters[nameof(evalConfig.EgKnightOutpost)].Value = evalConfig.EgKnightOutpost;
        parameters[nameof(evalConfig.MgBishopOutpost)].Value = evalConfig.MgBishopOutpost;
        parameters[nameof(evalConfig.EgBishopOutpost)].Value = evalConfig.EgBishopOutpost;

        // Major Pieces
        parameters[nameof(evalConfig.MgRook7thRank)].Value = evalConfig.MgRook7thRank;
        parameters[nameof(evalConfig.EgRook7thRank)].Value = evalConfig.EgRook7thRank;

        // Endgame Scale
        parameters[nameof(evalConfig.EgScaleMinorAdvantage)].Value = evalConfig.EgScaleMinorAdvantage;
        parameters[nameof(evalConfig.EgScaleOppBishopsPerPassedPawn)].Value = evalConfig.EgScaleOppBishopsPerPassedPawn;
        parameters[nameof(evalConfig.EgScalePerPawnAdvantage)].Value = evalConfig.EgScalePerPawnAdvantage;
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

        _writeMessageLine($"Optimizing {firstParticleInFirstSwarm.Parameters.Count} parameters in a space of {parameterSpace:e2} discrete parameter combinations.");

        // Create game objects for each particle swarm.
        var boards = new Board[Count];
        var searches = new Search[Count];
        var evals = new Eval[Count];

        for (var index = 0; index < Count; index++)
        {
            var board = new Board(_writeMessageLine, UciStream.NodesInfoInterval);
            boards[index] = board;

            var stats = new Stats();
            var cache = new Cache(1, stats, board.ValidateMove);
            var killerMoves = new KillerMoves();
            var moveHistory = new MoveHistory();

            var eval = new Eval(stats, board.IsRepeatPosition, () => false, _writeMessageLine);
            evals[index] = eval;

            searches[index] = new Search(stats, cache, killerMoves, moveHistory, eval, () => false, _displayStats, _writeMessageLine);
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


    private void UpdateStatus()
    {
        const int padding = 39;

        // Display iterations and original evaluation error.
        _writeMessageLine(null);
        _writeMessageLine($"{"Iterations",-padding} = {_iterations,6:000}    ");
        _writeMessageLine($"{"Original Evaluation Error",-padding} = {_originalEvaluationError,10:0.000}");

        // Display globally best evaluation error.
        var bestParticle = GetBestParticle();
        _writeMessageLine($"{"Best Evaluation Error",-padding} = {bestParticle.BestEvaluationError,10:0.000}");
        _writeMessageLine(null);

        // Display evaluation error of every particle in every particle swarm.
        for (var swarmIndex = 0; swarmIndex < Count; swarmIndex++)
        {
            // Display evaluation error of best particle in swarm.
            var particleSwarm = this[swarmIndex];
            var bestSwarmParticle = particleSwarm.GetBestParticle();
            _writeMessageLine($"Particle Swarm {swarmIndex:00} Best Evaluation Error = {bestSwarmParticle.BestEvaluationError,10:0.000}");

            // Display evaluation error of all particles in swarm.
            for (var particleIndex = 0; particleIndex < particleSwarm.Particles.Count; particleIndex++)
            {
                var particle = particleSwarm.Particles[particleIndex];
                _writeMessageLine($"  Particle {particleIndex:00} Evaluation Error          = {particle.EvaluationError,10:0.000}");
            }
        }

        _writeMessageLine(null);

        // Display globally best parameter values.
        for (var parameterIndex = 0; parameterIndex < bestParticle.BestParameters.Count; parameterIndex++)
        {
            var parameter = bestParticle.BestParameters[parameterIndex];
            _writeMessageLine($"{parameter.Name,-padding} = {parameter.Value,6}");
        }
    }
}