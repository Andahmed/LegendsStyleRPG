using System;
using UnityEngine;
using LegendsRPG.Combat;

namespace LegendsRPG.Characters
{
    /// <summary>
    /// HP pool with damage / heal / death / revive.
    /// Raises global CombatEvents so UI popups, camera shake and audio stay decoupled.
    /// </summary>
    public class Health : MonoBehaviour
    {
        public float Max = 100f;
        public float Current = 100f;
        public bool IsDead { get; private set; }

        public event Action<Health> Changed;
        public event Action<Health> Died;
        public event Action<Health> Revived;

        public void Init(float max)
        {
            Max = Mathf.Max(1f, max);
            Current = Max;
            IsDead = false;
            RaiseChanged();
        }

        public float Ratio { get { return Max > 0f ? Mathf.Clamp01(Current / Max) : 0f; } }

        public void ApplyDamage(float amount, GameObject source, ElementType element, bool isCrit, bool isSkill)
        {
            if (IsDead || amount <= 0f) return;
            Current -= amount;
            bool killed = Current <= 0f;
            if (killed) Current = 0f;

            var info = new DamageInfo
            {
                target = gameObject,
                source = source,
                amount = amount,
                element = element,
                isCrit = isCrit,
                isSkill = isSkill,
                killed = killed
            };
            CombatEvents.RaiseDamage(info);
            RaiseChanged();

            var anim = GetComponent<CharacterAnimator>();
            if (anim != null && !killed) anim.PlayHit();

            if (killed) Die();
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Current = Mathf.Min(Max, Current + amount);
            RaiseChanged();
        }

        public void Revive(float pct)
        {
            if (!IsDead) return;
            IsDead = false;
            Current = Mathf.Max(1f, Max * Mathf.Clamp01(pct));
            var anim = GetComponent<CharacterAnimator>();
            if (anim != null) anim.ResetDeath();
            CombatEvents.RaiseRespawn(gameObject);
            RaiseChanged();
            if (Revived != null) Revived(this);
        }

        private void Die()
        {
            IsDead = true;
            var motor = GetComponent<CharacterMotor>();
            if (motor != null) motor.Stop();
            var anim = GetComponent<CharacterAnimator>();
            if (anim != null) anim.PlayDeath();
            CombatEvents.RaiseDeath(gameObject);
            if (Died != null) Died(this);
        }

        private void RaiseChanged()
        {
            if (Changed != null) Changed(this);
        }
    }
}
