using Platformer.Mechanics;
using Platformer.Mechanics.Stats;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    public class MeleeEnemyCollision : Event<MeleeEnemyCollision>
    {
        public MeleeAttack melee;
        public EnemyController enemy;

        PlatformerModel model = GetModel<PlatformerModel>();

        public override void Execute()
        {
            //get enemys capsulecollider2d
            var enemyCollider = enemy.GetComponent<CapsuleCollider2D>();
            if (!enemyCollider.isTrigger)
            {
                var enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.Decrement();
                    if (!enemyHealth.IsAlive)
                    {
                        Schedule<EnemyDeath>().enemy = enemy;

                    }
                }
            }

        }
    }
}