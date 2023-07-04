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
using static ANU.IngameDebug.Utils.Extensions.ColumnAlignment;
using static ANU.IngameDebug.Utils.Extensions;

[assembly: RegisterDebugCommandTypes(typeof(DefinesPreprocessor))]

namespace ANU.IngameDebug.Console
{
    public class DefinesPreprocessor : ICommandInputPreprocessor, IInjectDebugConsoleContext
    {
        private readonly Regex _defineNameRegex = new(@"(?<name>#[\w\d\-\._]+?)(?![\w\d\-\._])");

        private int _forceProcess;
        private IReadOnlyDebugConsoleProcessor _context;

        int ICommandInputPreprocessor.Priority => 500;
        public IReadOnlyDebugConsoleProcessor Context
        {
            get => _context;
            set
            {
                _context = value;
                _context.InstanceTargets.Register(this);
            }
        }

        public string Preprocess(string input)
        {
            input = input.Trim();

            // skip evaluation for #define commands
            if (_forceProcess <= 0 && input.StartsWith("#"))
            {
                var name = input.SplitCommandLine().FirstOrDefault();
                if (Context.Commands.Commands.ContainsKey(name))
                    return input;
            }

            if (!_defineNameRegex.IsMatch(input))
                return input;

            using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
            {
                //WE can not use thread, because commands can access UnityEngine code
                // which can be accessible only from work thread
                try
                {
                    while (_defineNameRegex.IsMatch(input) && !tokenSource.IsCancellationRequested)
                    {
                        input = _defineNameRegex.Replace(input, m =>
                        {
                            try
                            {
                                _forceProcess++;

                                if (Context.Defines.Defines.TryGetValue(m.Value.Trim('"').Trim('#'), out var val))
                                    return Context.Preprocessors.Preprocess(val);
                                else
                                    return m.Value;
                            }
                            catch (Exception ex)
                            {
                                Context.Logger.LogException(ex);
                                return m.Value;
                            }
                            finally
                            {
                                _forceProcess--;
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Context.Logger.LogException(ex);
                }

                if (tokenSource.IsCancellationRequested)
                    Context.Logger.LogWarning($"Define replaceing timeout hitted. End result are invalid: {input}");
            }

            return input;
        }

        [DebugCommand(Name = "#echo", Description = @"Print defined value without evaluation")]
        private string EchoDefine(string name)
        {
            if (Context.Defines.Defines.TryGetValue(name.Trim('#'), out var val))
                return val;

            throw new Exception($"{name} not defined");
        }

        [DebugCommand(Name = "#define", Description = @"Define custom value by given name")]
        private void Define(string name, string value)
            => Context.Defines.Add(name, value);

        [DebugCommand(Name = "#undefine", Description = "Remove defined value with associated name")]
        private void Undefine(string name)
            => Context.Defines.Remove(name);

        [DebugCommand(Name = "#clear", Description = "Clear all defined values")]
        private void DefinesClear() => Context.Defines.Clear();

        [DebugCommand(Name = "#list", Description = "Print all defined values")]
        private void DefinesList()
        {
            if (!Context.Defines.Defines.Any())
                Context.Logger.LogReturnValue("There are no any defines defined");

            var sb = new StringBuilder();
            sb.AppendLine();

            sb.PrintTable(
                Context.Defines.Defines,
                new string[] { "Define", "Value" },
                item => new string[] { item.Key, item.Value },
                new ColumnAlignment[] { Right, Left }
            );

            Context.Logger.LogReturnValue(sb);
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