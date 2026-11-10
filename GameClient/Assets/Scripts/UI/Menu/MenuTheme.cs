using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LegendsRPG.UI
{
    /// <summary>
    /// Original "tavern ledger" look: dark wood, parchment panels, gold trim.
    /// All widgets are generated in code (no sprites needed).
    /// </summary>
    public static class MenuTheme
    {
        public static readonly Color Wood = new Color(0.025f, 0.035f, 0.09f, 1f);
        public static readonly Color WoodLight = new Color(0.10f, 0.13f, 0.28f, 1f);
        public static readonly Color Parchment = new Color(0.90f, 0.94f, 1f, 1f);
        public static readonly Color ParchmentDark = new Color(0.62f, 0.72f, 0.90f, 1f);
        public static readonly Color Gold = new Color(1f, 0.78f, 0.28f, 1f);
        public static readonly Color GoldDark = new Color(0.50f, 0.30f, 0.10f, 1f);
        public static readonly Color Ink = new Color(0.035f, 0.05f, 0.13f, 1f);
        public static readonly Color Slate = new Color(0.10f, 0.13f, 0.28f, 1f);
        public static readonly Color Green = new Color(0.31f, 1f, 0.73f, 1f);
        public static readonly Color Red = new Color(1f, 0.30f, 0.35f, 1f);

        private static Sprite _circle;
        public static Sprite CircleSprite
        {
            get
            {
                if (_circle != null) return _circle;
                int n = 64;
                var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f));
                        float a = Mathf.Clamp01(n / 2f - d);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                    }
                tex.Apply();
                _circle = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
                return _circle;
            }
        }

        // ---------------- basic building blocks ----------------
        public static RectTransform Box(Transform parent, string name, Vector2 aMin, Vector2 aMax,
            Vector2 oMin, Vector2 oMax, Color color, bool rounded = true, bool raycast = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.offsetMin = oMin; rt.offsetMax = oMax;
            var img = go.GetComponent<Image>();
            img.sprite = rounded ? UIBuilder.RoundedSprite : UIBuilder.SolidSprite;
            img.type = rounded ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color;
            img.raycastTarget = raycast;
            return rt;
        }

        public static RectTransform Stretch(Transform parent, string name, Color color, bool rounded = false)
        {
            return Box(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, color, rounded);
        }

        /// <summary>Box positioned by anchor + pivot-centered position + size.</summary>
        public static RectTransform At(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size,
            Color color, bool rounded = true, bool raycast = false)
        {
            var rt = Box(parent, name, anchor, anchor, Vector2.zero, Vector2.zero, color, rounded, raycast);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        public static RectTransform Circle(Transform parent, string name, Vector2 anchor, Vector2 pos, float diameter, Color color)
        {
            var rt = At(parent, name, anchor, pos, new Vector2(diameter, diameter), color, false);
            var img = rt.GetComponent<Image>();
            img.sprite = CircleSprite;
            img.type = Image.Type.Simple;
            return rt;
        }

        /// <summary>Border + fill. Returns the inner (fill) rect.</summary>
        public static RectTransform Framed(Transform parent, string name, Vector2 aMin, Vector2 aMax,
            Vector2 oMin, Vector2 oMax, Color border, Color fill, float inset = 4f)
        {
            var outer = Box(parent, name, aMin, aMax, oMin, oMax, border);
            var innerRt = Box(outer, "Fill", Vector2.zero, Vector2.one, new Vector2(inset, inset), new Vector2(-inset, -inset), fill);
            var panelSkin = UISkin.Panel;
            if (panelSkin != null)
            {
                var oi = outer.GetComponent<Image>();
                UISkin.Apply(oi, panelSkin, 30f);
                oi.color = Color.white;
                var ii = innerRt.GetComponent<Image>();
                var insetSkin = UISkin.PanelInset;
                UISkin.Apply(ii, insetSkin != null ? insetSkin : panelSkin, 18f);
                ii.color = Color.Lerp(Color.white, fill, 0.6f);
            }
            return innerRt;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor align, bool outline = false)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(6f, 2f); rt.offsetMax = new Vector2(-6f, -2f);
            var t = go.GetComponent<Text>();
            t.font = UIBuilder.BuiltinFont;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = FontStyle.Bold;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = Mathf.Max(10, size / 2);
            t.resizeTextMaxSize = size;
            if (outline)
            {
                var ol = go.AddComponent<Outline>();
                ol.effectColor = new Color(0f, 0f, 0f, 0.85f);
                ol.effectDistance = new Vector2(2f, -2f);
            }
            return t;
        }

        public static Button Btn(Transform parent, string name, string label, Vector2 aMin, Vector2 aMax,
            Vector2 oMin, Vector2 oMax, Color fill, Color textColor, int fontSize, UnityAction onClick)
        {
            var outer = Box(parent, name, aMin, aMax, oMin, oMax, GoldDark, true, true);
            var inner = Box(outer, "Fill", Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f), fill);
            var btnSkin = UISkin.Button;
            if (btnSkin != null)
            {
                var oi = outer.GetComponent<Image>();
                UISkin.Apply(oi, btnSkin, 22f);
                oi.color = Color.Lerp(Color.white, fill, 0.5f);
                inner.GetComponent<Image>().enabled = false;
            }
            Label(inner, label, fontSize, textColor, TextAnchor.MiddleCenter);
            var b = outer.gameObject.AddComponent<Button>();
            b.targetGraphic = outer.GetComponent<Image>();
            var c = b.colors;
            c.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            c.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            b.colors = c;
            if (onClick != null) b.onClick.AddListener(onClick);
            return b;
        }

        public static Button BtnAt(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size,
            Color fill, Color textColor, int fontSize, UnityAction onClick)
        {
            var b = Btn(parent, name, label, anchor, anchor, Vector2.zero, Vector2.zero, fill, textColor, fontSize, onClick);
            var rt = (RectTransform)b.transform;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return b;
        }

        // ---------------- scroll containers ----------------
        public static RectTransform ScrollGrid(Transform parent, string name, Vector2 aMin, Vector2 aMax,
            Vector2 oMin, Vector2 oMax, Vector2 cell, Vector2 spacing, int columns)
        {
            var vp = Box(parent, name, aMin, aMax, oMin, oMax, new Color(0f, 0f, 0f, 0.001f), false, true);
            vp.gameObject.AddComponent<RectMask2D>();

            var content = new GameObject("Content", typeof(RectTransform));
            var crt = (RectTransform)content.transform;
            crt.SetParent(vp, false);
            crt.anchorMin = new Vector2(0f, 1f); crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.sizeDelta = Vector2.zero;

            var grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = cell;
            grid.spacing = spacing;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.childAlignment = TextAnchor.UpperCenter;

            var fit = content.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = vp.gameObject.AddComponent<ScrollRect>();
            sr.content = crt;
            sr.viewport = vp;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 30f;
            return crt;
        }

        public static RectTransform ScrollList(Transform parent, string name, Vector2 aMin, Vector2 aMax,
            Vector2 oMin, Vector2 oMax, float spacing)
        {
            var vp = Box(parent, name, aMin, aMax, oMin, oMax, new Color(0f, 0f, 0f, 0.001f), false, true);
            vp.gameObject.AddComponent<RectMask2D>();

            var content = new GameObject("Content", typeof(RectTransform));
            var crt = (RectTransform)content.transform;
            crt.SetParent(vp, false);
            crt.anchorMin = new Vector2(0f, 1f); crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.sizeDelta = Vector2.zero;

            var v = content.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = new RectOffset(8, 8, 8, 8);
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            var fit = content.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = vp.gameObject.AddComponent<ScrollRect>();
            sr.content = crt;
            sr.viewport = vp;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 30f;
            return crt;
        }

        // ---------------- small composites ----------------
        /// <summary>Row of small gold diamonds.</summary>
        public static void Stars(Transform parent, int count, Vector2 anchor, Vector2 pos, float size = 22f)
        {
            float total = (count - 1) * (size * 0.9f);
            for (int i = 0; i < count; i++)
            {
                var s = At(parent, "Star" + i, anchor, pos + new Vector2(-total / 2f + i * size * 0.9f, 0f),
                    new Vector2(size * 0.7f, size * 0.7f), Gold, false);
                s.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
        }

        /// <summary>Item tile: rarity-coloured frame, abbreviation, count.</summary>
        public static RectTransform ItemTile(Transform parent, Core.ItemDef def, int count, float size)
        {
            var outer = Box(parent, "Item_" + def.id, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                Core.ItemDatabase.RarityColor(def.rarity), true, true);
            var inner = Box(outer, "Fill", Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f),
                new Color(0.10f, 0.08f, 0.12f, 1f));
            var gem = Circle(inner, "Icon", new Vector2(0.5f, 0.55f), Vector2.zero, size * 0.55f, def.color);
            Label(gem, def.abbr, Mathf.RoundToInt(size * 0.2f), Color.white, TextAnchor.MiddleCenter, true);
            if (count > 0)
            {
                var cnt = Label(inner, count.ToString(), Mathf.RoundToInt(size * 0.2f), Color.white, TextAnchor.LowerRight, true);
            }
            return outer;
        }

        /// <summary>Round hero portrait (element-coloured) with level badge.</summary>
        public static RectTransform HeroPortrait(Transform parent, Vector2 anchor, Vector2 pos, float diameter,
            Characters.HeroData data, int level)
        {
            Color el = Combat.ElementalMatrix.GetElementColor(data.element);
            var ring = Circle(parent, "Portrait_" + data.displayName, anchor, pos, diameter, AnimeTheme.Rarity(data.rarity));
            var body = Circle(ring, "Body", new Vector2(0.5f, 0.5f), Vector2.zero, diameter * 0.88f,
                Color.Lerp(data.primaryColor, Color.black, 0.45f));
            var glow = Circle(body, "Glow", new Vector2(0.5f, 0.5f), Vector2.zero, diameter * 0.66f, el);
            Label(glow, data.displayName.Substring(0, 1), Mathf.RoundToInt(diameter * 0.42f), Color.white, TextAnchor.MiddleCenter, true);
            AnimeTheme.AddRarityPips(ring, data.rarity, new Vector2(0.5f, 0f), new Vector2(-((int)data.rarity * 9.5f), diameter * 0.10f), 5);
            if (level > 0)
            {
                var badge = Circle(ring, "Lv", new Vector2(1f, 0f), new Vector2(-diameter * 0.12f, diameter * 0.12f), diameter * 0.36f, Color.black);
                Label(badge, level.ToString(), Mathf.RoundToInt(diameter * 0.2f), Color.white, TextAnchor.MiddleCenter);
            }
            return ring;
        }
    }
}
