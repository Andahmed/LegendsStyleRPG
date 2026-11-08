using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Audio;
using LegendsRPG.Cam;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;
using LegendsRPG.InputCtl;
using LegendsRPG.Team;
using LegendsRPG.UI;
using LegendsRPG.Vfx;
using LegendsRPG.World;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Single entry point placed in the Boot scene by the editor bootstrap.
    /// Builds the entire playable prototype at runtime: audio, arena, camera,
    /// team, tower, waves, input and HUD. Nothing requires manual scene setup.
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        private static bool _built;

        private void Awake()
        {
            if (_built)
            {
                Destroy(gameObject);
                return;
            }
            _built = true;
            DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID && !UNITY_EDITOR
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif

            BuildGame();
        }

        private void BuildGame()
        {
            // 1) audio + pools + vfx
            var audioGo = new GameObject("AudioSystem");
            audioGo.AddComponent<AudioSystem>();
            if (GetComponent<AudioListener>() == null && Object.FindObjectOfType<AudioListener>() == null)
            {
                audioGo.AddComponent<AudioListener>();
            }

            var poolGo = new GameObject("ObjectPool");
            poolGo.AddComponent<ObjectPool>();

            var vfxGo = new GameObject("VFXFactory");
            vfxGo.AddComponent<VFXFactory>();

            // 2) arena
            var arena = new GameObject("ArenaBuilder");
            arena.AddComponent<ArenaBuilder>().Build();

            // 3) camera
            var camGo = new GameObject("BattleCamera");
            var camera = camGo.AddComponent<IsometricCameraController>();
            camGo.tag = "MainCamera";

            // 4) manager + systems
            var managerGo = new GameObject("GameManager");
            var gm = managerGo.AddComponent<GameManager>();

            var team = managerGo.AddComponent<TeamManager>();
            var tower = managerGo.AddComponent<TowerSystem>();
            var spawner = managerGo.AddComponent<WaveSpawner>();
            var auto = managerGo.AddComponent<AutoBattleController>();

            gm.Team = team;
            gm.Tower = tower;
            gm.Spawner = spawner;
            gm.Audio = audioGo.GetComponent<AudioSystem>();
            gm.Vfx = vfxGo.GetComponent<VFXFactory>();

            team.SetAutoBattle(auto);

            // 5) input
            var inputGo = new GameObject("TouchInput");
            inputGo.AddComponent<TouchInputController>();

            // 6) save + roster + team
            SaveSystem.Load();
            var roster = HeroDatabase.CreateDefaultRoster();
            var monsters = HeroDatabase.CreateMonsters();
            // v0.4: the save drives unlocks (player starts with Kael and summons
            // the rest) — demoUnlockAll is off so the gacha loop is real.
            team.Roster = roster;
            team.CreateTeam(roster, new Vector3(0f, 0f, 8f), demoUnlockAll: false);

            camera.SetTarget(team.ActiveHero != null ? team.ActiveHero.transform : null);

            // 7) HUD
            var hudGo = new GameObject("HUD");
            var hud = hudGo.AddComponent<HUDController>();
            hud.Build(team, camera);
            gm.Hud = hud;

            // 8) wiring
            var input = inputGo.GetComponent<TouchInputController>();
            input.DragRotate += camera.AddOrbitInput;
            input.PinchZoom += camera.AddZoomInput;

            // 9) tower + first floor
            spawner.Init(monsters);
            tower.RestoreFromSave();
            tower.FloorStarted += (floor, diff) => { };

            // subscribe camera juice to combat events
            CombatEvents.DamageDealt += info =>
            {
                if (info.isCrit || (info.isSkill && info.amount > 25f))
                {
                    camera.Shake(0.18f, 0.1f);
                }
            };

            spawner.StartFloor();
            gm.SetState(GameManager.GameState.Battle);

            // 10) main menu (battle stays paused until the player presses ENTER BATTLE)
            var menuGo = new GameObject("MenuController");
            var menu = menuGo.AddComponent<MenuController>();
            menu.Build(team, hud);

            // 11) main hub (v0.3): 3D diorama + full hub UI. The hub is the home screen now.
            var hubSceneGo = new GameObject("HubScene");
            var hubCenter = hubSceneGo.AddComponent<LegendsRPG.World.HubScene>().Build(team);

            var hubGo = new GameObject("HubScreen");
            var hub = hubGo.AddComponent<LegendsRPG.UI.HubScreen>();
            hub.Build(team, hud, menu);
            hub.ManagementRouter = id =>
            {
                menu.Open();          // pauses time, hides HUD, opens the management screen
                menu.ShowScreen(id);  // jump straight to the requested screen
                return true;
            };
            hub.Open();
            camera.SetTarget(hubCenter);

            Debug.Log("[LegendsRPG] Prototype booted. Team=" + team.Heroes.Count +
                      " heroes, Floor=" + tower.CurrentFloor +
                      ", SavePath=" + SaveSystem.GetSavePathForDocs());
        }
    }
}
