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
        public const float DefaultMaxSpeed = 4f;
        public static float MaxSpeed = 4f;
        public const float DefaultJumpTakeOffSpeed = 10f;
        public static float JumpTakeOffSpeed = 10f;
        #endregion

        #region actions
        public const float JumpDuration = 0.5f;
        public const float JumpCooldown = 0.5f;
        public const float WallJumpForce = 5.0f;

        public const float SprintDuration = 1f;
        public const float SprintCooldown = 1f;
        public const float SprintSpeedMultiplier = 1.5f;

        public const float SlideDuration = .25f;
        public const float SlideCooldown = 1f;
        public const float SlideSpeedMultiplier = 1.75f;

        public const float GrabDuration = 0.5f;
        public const float GrabCooldown = 1f;
        public const float GrabSpeedMultiplier = 1f;
        public const float PercentageOfGravityWhileGrabbing = 0.5f; // example default value, adjust as needed
        #endregion
    }
}
