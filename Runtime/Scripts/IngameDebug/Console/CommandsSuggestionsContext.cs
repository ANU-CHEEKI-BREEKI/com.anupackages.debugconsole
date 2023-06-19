using System;
using System.Collections.Generic;
using System.Linq;
using ANU.IngameDebug.Console.Commands;
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
            var names = input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 1)
                return base.GetSuggestions(input);

            var command = _commands.FirstOrDefault(c => c.Value.Name == names.FirstOrDefault()).Value;
            if (command != null)
            {
                var last = names.Last();
                var options = last.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

                if (last.Contains('='))
                    options = options.Append("").ToArray();

                var optionName = options.First().TrimStart('-').Trim(' ');
                if (options.Length == 1)
                {
                    if (options.First().StartsWith("--"))
                        return new ComandParameterNameSuggestionContext(command).GetSuggestions(optionName);
                }
                else
                {
                    var param = command.Options.FirstOrDefault(o => Array.IndexOf(o.GetNames(), optionName) > -1);
                    if (param != null)
                        return new ComandParameterValueSuggestionContext(command, param).GetSuggestions(options.Last().Trim(' '));
                }
            }

            return base.GetSuggestions(input);
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
            var last = fullInput.LastIndexOf(paramStr);
            if (last > -1)
                fullInput = fullInput.Substring(0, last);
            return fullInput + paramStr + (item.Source as Option).GetNames().First();
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
            return fullInput + paramStr + item.Source as string + " ";
        }
    }

}