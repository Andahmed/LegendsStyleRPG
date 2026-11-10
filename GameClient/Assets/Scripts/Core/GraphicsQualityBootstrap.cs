// GraphicsQualityBootstrap.cs
// Runs automatically at startup (no scene setup needed).
//
// Phone-side fixes that complement GameRoot:
//   1. Screen never sleeps (also applied here before the splash screen).
//   2. Locks 60 FPS (Unity's mobile default is 30).
//   3. Disables the GPU-heavy defaults that make phone "Game Booster / Game
//      Turbo" show the "game quality is higher than this device" warning
//      (soft shadows, MSAA, realtime reflection probes).
//   4. Safety camera: if no enabled camera exists after the first scene
//      loads, creates a fallback so the screen is never black. Removes
//      itself once GameRoot's BattleCamera is alive.

using UnityEngine;

namespace LegendsRPG.Core
{
    public static class GraphicsQualityBootstrap
    {
        private static bool _applied;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Bootstrap()
        {
            if (_applied) return;
            _applied = true;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            int ramMb = SystemInfo.systemMemorySize;
            int fps = (ramMb > 0 && ramMb < 3000) ? 30 : 60;
            Application.targetFrameRate = fps;
            QualitySettings.vSyncCount = 0;

            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            QualitySettings.antiAliasing = 0;
            QualitySettings.realtimeReflectionProbes = false;

            Application.lowMemory += OnLowMemory;

            Debug.Log("[LegendsRPG] Graphics bootstrap: api=" + SystemInfo.graphicsDeviceType +
                      " gpu='" + SystemInfo.graphicsDeviceName + "'" +
                      " ram=" + ramMb + "MB fps=" + fps +
                      " shadows=off aa=off");
        }

        private static void OnLowMemory()
        {
            QualitySettings.globalTextureMipmapLimit = 1; // half-resolution textures
            Debug.LogWarning("[LegendsRPG] Low memory: reduced texture quality.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CameraSafetyNet()
        {
            // GameRoot creates BattleCamera in Awake, so normally this is a no-op.
            if (Object.FindObjectOfType<Camera>() != null) return;

            var go = new GameObject("FallbackSafetyCamera");
            var cam = go.AddComponent<Camera>();
            cam.tag = "Untagged"; // do not steal Camera.main from GameRoot
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.043f, 0.055f, 0.102f); // dark navy
            cam.transform.position = new Vector3(0f, 12f, -12f);
            cam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            cam.fieldOfView = 55f;
            go.AddComponent<SafetyCameraGuard>();

            Debug.LogWarning("[LegendsRPG] No camera in first scene — created FallbackSafetyCamera.");
        }
    }

    /// <summary>
    /// Guards the fallback camera: if GameRoot's real camera appears within
    /// 2 seconds, destroy the fallback. If nothing else appeared, adopt it
    /// as MainCamera so the game is never stuck on a black screen.
    /// </summary>
    public class SafetyCameraGuard : MonoBehaviour
    {
        private float _elapsed;

        private void LateUpdate()
        {
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < 2f) return;

            foreach (var cam in Camera.allCameras)
            {
                if (cam == null || cam.gameObject == gameObject) continue;
                if (cam.isActiveAndEnabled && (cam.CompareTag("MainCamera") || cam.name == "BattleCamera"))
                {
                    Destroy(gameObject);
                    return;
                }
            }

            gameObject.tag = "MainCamera";
            enabled = false; // stop checking
        }
    }
}
