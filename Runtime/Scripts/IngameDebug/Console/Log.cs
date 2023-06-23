using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    public class Log
    {
        public readonly ConsoleLogType ConsoleLogtype;
        public readonly LogType MessageType;
        public readonly string Message;
        public readonly string StackTrace;
        public readonly DateTime ReceivedTime;

        private string _selectionSubstring;
        private string _selectedMessage;

        public bool IsExpanded { get; set; }
        public int WholeIndex { get; set; }
        /// <summary>
        /// It is invalid if item not in filtered items list
        /// </summary>
        public int FilteredIndex { get; set; }

        public Log(ConsoleLogType consoleLogType, LogType messageType, string message, string stackTrace)
            : this(consoleLogType, messageType, message, stackTrace, DateTime.Now) { }

        public Log(ConsoleLogType consoleLogType, LogType messageType, string message, string stackTrace, DateTime receivedTime)
        {
            ConsoleLogtype = consoleLogType;
            MessageType = messageType;

            Message = message;
            StackTrace = stackTrace;
            ReceivedTime = receivedTime;

            _selectedMessage = Message;
        }

        public string DisplayString => _selectedMessage;

        public void Select(string substring)
        {
            _selectionSubstring = substring;
            if (string.IsNullOrEmpty(_selectionSubstring))
                _selectedMessage = Message;
            else
                _selectedMessage = Regex.Replace(Message, $"(?<g>{_selectionSubstring})", "<mark=#ffff00aa>${g}</mark>");
        }

        public void Deselect() => _selectedMessage = Message;
    }
}