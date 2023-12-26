using Platformer.Mechanics.Stats;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player health reaches 0. This usually would result in a 
    /// PlayerDeath event.
    /// </summary>
    /// <typeparam name="HealthIsZero"></typeparam>
    public class HealthIsZero : Event<HealthIsZero>
    {
        public Health health;

        public override void Execute()
        {
            Schedule<PlayerDeath>();
        }
    }
}