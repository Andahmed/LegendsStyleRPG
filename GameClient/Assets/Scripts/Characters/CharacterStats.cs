using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Combat;

namespace LegendsRPG.Characters
{
    [Serializable]
    public class StatBonus
    {
        public float hp;
        public float attack;
        public float defense;
        public float moveSpeed;
    }

    /// <summary>
    /// All combat stats of a character (hero or monster) + the damage formula entry point.
    /// Supports level growth, equipment bonuses and rarity multipliers (Phase 0.4 systems).
    /// </summary>
    public class CharacterStats : MonoBehaviour
    {
        [Header("Identity")]
        public int Level = 1;
        public float Exp = 0f;
        public ElementType Element = ElementType.None;

        [Header("Core stats")]
        public float MaxHP = 100f;
        public float Attack = 10f;
        public float Defense = 5f;
        public float MoveSpeed = 5f;
        public float AttackSpeed = 1f;     // attacks per second
        public float AttackRange = 2.2f;
        public float CritChance = GameConfig.CritDefaultChance;
        public float CritDamage = GameConfig.CritDefaultDamage;

        [NonSerialized] public List<StatBonus> EquipmentBonuses = new List<StatBonus>();

        public event Action<int> OnLevelUp;    // new level

        private float _baseMaxHP, _baseAttack, _baseDefense, _baseMoveSpeed;

        public void Init(float maxHp, float attack, float defense, float moveSpeed,
            float attackSpeed, float attackRange, ElementType element, int level)
        {
            _baseMaxHP = maxHp; _baseAttack = attack; _baseDefense = defense; _baseMoveSpeed = moveSpeed;
            AttackSpeed = attackSpeed; AttackRange = attackRange; Element = element;
            Level = Mathf.Max(1, level);
            Recalculate();
            var h = GetComponent<Health>();
            if (h != null) h.Init(MaxHP);
        }

        public void Recalculate()
        {
            MaxHP = _baseMaxHP; Attack = _baseAttack; Defense = _baseDefense; MoveSpeed = _baseMoveSpeed;
            foreach (var b in EquipmentBonuses)
            {
                MaxHP += b.hp; Attack += b.attack; Defense += b.defense; MoveSpeed += b.moveSpeed;
            }
        }

        public void AddEquipmentBonus(StatBonus bonus)
        {
            EquipmentBonuses.Add(bonus);
            float hpBefore = MaxHP;
            Recalculate();
            var h = GetComponent<Health>();
            if (h != null)
            {
                h.Max = MaxHP;
                h.Current += Mathf.Max(0f, MaxHP - hpBefore);
            }
        }

        // ---- EXP / levels ----
        public static float NeededExp(int level)
        {
            return GameConfig.ExpBase * Mathf.Pow(level, GameConfig.ExpGrowth);
        }

        public void GrantExp(float amount)
        {
            if (amount <= 0f) return;
            Exp += amount;
            while (Exp >= NeededExp(Level))
            {
                Exp -= NeededExp(Level);
                Level++;
                // per-level growth applied by owner (HeroCharacter knows its HeroData)
                if (OnLevelUp != null) OnLevelUp(Level);
            }
        }

        public void ApplyLevelGrowth(float hpPerLevel, float atkPerLevel, float defPerLevel)
        {
            _baseMaxHP += hpPerLevel;
            _baseAttack += atkPerLevel;
            _baseDefense += defPerLevel;
            Recalculate();
            var h = GetComponent<Health>();
            if (h != null)
            {
                float ratio = h.Max > 0f ? h.Current / h.Max : 1f;
                h.Max = MaxHP;
                h.Current = MaxHP * ratio;
            }
        }
    }
}
