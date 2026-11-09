using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;

namespace LegendsRPG.Team
{
    /// <summary>
    /// v1.1 battle AI — attached to EVERY hero (Legends Reborn style; there is
    /// no player-controlled hero anymore):
    ///  - hunts the nearest monster anywhere in the arena and chases it,
    ///  - melee heroes push to the front ring, ranged heroes hold a wider
    ///    back ring around the team center (front/back formation),
    ///  - movement + basic attacks are ALWAYS automatic,
    ///  - skills and ultimates fire automatically ONLY while AutoSkills is ON;
    ///    while it is OFF the player fires ultimates by tapping portraits.
    /// </summary>
    public class TeamAIBrain : MonoBehaviour
    {
        private HeroCharacter _hero;
        private TeamManager _team;

        private const float HuntRadius = 34f;      // chase the nearest enemy anywhere in the arena
        private const float RangedHoldFraction = 0.6f; // ranged heroes stop at 60% of their range

        public void Bind(HeroCharacter hero, TeamManager team)
        {
            _hero = hero;
            _team = team;
        }

        private void Update()
        {
            if (_hero == null || _team == null) return;
            if (_hero.HP == null || _hero.HP.IsDead) return;
            if (Core.GameManager.Instance == null) return;
            if (Core.GameManager.Instance.State != Core.GameManager.GameState.Battle) return;

            var enemy = CombatUtil.FindNearestMonster(_hero.transform.position, HuntRadius);
            bool ranged = _hero.Stats.AttackRange > 4f;

            if (enemy != null)
            {
                float dist = Vector3.Distance(_hero.transform.position, enemy.transform.position);
                float ideal = ranged ? _hero.Stats.AttackRange * RangedHoldFraction
                                     : _hero.Stats.AttackRange * 0.9f;

                if (dist > ideal)
                {
                    _hero.AutoTick(enemy.transform.position, false);
                }
                else
                {
                    _hero.AutoTick(_hero.transform.position, true);
                    _hero.Motor.FaceTowards(enemy.transform.position);
                }

                // v1.1: automatic skill rotation only while AUTO is ON
                var auto = AutoSkills.Instance;
                if (auto != null && auto.Enabled && dist <= Mathf.Max(_hero.Stats.AttackRange, 3.5f))
                {
                    if (_hero.Caster.GetCooldownLeft(0) <= 0f) _hero.Caster.TryCast(0, enemy.transform);
                    else if (_hero.Caster.GetCooldownLeft(1) <= 0f) _hero.Caster.TryCast(1, enemy.transform);
                    else if (_hero.Caster.UltCharge >= 100f) _hero.Caster.TryCast(2, enemy.transform);
                }
                return;
            }

            // no enemies nearby: hold the front/back formation around the team center
            Vector3 center = _team.FocusTransform != null ? _team.FocusTransform.position : _team.BasePosition;
            Vector3 formation = center + FormationOffset(ranged);
            if (Vector3.Distance(_hero.transform.position, formation) > 0.6f)
            {
                _hero.AutoTick(formation, false);
            }
            else
            {
                _hero.AutoTick(_hero.transform.position, false);
            }
        }

        /// <summary>
        /// Front/back formation: melee hug the team center (front ring),
        /// ranged units stand in a wider ring pushed slightly behind it.
        /// </summary>
        private Vector3 FormationOffset(bool ranged)
        {
            float radius = ranged ? 3.6f : 1.7f;
            float a = (_hero.Slot * 60f + 30f) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(a) * radius, 0f,
                               Mathf.Cos(a) * radius - (ranged ? 2.4f : 0f));
        }
    }
}
