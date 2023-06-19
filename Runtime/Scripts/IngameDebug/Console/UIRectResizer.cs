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
        private const string PrefsSaveUIScale_SizeX = PrefsSavePrefix + nameof(SizeX);
        private const string PrefsSaveUIScale_SizeY = PrefsSavePrefix + nameof(SizeY);
        private const string PrefsSaveUIScale_SizeXVPort = PrefsSavePrefix + nameof(SizeXVPort);
        private const string PrefsSaveUIScale_SizeYVPort = PrefsSavePrefix + nameof(SizeYVPort);
        private const string PrefsSaveUIScale_SizeStored = PrefsSavePrefix + nameof(HasSize);

        private Vector3[] _corners = new Vector3[4];

        private bool HasSize
        {
            get => PlayerPrefs.GetInt(PrefsSaveUIScale_SizeStored, 0) == 1;
            set => PlayerPrefs.GetInt(PrefsSaveUIScale_SizeStored, value ? 1 : 0);
        }
        private float SizeX
        {
            get => PlayerPrefs.GetFloat(PrefsSaveUIScale_SizeX, -1);
            set => PlayerPrefs.SetFloat(PrefsSaveUIScale_SizeX, value);
        }
        private float SizeY
        {
            get => PlayerPrefs.GetFloat(PrefsSaveUIScale_SizeY, -1);
            set => PlayerPrefs.SetFloat(PrefsSaveUIScale_SizeY, value);
        }
        private float SizeXVPort
        {
            get => PlayerPrefs.GetFloat(PrefsSaveUIScale_SizeXVPort, -1);
            set => PlayerPrefs.SetFloat(PrefsSaveUIScale_SizeXVPort, value);
        }
        private float SizeYVPort
        {
            get => PlayerPrefs.GetFloat(PrefsSaveUIScale_SizeYVPort, -1);
            set => PlayerPrefs.SetFloat(PrefsSaveUIScale_SizeYVPort, value);
        }

        private Vector2 Delta { get; set; }

        private Vector2 Size
        {
            get => new Vector2(SizeX, SizeY);
            set
            {
                SizeX = value.x;
                SizeY = value.y;
            }
        }

        private void Awake()
        {
            if (!HasSize)
                Size = _rect.sizeDelta;

            _rect.sizeDelta = Size;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out var localPosition);
            Delta = Size - localPosition;
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
            Size = value;
            _rect.sizeDelta = value;

            _rect.GetWorldCorners(_corners);
            var corner = _corners[2];
            SizeXVPort = corner.x / Screen.width;
            SizeYVPort = corner.y / Screen.height;
        }

        [DebugCommand(Name = "refresh-size")]
        private void RefreshConsoleSize() => ConsoleSize(new Vector2Int(
            Mathf.RoundToInt(SizeXVPort * 100f),
            Mathf.RoundToInt(SizeYVPort * 100f)
        ));

        [DebugCommand(Name = "size", Description = "Set console rect size relative to vieport. Values is Vector2Int in range [0, 100]")]
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