using UnityEngine;

namespace LegendsRPG.Combat
{
    public enum SkillKind
    {
        MeleeStrike,      // instant damage to nearest target in range
        Projectile,       // spawns a homing projectile
        AoEBurst,         // damage every enemy around the caster
        HealAlly,         // heals the lowest-HP teammate
        UltimateNuke,     // heavy single-target ultimate
        UltimateAoe       // heavy area ultimate with shockwave
    }

    /// <summary>
    /// Data-driven skill definition (ScriptableObject created at runtime by HeroDatabase).
    /// Adding new heroes/skills never requires new code paths — only new data.
    /// </summary>
    [CreateAssetMenu(menuName = "LegendsRPG/SkillData", fileName = "NewSkill")]
    public class SkillData : ScriptableObject
    {
        public string skillId = "skill";
        public string displayName = "Skill";
        public SkillKind kind = SkillKind.MeleeStrike;
        public ElementType element = ElementType.None;

        [Header("Power")]
        public float powerMultiplier = 1.8f;     // x attacker Attack stat
        public float cooldownSeconds = 5f;
        public float castRange = 6f;             // melee/projectile range
        public float aoeRadius = 3.5f;           // AoE/Ultimate radius
        public float projectileSpeed = 22f;
        public float ultimateChargeCost = 100f;

        [Header("Presentation")]
        public string vfxKey = "hit";
        public Color vfxColor = new Color(1f, 0.6f, 0.2f);
        public bool cameraShake = true;
        public float shakeMagnitude = 0.25f;

        public bool IsUltimate
        {
            get { return kind == SkillKind.UltimateNuke || kind == SkillKind.UltimateAoe; }
        }
    }
}
