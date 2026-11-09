using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Characters;
using LegendsRPG.Core;
using LegendsRPG.Combat;

namespace LegendsRPG.Items
{
    /// <summary>
    /// Ownership + equipping of items, wired into CharacterStats bonuses,
    /// hero visuals (wings / pet orb) and the save file.
    /// Equipment applies REAL stat changes through CharacterStats.AddEquipmentBonus.
    /// Floor clears roll loot, so items are actually obtainable in gameplay.
    /// </summary>
    public static class InventorySystem
    {
        public const int SlotWeapon = 0;
        public const int SlotArmor = 1;
        public const int SlotAmulet = 2;
        public const int SlotWings = 3;
        public const int SlotPet = 4;
        public const int SlotMount = 5;
        public const int SlotCount = 6;

        private static readonly string[] SlotNames =
            { "Weapon", "Armor", "Amulet", "Wings", "Pet", "Mount" };

        private static HashSet<string> _owned = new HashSet<string>();
        // heroId -> (slot -> itemId)
        private static Dictionary<string, Dictionary<int, string>> _equipped =
            new Dictionary<string, Dictionary<int, string>>();

        public static int OwnedCount { get { return _owned.Count; } }
        public static bool IsOwned(string itemId) { return itemId != null && _owned.Contains(itemId); }

        public static string GetEquippedId(string heroId, int slot)
        {
            Dictionary<int, string> slots;
            if (heroId != null && _equipped.TryGetValue(heroId, out slots))
            {
                string id;
                if (slots.TryGetValue(slot, out id)) return id;
            }
            return null;
        }

        public static string GetEquippedName(string heroId, int slot)
        {
            var item = ItemDatabase.Get(GetEquippedId(heroId, slot));
            return item != null ? item.displayName : SlotNames[slot] + ": -";
        }

        // ================= lifecycle =================

        /// <summary>Call once after SaveSystem.Load(), before the team is created.</summary>
        public static void Initialize()
        {
            ItemDatabase.Init();
            _owned = new HashSet<string>();
            _equipped = new Dictionary<string, Dictionary<int, string>>();
            ReadFrom(SaveSystem.Current);

            if (!SaveSystem.Current.starterKitGranted)
            {
                GrantStarterKit();
                SaveSystem.Current.starterKitGranted = true;
            }
        }

        private static void GrantStarterKit()
        {
            // one usable piece per slot so every system is testable from minute one
            string[] kit = { "wpn_0_r0", "arm_0_r0", "amu_0_r0", "wing_0_r0", "pet_0_r0", "mnt_0_r0" };
            foreach (var id in kit) Grant(id);

            for (int s = 0; s < SlotCount; s++) Equip("kael_flameblade", s, kit[s], persist: false);
        }

        public static void Grant(string itemId)
        {
            if (itemId == null || !ItemDatabase.All.ContainsKey(itemId)) return;
            if (_owned.Contains(itemId)) return;
            _owned.Add(itemId);
        }

        // ================= equipping =================

        public static void Equip(string heroId, int slot, string itemId, bool persist = true)
        {
            if (heroId == null || slot < 0 || slot >= SlotCount) return;
            if (itemId != null && !IsOwned(itemId)) return;

            Dictionary<int, string> slots;
            if (!_equipped.TryGetValue(heroId, out slots))
            {
                slots = new Dictionary<int, string>();
                _equipped[heroId] = slots;
            }

            // an item can only be worn by one hero at a time — unequip elsewhere
            if (itemId != null)
            {
                foreach (var kv in _equipped)
                {
                    foreach (var s in new List<int>(kv.Value.Keys))
                    {
                        if (kv.Value[s] == itemId && !(kv.Key == heroId && s == slot))
                        {
                            kv.Value.Remove(s);
                            RefreshHeroById(kv.Key);
                        }
                    }
                }
            }

            if (itemId == null) slots.Remove(slot);
            else slots[slot] = itemId;

            RefreshHeroById(heroId);

            if (persist) SaveSystem.Save(GameManager.Instance != null ? GameManager.Instance.Tower : null,
                                         GameManager.Instance != null ? GameManager.Instance.Team : null);
        }

        private static void RefreshHeroById(string heroId)
        {
            var team = Team.TeamManager.InstanceRef;
            if (team == null) return;
            foreach (var h in team.Heroes)
            {
                if (h != null && h.Data != null && h.Data.heroId == heroId)
                {
                    ApplyToHero(h);
                    break;
                }
            }
        }

        // ================= stat application =================

        /// <summary>Re-applies the full equipped loadout of this hero (idempotent).</summary>
        public static void ApplyToHero(HeroCharacter hero)
        {
            if (hero == null || hero.Data == null || hero.Stats == null) return;

            RemoveApplied(hero);
            RemoveVisuals(hero);

            var applied = new List<StatBonus>();

            for (int s = 0; s < SlotCount; s++)
            {
                var item = ItemDatabase.Get(GetEquippedId(hero.Data.heroId, s));
                if (item == null) continue;

                var bonus = item.ToStatBonus();
            float forgeMult = Core.ForgeSystem.StatMult(item.itemId);
            bonus.hp *= forgeMult;
            bonus.attack *= forgeMult;
            bonus.defense *= forgeMult;
            bonus.moveSpeed *= forgeMult;
                hero.Stats.AddEquipmentBonus(bonus);
                applied.Add(bonus);

                if (s == SlotWings) AttachWings(hero, item);
                if (s == SlotPet) AttachPet(hero, item);
            }

            if (applied.Count > 0)
            {
                var marker = hero.gameObject.AddComponent<AppliedBonusMarker>();
                marker.Bonuses = applied;
            }
        }

        private static void RemoveApplied(HeroCharacter hero)
        {
            var marker = hero.gameObject.GetComponent<AppliedBonusMarker>();
            if (marker == null) return;

            foreach (var b in marker.Bonuses)
            {
                hero.Stats.EquipmentBonuses.Remove(b);
            }
            hero.Stats.Recalculate();
            var hp = hero.HP;
            if (hp != null) hp.Max = hero.Stats.MaxHP;

            Object.Destroy(marker);
        }

        private class AppliedBonusMarker : MonoBehaviour
        {
            public List<StatBonus> Bonuses = new List<StatBonus>();
        }

        // ================= loot =================

        /// <summary>
        /// Rolls a floor-clear reward. Rarity odds shift with floor & difficulty.
        /// Returns the display name when something dropped, otherwise null.
        /// </summary>
        public static string RollFloorReward(int floor, int difficulty)
        {
            float dropChance = 0.30f + 0.03f * difficulty;
            if (Random.value > dropChance) return null;

            // rarity weights, tilted upward by progress
            float progress = floor / 50f + difficulty * 0.5f;
            float[] w =
            {
                60f,
                25f * (1f + progress),
                10f * (1f + progress * 0.9f),
                4f * (1f + progress * 0.8f),
                1f * (1f + progress * 0.6f)
            };
            int rarity = PickWeighted(w);

            int cat = Random.Range(0, SlotCount);
            string id;
            switch (cat)
            {
                case SlotWeapon: id = "wpn_" + Random.Range(0, 8) + "_r" + rarity; break;
                case SlotArmor: id = "arm_" + Random.Range(0, 8) + "_r" + rarity; break;
                case SlotAmulet: id = "amu_" + Random.Range(0, 8) + "_r" + rarity; break;
                case SlotWings: id = "wing_" + Random.Range(0, 8) + "_r" + rarity; break;
                case SlotPet: id = "pet_" + Random.Range(0, PetElementsCount()) + "_r" + rarity; break;
                default: id = "mnt_" + Random.Range(0, 8) + "_r" + rarity; break;
            }

            var item = ItemDatabase.Get(id);
            if (item == null) return null;

            Grant(id);
            return item.displayName;
        }

        private static int PetElementsCount() { return 7; }

        private static int PickWeighted(float[] weights)
        {
            float total = 0f;
            foreach (var w in weights) total += w;
            float roll = Random.value * total;
            for (int i = 0; i < weights.Length; i++)
            {
                roll -= weights[i];
                if (roll <= 0f) return i;
            }
            return 0;
        }

        // ================= visuals =================

        private static void AttachWings(HeroCharacter hero, ItemData item)
        {
            var wing = hero.gameObject.AddComponent<WingVisual>();
            wing.Build(item.element == Combat.ElementType.None
                ? new Color(0.9f, 0.9f, 0.95f)
                : ElementalMatrix.GetElementColor(item.element));
        }

        private static void AttachPet(HeroCharacter hero, ItemData item)
        {
            var pet = hero.gameObject.AddComponent<PetOrb>();
            pet.Build(item.element == Combat.ElementType.None
                ? new Color(0.9f, 0.9f, 0.95f)
                : ElementalMatrix.GetElementColor(item.element));
        }

        private static void RemoveVisuals(HeroCharacter hero)
        {
            var wing = hero.gameObject.GetComponent<WingVisual>();
            if (wing != null) Object.Destroy(wing);
            var pet = hero.gameObject.GetComponent<PetOrb>();
            if (pet != null) Object.Destroy(pet);
        }

        // ================= persistence =================

        public static void ReadFrom(SaveSystem.SaveData data)
        {
            if (data == null) return;

            if (data.ownedItemIds != null)
            {
                foreach (var id in data.ownedItemIds)
                {
                    if (!string.IsNullOrEmpty(id)) _owned.Add(id);
                }
            }

            if (data.equipped != null)
            {
                foreach (var rec in data.equipped)
                {
                    if (rec == null || string.IsNullOrEmpty(rec.heroId)) continue;
                    if (rec.slot < 0 || rec.slot >= SlotCount) continue;
                    if (string.IsNullOrEmpty(rec.itemId) || !IsOwned(rec.itemId)) continue;

                    Dictionary<int, string> slots;
                    if (!_equipped.TryGetValue(rec.heroId, out slots))
                    {
                        slots = new Dictionary<int, string>();
                        _equipped[rec.heroId] = slots;
                    }
                    slots[rec.slot] = rec.itemId;
                }
            }
        }

        public static void WriteInto(SaveSystem.SaveData data)
        {
            if (data == null) return;

            data.ownedItemIds = new List<string>(_owned);

            data.equipped = new List<SaveSystem.EquippedItemRecord>();
            foreach (var heroKv in _equipped)
            {
                foreach (var slotKv in heroKv.Value)
                {
                    data.equipped.Add(new SaveSystem.EquippedItemRecord
                    {
                        heroId = heroKv.Key,
                        slot = slotKv.Key,
                        itemId = slotKv.Value
                    });
                }
            }
        }
    }
}
