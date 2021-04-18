// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2020.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


namespace ErikTheCoder.MadChess.Engine.Tuning
{
    // See https://msdn.microsoft.com/en-us/magazine/dn385711.aspx.
    public sealed class ParticleSwarm
    {
        public const double Influence = 1.50d;
        public readonly Particles Particles;
        private const double _particleDeathFraction = 0.05d;
        private readonly int _winScale;
        

        public ParticleSwarm(PgnGames PgnGames, Parameters Parameters, int Particles, int WinScale)
        {
            // Create particles at random locations.
            this.Particles = new Particles();
            _winScale = WinScale;
            for (var particle = 0; particle < Particles; particle++) this.Particles.Add(new Particle(PgnGames, Parameters.DuplicateWithRandomValues()));
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


        public void Iterate(Board Board, Search Search, Evaluation Evaluation)
        {
            var bestParticle = GetBestParticle();
            for (var index = 0; index < Particles.Count; index++)
            {
                var particle = Particles[index];
                if (!ReferenceEquals(particle, bestParticle) && (SafeRandom.NextDouble() <= _particleDeathFraction))
                {
                    // Recreate particle at random location.
                    particle = new Particle(particle.PgnGames, particle.Parameters.DuplicateWithRandomValues());
                    Particles[index] = particle;
                }
                particle.Move();
                particle.ConfigureEvaluation(Evaluation);
                particle.CalculateEvaluationError(Board, Search, _winScale);
            }
        }


        public void UpdateVelocity(Particle GloballyBestParticle)
        {
            var bestSwarmParticle = GetBestParticle();
            for (var index = 0; index < Particles.Count; index++)
            {
                var particle = Particles[index];
                particle.UpdateVelocity(bestSwarmParticle, GloballyBestParticle);
            }
        }
    }
}
