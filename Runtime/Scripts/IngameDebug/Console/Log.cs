using System;
using System.Text.RegularExpressions;

namespace ANU.IngameDebug.Console
{
    public class Log
    {
        public readonly LogTypes Type;
        public readonly string Message;
        public readonly string StackTrace;
        public readonly DateTime ReceivedTime;

        private string _selectionSubstring;
        private string _selectedMessage;

        public Log(LogTypes type, string message, string stackTrace) : this(type, message, stackTrace, DateTime.Now) { }
        public Log(LogTypes type, string message, string stackTrace, DateTime receivedTime)
        {
            Type = type;
            Message = message;
            StackTrace = stackTrace;
            ReceivedTime = receivedTime;
            
            _selectedMessage = Message;
        }

        public string DisplayString => _selectedMessage;

        public void Select(string substring)
            => _selectedMessage = Regex.Replace(Message, $"(?<g>{_selectionSubstring})", "<mark=#ffff00aa>${g}</mark>");

        public void Deselect() => _selectedMessage = Message;
    }
}