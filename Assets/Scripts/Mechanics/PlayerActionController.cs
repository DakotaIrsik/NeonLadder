using Assets.Scripts;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;
using static Assets.Scripts.Mechanics.MechanicEnums;
using static Platformer.Core.Simulation;

public class PlayerActionController : MonoBehaviour
{
    public PlayerController player;

    public PlayerActionController(PlayerController playerController)
    {
        player = playerController;
    }

    #region Jumping
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
    public float sprintDuration = Constants.SprintDuration; // seconds
    public ActionState sprintState = ActionState.Ready;
    public bool IsSprinting => sprintState == ActionState.Preparing || sprintState == ActionState.Acting || sprintState == ActionState.InAction || !stopSprint;
    public bool stopSprint;
    #endregion

    #region Rolling
    [SerializeField]
    public float rollSpeed = Constants.DefaultMaxSpeed * Constants.RollSpeedMultiplier;
    public float rollDuration = Constants.RollDuration; // seconds
    public bool IsRollCancellable = true;
    public ActionState rollState = ActionState.Ready;
    public bool IsRolling => rollState == ActionState.Preparing || rollState == ActionState.Acting || rollState == ActionState.InAction || !stopRoll;
    public bool stopRoll;
    #endregion

    public void UpdateRollState(Vector2 move, SpriteRenderer sprite)
    {
        switch (rollState)
        {
            case ActionState.Preparing:
                rollState = ActionState.Acting;
                stopRoll = false;
                Constants.MaxSpeed = Constants.DefaultMaxSpeed * Constants.RollSpeedMultiplier;
                rollDuration = Constants.RollDuration;
                break;

            case ActionState.Acting:
                if ((IsRollCancellable && stopRoll) || rollDuration <= 0)
                {
                    rollState = ActionState.Acted;
                }
                else
                {
                    move.x = (sprite.flipX ? -1 : 1) * rollSpeed;
                    //move.x = rollSpeed * (player.IsFacingLeft ? -1 : 1);

                }
                break;

            case ActionState.Acted:
                rollState = ActionState.Ready;
                stopRoll = false;
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
                sprintState = ActionState.Acting;
                stopSprint = false;
                sprintDuration = Constants.SprintDuration; // Reset the sprint duration
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
}
