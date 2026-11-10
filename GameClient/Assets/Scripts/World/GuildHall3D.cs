using UnityEngine;
using UnityEngine.EventSystems;
using LegendsRPG.Core;
using LegendsRPG.Team;

namespace LegendsRPG.World
{
    /// <summary>
    /// The 3D guild hall interior: a grand statue, braziers and six clickable
    /// stations — Hunt Altar, Caravan Dock, War Banner, Tech Table, Gift Chest
    /// and Wish Wall. Entering moves the hero party inside; every station
    /// opens the real guild screen on the matching tab.
    /// </summary>
    public class GuildHall3D : MonoBehaviour
    {
        public const float HallOffsetZ = GameConfig.GuildHallOffsetZ;   // -260
        public static Vector3 HallCenter => new Vector3(0f, 0f, HallOffsetZ);
        public static bool Inside { get; private set; }

        private static bool _built;

        public static void EnsureBuilt()
        {
            if (_built) return;
            _built = true;

            var hall = new GameObject("GuildHall");

            BuildEnvironment();
            BuildFloor(hall.transform);
            BuildStatue(hall.transform);
            BuildStations(hall.transform);
            BuildAmbience(hall.transform);
        }

        public static void Enter()
        {
            EnsureBuilt();
            Inside = true;
            CityWalker.WanderCenter = HallCenter + new Vector3(0f, 0f, 2f);

            var team = TeamManager.InstanceRef;
            if (team != null)
            {
                team.BasePosition = HallCenter;
                team.SetBrainsEnabled(false);
                for (int i = 0; i < team.Heroes.Count; i++)
                {
                    if (team.Heroes[i] != null) team.Heroes[i].Motor.Teleport(team.FormationPosition(i));
                }
                var cam = Camera.main != null ? Camera.main.GetComponent<Cam.IsometricCameraController>() : null;
                if (cam != null)
                {
                    cam.SetTarget(team.FocusTransform);   // v1.1: camera follows the team center
                    cam.SnapToTarget();
                }
            }
            if (UI.UIManager.Instance != null) UI.UIManager.Instance.Notify("Entered the Guild Hall — tap the statue & stations");
            Audio.AudioSystem.Play("Open");
        }

        public static void Exit()
        {
            Inside = false;
            CityWalker.WanderCenter = new Vector3(0f, 0f, 4f);

            var team = TeamManager.InstanceRef;
            if (team != null)
            {
                team.BasePosition = CityBuilder.CitySpawnPoint;
                for (int i = 0; i < team.Heroes.Count; i++)
                {
                    if (team.Heroes[i] != null) team.Heroes[i].Motor.Teleport(team.FormationPosition(i));
                }
                var cam = Camera.main != null ? Camera.main.GetComponent<Cam.IsometricCameraController>() : null;
                if (cam != null)
                {
                    cam.SetTarget(team.FocusTransform);   // v1.1: camera follows the team center
                    cam.SnapToTarget();
                }
            }
            if (UI.UIManager.Instance != null) UI.UIManager.Instance.Notify("Back in the city");
            Audio.AudioSystem.Play("Open");
        }

        // ================= construction =================
        private static void BuildEnvironment()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.30f, 0.26f, 0.38f);
            RenderSettings.ambientEquatorColor = new Color(0.22f, 0.18f, 0.26f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.10f, 0.12f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.fogColor = new Color(0.10f, 0.08f, 0.14f);
        }

        private static void BuildFloor(Transform parent)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "HallFloor";
            floor.transform.SetParent(parent, false);
            floor.transform.position = new Vector3(0f, 0f, HallOffsetZ);
            floor.transform.localScale = new Vector3(4.5f, 1f, 3.4f);
            floor.GetComponent<MeshRenderer>().material = Vfx.MaterialLibrary.Solid(
                new Color(0.28f, 0.24f, 0.30f), 0.7f, 0.3f);

            // carpet path
            var carpet = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Destroy(carpet.GetComponent<Collider>());
            carpet.name = "Carpet";
            carpet.transform.SetParent(parent, false);
            carpet.transform.position = new Vector3(0f, 0.01f, HallOffsetZ + 4f);
            carpet.transform.localScale = new Vector3(0.8f, 1f, 2f);
            carpet.GetComponent<MeshRenderer>().material = Vfx.MaterialLibrary.Solid(
                new Color(0.55f, 0.15f, 0.15f), 0.8f, 0.2f);
        }

        private static void BuildStatue(Transform parent)
        {
            var statue = new GameObject("GuildStatue");
            statue.transform.SetParent(parent, false);
            statue.transform.position = HallCenter + new Vector3(0f, 0f, -6f);

            var stone = Vfx.MaterialLibrary.Solid(new Color(0.62f, 0.60f, 0.66f), 0.6f, 0.35f);
            var gold = Vfx.MaterialLibrary.Emissive(new Color(1f, 0.82f, 0.3f), 1.8f);

            Prim(statue.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f), new Vector3(3.2f, 0.35f, 3.2f), stone);
            Prim(statue.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.2f, 0f), new Vector3(2f, 0.9f, 2f), stone);
            Prim(statue.transform, PrimitiveType.Capsule, new Vector3(0f, 3.1f, 0f), new Vector3(1.3f, 1.6f, 1.3f), gold);
            Prim(statue.transform, PrimitiveType.Sphere, new Vector3(0f, 4.4f, 0f), new Vector3(0.55f, 0.55f, 0.55f), gold);
            // sword held high
            Prim(statue.transform, PrimitiveType.Cube, new Vector3(0f, 5.6f, 0f), new Vector3(0.18f, 2.2f, 0.18f),
                Vfx.MaterialLibrary.Emissive(new Color(0.9f, 0.95f, 1f), 1.4f));

            var l = new GameObject("StatueLight").AddComponent<Light>();
            l.transform.SetParent(statue.transform, false);
            l.transform.localPosition = new Vector3(0f, 5.2f, 1f);
            l.type = LightType.Point;
            l.color = new Color(1f, 0.85f, 0.5f);
            l.range = 14f;
            l.intensity = 1.8f;

            var cb = statue.AddComponent<CityBuilding>();
            cb.Init("guild", "GUILD HALL", new Color(1f, 0.82f, 0.35f));
        }

        private static void BuildStations(Transform parent)
        {
            // six stations arranged in an arc in front of the statue
            CreateStation(parent, "hunt", "GUILD HUNT", new Color(0.95f, 0.4f, 0.3f), HallCenter + new Vector3(-5.5f, 0f, 1.5f));
            CreateStation(parent, "caravan", "CARAVAN", new Color(0.9f, 0.75f, 0.35f), HallCenter + new Vector3(-2.8f, 0f, 4.5f));
            CreateStation(parent, "war", "GUILD WAR", new Color(0.85f, 0.25f, 0.4f), HallCenter + new Vector3(0f, 0f, 6f));
            CreateStation(parent, "tech", "GUILD TECH", new Color(0.4f, 0.7f, 0.95f), HallCenter + new Vector3(2.8f, 0f, 4.5f));
            CreateStation(parent, "gifts", "GIFT CHEST", new Color(0.5f, 0.9f, 0.55f), HallCenter + new Vector3(5.5f, 0f, 1.5f));
            CreateStation(parent, "wish", "WISH WALL", new Color(0.75f, 0.5f, 0.95f), HallCenter + new Vector3(7.5f, 0f, -2.5f));
        }

        private static void CreateStation(Transform parent, string tab, string label, Color accent, Vector3 pos)
        {
            var go = new GameObject("Station_" + tab);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.LookRotation(HallCenter + Vector3.back * 2f - pos);

            // Prefer the real Quaternius dungeon prop. Keep the original primitive
            // pedestal/orb as a safe fallback when a model is unavailable at runtime.
            string model = tab == "hunt" ? "Pedestal"
                : tab == "caravan" ? "Barrel"
                : tab == "war" ? "Banner"
                : tab == "tech" ? "Table_Big"
                : tab == "gifts" ? "Chest_Gold"
                : "Banner_wall";
            float modelHeight = tab == "war" || tab == "wish" ? 3.0f : 2.2f;
            var imported = QuaterniusAssetLibrary.InstantiateDungeonProp(
                model, go.transform, pos, modelHeight, go.transform.eulerAngles.y);

            if (imported == null)
            {
                var stone = Vfx.MaterialLibrary.Solid(new Color(0.4f, 0.36f, 0.42f), 0.5f, 0.2f);
                Prim(go.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f),
                    new Vector3(1.6f, 0.55f, 1.6f), stone);
                Prim(go.transform, PrimitiveType.Sphere, new Vector3(0f, 1.5f, 0f),
                    new Vector3(0.45f, 0.45f, 0.45f), Vfx.MaterialLibrary.Emissive(accent, 2.2f));
            }

            var l = new GameObject("Light").AddComponent<Light>();
            l.transform.SetParent(go.transform, false);
            l.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            l.type = LightType.Point;
            l.color = accent;
            l.range = 6f;
            l.intensity = 1.1f;

            var cb = go.AddComponent<GuildStation>();
            cb.Init(tab, label, accent);
        }

        private static void BuildAmbience(Transform parent)
        {
            var pillarMat = Vfx.MaterialLibrary.Solid(new Color(0.35f, 0.32f, 0.4f), 0.4f, 0.3f);
            var flameMat = Vfx.MaterialLibrary.Emissive(new Color(1f, 0.6f, 0.2f), 2.8f);

            for (int i = 0; i < 6; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * 8.5f;
                float z = HallOffsetZ + (i / 2) * 7f - 3f;
                Vector3 pillarPos = new Vector3(x, 0f, z);
                var column = QuaterniusAssetLibrary.InstantiateDungeonProp(
                    "Column", parent, pillarPos, 7.0f, 0f);
                if (column == null)
                {
                    var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    Destroy(pillar.GetComponent<Collider>());
                    pillar.name = "Pillar" + i;
                    pillar.transform.SetParent(parent, false);
                    pillar.transform.position = new Vector3(x, 3.5f, z);
                    pillar.transform.localScale = new Vector3(1f, 3.5f, 1f);
                    pillar.GetComponent<MeshRenderer>().material = pillarMat;
                }

                Vector3 torchPos = new Vector3(x, 0f, z);
                var torch = QuaterniusAssetLibrary.InstantiateDungeonProp(
                    "Torch", parent, torchPos + Vector3.up * 5.8f, 2.0f, 0f);
                if (torch == null)
                {
                    var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(flame.GetComponent<Collider>());
                    flame.name = "Brazier" + i;
                    flame.transform.SetParent(parent, false);
                    flame.transform.position = new Vector3(x, 7.4f, z);
                    flame.transform.localScale = Vector3.one * 0.4f;
                    flame.GetComponent<MeshRenderer>().material = flameMat;
                }
                var lightGo = new GameObject("GuildTorchLight" + i);
                lightGo.transform.SetParent(parent, false);
                lightGo.transform.position = new Vector3(x, 7.0f, z);
                var torchLight = lightGo.AddComponent<Light>();
                torchLight.type = LightType.Point;
                torchLight.color = new Color(1f, 0.58f, 0.25f);
                torchLight.range = 9f;
                torchLight.intensity = 1.4f;
            }

            // Quaternius wall banners add real silhouettes to the hall. Use the
            // former colored cubes only if the imported banner cannot be loaded.
            var bannerMat = Vfx.MaterialLibrary.Solid(new Color(0.6f, 0.2f, 0.25f), 0.8f, 0.2f);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * 13f;
                float z = HallOffsetZ + (i / 2) * 10f - 2f;
                var banner = QuaterniusAssetLibrary.InstantiateDungeonProp(
                    "Banner_wall", parent, new Vector3(x, 2.2f, z), 3.2f, x < 0f ? 90f : -90f);
                if (banner == null)
                {
                    var fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(fallback.GetComponent<Collider>());
                    fallback.name = "Banner" + i;
                    fallback.transform.SetParent(parent, false);
                    fallback.transform.position = new Vector3(x, 4f, z);
                    fallback.transform.localScale = new Vector3(0.15f, 3.2f, 1.6f);
                    fallback.GetComponent<MeshRenderer>().material = bannerMat;
                }
            }

            Vfx.VFXFactory.SpawnContinuous("campfire", HallCenter + new Vector3(0f, 0.4f, -6f),
                new Color(1f, 0.7f, 0.3f), 1.2f);
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
    }

    /// <summary>A clickable guild station inside the 3D hall.</summary>
    public class GuildStation : MonoBehaviour, IPointerClickHandler
    {
        public string Tab;
        public string Label;
        public Color Accent;

        public void Init(string tab, string label, Color accent)
        {
            Tab = tab;
            Label = label;
            Accent = accent;

            var col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(2.6f, 3.4f, 2.6f);
            col.center = new Vector3(0f, 1.5f, 0f);

            var labelGo = new GameObject("Label3D");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 2.9f, 0f);
            var tm = labelGo.AddComponent<TextMesh>();
            tm.text = label;
            tm.fontSize = 40;
            tm.characterSize = 0.16f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = accent;
            var mr = tm.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        private void Update()
        {
            var cam = Camera.main;
            if (cam == null) return;
            var t = transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                if (child.name == "Label3D")
                {
                    var dir = cam.transform.position - child.position;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.01f) child.rotation = Quaternion.LookRotation(dir.normalized);
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Audio.AudioSystem.Play("Open");
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.OpenGuildTab(Tab);
            }
        }
    }
}
