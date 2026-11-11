#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LegendsRPG.EditorTools
{
    /// <summary>
    /// CI-friendly Android build entry points.
    /// Unity version is pinned by ProjectSettings/ProjectVersion.txt.
    /// </summary>
    public static class BuildTools
    {
        private const string OutputDir = "Builds/Android";
        private const string ApkName = "LegendsRPG-v1.2.0.apk";

        /// <summary>
        /// Cloud Build pre-export hook: creates Boot.unity when needed,
        /// registers it in Build Settings, and applies Android player settings.
        /// </summary>
        public static void PreExport()
        {
            ProjectBootstrapEditor.EnsureBootScene();
            ApplyAndroidPlayerSettings();
        }

        /// <summary>
        /// Local and GitHub Actions build entry point.
        /// A successful report is not accepted unless a non-empty APK exists.
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

            Directory.CreateDirectory(OutputDir);
            string outputPath = Path.Combine(OutputDir, ApkName).Replace("\\", "/");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ProjectBootstrapEditor.BootScenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            bool apkExists = File.Exists(outputPath);
            long apkBytes = apkExists ? new FileInfo(outputPath).Length : 0L;

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded &&
                apkExists && apkBytes > 0L)
            {
                Debug.Log("[BuildTools] APK built successfully: " + outputPath +
                          " size=" + (apkBytes / (1024f * 1024f)).ToString("F1") + " MB");
            }
            else
            {
                Debug.LogError("[BuildTools] APK build FAILED or output missing: result=" +
                               report.summary.result + " totalErrors=" + report.summary.totalErrors +
                               " fileExists=" + apkExists + " fileBytes=" + apkBytes +
                               " — inspect the Unity log for the first compile/build error.");
                EditorApplication.Exit(1);
            }
        }

        /// <summary>Android settings shared by local builds, CI and Cloud Build.</summary>
        public static void ApplyAndroidPlayerSettings()
        {
            PlayerSettings.companyName = "LegendsStyleRPG";
            PlayerSettings.productName = "Legends Reborn Style RPG";
            PlayerSettings.bundleVersion = "1.2.0";
            PlayerSettings.Android.bundleVersionCode = 14;

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.ahmed.legendsstylerpg");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)23;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;

            // Landscape only; allow both landscape orientations.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
        }
    }
}
#endif
