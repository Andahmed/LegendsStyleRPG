#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LegendsRPG.EditorTools
{
    /// <summary>
    /// Automated project bootstrap.
    /// - On first open: creates Assets/Scenes/Boot.unity containing the GameRoot component
    ///   and registers it in Build Settings (no manual scene setup required).
    /// - Provides menu commands: recreate scene, verify structure, build Android APK.
    /// </summary>
    public static class ProjectBootstrapEditor
    {
        public const string BootScenePath = "Assets/Scenes/Boot.unity";

        [InitializeOnLoadMethod]
        private static void OnProjectLoaded()
        {
            EnsureBootScene();
        }

        [MenuItem("Legends RPG/Setup/Create Boot Scene", priority = 0)]
        private static void CreateBootSceneMenu()
        {
            EnsureBootScene();
            Debug.Log("[LegendsRPG] Boot scene ready at " + BootScenePath);
        }

        /// <summary>
        /// Creates the Boot scene if it is missing and makes sure it is in the
        /// build settings. Safe in batchmode (used by BuildTools.PreExport).
        /// </summary>
        public static void EnsureBootScene()
        {
            if (!File.Exists(BootScenePath))
            {
                CreateBootScene();
            }
            EnsureSceneInBuildSettings();
        }

        private static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("GameRoot");
            root.AddComponent<LegendsRPG.Core.GameRoot>();

            var lightGo = new GameObject("FallbackDirectionalLight");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.05f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, BootScenePath);
            EnsureSceneInBuildSettings();
        }

        public static void EnsureSceneInBuildSettings()
        {
            if (!File.Exists(BootScenePath)) return;

            var current = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = current.Exists(s => s.path == BootScenePath);
            if (!found)
            {
                current.Insert(0, new EditorBuildSettingsScene(BootScenePath, true));
                EditorBuildSettings.scenes = current.ToArray();
            }
        }

        [MenuItem("Legends RPG/Setup/Verify Project Structure", priority = 1)]
        private static void VerifyStructure()
        {
            string[] required =
            {
                "Assets/Scripts/Core", "Assets/Scripts/Characters", "Assets/Scripts/Combat",
                "Assets/Scripts/Team", "Assets/Scripts/World", "Assets/Scripts/VFX",
                "Assets/Scripts/UI", "Assets/Scripts/Audio", "Assets/Scenes"
            };
            foreach (var path in required)
            {
                if (!Directory.Exists(path))
                {
                    Debug.LogError("[LegendsRPG] Missing folder: " + path);
                    return;
                }
            }
            Debug.Log("[LegendsRPG] Project structure OK — all required folders present.");
        }

        [MenuItem("Legends RPG/Build/Build Android APK", priority = 20)]
        private static void BuildAndroidApk()
        {
            // v1.2: delegate to BuildTools so the menu path and the CI path are identical
            BuildTools.BuildAndroid();
        }
    }
}
#endif
