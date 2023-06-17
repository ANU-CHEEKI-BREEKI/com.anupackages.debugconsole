using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [RequireComponent(typeof(RectTransform))]
    public class UILogPresenter : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _message;
        [SerializeField] private TextMeshProUGUI _stacktrace;
        [SerializeField] private TextMeshProUGUI _receivedTime;
        [SerializeField] private Image _icon;
        [Space]
        [SerializeField] private Sprite[] _icons;

        private Log _log;
        private Action _onClick;

        public Log Log => _log;
        public int Index { get; private set; }
        public RectTransform RectTransform => transform as RectTransform;

        //TODO: later add there other elements
        public float PrefferedHeight => LayoutUtility.GetPreferredHeight(_message.rectTransform);

        private void Awake()
        {
            _button.onClick.AddListener(() =>
            {
                if (_log == null)
                    return;
                _log.IsExpanded = !_log.IsExpanded;

                UpdateStacktrace();

                _onClick?.Invoke();
            });
        }

        public void Present(int index, Log log, Action onClick)
        {
            Index = index;
            _onClick = onClick;
            _log = log;
            _message.text = "                      " + _log.DisplayString;
            _receivedTime.text = $"[{_log.ReceivedTime:hh:mm:ss}]";
            _icon.sprite = _icons[(int)_log.Type];

            UpdateStacktrace();
        }

        private void UpdateStacktrace()
        {
            if (_log.IsExpanded)
            {
                _stacktrace.text = _log.StackTrace;
                _stacktrace.gameObject.SetActive(true);
            }
            else
            {
                _stacktrace.gameObject.SetActive(false);
            }
        }

        // private void Update() {
        //     _message.text = System.Text.RegularExpressions.Regex.Replace(_message.text, @"\{.*\}", $"{{{RectTransform.rect.center}}}");
        // }
    }
}