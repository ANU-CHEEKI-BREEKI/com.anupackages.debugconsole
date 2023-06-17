using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ANU.IngameDebug.Utils;
using ANU.IngameDebug.Console.Commands;
using ANU.IngameDebug.Console.Commands.Implementations;
using System.Reflection;
using ANU.IngameDebug.Console.Converters;
using ANU.IngameDebug.Console.CommandLinePreprocessors;

namespace ANU.IngameDebug.Console
{
    [DisallowMultipleComponent]
    public class DebugConsole : MonoBehaviour
    {
        private const float ColsoleInputHeightPercentage = 0.1f;
        private const float Padding = 10f;
        private const string PrefsHistoryKey = nameof(CommandLineHistory) + "-save";

        [SerializeField] private GameObject _content;
        [SerializeField] private TMP_InputField _input;
        [Space]
        [SerializeField] private SuggestionPopUp _suggestions;
        [Space]
        [SerializeField] private SearchInputField _searchInput;
        [SerializeField] private Toggle _logToggle;
        [SerializeField] private Toggle _warningToggle;
        [SerializeField] private Toggle _errorToggle;
        [Space]
        [SerializeField] private UITheme _theme;

        private static CommandLineHistory _commandsHistory = new CommandLineHistory();

        private static ISuggestionsContext _suggestionsContext;
        private static CommandsSuggestionsContext _commandsContext;
        private static HistorySuggestionsContext _historyContext;
        private static CommandsLogger _commandsLogger = new CommandsLogger()
        {
            Logger = new UnityLogger()
        };

        private static DebugConsole Instance { get; set; }

        private static Dictionary<string, ADebugCommand> _commands = new();
        internal static LogsContainer Logs { get; } = new();

        public static bool IsOpened => Instance._content.activeInHierarchy;
        public static ICommandsRouter Router { get; set; } = null;
        
        public static bool ShowLogs { get; set; } = true;

        public static ILogger Logger
        {
            get => _commandsLogger.Logger;
            set => _commandsLogger.Logger = value;
        }

        public static IConverterRegistry Converters { get; } = new ConverterRegistry();
        public static ICommandInputPreprocessorRegistry Preprocessors { get; } = new CommandInputPreprocessorRegistry();

        private static ISuggestionsContext SuggestionsContext
        {
            get => _suggestionsContext;
            set
            {
                _suggestionsContext = value;
                if (Instance != null)
                    Instance._suggestions.Title = _suggestionsContext?.Title;
            }
        }

        public UITheme Theme => _theme;

        public static void RegisterCommands(params ADebugCommand[] commands)
        {
            foreach (var command in commands)
                RegisterCommand(command);
        }

        public static void RegisterCommand(ADebugCommand command)
        {
            _commands[command.Name] = command;
            command.Logger = _commandsLogger;
        }

        public static void RegisterCommand(string name, string description, Action command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand(Delegate command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand(string name, string description, Delegate command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand<T1>(string name, string description, Action<T1> command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand<T1, T2>(string name, string description, Action<T1, T2> command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand<T1, T2, T3>(string name, string description, Action<T1, T2, T3> command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand<T1, T2, T3, T4>(string name, string description, Action<T1, T2, T3, T4> command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand<T1>(string name, string description, Func<T1> command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand<T1, T2>(string name, string description, Func<T1, T2> command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand<T1, T2, T3>(string name, string description, Func<T1, T2, T3> command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand<T1, T2, T3, T4>(string name, string description, Func<T1, T2, T3, T4> command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        public static void RegisterCommand<T1, T2, T3, T4, T5>(string name, string description, Func<T1, T2, T3, T4, T5> command, Action<ICommandMetaData[]> metaDataCustomize = null)
            => RegisterCommand(name, description, command.Method, command.Target, metaDataCustomize);

        private static void RegisterCommand(string name, string description, MethodInfo method, object target, Action<ICommandMetaData[]> metaDataCustomize)
        {
            //TODO: add MethodCommand arguments support check
            var methodCommand = new MethodCommand(name, description, method, target);
            RegisterCommand(methodCommand, metaDataCustomize);
        }

        private static void RegisterCommand(MethodInfo method, object target, Action<ICommandMetaData[]> metaDataCustomize)
        {
            //TODO: add MethodCommand arguments support check
            var methodCommand = new MethodCommand(method, target);
            RegisterCommand(methodCommand, metaDataCustomize);
        }

        private static void RegisterCommand(MethodCommand command, Action<ICommandMetaData[]> metaDataCustomize)
        {
            var methodCommand = command;
            var metaData = methodCommand.Options.Select(v => new CommandMetaData
            {
                Key = v,
                // CustomName = v.Key.Prototype,
                AvailableValues = methodCommand.ValueHints.TryGetValue(v, out var av) ? av : new AvailableValuesHint()
            }).ToArray();
            metaDataCustomize?.Invoke(metaData);
            for (int i = 0; i < metaData.Length; i++)
            {
                var item = metaData[i];
                methodCommand.ValueHints[item.Key] = item.AvailableValues;
            }
            RegisterCommand(methodCommand);
        }

        public interface ICommandMetaData
        {
            // public string CustomName { get; set; }
            public AvailableValuesHint AvailableValues { get; set; }

        }
        private class CommandMetaData : ICommandMetaData
        {
            public Option Key { get; set; }
            // public string CustomName { get; set; }
            public AvailableValuesHint AvailableValues { get; set; }
        }

        public static void ExecuteCommand(string commandLine, bool silent = false)
        {
            try
            {
                if (Instance != null)
                    Instance._input.text = "";


                if (!silent)
                {
                    _commandsHistory.Record(commandLine);
                    Log("> " + commandLine);
                }

                if (Router != null)
                    Router.SendCommand(commandLine);
                else
                    ExecuteCommandInternal(commandLine);
            }
            finally
            {
                Instance._input.ActivateInputField();
            }
        }

        public static void Log(string message, object context = null) => Logger?.Log(message, context == null ? Instance : context);
        public static void LogWarning(string message, object context = null) => Logger?.LogWarning(message, context == null ? Instance : context);
        public static void LogError(string message, object context = null) => Logger?.LogError(message, context == null ? Instance : context);
        public static void LogException(Exception exception, object context = null) => Logger?.LogException(exception, context == null ? Instance : context);

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.logMessageReceived += LogMessageReceived;

            _commandsContext = new CommandsSuggestionsContext(_commands);
            _historyContext = new HistorySuggestionsContext(_commandsHistory);
            _content.SetActive(false);

            SetUpPreprocessors();
            SetUpConverters();
            SetupConsoleCommands();

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

            _suggestions.Chosen += s =>
            {
                _input.text = s.ApplySuggestion(_input.text);

                _input.caretPosition = _input.text.Length;
                _input.ActivateInputField();
            };

            _suggestions.Hided += () => SuggestionsContext = _commandsContext;

            ExecuteCommand("clear", true);
            ExecuteCommand("help", true);
            ExecuteCommand("refresh-console-size", true);

            LoadCommandsHistory(_commandsHistory);
        }

        private void SetUpPreprocessors()
        {
            Preprocessors.Add(new BracketsToStringPreprocessor());
        }

        private void SetUpConverters()
        {
            Converters.Register(new Vector2IntConverter());
            Converters.Register(new Vector2Converter());
            Converters.Register(new Vector3IntConverter());
            Converters.Register(new Vector3Converter());
            Converters.Register(new Vector4Converter());
            Converters.Register(new QuaternionConverter());
            Converters.Register(new ColorConverter());
            Converters.Register(new Color32Converter());
            Converters.Register(new GameObjectConverter());
            Converters.Register(new ComponentConverter());
            Converters.Register(new BoolConverter());
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= LogMessageReceived;
        }

        private void OnApplicationQuit() => SaveCommandsHistory(_commandsHistory);

        private static void ExecuteCommandInternal(string commandLine)
        {
            commandLine = Preprocessors.Preprocess(commandLine);
            var commandName = ExtractCommandName(commandLine);

            if (!_commands.ContainsKey(commandName))
            {
                LogError($"There is no command with name \"{commandName}\". Enter \"help\" to see command usage.");
                return;
            }

            var command = _commands[commandName];

            // remove command name from command line input
            if (commandLine != null)
            {
                var nameIndex = commandLine.IndexOf(command.Name);
                commandLine = commandLine
                    .Remove(0, command.Name.Length)
                    .Trim();
            }

            command.Execute(commandLine);
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
                new LambdaCommand("help", "print help", () => Log(
$@"To call command               - enter command name and parameters
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
                new LambdaCommand("list", "print all command names with descriptions", () =>
                {
                    var maxLength = _commands.Values.Max(n => n.Name.Length);
                    var nameLength = Mathf.Max(maxLength, maxLength + 5);

                    var builder = new StringBuilder();
                    builder.AppendLine("Available commands:");
                    foreach (var command in _commands.Values)
                    {
                        builder.Append("  - ");
                        builder.Append(command.Name);
                        builder.Append(new string('-', nameLength - command.Name.Length));
                        builder.AppendLine(command.Description);
                    }

                    Log(builder.ToString());
                }),
                new LambdaCommand("clear", "clear console log", Logs.Clear),
                new LambdaCommand("suggestions-context", "switch suggestions context", SwitchContext),
                new LambdaCommand("time", "", optionValuesHint =>
                {
                    var set = new OptionSet();
                    set.Add<float>("scale=", scale => Time.timeScale = Mathf.Max(0, scale));
                    return set;
                })
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

        private static void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if(!ShowLogs)
                return;

            if (Instance == null)
                return;

            Logs.Add(new Log(
                (LogTypes)type,
                condition,
                stackTrace
            ));
        }

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

        private class CommandsLogger : ILogger
        {
            public ILogger Logger { get; set; }

            public void Log(string message, object context = null)
            {
                Logger.Log(message, context);
            }

            public void LogError(string message, object context = null)
            {
                Logger.LogError(message, context);
            }

            public void LogException(Exception exception, object context = null)
            {
                Logger.LogException(exception, context);
            }

            public void LogWarning(string message, object context = null)
            {
                Logger.LogWarning(message, context);
            }
        }
    }
}