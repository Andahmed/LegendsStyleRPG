using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Combat;
using LegendsRPG.Characters;

namespace LegendsRPG.Core
{
    // =====================================================================
    //  Items (original content - names, colours and abbreviations are ours)
    // =====================================================================
    public enum ItemTab { Items = 0, Shards = 1, Equip = 2 }

    public class ItemDef
    {
        public string id, name, desc, abbr;
        public ItemTab tab;
        public int rarity;           // 0 common .. 4 mythic
        public Color color;
        // equipment only
        public int slot = -1;        // 0 weapon, 1 helmet, 2 armor, 3 boots
        public float hp, atk, def, spd;
    }

    public static class ItemDatabase
    {
        private static readonly Dictionary<string, ItemDef> _all = new Dictionary<string, ItemDef>();
        public static readonly List<ItemDef> List = new List<ItemDef>();

        static ItemDatabase()
        {
            Add("mystic_chest", "Mystic Chest", "Open to receive random rewards: gold, gems or materials.", "MC", ItemTab.Items, 2, new Color(0.85f, 0.55f, 0.2f));
            Add("exp_elixir", "EXP Elixir", "Grants 250 EXP to every living hero.", "XP", ItemTab.Items, 1, new Color(0.3f, 0.7f, 1f));
            Add("ember_core", "Ember Core", "A smouldering core used in advanced crafting.", "EC", ItemTab.Items, 3, new Color(1f, 0.4f, 0.15f));
            Add("forge_ember", "Forge Ember", "Fuel for the forge. Needed to craft equipment.", "FE", ItemTab.Items, 2, new Color(1f, 0.6f, 0.2f));
            Add("ascend_stone", "Ascension Stone", "Used to break through a hero's level cap.", "AS", ItemTab.Items, 3, new Color(0.6f, 0.4f, 0.95f));
            Add("skill_scroll", "Skill Scroll", "Upgrades hero skills.", "SS", ItemTab.Items, 2, new Color(0.9f, 0.85f, 0.5f));
            Add("shard_kael", "Kael Shard", "Collect to strengthen Kael.", "Ka", ItemTab.Shards, 2, new Color(0.9f, 0.4f, 0.15f));
            Add("shard_sylra", "Sylra Shard", "Collect to strengthen Sylra.", "Sy", ItemTab.Shards, 3, new Color(0.35f, 0.75f, 1f));
            Add("shard_rania", "Rania Shard", "Collect to strengthen Rania.", "Ra", ItemTab.Shards, 2, new Color(0.95f, 0.85f, 0.3f));
            Add("shard_doran", "Doran Shard", "Collect to strengthen Doran.", "Do", ItemTab.Shards, 1, new Color(0.95f, 0.9f, 0.65f));
            Add("shard_nyx", "Nyx Shard", "Collect to strengthen Nyx.", "Nx", ItemTab.Shards, 3, new Color(0.6f, 0.3f, 0.9f));
            Add("shard_verda", "Verda Shard", "Collect to strengthen Verda.", "Ve", ItemTab.Shards, 2, new Color(0.35f, 0.8f, 0.4f));
            //            id            name            slot rarity color                                   abbr  hp  atk def spd
            AddEquip("iron_blade",   "Iron Blade",    0, 1, new Color(0.7f, 0.75f, 0.8f),  "IB",  0f,  6f, 0f, 0f);
            AddEquip("oak_staff",    "Oak Staff",     0, 1, new Color(0.5f, 0.7f, 0.35f),  "OS", 10f,  5f, 0f, 0f);
            AddEquip("war_axe",      "War Axe",       0, 3, new Color(0.85f, 0.4f, 0.25f), "WA",  0f, 14f, 0f, 0f);
            AddEquip("steel_helm",   "Steel Helm",    1, 2, new Color(0.65f, 0.7f, 0.8f),  "SH", 30f,  0f, 4f, 0f);
            AddEquip("leather_vest", "Leather Vest",  2, 1, new Color(0.65f, 0.45f, 0.3f), "LV", 40f,  0f, 3f, 0f);
            AddEquip("knight_plate", "Knight Plate",  2, 3, new Color(0.75f, 0.78f, 0.9f), "KP", 90f,  0f, 9f, 0f);
            AddEquip("swift_boots",  "Swift Boots",   3, 2, new Color(0.4f, 0.6f, 0.9f),   "SB", 15f,  0f, 1f, 0.4f);
            Add("hero_essence", "Hero Essence", "Condensed hero soul. Duplicate heroes convert into Essence for crafting and ascension.", "HE", ItemTab.Shards, 3, new Color(0.65f, 0.4f, 0.95f));
        }

        private static void Add(string id, string name, string desc, string abbr, ItemTab tab, int rarity, Color c)
        {
            var d = new ItemDef { id = id, name = name, desc = desc, abbr = abbr, tab = tab, rarity = rarity, color = c };
            _all[id] = d;
            List.Add(d);
        }

        public static readonly string[] SlotNames = { "Weapon", "Helmet", "Armor", "Boots" };

        private static void AddEquip(string id, string name, int slot, int rarity, Color c, string abbr,
            float hp, float atk, float def, float spd)
        {
            string stats = "";
            if (atk > 0f) stats += "  ATK +" + atk;
            if (hp > 0f) stats += "  HP +" + hp;
            if (def > 0f) stats += "  DEF +" + def;
            if (spd > 0f) stats += "  SPD +" + spd;
            Add(id, name, SlotNames[slot] + "." + stats, abbr, ItemTab.Equip, rarity, c);
            var d = Get(id);
            d.slot = slot; d.hp = hp; d.atk = atk; d.def = def; d.spd = spd;
        }

        public static ItemDef Get(string id)
        {
            ItemDef d;
            return _all.TryGetValue(id, out d) ? d : null;
        }

        public static Color RarityColor(int rarity)
        {
            switch (rarity)
            {
                case 0: return new Color(0.7f, 0.7f, 0.72f);
                case 1: return new Color(0.35f, 0.8f, 0.4f);
                case 2: return new Color(0.3f, 0.6f, 1f);
                case 3: return new Color(0.7f, 0.4f, 0.95f);
                default: return new Color(1f, 0.75f, 0.2f);
            }
        }
    }

    // =====================================================================
    //  Inventory / currencies (stored in SaveSystem.Current)
    // =====================================================================
    public static class Inventory
    {
        public static event Action Changed;

        public static int Gold { get { return (int)System.Math.Min(SaveSystem.Current.gold, (long)int.MaxValue); } }
        public static int Gems { get { return SaveSystem.Current.gems; } }

        public static void MigrateAndSeed()
        {
            var s = SaveSystem.Current;
            if (s.itemIds == null) s.itemIds = new List<string>();
            if (s.itemCounts == null) s.itemCounts = new List<int>();
            if (s.itemIds.Count == 0 && s.gold == 0 && s.gems == 0)
            {
                // starter pack (first run, or an old v1 save)
                // gems cover a full Advanced x10 summon right away (v0.4 loop)
                s.gold = 5000;
                s.gems = 3200;
                s.itemIds.AddRange(new[] { "mystic_chest", "exp_elixir", "forge_ember", "ember_core", "skill_scroll", "iron_blade", "oak_staff", "steel_helm", "leather_vest", "swift_boots" });
                s.itemCounts.AddRange(new[] { 3, 10, 5, 4, 6, 1, 1, 1, 1, 1 });
            }
            s.schemaVersion = 3;
            EnsureToday();
        }

        public static void EnsureToday()
        {
            var s = SaveSystem.Current;
            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (s.dayKey == today) return;
            s.dayKey = today;
            s.missionProgress.Clear();
            s.missionClaimed.Clear();
            s.shopKeys.Clear();
            s.shopCounts.Clear();
            DailyMissions.Add(DailyMissions.Login, 1);
        }

        /// <summary>Seconds until the daily reset (UTC midnight).</summary>
        public static double SecondsToReset()
        {
            var now = DateTime.UtcNow;
            return (now.Date.AddDays(1) - now).TotalSeconds;
        }

        private static void Raise() { if (Changed != null) Changed(); }

        public static void AddGold(int n) { SaveSystem.Current.gold += n; Raise(); }
        public static void AddGems(int n) { SaveSystem.Current.gems += n; Raise(); }

        public static bool SpendGold(int n)
        {
            if (SaveSystem.Current.gold < n) return false;
            SaveSystem.Current.gold -= n;
            Raise();
            return true;
        }

        // ---- per-hero progress ----
        public static int HeroLevel(string heroId)
        {
            int i = SaveSystem.Current.levelHeroIds.IndexOf(heroId);
            return i >= 0 ? Mathf.Max(1, SaveSystem.Current.levelValues[i]) : Mathf.Max(1, SaveSystem.Current.playerLevel);
        }

        public static void SetHeroLevel(string heroId, int level)
        {
            var sv = SaveSystem.Current;
            int i = sv.levelHeroIds.IndexOf(heroId);
            if (i >= 0) sv.levelValues[i] = level;
            else { sv.levelHeroIds.Add(heroId); sv.levelValues.Add(level); }
        }

        public static bool SpendGems(int n)
        {
            if (SaveSystem.Current.gems < n) return false;
            SaveSystem.Current.gems -= n;
            Raise();
            return true;
        }

        public static int Count(string id)
        {
            var s = SaveSystem.Current;
            int i = s.itemIds.IndexOf(id);
            return i >= 0 ? s.itemCounts[i] : 0;
        }

        public static void AddItem(string id, int n)
        {
            var s = SaveSystem.Current;
            int i = s.itemIds.IndexOf(id);
            if (i >= 0) s.itemCounts[i] += n;
            else { s.itemIds.Add(id); s.itemCounts.Add(n); }
            Raise();
        }

        public static bool ConsumeItem(string id, int n)
        {
            var s = SaveSystem.Current;
            int i = s.itemIds.IndexOf(id);
            if (i < 0 || s.itemCounts[i] < n) return false;
            s.itemCounts[i] -= n;
            if (s.itemCounts[i] <= 0) { s.itemIds.RemoveAt(i); s.itemCounts.RemoveAt(i); }
            Raise();
            return true;
        }

        /// <summary>Applies the effect of a usable item. Returns a message for the UI, or null if not usable.</summary>
        public static string Use(string id, Team.TeamManager team)
        {
            if (Count(id) <= 0) return null;
            switch (id)
            {
                case "exp_elixir":
                    if (team == null) return null;
                    ConsumeItem(id, 1);
                    team.ShareEXP(250f * Mathf.Max(1, team.Heroes.Count));
                    return "+250 EXP to each hero";
                case "mystic_chest":
                    ConsumeItem(id, 1);
                    return OpenChest();
            }
            return null;
        }

        private static string OpenChest()
        {
            int roll = UnityEngine.Random.Range(0, 4);
            if (roll == 0) { int g = UnityEngine.Random.Range(800, 2500); AddGold(g); return "+" + g + " Gold"; }
            if (roll == 1) { int g = UnityEngine.Random.Range(20, 80); AddGems(g); return "+" + g + " Gems"; }
            var pool = new[] { "ember_core", "forge_ember", "ascend_stone", "skill_scroll", "shard_kael", "shard_sylra", "shard_rania", "shard_doran", "shard_nyx", "shard_verda", "iron_blade", "steel_helm", "leather_vest", "war_axe", "knight_plate" };
            string pick = pool[UnityEngine.Random.Range(0, pool.Length)];
            int n = UnityEngine.Random.Range(1, 4);
            AddItem(pick, n);
            return "+" + n + " " + ItemDatabase.Get(pick).name;
        }
    }

    // =====================================================================
    //  Daily missions
    // =====================================================================
    public class MissionDef
    {
        public string title;
        public int target;
        public int rewardGems;
        public int rewardGold;
    }

    public static class DailyMissions
    {
        public const int Login = 0, ClearFloors = 1, KillMonsters = 2, SwitchHeroes = 3;

        public static readonly MissionDef[] All =
        {
            new MissionDef { title = "Log in",                    target = 1,  rewardGems = 10, rewardGold = 2000 },
            new MissionDef { title = "Clear 3 tower floors",      target = 3,  rewardGems = 20, rewardGold = 3000 },
            new MissionDef { title = "Defeat 30 monsters",        target = 30, rewardGems = 20, rewardGold = 3000 },
            new MissionDef { title = "Switch heroes 5 times",     target = 5,  rewardGems = 10, rewardGold = 1500 },
        };

        private static bool _hooked;

        public static void Hook(Team.TeamManager team)
        {
            if (_hooked) return;
            _hooked = true;
            CombatEvents.CharacterDied += go =>
            {
                if (go != null && go.GetComponent<MonsterCharacter>() != null) Add(KillMonsters, 1);
            };
        }

        private static void Fit()
        {
            var s = SaveSystem.Current;
            while (s.missionProgress.Count < All.Length) s.missionProgress.Add(0);
            while (s.missionClaimed.Count < All.Length) s.missionClaimed.Add(0);
        }

        public static void Add(int index, int n)
        {
            Fit();
            var s = SaveSystem.Current;
            s.missionProgress[index] = Mathf.Min(All[index].target, s.missionProgress[index] + n);
        }

        public static int Progress(int index) { Fit(); return SaveSystem.Current.missionProgress[index]; }
        public static bool Claimed(int index) { Fit(); return SaveSystem.Current.missionClaimed[index] == 1; }
        public static bool CanClaim(int index) { return !Claimed(index) && Progress(index) >= All[index].target; }

        public static bool Claim(int index)
        {
            if (!CanClaim(index)) return false;
            SaveSystem.Current.missionClaimed[index] = 1;
            Inventory.AddGems(All[index].rewardGems);
            Inventory.AddGold(All[index].rewardGold);
            return true;
        }
    }

    // =====================================================================
    //  Daily shop
    // =====================================================================
    public class ShopOffer
    {
        public string itemId;
        public int qty, priceGems, discountPct, dailyLimit;
        public string Key { get { return itemId + "_" + qty + "_" + priceGems; } }
    }

    public static class DailyShop
    {
        public static readonly ShopOffer[] Offers =
        {
            new ShopOffer { itemId = "mystic_chest",  qty = 1,  priceGems = 30,  discountPct = 90, dailyLimit = 1 },
            new ShopOffer { itemId = "exp_elixir",    qty = 5,  priceGems = 120, discountPct = 50, dailyLimit = 2 },
            new ShopOffer { itemId = "ember_core",    qty = 3,  priceGems = 200, discountPct = 40, dailyLimit = 2 },
            new ShopOffer { itemId = "ascend_stone",  qty = 2,  priceGems = 250, discountPct = 50, dailyLimit = 1 },
            new ShopOffer { itemId = "shard_sylra",   qty = 5,  priceGems = 150, discountPct = 25, dailyLimit = 5 },
            new ShopOffer { itemId = "forge_ember",   qty = 1,  priceGems = 80,  discountPct = 20, dailyLimit = 5 },
            new ShopOffer { itemId = "skill_scroll",  qty = 3,  priceGems = 100, discountPct = 20, dailyLimit = 3 },
            new ShopOffer { itemId = "shard_nyx",     qty = 5,  priceGems = 180, discountPct = 20, dailyLimit = 3 },
        };

        public static int Bought(ShopOffer o)
        {
            int i = SaveSystem.Current.shopKeys.IndexOf(o.Key);
            return i >= 0 ? SaveSystem.Current.shopCounts[i] : 0;
        }

        public static bool Buy(ShopOffer o, out string error)
        {
            error = null;
            if (Bought(o) >= o.dailyLimit) { error = "Daily limit reached"; return false; }
            if (!Inventory.SpendGems(o.priceGems)) { error = "Not enough gems"; return false; }
            Inventory.AddItem(o.itemId, o.qty);
            var s = SaveSystem.Current;
            int i = s.shopKeys.IndexOf(o.Key);
            if (i >= 0) s.shopCounts[i]++;
            else { s.shopKeys.Add(o.Key); s.shopCounts.Add(1); }
            return true;
        }
    }

    // =====================================================================
    //  Equipment (4 slots per hero, persisted in the save)
    // =====================================================================
    public static class Equipment
    {
        public static string Get(string heroId, int slot)
        {
            var s = SaveSystem.Current;
            for (int i = 0; i < s.equipHero.Count; i++)
                if (s.equipHero[i] == heroId && s.equipSlot[i] == slot) return s.equipItem[i];
            return null;
        }

        private static void Remove(string heroId, int slot)
        {
            var s = SaveSystem.Current;
            for (int i = 0; i < s.equipHero.Count; i++)
            {
                if (s.equipHero[i] == heroId && s.equipSlot[i] == slot)
                {
                    s.equipHero.RemoveAt(i); s.equipSlot.RemoveAt(i); s.equipItem.RemoveAt(i);
                    return;
                }
            }
        }

        public static void Equip(string heroId, int slot, string itemId)
        {
            var def = ItemDatabase.Get(itemId);
            if (def == null || def.slot != slot) return;
            if (!Inventory.ConsumeItem(itemId, 1)) return;
            string old = Get(heroId, slot);
            if (old != null) { Remove(heroId, slot); Inventory.AddItem(old, 1); }
            var s = SaveSystem.Current;
            s.equipHero.Add(heroId); s.equipSlot.Add(slot); s.equipItem.Add(itemId);
        }

        public static void Unequip(string heroId, int slot)
        {
            string old = Get(heroId, slot);
            if (old == null) return;
            Remove(heroId, slot);
            Inventory.AddItem(old, 1);
        }

        /// <summary>Rebuilds the hero's equipment bonuses from the save.</summary>
        public static void Apply(HeroCharacter h)
        {
            if (h == null || h.Stats == null) return;
            h.Stats.EquipmentBonuses.Clear();
            h.Stats.Recalculate();
            for (int slot = 0; slot < 4; slot++)
            {
                string id = Get(h.Data.heroId, slot);
                var def = id != null ? ItemDatabase.Get(id) : null;
                if (def == null) continue;
                h.Stats.AddEquipmentBonus(new StatBonus { hp = def.hp, attack = def.atk, defense = def.def, moveSpeed = def.spd });
            }
            if (h.HP != null)
            {
                h.HP.Max = h.Stats.MaxHP;
                h.HP.Current = Mathf.Min(h.HP.Current, h.HP.Max);
            }
        }
    }
}
