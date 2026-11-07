using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LegendsRPG.InputCtl
{
    /// <summary>
    /// Unified touch + editor input.
    /// - Virtual joystick (bottom-left) drives hero movement.
    /// - One-finger drag on the battlefield rotates the isometric camera,
    ///   two-finger pinch zooms.
    /// - Skill/attack buttons are uGUI and handled by the EventSystem; this
    ///   class only exposes movement, camera gestures and editor hotkeys.
    /// </summary>
    public class TouchInputController : MonoBehaviour
    {
        public static TouchInputController Instance { get; private set; }

        public Vector2 MoveVector { get; private set; }

        /// <summary>True while the editor attack key (Space) is held.</summary>
        public bool AttackHeldState { get; private set; }

        public event Action<Vector2> DragRotate;      // pixel delta
        public event Action<float> PinchZoom;         // world-ish delta (pixels / 100)
        public event Action<bool> AttackHeld;         // editor Space key
        public event Action<int> SkillHotkey;         // editor keys 1/2/3, U => 3 (ult)

        private RectTransform _joystickArea;          // touch zone
        private RectTransform _knob;
        private int _joystickFingerId = -1;
        private int _cameraFingerId = -1;
        private Vector2 _cameraLastPos;
        private Vector2 _joystickCenter;
        private float _joystickRadius = 130f;
        private Vector2 _editorMouseLast;
        private bool _editorDragging;
        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void BindJoystick(RectTransform area, RectTransform knob)
        {
            _joystickArea = area;
            _knob = knob;
            if (area != null)
            {
                _joystickRadius = area.sizeDelta.x * 0.5f;
            }
        }

        private void Update()
        {
            UpdateTouch();
            UpdateEditorFallback();
        }

        private void UpdateTouch()
        {
            Vector2 move = Vector2.zero;
            int pinchCount = 0;
            Vector2 pinchA = Vector2.zero, pinchB = Vector2.zero;
            bool dragFound = false;
            Vector2 dragPos = Vector2.zero;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId);

                switch (t.phase)
                {
                    case TouchPhase.Began:
                        if (!overUi && InsideJoystick(t.position) && _joystickFingerId == -1)
                        {
                            _joystickFingerId = t.fingerId;
                            _joystickCenter = t.position;
                        }
                        else if (!overUi && _cameraFingerId == -1)
                        {
                            _cameraFingerId = t.fingerId;
                            _cameraLastPos = t.position;
                        }
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (t.fingerId == _joystickFingerId)
                        {
                            Vector2 d = t.position - _joystickCenter;
                            move = Vector2.ClampMagnitude(d, _joystickRadius) / _joystickRadius;
                            if (_knob != null)
                            {
                                Vector2 local;
                                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                                    _joystickArea, _joystickCenter + Vector2.ClampMagnitude(d, _joystickRadius),
                                    null, out local);
                                _knob.localPosition = local;
                            }
                        }
                        else if (t.fingerId == _cameraFingerId)
                        {
                            dragFound = true;
                            dragPos = t.position;
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (t.fingerId == _joystickFingerId)
                        {
                            _joystickFingerId = -1;
                            if (_knob != null) _knob.anchoredPosition = Vector2.zero;
                        }
                        if (t.fingerId == _cameraFingerId) _cameraFingerId = -1;
                        break;
                }

                pinchCount = Input.touchCount >= 2 ? 2 : 0;
                if (pinchCount == 2)
                {
                    pinchA = Input.GetTouch(0).position;
                    pinchB = Input.GetTouch(1).position;
                }
            }

            bool pinchActive = pinchCount == 2 && _joystickFingerId == -1;
            if (pinchActive)
            {
                float dist = Vector2.Distance(pinchA, pinchB);
                float delta = (dist - _lastPinchDist) / 100f;
                if (_lastPinchDist > 0f && Mathf.Abs(delta) > 0.001f && PinchZoom != null) PinchZoom(delta);
                _lastPinchDist = dist;
            }
            else
            {
                _lastPinchDist = 0f;
            }

            if (dragFound && _lastDragValid && !pinchActive)
            {
                Vector2 delta = dragPos - _lastDragPos;
                if (DragRotate != null) DragRotate(delta);
            }
            _lastDragPos = dragPos;
            _lastDragValid = dragFound;

            MoveVector = move;
        }

        private float _lastPinchDist;
        private Vector2 _lastDragPos;
        private bool _lastDragValid;

        private bool InsideJoystick(Vector2 screenPos)
        {
            if (_joystickArea == null) return false;
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_joystickArea, screenPos, null, out local);
            return local.magnitude <= _joystickArea.sizeDelta.x * 0.75f;
        }

        // ---- Editor fallback (WASD / mouse / wheel / hotkeys) ----
        private void UpdateEditorFallback()
        {
            Vector2 kv = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) kv += Vector2.up;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) kv += Vector2.down;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) kv += Vector2.left;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) kv += Vector2.right;

            if (MoveVector == Vector2.zero) MoveVector = kv.normalized;

            bool space = Input.GetKey(KeyCode.Space);
            if (space != AttackHeldState)
            {
                AttackHeldState = space;
                if (AttackHeld != null) AttackHeld(space);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) && SkillHotkey != null) SkillHotkey(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) && SkillHotkey != null) SkillHotkey(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) && SkillHotkey != null) SkillHotkey(2);
            if (Input.GetKeyDown(KeyCode.U) && SkillHotkey != null) SkillHotkey(2);

            if (Input.GetKeyDown(KeyCode.Q)) { if (DragRotate != null) DragRotate(new Vector2(-40f, 0f)); }
            if (Input.GetKeyDown(KeyCode.E)) { if (DragRotate != null) DragRotate(new Vector2(40f, 0f)); }

            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(wheel) > 0.001f && PinchZoom != null) PinchZoom(wheel * 6f);

            if (Input.GetMouseButtonDown(0) && !IsMouseOverUi())
            {
                _editorDragging = true;
                _editorMouseLast = Input.mousePosition;
            }
            if (Input.GetMouseButton(0) && _editorDragging)
            {
                Vector2 now = Input.mousePosition;
                Vector2 delta = now - _editorMouseLast;
                _editorMouseLast = now;
                if (DragRotate != null) DragRotate(delta);
            }
            if (Input.GetMouseButtonUp(0)) _editorDragging = false;
        }

        private bool IsMouseOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
