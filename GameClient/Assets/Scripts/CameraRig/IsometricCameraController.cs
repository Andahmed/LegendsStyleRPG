using UnityEngine;
using LegendsRPG.Core;

namespace LegendsRPG.Cam
{
    /// <summary>
    /// Isometric / top-down 3/4 view camera in the style of Legends Reborn.
    /// - Follows the active hero smoothly.
    /// - One-finger drag rotates yaw/pitch, two-finger pinch zooms (touch)
    ///   or mouse drag + wheel in the editor.
    /// - Adds impact shake used on hits and Ultimate skills.
    /// </summary>
    public class IsometricCameraController : MonoBehaviour
    {
        public Transform Target;

        private float _yaw = GameConfig.CameraDefaultYaw;
        private float _pitch = GameConfig.CameraDefaultPitch;
        private float _distance = 13f;
        private float _yawInput, _pitchInput, _zoomInput;
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

        public void SetTarget(Transform t) { Target = t; }

        public void AddOrbitInput(Vector2 delta)
        {
            _yawInput += delta.x;
            _pitchInput -= delta.y;
        }

        public void AddZoomInput(float delta)
        {
            _zoomInput += delta;
        }

        public void Shake(float duration, float magnitude)
        {
            _shakeTimeLeft = Mathf.Max(_shakeTimeLeft, duration);
            _shakeMagnitude = Mathf.Max(_shakeMagnitude, magnitude);
        }

        private void LateUpdate()
        {
            // consume queued input
            float rotSpeed = 2.2f;
            _yaw += _yawInput * rotSpeed;
            _pitch = Mathf.Clamp(_pitch + _pitchInput * rotSpeed * 0.7f,
                GameConfig.CameraMinPitch, GameConfig.CameraMaxPitch);
            _distance = Mathf.Clamp(_distance + _zoomInput, GameConfig.CameraMinDistance, GameConfig.CameraMaxDistance);
            _yawInput = 0f; _pitchInput = 0f; _zoomInput = 0f;

            if (Target == null) return;

            Vector3 dir = DirectionFromAngles(_yaw, _pitch);
            Vector3 focus = Target.position + Vector3.up * 1.6f;
            Vector3 desired = focus + dir * _distance;
            float lerp = 1f - Mathf.Exp(-GameConfig.CameraFollowLerp * Time.deltaTime);
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
