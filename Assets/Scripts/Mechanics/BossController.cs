using Assets.Scripts;
using Platformer.Mechanics.Stats;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    public class BossController : KinematicObject
    {

        public int moveDirection;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public Collider2D collider2d { get; set; }
        public readonly PlatformerModel model = GetModel<PlatformerModel>();
        public Health health;
        public Stamina stamina;
        [SerializeField]
        public float staminaRegenTimer = 0f;


        Vector2 move;
        public SpriteRenderer spriteRenderer { get; set; }
        internal Animator animator;
        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
           

        }


        protected override void Update()
        {
            RegenerateStamina();
            base.Update();
        }

        private void RegenerateStamina()
        {
            staminaRegenTimer += Time.deltaTime;
            if (staminaRegenTimer >= 0.1f) // Check if 1/10th of a second has passed
            {
                stamina.Increment(0.1f); // Increment stamina by 1/10th of a unit
                staminaRegenTimer -= 0.1f; // Decrease the timer by 0.1f instead of resetting to 0
            }
        }


        protected override void ComputeVelocity()
        {


            if (moveDirection != 0)
            {
                spriteRenderer.flipX = moveDirection < 0;
                move.x = moveDirection;
            }
            else
            {
                move.x = 0; // Stop movement when moveDirection is zero
            }

            ApplyGravity();
            GroundedAnimation();
            move.x = Mathf.Clamp(move.x, -Constants.MaxSpeed, Constants.MaxSpeed);
            targetVelocity = move * Constants.MaxSpeed;
        }

        public void ApplyGravity(float gravity = Constants.DefaultGravity)
        {
            if (!IsGrounded)
            {
                velocity.y += -gravity * Time.deltaTime;
            }
        }

        public void GroundedAnimation()
        {
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / Constants.MaxSpeed);
        }
    }
}