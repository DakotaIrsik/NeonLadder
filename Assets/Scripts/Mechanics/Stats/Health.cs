using Assets.Scripts;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics.Stats
{
    public class Health : BaseStat
    {
        public bool IsAlive => !IsDepleted;

        new void Awake()
        {
            //max = Constants.MaxHealth; 
            base.Awake(); 
        }

        protected override void OnDepleted()
        {
            var ev = Schedule<HealthIsZero>();
            ev.health = this;
        }

        public void Die()
        {
            Deplete();
        }
    }
}
