using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NDesk.Options;
using IngameDebug.Utils;

namespace IngameDebug.Commands
{
    public abstract class ADebugCommand
    {
        private readonly string _name;
        private readonly string _description;

        private string _optionsHint = null;
        private OptionSet _options;

        private bool _printHelp = false;

        private OptionSet Options
        {
            get
            {
                if (_options == null)
                {
                    _options = CreateOptions();
                    // add help if already not defined
                    if (!_options.Contains("help"))
                        _options.Add("help", "see this command help", v => _printHelp = v != null);
                }
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
                _options.WriteOptionDescriptions(writer);

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

        public void Execute(string args = null)
        {
            var options = Options;

            if (string.IsNullOrEmpty(args))
                args = "<>";

            try
            {
                var notParcedOptions = options.Parse(args.SplitCommandLine());

                if (notParcedOptions.Any())
                {
                    var builder = new StringBuilder();
                    builder.AppendLine("there is no options with names:");
                    foreach (var item in notParcedOptions)
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

        protected abstract OptionSet CreateOptions();
        protected abstract void OnParsed();
    }
}