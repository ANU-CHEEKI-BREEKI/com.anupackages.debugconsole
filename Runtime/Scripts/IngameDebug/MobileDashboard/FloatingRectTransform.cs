using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ANU.IngameDebug.Console.Dashboard
{
    [RequireComponent(typeof(RectTransform))]
    public class FloatingRectTransform : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private bool _drag;

        public event Action<PointerEventData> Clicked;
        public event Action<PointerEventData> DragEnd;

        public RectTransform RT => transform as RectTransform;
        public RectTransform RTParent => transform.parent as RectTransform;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) => _drag = true;
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _drag = false;
            DragEnd?.Invoke(eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RTParent, eventData.position, eventData.pressEventCamera, out var localPoint);

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
    }
}