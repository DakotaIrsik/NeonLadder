using Assets.Scripts;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Represebts the current vital statistics of some game entity.
    /// </summary>
    public class Stamina : MonoBehaviour
    {
        /// <summary>
        /// The maximum hit points for the entity.
        /// </summary>
        public int maxStamina = Constants.MaxStamina;

        /// <summary>
        /// Indicates if the entity should be considered 'Exhausted'.
        /// </summary>
        public bool IsExhausted => currentStamina == 0;

        [SerializeField]
        public int currentStamina;

        /// <summary>
        /// Increment the Stamina of the entity.
        /// </summary>
        public void Increment(int amount = 1)
        {
            currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
        }

        /// <summary>
        /// Decrement the Stamina of the entity. Will trigger a HealthIsZero event when
        /// current Stamina reaches 0.
        /// </summary>
        public void Decrement(int amount = 1)
        {
            currentStamina = Mathf.Clamp(currentStamina - amount, 0, maxStamina);
            if (currentStamina == 0)
            {
                var ev = Schedule<StaminaIsZero>();
                ev.stamina = this;
            }
        }

        /// <summary>
        /// Decrement the Stamina of the entitiy until Stamina reaches 0.
        /// </summary>
        public void Exhaust()
        {
            while (currentStamina > 0) Decrement();
        }

        void Awake()
        {
            currentStamina = maxStamina;
        }
    }
}
