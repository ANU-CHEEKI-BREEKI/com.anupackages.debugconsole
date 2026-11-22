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
            var buildList = CommandMembers.LoadBuildList();

            if (_commandRegistrationType == CommandRegistrationType.Synchronous)
            {
                new AttributeCommandsInitializerProcessor(
                    DebugConsole.Logger,
                    DebugConsole.Commands
                ).Initialize(buildList);
            }
            else
            {
                var logger = new UnityLogger(ConsoleLogType.Output);
                var commands = new CommandsRegistry(DebugConsole.Processor);

                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                await ConsoleTaskWrapper.Run(() =>
                {
                    new AttributeCommandsInitializerProcessor(
                        logger,
                        commands
                    ).Initialize(buildList);
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

        public void Initialize() => Initialize(null);

        public void Initialize(string buildList)
        {
            var timer = Stopwatch.StartNew();
            IReadOnlyList<MemberInfo> members;
            string source;

            if (buildList != null)
            {
                var (listed, missing) = CommandMembers.Read(buildList);
                members = listed;
                source = $"the build list ({listed.Count} members)";

                if (missing.Count > 0)
                    Logger.LogWarning($"{missing.Count} members of the build command list are not in this build: {string.Join("; ", missing.Take(10))}");
            }
            else
            {
                var assemblies = CommandMembers.CommandAssemblies(AppDomain.CurrentDomain.GetAssemblies());
                members = CommandMembers.Scan(assemblies);
                source = $"{assemblies.Length} assemblies";
            }

            var commands = CommandMembers.Create(members);
            Commands.RegisterCommands(commands.ToArray());

            Logger.LogInfo($"Commands declared by attributes: {commands.Count} from {source} in {timer.Elapsed.TotalSeconds:0.000} s");
        }
    }

    public static class ConsoleTaskWrapper
    {
        public static Task Run(Action action)
        {
            // no thread pool on WebGL - run synchronously
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                action.Invoke();
                return Task.CompletedTask;
            }

            return Task.Run(action);
        }
    }
}