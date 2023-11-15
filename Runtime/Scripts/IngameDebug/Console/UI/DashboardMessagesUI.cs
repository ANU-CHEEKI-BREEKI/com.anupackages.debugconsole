using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    internal class DashboardMessagesUI : MonoBehaviour
    {
        [SerializeField] private Image _lastMessageTypeIcon;
        [SerializeField] private TextMeshProUGUI _lastMessage;
        [Space]
        [SerializeField] private TextMeshProUGUI _infoCount;
        [SerializeField] private TextMeshProUGUI _warningCount;
        [SerializeField] private TextMeshProUGUI _errorCount;
        [Space]
        [SerializeField] private Sprite _logIcon;
        [SerializeField] private Sprite _warningIcon;
        [SerializeField] private Sprite _errorIcon;

        private void Awake()
        {
            DebugConsole.Logs.Cleared += UpdateUI;
            DebugConsole.Logs.Added += UpdateUI;
            UpdateUI();
        }

        private void OnDestroy()
        {
            DebugConsole.Logs.Cleared -= UpdateUI;
            DebugConsole.Logs.Added -= UpdateUI;
        }

        private void UpdateUI(LogsContainer.CollectionChangedArgs args) => UpdateUI();
        private void UpdateUI(LogsContainer.CollectionClearedArgs args) => UpdateUI();

        private void UpdateUI()
        {
            var lastLog = DebugConsole.Logs.AllLogs.LastOrDefault();
            if (lastLog == null)
            {
                _lastMessage.text = "";
                _lastMessageTypeIcon.enabled = false;
            }
            else
            {
                _lastMessage.text = $"[{lastLog.ReceivedTime:hh:mm:ss}]{lastLog.Message}";
                _lastMessageTypeIcon.enabled = true;
                _lastMessageTypeIcon.sprite = lastLog.MessageType switch
                {
                    LogType.Log => _logIcon,
                    LogType.Warning => _warningIcon,
                    _ => _errorIcon,
                };
                _lastMessageTypeIcon.color = lastLog.MessageType switch
                {
                    LogType.Log => DebugConsole.CurrentTheme?.Message_Log ?? Color.white,
                    LogType.Warning => DebugConsole.CurrentTheme?.Message_Warnings ?? Color.white,
                    _ => DebugConsole.CurrentTheme?.Message_Errors ?? Color.white,
                };
                _lastMessage.color = _lastMessageTypeIcon.color;
            }

            _infoCount.text = DebugConsole.Logs.GetMessagesCountFor(LogType.Log).ToString();
            _warningCount.text = DebugConsole.Logs.GetMessagesCountFor(LogType.Warning).ToString();
            _errorCount.text = (DebugConsole.Logs.GetMessagesCountFor(LogType.Error)
                + DebugConsole.Logs.GetMessagesCountFor(LogType.Exception)
                + DebugConsole.Logs.GetMessagesCountFor(LogType.Assert)).ToString();
        }
    }
}