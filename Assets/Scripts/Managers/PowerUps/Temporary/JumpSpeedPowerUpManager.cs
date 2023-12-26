using Assets.Scripts;
using UnityEngine;
using static Assets.Scripts.Mechanics.MechanicEnums;

public class JumpSpeedPowerUpManager : BasePowerUpManager
{

    protected override void Start()
    {
        base.Start();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject.name != "Enemy")
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

    protected override void ApplyTemporaryBuffEffect(Buff buff)
    {
        Constants.JumpTakeOffSpeed += buff.Magnitude * Constants.DefaultJumpTakeOffSpeed;
    }

    protected override void RemoveBuffEffect(Buff buff)
    {
        Constants.JumpTakeOffSpeed = Constants.DefaultJumpTakeOffSpeed;
    }
}
