using System.IO;
using UnityEditor;
using UnityEngine;

namespace LegendsRPG.EditorTools
{
    /// <summary>
    /// Keeps images under Resources/UISkin usable as UI sprites:
    ///  - brand-new textures (no .meta yet) are imported as Sprite, no mipmaps;
    ///  - frames / panels / buttons without a 9-slice border get one automatically
    ///    (20% of the smaller side, clamped), so they stretch cleanly. Borders you set yourself are never touched;
    ///  - textures that are already configured (e.g. multi-sprite sheets) keep their settings.
    /// </summary>
    public class UISkinImporter : AssetPostprocessor
    {
        // bump to force a re-import of existing textures after changing the rules
        public override uint GetVersion() { return 3; }

        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.Contains("/Resources/UISkin/")) return;

            var ti = (TextureImporter)assetImporter;
            if (assetImporter.importSettingsMissing || ti.textureType != TextureImporterType.Sprite)
            {
                if (ti.textureType != TextureImporterType.Sprite || ti.spriteImportMode == SpriteImportMode.None)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                }
                ti.mipmapEnabled = false;
                ti.alphaIsTransparency = true;
                ti.filterMode = FilterMode.Bilinear;
                ti.maxTextureSize = 2048;
            }

            if (ti.spriteImportMode != SpriteImportMode.Single) return;
            if (ti.spriteBorder != Vector4.zero) return;      // authored borders win

            string n = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            bool sliceable = n.Contains("frame") || n.Contains("panel") || n.Contains("button") || n.StartsWith("btn");
            if (!sliceable) return;

            int w, h;
            if (!TryReadPngSize(path, out w, out h)) return;
            int b = Mathf.Clamp(Mathf.RoundToInt(Mathf.Min(w, h) * 0.2f), 6, 200);
            ti.spriteBorder = new Vector4(b, b, b, b);
        }

        private static bool TryReadPngSize(string assetPath, out int w, out int h)
        {
            w = h = 0;
            try
            {
                if (!assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) return false;
                using (var fs = File.OpenRead(assetPath))
                {
                    var d = new byte[24];
                    if (fs.Read(d, 0, 24) < 24) return false;
                    w = (d[16] << 24) | (d[17] << 16) | (d[18] << 8) | d[19];
                    h = (d[20] << 24) | (d[21] << 16) | (d[22] << 8) | d[23];
                }
                return w > 0 && h > 0;
            }
            catch { return false; }
        }
    }
}
