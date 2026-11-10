using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;

namespace LegendsRPG.World
{
    /// <summary>
    /// Spawns monster waves for the current tower floor.
    /// Composition and scaling are derived from floor/difficulty formulas:
    /// every 10th floor is a boss floor. No hand-authored wave tables.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        public event Action<int, bool> WaveStarted;   // (count, isBossFloor)

        // ---- v1.0 battle-session overrides ----
        public static Vector3 ArenaCenter = new Vector3(0f, 0f, 600f);
        public string ForcedMonsterKey = "";
        public int ForcedMonsterCount = 0;
        public float ExtraHpScale = 1f;
        public float ExtraAtkScale = 1f;
        public bool OverrideBossFloorRules;
        public bool BossFloorOverride;
        public int CampaignChapter;

        /// <summary>Registers a runtime MonsterData (e.g. arena shadows, guild boss).</summary>
        public void AddMonster(MonsterData data)
        {
            if (_monsters != null && data != null) _monsters[data.monsterId] = data;
        }

        private readonly List<MonsterCharacter> _alive = new List<MonsterCharacter>();
        private Dictionary<string, MonsterData> _monsters;
        private Transform _container;
        private bool _waveActive;
        private bool _clearing;

        public void Init(Dictionary<string, MonsterData> monsters)
        {
            _monsters = monsters;
            _container = new GameObject("Monsters").transform;
        }

        public void StartFloor()
        {
            if (GameManager.Instance == null || GameManager.Instance.Tower == null) return;
            var tower = GameManager.Instance.Tower;

            ClearLeftovers();

            bool bossFloor = OverrideBossFloorRules
                ? BossFloorOverride
                : tower.CurrentFloor % GameConfig.BossEveryNFloors == 0 || !string.IsNullOrEmpty(ForcedMonsterKey);
            int count = ForcedMonsterCount > 0 ? ForcedMonsterCount
                : !string.IsNullOrEmpty(ForcedMonsterKey) ? 1
                : bossFloor ? 2 : Mathf.Min(3 + (tower.CurrentFloor - 1) / 4, 9);

            _clearing = false;
            _alive.Clear();
            _waveActive = true;

            string[] defaultPool = { "goblin", "wolf", "shaman", "golem", "wraith",
                                     "storm_hawk", "coral_crab", "vine_spider", "sun_eagle" };
            string[][] chapterPools =
            {
                new[] { "goblin", "golem", "vine_spider" },
                new[] { "shaman", "coral_crab", "wolf" },
                new[] { "wraith", "vine_spider", "golem" },
                new[] { "wolf", "shaman", "wraith" },
                new[] { "coral_crab", "shaman", "golem" },
                new[] { "storm_hawk", "wolf", "goblin" },
                new[] { "goblin", "golem", "wraith" },
                new[] { "wraith", "storm_hawk", "shaman" },
                new[] { "golem", "vine_spider", "sun_eagle" },
                new[] { "wraith", "golem", "storm_hawk" },
                new[] { "sun_eagle", "shaman", "golem" },
                new[] { "wraith", "storm_hawk", "golem" }
            };
            string[] pool = CampaignChapter >= 1 && CampaignChapter <= chapterPools.Length
                ? chapterPools[CampaignChapter - 1] : defaultPool;

            for (int i = 0; i < count; i++)
            {
                float angle = (i * 360f / count + tower.CurrentFloor * 17f) * Mathf.Deg2Rad;
                float radius = bossFloor ? 6f : 9f + (i % 3) * 1.6f;
                Vector3 pos = ArenaCenter + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);

                string key = !string.IsNullOrEmpty(ForcedMonsterKey)
                    ? ForcedMonsterKey
                    : bossFloor
                        ? (tower.CurrentFloor % 20 == 0 ? "boss_lich" : "boss_titan")
                        : pool[(tower.CurrentFloor + i) % pool.Length];

                SpawnMonster(key, pos);
            }

            if (WaveStarted != null) WaveStarted(_alive.Count, bossFloor);
            CombatEvents.RaiseNotification("Floor " + tower.CurrentFloor + " — " +
                (_alive.Count) + (bossFloor ? " (BOSS FLOOR)" : " enemies"));
        }

        private void SpawnMonster(string key, Vector3 pos)
        {
            if (_monsters == null || !_monsters.ContainsKey(key)) return;
            var data = _monsters[key];
            var tower = GameManager.Instance.Tower;

            var go = new GameObject("MonsterRoot");
            var m = go.AddComponent<MonsterCharacter>();
            float hpS = TowerSystem.CurrentHpScale * ExtraHpScale;
            float atkS = TowerSystem.CurrentAtkScale * ExtraAtkScale;
            m.Setup(data, hpS, atkS, TowerSystem.CurrentExpScale, pos);
            go.transform.SetParent(_container, false);

            m.MonsterDied += OnMonsterDied;
            _alive.Add(m);
        }

        private void OnMonsterDied(MonsterCharacter m)
        {
            _alive.Remove(m);
            if (_waveActive && _alive.Count == 0 && !_clearing)
            {
                _clearing = true;
                _waveActive = false;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnFloorCleared();
                }
            }
        }

        private void ClearLeftovers()
        {
            for (int i = _alive.Count - 1; i >= 0; i--)
            {
                if (_alive[i] != null) Destroy(_alive[i].gameObject);
            }
            _alive.Clear();

            // safety net: destroy any orphaned monsters from a previous floor
            for (int i = MonsterCharacter.All.Count - 1; i >= 0; i--)
            {
                var m = MonsterCharacter.All[i];
                if (m != null) Destroy(m.gameObject);
            }
        }
    }
}
