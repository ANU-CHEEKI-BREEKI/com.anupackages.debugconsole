using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    public class CustomScrollBar : Scrollbar, IEndDragHandler
    {
        public bool IsDragging { get; private set; }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            IsDragging = true;
            base.OnBeginDrag(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;
        }
    }
}