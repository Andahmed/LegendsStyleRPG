using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Core;
using LegendsRPG.Vfx;

namespace LegendsRPG.Combat
{
    /// <summary>
    /// Pooled projectile with optional homing. Carries the caster stats so the
    /// full damage formula (crit + elemental + defense) resolves at impact.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private Transform _target;
        private SkillData _skill;
        private CharacterStats _caster;
        private GameObject _source;
        private float _speed;
        private float _life;
        private bool _heavy;
        private Vector3 _dir;

        private GameObject _visual;

        public static void Launch(Vector3 origin, MonsterCharacter target, SkillData skill,
            CharacterStats caster, GameObject source, bool heavy = false)
        {
            if (target == null) return;
            var go = ObjectPool.Instance.Get("projectile", () => BuildProjectile());
            var p = go.GetComponent<Projectile>();
            p.transform.position = origin;
            p.transform.localScale = Vector3.one * (heavy ? 1.6f : 1f);
            p.Init(target.transform, skill, caster, source, heavy);
        }

        private static GameObject BuildProjectile()
        {
            var go = new GameObject("Projectile");
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(sphere.GetComponent<Collider>());
            sphere.transform.SetParent(go.transform, false);
            sphere.transform.localScale = Vector3.one * 0.42f;
            sphere.GetComponent<MeshRenderer>().material = MaterialLibrary.Emissive(new Color(1f, 0.6f, 0.2f), 2.4f);
            go.AddComponent<Light>().range = 4f;
            go.AddComponent<Projectile>();
            return go;
        }

        private void Init(Transform target, SkillData skill, CharacterStats caster, GameObject source, bool heavy)
        {
            _target = target;
            _skill = skill;
            _caster = caster;
            _source = source;
            _heavy = heavy;
            _speed = skill.projectileSpeed * (heavy ? 0.8f : 1f);
            _life = 3.5f;
            _dir = (target.position + Vector3.up * 1.2f - transform.position).normalized;

            _visual = transform.GetChild(0).gameObject;
            var r = _visual.GetComponent<MeshRenderer>();
            if (r != null)
            {
                var mat = MaterialLibrary.Emissive(skill.vfxColor, 2.4f);
                r.material = mat;
                var light = GetComponent<Light>();
                if (light != null) light.color = skill.vfxColor;
            }

            VFXFactory.AttachTrail(gameObject, skill.vfxColor);
        }

        private void Update()
        {
            if (_life <= 0f) return;
            _life -= Time.deltaTime;
            if (_life <= 0f)
            {
                Release();
                return;
            }

            Vector3 aim = _dir;
            if (_target != null && _target.gameObject.activeInHierarchy)
            {
                aim = (_target.position + Vector3.up * 1.2f - transform.position).normalized;
            }

            transform.position += aim * _speed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(aim);

            // hit check
            if (_target != null && _target.gameObject.activeInHierarchy)
            {
                float dist = Vector3.Distance(transform.position, _target.position + Vector3.up * 1.1f);
                if (dist < 0.75f)
                {
                    Impact(_target.GetComponentInParent<MonsterCharacter>());
                }
            }
        }

        private void Impact(MonsterCharacter m)
        {
            _life = 0f;
            if (m != null && m.Stats != null && _skill != null)
            {
                CombatUtil.DealDamage(_caster, m.Stats, _caster.Attack * _skill.powerMultiplier,
                    _skill.element, true, _source);
                VFXFactory.Spawn(_heavy ? "explosion" : "hit",
                    transform.position, _skill.vfxColor, _heavy ? 1.6f : 1f);
            }
            Release();
        }

        private void Release()
        {
            _target = null;
            if (ObjectPool.Instance != null)
            {
                ObjectPool.Instance.Release("projectile", gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
