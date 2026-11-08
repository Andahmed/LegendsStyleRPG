using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Combat;

namespace LegendsRPG.Characters
{
    /// <summary>
    /// Runtime roster factory. In the production pipeline these become real
    /// .asset files / backend-driven data; the class interface stays identical.
    /// </summary>
    public static class HeroDatabase
    {
        private static List<HeroData> _all;

        /// <summary>The full roster (cached) — every summonable hero.</summary>
        public static List<HeroData> AllHeroes
        {
            get
            {
                if (_all == null) _all = BuildRoster();
                return _all;
            }
        }

        public static List<HeroData> CreateDefaultRoster()
        {
            return AllHeroes;
        }

        private static List<HeroData> BuildRoster()
        {
            var list = new List<HeroData>();

            // 1 — Kael the Flameblade (starter hero)
            list.Add(MakeHero("kael_flameblade", "Kael", HeroClass.Warrior, ElementType.Fire, Rarity.Epic,
                150, 16, 10, 5.2f, 1.15f, 2.4f, 17, 2.3f, 1.2f,
                new Color(0.85f, 0.38f, 0.16f), new Color(0.28f, 0.3f, 0.36f),
                MakeSkill("kael_s1", "Flame Slash", SkillKind.MeleeStrike, ElementType.Fire, 2.2f, 4f, 2.8f, "slash"),
                MakeSkill("kael_s2", "Fireball", SkillKind.Projectile, ElementType.Fire, 1.9f, 6f, 9f, "fireball"),
                MakeSkill("kael_ult", "Inferno Nova", SkillKind.UltimateAoe, ElementType.Fire, 3.6f, 14f, 0f, "ultimate", 4.5f)));

            // 2 — Sylra of the Frozen Peak
            list.Add(MakeHero("sylra_frost", "Sylra", HeroClass.Mage, ElementType.Ice, Rarity.Legendary,
                115, 19, 7, 4.9f, 0.95f, 7.5f, 14, 2.8f, 0.9f,
                new Color(0.35f, 0.7f, 0.95f), new Color(0.2f, 0.24f, 0.34f),
                MakeSkill("sylra_s1", "Frost Bolt", SkillKind.Projectile, ElementType.Ice, 2.1f, 3.5f, 10f, "ice_shard"),
                MakeSkill("sylra_s2", "Glacier Nova", SkillKind.AoEBurst, ElementType.Ice, 1.7f, 6f, 0f, "ice_nova", 4f),
                MakeSkill("sylra_ult", "Absolute Zero", SkillKind.UltimateAoe, ElementType.Ice, 3.8f, 15f, 0f, "ultimate", 4.5f)));

            // 3 — Rania Stormcaller
            list.Add(MakeHero("rania_storm", "Rania", HeroClass.Archer, ElementType.Lightning, Rarity.Epic,
                105, 17, 6, 5.6f, 1.35f, 8f, 13, 2.4f, 0.85f,
                new Color(0.95f, 0.85f, 0.3f), new Color(0.25f, 0.27f, 0.33f),
                MakeSkill("rania_s1", "Chain Spark", SkillKind.Projectile, ElementType.Lightning, 1.8f, 3f, 11f, "lightning"),
                MakeSkill("rania_s2", "Thunder Step", SkillKind.AoEBurst, ElementType.Lightning, 1.5f, 5f, 0f, "lightning", 3.2f),
                MakeSkill("rania_ult", "Storm Verdict", SkillKind.UltimateNuke, ElementType.Lightning, 5.2f, 16f, 12f, "ultimate", 2f)));

            // 4 — Doran the Dawnward
            list.Add(MakeHero("doran_dawn", "Doran", HeroClass.Paladin, ElementType.Light, Rarity.Rare,
                185, 13, 14, 4.7f, 0.9f, 2.2f, 19, 1.8f, 1.5f,
                new Color(0.95f, 0.9f, 0.65f), new Color(0.35f, 0.34f, 0.3f),
                MakeSkill("doran_s1", "Holy Bash", SkillKind.MeleeStrike, ElementType.Light, 2.0f, 4.5f, 2.6f, "slash"),
                MakeSkill("doran_s2", "Radiant Ward", SkillKind.HealAlly, ElementType.Light, 2.4f, 7f, 0f, "heal"),
                MakeSkill("doran_ult", "Judgement Day", SkillKind.UltimateAoe, ElementType.Light, 3.2f, 15f, 0f, "ultimate", 5f)));

            // 5 — Nyx, the Nightblade
            list.Add(MakeHero("nyx_night", "Nyx", HeroClass.Assassin, ElementType.Dark, Rarity.Legendary,
                98, 22, 5, 6.1f, 1.5f, 2.1f, 12, 2.0f, 0.8f,
                new Color(0.55f, 0.3f, 0.85f), new Color(0.16f, 0.16f, 0.2f),
                MakeSkill("nyx_s1", "Shadow Cut", SkillKind.MeleeStrike, ElementType.Dark, 2.6f, 3.5f, 2.3f, "slash"),
                MakeSkill("nyx_s2", "Void Dagger", SkillKind.Projectile, ElementType.Dark, 1.7f, 4.5f, 8f, "dark_bolt"),
                MakeSkill("nyx_ult", "Eclipse Reap", SkillKind.UltimateNuke, ElementType.Dark, 5.6f, 16f, 3f, "ultimate", 2.5f)));

            // 6 — Verda of the Wild Grove
            list.Add(MakeHero("verda_grove", "Verda", HeroClass.Healer, ElementType.Nature, Rarity.Epic,
                130, 12, 9, 5.0f, 1.0f, 7f, 15, 1.9f, 1.0f,
                new Color(0.35f, 0.8f, 0.4f), new Color(0.3f, 0.26f, 0.22f),
                MakeSkill("verda_s1", "Thorn Vine", SkillKind.Projectile, ElementType.Nature, 1.6f, 3.5f, 9f, "nature_bolt"),
                MakeSkill("verda_s2", "Life Bloom", SkillKind.HealAlly, ElementType.Nature, 3.0f, 6f, 0f, "heal"),
                MakeSkill("verda_ult", "Worldspring", SkillKind.UltimateAoe, ElementType.Nature, 2.6f, 15f, 0f, "ultimate", 4.5f)));

            // ============================================================
            //  v0.4 summon pool — 14 additional heroes (roster total: 20)
            // ============================================================

            // --- Common ---
            AddRosterHero(list, "bram_ironvow", "Bram", HeroClass.Warrior, ElementType.Fire, Rarity.Common,
                110, 12, 9, 5.0f, 1.05f, 2.2f,
                new Color(0.80f, 0.45f, 0.20f), new Color(0.30f, 0.26f, 0.22f),
                "Cinder Cleave", "Ember Burst", "Flame Wall");
            AddRosterHero(list, "pell_mossfoot", "Pell", HeroClass.Healer, ElementType.Nature, Rarity.Common,
                120, 10, 8, 4.9f, 0.95f, 7f,
                new Color(0.45f, 0.70f, 0.35f), new Color(0.28f, 0.24f, 0.18f),
                "Thorn Dart", "Mending Spores", "Grove Blessing");
            AddRosterHero(list, "juno_glint", "Juno", HeroClass.Archer, ElementType.Lightning, Rarity.Common,
                95, 12, 5, 5.4f, 1.30f, 8f,
                new Color(0.95f, 0.80f, 0.35f), new Color(0.24f, 0.25f, 0.30f),
                "Spark Shot", "Static Field", "Sky Piercer");

            // --- Rare ---
            AddRosterHero(list, "tamsin_dusk", "Tamsin", HeroClass.Assassin, ElementType.Dark, Rarity.Rare,
                100, 16, 5, 5.9f, 1.45f, 2.1f,
                new Color(0.40f, 0.30f, 0.60f), new Color(0.15f, 0.14f, 0.20f),
                "Umbral Stab", "Night Bolt", "Veil of Dusk");
            AddRosterHero(list, "odo_shieldhand", "Odo", HeroClass.Paladin, ElementType.Light, Rarity.Rare,
                175, 12, 13, 4.7f, 0.90f, 2.2f,
                new Color(0.90f, 0.85f, 0.60f), new Color(0.32f, 0.30f, 0.26f),
                "Blessed Bash", "Sanctuary", "Aegis Verdict");
            AddRosterHero(list, "wren_hailcaller", "Wren", HeroClass.Mage, ElementType.Ice, Rarity.Rare,
                105, 15, 6, 4.9f, 0.95f, 7.5f,
                new Color(0.45f, 0.75f, 0.90f), new Color(0.20f, 0.24f, 0.32f),
                "Hail Spike", "Frost Ring", "Whiteout");
            AddRosterHero(list, "maro_tidebind", "Maro", HeroClass.Mage, ElementType.Water, Rarity.Rare,
                108, 15, 6, 4.8f, 0.95f, 7.5f,
                new Color(0.35f, 0.60f, 0.95f), new Color(0.18f, 0.22f, 0.30f),
                "Tide Bolt", "Riptide", "Maelstrom");

            // --- Epic ---
            AddRosterHero(list, "iskra_cinder", "Iskra", HeroClass.Warrior, ElementType.Fire, Rarity.Epic,
                155, 17, 10, 5.3f, 1.20f, 2.4f,
                new Color(0.95f, 0.50f, 0.15f), new Color(0.28f, 0.22f, 0.20f),
                "Cinder Edge", "Pyre Wave", "Ashfall Reckoning");
            AddRosterHero(list, "fenn_lorekeeper", "Fenn", HeroClass.Healer, ElementType.Light, Rarity.Epic,
                135, 12, 9, 5.0f, 1.00f, 7f,
                new Color(0.95f, 0.90f, 0.70f), new Color(0.30f, 0.28f, 0.22f),
                "Lumen Dart", "Dawn Mend", "Aurora Veil");
            AddRosterHero(list, "vashta_nightpine", "Vashta", HeroClass.Archer, ElementType.Nature, Rarity.Epic,
                115, 17, 6, 5.5f, 1.35f, 8.5f,
                new Color(0.40f, 0.75f, 0.45f), new Color(0.22f, 0.26f, 0.20f),
                "Bramble Shot", "Overgrowth", "Wildhunt Volley");
            AddRosterHero(list, "korrag_deepfury", "Korrag", HeroClass.Warrior, ElementType.Water, Rarity.Epic,
                165, 16, 11, 5.1f, 1.10f, 2.4f,
                new Color(0.30f, 0.55f, 0.85f), new Color(0.20f, 0.24f, 0.30f),
                "Tidebreaker", "Whirlpool Strike", "Ocean's Wrath");

            // --- Legendary ---
            AddRosterHero(list, "lyra_dawnveil", "Lyra", HeroClass.Paladin, ElementType.Light, Rarity.Legendary,
                190, 15, 15, 4.8f, 0.95f, 2.3f,
                new Color(1f, 0.90f, 0.60f), new Color(0.34f, 0.30f, 0.24f),
                "Radiant Smite", "Guardian Light", "Sunfall Judgement");
            AddRosterHero(list, "morvath_ashen", "Morvath", HeroClass.Mage, ElementType.Dark, Rarity.Legendary,
                120, 20, 7, 4.9f, 0.95f, 7.5f,
                new Color(0.50f, 0.30f, 0.80f), new Color(0.14f, 0.12f, 0.20f),
                "Grave Bolt", "Wither Wave", "Eternal Night");

            // --- Mythic ---
            AddRosterHero(list, "seraphel", "Seraphel", HeroClass.Assassin, ElementType.Lightning, Rarity.Mythic,
                110, 25, 6, 6.2f, 1.55f, 2.1f,
                new Color(0.65f, 0.85f, 1f), new Color(0.18f, 0.20f, 0.30f),
                "Storm Fang", "Thunderbind", "Tempest Execution");

            return list;
        }

        // ---- v0.4 compact roster helpers ----
        private static HeroData AddRosterHero(List<HeroData> list, string id, string name, HeroClass cls,
            ElementType el, Rarity r, float hp, float atk, float def, float ms, float aspd, float range,
            Color primary, Color secondary, string s1, string s2, string ult)
        {
            var h = MakeHero(id, name, cls, el, r,
                hp, atk, def, ms, aspd, range,
                hp * 0.11f, atk * 0.14f, def * 0.13f,
                primary, secondary,
                ClassSkill(cls, el, 1, s1),
                ClassSkill(cls, el, 2, s2),
                ClassSkill(cls, el, 3, ult));
            list.Add(h);
            return h;
        }

        /// <summary>Builds a class-appropriate skill set: [1] signature move,
        /// [2] secondary, [3] ultimate — balanced close to the hand-made heroes.</summary>
        private static SkillData ClassSkill(HeroClass cls, ElementType el, int tier, string displayName)
        {
            SkillKind kind;
            float power, cooldown, castRange;

            switch (cls)
            {
                case HeroClass.Mage:
                    kind = tier == 1 ? SkillKind.Projectile : tier == 2 ? SkillKind.AoEBurst : SkillKind.UltimateAoe;
                    power = tier == 1 ? 2.1f : tier == 2 ? 1.7f : 3.8f;
                    cooldown = tier == 1 ? 3.5f : tier == 2 ? 6f : 15f;
                    castRange = tier == 1 ? 10f : 0f;
                    break;
                case HeroClass.Archer:
                    kind = tier == 1 ? SkillKind.Projectile : tier == 2 ? SkillKind.AoEBurst : SkillKind.UltimateNuke;
                    power = tier == 1 ? 1.8f : tier == 2 ? 1.5f : 5.2f;
                    cooldown = tier == 1 ? 3f : tier == 2 ? 5f : 16f;
                    castRange = tier == 1 ? 11f : tier == 3 ? 12f : 0f;
                    break;
                case HeroClass.Paladin:
                    kind = tier == 1 ? SkillKind.MeleeStrike : tier == 2 ? SkillKind.HealAlly : SkillKind.UltimateAoe;
                    power = tier == 1 ? 2.0f : tier == 2 ? 2.4f : 3.2f;
                    cooldown = tier == 1 ? 4.5f : tier == 2 ? 7f : 15f;
                    castRange = tier == 1 ? 2.6f : 0f;
                    break;
                case HeroClass.Assassin:
                    kind = tier == 1 ? SkillKind.MeleeStrike : tier == 2 ? SkillKind.Projectile : SkillKind.UltimateNuke;
                    power = tier == 1 ? 2.6f : tier == 2 ? 1.7f : 5.6f;
                    cooldown = tier == 1 ? 3.5f : tier == 2 ? 4.5f : 16f;
                    castRange = tier == 1 ? 2.3f : tier == 2 ? 8f : 3f;
                    break;
                case HeroClass.Healer:
                    kind = tier == 1 ? SkillKind.Projectile : tier == 2 ? SkillKind.HealAlly : SkillKind.UltimateAoe;
                    power = tier == 1 ? 1.6f : tier == 2 ? 3.0f : 2.6f;
                    cooldown = tier == 1 ? 3.5f : tier == 2 ? 6f : 15f;
                    castRange = tier == 1 ? 9f : 0f;
                    break;
                default: // Warrior
                    kind = tier == 1 ? SkillKind.MeleeStrike : tier == 2 ? SkillKind.AoEBurst : SkillKind.UltimateAoe;
                    power = tier == 1 ? 2.2f : tier == 2 ? 1.7f : 3.6f;
                    cooldown = tier == 1 ? 4f : tier == 2 ? 6f : 14f;
                    castRange = tier == 1 ? 2.8f : 0f;
                    break;
            }

            string vfx = tier == 3 ? "ultimate" : VfxFor(kind, el);
            return MakeSkill("gen_" + cls + "_" + tier + "_" + el, displayName, kind, el, power, cooldown, castRange, vfx);
        }

        private static string VfxFor(SkillKind kind, ElementType el)
        {
            switch (kind)
            {
                case SkillKind.MeleeStrike: return "slash";
                case SkillKind.HealAlly: return "heal";
                case SkillKind.AoEBurst: return "ice_nova";
                case SkillKind.Projectile:
                    switch (el)
                    {
                        case ElementType.Fire: return "fireball";
                        case ElementType.Ice: return "ice_shard";
                        case ElementType.Water: return "ice_shard";
                        case ElementType.Lightning: return "lightning";
                        case ElementType.Light: return "lightning";
                        case ElementType.Nature: return "nature_bolt";
                        case ElementType.Dark: return "dark_bolt";
                        default: return "fireball";
                    }
                default: return "hit";
            }
        }

        public static Dictionary<string, MonsterData> CreateMonsters()
        {
            var map = new Dictionary<string, MonsterData>();

            map["goblin"] = MakeMonster("goblin", "Ember Goblin", ElementType.Fire, false,
                55, 9, 3, 3.6f, 0.9f, 1.9f, 1.0f, 24f,
                new Color(0.5f, 0.62f, 0.3f), new Color(0.3f, 0.24f, 0.2f));

            map["wolf"] = MakeMonster("wolf", "Frost Wolf", ElementType.Ice, false,
                70, 11, 4, 4.6f, 1.1f, 1.8f, 1.05f, 28f,
                new Color(0.55f, 0.72f, 0.85f), new Color(0.25f, 0.3f, 0.38f));

            map["shaman"] = MakeMonster("shaman", "Tide Shaman", ElementType.Water, false,
                62, 12, 3, 3.0f, 0.75f, 6.5f, 1.0f, 30f,
                new Color(0.3f, 0.55f, 0.9f), new Color(0.2f, 0.25f, 0.32f));

            map["golem"] = MakeMonster("golem", "Moss Golem", ElementType.Nature, false,
                140, 13, 9, 2.2f, 0.55f, 2.1f, 1.45f, 40f,
                new Color(0.35f, 0.55f, 0.35f), new Color(0.4f, 0.34f, 0.24f));

            map["wraith"] = MakeMonster("wraith", "Night Wraith", ElementType.Dark, false,
                58, 14, 2, 4.2f, 1.2f, 2.0f, 0.9f, 34f,
                new Color(0.45f, 0.3f, 0.65f), new Color(0.18f, 0.16f, 0.22f));

            map["boss_titan"] = MakeMonster("boss_titan", "Ember Titan", ElementType.Fire, true,
                520, 20, 12, 2.6f, 0.6f, 2.8f, 2.4f, 220f,
                new Color(0.9f, 0.35f, 0.15f), new Color(0.25f, 0.2f, 0.18f));

            map["boss_lich"] = MakeMonster("boss_lich", "Ancient Lich", ElementType.Dark, true,
                480, 24, 8, 2.4f, 0.7f, 7.5f, 2.2f, 260f,
                new Color(0.5f, 0.35f, 0.9f), new Color(0.15f, 0.13f, 0.2f));

            return map;
        }

        // ---- factories ----
        private static HeroData MakeHero(string id, string name, HeroClass cls, ElementType el, Rarity rarity,
            float hp, float atk, float def, float ms, float aspd, float range,
            float hpLv, float atkLv, float defLv,
            Color primary, Color secondary,
            SkillData s1, SkillData s2, SkillData ult)
        {
            var h = ScriptableObject.CreateInstance<HeroData>();
            h.heroId = id; h.displayName = name; h.heroClass = cls; h.element = el; h.rarity = rarity;
            h.baseHP = hp; h.baseAttack = atk; h.baseDefense = def;
            h.baseMoveSpeed = ms; h.baseAttackSpeed = aspd; h.baseAttackRange = range;
            h.hpPerLevel = hpLv; h.atkPerLevel = atkLv; h.defPerLevel = defLv;
            h.primaryColor = primary; h.secondaryColor = secondary;
            h.skill1 = s1; h.skill2 = s2; h.ultimate = ult;
            return h;
        }

        private static SkillData MakeSkill(string id, string name, SkillKind kind, ElementType el,
            float power, float cooldown, float range, string vfxKey, float aoeRadius = 0f)
        {
            var s = ScriptableObject.CreateInstance<SkillData>();
            s.skillId = id; s.displayName = name; s.kind = kind; s.element = el;
            s.powerMultiplier = power; s.cooldownSeconds = cooldown; s.castRange = range;
            s.aoeRadius = aoeRadius > 0f ? aoeRadius : 3.2f;
            s.vfxKey = vfxKey;
            s.vfxColor = ElementalMatrix.GetElementColor(el);
            s.cameraShake = kind == SkillKind.UltimateAoe || kind == SkillKind.UltimateNuke;
            s.shakeMagnitude = s.cameraShake ? 0.35f : 0.12f;
            return s;
        }

        private static MonsterData MakeMonster(string id, string name, ElementType el, bool boss,
            float hp, float atk, float def, float ms, float aspd, float range, float scale, float exp,
            Color primary, Color secondary)
        {
            var m = ScriptableObject.CreateInstance<MonsterData>();
            m.monsterId = id; m.displayName = name; m.element = el; m.isBoss = boss;
            m.baseHP = hp; m.baseAttack = atk; m.baseDefense = def;
            m.moveSpeed = ms; m.attackSpeed = aspd; m.attackRange = range;
            m.scale = scale; m.expReward = exp;
            m.primaryColor = primary; m.secondaryColor = secondary;
            m.aggroRadius = boss ? 26f : 12f;
            m.attackVfxKey = boss ? "explosion" : "hit";
            return m;
        }
    }
}
