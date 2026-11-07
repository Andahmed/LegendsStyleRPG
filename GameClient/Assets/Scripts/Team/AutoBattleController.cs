using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;

namespace LegendsRPG.Team
{
    /// <summary>
    /// Auto Battle: unlocked from the start in this prototype (configurable),
    /// toggled from the HUD. When enabled it fully drives the ACTIVE hero —
    /// movement, basic attacks, skill rotation and ultimate — through the same
    /// ManualTick path used by human input.
    /// </summary>
    public class AutoBattleController : MonoBehaviour
    {
        public static AutoBattleController Instance { get; private set; }

        public bool Enabled { get; private set; }
        public event System.Action<bool> OnToggled;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetEnabled(bool value)
        {
            Enabled = value;
            if (OnToggled != null) OnToggled(value);
            Combat.CombatEvents.RaiseNotification("Auto Battle: " + (value ? "ON" : "OFF"));
            Audio.AudioSystem.Play("UI");
        }

        public void Toggle()
        {
            SetEnabled(!Enabled);
        }

        /// <summary>Called by TeamManager instead of human input while enabled.</summary>
        public void DriveActiveHero(HeroCharacter hero)
        {
            if (hero == null || hero.HP == null || hero.HP.IsDead) return;

            var enemy = CombatUtil.FindNearestMonster(hero.transform.position, 16f);
            if (enemy == null)
            {
                // no enemies: return to arena center
                Vector3 toCenter = -hero.transform.position; toCenter.y = 0f;
                if (toCenter.magnitude > 2f) hero.ManualTick(Vector2.up, false);
                else hero.ManualTick(Vector2.zero, false);
                return;
            }

            float dist = Vector3.Distance(hero.transform.position, enemy.transform.position);
            Vector2 stick = Vector2.zero;
            bool attack = false;

            if (dist > hero.Stats.AttackRange)
            {
                // steer toward the enemy relative to the camera
                var cam = Camera.main;
                Vector3 fwd = Vector3.forward, right = Vector3.right;
                if (cam != null)
                {
                    fwd = cam.transform.forward; fwd.y = 0f; fwd.Normalize();
                    right = cam.transform.right; right.y = 0f; right.Normalize();
                }
                Vector3 dir = (enemy.transform.position - hero.transform.position).normalized;
                Vector3 move = fwd * dir.z + right * dir.x;
                stick = new Vector2(move.x, move.z);
                if (stick.magnitude > 1f) stick.Normalize();
            }
            else
            {
                attack = true;
            }

            hero.ManualTick(stick, attack);

            // skill rotation
            if (dist <= Mathf.Max(hero.Stats.AttackRange, 3f))
            {
                var t = enemy.transform;
                if (hero.Caster.GetCooldownLeft(0) <= 0f) hero.Caster.TryCast(0, t);
                else if (hero.Caster.GetCooldownLeft(1) <= 0f) hero.Caster.TryCast(1, t);
                else if (hero.Caster.UltCharge >= 100f) hero.Caster.TryCast(2, t);
            }
        }
    }
}
