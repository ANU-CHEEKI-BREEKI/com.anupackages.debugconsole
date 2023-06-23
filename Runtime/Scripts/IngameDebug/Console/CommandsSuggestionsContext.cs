using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            if (!names.Any())
                return base.GetSuggestions(input);

            if (!_commands.TryGetValue(names.First(), out var command))
                return base.GetSuggestions(input);

            var last = names.Skip(1).LastOrDefault() ?? "";

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
                    if (prev != null && prev.Trim().StartsWith("-"))
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

            valueName = valueName?.Trim('"')?.Trim('\'');
            var trimOption = optionName?.TrimStart('-'); ;

            var option = command.Options.FirstOrDefault(o => Array.IndexOf(o.GetNames(), trimOption) > -1);

            if (option == null && optionName.Trim().StartsWith("-"))
                return new ComandParameterNameSuggestionContext(command, input).GetSuggestions(trimOption ?? "");

            if (option == null)
            {
                var namedParameterGroup = input.GetFirstNamedParameter();

                if (!namedParameterGroup.Success)
                {
                    var parametersCount = names.Count();
                    if (command.Options.Count >= parametersCount)
                        option = command.Options[parametersCount - 1];
                }
            }

            if (option != null)
                return new ComandParameterValueSuggestionContext(command, option).GetSuggestions(valueName?.Trim(' ') ?? "");

            return base.GetSuggestions(input);
        }


        private class ComandParameterNameSuggestionContext : ASuggestionContext<Option>
        {
            private readonly ADebugCommand _command;
            private readonly string _fullInput;

            public ComandParameterNameSuggestionContext(ADebugCommand command, string fullInput)
            {
                _fullInput = fullInput;
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

                var delimiter = paramStr;
                var name = (item.Source as Option).GetNames().First();

                if (lastParam > -1)
                {
                    fullInput = fullInput.Substring(0, lastParam);
                }
                else if (lastOpt > -1)
                {
                    fullInput = fullInput.Substring(0, lastOpt);
                    delimiter = optString;
                    name = (item.Source as Option).GetNames().OrderBy(d => d.Length).First();
                }

                return $"{fullInput}{delimiter}{name}";
            }

            private protected override IEnumerable<Option> FilterItems(IEnumerable<Option> items, string input, Func<Option, string> filteredStringGetter)
            {
                // find existing names to exclude from suggestions
                IEnumerable<Option> names;

                var noNamedParameters = "";
                var getNamed = _fullInput;
                var doNOtSkip = false;

                var namedParameter = _fullInput.GetFirstNamedParameter();
                if (namedParameter.Success)
                {
                    noNamedParameters = _fullInput.Substring(0, namedParameter.Index);
                    getNamed = _fullInput.Substring(noNamedParameters.Length);
                    doNOtSkip = true;
                }

                var values = noNamedParameters.SplitCommandLine().Skip(1);
                var cnt = values.Count();
                names = _command.Options.Take(cnt);

                values = getNamed.SplitCommandLine();
                if (!doNOtSkip)
                    values = values.Skip(1);
                    
                var set = values.Where(v => v.Trim().StartsWith("-")).Select(p => p.Trim('-')).ToHashSet();
                names = names.Concat(
                    _command.Options.Where(o => o.GetNames().Any(n => set.Contains(n)))
                );

                var namesSet = names.ToHashSet();

                var preFiltered = base.FilterItems(items, input, filteredStringGetter);
                return preFiltered.Where(p => !namesSet.Contains(p));
            }
        }

        private class ComandParameterValueSuggestionContext : ASuggestionContext<string>
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

                var lastOption = fullInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                if (lastOption == null || !lastOption.StartsWith("-"))
                    paramStr = " ";

                return $"{fullInput}{paramStr}{item.Source} ";
            }
        }
    }
}