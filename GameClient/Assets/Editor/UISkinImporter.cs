using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LegendsRPG.EditorTools
{
    /// <summary>
    /// Any texture placed under Resources/UISkin is imported as a Sprite automatically
    /// (so files uploaded from a phone / without .meta files work in cloud builds).
    /// 9-slice border: add "_b16" to the file name (panel_b16.png) or rely on the default
    /// of 12px for files starting with "panel" or "button".
    /// </summary>
    public class UISkinImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.Contains("/Resources/UISkin/")) return;

            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Bilinear;
            ti.maxTextureSize = 1024;

            string n = Path.GetFileNameWithoutExtension(path);
            int b = 0;
            var m = Regex.Match(n, @"_b(\d+)$");
            if (m.Success) b = int.Parse(m.Groups[1].Value);
            else if (n.StartsWith("panel") || n.StartsWith("button")) b = 12;
            if (b > 0) ti.spriteBorder = new Vector4(b, b, b, b);
        }
    }
}
