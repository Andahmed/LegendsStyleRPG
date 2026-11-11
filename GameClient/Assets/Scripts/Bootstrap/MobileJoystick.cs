using UnityEngine;
using UnityEngine.EventSystems;

namespace LegendsReborn.Rebuild
{
    public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public Vector2 Direction { get; private set; }

        private RectTransform background;
        private RectTransform handle;

        public void Initialize(RectTransform handleTransform)
        {
            background = GetComponent<RectTransform>();
            handle = handleTransform;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null) return;

            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out localPoint))
                return;

            float radius = Mathf.Min(background.rect.width, background.rect.height) * 0.36f;
            Direction = Vector2.ClampMagnitude(localPoint / Mathf.Max(radius, 1f), 1f);
            handle.anchoredPosition = Direction * radius;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
        }
    }
}
