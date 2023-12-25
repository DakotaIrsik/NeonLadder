using Assets.Scripts;
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
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        [SerializeField]
        public PlayerActionController playerActions;
        public bool IsFacingLeft => spriteRenderer.flipX;
        public bool IsFacingRight => !IsFacingLeft;
        public Collider2D collider2d { get; set; }
        public readonly PlatformerModel model = GetModel<PlatformerModel>();
        public Collider2D FacingCollider { get; set; } //test
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;
        [SerializeField]
        public InputActionAsset controls;

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
            if (collision.collider.name == "Level")
            {
                return;
            }
            else if (collision.collider.name == "Wall")
            {
                FacingCollider = collision.collider;
            }
            else if (collision.collider.name == "Enemy" && playerActions.slideState == ActionState.Acting)
            {
                CapsuleCollider2D enemyCollider = collision.collider.GetComponent<CapsuleCollider2D>();
                enemyCollider.isTrigger = true;
                StartCoroutine(ResetColliderAfterRoll(enemyCollider));
            }
        }

        IEnumerator ResetColliderAfterRoll(CapsuleCollider2D collider)
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
            playerActions.UpdateSlideState(move, spriteRenderer);
            base.Update();
        }

        private void HandleInput()
        {
            if (controlEnabled)
            {
                HorizontalMovement();
                playerActions.HandleJump();
                playerActions.HandleWallGrab(velocity);
            }
        }

        public void HorizontalMovement()
        {
            move.x = Input.GetAxis("Horizontal");
        }

        protected override void ComputeVelocity()
        {
            if (playerActions.jump && IsGrounded)
            {
                velocity.y = Constants.JumpTakeOffSpeed * model.jumpModifier;
                playerActions.jump = false;
            }
            else if (playerActions.stopJump)
            {
                playerActions.stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
            {
                spriteRenderer.flipX = false;
            }
            else if (move.x < -0.01f)
            {
                spriteRenderer.flipX = true;
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

        private void GroundedAnimation()
        {
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / Constants.MaxSpeed);
        }
    }
}