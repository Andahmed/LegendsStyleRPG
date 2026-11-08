using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;

namespace LegendsRPG.Team
{
    /// <summary>
    /// AI brain attached to every non-active hero: follows the active hero in
    /// formation, engages nearby monsters, auto-attacks and opportunistically
    /// casts skills — exactly the "AI for the rest of the team" requirement.
    /// </summary>
    public class TeamAIBrain : MonoBehaviour
    {
        private HeroCharacter _hero;
        private TeamManager _team;

        private const float EngageRadius = 11f;
        private const float FollowDistance = 1.4f;

        public void Bind(HeroCharacter hero, TeamManager team)
        {
            _hero = hero;
            _team = team;
        }

        private void Update()
        {
            if (_hero == null || _team == null) return;
            if (_hero.IsActive) return;                 // active hero is human-controlled
            if (_hero.HP == null || _hero.HP.IsDead) return;
            if (Core.GameManager.Instance == null) return;
            if (Core.GameManager.Instance.State != Core.GameManager.GameState.Battle) return;

            HeroCharacter anchor = _team.ActiveHero != null && !_team.ActiveHero.HP.IsDead
                ? _team.ActiveHero
                : null;

            Vector3 anchorPos = anchor != null ? anchor.transform.position : _team.BasePosition;

            var enemy = CombatUtil.FindNearestMonster(_hero.transform.position, EngageRadius);
            if (enemy != null)
            {
                float dist = Vector3.Distance(_hero.transform.position, enemy.transform.position);
                if (dist > _hero.Stats.AttackRange)
                {
                    _hero.AutoTick(enemy.transform.position, false);
                }
                else
                {
                    _hero.AutoTick(_hero.transform.position, true);
                    _hero.Motor.FaceTowards(enemy.transform.position);
                }
                return;
            }

            // no enemies nearby: hold formation around the active hero
            Vector3 formation = anchorPos + FormationOffset();
            if (Vector3.Distance(_hero.transform.position, formation) > FollowDistance)
            {
                _hero.AutoTick(formation, false);
            }
            else
            {
                _hero.AutoTick(_hero.transform.position, false);
            }
        }

        private Vector3 FormationOffset()
        {
            float a = (_hero.Slot * 60f + 30f) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(a) * 3.0f, 0f, Mathf.Cos(a) * 3.0f);
        }
    }
}
