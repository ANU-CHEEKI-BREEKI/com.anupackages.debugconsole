using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ANU.IngameDebug.Console
{
    internal class InstancesTargetRegistry : IInstancesTargetRegistry
    {
        private readonly Dictionary<Type, HashSet<object>> _targetsByType = new();
        private readonly Dictionary<Type, HashSet<Delegate>> _providers1 = new();
        private readonly Dictionary<Type, HashSet<Delegate>> _providers2 = new();

        public IEnumerable<T> Get<T>()
            => Get(typeof(T)).Select(o => (T)o);

        public IEnumerable<object> Get(Type type)
        {
            if (!_targetsByType.ContainsKey(type))
                _targetsByType[type] = new HashSet<object>();

            var directRegistry = _targetsByType[type] as IEnumerable<object>;

            if (!_providers1.ContainsKey(type))
                _providers1[type] = new HashSet<Delegate>();

            var lazyRagistry1 = _providers1[type].Select(d => d.DynamicInvoke());

            if (!_providers2.ContainsKey(type))
                _providers2[type] = new HashSet<Delegate>();

            var lazyRagistry2 = _providers2[type].SelectMany(d => (d.DynamicInvoke() as IEnumerable).Cast<object>());

            return directRegistry.Concat(lazyRagistry1).Concat(lazyRagistry2);
        }

        public void Register<T>(params T[] targets)
            => Register(targets.AsEnumerable());

        public void Register<T>(IEnumerable<T> targets)
        {
            if (!_targetsByType.ContainsKey(typeof(T)))
                _targetsByType[typeof(T)] = new HashSet<object>();

            var targetsList = _targetsByType[typeof(T)];
            foreach (var item in targets)
                targetsList.Add(item);
        }

        public void Unregister<T>(params T[] targets)
            => Unregister(targets.AsEnumerable());

        public void UnRegister<T>(IEnumerable<T> targets)
        {
            if (!_targetsByType.ContainsKey(typeof(T)))
                return;

            var targetsList = _targetsByType[typeof(T)];
            foreach (var item in targets)
                targetsList.Remove(item);
        }

        public void AddProvider<T>(Func<T> target)
        {
            if (!_providers1.ContainsKey(typeof(T)))
                _providers1[typeof(T)] = new HashSet<Delegate>();
            _providers1[typeof(T)].Add(target);
        }

        public void AddProvider<T>(Func<IEnumerable<T>> targets)
        {
            if (!_providers2.ContainsKey(typeof(T)))
                _providers2[typeof(T)] = new HashSet<Delegate>();
            _providers2[typeof(T)].Add(targets);
        }

        public void RemoveProvider<T>(Func<T> target)
        {
            if (!_providers1.ContainsKey(typeof(T)))
                return;
            _providers1[typeof(T)].Remove(target);
        }

        public void RemoveProvider<T>(Func<IEnumerable<T>> targets)
        {
            if (!_providers2.ContainsKey(typeof(T)))
                return;
            _providers2[typeof(T)].Remove(targets);
        }
    }
}