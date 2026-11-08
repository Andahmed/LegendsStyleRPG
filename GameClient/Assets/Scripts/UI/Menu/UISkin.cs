using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LegendsRPG.UI
{
    /// <summary>
    /// Optional art skin for the menu. Drop PNGs (any sub-folder) into
    /// Assets/Resources/UISkin/ and the menu uses them automatically.
    /// No file found -> the code-drawn look is used, so nothing can break.
    /// Sprites are found by file name; Kenney "UI pack RPG extension" names work as-is.
    /// </summary>
    public static class UISkin
    {
        public static bool UseSkin = true;

        private static Dictionary<string, Sprite> _all;

        private static void Load()
        {
            if (_all != null) return;
            _all = new Dictionary<string, Sprite>();
            try
            {
                var arr = Resources.LoadAll<Sprite>("UISkin");
                foreach (var s in arr)
                {
                    if (s == null) continue;
                    if (!_all.ContainsKey(s.name)) _all[s.name] = s;
                    // "panel_brown_b16" is also reachable as "panel_brown"
                    string plain = Regex.Replace(s.name, @"_b\d+$", "");
                    if (!_all.ContainsKey(plain)) _all[plain] = s;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[UISkin] load failed: " + e.Message);
            }
        }

        public static Sprite Find(params string[] names)
        {
            if (!UseSkin) return null;
            Load();
            Sprite s;
            for (int i = 0; i < names.Length; i++)
                if (_all.TryGetValue(names[i], out s)) return s;
            return null;
        }

        private static Sprite LoadPath(params string[] paths)
        {
            if (!UseSkin) return null;
            for (int i = 0; i < paths.Length; i++)
            {
                var sprite = Resources.Load<Sprite>(paths[i]);
                if (sprite != null) return sprite;
            }
            return null;
        }

        // slot -> candidate file names (first match wins)
        public static Sprite Button
        {
            get
            {
                return LoadPath(
                    "UISkin/AssetStore/GUIParts/GUI_Parts/Gui_parts/button",
                    "UISkin/AssetStore/GUIParts/GUI_Parts/Gui_parts/button_ready_on",
                    "UISkin/AssetStore/GUIParts/GUI_Parts/Gui_parts/button_agree")
                    ?? Find("button", "buttonLong_brown", "buttonLong_beige", "buttonLong_blue", "buttonSquare_brown");
            }
        }

        public static Sprite Panel
        {
            get
            {
                return LoadPath(
                    "UISkin/AssetStore/GUIParts/GUI_Parts/Gui_parts/big_roundframe",
                    "UISkin/AssetStore/GUIParts/GUI_Parts/Gui_parts/Frame_big",
                    "UISkin/AssetStore/CuteCartoonMobileGUI/HONETi/mobile_cartoon_GUI/GUI Elements/Panels/panel_start")
                    ?? Find("panel", "panel_brown", "panel_beige", "panel_blue");
            }
        }

        public static Sprite PanelInset
        {
            get
            {
                return LoadPath(
                    "UISkin/AssetStore/GUIParts/GUI_Parts/Gui_parts/lil_roundframe",
                    "UISkin/AssetStore/GUIParts/GUI_Parts/Gui_parts/Mini_frame0",
                    "UISkin/AssetStore/CuteCartoonMobileGUI/HONETi/mobile_cartoon_GUI/GUI Elements/Panels/panel_top")
                    ?? Find("panel_inset", "panelInset_brown", "panelInset_beige", "panelInset_blue");
            }
        }
    }
}
