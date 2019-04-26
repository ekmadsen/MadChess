// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System;


namespace ErikTheCoder.MadChess.Engine.Tuning
{
    public sealed class Particle
    {
        public readonly PgnGames PgnGames;
        public readonly Parameters Parameters;
        public readonly Parameters BestParameters;
        public double EvaluationError;
        public double BestEvaluationError;
        private const double _maxInitialVelocityPercent = 0.10;
        private const double _inertia = 0.75d;
        private const double _influence = 1.50d;
        private readonly double[] _velocities;
        
        
        public Particle(PgnGames PgnGames, Parameters Parameters)
        {
            this.PgnGames = PgnGames;
            this.Parameters = Parameters;
            BestParameters = Parameters.DuplicateWithSameValues();
            EvaluationError = double.MaxValue;
            BestEvaluationError = double.MaxValue;
            // Initialize random velocities.
            _velocities = new double[Parameters.Count];
            InitializeRandomVelocities();
        }


        private void InitializeRandomVelocities()
        {
            for (int index = 0; index < Parameters.Count; index++)
            {
                Parameter parameter = Parameters[index];
                double maxVelocity = _maxInitialVelocityPercent * (parameter.MaxValue - parameter.MinValue);
                // Allow positive or negative velocity.
                _velocities[index] = (SafeRandom.NextDouble() * maxVelocity * 2) - maxVelocity;
            }
        }


        public void Move()
        {
            // Move particle in parameter space.
            for (int index = 0; index < Parameters.Count; index++)
            {
                Parameter parameter = Parameters[index];
                parameter.Value += (int) _velocities[index];
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


        public void ConfigureEvaluation(Evaluation Evaluation)
        {
            EvaluationConfig evalConfig = Evaluation.Config;
            // Pawns
            evalConfig.MgPawnAdvancement = Parameters[nameof(EvaluationConfig.MgPawnAdvancement)].Value;
            evalConfig.EgPawnAdvancement = Parameters[nameof(EvaluationConfig.EgPawnAdvancement)].Value;
            evalConfig.MgPawnCentrality = Parameters[nameof(EvaluationConfig.MgPawnCentrality)].Value;
            evalConfig.EgPawnCentrality = Parameters[nameof(EvaluationConfig.EgPawnCentrality)].Value;
            evalConfig.EgPawnConstant = Parameters[nameof(EvaluationConfig.EgPawnConstant)].Value;
            // Knights
            evalConfig.MgKnightAdvancement = Parameters[nameof(EvaluationConfig.MgKnightAdvancement)].Value;
            evalConfig.EgKnightAdvancement = Parameters[nameof(EvaluationConfig.EgKnightAdvancement)].Value;
            evalConfig.MgKnightCentrality = Parameters[nameof(EvaluationConfig.MgKnightCentrality)].Value;
            evalConfig.EgKnightCentrality = Parameters[nameof(EvaluationConfig.EgKnightCentrality)].Value;
            evalConfig.MgKnightCorner = Parameters[nameof(EvaluationConfig.MgKnightCorner)].Value;
            evalConfig.EgKnightCorner = Parameters[nameof(EvaluationConfig.EgKnightCorner)].Value;
            evalConfig.MgKnightConstant = Parameters[nameof(EvaluationConfig.MgKnightConstant)].Value;
            evalConfig.EgKnightConstant = Parameters[nameof(EvaluationConfig.EgKnightConstant)].Value;
            // Bishops
            evalConfig.MgBishopAdvancement = Parameters[nameof(EvaluationConfig.MgBishopAdvancement)].Value;
            evalConfig.EgBishopAdvancement = Parameters[nameof(EvaluationConfig.EgBishopAdvancement)].Value;
            evalConfig.MgBishopCentrality = Parameters[nameof(EvaluationConfig.MgBishopCentrality)].Value;
            evalConfig.EgBishopCentrality = Parameters[nameof(EvaluationConfig.EgBishopCentrality)].Value;
            evalConfig.MgBishopCorner = Parameters[nameof(EvaluationConfig.MgBishopCorner)].Value;
            evalConfig.EgBishopCorner = Parameters[nameof(EvaluationConfig.EgBishopCorner)].Value;
            evalConfig.MgBishopConstant = Parameters[nameof(EvaluationConfig.MgBishopConstant)].Value;
            evalConfig.EgBishopConstant = Parameters[nameof(EvaluationConfig.EgBishopConstant)].Value;
            // Rooks
            evalConfig.MgRookAdvancement = Parameters[nameof(EvaluationConfig.MgRookAdvancement)].Value;
            evalConfig.EgRookAdvancement = Parameters[nameof(EvaluationConfig.EgRookAdvancement)].Value;
            evalConfig.MgRookCentrality = Parameters[nameof(EvaluationConfig.MgRookCentrality)].Value;
            evalConfig.EgRookCentrality = Parameters[nameof(EvaluationConfig.EgRookCentrality)].Value;
            evalConfig.MgRookCorner = Parameters[nameof(EvaluationConfig.MgRookCorner)].Value;
            evalConfig.EgRookCorner = Parameters[nameof(EvaluationConfig.EgRookCorner)].Value;
            evalConfig.MgRookConstant = Parameters[nameof(EvaluationConfig.MgRookConstant)].Value;
            evalConfig.EgRookConstant = Parameters[nameof(EvaluationConfig.EgRookConstant)].Value;
            // Queens
            evalConfig.MgQueenAdvancement = Parameters[nameof(EvaluationConfig.MgQueenAdvancement)].Value;
            evalConfig.EgQueenAdvancement = Parameters[nameof(EvaluationConfig.EgQueenAdvancement)].Value;
            evalConfig.MgQueenCentrality = Parameters[nameof(EvaluationConfig.MgQueenCentrality)].Value;
            evalConfig.EgQueenCentrality = Parameters[nameof(EvaluationConfig.EgQueenCentrality)].Value;
            evalConfig.MgQueenCorner = Parameters[nameof(EvaluationConfig.MgQueenCorner)].Value;
            evalConfig.EgQueenCorner = Parameters[nameof(EvaluationConfig.EgQueenCorner)].Value;
            evalConfig.MgQueenConstant = Parameters[nameof(EvaluationConfig.MgQueenConstant)].Value;
            evalConfig.EgQueenConstant = Parameters[nameof(EvaluationConfig.EgQueenConstant)].Value;
            // King
            evalConfig.MgKingAdvancement = Parameters[nameof(EvaluationConfig.MgKingAdvancement)].Value;
            evalConfig.EgKingAdvancement = Parameters[nameof(EvaluationConfig.EgKingAdvancement)].Value;
            evalConfig.MgKingCentrality = Parameters[nameof(EvaluationConfig.MgKingCentrality)].Value;
            evalConfig.EgKingCentrality = Parameters[nameof(EvaluationConfig.EgKingCentrality)].Value;
            evalConfig.MgKingCorner = Parameters[nameof(EvaluationConfig.MgKingCorner)].Value;
            evalConfig.EgKingCorner = Parameters[nameof(EvaluationConfig.EgKingCorner)].Value;
            // Passed Pawns
            evalConfig.MgPassedPawnScalePercent = Parameters[nameof(EvaluationConfig.MgPassedPawnScalePercent)].Value;
            evalConfig.EgPassedPawnScalePercent = Parameters[nameof(EvaluationConfig.EgPassedPawnScalePercent)].Value;
            evalConfig.EgFreePassedPawnScalePercent = Parameters[nameof(EvaluationConfig.EgFreePassedPawnScalePercent)].Value;
            evalConfig.EgKingEscortedPassedPawn = Parameters[nameof(EvaluationConfig.EgKingEscortedPassedPawn)].Value;
            Evaluation.Configure();
        }
        

        // See http://talkchess.com/forum/viewtopic.php?t=50823&postdays=0&postorder=asc&highlight=texel+tuning&topic_view=flat&start=20.
        public void CalculateEvaluationError(Board Board, Search Search, int WinPercentScale)
        {
            // Sum the square of evaluation error over all games.
            double evaluationError = 0;
            for (int gameIndex = 0; gameIndex < PgnGames.Count; gameIndex++)
            {
                PgnGame game = PgnGames[gameIndex];
                if (game.Result == GameResult.Unknown) continue; // Skip games with unknown results.
                Board.SetPosition(Board.StartPositionFen, true);
                for (int moveIndex = 0; moveIndex < game.Moves.Count; moveIndex++)
                {
                    ulong move = game.Moves[moveIndex];
                    // Play move.
                    Board.PlayMove(move);
                    // Get quiet score.
                    Board.NodesExamineTime = long.MaxValue;
                    Search.PvInfoUpdate = false;
                    Search.Continue = true;
                    int quietScore = Search.GetQuietScore(Board, 1, 1, Board.AllSquaresMask, -StaticScore.Max, StaticScore.Max);
                    // Convert quiet score to win percent.
                    double winPercent = GetWinPercent(quietScore, WinPercentScale);
                    // Compare win percent to game result.
                    double result;
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (game.Result)
                    {
                        case GameResult.WhiteWon:
                            result = Board.CurrentPosition.WhiteMove ? 1d : 0;
                            break;
                        case GameResult.Draw:
                            result = 0.5d;
                            break;
                        case GameResult.BlackWon:
                            result = Board.CurrentPosition.WhiteMove ? 0 : 1d;
                            break;
                        default:
                            throw new InvalidOperationException($"{game.Result} game result not supported.");
                    }
                    evaluationError += Math.Pow(winPercent - result, 2);
                }
            }
            EvaluationError = evaluationError;
            if (EvaluationError < BestEvaluationError)
            {
                BestEvaluationError = EvaluationError;
                Parameters.CopyValuesTo(BestParameters);
            }
        }


        public void UpdateVelocity(Particle BestSwarmParticle, Particle GloballyBestParticle)
        {
            for (int index = 0; index < Parameters.Count; index++)
            {
                Parameter parameter = Parameters[index];
                Parameter bestParameter = BestParameters[index];
                Parameter bestSwarmParameter = BestSwarmParticle.BestParameters[index];
                Parameter globallyBestParameter = GloballyBestParticle.BestParameters[index];
                double velocity = _inertia * _velocities[index];
                double particleMagnitude = SafeRandom.NextDouble() * _influence;
                velocity += particleMagnitude * (bestParameter.Value - parameter.Value);
                double swarmMagnitude = SafeRandom.NextDouble() * ParticleSwarm.Influence;
                velocity += swarmMagnitude * (bestSwarmParameter.Value - parameter.Value);
                double allSwarmsMagnitude = SafeRandom.NextDouble() * ParticleSwarms.Influence;
                velocity += allSwarmsMagnitude * (globallyBestParameter.Value - parameter.Value);
                _velocities[index] = velocity;
            }
        }
        

        private static double GetWinPercent(int Score, int WinPercentScale) => 1d / (1d + Math.Pow(10d, -1d * Score / WinPercentScale)); // Use a sigmoid function to map score to winning percent.  See WinPercent.xlsx.
    }
}
