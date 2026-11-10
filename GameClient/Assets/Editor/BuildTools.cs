#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LegendsRPG.EditorTools
{
    /// <summary>
    /// v1.2: CI-friendly build entry points for Unity Cloud Build (Build
    /// Automation) and local batchmode builds — no editor window needed.
    ///
    /// Unity Cloud Build setup:
    ///   Pre-export method:  LegendsRPG.EditorTools.BuildTools.PreExport
    ///   (Build method optional:) LegendsRPG.EditorTools.BuildTools.BuildAndroid
    ///
    /// Both are safe in batchmode: the Boot scene is created programmatically
    /// (EditorSceneManager) and registered in EditorBuildSettings.
    /// </summary>
    public static class BuildTools
    {
        private const string OutputDir = "Builds/Android";
        private const string ApkName = "LegendsRPG-v1.2.0.apk";

        /// <summary>
        /// Cloud Build "Pre-export method": guarantees the Boot scene exists and
        /// is in EditorBuildSettings, then applies the Android player settings
        /// (identifier com.ahmed.legendsstylerpg, IL2CPP, ARMv7 + ARM64, API 23/34).
        /// </summary>
        public static void PreExport()
        {
            ProjectBootstrapEditor.EnsureBootScene();
            ApplyAndroidPlayerSettings();
        }

        /// <summary>
        /// -executeMethod entry point: pre-export + build an APK. On failure the
        /// reason is printed to the log (Debug.LogError) and the process exits
        /// with code 1 so CI marks the build red.
        /// </summary>
        public static void BuildAndroid()
        {
            PreExport();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                {
                    Debug.LogError("[BuildTools] Could not switch the active build target to Android.");
                    EditorApplication.Exit(1);
                    return;
                }
            }

            string outputPath = OutputDir + "/" + ApkName;
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ProjectBootstrapEditor.BootScenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("[BuildTools] APK built successfully: " + report.summary.outputPath +
                          "  size=" + (report.summary.totalSize / (1024f * 1024f)).ToString("F1") + " MB");
            }
            else
            {
                Debug.LogError("[BuildTools] APK build FAILED: result=" + report.summary.result +
                               "  totalErrors=" + report.summary.totalErrors +
                               " — read the full Unity log above for the first compile error.");
                EditorApplication.Exit(1);
            }
        }

        /// <summary>Android player settings applied for every CI build.</summary>
        public static void ApplyAndroidPlayerSettings()
        {
            PlayerSettings.companyName = "LegendsStyleRPG";
            PlayerSettings.productName = "Legends Reborn Style RPG";
            PlayerSettings.bundleVersion = "1.2.0";
            PlayerSettings.Android.bundleVersionCode = 14;

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.ahmed.legendsstylerpg");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64; // 32-bit + 64-bit device support
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)23;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;

            // Landscape UI is built against a 1920x1080 reference canvas.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
        }
    }
}
#endif
