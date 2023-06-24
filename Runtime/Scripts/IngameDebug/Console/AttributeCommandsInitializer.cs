using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ANU.IngameDebug.Console;
using ANU.IngameDebug.Console.Commands.Implementations;
using NDesk.Options;
using UnityEngine;
using System.Diagnostics;

namespace ANU.IngameDebug.Console
{
    public class AttributeCommandsInitializer : MonoBehaviour
    {
        private void Start()
        {
            var timer = StartLog("");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var timerMethods = StartLog("method");

            DebugConsole.Commands.RegisterCommands(
                assemblies
                    .SelectMany(asm => asm.GetTypes())
                    .Where(t => typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(t))
                    .Concat(assemblies
                        .SelectMany(asm => asm.GetCustomAttributes<RegisterDebugCommandTypesAttribute>())
                        .Where(atr => atr != null)
                        .SelectMany(atr => atr.DeclaredTypes)
                    )
                    .Distinct()
                    .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    .Select(method => new
                    {
                        method = method,
                        attribute = method.GetCustomAttribute<DebugCommandAttribute>()
                    })
                    .Where(method => method.attribute != null)
                    .Select(method => new
                    {
                        method = method.method,
                        name = method.method.GenerateCommandName(),
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

            Log(timer, "method");

            var timerProperty = StartLog("property");

            DebugConsole.Commands.RegisterCommands(
                assemblies
                    .SelectMany(asm => asm.GetTypes())
                    .Where(t => typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(t))
                    .Concat(assemblies
                        .SelectMany(asm => asm.GetCustomAttributes<RegisterDebugCommandTypesAttribute>())
                        .Where(atr => atr != null)
                        .SelectMany(atr => atr.DeclaredTypes)
                    )
                    .Distinct()
                    .SelectMany(type => type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    .Select(field => new
                    {
                        field = field,
                        attribute = field.GetCustomAttribute<DebugCommandAttribute>()
                    })
                    .Where(field => field.attribute != null)
                    .Select(field => new
                    {
                        method = field.field,
                        name = field.field.GenerateCommandName()
                    })
                    .GroupBy(field => field.name)
                    .SelectMany(group =>
                    {
                        // if count greater than 1
                        // add command name prefix as DeclaringType
                        var addPrefix = group.Skip(1).Any();
                        return group.Select(method => new PropertyCommand(
                            property: method.method,
                            instance: null,
                            addPrefix ? method.method.DeclaringType.Name : ""
                        ));
                    })
                    .ToArray()
            );

            Log(timerProperty, "property");

            var timerfield = StartLog("field");

            DebugConsole.Commands.RegisterCommands(
                assemblies
                    .SelectMany(asm => asm.GetTypes())
                    .Where(t => typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(t))
                    .Concat(assemblies
                        .SelectMany(asm => asm.GetCustomAttributes<RegisterDebugCommandTypesAttribute>())
                        .Where(atr => atr != null)
                        .SelectMany(atr => atr.DeclaredTypes)
                    )
                    .Distinct()
                    .SelectMany(type => type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    .Select(field => new
                    {
                        field = field,
                        attribute = field.GetCustomAttribute<DebugCommandAttribute>()
                    })
                    .Where(field => field.attribute != null)
                    .Select(field => new
                    {
                        method = field.field,
                        name = field.field.GenerateCommandName()
                    })
                    .GroupBy(field => field.name)
                    .SelectMany(group =>
                    {
                        // if count greater than 1
                        // add command name prefix as DeclaringType
                        var addPrefix = group.Skip(1).Any();
                        return group.Select(method => new FieldCommand(
                            field: method.method,
                            instance: null,
                            addPrefix ? method.method.DeclaringType.Name : ""
                        ));
                    })
                    .ToArray()
            );

            Log(timerfield, "field");

            Log(timer, "");
        }

        private Stopwatch StartLog(string name)
        {
            DebugConsole.Logger.LogInfo($"Start searching {name} commands declared by attributes...");
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            return timer;
        }
        private void Log(Stopwatch timer, string name)
        {
            timer.Stop();
            var log = $"Searching {name} commands declared by attributes ended.\nOperation elapsed duration: {timer.Elapsed:ss's.'fff'ms'}, ticks: {timer.ElapsedTicks}";
            if (timer.Elapsed.Seconds < 3)
                DebugConsole.Logger.LogInfo(log);
            else if (timer.Elapsed.Seconds < 5)
                DebugConsole.Logger.LogWarning(log);
            else
                DebugConsole.Logger.LogError(log);
        }
    }
}