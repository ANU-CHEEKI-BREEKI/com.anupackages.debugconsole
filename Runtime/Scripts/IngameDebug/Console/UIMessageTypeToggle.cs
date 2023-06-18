using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [RequireComponent(typeof(Toggle))]
    internal class UIMessageTypeToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TextMeshProUGUI _label;
        [Space]
        [SerializeField] private LogType[] _types;

        public LogType[] Types => _toggle.isOn ? _types : Array.Empty<LogType>();

        public event Action Toggled;
        public int Count { set => _label.text = value.ToString(); }
        public Toggle Toggle => _toggle;

        private void Awake() => _toggle.onValueChanged.AddListener(s => Toggled?.Invoke());
    }
}