using UnityEngine;
using LegendsRPG.Art;
using UnityEngine.EventSystems;
using LegendsRPG.Core;

namespace LegendsRPG.World
{
    /// <summary>
    /// The Legends-Reborn style 3D home city: central plaza with the AFK
    /// treasure chest, ten functional buildings around the ring (summon
    /// fountain, guild hall, arena, dungeon gate, continent gate, ranking
    /// obelisk, mine, beast den, market, forge), trees, lanterns, walls and
    /// the hero party idling/walking in the plaza. Every building is really
    /// clickable and opens its real screen.
    /// </summary>
    public class CityBuilder : MonoBehaviour
    {
        public static Vector3 CitySpawnPoint => new Vector3(0f, 0f, 7f);
        public static Vector3 CityCenter => Vector3.zero;

        private static CityWalker _walker;

        public void Build()
        {
            var city = new GameObject("City");

            BuildEnvironment();
            BuildGround(city.transform);
            // Imported Quaternius foliage replaces the empty outskirts around the fallback city.
            QuaterniusAssetLibrary.BuildCityNature(city.transform);
            BuildPlaza(city.transform);
            BuildAFKChest(city.transform);
            BuildBuildings(city.transform);
            BuildDecorations(city.transform);
            BuildWalls(city.transform);

            if (_walker == null)
            {
                var go = new GameObject("CityWalker");
                _walker = go.AddComponent<CityWalker>();
            }
        }

        // ================= environment =================
        /// <summary>Bright city atmosphere (battles switch to the dark mood).</summary>
        public static void ApplyCityMood()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.62f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.42f, 0.45f);
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.24f, 0.20f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.fogColor = new Color(0.62f, 0.72f, 0.86f);
        }

        private static void BuildEnvironment()
        {
            bool hasDirectional = false;
            foreach (var l in Object.FindObjectsOfType<Light>())
            {
                if (l.type == LightType.Directional) { hasDirectional = true; break; }
            }
            if (!hasDirectional)
            {
                var go = new GameObject("Sun");
                var l = go.AddComponent<Light>();
                l.type = LightType.Directional;
                l.intensity = 1.1f;
                l.shadows = LightShadows.Soft;
                go.transform.rotation = Quaternion.Euler(50f, -40f, 0f);
            }

            // bright day mood — distinctly different from the battle arena
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.62f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.42f, 0.45f);
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.24f, 0.20f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.fogColor = new Color(0.62f, 0.72f, 0.86f);
        }

        private void BuildGround(Transform parent)
        {
            var grass = GameObject.CreatePrimitive(PrimitiveType.Plane);
            grass.name = "CityGround";
            grass.transform.SetParent(parent, false);
            grass.transform.localScale = new Vector3(9.5f, 1f, 9.5f);   // ~95m
            grass.GetComponent<MeshRenderer>().material = Vfx.MaterialLibrary.Solid(
                new Color(0.32f, 0.52f, 0.30f), 0.8f, 0.2f);
        }

        private void BuildPlaza(Transform parent)
        {
            var stone = Vfx.MaterialLibrary.Solid(new Color(0.62f, 0.58f, 0.52f), 0.9f, 0.25f);
            var dark = Vfx.MaterialLibrary.Solid(new Color(0.45f, 0.42f, 0.38f), 0.9f, 0.25f);

            // main plaza disc
            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(plaza.GetComponent<Collider>());
            plaza.name = "Plaza";
            plaza.transform.SetParent(parent, false);
            plaza.transform.position = new Vector3(0f, 0.02f, 0f);
            plaza.transform.localScale = new Vector3(24f, 0.02f, 24f);
            plaza.GetComponent<MeshRenderer>().material = stone;

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(ring.GetComponent<Collider>());
            ring.name = "PlazaRing";
            ring.transform.SetParent(parent, false);
            ring.transform.position = new Vector3(0f, 0.03f, 0f);
            ring.transform.localScale = new Vector3(25f, 0.015f, 25f);
            ring.GetComponent<MeshRenderer>().material = dark;

            // radial paths to each building
            for (int i = 0; i < 10; i++)
            {
                float a = (i * 36f + 18f) * Mathf.Deg2Rad;
                var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(path.GetComponent<Collider>());
                path.name = "Path" + i;
                path.transform.SetParent(parent, false);
                path.transform.position = new Vector3(Mathf.Sin(a) * 16f, 0.025f, Mathf.Cos(a) * 16f);
                path.transform.rotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
                path.transform.localScale = new Vector3(3.4f, 0.03f, 12f);
                path.GetComponent<MeshRenderer>().material = stone;
            }

            // fountain centerpiece
            var fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(fountain.GetComponent<Collider>());
            fountain.name = "FountainBase";
            fountain.transform.SetParent(parent, false);
            fountain.transform.position = new Vector3(0f, 0.35f, -3.5f);
            fountain.transform.localScale = new Vector3(4f, 0.35f, 4f);
            fountain.GetComponent<MeshRenderer>().material = stone;

            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(water.GetComponent<Collider>());
            water.name = "FountainWater";
            water.transform.SetParent(parent, false);
            water.transform.position = new Vector3(0f, 0.62f, -3.5f);
            water.transform.localScale = new Vector3(3.6f, 0.03f, 3.6f);
            water.GetComponent<MeshRenderer>().material = Vfx.MaterialLibrary.Emissive(new Color(0.25f, 0.55f, 0.9f), 0.8f);

            Vfx.VFXFactory.SpawnContinuous("heal", new Vector3(0f, 1.1f, -3.5f),
                new Color(0.5f, 0.8f, 1f), 1.2f);
        }

        // ================= AFK chest =================
        private void BuildAFKChest(Transform parent)
        {
            var chest = new GameObject("AFKChest");
            chest.transform.SetParent(parent, false);
            chest.transform.position = new Vector3(0f, 0f, 1.8f);

            var goldMat = Vfx.MaterialLibrary.Solid(new Color(0.85f, 0.68f, 0.2f), 0.7f, 0.4f);
            var woodMat = Vfx.MaterialLibrary.Solid(new Color(0.42f, 0.28f, 0.14f), 0.6f, 0.2f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "ChestBody";
            body.transform.SetParent(chest.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            body.transform.localScale = new Vector3(1.6f, 1.0f, 1.1f);
            body.GetComponent<MeshRenderer>().material = woodMat;

            var lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = "ChestLid";
            lid.transform.SetParent(chest.transform, false);
            lid.transform.localPosition = new Vector3(0f, 1.12f, -0.28f);
            lid.transform.localScale = new Vector3(1.6f, 0.35f, 0.6f);
            lid.GetComponent<MeshRenderer>().material = goldMat;

            var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(glow.GetComponent<Collider>());
            glow.name = "ChestGlow";
            glow.transform.SetParent(chest.transform, false);
            glow.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            glow.transform.localScale = Vector3.one * 0.35f;
            glow.GetComponent<MeshRenderer>().material = Vfx.MaterialLibrary.Emissive(new Color(1f, 0.85f, 0.3f), 3.5f);

            var light = new GameObject("ChestLight");
            light.transform.SetParent(chest.transform, false);
            light.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            var l = light.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.8f, 0.35f);
            l.range = 7f;
            l.intensity = 1.2f;

            Vfx.VFXFactory.SpawnContinuous("levelup", new Vector3(0f, 1.6f, 1.8f),
                new Color(1f, 0.85f, 0.3f), 0.8f);

            var cb = chest.AddComponent<CityBuilding>();
            cb.Init("afk", "AFK Rewards", new Color(1f, 0.8f, 0.25f));
        }

        // ================= the 10 functional buildings =================
        private void BuildBuildings(Transform parent)
        {
            CreateSummonFountain(parent, 0);
            CreateGuildHall(parent, 1);
            CreateArena(parent, 2);
            CreateDungeonGate(parent, 3);
            CreateContinentGate(parent, 4);
            CreateRankingObelisk(parent, 5);
            CreateMine(parent, 6);
            CreateBeastDen(parent, 7);
            CreateMarket(parent, 8);
            CreateForge(parent, 9);
        }

        private static Vector3 RingPos(int index, float extraRadius = 0f)
        {
            float a = (index * 36f + 18f) * Mathf.Deg2Rad;
            float r = GameConfig.BuildingRingRadius + extraRadius;
            return new Vector3(Mathf.Sin(a) * r, 0f, Mathf.Cos(a) * r);
        }

        private static void FaceCenter(GameObject go)
        {
            var dir = Vector3.zero - go.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f) go.transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        private static CityBuilding CreateShell(Transform parent, int index, string id, string label, Color accent)
        {
            var go = new GameObject("Building_" + id);
            go.transform.SetParent(parent, false);
            go.transform.position = RingPos(index);
            FaceCenter(go);

            var cb = go.AddComponent<CityBuilding>();
            cb.Init(id, label, accent);
            return cb;
        }

        private static MeshRenderer Prim(Transform parent, PrimitiveType type, Vector3 localPos, Vector3 scale, Material mat)
        {
            var p = GameObject.CreatePrimitive(type);
            Object.Destroy(p.GetComponent<Collider>());
            p.transform.SetParent(parent, false);
            p.transform.localPosition = localPos;
            p.transform.localScale = scale;
            p.GetComponent<MeshRenderer>().material = mat;
            return p.GetComponent<MeshRenderer>();
        }

        // 1 — Summon Fountain
        private static void CreateSummonFountain(Transform parent, int index)
        {
            var cb = CreateShell(parent, index, "summon", "SUMMON", new Color(0.55f, 0.8f, 1f));
            var t = cb.transform;
            var stone = Vfx.MaterialLibrary.Solid(new Color(0.7f, 0.68f, 0.64f), 0.85f, 0.2f);

            Prim(t, PrimitiveType.Cylinder, new Vector3(0f, 0.4f, 0f), new Vector3(5f, 0.4f, 5f), stone);
            Prim(t, PrimitiveType.Cylinder, new Vector3(0f, 0.85f, 0f), new Vector3(4.4f, 0.06f, 4.4f),
                Vfx.MaterialLibrary.Emissive(new Color(0.4f, 0.7f, 1f), 1.2f));
            Prim(t, PrimitiveType.Cylinder, new Vector3(0f, 1.6f, 0f), new Vector3(0.6f, 1.4f, 0.6f), stone);
            Prim(t, PrimitiveType.Sphere, new Vector3(0f, 3.2f, 0f), new Vector3(0.9f, 1.3f, 0.9f),
                Vfx.MaterialLibrary.Emissive(new Color(0.6f, 0.85f, 1f), 2.2f));

            for (int i = 0; i < 5; i++)
            {
                float a = i * 72f * Mathf.Deg2Rad;
                Prim(t, PrimitiveType.Cylinder, new Vector3(Mathf.Sin(a) * 2.8f, 0.9f, Mathf.Cos(a) * 2.8f),
                    new Vector3(0.3f, 0.5f, 0.3f), stone);
            }
            Vfx.VFXFactory.SpawnContinuous("heal", t.position + new Vector3(0f, 3.4f, 0f), new Color(0.6f, 0.85f, 1f), 1.4f);
        }

        // 2 — Guild Hall
        private static void CreateGuildHall(Transform parent, int index)
        {
            var cb = CreateShell(parent, index, "guild", "GUILD", new Color(0.9f, 0.65f, 0.25f));
            var t = cb.transform;
            var wall = Vfx.MaterialLibrary.Solid(new Color(0.55f, 0.42f, 0.3f), 0.7f, 0.3f);
            var roof = Vfx.MaterialLibrary.Solid(new Color(0.6f, 0.2f, 0.18f), 0.7f, 0.3f);
            var gold = Vfx.MaterialLibrary.Emissive(new Color(1f, 0.8f, 0.3f), 1.6f);

            Prim(t, PrimitiveType.Cube, new Vector3(0f, 2.2f, 0f), new Vector3(8f, 4.4f, 6f), wall);
            Prim(t, PrimitiveType.Cube, new Vector3(0f, 5.1f, 0f), new Vector3(9f, 1.4f, 7f), roof);
            Prim(t, PrimitiveType.Cylinder, new Vector3(-3.4f, 3.4f, 2.2f), new Vector3(0.8f, 3.6f, 0.8f), wall);
            Prim(t, PrimitiveType.Cylinder, new Vector3(3.4f, 3.4f, 2.2f), new Vector3(0.8f, 3.6f, 0.8f), wall);
            Prim(t, PrimitiveType.Sphere, new Vector3(-3.4f, 6.2f, 2.2f), new Vector3(1.1f, 1.1f, 1.1f), gold);
            Prim(t, PrimitiveType.Sphere, new Vector3(3.4f, 6.2f, 2.2f), new Vector3(1.1f, 1.1f, 1.1f), gold);
            Prim(t, PrimitiveType.Cube, new Vector3(0f, 1.2f, 3.05f), new Vector3(1.8f, 2.4f, 0.2f),
                Vfx.MaterialLibrary.Solid(new Color(0.28f, 0.18f, 0.1f), 0.6f, 0.2f));
        }

        // 3 — PVP Arena (colosseum)
        private static void CreateArena(Transform parent, int index)
        {
            var cb = CreateShell(parent, index, "arena", "ARENA", new Color(0.95f, 0.45f, 0.3f));
            var t = cb.transform;
            var stone = Vfx.MaterialLibrary.Solid(new Color(0.6f, 0.55f, 0.48f), 0.85f, 0.2f);
            var sand = Vfx.MaterialLibrary.Solid(new Color(0.85f, 0.75f, 0.5f), 0.9f, 0.15f);

            Prim(t, PrimitiveType.Cylinder, new Vector3(0f, 0.25f, 0f), new Vector3(9f, 0.25f, 9f), sand);
            for (int i = 0; i < 10; i++)
            {
                float a = i * 36f * Mathf.Deg2Rad;
                Prim(t, PrimitiveType.Cube, new Vector3(Mathf.Sin(a) * 4.6f, 1.1f, Mathf.Cos(a) * 4.6f),
                    new Vector3(1.6f, 2.2f, 0.7f), stone).transform.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
            }
            Prim(t, PrimitiveType.Cylinder, new Vector3(0f, 2.6f, 0f), new Vector3(0.01f, 0.01f, 0.01f), stone);
        }

        // 4 — Dungeon Gate
        private static void CreateDungeonGate(Transform parent, int index)
        {
            var cb = CreateShell(parent, index, "dungeon", "DUNGEON", new Color(0.5f, 0.4f, 0.85f));
            var t = cb.transform;
            var rock = Vfx.MaterialLibrary.Solid(new Color(0.32f, 0.3f, 0.34f), 0.6f, 0.1f);
            var portal = Vfx.MaterialLibrary.Emissive(new Color(0.55f, 0.35f, 0.95f), 2.6f);

            Prim(t, PrimitiveType.Cube, new Vector3(-2f, 2f, 0f), new Vector3(1.2f, 4f, 1.4f), rock);
            Prim(t, PrimitiveType.Cube, new Vector3(2f, 2f, 0f), new Vector3(1.2f, 4f, 1.4f), rock);
            Prim(t, PrimitiveType.Cube, new Vector3(0f, 4.4f, 0f), new Vector3(5.4f, 1.2f, 1.4f), rock);
            Prim(t, PrimitiveType.Cube, new Vector3(0f, 2f, 0f), new Vector3(2.8f, 3.9f, 0.25f), portal);
            Vfx.VFXFactory.SpawnContinuous("dark_bolt", t.position + new Vector3(0f, 2f, 0.4f), new Color(0.6f, 0.4f, 1f), 1.1f);
        }

        // 5 — Continent Gate
        private static void CreateContinentGate(Transform parent, int index)
        {
            var cb = CreateShell(parent, index, "continent", "CONTINENT", new Color(0.4f, 0.9f, 0.75f));
            var t = cb.transform;
            var marble = Vfx.MaterialLibrary.Solid(new Color(0.8f, 0.82f, 0.85f), 0.8f, 0.3f);

            Prim(t, PrimitiveType.Cylinder, new Vector3(-1.9f, 2.6f, 0f), new Vector3(0.7f, 2.6f, 0.7f), marble);
            Prim(t, PrimitiveType.Cylinder, new Vector3(1.9f, 2.6f, 0f), new Vector3(0.7f, 2.6f, 0.7f), marble);
            Prim(t, PrimitiveType.Cube, new Vector3(0f, 5.5f, 0f), new Vector3(4.8f, 0.7f, 0.9f), marble);
            var ring = Prim(t, PrimitiveType.Cylinder, new Vector3(0f, 2.7f, 0f), new Vector3(3.2f, 0.06f, 3.2f),
                Vfx.MaterialLibrary.Emissive(new Color(0.35f, 0.95f, 0.8f), 2.4f));
            ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Vfx.VFXFactory.SpawnContinuous("heal", t.position + new Vector3(0f, 2.8f, 0f), new Color(0.4f, 1f, 0.85f), 1.5f);
        }

        // 6 — Ranking Obelisk
        private static void CreateRankingObelisk(Transform parent, int index)
        {
            var cb = CreateShell(parent, index, "rank", "RANKING", new Color(1f, 0.85f, 0.4f));
            var t = cb.transform;
            var obsidian = Vfx.MaterialLibrary.Solid(new Color(0.16f, 0.15f, 0.2f), 0.4f, 0.25f);
            var gold = Vfx.MaterialLibrary.Emissive(new Color(1f, 0.85f, 0.3f), 2.8f);

            Prim(t, PrimitiveType.Cube, new Vector3(0f, 0.3f, 0f), new Vector3(3.4f, 0.6f, 3.4f), obsidian);
            Prim(t, PrimitiveType.Cube, new Vector3(0f, 3f, 0f), new Vector3(1.4f, 5f, 1.4f), obsidian);
            Prim(t, PrimitiveType.Sphere, new Vector3(0f, 5.9f, 0f), new Vector3(0.7f, 0.7f, 0.7f), gold);
            var l = new GameObject("ObeliskLight").AddComponent<Light>();
            l.transform.SetParent(t, false);
            l.transform.localPosition = new Vector3(0f, 6.4f, 0f);
            l.type = LightType.Point;
            l.color = new Color(1f, 0.85f, 0.4f);
            l.range = 10f;
            l.intensity = 1.3f;
        }

        // 7 — Mine
        private static void CreateMine(Transform parent, int index)
        {
            var cb = CreateShell(parent, index, "mine", "MINE", new Color(0.75f, 0.6f, 0.4f));
            var t = cb.transform;
            var rock = Vfx.MaterialLibrary.Solid(new Color(0.42f, 0.38f, 0.34f), 0.5f, 0.05f);
            var wood = Vfx.MaterialLibrary.Solid(new Color(0.45f, 0.3f, 0.16f), 0.6f, 0.2f);
            var gold = Vfx.MaterialLibrary.Emissive(new Color(1f, 0.8f, 0.25f), 1.4f);

            Prim(t, PrimitiveType.Sphere, new Vector3(0f, 1.6f, -1f), new Vector3(6.5f, 3.4f, 5f), rock);
            Prim(t, PrimitiveType.Cube, new Vector3(0f, 1.5f, 1.2f), new Vector3(2.2f, 3f, 0.35f), rock);
            Prim(t, PrimitiveType.Cube, new Vector3(-1.2f, 2.2f, 1.6f), new Vector3(0.3f, 2.4f, 0.3f), wood);
            Prim(t, PrimitiveType.Cube, new Vector3(1.2f, 2.2f, 1.6f), new Vector3(0.3f, 2.4f, 0.3f), wood);
            Prim(t, PrimitiveType.Cube, new Vector3(0f, 3.5f, 1.6f), new Vector3(2.8f, 0.3f, 0.3f), wood);
            Prim(t, PrimitiveType.Sphere, new Vector3(2.6f, 0.4f, 1.8f), new Vector3(0.45f, 0.45f, 0.45f), gold);
            Prim(t, PrimitiveType.Sphere, new Vector3(3.1f, 0.25f, 1.4f), new Vector3(0.3f, 0.3f, 0.3f), gold);
        }

        // 8 — Beast Den
        private static void CreateBeastDen(Transform parent, int index)
        {
            var cb = CreateShell(parent, index, "beast", "BEASTS", new Color(0.45f, 0.85f, 0.45f));
            var t = cb.transform;
            var den = Vfx.MaterialLibrary.Solid(new Color(0.4f, 0.5f, 0.35f), 0.6f, 0.1f);
            var bone = Vfx.MaterialLibrary.Solid(new Color(0.9f, 0.88f, 0.8f), 0.5f, 0.2f);
            var eye = Vfx.MaterialLibrary.Emissive(new Color(0.3f, 1f, 0.5f), 2.2f);

            Prim(t, PrimitiveType.Sphere, new Vector3(0f, 1.2f, -0.5f), new Vector3(6f, 3f, 5.5f), den);
            Prim(t, PrimitiveType.Sphere, new Vector3(0f, 0.6f, 2.2f), new Vector3(2.2f, 1.8f, 1.4f),
                Vfx.MaterialLibrary.Solid(new Color(0.1f, 0.12f, 0.1f), 0.2f, 0f));
            Prim(t, PrimitiveType.Cylinder, new Vector3(-2.2f, 1.8f, 1.8f), new Vector3(0.22f, 1.4f, 0.22f), bone)
                .transform.localRotation = Quaternion.Euler(20f, 0f, 25f);
            Prim(t, PrimitiveType.Cylinder, new Vector3(2.2f, 1.8f, 1.8f), new Vector3(0.22f, 1.4f, 0.22f), bone)
                .transform.localRotation = Quaternion.Euler(20f, 0f, -25f);
            Prim(t, PrimitiveType.Sphere, new Vector3(0.5f, 1.6f, 2.6f), new Vector3(0.25f, 0.25f, 0.25f), eye);
            Prim(t, PrimitiveType.Sphere, new Vector3(-0.5f, 1.6f, 2.6f), new Vector3(0.25f, 0.25f, 0.25f), eye);
        }

        // 9 — Market Bazaar
        private static void CreateMarket(Transform parent, int index)
        {
            var cb = CreateShell(parent, index, "shop", "SHOP", new Color(0.95f, 0.7f, 0.3f));
            var t = cb.transform;
            var wood = Vfx.MaterialLibrary.Solid(new Color(0.5f, 0.34f, 0.18f), 0.6f, 0.25f);
            var canopyA = Vfx.MaterialLibrary.Solid(new Color(0.85f, 0.3f, 0.3f), 0.8f, 0.2f);
            var canopyB = Vfx.MaterialLibrary.Solid(new Color(0.95f, 0.85f, 0.6f), 0.8f, 0.2f);

            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 3.4f;
                Prim(t, PrimitiveType.Cube, new Vector3(x, 0.9f, 0f), new Vector3(2.4f, 1.8f, 1.6f), wood);
                Prim(t, PrimitiveType.Cube, new Vector3(x, 2.1f, 0f), new Vector3(3f, 0.25f, 2.2f),
                    i % 2 == 0 ? canopyA : canopyB);
                Prim(t, PrimitiveType.Cylinder, new Vector3(x - 1.3f, 1.1f, 1f), new Vector3(0.14f, 2.2f, 0.14f), wood);
                Prim(t, PrimitiveType.Cylinder, new Vector3(x + 1.3f, 1.1f, 1f), new Vector3(0.14f, 2.2f, 0.14f), wood);
            }
            Prim(t, PrimitiveType.Sphere, new Vector3(0f, 0.6f, 1.8f), new Vector3(0.5f, 0.5f, 0.5f),
                Vfx.MaterialLibrary.Emissive(new Color(1f, 0.8f, 0.25f), 1.6f));
        }

        // 10 — Forge (blacksmith)
        private static void CreateForge(Transform parent, int index)
        {
            var cb = CreateShell(parent, index, "forge", "FORGE", new Color(1f, 0.5f, 0.2f));
            var t = cb.transform;
            var stone = Vfx.MaterialLibrary.Solid(new Color(0.35f, 0.33f, 0.32f), 0.6f, 0.1f);
            var fire = Vfx.MaterialLibrary.Emissive(new Color(1f, 0.45f, 0.1f), 3f);

            Prim(t, PrimitiveType.Cube, new Vector3(0f, 1.6f, 0f), new Vector3(6f, 3.2f, 5f), stone);
            Prim(t, PrimitiveType.Cube, new Vector3(0f, 3.4f, 0f), new Vector3(7f, 0.5f, 6f),
                Vfx.MaterialLibrary.Solid(new Color(0.25f, 0.22f, 0.2f), 0.5f, 0.1f));
            Prim(t, PrimitiveType.Cylinder, new Vector3(1.8f, 5f, -1f), new Vector3(0.7f, 2.8f, 0.7f), stone);
            Prim(t, PrimitiveType.Cube, new Vector3(-1.4f, 0.9f, 2.6f), new Vector3(1.8f, 1.8f, 0.5f), stone);
            Prim(t, PrimitiveType.Sphere, new Vector3(-1.4f, 1.1f, 2.8f), new Vector3(0.7f, 0.4f, 0.3f), fire);
            var l = new GameObject("ForgeLight").AddComponent<Light>();
            l.transform.SetParent(t, false);
            l.transform.localPosition = new Vector3(-1.4f, 1.6f, 2.8f);
            l.type = LightType.Point;
            l.color = new Color(1f, 0.5f, 0.15f);
            l.range = 8f;
            l.intensity = 1.6f;
            Vfx.VFXFactory.SpawnContinuous("campfire", t.position + new Vector3(-1.4f, 1.2f, 2.9f),
                new Color(1f, 0.5f, 0.15f), 0.9f);
        }

        // ================= decorations =================
        private void BuildDecorations(Transform parent)
        {
            Random.InitState(777);

            var trunkMat = Vfx.MaterialLibrary.Solid(new Color(0.4f, 0.28f, 0.16f), 0.6f, 0.2f);
            var leafMat = Vfx.MaterialLibrary.Solid(new Color(0.25f, 0.5f, 0.22f), 0.7f, 0.15f);
            var poleMat = Vfx.MaterialLibrary.Solid(new Color(0.25f, 0.2f, 0.15f), 0.4f, 0.3f);
            var bulbMat = Vfx.MaterialLibrary.Emissive(new Color(1f, 0.85f, 0.45f), 2.4f);

            // trees scattered between paths
            for (int i = 0; i < 16; i++)
            {
                float a = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(12f, 42f);
                if (r > 20f && r < 32f) continue;   // keep the building ring clear

                var tree = new GameObject("Tree" + i);
                tree.transform.SetParent(parent, false);
                tree.transform.position = new Vector3(Mathf.Sin(a) * r, 0f, Mathf.Cos(a) * r);
                float h = Random.Range(1.6f, 2.6f);
                Prim(tree.transform, PrimitiveType.Cylinder, new Vector3(0f, h * 0.5f, 0f),
                    new Vector3(0.28f, h, 0.28f), trunkMat);
                Prim(tree.transform, PrimitiveType.Sphere, new Vector3(0f, h + 0.9f, 0f),
                    new Vector3(1.8f, 1.5f, 1.8f), leafMat);
                Prim(tree.transform, PrimitiveType.Sphere, new Vector3(0.5f, h + 0.5f, 0.2f),
                    new Vector3(1.2f, 1.0f, 1.2f), leafMat);
            }

            // lanterns around the plaza
            for (int i = 0; i < 8; i++)
            {
                float a = (i * 45f) * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Sin(a) * 11f, 0f, Mathf.Cos(a) * 11f);

                var lantern = new GameObject("Lantern" + i);
                lantern.transform.SetParent(parent, false);
                lantern.transform.position = pos;
                Prim(lantern.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.4f, 0f),
                    new Vector3(0.12f, 1.4f, 0.12f), poleMat);
                Prim(lantern.transform, PrimitiveType.Sphere, new Vector3(0f, 3f, 0f),
                    new Vector3(0.35f, 0.35f, 0.35f), bulbMat);

                var l = new GameObject("LanternLight").AddComponent<Light>();
                l.transform.SetParent(lantern.transform, false);
                l.transform.localPosition = new Vector3(0f, 3f, 0f);
                l.type = LightType.Point;
                l.color = new Color(1f, 0.8f, 0.45f);
                l.range = 9f;
                l.intensity = 1.1f;
            }

            // banners near the plaza ring
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                var banner = new GameObject("Banner" + i);
                banner.transform.SetParent(parent, false);
                banner.transform.position = new Vector3(Mathf.Sin(a) * 8.5f, 0f, Mathf.Cos(a) * 8.5f);
                banner.transform.rotation = Quaternion.LookRotation(Vector3.zero - banner.transform.position);
                var pole = Prim(banner.transform, PrimitiveType.Cylinder, new Vector3(0f, 2f, 0f),
                    new Vector3(0.1f, 2f, 0.1f), poleMat);
                Prim(banner.transform, PrimitiveType.Cube, new Vector3(0.35f, 3.2f, 0f),
                    new Vector3(0.6f, 1.6f, 0.06f),
                    Vfx.MaterialLibrary.Solid(new Color(0.75f, 0.25f, 0.2f), 0.8f, 0.2f));
            }
        }

        private void BuildWalls(Transform parent)
        {
            var wallMat = Vfx.MaterialLibrary.Solid(new Color(0.5f, 0.46f, 0.4f), 0.7f, 0.2f);
            int count = 36;
            for (int i = 0; i < count; i++)
            {
                float a = i * 360f / count * Mathf.Deg2Rad;
                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(seg.GetComponent<Collider>());
                seg.name = "Wall" + i;
                seg.transform.SetParent(parent, false);
                seg.transform.position = new Vector3(Mathf.Sin(a) * 44f, 1.2f, Mathf.Cos(a) * 44f);
                seg.transform.rotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
                seg.transform.localScale = new Vector3(7.5f, 2.4f, 0.9f);
                seg.GetComponent<MeshRenderer>().material = wallMat;
            }
        }
    }

    // ====================================================================
    // Clickable 3D building. Needs a PhysicsRaycaster on the camera.
    // ====================================================================
    public class CityBuilding : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public string ScreenId;
        public string Label;
        public Color Accent;

        private Vector3 _baseScale;
        private bool _hover;
        private TextMesh _label;
        private float _pulse;

        public void Init(string screenId, string label, Color accent)
        {
            Init(screenId, label, accent, new Vector3(8.5f, 7f, 8.5f), new Vector3(0f, 3.2f, 0f));
        }

        public void Init(string screenId, string label, Color accent, Vector3 colliderSize, Vector3 colliderCenter)
        {
            ScreenId = screenId;
            Label = label;
            Accent = accent;

            var col = gameObject.AddComponent<BoxCollider>();
            col.size = colliderSize;
            col.center = colliderCenter;

            // floating 3D label
            var labelGo = new GameObject("Label3D");
            labelGo.transform.SetParent(transform, false);
            float labelHeight = colliderSize.y >= 7f ? 7.6f : colliderCenter.y + colliderSize.y * 0.5f + 0.25f;
            labelGo.transform.localPosition = new Vector3(0f, labelHeight, 0f);
            _label = labelGo.AddComponent<TextMesh>();
            _label.text = label;
            _label.fontSize = 44;
            _label.characterSize = 0.20f;
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.color = accent;
            var mr = _label.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            _baseScale = transform.localScale;
        }

        private void LateUpdate()
        {
            // Full camera-facing billboard: include camera pitch so the TextMesh front
            // faces the player instead of remaining in a horizontal-only orientation.
            if (_label != null && Camera.main != null)
            {
                var cam = Camera.main.transform;
                var toCamera = cam.position - _label.transform.position;
                if (toCamera.sqrMagnitude > 0.01f)
                    _label.transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
            }

            // hover / ready pulse
            _pulse += Time.deltaTime * 2.4f;
            float s = _hover ? 1.06f : 1f + Mathf.Sin(_pulse) * 0.012f;
            transform.localScale = _baseScale * s;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Audio.AudioSystem.Play("Open");
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.OpenCityBuilding(ScreenId);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) { _hover = true; }
        public void OnPointerExit(PointerEventData eventData) { _hover = false; }
    }

    // ====================================================================
    // Keeps the hero party alive in peaceful areas: heroes wander the plaza,
    // walking with real locomotion animation and formation offsets.
    // ====================================================================
    public class CityWalker : MonoBehaviour
    {
        public static Vector3 WanderCenter = new Vector3(0f, 0f, 4f);
        public static bool Enabled = true;

        private readonly Vector3[] _targets = new Vector3[GameConfig.TeamSize];
        private readonly float[] _waitTimers = new float[GameConfig.TeamSize];
        private float _reshuffle;

        private void Update()
        {
            var team = Team.TeamManager.InstanceRef;
            if (team == null || !Enabled) return;
            if (GameManager.Instance != null && GameManager.Instance.State == GameManager.GameState.Battle) return;

            _reshuffle -= Time.deltaTime;
            if (_reshuffle <= 0f)
            {
                _reshuffle = Random.Range(4f, 8f);
            }

            for (int i = 0; i < team.Heroes.Count && i < GameConfig.TeamSize; i++)
            {
                var hero = team.Heroes[i];
                if (hero == null || hero.HP == null || hero.HP.IsDead) continue;

                if (_waitTimers[i] > 0f)
                {
                    _waitTimers[i] -= Time.deltaTime;
                    hero.Motor.Stop();
                    hero.Anim.SetMoving(false);

                    // face the camera-ish direction when idle
                    if (Camera.main != null && Random.value < 0.01f)
                    {
                        var look = Camera.main.transform.position - hero.transform.position;
                        look.y = 0f;
                        if (look.sqrMagnitude > 0.01f) hero.Motor.FaceTowards(hero.transform.position + look.normalized);
                    }
                    continue;
                }

                Vector3 target = _targets[i];
                Vector3 toTarget = target - hero.transform.position;
                toTarget.y = 0f;

                if (toTarget.magnitude < 0.5f)
                {
                    _waitTimers[i] = Random.Range(1.5f, 5f);
                    PickNewTarget(i, hero.transform.position);
                    continue;
                }

                hero.Motor.Move(toTarget.normalized, hero.Stats.MoveSpeed * 0.45f);
                hero.Anim.SetMoving(true);
            }
        }

        private void PickNewTarget(int index, Vector3 near)
        {
            float a = Random.Range(0f, Mathf.PI * 2f);
            float r = Random.Range(2f, 13f);
            var candidate = WanderCenter + new Vector3(Mathf.Sin(a) * r, 0f, Mathf.Cos(a) * r);

            // keep the party loosely together
            var team = Team.TeamManager.InstanceRef;
            if (team != null && team.Heroes.Count > 0 && team.Heroes[0] != null)
            {
                var anchor = team.Heroes[index].transform.position;
                if ((candidate - anchor).magnitude > 9f)
                {
                    var dir = (candidate - anchor).normalized;
                    candidate = anchor + dir * Random.Range(3f, 8f);
                }
            }
            _targets[index] = candidate;
        }

        public void NudgeAllTo(Vector3 point)
        {
            var team = Team.TeamManager.InstanceRef;
            if (team == null) return;
            for (int i = 0; i < team.Heroes.Count && i < _targets.Length; i++)
            {
                float ang = i * 60f * Mathf.Deg2Rad;
                _targets[i] = point + new Vector3(Mathf.Sin(ang) * 1.8f, 0f, Mathf.Cos(ang) * 1.8f);
                _waitTimers[i] = 0f;
            }
        }
    }
}
