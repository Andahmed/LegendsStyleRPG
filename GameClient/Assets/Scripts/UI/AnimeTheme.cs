using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using LegendsRPG.Characters;
using LegendsRPG.Combat;

namespace LegendsRPG.UI
{
    /// <summary>
    /// Original anime-fantasy presentation tokens and card builders.
    /// This is intentionally data-agnostic: it styles the existing LegendsRPG models
    /// without importing code or assets from external projects.
    /// </summary>
    public static class AnimeTheme
    {
        public static readonly Color Ink = new Color(0.025f, 0.035f, 0.09f, 0.98f);
        public static readonly Color Panel = new Color(0.055f, 0.075f, 0.17f, 0.96f);
        public static readonly Color PanelRaised = new Color(0.10f, 0.13f, 0.28f, 0.98f);
        public static readonly Color PanelSoft = new Color(0.16f, 0.19f, 0.36f, 0.90f);
        public static readonly Color Gold = new Color(1f, 0.78f, 0.28f, 1f);
        public static readonly Color Cyan = new Color(0.25f, 0.86f, 1f, 1f);
        public static readonly Color Pink = new Color(1f, 0.34f, 0.62f, 1f);
        public static readonly Color Mint = new Color(0.31f, 1f, 0.73f, 1f);
        public static readonly Color Danger = new Color(1f, 0.30f, 0.35f, 1f);
        public static readonly Color Text = new Color(0.94f, 0.97f, 1f, 1f);
        public static readonly Color MutedText = new Color(0.62f, 0.70f, 0.86f, 1f);

        public static Color Rarity(Rarity rarity)
        {
            switch (rarity)
            {
                case LegendsRPG.Characters.Rarity.Common: return new Color(0.58f, 0.65f, 0.76f);
                case LegendsRPG.Characters.Rarity.Rare: return new Color(0.26f, 0.68f, 1f);
                case LegendsRPG.Characters.Rarity.Epic: return new Color(0.72f, 0.35f, 1f);
                case LegendsRPG.Characters.Rarity.Legendary: return new Color(1f, 0.55f, 0.18f);
                case LegendsRPG.Characters.Rarity.Mythic: return new Color(1f, 0.25f, 0.55f);
                default: return Gold;
            }
        }

        public static Color Element(ElementType element)
        {
            return Combat.ElementalMatrix.GetElementColor(element);
        }

        public static RectTransform Card(Transform parent, string name, Vector2 anchor, Vector2 pos,
            Vector2 size, Color accent, UnityAction onClick = null)
        {
            var outer = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Shadow));
            var rt = (RectTransform)outer.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchor; rt.anchorMax = anchor;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            var img = outer.GetComponent<Image>();
            img.sprite = UIBuilder.RoundedSprite;
            img.type = Image.Type.Sliced;
            img.color = Panel;
            var shadow = outer.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            shadow.effectDistance = new Vector2(0f, -8f);

            var accentRt = UIBuilder.CreateFreeRect(rt, "Accent", new Vector2(8f, size.y - 18f),
                new Vector2(0f, 0.5f), new Vector2(12f, 0f));
            var accentImg = accentRt.gameObject.AddComponent<Image>();
            accentImg.sprite = UIBuilder.RoundedSprite;
            accentImg.type = Image.Type.Sliced;
            accentImg.color = accent;
            accentImg.raycastTarget = false;

            if (onClick != null)
            {
                var button = outer.AddComponent<Button>();
                button.targetGraphic = img;
                button.onClick.AddListener(onClick);
                var colors = button.colors;
                colors.highlightedColor = Color.Lerp(img.color, Color.white, 0.18f);
                colors.pressedColor = Color.Lerp(img.color, Color.black, 0.18f);
                button.colors = colors;
            }
            return rt;
        }

        public static void AddRarityPips(Transform parent, Rarity rarity, Vector2 anchor, Vector2 pos, int max = 5)
        {
            int count = Mathf.Clamp((int)rarity + 1, 1, max);
            for (int i = 0; i < count; i++)
            {
                var pip = UIBuilder.CreateFreeRect(parent, "RarityPip" + i, new Vector2(14f, 14f),
                    anchor, pos + new Vector2(i * 19f, 0f));
                var image = pip.gameObject.AddComponent<Image>();
                image.sprite = UIBuilder.SolidSprite;
                image.color = Rarity(rarity);
                image.raycastTarget = false;
            }
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color,
            TextAnchor alignment, Vector2 anchor, Vector2 pos, Vector2 dimensions)
        {
            return UIBuilder.CreateText(parent, name, text, size, color, alignment, anchor, pos, dimensions);
        }
    }
}
