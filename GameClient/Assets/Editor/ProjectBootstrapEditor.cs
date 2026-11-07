#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LegendsRPG.EditorTools
{
    public static class ProjectBootstrapEditor
    {
        private const string ScenePath = "Assets/Scenes/Boot.unity";

        [InitializeOnLoadMethod]
        private static void OnProjectLoaded()
        {
            if (!File.Exists(ScenePath)) CreateBootScene();
            EnsureSceneInBuildSettings();
        }

        [MenuItem("Legends RPG/Setup/Create Boot Scene", priority = 0)]
        private static void CreateBootSceneMenu()
        {
            CreateBootScene();
            Debug.Log("[LegendsRPG] Boot scene created at " + ScenePath);
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
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureSceneInBuildSettings();
        }

        private static void EnsureSceneInBuildSettings()
        {
            if (!File.Exists(ScenePath)) return;
            var current = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!current.Exists(s => s.path == ScenePath))
            {
                current.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
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
            Debug.Log("[LegendsRPG] Project structure OK.");
        }

        [MenuItem("Legends RPG/Build/Build Android APK", priority = 20)]
        private static void BuildAndroidApk()
        {
            EnsureSceneInBuildSettings();

            PlayerSettings.companyName = "LegendsStyleRPG";
            PlayerSettings.productName = "Legends Reborn Style RPG";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)23;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Android/LegendsRPG-v0.1.0.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("[LegendsRPG] APK built: " + report.summary.outputPath);
            }
            else
            {
                Debug.LogError("[LegendsRPG] APK build FAILED: " + report.summary.result);
            }
        }
    }
}
#endif
