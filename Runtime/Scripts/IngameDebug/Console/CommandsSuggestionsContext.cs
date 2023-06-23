using System;
using System.Collections.Generic;
using System.Linq;
using ANU.IngameDebug.Console.Commands;
using ANU.IngameDebug.Utils;
using NDesk.Options;

namespace ANU.IngameDebug.Console
{
    public class CommandsSuggestionsContext : ASuggestionContext<ADebugCommand>
    {
        private readonly IReadOnlyDictionary<string, ADebugCommand> _commands;

        public CommandsSuggestionsContext(IReadOnlyDictionary<string, ADebugCommand> commands)
            => _commands = commands;

        public override string Title => "commands";

        protected override IEnumerable<ADebugCommand> Collection => _commands.Values;

        protected override string GetDisplayName(ADebugCommand item) => $"{item.Name} {item.OptionsHint}";
        protected override string GetFilteringName(ADebugCommand item) => item.Name;
        protected override string GetFullSuggestedText(Suggestion item, string fullInput) => (item.Source as ADebugCommand).Name + " ";

        public override IEnumerable<Suggestion> GetSuggestions(string input)
        {
            input = input.Trim(' ');

            var names = input.SplitCommandLine();
            if (!names.Any() || !names.Skip(1).Any())
                return base.GetSuggestions(input);

            if (!_commands.TryGetValue(names.First(), out var command))
                return base.GetSuggestions(input);

            var last = names.Last();

            // if last is named parameter with value

            var optionName = "";
            var valueName = "";

            if (last.Contains("="))
            {
                var split = last.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                optionName = split.FirstOrDefault();
                valueName = split.Skip(1).LastOrDefault();
            }
            else
            {
                if (last.StartsWith("-"))
                {
                    optionName = last;
                    valueName = null;
                }
                else
                {
                    valueName = last;
                    var prev = names.Reverse().Skip(1).Take(1).FirstOrDefault();
                    if (prev != null)
                    {
                        optionName = prev;
                    }
                    else
                    {
                        optionName = last;
                        valueName = null;
                    }
                }
            }

            optionName = optionName?.TrimStart('-');

            var option = command.Options.FirstOrDefault(o => Array.IndexOf(o.GetNames(), optionName) > -1);
            if (option != null)
                return new ComandParameterValueSuggestionContext(command, option).GetSuggestions(valueName?.Trim(' ') ?? "");

            return new ComandParameterNameSuggestionContext(command).GetSuggestions(optionName ?? "");
        }
    }

    public class ComandParameterNameSuggestionContext : ASuggestionContext<Option>
    {
        private readonly ADebugCommand _command;

        public ComandParameterNameSuggestionContext(ADebugCommand command)
        {
            _command = command;
        }

        public override string Title => _command.Name;

        protected override IEnumerable<Option> Collection => _command.Options;

        protected override string GetDisplayName(Option item) => string.Join("|", item.GetNames());
        protected override string GetFilteringName(Option item) => item.GetNames().First();

        protected override string GetFullSuggestedText(Suggestion item, string fullInput)
        {
            var paramStr = " --";
            var optString = " -";

            var lastParam = fullInput.LastIndexOf(paramStr);
            var lastOpt = fullInput.LastIndexOf(optString);

            var delimiter = "";
            var name = "";

            if (lastParam > -1)
            {
                fullInput = fullInput.Substring(0, lastParam);
                delimiter = paramStr;
                name = (item.Source as Option).GetNames().First();
            }
            else if (lastOpt > -1)
            {
                fullInput = fullInput.Substring(0, lastOpt);
                delimiter = optString;
                name = (item.Source as Option).GetNames().OrderBy(d => d.Length).First();
            }

            return $"{fullInput}{delimiter}{name}";
        }
    }

    public class ComandParameterValueSuggestionContext : ASuggestionContext<string>
    {
        private readonly ADebugCommand _command;
        private readonly Option _option;

        public ComandParameterValueSuggestionContext(ADebugCommand command, Option option)
        {
            _option = option;
            _command = command;
        }

        public override string Title => _command.Name;

        protected override IEnumerable<string> Collection
        {
            get
            {
                if (_command.ValueHints.ContainsKey(_option))
                    return _command.ValueHints[_option];
                else
                    return Array.Empty<string>();
            }
        }

        protected override string GetDisplayName(string item) => item;
        protected override string GetFilteringName(string item) => item;

        protected override string GetFullSuggestedText(Suggestion item, string fullInput)
        {
            var paramStr = "=";
            var last = fullInput.LastIndexOf(paramStr);
            if (last > -1)
                fullInput = fullInput.Substring(0, last);

            fullInput = fullInput.Trim();

            return $"{fullInput}{paramStr}{item.Source} ";
        }
    }

}