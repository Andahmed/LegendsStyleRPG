#if UNITY_EDITOR
using UnityEditor;

namespace LegendsRPG.EditorTools
{
    /// <summary>
    /// Import rules for UI icons. Kept under GameClient/Assets/Editor so the
    /// Unity Cloud Build project (projectFolder: GameClient) compiles it.
    /// The legacy Art/Textures folder at repository root is outside the Unity
    /// project and is intentionally not treated as an importable asset folder.
    /// </summary>
    public sealed class IconImporter : AssetPostprocessor
    {
        private const string IconsRoot = "Assets/Resources/Art/UI/Icons/";

        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.StartsWith(IconsRoot)) return;

            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.mipmapEnabled = false;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.filterMode = UnityEngine.FilterMode.Bilinear;
            ti.maxTextureSize = 256;
            ti.alphaIsTransparency = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}
#endif
