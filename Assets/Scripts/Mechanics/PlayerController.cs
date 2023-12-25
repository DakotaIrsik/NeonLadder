using Assets.Scripts;
using Platformer.Gameplay;
using Platformer.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        readonly PlatformerModel model = GetModel<PlatformerModel>();
        public Collider2D FacingCollider { get; set; } //test
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;
        [SerializeField]
        public InputActionAsset controls;

        // You can also have a non-serialized public property if needed
        public InputActionAsset Controls
        {
            get { return controls; }
            set { controls = value; }
        }

        Vector2 move;
        public SpriteRenderer spriteRenderer { get; set; }
        internal Animator animator;
        public Bounds Bounds => collider2d.bounds;
        private string bindingOutput = string.Empty;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            if (bindingOutput == string.Empty)
            {
                var fullBindings = new List<string>();
                foreach (var action in Controls.FindActionMap("Player").actions)
                {
                    var bindings = action.bindings.Select(b =>
                    {
                        string deviceName = b.path.Split('/')[0].Replace("<", "").Replace(">", ""); // Extract device name
                        string controlName = b.path.Split('/').Last();
                        string formattedDeviceName = FormatDeviceName(deviceName); // Format the device name for readability
                        return $"{formattedDeviceName}: {controlName} \n";
                    });

                    fullBindings.Add($"Name: {action.name} -> \n{string.Join("", bindings)}");
                }
                bindingOutput = string.Join("\n", fullBindings);
                Debug.Log(string.Join("\n", fullBindings));
            }

            var playerActionMap = controls.FindActionMap("Player");
            playerActionMap.Enable();

            // Get the 'Grab' action and subscribe to its events.
            var grabAction = playerActionMap.FindAction("Grab");
            grabAction.performed += OnGrabPerformed;
            grabAction.canceled += OnGrabCanceled;

            var jumpAction = playerActionMap.FindAction("Jump");
            jumpAction.performed += OnJumpPerformed;

            var sprintAction = playerActionMap.FindAction("Sprint");
            sprintAction.performed += OnSprintPerformed;
            sprintAction.canceled += OnSprintCanceled;

            var rollAction = playerActionMap.FindAction("Roll");
            rollAction.performed += OnRollPerformed;
            rollAction.canceled += OnRollCanceled;
        }

        private void OnRollPerformed(InputAction.CallbackContext context)
        {
            if (playerActions.rollState == ActionState.Ready)
            {
                gameObject.layer = LayerMask.NameToLayer("Rolling Layer");
                playerActions.rollState = ActionState.Preparing;
                playerActions.stopRoll = false;
            }
        }

        private void OnRollCanceled(InputAction.CallbackContext context)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            if (playerActions.IsRolling)
            {
                playerActions.stopRoll = true;
            }
        }

        public void ResetMovement()
        {
            ApplyGravity();
            GroundedAnimation();
            move.x = Mathf.Clamp(move.x, -Constants.MaxSpeed, Constants.MaxSpeed);
            ResetVelocity();
        }


        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            if (playerActions.sprintState == ActionState.Ready)
            {
                playerActions.sprintState = ActionState.Preparing;
                playerActions.stopSprint = false;
                // Additional logic for starting the sprint, such as playing an animation
            }
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            if (playerActions.IsSprinting)
            {
                playerActions.stopSprint = true;
            }
        }

        private string FormatDeviceName(string deviceName)
        {
            switch (deviceName)
            {
                case "Keyboard":
                    return "Keyboard";
                case "XInputContsprinter":
                    return "Xbox"; // maybe also steam?
                case "SwitchProContsprinterHID":
                    return "Nintendo Switch";
                case "DualShockGamepad":// Add more cases as needed for other devices
                    return "Playstation";
                default:
                    return deviceName; // Return the original name if not recognized
            }
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (playerActions.jumpState == ActionState.Ready || (playerActions.grabState == ActionState.Acting && FacingCollider != null))
            {
                playerActions.jumpState = ActionState.Preparing;

                if (playerActions.grabState == ActionState.Acting)
                {
                    velocity.x = Constants.WallJumpForce * (spriteRenderer.flipX ? 1 : -1);
                    playerActions.grabState = ActionState.Ready;
                }
                velocity.y = Constants.JumpTakeOffSpeed * model.jumpModifier;
            }
        }


        private void OnGrabPerformed(InputAction.CallbackContext context)
        {
            if (FacingCollider != null)
            {
                playerActions.grabState = ActionState.Acting;
                velocity = -Constants.DefaultGravity * Constants.PercentageOfGravityWhileGrabbing * Time.deltaTime * -Vector2.up;

            }
        }

        private void OnGrabCanceled(InputAction.CallbackContext context)
        {
            if (playerActions.grabState == ActionState.Acting || playerActions.grabState == ActionState.InAction)
            {
                playerActions.grabState = ActionState.Ready;
            }
        }

        protected override void Update()
        {
            HandleInput();
            playerActions.UpdateJumpState(IsGrounded);
            playerActions.UpdateGrabState(velocity);
            playerActions.UpdateSprintState(move, velocity);
            playerActions.UpdateRollState(move, spriteRenderer);
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

        private void HorizontalMovement()
        {
            move.x = Input.GetAxis("Horizontal");
        }

        protected override void ComputeVelocity()
        {
            if (playerActions.sprintState == ActionState.Acting)
            {
                //help gpt
            }
            else
            {
                // Normal movement logic here
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
            }

            ResetMovement();
        }

        public void ApplyGravity(float gravity = Constants.DefaultGravity)
        {
            if (!IsGrounded)
            {
                velocity.y += -gravity * Time.deltaTime;
            }
        }

        private void ResetVelocity()
        {
            targetVelocity = move * Constants.MaxSpeed;
        }

        private void GroundedAnimation()
        {
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / Constants.MaxSpeed);
        }
    }
}