using UnityEngine;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Central game manager: owns global state, wires subsystems together,
    /// handles autosave and the lightweight FPS meter used by performance tests.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState { Boot, Battle, FloorCleared, Paused }

        public GameState State { get; private set; } = GameState.Boot;

        public World.TowerSystem Tower;
        public World.WaveSpawner Spawner;
        public Team.TeamManager Team;
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

            if (State == GameState.Battle)
            {
                _autosaveTimer += Time.deltaTime;
                if (_autosaveTimer >= GameConfig.AutosaveInterval)
                {
                    _autosaveTimer = 0f;
                    SaveSystem.Save(Tower, Team);
                }
            }
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

        public void OnFloorCleared()
        {
            if (State != GameState.Battle) return;
            State = GameState.FloorCleared;
            Audio.Play("Victory");
            CombatEvents.RaiseNotification("Floor cleared!");
            if (Tower != null) Tower.AdvanceFloor();
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
            Time.timeScale = pause ? 0f : 1f;
            State = pause ? GameState.Paused : GameState.Battle;
        }
    }
}
