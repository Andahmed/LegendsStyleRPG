#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LegendsRPG.EditorTools
{
    /// <summary>
    /// Unity Cloud Build applies its dashboard Player Settings after the project
    /// has loaded. Enforce the native Android architectures at the last build
    /// callback so a stale Cloud setting cannot silently produce a 32-bit-only APK.
    /// </summary>
    public sealed class AndroidArchitectureBuildEnforcer : IPreprocessBuildWithReport
    {
        // Run after ordinary project preprocessors.
        public int callbackOrder => 10000;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

            PlayerSettings.Android.targetArchitectures =
                AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

            var backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android);
            var architectures = PlayerSettings.Android.targetArchitectures;

            if (backend != ScriptingImplementation.IL2CPP ||
                (architectures & AndroidArchitecture.ARM64) == 0)
            {
                throw new BuildFailedException(
                    "[LegendsRPG] Android release must use IL2CPP and include ARM64.");
            }

            Debug.Log("[LegendsRPG] Android build settings enforced: " +
                      "IL2CPP, ARMv7 + ARM64. 64-bit native support is enabled.");
        }
    }
}
#endif
