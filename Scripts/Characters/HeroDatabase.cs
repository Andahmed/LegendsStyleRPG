using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Combat;

namespace LegendsRPG.Characters
{
    /// <summary>
    /// Runtime roster factory — 20 handcrafted heroes + 42 formula-generated = 62.
    /// In the production pipeline these become real
    /// .asset files / backend-driven data; the class interface stays identical.
    /// </summary>
    public static class HeroDatabase
    {
        public static List<HeroData> CreateDefaultRoster()
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

            // 7 — Torga of the Tide Reach
            list.Add(MakeHero("torga_tide", "Torga", HeroClass.Warrior, ElementType.Water, Rarity.Rare,
                160, 15, 11, 5.0f, 1.05f, 2.4f, 18, 2.1f, 1.25f,
                new Color(0.3f, 0.6f, 0.95f), new Color(0.2f, 0.25f, 0.35f),
                MakeSkill("torga_s1", "Tide Slash", SkillKind.MeleeStrike, ElementType.Water, 2.1f, 4f, 2.8f, "slash"),
                MakeSkill("torga_s2", "Undertow", SkillKind.AoEBurst, ElementType.Water, 1.6f, 6.5f, 0f, "water_bolt", 4f),
                MakeSkill("torga_ult", "Riptide Reign", SkillKind.UltimateAoe, ElementType.Water, 3.5f, 15f, 0f, "ultimate", 4.8f)));

            // 8 — Ignara the Cinder Witch
            list.Add(MakeHero("ignara_cinder", "Ignara", HeroClass.Mage, ElementType.Fire, Rarity.Epic,
                112, 20, 7, 4.8f, 0.95f, 7.5f, 13, 3.0f, 0.8f,
                new Color(0.95f, 0.5f, 0.2f), new Color(0.3f, 0.2f, 0.18f),
                MakeSkill("ignara_s1", "Cinder Bolt", SkillKind.Projectile, ElementType.Fire, 2.0f, 3.5f, 9.5f, "fireball"),
                MakeSkill("ignara_s2", "Ash Storm", SkillKind.AoEBurst, ElementType.Fire, 1.7f, 6f, 0f, "explosion", 4.4f),
                MakeSkill("ignara_ult", "Pyre of Stars", SkillKind.UltimateAoe, ElementType.Fire, 3.7f, 15f, 0f, "ultimate", 4.6f)));

            // 9 — Zephyra Windstring
            list.Add(MakeHero("zephyra_wind", "Zephyra", HeroClass.Archer, ElementType.Nature, Rarity.Epic,
                106, 18, 6, 5.7f, 1.4f, 8.0f, 12, 2.5f, 0.8f,
                new Color(0.5f, 0.9f, 0.45f), new Color(0.24f, 0.3f, 0.24f),
                MakeSkill("zephyra_s1", "Leaf Volley", SkillKind.Projectile, ElementType.Nature, 1.9f, 3.2f, 10f, "nature_bolt"),
                MakeSkill("zephyra_s2", "Gale Winds", SkillKind.AoEBurst, ElementType.Nature, 1.5f, 5.5f, 0f, "nature_bolt", 4.2f),
                MakeSkill("zephyra_ult", "Thorn Tempest", SkillKind.UltimateNuke, ElementType.Nature, 5.1f, 15f, 10f, "ultimate")));

            // 10 — Morvath the Grave Speaker
            list.Add(MakeHero("morvath_grave", "Morvath", HeroClass.Mage, ElementType.Dark, Rarity.Legendary,
                118, 21, 7, 4.9f, 0.9f, 7.8f, 14, 3.1f, 0.85f,
                new Color(0.6f, 0.35f, 0.9f), new Color(0.2f, 0.18f, 0.28f),
                MakeSkill("morvath_s1", "Grave Bolt", SkillKind.Projectile, ElementType.Dark, 2.2f, 3.6f, 9.5f, "dark_bolt"),
                MakeSkill("morvath_s2", "Soul Siphon", SkillKind.AoEBurst, ElementType.Dark, 1.8f, 6.5f, 0f, "explosion", 4.2f),
                MakeSkill("morvath_ult", "Nightfall", SkillKind.UltimateAoe, ElementType.Dark, 3.9f, 15f, 0f, "ultimate", 5f)));

            // 11 — Aurelia of the Last Chalice
            list.Add(MakeHero("aurelia_chalice", "Aurelia", HeroClass.Healer, ElementType.Light, Rarity.Epic,
                128, 13, 9, 5.0f, 1.0f, 7.0f, 15, 1.9f, 1.0f,
                new Color(1f, 0.95f, 0.7f), new Color(0.35f, 0.33f, 0.28f),
                MakeSkill("aurelia_s1", "Light Mend", SkillKind.HealAlly, ElementType.Light, 2.5f, 5f, 0f, "heal"),
                MakeSkill("aurelia_s2", "Sanctuary Pulse", SkillKind.AoEBurst, ElementType.Light, 1.3f, 6f, 0f, "cast", 3.8f),
                MakeSkill("aurelia_ult", "Dawn Salvation", SkillKind.UltimateAoe, ElementType.Light, 2.7f, 15f, 0f, "ultimate", 5f)));

            // 12 — Grimm the Plague Doctor
            list.Add(MakeHero("grimm_plague", "Grimm", HeroClass.Assassin, ElementType.Dark, Rarity.Epic,
                96, 22, 5, 6.2f, 1.55f, 2.1f, 12, 2.6f, 0.7f,
                new Color(0.35f, 0.4f, 0.45f), new Color(0.15f, 0.16f, 0.2f),
                MakeSkill("grimm_s1", "Plague Cut", SkillKind.MeleeStrike, ElementType.Dark, 2.5f, 3.2f, 2.3f, "slash"),
                MakeSkill("grimm_s2", "Venom Flask", SkillKind.Projectile, ElementType.Dark, 1.8f, 4.5f, 8f, "dark_bolt"),
                MakeSkill("grimm_ult", "Blackout", SkillKind.UltimateNuke, ElementType.Dark, 5.5f, 15f, 3f, "ultimate")));

            // 13 — Solinne Sunmantle
            list.Add(MakeHero("solinne_sun", "Solinne", HeroClass.Paladin, ElementType.Light, Rarity.Legendary,
                190, 14, 15, 4.7f, 0.9f, 2.2f, 20, 1.8f, 1.6f,
                new Color(0.98f, 0.9f, 0.6f), new Color(0.4f, 0.36f, 0.28f),
                MakeSkill("solinne_s1", "Sunlit Bash", SkillKind.MeleeStrike, ElementType.Light, 2.1f, 4.2f, 2.6f, "slash"),
                MakeSkill("solinne_s2", "Divine Aegis", SkillKind.HealAlly, ElementType.Light, 2.5f, 7f, 0f, "heal"),
                MakeSkill("solinne_ult", "Solar Judgement", SkillKind.UltimateAoe, ElementType.Light, 3.3f, 15f, 0f, "ultimate", 5.2f)));

            // 14 — Ferrox Stormhand
            list.Add(MakeHero("ferrox_storm", "Ferrox", HeroClass.Warrior, ElementType.Lightning, Rarity.Rare,
                148, 16, 10, 5.3f, 1.2f, 2.4f, 17, 2.3f, 1.15f,
                new Color(0.95f, 0.85f, 0.3f), new Color(0.3f, 0.28f, 0.2f),
                MakeSkill("ferrox_s1", "Volt Blade", SkillKind.MeleeStrike, ElementType.Lightning, 2.2f, 3.8f, 2.8f, "slash"),
                MakeSkill("ferrox_s2", "Static Field", SkillKind.AoEBurst, ElementType.Lightning, 1.6f, 6f, 0f, "lightning", 4f),
                MakeSkill("ferrox_ult", "Thundercage", SkillKind.UltimateAoe, ElementType.Lightning, 3.5f, 15f, 0f, "ultimate", 4.6f)));

            // 15 — Mirelle the Pearl Tide
            list.Add(MakeHero("mirelle_pearl", "Mirelle", HeroClass.Healer, ElementType.Water, Rarity.Rare,
                132, 12, 9, 5.0f, 1.0f, 6.8f, 16, 1.8f, 1.0f,
                new Color(0.4f, 0.7f, 0.95f), new Color(0.22f, 0.28f, 0.36f),
                MakeSkill("mirelle_s1", "Tidal Balm", SkillKind.HealAlly, ElementType.Water, 2.4f, 5.5f, 0f, "heal"),
                MakeSkill("mirelle_s2", "Mist Veil", SkillKind.AoEBurst, ElementType.Water, 1.2f, 6.2f, 0f, "water_bolt", 3.6f),
                MakeSkill("mirelle_ult", "Healing Monsoon", SkillKind.UltimateAoe, ElementType.Water, 2.6f, 15f, 0f, "ultimate", 4.8f)));

            // 16 — Cryon of the Glacier Gate
            list.Add(MakeHero("cryon_glacier", "Cryon", HeroClass.Warrior, ElementType.Ice, Rarity.Epic,
                156, 15, 12, 4.9f, 1.0f, 2.4f, 18, 2.0f, 1.3f,
                new Color(0.55f, 0.8f, 0.95f), new Color(0.25f, 0.3f, 0.4f),
                MakeSkill("cryon_s1", "Frost Edge", SkillKind.MeleeStrike, ElementType.Ice, 2.2f, 4f, 2.8f, "slash"),
                MakeSkill("cryon_s2", "Permafrost", SkillKind.AoEBurst, ElementType.Ice, 1.6f, 6.4f, 0f, "ice_nova", 4.2f),
                MakeSkill("cryon_ult", "Glacier Prison", SkillKind.UltimateAoe, ElementType.Ice, 3.6f, 15f, 0f, "ultimate", 4.8f)));

            // 17 — Volkan the Bomb Thrower
            list.Add(MakeHero("volkan_bomb", "Volkan", HeroClass.Assassin, ElementType.Fire, Rarity.Legendary,
                100, 23, 5, 6.0f, 1.5f, 2.1f, 12, 2.7f, 0.75f,
                new Color(0.9f, 0.4f, 0.15f), new Color(0.2f, 0.15f, 0.14f),
                MakeSkill("volkan_s1", "Ember Fang", SkillKind.MeleeStrike, ElementType.Fire, 2.6f, 3.2f, 2.3f, "slash"),
                MakeSkill("volkan_s2", "Bomb Toss", SkillKind.Projectile, ElementType.Fire, 1.9f, 4.6f, 8f, "fireball"),
                MakeSkill("volkan_ult", "Detonation", SkillKind.UltimateNuke, ElementType.Fire, 5.6f, 15f, 3f, "ultimate")));

            // 18 — Sylvara Bloomsong
            list.Add(MakeHero("sylvara_bloom", "Sylvara", HeroClass.Mage, ElementType.Nature, Rarity.Rare,
                110, 19, 7, 4.9f, 0.95f, 7.4f, 13, 2.9f, 0.8f,
                new Color(0.4f, 0.85f, 0.5f), new Color(0.26f, 0.3f, 0.24f),
                MakeSkill("sylvara_s1", "Bloom Bolt", SkillKind.Projectile, ElementType.Nature, 2.0f, 3.5f, 9.5f, "nature_bolt"),
                MakeSkill("sylvara_s2", "Pollen Burst", SkillKind.AoEBurst, ElementType.Nature, 1.6f, 6.2f, 0f, "nature_bolt", 4f),
                MakeSkill("sylvara_ult", "Grove of Thorns", SkillKind.UltimateAoe, ElementType.Nature, 3.4f, 15f, 0f, "ultimate", 4.6f)));

            // 19 — Umbros, Reaper of the Ninth Hall
            list.Add(MakeHero("umbros_reaper", "Umbros", HeroClass.Warrior, ElementType.Dark, Rarity.Mythic,
                175, 18, 12, 5.1f, 1.1f, 2.4f, 20, 2.5f, 1.35f,
                new Color(0.45f, 0.3f, 0.7f), new Color(0.18f, 0.17f, 0.24f),
                MakeSkill("umbros_s1", "Reaper Cut", SkillKind.MeleeStrike, ElementType.Dark, 2.3f, 3.9f, 2.8f, "slash"),
                MakeSkill("umbros_s2", "Nightfall Sweep", SkillKind.AoEBurst, ElementType.Dark, 1.7f, 6.2f, 0f, "dark_bolt", 4.2f),
                MakeSkill("umbros_ult", "Death Harvest", SkillKind.UltimateAoe, ElementType.Dark, 3.8f, 15f, 0f, "ultimate", 5f)));

            // 20 — Lumina the Radiant Oracle
            list.Add(MakeHero("lumina_oracle", "Lumina", HeroClass.Mage, ElementType.Light, Rarity.Mythic,
                120, 23, 8, 4.9f, 1.0f, 8.0f, 15, 3.3f, 0.9f,
                new Color(1f, 0.97f, 0.8f), new Color(0.35f, 0.35f, 0.4f),
                MakeSkill("lumina_s1", "Radiant Bolt", SkillKind.Projectile, ElementType.Light, 2.3f, 3.4f, 10f, "cast"),
                MakeSkill("lumina_s2", "Halo Burst", SkillKind.AoEBurst, ElementType.Light, 1.8f, 6f, 0f, "cast", 4.4f),
                MakeSkill("lumina_ult", "Supernova Prayer", SkillKind.UltimateAoe, ElementType.Light, 4.0f, 15f, 0f, "ultimate", 5.2f)));

            // 21..62 — formula-generated roster: 7 elements x 6 heroes = 42 more.
            // Deterministic (no Random) so hero ids stay stable across saves.
            list.AddRange(CreateGeneratedRoster());

            return list;
        }

        // ================= generated roster (data-driven, zero duplication) =================

        private static readonly ElementType[] GenElements =
        {
            ElementType.Fire, ElementType.Water, ElementType.Nature, ElementType.Ice,
            ElementType.Lightning, ElementType.Dark, ElementType.Light
        };

        private static readonly string[][] GenNames =
        {
            new[] { "Ember", "Ash", "Cinder", "Blaze", "Pyra", "Magma", "Scorch", "Ignis" },
            new[] { "Marina", "Coral", "Tide", "Brine", "Mistral", "Rill", "Undine", "Nautis" },
            new[] { "Fern", "Thorne", "Briar", "Sage", "Willow", "Moss", "Rosethorn", "Verdling" },
            new[] { "Frost", "Snowe", "Glacia", "Rime", "Crystal", "Boreal", "Sleet", "Winter" },
            new[] { "Volt", "Storm", "Static", "Bolt", "Tempest", "Spark", "Thunder", "Galvan" },
            new[] { "Umbra", "Shade", "Dusk", "Raven", "Noctis", "Hex", "Eclipse", "Nyxar" },
            new[] { "Lux", "Dawn", "Aurora", "Radiant", "Sol", "Gleam", "Halo", "Lumen" }
        };

        private static readonly string[] GenSurnames =
            { "blade", "song", "fang", "veil", "heart", "wind", "oath", "ward" };

        private static readonly Rarity[] GenRarities =
        {
            Rarity.Common, Rarity.Rare, Rarity.Rare,
            Rarity.Epic, Rarity.Legendary, Rarity.Mythic
        };

        private static readonly float[] RarityStatMult = { 0.85f, 1.0f, 1.15f, 1.3f, 1.5f };

        private static List<HeroData> CreateGeneratedRoster()
        {
            var roster = new List<HeroData>();

            for (int e = 0; e < GenElements.Length; e++)
            {
                ElementType el = GenElements[e];
                for (int i = 0; i < 6; i++)   // 7 elements x 6 = 42 generated + 20 handcrafted = 62 total
                {
                    HeroClass cls = (HeroClass)((i + e) % 6);   // every element covers all 6 classes
                    Rarity rarity = GenRarities[i];
                    string name = GenNames[e][i] + " " + GenSurnames[(i + e) % GenSurnames.Length];
                    roster.Add(MakeGeneratedHero(el, i, cls, rarity, name));
                }
            }
            return roster;
        }

        private static HeroData MakeGeneratedHero(ElementType el, int idx,
            HeroClass cls, Rarity rarity, string name)
        {
            int rarityIdx = (int)rarity;
            float m = RarityStatMult[rarityIdx];

            // class archetype (level-1 stats before rarity scaling)
            float hp, atk, def, ms, aspd, range, hpLv, atkLv, defLv;
            switch (cls)
            {
                case HeroClass.Mage:
                    hp = 110; atk = 20; def = 6; ms = 4.9f; aspd = 0.95f; range = 7.5f;
                    hpLv = 13; atkLv = 2.9f; defLv = 0.8f; break;
                case HeroClass.Archer:
                    hp = 105; atk = 17; def = 6; ms = 5.6f; aspd = 1.35f; range = 8.0f;
                    hpLv = 12; atkLv = 2.4f; defLv = 0.8f; break;
                case HeroClass.Paladin:
                    hp = 185; atk = 13; def = 14; ms = 4.7f; aspd = 0.9f; range = 2.2f;
                    hpLv = 19; atkLv = 1.7f; defLv = 1.5f; break;
                case HeroClass.Assassin:
                    hp = 95; atk = 22; def = 5; ms = 6.1f; aspd = 1.5f; range = 2.1f;
                    hpLv = 12; atkLv = 2.5f; defLv = 0.7f; break;
                case HeroClass.Healer:
                    hp = 130; atk = 12; def = 9; ms = 5.0f; aspd = 1.0f; range = 7.0f;
                    hpLv = 15; atkLv = 1.8f; defLv = 1.0f; break;
                default: // Warrior
                    hp = 150; atk = 15; def = 11; ms = 5.2f; aspd = 1.1f; range = 2.4f;
                    hpLv = 17; atkLv = 2.2f; defLv = 1.2f; break;
            }

            hp *= m; atk *= m; def *= m; hpLv *= m; atkLv *= m; defLv *= m;

            Color primary = ElementalMatrix.GetElementColor(el);
            primary = Color.Lerp(primary, Color.white, (idx % 3) * 0.08f);
            Color secondary = Color.Lerp(primary, new Color(0.16f, 0.17f, 0.2f), 0.6f);

            string id = "gen_" + el.ToString().ToLower() + "_" + idx;
            float powerScale = 1f + 0.06f * rarityIdx;

            SkillData s1, s2, ult;
            switch (cls)
            {
                case HeroClass.Mage:
                    s1 = GenSkill(id, "_s1", "Element Bolt", SkillKind.Projectile, el, powerScale, 4f, 9f, ProjKey(el));
                    s2 = GenSkill(id, "_s2", "Element Burst", SkillKind.AoEBurst, el, 1.6f * powerScale, 6.5f, 0f, NovaKey(el), 4.2f);
                    ult = GenSkill(id, "_ult", "Cataclysm", SkillKind.UltimateAoe, el, 3.4f * powerScale, 15f, 0f, "ultimate", 5f);
                    break;
                case HeroClass.Archer:
                    s1 = GenSkill(id, "_s1", "Piercing Shot", SkillKind.Projectile, el, powerScale, 3.6f, 10f, ProjKey(el));
                    s2 = GenSkill(id, "_s2", "Volley", SkillKind.AoEBurst, el, 1.5f * powerScale, 6f, 0f, NovaKey(el), 4f);
                    ult = GenSkill(id, "_ult", "Deadeye Mark", SkillKind.UltimateNuke, el, 5.0f * powerScale, 15f, 11f, "ultimate");
                    break;
                case HeroClass.Paladin:
                    s1 = GenSkill(id, "_s1", "Shield Bash", SkillKind.MeleeStrike, el, 2.0f * powerScale, 4.2f, 2.6f, "slash");
                    s2 = GenSkill(id, "_s2", "Blessing", SkillKind.HealAlly, el, 2.4f * powerScale, 7f, 0f, "heal");
                    ult = GenSkill(id, "_ult", "Sanctuary", SkillKind.UltimateAoe, el, 3.2f * powerScale, 15f, 0f, "ultimate", 5f);
                    break;
                case HeroClass.Assassin:
                    s1 = GenSkill(id, "_s1", "Quick Cut", SkillKind.MeleeStrike, el, 2.4f * powerScale, 3.2f, 2.3f, "slash");
                    s2 = GenSkill(id, "_s2", "Poison Dart", SkillKind.Projectile, el, 1.8f * powerScale, 5f, 8f, ProjKey(el));
                    ult = GenSkill(id, "_ult", "Death Mark", SkillKind.UltimateNuke, el, 5.4f * powerScale, 15f, 3.5f, "ultimate");
                    break;
                case HeroClass.Healer:
                    s1 = GenSkill(id, "_s1", "Nature's Gift", SkillKind.HealAlly, el, 2.6f * powerScale, 5.5f, 0f, "heal");
                    s2 = GenSkill(id, "_s2", "Pacify", SkillKind.AoEBurst, el, 1.3f * powerScale, 6f, 0f, NovaKey(el), 3.8f);
                    ult = GenSkill(id, "_ult", "Renewal Wave", SkillKind.UltimateAoe, el, 2.8f * powerScale, 15f, 0f, "ultimate", 5f);
                    break;
                default: // Warrior
                    s1 = GenSkill(id, "_s1", "Heavy Slash", SkillKind.MeleeStrike, el, 2.2f * powerScale, 4f, 2.8f, "slash");
                    s2 = GenSkill(id, "_s2", "Cleave", SkillKind.AoEBurst, el, 1.6f * powerScale, 6.5f, 0f, NovaKey(el), 4f);
                    ult = GenSkill(id, "_ult", "War Cry", SkillKind.UltimateAoe, el, 3.4f * powerScale, 15f, 0f, "ultimate", 4.8f);
                    break;
            }

            return MakeHero(id, name, cls, el, rarity,
                Mathf.RoundToInt(hp), Mathf.RoundToInt(atk), Mathf.RoundToInt(def),
                ms, aspd, range, Mathf.RoundToInt(hpLv), atkLv, defLv,
                primary, secondary, s1, s2, ult);
        }

        private static string ProjKey(ElementType el)
        {
            switch (el)
            {
                case ElementType.Fire: return "fireball";
                case ElementType.Water: return "water_bolt";
                case ElementType.Nature: return "nature_bolt";
                case ElementType.Lightning: return "lightning";
                case ElementType.Dark: return "dark_bolt";
                case ElementType.Light: return "cast";
                default: return "ice_shard";
            }
        }

        private static string NovaKey(ElementType el)
        {
            return (el == ElementType.Fire || el == ElementType.Dark) ? "explosion" : "ice_nova";
        }

        private static SkillData GenSkill(string heroId, string suffix, string name, SkillKind kind,
            ElementType el, float power, float cooldown, float range, string vfxKey, float aoeRadius = 0f)
        {
            return MakeSkill(heroId + suffix, name, kind, el, power, cooldown, range, vfxKey, aoeRadius);
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

            map["storm_hawk"] = MakeMonster("storm_hawk", "Storm Hawk", ElementType.Lightning, false,
                52, 15, 2, 5.4f, 1.3f, 2.6f, 0.85f, 36f,
                new Color(0.9f, 0.85f, 0.3f), new Color(0.2f, 0.22f, 0.3f));

            map["coral_crab"] = MakeMonster("coral_crab", "Coral Crab", ElementType.Water, false,
                95, 10, 8, 2.8f, 0.7f, 2.0f, 1.0f, 30f,
                new Color(0.95f, 0.55f, 0.5f), new Color(0.25f, 0.45f, 0.65f));

            map["vine_spider"] = MakeMonster("vine_spider", "Vine Spider", ElementType.Nature, false,
                66, 12, 4, 4.4f, 1.25f, 2.2f, 0.95f, 32f,
                new Color(0.3f, 0.6f, 0.3f), new Color(0.2f, 0.3f, 0.18f));

            map["sun_eagle"] = MakeMonster("sun_eagle", "Sun Eagle", ElementType.Light, false,
                74, 13, 5, 5.0f, 1.2f, 2.4f, 1.05f, 38f,
                new Color(0.98f, 0.9f, 0.55f), new Color(0.5f, 0.42f, 0.25f));

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
