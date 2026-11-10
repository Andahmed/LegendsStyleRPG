using UnityEngine;

namespace LegendsRPG.Cam
{
    /// <summary>
    /// Isometric / top-down 3/4 view camera in the style of Legends Reborn.
    /// v1.1: follows the TEAM CENTER (a focus transform owned by TeamManager)
    /// — there is no "active hero" to follow and NO finger-drag rotation.
    /// The angle changes only through the HUD camera buttons:
    ///  - CycleAngle(): rotate the fixed isometric view in 60° steps,
    ///  - ResetView(): back to the default angle/zoom.
    /// Two-finger pinch (or mouse wheel) still zooms.
    /// The follow uses Time.unscaledDeltaTime so the camera keeps moving while
    /// the game is paused (Time.timeScale = 0) or during screen transitions,
    /// and SetTarget(t, snap) can jump instantly on scene transitions.
    /// Impact shake is kept for hits and Ultimate skills.
    /// </summary>
    public class IsometricCameraController : MonoBehaviour
    {
        public Transform Target;

        private float _yaw = GameConfig.CameraDefaultYaw;
        private float _pitch = GameConfig.CameraDefaultPitch;
        private float _distance = 13f;
        private float _zoomInput;
        private bool _snapNext;
        private bool _homeLocked;
        private Vector3 _shakeOffset;
        private float _shakeTimeLeft, _shakeMagnitude;
        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = gameObject.AddComponent<Camera>();
            _cam.fieldOfView = 50f;
            _cam.nearClipPlane = 0.3f;
            _cam.farClipPlane = 220f;
            _cam.backgroundColor = new Color(0.05f, 0.06f, 0.10f);
            _cam.clearFlags = CameraClearFlags.SolidColor;
        }

        public void SetTarget(Transform t, bool snap = false)
        {
            Target = t;
            if (snap) _snapNext = true;
        }

        /// <summary>Kept for existing callers (guild hall, battle session): jump to the target.</summary>
        public void SnapToTarget()
        {
            _snapNext = true;
        }

        /// <summary>Locks the home camera to a portrait tabletop composition.</summary>
        public void SetDioramaView(Transform target)
        {
            Target = target;
            _yaw = 0f;
            _pitch = 48f;
            _distance = 22f;
            _zoomInput = 0f;
            _homeLocked = true;
            _snapNext = true;
        }

        /// <summary>Restores the normal battle camera framing.</summary>
        public void SetBattleView(Transform target)
        {
            Target = target;
            _yaw = GameConfig.CameraDefaultYaw;
            _pitch = GameConfig.CameraDefaultPitch;
            _distance = 13f;
            _zoomInput = 0f;
            _homeLocked = false;
            _snapNext = true;
        }

        public void AddZoomInput(float delta)
        {
            if (_homeLocked) return;
            _zoomInput += delta;
        }

        /// <summary>HUD "angle" button: rotates the fixed isometric view by 60° per press.</summary>
        public void CycleAngle()
        {
            if (_homeLocked) return;
            _yaw += 60f;
            Audio.AudioSystem.Play("UI");
        }

        /// <summary>HUD "reset" button: back to the default isometric angle and zoom.</summary>
        public void ResetView()
        {
            _yaw = GameConfig.CameraDefaultYaw;
            _pitch = GameConfig.CameraDefaultPitch;
            _distance = 13f;
            Audio.AudioSystem.Play("UI");
        }

        public void Shake(float duration, float magnitude)
        {
            _shakeTimeLeft = Mathf.Max(_shakeTimeLeft, duration);
            _shakeMagnitude = Mathf.Max(_shakeMagnitude, magnitude);
        }

        private void LateUpdate()
        {
            // consume queued zoom
            _distance = Mathf.Clamp(_distance + _zoomInput, GameConfig.CameraMinDistance, GameConfig.CameraMaxDistance);
            _zoomInput = 0f;

            if (Target == null) return;

            Vector3 dir = DirectionFromAngles(_yaw, _pitch);
            Vector3 focus = Target.position + Vector3.up * 1.6f;
            Vector3 desired = focus + dir * _distance;
            // unscaled: screens pause the game (Time.timeScale = 0) — the camera must still follow
            float lerp = _snapNext ? 1f : 1f - Mathf.Exp(-GameConfig.CameraFollowLerp * Time.unscaledDeltaTime);
            _snapNext = false;
            transform.position = Vector3.Lerp(transform.position, desired, lerp);

            UpdateShake();
            transform.LookAt(focus + _shakeOffset);
        }

        private void UpdateShake()
        {
            if (_shakeTimeLeft > 0f)
            {
                _shakeTimeLeft -= Time.deltaTime;
                float m = _shakeMagnitude * Mathf.Clamp01(_shakeTimeLeft * 3f);
                _shakeOffset = new Vector3(
                    (Mathf.PerlinNoise(Time.time * 30f, 0f) - 0.5f) * m,
                    (Mathf.PerlinNoise(0f, Time.time * 30f) - 0.5f) * m,
                    0f);
                transform.position += _shakeOffset;
            }
            else
            {
                _shakeOffset = Vector3.zero;
                _shakeMagnitude = 0f;
            }
        }

        public static Vector3 DirectionFromAngles(float yawDeg, float pitchDeg)
        {
            float yaw = yawDeg * Mathf.Deg2Rad;
            float pitch = pitchDeg * Mathf.Deg2Rad;
            return new Vector3(
                Mathf.Sin(yaw) * Mathf.Cos(pitch),
                Mathf.Sin(pitch),
                Mathf.Cos(yaw) * Mathf.Cos(pitch));
        }
    }
}
