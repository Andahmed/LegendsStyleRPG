using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;

namespace LegendsRPG.Team
{
    /// <summary>
    /// Manages the hero team: formation spawning, death respawn and EXP sharing.
    /// v1.1 (Legends Reborn style): there is NO player-controlled active hero
    /// and NO switching during combat — every hero runs TeamAIBrain. The old
    /// ActiveHero meaning is now the internal "focus hero" (first alive one),
    /// kept only so screens that need a representative hero keep working;
    /// the battle camera follows FocusTransform (the live team center) instead.
    /// </summary>
    public class TeamManager : MonoBehaviour
    {
        public static TeamManager InstanceRef { get; private set; }

        public List<HeroCharacter> Heroes { get; } = new List<HeroCharacter>();
        public List<string> UnlockedHeroes { get; } = new List<string>();
        public Vector3 BasePosition { get; set; }

        /// <summary>
        /// Internal focus hero (first alive) for screens/UI only — NOT
        /// player-controlled, and NOT the camera target (the camera follows
        /// FocusTransform, the live team center).
        /// </summary>
        public HeroCharacter ActiveHero
        {
            get
            {
                for (int i = 0; i < Heroes.Count; i++)
                {
                    if (Heroes[i] != null && Heroes[i].HP != null && !Heroes[i].HP.IsDead) return Heroes[i];
                }
                return Heroes.Count > 0 ? Heroes[0] : null;
            }
        }

        public event Action<HeroCharacter> HeroRespawned;

        private Transform _focus;           // live team center — battle camera target
        private AutoSkills _autoSkills;     // kept for symmetric wiring; consumed by TeamAIBrain

        /// <summary>Live team center (centroid of alive heroes) — the camera's follow target.</summary>
        public Transform FocusTransform
        {
            get
            {
                if (_focus == null)
                {
                    var go = new GameObject("TeamFocus");
                    _focus = go.transform;
                }
                return _focus;
            }
        }

        /// <summary>Centroid of the alive heroes (falls back to any hero, then to BasePosition).</summary>
        public Vector3 TeamCenter
        {
            get
            {
                Vector3 sum = Vector3.zero;
                int n = 0;
                for (int i = 0; i < Heroes.Count; i++)
                {
                    var h = Heroes[i];
                    if (h == null || h.HP == null || h.HP.IsDead) continue;
                    sum += h.transform.position;
                    n++;
                }
                if (n == 0)
                {
                    for (int i = 0; i < Heroes.Count; i++)
                    {
                        if (Heroes[i] != null) { sum += Heroes[i].transform.position; n++; }
                    }
                }
                return n > 0 ? sum / n : BasePosition;
            }
        }

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

            // In the shipped prototype the full roster (62 heroes) is unlocked so the
            // team systems can be tested. Flip this flag to false
            // to restore "player starts with one hero" per the design document.
            if (demoUnlockAll)
            {
                foreach (var r in roster) UnlockedHeroes.Add(r.heroId);
            }
            else
            {
                foreach (var id in SaveSystem.Current.unlockedHeroIds) UnlockedHeroes.Add(id);
            }

            // v1.0: honor the player's chosen formation order
            var ordered = OrderRosterByTeam(roster);

            int slot = 0;
            foreach (var data in ordered)
            {
                if (!UnlockedHeroes.Contains(data.heroId)) continue;
                if (slot >= GameConfig.TeamSize) break;

                var go = new GameObject("HeroRoot_" + data.displayName);
                var hero = go.AddComponent<HeroCharacter>();
                int level = Mathf.Max(1, SaveSystem.Current.playerLevel);
                hero.Setup(data, level, slot, FormationPosition(slot));
                Items.InventorySystem.ApplyToHero(hero);   // equipment stats + wings/pet visuals
                go.AddComponent<TeamAIBrain>().Bind(hero, this);

                var health = hero.HP;
                var captured = hero;
                health.Died += delegate { OnHeroDied(captured); };

                Heroes.Add(hero);
                slot++;
            }

            UpdateFocus();
        }

        /// <summary>Orders the roster by the saved team formation (drag order).</summary>
        private List<HeroData> OrderRosterByTeam(List<HeroData> roster)
        {
            var ids = SaveSystem.Current.teamHeroIds;
            var map = new Dictionary<string, HeroData>();
            foreach (var r in roster) map[r.heroId] = r;

            var result = new List<HeroData>();
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    if (map.ContainsKey(id) && UnlockedHeroes.Contains(id))
                    {
                        result.Add(map[id]);
                        map.Remove(id);
                    }
                }
            }
            foreach (var kv in map)
            {
                if (UnlockedHeroes.Contains(kv.Key)) result.Add(kv.Value);
            }
            return result;
        }

        /// <summary>Show or hide the party models while the home diorama is displayed.</summary>
        public void SetVisualsVisible(bool visible)
        {
            for (int i = 0; i < Heroes.Count; i++)
            {
                var hero = Heroes[i];
                if (hero == null) continue;
                var renderers = hero.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < renderers.Length; r++)
                    if (renderers[r] != null) renderers[r].enabled = visible;
            }
        }

        /// <summary>Enables/disables the AI brains (city walking vs battle AI).</summary>
        public void SetBrainsEnabled(bool enabled)
        {
            foreach (var h in Heroes)
            {
                if (h == null) continue;
                var brain = h.GetComponent<TeamAIBrain>();
                if (brain != null) brain.enabled = enabled;
            }
        }

        public Vector3 FormationPosition(int slot)
        {
            float angle = slot * 60f * Mathf.Deg2Rad;
            return BasePosition + new Vector3(Mathf.Sin(angle) * 1.6f, 0f, Mathf.Cos(angle) * 1.6f);
        }

        // v1.1: no manual driving in Update — the only job here is keeping the
        // team-center focus point on the centroid of the alive heroes.
        private void Update()
        {
            UpdateFocus();
        }

        private void UpdateFocus()
        {
            if (_focus == null) return;      // created lazily by FocusTransform on first request
            if (Heroes.Count > 0) _focus.position = TeamCenter;
        }

        public void SetAutoSkills(AutoSkills auto)
        {
            // v1.1: AUTO is a global toggle consumed by TeamAIBrain directly; the
            // reference is kept so the GameRoot wiring stays symmetric.
            _autoSkills = auto;
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

            if (HeroRespawned != null) HeroRespawned(hero);
        }
    }
}
