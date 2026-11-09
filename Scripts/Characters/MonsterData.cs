using UnityEngine;
using LegendsRPG.Combat;

namespace LegendsRPG.Characters
{
    /// <summary>Monster definition (runtime ScriptableObject, see HeroDatabase).</summary>
    public class MonsterData : ScriptableObject
    {
        public string monsterId = "monster";
        public string displayName = "Monster";
        public ElementType element = ElementType.None;
        public bool isBoss = false;

        public float baseHP = 60f;
        public float baseAttack = 8f;
        public float baseDefense = 3f;
        public float moveSpeed = 3.2f;
        public float attackSpeed = 0.8f;
        public float attackRange = 1.9f;
        public float aggroRadius = 12f;
        public float expReward = 22f;

        [Header("Visuals")]
        public float scale = 1f;
        public Color primaryColor = new Color(0.45f, 0.6f, 0.35f);
        public Color secondaryColor = new Color(0.25f, 0.28f, 0.3f);
        public string attackVfxKey = "hit";
    }
}
