using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace ANU.IngameDebug.Console
{
    public class LogsContainer : IEnumerable<Log>, IReadOnlyList<Log>
    {
        public class CollectionChangedArgs
        {
            public enum ChangeType { Add, Remove }

            public ChangeType Type;
            public readonly LogsContainer Container;

            public CollectionChangedArgs(LogsContainer container)
            {
                Container = container;
            }

            public readonly List<Log> ChangedItems = new();
        }

        private readonly List<Log> _logs = new();
        private readonly CollectionChangedArgs _sharedArgs;

        public int Count => ((IReadOnlyCollection<Log>)_logs).Count;
        public Log this[int index] => ((IReadOnlyList<Log>)_logs)[index];

        public event Action<CollectionChangedArgs> Changed;

        public LogsContainer()
            => _sharedArgs = new CollectionChangedArgs(this);

        public void Add(Log log)
        {
            _sharedArgs.ChangedItems.Clear();
            _sharedArgs.ChangedItems.Add(log);
            _sharedArgs.Type = CollectionChangedArgs.ChangeType.Add;

            _logs.Add(log);

            Changed?.Invoke(_sharedArgs);
        }

        public void Remove(Log log)
        {
            _sharedArgs.ChangedItems.Clear();
            _sharedArgs.ChangedItems.Add(log);
            _sharedArgs.Type = CollectionChangedArgs.ChangeType.Remove;

            _logs.Remove(log);

            Changed?.Invoke(_sharedArgs);
        }

        public void Clear()
        {
            _sharedArgs.ChangedItems.Clear();
            _sharedArgs.ChangedItems.AddRange(_logs);
            _sharedArgs.Type = CollectionChangedArgs.ChangeType.Remove;

            _logs.Clear();

            Changed?.Invoke(_sharedArgs);
        }

        public IEnumerable<Log> Filter(LogTypes[] types = null)
            => _logs.Where(l => types == null || types.Contains(l.Type));

        public IEnumerable<Log> Filter(string search, LogTypes[] types = null)
            => Filter(types).Where(l => l.Message.Contains(search));

        public IEnumerator<Log> GetEnumerator() => ((IEnumerable<Log>)_logs).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_logs).GetEnumerator();
    }
}