// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;


namespace ErikTheCoder.MadChess.Engine.Tuning
{
    public sealed class ParticleSwarms : List<ParticleSwarm>
    {
        public const double Influence = 0.375d;
        private readonly Delegates.DisplayStats _displayStats;
        private readonly Delegates.WriteMessageLine _writeMessageLine;
        private readonly double _originalEvaluationError;
        private int _iterations;


        public ParticleSwarms(string pgnFilename, int particleSwarms, int particlesPerSwarm, int winScale, Delegates.DisplayStats displayStats, Delegates.WriteMessageLine writeMessageLine)
        {
            _displayStats = displayStats;
            _writeMessageLine = writeMessageLine;
            // Load games.
            writeMessageLine("Loading games.");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var board = new Board(writeMessageLine);
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
                // Set parameter values of all particles in swarm to known best.
                for (var particleIndex = 0; particleIndex < particleSwarm.Particles.Count; particleIndex++) SetDefaultParameters(particleSwarm.Particles[particleIndex].Parameters);
            }
            var stats = new Stats();
            var cache = new Cache(1, stats, board.ValidateMove);
            var killerMoves = new KillerMoves(Search.MaxHorizon);
            var moveHistory = new MoveHistory();
            var evaluation = new Evaluation(stats, board.IsRepeatPosition, () => false, writeMessageLine);
            var search = new Search(stats, cache, killerMoves, moveHistory, evaluation, () => false, displayStats, writeMessageLine);
            var firstParticleInFirstSwarm = this[0].Particles[0];
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
            var evaluationConfig = new EvaluationConfig();
            return new Parameters
            {
                // Endgame Material
                new(nameof(EvaluationConfig.EgKnightMaterial), evaluationConfig.MgKnightMaterial, evaluationConfig.MgKnightMaterial + 200),
                new(nameof(EvaluationConfig.EgBishopMaterial), evaluationConfig.MgBishopMaterial, evaluationConfig.MgBishopMaterial + 300),
                new(nameof(EvaluationConfig.EgRookMaterial), evaluationConfig.MgRookMaterial, evaluationConfig.MgRookMaterial + 400),
                new(nameof(EvaluationConfig.EgQueenMaterial), evaluationConfig.MgQueenMaterial, evaluationConfig.MgQueenMaterial + 600), 
                // Pawn Location
                new(nameof(EvaluationConfig.MgPawnAdvancement), 0, 25),
                new(nameof(EvaluationConfig.EgPawnAdvancement), 0, 25),
                new(nameof(EvaluationConfig.MgPawnCentrality), 0, 25),
                new(nameof(EvaluationConfig.EgPawnCentrality), -25, 25),
                // Knight Location
                new(nameof(EvaluationConfig.MgKnightAdvancement), -25, 25),
                new(nameof(EvaluationConfig.EgKnightAdvancement), 0, 50),
                new(nameof(EvaluationConfig.MgKnightCentrality), 0, 25),
                new(nameof(EvaluationConfig.EgKnightCentrality), 0, 50),
                new(nameof(EvaluationConfig.MgKnightCorner), -25, 0),
                new(nameof(EvaluationConfig.EgKnightCorner), -50, 0),
                // Bishop Location
                new(nameof(EvaluationConfig.MgBishopAdvancement), -25, 25),
                new(nameof(EvaluationConfig.EgBishopAdvancement), 0, 50),
                new(nameof(EvaluationConfig.MgBishopCentrality), 0, 25),
                new(nameof(EvaluationConfig.EgBishopCentrality), 0, 25),
                new(nameof(EvaluationConfig.MgBishopCorner), -25, 0),
                new(nameof(EvaluationConfig.EgBishopCorner), -50, 0),
                // Rook Location
                new(nameof(EvaluationConfig.MgRookAdvancement), -25, 25),
                new(nameof(EvaluationConfig.EgRookAdvancement), 0, 50),
                new(nameof(EvaluationConfig.MgRookCentrality), 0, 25),
                new(nameof(EvaluationConfig.EgRookCentrality), -25, 25),
                new(nameof(EvaluationConfig.MgRookCorner), -25, 0),
                new(nameof(EvaluationConfig.EgRookCorner), -25, 25),
                // Queen Location
                new(nameof(EvaluationConfig.MgQueenAdvancement), -25, 25),
                new(nameof(EvaluationConfig.EgQueenAdvancement), 0, 50),
                new(nameof(EvaluationConfig.MgQueenCentrality), 0, 25),
                new(nameof(EvaluationConfig.EgQueenCentrality), -25, 25),
                new(nameof(EvaluationConfig.MgQueenCorner), -25, 0),
                new(nameof(EvaluationConfig.EgQueenCorner), -25, 25),
                // King Location
                new(nameof(EvaluationConfig.MgKingAdvancement), -50, 0),
                new(nameof(EvaluationConfig.EgKingAdvancement), 0, 50),
                new(nameof(EvaluationConfig.MgKingCentrality), -50, 0),
                new(nameof(EvaluationConfig.EgKingCentrality), 0, 50),
                new(nameof(EvaluationConfig.MgKingCorner), 0, 50),
                new(nameof(EvaluationConfig.EgKingCorner), -50, 0),
                // Passed Pawns
                new(nameof(EvaluationConfig.PassedPawnPowerPer128), 192, 320),
                new(nameof(EvaluationConfig.MgPassedPawnScalePer128), 0, 256),
                new(nameof(EvaluationConfig.EgPassedPawnScalePer128), 256, 768),
                new(nameof(EvaluationConfig.EgFreePassedPawnScalePer128), 512, 1280),
                new(nameof(EvaluationConfig.EgKingEscortedPassedPawn), 0, 32),
                // Piece Mobility
                new(nameof(EvaluationConfig.PieceMobilityPowerPer128), 32, 96),
                new(nameof(EvaluationConfig.MgKnightMobilityScale), 0, 128),
                new(nameof(EvaluationConfig.EgKnightMobilityScale), 0, 256),
                new(nameof(EvaluationConfig.MgBishopMobilityScale), 0, 128),
                new(nameof(EvaluationConfig.EgBishopMobilityScale), 0, 512),
                new(nameof(EvaluationConfig.MgRookMobilityScale), 0, 256),
                new(nameof(EvaluationConfig.EgRookMobilityScale), 0, 512),
                new(nameof(EvaluationConfig.MgQueenMobilityScale), 0, 256),
                new(nameof(EvaluationConfig.EgQueenMobilityScale), 0, 1024),
                // King Safety
                new(nameof(EvaluationConfig.MgKingSafetyPowerPer128), 192, 320),
                new(nameof(EvaluationConfig.MgKingSafetyScalePer128), 0, 128),
                new(nameof(EvaluationConfig.MgKingSafetyMinorAttackOuterRingPer8), 0, 64),
                new(nameof(EvaluationConfig.MgKingSafetyMinorAttackInnerRingPer8), 0, 64),
                new(nameof(EvaluationConfig.MgKingSafetyRookAttackOuterRingPer8), 0, 64),
                new(nameof(EvaluationConfig.MgKingSafetyRookAttackInnerRingPer8), 0, 64),
                new(nameof(EvaluationConfig.MgKingSafetyQueenAttackOuterRingPer8), 0, 64),
                new(nameof(EvaluationConfig.MgKingSafetyQueenAttackInnerRingPer8), 0, 64),
                new(nameof(EvaluationConfig.MgKingSafetySemiOpenFilePer8), 0, 64),
                new(nameof(EvaluationConfig.MgKingSafetyPawnShieldPer8), 0, 64),
                // Minor Pieces
                new(nameof(EvaluationConfig.MgBishopPair), 0, 50),
                new(nameof(EvaluationConfig.EgBishopPair), 50, 200),
                // Endgame Scaling
                new(nameof(EvaluationConfig.EgBishopAdvantagePer128), 0, 64),
                new(nameof(EvaluationConfig.EgOppBishopsPerPassedPawn), 0, 64),
                new(nameof(EvaluationConfig.EgOppBishopsPer128), 0, 64),
                new(nameof(EvaluationConfig.EgWinningPerPawn), 0, 32)
            };
        }


        private static void SetDefaultParameters(Parameters parameters)
        {
            var evaluationConfig = new EvaluationConfig();
            // Endgame Material
            parameters[nameof(EvaluationConfig.EgKnightMaterial)].Value = evaluationConfig.EgKnightMaterial;
            parameters[nameof(EvaluationConfig.EgBishopMaterial)].Value = evaluationConfig.EgBishopMaterial;
            parameters[nameof(EvaluationConfig.EgRookMaterial)].Value = evaluationConfig.EgRookMaterial;
            parameters[nameof(EvaluationConfig.EgQueenMaterial)].Value = evaluationConfig.EgQueenMaterial;
            // Pawn Location
            parameters[nameof(EvaluationConfig.MgPawnAdvancement)].Value = evaluationConfig.MgPawnAdvancement;
            parameters[nameof(EvaluationConfig.EgPawnAdvancement)].Value = evaluationConfig.EgPawnAdvancement;
            parameters[nameof(EvaluationConfig.MgPawnCentrality)].Value = evaluationConfig.MgPawnCentrality;
            parameters[nameof(EvaluationConfig.EgPawnCentrality)].Value = evaluationConfig.EgPawnCentrality;
            // Knight Location
            parameters[nameof(EvaluationConfig.MgKnightAdvancement)].Value = evaluationConfig.MgKnightAdvancement;
            parameters[nameof(EvaluationConfig.EgKnightAdvancement)].Value = evaluationConfig.EgKnightAdvancement;
            parameters[nameof(EvaluationConfig.MgKnightCentrality)].Value = evaluationConfig.MgKnightCentrality;
            parameters[nameof(EvaluationConfig.EgKnightCentrality)].Value = evaluationConfig.EgKnightCentrality;
            parameters[nameof(EvaluationConfig.MgKnightCorner)].Value = evaluationConfig.MgKnightCorner;
            parameters[nameof(EvaluationConfig.EgKnightCorner)].Value = evaluationConfig.EgKnightCorner;
            // Bishop Location
            parameters[nameof(EvaluationConfig.MgBishopAdvancement)].Value = evaluationConfig.MgBishopAdvancement;
            parameters[nameof(EvaluationConfig.EgBishopAdvancement)].Value = evaluationConfig.EgBishopAdvancement;
            parameters[nameof(EvaluationConfig.MgBishopCentrality)].Value = evaluationConfig.MgBishopCentrality;
            parameters[nameof(EvaluationConfig.EgBishopCentrality)].Value = evaluationConfig.EgBishopCentrality;
            parameters[nameof(EvaluationConfig.MgBishopCorner)].Value = evaluationConfig.MgBishopCorner;
            parameters[nameof(EvaluationConfig.EgBishopCorner)].Value = evaluationConfig.EgBishopCorner;
            // Rook Location
            parameters[nameof(EvaluationConfig.MgRookAdvancement)].Value = evaluationConfig.MgRookAdvancement;
            parameters[nameof(EvaluationConfig.EgRookAdvancement)].Value = evaluationConfig.EgRookAdvancement;
            parameters[nameof(EvaluationConfig.MgRookCentrality)].Value = evaluationConfig.MgRookCentrality;
            parameters[nameof(EvaluationConfig.EgRookCentrality)].Value = evaluationConfig.EgRookCentrality;
            parameters[nameof(EvaluationConfig.MgRookCorner)].Value = evaluationConfig.MgRookCorner;
            parameters[nameof(EvaluationConfig.EgRookCorner)].Value = evaluationConfig.EgRookCorner;
            // Queen Location
            parameters[nameof(EvaluationConfig.MgQueenAdvancement)].Value = evaluationConfig.MgQueenAdvancement;
            parameters[nameof(EvaluationConfig.EgQueenAdvancement)].Value = evaluationConfig.EgQueenAdvancement;
            parameters[nameof(EvaluationConfig.MgQueenCentrality)].Value = evaluationConfig.MgQueenCentrality;
            parameters[nameof(EvaluationConfig.EgQueenCentrality)].Value = evaluationConfig.EgQueenCentrality;
            parameters[nameof(EvaluationConfig.MgQueenCorner)].Value = evaluationConfig.MgQueenCorner;
            parameters[nameof(EvaluationConfig.EgQueenCorner)].Value = evaluationConfig.EgQueenCorner;
            // King Location
            parameters[nameof(EvaluationConfig.MgKingAdvancement)].Value = evaluationConfig.MgKingAdvancement;
            parameters[nameof(EvaluationConfig.EgKingAdvancement)].Value = evaluationConfig.EgKingAdvancement;
            parameters[nameof(EvaluationConfig.MgKingCentrality)].Value = evaluationConfig.MgKingCentrality;
            parameters[nameof(EvaluationConfig.EgKingCentrality)].Value = evaluationConfig.EgKingCentrality;
            parameters[nameof(EvaluationConfig.MgKingCorner)].Value = evaluationConfig.MgKingCorner;
            parameters[nameof(EvaluationConfig.EgKingCorner)].Value = evaluationConfig.EgKingCorner;
            // Passed Pawns
            parameters[nameof(EvaluationConfig.PassedPawnPowerPer128)].Value = evaluationConfig.PassedPawnPowerPer128;
            parameters[nameof(EvaluationConfig.MgPassedPawnScalePer128)].Value = evaluationConfig.MgPassedPawnScalePer128;
            parameters[nameof(EvaluationConfig.EgPassedPawnScalePer128)].Value = evaluationConfig.EgPassedPawnScalePer128;
            parameters[nameof(EvaluationConfig.EgFreePassedPawnScalePer128)].Value = evaluationConfig.EgFreePassedPawnScalePer128;
            parameters[nameof(EvaluationConfig.EgKingEscortedPassedPawn)].Value = evaluationConfig.EgKingEscortedPassedPawn;
            // Piece Mobility
            parameters[nameof(EvaluationConfig.PieceMobilityPowerPer128)].Value = evaluationConfig.PieceMobilityPowerPer128;
            parameters[nameof(EvaluationConfig.MgKnightMobilityScale)].Value = evaluationConfig.MgKnightMobilityScale;
            parameters[nameof(EvaluationConfig.EgKnightMobilityScale)].Value = evaluationConfig.EgKnightMobilityScale;
            parameters[nameof(EvaluationConfig.MgBishopMobilityScale)].Value = evaluationConfig.MgBishopMobilityScale;
            parameters[nameof(EvaluationConfig.EgBishopMobilityScale)].Value = evaluationConfig.EgBishopMobilityScale;
            parameters[nameof(EvaluationConfig.MgRookMobilityScale)].Value = evaluationConfig.MgRookMobilityScale;
            parameters[nameof(EvaluationConfig.EgRookMobilityScale)].Value = evaluationConfig.EgRookMobilityScale;
            parameters[nameof(EvaluationConfig.MgQueenMobilityScale)].Value = evaluationConfig.MgQueenMobilityScale;
            parameters[nameof(EvaluationConfig.EgQueenMobilityScale)].Value = evaluationConfig.EgQueenMobilityScale;
            // King Safety
            parameters[nameof(EvaluationConfig.MgKingSafetyPowerPer128)].Value = evaluationConfig.MgKingSafetyPowerPer128;
            parameters[nameof(EvaluationConfig.MgKingSafetyScalePer128)].Value = evaluationConfig.MgKingSafetyScalePer128;
            parameters[nameof(EvaluationConfig.MgKingSafetyMinorAttackOuterRingPer8)].Value = evaluationConfig.MgKingSafetyMinorAttackOuterRingPer8;
            parameters[nameof(EvaluationConfig.MgKingSafetyMinorAttackInnerRingPer8)].Value = evaluationConfig.MgKingSafetyMinorAttackInnerRingPer8;
            parameters[nameof(EvaluationConfig.MgKingSafetyRookAttackOuterRingPer8)].Value = evaluationConfig.MgKingSafetyRookAttackOuterRingPer8;
            parameters[nameof(EvaluationConfig.MgKingSafetyRookAttackInnerRingPer8)].Value = evaluationConfig.MgKingSafetyRookAttackInnerRingPer8;
            parameters[nameof(EvaluationConfig.MgKingSafetyQueenAttackOuterRingPer8)].Value = evaluationConfig.MgKingSafetyQueenAttackOuterRingPer8;
            parameters[nameof(EvaluationConfig.MgKingSafetyQueenAttackInnerRingPer8)].Value = evaluationConfig.MgKingSafetyQueenAttackInnerRingPer8;
            parameters[nameof(EvaluationConfig.MgKingSafetySemiOpenFilePer8)].Value = evaluationConfig.MgKingSafetySemiOpenFilePer8;
            parameters[nameof(EvaluationConfig.MgKingSafetyPawnShieldPer8)].Value = evaluationConfig.MgKingSafetyPawnShieldPer8;
            // Minor Pieces
            parameters[nameof(EvaluationConfig.MgBishopPair)].Value = evaluationConfig.MgBishopPair;
            parameters[nameof(EvaluationConfig.EgBishopPair)].Value = evaluationConfig.EgBishopPair;
            // Endgame Scaling
            parameters[nameof(EvaluationConfig.EgBishopAdvantagePer128)].Value = evaluationConfig.EgBishopAdvantagePer128;
            parameters[nameof(EvaluationConfig.EgOppBishopsPerPassedPawn)].Value = evaluationConfig.EgOppBishopsPerPassedPawn;
            parameters[nameof(EvaluationConfig.EgOppBishopsPer128)].Value = evaluationConfig.EgOppBishopsPer128;
            parameters[nameof(EvaluationConfig.EgWinningPerPawn)].Value = evaluationConfig.EgWinningPerPawn;
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
            var evaluations = new Evaluation[Count];
            for (var index = 0; index < Count; index++)
            {
                var board = new Board(_writeMessageLine);
                boards[index] = board;
                var stats = new Stats();
                var cache = new Cache(1, stats, board.ValidateMove);
                var killerMoves = new KillerMoves(Search.MaxHorizon);
                var moveHistory = new MoveHistory();
                var evaluation = new Evaluation(stats, board.IsRepeatPosition, () => false, _writeMessageLine);
                evaluations[index] = evaluation;
                searches[index] = new Search(stats, cache, killerMoves, moveHistory, evaluation, () => false, _displayStats, _writeMessageLine);
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
                    var evaluation = evaluations[index];
                    tasks[index] = Task.Run(() => particleSwarm.Iterate(board, search, evaluation));
                }
                // Wait for all particle swarms to complete an iteration.
                Task.WaitAll(tasks);
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
}
