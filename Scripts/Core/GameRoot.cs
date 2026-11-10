using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Audio;
using LegendsRPG.Cam;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Social;
using LegendsRPG.Team;
using LegendsRPG.UI;
using LegendsRPG.Vfx;
using LegendsRPG.World;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Single entry point placed in the Boot scene by the editor bootstrap.
    /// v1.0: boots into the full game — city, all meta systems, battle arena,
    /// 3D guild hall, city HUD, screens and notifications. Nothing requires
    /// manual scene setup.
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
            Screen.orientation = ScreenOrientation.Portrait;
#endif

            BuildGame();
        }

        private void BuildGame()
        {
            // ============ 1) core services ============
            var audioGo = new GameObject("AudioSystem");
            var audio = audioGo.AddComponent<AudioSystem>();
            if (GetComponent<AudioListener>() == null && Object.FindObjectOfType<AudioListener>() == null)
            {
                audioGo.AddComponent<AudioListener>();
            }

            var poolGo = new GameObject("ObjectPool");
            poolGo.AddComponent<ObjectPool>();

            var vfxGo = new GameObject("VFXFactory");
            vfxGo.AddComponent<VFXFactory>();

            // ============ 2) save + every meta system ============
            SaveSystem.Load();
            Items.InventorySystem.Initialize();

            Economy.ReadFrom(SaveSystem.Current);
            PlayerProfile.ReadFrom(SaveSystem.Current);
            HeroProgress.ReadFrom(SaveSystem.Current);
            TechSystem.ReadFrom(SaveSystem.Current);
            ForgeSystem.ReadFrom(SaveSystem.Current);
            QuestSystem.ReadFrom(SaveSystem.Current);
            ShopSystem.ReadFrom(SaveSystem.Current);
            SignInSystem.ReadFrom(SaveSystem.Current);
            LimitedEvent.ReadFrom(SaveSystem.Current);
            MerchantSystem.ReadFrom(SaveSystem.Current);
            SummonSystem.ReadFrom(SaveSystem.Current);
            CampaignSystem.ReadFrom(SaveSystem.Current);
            ContinentGateSystem.ReadFrom(SaveSystem.Current);
            DungeonSystem.ReadFrom(SaveSystem.Current);
            ArenaSystem.ReadFrom(SaveSystem.Current);
            MineSystem.ReadFrom(SaveSystem.Current);
            BeastSystem.ReadFrom(SaveSystem.Current);
            BeastSystem.EnsureDefaults();
            GuildSystem.ReadFrom(SaveSystem.Current);
            AFKReward.ReadFrom(SaveSystem.Current);
            QuestSystem.DailyReset();
            GuildSystem.DailyGiftCheck();

            GfxSettings.ApplyFromSave();
            AudioSystem.SetMusicVolume(SaveSystem.Current.musicVolume);
            AudioSystem.SetSfxVolume(SaveSystem.Current.sfxVolume);

            // ============ 3) battle arena (far from the city) ============
            WaveSpawner.ArenaCenter = new Vector3(0f, 0f, BattleSession.ArenaOffsetZ);
            var arenaGo = new GameObject("ArenaBuilder");
            arenaGo.AddComponent<ArenaBuilder>().Build();
            if (ArenaBuilder.ArenaRoot != null)
            {
                ArenaBuilder.ArenaRoot.transform.position = new Vector3(0f, 0f, BattleSession.ArenaOffsetZ);
            }

            // ============ 4) static tabletop home (legacy city is fallback) ============
            Transform homeCameraTarget = null;
            CityWalker.Enabled = false;
            try
            {
                var homeGo = new GameObject("DioramaHomeBuilder");
                homeCameraTarget = homeGo.AddComponent<DioramaHomeBuilder>().Build();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[LegendsRPG] Diorama home failed; restoring legacy city. " + ex);
                CityWalker.Enabled = true;
                var fallbackCity = new GameObject("CityBuilder_Fallback");
                fallbackCity.AddComponent<CityBuilder>().Build();
            }

            // ============ 5) camera with 3D-click support ============
            var camGo = new GameObject("BattleCamera");
            var camera = camGo.AddComponent<IsometricCameraController>();
            camGo.tag = "MainCamera";
            camGo.AddComponent<PhysicsRaycaster>();   // lets CityBuilding receive clicks

            // ============ 6) manager + battle systems ============
            var managerGo = new GameObject("GameManager");
            var gm = managerGo.AddComponent<GameManager>();

            var team = managerGo.AddComponent<TeamManager>();
            var tower = managerGo.AddComponent<TowerSystem>();
            var spawner = managerGo.AddComponent<WaveSpawner>();
            var auto = managerGo.AddComponent<AutoSkills>();   // v1.1: auto-casts skills/ults (no manual drive)

            gm.Team = team;
            gm.Tower = tower;
            gm.Spawner = spawner;
            gm.Audio = audio;
            gm.Vfx = vfxGo.GetComponent<VFXFactory>();
            team.SetAutoSkills(auto);

            // ============ 7) monsters (incl. arena shadows + guild boss) ============
            var monsters = HeroDatabase.CreateMonsters();
            ArenaSystem.RegisterShadows(monsters);
            monsters["guild_hunt_boss"] = MakeGuildBoss();
            spawner.Init(monsters);

            // ============ 8) the hero party lives in the city ============
            var roster = HeroDatabase.CreateDefaultRoster();
            team.CreateTeam(roster, CityBuilder.CitySpawnPoint, demoUnlockAll: false);
            team.SetBrainsEnabled(false);
            team.SetVisualsVisible(homeCameraTarget == null); // hide the party only while the tabletop home is active
            if (homeCameraTarget != null) camera.SetDioramaView(homeCameraTarget);
            else
            {
                camera.SetTarget(team.FocusTransform);
                camera.SnapToTarget();
            }

            // ============ 9) battle HUD (hidden until a battle starts) ============
            var hudGo = new GameObject("HUD");
            var hud = hudGo.AddComponent<HUDController>();
            hud.Build(team, camera);
            gm.Hud = hud;
            hud.SetBattleHudVisible(false, "");

            // ============ 10) UI manager: all screens + city HUD ============
            var uiGo = new GameObject("UIManager");
            var ui = uiGo.AddComponent<UIManager>();
            ui.BuildAll();

            // ============ 11) input wiring ============
            var inputGo = new GameObject("TouchInput");
            var input = inputGo.AddComponent<TouchInputController>();
            input.PinchZoom += camera.AddZoomInput;   // v1.1: pinch zoom only — no drag rotation

            // ============ 12) tower restore ============
            tower.RestoreFromSave();

            // ============ 13) combat juice + guild hunt damage tracking ============
            CombatEvents.DamageDealt += info =>
            {
                if (info.isCrit || (info.isSkill && info.amount > 25f))
                {
                    camera.Shake(0.18f, 0.1f);
                }
            };

            long huntDamage = 0;
            CombatEvents.DamageDealt += info =>
            {
                if (BattleSession.Active && BattleSession.Current != null &&
                    BattleSession.Current.type == BattleSession.BattleType.GuildHunt)
                {
                    huntDamage += Mathf.Max(0, Mathf.RoundToInt(info.amount));
                }
            };
            BattleSession.BattleEnded += (win, cfg) =>
            {
                if (cfg != null && cfg.type == BattleSession.BattleType.GuildHunt)
                {
                    if (huntDamage > 0) GuildSystem.RecordHuntDamage(huntDamage);
                    huntDamage = 0;
                }
            };

            // AUTO (auto skills) ON by default — the player can switch it off to
            // fire the ultimates manually from the portraits
            auto.SetEnabled(true);

            // ============ 14) start in the city ============
            gm.SetState(GameManager.GameState.City);
            if (DioramaHomeBuilder.IsActive) DioramaHomeBuilder.ApplyMood();
            else CityBuilder.ApplyCityMood();

            // welcome the new player
            if (SaveSystem.Current.playerLevel <= 1 && CampaignSystem.ClearedStageCount() == 0)
            {
                ui.Notify("Welcome, " + PlayerProfile.Name + "! Tap the glowing buildings to begin.");
            }

            Debug.Log("[LegendsRPG] v1.0 booted. Heroes=" + team.Heroes.Count +
                      "/" + SaveSystem.Current.unlockedHeroIds.Count +
                      ", Gold=" + Economy.Gold + ", Dia=" + Economy.Diamonds +
                      ", Campaign=" + CampaignSystem.ClearedStageCount() +
                      ", Guild=" + (GuildSystem.InGuild ? GuildSystem.GuildName : "none") +
                      ", Save=" + SaveSystem.GetSavePathForDocs());
        }

        /// <summary>Guild hunt boss scales with guild level so hunts stay relevant.</summary>
        private static MonsterData MakeGuildBoss()
        {
            var m = ScriptableObject.CreateInstance<MonsterData>();
            m.monsterId = "guild_hunt_boss";
            m.displayName = "Ember Titan";
            m.element = ElementType.Fire;
            m.isBoss = true;
            m.baseHP = 420f;
            m.baseAttack = 26f;
            m.baseDefense = 14f;
            m.moveSpeed = 2.8f;
            m.attackSpeed = 0.7f;
            m.attackRange = 3f;
            m.scale = 2.6f;
            m.expReward = 200f;
            m.primaryColor = new Color(0.95f, 0.4f, 0.15f);
            m.secondaryColor = new Color(0.25f, 0.2f, 0.18f);
            m.aggroRadius = 30f;
            m.attackVfxKey = "explosion";
            return m;
        }
    }
}
