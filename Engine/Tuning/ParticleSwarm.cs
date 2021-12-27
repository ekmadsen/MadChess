// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using ErikTheCoder.MadChess.Core.Game;
using ErikTheCoder.MadChess.Core.Utilities;
using ErikTheCoder.MadChess.Engine.Evaluation;


namespace ErikTheCoder.MadChess.Engine.Tuning;


// See https://msdn.microsoft.com/en-us/magazine/dn385711.aspx.
public sealed class ParticleSwarm
{
    public const double Influence = 1.50d;
    public readonly Particles Particles;
    private const double _particleDeathFraction = 0.05d;
    private readonly int _winScale;
        

    public ParticleSwarm(QuietPositions quietPositions, Parameters parameters, int particles, int winScale)
    {
        // Create particles at random locations.
        Particles = new Particles();
        _winScale = winScale;
        for (var particle = 0; particle < particles; particle++) Particles.Add(new Particle(quietPositions, parameters.DuplicateWithRandomValues()));
    }


    public Particle GetBestParticle()
    {
        var bestParticle = Particles[0];
        for (var index = 1; index < Particles.Count; index++)
        {
            var particle = Particles[index];
            if (particle.BestEvaluationError < bestParticle.BestEvaluationError) bestParticle = particle;
        }
        return bestParticle;
    }


    public void Iterate(Board board, Eval eval)
    {
        var bestParticle = GetBestParticle();
        for (var index = 0; index < Particles.Count; index++)
        {
            var particle = Particles[index];
            if ((particle != bestParticle) && (SafeRandom.NextDouble() <= _particleDeathFraction))
            {
                // Recreate particle at random location.
                particle = new Particle(particle.QuietPositions, particle.Parameters.DuplicateWithRandomValues());
                Particles[index] = particle;
            }
            particle.Move();
            particle.ConfigureEvaluation(eval);
            particle.CalculateEvaluationError(board, eval, _winScale);
        }
    }


    public void UpdateVelocity(Particle globallyBestParticle)
    {
        var bestSwarmParticle = GetBestParticle();
        for (var index = 0; index < Particles.Count; index++)
        {
            var particle = Particles[index];
            particle.UpdateVelocity(bestSwarmParticle, globallyBestParticle);
        }
    }
}