using System;
using UnityEngine;

namespace ANU.IngameDebug
{
    public interface ILogger
    {
        void Log(string message, object context = null);
        void LogWarning(string message, object context = null);
        void LogError(string message, object context = null);
        void LogException(Exception exception, object context = null);
    }

    public class UnityLogger : ILogger
    {
        public void Log(string message, object context) => Debug.Log(message, context as UnityEngine.Object);
        public void LogWarning(string message, object context) => Debug.LogWarning(message, context as UnityEngine.Object);
        public void LogError(string message, object context) => Debug.LogError(message, context as UnityEngine.Object);
        public void LogException(Exception exception, object context) => Debug.LogException(exception, context as UnityEngine.Object);
    }
}