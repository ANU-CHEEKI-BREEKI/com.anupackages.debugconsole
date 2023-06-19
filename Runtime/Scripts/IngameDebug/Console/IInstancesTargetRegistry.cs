using System;
using System.Collections.Generic;

namespace ANU.IngameDebug.Console
{
    public interface IInstancesTargetRegistry
    {
        void Register<T>(params T[] targets);
        // void Register<T>(Func<T> targets);
        // void Register<T>(Func<IEnumerable<T>> targets);

        void Unregister<T>(params T[] targets);
        // void Unregister<T>(Func<T> targets);
        // void Unregister<T>(Func<IEnumerable<T>> targets);

        IEnumerable<T> Get<T>();
        IEnumerable<object> Get(Type type);
    }
}