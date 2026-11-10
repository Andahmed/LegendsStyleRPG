using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Team;
using LegendsRPG.World;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Launches every battle in the game (campaign stages, tower floors,
    /// dungeons, arena, guild hunt, continent gate). The city stays at the
    /// origin; the battle arena lives at (0,0,600). BattleSession handles
    /// team teleport, camera swap, wave overrides, win/lose callbacks and
    /// the victory/defeat result panel.
    /// </summary>
    public static class BattleSession
    {
        public enum BattleType
        {
            Campaign, Tower, DungeonGold, DungeonExp, Arena, GuildHunt, ContinentGate
        }

        public class Config
        {
            public BattleType type;
            public string title = "Battle";
            public int floor = 1;
            public int difficulty = 0;
            public bool boss = false;
            public float hpScale = 1f;
            public float atkScale = 1f;
            public string forcedMonsterKey = "";
            public int monsterCount = 0;          // 0 = formula
            public Action onWin;
            public Action onLose;

            // retry context — lets every battle type rebuild its own callbacks
            public int chapter;
            public int stage;
            public int region;
            public int tier;
            public bool goldDungeon;
            public object context;                 // e.g. ArenaSystem.Opponent
        }

        public static bool Active { get; private set; }
        public static Config Current { get; private set; }

        public const float ArenaOffsetZ = 600f;
        public static Vector3 ArenaCenter => new Vector3(0f, 0f, ArenaOffsetZ);

        private static int _savedTowerFloor;
        private static int _savedTowerDifficulty;
        private static Vector3 _savedCityPos;

        /// <summary>Fired when a battle ends so UI can show the result panel.</summary>
        public static event Action<bool, Config> BattleEnded;

        /// <summary>Heroes that died during the current battle (used for star ratings).</summary>
        public static int DeathsThisBattle { get; private set; }

        static BattleSession()
        {
            Combat.CombatEvents.CharacterDied += _ =>
            {
                if (Active) DeathsThisBattle++;
            };
        }

        // ----------------------------------------------------------------
        public static void Start(Config cfg)
        {
            if (cfg == null || Active) return;
            if (GameManager.Instance == null || GameManager.Instance.Tower == null ||
                GameManager.Instance.Spawner == null || TeamManager.InstanceRef == null)
            {
                Combat.CombatEvents.RaiseNotification("Battle systems not ready");
                return;
            }

            Current = cfg;
            Active = true;
            DeathsThisBattle = 0;

            var tower = GameManager.Instance.Tower;
            _savedTowerFloor = tower.CurrentFloor;
            _savedTowerDifficulty = (int)tower.Difficulty;

            tower.SetDifficulty((TowerSystem.TowerDifficulty)Mathf.Clamp(cfg.difficulty, 0, 3), false);
            tower.SetFloor(Mathf.Max(1, cfg.floor), false);

            var spawner = GameManager.Instance.Spawner;
            spawner.ForcedMonsterKey = cfg.forcedMonsterKey;
            spawner.ForcedMonsterCount = cfg.monsterCount;
            spawner.ExtraHpScale = cfg.hpScale;
            spawner.ExtraAtkScale = cfg.atkScale;

            // teleport team into the arena, reset HP, enable AI
            _savedCityPos = World.CityBuilder.CitySpawnPoint;
            var team = TeamManager.InstanceRef;
            team.BasePosition = ArenaCenter;
            for (int i = 0; i < team.Heroes.Count; i++)
            {
                var h = team.Heroes[i];
                if (h == null) continue;
                h.Motor.Teleport(team.FormationPosition(i) + Vector3.back * 2.5f);
                h.HP.Revive(1f);
            }
            team.SetBrainsEnabled(true);
            team.SetVisualsVisible(true);

            // camera into the arena
            var cam = Camera.main != null ? Camera.main.GetComponent<Cam.IsometricCameraController>() : null;
            if (cam != null)
            {
                cam.SetBattleView(team.FocusTransform);
            }

            // dark battle atmosphere
            World.ArenaBuilder.ApplyBattleMood();

            if (UI.UIManager.Instance != null) UI.UIManager.Instance.EnterBattleMode(cfg.title);

            GameManager.Instance.SetState(GameManager.GameState.Battle);
            Audio.AudioSystem.Play("BossRoar", 0.5f);
            spawner.StartFloor();

            Debug.Log("[BattleSession] " + cfg.type + " — " + cfg.title + " (floor " + cfg.floor + ")");
        }

        // ----------------------------------------------------------------
        public static void HandleWin()
        {
            if (!Active) return;
            Active = false;
            RestoreSpawner();
            RestoreTower();

            var team = TeamManager.InstanceRef;
            if (team != null) team.SetBrainsEnabled(false);

            Time.timeScale = 1f;
            ApplyHomeMood();
            Audio.AudioSystem.Play("Victory");

            LimitedEvent.AddPoints(1);
            QuestSystem.ReportProgress("win_battle", 1);

            var cfg = Current;
            if (cfg != null && cfg.onWin != null) cfg.onWin();
            if (cfg != null && BattleEnded != null) BattleEnded(true, cfg);

            if (UI.UIManager.Instance != null) UI.UIManager.Instance.ExitBattleMode();
        }

        public static void HandleLose()
        {
            if (!Active) return;
            Active = false;
            RestoreSpawner();
            RestoreTower();

            var team = TeamManager.InstanceRef;
            if (team != null) team.SetBrainsEnabled(false);

            Time.timeScale = 1f;
            ApplyHomeMood();
            Audio.AudioSystem.Play("Death");

            var cfg = Current;
            if (cfg != null && cfg.onLose != null) cfg.onLose();
            if (cfg != null && BattleEnded != null) BattleEnded(false, cfg);

            if (UI.UIManager.Instance != null) UI.UIManager.Instance.ExitBattleMode();
        }

        /// <summary>Manual retreat — treated as a loss without extra punishment.</summary>
        public static void Retreat()
        {
            if (!Active) return;
            HandleLose();
        }

        /// <summary>Replays the same battle without counting an extra loss.</summary>
        public static void Retry()
        {
            var cfg = Current;
            if (cfg == null) return;

            // teardown without invoking onLose
            Active = false;
            RestoreSpawner();
            RestoreTower();
            var team = TeamManager.InstanceRef;
            if (team != null) team.SetBrainsEnabled(false);
            Time.timeScale = 1f;
            ApplyHomeMood();
            if (UI.UIManager.Instance != null) UI.UIManager.Instance.ExitBattleMode();

            var retry = new Config
            {
                type = cfg.type,
                title = cfg.title,
                floor = cfg.floor,
                difficulty = cfg.difficulty,
                boss = cfg.boss,
                hpScale = cfg.hpScale,
                atkScale = cfg.atkScale,
                forcedMonsterKey = cfg.forcedMonsterKey,
                monsterCount = cfg.monsterCount,
                chapter = cfg.chapter,
                stage = cfg.stage,
                region = cfg.region,
                tier = cfg.tier,
                goldDungeon = cfg.goldDungeon,
                context = cfg.context
            };
            ProgressionSystems.RewireBattleCallbacks(retry);
            Start(retry);
        }

        // ----------------------------------------------------------------
        public static void ReturnToCity()
        {
            var team = TeamManager.InstanceRef;
            if (team == null) return;

            for (int i = 0; i < team.Heroes.Count; i++)
            {
                var h = team.Heroes[i];
                if (h == null) continue;
                h.HP.Revive(1f);
            }
            team.BasePosition = _savedCityPos;
            team.SetVisualsVisible(!DioramaHomeBuilder.IsActive);

            var cam = Camera.main != null ? Camera.main.GetComponent<Cam.IsometricCameraController>() : null;
            if (cam != null)
            {
                var homeTarget = DioramaHomeBuilder.IsActive ? DioramaHomeBuilder.HomeCameraTarget : team.FocusTransform;
                if (DioramaHomeBuilder.IsActive) cam.SetDioramaView(homeTarget);
                else
                {
                    cam.SetTarget(homeTarget);
                    cam.SnapToTarget();
                }
            }

            Time.timeScale = 1f;
        }

        private static void ApplyHomeMood()
        {
            if (DioramaHomeBuilder.IsActive) DioramaHomeBuilder.ApplyMood();
            else World.CityBuilder.ApplyCityMood();
        }

        private static void RestoreSpawner()
        {
            var spawner = GameManager.Instance.Spawner;
            if (spawner != null)
            {
                spawner.ForcedMonsterKey = "";
                spawner.ForcedMonsterCount = 0;
                spawner.ExtraHpScale = 1f;
                spawner.ExtraAtkScale = 1f;
            }
        }

        private static void RestoreTower()
        {
            var tower = GameManager.Instance.Tower;
            if (tower != null)
            {
                tower.SetDifficulty((TowerSystem.TowerDifficulty)_savedTowerDifficulty, false);
                tower.SetFloor(_savedTowerFloor, false);
            }
        }
    }
}
