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

        public void Build()
        {
            Random.InitState(Seed);
            var arena = new GameObject("Arena");

            BuildLighting();
            BuildGround(arena.transform);
            BuildBoundary(arena.transform);
            BuildProps(arena.transform);
            BuildBraziers(arena.transform);
            BuildCampfire(arena.transform);
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
                go.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
            }
            // tune whichever directional light exists (the bootstrap one included)
            foreach (var dl in FindObjectsOfType<Light>())
            {
                if (dl.type != LightType.Directional) continue;
                dl.intensity = 1.15f;
                dl.color = new Color(1f, 0.93f, 0.82f);          // warm key light
                dl.shadows = LightShadows.Soft;
                dl.shadowStrength = 0.75f;
                dl.shadowBias = 0.05f;
                dl.shadowNormalBias = 0.4f;
                break;
            }
            QualitySettings.shadowDistance = 45f;
            QualitySettings.shadowCascades = 2;
            QualitySettings.antiAliasing = 4;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            QualitySettings.pixelLightCount = 6;

            // gradient sky (guarded: shader may be missing in a stripped build)
            var skySh = Shader.Find("Skybox/Procedural");
            if (skySh != null)
            {
                var sky = new Material(skySh);
                sky.SetFloat("_AtmosphereThickness", 0.6f);
                sky.SetColor("_SkyTint", new Color(0.35f, 0.38f, 0.6f));
                sky.SetColor("_GroundColor", new Color(0.08f, 0.08f, 0.12f));
                sky.SetFloat("_Exposure", 0.7f);
                RenderSettings.skybox = sky;
                var cam = Camera.main;
                if (cam != null) cam.clearFlags = CameraClearFlags.Skybox;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.42f, 0.46f, 0.62f);
            RenderSettings.ambientEquatorColor = new Color(0.30f, 0.31f, 0.40f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.11f, 0.10f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.010f;
            RenderSettings.fogColor = new Color(0.10f, 0.11f, 0.18f);
        }

        private void BuildGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(5.2f, 1f, 5.2f);   // ~52m x 52m
            ground.GetComponent<MeshRenderer>().material = MaterialLibrary.GroundMaterial;
            ground.GetComponent<MeshRenderer>().receiveShadows = true;
        }

        private void BuildBoundary(Transform parent)
        {
            var pillarMat = MaterialLibrary.Solid(new Color(0.42f, 0.38f, 0.36f), 0.15f, 0.2f);
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
            var rockMat = MaterialLibrary.Solid(new Color(0.36f, 0.33f, 0.31f), 0.1f, 0.05f);

            for (int i = 0; i < 14; i++)
            {
                float a = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(9f, GameConfig.ArenaRadius - 3f);
                var rock = GameObject.CreatePrimitive(Random.value > 0.5f ? PrimitiveType.Cube : PrimitiveType.Sphere);
                Destroy(rock.GetComponent<Collider>());
                rock.name = "Rock" + i;
                rock.transform.SetParent(parent, false);
                Vector3 rp = new Vector3(Mathf.Sin(a) * r, 0f, Mathf.Cos(a) * r);
                if ((rp - new Vector3(0f, 0f, 8f)).magnitude < 7f) { Destroy(rock); continue; }   // keep the party area clear
                rock.transform.position = new Vector3(rp.x, Random.Range(0.1f, 0.3f), rp.z);
                rock.transform.rotation = Quaternion.Euler(Random.Range(0f, 90f), Random.Range(0f, 360f), Random.Range(0f, 90f));
                float s = Random.Range(0.5f, 1.1f);
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
