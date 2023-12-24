using UnityEngine;
using static Assets.Scripts.Mechanics.MechanicEnums;

public class JumpSpeedPowerUpManager : BasePowerUpManager
{
    private float OriginalJumpTakeOffSpeed;

    protected override void Start()
    {
        base.Start();
        OriginalJumpTakeOffSpeed = Player.JumpTakeOffSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {

            Buff newBuff;
            if (buffType == BuffType.Permanent)
            {
                newBuff = new Buff(BuffType.Permanent, 0, Magnitude); // Permanent buff
                ApplyBuff(newBuff);
            }
            else
            {
                newBuff = new Buff(BuffType.Temporary, PowerUpDuration, Magnitude); // Temporary buff
                ApplyBuff(newBuff);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player.FacingCollider = null;
        }
    }
    protected override void ApplyPermanentBuffEffect(Buff buff)
    {
        Player.JumpTakeOffSpeed += buff.Magnitude * OriginalJumpTakeOffSpeed;
    }

    protected override void ApplyTemporaryBuffEffect(Buff buff)
    {
        Player.JumpTakeOffSpeed += buff.Magnitude * OriginalJumpTakeOffSpeed;
    }

    protected override void RemoveBuffEffect(Buff buff)
    {
        Player.JumpTakeOffSpeed = OriginalJumpTakeOffSpeed;
    }
}
