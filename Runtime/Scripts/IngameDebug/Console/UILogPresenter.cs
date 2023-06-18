using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [RequireComponent(typeof(RectTransform))]
    internal class UILogPresenter : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _message;
        [SerializeField] private TextMeshProUGUI _stacktrace;
        [SerializeField] private TextMeshProUGUI _receivedTime;
        [SerializeField] private Image _icon;
        [Space]
        [SerializeField] private Sprite _iconLog;
        [SerializeField] private Sprite _iconWarning;
        [SerializeField] private Sprite _iconError;
        [SerializeField] private Sprite _iconInput;
        [SerializeField] private Sprite _iconOutput;

        private Action _onClick;

        public Log Log => Node.Value;
        public LogNode Node { get; private set; }
        public RectTransform RectTransform => transform as RectTransform;

        //TODO: later add there other elements
        public float PrefferedHeight => LayoutUtility.GetPreferredHeight(_message.rectTransform);

        private void Awake()
        {
            _button.onClick.AddListener(() =>
            {
                if (Log == null)
                    return;
                Log.IsExpanded = !Log.IsExpanded;

                UpdateStacktrace();

                _onClick?.Invoke();
            });
        }

        public void Present(LogNode node, Action onClick)
        {
            Node = node;
            _onClick = onClick;
            _message.text = "                      " + Log.DisplayString;
            _receivedTime.text = $"[{Log.ReceivedTime:hh:mm:ss}]";

            _icon.sprite = Log.ConsoleLogtype switch
            {
                ConsoleLogType.Input => _iconInput,
                ConsoleLogType.Output => _iconOutput,
                ConsoleLogType.AppMessage => Log.MessageType switch
                {
                    LogType.Log => _iconLog,
                    LogType.Warning => _iconWarning,
                    LogType.Exception => _iconError,
                    LogType.Assert => _iconError,
                    LogType.Error => _iconError,
                    _ => throw new System.NotImplementedException()
                },
                _ => throw new System.NotImplementedException()
            };

            UpdateStacktrace();
        }

        private void UpdateStacktrace()
        {
            if (Log.IsExpanded)
            {
                _stacktrace.text = Log.StackTrace;
                _stacktrace.gameObject.SetActive(true);
            }
            else
            {
                _stacktrace.gameObject.SetActive(false);
            }
        }
    }
}