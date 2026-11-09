using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegendsRPG.UI
{
    /// <summary>
    /// Optional art skin for the menu. Sprites are looked up with Resources.Load by exact path
    /// (no mass loading), so a large art library inside Resources does not slow the game down.
    /// Nothing found -> the code-drawn look is used, so nothing can break.
    ///
    /// Two ways to supply art:
    ///  1) the explicit Asset Store paths below, or
    ///  2) drop a PNG directly into Resources/UISkin/ named  button / panel / panel_inset
    ///     (or Kenney names: buttonLong_brown, panel_brown, panelInset_brown ...).
    /// </summary>
    public static class UISkin
    {
        public static bool UseSkin = true;

        private const string GP = "UISkin/AssetStore/GUIParts/GUI_Parts/Gui_parts/";
        private const string CC = "UISkin/AssetStore/CuteCartoonMobileGUI/HONETi/mobile_cartoon_GUI/GUI Elements/";

        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        /// <summary>Resources.Load with caching (null results are cached too).</summary>
        private static Sprite LoadCached(string path)
        {
            Sprite s;
            if (_cache.TryGetValue(path, out s)) return s;
            s = Resources.Load<Sprite>(path);
            _cache[path] = s;
            return s;
        }

        private static Sprite LoadPath(params string[] paths)
        {
            if (!UseSkin) return null;
            for (int i = 0; i < paths.Length; i++)
            {
                var s = LoadCached(paths[i]);
                if (s != null) return s;
            }
            return null;
        }

        /// <summary>Looks for UISkin/&lt;name&gt; (top level of the folder only).</summary>
        public static Sprite Find(params string[] names)
        {
            if (!UseSkin) return null;
            for (int i = 0; i < names.Length; i++)
            {
                var s = LoadCached("UISkin/" + names[i]);
                if (s != null) return s;
            }
            return null;
        }

        public static Sprite Button
        {
            get
            {
                return LoadPath(GP + "button", GP + "button_ready_on", GP + "button_agree")
                    ?? Find("button", "buttonLong_brown", "buttonLong_beige", "buttonLong_blue", "buttonSquare_brown");
            }
        }

        public static Sprite Panel
        {
            get
            {
                return LoadPath(GP + "big_roundframe", GP + "Frame_big", CC + "Panels/panel_start")
                    ?? Find("panel", "panel_brown", "panel_beige", "panel_blue");
            }
        }

        public static Sprite PanelInset
        {
            get
            {
                return LoadPath(GP + "lil_roundframe", GP + "Mini_frame0", CC + "Panels/panel_top")
                    ?? Find("panel_inset", "panelInset_brown", "panelInset_beige", "panelInset_blue");
            }
        }

        /// <summary>Assigns a skin sprite as a 9-slice and keeps its borders at a sensible on-screen size.</summary>
        public static void Apply(Image img, Sprite sprite, float targetBorderPx = 26f)
        {
            // Missing optional art should leave the existing UI unchanged.
            if (img == null || sprite == null) return;

            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            float b = Mathf.Max(sprite.border.x, sprite.border.w);
            // large source art would otherwise draw huge corners
            img.pixelsPerUnitMultiplier = b > targetBorderPx ? b / targetBorderPx : 1f;
        }
    }
}
