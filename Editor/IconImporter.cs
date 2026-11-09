using UnityEditor;

namespace LegendsRPG.EditorTools
{
    /// <summary>
    /// Import rules for the free-asset art folders (no .meta files ship in the
    /// repo — Unity Cloud Build / first Editor import creates them with these
    /// settings automatically).
    ///
    ///  - Assets/Resources/Art/UI/Icons -> single-mode UI sprites (white
    ///    glyphs), no mipmaps, NPOT kept as-is, 256px cap.
    ///  - Assets/Art/Textures  -> tiling ground textures: repeat wrap, no
    ///    mipmaps needed for mobile UI distance, 512px cap.
    /// </summary>
    public sealed class IconImporter : AssetPostprocessor
    {
        private const string IconsRoot = "Assets/Resources/Art/UI/Icons";
        private const string TexturesRoot = "Assets/Art/Textures";

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
                ti.mipmapEnabled = false;
                ti.wrapMode = UnityEngine.TextureWrapMode.Repeat;
                ti.filterMode = UnityEngine.FilterMode.Bilinear;
                ti.maxTextureSize = 512;
                ti.textureCompression = TextureImporterCompression.Compressed;
            }
        }
    }
}
