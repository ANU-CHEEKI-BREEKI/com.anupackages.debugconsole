using UnityEngine;
using UnityEngine.EventSystems;

namespace ANU.IngameDebug.Console
{
    public class UIRectResizer : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _rect;

        private const string PrefsSavePrefix = nameof(DebugConsole) + nameof(UIRectResizer);
        private const string PrefsSaveUIScale_SizeX = PrefsSavePrefix + nameof(SizeX);
        private const string PrefsSaveUIScale_SizeY = PrefsSavePrefix + nameof(SizeY);

        private float SizeX
        {
            get => PlayerPrefs.GetFloat(PrefsSaveUIScale_SizeX, -1);
            set => PlayerPrefs.GetFloat(PrefsSaveUIScale_SizeX, value);
        }
        private float SizeY
        {
            get => PlayerPrefs.GetFloat(PrefsSaveUIScale_SizeY, -1);
            set => PlayerPrefs.GetFloat(PrefsSaveUIScale_SizeY, value);
        }

        private Vector2? Size
        {
            get
            {
                var x = SizeX;
                var y = SizeY;

                if (x < 0 || y < 0)
                    return null;
                else
                    return new Vector2(x, y);
            }
            set
            {
                var v = value ?? Vector2.one * -1;
                SizeX = v.x;
                SizeY = v.y;
            }
        }

        private void Awake()
        {
            if (Size == null)
                Size = _rect.sizeDelta;

            //FIXME: nullref there
            ConsoleSize(Size.Value);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (Size == null)
                Size = _rect.sizeDelta;
            ConsoleSize(Size.Value + eventData.delta);
        }

        private void ConsoleSize(
            [OptAltNames("v")]
            Vector2 value
        )
        {
            value.x = Mathf.Clamp(value.x, 100f, 100f);
            value.y = Mathf.Clamp(value.y, 100f, 100f);
            Size = value;
            _rect.sizeDelta = value;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) { }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) { }
    }
}