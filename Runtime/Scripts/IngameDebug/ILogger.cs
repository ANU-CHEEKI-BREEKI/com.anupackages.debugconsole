using System;
using UnityEngine;

namespace ANU.IngameDebug
{
    public interface ILogger
    {
        void Log(string message, UnityEngine.Object context = null);
        void LogWarning(string message, UnityEngine.Object context = null);
        void LogError(string message, UnityEngine.Object context = null);
        void LogException(Exception exception, UnityEngine.Object context = null);
    }
}