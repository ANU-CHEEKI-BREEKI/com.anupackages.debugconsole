using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ANU.IngameDebug.Console;
using ANU.IngameDebug.Console.Commands.Implementations;
using NDesk.Options;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    public class AttributeCommandsInitializer : MonoBehaviour
    {
        private void Start()
        {
#if DEBUG
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            DebugConsole.Logger.Log("Start searching commands declared by attributes...");
#endif

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            DebugConsole.RegisterCommands(
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(asm => asm.GetTypes())
                    .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    .Select(method => new
                    {
                        method = method,
                        attribute = method.GetCustomAttribute<DebugCommandAttribute>(),
                    })
                    .Where(method => method.attribute != null)
                    .Select(method => new
                    {
                        method = method.method,
                        name = MethodCommand.GetName(method.method),
                    })
                    .GroupBy(method => method.name)
                    .SelectMany(group =>
                    {
                        // if count greater than 1
                        // add command name prefix as DeclaringType
                        var addPrefix = group.Skip(1).Any();
                        return group.Select(method => new MethodCommand(
                            method: method.method,
                            instance: null,
                            addPrefix ? method.method.DeclaringType.Name : ""
                        ));
                    })
                    .ToArray()
            );

#if DEBUG
            timer.Stop();
            var log = $"Searching commands declared by attributes ended.\nOperation elapsed duration: {timer.Elapsed:ss's :'fff'ms'}, ticks: {timer.ElapsedTicks}";
            if (timer.Elapsed.Seconds < 3)
                DebugConsole.Logger.Log(log);
            else if (timer.Elapsed.Seconds < 5)
                DebugConsole.Logger.LogWarning(log);
            else
                DebugConsole.Logger.LogError(log);
#endif
        }
    }
}