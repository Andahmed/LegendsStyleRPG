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
            public int schemaVersion = 1;
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

            if (OnLoaded != null) OnLoaded();
            return true;
        }

        private static void SeedFirstRun()
        {
            // The player starts with one hero: Kael the Flameblade.
            Current.unlockedHeroIds.Add("kael_flameblade");
            Current.playerLevel = 1;
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
