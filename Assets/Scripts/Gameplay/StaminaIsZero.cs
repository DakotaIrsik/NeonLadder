using Platformer.Mechanics.Stats;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player stamina reaches 0. This usually would result in a 
    /// PlayerExhausted event.
    /// </summary>
    /// <typeparam name="StaminaIsZero"></typeparam>
    public class StaminaIsZero : Event<StaminaIsZero>
    {
        public Stamina stamina;

        public override void Execute()
        {
            Schedule<PlayerExhausted>();
        }
    }
}