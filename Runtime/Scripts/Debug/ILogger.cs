using System;
using UnityEngine;

public interface ILogger
{
    void Log(string message, object context = null);
    void LogWarning(string message, object context = null);
    void LogError(string message, object context = null);
    void LogException(Exception excepion, object context = null);
}

public class UnityLogger : ILogger
{
    public void Log(string message, object context) => Debug.Log(message, context as UnityEngine.Object);
    public void LogWarning(string message, object context) => Debug.LogWarning(message, context as UnityEngine.Object);
    public void LogError(string message, object context) => Debug.LogError(message, context as UnityEngine.Object);
    public void LogException(Exception excepion, object context) => Debug.LogException(excepion, context as UnityEngine.Object);
}