using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IngameDebug.Commands.Implementations;
using NDesk.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IngameDebug.Utils;
using System.Text.RegularExpressions;

namespace IngameDebug.Commands.Console
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(DebugCommandsContainer))]
    public class DebugConsole : Singletone<DebugConsole>, ILogger
    {
        [SerializeField] private GameObject _content;
        [SerializeField] private TMP_InputField _input;
        [Space]
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private TextMeshProUGUI _consoleLog;
        [Space]
        [SerializeField] private SuggestionPopUp _suggestions;
        [Space]
        [SerializeField] private Color _warnings;
        [SerializeField] private Color _errors;
        [SerializeField] private Color _exceptions;

        private DebugCommandsContainer _commandsContainer;

        private const float _colsoleInputHeightPercentage = 0.1f;
        private const float _padding = 10f;

        private ILogger Logger = new UnityLogger();

        private CommandLineHistory _commandsHistory = new CommandLineHistory();

        private ISuggestionsContext _suggestionsContext;
        private CommandsSuggestionsContext _commandsContext;
        private HistorySuggestionsContext _historyContext;

        private ISuggestionsContext SuggestionsContext
        {
            get => _suggestionsContext;
            set
            {
                _suggestionsContext = value;
                _suggestions.Title = _suggestionsContext?.Title;
            }
        }

        public static void RegisterCommands(params ADebugCommand[] commands)
        {
            Instance._commandsContainer.RegisterCommands(commands);
            foreach (var command in commands)
                command.Logger = Instance;
        }

        protected override void Initialize()
        {
            _commandsContainer = GetComponent<DebugCommandsContainer>();
            _commandsContext = new CommandsSuggestionsContext(_commandsContainer);
            _historyContext = new HistorySuggestionsContext(_commandsHistory);
            _content.SetActive(false);

            SetupConsoleCommands();

            foreach (var command in _commandsContainer.Commands.Values)
                command.Logger = this;

            _input.onSubmit.AddListener(text =>
            {
                if (string.IsNullOrEmpty(text))
                    return;
                ExecuteCommand(text);
            });
            _input.onValueChanged.AddListener(text =>
            {
                if (text != _commandsHistory.Current)
                    _commandsHistory.Reset();

                DisplaySuggestions(text);
            });

            _suggestions.Choosen += s =>
            {
                _input.text = s.ApplySuggestion(_input.text);

                _input.caretPosition = _input.text.Length;
                _input.ActivateInputField();
            };

            _suggestions.Hided += () => SuggestionsContext = _commandsContext;

            ExecuteCommand("clear");
            ExecuteCommand("help");

            LoadCommandsHistory(_commandsHistory);
        }

        private void OnApplicationQuit()
        {
            SaveCommandsHistory(_commandsHistory);
        }

        private void ExecuteCommand(string commandLine)
        {
            try
            {
                _input.text = "";
                _commandsHistory.Record(commandLine);
                this.Log("> " + commandLine);

                var commandName = ExtractCommandName(commandLine);

                if (!_commandsContainer.Commands.ContainsKey(commandName))
                {
                    this.LogError($"There is no command with name \"{commandName}\". Enter \"help\" to see command usage.");
                    return;
                }

                var command = _commandsContainer.Commands[commandName];

                // remove command name from commanr line input
                if (commandLine != null)
                {
                    var nameIndex = commandLine.IndexOf(command.Name);
                    commandLine = commandLine
                        .Remove(0, command.Name.Length)
                        .Trim();
                }

                command.Execute(commandLine);
            }
            finally
            {
                _input.ActivateInputField();
            }
        }

        private static string ExtractCommandName(string commandLine)
        {
            var commandName = commandLine;
            var spaceindex = commandLine.IndexOf(" ");
            if (spaceindex >= 0)
                commandName = commandLine.Substring(0, spaceindex);
            return commandName;
        }

        private void SetupConsoleCommands()
        {
            DebugConsole.RegisterCommands(
                new LambdaCommand("help", "print help", () => this.Log(
$@"To call command                 - enter command name and parameters
    for example: ""command_name -param_name_1 -param_name_2""
To see all commands              - enter command ""list""
To see concrete command help     - enter command name with parameter ""-help""
---------------------------------
To force show suggestions       - press ""Ctrl + .""
To switch suggestions context   - press ""Ctrl + ~""
    available contexts: commands, history
To select suggestions           - use ArrowUp and ArrowDown
To choose suggestion            - press Tab or Enter
To choose first suggestion      - press Tab when no selected suggestions
To search history               - use ArrowUp and ArrowDown when suggestions not shown
---------------------------------
")),
                new LambdaCommand("list", "pring all command names with descriptions", () =>
                {
                    var builder = new StringBuilder();
                    foreach (var command in _commandsContainer.Commands.Values)
                    {
                        builder.Append("  - ");
                        builder.AppendLine(command.Name);
                        builder.Append("        ");
                        builder.AppendLine(command.Description);
                    }
                    this.Log(builder.ToString());
                }),
                new LambdaCommand("clear", "clear console log", ClearLog),
                new LambdaCommand("suggestions-context", "switch suggestions context", SwitchContext)
            );
        }

        private void Update()
        {
            var controlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var tildePressed = Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote);
            var dotPressed = Input.GetKeyDown(KeyCode.Period);

            if (_content.activeInHierarchy && controlDown)
            {
                if (tildePressed)
                    SwitchContext();
                else if (dotPressed)
                    DisplaySuggestions(_input.text, forced: true);
            }
            else if (tildePressed)
            {
                _content.SetActive(!_content.activeSelf);
                if (_content.activeInHierarchy)
                {
                    _input.ActivateInputField();
                    SuggestionsContext = _commandsContext;
                }
                _input.text = "";
            }

            if (!_content.activeInHierarchy)
                return;

            if (_suggestions.IsShown)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    _suggestions.MoveUp();

                if (Input.GetKeyDown(KeyCode.DownArrow))
                    _suggestions.MoveDown();

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    if (_suggestions.Selected == null)
                        _suggestions.MoveUp();
                    _suggestions.TryChooseCurrent();
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SuggestionsContext = _commandsContext;
                    DisplaySuggestions(_input.text);
                    _suggestions.Deselect();
                    _input.ActivateInputField();
                }
            }
            else
            {
                if ((Input.GetKeyDown(KeyCode.UpArrow) && _commandsHistory.TryMoveUp(out var command))
                || (Input.GetKeyDown(KeyCode.DownArrow) && _commandsHistory.TryMoveDown(out command)))
                {
                    _input.text = command;
                    _input.caretPosition = _input.text.Length;
                }
            }
        }

        private void SwitchContext()
        {
            if (SuggestionsContext == _historyContext)
                SuggestionsContext = _commandsContext;
            else if (SuggestionsContext == _commandsContext)
                SuggestionsContext = _historyContext;

            DisplaySuggestions(_input.text, forced: true);
        }

        private void DisplaySuggestions(string input, bool forced = false)
        {
            if (!forced && string.IsNullOrEmpty(input))
            {
                _suggestions.Hide();
                return;
            }

            _suggestions.Deselect();

            var suggestions = SuggestionsContext?.GetSuggestions(input);
            _suggestions.Suggestions = suggestions;

            if (suggestions.Any())
                _suggestions.Show();
            else
                _suggestions.Hide();
        }

        public void ClearLog()
        {
            _consoleLog.text = "";
            _scroll.verticalScrollbar.value = 0f;
        }

        public void Log(string message, object context = null)
        {
            Logger.Log(message, context == null ? this : context);
            AppendLine(message);
        }

        public void LogWarning(string message, object context = null)
        {
            Logger.LogWarning(message, context == null ? this : context);
            AppendLine(message, _warnings);
        }

        public void LogError(string message, object context = null)
        {
            Logger.LogError(message, context == null ? this : context);
            AppendLine(message, _errors);
        }

        public void LogException(Exception excepion, object context = null)
        {
            Logger.LogException(excepion, context == null ? this : context);
            AppendLine(excepion.ToString(), _exceptions);
        }

        private void AppendLine(string message)
        {
            _consoleLog.text += $"{message}\r\n";
            this.InvokeSkipOneFrame(() => _scroll.verticalScrollbar.value = 0f);
        }
        private void AppendLine(string message, Color color) => AppendLine($"<color=#{color.ToHexString()}>{message}</color>");

        private interface ISuggestionsContext
        {
            string Title { get; }

            IEnumerable<Suggestion> GetSuggestions(string input);
        }

        private abstract class ASuggestionContext<T> : ISuggestionsContext
        {
            public virtual IEnumerable<Suggestion> GetSuggestions(string input)
            {
                return FilterItems(
                    Collection,
                    input,
                    GetFilteringName
                )
                .Select(c => new Suggestion(
                    GetDisplayName(c),
                    c,
                    GetFullSuggestedText
                ));
            }

            protected abstract IEnumerable<T> Collection { get; }

            public abstract string Title { get; }

            protected abstract string GetDisplayName(T item);
            protected abstract string GetFilteringName(T item);
            protected abstract string GetFullSuggestedText(Suggestion item, string fullInput);

            private protected IOrderedEnumerable<T> FilterItems(IEnumerable<T> items, string input, Func<T, string> filteredStringGetter)
            {
                return items
                    .Where(c => filteredStringGetter.Invoke(c).Contains(input))
                    .OrderByDescending(c => filteredStringGetter.Invoke(c).IndexOf(input));
            }
        }

        private class CommandsSuggestionsContext : ASuggestionContext<ADebugCommand>
        {
            private DebugCommandsContainer _commandsContainer;

            public CommandsSuggestionsContext(DebugCommandsContainer commandsContainer)
            {
                _commandsContainer = commandsContainer;
            }

            public override string Title => "commands";

            protected override IEnumerable<ADebugCommand> Collection => _commandsContainer.Commands.Values;

            protected override string GetDisplayName(ADebugCommand item) => $"{item.Name} [{item.OptionsHint}]";
            protected override string GetFilteringName(ADebugCommand item) => item.Name;
            protected override string GetFullSuggestedText(Suggestion item, string fullInput) => (item.Source as ADebugCommand).Name + " ";

            public override IEnumerable<Suggestion> GetSuggestions(string input)
            {
                input = input.Trim(' ');
                var names = input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length == 1)
                    return base.GetSuggestions(input);

                var command = _commandsContainer.Commands.FirstOrDefault(c => c.Value.Name == names.FirstOrDefault()).Value;
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

        private class HistorySuggestionsContext : ASuggestionContext<string>
        {
            private CommandLineHistory _commandsHistory;

            public HistorySuggestionsContext(CommandLineHistory commandsHistory)
            {
                _commandsHistory = commandsHistory;
            }

            public override string Title => "history";

            protected override IEnumerable<string> Collection => _commandsHistory.Commands;

            protected override string GetDisplayName(string item) => item;
            protected override string GetFilteringName(string item) => item;
            protected override string GetFullSuggestedText(Suggestion item, string fullInput) => item.Source as string + " ";
        }

        private class ComandParameterNameSuggestionContext : ASuggestionContext<Option>
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
                return fullInput + paramStr + item.Source as string + " ";
            }
        }

        private const string PrefsHistoryKey = nameof(CommandLineHistory) + "-save";

        private void SaveCommandsHistory(CommandLineHistory history)
        {
            var list = history.Commands.ToList();
            var json = JsonUtility.ToJson(new SerializedCommandsHistory()
            {
                _list = list
            });
            PlayerPrefs.SetString(PrefsHistoryKey, json);
        }

        private void LoadCommandsHistory(CommandLineHistory history)
        {
            var list = JsonUtility.FromJson<SerializedCommandsHistory>(
                PlayerPrefs.GetString(PrefsHistoryKey)
            )?._list;
            history.Clear();

            if (list != null)
            {
                foreach (var command in list)
                    history.Record(command);
            }
        }

        private class SerializedCommandsHistory
        {
            public List<string> _list;
        }
    }
}