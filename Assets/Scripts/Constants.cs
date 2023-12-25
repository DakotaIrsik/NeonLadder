namespace Assets.Scripts
{
    public static class Constants
    {
        #region world
        public const float DefaultGravity = 9.81f;
        public static float DefaultRayLength = 0.5f;
        public static float DefaultRayWidth = 0.05f;
        #endregion

        #region player
        public static float DefaultMaxSpeed = 4f;
        public static float MaxSpeed = 4f;
        public static float DefaultJumpTakeOffSpeed = 10f;
        public static float JumpTakeOffSpeed = 10f;
        #endregion

        #region actions
        public static float JumpDuration = 0.5f;
        public static float JumpCooldown = 0.5f;
        public static float WallJumpForce = 5.0f;

        public static float SprintDuration = 1f;
        public static float SprintCooldown = 1f;
        public static float SprintSpeedMultiplier = 1.5f;

        public static float RollDuration = 0.5f;
        public static float RollCooldown = 1f;
        public static float RollSpeedMultiplier = 2f;

        public static float GrabDuration = 0.5f;
        public static float GrabCooldown = 1f;
        public static float GrabSpeedMultiplier = 1f;
        public static float PercentageOfGravityWhileGrabbing = 0.5f; // example default value, adjust as needed
        #endregion
    }
}
