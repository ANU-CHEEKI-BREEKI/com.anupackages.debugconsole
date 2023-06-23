using System;
using System.Collections.Generic;
using System.Linq;

namespace ANU.IngameDebug.Console
{
    internal class InstancesTargetRegistry : IInstancesTargetRegistry
    {
        private readonly Dictionary<Type, HashSet<object>> _targetsByType = new();

        public IEnumerable<T> Get<T>() => Get(typeof(T)).Select(o => (T)o);

        public IEnumerable<object> Get(Type type)
        {
            if (!_targetsByType.ContainsKey(type))
                _targetsByType[type] = new HashSet<object>();

            return _targetsByType[type] as IEnumerable<object>;
        }

        public void Register<T>(params T[] targets)
        {
            if (!_targetsByType.ContainsKey(typeof(T)))
                _targetsByType[typeof(T)] = new HashSet<object>();

            var targetsList = _targetsByType[typeof(T)];
            foreach (var item in targets)
                targetsList.Add(item);
        }

        public void Unregister<T>(params T[] targets)
        {
            if (!_targetsByType.ContainsKey(typeof(T)))
                return;

            var targetsList = _targetsByType[typeof(T)];
            foreach (var item in targets)
                targetsList.Remove(item);
        }
    }
}