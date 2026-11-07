using UnityEngine;
using LegendsRPG.Combat;

namespace LegendsRPG.Characters
{
    public enum HeroClass { Warrior, Mage, Archer, Paladin, Assassin, Healer }
    public enum Rarity { Common, Rare, Epic, Legendary, Mythic }

    /// <summary>
    /// Hero definition. Runtime-created ScriptableObject (see HeroDatabase) —
    /// designed so the roster can grow to 50+ heroes purely by data.
    /// </summary>
    public class HeroData : ScriptableObject
    {
        public string heroId = "hero";
        public string displayName = "Hero";
        public HeroClass heroClass = HeroClass.Warrior;
        public ElementType element = ElementType.Fire;
        public Rarity rarity = Rarity.Rare;

        [Header("Base stats (level 1)")]
        public float baseHP = 120f;
        public float baseAttack = 14f;
        public float baseDefense = 8f;
        public float baseMoveSpeed = 5.2f;
        public float baseAttackSpeed = 1.1f;
        public float baseAttackRange = 2.4f;

        [Header("Growth per level")]
        public float hpPerLevel = 16f;
        public float atkPerLevel = 2.2f;
        public float defPerLevel = 1.1f;

        [Header("Skills")]
        public SkillData skill1;
        public SkillData skill2;
        public SkillData ultimate;

        [Header("Visuals")]
        public Color primaryColor = new Color(0.85f, 0.4f, 0.2f);
        public Color secondaryColor = new Color(0.3f, 0.32f, 0.38f);
        public Rarity RarityOf { get { return rarity; } }
    }
}
