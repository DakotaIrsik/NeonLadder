using Assets.Scripts;
using Platformer.Gameplay;
using Platformer.Mechanics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static Assets.Scripts.Mechanics.MechanicEnums;
using static Platformer.Core.Simulation;
using static UnityEngine.InputSystem.DefaultInputActions;
namespace Platformer.Mechanics
{

    public class PlayerActionController : MonoBehaviour
    {
        private string bindingOutput = string.Empty;
        public PlayerController player;

        private void Start()
        {
            DebugControlConfiruation();
            ConfigureControls();
        }

        #region Jumping
        [SerializeField]
        public ActionState jumpState = ActionState.Ready;
        public bool IsJumping => jumpState == ActionState.Preparing || jumpState == ActionState.Acting || jumpState == ActionState.InAction || !stopJump;
        public bool stopJump;
        public bool jump;
        #endregion

        #region Grabbing
        [SerializeField]
        public ActionState grabState = ActionState.Ready;
        public bool IsGrabbing => grabState == ActionState.Acting || grabState == ActionState.InAction || !stopGrab;
        public bool stopGrab;
        #endregion

        #region Sprinting
        public float sprintSpeed = Constants.DefaultMaxSpeed * Constants.SprintSpeedMultiplier;
        [SerializeField]
        public float sprintDuration = Constants.SprintDuration; // seconds
        [SerializeField]
        public ActionState sprintState = ActionState.Ready;
        public bool IsSprinting => sprintState == ActionState.Preparing || sprintState == ActionState.Acting || sprintState == ActionState.InAction || !stopSprint;
        public bool stopSprint;
        #endregion

        #region Slideing
        public float slideSpeed = Constants.DefaultMaxSpeed * Constants.SlideSpeedMultiplier;
        [SerializeField]
        public float slideDuration = Constants.SlideDuration; // seconds
        public bool IsSlideCancellable = true;
        [SerializeField]
        public ActionState slideState = ActionState.Ready;
        public bool IsSliding => slideState == ActionState.Preparing || slideState == ActionState.Acting || slideState == ActionState.InAction || !stopSlide;
        public bool stopSlide;
        #endregion

        public void UpdateSlideState(Vector2 move, SpriteRenderer sprite)
        {
            switch (slideState)
            {
                case ActionState.Preparing:
                    Constants.MaxSpeed = Constants.DefaultMaxSpeed * Constants.SlideSpeedMultiplier;
                    slideDuration = Constants.SlideDuration;
                    slideState = ActionState.Acting;
                    stopSlide = false;
                    break;

                case ActionState.Acting:
                    if ((IsSlideCancellable && stopSlide) || slideDuration <= 0)
                    {
                        slideState = ActionState.Acted;
                    }
                    else
                    {
                        float slideDirection = sprite.flipX ? -1 : 1;
                        /*
                        animation for the non-cancellable slide
                        move.x = (sprite.flipX ? -1 : 1) * slideSpeed;
                        player.velocity.x = move.x;
                        */
                        player.transform.Translate(slideDirection * slideSpeed * Time.deltaTime, 0, 0);
                        slideDuration -= Time.deltaTime;
                    }
                    break;

                case ActionState.Acted:
                    slideState = ActionState.Ready;
                    stopSlide = false;
                    Constants.MaxSpeed = Constants.DefaultMaxSpeed;
                    break;
            }
        }

        public void UpdateSprintState(Vector2 move, Vector2 velocity)
        {
            switch (sprintState)
            {
                case ActionState.Preparing:
                    Constants.MaxSpeed = Constants.DefaultMaxSpeed * Constants.SprintSpeedMultiplier;
                    sprintDuration = Constants.SprintDuration; // Reset the sprint duration
                    sprintState = ActionState.Acting;
                    stopSprint = false;
                    break;

                case ActionState.Acting:
                    if (stopSprint || sprintDuration <= 0)
                    {
                        sprintState = ActionState.Acted;
                    }
                    else
                    {
                        // Apply sprint speed in the current movement direction
                        if (Mathf.Abs(move.x) > 0)
                        {
                            velocity.x = Mathf.Sign(move.x) * sprintSpeed;
                        }
                        sprintDuration -= Time.deltaTime;
                    }
                    break;

                case ActionState.Acted:
                    sprintState = ActionState.Ready;
                    stopSprint = false;
                    Constants.MaxSpeed = Constants.DefaultMaxSpeed; // Reset to normal speed
                    break;
            }
        }

        public void UpdateJumpState(bool IsGrounded)
        {
            jump = false;
            switch (jumpState)
            {
                case ActionState.Preparing:
                    jumpState = ActionState.Acting;
                    jump = true;
                    stopJump = false;
                    break;
                case ActionState.Acting:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = player;
                        jumpState = ActionState.InAction;
                    }
                    break;
                case ActionState.InAction:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = player;
                        jumpState = ActionState.Acted;
                    }
                    break;
                case ActionState.Acted:
                    jumpState = ActionState.Ready;
                    break;
            }
        }

        public void HandleJump()
        {
            if (Input.GetButtonUp("Jump"))
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = player;
            }
        }

        public void HandleWallGrab(Vector2 velocity)
        {
            if (grabState == ActionState.Acting)
            {
                if (Mathf.Abs(velocity.y) > 0.1f)
                {
                    velocity.y *= Constants.PercentageOfGravityWhileGrabbing;
                }
                else
                {
                    velocity.y = 0;
                }
            }
        }

        public void UpdateGrabState(Vector2 velocity)
        {
            if (grabState == ActionState.Acting)
            {
                if (velocity.y < 0)
                {
                    jumpState = ActionState.Ready;
                }
            }
        }

        private void OnGrabPerformed(InputAction.CallbackContext context)
        {
            if (player.FacingCollider != null)
            {
                grabState = ActionState.Acting;
                player.velocity = -Constants.DefaultGravity * Constants.PercentageOfGravityWhileGrabbing * Time.deltaTime * -Vector2.up;

            }
        }

        private void OnGrabCanceled(InputAction.CallbackContext context)
        {
            if (grabState == ActionState.Acting || grabState == ActionState.InAction)
            {
                grabState = ActionState.Ready;
            }
        }

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            if (sprintState == ActionState.Ready)
            {
                sprintState = ActionState.Preparing;
                stopSprint = false;
            }
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            if (IsSprinting)
            {
                stopSprint = true;
            }
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (jumpState == ActionState.Ready || (grabState == ActionState.Acting && player.FacingCollider != null))
            {
                jumpState = ActionState.Preparing;

                if (grabState == ActionState.Acting)
                {
                    player.velocity.x = Constants.WallJumpForce * (player.spriteRenderer.flipX ? 1 : -1);
                    grabState = ActionState.Ready;
                }
                player.velocity.y = Constants.JumpTakeOffSpeed * player.model.jumpModifier;
            }
        }

        private void OnSlidePerformed(InputAction.CallbackContext context)
        {
            if (slideState == ActionState.Ready)
            {
                slideState = ActionState.Preparing;
                stopSlide = false;
            }
        }

        private void OnSlideCanceled(InputAction.CallbackContext context)
        {
            if (IsSliding)
            {
                stopSlide = true;
            }
        }

        private void DebugControlConfiruation()
        {
            if (bindingOutput == string.Empty)
            {
                var fullBindings = new List<string>();
                foreach (var action in player.Controls.FindActionMap("Player").actions)
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
        }

        private void ConfigureControls()
        {
            var playerActionMap = player.controls.FindActionMap("Player");
            playerActionMap.Enable();

            var grabAction = playerActionMap.FindAction("Grab");
            grabAction.performed += OnGrabPerformed;
            grabAction.canceled += OnGrabCanceled;

            var jumpAction = playerActionMap.FindAction("Jump");
            jumpAction.performed += OnJumpPerformed;

            var sprintAction = playerActionMap.FindAction("Sprint");
            sprintAction.performed += OnSprintPerformed;
            sprintAction.canceled += OnSprintCanceled;

            var slideAction = playerActionMap.FindAction("Slide");
            slideAction.performed += OnSlidePerformed;
            slideAction.canceled += OnSlideCanceled;
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
    }
}
