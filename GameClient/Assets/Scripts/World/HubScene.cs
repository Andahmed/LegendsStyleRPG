using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Team;
using LegendsRPG.UI;
using LegendsRPG.Vfx;

namespace LegendsRPG.World
{
    /// <summary>
    /// A tappable 3D building on the hub continent. Owns a glow disc, a 3D
    /// name label and an optional red notification dot. Idle motion runs on
    /// unscaled time so it animates while the battle is paused (Time.timeScale = 0).
    /// </summary>
    public class HubBuilding : MonoBehaviour
    {
        public string Id;
        public string DisplayName;
        public bool HasDot;

        private Transform _spin;      // part that slowly rotates (portals, crystals)
        private Transform _bob;       // part that bobs up/down
        private float _baseY;
        private float _pressT = -1f;
        private Vector3 _baseScale;

        public void Setup(string id, string displayName, Transform spin, Transform bob, bool dot)
        {
            Id = id;
            DisplayName = displayName;
            _spin = spin;
            _bob = bob != null ? bob : transform;
            _baseY = _bob.localPosition.y;
            _baseScale = transform.localScale;
            HasDot = dot;

            // one generous collider for the whole building
            var col = gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 3.2f, 0f);
            col.size = new Vector3(7f, 8f, 7f);

            if (dot) AddDot();
            AddLabel(displayName);
        }

        private void AddDot()
        {
            var dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name = "Dot";
            Destroy(dot.GetComponent<Collider>());
            dot.transform.SetParent(transform, false);
            dot.transform.localPosition = new Vector3(2.4f, 6.6f, 0f);
            dot.transform.localScale = Vector3.one * 0.7f;
            dot.GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Emissive(new Color(1f, 0.22f, 0.18f), 1.8f);
        }

        private void AddLabel(string text)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 7.8f, 0f);
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 52;
            tm.characterSize = 0.11f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.98f, 0.88f, 0.62f);
        }

        public void Press()
        {
            _pressT = 0f;
            if (HubScreen.Instance != null) HubScreen.Instance.NotifyBuilding(Id);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float t = Time.unscaledTime;

            if (_spin != null) _spin.Rotate(0f, 22f * dt, 0f, Space.Self);
            _bob.localPosition = new Vector3(_bob.localPosition.x,
                _baseY + Mathf.Sin(t * 1.4f + _baseY) * 0.18f, _bob.localPosition.z);

            if (_pressT >= 0f)
            {
                _pressT += dt;
                float k = Mathf.Clamp01(_pressT / 0.28f);
                float punch = 1f + Mathf.Sin(k * Mathf.PI) * 0.12f;
                transform.localScale = _baseScale * punch;
                if (k >= 1f)
                {
                    _pressT = -1f;
                    transform.localScale = _baseScale;
                }
            }
        }
    }

    /// <summary>
    /// Builds the main-hub diorama entirely from primitives at runtime
    /// (code-first, same philosophy as the rest of the prototype).
    /// Continent disc + water + ten tappable buildings matching the
    /// reference layout: Summon, AFK Chest, Forge, Beast, Dwarven Mine,
    /// Dungeon, Tower, Guild, Arena, Continent portal.
    /// Also plants a few idle hero "greeters" using CharacterBodyFactory.
    /// </summary>
    public class HubScene : MonoBehaviour
    {
        private readonly List<HubBuilding> _buildings = new List<HubBuilding>();
        private readonly List<Transform> _portals = new List<Transform>();
        private Transform _root;
        private float _waterBase;

        /// <summary>Builds the hub 60 units behind the battle arena and returns the continent centre (camera target).</summary>
        public Transform Build(TeamManager team = null)
        {
            _root = new GameObject("HubContinent").transform;
            _root.SetParent(transform, false);
            _root.position = new Vector3(0f, 0f, -75f);

            BuildTerrain();
            BuildSummon(new Vector3(0f, 0f, 26f));
            BuildAfkChest(new Vector3(10f, 0f, 22f));
            BuildForge(new Vector3(26f, 0f, 12f));
            BuildBeast(new Vector3(-26f, 0f, 12f));
            BuildMine(new Vector3(-36f, 0f, -2f));
            BuildDungeon(new Vector3(-26f, 0f, -16f));
            BuildGuild(new Vector3(0f, 0f, -26f));
            BuildArena(new Vector3(28f, 0f, -8f));
            BuildTower(new Vector3(14f, 0f, -22f));
            BuildContinent(new Vector3(-14f, 0f, -22f));

            if (team != null) PlantGreeters(team);

            // warm key light + central fill so the diorama reads gold/brown like the reference
            var lightGo = new GameObject("HubSun");
            lightGo.transform.SetParent(_root, false);
            lightGo.transform.localRotation = Quaternion.Euler(52f, -28f, 0f);
            var sun = lightGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.86f, 0.68f);
            sun.intensity = 1.05f;

            var fillGo = new GameObject("HubFill");
            fillGo.transform.SetParent(_root, false);
            fillGo.transform.localPosition = new Vector3(0f, 18f, 0f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.color = new Color(1f, 0.75f, 0.45f);
            fill.intensity = 0.65f;
            fill.range = 90f;

            return _root;
        }

        // ------------------------------------------------------------ terrain
        private void BuildTerrain()
        {
            // water
            var water = Prim(PrimitiveType.Cylinder, "Water", _root,
                new Vector3(0f, -0.6f, 0f), new Vector3(150f, 0.5f, 150f),
                MaterialLibrary.Solid(new Color(0.13f, 0.32f, 0.46f), 0.65f, 0.05f));
            _waterBase = water.localScale.x;
            _portals.Add(water); // reuse pulse list, very subtle for water

            // continent rim (dark earth) + grass top
            Prim(PrimitiveType.Cylinder, "Rim", _root,
                new Vector3(0f, -0.1f, 0f), new Vector3(92f, 1.6f, 92f),
                MaterialLibrary.Solid(new Color(0.24f, 0.16f, 0.10f), 0.3f, 0f));
            Prim(PrimitiveType.Cylinder, "Grass", _root,
                new Vector3(0f, 0.55f, 0f), new Vector3(88f, 0.5f, 88f),
                MaterialLibrary.Solid(new Color(0.34f, 0.52f, 0.26f), 0.25f, 0f));

            // sand plaza between summon and guild
            var path = Prim(PrimitiveType.Cube, "Path", _root,
                new Vector3(0f, 0.82f, 0f), new Vector3(7f, 0.12f, 44f),
                MaterialLibrary.Solid(new Color(0.78f, 0.66f, 0.42f), 0.2f, 0f));
            path.localRotation = Quaternion.identity;
        }

        // ------------------------------------------------------------ buildings
        private void BuildSummon(Vector3 pos)
        {
            var b = NewBuilding("summon", "Summon", pos, true);
            Prim(PrimitiveType.Cylinder, "Pedestal", b, new Vector3(0f, 0.9f, 0f),
                new Vector3(7.5f, 0.9f, 7.5f), MaterialLibrary.Solid(new Color(0.45f, 0.16f, 0.12f), 0.3f, 0.2f));
            var disc = Prim(PrimitiveType.Cylinder, "Glow", b, new Vector3(0f, 1.42f, 0f),
                new Vector3(9.5f, 0.1f, 9.5f), MaterialLibrary.Emissive(new Color(1f, 0.45f, 0.1f), 1.1f));
            var crystal = Prim(PrimitiveType.Cube, "Crystal", b, new Vector3(0f, 4.2f, 0f),
                new Vector3(2.6f, 4.2f, 2.6f), MaterialLibrary.Emissive(new Color(1f, 0.28f, 0.22f), 1.5f));
            crystal.localRotation = Quaternion.Euler(0f, 45f, 0f);
            // orbiting shards
            var orbit = new GameObject("Orbit").transform;
            orbit.SetParent(b, false);
            orbit.localPosition = new Vector3(0f, 4.2f, 0f);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f;
                var shard = Prim(PrimitiveType.Cube, "Shard" + i, orbit,
                    new Vector3(Mathf.Cos(a) * 3.4f, Mathf.Sin(a * 2f) * 0.6f, Mathf.Sin(a) * 3.4f),
                    Vector3.one * 0.7f, MaterialLibrary.Emissive(new Color(1f, 0.55f, 0.25f), 1.6f));
                shard.localRotation = Quaternion.Euler(30f, 45f, 0f);
            }
            b.GetComponent<HubBuilding>().Setup("summon", "Summon", orbit, crystal, true);
        }

        private void BuildAfkChest(Vector3 pos)
        {
            var b = NewBuilding("afk", "AFK Chest", pos, true);
            var disc = Prim(PrimitiveType.Cylinder, "Glow", b, new Vector3(0f, 0.95f, 0f),
                new Vector3(6f, 0.08f, 6f), MaterialLibrary.Emissive(new Color(1f, 0.8f, 0.25f), 0.9f));
            var baseBox = Prim(PrimitiveType.Cube, "Base", b, new Vector3(0f, 2.0f, 0f),
                new Vector3(3.6f, 1.8f, 2.4f), MaterialLibrary.Solid(new Color(0.72f, 0.5f, 0.16f), 0.45f, 0.55f));
            var lid = Prim(PrimitiveType.Cube, "Lid", b, new Vector3(0f, 3.15f, -0.25f),
                new Vector3(3.7f, 0.8f, 2.5f), MaterialLibrary.Solid(new Color(0.85f, 0.62f, 0.2f), 0.5f, 0.6f));
            lid.localRotation = Quaternion.Euler(-18f, 0f, 0f);
            Prim(PrimitiveType.Sphere, "Lock", b, new Vector3(0f, 2.5f, 1.25f),
                Vector3.one * 0.55f, MaterialLibrary.Emissive(new Color(1f, 0.9f, 0.4f), 1.3f));
            b.GetComponent<HubBuilding>().Setup("afk", "AFK Chest", null, lid, false);
        }

        private void BuildForge(Vector3 pos)
        {
            var b = NewBuilding("forge", "Forge", pos, true);
            Prim(PrimitiveType.Cube, "Base", b, new Vector3(0f, 1.2f, 0f),
                new Vector3(6f, 2.4f, 5f), MaterialLibrary.Solid(new Color(0.32f, 0.22f, 0.16f), 0.25f, 0.1f));
            // anvil
            Prim(PrimitiveType.Cube, "AnvilTop", b, new Vector3(0f, 3.6f, 1.6f),
                new Vector3(2.6f, 0.7f, 1.2f), MaterialLibrary.Solid(new Color(0.3f, 0.3f, 0.34f), 0.5f, 0.7f));
            Prim(PrimitiveType.Cube, "AnvilBase", b, new Vector3(0f, 2.6f, 1.6f),
                new Vector3(1.1f, 1.2f, 0.9f), MaterialLibrary.Solid(new Color(0.22f, 0.22f, 0.25f), 0.4f, 0.6f));
            // chimney with glowing mouth
            Prim(PrimitiveType.Cylinder, "Chimney", b, new Vector3(-1.8f, 4.6f, -1.2f),
                new Vector3(1.8f, 7.5f, 1.8f), MaterialLibrary.Solid(new Color(0.4f, 0.38f, 0.4f), 0.3f, 0.1f));
            var mouth = Prim(PrimitiveType.Cylinder, "Mouth", b, new Vector3(-1.8f, 8.45f, -1.2f),
                new Vector3(1.9f, 0.25f, 1.9f), MaterialLibrary.Emissive(new Color(1f, 0.5f, 0.12f), 1.8f));
            // awning
            var roof = Prim(PrimitiveType.Cube, "Roof", b, new Vector3(0.6f, 5.3f, 0.4f),
                new Vector3(6.8f, 0.35f, 5.6f), MaterialLibrary.Solid(new Color(0.55f, 0.2f, 0.12f), 0.3f, 0.1f));
            roof.localRotation = Quaternion.Euler(0f, 0f, -8f);
            b.GetComponent<HubBuilding>().Setup("forge", "Forge", mouth, roof, false);
        }

        private void BuildBeast(Vector3 pos)
        {
            var b = NewBuilding("beast", "Beast", pos, true);
            // red "tome" cage with emissive cracks
            Prim(PrimitiveType.Cube, "Book", b, new Vector3(0f, 3.2f, 0f),
                new Vector3(4.6f, 6f, 1.6f), MaterialLibrary.Solid(new Color(0.5f, 0.14f, 0.14f), 0.35f, 0.2f));
            Prim(PrimitiveType.Cube, "Spine", b, new Vector3(0f, 3.2f, 0.95f),
                new Vector3(1.1f, 6.1f, 0.5f), MaterialLibrary.Solid(new Color(0.32f, 0.08f, 0.08f), 0.3f, 0.3f));
            var eye = Prim(PrimitiveType.Sphere, "Eye", b, new Vector3(0f, 3.6f, 0.9f),
                Vector3.one * 1.5f, MaterialLibrary.Emissive(new Color(0.9f, 0.15f, 0.2f), 1.6f));
            // horns
            var h1 = Prim(ConePrim, "HornL", b, new Vector3(-2.1f, 6.6f, 0f),
                new Vector3(1f, 2.4f, 1f), MaterialLibrary.Solid(new Color(0.9f, 0.85f, 0.75f), 0.4f, 0.1f));
            h1.localRotation = Quaternion.Euler(0f, 0f, 18f);
            var h2 = Prim(ConePrim, "HornR", b, new Vector3(2.1f, 6.6f, 0f),
                new Vector3(1f, 2.4f, 1f), MaterialLibrary.Solid(new Color(0.9f, 0.85f, 0.75f), 0.4f, 0.1f));
            h2.localRotation = Quaternion.Euler(0f, 0f, -18f);
            b.GetComponent<HubBuilding>().Setup("beast", "Beast", eye, null, false);
        }

        private void BuildMine(Vector3 pos)
        {
            var b = NewBuilding("mine", "Dwarven Mine", pos, false);
            // rock pile
            Prim(PrimitiveType.Sphere, "Rock1", b, new Vector3(-1.8f, 1.6f, -0.6f),
                new Vector3(4.2f, 3.2f, 3.4f), MaterialLibrary.Solid(new Color(0.42f, 0.4f, 0.44f), 0.2f, 0.1f));
            Prim(PrimitiveType.Sphere, "Rock2", b, new Vector3(1.9f, 1.3f, 0.4f),
                new Vector3(3.4f, 2.6f, 3f), MaterialLibrary.Solid(new Color(0.36f, 0.34f, 0.38f), 0.2f, 0.1f));
            // wooden frame
            var wood = MaterialLibrary.Solid(new Color(0.4f, 0.26f, 0.14f), 0.25f, 0.05f);
            Prim(PrimitiveType.Cube, "BeamL", b, new Vector3(-1.5f, 2.2f, 2.2f), new Vector3(0.5f, 4.4f, 0.5f), wood);
            Prim(PrimitiveType.Cube, "BeamR", b, new Vector3(1.5f, 2.2f, 2.2f), new Vector3(0.5f, 4.4f, 0.5f), wood);
            var top = Prim(PrimitiveType.Cube, "BeamTop", b, new Vector3(0f, 4.5f, 2.2f), new Vector3(3.8f, 0.5f, 0.5f), wood);
            // dark entrance with a faint gold glimmer
            Prim(PrimitiveType.Cube, "Mouth", b, new Vector3(0f, 1.7f, 1.8f),
                new Vector3(2.4f, 3.2f, 0.4f), MaterialLibrary.Solid(new Color(0.05f, 0.04f, 0.05f), 0.1f, 0f));
            var glimmer = Prim(PrimitiveType.Sphere, "Glimmer", b, new Vector3(0.4f, 1.5f, 1.95f),
                Vector3.one * 0.5f, MaterialLibrary.Emissive(new Color(1f, 0.8f, 0.3f), 1.4f));
            b.GetComponent<HubBuilding>().Setup("mine", "Dwarven Mine", glimmer, top, false);
        }

        private void BuildDungeon(Vector3 pos)
        {
            var b = NewBuilding("dungeon", "Dungeon", pos, true);
            var rock = MaterialLibrary.Solid(new Color(0.2f, 0.16f, 0.24f), 0.3f, 0.1f);
            Prim(PrimitiveType.Sphere, "Rock1", b, new Vector3(-2.2f, 1.8f, 0f),
                new Vector3(4.4f, 3.6f, 3.6f), rock);
            Prim(PrimitiveType.Sphere, "Rock2", b, new Vector3(2.2f, 1.5f, -0.4f),
                new Vector3(3.6f, 3f, 3.2f), rock);
            // purple portal
            var portal = Prim(PrimitiveType.Cylinder, "Portal", b, new Vector3(0f, 3.0f, 2.0f),
                new Vector3(3.4f, 0.25f, 3.4f), MaterialLibrary.Emissive(new Color(0.65f, 0.25f, 0.95f), 1.7f));
            portal.localRotation = Quaternion.Euler(90f, 0f, 0f);
            // spikes
            var spike = MaterialLibrary.Solid(new Color(0.14f, 0.11f, 0.18f), 0.35f, 0.2f);
            var s1 = Prim(ConePrim, "Spike1", b, new Vector3(-3.6f, 3.4f, 0.6f), new Vector3(1.2f, 4.6f, 1.2f), spike);
            s1.localRotation = Quaternion.Euler(0f, 0f, 14f);
            var s2 = Prim(ConePrim, "Spike2", b, new Vector3(3.5f, 3.0f, -0.2f), new Vector3(1f, 3.6f, 1f), spike);
            s2.localRotation = Quaternion.Euler(0f, 0f, -12f);
            b.GetComponent<HubBuilding>().Setup("dungeon", "Dungeon", portal, null, true);
        }

        private void BuildGuild(Vector3 pos)
        {
            var b = NewBuilding("guild", "Guild", pos, false);
            var stone = MaterialLibrary.Solid(new Color(0.88f, 0.85f, 0.76f), 0.3f, 0.05f);
            var gold = MaterialLibrary.Solid(new Color(0.85f, 0.65f, 0.2f), 0.55f, 0.65f);
            Prim(PrimitiveType.Cube, "Keep", b, new Vector3(0f, 3.4f, 0f), new Vector3(8.4f, 6.8f, 6.4f), stone);
            Prim(PrimitiveType.Cube, "Roof", b, new Vector3(0f, 7.3f, 0f), new Vector3(9f, 1.4f, 7f), gold);
            // twin towers + spires
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Cylinder, "Tower" + side, b, new Vector3(side * 4.6f, 4.6f, 0f),
                    new Vector3(2.4f, 9.2f, 2.4f), stone);
                var spire = Prim(ConePrim, "Spire" + side, b, new Vector3(side * 4.6f, 10.4f, 0f),
                    new Vector3(2.8f, 3.2f, 2.8f), gold);
                spire.localRotation = Quaternion.identity;
            }
            // gate + windows
            Prim(PrimitiveType.Cube, "Gate", b, new Vector3(0f, 1.9f, 3.25f),
                new Vector3(2.6f, 3.8f, 0.4f), MaterialLibrary.Solid(new Color(0.2f, 0.13f, 0.08f), 0.2f, 0.2f));
            for (int i = -1; i <= 1; i++)
                Prim(PrimitiveType.Cube, "Win" + i, b, new Vector3(i * 2.6f, 4.6f, 3.25f),
                    new Vector3(0.8f, 1.4f, 0.3f), MaterialLibrary.Emissive(new Color(1f, 0.85f, 0.45f), 1.2f));
            // eagle statue on top
            Prim(PrimitiveType.Sphere, "Orb", b, new Vector3(0f, 8.6f, 0f),
                Vector3.one * 0.9f, MaterialLibrary.Emissive(new Color(1f, 0.85f, 0.4f), 1.4f));
            b.GetComponent<HubBuilding>().Setup("guild", "Guild", null, null, true);
        }

        private void BuildArena(Vector3 pos)
        {
            var b = NewBuilding("arena", "PVP", pos, true);
            var stone = MaterialLibrary.Solid(new Color(0.62f, 0.58f, 0.52f), 0.3f, 0.05f);
            Prim(PrimitiveType.Cylinder, "Floor", b, new Vector3(0f, 0.95f, 0f),
                new Vector3(13f, 0.35f, 13f), MaterialLibrary.Solid(new Color(0.7f, 0.62f, 0.46f), 0.25f, 0.05f));
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                Prim(PrimitiveType.Cylinder, "Col" + i, b,
                    new Vector3(Mathf.Cos(a) * 5.6f, 3.0f, Mathf.Sin(a) * 5.6f),
                    new Vector3(1.1f, 5.4f, 1.1f), stone);
            }
            // central banner
            Prim(PrimitiveType.Cylinder, "Pole", b, new Vector3(0f, 5.4f, 0f),
                new Vector3(0.5f, 9.6f, 0.5f), MaterialLibrary.Solid(new Color(0.3f, 0.2f, 0.12f), 0.3f, 0.3f));
            var flag = Prim(PrimitiveType.Cube, "Flag", b, new Vector3(1.5f, 8.4f, 0f),
                new Vector3(3.4f, 2.2f, 0.15f), MaterialLibrary.Solid(new Color(0.65f, 0.12f, 0.1f), 0.35f, 0.1f));
            var embl = Prim(PrimitiveType.Cylinder, "Emblem", b, new Vector3(1.5f, 8.4f, 0.1f),
                new Vector3(1.2f, 0.12f, 1.2f), MaterialLibrary.Emissive(new Color(1f, 0.8f, 0.3f), 1.3f));
            embl.localRotation = Quaternion.Euler(90f, 0f, 0f);
            b.GetComponent<HubBuilding>().Setup("arena", "PVP", embl, flag, true);
        }

        private void BuildTower(Vector3 pos)
        {
            var b = NewBuilding("tower", "Ranking", pos, false);
            var stone = MaterialLibrary.Solid(new Color(0.82f, 0.79f, 0.7f), 0.3f, 0.05f);
            float y = 1.2f;
            for (int i = 0; i < 5; i++)
            {
                float r = 3.4f - i * 0.42f;
                Prim(PrimitiveType.Cylinder, "Seg" + i, b, new Vector3(0f, y + 1.6f, 0f),
                    new Vector3(r * 2f, 3.2f, r * 2f), stone);
                Prim(PrimitiveType.Cube, "Win" + i, b, new Vector3(0f, y + 1.8f, r - 0.05f),
                    new Vector3(0.7f, 1.1f, 0.25f), MaterialLibrary.Emissive(new Color(1f, 0.85f, 0.45f), 1.2f));
                y += 3.3f;
            }
            var tip = Prim(ConePrim, "Tip", b, new Vector3(0f, y + 1.4f, 0f),
                new Vector3(3.6f, 2.8f, 3.6f), MaterialLibrary.Solid(new Color(0.85f, 0.65f, 0.2f), 0.55f, 0.65f));
            b.GetComponent<HubBuilding>().Setup("tower", "Ranking", null, tip, false);
        }

        private void BuildContinent(Vector3 pos)
        {
            var b = NewBuilding("continent", "Continent", pos, true);
            var stone = MaterialLibrary.Solid(new Color(0.55f, 0.53f, 0.5f), 0.3f, 0.1f);
            Prim(PrimitiveType.Cube, "PillarL", b, new Vector3(-2.6f, 3.4f, 0f), new Vector3(1.4f, 6.8f, 1.6f), stone);
            Prim(PrimitiveType.Cube, "PillarR", b, new Vector3(2.6f, 3.4f, 0f), new Vector3(1.4f, 6.8f, 1.6f), stone);
            Prim(PrimitiveType.Cube, "Arch", b, new Vector3(0f, 7.1f, 0f), new Vector3(7f, 1.4f, 1.8f), stone);
            var portal = Prim(PrimitiveType.Cylinder, "Portal", b, new Vector3(0f, 3.6f, 0f),
                new Vector3(4.6f, 0.2f, 4.6f), MaterialLibrary.Emissive(new Color(0.25f, 0.6f, 1f), 1.9f));
            portal.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Prim(PrimitiveType.Cylinder, "Steps", b, new Vector3(0f, 0.95f, 0f),
                new Vector3(8f, 0.4f, 8f), MaterialLibrary.Solid(new Color(0.45f, 0.43f, 0.4f), 0.3f, 0.1f));
            b.GetComponent<HubBuilding>().Setup("continent", "Continent", portal, null, false);
        }

        // ------------------------------------------------------------ helpers
        private Transform NewBuilding(string id, string displayName, Vector3 localPos, bool dot)
        {
            var go = new GameObject("B_" + id);
            go.transform.SetParent(_root, false);
            go.transform.localPosition = localPos;
            var hb = go.AddComponent<HubBuilding>();
            _buildings.Add(hb);
            return go.transform;
        }

        // Unity has no PrimitiveType.Cone, so build one (same size as a cylinder: radius 0.5, height 2).
        private const PrimitiveType ConePrim = (PrimitiveType)100;
        private static Mesh _coneMesh;

        private static Mesh ConeMesh()
        {
            if (_coneMesh != null) return _coneMesh;
            const int seg = 24;
            var v = new List<Vector3>();
            var tri = new List<int>();
            var apex = new Vector3(0f, 1f, 0f);
            for (int i = 0; i < seg; i++)
            {
                float a0 = i * Mathf.PI * 2f / seg;
                float a1 = (i + 1) * Mathf.PI * 2f / seg;
                int s = v.Count;
                v.Add(apex);
                v.Add(new Vector3(Mathf.Cos(a1) * 0.5f, -1f, Mathf.Sin(a1) * 0.5f));
                v.Add(new Vector3(Mathf.Cos(a0) * 0.5f, -1f, Mathf.Sin(a0) * 0.5f));
                tri.Add(s); tri.Add(s + 1); tri.Add(s + 2);
                int c = v.Count;
                v.Add(new Vector3(0f, -1f, 0f));
                v.Add(new Vector3(Mathf.Cos(a0) * 0.5f, -1f, Mathf.Sin(a0) * 0.5f));
                v.Add(new Vector3(Mathf.Cos(a1) * 0.5f, -1f, Mathf.Sin(a1) * 0.5f));
                tri.Add(c); tri.Add(c + 1); tri.Add(c + 2);
            }
            var m = new Mesh { name = "ConeMesh" };
            m.SetVertices(v);
            m.SetTriangles(tri, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            _coneMesh = m;
            return m;
        }

        private Transform Prim(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject go;
            if (type == ConePrim)
            {
                go = new GameObject(name);
                go.AddComponent<MeshFilter>().sharedMesh = ConeMesh();
                go.AddComponent<MeshRenderer>();
            }
            else go = GameObject.CreatePrimitive(type);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go.transform;
        }

        private void PlantGreeters(TeamManager team)
        {
            var spots = new[]
            {
                new Vector3(5.5f, 0.9f, 17.5f),
                new Vector3(-6.5f, 0.9f, 19.5f),
                new Vector3(-11f, 0.9f, 15f)
            };
            int n = Mathf.Min(3, team.Heroes.Count);
            for (int i = 0; i < n; i++)
            {
                var hero = team.Heroes[i];
                var go = new GameObject("HubGreeter_" + hero.Data.displayName);
                go.transform.SetParent(_root, false);
                go.transform.localPosition = spots[i];
                go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // face the camera side
                var el = ElementalMatrix.GetElementColor(hero.Data.element);
                CharacterBodyFactory.Build(go, hero.Data.element, true, 1.15f,
                    el, Color.Lerp(el, Color.black, 0.45f),
                    Art.ArtSlots.SlotForHeroClass(hero.Data.heroClass));
            }
        }

        private void Update()
        {
            // gentle portal/water pulse (unscaled time — runs while paused)
            float t = Time.unscaledTime;
            for (int i = 0; i < _portals.Count; i++)
            {
                var p = _portals[i];
                if (p == null) continue;
                float k = 1f + Mathf.Sin(t * 1.6f + i * 1.3f) * 0.02f;
                p.localScale = new Vector3(p.localScale.x, p.localScale.y, p.localScale.x * k);
            }
        }
    }
}