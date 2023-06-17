using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [RequireComponent(typeof(RectTransform))]
    public class UILogPresenter : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private TextMeshProUGUI _receivedTime;
        [SerializeField] private Image _icon;
        [Space]
        [SerializeField] private Sprite[] _icons;

        private Log _log;

        public Log Log => _log;
        public RectTransform RectTransform => transform as RectTransform;

        //TODO: later add there other elements
        public float PrefferedHeight => LayoutUtility.GetPreferredHeight(_text.rectTransform);

        public void Present(Log log)
        {
            _log = log;
            _text.text = "                      " + _log.DisplayString;
            _receivedTime.text = $"[{_log.ReceivedTime:hh:mm:ss}]";
            _icon.sprite = _icons[(int)_log.Type];
        }
    }
}