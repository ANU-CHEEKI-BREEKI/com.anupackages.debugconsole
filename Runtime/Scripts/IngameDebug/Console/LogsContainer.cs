using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class LogsContainer
    {
        public struct CollectionFilteredArgs
        {
            public LogsContainer Container;
            public IReadOnlyList<LogType> FilterTypes;
            public string SearchStrgin;

            public CollectionFilteredArgs(LogsContainer container, IReadOnlyList<LogType> filterTypes, string searchStrgin)
            {
                Container = container;
                FilterTypes = filterTypes;
                SearchStrgin = searchStrgin;
            }
        }
        public struct CollectionChangedArgs
        {
            public Log Log;
            public LogsContainer Container;

            public CollectionChangedArgs(Log log, LogsContainer container)
            {
                Log = log;
                Container = container;
            }
        }
        public struct CollectionClearedArgs
        {
            public LogsContainer Container;

            public CollectionClearedArgs(LogsContainer container)
                => Container = container;
        }

        private readonly LinkedList<Log> _allLogs = new();
        private readonly LinkedList<Log> _filteredLogs = new();

        private readonly Dictionary<(ConsoleLogType, LogType), int> _messagesCount = new();

        public string SearchString { get; private set; }
        public IReadOnlyList<LogType> FilterTypes { get; private set; }

        public IEnumerable<Log> AllLogs => _allLogs;
        public IEnumerable<Log> FilteredLogs => _filteredLogs;

        public int Count => ((IReadOnlyCollection<Log>)_filteredLogs).Count;

        public event Action<CollectionChangedArgs> Added;
        public event Action<CollectionClearedArgs> Cleared;
        public event Action<CollectionFilteredArgs> Filtered;

        public void Add(Log log)
        {
            log.WholeIndex = _allLogs.Count;

            _allLogs.AddLast(log);
            AddAsFiltered(log);

            var types = (log.ConsoleLogtype, log.MessageType);
            if (!_messagesCount.ContainsKey(types))
                _messagesCount[types] = 0;
            _messagesCount[types]++;

            Added?.Invoke(new CollectionChangedArgs(log, this));
        }

        public int GetMessagesCountFor(LogType t2)
        {
            var types = (ConsoleLogType.AppMessage, t2);
            if (!_messagesCount.ContainsKey(types))
                _messagesCount[types] = 0;
            return _messagesCount[types];
        }

        private void AddAsFiltered(Log log)
        {
            if (!IsMatchFilters(log))
                return;

            log.FilteredIndex = _filteredLogs.Count;
            log.Select(SearchString);
            _filteredLogs.AddLast(log);
        }

        public void Clear()
        {
            _allLogs.Clear();
            _filteredLogs.Clear();

            _messagesCount.Clear();
            Cleared?.Invoke(new CollectionClearedArgs(this));
        }

        public void Filter(string search = null, LogType[] types = null)
        {
            if (string.IsNullOrEmpty(SearchString) && string.IsNullOrEmpty(search)
                && (types == FilterTypes
                    || (types != null && FilterTypes != null && types.Length == FilterTypes.Count && types.Zip(FilterTypes, (a, b) => new { a = a, b = b }).All(b => b.a == b.b)))
                )
                return;

            SearchString = search;
            FilterTypes = types;
            _filteredLogs.Clear();

            foreach (var item in AllLogs)
                AddAsFiltered(item);

            Filtered?.Invoke(new CollectionFilteredArgs(this, FilterTypes, SearchString));
        }

        private bool IsMatchFilters(Log log)
            => (FilterTypes == null || log.ConsoleLogtype != ConsoleLogType.AppMessage || FilterTypes.Contains(log.MessageType))
            && (string.IsNullOrEmpty(SearchString) || log.Message.Contains(SearchString));

        private IEnumerable<LinkedListNode<Log>> AsEnumerable()
        {
            var next = _filteredLogs.First;
            while (next != null)
            {
                var current = next;
                next = next.Next;
                yield return current;
            }
        }

        // TODO: make some caching of index
        public LogNode ElementAtIndexClampedOrDefault(int index)
        {
            if (index <= 0)
                return _filteredLogs.First;
            else if (index >= _filteredLogs.Count)
                return _filteredLogs.Last;
            else
                return AsEnumerable().ElementAtOrDefault(index);
        }
    }

    internal struct LogNode
    {
        private readonly LinkedListNode<Log> _node;

        public LogNode(LinkedListNode<Log> node) : this()
            => _node = node;

        public LogNode Next => _node?.Next;
        public LogNode Previous => _node?.Previous;
        public Log Value => _node?.Value;

        public static implicit operator LogNode(LinkedListNode<Log> node)
            => new LogNode(node);

        public static bool operator ==(LogNode a, LogNode b)
        {
            if (a._node == null && b._node == null)
                return true;
            return a._node == b._node;
        }

        public static bool operator !=(LogNode a, LogNode b)
        {
            var equals = a == b;
            return !equals;
        }

        public override bool Equals(object obj)
            => obj is LogNode node && this == node;

        public override int GetHashCode() => _node?.GetHashCode() ?? 0;
    }
}