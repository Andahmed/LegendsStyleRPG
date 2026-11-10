using UnityEngine;

namespace LegendsRPG.UI
{
    /// <summary>
    /// Fits a UI RectTransform to the device safe area. Its parent must be
    /// the full-screen Canvas (or another full-screen RectTransform).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void OnEnable()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea ||
                Screen.width != _lastScreenSize.x ||
                Screen.height != _lastScreenSize.y)
                ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();
            if (_rect == null || Screen.width <= 0 || Screen.height <= 0) return;

            Rect safe = Screen.safeArea;
            _lastSafeArea = safe;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
