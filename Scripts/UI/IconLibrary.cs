using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Characters;
using LegendsRPG.Combat;

namespace LegendsRPG.UI
{
    /// <summary>
    /// Central loader for the free-asset icon set (game-icons.net, CC-BY 3.0 —
    /// see Assets/Art/CREDITS.md). Icons live under "Assets/Resources/Art/UI/Icons"
    /// (inside a Resources folder so they survive into the built APK) and are
    /// loaded with Resources.Load + a cache that also caches negative results,
    /// so a missing icon costs exactly one lookup per key.
    ///
    /// Every sprite is a white glyph on transparent background: tint it with
    /// Image.color to match the element / skill / rarity color.
    /// If the icon file is absent (e.g. folder removed) every getter falls back
    /// to a safe non-null sprite or null-safe UI behavior — the UI never breaks.
    /// </summary>
    public static class IconLibrary
    {
        private const string Root = "Art/UI/Icons/";

        private static readonly Dictionary<string, Sprite> _cache =
            new Dictionary<string, Sprite>();

        /// <summary>Load icon by key (file name without extension). Cached incl. null.</summary>
        public static Sprite Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (_cache.TryGetValue(key, out Sprite sp)) return sp;
            sp = Resources.Load<Sprite>(Root + key);
            _cache[key] = sp;
            return sp;
        }

        /// <summary>Assign a sprite to an existing Image if the icon exists.
        /// Keeps the Image's current tint (icons are white glyphs, tint-friendly).
        /// Returns true when a real icon was applied.</summary>
        public static bool TrySet(Image img, string key)
        {
            if (img == null) return false;
            Sprite sp = Get(key);
            if (sp == null) return false;
            img.sprite = sp;
            img.type = Image.Type.Simple;
            return true;
        }

        /// <summary>Element badge icon ("element_fire", "element_water", ...).</summary>
        public static Sprite ElementIcon(ElementType element)
        {
            return Get("element_" + element.ToString().ToLowerInvariant());
        }

        /// <summary>Class icon ("class_warrior", "class_mage", ...).</summary>
        public static Sprite ClassIcon(HeroClass cls)
        {
            return Get("class_" + cls.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Skill icon: prefers the skill's vfxKey ("slash", "fireball", ...),
        /// then a per-kind fallback ("skill_melee", "skill_projectile", ...).
        /// </summary>
        public static Sprite SkillIcon(SkillData skill)
        {
            if (skill == null) return null;
            Sprite sp = Get(skill.vfxKey);
            if (sp != null) return sp;
            switch (skill.kind)
            {
                case SkillKind.MeleeStrike: return Get("skill_melee");
                case SkillKind.Projectile: return Get("skill_projectile");
                case SkillKind.AoEBurst: return Get("skill_aoe");
                case SkillKind.HealAlly: return Get("skill_heal");
                case SkillKind.UltimateNuke: return Get("skill_ult_nuke");
                case SkillKind.UltimateAoe: return Get("skill_ult_aoe");
                default: return Get("ultimate");
            }
        }

        /// <summary>
        /// Hero portrait art: "hero_&lt;first-word-of-id&gt;" (e.g. hero_kael).
        /// The 20 handcrafted heroes have dedicated icons; generated heroes fall
        /// back to their element icon, then class icon.
        /// </summary>
        public static Sprite HeroIcon(HeroData hero)
        {
            if (hero == null) return null;
            string id = hero.heroId ?? "";
            int cut = id.IndexOf('_');
            string first = cut > 0 ? id.Substring(0, cut) : id;
            Sprite sp = Get("hero_" + first);
            if (sp != null) return sp;
            sp = ElementIcon(hero.element);
            if (sp != null) return sp;
            return ClassIcon(hero.heroClass);
        }
    }
}
