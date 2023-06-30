using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ANU.IngameDebug.Console.Commands;
using ANU.IngameDebug.Console.Commands.Implementations;
using NDesk.Options;

namespace ANU.IngameDebug.Console.Converters
{
    public interface ICommandMetaData
    {
        IReadOnlyList<IOptionMetaData> Options { get; }
    }

    public interface IOptionMetaData
    {
        [Obsolete("Not implemented yet. Because of bug in OptionSet.Remove method", error: true)]
        public HashSet<string> Names { get; set; }
        [Obsolete("Not implemented yet. Because of bug in OptionSet.Remove method", error: true)]
        public string Description { get; set; }

        public HashSet<string> AvailableValues { get; set; }
    }

    public interface IReadOnlyCommandsRegistry
    {
        IReadOnlyDictionary<string, ADebugCommand> Commands { get; }
    }

    public interface ICommandsRegistry : IReadOnlyCommandsRegistry
    {
        void RegisterCommands(params ADebugCommand[] commands);
        void RegisterCommand(ADebugCommand command);

        void RegisterCommand(Delegate command, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand(string name, string description, Delegate command, Action<ICommandMetaData> metaDataCustomize = null);

        void RegisterCommand(string name, string description, Action command, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand<T1>(string name, string description, Action<T1> command, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand<T1, T2>(string name, string description, Action<T1, T2> command, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand<T1, T2, T3>(string name, string description, Action<T1, T2, T3> command, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand<T1, T2, T3, T4>(string name, string description, Action<T1, T2, T3, T4> command, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand<T1>(string name, string description, Func<T1> command, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand<T1, T2>(string name, string description, Func<T1, T2> command, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand<T1, T2, T3>(string name, string description, Func<T1, T2, T3> command, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand<T1, T2, T3, T4>(string name, string description, Func<T1, T2, T3, T4> command, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand<T1, T2, T3, T4, T5>(string name, string description, Func<T1, T2, T3, T4, T5> command, Action<ICommandMetaData> metaDataCustomize = null);

        void RegisterCommand(string methodName, Type ownerType, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand(string methodName, object instance, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand(string name, string description, string methodName, Type ownerType, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand(string name, string description, string methodName, object instance, Action<ICommandMetaData> metaDataCustomize = null);

        void RegisterCommand(string name, string description, MethodInfo method, object instance = null, Action<ICommandMetaData> metaDataCustomize = null);
        void RegisterCommand(MethodInfo method, object instance = null, Action<ICommandMetaData> metaDataCustomize = null);
    }

    internal class CommandsRegistry : ICommandsRegistry
    {
        private readonly Dictionary<string, ADebugCommand> _commands = new();

        public CommandsRegistry(IReadOnlyDebugConsoleProcessor context) => Context= context;

        private IReadOnlyDebugConsoleProcessor Context{ get; }
        public IReadOnlyDictionary<string, ADebugCommand> Commands => _commands;

        private class CommandMetaData : ICommandMetaData
        {
            public IReadOnlyList<IOptionMetaData> Options { get; set; }
        }

        private class OptionMetaData : IOptionMetaData
        {
            public Option Key { get; set; }
            public HashSet<string> Names { get; set; }
            public string Description { get; set; }
            public HashSet<string> AvailableValues { get; set; }
        }

        public void RegisterCommands(params ADebugCommand[] commands)
        {
            foreach (var command in commands)
                RegisterCommand(command);
        }

        public void RegisterCommand(ADebugCommand command)
        {
            _commands[command.Name] = command;
            command.Logger = Context.Logger;
            
            if(command is IInjectDebugConsoleContext consoleContext)
                consoleContext.Context = Context;
        }

        public void RegisterCommand(string name, string description, Action command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand(Delegate command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand(string name, string description, Delegate command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand<T1>(string name, string description, Action<T1> command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand<T1, T2>(string name, string description, Action<T1, T2> command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand<T1, T2, T3>(string name, string description, Action<T1, T2, T3> command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand<T1, T2, T3, T4>(string name, string description, Action<T1, T2, T3, T4> command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand<T1>(string name, string description, Func<T1> command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand<T1, T2>(string name, string description, Func<T1, T2> command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand<T1, T2, T3>(string name, string description, Func<T1, T2, T3> command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand<T1, T2, T3, T4>(string name, string description, Func<T1, T2, T3, T4> command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand<T1, T2, T3, T4, T5>(string name, string description, Func<T1, T2, T3, T4, T5> command, Action<ICommandMetaData> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public void RegisterCommand(string name, string description, MethodInfo method, object target = null, Action<ICommandMetaData> metaDataCustomize = null)
        {
            var methodCommand = new MethodCommand(name, description, method, target);
            RegisterCommand(methodCommand, metaDataCustomize);
        }

        public void RegisterCommand(MethodInfo method, object target = null, Action<ICommandMetaData> metaDataCustomize = null)
        {
            var methodCommand = new MethodCommand(method, target);
            RegisterCommand(methodCommand, metaDataCustomize);
        }

        public void RegisterCommand(string methodName, Type ownerType, Action<ICommandMetaData> metaDataCustomize = null)
        {
            var method = ownerType?.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
                RegisterCommand(method, null, metaDataCustomize);
        }

        public void RegisterCommand(string name, string description, string methodName, Type ownerType, Action<ICommandMetaData> metaDataCustomize = null)
        {
            var method = ownerType?.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
                RegisterCommand(name, description, method, null, metaDataCustomize);
        }

        public void RegisterCommand(string methodName, object instance, Action<ICommandMetaData> metaDataCustomize = null)
        {
            var method = instance?.GetType()?.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
                RegisterCommand(method, null, metaDataCustomize);
        }

        public void RegisterCommand(string name, string description, string methodName, object instance, Action<ICommandMetaData> metaDataCustomize = null)
        {
            var method = instance?.GetType()?.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
                RegisterCommand(name, description, method, null, metaDataCustomize);
        }

        private void RegisterCommand(MethodCommand command, Action<ICommandMetaData> metaDataCustomize)
        {
            var methodCommand = command;
            var metaData = new CommandMetaData
            {
                Options = methodCommand.Options.Select(v => new OptionMetaData
                {
                    Key = v,
                    Names = v.GetNames().ToHashSet(),
                    Description = v.Description,
                    AvailableValues = (methodCommand.ValueHints.TryGetValue(v, out var av) ? av : new AvailableValuesHint()).ToHashSet()
                }).ToArray()
            };

            metaDataCustomize?.Invoke(metaData);

            for (int i = 0; i < metaData.Options.Count; i++)
            {
                var optionMetaData = metaData.Options[i] as OptionMetaData;

                // methodCommand.Options.Remove(optionMetaData.Key);

                // var names = optionMetaData.Names.ToArray();
                // var name = string.Join("|", names);
                // name += optionMetaData.Key.OptionValueType switch
                // {
                //     OptionValueType.Optional => ":",
                //     OptionValueType.Required => "=",
                //     _ => ""
                // };

                // var action = methodCommand.GetOptionAction(optionMetaData.Key);
                // methodCommand.Options.Add(
                //     name,
                //     optionMetaData.Description,
                //     action
                // );
                // var option = methodCommand.Options[names[0]];
                // methodCommand.ReplaceOptionActionKey(optionMetaData.Key, option);

                // methodCommand.ValueHints.Remove(optionMetaData.Key);

                methodCommand.ValueHints[optionMetaData.Key] = new AvailableValuesHint(optionMetaData.AvailableValues);
            }

            RegisterCommand(methodCommand);
        }

    }
}