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

        public int moveDirection;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        [SerializeField]
        public PlayerActionController playerActions;
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
                if (playerActions.slideState == ActionState.Acting)
                {
                    playerActions.stopSlide = true;
                }
            }
            else if (collision.collider.name == "Wall")
            {
                FacingCollider = collision.collider;
                if (playerActions.slideState == ActionState.Acting)
                {
                    playerActions.stopSlide = true;
                }   
            }
            else if (collision.collider.name == "Enemy" && playerActions.slideState == ActionState.Acting)
            {
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
            playerActions.UpdateSlideState(move, spriteRenderer);
            base.Update();
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

        private void GroundedAnimation()
        {
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / Constants.MaxSpeed);
        }
    }
}