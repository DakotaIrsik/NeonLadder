using Platformer.Mechanics;
using Platformer.Mechanics.Stats;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    public class PlayerEnemyCollision : Event<PlayerEnemyCollision>
    {
        public EnemyController enemy;
        public PlayerController player;

        PlatformerModel model = GetModel<PlatformerModel>();

        public override void Execute()
        {
            //get enemys capsulecollider2d
            var enemyCollider = enemy.GetComponent<CapsuleCollider2D>();
            if (!enemyCollider.isTrigger)
            {
                var willHurtEnemy = player.Bounds.center.y >= enemy.Bounds.max.y;
                var willHurtPlayer = !willHurtEnemy;
                if (willHurtEnemy)
                {
                    var enemyHealth = enemy.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.Decrement();
                        if (!enemyHealth.IsAlive)
                        {
                            Schedule<EnemyDeath>().enemy = enemy;
                            player.Bounce(2);
                        }
                        else
                        {
                            player.Bounce(7);
                        }
                    }
                    else
                    {
                        Schedule<EnemyDeath>().enemy = enemy;
                        player.Bounce(2);
                    }
                }
                
                if (willHurtPlayer)
                {
                    player.health.Decrement();
                    player.playerActions.knockback = true;
                }

            }
        }
    }
}