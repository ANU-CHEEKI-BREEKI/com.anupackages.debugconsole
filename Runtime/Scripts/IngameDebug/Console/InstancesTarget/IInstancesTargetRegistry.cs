using System;
using System.Collections.Generic;

namespace ANU.IngameDebug.Console
{
    public interface IInstancesTargetRegistry
    {
        void Register<T>(params T[] targets);
        void Register<T>(IEnumerable<T> targets);
        void Unregister<T>(params T[] targets);
        void UnRegister<T>(IEnumerable<T> targets);

        void AddProvider<T>(Func<T> target);
        void AddProvider<T>(Func<IEnumerable<T>> targets);
        void RemoveProvider<T>(Func<T> target);
        void RemoveProvider<T>(Func<IEnumerable<T>> targets);

        IEnumerable<T> Get<T>();
        IEnumerable<object> Get(Type type);
    }
}