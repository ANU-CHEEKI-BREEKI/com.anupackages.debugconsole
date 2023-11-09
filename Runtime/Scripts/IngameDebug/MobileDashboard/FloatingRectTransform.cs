using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ANU.IngameDebug.Console.Dashboard
{
    [RequireComponent(typeof(RectTransform))]
    public class FloatingRectTransform : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        private bool _drag;

        private Vector2 _offset;

        public event Action<PointerEventData> Clicked;
        public event Action<PointerEventData> DragEnd;

        public RectTransform RT => transform as RectTransform;
        public RectTransform RTParent => transform.parent as RectTransform;

        private void OnEnable() => ClampPosition(RT.localPosition);

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) => _drag = true;
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _drag = false;
            DragEnd?.Invoke(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RTParent, eventData.position, eventData.pressEventCamera, out var localPoint);
            localPoint -= _offset;
            ClampPosition(localPoint);
        }

        private void ClampPosition(Vector2 localPoint)
        {
            var size = RTParent.rect.size;
            var localNormalized = (localPoint + (size / 2f)) / size;
            var rtNormalizedSize = RT.rect.size / size / 2f;

            localNormalized.x = Mathf.Clamp(localNormalized.x, rtNormalizedSize.x, 1f - rtNormalizedSize.x);
            localNormalized.y = Mathf.Clamp(localNormalized.y, rtNormalizedSize.y, 1f - rtNormalizedSize.y);

            RT.anchorMin = localNormalized;
            RT.anchorMax = localNormalized;
            RT.anchoredPosition = Vector2.zero;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (_drag)
                return;
            Clicked?.Invoke(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) { }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RT, eventData.position, eventData.pressEventCamera, out var localPoint);
            _offset = localPoint;
        }
    }
}