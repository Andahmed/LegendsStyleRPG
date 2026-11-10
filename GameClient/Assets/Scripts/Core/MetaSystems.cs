using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;

namespace LegendsRPG.Core
{
    // ====================================================================
    // DAILY QUESTS — 8 real tasks tracked by gameplay events, claimable.
    // ====================================================================
    public static class QuestSystem
    {
        public class QuestDef
        {
            public string id;
            public string title;
            public int target;
            public long goldReward;
            public int diamondReward;
            public float expReward;
        }

        public static readonly QuestDef[] Daily = new QuestDef[]
        {
            new QuestDef{ id="win_battle",  title="Win 5 battles",            target=5,  goldReward=800,   diamondReward=20, expReward=60 },
            new QuestDef{ id="summon",      title="Summon 1 hero",            target=1,  goldReward=500,   diamondReward=30, expReward=40 },
            new QuestDef{ id="hero_upgrade",title="Level up any hero 3 times",target=3,  goldReward=600,   diamondReward=15, expReward=50 },
            new QuestDef{ id="forge_upgrade",title="Forge 1 equipment",       target=1,  goldReward=700,   diamondReward=15, expReward=45 },
            new QuestDef{ id="earn_gold",   title="Earn 5000 gold",           target=5000,goldReward=400,  diamondReward=10, expReward=40 },
            new QuestDef{ id="afk_claim",   title="Claim AFK rewards",        target=1,  goldReward=300,   diamondReward=10, expReward=30 },
            new QuestDef{ id="arena_win",   title="Win 1 Arena fight",        target=1,  goldReward=900,   diamondReward=25, expReward=55 },
            new QuestDef{ id="dungeon",     title="Clear any dungeon",        target=1,  goldReward=800,   diamondReward=20, expReward=50 },
        };

        private static readonly Dictionary<string, int> _progress = new Dictionary<string, int>();
        private static readonly HashSet<string> _claimed = new HashSet<string>();
        private static string _day = "";

        public static event Action OnChanged;

        public static void DailyReset()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (_day == today) return;
            _day = today;
            _progress.Clear();
            _claimed.Clear();
            Combat.CombatEvents.RaiseNotification("Daily quests refreshed!");
            OnChanged?.Invoke();
        }

        /// <summary>Called by every gameplay system to push progress into dailies.</summary>
        public static void ReportProgress(string questId, int amount)
        {
            DailyReset();
            foreach (var q in Daily)
            {
                if (q.id != questId) continue;
                int cur;
                _progress.TryGetValue(q.id, out cur);
                if (cur >= q.target) return;
                _progress[q.id] = Mathf.Min(q.target, cur + (amount > 0 ? amount : 1));
                if (_progress[q.id] >= q.target)
                {
                    Combat.CombatEvents.RaiseNotification("Quest ready: " + q.title);
                    Audio.AudioSystem.Play("UI");
                }
                OnChanged?.Invoke();
                return;
            }
        }

        public static int ProgressOf(string questId)
        {
            int v;
            return _progress.TryGetValue(questId, out v) ? v : 0;
        }

        public static bool IsClaimable(QuestDef q)
        {
            return !_claimed.Contains(q.id) && ProgressOf(q.id) >= q.target;
        }

        public static bool TryClaim(QuestDef q)
        {
            DailyReset();
            if (!IsClaimable(q)) return false;
            _claimed.Add(q.id);
            Economy.AddGold(q.goldReward, "Quest");
            Economy.AddDiamonds(q.diamondReward, "Quest");
            PlayerProfile.GrantExp(q.expReward);
            Audio.AudioSystem.Play("Coin");
            OnChanged?.Invoke();
            return true;
        }

        public static int ClaimableCount()
        {
            DailyReset();
            int n = 0;
            foreach (var q in Daily) if (IsClaimable(q)) n++;
            return n;
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.questDay = _day;
            s.questProgress = new List<int>();
            s.questClaimed = new List<string>();
            foreach (var q in Daily) s.questProgress.Add(ProgressOf(q.id));
            foreach (var c in _claimed) s.questClaimed.Add(c);
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            _day = s.questDay ?? "";
            _progress.Clear();
            _claimed.Clear();
            if (s.questProgress != null)
            {
                for (int i = 0; i < Daily.Length && i < s.questProgress.Count; i++)
                {
                    _progress[Daily[i].id] = s.questProgress[i];
                }
            }
            if (s.questClaimed != null)
            {
                foreach (var c in s.questClaimed) _claimed.Add(c);
            }
            DailyReset();
        }
    }

    // ====================================================================
    // DAILY SHOP — 6 slots refreshed every 24h with random discounts.
    // ====================================================================
    public static class ShopSystem
    {
        public class ShopSlot
        {
            public string key;          // stable id for this slot
            public string itemId;       // "" => pure currency pack
            public int heroShardsFor;   // -1 none
            public string heroId;
            public long goldCost;
            public int diamondCost;
            public int discountPct;     // 0..50
            public bool bought;
        }

        private static readonly List<ShopSlot> _slots = new List<ShopSlot>();
        private static DateTime _refreshUtc = DateTime.MinValue;

        public static event Action OnChanged;

        public static IReadOnlyList<ShopSlot> Slots { get { return _slots; } }

        public static TimeSpan TimeToRefresh()
        {
            var next = _refreshUtc.AddHours(24);
            return next > DateTime.UtcNow ? next - DateTime.UtcNow : TimeSpan.Zero;
        }

        public static void EnsureFresh()
        {
            if (DateTime.UtcNow < _refreshUtc.AddHours(24) && _slots.Count > 0) return;
            Generate(DateTime.UtcNow);
        }

        private static void Generate(DateTime now)
        {
            _slots.Clear();
            _refreshUtc = now;
            var rnd = new System.Random(now.DayOfYear * 100 + now.Hour);
            var items = Items.ItemDatabase.AllItemIds();
            for (int i = 0; i < 6; i++)
            {
                int discount = rnd.Next(0, 6) * 10;   // 0..50%
                var slot = new ShopSlot { key = "slot" + i + "_" + now.ToString("yyyyMMdd"), discountPct = discount };

                int kind = i % 3;
                if (kind == 0 && items.Count > 0)
                {
                    slot.itemId = items[rnd.Next(items.Count)];
                    long baseCost = 1500 + Items.ItemDatabase.Get(slot.itemId).RarityIndex * 2200;
                    slot.goldCost = (long)(baseCost * (1f - discount / 100f));
                }
                else if (kind == 1)
                {
                    slot.heroShardsFor = 20;
                    var roster = HeroDatabase.CreateDefaultRoster();
                    var hero = roster[rnd.Next(roster.Count)];
                    slot.heroId = hero.heroId;
                    long baseCost = 60 + (int)hero.rarity * 40;
                    slot.diamondCost = (int)(baseCost * (1f - discount / 100f));
                }
                else
                {
                    // currency pack: gold for diamonds
                    long gold = rnd.Next(2, 9) * 1000;
                    slot.goldCost = gold;
                    slot.diamondCost = Mathf.Max(10, (int)(gold * 0.02f * (1f - discount / 100f)));
                }
                _slots.Add(slot);
            }
            OnChanged?.Invoke();
        }

        public static bool TryBuy(ShopSlot slot)
        {
            EnsureFresh();
            if (slot == null || slot.bought) return false;
            bool paid;
            if (slot.diamondCost > 0) paid = Economy.TrySpendDiamonds(slot.diamondCost);
            else paid = Economy.TrySpendGold(slot.goldCost);
            if (!paid) return false;

            if (!string.IsNullOrEmpty(slot.itemId))
            {
                Items.InventorySystem.Grant(slot.itemId);
            }
            else if (slot.heroShardsFor > 0 && !string.IsNullOrEmpty(slot.heroId))
            {
                HeroProgress.AddShards(slot.heroId, slot.heroShardsFor);
                Combat.CombatEvents.RaiseNotification("+20 shards");
            }
            slot.bought = true;
            Audio.AudioSystem.Play("Coin");
            QuestSystem.ReportProgress("shop_buy", 1);
            OnChanged?.Invoke();
            return true;
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.shopRefreshUtc = _refreshUtc.Ticks;
            s.shopSlots.Clear();
            foreach (var slot in _slots)
            {
                s.shopSlots.Add(new SaveSystem.ShopSlotSave
                {
                    key = slot.key, itemId = slot.itemId ?? "", heroId = slot.heroId ?? "",
                    heroShards = slot.heroShardsFor, goldCost = slot.goldCost,
                    diamondCost = slot.diamondCost, discount = slot.discountPct, bought = slot.bought
                });
            }
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            _slots.Clear();
            _refreshUtc = s.shopRefreshUtc > 0 ? new DateTime(s.shopRefreshUtc, DateTimeKind.Utc) : DateTime.MinValue;
            if (s.shopSlots != null)
            {
                foreach (var r in s.shopSlots)
                {
                    if (r == null) continue;
                    _slots.Add(new ShopSlot
                    {
                        key = r.key, itemId = r.itemId, heroId = r.heroId,
                        heroShardsFor = r.heroShards, goldCost = r.goldCost,
                        diamondCost = r.diamondCost, discountPct = r.discount, bought = r.bought
                    });
                }
            }
            EnsureFresh();
        }
    }

    // ====================================================================
    // SIGN-IN — 8-day escalating reward cycle.
    // ====================================================================
    public static class SignInSystem
    {
        public const int DaysPerCycle = 8;

        public static int Cycle { get; private set; } = 1;
        public static int ClaimedToday { get; private set; } = -1;   // -1 = not claimed today
        private static string _lastClaimDay = "";
        private static readonly HashSet<int> _claimedIndices = new HashSet<int>();

        public static event Action OnChanged;

        public static string DayTitle(int day)
        {
            switch (day)
            {
                case 0: return "Day 1 — Gold";
                case 1: return "Day 2 — Diamonds";
                case 2: return "Day 3 — Hero Shards";
                case 3: return "Day 4 — Equipment";
                case 4: return "Day 5 — Diamonds x120";
                case 5: return "Day 6 — Beast Snack";
                case 6: return "Day 7 — Rare Hero Shards";
                default: return "Day 8 — EPIC HERO";
            }
        }

        public static bool IsClaimed(int dayIndex) => _claimedIndices.Contains(dayIndex);
        public static bool CanClaimToday() => _lastClaimDay != DateTime.UtcNow.ToString("yyyyMMdd");

        public static bool TryClaimToday()
        {
            if (!CanClaimToday()) return false;
            int day = CurrentDayIndex();
            if (_claimedIndices.Contains(day)) return false;

            _lastClaimDay = DateTime.UtcNow.ToString("yyyyMMdd");
            _claimedIndices.Add(day);
            ClaimedToday = day;

            switch (day)
            {
                case 0: Economy.AddGold(2000, "Sign-in"); break;
                case 1: Economy.AddDiamonds(60, "Sign-in"); break;
                case 2: HeroProgress.AddShards(StarterShardHero(), 15); break;
                case 3: Items.InventorySystem.Grant(Items.ItemDatabase.RollRandomItemId()); break;
                case 4: Economy.AddDiamonds(120, "Sign-in"); break;
                case 5: World.BeastSystem.GrantSnack(3); break;
                case 6: HeroProgress.AddShards(StarterShardHero(), 30); break;
                default:
                {
                    // Day 8 — guaranteed Epic hero
                    var roster = HeroDatabase.CreateDefaultRoster();
                    HeroData epic = null;
                    foreach (var h in roster)
                    {
                        if (h.rarity == Rarity.Epic && !SaveSystem.Current.unlockedHeroIds.Contains(h.heroId)) { epic = h; break; }
                    }
                    if (epic == null)
                    {
                        foreach (var h in roster) if (h.rarity == Rarity.Epic) { epic = h; break; }
                    }
                    if (epic != null) SummonSystem.UnlockHeroDirect(epic.heroId, "Sign-in Day 8");
                    break;
                }
            }

            if (_claimedIndices.Count >= DaysPerCycle)
            {
                _claimedIndices.Clear();
                Cycle++;
                Economy.AddDiamonds(200, "Cycle complete");
            }
            PlayerProfile.GrantExp(50);
            Audio.AudioSystem.Play("Fanfare");
            OnChanged?.Invoke();
            return true;
        }

        public static int CurrentDayIndex()
        {
            // day index = how many claims already made this cycle
            int idx = 0;
            for (int i = 0; i < DaysPerCycle; i++)
            {
                if (!_claimedIndices.Contains(i)) { idx = i; break; }
                idx = i + 1;
            }
            return Mathf.Min(idx, DaysPerCycle - 1);
        }

        private static string StarterShardHero()
        {
            var team = SaveSystem.Current.teamOrder;
            if (team != null && SaveSystem.Current.unlockedHeroIds.Count > 0)
                return SaveSystem.Current.unlockedHeroIds[0];
            return "kael_flameblade";
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.signinCycle = Cycle;
            s.signinLastClaimDay = _lastClaimDay;
            s.signinClaimed = new List<int>(_claimedIndices);
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            Cycle = Mathf.Max(1, s.signinCycle);
            _lastClaimDay = s.signinLastClaimDay ?? "";
            _claimedIndices.Clear();
            if (s.signinClaimed != null) foreach (var i in s.signinClaimed) _claimedIndices.Add(i);
        }
    }

    // ====================================================================
    // LIMITED EVENT — 7-day rotating event with battle points + milestones.
    // ====================================================================
    public static class LimitedEvent
    {
        public static readonly string[] EventNames =
            { "Ember Siege", "Frost Trial", "Storm Rush", "Shadow Hunt", "Light Pilgrimage", "Wild Growth", "Tide Crash" };

        public static string EventName
        {
            get
            {
                int week = Mathf.Abs(DateTime.UtcNow.DayOfYear) / 7;
                return EventNames[week % EventNames.Length];
            }
        }

        public static TimeSpan TimeLeft
        {
            get
            {
                var now = DateTime.UtcNow;
                int daysToSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;
                var end = now.Date.AddDays(daysToSunday + 1);
                return end - now;
            }
        }

        public static int Points { get; private set; }
        private static readonly HashSet<int> _claimedMilestones = new HashSet<int>();

        public static event Action OnChanged;

        public static readonly int[] Milestones = { 100, 300, 700, 1200, 2000, 3000 };

        public static void AddPoints(int battleWon)
        {
            Points += battleWon * 50;
            OnChanged?.Invoke();
        }

        public static bool IsMilestoneClaimable(int index)
        {
            return !_claimedMilestones.Contains(index) && Points >= Milestones[index];
        }

        public static bool TryClaimMilestone(int index)
        {
            if (index < 0 || index >= Milestones.Length) return false;
            if (!IsMilestoneClaimable(index)) return false;
            _claimedMilestones.Add(index);
            switch (index)
            {
                case 0: Economy.AddGold(3000, "Event"); break;
                case 1: Economy.AddDiamonds(80, "Event"); break;
                case 2: HeroProgress.AddShards(StarterHero(), 20); break;
                case 3: Items.InventorySystem.Grant(Items.ItemDatabase.RollRandomItemId()); break;
                case 4: Economy.AddDiamonds(150, "Event"); break;
                case 5:
                    var roster = HeroDatabase.CreateDefaultRoster();
                    foreach (var h in roster)
                    {
                        if (h.rarity == Rarity.Legendary && !SaveSystem.Current.unlockedHeroIds.Contains(h.heroId))
                        {
                            SummonSystem.UnlockHeroDirect(h.heroId, "Event milestone");
                            break;
                        }
                    }
                    break;
            }
            Audio.AudioSystem.Play("Coin");
            OnChanged?.Invoke();
            return true;
        }

        private static string StarterHero()
        {
            return SaveSystem.Current.unlockedHeroIds.Count > 0 ? SaveSystem.Current.unlockedHeroIds[0] : "kael_flameblade";
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.eventPoints = Points;
            s.eventClaimed = new List<int>(_claimedMilestones);
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            Points = Mathf.Max(0, s.eventPoints);
            _claimedMilestones.Clear();
            if (s.eventClaimed != null) foreach (var i in s.eventClaimed) _claimedMilestones.Add(i);
        }
    }

    // ====================================================================
    // MERCHANT — rotating 6h offers, deeper discounts than daily shop.
    // ====================================================================
    public static class MerchantSystem
    {
        public class Offer
        {
            public string label;
            public string detail;
            public int diamondCost;
            public string itemId;
            public string heroId;
            public int shards;
            public long goldGrant;
            public int diamondGrant;
            public bool bought;
        }

        public static event Action OnChanged;

        private static readonly List<Offer> _offers = new List<Offer>();
        private static DateTime _refreshUtc = DateTime.MinValue;

        public static IReadOnlyList<Offer> Offers { get { return _offers; } }

        public static TimeSpan TimeToRefresh()
        {
            var next = _refreshUtc.AddHours(6);
            return next > DateTime.UtcNow ? next - DateTime.UtcNow : TimeSpan.Zero;
        }

        public static void EnsureFresh()
        {
            if (DateTime.UtcNow < _refreshUtc.AddHours(6) && _offers.Count > 0) return;
            _offers.Clear();
            _refreshUtc = DateTime.UtcNow;
            var rnd = new System.Random(DateTime.UtcNow.Hour * 7 + DateTime.UtcNow.DayOfYear);

            var items = Items.ItemDatabase.AllItemIds();
            var roster = HeroDatabase.CreateDefaultRoster();

            for (int i = 0; i < 4; i++)
            {
                var o = new Offer { label = "Mystery Gear", detail = "Random equipment x1" };
                o.itemId = items[rnd.Next(items.Count)];
                o.diamondCost = 40 + rnd.Next(0, 4) * 20;
                _offers.Add(o);
            }
            for (int i = 0; i < 2; i++)
            {
                var hero = roster[rnd.Next(roster.Count)];
                var o = new Offer
                {
                    label = hero.displayName + " Shards x25",
                    detail = "Ascension shards for " + hero.displayName,
                    heroId = hero.heroId,
                    shards = 25,
                    diamondCost = 60 + (int)hero.rarity * 30
                };
                _offers.Add(o);
            }
            var goldPack = new Offer { label = "Gold Bag x20K", detail = "Instant 20,000 gold", goldGrant = 20000, diamondCost = 100 };
            _offers.Add(goldPack);
        }

        public static bool TryBuy(Offer o)
        {
            if (o == null || o.bought) return false;
            if (!Economy.TrySpendDiamonds(o.diamondCost)) return false;

            if (!string.IsNullOrEmpty(o.itemId)) Items.InventorySystem.Grant(o.itemId);
            if (!string.IsNullOrEmpty(o.heroId) && o.shards > 0) HeroProgress.AddShards(o.heroId, o.shards);
            if (o.goldGrant > 0) Economy.AddGold(o.goldGrant, "Merchant");
            if (o.diamondGrant > 0) Economy.AddDiamonds(o.diamondGrant, "Merchant");

            o.bought = true;
            Audio.AudioSystem.Play("Coin");
            OnChanged?.Invoke();
            return true;
        }

        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.merchantRefreshUtc = _refreshUtc.Ticks;
            // offers are transient; bought flags only
            s.merchantBoughtCount = 0;
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            _refreshUtc = s.merchantRefreshUtc > 0 ? new DateTime(s.merchantRefreshUtc, DateTimeKind.Utc) : DateTime.MinValue;
            EnsureFresh();
        }
    }

    // ====================================================================
    // CHAT — world + guild channels with live simulated community.
    // ====================================================================
    public static class ChatSystem
    {
        public class ChatMessage
        {
            public string author;
            public string text;
            public DateTime time;
            public bool isPlayer;
            public Color color;
        }

        private static readonly List<ChatMessage> _world = new List<ChatMessage>();
        private static readonly List<ChatMessage> _guild = new List<ChatMessage>();

        public static event Action OnChanged;

        private static readonly string[] BotNames =
            { "Zaid", "Omar", "Layla", "Karim", "Nour", "Yousef", "Adam", "Hana", "Tariq", "Salma" };

        private static readonly string[] BotLines =
        {
            "anyone clear chapter 6-10 yet?", "the frost event boss is brutal", "LF guild, power 45k",
            "just pulled a Mythic from the 10x!!", "guys how do I beat 3-15", "merchant has gold packs today",
            "guild hunt resets soon, join up", "arena reset got me top 100 finally", "who needs shards? trading in wish wall",
            "AFK chest capped, go claim", "new tech levels are worth it", "that lich drops great gear"
        };

        public static IReadOnlyList<ChatMessage> World => _world;
        public static IReadOnlyList<ChatMessage> Guild => _guild;

        private static float _nextBotAt = 4f;
        private static int _seed;

        public static void SeedWelcome()
        {
            if (_world.Count > 0) return;
            Push(_world, "System", "Welcome to the World Channel! Be kind, share tips.", true, new Color(1f, 0.8f, 0.3f));
            Push(_world, "Mira", "Chapter 2 gives great gold — farm 2-12 if stuck", false, Color.white);
            Push(_guild, "System", "Guild channel unlocked when you join a guild.", true, new Color(1f, 0.8f, 0.3f));
        }

        public static void Tick()
        {
            if (Time.unscaledTime >= _nextBotAt)
            {
                _nextBotAt = Time.unscaledTime + UnityEngine.Random.Range(18f, 45f);
                var rnd = new System.Random(_seed++);
                Push(_world, BotNames[rnd.Next(BotNames.Length)], BotLines[rnd.Next(BotLines.Length)], false, Color.white);
            }
        }

        public static void SendWorld(string text) => Send(_world, text);
        public static void SendGuild(string text)
        {
            if (!Social.GuildSystem.InGuild)
            {
                Combat.CombatEvents.RaiseNotification("Join a guild first!");
                Audio.AudioSystem.Play("Error");
                return;
            }
            Send(_guild, text);
        }

        private static void Send(List<ChatMessage> channel, string text)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) return;
            Push(channel, PlayerProfile.Name, text, true, new Color(0.5f, 0.85f, 1f));
            Audio.AudioSystem.Play("UI");
        }

        private static void Push(List<ChatMessage> channel, string author, string text, bool sys, Color color)
        {
            channel.Add(new ChatMessage { author = author, text = text, time = DateTime.UtcNow, isPlayer = sys, color = color });
            if (channel.Count > 60) channel.RemoveAt(0);
            OnChanged?.Invoke();
        }
    }

    // ====================================================================
    // RANKING — leaderboard blending bots with the real player.
    // ====================================================================
    public static class RankSystem
    {
        public class RankEntry
        {
            public string name;
            public long power;
            public int stage;
            public bool isPlayer;
        }

        private static readonly List<RankEntry> _board = new List<RankEntry>();

        public static event Action OnChanged;

        public static IReadOnlyList<RankEntry> Board { get { return _board; } }

        public static void Rebuild()
        {
            _board.Clear();
            var rnd = new System.Random(1234);
            string[] names =
            {
                "Phoenix", "ShadowKing", "Mira", "BlazeX", "FrostQueen", "Volt", "Auras", "Kryptos",
                "Nova", "Dragoon", "Sylph", "TitanSlayer", "Echo", "Raven", "Lumen", "Warden",
                "Zephyr", "Obsidian", "StarLord", "Vortex", "Ember", "Glacier", "Thorn", "Prism",
                "Cinder", "Aegis", "Fable", "Grimm", "Halo", "Iris"
            };
            long playerPower = 0;
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
                playerPower = HeroProgress.TeamPower(teamData);
            }
            else
            {
                playerPower = 1500 + World.CampaignSystem.ClearedStageCount() * 350;
            }

            for (int i = 0; i < names.Length; i++)
            {
                _board.Add(new RankEntry
                {
                    name = names[i],
                    power = (long)(playerPower * UnityEngine.Random.Range(0.55f, 2.4f)) + 800 + i * 137,
                    stage = Mathf.Max(1, World.CampaignSystem.ClearedStageCount() + UnityEngine.Random.Range(-8, 15)),
                    isPlayer = false
                });
            }
            _board.Add(new RankEntry { name = PlayerProfile.Name, power = playerPower, stage = World.CampaignSystem.ClearedStageCount(), isPlayer = true });
            _board.Sort((a, b) => b.power.CompareTo(a.power));
            OnChanged?.Invoke();
        }

        public static int PlayerRank()
        {
            for (int i = 0; i < _board.Count; i++)
            {
                if (_board[i].isPlayer) return i + 1;
            }
            return -1;
        }
    }
}
