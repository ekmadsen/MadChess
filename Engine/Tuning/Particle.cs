// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
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
            for (var index = 0; index < Parameters.Count; index++)
            {
                var parameter = Parameters[index];
                var maxVelocity = _maxInitialVelocityPercent * (parameter.MaxValue - parameter.MinValue);
                // Allow positive or negative velocity.
                _velocities[index] = (SafeRandom.NextDouble() * maxVelocity * 2) - maxVelocity;
            }
        }


        public void Move()
        {
            // Move particle in parameter space.
            for (var index = 0; index < Parameters.Count; index++)
            {
                var parameter = Parameters[index];
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
            // Endgame Material
            Evaluation.Config.EgPawnMaterial = Parameters[nameof(EvaluationConfig.EgPawnMaterial)].Value;
            Evaluation.Config.EgKnightMaterial = Parameters[nameof(EvaluationConfig.EgKnightMaterial)].Value;
            Evaluation.Config.EgBishopMaterial = Parameters[nameof(EvaluationConfig.EgBishopMaterial)].Value;
            Evaluation.Config.EgRookMaterial = Parameters[nameof(EvaluationConfig.EgRookMaterial)].Value;
            Evaluation.Config.EgQueenMaterial = Parameters[nameof(EvaluationConfig.EgQueenMaterial)].Value;
            // Pawns
            Evaluation.Config.MgPawnAdvancement = Parameters[nameof(EvaluationConfig.MgPawnAdvancement)].Value;
            Evaluation.Config.EgPawnAdvancement = Parameters[nameof(EvaluationConfig.EgPawnAdvancement)].Value;
            Evaluation.Config.MgPawnCentrality = Parameters[nameof(EvaluationConfig.MgPawnCentrality)].Value;
            Evaluation.Config.EgPawnCentrality = Parameters[nameof(EvaluationConfig.EgPawnCentrality)].Value;
            // Knights
            Evaluation.Config.MgKnightAdvancement = Parameters[nameof(EvaluationConfig.MgKnightAdvancement)].Value;
            Evaluation.Config.EgKnightAdvancement = Parameters[nameof(EvaluationConfig.EgKnightAdvancement)].Value;
            Evaluation.Config.MgKnightCentrality = Parameters[nameof(EvaluationConfig.MgKnightCentrality)].Value;
            Evaluation.Config.EgKnightCentrality = Parameters[nameof(EvaluationConfig.EgKnightCentrality)].Value;
            Evaluation.Config.MgKnightCorner = Parameters[nameof(EvaluationConfig.MgKnightCorner)].Value;
            Evaluation.Config.EgKnightCorner = Parameters[nameof(EvaluationConfig.EgKnightCorner)].Value;
            // Bishops
            Evaluation.Config.MgBishopAdvancement = Parameters[nameof(EvaluationConfig.MgBishopAdvancement)].Value;
            Evaluation.Config.EgBishopAdvancement = Parameters[nameof(EvaluationConfig.EgBishopAdvancement)].Value;
            Evaluation.Config.MgBishopCentrality = Parameters[nameof(EvaluationConfig.MgBishopCentrality)].Value;
            Evaluation.Config.EgBishopCentrality = Parameters[nameof(EvaluationConfig.EgBishopCentrality)].Value;
            Evaluation.Config.MgBishopCorner = Parameters[nameof(EvaluationConfig.MgBishopCorner)].Value;
            Evaluation.Config.EgBishopCorner = Parameters[nameof(EvaluationConfig.EgBishopCorner)].Value;
            // Rooks
            //Evaluation.Config.MgRookAdvancement = Parameters[nameof(EvaluationConfig.MgRookAdvancement)].Value;
            Evaluation.Config.EgRookAdvancement = Parameters[nameof(EvaluationConfig.EgRookAdvancement)].Value;
            Evaluation.Config.MgRookCentrality = Parameters[nameof(EvaluationConfig.MgRookCentrality)].Value;
            Evaluation.Config.EgRookCentrality = Parameters[nameof(EvaluationConfig.EgRookCentrality)].Value;
            Evaluation.Config.MgRookCorner = Parameters[nameof(EvaluationConfig.MgRookCorner)].Value;
            Evaluation.Config.EgRookCorner = Parameters[nameof(EvaluationConfig.EgRookCorner)].Value;
            // Queens
            //Evaluation.Config.MgQueenAdvancement = Parameters[nameof(EvaluationConfig.MgQueenAdvancement)].Value;
            Evaluation.Config.EgQueenAdvancement = Parameters[nameof(EvaluationConfig.EgQueenAdvancement)].Value;
            Evaluation.Config.MgQueenCentrality = Parameters[nameof(EvaluationConfig.MgQueenCentrality)].Value;
            Evaluation.Config.EgQueenCentrality = Parameters[nameof(EvaluationConfig.EgQueenCentrality)].Value;
            Evaluation.Config.MgQueenCorner = Parameters[nameof(EvaluationConfig.MgQueenCorner)].Value;
            Evaluation.Config.EgQueenCorner = Parameters[nameof(EvaluationConfig.EgQueenCorner)].Value;
            // King
            Evaluation.Config.MgKingAdvancement = Parameters[nameof(EvaluationConfig.MgKingAdvancement)].Value;
            Evaluation.Config.EgKingAdvancement = Parameters[nameof(EvaluationConfig.EgKingAdvancement)].Value;
            Evaluation.Config.MgKingCentrality = Parameters[nameof(EvaluationConfig.MgKingCentrality)].Value;
            Evaluation.Config.EgKingCentrality = Parameters[nameof(EvaluationConfig.EgKingCentrality)].Value;
            Evaluation.Config.MgKingCorner = Parameters[nameof(EvaluationConfig.MgKingCorner)].Value;
            Evaluation.Config.EgKingCorner = Parameters[nameof(EvaluationConfig.EgKingCorner)].Value;
            // Passed Pawns
            Evaluation.Config.PassedPawnPowerPer128 = Parameters[nameof(EvaluationConfig.PassedPawnPowerPer128)].Value;
            Evaluation.Config.MgPassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.MgPassedPawnScalePer128)].Value;
            Evaluation.Config.EgPassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.EgPassedPawnScalePer128)].Value;
            Evaluation.Config.EgFreePassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.EgFreePassedPawnScalePer128)].Value;
            Evaluation.Config.EgKingEscortedPassedPawn = Parameters[nameof(EvaluationConfig.EgKingEscortedPassedPawn)].Value;
            // Piece Mobility
            Evaluation.Config.PieceMobilityPowerPer128 = Parameters[nameof(EvaluationConfig.PieceMobilityPowerPer128)].Value;
            Evaluation.Config.MgKnightMobilityScale = Parameters[nameof(EvaluationConfig.MgKnightMobilityScale)].Value;
            Evaluation.Config.EgKnightMobilityScale = Parameters[nameof(EvaluationConfig.EgKnightMobilityScale)].Value;
            Evaluation.Config.MgBishopMobilityScale = Parameters[nameof(EvaluationConfig.MgBishopMobilityScale)].Value;
            Evaluation.Config.EgBishopMobilityScale = Parameters[nameof(EvaluationConfig.EgBishopMobilityScale)].Value;
            Evaluation.Config.MgRookMobilityScale = Parameters[nameof(EvaluationConfig.MgRookMobilityScale)].Value;
            Evaluation.Config.EgRookMobilityScale = Parameters[nameof(EvaluationConfig.EgRookMobilityScale)].Value;
            Evaluation.Config.MgQueenMobilityScale = Parameters[nameof(EvaluationConfig.MgQueenMobilityScale)].Value;
            Evaluation.Config.EgQueenMobilityScale = Parameters[nameof(EvaluationConfig.EgQueenMobilityScale)].Value;
            Evaluation.CalculatePositionalFactors();
            // King Safety
            Evaluation.Config.KingSafetyPowerPer128 = Parameters[nameof(EvaluationConfig.KingSafetyPowerPer128)].Value;
            Evaluation.Config.MgKingSafetySemiOpenFilePer8 = Parameters[nameof(EvaluationConfig.MgKingSafetySemiOpenFilePer8)].Value;
            Evaluation.Config.KingSafetyMinorAttackOuterRingPer8 = Parameters[nameof(EvaluationConfig.KingSafetyMinorAttackOuterRingPer8)].Value;
            Evaluation.Config.KingSafetyMinorAttackInnerRingPer8 = Parameters[nameof(EvaluationConfig.KingSafetyMinorAttackInnerRingPer8)].Value;
            Evaluation.Config.KingSafetyRookAttackOuterRingPer8 = Parameters[nameof(EvaluationConfig.KingSafetyRookAttackOuterRingPer8)].Value;
            Evaluation.Config.KingSafetyRookAttackInnerRingPer8 = Parameters[nameof(EvaluationConfig.KingSafetyRookAttackInnerRingPer8)].Value;
            Evaluation.Config.KingSafetyQueenAttackOuterRingPer8 = Parameters[nameof(EvaluationConfig.KingSafetyQueenAttackOuterRingPer8)].Value;
            Evaluation.Config.KingSafetyQueenAttackInnerRingPer8 = Parameters[nameof(EvaluationConfig.KingSafetyQueenAttackInnerRingPer8)].Value;
            Evaluation.Config.KingSafetyScalePer128 = Parameters[nameof(EvaluationConfig.KingSafetyScalePer128)].Value;
        }
        

        // See http://talkchess.com/forum/viewtopic.php?t=50823&postdays=0&postorder=asc&highlight=texel+tuning&topic_view=flat&start=20.
        public void CalculateEvaluationError(Board Board, Search Search, int WinPercentScale)
        {
            // Sum the square of evaluation error over all games.
            double evaluationError = 0;
            for (var gameIndex = 0; gameIndex < PgnGames.Count; gameIndex++)
            {
                var game = PgnGames[gameIndex];
                if (game.Result == GameResult.Unknown) continue; // Skip games with unknown results.
                Board.SetPosition(Board.StartPositionFen, true);
                for (var moveIndex = 0; moveIndex < game.Moves.Count; moveIndex++)
                {
                    var move = game.Moves[moveIndex];
                    // Play move.
                    Board.PlayMove(move);
                    // Get quiet score.
                    Board.NodesExamineTime = long.MaxValue;
                    Search.PvInfoUpdate = false;
                    Search.Continue = true;
                    var quietScore = Search.GetQuietScore(Board, 1, 1, Board.AllSquaresMask, -StaticScore.Max, StaticScore.Max);
                    // Convert quiet score to win percent.
                    var winPercent = GetWinPercent(quietScore, WinPercentScale);
                    // Compare win percent to game result.
                    // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                    var result = game.Result switch
                    {
                        GameResult.WhiteWon => (Board.CurrentPosition.WhiteMove ? 1d : 0),
                        GameResult.Draw => 0.5d,
                        GameResult.BlackWon => (Board.CurrentPosition.WhiteMove ? 0 : 1d),
                        _ => throw new InvalidOperationException($"{game.Result} game result not supported.")
                    };
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
            for (var index = 0; index < Parameters.Count; index++)
            {
                var parameter = Parameters[index];
                var bestParameter = BestParameters[index];
                var bestSwarmParameter = BestSwarmParticle.BestParameters[index];
                var globallyBestParameter = GloballyBestParticle.BestParameters[index];
                var velocity = _inertia * _velocities[index];
                var particleMagnitude = SafeRandom.NextDouble() * _influence;
                velocity += particleMagnitude * (bestParameter.Value - parameter.Value);
                var swarmMagnitude = SafeRandom.NextDouble() * ParticleSwarm.Influence;
                velocity += swarmMagnitude * (bestSwarmParameter.Value - parameter.Value);
                var allSwarmsMagnitude = SafeRandom.NextDouble() * ParticleSwarms.Influence;
                velocity += allSwarmsMagnitude * (globallyBestParameter.Value - parameter.Value);
                _velocities[index] = velocity;
            }
        }
        

        private static double GetWinPercent(int Score, int WinPercentScale) => 1d / (1d + Math.Pow(10d, -1d * Score / WinPercentScale)); // Use a sigmoid function to map score to winning percent.
    }
}
