using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Local JSON save system (Application.persistentDataPath).
    /// Designed so the same schema can later sync to the Python backend
    /// (cloud save / multi-device) without changing callers.
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
            public string lastSaveUtc = "";

            // ---- economy / inventory (schema 2) ----
            public int gold = 0;
            public int gems = 0;
            public List<string> itemIds = new List<string>();
            public List<int> itemCounts = new List<int>();

            // ---- daily systems ----
            public string dayKey = "";
            public List<int> missionProgress = new List<int>();
            public List<int> missionClaimed = new List<int>();
            public List<string> shopKeys = new List<string>();
            public List<int> shopCounts = new List<int>();

            // ---- summoning + loadout (schema 3) ----
            public int gachaTotalPulls = 0;
            public List<int> gachaPity = new List<int>();      // pulls since last guaranteed Legendary, per banner
            public string wishElement = "";                     // Faction Wish element (after unlock)
            public List<string> teamLoadout = new List<string>(); // battle team order (max 6)
            public bool summonGiftGiven = false;
            // ---- per-hero progress + equipment (v0.5) ----
            public List<string> levelHeroIds = new List<string>();
            public List<int> levelValues = new List<int>();
            public List<string> equipHero = new List<string>();
            public List<int> equipSlot = new List<int>();
            public List<string> equipItem = new List<string>();
            // ---- idle / offline progression (schema 4) ----
            public string idleLastUtc = "";
            public int idlePendingGold = 0;
            public int idlePendingExp = 0;
            public int idlePendingSeconds = 0;
            public int schemaVersion = 4;
        }

        private static string FilePath
        {
            get { return Path.Combine(Application.persistentDataPath, "legends_save_v1.json"); }
        }

        public static SaveData Current { get; private set; } = new SaveData();

        public static event Action OnLoaded;

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
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Load failed, starting fresh: " + e.Message);
                Current = new SaveData();
                SeedFirstRun();
            }

            Migrate();
            Inventory.MigrateAndSeed();
            IdleProgression.Reconcile();
            if (OnLoaded != null) OnLoaded();
            return true;
        }

        private static void SeedFirstRun()
        {
            // The player starts with one hero: Kael the Flameblade.
            Current.unlockedHeroIds.Add("kael_flameblade");
            Current.playerLevel = 1;
        }

        private static void Migrate()
        {
            var s = Current;
            if (s.gachaPity == null) s.gachaPity = new List<int>();
            if (s.teamLoadout == null) s.teamLoadout = new List<string>();

            // v0.2/v0.3 saves (schema <= 2): grant the v0.4 summon launch gift once
            if (s.schemaVersion < 3 && !s.summonGiftGiven)
            {
                s.gems += 3200;
                s.summonGiftGiven = true;
                Debug.Log("[SaveSystem] v0.4 migration: +3200 gems summon launch gift");
            }
            s.schemaVersion = 4;
        }

        public static void Save(World.TowerSystem tower, Team.TeamManager team)
        {
            try
            {
                if (tower != null)
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

                Current.lastSaveUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                IdleProgression.MarkSessionSaved();
                var json = JsonUtility.ToJson(Current, true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SaveSystem] Save failed: " + e.Message);
            }
        }

        public static string GetSavePathForDocs()
        {
            return FilePath;
        }
    }
}
