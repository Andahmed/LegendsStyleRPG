using System.Collections;
using UnityEngine;
using LegendsRPG.Core;

namespace LegendsRPG.Vfx
{
    /// <summary>
    /// Procedural particle VFX: hit sparks, slashes, fireballs, ice nova,
    /// lightning, heal motes, explosions, ultimate shockwaves, trails...
    /// Every effect is pooled and configured in code — zero asset files,
    /// full element-color support, mobile-friendly budgets.
    /// </summary>
    public class VFXFactory : MonoBehaviour
    {
        public static VFXFactory Instance { get; private set; }

        private Transform _root;

        private void Awake()
        {
            Instance = this;
            _root = new GameObject("VfxRoot").transform;
            _root.SetParent(transform, false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static GameObject Spawn(string key, Vector3 pos, Color color, float scale = 1f)
        {
            if (Instance == null || ObjectPool.Instance == null) return null;
            string poolKey = "vfx_" + key;
            var go = ObjectPool.Instance.Get(poolKey, () => BuildOneShot(key));
            go.transform.SetParent(Instance._root, false);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * scale;

            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = new ParticleSystem.MinMaxGradient(color);
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;

                var flash = go.GetComponent<Light>();
                if (flash != null) flash.color = color;

                ps.Clear();
                ps.Play();
                float life = main.duration + main.startLifetime.constantMax + 0.4f;
                ObjectPool.Instance.Release(poolKey, go, life);
            }
            return go;
        }

        public static GameObject SpawnContinuous(string key, Vector3 pos, Color color, float scale = 1f)
        {
            var go = BuildOneShot(key, looping: true);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * scale;
            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);
            ps.Play();
            return go;
        }

        public static GameObject AttachTrail(GameObject target, Color color)
        {
            var existing = target.transform.Find("Trail");
            GameObject trailGo;
            if (existing != null)
            {
                trailGo = existing.gameObject;
            }
            else
            {
                trailGo = new GameObject("Trail");
                trailGo.transform.SetParent(target.transform, false);
                var ps = trailGo.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.duration = 1f;
                main.loop = true;
                main.startLifetime = 0.35f;
                main.startSpeed = 0f;
                main.startSize = 0.3f;
                main.maxParticles = 60;
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                var em = ps.emission;
                em.rateOverTime = 40f;
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.12f;
                var rend = ps.GetComponent<ParticleSystemRenderer>();
                rend.material = MaterialLibrary.ParticleMaterial;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                trailGo.GetComponent<ParticleSystemRenderer>().trailMaterial = null;
            }

            var trailPs = trailGo.GetComponent<ParticleSystem>();
            var tmain = trailPs.main;
            tmain.startColor = new ParticleSystem.MinMaxGradient(color);
            return trailGo;
        }

        // ---- builders ----
        private static GameObject BuildOneShot(string key, bool looping = false)
        {
            var go = new GameObject("VFX_" + key);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = looping;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new ParticleSystem.MinMaxGradient(Color.white);

            var em = ps.emission;
            em.rateOverTime = looping ? 18f : 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.1f));

            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.material = MaterialLibrary.ParticleMaterial;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;

            switch (key)
            {
                case "hit":
                    Burst(ps, 14, 0.35f, 3.2f, 0.28f);
                    shape.radius = 0.12f;
                    break;

                case "slash":
                    Burst(ps, 10, 0.3f, 2.6f, 0.22f, spread: 0.4f);
                    break;

                case "cast":
                    Burst(ps, 10, 0.4f, 1.2f, 0.2f);
                    break;

                case "fireball":
                    Burst(ps, 26, 0.5f, 4.5f, 0.4f);
                    AddLight(go, colorHolder: null, 5f);
                    break;

                case "ice_shard":
                    Burst(ps, 20, 0.45f, 5f, 0.28f);
                    break;

                case "ice_nova":
                    RingBurst(ps, 40, 0.8f, 7.5f, 0.35f);
                    break;

                case "lightning":
                    Beam(ps);
                    AddLight(go, null, 6f);
                    break;

                case "nature_bolt":
                    Burst(ps, 18, 0.4f, 3.5f, 0.3f);
                    break;

                case "dark_bolt":
                    Burst(ps, 18, 0.45f, 3.8f, 0.32f);
                    break;

                case "heal":
                    RisingMotes(ps, 22, 0.9f, 1.6f, 0.22f);
                    break;

                case "explosion":
                    Burst(ps, 42, 0.7f, 7f, 0.55f);
                    AddLight(go, null, 9f);
                    break;

                case "death":
                    Implode(ps, 26, 0.6f);
                    break;

                case "aura":
                    Orbiting(ps, 24, 1.1f);
                    break;

                case "ultimate":
                    Burst(ps, 60, 1.0f, 9f, 0.7f);
                    RingBurst(ps, 30, 0.9f, 9f, 0.4f);
                    AddLight(go, null, 12f);
                    break;

                case "levelup":
                    RisingMotes(ps, 30, 1.1f, 2.2f, 0.26f);
                    AddLight(go, null, 7f);
                    break;

                case "campfire":
                    em.rateOverTime = 14f;
                    main.startLifetime = 1.1f;
                    main.startSpeed = 1.4f;
                    main.startSize = 0.32f;
                    main.gravityModifier = -0.35f;
                    shape.radius = 0.22f;
                    break;

                default:
                    Burst(ps, 12, 0.4f, 3f, 0.25f);
                    break;
            }
            return go;
        }

        private static void ConfigureDefaults(ParticleSystem ps, float lifetime, float size)
        {
            var main = ps.main;
            main.startLifetime = lifetime;
            main.startSize = size;
        }

        private static void Burst(ParticleSystem ps, int count, float lifetime, float speed, float size,
            float spread = 1f)
        {
            var main = ps.main;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.gravityModifier = 0.25f * spread;

            var em = ps.emission;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.18f;
        }

        private static void RingBurst(ParticleSystem ps, int count, float lifetime, float speed, float size)
        {
            var main = ps.main;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;

            var em = ps.emission;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.25f;
        }

        private static void RisingMotes(ParticleSystem ps, int count, float lifetime, float speed, float size)
        {
            var main = ps.main;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.gravityModifier = -0.2f;

            var em = ps.emission;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
        }

        private static void Implode(ParticleSystem ps, int count, float lifetime)
        {
            var main = ps.main;
            main.startLifetime = lifetime;
            main.startSpeed = 2.5f;
            main.startSize = 0.3f;
            main.gravityModifier = 0f;

            var em = ps.emission;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.9f;
        }

        private static void Orbiting(ParticleSystem ps, int count, float lifetime)
        {
            var main = ps.main;
            main.startLifetime = lifetime;
            main.startSpeed = 0.4f;
            main.startSize = 0.26f;
            main.gravityModifier = 0f;

            var em = ps.emission;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.8f;
        }

        private static void Beam(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = 0.35f;
            main.startSpeed = 14f;
            main.startSize = 0.45f;
            main.gravityModifier = 0f;

            var em = ps.emission;
            em.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 6f;
            shape.radius = 0.05f;
            ps.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);   // beam strikes downward
        }

        private static void AddLight(GameObject go, Color? colorHolder, float range)
        {
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = range;
            l.intensity = 2.4f;
            l.shadows = LightShadows.None;
        }
    }
}
