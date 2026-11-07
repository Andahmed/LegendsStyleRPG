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

            bool bossFloor = tower.CurrentFloor % GameConfig.BossEveryNFloors == 0;
            int count = bossFloor ? 2 : Mathf.Min(3 + (tower.CurrentFloor - 1) / 4, 9);

            _clearing = false;
            _alive.Clear();
            _waveActive = true;

            string[] pool = { "goblin", "wolf", "shaman", "golem", "wraith" };

            for (int i = 0; i < count; i++)
            {
                float angle = (i * 360f / count + tower.CurrentFloor * 17f) * Mathf.Deg2Rad;
                float radius = bossFloor ? 6f : 9f + (i % 3) * 1.6f;
                Vector3 pos = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);

                string key = bossFloor
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
            m.Setup(data, TowerSystem.CurrentHpScale, TowerSystem.CurrentAtkScale,
                TowerSystem.CurrentExpScale, pos);
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
