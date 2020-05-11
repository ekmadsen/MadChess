// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
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
        private const int _maxIterationsWithoutProgress = 10;
        private readonly Delegates.WriteMessageLine _writeMessageLine;
        private readonly double _originalEvaluationError;
        private int _iterations;


        public ParticleSwarms(string PgnFilename, int ParticleSwarms, int ParticlesPerSwarm, int WinPercentScale, Delegates.WriteMessageLine WriteMessageLine)
        {
            _writeMessageLine = WriteMessageLine;
            // Load games.
            WriteMessageLine("Loading games.");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Board board = new Board(WriteMessageLine);
            PgnGames pgnGames = new PgnGames();
            pgnGames.Load(board, PgnFilename);
            stopwatch.Stop();
            // Count positions.
            long positions = 0;
            for (int gameIndex = 0; gameIndex < pgnGames.Count; gameIndex++)
            {
                PgnGame pgnGame = pgnGames[gameIndex];
                positions += pgnGame.Moves.Count;
            }
            int positionsPerSecond = (int)(positions / stopwatch.Elapsed.TotalSeconds);
            WriteMessageLine($"Loaded {pgnGames.Count:n0} games with {positions:n0} positions in {stopwatch.Elapsed.TotalSeconds:0.000} seconds ({positionsPerSecond:n0} positions per second).");
            stopwatch.Restart();
            WriteMessageLine("Creating data structures.");
            // Create parameters and particle swarms.
            Parameters parameters = CreateParameters();
            for (int index = 0; index < ParticleSwarms; index++)
            {
                Add(new ParticleSwarm(pgnGames, parameters, ParticlesPerSwarm, WinPercentScale));
            }
            // Set parameter values of first particle in first swarm to known best.
            Particle firstParticleInFirstSwarm = this[0].Particles[0];
            SetDefaultParameters(firstParticleInFirstSwarm.Parameters);
            Cache cache = new Cache(1, board.ValidateMove);
            KillerMoves killerMoves = new KillerMoves(Search.MaxHorizon);
            MoveHistory moveHistory = new MoveHistory();
            EvaluationDelegates evaluationDelegates = new EvaluationDelegates
            {
                GetPositionCount = board.GetPositionCount,
                GetKnightDestinations = Board.GetKnightDestinations,
                GetBishopDestinations = Board.GetBishopDestinations,
                GetRookDestinations = Board.GetRookDestinations,
                GetQueenDestinations = Board.GetQueenDestinations,
                Debug = () => false,
                WriteMessageLine = WriteMessageLine
            };
            Evaluation evaluation = new Evaluation(evaluationDelegates);
            Search search = new Search(cache, killerMoves, moveHistory, evaluation, () => false, WriteMessageLine);
            firstParticleInFirstSwarm.CalculateEvaluationError(board, search, WinPercentScale);
            _originalEvaluationError = firstParticleInFirstSwarm.EvaluationError;
            stopwatch.Stop();
            WriteMessageLine($"Created data structures in {stopwatch.Elapsed.TotalSeconds:0.000} seconds.");
        }


        private Particle GetBestParticle()
        {
            Particle bestParticle = this[0].GetBestParticle();
            for (int index = 1; index < Count; index++)
            {
                Particle particle = this[index].GetBestParticle();
                if (particle.BestEvaluationError < bestParticle.BestEvaluationError) bestParticle = particle;
            }
            return bestParticle;
        }


        private void RandomizeParticles(Particle BestParticle)
        {
            for (int index = 0; index < Count; index++) this[index].RandomizeParticles(BestParticle);
        }


        public static Parameters CreateParameters()
        {
            return new Parameters
            {
                // Piece Location
                // Pawns
                //new Parameter(nameof(EvaluationConfig.MgPawnAdvancement), 0, 25),
                //new Parameter(nameof(EvaluationConfig.EgPawnAdvancement), 0, 25),
                //new Parameter(nameof(EvaluationConfig.MgPawnCentrality), 0, 25),
                //new Parameter(nameof(EvaluationConfig.EgPawnCentrality), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgPawnConstant), 0, 50),
                //// Knights
                //new Parameter(nameof(EvaluationConfig.MgKnightAdvancement), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgKnightAdvancement), -25, 25),
                //new Parameter(nameof(EvaluationConfig.MgKnightCentrality), 0, 25),
                //new Parameter(nameof(EvaluationConfig.EgKnightCentrality), -25, 25),
                //new Parameter(nameof(EvaluationConfig.MgKnightCorner), -25, 0),
                //new Parameter(nameof(EvaluationConfig.EgKnightCorner), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgKnightConstant), -100, 100),
                //// Bishops
                //new Parameter(nameof(EvaluationConfig.MgBishopAdvancement), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgBishopAdvancement), -25, 25),
                //new Parameter(nameof(EvaluationConfig.MgBishopCentrality), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgBishopCentrality), -25, 25),
                //new Parameter(nameof(EvaluationConfig.MgBishopCorner), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgBishopCorner), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgBishopConstant), -100, 100),
                //// Rooks
                //new Parameter(nameof(EvaluationConfig.MgRookAdvancement), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgRookAdvancement), -25, 25),
                //new Parameter(nameof(EvaluationConfig.MgRookCentrality), 0, 25),
                //new Parameter(nameof(EvaluationConfig.EgRookCentrality), -25, 25),
                //new Parameter(nameof(EvaluationConfig.MgRookCorner), -25, 0),
                //new Parameter(nameof(EvaluationConfig.EgRookCorner), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgRookConstant), -100, 100),
                //// Queens
                //new Parameter(nameof(EvaluationConfig.MgQueenAdvancement), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgQueenAdvancement), -25, 25),
                //new Parameter(nameof(EvaluationConfig.MgQueenCentrality), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgQueenCentrality), -25, 25),
                //new Parameter(nameof(EvaluationConfig.MgQueenCorner), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgQueenCorner), -25, 25),
                //new Parameter(nameof(EvaluationConfig.EgQueenConstant), -100, 100),
                //// King
                //new Parameter(nameof(EvaluationConfig.MgKingAdvancement), -50, 0),
                //new Parameter(nameof(EvaluationConfig.EgKingAdvancement), 0, 50),
                //new Parameter(nameof(EvaluationConfig.MgKingCentrality), -50, 0),
                //new Parameter(nameof(EvaluationConfig.EgKingCentrality), 0, 50),
                //new Parameter(nameof(EvaluationConfig.MgKingCorner), 0, 50),
                //new Parameter(nameof(EvaluationConfig.EgKingCorner), -50, 0),
                //// Passed Pawns
                //new Parameter(nameof(EvaluationConfig.MgPassedPawnScalePercent), 0, 200),
                //new Parameter(nameof(EvaluationConfig.EgPassedPawnScalePercent), 200, 1000),
                //new Parameter(nameof(EvaluationConfig.EgFreePassedPawnScalePercent), 400, 1000),
                //new Parameter(nameof(EvaluationConfig.EgKingEscortedPassedPawn), 0, 25),
                // Piece Mobility
                new Parameter(nameof(EvaluationConfig.MgKnightMobilityScale), 0, 100),
                new Parameter(nameof(EvaluationConfig.EgKnightMobilityScale), 0, 100),
                new Parameter(nameof(EvaluationConfig.MgBishopMobilityScale), 0, 100),
                new Parameter(nameof(EvaluationConfig.EgBishopMobilityScale), 0, 300),
                new Parameter(nameof(EvaluationConfig.MgRookMobilityScale), 0, 100),
                new Parameter(nameof(EvaluationConfig.EgRookMobilityScale), 0, 300),
                new Parameter(nameof(EvaluationConfig.MgQueenMobilityScale), 0, 200),
                new Parameter(nameof(EvaluationConfig.EgQueenMobilityScale), 0, 500)
            };
        }


        private void SetDefaultParameters(Parameters Parameters)
        {
            EvaluationConfig evaluationConfig = new EvaluationConfig();
            // Pawns
            //Parameters[nameof(EvaluationConfig.MgPawnAdvancement)].Value = evaluationConfig.MgPawnAdvancement;
            //Parameters[nameof(EvaluationConfig.EgPawnAdvancement)].Value = evaluationConfig.EgPawnAdvancement;
            //Parameters[nameof(EvaluationConfig.MgPawnCentrality)].Value = evaluationConfig.MgPawnCentrality;
            //Parameters[nameof(EvaluationConfig.EgPawnCentrality)].Value = evaluationConfig.EgPawnCentrality;
            //Parameters[nameof(EvaluationConfig.EgPawnConstant)].Value = evaluationConfig.EgPawnConstant;
            //// Knights
            //Parameters[nameof(EvaluationConfig.MgKnightAdvancement)].Value = evaluationConfig.MgKnightAdvancement;
            //Parameters[nameof(EvaluationConfig.EgKnightAdvancement)].Value = evaluationConfig.EgKnightAdvancement;
            //Parameters[nameof(EvaluationConfig.MgKnightCentrality)].Value = evaluationConfig.MgKnightCentrality;
            //Parameters[nameof(EvaluationConfig.EgKnightCentrality)].Value = evaluationConfig.EgKnightCentrality;
            //Parameters[nameof(EvaluationConfig.MgKnightCorner)].Value = evaluationConfig.MgKnightCorner;
            //Parameters[nameof(EvaluationConfig.EgKnightCorner)].Value = evaluationConfig.EgKnightCorner;
            //Parameters[nameof(EvaluationConfig.EgKnightConstant)].Value = evaluationConfig.EgKnightConstant;
            //// Bishops
            //Parameters[nameof(EvaluationConfig.MgBishopAdvancement)].Value = evaluationConfig.MgBishopAdvancement;
            //Parameters[nameof(EvaluationConfig.EgBishopAdvancement)].Value = evaluationConfig.EgBishopAdvancement;
            //Parameters[nameof(EvaluationConfig.MgBishopCentrality)].Value = evaluationConfig.MgBishopCentrality;
            //Parameters[nameof(EvaluationConfig.EgBishopCentrality)].Value = evaluationConfig.EgBishopCentrality;
            //Parameters[nameof(EvaluationConfig.MgBishopCorner)].Value = evaluationConfig.MgBishopCorner;
            //Parameters[nameof(EvaluationConfig.EgBishopCorner)].Value = evaluationConfig.EgBishopCorner;
            //Parameters[nameof(EvaluationConfig.EgBishopConstant)].Value = evaluationConfig.EgBishopConstant;
            //// Rooks
            //Parameters[nameof(EvaluationConfig.MgRookAdvancement)].Value = evaluationConfig.MgRookAdvancement;
            //Parameters[nameof(EvaluationConfig.EgRookAdvancement)].Value = evaluationConfig.EgRookAdvancement;
            //Parameters[nameof(EvaluationConfig.MgRookCentrality)].Value = evaluationConfig.MgRookCentrality;
            //Parameters[nameof(EvaluationConfig.EgRookCentrality)].Value = evaluationConfig.EgRookCentrality;
            //Parameters[nameof(EvaluationConfig.MgRookCorner)].Value = evaluationConfig.MgRookCorner;
            //Parameters[nameof(EvaluationConfig.EgRookCorner)].Value = evaluationConfig.EgRookCorner;
            //Parameters[nameof(EvaluationConfig.EgRookConstant)].Value = evaluationConfig.EgRookConstant;
            //// Queens
            //Parameters[nameof(EvaluationConfig.MgQueenAdvancement)].Value = evaluationConfig.MgQueenAdvancement;
            //Parameters[nameof(EvaluationConfig.EgQueenAdvancement)].Value = evaluationConfig.EgQueenAdvancement;
            //Parameters[nameof(EvaluationConfig.MgQueenCentrality)].Value = evaluationConfig.MgQueenCentrality;
            //Parameters[nameof(EvaluationConfig.EgQueenCentrality)].Value = evaluationConfig.EgQueenCentrality;
            //Parameters[nameof(EvaluationConfig.MgQueenCorner)].Value = evaluationConfig.MgQueenCorner;
            //Parameters[nameof(EvaluationConfig.EgQueenCorner)].Value = evaluationConfig.EgQueenCorner;
            //Parameters[nameof(EvaluationConfig.EgQueenConstant)].Value = evaluationConfig.EgQueenConstant;
            //// King
            //Parameters[nameof(EvaluationConfig.MgKingAdvancement)].Value = evaluationConfig.MgKingAdvancement;
            //Parameters[nameof(EvaluationConfig.EgKingAdvancement)].Value = evaluationConfig.EgKingAdvancement;
            //Parameters[nameof(EvaluationConfig.MgKingCentrality)].Value = evaluationConfig.MgKingCentrality;
            //Parameters[nameof(EvaluationConfig.EgKingCentrality)].Value = evaluationConfig.EgKingCentrality;
            //Parameters[nameof(EvaluationConfig.MgKingCorner)].Value = evaluationConfig.MgKingCorner;
            //Parameters[nameof(EvaluationConfig.EgKingCorner)].Value = evaluationConfig.EgKingCorner;
            //// Passed Pawns
            //Parameters[nameof(EvaluationConfig.MgPassedPawnScalePercent)].Value = evaluationConfig.MgPassedPawnScalePercent;
            //Parameters[nameof(EvaluationConfig.EgPassedPawnScalePercent)].Value = evaluationConfig.EgPassedPawnScalePercent;
            //Parameters[nameof(EvaluationConfig.EgFreePassedPawnScalePercent)].Value = evaluationConfig.EgFreePassedPawnScalePercent;
            //Parameters[nameof(EvaluationConfig.EgKingEscortedPassedPawn)].Value = evaluationConfig.EgKingEscortedPassedPawn;
            // Piece Mobility
            Parameters[nameof(EvaluationConfig.MgKnightMobilityScale)].Value = evaluationConfig.MgKnightMobilityScale;
            Parameters[nameof(EvaluationConfig.EgKnightMobilityScale)].Value = evaluationConfig.EgKnightMobilityScale;
            Parameters[nameof(EvaluationConfig.MgBishopMobilityScale)].Value = evaluationConfig.MgBishopMobilityScale;
            Parameters[nameof(EvaluationConfig.EgBishopMobilityScale)].Value = evaluationConfig.EgBishopMobilityScale;
            Parameters[nameof(EvaluationConfig.MgRookMobilityScale)].Value = evaluationConfig.MgRookMobilityScale;
            Parameters[nameof(EvaluationConfig.EgRookMobilityScale)].Value = evaluationConfig.EgRookMobilityScale;
            Parameters[nameof(EvaluationConfig.MgQueenMobilityScale)].Value = evaluationConfig.MgQueenMobilityScale;
            Parameters[nameof(EvaluationConfig.EgQueenMobilityScale)].Value = evaluationConfig.EgQueenMobilityScale;
        }


        public void Optimize(int Iterations)
        {
            // Determine size of parameter space.
            double parameterSpace = 1d;
            Particle firstParticleInFirstSwarm = this[0].Particles[0];
            for (int index = 0; index < firstParticleInFirstSwarm.Parameters.Count; index++)
            {
                Parameter parameter = firstParticleInFirstSwarm.Parameters[index];
                parameterSpace *= parameter.MaxValue - parameter.MinValue + 1;
            }
            _writeMessageLine($"Optimizing {firstParticleInFirstSwarm.Parameters.Count} parameters in a space of {parameterSpace:e2} discrete parameter combinations.");
            // Create game objects for each particle swarm.
            Board[] boards = new Board[Count];
            Search[] searches = new Search[Count];
            Evaluation[] evaluations = new Evaluation[Count];
            for (int index = 0; index < Count; index++)
            {
                Board board = new Board(_writeMessageLine);
                boards[index] = board;
                Cache cache = new Cache(1, board.ValidateMove);
                KillerMoves killerMoves = new KillerMoves(Search.MaxHorizon);
                MoveHistory moveHistory = new MoveHistory();
                EvaluationDelegates evaluationDelegates = new EvaluationDelegates
                {
                    GetPositionCount = board.GetPositionCount,
                    GetKnightDestinations = Board.GetKnightDestinations,
                    GetBishopDestinations = Board.GetBishopDestinations,
                    GetRookDestinations = Board.GetRookDestinations,
                    GetQueenDestinations = Board.GetQueenDestinations,
                    Debug = () => false,
                    WriteMessageLine = _writeMessageLine
                };
                Evaluation evaluation = new Evaluation(evaluationDelegates);
                evaluations[index] = evaluation;
                searches[index] = new Search(cache, killerMoves, moveHistory, evaluation, () => false, _writeMessageLine);
            }
            Task[] tasks = new Task[Count];
            int iterationsWithoutProgress = 0;
            double bestEvaluationError = double.MaxValue;
            for (int iteration = 1; iteration <= Iterations; iteration++)
            {
                // Run iteration tasks on threadpool.
                _iterations = iteration;
                for (int index = 0; index < Count; index++)
                {
                    ParticleSwarm particleSwarm = this[index];
                    Board board = boards[index];
                    Search search = searches[index];
                    Evaluation evaluation = evaluations[index];
                    tasks[index] = Task.Run(() => particleSwarm.Iterate(board, search, evaluation));
                }
                // Wait for all particle swarms to complete an iteration.
                Task.WaitAll(tasks);
                Particle bestParticle = GetBestParticle();
                if (bestParticle.EvaluationError < bestEvaluationError)
                {
                    bestEvaluationError = bestParticle.BestEvaluationError;
                    iterationsWithoutProgress = 0;
                }
                else iterationsWithoutProgress++;
                if (iterationsWithoutProgress == _maxIterationsWithoutProgress)
                {
                    RandomizeParticles(bestParticle);
                    iterationsWithoutProgress = 0;
                }
                else UpdateVelocity();
                UpdateStatus();
            }
        }


        private void UpdateVelocity()
        {
            Particle bestParticle = GetBestParticle();
            for (int index = 0; index < Count; index++)
            {
                ParticleSwarm particleSwarm = this[index];
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
            Particle bestParticle = GetBestParticle();
            _writeMessageLine($"{"Best Evaluation Error",-padding} = {bestParticle.BestEvaluationError,10:0.000}");
            _writeMessageLine(null);
            for (int swarmIndex = 0; swarmIndex < Count; swarmIndex++)
            {
                ParticleSwarm particleSwarm = this[swarmIndex];
                Particle bestSwarmParticle = particleSwarm.GetBestParticle();
                _writeMessageLine($"Particle Swarm {swarmIndex:00} Best Evaluation Error = {bestSwarmParticle.BestEvaluationError,10:0.000}");
                for (int particleIndex = 0; particleIndex < particleSwarm.Particles.Count; particleIndex++)
                {
                    Particle particle = particleSwarm.Particles[particleIndex];
                    _writeMessageLine($"  Particle {particleIndex:00} Evaluation Error          = {particle.EvaluationError,10:0.000}");
                }
            }
            _writeMessageLine(null);
            for (int parameterIndex = 0; parameterIndex < bestParticle.BestParameters.Count; parameterIndex++)
            {
                Parameter parameter = bestParticle.BestParameters[parameterIndex];
                _writeMessageLine($"{parameter.Name,-padding} = {parameter.Value,6}");
            }
        }
    }
}
