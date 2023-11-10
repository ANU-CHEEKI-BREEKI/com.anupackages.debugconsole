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
using ANU.IngameDebug.Console.Converters;
using System.Threading.Tasks;
using System.Threading;
using ANU.IngameDebug.Console.Dashboard;

namespace ANU.IngameDebug.Console
{
    public class AttributeCommandsInitializer : MonoBehaviour
    {
        [SerializeField] private CommandRegistrationType _commandRegistrationType = CommandRegistrationType.Asynchronous;

        private CancellationTokenSource _cancellationTokenSource;

        private async void Start()
        {
            if (_commandRegistrationType == CommandRegistrationType.Synchronous)
            {
                new AttributeCommandsInitializerProcessor(
                    DebugConsole.Logger,
                    DebugConsole.Commands
                ).Initialize();
            }
            else
            {
                var logger = new UnityLogger(ConsoleLogType.Output);
                var commands = new CommandsRegistry(DebugConsole.Processor);

                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                await Task.Run(() =>
                {
                    new AttributeCommandsInitializerProcessor(
                        logger,
                        commands
                    ).Initialize();
                });

                if (token.IsCancellationRequested)
                    return;

                foreach (var item in commands.Commands.Values)
                    DebugConsole.Commands.RegisterCommand(item);
            }
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    public class AttributeCommandsInitializerProcessor
    {
        public ILogger Logger { get; }
        public ICommandsRegistry Commands { get; set; }

        public AttributeCommandsInitializerProcessor(ILogger logger, ICommandsRegistry commands)
        {
            Commands = commands;
            Logger = logger;
        }

        public void Initialize()
        {
            Logger.LogInfo($"Start searching commands declared by attributes...");
            var timer = StartLog(null);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var timerMethods = StartLog("method");

            Commands.RegisterCommands(
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
                    .Where(method => method.attribute.Platforms.HasCurrentPlatform())
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

            Log(timerMethods, "method");

            var timerProperty = StartLog("property");

            Commands.RegisterCommands(
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
                    .Where(method => method.attribute != null)
                    .Where(method => method.attribute.Platforms.HasCurrentPlatform())
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

            var timerField = StartLog("field");

            Commands.RegisterCommands(
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
                    .Where(method => method.attribute != null)
                    .Where(method => method.attribute.Platforms.HasCurrentPlatform())
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

            Log(timerField, "field");

            Log(timer, null);
            var log = $"Searching commands declared by attributes ended.\nOperation elapsed duration: {timer.Elapsed:ss's.'fff'ms'}, ticks: {timer.ElapsedTicks}";
            if (timer.Elapsed.Seconds < 1)
                Logger.LogInfo(log);
            else if (timer.Elapsed.Seconds < 2)
                Logger.LogWarning(log);
            else
                Logger.LogError(log);
        }

        private Stopwatch StartLog(string name)
        {
            if (name != null)
                Logger.LogInfo($"Start searching {name}...");

            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            return timer;
        }

        private void Log(Stopwatch timer, string name)
        {
            timer.Stop();

            if (name == null)
                return;

            var log = $"Searching {name} ended. Operation elapsed duration: {timer.Elapsed:ss's.'fff'ms'}, ticks: {timer.ElapsedTicks}";
            if (timer.Elapsed.Seconds < 3)
                Logger.LogInfo(log);
            else if (timer.Elapsed.Seconds < 5)
                Logger.LogWarning(log);
            else
                Logger.LogError(log);
        }
    }
}