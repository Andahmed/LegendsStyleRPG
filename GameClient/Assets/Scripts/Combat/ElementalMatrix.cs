using UnityEngine;
using LegendsRPG.Core;

namespace LegendsRPG.Combat
{
    /// <summary>
    /// Elemental strengths &amp; weaknesses (7 elements from the design doc):
    /// Fire &gt; Ice &gt; Nature &gt; Water &gt; Fire (cycle),
    /// Lightning &gt; Water, Nature &gt; Lightning,
    /// Light &lt;-&gt; Dark (mutual advantage).
    /// Everything else is neutral 1.0.
    /// </summary>
    public static class ElementalMatrix
    {
        public static float GetMultiplier(ElementType attack, ElementType defend)
        {
            if (attack == ElementType.None || defend == ElementType.None) return 1f;
            if (attack == defend) return 1f;

            switch (attack)
            {
                case ElementType.Fire:
                    if (defend == ElementType.Ice || defend == ElementType.Nature) return GameConfig.StrongMultiplier;
                    if (defend == ElementType.Water) return GameConfig.WeakMultiplier;
                    break;

                case ElementType.Ice:
                    if (defend == ElementType.Nature || defend == ElementType.Water) return GameConfig.StrongMultiplier;
                    if (defend == ElementType.Fire) return GameConfig.WeakMultiplier;
                    break;

                case ElementType.Nature:
                    if (defend == ElementType.Water || defend == ElementType.Lightning) return GameConfig.StrongMultiplier;
                    if (defend == ElementType.Ice) return GameConfig.WeakMultiplier;
                    break;

                case ElementType.Water:
                    if (defend == ElementType.Fire) return GameConfig.StrongMultiplier;
                    if (defend == ElementType.Nature || defend == ElementType.Ice) return GameConfig.WeakMultiplier;
                    break;

                case ElementType.Lightning:
                    if (defend == ElementType.Water) return GameConfig.StrongMultiplier;
                    if (defend == ElementType.Nature) return GameConfig.WeakMultiplier;
                    break;

                case ElementType.Dark:
                case ElementType.Light:
                    if (defend == ElementType.Dark || defend == ElementType.Light) return GameConfig.StrongMultiplier;
                    break;
            }
            return 1f;
        }

        public static Color GetElementColor(ElementType el)
        {
            switch (el)
            {
                case ElementType.Fire: return new Color(1.00f, 0.45f, 0.15f);
                case ElementType.Water: return new Color(0.20f, 0.55f, 1.00f);
                case ElementType.Ice: return new Color(0.45f, 0.90f, 1.00f);
                case ElementType.Lightning: return new Color(1.00f, 0.90f, 0.25f);
                case ElementType.Nature: return new Color(0.30f, 0.90f, 0.35f);
                case ElementType.Dark: return new Color(0.65f, 0.30f, 0.95f);
                case ElementType.Light: return new Color(1.00f, 0.95f, 0.75f);
                default: return new Color(0.75f, 0.75f, 0.78f);
            }
        }

        public static string GetElementName(ElementType el)
        {
            switch (el)
            {
                case ElementType.Fire: return "Fire";
                case ElementType.Water: return "Water";
                case ElementType.Ice: return "Ice";
                case ElementType.Lightning: return "Lightning";
                case ElementType.Nature: return "Nature";
                case ElementType.Dark: return "Dark";
                case ElementType.Light: return "Light";
                default: return "Physical";
            }
        }
    }
}
