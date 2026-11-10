using UnityEngine;
using LegendsRPG.Team;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Central game manager: owns global state, wires subsystems together,
    /// handles autosave and the lightweight FPS meter used by performance tests.
    /// v1.0: adds the City state and routes every battle outcome through
    /// BattleSession (campaign stages, dungeons, arena, guild hunt...).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState { Boot, City, Battle, FloorCleared, Paused }

        public GameState State { get; private set; } = GameState.Boot;

        public World.TowerSystem Tower;
        public World.WaveSpawner Spawner;
        public TeamManager Team;
        public UI.HUDController Hud;
        public Audio.AudioSystem Audio;
        public Vfx.VFXFactory Vfx;

        private float _fpsAccum;
        private int _fpsFrames;
        private float _autosaveTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = GameConfig.TargetFrameRate;
        }

        private void Update()
        {
            TrackFps();

            _autosaveTimer += Time.unscaledDeltaTime;
            if (_autosaveTimer >= GameConfig.AutosaveInterval || SaveSystem.ConsumeDirty())
            {
                if (_autosaveTimer >= 2f)
                {
                    _autosaveTimer = 0f;
                    SaveSystem.Save(Tower, Team);
                }
            }

            if (State == GameState.Battle && BattleSession.Active)
            {
                CheckTeamWiped();
            }
        }

        /// <summary>All heroes down at the same moment = stage failed.</summary>
        private void CheckTeamWiped()
        {
            var team = Team;
            if (team == null || team.Heroes.Count == 0) return;

            bool allDead = true;
            foreach (var h in team.Heroes)
            {
                if (h != null && !h.HP.IsDead) { allDead = false; break; }
            }
            if (allDead) BattleSession.HandleLose();
        }

        private void TrackFps()
        {
            _fpsAccum += Time.unscaledDeltaTime;
            _fpsFrames++;
            if (_fpsAccum >= 0.5f)
            {
                CurrentFps = _fpsFrames / _fpsAccum;
                _fpsAccum = 0f;
                _fpsFrames = 0;
            }
        }

        public float CurrentFps { get; private set; } = 60f;

        public void SetState(GameState s)
        {
            State = s;
        }

        /// <summary>All enemies of the wave are dead → battle won.</summary>
        public void OnFloorCleared()
        {
            if (State != GameState.Battle) return;

            if (BattleSession.Active)
            {
                State = GameState.City;
                BattleSession.HandleWin();
                return;
            }

            // legacy free-play path (no session — e.g. debug mode)
            State = GameState.FloorCleared;
            LegendsRPG.Audio.AudioSystem.Play("Victory");
            Combat.CombatEvents.RaiseNotification("Floor cleared!");
            if (Tower != null) Tower.AdvanceFloor();

            var loot = Items.InventorySystem.RollFloorReward(Tower.CurrentFloor, (int)Tower.Difficulty);
            if (loot != null) Combat.CombatEvents.RaiseNotification("LOOT: " + loot);

            SaveSystem.Save(Tower, Team);
            if (Hud != null) Hud.ShowFloorClearedPanel(true);
        }

        public void StartNextFloor()
        {
            if (Hud != null) Hud.ShowFloorClearedPanel(false);
            if (Spawner != null) Spawner.StartFloor();
            State = GameState.Battle;
        }

        public void Pause(bool pause)
        {
            if (pause) Time.timeScale = 0f;
            else Time.timeScale = UI.HUDController.BattleTimeScale;   // v1.1: resume at the selected battle speed
            State = pause ? GameState.Paused : GameState.Battle;
        }
    }
}
