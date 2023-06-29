using System;
using System.Collections.Generic;
using ANU.IngameDebug.Console.CommandLinePreprocessors;
using ANU.IngameDebug.Console;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using ANU.IngameDebug.Utils;

[assembly: RegisterDebugCommandTypes(typeof(DefinesPreprocessor))]

namespace ANU.IngameDebug.Console
{
    public class DefinesPreprocessor : ICommandInputPreprocessor
    {
        private readonly Regex _defineNameRegex = new(@"(?<name>#[\w\d\-\._]+?)(?![\w\d\-\._])");

        int ICommandInputPreprocessor.Priority => 500;

        public string Preprocess(string input)
        {
            input = input.Trim();

            // skip evaluation for #define commands
            if (input.StartsWith("#"))
            {
                var name = input.SplitCommandLine().FirstOrDefault();
                if (DebugConsole.Commands.Commands.ContainsKey(name))
                    return input;
            }

            if (input.StartsWith("#") && input.Skip(1).FirstOrDefault() != ' ')
                return $"# --name=\"{input}\"";

            if (!_defineNameRegex.IsMatch(input))
                return input;

            using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
            using (var waiter = new AutoResetEvent(false))
            {
                ThreadPool.QueueUserWorkItem(obj =>
                {
                    while (_defineNameRegex.IsMatch(input) && !tokenSource.IsCancellationRequested)
                    {
                        input = _defineNameRegex.Replace(input, m =>
                        {
                            if (DebugConsole.Defines.Defines.TryGetValue(m.Value.Trim('"').Trim('#'), out var val))
                                return $"{val}";
                            else
                                return m.Value;
                        });
                    }
                    waiter.Set();
                });

                waiter.WaitOne();

                if (tokenSource.IsCancellationRequested)
                    Debug.LogWarning($"Define replaceing timeout hitted. End result are invalid: {input}");
            }

            return input;
        }

        //  echo --value="#2x2x2"
        [DebugCommand(
            Name = "#",
            Description = @"Print defined value without evaluation.
You can use this command as ""#name"" or ""# name"""
        )]
        private static string EchoDefine(string name)
        {
            if (DebugConsole.Defines.Defines.TryGetValue(name.Trim('#'), out var val))
                return val;

            throw new Exception($"{name} not defined");
        }

        [DebugCommand(
            Name = "#define",
            Description = @"Define custom value by given name.
Later you can enter ""#name"" as parameter and name will be replaced by actual defined value"
        )]
        private static void Define(string name, string value)
            => DebugConsole.Defines.Add(name, value);

        [DebugCommand(Name = "#undefine", Description = "Remove defined value with associated name")]
        private static void Undefine(string name)
            => DebugConsole.Defines.Remove(name);

        [DebugCommand(Name = "#clear", Description = "Clear all defined values")]
        private static void DefinesClear() => DebugConsole.Defines.Clear();

        [DebugCommand(Name = "#list", Description = "Print all defined values")]
        private static string DefinesList()
        {
            if (!DebugConsole.Defines.Defines.Any())
                return "There are no defines defined";

            var sb = new StringBuilder();

            var nameLen = DebugConsole.Defines.Defines.Keys.Max(k => k.Length);
            var addSpace = 6;
            var lastPart = addSpace / 2f;

            sb.Append(" define");
            for (int i = 0; i < (nameLen - 3) / 2; i++)
                sb.Append(" ");
            sb.Append("|");
            for (int i = 0; i < (nameLen - 3) / 2; i++)
                sb.Append(" ");
            sb.AppendLine("value");
            sb.AppendLine("___________________________________");

            foreach (var item in DebugConsole.Defines.Defines)
            {
                var fulLen = nameLen + addSpace - item.Key.Length;
                var secondHalf = lastPart;
                var firstHalf = fulLen - lastPart;

                sb.Append("#");
                sb.Append(item.Key);
                for (int i = 0; i < firstHalf; i++)
                    sb.Append(" ");
                sb.Append("|");
                for (int i = 0; i < secondHalf; i++)
                    sb.Append(" ");
                sb.AppendLine(item.Value);
            }

            return sb.ToString();
        }
    }

    public interface IDefinesRegistry
    {
        IReadOnlyDictionary<string, string> Defines { get; }

        void Add(string name, string value);
        void Remove(string name);
        void Clear();
    }

    internal class DefinesRegistry : IDefinesRegistry
    {
        private readonly Dictionary<string, string> _defines = new();
        public IReadOnlyDictionary<string, string> Defines => _defines;

        public void Add(string name, string value) => _defines[name] = value;

        //TODO: also remove all demendant defines???
        public void Remove(string name) => _defines.Remove(name);
        public void Clear() => _defines.Clear();
    }
}