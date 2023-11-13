using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ANU.IngameDebug.Console
{
    [DebugCommandPrefix("console")]
    public class UIRectResizer : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _rect;

        private const string PrefsSavePrefix = nameof(DebugConsole) + nameof(UIRectResizer);
        private const string PrefsSaveUIScale_SizeXVPort = PrefsSavePrefix + nameof(SizeXVPort);
        private const string PrefsSaveUIScale_SizeYVPort = PrefsSavePrefix + nameof(SizeYVPort);

        private Vector2 DefaultSize =>
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        new Vector2(1f, 1f)
#else
        new Vector2(0.7f, 0.7f)
#endif
        ;

        private Vector3[] _corners = new Vector3[4];

        private float SizeXVPort
        {
            get => PlayerPrefs.GetFloat(PrefsSaveUIScale_SizeXVPort, DefaultSize.x);
            set => PlayerPrefs.SetFloat(PrefsSaveUIScale_SizeXVPort, value);
        }
        private float SizeYVPort
        {
            get => PlayerPrefs.GetFloat(PrefsSaveUIScale_SizeYVPort, DefaultSize.y);
            set => PlayerPrefs.SetFloat(PrefsSaveUIScale_SizeYVPort, value);
        }

        private Vector2 Delta { get; set; }

        private void Awake() => RefreshConsoleSize();

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out var localPosition);
            Delta = _rect.sizeDelta - localPosition;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) { }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out var localPosition);

            var size = localPosition;
            //compensate relative mouse position in the corner
            size += Delta;

            InternalConsoleSize(size);
        }

        private void InternalConsoleSize(Vector2 value)
        {
            var maxScreen = Camera.main.ViewportToScreenPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
            var c = _rect.GetComponentInParent<Canvas>().rootCanvas;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rect,
                maxScreen,
                c.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : c.worldCamera ?? Camera.main,
                out var localPosition
            );

            value.x = Mathf.Clamp(value.x, 800f, localPosition.x);
            value.y = Mathf.Clamp(value.y, 600f, localPosition.y);
            _rect.sizeDelta = value;

            _rect.GetWorldCorners(_corners);
            var corner = _corners[2];
            SizeXVPort = corner.x / Screen.width;
            SizeYVPort = corner.y / Screen.height;
        }

        [DebugCommand(Name = "refresh-size", DisplayOptions = CommandDisplayOptions.All & ~CommandDisplayOptions.Dashboard)]
        public void RefreshConsoleSize() => ConsoleSize(new Vector2Int(
            Mathf.RoundToInt(SizeXVPort * 100f),
            Mathf.RoundToInt(SizeYVPort * 100f)
        ));

        [DebugCommand(Name = "size", Description = "Set console rect size relative to viewport. Values is Vector2Int in range [0, 100]")]
        private void ConsoleSize(
            [OptAltNames("v")]
            [OptDesc("in range [10,100]")]
            [OptVal("[50, 50]", "[70, 70]", "[100, 100]")]
            Vector2Int value
        )
        {
            Canvas.ForceUpdateCanvases();

            var x = Mathf.Clamp(value.x, 10, 100) / 100f;
            var y = Mathf.Clamp(value.y, 10, 100) / 100f;

            var maxScreen = Camera.main.ViewportToScreenPoint(new Vector3(x, y, Camera.main.nearClipPlane));
            var c = _rect.GetComponentInParent<Canvas>().rootCanvas;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rect,
                maxScreen,
                c.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : c.worldCamera ?? Camera.main,
                out var localPosition
            );
            InternalConsoleSize(localPosition);
        }
    }
}