using UnityEditor;

namespace LegendsRPG.Editor
{
    /// <summary>Consistent import defaults for the bundled Quaternius FBX assets.</summary>
    public sealed class QuaterniusFbxImporter : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            if (assetPath.IndexOf("/Resources/GameArt/Free3D/Quaternius_", System.StringComparison.OrdinalIgnoreCase) < 0)
                return;

            var importer = (ModelImporter)assetImporter;
            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.isReadable = false;
        }
    }
}
