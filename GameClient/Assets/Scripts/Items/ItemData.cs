using UnityEngine;
using LegendsRPG.Characters;

namespace LegendsRPG.Items
{
    public enum ItemCategory { Weapon, Armor, Amulet, Wings, Pet, Mount }

    /// <summary>
    /// One equipment piece: weapon / armor / amulet / wings / pet / mount.
    /// Created at runtime by ItemDatabase (formula-driven, like heroes).
    /// Rarity uses the same 5 tiers as heroes: Common, Rare, Epic, Legendary, Mythic.
    /// </summary>
    public class ItemData
    {
        public string itemId = "item";
        public string displayName = "Item";
        public ItemCategory category = ItemCategory.Weapon;
        public Rarity rarity = Rarity.Common;
        public Combat.ElementType element = Combat.ElementType.None;

        public float bonusHp;
        public float bonusAttack;
        public float bonusDefense;
        public float bonusMoveSpeed;

        public int RarityIndex { get { return (int)rarity; } }
        public string RarityName
        {
            get
            {
                switch (rarity)
                {
                    case Rarity.Rare: return "Rare";
                    case Rarity.Epic: return "Epic";
                    case Rarity.Legendary: return "Legendary";
                    case Rarity.Mythic: return "Mythic";
                    default: return "Common";
                }
            }
        }

        public StatBonus ToStatBonus()
        {
            return new StatBonus
            {
                hp = bonusHp,
                attack = bonusAttack,
                defense = bonusDefense,
                moveSpeed = bonusMoveSpeed
            };
        }

        public string StatSummary()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (bonusHp > 0f) parts.Add("HP +" + Mathf.RoundToInt(bonusHp));
            if (bonusAttack > 0f) parts.Add("ATK +" + bonusAttack.ToString("0.#"));
            if (bonusDefense > 0f) parts.Add("DEF +" + bonusDefense.ToString("0.#"));
            if (bonusMoveSpeed > 0f) parts.Add("SPD +" + bonusMoveSpeed.ToString("0.##"));
            return parts.Count > 0 ? string.Join(", ", parts.ToArray()) : "No stats";
        }
    }
}
