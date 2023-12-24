namespace Assets.Scripts.Mechanics
{
    public static class MechanicEnums
    {
        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }

        public enum SprintState
        {
            Grounded,
            PrepareToSprint,
            Sprinting,
            MidSprint,
            Sprinted
        }

        public enum RollState
        {
            Grounded,
            PrepareToRoll,
            Rolling,
            MidRoll,
            Rolled
        }

        public enum PickupState
        {
            Ready,
            PickingUp,
            Completed
        }
        public enum GrabState
        {
            Unable,
            Ready,
            Grabbing,
            Holding,
            ReleasingHold
        }

        public enum FireState
        {
            Ready,
            Firing,
            SecondaryFiring,
            Reloading
        }

        public enum BuffType
        {
            Temporary,
            Permanent
        }

    }
}
