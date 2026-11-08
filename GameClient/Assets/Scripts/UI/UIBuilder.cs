using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LegendsRPG.UI
{
    /// <summary>
    /// Code-first uGUI construction helpers (landscape 1920x1080 canvas).
    /// Uses the built-in runtime font — the Arabic localization pass will
    /// switch to TextMeshPro with an Arabic font + reshaper in a later phase.
    /// </summary>
    public static class UIBuilder
    {
        private static Font _font;

        public static Font BuiltinFont
        {
            get
            {
                if (_font == null)
                {
                    try { _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
                    catch { _font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                }
                return _font;
            }
        }

        private static Sprite _solid, _rounded;

        public static Sprite SolidSprite
        {
            get
            {
                if (_solid != null) return _solid;
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                var c = Color.white;
                for (int y = 0; y < 4; y++)
                    for (int x = 0; x < 4; x++)
                        tex.SetPixel(x, y, c);
                tex.Apply();
                _solid = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                return _solid;
            }
        }

        /// <summary>9-sliced rounded rectangle with a soft light top edge — used for buttons and panels.</summary>
        public static Sprite RoundedSprite
        {
            get
            {
                if (_rounded != null) return _rounded;
                int n = 64, r = 20;
                var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                for (int y = 0; y < n; y++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        float cx = Mathf.Clamp(x, r, n - 1 - r), cy = Mathf.Clamp(y, r, n - 1 - r);
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        float a = Mathf.Clamp01(r - d + 0.5f);
                        float shade = Mathf.Lerp(0.82f, 1f, y / (float)n);    // lighter top, darker bottom
                        tex.SetPixel(x, y, new Color(shade, shade, shade, a));
                    }
                }
                tex.Apply();
                _rounded = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f, 0,
                    SpriteMeshType.FullRect, new Vector4(r, r, r, r));
                return _rounded;
            }
        }

        public static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
            return canvas;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var img = go.AddComponent<Image>();
            img.sprite = RoundedSprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
            return rt;
        }

        public static RectTransform CreateFreeRect(Transform parent, string name, Vector2 size,
            Vector2 anchor, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = size;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = anchoredPos;
            return rt;
        }

        public static Text CreateText(Transform parent, string name, string content, int size,
            Color color, TextAnchor alignment, Vector2 anchor, Vector2 anchoredPos,
            Vector2 sizeDelta)
        {
            var rt = CreateFreeRect(parent, name, sizeDelta, anchor, anchoredPos);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = BuiltinFont;
            t.text = content;
            t.fontSize = size;
            t.color = color;
            t.alignment = alignment;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.fontStyle = FontStyle.Bold;
            var ol = rt.gameObject.AddComponent<Outline>();     // readable on any background
            ol.effectColor = new Color(0f, 0f, 0f, 0.8f);
            ol.effectDistance = new Vector2(2f, -2f);
            return t;
        }

        public static Button CreateButton(Transform parent, string name, string label, Vector2 size,
            Vector2 anchor, Vector2 anchoredPos, Color bgColor, Color textColor,
            int fontSize, UnityAction onClick)
        {
            var rt = CreateFreeRect(parent, name, size, anchor, anchoredPos);

            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = RoundedSprite;
            img.type = Image.Type.Sliced;
            img.color = bgColor;
            img.raycastTarget = true;
            var shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -6f);

            var btn = rt.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r * 1.2f, bgColor.g * 1.2f, bgColor.b * 1.2f);
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(onClick);

            var t = CreateText(rt, "Label", label, fontSize, textColor, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, size);
            t.rectTransform.offsetMin = Vector2.zero;
            t.rectTransform.offsetMax = Vector2.zero;
            return btn;
        }
    }
}
