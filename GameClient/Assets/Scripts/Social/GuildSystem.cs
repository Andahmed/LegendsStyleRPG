using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;

namespace LegendsRPG.Social
{
    /// <summary>
    /// Full guild experience: create/join, donations, guild tech, guild hunt
    /// (shared boss HP across the whole guild), caravan dispatch, guild war,
    /// daily gifts and the wish wall. Members are simulated players so the
    /// hall feels alive, and every panel performs real state changes.
    /// </summary>
    public static class GuildSystem
    {
        public class Member
        {
            public string name;
            public long power;
            public bool isPlayer;
            public string role;   // "Leader" / "Officer" / "Member"
        }

        public class Wish
        {
            public string owner;
            public string itemName;
            public DateTime postedUtc;
            public bool fulfilled;
        }

        public static bool InGuild { get; private set; }
        public static string GuildName { get; private set; } = "";
        public static int GuildLevel { get; private set; } = 1;
        public static long GuildFunds { get; private set; }
        public static long HuntBossMaxHp { get; private set; } = 300000;
        public static long HuntBossHp { get; private set; } = 300000;
        public static string HuntBossName { get; private set; } = "Ember Titan";

        public static int TechAttack { get; private set; }
        public static int TechDefense { get; private set; }
        public static int TechHp { get; private set; }

        public static DateTime CaravanEndUtc { get; private set; } = DateTime.MinValue;
        public static int CaravanReward { get; private set; }
        public static bool CaravanActive => DateTime.UtcNow < CaravanEndUtc;
        public static bool CaravanReady => !CaravanActive && CaravanEndUtc > DateTime.MinValue && !_caravanCollected;
        private static bool _caravanCollected;

        public static bool GiftAvailable { get; private set; } = true;
        private static string _giftDay = "";

        public static int WarPoints { get; private set; }
        public static int WarWins { get; private set; }

        public static readonly List<Member> Members = new List<Member>();
        public static readonly List<Wish> Wishes = new List<Wish>();

        public static event Action OnChanged;

        // ---------------- membership ----------------
        public static void Create(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length < 3 || name.Length > 16)
            {
                CombatEvents.RaiseNotification("Guild name must be 3-16 letters");
                Audio.AudioSystem.Play("Error");
                return;
            }
            GuildName = name;
            InGuild = true;
            GuildLevel = 1;
            GuildFunds = 0;
            HuntBossMaxHp = 300000;
            HuntBossHp = HuntBossMaxHp;
            BuildRoster();
            SeedWishes();
            CombatEvents.RaiseNotification("Guild \"" + name + "\" founded!");
            Audio.AudioSystem.Play("Fanfare");
            OnChanged?.Invoke();
        }

        public static void Join(string name)
        {
            GuildName = string.IsNullOrEmpty(name) ? "Legends United" : name;
            InGuild = true;
            GuildLevel = UnityEngine.Random.Range(2, 6);
            GuildFunds = 20000 * GuildLevel;
            HuntBossMaxHp = 300000 + GuildLevel * 120000;
            HuntBossHp = HuntBossMaxHp;
            BuildRoster();
            SeedWishes();
            CombatEvents.RaiseNotification("Joined \"" + GuildName + "\" — welcome!");
            Audio.AudioSystem.Play("Fanfare");
            OnChanged?.Invoke();
        }

        public static void Leave()
        {
            InGuild = false;
            GuildName = "";
            Members.Clear();
            Wishes.Clear();
            CaravanEndUtc = DateTime.MinValue;
            _caravanCollected = false;
            WarPoints = 0;
            OnChanged?.Invoke();
        }

        private static void BuildRoster()
        {
            Members.Clear();
            string[] names =
            {
                "Zaid", "Omar", "Layla", "Karim", "Nour", "Yousef", "Salma", "Tariq", "Adam", "Hana", "Mira", "Khalid"
            };
            var rnd = new System.Random(GuildName.GetHashCode());
            long myPower = World.ArenaSystem.EstimatePlayerPower();

            Members.Add(new Member { name = PlayerProfile.Name, power = myPower, isPlayer = true, role = "Member" });

            int count = 6 + rnd.Next(0, 5);
            for (int i = 0; i < count; i++)
            {
                Members.Add(new Member
                {
                    name = names[rnd.Next(names.Length)] + rnd.Next(2, 99),
                    power = (long)(myPower * UnityEngine.Random.Range(0.4f, 1.6f)),
                    isPlayer = false,
                    role = "Member"
                });
            }
            Members[1].role = "Leader";
            if (Members.Count > 3) Members[2].role = "Officer";
            Members.Sort((a, b) =>
            {
                int ra = a.role == "Leader" ? 0 : a.role == "Officer" ? 1 : 2;
                int rb = b.role == "Leader" ? 0 : b.role == "Officer" ? 1 : 2;
                if (ra != rb) return ra.CompareTo(rb);
                return b.power.CompareTo(a.power);
            });
        }

        private static void SeedWishes()
        {
            Wishes.Clear();
            var rnd = new System.Random(GuildName.GetHashCode() + 5);
            string[] items = { "Flame Sword +3", "Dragon Plate", "Wind Amulet", "Phoenix Wings", "Void Relic" };
            for (int i = 0; i < 3 && i < Members.Count; i++)
            {
                Wishes.Add(new Wish
                {
                    owner = Members[i].name,
                    itemName = items[rnd.Next(items.Length)],
                    postedUtc = DateTime.UtcNow.AddMinutes(-rnd.Next(5, 90)),
                    fulfilled = false
                });
            }
        }

        // ---------------- donation ----------------
        public static long DonationCost => 1000;

        public static bool Donate()
        {
            if (!InGuild) return false;
            if (!Economy.TrySpendGold(DonationCost)) return false;
            GuildFunds += DonationCost * 2;

            long needed = GuildLevel * 40000;
            if (GuildFunds >= needed && GuildLevel < 10)
            {
                GuildFunds -= needed;
                GuildLevel++;
                CombatEvents.RaiseNotification("Guild leveled up to " + GuildLevel + "!");
                Audio.AudioSystem.Play("LevelUp");
            }
            Audio.AudioSystem.Play("Coin");
            OnChanged?.Invoke();
            return true;
        }

        // ---------------- guild tech ----------------
        public static long TechCost(int branch)
        {
            int lv = branch == 0 ? TechAttack : branch == 1 ? TechDefense : TechHp;
            return (long)(3000 * Mathf.Pow(1.7f, lv));
        }

        public static bool BuyTech(int branch)
        {
            if (!InGuild) return false;
            if (GuildFunds < TechCost(branch))
            {
                CombatEvents.RaiseNotification("Not enough guild funds — donate gold!");
                Audio.AudioSystem.Play("Error");
                return false;
            }
            GuildFunds -= TechCost(branch);
            if (branch == 0) TechAttack++;
            else if (branch == 1) TechDefense++;
            else TechHp++;
            Audio.AudioSystem.Play("Fanfare");
            OnChanged?.Invoke();
            return true;
        }

        // ---------------- guild hunt ----------------
        public static void StartHunt()
        {
            if (!InGuild) return;
            if (HuntBossHp <= 0)
            {
                CombatEvents.RaiseNotification("Boss already slain — resets weekly!");
                Audio.AudioSystem.Play("Error");
                return;
            }

            var cfg = new BattleSession.Config
            {
                type = BattleSession.BattleType.GuildHunt,
                title = "Guild Hunt: " + HuntBossName,
                floor = 40 + GuildLevel * 8,
                difficulty = 2,
                boss = true,
                monsterCount = 1,
                forcedMonsterKey = "guild_hunt_boss",
                onWin = () => { },
                onLose = () => { }
            };
            BattleSession.Start(cfg);
        }

        /// <summary>Applies real battle damage to the shared guild boss.</summary>
        public static void RecordHuntDamage(long damage)
        {
            HuntBossHp = Math.Max(0, HuntBossHp - damage);
            if (HuntBossHp == 0)
            {
                CombatEvents.RaiseNotification("GUILD BOSS SLAIN!");
                Economy.AddDiamonds(150, "Boss slain");
                PlayerProfile.UnlockTitle("Boss Slayer");
            }
            long gold = 300 + damage / 50;
            Economy.AddGold(gold, "Hunt");
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
        }

        // ---------------- caravan ----------------
        public static void DispatchCaravan(int durationMinutes)
        {
            if (!InGuild) return;
            if (CaravanActive || CaravanReady)
            {
                CombatEvents.RaiseNotification("Caravan already on the road!");
                Audio.AudioSystem.Play("Error");
                return;
            }
            if (!Economy.TrySpendGold(500)) return;

            CaravanEndUtc = DateTime.UtcNow.AddMinutes(durationMinutes);
            CaravanReward = 750 * durationMinutes / 15 * GuildLevel;
            _caravanCollected = false;
            CombatEvents.RaiseNotification("Caravan dispatched — returns in " + durationMinutes + " min");
            Audio.AudioSystem.Play("UI");
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
        }

        public static bool CollectCaravan()
        {
            if (!CaravanReady) return false;
            Economy.AddGold(CaravanReward, "Caravan");
            _caravanCollected = true;
            CaravanEndUtc = DateTime.MinValue;
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
            return true;
        }

        // ---------------- guild war ----------------
        public static void StartWar()
        {
            if (!InGuild) return;
            var cfg = new BattleSession.Config
            {
                type = BattleSession.BattleType.GuildHunt,
                title = "Guild War: Rival Vanguard",
                floor = 55 + GuildLevel * 6,
                difficulty = 2,
                monsterCount = 4,
                onWin = () =>
                {
                    WarWins++;
                    WarPoints += 100;
                    Economy.AddDiamonds(60, "Guild war");
                    Economy.AddGold(2000, "Guild war");
                    if (WarWins == 3) PlayerProfile.UnlockTitle("Warlord");
                    SaveSystem.MarkDirty();
                    UI.UIManager.Instance.ShowBattleResult(true, "Rival guild defeated!",
                        new[] { "+100 War Points", "+60 Diamonds", "+2,000 Gold" });
                }
            };
            BattleSession.Start(cfg);
        }

        // ---------------- gifts ----------------
        public static void DailyGiftCheck()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (_giftDay != today)
            {
                _giftDay = today;
                GiftAvailable = InGuild;
            }
        }

        public static bool ClaimGift()
        {
            DailyGiftCheck();
            if (!InGuild || !GiftAvailable) return false;
            GiftAvailable = false;
            Economy.AddGold(800, "Guild gift");
            Economy.AddDiamonds(15, "Guild gift");
            World.BeastSystem.GrantSnack(1);
            Audio.AudioSystem.Play("Coin");
            SaveSystem.MarkDirty();
            OnChanged?.Invoke();
            return true;
        }

        // ---------------- wish wall ----------------
        public static void PostWish(string itemName)
        {
            if (!InGuild || string.IsNullOrEmpty(itemName)) return;
            Wishes.Add(new Wish { owner = PlayerProfile.Name, itemName = itemName, postedUtc = DateTime.UtcNow, fulfilled = false });
            CombatEvents.RaiseNotification("Wish posted on the wall");
            OnChanged?.Invoke();
        }

        public static void TickWishes()
        {
            if (!InGuild) return;
            foreach (var w in Wishes)
            {
                if (w.fulfilled) continue;
                if ((DateTime.UtcNow - w.postedUtc).TotalMinutes >= 2)
                {
                    w.fulfilled = true;
                    if (w.owner == PlayerProfile.Name)
                    {
                        Items.InventorySystem.Grant(Items.ItemDatabase.RollRandomItemId());
                        CombatEvents.RaiseNotification("Wish granted: " + w.itemName + "!");
                        Audio.AudioSystem.Play("Fanfare");
                        SaveSystem.MarkDirty();
                    }
                }
            }
        }

        // ---------------- persistence ----------------
        public static void WriteInto(SaveSystem.SaveData s)
        {
            s.guildName = GuildName;
            s.guildLevel = GuildLevel;
            s.guildFunds = GuildFunds;
            s.huntBossHp = HuntBossHp;
            s.huntBossMaxHp = HuntBossMaxHp;
            s.guildTechA = TechAttack;
            s.guildTechD = TechDefense;
            s.guildTechH = TechHp;
            s.caravanEndUtc = CaravanEndUtc.Ticks;
            s.caravanReward = CaravanReward;
            s.caravanCollected = _caravanCollected;
            s.giftDay = _giftDay;
            s.giftAvailable = GiftAvailable;
            s.warPoints = WarPoints;
            s.warWins = WarWins;
        }

        public static void ReadFrom(SaveSystem.SaveData s)
        {
            InGuild = !string.IsNullOrEmpty(s.guildName);
            GuildName = s.guildName ?? "";
            GuildLevel = Mathf.Max(1, s.guildLevel);
            GuildFunds = Math.Max(0, s.guildFunds);
            HuntBossMaxHp = Math.Max(100000, s.huntBossMaxHp);
            HuntBossHp = Math.Min(Math.Max(0L, s.huntBossHp), HuntBossMaxHp);
            TechAttack = Mathf.Max(0, s.guildTechA);
            TechDefense = Mathf.Max(0, s.guildTechD);
            TechHp = Mathf.Max(0, s.guildTechH);
            CaravanEndUtc = s.caravanEndUtc > 0 ? new DateTime(s.caravanEndUtc, DateTimeKind.Utc) : DateTime.MinValue;
            CaravanReward = Mathf.Max(0, s.caravanReward);
            _caravanCollected = s.caravanCollected;
            _giftDay = s.giftDay ?? "";
            GiftAvailable = s.giftAvailable;
            WarPoints = Mathf.Max(0, s.warPoints);
            WarWins = Mathf.Max(0, s.warWins);

            if (InGuild) BuildRoster();
            DailyGiftCheck();
        }
    }
}
