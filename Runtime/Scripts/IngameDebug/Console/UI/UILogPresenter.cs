using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
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
        [Space]
        [SerializeField] private ConsoleLogType _debugConsoleLogType;
        [SerializeField] private LogType _debugMessageType;
        [SerializeField] private bool _debugExpanded;
        [Space]
        [SerializeField] private string _iconSpace = "     ";
        [SerializeField] private string _timeSpace = "                 ";

        private Action _onClick;

        public Log Log => Node.Value ?? new Log(
            _debugConsoleLogType,
            _debugMessageType,
            $"{_debugConsoleLogType} {_debugMessageType} {_message.text.Replace(_debugConsoleLogType.ToString(), "").Replace(_debugMessageType.ToString(), "").Trim()}",
            _stacktrace.text.Trim())
        {
            IsExpanded = _debugExpanded
        };

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

        private void OnEnable()
        {
            DebugConsole.ThemeChanged += UpdateTheme;
            UpdateTheme(DebugConsole.CurrentTheme);
        }

        private void OnDisable()
        {
            DebugConsole.ThemeChanged -= UpdateTheme;
        }

        public void Present(LogNode node, Action onClick)
        {
            Node = node;
            _onClick = onClick;
            UpdateMessage();
            _receivedTime.text = $"[{Log.ReceivedTime:hh:mm:ss}]";

            UpdateIcon();
            UpdateTheme(DebugConsole.CurrentTheme);
            UpdateStacktrace();
        }

        private void UpdateMessage() => _message.text = _iconSpace + Log.DisplayString;

        private void UpdateIcon()
        {
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
        }

        private void UpdateTheme(UITheme theme)
        {
            var color = theme == null
                ? Color.white
                : Log.ConsoleLogtype switch
                {
                    ConsoleLogType.Input => theme.Input,
                    ConsoleLogType.Output => Log.MessageType switch
                    {
                        LogType.Log => theme.Output_Log,
                        LogType.Warning => theme.Output_Warnings,
                        LogType.Exception => theme.Output_Exceptions,
                        LogType.Assert => theme.Output_Assert,
                        LogType.Error => theme.Output_Errors,
                        _ => throw new System.NotImplementedException()
                    },
                    ConsoleLogType.AppMessage => Log.MessageType switch
                    {
                        LogType.Log => theme.Message_Log,
                        LogType.Warning => theme.Message_Warnings,
                        LogType.Exception => theme.Message_Exceptions,
                        LogType.Assert => theme.Message_Assert,
                        LogType.Error => theme.Message_Errors,
                        _ => throw new System.NotImplementedException()
                    },
                    _ => throw new System.NotImplementedException()
                };

            _icon.color = color;
            _receivedTime.color = color;
            _message.color = color;
            _stacktrace.color = theme == null ? Color.white : theme.Stacktrace;
        }

        private void UpdateStacktrace()
        {
            if (Log.IsExpanded && !string.IsNullOrEmpty(Log.StackTrace))
            {
                _stacktrace.text = _timeSpace + Log.StackTrace;
                _stacktrace.gameObject.SetActive(true);
            }
            else
            {
                _stacktrace.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateStacktrace();
            if (Application.isPlaying)
                return;
            UpdateMessage();
            UpdateIcon();
        }
    }
}