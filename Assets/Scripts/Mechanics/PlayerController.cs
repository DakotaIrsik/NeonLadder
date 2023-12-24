using Platformer.Gameplay;
using Platformer.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public float PercentageOfGravityWhileGrabbing { get; set; } = 0.95f; // example default value, adjust as needed
        public const float DefaultGravity = 9.81f; // example default value, adjust as needed
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public float maxSpeed = 7;
        public float JumpTakeOffSpeed { get; set; } = 7;
        public JumpState jumpState { get; set; } = JumpState.Grounded;
        public bool IsJumping => jumpState == JumpState.PrepareToJump || jumpState == JumpState.Jumping || jumpState == JumpState.InFlight;
        public GrabState grabState = GrabState.Ready;
        public bool IsGrabbing => grabState == GrabState.Grabbing || grabState == GrabState.Holding;
        private bool stopJump;
        public Collider2D collider2d { get; set; }
        readonly PlatformerModel model = GetModel<PlatformerModel>();
        public Collider2D FacingCollider { get; set; }
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
        bool jump;
        bool grab;
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
            jumpAction.canceled += OnJumpCanceled;

            PrintUnusedAssets();
        }

        private void PrintUnusedAssets()
        {
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            HashSet<string> usedAssets = new HashSet<string>();
            var sb = new StringBuilder();
            foreach (string asset in allAssets)
            {
                if (!usedAssets.Contains(asset) && !asset.Contains("Package") && !Directory.Exists(asset) && !asset.Contains("Manager") && !asset.Contains("Platformer")
                && !asset.Contains("Scripts/Mechanics") && !asset.Contains("Tiles") && !asset.Contains("Billboards")
                && !asset.Contains("Scripts/Gameplay") && !asset.Contains("Settings") && !asset.Contains("TextMesh Pro") && !asset.Contains("Assets/Prefabs") && !asset.Contains("Icons")
                && !asset.Contains("Environment/Sprites") && !asset.Contains("Character/") && !asset.Contains("Audio") && !asset.Contains("Scripts/View") && !asset.Contains("Rendering")
                && !asset.Contains("Input") && !asset.Contains("Scenes/") && !asset.Contains("Fonts/") && !asset.Contains("UI/") && !asset.Contains("Editor/") && !asset.Contains("Scripts/Core")
                && !asset.Contains("HUD") && !asset.Contains("Controlers/") && !asset.Contains("Library/") && !asset.Contains("Controls"))
                {
                    sb.Append(asset + "\r\n");
                }
            }
            File.WriteAllText(@".\unused_assets.txt", sb.ToString());
        }

        private string FormatDeviceName(string deviceName)
        {
            switch (deviceName)
            {
                case "Keyboard":
                    return "Keyboard";
                case "XInputController":
                    return "Xbox"; // maybe also steam?
                case "SwitchProControllerHID":
                    return "Nintendo Switch";
                case "DualShockGamepad":// Add more cases as needed for other devices
                    return "Playstation";
                default:
                    return deviceName; // Return the original name if not recognized
            }
        }


        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            //anything fancy?
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
            base.Update();
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
            if (grabState == GrabState.Holding)
            {
                // Example: Stop vertical movement
                velocity.y = 0;
            }
            else
            {

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
                else
                {
                    // Apply normal gravity
                    ApplyGravity();
                }
                GroundedAnimation();

                move.x = Mathf.Clamp(move.x, -maxSpeed, maxSpeed);
                ResetVelocity();
            }
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