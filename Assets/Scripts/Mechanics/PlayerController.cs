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
        public float WallJumpForce = 5.0f;
        public float maxSpeed = 4f;
        public float PercentageOfGravityWhileGrabbing = 0.95f; // example default value, adjust as needed
        public const float DefaultGravity = 9.81f; // example default value, adjust as needed
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;


        #region Jumping
        public float JumpTakeOffSpeed = 7;
        public JumpState jumpState = JumpState.Grounded;
        public bool IsJumping => jumpState == JumpState.PrepareToJump || jumpState == JumpState.Jumping || jumpState == JumpState.InFlight || !stopJump;
        private bool stopJump;
        bool jump;
        #endregion

        #region Grabbing
        [SerializeField]
        public GrabState grabState = GrabState.Unable;
        public bool IsGrabbing => grabState == GrabState.Grabbing || grabState == GrabState.Holding || !stopGrab;
        private bool stopGrab;
        #endregion

        #region Sprinting
        public float sprintSpeed = 0.8f;
        public float sprintDuration = 0.75f; // seconds
        public SprintState sprintState = SprintState.Grounded;
        public bool IsSprinting => sprintState == SprintState.PrepareToSprint || sprintState == SprintState.Sprinting || sprintState == SprintState.MidSprint || !stopSprint;
        private bool stopSprint;
        #endregion

        #region Rolling
        public float rollSpeed = .8f;
        public float rollDuration = 0.25f; // seconds
        public bool IsRollCancellable = true;
        public RollState rollState = RollState.Grounded;
        public bool IsRolling => rollState == RollState.PrepareToRoll || rollState == RollState.Rolling || rollState == RollState.MidRoll || !stopRoll;
        private bool stopRoll;
        #endregion


        public Collider2D collider2d { get; set; }
        readonly PlatformerModel model = GetModel<PlatformerModel>();
        public Collider2D FacingCollider { get; set; } //test
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;
        [SerializeField]
        private InputActionAsset playerControlsAsset;

        // You can also have a non-serialized public property if needed
        public InputActionAsset PlayerControlsAsset
        {
            get { return playerControlsAsset; }
            set { playerControlsAsset = value; }
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
                foreach (var action in PlayerControlsAsset.FindActionMap("Player").actions)
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

            var playerActionMap = playerControlsAsset.FindActionMap("Player");
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
            if (rollState == RollState.Grounded)
            {
                gameObject.layer = LayerMask.NameToLayer("Rolling Layer");
                rollState = RollState.PrepareToRoll;
                stopRoll = false;
            }
        }

        private void OnRollCanceled(InputAction.CallbackContext context)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            if (IsRolling)
            {
                stopRoll = true;
            }
        }


        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            if (sprintState == SprintState.Grounded)
            {
                sprintState = SprintState.PrepareToSprint;
                stopSprint = false;
                // Additional logic for starting the sprint, such as playing an animation
            }
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            if (IsSprinting)
            {
                stopSprint = true;
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
            if (jumpState == JumpState.Grounded || (grabState == GrabState.Grabbing && FacingCollider != null))
            {
                jumpState = JumpState.PrepareToJump;

                if (grabState == GrabState.Grabbing)
                {
                    velocity.x = WallJumpForce * (spriteRenderer.flipX ? 1 : -1);
                    grabState = GrabState.Unable;
                }
                velocity.y = JumpTakeOffSpeed * model.jumpModifier;
            }
        }


        private void OnGrabPerformed(InputAction.CallbackContext context)
        {
            if (FacingCollider != null)
            {
                grabState = GrabState.Grabbing;
                velocity = -DefaultGravity * PercentageOfGravityWhileGrabbing * Time.deltaTime * -Vector2.up;

            }
        }

        private void OnGrabCanceled(InputAction.CallbackContext context)
        {
            if (grabState == GrabState.Grabbing || grabState == GrabState.Holding)
            {
                grabState = GrabState.Unable;
            }
        }

        protected override void Update()
        {
            HandleInput();
            UpdateJumpState();
            UpdateGrabState();
            UpdateSprintState();
            UpdateRollState();
            base.Update();
        }


        private void UpdateRollState()
        {
            switch (rollState)
            {
                case RollState.PrepareToRoll:
                    rollState = RollState.Rolling;
                    stopRoll = false;
                    maxSpeed = 10f;
                    rollDuration = 0.25f; 
                    break;

                case RollState.Rolling:
                    if ((IsRollCancellable && stopRoll) || rollDuration <= 0)
                    {
                        rollState = RollState.Rolled;
                    }
                    else
                    {
                        // Update position here for autorun
                        move.x = rollSpeed * (spriteRenderer.flipX ? -1 : 1);
                        rollDuration -= Time.deltaTime;
                    }
                    break;

                case RollState.Rolled:
                    rollState = RollState.Grounded;
                    stopRoll = false;
                    maxSpeed = 5f; // Reset to normal speed

                    break;
            }
        }


        private void HandleInput()
        {
            if (controlEnabled)
            {
                HorizontalMovement();
                HandleJump();
                HandleWallGrab();
            }
        }

        private void UpdateSprintState()
        {
            switch (sprintState)
            {
                case SprintState.PrepareToSprint:
                    maxSpeed = sprintSpeed;
                    sprintDuration = 0.5f; // Reset the sprint duration
                    sprintState = SprintState.Sprinting;
                    break;

                case SprintState.Sprinting:
                    if (stopSprint || sprintDuration <= 0)
                    {
                        sprintState = SprintState.Sprinted;
                    }
                    else if (Mathf.Abs(move.x) > 0) // Check if there's horizontal input
                    {
                        velocity.x = Mathf.Lerp(velocity.x, sprintSpeed * (spriteRenderer.flipX ? -1 : 1), Time.deltaTime * 5f);
                    }
                    sprintDuration -= Time.deltaTime;
                    break;
                case SprintState.Sprinted:
                    sprintState = SprintState.Grounded;
                    stopSprint = false; // Ensure stopSprint is reset
                    maxSpeed = 5f;
                    break;
            }
        }


        private void HorizontalMovement()
        {
            move.x = Input.GetAxis("Horizontal");
        }

        private void HandleJump()
        {
            if (Input.GetButtonUp("Jump"))
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }


        private void UpdateGrabState()
        {
            if (grabState == GrabState.Grabbing)
            {
                if (velocity.y < 0)
                {
                    jumpState = JumpState.Grounded;
                }
            }
        }

        private void HandleWallGrab()
        {
            if (grabState == GrabState.Grabbing)
            {
                if (Mathf.Abs(velocity.y) > 0.1f)
                {
                    velocity.y *= PercentageOfGravityWhileGrabbing;
                }
                else
                {
                    velocity.y = 0;
                }
            }
        }

        void UpdateJumpState()
        {

            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (sprintState == SprintState.Sprinting)
            {
                //help gpt
            }
            else
            {
                // Normal movement logic here
                if (jump && IsGrounded)
                {
                    velocity.y = JumpTakeOffSpeed * model.jumpModifier;
                    jump = false;
                }
                else if (stopJump)
                {
                    stopJump = false;
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

                if (grabState == GrabState.Grabbing)
                {
                    ApplyGravity(PercentageOfGravityWhileGrabbing);
                }
            }


            ApplyGravity();
            GroundedAnimation();
            move.x = Mathf.Clamp(move.x, -maxSpeed, maxSpeed);
            ResetVelocity();
        }

        public void ApplyGravity(float gravity = DefaultGravity)
        {
            if (!IsGrounded)
            {
                velocity.y += -gravity * Time.deltaTime;
            }
        }


        private void ResetVelocity()
        {
            targetVelocity = move * maxSpeed;
        }

        private void GroundedAnimation()
        {
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
        }
    }
}