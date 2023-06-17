using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
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

        public void Present(Log log)
        {
            _log = log;
            _text.text = "                      " + _log.DisplayString;
            _receivedTime.text = $"[{_log.ReceivedTime:hh:mm:ss}]";
            _icon.sprite = _icons[(int)_log.Type];
        }
    }
}