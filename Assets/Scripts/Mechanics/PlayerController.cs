using Assets.Scripts;
using Platformer.Mechanics.Stats;
using Platformer.Model;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static Assets.Scripts.Mechanics.MechanicEnums;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    public class PlayerController : KinematicObject
    {

        public int moveDirection;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        [SerializeField]
        public PlayerActionController playerActions;
        public Collider2D collider2d { get; set; }
        public readonly PlatformerModel model = GetModel<PlatformerModel>();
        public AudioSource audioSource;
        public Health health;
        public Stamina stamina;
        public bool controlEnabled = true;
        [SerializeField]
        public InputActionAsset controls;
        [SerializeField]
        public float staminaRegenTimer = 0f;

        public InputActionAsset Controls
        {
            get { return controls; }
            set { controls = value; }
        }

        Vector2 move;
        public SpriteRenderer spriteRenderer { get; set; }
        internal Animator animator;
        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }




        public void OnCollisionEnter2D(Collision2D collision)
        {
            //Debug.Log("Collision detected with name: " + collision.collider.name + "\n Collision detected with tag: " + collision.collider.tag);
            if (collision.collider.name == "Enemy" && playerActions.slideState == ActionState.Acting)
            {
                //Debug.Log("Detected Enemy in front of player");
                CapsuleCollider2D enemyCollider = collision.collider.GetComponent<CapsuleCollider2D>();
                enemyCollider.isTrigger = true;
                StartCoroutine(ResetColliderAfterSlide(enemyCollider));
            }
        }
        IEnumerator ResetColliderAfterSlide(CapsuleCollider2D collider)
        {
            yield return new WaitForSeconds(Constants.SlideDuration);
            collider.isTrigger = false;
        }

        protected override void Update()
        {
            HandleInput();
            playerActions.UpdateJumpState(IsGrounded);
            playerActions.UpdateGrabState(velocity);
            playerActions.UpdateSprintState(move, velocity);
            playerActions.UpdateKnockbackstate();
            playerActions.UpdateSlideState();
            RegenerateStamina();
            base.Update();
        }

        private void RegenerateStamina()
        {
            staminaRegenTimer += Time.deltaTime;
            if (staminaRegenTimer >= Constants.StaminaRegenRate)
            {
                stamina.Increment(1); // Increment stamina by 1 (or your desired amount)
                staminaRegenTimer = 0f; // Reset the timer
            }
        }

        private void HandleInput()
        {
            if (controlEnabled)
            {
                playerActions.HandleWallGrab(velocity);
            }
        }

        public void UpdateMoveDirection(int direction)
        {
            moveDirection = direction;
        }

        public void StopSlide()
        {
            // Reset velocity or modify movement logic to stop sliding
            velocity = Vector2.zero; // Or appropriate logic to stop sliding
        }


        protected override void ComputeVelocity()
        {
            if (playerActions.jump && IsGrounded)
            {
                velocity.y = Constants.JumpTakeOffSpeed * model.jumpModifier;
            }
            else if (playerActions.stopJump)
            {

                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (moveDirection != 0)
            {
                spriteRenderer.flipX = moveDirection < 0;
                move.x = moveDirection;
            }
            else
            {
                move.x = 0; // Stop movement when moveDirection is zero
            }

            if (playerActions.grabState == ActionState.Acting)
            {
                ApplyGravity(Constants.PercentageOfGravityWhileGrabbing);
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