using System;
using UnityEngine;

namespace LegendsRPG.InputCtl
{
    /// <summary>
    /// v1.1 (Legends Reborn style): combat is fully autonomous — there is no
    /// virtual joystick, no manual movement, no manual attack and no drag
    /// rotation anymore. The only touch inputs are:
    ///  - uGUI buttons / hero portraits (handled by the EventSystem), and
    ///  - two-finger pinch (device) or mouse wheel (editor) for camera zoom.
    /// </summary>
    public class TouchInputController : MonoBehaviour
    {
        public static TouchInputController Instance { get; private set; }

        /// <summary>Two-finger pinch on device, mouse wheel in the editor (pixels / 100).</summary>
        public event Action<float> PinchZoom;

        private float _lastPinchDist;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            UpdateTouch();
            UpdateEditorFallback();
        }

        private void UpdateTouch()
        {
            if (Input.touchCount == 2)
            {
                float dist = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                float delta = (dist - _lastPinchDist) / 100f;
                if (_lastPinchDist > 0f && Mathf.Abs(delta) > 0.001f && PinchZoom != null) PinchZoom(delta);
                _lastPinchDist = dist;
            }
            else
            {
                _lastPinchDist = 0f;
            }
        }

        private void UpdateEditorFallback()
        {
            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(wheel) > 0.001f && PinchZoom != null) PinchZoom(wheel * 6f);
        }
    }
}
