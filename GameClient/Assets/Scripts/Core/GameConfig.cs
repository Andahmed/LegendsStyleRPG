using UnityEngine;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Central tuning constants for the whole game.
    /// All gameplay numbers live here so balance changes never require
    /// manual duplication across systems (Tower floors, waves, heroes...).
    /// </summary>
    public static class GameConfig
    {
        // ---- Team ----
        public const int TeamSize = 6;

        // ---- Camera ----
        public const float CameraDefaultYaw = 45f;
        public const float CameraDefaultPitch = 48f;
        public const float CameraMinPitch = 25f;
        public const float CameraMaxPitch = 70f;
        public const float CameraMinDistance = 6f;
        public const float CameraMaxDistance = 22f;
        public const float CameraFollowLerp = 8f;

        // ---- Arena ----
        public const float ArenaRadius = 20f;

        // ---- Progression ----
        public const float ExpBase = 40f;          // EXP required for level L = ExpBase * L^ExpGrowth
        public const float ExpGrowth = 1.35f;
        public const float RespawnSeconds = 6f;

        // ---- Tower ----
        public const int FloorsPerDifficulty = 200;
        public const int BossEveryNFloors = 10;
        public const float TowerHpGrowthPerFloor = 0.115f;   // +11.5% compound per floor
        public const float TowerAtkGrowthPerFloor = 0.075f;

        public static readonly float[] DifficultyHpMult = { 1.0f, 2.2f, 4.8f, 9.0f };      // Easy, Hard, Hell, Nightmare
        public static readonly float[] DifficultyAtkMult = { 1.0f, 1.8f, 3.0f, 4.6f };
        public static readonly float[] DifficultyExpMult = { 1.0f, 1.6f, 2.4f, 3.5f };

        // ---- Combat ----
        public const float StrongMultiplier = 1.35f;
        public const float WeakMultiplier = 0.7f;
        public const float CritDefaultChance = 0.08f;
        public const float CritDefaultDamage = 1.6f;
        public const float DefenseFormulaK = 60f;    // reduction = Def / (Def + K * attackerLevel+10)

        // ---- Performance ----
        public const int TargetFrameRate = 60;
        public const float AutosaveInterval = 30f;
    }
}
