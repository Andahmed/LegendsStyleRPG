using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;

namespace LegendsRPG.Team
{
    /// <summary>
    /// Manages the 6-hero team: formation spawning, active-hero switching,
    /// AI control for non-active heroes, death respawn and EXP sharing.
    /// </summary>
    public class TeamManager : MonoBehaviour
    {
        public static TeamManager InstanceRef { get; private set; }

        public List<HeroCharacter> Heroes { get; } = new List<HeroCharacter>();
        public List<string> UnlockedHeroes { get; } = new List<string>();

        /// <summary>Full roster injected by GameRoot — used by RebuildTeam (summon / loadout edits).</summary>
        public List<HeroData> Roster { get; set; }
        public int ActiveIndex { get; private set; }
        public HeroCharacter ActiveHero
        {
            get
            {
                if (ActiveIndex >= 0 && ActiveIndex < Heroes.Count) return Heroes[ActiveIndex];
                return null;
            }
        }
        public Vector3 BasePosition { get; private set; }

        public event Action<int> OnActiveChanged;
        public event Action<HeroCharacter> HeroRespawned;

        private AutoBattleController _autoBattle;

        private void Awake()
        {
            InstanceRef = this;
        }

        private void OnDestroy()
        {
            if (InstanceRef == this) InstanceRef = null;
        }

        public void CreateTeam(List<HeroData> roster, Vector3 basePosition, bool demoUnlockAll)
        {
            BasePosition = basePosition;

            foreach (var h in Heroes) { if (h != null) Destroy(h.gameObject); }
            Heroes.Clear();
            UnlockedHeroes.Clear();

            // In the shipped prototype all six demo heroes are unlocked so the
            // switching / team systems can be tested. Flip this flag to false
            // to restore "player starts with one hero" per the design document.
            if (demoUnlockAll)
            {
                foreach (var r in roster) UnlockedHeroes.Add(r.heroId);
            }
            else
            {
                foreach (var id in SaveSystem.Current.unlockedHeroIds) UnlockedHeroes.Add(id);
            }

            int slot = 0;

            // spawn order: the saved team loadout first, then any other unlocked heroes
            var ordered = new List<HeroData>();
            var loadout = SaveSystem.Current.teamLoadout;
            for (int i = 0; i < loadout.Count; i++)
            {
                var d = FindInRoster(roster, loadout[i]);
                if (d != null && UnlockedHeroes.Contains(loadout[i]) && ordered.IndexOf(d) < 0) ordered.Add(d);
            }
            foreach (var data in roster)
            {
                if (UnlockedHeroes.Contains(data.heroId) && ordered.IndexOf(data) < 0) ordered.Add(data);
            }

            foreach (var data in ordered)
            {
                if (slot >= GameConfig.TeamSize) break;

                var go = new GameObject("HeroRoot_" + data.displayName);
                var hero = go.AddComponent<HeroCharacter>();
                hero.Setup(data, Inventory.HeroLevel(data.heroId), slot, FormationPosition(slot));
                go.AddComponent<TeamAIBrain>().Bind(hero, this);

                var health = hero.HP;
                var captured = hero;
                health.Died += delegate { OnHeroDied(captured); };

                // v0.5: per-hero level persistence + equipment bonuses
                string heroKey = data.heroId;
                hero.Stats.OnLevelUp += lv => Inventory.SetHeroLevel(heroKey, lv);
                Equipment.Apply(hero);

                Heroes.Add(hero);
                slot++;
            }

            ActiveIndex = 0;
            ApplyActiveState();
        }

        public Vector3 FormationPosition(int slot)
        {
            // two rows of three, 2.6m apart, so heroes never overlap
            float x = (slot % 3 - 1) * 2.6f;
            float z = -(slot / 3) * 2.6f;
            return BasePosition + new Vector3(x, 0f, z);
        }

        /// <summary>Respawns the whole team from the roster + current save state.
        /// Called after summoning heroes or editing the team loadout in the UI.</summary>
        public void RebuildTeam()
        {
            if (BasePosition == Vector3.zero && Heroes.Count == 0)
            {
                // CreateTeam never ran (should not happen) — nothing sane to rebuild from
                return;
            }
            var roster = Roster != null && Roster.Count > 0 ? Roster : HeroDatabase.AllHeroes;
            CreateTeam(roster, BasePosition, false);
        }

        private static HeroData FindInRoster(List<HeroData> roster, string heroId)
        {
            for (int i = 0; i < roster.Count; i++)
            {
                if (roster[i].heroId == heroId) return roster[i];
            }
            return null;
        }

        public void SwitchTo(int index)
        {
            if (index < 0 || index >= Heroes.Count) return;
            var target = Heroes[index];
            if (target == null || target.HP.IsDead) return;

            ActiveIndex = index;
            ApplyActiveState();
            CombatEvents.RaiseNotification("Switched to " + target.Data.displayName);

            var cam = Camera.main != null ? Camera.main.GetComponent<Cam.IsometricCameraController>() : null;
            if (cam != null) cam.SetTarget(target.transform);
        }

        public void SwitchNext()
        {
            int n = Heroes.Count;
            for (int step = 1; step <= n; step++)
            {
                int idx = (ActiveIndex + step) % n;
                var h = Heroes[idx];
                if (h != null && !h.HP.IsDead)
                {
                    SwitchTo(idx);
                    return;
                }
            }
        }

        private void ApplyActiveState()
        {
            for (int i = 0; i < Heroes.Count; i++)
            {
                if (Heroes[i] != null) Heroes[i].SetActive(i == ActiveIndex);
            }
            if (OnActiveChanged != null) OnActiveChanged(ActiveIndex);
        }

        private void Update()
        {
            if (Heroes.Count == 0) return;

            var input = InputCtl.TouchInputController.Instance;
            bool attackHeld = (input != null && input.AttackHeldState) || UI.HUDController.AttackButtonHeld;

            var active = ActiveHero;
            bool autoOn = _autoBattle != null && _autoBattle.Enabled;

            if (active != null && !active.HP.IsDead)
            {
                if (autoOn)
                {
                    _autoBattle.DriveActiveHero(active);
                }
                else
                {
                    Vector2 stick = input != null ? input.MoveVector : Vector2.zero;
                    active.ManualTick(stick, attackHeld);
                }
            }
        }

        public void SetAutoBattle(AutoBattleController auto)
        {
            _autoBattle = auto;
        }

        public void ShareEXP(float total)
        {
            int alive = 0;
            foreach (var h in Heroes)
            {
                if (h != null && !h.HP.IsDead) alive++;
            }
            if (alive == 0) return;

            float per = total / alive;
            foreach (var h in Heroes)
            {
                if (h != null && !h.HP.IsDead) h.Stats.GrantExp(per);
            }
        }

        // ---- respawn ----
        private void OnHeroDied(HeroCharacter hero)
        {
            CombatEvents.RaiseNotification(hero.Data.displayName + " has fallen — respawning in " +
                                           GameConfig.RespawnSeconds + "s");
            StartCoroutine(RespawnRoutine(hero));
        }

        private System.Collections.IEnumerator RespawnRoutine(HeroCharacter hero)
        {
            yield return new WaitForSeconds(GameConfig.RespawnSeconds);

            if (hero == null || hero.HP == null) yield break;

            // if the whole team was dead, respawn refills the run at base
            hero.HP.Revive(0.6f);
            hero.Motor.Teleport(BasePosition + Vector3.right * 2f);
            hero.Motor.Stop();

            // switching away if the dead hero was active
            if (Heroes[ActiveIndex] == hero)
            {
                SwitchNext();
            }

            if (HeroRespawned != null) HeroRespawned(hero);
        }
    }
}
