using static Assets.Scripts.Mechanics.MechanicEnums;

public class Buff
{
    public BuffType Type;
    public float Duration;
    public float Magnitude;

    // Constructor
    public Buff(BuffType type, float duration, float magnitude)
    {
        Type = type;
        Duration = duration;
        Magnitude = magnitude;
    }
}
