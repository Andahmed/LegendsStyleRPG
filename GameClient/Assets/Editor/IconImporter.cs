#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LegendsRPG.EditorTools
{
    /// <summary>
    /// Import settings for the free art assets inside the Unity project.
    /// This file must live under GameClient/Assets/Editor so Unity Cloud Build
    /// compiles and applies the rules (the repository-root Editor folder is outside projectFolder).
    /// </summary>
    public sealed class IconImporter : AssetPostprocessor
    {
        private const string IconsRoot = "Assets/Resources/Art/UI/Icons/";
        private const string TexturesRoot = "Assets/Art/Textures/";

        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            var ti = (TextureImporter)assetImporter;

            if (path.StartsWith(IconsRoot))
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.mipmapEnabled = false;
                ti.npotScale = TextureImporterNPOTScale.None;
                ti.filterMode = FilterMode.Bilinear;
                ti.maxTextureSize = 256;
                ti.alphaIsTransparency = true;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
            }
            else if (path.StartsWith(TexturesRoot))
            {
                ti.textureType = TextureImporterType.Default;
                ti.mipmapEnabled = true;
                ti.wrapMode = TextureWrapMode.Repeat;
                ti.filterMode = FilterMode.Bilinear;
                ti.maxTextureSize = 1024;
                ti.textureCompression = TextureImporterCompression.Compressed;
            }
        }
    }
}
#endif
