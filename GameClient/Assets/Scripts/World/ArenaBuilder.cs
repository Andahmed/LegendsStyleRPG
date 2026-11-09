using UnityEngine;
using LegendsRPG.Core;
using LegendsRPG.Vfx;

namespace LegendsRPG.World
{
    /// <summary>
    /// Procedurally builds the 3D battle arena: ground, boundary pillars,
    /// obelisks, scattered rocks, braziers, campfire, lighting and fog.
    /// Deterministic seed so every run looks identical (good for testing).
    /// </summary>
    public class ArenaBuilder : MonoBehaviour
    {
        private const int Seed = 20260101;
        public static GameObject ArenaRoot { get; private set; }

        public void Build()
        {
            Random.InitState(Seed);
            var arena = new GameObject("Arena");
            ArenaRoot = arena;

            BuildLighting();
            BuildGround(arena.transform);
            BuildBoundary(arena.transform);
            BuildProps(arena.transform);
            BuildBraziers(arena.transform);
            BuildCampfire(arena.transform);
        }

        /// <summary>Re-applies the dark battle atmosphere (city switches it back).</summary>
        public static void ApplyBattleMood()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.28f, 0.32f, 0.45f);
            RenderSettings.ambientEquatorColor = new Color(0.20f, 0.21f, 0.28f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.11f, 0.10f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.10f);
        }

        private void BuildLighting()
        {
            // directional light is created in the boot scene by the editor
            // bootstrap; make sure one exists at runtime as a safety net.
            bool hasDirectional = false;
            var lights = FindObjectsOfType<Light>();
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional) { hasDirectional = true; break; }
            }
            if (!hasDirectional)
            {
                var go = new GameObject("Sun");
                var l = go.AddComponent<Light>();
                l.type = LightType.Directional;
                l.intensity = 1.05f;
                l.shadows = LightShadows.Soft;
                go.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.28f, 0.32f, 0.45f);
            RenderSettings.ambientEquatorColor = new Color(0.20f, 0.21f, 0.28f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.11f, 0.10f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.10f);
        }

        private void BuildGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(4.2f, 1f, 4.2f);   // ~42m x 42m
            ground.GetComponent<MeshRenderer>().material = MaterialLibrary.GroundMaterial;
            ground.GetComponent<MeshRenderer>().receiveShadows = true;
        }

        private void BuildBoundary(Transform parent)
        {
            var pillarMat = MaterialLibrary.Solid(new Color(0.32f, 0.30f, 0.34f), 0.15f, 0.3f);
            var obeliskMat = MaterialLibrary.Emissive(new Color(0.55f, 0.35f, 0.9f), 0.9f);

            int pillars = 28;
            for (int i = 0; i < pillars; i++)
            {
                float a = i * 360f / pillars * Mathf.Deg2Rad;
                float h = Random.Range(0.6f, 1.6f);
                var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(p.GetComponent<Collider>());
                p.name = "WallPillar" + i;
                p.transform.SetParent(parent, false);
                p.transform.position = new Vector3(Mathf.Sin(a) * GameConfig.ArenaRadius, h * 0.5f, Mathf.Cos(a) * GameConfig.ArenaRadius);
                p.transform.localScale = new Vector3(0.9f, h, 0.9f);
                p.GetComponent<MeshRenderer>().material = pillarMat;
            }

            int obelisks = 6;
            for (int i = 0; i < obelisks; i++)
            {
                float a = (i * 60f + 12f) * Mathf.Deg2Rad;
                float h = Random.Range(3.2f, 4.6f);
                var o = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(o.GetComponent<Collider>());
                o.name = "Obelisk" + i;
                o.transform.SetParent(parent, false);
                o.transform.position = new Vector3(Mathf.Sin(a) * (GameConfig.ArenaRadius + 1.4f), h * 0.5f, Mathf.Cos(a) * (GameConfig.ArenaRadius + 1.4f));
                o.transform.localScale = new Vector3(0.7f, h * 0.5f, 0.7f);
                o.GetComponent<MeshRenderer>().material = obeliskMat;
            }
        }

        private void BuildProps(Transform parent)
        {
            var rockMat = MaterialLibrary.Solid(new Color(0.26f, 0.25f, 0.28f), 0.1f, 0.05f);

            for (int i = 0; i < 14; i++)
            {
                float a = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(7f, GameConfig.ArenaRadius - 3f);
                var rock = GameObject.CreatePrimitive(Random.value > 0.5f ? PrimitiveType.Cube : PrimitiveType.Sphere);
                Destroy(rock.GetComponent<Collider>());
                rock.name = "Rock" + i;
                rock.transform.SetParent(parent, false);
                rock.transform.position = new Vector3(Mathf.Sin(a) * r, Random.Range(0.15f, 0.5f), Mathf.Cos(a) * r);
                rock.transform.rotation = Quaternion.Euler(Random.Range(0f, 90f), Random.Range(0f, 360f), Random.Range(0f, 90f));
                float s = Random.Range(0.5f, 1.4f);
                rock.transform.localScale = new Vector3(s, s * Random.Range(0.5f, 0.9f), s);
                rock.GetComponent<MeshRenderer>().material = rockMat;
            }
        }

        private void BuildBraziers(Transform parent)
        {
            var postMat = MaterialLibrary.Solid(new Color(0.2f, 0.18f, 0.16f), 0.1f, 0.4f);
            var flameMat = MaterialLibrary.Emissive(new Color(1f, 0.55f, 0.15f), 2.4f);

            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(a) * 15f, 0f, Mathf.Cos(a) * 15f);

                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(post.GetComponent<Collider>());
                post.name = "Brazier" + i;
                post.transform.SetParent(parent, false);
                post.transform.position = pos + Vector3.up * 0.65f;
                post.transform.localScale = new Vector3(0.22f, 0.65f, 0.22f);
                post.GetComponent<MeshRenderer>().material = postMat;

                var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(flame.GetComponent<Collider>());
                flame.name = "BrazierFlame" + i;
                flame.transform.SetParent(parent, false);
                flame.transform.position = pos + Vector3.up * 1.5f;
                flame.transform.localScale = Vector3.one * 0.28f;
                flame.GetComponent<MeshRenderer>().material = flameMat;

                var lightGo = new GameObject("BrazierLight" + i);
                lightGo.transform.SetParent(parent, false);
                lightGo.transform.position = pos + Vector3.up * 2.2f;
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.6f, 0.25f);
                light.range = 11f;
                light.intensity = 1.35f;
            }
        }

        private void BuildCampfire(Transform parent)
        {
            var lightGo = new GameObject("CampfireLight");
            lightGo.transform.SetParent(parent, false);
            lightGo.transform.position = new Vector3(0f, 1.2f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.55f, 0.2f);
            light.range = 8f;
            light.intensity = 1.1f;
            lightGo.AddComponent<CampfireFlicker>().Light = light;

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(ring.GetComponent<Collider>());
            ring.name = "CampfireRing";
            ring.transform.SetParent(parent, false);
            ring.transform.position = new Vector3(0f, 0.06f, 0f);
            ring.transform.localScale = new Vector3(0.9f, 0.03f, 0.9f);
            ring.GetComponent<MeshRenderer>().material = MaterialLibrary.Solid(new Color(0.15f, 0.13f, 0.12f), 0.05f, 0f);

            VFXFactory.SpawnContinuous("campfire", new Vector3(0f, 0.15f, 0f),
                new Color(1f, 0.5f, 0.15f), 1f);
        }
    }

    /// <summary>Small light flicker for atmosphere.</summary>
    public class CampfireFlicker : MonoBehaviour
    {
        public Light Light;
        private float _base;

        private void Start() { _base = Light != null ? Light.intensity : 1f; }

        private void Update()
        {
            if (Light == null) return;
            Light.intensity = _base + Mathf.PerlinNoise(Time.time * 3.5f, 7.3f) * 0.5f;
        }
    }
}
