using Assets.Scripts;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics.Stats
{
    public class Stamina : BaseStat
    {
        public bool IsExhausted => IsDepleted;

        new void Awake()
        {
            max = Constants.MaxStamina;
            base.Awake();
        }

        protected override void OnDepleted()
        {
            var ev = Schedule<StaminaIsZero>();
            ev.stamina = this;
        }

        public void Exhaust()
        {
            Deplete();
        }
    }
}
