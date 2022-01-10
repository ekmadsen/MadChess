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
using System.IO;
using System.Threading.Tasks;
using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Engine.Evaluation;
using ErikTheCoder.MadChess.Engine.Heuristics;
using ErikTheCoder.MadChess.Engine.Uci;


namespace ErikTheCoder.MadChess.Engine.Tuning;


public sealed class ParticleSwarms : List<ParticleSwarm>
{
    public const double Influence = 0.375d;
    private readonly Core.Delegates.WriteMessageLine _writeMessageLine;
    private readonly double _originalEvaluationError;
    private int _iterations;


    public ParticleSwarms(string quietFilename, int particleSwarms, int particlesPerSwarm, int winScale, Core.Delegates.WriteMessageLine writeMessageLine)
    {
        _writeMessageLine = writeMessageLine;
        // Load quiet positions.
        writeMessageLine("Loading quiet positions.");
        var stopwatch = Stopwatch.StartNew();
        var quietPositions = new QuietPositions();
        using (var streamReader = File.OpenText(quietFilename))
        {
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                if (line == null) continue;
                var lineSegments = line.Split('|');
                if (lineSegments.Length < 2) continue;
                var fen = lineSegments[0];
                var resultText = lineSegments[1];
                var result = resultText switch
                {
                    "1-0" => GameResult.WhiteWon,
                    "1/2-1/2" => GameResult.Draw,
                    "0-1" => GameResult.BlackWon,
                    _ => GameResult.Unknown
                };
                var quietPosition = new QuietPosition(fen, result);
                quietPositions.Add(quietPosition);
                if ((quietPositions.Count % 10_000) == 0) writeMessageLine($"Loaded {quietPositions.Count:n0} quiet positions.");
            }
            writeMessageLine($"Loaded {quietPositions.Count:n0} quiet positions.");
        }
        var positionsPerSecond = (int)(quietPositions.Count / stopwatch.Elapsed.TotalSeconds);
        writeMessageLine($"Loaded {quietPositions.Count:n0} quiet positions in {stopwatch.Elapsed.TotalSeconds:0.000} seconds ({positionsPerSecond:n0} positions per second).");
        stopwatch.Restart();
        writeMessageLine("Creating data structures.");
        // Create parameters and particle swarms.
        var parameters = CreateParameters();
        for (var particleSwarmsIndex = 0; particleSwarmsIndex < particleSwarms; particleSwarmsIndex++)
        {
            var particleSwarm = new ParticleSwarm(quietPositions, parameters, particlesPerSwarm, winScale);
            Add(particleSwarm);
            // Set parameter values of all particles in swarm to known best.
            for (var particleIndex = 0; particleIndex < particleSwarm.Particles.Count; particleIndex++) SetDefaultParameters(particleSwarm.Particles[particleIndex].Parameters);
        }
        var board = new Board(writeMessageLine, UciStream.NodesInfoInterval);
        var stats = new Stats();
        var eval = new Eval(stats, board.IsRepeatPosition, () => false, writeMessageLine);
        var firstParticleInFirstSwarm = this[0].Particles[0];
        firstParticleInFirstSwarm.CalculateEvaluationError(board, eval, winScale);
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
        var evalConfig = new EvalConfig();
        return new Parameters
        {
            // Endgame Material
            new(nameof(evalConfig.EgKnightMaterial), evalConfig.MgKnightMaterial, evalConfig.MgKnightMaterial + 200),
            new(nameof(evalConfig.EgBishopMaterial), evalConfig.MgBishopMaterial, evalConfig.MgBishopMaterial + 300),
            new(nameof(evalConfig.EgRookMaterial), evalConfig.MgRookMaterial, evalConfig.MgRookMaterial + 400),
            new(nameof(evalConfig.EgQueenMaterial), evalConfig.MgQueenMaterial, evalConfig.MgQueenMaterial + 600), 
            // Pawn Location
            new(nameof(evalConfig.MgPawnAdvancement), 0, 25),
            new(nameof(evalConfig.EgPawnAdvancement), 0, 25),
            new(nameof(evalConfig.MgPawnCentrality), 0, 25),
            new(nameof(evalConfig.EgPawnCentrality), -25, 25),
            // Knight Location
            new(nameof(evalConfig.MgKnightAdvancement), -25, 25),
            new(nameof(evalConfig.EgKnightAdvancement), 0, 50),
            new(nameof(evalConfig.MgKnightCentrality), 0, 25),
            new(nameof(evalConfig.EgKnightCentrality), 0, 50),
            new(nameof(evalConfig.MgKnightCorner), -25, 0),
            new(nameof(evalConfig.EgKnightCorner), -50, 0),
            // Bishop Location
            new(nameof(evalConfig.MgBishopAdvancement), -25, 25),
            new(nameof(evalConfig.EgBishopAdvancement), 0, 50),
            new(nameof(evalConfig.MgBishopCentrality), 0, 25),
            new(nameof(evalConfig.EgBishopCentrality), 0, 25),
            new(nameof(evalConfig.MgBishopCorner), -25, 0),
            new(nameof(evalConfig.EgBishopCorner), -50, 0),
            // Rook Location
            new(nameof(evalConfig.MgRookAdvancement), -25, 25),
            new(nameof(evalConfig.EgRookAdvancement), 0, 50),
            new(nameof(evalConfig.MgRookCentrality), 0, 25),
            new(nameof(evalConfig.EgRookCentrality), -25, 25),
            new(nameof(evalConfig.MgRookCorner), -25, 0),
            new(nameof(evalConfig.EgRookCorner), -25, 25),
            // Queen Location
            new(nameof(evalConfig.MgQueenAdvancement), -25, 25),
            new(nameof(evalConfig.EgQueenAdvancement), 0, 50),
            new(nameof(evalConfig.MgQueenCentrality), 0, 25),
            new(nameof(evalConfig.EgQueenCentrality), -25, 25),
            new(nameof(evalConfig.MgQueenCorner), -25, 0),
            new(nameof(evalConfig.EgQueenCorner), -25, 25),
            // King Location
            new(nameof(evalConfig.MgKingAdvancement), -50, 0),
            new(nameof(evalConfig.EgKingAdvancement), 0, 50),
            new(nameof(evalConfig.MgKingCentrality), -50, 0),
            new(nameof(evalConfig.EgKingCentrality), 0, 50),
            new(nameof(evalConfig.MgKingCorner), 0, 50),
            new(nameof(evalConfig.EgKingCorner), -50, 0),
            // Pawn Structure
            new(nameof(evalConfig.MgIsolatedPawn), 0, 50),
            new(nameof(evalConfig.EgIsolatedPawn), 0, 50),
            new(nameof(evalConfig.MgDoubledPawn), 0, 50),
            new(nameof(evalConfig.EgDoubledPawn), 0, 50),
            // Passed Pawns
            new(nameof(evalConfig.PassedPawnPowerPer128), 192, 320),
            new(nameof(evalConfig.MgPassedPawnScalePer128), 0, 256),
            new(nameof(evalConfig.EgPassedPawnScalePer128), 256, 768),
            new(nameof(evalConfig.EgFreePassedPawnScalePer128), 512, 1280),
            new(nameof(evalConfig.EgKingEscortedPassedPawn), 0, 32),
            // Piece Mobility
            new(nameof(evalConfig.PieceMobilityPowerPer128), 32, 96),
            new(nameof(evalConfig.MgKnightMobilityScale), 0, 128),
            new(nameof(evalConfig.EgKnightMobilityScale), 0, 256),
            new(nameof(evalConfig.MgBishopMobilityScale), 0, 128),
            new(nameof(evalConfig.EgBishopMobilityScale), 0, 512),
            new(nameof(evalConfig.MgRookMobilityScale), 0, 256),
            new(nameof(evalConfig.EgRookMobilityScale), 0, 512),
            new(nameof(evalConfig.MgQueenMobilityScale), 0, 256),
            new(nameof(evalConfig.EgQueenMobilityScale), 0, 1024),
            // King Safety
            new(nameof(evalConfig.MgKingSafetyPowerPer128), 192, 320),
            new(nameof(evalConfig.MgKingSafetyScalePer128), 0, 128),
            new(nameof(evalConfig.MgKingSafetyMinorAttackOuterRingPer8), 0, 64),
            new(nameof(evalConfig.MgKingSafetyMinorAttackInnerRingPer8), 0, 64),
            new(nameof(evalConfig.MgKingSafetyRookAttackOuterRingPer8), 0, 64),
            new(nameof(evalConfig.MgKingSafetyRookAttackInnerRingPer8), 0, 64),
            new(nameof(evalConfig.MgKingSafetyQueenAttackOuterRingPer8), 0, 64),
            new(nameof(evalConfig.MgKingSafetyQueenAttackInnerRingPer8), 0, 64),
            new(nameof(evalConfig.MgKingSafetySemiOpenFilePer8), 0, 64),
            new(nameof(evalConfig.MgKingSafetyPawnShieldPer8), 0, 64),
            // Threats
            new(nameof(evalConfig.MgPawnThreatenMinor), 0, 100),
            new(nameof(evalConfig.EgPawnThreatenMinor), 0, 100),
            new(nameof(evalConfig.MgPawnThreatenMajor), 0, 100),
            new(nameof(evalConfig.EgPawnThreatenMajor), 0, 100),
            new(nameof(evalConfig.MgMinorThreatenMajor), 0, 100),
            new(nameof(evalConfig.EgMinorThreatenMajor), 0, 100),
            // Minor Pieces
            new(nameof(evalConfig.MgBishopPair), 0, 50),
            new(nameof(evalConfig.EgBishopPair), 50, 200),
            // Endgame Scale
            new(nameof(evalConfig.EgScaleBishopAdvantagePer128), 0, 64),
            new(nameof(evalConfig.EgScaleOppBishopsPerPassedPawn), 0, 64),
            new(nameof(evalConfig.EgScaleWinningPerPawn), 0, 64)
        };
    }


    private static void SetDefaultParameters(Parameters parameters)
    {
        var evalConfig = new EvalConfig();
        // Endgame Material
        parameters[nameof(evalConfig.EgKnightMaterial)].Value = evalConfig.EgKnightMaterial;
        parameters[nameof(evalConfig.EgBishopMaterial)].Value = evalConfig.EgBishopMaterial;
        parameters[nameof(evalConfig.EgRookMaterial)].Value = evalConfig.EgRookMaterial;
        parameters[nameof(evalConfig.EgQueenMaterial)].Value = evalConfig.EgQueenMaterial;
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
        // Pawn Structure
        parameters[nameof(evalConfig.MgIsolatedPawn)].Value = evalConfig.MgIsolatedPawn;
        parameters[nameof(evalConfig.EgIsolatedPawn)].Value = evalConfig.EgIsolatedPawn;
        parameters[nameof(evalConfig.MgDoubledPawn)].Value = evalConfig.MgDoubledPawn;
        parameters[nameof(evalConfig.EgDoubledPawn)].Value = evalConfig.EgDoubledPawn;
        // Passed Pawns
        parameters[nameof(evalConfig.PassedPawnPowerPer128)].Value = evalConfig.PassedPawnPowerPer128;
        parameters[nameof(evalConfig.MgPassedPawnScalePer128)].Value = evalConfig.MgPassedPawnScalePer128;
        parameters[nameof(evalConfig.EgPassedPawnScalePer128)].Value = evalConfig.EgPassedPawnScalePer128;
        parameters[nameof(evalConfig.EgFreePassedPawnScalePer128)].Value = evalConfig.EgFreePassedPawnScalePer128;
        parameters[nameof(evalConfig.EgKingEscortedPassedPawn)].Value = evalConfig.EgKingEscortedPassedPawn;
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
        // Endgame Scale
        parameters[nameof(evalConfig.EgScaleBishopAdvantagePer128)].Value = evalConfig.EgScaleBishopAdvantagePer128;
        parameters[nameof(evalConfig.EgScaleOppBishopsPerPassedPawn)].Value = evalConfig.EgScaleOppBishopsPerPassedPawn;
        parameters[nameof(evalConfig.EgScaleWinningPerPawn)].Value = evalConfig.EgScaleWinningPerPawn;
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
        var evals = new Eval[Count];
        for (var index = 0; index < Count; index++)
        {
            var board = new Board(_writeMessageLine, UciStream.NodesInfoInterval);
            boards[index] = board;
            var stats = new Stats();
            var eval = new Eval(stats, board.IsRepeatPosition, () => false, _writeMessageLine);
            evals[index] = eval;
        }
        var tasks = new Task[Count];
        for (var iteration = 1; iteration <= iterations; iteration++)
        {
            // Run iteration tasks on threadpool.
            _iterations = iteration;
            for (var index = 0; index < Count; index++)
            {
                var particleSwarm = this[index];
                var board = boards[index];
                var eval = evals[index];
                tasks[index] = Task.Run(() => particleSwarm.Iterate(board, eval));
            }
            // Wait for all particle swarms to complete an iteration.
            Task.WaitAll(tasks);
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
        // Display iteration, best evaluation error, and best parameters.
        _writeMessageLine(null);
        _writeMessageLine($"{"Iterations",-padding} = {_iterations,6:000}    ");
        _writeMessageLine($"{"Original Evaluation Error",-padding} = {_originalEvaluationError,10:0.000}");
        var bestParticle = GetBestParticle();
        _writeMessageLine($"{"Best Evaluation Error",-padding} = {bestParticle.BestEvaluationError,10:0.000}");
        _writeMessageLine(null);
        for (var swarmIndex = 0; swarmIndex < Count; swarmIndex++)
        {
            var particleSwarm = this[swarmIndex];
            var bestSwarmParticle = particleSwarm.GetBestParticle();
            _writeMessageLine($"Particle Swarm {swarmIndex:00} Best Evaluation Error = {bestSwarmParticle.BestEvaluationError,10:0.000}");
            for (var particleIndex = 0; particleIndex < particleSwarm.Particles.Count; particleIndex++)
            {
                var particle = particleSwarm.Particles[particleIndex];
                _writeMessageLine($"  Particle {particleIndex:00} Evaluation Error          = {particle.EvaluationError,10:0.000}");
            }
        }
        _writeMessageLine(null);
        for (var parameterIndex = 0; parameterIndex < bestParticle.BestParameters.Count; parameterIndex++)
        {
            var parameter = bestParticle.BestParameters[parameterIndex];
            _writeMessageLine($"{parameter.Name,-padding} = {parameter.Value,6}");
        }
    }
}