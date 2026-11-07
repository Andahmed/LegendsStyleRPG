using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Core;
using LegendsRPG.Vfx;

namespace LegendsRPG.Combat
{
    /// <summary>
    /// Executes hero skills: cooldown tracking, ultimate charge, VFX + audio,
    /// damage via the shared CombatUtil pipeline. Every skill kind is fully
    /// functional (melee / projectile / AoE / heal / ultimates).
    /// </summary>
    public class SkillCaster : MonoBehaviour
    {
        public SkillData[] Skills = new SkillData[3];
        public float UltCharge { get; private set; }   // 0..100

        private readonly float[] _cooldownLeft = new float[3];
        private CharacterStats _stats;
        private CharacterAnimator _anim;
        private HeroCharacter _hero;

        public void Bind(HeroCharacter hero, SkillData s1, SkillData s2, SkillData ult)
        {
            _hero = hero;
            _stats = hero.Stats;
            _anim = hero.Anim;
            Skills[0] = s1; Skills[1] = s2; Skills[2] = ult;
        }

        private void Update()
        {
            for (int i = 0; i < _cooldownLeft.Length; i++)
            {
                if (_cooldownLeft[i] > 0f) _cooldownLeft[i] -= Time.deltaTime;
            }
        }

        public void AddUltCharge(float amount)
        {
            if (Skills[2] == null) return;
            UltCharge = Mathf.Clamp(UltCharge + amount, 0f, 100f);
        }

        public float GetCooldownLeft(int index)
        {
            return index >= 0 && index < 3 ? Mathf.Max(0f, _cooldownLeft[index]) : 0f;
        }

        public float GetCooldownRatio(int index)
        {
            if (index < 0 || index >= 3 || Skills[index] == null) return 0f;
            return Mathf.Clamp01(_cooldownLeft[index] / Mathf.Max(0.01f, Skills[index].cooldownSeconds));
        }

        public bool TryCast(int index, Transform target)
        {
            if (_stats == null || Skills == null) return false;
            if (index < 0 || index >= Skills.Length) return false;
            var skill = Skills[index];
            if (skill == null) return false;
            if (_cooldownLeft[index] > 0f) return false;
            if (skill.IsUltimate && UltCharge < skill.ultimateChargeCost) return false;

            bool casted = false;
            switch (skill.kind)
            {
                case SkillKind.MeleeStrike:
                    casted = CastMelee(skill, target);
                    break;
                case SkillKind.Projectile:
                    casted = CastProjectile(skill, target);
                    break;
                case SkillKind.AoEBurst:
                    casted = CastAoE(skill);
                    break;
                case SkillKind.HealAlly:
                    casted = CastHeal(skill);
                    break;
                case SkillKind.UltimateNuke:
                    casted = CastUltimateNuke(skill, target);
                    break;
                case SkillKind.UltimateAoe:
                    casted = CastUltimateAoe(skill);
                    break;
            }

            if (casted)
            {
                _cooldownLeft[index] = skill.cooldownSeconds;
                if (skill.IsUltimate) UltCharge = 0f;
                if (_anim != null && skill.kind == SkillKind.MeleeStrike) _anim.PlayAttack(0.4f);
                if (_anim != null && skill.kind != SkillKind.MeleeStrike) _anim.PlayCast(0.6f);
                Audio.AudioSystem.Play(skill.IsUltimate ? "Ult" : "Skill");
                if (skill.cameraShake) ShakeCamera(skill.shakeMagnitude);
            }
            return casted;
        }

        private bool CastMelee(SkillData s, Transform target)
        {
            var m = CombatUtil.FindNearestMonster(transform.position, s.castRange + 0.6f);
            if (m == null) return false;
            Face(m.transform.position);
            CombatUtil.DealDamage(_stats, m.Stats, _stats.Attack * s.powerMultiplier, s.element, true, gameObject);
            VFXFactory.Spawn(s.vfxKey, m.transform.position + Vector3.up * 1.2f, s.vfxColor, 1f);
            return true;
        }

        private bool CastProjectile(SkillData s, Transform target)
        {
            Transform from = _hero != null && _hero.WeaponTip != null ? _hero.WeaponTip : transform;
            Vector3 origin = from.position + from.forward * 0.4f + Vector3.up * 0.2f;

            var m = target != null && target.GetComponentInParent<MonsterCharacter>() != null
                ? target.GetComponentInParent<MonsterCharacter>()
                : CombatUtil.FindNearestMonster(transform.position, Mathf.Max(s.castRange, 11f));
            if (m == null) return false;

            Face(m.transform.position);
            Projectile.Launch(origin, m, s, _stats, gameObject);
            VFXFactory.Spawn("cast", origin, s.vfxColor, 0.6f);
            return true;
        }

        private bool CastAoE(SkillData s)
        {
            var targets = CombatUtil.MonstersWithin(transform.position, s.aoeRadius);
            if (targets.Count == 0) return false;
            VFXFactory.Spawn(s.vfxKey, transform.position + Vector3.up * 0.4f, s.vfxColor, s.aoeRadius / 3.5f);
            foreach (var m in targets)
            {
                CombatUtil.DealDamage(_stats, m.Stats, _stats.Attack * s.powerMultiplier, s.element, true, gameObject);
            }
            return true;
        }

        private bool CastHeal(SkillData s)
        {
            var team = Team.TeamManager.InstanceRef;
            if (team == null) return false;

            Health lowest = null;
            foreach (var h in team.Heroes)
            {
                if (h == null || h.HP == null || h.HP.IsDead) continue;
                if (lowest == null || h.HP.Ratio < lowest.Ratio) lowest = h.HP;
            }
            if (lowest == null) return false;

            float heal = _stats.Attack * s.powerMultiplier;
            lowest.Heal(heal);
            VFXFactory.Spawn(s.vfxKey, lowest.transform.position + Vector3.up * 1.2f, s.vfxColor, 1f);
            DamagePopup.Spawn(lowest.transform.position + Vector3.up * 2.4f,
                "+" + Mathf.RoundToInt(heal), new Color(0.4f, 1f, 0.45f));
            return true;
        }

        private bool CastUltimateNuke(SkillData s, Transform target)
        {
            var m = target != null && target.GetComponentInParent<MonsterCharacter>() != null
                ? target.GetComponentInParent<MonsterCharacter>()
                : CombatUtil.FindNearestMonster(transform.position, 13f);
            if (m == null) return false;

            Face(m.transform.position);
            var skill = ScriptableObject.Instantiate(s);
            skill.powerMultiplier = s.powerMultiplier;
            Projectile.Launch(transform.position + Vector3.up * 1.4f, m, skill, _stats, gameObject, true);
            VFXFactory.Spawn("aura", transform.position + Vector3.up * 0.2f, s.vfxColor, 1.2f);
            return true;
        }

        private bool CastUltimateAoe(SkillData s)
        {
            var targets = CombatUtil.MonstersWithin(transform.position, s.aoeRadius);
            if (targets.Count == 0) return false;

            VFXFactory.Spawn("ultimate", transform.position + Vector3.up * 0.5f, s.vfxColor, s.aoeRadius / 3.5f);
            foreach (var m in targets)
            {
                CombatUtil.DealDamage(_stats, m.Stats, _stats.Attack * s.powerMultiplier, s.element, true, gameObject);
            }
            return true;
        }

        private void Face(Vector3 pos)
        {
            var motor = GetComponent<CharacterMotor>();
            if (motor != null) motor.FaceTowards(pos);
        }

        private void ShakeCamera(float magnitude)
        {
            var cam = FindCamera();
            if (cam != null) cam.Shake(0.35f, magnitude);
        }

        private Cam.IsometricCameraController FindCamera()
        {
            return Camera.main != null ? Camera.main.GetComponent<Cam.IsometricCameraController>() : null;
        }
    }
}
