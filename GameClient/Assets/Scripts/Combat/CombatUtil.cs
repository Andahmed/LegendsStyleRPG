using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Core;

namespace LegendsRPG.Combat
{
    /// <summary>
    /// Shared damage + targeting helpers used by heroes, monsters, skills and projectiles.
    /// Keeps the damage formula in exactly one place.
    /// </summary>
    public static class CombatUtil
    {
        // ---- Damage pipeline ----
        // final = base * elementMult * defenseReduction * (crit ? critDamage : 1)
        public static float DealDamage(CharacterStats attacker, CharacterStats defender,
            float baseAmount, ElementType element, bool isSkill, GameObject sourceGO)
        {
            if (attacker == null || defender == null) return 0f;

            var defHealth = defender.GetComponent<Health>();
            if (defHealth == null || defHealth.IsDead) return 0f;

            bool crit = Random.value < attacker.CritChance;
            float elemental = ElementalMatrix.GetMultiplier(element, defender.Element);
            float defense = defender.Defense / (defender.Defense + GameConfig.DefenseFormulaK * (attacker.Level + 10f));
            float final = baseAmount * elemental * (1f - Mathf.Clamp01(defense));
            if (crit) final *= attacker.CritDamage;
            final = Mathf.Max(1f, final);

            defHealth.ApplyDamage(final, sourceGO, element, crit, isSkill);

            // Ultimate charge for hero attackers (fills while fighting)
            var hero = attacker.GetComponent<HeroCharacter>();
            if (hero != null && hero.Caster != null)
            {
                hero.Caster.AddUltCharge(final * 0.35f);
            }

            return final;
        }

        // ---- Registries & targeting ----
        public static MonsterCharacter FindNearestMonster(Vector3 pos, float maxRange)
        {
            MonsterCharacter best = null;
            float bestSqr = maxRange * maxRange;
            var all = MonsterCharacter.All;
            for (int i = 0; i < all.Count; i++)
            {
                var m = all[i];
                if (m == null || m.HP == null || m.HP.IsDead) continue;
                float d = (m.transform.position - pos).sqrMagnitude;
                if (d <= bestSqr) { bestSqr = d; best = m; }
            }
            return best;
        }

        public static HeroCharacter FindNearestHero(Vector3 pos, float maxRange)
        {
            HeroCharacter best = null;
            float bestSqr = maxRange * maxRange;
            var team = Team.TeamManager.InstanceRef;
            if (team == null) return null;
            var heroes = team.Heroes;
            for (int i = 0; i < heroes.Count; i++)
            {
                var h = heroes[i];
                if (h == null || h.HP == null || h.HP.IsDead) continue;
                float d = (h.transform.position - pos).sqrMagnitude;
                if (d <= bestSqr) { bestSqr = d; best = h; }
            }
            return best;
        }

        public static List<MonsterCharacter> MonstersWithin(Vector3 pos, float radius)
        {
            var result = new List<MonsterCharacter>();
            float sqr = radius * radius;
            var all = MonsterCharacter.All;
            for (int i = 0; i < all.Count; i++)
            {
                var m = all[i];
                if (m == null || m.HP == null || m.HP.IsDead) continue;
                if ((m.transform.position - pos).sqrMagnitude <= sqr) result.Add(m);
            }
            return result;
        }
    }
}
