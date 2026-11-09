using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;

namespace LegendsRPG.Core
{
    public enum BannerId { Basic = 0, Advanced = 1, Supreme = 2 }

    /// <summary>One summon outcome: a hero plus what the player received.</summary>
    public class GachaResult
    {
        public HeroData hero;
        public bool isNew;
        public int essence;    // duplicate -> Hero Essence
        public int goldBack;   // duplicate -> gold refund
        public bool pityHit;   // guaranteed Legendary/Mythic landed here
    }

    /// <summary>
    /// Hero summoning (v0.4). Three banners in the style of the reference game:
    ///   Basic    — gold currency, cheap, Rare floor on x10
    ///   Advanced — gems, the main hero source, Epic floor on x10, Legendary pity 30
    ///   Supreme  — premium gems, boosted Legendary/Mythic, Legendary pity 20
    /// Duplicates convert into Hero Essence + gold. The Faction Wish (unlocked
    /// after 60 total pulls) forces the next guaranteed Legendary onto the
    /// wished element when such a hero exists in the pool.
    /// All state lives in SaveSystem.Current (schema 3) and is saved after
    /// every pull batch, exactly like the rest of the economy.
    /// </summary>
    public static class GachaSystem
    {
        public class Banner
        {
            public string name;
            public string sub;
            public Color theme;
            public int costGold1, costGold10;
            public int costGems1, costGems10;
            public float rare, epic, legendary, mythic;   // Common = the remainder
            public int pityLegendary;                     // hard pity pulls
            public Rarity floorRarity;                    // x10 guarantee
        }

        public static readonly Banner[] Banners =
        {
            // ---------- Basic (gold) ----------
            new Banner
            {
                name = "BASIC SUMMON",
                sub = "Gold banner — a cheap way to grow the roster",
                theme = new Color(0.30f, 0.55f, 0.90f),
                costGold1 = 500, costGold10 = 4500, costGems1 = 60, costGems10 = 540,
                rare = 0.30f, epic = 0.12f, legendary = 0.03f, mythic = 0f,
                pityLegendary = 20, floorRarity = Rarity.Rare
            },
            // ---------- Advanced (gems) ----------
            new Banner
            {
                name = "ADVANCED SUMMON",
                sub = "Gem banner — the main hero source",
                theme = new Color(0.90f, 0.72f, 0.20f),
                costGold1 = 0, costGold10 = 0, costGems1 = 300, costGems10 = 2700,
                rare = 0.46f, epic = 0.40f, legendary = 0.12f, mythic = 0.02f,
                pityLegendary = 30, floorRarity = Rarity.Epic
            },
            // ---------- Supreme (premium gems) ----------
            new Banner
            {
                name = "SUPREME SUMMON",
                sub = "Premium banner — boosted Legendary & Mythic",
                theme = new Color(0.85f, 0.25f, 0.22f),
                costGold1 = 0, costGold10 = 0, costGems1 = 800, costGems10 = 7200,
                rare = 0.25f, epic = 0.45f, legendary = 0.25f, mythic = 0.05f,
                pityLegendary = 20, floorRarity = Rarity.Epic
            },
        };

        /// <summary>Total pulls needed to unlock the Faction Wish.</summary>
        public const int WishUnlockPulls = 60;

        private static void Fit()
        {
            var s = SaveSystem.Current;
            while (s.gachaPity.Count < Banners.Length) s.gachaPity.Add(0);
        }

        public static int Pity(int banner)
        {
            Fit();
            return SaveSystem.Current.gachaPity[banner];
        }

        /// <summary>Pulls left until the guaranteed Legendary on this banner.</summary>
        public static int PullsToLegendary(int banner)
        {
            Fit();
            return Mathf.Max(0, Banners[banner].pityLegendary - SaveSystem.Current.gachaPity[banner]);
        }

        public static bool WishUnlocked
        {
            get { return SaveSystem.Current.gachaTotalPulls >= WishUnlockPulls; }
        }

        public static bool HasWish
        {
            get { return !string.IsNullOrEmpty(SaveSystem.Current.wishElement); }
        }

        public static void SetWish(string elementName)
        {
            if (!WishUnlocked) return;
            SaveSystem.Current.wishElement = elementName;
            SaveSystem.Save(null, null);
        }

        /// <summary>Cost of a pull batch in the banner's own currency.</summary>
        public static int Cost(BannerId id, int count)
        {
            var b = Banners[(int)id];
            bool gold = id == BannerId.Basic;
            return count == 10 ? (gold ? b.costGold10 : b.costGems10)
                               : (gold ? b.costGold1 : b.costGems1) * count;
        }

        public static bool IsGoldBanner(BannerId id) { return id == BannerId.Basic; }

        /// <summary>Rolls <paramref name="count"/> pulls. Returns null + error when unaffordable.</summary>
        public static List<GachaResult> Pull(BannerId bannerId, int count, out string error)
        {
            error = null;
            if (count != 1 && count != 10) { error = "Invalid pull count"; return null; }
            Fit();

            int total = Cost(bannerId, count);
            if (total <= 0) { error = "Invalid banner"; return null; }

            bool gold = IsGoldBanner(bannerId);
            if (gold)
            {
                if (Inventory.Gold < total) { error = "Not enough gold"; return null; }
                Inventory.AddGold(-total);
            }
            else
            {
                if (Inventory.Gems < total) { error = "Not enough gems"; return null; }
                Inventory.SpendGems(total);
            }

            var b = Banners[(int)bannerId];
            var s = SaveSystem.Current;
            var results = new List<GachaResult>();

            for (int i = 0; i < count; i++)
            {
                s.gachaTotalPulls++;
                s.gachaPity[(int)bannerId]++;

                Rarity r = RollRarity(b);
                bool pity = false;
                if ((int)r >= (int)Rarity.Legendary)
                {
                    if (s.gachaPity[(int)bannerId] >= b.pityLegendary) pity = true;
                    s.gachaPity[(int)bannerId] = 0;
                }
                else if (s.gachaPity[(int)bannerId] >= b.pityLegendary)
                {
                    r = Rarity.Legendary;      // hard pity reached
                    pity = true;
                    s.gachaPity[(int)bannerId] = 0;
                }

                var hero = PickHero(r);
                var res = Apply(hero);
                res.pityHit = pity;
                results.Add(res);
            }

            // x10 floor: make sure the batch respects the banner guarantee
            if (count == 10) EnsureFloor(results, b.floorRarity);

            SaveSystem.Save(null, null);
            return results;
        }

        // ---------------- internals ----------------

        private static Rarity RollRarity(Banner b)
        {
            float roll = Random.value;
            if (b.mythic > 0f && roll < b.mythic) return Rarity.Mythic;
            if (roll < b.mythic + b.legendary) return Rarity.Legendary;
            if (roll < b.mythic + b.legendary + b.epic) return Rarity.Epic;
            if (roll < b.mythic + b.legendary + b.epic + b.rare) return Rarity.Rare;
            return Rarity.Common;
        }

        private static HeroData PickHero(Rarity r)
        {
            var roster = HeroDatabase.AllHeroes;

            // Faction Wish: steer guaranteed Legendaries onto the wished element
            var s = SaveSystem.Current;
            if (r == Rarity.Legendary && HasWish)
            {
                var wished = new List<HeroData>();
                for (int i = 0; i < roster.Count; i++)
                {
                    if (roster[i].rarity == r && roster[i].element.ToString() == s.wishElement) wished.Add(roster[i]);
                }
                if (wished.Count > 0) return wished[Random.Range(0, wished.Count)];
            }

            // walk down (then up) until a pool is non-empty
            for (int step = 0; step <= 4; step++)
            {
                int rr = (int)r - step;
                for (int dir = 0; dir < 2; dir++)
                {
                    int target = dir == 0 ? rr : (int)r + step;
                    if (target < 0 || target > 4) continue;
                    var pool = new List<HeroData>();
                    for (int i = 0; i < roster.Count; i++)
                    {
                        if ((int)roster[i].rarity == target) pool.Add(roster[i]);
                    }
                    if (pool.Count > 0) return pool[Random.Range(0, pool.Count)];
                }
            }
            return roster[Random.Range(0, roster.Count)];
        }

        private static GachaResult Apply(HeroData hero)
        {
            var s = SaveSystem.Current;
            var res = new GachaResult { hero = hero, isNew = !s.unlockedHeroIds.Contains(hero.heroId) };

            if (res.isNew)
            {
                s.unlockedHeroIds.Add(hero.heroId);
                // empty team slot? the newcomer joins the battle team immediately
                if (s.teamLoadout.Count < GameConfig.TeamSize) s.teamLoadout.Add(hero.heroId);
            }
            else
            {
                int tier = 1 + (int)hero.rarity;
                res.essence = 10 * tier;
                res.goldBack = 800 * tier;
                Inventory.AddItem("hero_essence", res.essence);
                Inventory.AddGold(res.goldBack);
            }
            return res;
        }

        private static void EnsureFloor(List<GachaResult> results, Rarity floor)
        {
            Rarity best = Rarity.Common;
            int bestIdx = 0;
            for (int i = 0; i < results.Count; i++)
            {
                if ((int)results[i].hero.rarity > (int)best) { best = results[i].hero.rarity; bestIdx = i; }
            }
            if ((int)best >= (int)floor) return;

            // upgrade the worst pull of the batch to the floor rarity
            int worstIdx = 0;
            for (int i = 1; i < results.Count; i++)
            {
                if ((int)results[i].hero.rarity < (int)results[worstIdx].hero.rarity) worstIdx = i;
            }

            var s = SaveSystem.Current;
            var old = results[worstIdx];
            if (!old.isNew)
            {
                // take back the duplicate refund before re-rolling the slot
                Inventory.ConsumeItem("hero_essence", old.essence);
                s.gold = System.Math.Max(0L, s.gold - old.goldBack);
            }
            var upgraded = Apply(PickHero(floor));
            upgraded.pityHit = old.pityHit;
            results[worstIdx] = upgraded;
        }
    }
}
