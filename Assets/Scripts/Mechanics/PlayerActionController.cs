using Assets.Scripts;
using Platformer.Gameplay;
using Platformer.Mechanics.Stats;
using Platformer.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static Assets.Scripts.Mechanics.MechanicEnums;
using static Platformer.Core.Simulation;
namespace Platformer.Mechanics
{

    public class PlayerActionController : MonoBehaviour
    {
        public MetaGameController meta;
        public GameObject bulletPrefab;
        private string bindingOutput = string.Empty;
        public PlayerController player;
        public LineRenderer aim;
        public AudioClip jumpAudio;
        public MeleeAttack meleePrefab;
        private Vector2 playerInput;
        private float sprintTimeAccumulator = 0f;
        private Vector3 currentAimDirection;
        private bool isAiming = false;
        private float aimDistance = 5f;
        private int currentBulletsOnScreen = 0;

        private void Start()
        {
            if (aim != null)
            {
                aim.positionCount = 2; // Line has two points
                aim.SetPosition(0, player.transform.position); // Starting point at player's position
                aim.SetPosition(1, player.transform.position); // End point also at player's position initially
                                                               // Set color and width as desired
            }   aim.startColor = Color.red;
            aim.endColor = Color.red;
            aim.startWidth = .05f;

            meleeAttackInstance = Instantiate(meleePrefab);
            meleeAttackInstance.gameObject.SetActive(false); // Start with the attack deactivated.


            PrintDebugControlConfiguration();
            ConfigureControls();
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
            jumpAction.canceled += OnJumpCanceled;

            var sprintAction = playerActionMap.FindAction("Sprint");
            sprintAction.performed += OnSprintPerformed;
            sprintAction.canceled += OnSprintCanceled;

            var slideAction = playerActionMap.FindAction("Slide");
            slideAction.performed += OnSlidePerformed;

            var moveLeftAction = playerActionMap.FindAction("MoveLeft");
            moveLeftAction.performed += OnMoveLeftPerformed;
            moveLeftAction.canceled += OnMoveCanceled;

            var moveRightAction = playerActionMap.FindAction("MoveRight");
            moveRightAction.performed += OnMoveRightPerformed;
            moveRightAction.canceled += OnMoveCanceled;

            var moveUpAction = playerActionMap.FindAction("MoveUp");
            moveUpAction.performed += OnJumpPerformed;
            moveUpAction.canceled += OnJumpCanceled;

            var crouchAction = playerActionMap.FindAction("Crouch");
            crouchAction.performed += OnCrouchPerformed;
            crouchAction.canceled += OnCrouchCanceled;


            var menuAction = playerActionMap.FindAction("Menu");
            menuAction.performed += OnMenuPerformed;

            var aimAction = playerActionMap.FindAction("Aim");
            aimAction.performed += OnAimPerformed;


            var fireAction = playerActionMap.FindAction("Fire");
            fireAction.performed += OnFireActionPerformed;

            var meleeAction = playerActionMap.FindAction("MeleeAttack");
            meleeAction.performed += OnMeleeActionPerformed;

        }


        void OnDrawGizmos()
        {
            if (meleeAttackInstance != null && meleeAttackInstance.gameObject.activeInHierarchy)
            {
                PolygonCollider2D polygon = meleeAttackInstance.GetComponentInChildren<PolygonCollider2D>();
                if (polygon != null)
                {
                    Gizmos.color = Color.green;
                    Transform transform = polygon.transform;

                    if (polygon.points.Length > 0)
                    {
                        Vector2 firstPoint = transform.TransformPoint(polygon.points[0]);
                        Vector2 previousPoint = firstPoint;

                        for (int i = 1; i < polygon.points.Length; i++)
                        {
                            Vector2 currentPoint = transform.TransformPoint(polygon.points[i]);
                            Gizmos.DrawLine(previousPoint, currentPoint);
                            previousPoint = currentPoint;
                        }

                        Gizmos.DrawLine(previousPoint, firstPoint);
                    }
                }
            }
        }




        private MeleeAttack meleeAttackInstance;

        private float distanceFromPlayer = 1f;

        private void OnMeleeActionPerformed(InputAction.CallbackContext context)
        {
            if (isMeleeAttacking) return;

            isMeleeAttacking = true;
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Vector2 playerBounds = playerCollider.bounds.extents;
            Vector2 playerPosition = playerCollider.bounds.center;

            float horizontalOffset = (player.spriteRenderer.flipX ? -1 : 1) * (playerBounds.x + distanceFromPlayer);

            Vector2 startPosition = new Vector2(
                playerPosition.x + horizontalOffset,
                playerPosition.y + playerBounds.y / 2
            );

            meleeAttackInstance.transform.position = startPosition;
            meleeAttackInstance.transform.localScale = new Vector3(
                Constants.MeleeAttackScaleX * (player.spriteRenderer.flipX ? 1 : -1),
                Constants.MeleeAttackScaleY,
                Constants.MeleeAttackScaleZ
            );

            meleeAttackInstance.gameObject.SetActive(true);

            // Get the collider from the child "Sprite" GameObject
            PolygonCollider2D childCollider = meleeAttackInstance.GetComponentInChildren<PolygonCollider2D>();
            if (childCollider != null)
            {
                childCollider.enabled = true;
            }
            else
            {
                Debug.LogError("No PolygonCollider2D found on the child 'Sprite' GameObject of MeleeAttackInstance.");
            }

            StartCoroutine(DisableAfterDelay(meleeAttackInstance, Constants.MeleeAttackDuration));
        }


        private IEnumerator DisableAfterDelay(MeleeAttack attackObject, float delay)
        {
            yield return new WaitForSeconds(delay);

            // Deactivate the parent GameObject to ensure the entire attack is disabled
            attackObject.gameObject.SetActive(false);

            // Reset the attack flag
            isMeleeAttacking = false;
        }


        private void OnFireActionPerformed(InputAction.CallbackContext context)
        {
            if (player.stamina.IsExhausted || currentBulletsOnScreen >= Constants.RangedAttackMaxOnScreen) return;
            player.stamina.Decrement(Constants.RangedAttackStaminaCost); // Decrement stamina
            GameObject bulletInstance = Instantiate(bulletPrefab, player.transform.position, Quaternion.identity);

            // Increment the bullet count
            currentBulletsOnScreen++;

            // Automatically destroy the bullet after its lifetime and decrement the bullet count
            Destroy(bulletInstance, Constants.RangedAttackLifeTime);
            StartCoroutine(DecrementBulletCountAfterDelay(Constants.RangedAttackLifeTime));

            Rigidbody2D bulletRigidbody = bulletInstance.GetComponent<Rigidbody2D>();
            if (bulletRigidbody != null)
            {
                Vector2 fireDirection = currentAimDirection.normalized;
                bulletRigidbody.AddForce(fireDirection * Constants.RangedAttackSpeed, ForceMode2D.Impulse);
            }
        }

        private IEnumerator DecrementBulletCountAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            currentBulletsOnScreen--;
        }


        private void OnAimPerformed(InputAction.CallbackContext context)
        {
            Vector2 aimDirection = context.ReadValue<Vector2>();
            if(context.control.device is Mouse)
            {
                AimWithMouse();
            }
            else
            {
                AimWithJoystick(aimDirection);
            }
        }

        private void AimWithJoystick(Vector2 joystickDirection)
        {
            currentAimDirection = new Vector3(joystickDirection.x, joystickDirection.y, 0).normalized;
            isAiming = true;
        }

        private void AimWithMouse()
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorldPosition.z = 0; // Assuming 2D game
            Vector3 aimDirection = mouseWorldPosition - transform.position;
            currentAimDirection = aimDirection.normalized;
            isAiming = true;
        }

        private void OnMenuPerformed(InputAction.CallbackContext context)
        {
            //meta.ToggleMainMenu(!meta.mainMenu.isActiveAndEnabled);
        }

        private void PrintDebugControlConfiguration()
        {
            if (bindingOutput == string.Empty)
            {
                var actionBindings = new Dictionary<string, List<string>>();

                foreach (var action in player.Controls.FindActionMap("Player").actions)
                {
                    foreach (var binding in action.bindings)
                    {
                        string deviceName = FormatDeviceName(binding.path.Split('/')[0].Replace("<", "").Replace(">", "")); // Extract and format device name
                        string controlName = binding.path.Split('/').Last();

                        string formattedBinding = $"{controlName} ({deviceName})";

                        if (!actionBindings.ContainsKey(action.name))
                        {
                            actionBindings[action.name] = new List<string>();
                        }
                        actionBindings[action.name].Add(formattedBinding);
                    }
                }
                var fullBindings = new List<string>();
                foreach (var actionBinding in actionBindings)
                {
                    string actionName = actionBinding.Key;
                    string bindings = string.Join(", ", actionBinding.Value);
                    fullBindings.Add($"{actionName} = {bindings}");
                }

                bindingOutput = string.Join("\n", fullBindings);
                Debug.Log(bindingOutput);
            }
        }


        #region Jumping
        [SerializeField]
        public ActionState jumpState = ActionState.Ready;
        [SerializeField]
        public bool jump;
        public bool IsJumping => jumpState == ActionState.Preparing || jumpState == ActionState.Acting || jumpState == ActionState.InAction || !stopJump;
        [SerializeField]
        public bool stopJump;

        #endregion

        #region Grabbing
        [SerializeField]
        public ActionState grabState = ActionState.Ready;
        public bool IsGrabbing => grabState == ActionState.Acting || grabState == ActionState.InAction || !stopGrab;
        [SerializeField]
        public bool stopGrab;
        #endregion

        #region Sprinting
        [SerializeField]
        public float sprintSpeed = Constants.DefaultMaxSpeed * Constants.SprintSpeedMultiplier;
        [SerializeField]
        public float sprintDuration = Constants.SprintDuration; // seconds
        [SerializeField]
        public ActionState sprintState = ActionState.Ready;
        public bool IsSprinting => sprintState == ActionState.Preparing || sprintState == ActionState.Acting || sprintState == ActionState.InAction || !stopSprint;
        [SerializeField]
        public bool stopSprint;
        #endregion

        #region Slideing
        [SerializeField]
        public float slideSpeed = Constants.DefaultMaxSpeed * Constants.SlideSpeedMultiplier;
        [SerializeField]
        public float slideDuration = Constants.SlideDuration; // seconds
        [SerializeField]
        public ActionState slideState = ActionState.Ready;
        public bool IsSliding => slideState == ActionState.Preparing || slideState == ActionState.Acting || slideState == ActionState.InAction || !stopSlide;
        [SerializeField]
        public bool stopSlide;
        #endregion


        #region Knockback
        [SerializeField]
        public float knockbackDuration = Constants.DefaultKnockbackDuration;
        [SerializeField]
        public float knockbackSpeed = Constants.DefaultKnockbackSpeed;
        [SerializeField]
        public bool knockback;


        [SerializeField]
        public ActionState knockbackState = ActionState.Ready;
        public bool IsKnockedBack => knockbackState == ActionState.Preparing || sprintState == ActionState.Acting || sprintState == ActionState.InAction || knockback;
        #endregion

        #region Melee
        private bool isMeleeAttacking = false;
        #endregion


        private void Update()
        {
            if (isAiming) // You need a variable to track whether the player is currently aiming
            {
                UpdateAimLine();
            }
        }

        private void UpdateAimLine()
        {
            if (aim != null)
            {
                aim.SetPosition(0, player.transform.position); // Always start at player's position

                Vector3 aimEndPoint = player.transform.position + currentAimDirection * aimDistance; // Calculate the end point
                aim.SetPosition(1, aimEndPoint); // Set the end point of the line
            }
        }


        public void UpdateSlideState()
        {
            var playerRigidbody = player.GetComponent<Rigidbody2D>();
            var atAWall = player.collider2d.IsTouchingLayers(LayerMask.GetMask("Walls"));
            switch (slideState)
            {
                case ActionState.Preparing:
                    Constants.MaxSpeed = Constants.DefaultMaxSpeed * Constants.SlideSpeedMultiplier;
                    slideDuration = Constants.SlideDuration; // Reset the sprint duration
                    slideState = ActionState.Acting;
                    //animation etc.
                    break;
                case ActionState.Acting:
                    if (stopSlide || slideDuration <= 0 || atAWall)
                    {
                        // Stop sliding

                        stopSlide = true;
                        slideState = ActionState.Acted;
                        //move players x position away from the wall slightly.

                        player.transform.position = new Vector2(player.transform.position.x + (player.spriteRenderer.flipX ? 1 : -1) * 0.25f, player.transform.position.y);

                    }
                    else if (playerInput != Vector2.zero)
                    {
                        Vector3 slideDirection = new Vector3(playerInput.x, 0, playerInput.y);
                        playerRigidbody.velocity = slideDirection * slideSpeed;
                        slideDuration -= Time.deltaTime;
                    }
                    else
                    {
                        slideDuration -= Time.deltaTime;
                    }
                    //animation etc.
                    break;
                case ActionState.Acted:
                    slideState = ActionState.Ready;
                    stopSlide = false;
                    Constants.MaxSpeed = Constants.DefaultMaxSpeed; // Reset world speed
                    if (atAWall)
                    {
                        // If at a wall, reset only horizontal velocity
                        playerRigidbody.velocity = new Vector2(0, playerRigidbody.velocity.y);
                    }
                    else
                    {
                        // If not at a wall, reset all velocity
                        playerRigidbody.velocity = Vector2.zero;
                    }
                    break;
            }
        }

        public void UpdateSprintState(Vector2 move, Vector2 velocity)
        {
            float staminaCostPerTenthSecond = Constants.SprintStaminaCost * 0.1f;
            switch (sprintState)
            {
                case ActionState.Preparing:
                    Constants.MaxSpeed = Constants.DefaultMaxSpeed * Constants.SprintSpeedMultiplier;
                    sprintDuration = Constants.SprintDuration; // Reset the sprint duration
                    sprintState = ActionState.Acting;
                    stopSprint = false;
                    sprintTimeAccumulator = 0f; // Reset the time accumulator
                    break;

                case ActionState.Acting:
                    if (stopSprint || sprintDuration <= 0)
                    {
                        sprintState = ActionState.Acted;
                    }
                    else
                    {
                        sprintTimeAccumulator += Time.deltaTime;
                        if (sprintTimeAccumulator >= 0.1f)
                        {
                            player.stamina.Decrement(staminaCostPerTenthSecond); // Decrement stamina
                            sprintTimeAccumulator -= 0.1f; // Subtract 0.1 seconds from the accumulator
                        }

                        if (Mathf.Abs(move.x) > 0)
                        {
                            velocity.x = Mathf.Sign(move.x) * Constants.MaxSpeed;
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

        public void UpdateKnockbackstate()
        {
            if (knockback)
            {
                switch (knockbackState)
                {
                    case ActionState.Ready:
                        knockbackState = ActionState.Preparing;
                        break;
                    case ActionState.Preparing:
                        knockbackDuration = Constants.DefaultKnockbackDuration;
                        knockbackState = ActionState.Acting;
                        break;
                    case ActionState.Acting:
                        if (knockbackDuration <= 0 || player.health.current == 0)
                        {
                            knockbackState = ActionState.Acted;
                        }
                        Vector2 moveAmount = (player.spriteRenderer.flipX ? Vector2.right : Vector2.left) * Constants.DefaultKnockbackSpeed * Time.deltaTime;
                        player.transform.position += (Vector3)moveAmount;
                        knockbackDuration -= Time.deltaTime;
                        break;
                    case ActionState.InAction:
                        knockbackState = ActionState.Acted;
                        break;
                    case ActionState.Acted:
                        knockbackState = ActionState.Ready;
                        knockback = false;
                        break;
                }
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

        private void OnCrouchCanceled(InputAction.CallbackContext context)
        {
            player.transform.localScale = new Vector3(Constants.DefaultCharacterScale, Constants.DefaultCharacterScale, 1);
        }

        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            if (player.stamina.IsExhausted) return;
            player.stamina.Decrement(Constants.CrouchStaminaCost);
            player.transform.localScale = new Vector3(Constants.CrouchScale, Constants.CrouchScale, 1);
        }

        private void OnGrabPerformed(InputAction.CallbackContext context)
        {
            if (player.stamina.IsExhausted) return;
            player.stamina.Decrement(Constants.GrabStaminaCost);    
            //if player is facing the wall
            if (player.collider2d.IsTouchingLayers(LayerMask.GetMask("Walls")))
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
            if (!player.stamina.IsExhausted)
            {
                if (sprintState == ActionState.Ready)
                {
                    sprintState = ActionState.Preparing;
                    stopSprint = false;
                }
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
            if (player.stamina.IsExhausted) return;
            player.stamina.Decrement(Constants.JumpStaminaCost);
            if (jumpState == ActionState.Ready || (grabState == ActionState.Acting && player.collider2d.IsTouchingLayers(LayerMask.GetMask("Level"))))
            {
                jumpState = ActionState.Preparing;

                if (grabState == ActionState.Acting)
                {
                    player.velocity.x = Constants.WallJumpForce * (player.spriteRenderer.flipX ? 1 : -1);
                    grabState = ActionState.Ready;
                }
                player.velocity.y = Constants.JumpTakeOffSpeed * player.model.jumpModifier;
            }

            stopJump = true;
            Schedule<PlayerStopJump>().player = player;
        }

        private void OnSlidePerformed(InputAction.CallbackContext context)
        {
            if (player.stamina.IsExhausted) return;
            player.stamina.Decrement(Constants.SlideStaminaCost);
            if (slideState == ActionState.Ready)
            {
                slideState = ActionState.Preparing;
                stopSlide = false;
            }
        }

        private void OnMoveRightPerformed(InputAction.CallbackContext context)
        {
            player.moveDirection = 1;
            playerInput = new Vector2(1, 0); // Assuming right movement is along the positive x-axis
        }

        private void OnMoveLeftPerformed(InputAction.CallbackContext context)
        {
            player.moveDirection = -1;
            playerInput = new Vector2(-1, 0); // Assuming left movement is along the negative x-axis
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            player.UpdateMoveDirection(0);
            playerInput = Vector2.zero; // No movement input
        }


        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            if (IsJumping)
            {
                player.velocity.y *= Constants.JumpCutOffFactor;
                stopJump = true;
            }
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
    }
}
