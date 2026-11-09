using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;

namespace LegendsRPG.World
{
    // ====================================================================
    // CAMPAIGN — 12 chapters × 15 stages ("1-1" ... "12-15") with stars.
    // ====================================================================
    public static class CampaignSystem
    {
        public const int Chapters = 12;
        public const int StagesPerChapter = 15;

        public static readonly string[] ChapterNames =
        {
            "Emberfall Woods", "Tidebringer Coast", "Gloomshade Marsh", "Frostpeak Ascent",
            "Sunken Citadel", "Stormvein Plains", "Ashen Wastes", "Nightfall Hollow",
            "Crystal Spire", "Void Breach", "Celestial Gate", "Throne of Legends"
        };

        public class StageState
        {
            public string key;
            public int stars;
        }

        private static readonly Dictionary<string, int> _stars = new Dictionary<string, int>();

        public static event Action OnChanged;

        public static string Key(int chapter, int stage) => chapter + "-" + stage;

        public static int StarsOf(int chapter, int stage)
        {
            int v;
            return _stars.TryGetValue(Key(chapter, stage), out v) ? v : 0;
        }

        public static bool IsUnlocked(int chapter, int stage)
        {
            if (chapter == 1 && stage == 1) return true;
            if (stage > 1) return StarsOf(chapter, stage - 1) > 0;
            return StarsOf(chapter - 1, StagesPerChapter) > 0;
        }

        public static int HighestChapterUnlocked()
        {
            for (int c = Chapters; c >= 1; c--)
            {
                if (StarsOf(c, 1) > 0 || (c == 1)) return c;
                if (StarsOf(c - 1, StagesPerChapter) > 0) return c;
            }
            return 1;
        }

        public static int ClearedStageCount()
        {
            int n = 0;
            foreach (var kv in _stars) if (kv.Value > 0) n++;
            return n;
        }

        public static int TotalStars()
        {
            int n = 0;
            foreach (var kv in _stars) n += kv.Value;
            return n;
        }

        /// <summary>
        /// Launches a real battle for the stage. Records stars on victory:
        /// 1 star = clear, +1 if no hero died, +1 if team HP above 50%.
        /// </summary>
        public static void StartStage(int chapter, int stage)
        {
            if (!IsUnlocked(chapter, stage))
            {
                CombatEvents.RaiseNotification("Stage locked — clear the previous one!");
                Audio.AudioSystem.Play("Error");
                return;
            }

            int floor = Mathf.Max(1, (chapter - 1) * StagesPerChapter + stage);
            int difficulty = Mathf.Clamp((chapter - 1) / 3, 0, 3);
            bool boss = stage == StagesPerChapter;

            var cfg = new Core.BattleSession.Config
            {
                type = Core.BattleSession.BattleType.Campaign,
                title = chapter + "-" + stage + " " + ChapterNames[Mathf.Clamp(chapter - 1, 0, ChapterNames.Length - 1)],
                floor = floor,
                difficulty = difficulty,
                boss = boss,
                chapter = chapter,
                stage = stage
            };
            cfg.onWin = () => ApplyStageWin(chapter, stage);
            Core.BattleSession.Start(cfg);
        }

        /// <summary>Real stage completion: stars, rewards, first-clear bonuses.</summary>
        public static void ApplyStageWin(int chapter, int stage)
        {
            int floor = Mathf.Max(1, (chapter - 1) * StagesPerChapter + stage);
            var team = Team.TeamManager.InstanceRef;

            int gained = 1;
            if (Core.BattleSession.DeathsThisBattle == 0) gained++;
            float hpRatio = 0f;
            int alive = 0;
            if (team != null)
            {
                foreach (var h in team.Heroes)
                {
                    if (h == null) continue;
                    alive++;
                    hpRatio += h.HP.Ratio;
                }
            }
            if (alive > 0 && hpRatio / alive > 0.5f) gained++;

            int prev = StarsOf(chapter, stage);
            int best = Mathf.Max(prev, gained);
            _stars[Key(chapter, stage)] = best;

            long gold = 250 + floor * 95;
            float exp = 30 + floor * 9f;
            Core.Economy.AddGold(gold, "Stage " + chapter + "-" + stage);
            Core.PlayerProfile.GrantExp(exp);

            if (prev == 0)
            {
                Core.Economy.AddDiamonds(30, "First clear");
                if (stage == StagesPerChapter)
                {
                    Core.Economy.AddDiamonds(80, "Chapter clear!");
                    var roster = HeroDatabase.CreateDefaultRoster();
                    var rnd = new System.Random(chapter * 31 + stage);
                    var hero = roster[rnd.Next(roster.Count)];
                    Core.HeroProgress.AddShards(hero.heroId, 15);
                    CombatEvents.RaiseNotification("Chapter reward: " + hero.displayName + " shards x15");
                }
                int total = TotalStars();
                if (total == 30 || total == 60 || total == 90 || total == 120)
                {
                    Core.Economy.AddDiamonds(total * 2, "Star milestone " + total);
                }
            }

            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
            UI.UIManager.Instance.ShowBattleResult(true,
                "Stage " + chapter + "-" + stage + " cleared!",
                new[] { "+" + Core.Economy.Format(gold) + " Gold", "+" + Mathf.RoundToInt(exp) + " Player XP", best + " / 3 Stars" });
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.campaignStars.Clear();
            foreach (var kv in _stars)
            {
                s.campaignStars.Add(new SaveSystem.CampaignStarSave { key = kv.Key, stars = kv.Value });
            }
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            _stars.Clear();
            if (s.campaignStars == null) return;
            foreach (var c in s.campaignStars)
            {
                if (c != null && !string.IsNullOrEmpty(c.key)) _stars[c.key] = Mathf.Clamp(c.stars, 0, 3);
            }
        }
    }

    // ====================================================================
    // CONTINENT GATE — 5 regions × 10 stages, endgame content.
    // ====================================================================
    public static class ContinentGateSystem
    {
        public const int Regions = 5;
        public const int StagesPerRegion = 10;

        public static readonly string[] RegionNames =
        {
            "Verdant Reach", "Ember Expanse", "Frozen Wastes", "Storm Archipelago", "Umbra Frontier"
        };

        private static readonly Dictionary<string, int> _stars = new Dictionary<string, int>();

        public static event Action OnChanged;

        public static string Key(int region, int stage) => "CG" + region + "-" + stage;

        public static int StarsOf(int region, int stage)
        {
            int v;
            return _stars.TryGetValue(Key(region, stage), out v) ? v : 0;
        }

        public static bool IsUnlocked(int region, int stage)
        {
            if (CampaignSystem.ClearedStageCount() < 20) return false;   // gate: 20 campaign stages
            if (region == 1 && stage == 1) return true;
            if (stage > 1) return StarsOf(region, stage - 1) > 0;
            return StarsOf(region - 1, StagesPerRegion) > 0;
        }

        public static void StartStage(int region, int stage)
        {
            if (!IsUnlocked(region, stage))
            {
                CombatEvents.RaiseNotification(region == 1
                    ? "Clear 20 campaign stages first!"
                    : "Stage locked!");
                Audio.AudioSystem.Play("Error");
                return;
            }

            int floor = 150 + (region - 1) * StagesPerRegion + stage;

            var cfg = new Core.BattleSession.Config
            {
                type = Core.BattleSession.BattleType.ContinentGate,
                title = RegionNames[Mathf.Clamp(region - 1, 0, RegionNames.Length - 1)] + " " + stage,
                floor = floor,
                difficulty = 3,
                boss = stage == StagesPerRegion,
                region = region,
                stage = stage
            };
            cfg.onWin = () => ApplyGateWin(region, stage);
            Core.BattleSession.Start(cfg);
        }

        /// <summary>Real gate completion: progress + first-clear loot.</summary>
        public static void ApplyGateWin(int region, int stage)
        {
            int floor = 150 + (region - 1) * StagesPerRegion + stage;
            bool first = StarsOf(region, stage) == 0;
            _stars[Key(region, stage)] = Mathf.Max(StarsOf(region, stage), 1);
            long gold = 900 + floor * 60;
            Core.Economy.AddGold(gold, "Gate");
            if (first)
            {
                Core.Economy.AddDiamonds(120, "Gate first clear");
                Items.InventorySystem.Grant(Items.ItemDatabase.RollRandomItemId());
            }
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
            UI.UIManager.Instance.ShowBattleResult(true, "Region conquered!",
                new[] { "+" + Core.Economy.Format(gold) + " Gold", first ? "+120 Diamonds (first clear)" : "Gate progress" });
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.continentStars.Clear();
            foreach (var kv in _stars)
            {
                s.continentStars.Add(new SaveSystem.CampaignStarSave { key = kv.Key, stars = kv.Value });
            }
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            _stars.Clear();
            if (s.continentStars == null) return;
            foreach (var c in s.continentStars)
            {
                if (c != null && !string.IsNullOrEmpty(c.key)) _stars[c.key] = Mathf.Clamp(c.stars, 0, 3);
            }
        }
    }

    // ====================================================================
    // DUNGEONS — Gold & EXP dungeons, 2 entries/day each, 3 difficulties.
    // ====================================================================
    public static class DungeonSystem
    {
        public const int AttemptsPerDay = 2;

        public static int GoldAttemptsUsed { get; private set; }
        public static int ExpAttemptsUsed { get; private set; }
        private static string _day = "";

        public static event Action OnChanged;

        public static void DailyReset()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (_day == today) return;
            _day = today;
            GoldAttemptsUsed = 0;
            ExpAttemptsUsed = 0;
        }

        public static int AttemptsLeft(bool goldDungeon) => AttemptsPerDay - (goldDungeon ? GoldAttemptsUsed : ExpAttemptsUsed);

        public static void Start(bool goldDungeon, int tier)
        {
            DailyReset();
            if (AttemptsLeft(goldDungeon) <= 0)
            {
                CombatEvents.RaiseNotification("No attempts left today!");
                Audio.AudioSystem.Play("Error");
                return;
            }

            int floor = 30 + tier * 45;

            var cfg = new Core.BattleSession.Config
            {
                type = goldDungeon ? Core.BattleSession.BattleType.DungeonGold : Core.BattleSession.BattleType.DungeonExp,
                title = (goldDungeon ? "Gold Mine Depths " : "Trial of Experience ") + (tier + 1),
                floor = floor,
                difficulty = Mathf.Clamp(tier, 0, 2),
                monsterCount = 6,
                goldDungeon = goldDungeon,
                tier = tier
            };
            cfg.onWin = () => ApplyDungeonWin(goldDungeon, tier);
            Core.BattleSession.Start(cfg);
        }

        /// <summary>Real dungeon completion: attempts + big rewards.</summary>
        public static void ApplyDungeonWin(bool goldDungeon, int tier)
        {
            if (goldDungeon) GoldAttemptsUsed++; else ExpAttemptsUsed++;

            if (goldDungeon)
            {
                long gold = (tier + 1) * (tier + 1) * 9000L;
                Core.Economy.AddGold(gold, "Gold Dungeon");
                UI.UIManager.Instance.ShowBattleResult(true, "Treasure secured!",
                    new[] { "+" + Core.Economy.Format(gold) + " Gold" });
            }
            else
            {
                var team = Team.TeamManager.InstanceRef;
                int ups = 0;
                if (team != null)
                {
                    var roster = HeroDatabase.CreateDefaultRoster();
                    var map = new Dictionary<string, HeroData>();
                    foreach (var h in roster) map[h.heroId] = h;
                    foreach (var h in team.Heroes)
                    {
                        if (h == null || !map.ContainsKey(h.Data.heroId)) continue;
                        int lv = Core.HeroProgress.LevelOf(h.Data.heroId);
                        Core.HeroProgress.SetLevel(h.Data.heroId, lv + 1 + tier);
                        ups += 1 + tier;
                    }
                }
                Core.PlayerProfile.GrantExp(150 + tier * 120);
                UI.UIManager.Instance.ShowBattleResult(true, "Heroes grew stronger!",
                    new[] { "+" + ups + " hero levels", "+" + (150 + tier * 120) + " player XP" });
            }
            Core.QuestSystem.ReportProgress("dungeon", 1);
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.dungeonDay = _day;
            s.goldAttempts = GoldAttemptsUsed;
            s.expAttempts = ExpAttemptsUsed;
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            _day = s.dungeonDay ?? "";
            GoldAttemptsUsed = Mathf.Clamp(s.goldAttempts, 0, AttemptsPerDay);
            ExpAttemptsUsed = Mathf.Clamp(s.expAttempts, 0, AttemptsPerDay);
            DailyReset();
        }
    }

    // ====================================================================
    // PVP ARENA — rank 1..1000, generated opponents near your power.
    // ====================================================================
    public static class ArenaSystem
    {
        public class Opponent
        {
            public string name;
            public long power;
            public int rank;
            public string monsterKey;   // shadow used in battle
            public float hpScale, atkScale;
        }

        public static int Rank { get; private set; } = 950;
        public static int Wins { get; private set; }
        public static int Losses { get; private set; }
        public static int SeasonWins { get; private set; }

        private static readonly List<Opponent> _opponents = new List<Opponent>();
        private static DateTime _seasonStartUtc = DateTime.UtcNow;

        public static event Action OnChanged;

        public static IReadOnlyList<Opponent> Opponents => _opponents;

        public static TimeSpan SeasonTimeLeft
        {
            get
            {
                var end = _seasonStartUtc.AddDays(7);
                return end > DateTime.UtcNow ? end - DateTime.UtcNow : TimeSpan.Zero;
            }
        }

        public static void EnsureOpponents()
        {
            long myPower = EstimatePlayerPower();
            if (_opponents.Count > 0 && _cachedPower > 0 && Mathf.Abs(_cachedPower - myPower) < myPower * 0.4f) return;
            _cachedPower = myPower;

            _opponents.Clear();
            var rnd = new System.Random(DateTime.UtcNow.DayOfYear * 17 + 7);
            string[] shadowNames =
            {
                "Blazewing", "Tidecaller", "Stonehide", "Duskblade", "Stormfury", "Dawnkeeper"
            };

            for (int i = 0; i < 3; i++)
            {
                float mult = i == 0 ? 0.85f : i == 1 ? 1.0f : 1.18f;
                long power = (long)(myPower * mult);
                _opponents.Add(new Opponent
                {
                    name = shadowNames[rnd.Next(shadowNames.Length)] + rnd.Next(10, 99),
                    power = power,
                    rank = Mathf.Max(1, Rank - (i + 1) * UnityEngine.Random.Range(2, 9)),
                    monsterKey = "arena_shadow_" + i,
                    hpScale = Mathf.Max(0.4f, mult * 1.15f),
                    atkScale = Mathf.Max(0.4f, mult * 1.05f)
                });
            }
            _opponents.Sort((a, b) => a.rank.CompareTo(b.rank));
            OnChanged?.Invoke();
        }

        private static long _cachedPower;

        public static long EstimatePlayerPower()
        {
            var team = Team.TeamManager.InstanceRef;
            if (team != null && team.Heroes.Count > 0)
            {
                var roster = HeroDatabase.CreateDefaultRoster();
                var map = new Dictionary<string, HeroData>();
                foreach (var h in roster) map[h.heroId] = h;
                var teamData = new List<HeroData>();
                foreach (var hero in team.Heroes)
                {
                    if (hero != null && map.ContainsKey(hero.Data.heroId)) teamData.Add(map[hero.Data.heroId]);
                }
                return HeroProgress.TeamPower(teamData);
            }
            return 1200 + CampaignSystem.ClearedStageCount() * 300;
        }

        /// <summary>Registers the generated shadow MonsterData into the spawner.</summary>
        public static void RegisterShadows(Dictionary<string, MonsterData> monsters)
        {
            EnsureOpponents();
            for (int i = 0; i < _opponents.Count; i++)
            {
                var opp = _opponents[i];
                if (monsters.ContainsKey(opp.monsterKey)) continue;

                var md = UnityEngine.ScriptableObject.CreateInstance<MonsterData>();
                md.monsterId = opp.monsterKey;
                md.displayName = opp.name;
                md.element = (ElementType)((i + 1) % 7);
                md.isBoss = false;
                md.baseHP = 260f;
                md.baseAttack = 30f;
                md.baseDefense = 10f;
                md.moveSpeed = 5.0f;
                md.attackSpeed = 1.1f;
                md.attackRange = 2.4f;
                md.scale = 1.15f;
                md.expReward = 60f;
                md.primaryColor = new Color(0.9f - i * 0.15f, 0.35f + i * 0.1f, 0.3f + i * 0.2f);
                md.secondaryColor = new Color(0.2f, 0.2f, 0.25f);
                md.aggroRadius = 30f;
                md.attackVfxKey = "hit";
                monsters[opp.monsterKey] = md;
            }
        }

        public static void StartFight(Opponent opp)
        {
            if (opp == null) return;
            EnsureOpponents();

            int floor = 60 + Mathf.Clamp((950 - opp.rank) / 20, 0, 40);

            var cfg = new Core.BattleSession.Config
            {
                type = Core.BattleSession.BattleType.Arena,
                title = "Arena vs " + opp.name,
                floor = floor,
                difficulty = 1,
                monsterCount = 1,
                forcedMonsterKey = opp.monsterKey,
                hpScale = opp.hpScale,
                atkScale = opp.atkScale,
                context = opp
            };
            cfg.onWin = () => ApplyArenaWin(opp);
            cfg.onLose = () =>
            {
                Losses++;
                Rank = Mathf.Min(1000, Rank + UnityEngine.Random.Range(1, 4));
                SaveSystem.MarkDirty();
                OnChanged?.Invoke();
            };
            Core.BattleSession.Start(cfg);
        }

        /// <summary>Real arena win: rank swap, rewards, season progress.</summary>
        public static void ApplyArenaWin(Opponent opp)
        {
            if (opp == null) return;
            int oldRank = Rank;
            Rank = Mathf.Max(1, Mathf.Min(Rank, opp.rank));
            Wins++;
            SeasonWins++;
            long gold = 600 + Mathf.Max(0, oldRank - Rank) * 25;
            Core.Economy.AddGold(gold, "Arena win");
            Core.Economy.AddDiamonds(15, "Arena win");
            Core.PlayerProfile.GrantExp(40);
            Core.QuestSystem.ReportProgress("arena_win", 1);
            if (SeasonWins == 5 || SeasonWins == 15 || SeasonWins == 30)
            {
                Core.Economy.AddDiamonds(SeasonWins * 4, "Arena season");
            }
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
            UI.UIManager.Instance.ShowBattleResult(true, "Victory! Rank " + oldRank + " → " + Rank,
                new[] { "+" + Core.Economy.Format(gold) + " Gold", "+15 Diamonds", "New rank: " + Rank });
        }

        public static string RankTitle()
        {
            if (Rank <= 3) return "Challenger";
            if (Rank <= 10) return "Grandmaster";
            if (Rank <= 50) return "Master";
            if (Rank <= 200) return "Diamond";
            if (Rank <= 500) return "Gold";
            return "Fighter";
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.arenaRank = Rank;
            s.arenaWins = Wins;
            s.arenaLosses = Losses;
            s.arenaSeasonWins = SeasonWins;
            s.arenaSeasonStartUtc = _seasonStartUtc.Ticks;
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            Rank = Mathf.Clamp(s.arenaRank, 1, 1000);
            Wins = Mathf.Max(0, s.arenaWins);
            Losses = Mathf.Max(0, s.arenaLosses);
            SeasonWins = Mathf.Max(0, s.arenaSeasonWins);
            _seasonStartUtc = s.arenaSeasonStartUtc > 0
                ? new DateTime(s.arenaSeasonStartUtc, DateTimeKind.Utc)
                : DateTime.UtcNow;
            if (SeasonTimeLeft <= TimeSpan.Zero)
            {
                _seasonStartUtc = DateTime.UtcNow;
                SeasonWins = 0;
                Rank = Mathf.Max(600, Rank - 150);   // soft reset
            }
        }
    }

    // ====================================================================
    // MINE — idle gold production with levels (upgrade + claim).
    // ====================================================================
    public static class MineSystem
    {
        public const int MaxLevel = 10;
        public const float CapHours = 8f;

        public static int Level { get; private set; } = 1;
        private static DateTime _lastClaimUtc = DateTime.UtcNow;

        public static event Action OnChanged;

        public static long PerHour => 400L * Level * (1L + Level / 3);
        public static long Capacity => (long)(PerHour * CapHours);

        public static long UpgradeCost => (long)(2500 * Mathf.Pow(1.9f, Level - 1));

        public static (long gold, float capRatio) Pending()
        {
            double hours = Math.Min((DateTime.UtcNow - _lastClaimUtc).TotalHours, CapHours);
            return ((long)(PerHour * hours), (float)(hours / CapHours));
        }

        public static bool TryUpgrade()
        {
            if (Level >= MaxLevel) return false;
            if (!Core.Economy.TrySpendGold(UpgradeCost)) return false;
            Level++;
            Audio.AudioSystem.Play("Fanfare");
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
            return true;
        }

        public static long Claim()
        {
            var p = Pending();
            _lastClaimUtc = DateTime.UtcNow;
            if (p.gold > 0)
            {
                Core.Economy.AddGold(p.gold, "Mine");
                Audio.AudioSystem.Play("Coin");
            }
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
            return p.gold;
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.mineLevel = Level;
            s.mineLastClaimUtc = _lastClaimUtc.Ticks;
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            Level = Mathf.Clamp(s.mineLevel, 1, MaxLevel);
            _lastClaimUtc = s.mineLastClaimUtc > 0 ? new DateTime(s.mineLastClaimUtc, DateTimeKind.Utc) : DateTime.UtcNow;
        }
    }

    // ====================================================================
    // BEASTS — 6 collectible beasts granting team-wide passives.
    // ====================================================================
    public static class BeastSystem
    {
        public class Beast
        {
            public string id;
            public string name;
            public ElementType element;
            public string desc;
            public long unlockDiamonds;   // 0 = unlocked from start
            public float atkPctPerLevel;
            public float hpPctPerLevel;
        }

        public static readonly Beast[] Bestiary =
        {
            new Beast{ id="pidgeon",   name="Courser Pigeon",  element=ElementType.Light,     desc="+0.6% team ATK / lvl", unlockDiamonds=0,    atkPctPerLevel=0.006f, hpPctPerLevel=0.002f },
            new Beast{ id="wolfpup",   name="Frost Wolf Pup",  element=ElementType.Ice,       desc="+0.8% team HP / lvl",  unlockDiamonds=0,    atkPctPerLevel=0.002f, hpPctPerLevel=0.008f },
            new Beast{ id="emberling", name="Emberling",       element=ElementType.Fire,      desc="+1.0% team ATK / lvl", unlockDiamonds=600,  atkPctPerLevel=0.010f, hpPctPerLevel=0.002f },
            new Beast{ id="oakling",   name="Oakling",         element=ElementType.Nature,    desc="+1.0% team HP / lvl",  unlockDiamonds=800,  atkPctPerLevel=0.002f, hpPctPerLevel=0.010f },
            new Beast{ id="voltcub",   name="Volt Cub",        element=ElementType.Lightning, desc="+1.4% team ATK / lvl", unlockDiamonds=1200, atkPctPerLevel=0.014f, hpPctPerLevel=0.003f },
            new Beast{ id="shadekit",  name="Shade Kitten",    element=ElementType.Dark,      desc="+1.2% ATK & HP / lvl", unlockDiamonds=2000, atkPctPerLevel=0.012f, hpPctPerLevel=0.012f },
        };

        public class BeastState
        {
            public string id;
            public int level = 1;
            public bool unlocked;
        }

        private static readonly Dictionary<string, BeastState> _states = new Dictionary<string, BeastState>();
        public static int Snacks { get; private set; } = 3;

        public static event Action OnChanged;

        private static BeastState S(string id)
        {
            BeastState st;
            if (!_states.TryGetValue(id, out st))
            {
                st = new BeastState { id = id };
                _states[id] = st;
            }
            return st;
        }

        public static void EnsureDefaults()
        {
            foreach (var b in Bestiary)
            {
                var st = S(b.id);
                if (b.unlockDiamonds == 0) st.unlocked = true;
            }
        }

        public static int LevelOf(string id) => S(id).level;
        public static bool IsUnlocked(string id) => S(id).unlocked;

        public static long FeedCost(string id)
        {
            return 500L * (LevelOf(id) + 1) * (LevelOf(id) + 1);
        }

        public static bool TryFeed(string id)
        {
            var st = S(id);
            if (!st.unlocked) return false;
            if (Snacks <= 0)
            {
                CombatEvents.RaiseNotification("No beast snacks! Buy them in the shop.");
                Audio.AudioSystem.Play("Error");
                return false;
            }
            if (!Core.Economy.TrySpendGold(FeedCost(id))) return false;
            Snacks--;
            st.level++;
            Audio.AudioSystem.Play("LevelUp");
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
            return true;
        }

        public static bool TryUnlock(Beast b)
        {
            var st = S(b.id);
            if (st.unlocked) return false;
            if (!Core.Economy.TrySpendDiamonds(b.unlockDiamonds)) return false;
            st.unlocked = true;
            Audio.AudioSystem.Play("Fanfare");
            CombatEvents.RaiseNotification(b.name + " joined your menagerie!");
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
            return true;
        }

        public static void GrantSnack(int n)
        {
            Snacks += n;
            CombatEvents.RaiseNotification("+" + n + " Beast Snacks");
            OnChanged?.Invoke();
        }

        /// <summary>Total passive from the highest-level unlocked beast (active companion).</summary>
        public static (float atkPct, float hpPct) ActiveBonus()
        {
            float atk = 0f, hp = 0f;
            foreach (var b in Bestiary)
            {
                var st = S(b.id);
                if (!st.unlocked) continue;
                atk += b.atkPctPerLevel * st.level;
                hp += b.hpPctPerLevel * st.level;
            }
            return (atk, hp);
        }

        public static float TeamPowerBonusPct()
        {
            var b = ActiveBonus();
            return b.atkPct * 0.5f + b.hpPct * 0.5f;
        }

        /// <summary>Feeds passive stats into a hero's CharacterStats.</summary>
        public static void ApplyToHero(CharacterStats stats)
        {
            var bonus = ActiveBonus();
            stats.AddEquipmentBonus(new StatBonus
            {
                hp = stats.MaxHP * bonus.hpPct,
                attack = stats.Attack * bonus.atkPct,
                defense = 0f,
                moveSpeed = 0f
            });
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.beastSnacks = Snacks;
            s.beasts.Clear();
            foreach (var kv in _states)
            {
                s.beasts.Add(new SaveSystem.BeastSave { id = kv.Key, level = kv.Value.level, unlocked = kv.Value.unlocked });
            }
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            _states.Clear();
            Snacks = Mathf.Max(0, s.beastSnacks);
            if (s.beasts != null)
            {
                foreach (var b in s.beasts)
                {
                    if (b == null || string.IsNullOrEmpty(b.id)) continue;
                    _states[b.id] = new BeastState { id = b.id, level = Mathf.Max(1, b.level), unlocked = b.unlocked };
                }
            }
            EnsureDefaults();
        }
    }

    // ====================================================================
    // Retry plumbing — rebuilds win/lose closures for every battle type.
    // ====================================================================
    public static class ProgressionSystems
    {
        public static void RewireBattleCallbacks(Core.BattleSession.Config cfg)
        {
            switch (cfg.type)
            {
                case Core.BattleSession.BattleType.Campaign:
                    cfg.onWin = () => CampaignSystem.ApplyStageWin(cfg.chapter, cfg.stage);
                    break;

                case Core.BattleSession.BattleType.Tower:
                    cfg.onWin = () => ApplyTowerWin(cfg);
                    break;

                case Core.BattleSession.BattleType.DungeonGold:
                case Core.BattleSession.BattleType.DungeonExp:
                    cfg.onWin = () => DungeonSystem.ApplyDungeonWin(cfg.goldDungeon, cfg.tier);
                    break;

                case Core.BattleSession.BattleType.Arena:
                {
                    var opp = cfg.context as ArenaSystem.Opponent;
                    cfg.onWin = () => ArenaSystem.ApplyArenaWin(opp);
                    break;
                }

                case Core.BattleSession.BattleType.ContinentGate:
                    cfg.onWin = () => ContinentGateSystem.ApplyGateWin(cfg.region, cfg.stage);
                    break;

                default:
                    // GuildHunt damage is tracked globally via CombatEvents
                    cfg.onWin = () => { };
                    break;
            }
            cfg.onLose = () => { };
        }

        public static void ApplyTowerWin(Core.BattleSession.Config cfg)
        {
            var tower = GameManager.Instance != null ? GameManager.Instance.Tower : null;
            if (tower != null) tower.AdvanceFloor();
            SaveSystem.SaveNow();
            long gold = 200 + cfg.floor * 40;
            Economy.AddGold(gold, "Tower");
            UI.UIManager.Instance.ShowBattleResult(true, "Floor " + cfg.floor + " cleared!",
                new[] { "+" + Economy.Format(gold) + " Gold", "Next floor: " + (tower != null ? tower.CurrentFloor : cfg.floor + 1) });
        }

        /// <summary>Convenience launcher used by TowerScreen (retry-safe).</summary>
        public static void StartTowerBattle(int floor, int difficulty, string difficultyName)
        {
            var cfg = new Core.BattleSession.Config
            {
                type = Core.BattleSession.BattleType.Tower,
                title = "Tower Floor " + floor + " — " + difficultyName,
                floor = floor,
                difficulty = difficulty,
                boss = floor % GameConfig.BossEveryNFloors == 0
            };
            cfg.onWin = () => ApplyTowerWin(cfg);
            Core.BattleSession.Start(cfg);
        }
    }
}
