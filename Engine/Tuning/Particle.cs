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


        public void ConfigureEvaluation(Evaluation evaluation)
        {
            // Endgame Material
            evaluation.Config.EgKnightMaterial = Parameters[nameof(EvaluationConfig.EgKnightMaterial)].Value;
            evaluation.Config.EgBishopMaterial = Parameters[nameof(EvaluationConfig.EgBishopMaterial)].Value;
            evaluation.Config.EgRookMaterial = Parameters[nameof(EvaluationConfig.EgRookMaterial)].Value;
            evaluation.Config.EgQueenMaterial = Parameters[nameof(EvaluationConfig.EgQueenMaterial)].Value;
            // Pawn Location
            evaluation.Config.MgPawnAdvancement = Parameters[nameof(EvaluationConfig.MgPawnAdvancement)].Value;
            evaluation.Config.EgPawnAdvancement = Parameters[nameof(EvaluationConfig.EgPawnAdvancement)].Value;
            evaluation.Config.MgPawnCentrality = Parameters[nameof(EvaluationConfig.MgPawnCentrality)].Value;
            evaluation.Config.EgPawnCentrality = Parameters[nameof(EvaluationConfig.EgPawnCentrality)].Value;
            // Knight Location
            evaluation.Config.MgKnightAdvancement = Parameters[nameof(EvaluationConfig.MgKnightAdvancement)].Value;
            evaluation.Config.EgKnightAdvancement = Parameters[nameof(EvaluationConfig.EgKnightAdvancement)].Value;
            evaluation.Config.MgKnightCentrality = Parameters[nameof(EvaluationConfig.MgKnightCentrality)].Value;
            evaluation.Config.EgKnightCentrality = Parameters[nameof(EvaluationConfig.EgKnightCentrality)].Value;
            evaluation.Config.MgKnightCorner = Parameters[nameof(EvaluationConfig.MgKnightCorner)].Value;
            evaluation.Config.EgKnightCorner = Parameters[nameof(EvaluationConfig.EgKnightCorner)].Value;
            // Bishop Location
            evaluation.Config.MgBishopAdvancement = Parameters[nameof(EvaluationConfig.MgBishopAdvancement)].Value;
            evaluation.Config.EgBishopAdvancement = Parameters[nameof(EvaluationConfig.EgBishopAdvancement)].Value;
            evaluation.Config.MgBishopCentrality = Parameters[nameof(EvaluationConfig.MgBishopCentrality)].Value;
            evaluation.Config.EgBishopCentrality = Parameters[nameof(EvaluationConfig.EgBishopCentrality)].Value;
            evaluation.Config.MgBishopCorner = Parameters[nameof(EvaluationConfig.MgBishopCorner)].Value;
            evaluation.Config.EgBishopCorner = Parameters[nameof(EvaluationConfig.EgBishopCorner)].Value;
            // Rook Location
            evaluation.Config.MgRookAdvancement = Parameters[nameof(EvaluationConfig.MgRookAdvancement)].Value;
            evaluation.Config.EgRookAdvancement = Parameters[nameof(EvaluationConfig.EgRookAdvancement)].Value;
            evaluation.Config.MgRookCentrality = Parameters[nameof(EvaluationConfig.MgRookCentrality)].Value;
            evaluation.Config.EgRookCentrality = Parameters[nameof(EvaluationConfig.EgRookCentrality)].Value;
            evaluation.Config.MgRookCorner = Parameters[nameof(EvaluationConfig.MgRookCorner)].Value;
            evaluation.Config.EgRookCorner = Parameters[nameof(EvaluationConfig.EgRookCorner)].Value;
            // Queen Location
            evaluation.Config.MgQueenAdvancement = Parameters[nameof(EvaluationConfig.MgQueenAdvancement)].Value;
            evaluation.Config.EgQueenAdvancement = Parameters[nameof(EvaluationConfig.EgQueenAdvancement)].Value;
            evaluation.Config.MgQueenCentrality = Parameters[nameof(EvaluationConfig.MgQueenCentrality)].Value;
            evaluation.Config.EgQueenCentrality = Parameters[nameof(EvaluationConfig.EgQueenCentrality)].Value;
            evaluation.Config.MgQueenCorner = Parameters[nameof(EvaluationConfig.MgQueenCorner)].Value;
            evaluation.Config.EgQueenCorner = Parameters[nameof(EvaluationConfig.EgQueenCorner)].Value;
            // King Location
            evaluation.Config.MgKingAdvancement = Parameters[nameof(EvaluationConfig.MgKingAdvancement)].Value;
            evaluation.Config.EgKingAdvancement = Parameters[nameof(EvaluationConfig.EgKingAdvancement)].Value;
            evaluation.Config.MgKingCentrality = Parameters[nameof(EvaluationConfig.MgKingCentrality)].Value;
            evaluation.Config.EgKingCentrality = Parameters[nameof(EvaluationConfig.EgKingCentrality)].Value;
            evaluation.Config.MgKingCorner = Parameters[nameof(EvaluationConfig.MgKingCorner)].Value;
            evaluation.Config.EgKingCorner = Parameters[nameof(EvaluationConfig.EgKingCorner)].Value;
            // Passed Pawns
            evaluation.Config.PassedPawnPowerPer128 = Parameters[nameof(EvaluationConfig.PassedPawnPowerPer128)].Value;
            evaluation.Config.MgPassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.MgPassedPawnScalePer128)].Value;
            evaluation.Config.EgPassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.EgPassedPawnScalePer128)].Value;
            evaluation.Config.EgFreePassedPawnScalePer128 = Parameters[nameof(EvaluationConfig.EgFreePassedPawnScalePer128)].Value;
            evaluation.Config.EgKingEscortedPassedPawn = Parameters[nameof(EvaluationConfig.EgKingEscortedPassedPawn)].Value;
            // Piece Mobility
            evaluation.Config.PieceMobilityPowerPer128 = Parameters[nameof(EvaluationConfig.PieceMobilityPowerPer128)].Value;
            evaluation.Config.MgKnightMobilityScale = Parameters[nameof(EvaluationConfig.MgKnightMobilityScale)].Value;
            evaluation.Config.EgKnightMobilityScale = Parameters[nameof(EvaluationConfig.EgKnightMobilityScale)].Value;
            evaluation.Config.MgBishopMobilityScale = Parameters[nameof(EvaluationConfig.MgBishopMobilityScale)].Value;
            evaluation.Config.EgBishopMobilityScale = Parameters[nameof(EvaluationConfig.EgBishopMobilityScale)].Value;
            evaluation.Config.MgRookMobilityScale = Parameters[nameof(EvaluationConfig.MgRookMobilityScale)].Value;
            evaluation.Config.EgRookMobilityScale = Parameters[nameof(EvaluationConfig.EgRookMobilityScale)].Value;
            evaluation.Config.MgQueenMobilityScale = Parameters[nameof(EvaluationConfig.MgQueenMobilityScale)].Value;
            evaluation.Config.EgQueenMobilityScale = Parameters[nameof(EvaluationConfig.EgQueenMobilityScale)].Value;
            // King Safety
            evaluation.Config.MgKingSafetyPowerPer128 = Parameters[nameof(EvaluationConfig.MgKingSafetyPowerPer128)].Value;
            evaluation.Config.MgKingSafetyScalePer128 = Parameters[nameof(EvaluationConfig.MgKingSafetyScalePer128)].Value;
            evaluation.Config.MgKingSafetyMinorAttackOuterRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyMinorAttackOuterRingPer8)].Value;
            evaluation.Config.MgKingSafetyMinorAttackInnerRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyMinorAttackInnerRingPer8)].Value;
            evaluation.Config.MgKingSafetyRookAttackOuterRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyRookAttackOuterRingPer8)].Value;
            evaluation.Config.MgKingSafetyRookAttackInnerRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyRookAttackInnerRingPer8)].Value;
            evaluation.Config.MgKingSafetyQueenAttackOuterRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyQueenAttackOuterRingPer8)].Value;
            evaluation.Config.MgKingSafetyQueenAttackInnerRingPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyQueenAttackInnerRingPer8)].Value;
            evaluation.Config.MgKingSafetySemiOpenFilePer8 = Parameters[nameof(EvaluationConfig.MgKingSafetySemiOpenFilePer8)].Value;
            evaluation.Config.MgKingSafetyPawnShieldPer8 = Parameters[nameof(EvaluationConfig.MgKingSafetyPawnShieldPer8)].Value;
            // Minor Pieces
            evaluation.Config.MgBishopPair = Parameters[nameof(EvaluationConfig.MgBishopPair)].Value;
            evaluation.Config.EgBishopPair = Parameters[nameof(EvaluationConfig.EgBishopPair)].Value;
            // Endgame Scaling
            evaluation.Config.EgBishopAdvantagePer128 = Parameters[nameof(EvaluationConfig.EgBishopAdvantagePer128)].Value;
            evaluation.Config.EgOppBishopsPerPassedPawn = Parameters[nameof(EvaluationConfig.EgOppBishopsPerPassedPawn)].Value;
            evaluation.Config.EgOppBishopsPer128 = Parameters[nameof(EvaluationConfig.EgOppBishopsPer128)].Value;
            evaluation.Config.EgWinningPerPawn = Parameters[nameof(EvaluationConfig.EgWinningPerPawn)].Value;
            // Calculate positional factors after updating evaluation config.
            evaluation.CalculatePositionalFactors();
        }
        

        // See http://talkchess.com/forum/viewtopic.php?t=50823&postdays=0&postorder=asc&highlight=texel+tuning&topic_view=flat&start=20.
        public void CalculateEvaluationError(Board board, Search search, int winScale)
        {
            // Sum the square of evaluation error over all games.
            double evaluationError = 0;
            for (var gameIndex = 0; gameIndex < PgnGames.Count; gameIndex++)
            {
                var game = PgnGames[gameIndex];
                if (game.Result == GameResult.Unknown) continue; // Skip games with unknown results.
                board.SetPosition(Board.StartPositionFen, true);
                for (var moveIndex = 0; moveIndex < game.Moves.Count; moveIndex++)
                {
                    var move = game.Moves[moveIndex];
                    // Play move.
                    board.PlayMove(move);
                    // Get quiet score.
                    board.NodesExamineTime = long.MaxValue;
                    search.PvInfoUpdate = false;
                    search.Continue = true;
                    var quietScore = search.GetQuietScore(board, 1, 1, -StaticScore.Max, StaticScore.Max);
                    // Convert quiet score to win fraction and compare to game result.
                    var winFraction = GetWinFraction(quietScore, winScale);
                    // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                    var result = game.Result switch
                    {
                        GameResult.WhiteWon => board.CurrentPosition.WhiteMove ? 1d : 0,
                        GameResult.Draw => 0.5d,
                        GameResult.BlackWon => board.CurrentPosition.WhiteMove ? 0 : 1d,
                        _ => throw new InvalidOperationException($"{game.Result} game result not supported.")
                    };
                    evaluationError += Math.Pow(winFraction - result, 2);
                }
            }
            EvaluationError = evaluationError;
            if (EvaluationError < BestEvaluationError)
            {
                BestEvaluationError = EvaluationError;
                Parameters.CopyValuesTo(BestParameters);
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
        

        private static double GetWinFraction(int score, int winScale) => 1d / (1d + Math.Pow(10d, -1d * score / winScale)); // Use a sigmoid function to map score to win fraction.
    }
}
