using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Combat;

namespace LegendsRPG.Items
{
    /// <summary>
    /// Formula-driven item factory. Every category is a small template table
    /// crossed with the 5 rarity tiers (and elements for pets), so adding
    /// content never means duplicating code — exactly like the hero roster.
    /// Totals: 40 weapons + 40 armors + 40 amulets + 40 wings + 35 pets + 40 mounts = 235.
    /// </summary>
    public static class ItemDatabase
    {
        private static Dictionary<string, ItemData> _items;
        public static Dictionary<string, ItemData> All
        {
            get
            {
                if (_items == null) Init();
                return _items;
            }
        }

        // rarity multiplier: Common, Rare, Epic, Legendary, Mythic
        private static readonly float[] RarityMult = { 1.0f, 1.4f, 1.9f, 2.6f, 3.5f };

        private static readonly string[] WeaponNames =
        {
            "Rusty Blade", "Iron Saber", "Steel Greatsword", "Ember Edge",
            "Frost Fang", "Storm Piercer", "Dusk Carver", "Dawn Breaker"
        };

        private static readonly string[] ArmorNames =
        {
            "Cloth Tunic", "Leather Vest", "Chainmail", "Scale Cuirass",
            "Frostweave Coat", "Stormplate", "Shadow Shroud", "Sunward Aegis"
        };

        private static readonly string[] AmuletNames =
        {
            "Bone Charm", "Copper Talisman", "Silver Pendant", "Ember Locket",
            "Icy Sigil", "Storm Eye", "Void Rune", "Radiant Seal"
        };

        private static readonly string[] WingNames =
        {
            "Feather Wings", "Gale Wings", "Ember Wings", "Frost Wings",
            "Storm Wings", "Void Wings", "Radiant Wings", "Serpent Wings"
        };

        private static readonly string[] MountNames =
        {
            "Meadow Runner", "Dust Strider", "Frost Mane", "Ember Hound",
            "Storm Charger", "Dusk Panther", "Radiant Stag", "Ancient Colossus"
        };

        private static readonly ElementType[] PetElements =
        {
            ElementType.Fire, ElementType.Water, ElementType.Nature, ElementType.Ice,
            ElementType.Lightning, ElementType.Dark, ElementType.Light
        };

        private static readonly string[] PetNames =
        {
            "Ember Fox", "Tide Otter", "Grove Sprout", "Frost Puff",
            "Spark Sprite", "Shade Bat", "Glimmer Moth"
        };

        public static void Init()
        {
            if (_items != null) return;
            _items = new Dictionary<string, ItemData>();

            for (int t = 0; t < 8; t++)
                for (int r = 0; r < 5; r++)
                    Register(Weapon(WeaponNames[t], t, r));

            for (int t = 0; t < 8; t++)
                for (int r = 0; r < 5; r++)
                    Register(Armor(ArmorNames[t], t, r));

            for (int t = 0; t < 8; t++)
                for (int r = 0; r < 5; r++)
                    Register(Amulet(AmuletNames[t], t, r));

            for (int t = 0; t < 8; t++)
                for (int r = 0; r < 5; r++)
                    Register(Wings(WingNames[t], t, r));

            for (int e = 0; e < PetElements.Length; e++)
                for (int r = 0; r < 5; r++)
                    Register(Pet(PetNames[e], e, r));

            for (int t = 0; t < 8; t++)
                for (int r = 0; r < 5; r++)
                    Register(Mount(MountNames[t], t, r));
        }

        public static ItemData Get(string itemId)
        {
            if (itemId != null && All.ContainsKey(itemId)) return All[itemId];
            return null;
        }

        public static int Count { get { return All.Count; } }

        // ---- category factories ----

        private static ItemData Weapon(string name, int t, int r)
        {
            float m = RarityMult[r];
            // weapon identity: attack-focused, later templates are stronger
            float atk = (4f + 1.5f * t) * m;
            float hp = (6f + 2f * t) * m;
            return Build("wpn_" + t + "_r" + r, name + " [" + RarityName(r) + "]",
                ItemCategory.Weapon, r, ElementForTemplate(t), hp, atk, 0f, 0f);
        }

        private static ItemData Armor(string name, int t, int r)
        {
            float m = RarityMult[r];
            float hp = (30f + 10f * t) * m;
            float def = (3f + 1.2f * t) * m;
            return Build("arm_" + t + "_r" + r, name + " [" + RarityName(r) + "]",
                ItemCategory.Armor, r, ElementForTemplate(t), hp, 0f, def, 0f);
        }

        private static ItemData Amulet(string name, int t, int r)
        {
            float m = RarityMult[r];
            float hp = (12f + 3f * t) * m;
            float atk = (1.5f + 0.5f * t) * m;
            float def = (1.5f + 0.5f * t) * m;
            return Build("amu_" + t + "_r" + r, name + " [" + RarityName(r) + "]",
                ItemCategory.Amulet, r, ElementForTemplate(t), hp, atk, def, 0f);
        }

        private static ItemData Wings(string name, int t, int r)
        {
            float m = RarityMult[r];
            float hp = (15f + 4f * t) * m;
            float speed = (0.25f + 0.05f * t) * m;
            return Build("wing_" + t + "_r" + r, name + " [" + RarityName(r) + "]",
                ItemCategory.Wings, r, ElementForTemplate(t), hp, 0f, 0f, speed);
        }

        private static ItemData Pet(string name, int e, int r)
        {
            float m = RarityMult[r];
            float atk = (3f + 1.1f * e) * m;
            float hp = (10f + 3f * e) * m;
            return Build("pet_" + e + "_r" + r, name + " [" + RarityName(r) + "]",
                ItemCategory.Pet, r, PetElements[e], hp, atk, 0f, 0f);
        }

        private static ItemData Mount(string name, int t, int r)
        {
            float m = RarityMult[r];
            float speed = (0.7f + 0.12f * t) * m;
            float hp = (20f + 5f * t) * m;
            return Build("mnt_" + t + "_r" + r, name + " [" + RarityName(r) + "]",
                ItemCategory.Mount, r, ElementForTemplate(t), hp, 0f, 0f, speed);
        }

        // ---- helpers ----

        private static ItemData Build(string id, string name, ItemCategory cat, int r,
            ElementType el, float hp, float atk, float def, float speed)
        {
            var it = new ItemData();
            it.itemId = id;
            it.displayName = name;
            it.category = cat;
            it.rarity = (Rarity)r;
            it.element = el;
            it.bonusHp = Mathf.RoundToInt(hp);
            it.bonusAttack = Mathf.RoundToInt(atk * 10f) / 10f;
            it.bonusDefense = Mathf.RoundToInt(def * 10f) / 10f;
            it.bonusMoveSpeed = Mathf.RoundToInt(speed * 100f) / 100f;
            return it;
        }

        private static string RarityName(int r)
        {
            switch (r)
            {
                case 1: return "Rare";
                case 2: return "Epic";
                case 3: return "Legendary";
                case 4: return "Mythic";
                default: return "Common";
            }
        }

        private static ElementType ElementForTemplate(int t)
        {
            ElementType[] els =
            {
                ElementType.None, ElementType.None, ElementType.None,
                ElementType.Fire, ElementType.Ice, ElementType.Lightning,
                ElementType.Dark, ElementType.Light
            };
            return els[t];
        }

        private static void Register(ItemData item)
        {
            _items[item.itemId] = item;
        }
        // ---- v1.0 helpers used by shop / merchant / rewards ----
        public static System.Collections.Generic.List<string> AllItemIds()
        {
            return new System.Collections.Generic.List<string>(All.Keys);
        }

        public static string RollRandomItemId()
        {
            var ids = new System.Collections.Generic.List<string>(All.Keys);
            return ids.Count == 0 ? null : ids[UnityEngine.Random.Range(0, ids.Count)];
        }
    }
}