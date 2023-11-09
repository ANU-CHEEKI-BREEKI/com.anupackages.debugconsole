using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NDesk.Options;
using ANU.IngameDebug.Utils;
using System.Collections;

namespace ANU.IngameDebug.Console.Commands
{
    public abstract class ADebugCommand : IInjectDebugConsoleContext
    {
        private readonly string _name;
        private readonly string _description;

        private string _optionsHint = null;
        private OptionSet _options;

        private bool _printHelp = false;

        private readonly Dictionary<Option, AvailableValuesHint> _valueHints = new Dictionary<Option, AvailableValuesHint>();
        private readonly Dictionary<Option, Action<string>> _optionActions = new();

        public OptionSet Options
        {
            get
            {
                CacheOptions();
                return _options;
            }
        }

        private void PrintHelp() => Logger.LogInfo(GetHelp());

        public string GetHelp()
        {
            using var writer = new StringWriter();
            writer.WriteLine();
            writer.WriteLine($"{"Name:",-15} {Name}");
            writer.WriteLine($"{"Description:",-15} {Description}");
            writer.WriteLine("Options:");
            foreach (var opt in Options)
            {
                writer.Write("    --");
                writer.Write(string.Join("|", opt.GetNames()));

                var val = "=VALUE";
                if (InternalValueHints.ContainsKey(opt))
                    val = "=" + string.Join(", -", InternalValueHints[opt]);

                switch (opt.OptionValueType)
                {
                    case OptionValueType.None:
                        break;
                    case OptionValueType.Optional:
                        writer.Write("[");
                        writer.Write(val);
                        writer.Write("]");
                        break;
                    case OptionValueType.Required:
                        writer.Write(val);
                        break;
                }

                writer.Write("    ");
                writer.WriteLine(opt.Description);
            }

            return writer.ToString();
        }

        public ILogger Logger { get; set; }

        protected ADebugCommand(string name, string description)
        {
            _name = name;
            _description = description;
        }

        public string Name => _name;
        public string Description => _description;
        public string OptionsHint
        {
            get
            {
                if (_optionsHint == null)
                {
                    _optionsHint = string.Join(
                        " ",
                        Options.Select(option => option.OptionValueType != OptionValueType.Required
                            ? $"[{option.Prototype}]"
                            : option.Prototype
                        )
                    );
                }
                return _optionsHint;
            }
        }

        protected Dictionary<Option, AvailableValuesHint> InternalValueHints => _valueHints;
        public IDictionary<Option, AvailableValuesHint> ValueHints
        {
            get
            {
                CacheOptions();
                return _valueHints;
            }
        }

        IReadOnlyDebugConsoleProcessor IInjectDebugConsoleContext.Context { get; set; }
        protected IReadOnlyDebugConsoleProcessor Context => (this as IInjectDebugConsoleContext).Context;

        public ExecutionResult Execute(string args = null)
        {
            if (args == null)
                args = "";

            var options = Options;

            //TODO: preprocess command to define if there are any mandatory option leading -- or -
            // if no - add corresponding option names to match passed parameters
            // this way we will be able to use console like C# methods - pass mandatory parameters without option names
            // or pass with names manually, oor combine, but if we pass any option name - all next parameters should be passed with option names too

            try
            {
                var commandOptions = args.SplitCommandLine();
                var notParsedOptions = options.Parse(commandOptions);

                if (notParsedOptions.Any())
                {
                    var builder = new StringBuilder();
                    builder.AppendLine("there are no options with names:");
                    foreach (var item in notParsedOptions)
                    {
                        builder.Append("    ");
                        builder.AppendLine(item);
                    }
                    builder.AppendLine("maybe you forgot to type -- before option?:");
                    Logger.LogWarning(builder.ToString());
                }
                else
                {
                    if (_printHelp)
                    {
                        _printHelp = false;
                        PrintHelp();
                    }
                    else
                    {
                        return OnParsed();
                    }
                }

            }
            catch (OptionException oex)
            {
                if (Logger != null)
                    Logger.LogError($"{Name}: {oex.Message}.\r\nTry `{Name} --help' for more information.");
                else
                    throw oex;
            }
            catch (Exception ex)
            {
                if (Logger != null)
                    Logger.LogException(ex);
                else
                    throw ex;
            }

            return default;
        }

        protected abstract OptionSet CreateOptions(Dictionary<Option, AvailableValuesHint> valueHints);
        protected abstract ExecutionResult OnParsed();

        private void CacheOptions()
        {
            if (_options == null)
            {
                _options = CreateOptions(_valueHints);
                // add help if already not defined
                if (!_options.Contains("help"))
                    _options.Add("help|h|?", "see this command help", v => _printHelp = v != null);
            }
        }
    }

    public class AvailableValuesHint : IEnumerable<string>
    {
        public AvailableValuesHint(IEnumerable<string> items = null, IEnumerable<string> dynamicValuesProviderCommands = null)
        {
            if (items != null)
                Values = new List<string>(items);

            if (dynamicValuesProviderCommands != null)
                DynamicValueProviderCommands = new List<string>(dynamicValuesProviderCommands);
        }

        private List<string> Values { get; } = new();
        public List<string> DynamicValueProviderCommands { get; } = new();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<string> GetEnumerator()
        {
            foreach (var item in Values)
                yield return item;

            foreach (var command in DynamicValueProviderCommands)
            {
                IEnumerable<string> values = null;
                try
                {
                    var result = DebugConsole.ExecuteCommand(command, silent: true);
                    values = result.ReturnValues
                        .Select(b => b.ReturnValue as IEnumerable)
                        .Where(b => b != null)
                        .SelectMany(b => b.Cast<object>())
                        .Select(b => b?.ToString())
                        .Where(b => b != null);
                }
                catch (Exception ex)
                {
                    DebugConsole.Logger.LogException(ex);
                }

                if (values == null)
                    continue;

                foreach (var item in values)
                    yield return item;
            }
        }

    }
}