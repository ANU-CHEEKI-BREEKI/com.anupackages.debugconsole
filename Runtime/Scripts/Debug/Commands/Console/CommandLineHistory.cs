using System;
using System.Collections.Generic;
using IngameDebug.Utils;

namespace IngameDebug.Commands.Console
{
    public class CommandLineHistory
    {
        private readonly LinkedList<string> _commands = new LinkedList<string>();
        private LinkedListNode<string> _currentNode;

        public string Current => _currentNode?.Value;

        public IReadOnlyCollection<string> Commands => _commands;

        public bool TryMoveUp(out string command)
        {
            command = null;

            if (_currentNode == null)
            {
                if (_commands.Count == 0)
                    return false;

                _currentNode = _commands.Last;
                command = _currentNode.Value;
                return true;
            }

            if (_currentNode.Previous == null)
                return false;

            _currentNode = _currentNode.Previous;
            command = _currentNode.Value;
            return true;
        }

        public bool TryMoveDown(out string command)
        {
            command = null;

            if (_currentNode == null)
                return false;

            if (_currentNode.Next == null)
                return false;

            _currentNode = _currentNode.Next;
            command = _currentNode.Value;
            return true;
        }

        public bool Record(string command)
        {
            if (string.IsNullOrEmpty(command))
                return false;

            _commands.RemoveaAll(v => v == command);
            _commands.AddLast(command);
            _currentNode = null;
            return true;
        }

        public void Reset()
        {
            _currentNode = null;
        }

        public void Clear()
        {
            _commands.Clear();
            Reset();
        }
        
    }
}