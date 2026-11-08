using System;
using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;

namespace LegendsRPG.Art
{
    // =====================================================================
    //  ART KIT (v0.4)
    //  Bridges store-bought art (Asset Store packages, FBX, prefabs) into
    //  the code-first prototype WITHOUT breaking the plain-code build:
    //  - Drop prefabs anywhere under  Assets/GameArt/
    //  - Open  Legends RPG > Art > Art Slots  and auto-match (or drag+drop)
    //  - Save: the map is written to  Assets/Resources/ArtSlotMap.asset
    //  At runtime every character first tries its art slot; when no map is
    //  present (fresh CI checkout) the primitive bodies are used as before.
    // =====================================================================

    /// <summary>Canonical slot ids the game requests. The Art Slots window
    /// can auto-match prefabs found in Assets/GameArt by name keywords.</summary>
    public static class ArtSlots
    {
        public const string HeroWarrior = "hero_warrior";
        public const string HeroMage = "hero_mage";
        public const string HeroArcher = "hero_archer";
        public const string HeroPaladin = "hero_paladin";
        public const string HeroAssassin = "hero_assassin";
        public const string HeroHealer = "hero_healer";

        public const string PropChest = "prop_chest";
        public const string PropAltar = "prop_altar";

        public static readonly string[] HeroSlotIds =
        {
            HeroWarrior, HeroMage, HeroArcher, HeroPaladin, HeroAssassin, HeroHealer
        };

        public static string SlotForHeroClass(HeroClass cls)
        {
            switch (cls)
            {
                case HeroClass.Mage: return HeroMage;
                case HeroClass.Archer: return HeroArcher;
                case HeroClass.Paladin: return HeroPaladin;
                case HeroClass.Assassin: return HeroAssassin;
                case HeroClass.Healer: return HeroHealer;
                default: return HeroWarrior;
            }
        }

        // Monster slots use the raw monsterId (goblin, wolf, shaman, golem,
        // wraith, boss_titan, boss_lich) so new monsters plug in for free.

        /// <summary>Ordered auto-match keywords (slot -> name fragments).
        /// Checked case-insensitively against the prefab name and its path.</summary>
        public static readonly KeyValuePair<string, string[]>[] AutoMatch =
        {
            new KeyValuePair<string, string[]>(HeroWarrior, new[] { "warrior", "knight", "soldier", "sword", "guard", "berserk" }),
            new KeyValuePair<string, string[]>(HeroMage, new[] { "mage", "wizard", "witch", "sorcer", "spellcaster", "enchant" }),
            new KeyValuePair<string, string[]>(HeroArcher, new[] { "archer", "ranger", "bow", "huntress", "hunter" }),
            new KeyValuePair<string, string[]>(HeroPaladin, new[] { "paladin", "cleric", "templar", "crusader", "holy" }),
            new KeyValuePair<string, string[]>(HeroAssassin, new[] { "assassin", "rogue", "ninja", "thief", "shadow" }),
            new KeyValuePair<string, string[]>(HeroHealer, new[] { "healer", "druid", "priest", "monk", "support" }),

            new KeyValuePair<string, string[]>("goblin", new[] { "goblin", "imp", "gremlin" }),
            new KeyValuePair<string, string[]>("wolf", new[] { "wolf", "hound", "warg", "beast" }),
            new KeyValuePair<string, string[]>("shaman", new[] { "shaman", "witchdoctor", "hexxer" }),
            new KeyValuePair<string, string[]>("golem", new[] { "golem", "construct", "giant" }),
            new KeyValuePair<string, string[]>("wraith", new[] { "wraith", "ghost", "skeleton", "zombie", "undead", "phantom" }),
            new KeyValuePair<string, string[]>("boss_titan", new[] { "titan", "demon", "ogre", "minotaur" }),
            new KeyValuePair<string, string[]>("boss_lich", new[] { "lich", "necromancer", "deathlord" }),

            new KeyValuePair<string, string[]>(PropChest, new[] { "chest", "crate", "treasure" }),
            new KeyValuePair<string, string[]>(PropAltar, new[] { "altar", "shrine", "summon_circle", "portal" }),
        };
    }

    /// <summary>
    /// Slot -> prefab mapping persisted at Assets/Resources/ArtSlotMap.asset
    /// (Resources so it is packed into builds automatically; when the asset
    /// does not exist — plain CI checkout — the game falls back to primitives).
    /// </summary>
    [CreateAssetMenu(fileName = "ArtSlotMap", menuName = "Legends RPG/Art Slot Map")]
    public class ArtSlotMap : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string slot;
            public GameObject prefab;
        }

        public List<Entry> entries = new List<Entry>();

        public GameObject GetPrefab(string slot)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].prefab != null && entries[i].slot == slot) return entries[i].prefab;
            }
            return null;
        }
    }

    /// <summary>Runtime access to the art map. Never throws — everything is
    /// null-safe so the game keeps working with zero art installed.</summary>
    public static class ArtLibrary
    {
        private const string ResourcePath = "ArtSlotMap";

        private static ArtSlotMap _map;
        private static bool _tried;

        /// <summary>True when at least one prefab slot is assigned.</summary>
        public static bool HasArt
        {
            get { return GetMap() != null && GetMap().entries.Count > 0; }
        }

        public static ArtSlotMap GetMap()
        {
            if (_tried) return _map;
            _tried = true;
            try
            {
                _map = Resources.Load<ArtSlotMap>(ResourcePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ArtLibrary] Failed to load " + ResourcePath + ": " + e.Message);
                _map = null;
            }
            if (_map != null)
            {
                int n = 0;
                for (int i = 0; i < _map.entries.Count; i++) if (_map.entries[i].prefab != null) n++;
                if (n > 0) Debug.Log("[ArtLibrary] Art slot map loaded: " + n + " prefab(s).");
            }
            return _map;
        }

        public static GameObject GetPrefab(string slot)
        {
            var map = GetMap();
            return map != null ? map.GetPrefab(slot) : null;
        }

        /// <summary>
        /// Instantiates the prefab bound to <paramref name="slot"/> under
        /// <paramref name="parent"/>, strips its colliders and scales it so
        /// the model stands <paramref name="targetHeight"/> metres tall.
        /// Returns null when the slot has no art (caller keeps primitives).
        /// </summary>
        public static GameObject Instantiate(string slot, Transform parent, float targetHeight)
        {
            var prefab = GetPrefab(slot);
            if (prefab == null || parent == null) return null;

            GameObject go;
            try
            {
                go = UnityEngine.Object.Instantiate(prefab, parent, false);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ArtLibrary] Instantiate failed for slot '" + slot + "': " + e.Message);
                return null;
            }

            go.name = "ArtModel_" + slot;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = prefab.transform.localScale;

            // collision is owned by the CharacterController on the character root
            var colliders = go.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) UnityEngine.Object.Destroy(colliders[i]);

            // normalise the height so any source model (Synty, custom, ...) fits
            if (targetHeight > 0f) NormalizeHeight(go, targetHeight);
            return go;
        }

        private static void NormalizeHeight(GameObject go, float targetHeight)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            float h = bounds.size.y;
            if (h <= 0.0001f) return;

            float k = targetHeight / h;
            // run twice: scaling the root also moves the children in world space
            for (int pass = 0; pass < 2; pass++)
            {
                go.transform.localScale *= k;
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                h = bounds.size.y;
                if (h <= 0.0001f) return;
                k = targetHeight / h;
            }
        }
    }
}
