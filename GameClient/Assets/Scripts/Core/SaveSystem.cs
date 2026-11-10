using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Local JSON save system (Application.persistentDataPath).
    /// v3 schema: every meta system (economy, heroes, campaign, quests, shop,
    /// sign-in, events, tech, forge, guild, arena, mine, beasts...) persists
    /// here and syncs to the Python backend through the same shape.
    /// </summary>
    public static class SaveSystem
    {
        [Serializable]
        public class TowerProgress
        {
            public int difficulty;      // 0=Easy 1=Hard 2=Hell 3=Nightmare
            public int bestFloor;
        }

        [Serializable]
        public class EquippedItemRecord
        {
            public string heroId;
            public int slot;       // 0=Weapon 1=Armor 2=Amulet 3=Wings 4=Pet 5=Mount
            public string itemId;
        }

        [Serializable]
        public class HeroRecordSave
        {
            public string heroId;
            public int level;
            public int shards;
            public int stars;
            public int dupes;
        }

        [Serializable]
        public class CampaignStarSave
        {
            public string key;
            public int stars;
        }

        [Serializable]
        public class ForgeRecordSave
        {
            public string itemId;
            public int level;
        }

        [Serializable]
        public class ShopSlotSave
        {
            public string key;
            public string itemId;
            public string heroId;
            public int heroShards;
            public long goldCost;
            public int diamondCost;
            public int discount;
            public bool bought;
        }

        [Serializable]
        public class BeastSave
        {
            public string id;
            public int level;
            public bool unlocked;
        }

        [Serializable]
        public class SaveData
        {
            public int playerLevel = 1;
            public float exp = 0f;
            public List<string> unlockedHeroIds = new List<string>();
            public List<int> teamOrder = new List<int>();
            public int activeDifficulty = 0;
            public List<TowerProgress> tower = new List<TowerProgress>();
            public float qualityPref = 1f;   // 0 Low 1 Med 2 High 3 Ultra
            public float musicVolume = 0.6f;
            public float sfxVolume = 0.9f;
            public string lastSaveUtc = "";
            public int schemaVersion = 3;

            // v2 — items / equipment
            public List<string> ownedItemIds = new List<string>();
            public List<EquippedItemRecord> equipped = new List<EquippedItemRecord>();
            public bool starterKitGranted = false;

            // v3 — economy & profile
            public long gold = 2500;
            public long diamonds = 600;
            public int vipLevel = 0;
            public float vipExp = 0f;
            public long lifetimeGold = 0;
            public string playerName = "Legend";
            public string playerId = "";
            public string activeTitle = "";
            public List<string> titles = new List<string>();

            // v3 — hero progression & formation
            public List<HeroRecordSave> heroRecords = new List<HeroRecordSave>();
            public List<string> teamHeroIds = new List<string>();

            // v3 — idle income
            public long afkLastClaimUtc = 0;

            // v3 — tech & forge
            public List<int> techLevels = new List<int>();
            public List<ForgeRecordSave> forgeLevels = new List<ForgeRecordSave>();
            public int forgeShards = 0;

            // v3 — daily quests
            public string questDay = "";
            public List<int> questProgress = new List<int>();
            public List<string> questClaimed = new List<string>();

            // v3 — shop & merchant
            public long shopRefreshUtc = 0;
            public List<ShopSlotSave> shopSlots = new List<ShopSlotSave>();
            public long merchantRefreshUtc = 0;
            public long freeDiamondClaimUtc = 0;

            // v3 — sign-in & events
            public int signinCycle = 1;
            public string signinLastClaimDay = "";
            public List<int> signinClaimed = new List<int>();
            public int eventPoints = 0;
            public List<int> eventClaimed = new List<int>();

            // v3 — campaign & continent gate
            public List<CampaignStarSave> campaignStars = new List<CampaignStarSave>();
            public List<CampaignStarSave> continentStars = new List<CampaignStarSave>();
            public string worldBossDay = "";
            public List<int> worldBossAttempts = new List<int>();
            public List<int> worldBossRewardsClaimed = new List<int>();

            // v3 — dungeons
            public string dungeonDay = "";
            public int goldAttempts = 0;
            public int expAttempts = 0;

            // v3 — arena
            public int arenaRank = 950;
            public int arenaWins = 0;
            public int arenaLosses = 0;
            public int arenaSeasonWins = 0;
            public long arenaSeasonStartUtc = 0;

            // v3 — mine & beasts
            public int mineLevel = 1;
            public long mineLastClaimUtc = 0;
            public int beastSnacks = 3;
            public List<BeastSave> beasts = new List<BeastSave>();

            // v3 — guild
            public string guildName = "";
            public int guildLevel = 1;
            public long guildFunds = 0;
            public long huntBossHp = 300000;
            public long huntBossMaxHp = 300000;
            public int guildTechA = 0;
            public int guildTechD = 0;
            public int guildTechH = 0;
            public long caravanEndUtc = 0;
            public int caravanReward = 0;
            public bool caravanCollected = false;
            public string giftDay = "";
            public bool giftAvailable = true;
            public int warPoints = 0;
            public int warWins = 0;

            // v3 — summon pity
            public int pullsWithoutEpic = 0;
            public int pullsWithoutLegendary = 0;
            public int totalPulls = 0;

            // legacy (menu UI / Inventory / DailyMissions / DailyShop / Equipment / Gacha / Idle)
            public int gems = 0;
            public List<string> itemIds = new List<string>();
            public List<int> itemCounts = new List<int>();
            public string dayKey = "";
            public List<int> missionProgress = new List<int>();
            public List<int> missionClaimed = new List<int>();
            public List<string> shopKeys = new List<string>();
            public List<int> shopCounts = new List<int>();
            public int merchantBoughtCount = 0;
            public List<string> levelHeroIds = new List<string>();
            public List<int> levelValues = new List<int>();
            public List<string> equipHero = new List<string>();
            public List<int> equipSlot = new List<int>();
            public List<string> equipItem = new List<string>();
            public List<int> gachaPity = new List<int>();
            public int gachaTotalPulls = 0;
            public string wishElement = "";
            public List<string> teamLoadout = new List<string>();
            public int idlePendingGold = 0;
            public int idlePendingExp = 0;
            public int idlePendingSeconds = 0;
            public string idleLastUtc = "";
        }

        private static string FilePath
        {
            get { return Path.Combine(Application.persistentDataPath, "legends_save_v1.json"); }
        }

        public static SaveData Current { get; private set; } = new SaveData();

        public static event Action OnLoaded;

        private static bool _dirty;

        /// <summary>Requests a save on the next autosave tick (cheap during battle).</summary>
        public static void MarkDirty()
        {
            _dirty = true;
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    Current = JsonUtility.FromJson<SaveData>(json);
                    if (Current == null) Current = new SaveData();
                }
                else
                {
                    Current = new SaveData();
                    SeedFirstRun();
                }
                Normalize();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Load failed, starting fresh: " + e.Message);
                Current = new SaveData();
                SeedFirstRun();
                Normalize();
            }

            if (OnLoaded != null) OnLoaded();
            return true;
        }

        /// <summary>Older saves carry the new fields as null — JsonUtility leaves them null.</summary>
        private static void Normalize()
        {
            if (Current.unlockedHeroIds == null) Current.unlockedHeroIds = new List<string>();
            if (Current.teamOrder == null) Current.teamOrder = new List<int>();
            if (Current.tower == null) Current.tower = new List<TowerProgress>();
            if (Current.ownedItemIds == null) Current.ownedItemIds = new List<string>();
            if (Current.equipped == null) Current.equipped = new List<EquippedItemRecord>();
            if (Current.heroRecords == null) Current.heroRecords = new List<HeroRecordSave>();
            if (Current.teamHeroIds == null) Current.teamHeroIds = new List<string>();
            if (Current.titles == null) Current.titles = new List<string>();
            if (Current.techLevels == null) Current.techLevels = new List<int>();
            if (Current.forgeLevels == null) Current.forgeLevels = new List<ForgeRecordSave>();
            if (Current.forgeShards < 0) Current.forgeShards = 0;
            if (Current.questProgress == null) Current.questProgress = new List<int>();
            if (Current.questClaimed == null) Current.questClaimed = new List<string>();
            if (Current.shopSlots == null) Current.shopSlots = new List<ShopSlotSave>();
            if (Current.signinClaimed == null) Current.signinClaimed = new List<int>();
            if (Current.eventClaimed == null) Current.eventClaimed = new List<int>();
            if (Current.campaignStars == null) Current.campaignStars = new List<CampaignStarSave>();
            if (Current.continentStars == null) Current.continentStars = new List<CampaignStarSave>();
            if (Current.worldBossDay == null) Current.worldBossDay = "";
            if (Current.worldBossAttempts == null) Current.worldBossAttempts = new List<int>();
            if (Current.worldBossRewardsClaimed == null) Current.worldBossRewardsClaimed = new List<int>();
            if (Current.beasts == null) Current.beasts = new List<BeastSave>();
            if (Current.itemIds == null) Current.itemIds = new List<string>();
            if (Current.itemCounts == null) Current.itemCounts = new List<int>();
            if (Current.missionProgress == null) Current.missionProgress = new List<int>();
            if (Current.missionClaimed == null) Current.missionClaimed = new List<int>();
            if (Current.shopKeys == null) Current.shopKeys = new List<string>();
            if (Current.shopCounts == null) Current.shopCounts = new List<int>();
            if (Current.levelHeroIds == null) Current.levelHeroIds = new List<string>();
            if (Current.levelValues == null) Current.levelValues = new List<int>();
            if (Current.equipHero == null) Current.equipHero = new List<string>();
            if (Current.equipSlot == null) Current.equipSlot = new List<int>();
            if (Current.equipItem == null) Current.equipItem = new List<string>();
            if (Current.gachaPity == null) Current.gachaPity = new List<int>();
            if (Current.teamLoadout == null) Current.teamLoadout = new List<string>();
            if (Current.dayKey == null) Current.dayKey = "";
            if (Current.wishElement == null) Current.wishElement = "";
            if (Current.idleLastUtc == null) Current.idleLastUtc = "";

            if (Current.gold <= 0) Current.gold = 2500;
            if (Current.diamonds <= 0) Current.diamonds = 600;
            if (string.IsNullOrEmpty(Current.playerName)) Current.playerName = "Legend";

            // every unlocked hero must have a progression record
            foreach (var id in Current.unlockedHeroIds)
            {
                bool found = false;
                foreach (var hr in Current.heroRecords)
                {
                    if (hr != null && hr.heroId == id) { found = true; break; }
                }
                if (!found) Current.heroRecords.Add(new HeroRecordSave { heroId = id, level = 1, shards = 0, stars = 1 });
            }
        }

        private static void SeedFirstRun()
        {
            // The player starts with one hero: Kael the Flameblade.
            Current.unlockedHeroIds.Add("kael_flameblade");
            Current.playerLevel = 1;
            Current.teamHeroIds.Add("kael_flameblade");
        }

        public static void Save(World.TowerSystem tower, Team.TeamManager team)
        {
            try
            {
                // A mid-battle autosave must not record a campaign, dungeon or
                // World Boss floor as Tower progress. Tower progress is committed
                // after BattleSession restores the saved Tower state on victory.
                if (tower != null && !BattleSession.Active)
                {
                    Current.activeDifficulty = (int)tower.Difficulty;
                    TowerProgress tp = null;
                    foreach (var p in Current.tower)
                    {
                        if (p.difficulty == (int)tower.Difficulty) { tp = p; break; }
                    }
                    if (tp == null)
                    {
                        tp = new TowerProgress { difficulty = (int)tower.Difficulty };
                        Current.tower.Add(tp);
                    }
                    tp.bestFloor = Mathf.Max(tp.bestFloor, tower.CurrentFloor);
                }

                if (team != null)
                {
                    Current.teamOrder.Clear();
                    foreach (var h in team.Heroes)
                    {
                        Current.teamOrder.Add(team.UnlockedHeroes.Contains(h.Data.heroId) ? 1 : 0);
                    }
                }

                Items.InventorySystem.WriteInto(Current);

                // ---- v1.0: flush every meta system ----
                Economy.WriteInto(Current);
                PlayerProfile.WriteInto(Current);
                HeroProgress.WriteInto(Current);
                AFKReward.WriteInto(Current);
                TechSystem.WriteInto(Current);
                ForgeSystem.WriteInto(Current);
                QuestSystem.WriteInto(Current);
                ShopSystem.WriteInto(Current);
                SignInSystem.WriteInto(Current);
                LimitedEvent.WriteInto(Current);
                MerchantSystem.WriteInto(Current);
                SummonSystem.WriteInto(Current);
                World.CampaignSystem.WriteInto(Current);
                World.ContinentGateSystem.WriteInto(Current);
                World.WorldBossSystem.WriteInto(Current);
                World.DungeonSystem.WriteInto(Current);
                World.ArenaSystem.WriteInto(Current);
                World.MineSystem.WriteInto(Current);
                World.BeastSystem.WriteInto(Current);
                Social.GuildSystem.WriteInto(Current);

                Current.lastSaveUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var json = JsonUtility.ToJson(Current, true);
                File.WriteAllText(FilePath, json);
                _dirty = false;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Save failed: " + e.Message);
            }
        }

        /// <summary>Immediate save used after discrete meta actions.</summary>
        public static void SaveNow()
        {
            var gm = GameManager.Instance;
            Save(gm != null ? gm.Tower : null, gm != null ? gm.Team : null);
        }

        public static bool ConsumeDirty()
        {
            bool d = _dirty;
            _dirty = false;
            return d;
        }

        public static string GetSavePathForDocs()
        {
            return FilePath;
        }
    }
}
