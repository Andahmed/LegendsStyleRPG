using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;

namespace LegendsRPG.Core
{
    // ====================================================================
    // ECONOMY — gold / diamonds / VIP. Single source of truth, event-driven.
    // ====================================================================
    public static class Economy
    {
        public static long Gold { get; private set; }
        public static long Diamonds { get; private set; }
        public static int VipLevel { get; private set; }
        public static float VipExp { get; private set; }
        public static long LifetimeGoldEarned { get; private set; }

        public static event Action OnChanged;

        public static float GoldBonus => 1f + TechSystem.GoldBonus + VipBonusGold();
        public static float ExpBonus => 1f + TechSystem.ExpBonus + VipLevel * 0.02f;

        private static float VipBonusGold()
        {
            switch (VipLevel)
            {
                case 0: return 0f;
                case 1: return 0.05f;
                case 2: return 0.10f;
                case 3: return 0.16f;
                default: return 0.16f + (VipLevel - 3) * 0.02f;
            }
        }

        public static void AddGold(long amount, string source)
        {
            if (amount <= 0) return;
            long real = (long)Mathf.CeilToInt(amount * GoldBonus);
            Gold += real;
            LifetimeGoldEarned += real;
            QuestSystem.ReportProgress("earn_gold", (int)Math.Min(real, (long)int.MaxValue));
            Notify(source + " +" + Format(real) + " Gold");
        }

        public static bool TrySpendGold(long amount)
        {
            if (Gold < amount)
            {
                Notify("Not enough Gold!");
                Audio.AudioSystem.Play("Error");
                return false;
            }
            Gold -= amount;
            VipExp += amount * 0.0006f;
            CheckVipUp();
            OnChanged?.Invoke();
            return true;
        }

        public static void AddDiamonds(long amount, string source)
        {
            if (amount <= 0) return;
            Diamonds += amount;
            Notify(source + " +" + Format(amount) + " Diamonds");
        }

        public static bool TrySpendDiamonds(long amount)
        {
            if (Diamonds < amount)
            {
                Notify("Not enough Diamonds!");
                Audio.AudioSystem.Play("Error");
                return false;
            }
            Diamonds -= amount;
            VipExp += amount * 0.05f;
            CheckVipUp();
            OnChanged?.Invoke();
            return true;
        }

        private static void CheckVipUp()
        {
            // VIP thresholds per level (diamond-equivalent spending)
            while (VipLevel < 10 && VipExp >= VipExpNeeded(VipLevel + 1))
            {
                VipLevel++;
                CombatEvents.RaiseNotification("VIP " + VipLevel + " unlocked! Rewards added");
                Audio.AudioSystem.Play("LevelUp");
                // VIP gifts
                AddDiamonds(100 * VipLevel, "VIP" + VipLevel);
                for (int i = 0; i < VipLevel; i++)
                {
                    Items.InventorySystem.Grant(Items.ItemDatabase.RollRandomItemId());
                }
            }
        }

        public static float VipExpNeeded(int level)
        {
            return 100f * level * level;
        }

        public static string Format(long v)
        {
            if (v >= 1000000000) return (v / 1000000000f).ToString("0.##") + "B";
            if (v >= 1000000) return (v / 1000000f).ToString("0.##") + "M";
            if (v >= 10000) return (v / 1000f).ToString("0.#") + "K";
            return v.ToString();
        }

        private static void Notify(string msg)
        {
            Combat.CombatEvents.RaiseNotification(msg);
            OnChanged?.Invoke();
        }

        // ---- persistence ----
        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.gold = Gold;
            s.diamonds = Diamonds;
            s.vipLevel = VipLevel;
            s.vipExp = VipExp;
            s.lifetimeGold = LifetimeGoldEarned;
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            Gold = s.gold;
            Diamonds = s.diamonds;
            VipLevel = s.vipLevel;
            VipExp = s.vipExp;
            LifetimeGoldEarned = s.lifetimeGold;
        }
    }

    // ====================================================================
    // PLAYER PROFILE — name, id, level, titles.
    // ====================================================================
    public static class PlayerProfile
    {
        public static string Name { get; private set; } = "Legend";
        public static string PlayerId { get; private set; } = "";
        public static int Level { get; private set; } = 1;
        public static float Exp { get; private set; }
        public static string ActiveTitle { get; private set; } = "";
        public static List<string> Titles { get; } = new List<string> { "The Awakened" };

        public static event Action OnChanged;

        public static float ExpNeeded(int level) => GameConfig.ExpBase * Mathf.Pow(level, GameConfig.ExpGrowth);

        public static void GrantExp(float amount)
        {
            amount *= Economy.ExpBonus;
            Exp += amount;
            bool leveled = false;
            while (Level < 240 && Exp >= ExpNeeded(Level))
            {
                Exp -= ExpNeeded(Level);
                Level++;
                leveled = true;
                Economy.AddDiamonds(50, "Level " + Level);
            }
            if (leveled)
            {
                Combat.CombatEvents.RaiseNotification("Player Level " + Level + "!");
                Audio.AudioSystem.Play("LevelUp");
                QuestSystem.ReportProgress("player_level", 0);
                OnChanged?.Invoke();
            }
        }

        public static bool Rename(string newName)
        {
            newName = (newName ?? "").Trim();
            if (newName.Length < 2 || newName.Length > 14) return false;
            Name = newName;
            OnChanged?.Invoke();
            return true;
        }

        public static void UnlockTitle(string title)
        {
            if (!Titles.Contains(title))
            {
                Titles.Add(title);
                Combat.CombatEvents.RaiseNotification("Title unlocked: " + title);
            }
        }

        public static void SetTitle(string title)
        {
            ActiveTitle = Titles.Contains(title) ? title : "";
            OnChanged?.Invoke();
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.playerName = Name;
            s.playerId = PlayerId;
            s.playerLevel = Level;
            s.exp = Exp;
            s.titles = Titles;
            s.activeTitle = ActiveTitle;
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            Name = string.IsNullOrEmpty(s.playerName) ? "Legend" : s.playerName;
            PlayerId = string.IsNullOrEmpty(s.playerId) ? GenerateId() : s.playerId;
            Level = Mathf.Max(1, s.playerLevel);
            Exp = Mathf.Max(0f, s.exp);
            ActiveTitle = s.activeTitle ?? "";
            Titles.Clear();
            if (s.titles != null && s.titles.Count > 0) Titles.AddRange(s.titles);
            else Titles.Add("The Awakened");
            if (!Titles.Contains("The Awakened")) Titles.Insert(0, "The Awakened");
        }

        private static string GenerateId()
        {
            // stable per-install id
            long ticks = DateTime.UtcNow.Ticks;
            return "LGD-" + (ticks % 100000000L).ToString("D8");
        }
    }

    // ====================================================================
    // HERO PROGRESS — per-hero level / shards / stars (ascension) / power.
    // ====================================================================
    public static class HeroProgress
    {
        public class HeroRecord
        {
            public string heroId;
            public int level = 1;
            public int shards = 0;
            public int stars = 1;      // 1..6 ascension
            public int dupesPulled = 0;
        }

        private static readonly Dictionary<string, HeroRecord> Records = new Dictionary<string, HeroRecord>();

        public static event Action OnChanged;

        public static int MaxStar => 6;
        public static int LevelCap => 240;

        public static HeroRecord Get(string heroId)
        {
            HeroRecord r;
            if (!Records.TryGetValue(heroId, out r))
            {
                r = new HeroRecord { heroId = heroId };
                Records[heroId] = r;
            }
            return r;
        }

        public static int LevelOf(string heroId) { var r = Get(heroId); return r.level; }
        public static int StarsOf(string heroId) { var r = Get(heroId); return r.stars; }
        public static int ShardsOf(string heroId) { var r = Get(heroId); return r.shards; }

        public static void SetLevel(string heroId, int level)
        {
            Get(heroId).level = Mathf.Clamp(level, 1, LevelCap);
            OnChanged?.Invoke();
        }

        public static void AddShards(string heroId, int amount)
        {
            var r = Get(heroId);
            r.shards += amount;
            OnChanged?.Invoke();
        }

        /// <summary>Gold cost to level a hero once, scaled by current level and stars.</summary>
        public static long LevelUpCost(HeroData hero)
        {
            var r = Get(hero.heroId);
            return (long)(80 + r.level * 42f * (1f + 0.15f * (r.stars - 1)) * RarityCostMult(hero.rarity));
        }

        public static float RarityCostMult(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common: return 0.8f;
                case Rarity.Rare: return 1.0f;
                case Rarity.Epic: return 1.25f;
                case Rarity.Legendary: return 1.5f;
                default: return 1.8f;
            }
        }

        public static bool TryLevelUp(HeroData hero)
        {
            var r = Get(hero.heroId);
            if (r.level >= LevelCap) return false;
            long cost = LevelUpCost(hero);
            if (!Economy.TrySpendGold(cost)) return false;
            r.level++;
            QuestSystem.ReportProgress("hero_upgrade", 1);
            Audio.AudioSystem.Play("LevelUp");
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>Shards needed for the next star (2,4,8,16,32) plus gold.</summary>
        public static int ShardsNeededForNextStar(int currentStar)
        {
            return (int)Mathf.Pow(2f, currentStar);
        }

        public static long StarUpGoldCost(int currentStar, Rarity rarity)
        {
            return (long)(2000 * Mathf.Pow(2.6f, currentStar - 1) * RarityCostMult(rarity));
        }

        public static bool TryStarUp(HeroData hero)
        {
            var r = Get(hero.heroId);
            if (r.stars >= MaxStar) return false;
            int need = ShardsNeededForNextStar(r.stars);
            if (r.shards < need)
            {
                Combat.CombatEvents.RaiseNotification("Need " + need + " shards (have " + r.shards + ")");
                Audio.AudioSystem.Play("Error");
                return false;
            }
            if (!Economy.TrySpendGold(StarUpGoldCost(r.stars, hero.rarity))) return false;
            r.shards -= need;
            r.stars++;
            QuestSystem.ReportProgress("star_up", 1);
            Combat.CombatEvents.RaiseNotification(hero.displayName + " ascended to " + r.stars + "★!");
            Audio.AudioSystem.Play("Fanfare");
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>Star multiplier applied to every stat.</summary>
        public static float StarMult(int stars)
        {
            return 1f + 0.14f * (stars - 1);   // 1★=1.0 .. 6★=1.7
        }

        /// <summary>Estimated combat power for one hero.</summary>
        public static long PowerOf(HeroData hero)
        {
            var r = Get(hero.heroId);
            float hp = hero.baseHP + hero.hpPerLevel * (r.level - 1);
            float atk = hero.baseAttack + hero.atkPerLevel * (r.level - 1);
            float def = hero.baseDefense + hero.defPerLevel * (r.level - 1);
            float sm = StarMult(r.stars);
            hp *= sm; atk *= sm; def *= sm;
            float rarMult = 1f + 0.06f * (int)hero.rarity;
            return (long)((hp * 0.10f + atk * 2.2f + def * 1.6f) * rarMult);
        }

        public static long TeamPower(List<HeroData> teamHeroes)
        {
            long total = 0;
            if (teamHeroes != null)
            {
                foreach (var h in teamHeroes) total += PowerOf(h);
            }
            total = (long)(total * (1f + TechSystem.PowerBonusPct() + World.BeastSystem.TeamPowerBonusPct()));
            return total;
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.heroRecords.Clear();
            foreach (var kv in Records)
            {
                var hr = new SaveSystem.HeroRecordSave
                {
                    heroId = kv.Value.heroId,
                    level = kv.Value.level,
                    shards = kv.Value.shards,
                    stars = kv.Value.stars,
                    dupes = kv.Value.dupesPulled
                };
                s.heroRecords.Add(hr);
            }
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            Records.Clear();
            if (s.heroRecords == null) return;
            foreach (var hr in s.heroRecords)
            {
                if (hr == null || string.IsNullOrEmpty(hr.heroId)) continue;
                Records[hr.heroId] = new HeroRecord
                {
                    heroId = hr.heroId,
                    level = Mathf.Max(1, hr.level),
                    shards = Mathf.Max(0, hr.shards),
                    stars = Mathf.Clamp(hr.stars, 1, MaxStar),
                    dupesPulled = Mathf.Max(0, hr.dupes)
                };
            }
        }
    }

    // ====================================================================
    // AFK REWARDS — idle income tied to campaign progress, offline accrual.
    // ====================================================================
    public static class AFKReward
    {
        public const float CapHours = 12f;

        private static DateTime _lastClaimUtc = DateTime.UtcNow;

        /// <summary>Gold per hour, driven by campaign progress.</summary>
        public static long GoldPerHour => (long)(220 + World.CampaignSystem.ClearedStageCount() * 65f * Economy.GoldBonus);
        public static long ExpPerHour => (long)(90 + World.CampaignSystem.ClearedStageCount() * 22f);
        public static long DiamondPerHour => 6;

        public static event Action OnChanged;

        public static void Tick()
        {
            // accrual is virtual — everything computed from timestamps
            OnChanged?.Invoke();
        }

        public static (long gold, long exp, long diamonds) Pending()
        {
            double hours = (DateTime.UtcNow - _lastClaimUtc).TotalHours;
            hours = Math.Min(hours, CapHours);
            return ((long)(GoldPerHour * hours), (long)(ExpPerHour * hours), (long)(DiamondPerHour * hours));
        }

        public static float CapRatio()
        {
            double hours = (DateTime.UtcNow - _lastClaimUtc).TotalHours;
            return Mathf.Clamp01((float)(hours / CapHours));
        }

        public static string PendingTimeString()
        {
            var p = Pending();
            return Economy.Format(p.gold) + " Gold  •  " + Economy.Format(p.exp) + " XP  •  " + p.diamonds + " Diamonds";
        }

        public static (long gold, long exp, long diamonds) Claim()
        {
            var p = Pending();
            _lastClaimUtc = DateTime.UtcNow;
            if (p.gold > 0)
            {
                Economy.AddGold(p.gold, "AFK");
                PlayerProfile.GrantExp(p.exp);
                if (p.diamonds > 0) Economy.AddDiamonds(p.diamonds, "AFK");
                Audio.AudioSystem.Play("Coin");
                QuestSystem.ReportProgress("afk_claim", 1);
            }
            OnChanged?.Invoke();
            return p;
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.afkLastClaimUtc = _lastClaimUtc.Ticks;
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            _lastClaimUtc = s.afkLastClaimUtc > 0 ? new DateTime(s.afkLastClaimUtc, DateTimeKind.Utc) : DateTime.UtcNow;
        }
    }

    // ====================================================================
    // TECH SYSTEM — permanent account-wide upgrades purchased with gold.
    // ====================================================================
    public static class TechSystem
    {
        public static readonly string[] BranchNames = { "Attack", "Defense", "Vitality", "Precision", "Prosperity", "Wisdom" };
        public static readonly string[] BranchDesc =
        {
            "+1.5% team ATK per level",
            "+1.5% team DEF per level",
            "+1.5% team HP per level",
            "+0.4% crit chance per level",
            "+1% gold gain per level",
            "+1% EXP gain per level"
        };
        public const int MaxLevel = 20;

        private static readonly int[] Levels = new int[6];

        public static event Action OnChanged;

        public static int LevelOf(int branch) => Levels[branch];

        public static long CostOf(int branch, int level)
        {
            return (long)(600 * Mathf.Pow(1.45f, level) * (1f + branch * 0.08f));
        }

        public static bool TryUpgrade(int branch)
        {
            if (branch < 0 || branch >= 6) return false;
            if (Levels[branch] >= MaxLevel) return false;
            if (!Economy.TrySpendGold(CostOf(branch, Levels[branch]))) return false;
            Levels[branch]++;
            Audio.AudioSystem.Play("Fanfare");
            QuestSystem.ReportProgress("tech_upgrade", 1);
            OnChanged?.Invoke();
            return true;
        }

        public static float AttackBonusPct => Levels[0] * 0.015f;
        public static float DefenseBonusPct => Levels[1] * 0.015f;
        public static float HpBonusPct => Levels[2] * 0.015f;
        public static float CritBonus => Levels[3] * 0.004f;
        public static float GoldBonus => Levels[4] * 0.01f;
        public static float ExpBonus => Levels[5] * 0.01f;

        public static float PowerBonusPct()
        {
            return AttackBonusPct * 0.5f + DefenseBonusPct * 0.25f + HpBonusPct * 0.25f;
        }

        /// <summary>Applies all tech bonuses to one hero's stats.</summary>
        public static void ApplyToHero(CharacterStats stats)
        {
            stats.AddEquipmentBonus(new Characters.StatBonus
            {
                hp = stats.MaxHP * HpBonusPct,
                attack = stats.Attack * AttackBonusPct,
                defense = stats.Defense * DefenseBonusPct,
                moveSpeed = 0f
            });
            stats.CritChance += CritBonus;
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.techLevels = new List<int>(Levels);
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            if (s.techLevels == null || s.techLevels.Count < 6)
            {
                for (int i = 0; i < 6; i++) Levels[i] = 0;
                return;
            }
            for (int i = 0; i < 6; i++) Levels[i] = Mathf.Clamp(s.techLevels[i], 0, MaxLevel);
        }
    }

    // ====================================================================
    // EQUIPMENT FORGE — upgrade owned items, +12% stats per forge level.
    // ====================================================================
    public static class ForgeSystem
    {
        public const int MaxForgeLevel = 15;
        private static readonly Dictionary<string, int> Levels = new Dictionary<string, int>();

        public static event Action OnChanged;

        public static int LevelOf(string itemId)
        {
            int lv;
            return Levels.TryGetValue(itemId, out lv) ? lv : 0;
        }

        public static float StatMult(string itemId)
        {
            return 1f + 0.12f * LevelOf(itemId);
        }

        public static long CostOf(string itemId, int nextLevel)
        {
            var item = Items.ItemDatabase.Get(itemId);
            int rarity = item != null ? item.RarityIndex : 0;
            return (long)(400 * Mathf.Pow(1.5f, nextLevel) * (1f + rarity * 0.5f));
        }

        public static bool TryUpgrade(string itemId)
        {
            int cur = LevelOf(itemId);
            if (cur >= MaxForgeLevel) return false;
            if (!Economy.TrySpendGold(CostOf(itemId, cur + 1))) return false;
            Levels[itemId] = cur + 1;
            Audio.AudioSystem.Play("Equip");
            QuestSystem.ReportProgress("forge_upgrade", 1);
            OnChanged?.Invoke();
            return true;
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.forgeLevels.Clear();
            foreach (var kv in Levels)
            {
                s.forgeLevels.Add(new SaveSystem.ForgeRecordSave { itemId = kv.Key, level = kv.Value });
            }
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            Levels.Clear();
            if (s.forgeLevels == null) return;
            foreach (var f in s.forgeLevels)
            {
                if (f != null && !string.IsNullOrEmpty(f.itemId)) Levels[f.itemId] = Mathf.Clamp(f.level, 0, MaxForgeLevel);
            }
        }
    }
}
