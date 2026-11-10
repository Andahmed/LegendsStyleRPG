using UnityEngine;
using LegendsRPG.Art;

namespace LegendsRPG.World
{
    /// <summary>
    /// Static tabletop home inspired by a fantasy strategy diorama.
    /// The legacy CityBuilder remains intact as a runtime fallback.
    /// </summary>
    public sealed class DioramaHomeBuilder : MonoBehaviour
    {
        public static Transform HomeCameraTarget { get; private set; }
        public static bool IsActive { get; private set; }

        private Transform _root;
        private Material _wood, _darkWood, _gold, _stone, _grass, _water, _red, _blueGlow;

        public Transform Build()
        {
            var root = new GameObject("DioramaHome");
            _root = root.transform;
            try
            {
                CreateMaterials();
                BuildRoom();
                BuildTableAndMap();
                BuildNatureDressing();
                BuildPedestals();
                var focus = new GameObject("DioramaCameraFocus");
                focus.transform.SetParent(_root, false);
                focus.transform.localPosition = new Vector3(0f, 2.0f, 0f);
                HomeCameraTarget = focus.transform;
                IsActive = true;
                ApplyMood();
                return HomeCameraTarget;
            }
            catch
            {
                Destroy(root);
                HomeCameraTarget = null;
                IsActive = false;
                throw;
            }
        }

        public static void ApplyMood()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.42f, 0.34f, 0.27f);
            RenderSettings.ambientEquatorColor = new Color(0.30f, 0.24f, 0.19f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.09f, 0.07f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.58f, 0.73f, 0.86f);
            RenderSettings.fogDensity = 0.0035f;
        }

        private void CreateMaterials()
        {
            _wood = Vfx.MaterialLibrary.Solid(new Color(0.30f, 0.16f, 0.085f), 0.45f, 0.25f);
            _darkWood = Vfx.MaterialLibrary.Solid(new Color(0.13f, 0.075f, 0.045f), 0.35f, 0.2f);
            _gold = Vfx.MaterialLibrary.Solid(new Color(0.88f, 0.62f, 0.19f), 0.72f, 0.45f);
            _stone = Vfx.MaterialLibrary.Solid(new Color(0.48f, 0.47f, 0.43f), 0.75f, 0.2f);
            _grass = Vfx.MaterialLibrary.Solid(new Color(0.19f, 0.43f, 0.22f), 0.8f, 0.1f);
            _water = Vfx.MaterialLibrary.Emissive(new Color(0.08f, 0.38f, 0.72f), 0.55f);
            _red = Vfx.MaterialLibrary.Emissive(new Color(1f, 0.08f, 0.06f), 2.2f);
            _blueGlow = Vfx.MaterialLibrary.Emissive(new Color(0.18f, 0.62f, 1f), 2.1f);
        }

        private void BuildRoom()
        {
            Make(PrimitiveType.Cube, "WoodRoomBackWall", new Vector3(0f, 5.0f, -9.3f),
                new Vector3(13f, 12f, 0.55f), _darkWood);
            var trim = Vfx.MaterialLibrary.Solid(new Color(0.48f, 0.27f, 0.12f), 0.55f, 0.3f);
            Make(PrimitiveType.Cube, "WallTrim", new Vector3(0f, 10.7f, -8.95f), new Vector3(13f, 0.28f, 0.25f), trim);

            // Two simple wooden shelves and rows of colorful book spines.
            for (int shelf = 0; shelf < 2; shelf++)
            {
                float y = 5.2f + shelf * 2.45f;
                Make(PrimitiveType.Cube, "ShelfBoard" + shelf, new Vector3(0f, y, -8.72f),
                    new Vector3(9.8f, 0.22f, 0.85f), _wood);
                for (int i = 0; i < 13; i++)
                {
                    float x = -4.5f + i * 0.75f;
                    float h = 0.9f + (i % 4) * 0.18f;
                    var bookMat = Vfx.MaterialLibrary.Solid(BookColor(i), 0.45f, 0.2f);
                    Make(PrimitiveType.Cube, "Book_" + shelf + "_" + i,
                        new Vector3(x, y + 0.12f + h * 0.5f, -8.72f),
                        new Vector3(0.48f, h, 0.55f), bookMat);
                }
            }

            Make(PrimitiveType.Cube, "LeftRoomPost", new Vector3(-6.2f, 4.6f, -8.9f),
                new Vector3(0.35f, 10.5f, 0.45f), _wood);
            Make(PrimitiveType.Cube, "RightRoomPost", new Vector3(6.2f, 4.6f, -8.9f),
                new Vector3(0.35f, 10.5f, 0.45f), _wood);

            var key = new GameObject("WarmRoomKeyLight");
            key.transform.SetParent(_root, false);
            key.transform.position = new Vector3(-3f, 9f, 3f);
            var kl = key.AddComponent<Light>();
            kl.type = LightType.Point;
            kl.color = new Color(1f, 0.68f, 0.38f);
            kl.intensity = 2.1f;
            kl.range = 24f;
            kl.shadows = LightShadows.Soft;

            var fill = new GameObject("SoftRoomFill");
            fill.transform.SetParent(_root, false);
            fill.transform.position = new Vector3(3f, 7f, -2f);
            var fl = fill.AddComponent<Light>();
            fl.type = LightType.Point;
            fl.color = new Color(1f, 0.83f, 0.62f);
            fl.intensity = 1.0f;
            fl.range = 18f;
        }

        private void BuildTableAndMap()
        {
            // Table top, apron and four legs.
            Make(PrimitiveType.Cube, "TableTop", new Vector3(0f, 0.95f, 0f),
                new Vector3(10.6f, 0.65f, 17.4f), _wood);
            Make(PrimitiveType.Cube, "TableFrontApron", new Vector3(0f, 0.38f, 8.35f),
                new Vector3(10.0f, 0.75f, 0.35f), _darkWood);
            Make(PrimitiveType.Cube, "TableBackApron", new Vector3(0f, 0.38f, -8.35f),
                new Vector3(11.2f, 0.75f, 0.35f), _darkWood);
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
                Make(PrimitiveType.Cube, "TableLeg_" + x + "_" + z,
                    new Vector3(x * 4.6f, -1.55f, z * 7.5f), new Vector3(0.75f, 4.7f, 0.75f), _darkWood);

            Make(PrimitiveType.Cube, "MapBorder", new Vector3(0f, 1.32f, 0f),
                new Vector3(9.8f, 0.22f, 16.2f), _gold);
            Make(PrimitiveType.Cube, "MapGround", new Vector3(0f, 1.47f, 0f),
                new Vector3(9.4f, 0.18f, 15.8f), _grass);

            // Winding blue river, a few lakes and raised green hills.
            var riverA = Make(PrimitiveType.Cube, "WorldRiverA", new Vector3(0f, 1.59f, 0f),
                new Vector3(1.35f, 0.035f, 15.2f), _water);
            riverA.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);
            var riverB = Make(PrimitiveType.Cube, "WorldRiverB", new Vector3(1.5f, 1.60f, -2.0f),
                new Vector3(1.35f, 0.04f, 5.2f), _water);
            riverB.transform.localRotation = Quaternion.Euler(0f, -38f, 0f);
            Make(PrimitiveType.Cylinder, "WestLake", new Vector3(-3.6f, 1.60f, 3.6f),
                new Vector3(1.6f, 0.035f, 1.2f), _water);
            Make(PrimitiveType.Cylinder, "EastLake", new Vector3(3.6f, 1.60f, -4.2f),
                new Vector3(1.5f, 0.035f, 1.15f), _water);

            for (int i = 0; i < 8; i++)
            {
                float x = (i % 2 == 0) ? -4.3f : 4.3f;
                float z = -7.0f + (i / 2) * 4.65f;
                Make(PrimitiveType.Sphere, "MapHill" + i, new Vector3(x, 1.78f, z),
                    new Vector3(0.8f, 0.32f + (i % 2) * 0.1f, 0.8f), _grass);
            }
            var mountain = Vfx.MaterialLibrary.Solid(new Color(0.42f, 0.43f, 0.39f), 0.75f, 0.15f);
            for (int i = 0; i < 2; i++)
                Make(PrimitiveType.Capsule, "MapMountain" + i,
                    new Vector3(i == 0 ? -4.25f : 4.25f, 1.95f, 0f),
                    new Vector3(0.5f, 0.85f, 0.5f), mountain);
        }

        private void BuildNatureDressing()
        {
            // Six lightweight nature props around the map edge; procedural map remains the fallback.
            string[] props = { "CommonTree_1", "PineTree_2", "BirchTree_1", "Bush_1", "Rock_2", "TreeStump" };
            Vector3[] positions =
            {
                new Vector3(-4.15f, 1.58f, -6.85f),
                new Vector3(4.15f, 1.58f, -6.85f),
                new Vector3(-4.15f, 1.58f, 6.85f),
                new Vector3(4.15f, 1.58f, 6.85f),
                new Vector3(-4.15f, 1.58f, 2.9f),
                new Vector3(4.15f, 1.58f, -2.9f)
            };
            for (int i = 0; i < props.Length; i++)
            {
                float height = i < 3 ? 2.25f : 0.85f;
                var model = QuaterniusAssetLibrary.InstantiateNature(props[i], _root,
                    _root.TransformPoint(positions[i]), height, i * 47f);
                if (model == null) continue;
                model.name = "DioramaNature_" + props[i] + "_" + i;
            }
        }

        private bool BuildQuaterniusLandmark(Transform parent, int kind)
        {
            // Curated dungeon props replace primitive icons only when their imported FBX is available.
            string[] models =
            {
                "Statue_Horse", "Pedestal2", "Banner", "Arch", "Skull",
                "Chest_Gold", "Torch", "Chest", "Pedestal", "Table_Small", "Woodfire"
            };
            string modelName = models[Mathf.Clamp(kind, 0, models.Length - 1)];
            var model = QuaterniusAssetLibrary.InstantiateDungeonProp(modelName, parent,
                parent.TransformPoint(new Vector3(0f, 0.82f, 0f)),
                kind == 3 ? 2.25f : 1.55f, parent.eulerAngles.y);
            if (model == null) return false;
            model.name = "DioramaProp_" + modelName;
            return true;
        }

        private void BuildPedestals()
        {
            // 11 clickable systems: the nine main landmarks plus Shop and Forge.
            AddLandmark("guild", "GUILD", new Vector3(-3.0f, 1.62f, -5.3f), 0);
            AddLandmark("summon", "SUMMON", new Vector3(0f, 1.62f, -5.3f), 1);
            AddLandmark("arena", "ARENA", new Vector3(3.0f, 1.62f, -5.3f), 2);
            AddLandmark("continent", "CONTINENT", new Vector3(-3.0f, 1.62f, -1.8f), 3);
            AddLandmark("dungeon", "DUNGEON", new Vector3(0f, 1.62f, -1.8f), 4);
            AddLandmark("beast", "BEAST", new Vector3(3.0f, 1.62f, -1.8f), 5);
            AddLandmark("mine", "MINE", new Vector3(-3.0f, 1.62f, 1.8f), 6);
            AddLandmark("afk", "AFK", new Vector3(0f, 1.62f, 1.8f), 7);
            AddLandmark("rank", "RANKING", new Vector3(3.0f, 1.62f, 1.8f), 8);
            AddLandmark("shop", "SHOP", new Vector3(-1.5f, 1.62f, 5.3f), 9);
            AddLandmark("forge", "FORGE", new Vector3(1.5f, 1.62f, 5.3f), 10);
        }

        private void AddLandmark(string id, string label, Vector3 position, int kind)
        {
            var root = new GameObject("Diorama_" + id);
            root.transform.SetParent(_root, false);
            root.transform.localPosition = position;
            var accent = Accent(kind);
            var click = root.AddComponent<CityBuilding>();
            click.Init(id, label, accent, new Vector3(1.9f, 2.8f, 1.9f), new Vector3(0f, 1.0f, 0f));

            MakeChild(root.transform, PrimitiveType.Cube, "PedestalBase", new Vector3(0f, 0.12f, 0f),
                new Vector3(1.55f, 0.24f, 1.55f), _darkWood);
            MakeChild(root.transform, PrimitiveType.Cube, "PedestalGoldTrim", new Vector3(0f, 0.27f, 0f),
                new Vector3(1.44f, 0.08f, 1.44f), _gold);
            MakeChild(root.transform, PrimitiveType.Cube, "PedestalTop", new Vector3(0f, 0.38f, 0f),
                new Vector3(1.32f, 0.16f, 1.32f), _stone);

            if (!BuildQuaterniusLandmark(root.transform, kind))
                BuildIcon(root.transform, kind, accent);

            var dot = MakeChild(root.transform, PrimitiveType.Sphere, "NotificationDot",
                new Vector3(0.55f, 2.18f, -0.42f), Vector3.one * 0.23f, _red);
            dot.transform.localScale = Vector3.one * 0.23f;
        }

        private void BuildIcon(Transform p, int kind, Color accent)
        {
            var glow = Vfx.MaterialLibrary.Emissive(accent, 1.15f);
            switch (kind)
            {
                case 0: // Guild castle
                    MakeChild(p, PrimitiveType.Cube, "CastleKeep", new Vector3(0f, 1.05f, 0f), new Vector3(0.9f, 1.15f, 0.8f), _stone);
                    for (int x = -1; x <= 1; x += 2)
                    {
                        MakeChild(p, PrimitiveType.Cylinder, "CastleTower", new Vector3(x * 0.58f, 1.25f, 0f), new Vector3(0.32f, 1.55f, 0.32f), _gold);
                        MakeChild(p, PrimitiveType.Cylinder, "TowerCap", new Vector3(x * 0.58f, 2.12f, 0f), new Vector3(0.48f, 0.24f, 0.48f), accent);
                    }
                    break;
                case 1: // Summoning circle
                    MakeChild(p, PrimitiveType.Cylinder, "SummonCircle", new Vector3(0f, 0.78f, 0f), new Vector3(1.15f, 0.06f, 1.15f), glow);
                    MakeChild(p, PrimitiveType.Sphere, "SummonCrystal", new Vector3(0f, 1.35f, 0f), new Vector3(0.48f, 0.75f, 0.48f), _blueGlow);
                    break;
                case 2: // Arena flag
                    MakeChild(p, PrimitiveType.Cylinder, "ArenaFlagPole", new Vector3(0f, 1.28f, 0f), new Vector3(0.09f, 1.35f, 0.09f), _gold);
                    MakeChild(p, PrimitiveType.Cube, "ArenaFlag", new Vector3(0.43f, 1.75f, 0f), new Vector3(0.85f, 0.55f, 0.07f), accent);
                    break;
                case 3: // Blue continent portal
                    MakeChild(p, PrimitiveType.Cube, "GateLeft", new Vector3(-0.52f, 1.18f, 0f), new Vector3(0.22f, 1.65f, 0.25f), _stone);
                    MakeChild(p, PrimitiveType.Cube, "GateRight", new Vector3(0.52f, 1.18f, 0f), new Vector3(0.22f, 1.65f, 0.25f), _stone);
                    MakeChild(p, PrimitiveType.Cube, "GateTop", new Vector3(0f, 2.0f, 0f), new Vector3(1.25f, 0.25f, 0.28f), _gold);
                    MakeChild(p, PrimitiveType.Cube, "BluePortal", new Vector3(0f, 1.15f, 0.04f), new Vector3(0.78f, 1.35f, 0.06f), _blueGlow);
                    break;
                case 4: // Dragon-head approximation
                    MakeChild(p, PrimitiveType.Sphere, "DragonSkull", new Vector3(0f, 1.18f, 0f), new Vector3(1.0f, 0.82f, 0.85f), _stone);
                    MakeChild(p, PrimitiveType.Cube, "DragonSnout", new Vector3(0f, 0.98f, 0.48f), new Vector3(0.72f, 0.38f, 0.65f), accent);
                    for (int x = -1; x <= 1; x += 2)
                    {
                        var horn = MakeChild(p, PrimitiveType.Cylinder, "DragonHorn", new Vector3(x * 0.38f, 1.78f, -0.12f),
                            new Vector3(0.13f, 0.55f, 0.13f), _gold);
                        horn.transform.localRotation = Quaternion.Euler(0f, 0f, x * -28f);
                    }
                    break;
                case 5: // Beast book
                    MakeChild(p, PrimitiveType.Cube, "BeastBook", new Vector3(0f, 1.0f, 0f), new Vector3(1.1f, 0.18f, 0.85f), accent);
                    MakeChild(p, PrimitiveType.Cube, "BookPages", new Vector3(0f, 1.12f, 0f), new Vector3(0.94f, 0.08f, 0.76f), _gold);
                    MakeChild(p, PrimitiveType.Cube, "BookSpine", new Vector3(0f, 1.02f, -0.39f), new Vector3(0.16f, 0.25f, 0.12f), _gold);
                    break;
                case 6: // Mining helmet
                    MakeChild(p, PrimitiveType.Sphere, "MineHelmet", new Vector3(0f, 1.2f, 0f), new Vector3(1.05f, 0.72f, 0.9f), _gold);
                    MakeChild(p, PrimitiveType.Cube, "HelmetBrim", new Vector3(0f, 0.98f, 0.12f), new Vector3(1.2f, 0.13f, 0.8f), accent);
                    MakeChild(p, PrimitiveType.Sphere, "HelmetLamp", new Vector3(0f, 1.48f, 0.48f), Vector3.one * 0.28f, _blueGlow);
                    break;
                case 7: // AFK chest
                    MakeChild(p, PrimitiveType.Cube, "ChestBody", new Vector3(0f, 1.0f, 0f), new Vector3(1.1f, 0.72f, 0.8f), _wood);
                    MakeChild(p, PrimitiveType.Cube, "ChestLid", new Vector3(0f, 1.43f, 0f), new Vector3(1.14f, 0.2f, 0.84f), _gold);
                    MakeChild(p, PrimitiveType.Cube, "ChestLock", new Vector3(0f, 1.13f, 0.43f), new Vector3(0.2f, 0.28f, 0.08f), accent);
                    break;
                case 8: // Ranking trophy
                    MakeChild(p, PrimitiveType.Cylinder, "TrophyCup", new Vector3(0f, 1.38f, 0f), new Vector3(0.85f, 0.5f, 0.85f), _gold);
                    MakeChild(p, PrimitiveType.Cylinder, "TrophyStem", new Vector3(0f, 0.88f, 0f), new Vector3(0.2f, 0.45f, 0.2f), accent);
                    MakeChild(p, PrimitiveType.Cube, "TrophyBase", new Vector3(0f, 0.62f, 0f), new Vector3(0.85f, 0.16f, 0.6f), _gold);
                    for (int x = -1; x <= 1; x += 2)
                        MakeChild(p, PrimitiveType.Cylinder, "TrophyHandle", new Vector3(x * 0.58f, 1.42f, 0f), new Vector3(0.12f, 0.38f, 0.12f), _gold);
                    break;
                case 9: // Shop stall
                    MakeChild(p, PrimitiveType.Cube, "ShopCounter", new Vector3(0f, 0.95f, 0f), new Vector3(1.1f, 0.55f, 0.72f), _wood);
                    MakeChild(p, PrimitiveType.Cube, "ShopAwning", new Vector3(0f, 1.62f, 0f), new Vector3(1.35f, 0.18f, 0.9f), accent);
                    break;
                case 10: // Forge/anvil
                    MakeChild(p, PrimitiveType.Cube, "AnvilBase", new Vector3(0f, 0.88f, 0f), new Vector3(0.6f, 0.5f, 0.6f), _stone);
                    MakeChild(p, PrimitiveType.Cube, "AnvilTop", new Vector3(0f, 1.22f, 0f), new Vector3(1.05f, 0.22f, 0.45f), _gold);
                    MakeChild(p, PrimitiveType.Sphere, "ForgeEmber", new Vector3(0f, 1.55f, 0f), Vector3.one * 0.28f, glow);
                    break;
            }
        }

        private GameObject Make(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(_root, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = mat;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            return go;
        }

        private static GameObject MakeChild(Transform parent, PrimitiveType type, string name,
            Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = mat;
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            return go;
        }

        // Overload: lets callers pass a Color (e.g. the landmark accent) instead of a Material.
        private static GameObject MakeChild(Transform parent, PrimitiveType type, string name,
            Vector3 pos, Vector3 scale, Color color)
        {
            return MakeChild(parent, type, name, pos, scale,
                Vfx.MaterialLibrary.Solid(color, 0.5f, 0.2f));
        }

        private static Color Accent(int i)
        {
            switch (i)
            {
                case 0: return new Color(0.95f, 0.66f, 0.25f);
                case 1: return new Color(0.35f, 0.72f, 1f);
                case 2: return new Color(0.95f, 0.35f, 0.28f);
                case 3: return new Color(0.22f, 0.68f, 1f);
                case 4: return new Color(0.65f, 0.4f, 0.95f);
                case 5: return new Color(0.3f, 0.8f, 0.55f);
                case 6: return new Color(0.9f, 0.68f, 0.32f);
                case 7: return new Color(1f, 0.78f, 0.25f);
                case 8: return new Color(1f, 0.82f, 0.32f);
                case 9: return new Color(0.4f, 0.72f, 0.9f);
                default: return new Color(1f, 0.42f, 0.16f);
            }
        }

        private static Color BookColor(int i)
        {
            Color[] colors = {
                new Color(0.38f,0.12f,0.10f), new Color(0.12f,0.22f,0.42f),
                new Color(0.25f,0.34f,0.18f), new Color(0.48f,0.30f,0.12f)
            };
            return colors[i % colors.Length];
        }
    }
}
