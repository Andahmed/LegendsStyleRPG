using UnityEngine;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Real graphics quality tiers persisted in the save file.
    /// 0=Low (max FPS, no shadows) 1=Medium 2=High 3=Ultra.
    /// </summary>
    public static class GfxSettings
    {
        public static readonly string[] Names = { "Low", "Medium", "High", "Ultra" };

        public static int Level { get; private set; } = 2;

        public static void ApplyFromSave()
        {
            Apply(Mathf.Clamp(Mathf.RoundToInt(SaveSystem.Current.qualityPref), 0, 3));
        }

        public static void Apply(int level)
        {
            Level = Mathf.Clamp(level, 0, 3);
            SaveSystem.Current.qualityPref = Level;
            PlayerPrefs.SetInt("legends_gfx", Level);

            switch (Level)
            {
                case 0: // Low — battery friendly
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.pixelLightCount = 0;
                    QualitySettings.shadowDistance = 0f;
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 60;
                    RenderSettings.fogDensity *= 1f; // keep fog
                    break;

                case 1: // Medium — hard shadows
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.pixelLightCount = 2;
                    QualitySettings.shadowDistance = 40f;
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 60;
                    break;

                case 2: // High — soft shadows (default)
                    QualitySettings.shadows = ShadowQuality.SoftShadows;
                    QualitySettings.pixelLightCount = 4;
                    QualitySettings.shadowDistance = 70f;
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 60;
                    break;

                default: // Ultra — everything
                    QualitySettings.shadows = ShadowQuality.SoftShadows;
                    QualitySettings.pixelLightCount = 8;
                    QualitySettings.shadowDistance = 110f;
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = 60;
                    break;
            }
        }
    }
}
