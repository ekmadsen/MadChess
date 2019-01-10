// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
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
        private const double _particleDeathPercent = 0.05d;
        public readonly Particles Particles;
        private readonly int _winPercentScale;
        

        public ParticleSwarm(PgnGames PgnGames, Parameters Parameters, int Particles, int WinPercentScale)
        {
            // Create particles at random locations.
            this.Particles = new Particles();
            _winPercentScale = WinPercentScale;
            for (int particle = 0; particle < Particles; particle++) this.Particles.Add(new Particle(PgnGames, Parameters.DuplicateWithRandomValues()));
        }


        public Particle GetBestParticle()
        {
            Particle bestParticle = Particles[0];
            for (int index = 1; index < Particles.Count; index++)
            {
                Particle particle = Particles[index];
                if (particle.BestEvaluationError < bestParticle.BestEvaluationError) bestParticle = particle;
            }
            return bestParticle;
        }


        public void RandomizeParticles(Particle BestParticle)
        {
            for (int index = 1; index < Particles.Count; index++)
            {
                Particle particle = Particles[index];
                if (!ReferenceEquals(particle, BestParticle)) 
                {
                    // Recreate particle at random location.
                    particle = new Particle(particle.PgnGames, particle.Parameters.DuplicateWithRandomValues());
                    Particles[index] = particle;
                }
            }
        }
        

        public void Iterate(Board Board, Search Search, Evaluation Evaluation)
        {
            Particle bestParticle = GetBestParticle();
            for (int index = 0; index < Particles.Count; index++)
            {
                Particle particle = Particles[index];
                if (!ReferenceEquals(particle, bestParticle) && (SafeRandom.NextDouble() <= _particleDeathPercent))
                {
                    // Recreate particle at random location.
                    particle = new Particle(particle.PgnGames, particle.Parameters.DuplicateWithRandomValues());
                    Particles[index] = particle;
                }
                particle.Move();
                particle.ConfigureEvaluation(Evaluation);
                particle.CalculateEvaluationError(Board, Search, _winPercentScale);
            }
        }


        public void UpdateVelocity(Particle GloballyBestParticle)
        {
            Particle bestSwarmParticle = GetBestParticle();
            for (int index = 0; index < Particles.Count; index++)
            {
                Particle particle = Particles[index];
                particle.UpdateVelocity(bestSwarmParticle, GloballyBestParticle);
            }
        }
    }
}
