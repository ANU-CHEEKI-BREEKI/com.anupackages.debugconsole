using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NDesk.Options;
using ANU.IngameDebug.Utils;

namespace ANU.IngameDebug.Console.Commands
{
    public abstract class ADebugCommand
    {
        private readonly string _name;
        private readonly string _description;

        private string _optionsHint = null;
        private OptionSet _options;

        private bool _printHelp = false;

        private readonly Dictionary<Option, AvailableValuesHint> _valueHints = new Dictionary<Option, AvailableValuesHint>();

        public OptionSet Options
        {
            get
            {
                CacheOptions();
                return _options;
            }
        }

        private void PrintHelp()
        {
            using (var writer = new StringWriter())
            {
                writer.WriteLine("Name:");
                writer.Write("    ");
                writer.WriteLine(Name);

                writer.WriteLine("Description:");
                writer.Write("    ");
                writer.WriteLine(Description);

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

                Logger.Log(writer.ToString());
            }
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
                    _optionsHint = string.Join(", ", Options.Select(option => option.Prototype));
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

        public void Execute(string args = null)
        {
            var options = Options;

            if (string.IsNullOrEmpty(args))
                args = "<>";

            //TODO: preprocess command to define if there are any mandatory option leading -- or -
            // if no - add corresponding option names to match passed parameters
            // this way we will be able to use console like C# methods - pass mandatory parameters without option names
            // or pass with names manually, oor combine, but if we pass any option name - all next parameters should be passed with option names too

            try
            {
                var notParsedOptions = options.Parse(args.SplitCommandLine());

                if (notParsedOptions.Any())
                {
                    var builder = new StringBuilder();
                    builder.AppendLine("there is no options with names:");
                    foreach (var item in notParsedOptions)
                    {
                        builder.Append("    ");
                        builder.AppendLine(item);
                    }
                    builder.AppendLine("maybe you forgot type -- before option?:");
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
                        OnParsed();
                    }
                }

            }
            catch (OptionException oex)
            {
                Logger?.LogError($"{Name}: {oex.Message}.\r\nTry `{Name} --help' for more information.");
            }
            catch (Exception ex)
            {
                var builder = new StringBuilder();
                while (ex != null)
                {
                    builder.AppendLine(ex.Message);
                    ex = ex.InnerException;
                }
                Logger?.LogError(builder.ToString());
            }
        }

        protected abstract OptionSet CreateOptions(Dictionary<Option, AvailableValuesHint> valueHints);
        protected abstract void OnParsed();

        private void CacheOptions()
        {
            if (_options == null)
            {
                _options = CreateOptions(_valueHints);
                // add help if already not defined
                if (!_options.Contains("help"))
                    _options.Add("help|?|h", "see this command help", v => _printHelp = v != null);
            }
        }
    }

    public class AvailableValuesHint : List<string>
    {
        public AvailableValuesHint() { }
        public AvailableValuesHint(IEnumerable<string> items) : base(items) { }
    }
}