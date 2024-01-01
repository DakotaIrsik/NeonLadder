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
        public const float DefaultJumpTakeOffSpeed = 13f;
        public const float DefaultCharacterScale = 1f;
        public const float DefaultMaxHealth = 3f;
        public const float DefaultMaxStamina = 100f;
        public const float DefaultKnockbackDuration = 0.25f; // Duration of the knockback effect
        public const float DefaultKnockbackSpeed = 2f;
        public const float DefaultKnockMultiplier = 1f;
        public const float StaminaRegenRate = 5.0f;

        public static float MaxSpeed { get; set; } = DefaultMaxSpeed;
        public static float JumpTakeOffSpeed { get; set; } = DefaultJumpTakeOffSpeed;
        public static float CrouchScale { get; set; } = DefaultCharacterScale * 0.5f;
        public static float MaxHealth { get; set; } = DefaultMaxHealth;
        public static float MaxStamina { get; set; } = DefaultMaxStamina;
        #endregion

        #region actions
        /// <summary>
        /// The JumpCutOffFactor is a multiplier used to reduce the upward velocity of the player's character when the jump button is released before reaching the maximum jump height. This factor allows for variable jump heights, providing a more dynamic and responsive jumping mechanic in the game. Here's a detailed explanation and some sample values:
        /// </summary>
        public const float JumpCutOffFactor = 0.5f;
        public const float JumpDuration = 1.0f;
        public const float JumpCooldown = 0.5f;
        public const float JumpStaminaCost = 2.0f;
        public const float WallJumpForce = 5.0f;

        public const float SprintDuration = 1f;
        public const float SprintCooldown = 1f;
        public const float SprintSpeedMultiplier = 1.5f;
        public const float SprintStaminaCost = 3f;

        public const float SlideDuration = 0.5f;
        public const float SlideCooldown = 1f;
        public const float SlideSpeedMultiplier = 1f;
        public const float SlideStaminaCost = 50f;

        public const float GrabDuration = 0.5f;
        public const float GrabCooldown = 1f;
        public const float GrabSpeedMultiplier = 1f;
        public const float PercentageOfGravityWhileGrabbing = 0.0f; 
        public const float GrabStaminaCost = 25f;

        public const float MeleeAttackDuration = 0.1f; 
        public const float MeleeAttackScaleX = 0.5f;
        public const float MeleeAttackScaleY = 0.5f;
        public const float MeleeAttackScaleZ = 0.5f;
        public const float MeleeStaminaCost = 5.0f;

        public const float RangedAttackDuration = 0.1f;
        public const float RangedAttackStaminaCost = 5.0f;
        public const float RangedAttackMaxOnScreen = 3.0f;
        public const float RangedAttackLifeTime = 3.0f;
        public const float RangedAttackSpeed = 10.0f;

        public const float CrouchStaminaCost = 25.0f;

        #endregion
    }
}
