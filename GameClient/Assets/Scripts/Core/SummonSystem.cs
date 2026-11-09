using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;

namespace LegendsRPG.Core
{
    // ====================================================================
    // SUMMON SYSTEM — gacha with pity, dupes→shards, single & 10x pulls.
    // ====================================================================
    public static class SummonSystem
    {
        public const int CostSingle = 280;
        public const int CostTen = 2700;      // 10x discount (~10% off)

        // rates (percent)
        private const float PCommon = 44.0f;
        private const float PRare = 29.5f;
        private const float PEpic = 17.5f;
        private const float PLegendary = 7.5f;
        private const float PMythic = 1.5f;

        private static int _pullsSinceEpic = 0;    // pity: every 10th pull guarantees Epic+
        private static int _pullsSinceLegendary = 0; // pity: 90 pulls → guaranteed Legendary
        public static int TotalPulls { get; private set; }

        public static event Action OnChanged;

        public static int PullsSinceEpic => _pullsSinceEpic;
        public static int PullsSinceLegendary => _pullsSinceLegendary;
        public static int LegendaryPityLeft => Mathf.Max(0, 90 - _pullsSinceLegendary);
        public static int EpicPityLeft => Mathf.Max(0, 10 - (_pullsSinceEpic % 10));

        public class PullResult
        {
            public HeroData hero;
            public bool isNew;
            public int shardsGranted;
        }

        private static List<HeroData> _roster;

        private static List<HeroData> Roster()
        {
            if (_roster == null) _roster = HeroDatabase.CreateDefaultRoster();
            return _roster;
        }

        public static Rarity RollRarity(bool forceLegendary, bool forceEpicPlus)
        {
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (forceLegendary) return Rarity.Legendary;
            if (forceEpicPlus) return roll < PMythic * 2 ? Rarity.Mythic : (roll < 50f ? Rarity.Epic : Rarity.Legendary);

            if (roll < PMythic) return Rarity.Mythic;
            if (roll < PMythic + PLegendary) return Rarity.Legendary;
            if (roll < PMythic + PLegendary + PEpic) return Rarity.Epic;
            if (roll < PMythic + PLegendary + PEpic + PRare) return Rarity.Rare;
            return Rarity.Common;
        }

        private static HeroData RandomOfRarity(Rarity rarity)
        {
            var pool = Roster().FindAll(h => h.rarity == rarity);
            if (pool.Count == 0) pool = Roster();
            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }

        /// <summary>Single pull. Returns null if diamonds insufficient.</summary>
        public static PullResult PullOnce()
        {
            if (!Economy.TrySpendDiamonds(CostSingle)) return null;
            return ExecutePull();
        }

        /// <summary>10x pull with guaranteed Epic+ on the 10th. Returns null if diamonds insufficient.</summary>
        public static List<PullResult> PullTen()
        {
            if (!Economy.TrySpendDiamonds(CostTen)) return null;
            var results = new List<PullResult>();
            for (int i = 0; i < 10; i++)
            {
                bool isTenth = (i == 9);
                // ensure the 10th is Epic+ at minimum
                PullResult r;
                if (isTenth)
                {
                    int preEpic = _pullsSinceEpic;
                    r = ExecutePullRaw(RollRarity(false, true), true);
                    _pullsSinceEpic = preEpic + 1;
                }
                else
                {
                    r = ExecutePull();
                }
                results.Add(r);
            }
            TotalPulls += 10;
            QuestSystem.ReportProgress("summon", 10);
            OnChanged?.Invoke();
            return results;
        }

        private static PullResult ExecutePull()
        {
            _pullsSinceEpic++;
            _pullsSinceLegendary++;

            bool forceLegendary = _pullsSinceLegendary >= 90;
            bool forceEpicPlus = _pullsSinceEpic % 10 == 0;

            Rarity rarity = RollRarity(forceLegendary, forceLegendary || forceEpicPlus);
            if (rarity == Rarity.Legendary || rarity == Rarity.Mythic) _pullsSinceLegendary = 0;

            var r = ExecutePullRaw(rarity, false);
            TotalPulls++;
            QuestSystem.ReportProgress("summon", 1);
            OnChanged?.Invoke();
            return r;
        }

        private static PullResult ExecutePullRaw(Rarity rarity, bool skipCounters)
        {
            var hero = RandomOfRarity(rarity);
            bool owned = SaveSystem.Current.unlockedHeroIds.Contains(hero.heroId);
            int shards = owned ? 10 : 0;

            if (!owned)
            {
                UnlockHeroDirect(hero.heroId, "Summon");
            }
            else
            {
                HeroProgress.AddShards(hero.heroId, shards);
                HeroProgress.Get(hero.heroId).dupesPulled++;
            }

            return new PullResult { hero = hero, isNew = !owned, shardsGranted = shards };
        }

        /// <summary>Direct unlock (sign-in, events, quest rewards).</summary>
        public static void UnlockHeroDirect(string heroId, string source)
        {
            if (SaveSystem.Current.unlockedHeroIds.Contains(heroId)) return;
            SaveSystem.Current.unlockedHeroIds.Add(heroId);

            var roster = Roster();
            HeroData data = null;
            foreach (var h in roster) if (h.heroId == heroId) { data = h; break; }

            Combat.CombatEvents.RaiseNotification("NEW HERO: " + (data != null ? data.displayName : heroId));
            Audio.AudioSystem.Play("Fanfare");
            OnChanged?.Invoke();
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.pullsWithoutEpic = _pullsSinceEpic;
            s.pullsWithoutLegendary = _pullsSinceLegendary;
            s.totalPulls = TotalPulls;
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            _pullsSinceEpic = Mathf.Max(0, s.pullsWithoutEpic);
            _pullsSinceLegendary = Mathf.Max(0, s.pullsWithoutLegendary);
            TotalPulls = Mathf.Max(0, s.totalPulls);
        }
    }
}
