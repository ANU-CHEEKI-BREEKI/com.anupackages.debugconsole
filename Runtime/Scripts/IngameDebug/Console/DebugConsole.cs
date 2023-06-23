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
using System.Text.RegularExpressions;

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
        [SerializeField] private Button _submit;
        [SerializeField] private Button _clear;
        [SerializeField] private Button _close;
        [Space]
        [SerializeField] private SuggestionPopUp _suggestions;
        [Space]
        [SerializeField] private UITheme[] _themes;
        [SerializeField] private UITheme _currentTheme;

        private static CommandLineHistory _commandsHistory = new CommandLineHistory();

        private static readonly Regex _receivedMessageTypeRegex = new Regex(@"\[console:(?<type>.*?)\] ");

        private static ISuggestionsContext _suggestionsContext;
        private static CommandsSuggestionsContext _commandsContext;
        private static HistorySuggestionsContext _historyContext;
        private static readonly ILogger _logger = new UnityLogger(ConsoleLogType.Output);

        private static DebugConsole Instance { get; set; }

        private static readonly CommandsRegistry _commands = new(_logger);

        internal static LogsContainer Logs { get; } = new();

        public static bool IsOpened => Instance._content.activeInHierarchy;
        public static ICommandsRouter Router { get; set; } = null;

        public static bool ShowLogs { get; set; } = true;

        private static ILogger InputLogger { get; } = new UnityLogger(ConsoleLogType.Input);
        public static ILogger Logger => _logger;

        public static IConverterRegistry Converters { get; } = new ConverterRegistry(Logger);
        public static ICommandInputPreprocessorRegistry Preprocessors { get; } = new CommandInputPreprocessorRegistry(Logger);
        public static IInstancesTargetRegistry InstanceTargets { get; } = new InstancesTargetRegistry();
        public static ICommandsRegistry Commands => _commands;

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

        public static event Action<UITheme> ThemeChanged;
        public static UITheme CurrentTheme
        {
            get => Instance == null ? null : Instance._currentTheme;
            private set
            {
                if (Instance._currentTheme == value)
                    return;
                Instance._currentTheme = value;
                ThemeChanged?.Invoke(CurrentTheme);
            }
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
                    InputLogger.Log(commandLine);
                }

                if (Router != null)
                    Router.SendCommand(commandLine);
                else
                    ExecuteCommandInternal(commandLine, silent);
            }
            finally
            {
                if (Instance != null)
                    Instance._input.ActivateInputField();
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            Instance = this;
            ThemeChanged?.Invoke(CurrentTheme);

            if (CurrentTheme != null)
                CurrentTheme.Changed += () => ThemeChanged?.Invoke(CurrentTheme);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearStatic()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (!Application.isPlaying)
                return;

            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.logMessageReceived += LogMessageReceived;

            _commandsContext = new CommandsSuggestionsContext(_commands.Commands);
            _historyContext = new HistorySuggestionsContext(_commandsHistory);
            _content.SetActive(false);

            SetUpPreprocessors();
            SetUpConverters();
            SetupConsoleCommands();

            _input.onSubmit.AddListener(text => Submit());
            _input.onValueChanged.AddListener(text =>
            {
                if (text != _commandsHistory.Current)
                    _commandsHistory.Reset();

                DisplaySuggestions(text);
            });
            _submit.onClick.AddListener(Submit);
            _clear.onClick.AddListener(Logs.Clear);
            _close.onClick.AddListener(() =>
            {
                _content.SetActive(false);
                _input.text = "";
            });

            _suggestions.Chosen += s =>
            {
                _input.text = s.ApplySuggestion(_input.text);

                _input.caretPosition = _input.text.Length;
                _input.ActivateInputField();
            };

            _suggestions.Hided += () => SuggestionsContext = _commandsContext;

            LoadCommandsHistory(_commandsHistory);

            void Submit()
            {
                var text = _input.text;
                if (string.IsNullOrEmpty(text))
                    return;
                ExecuteCommand(text);
            }
        }

        private void Start()
        {
            ExecuteCommand("help", true);
        }

        private void SetUpPreprocessors()
        {
            Preprocessors.Add(new BracketsToStringPreprocessor());
            Preprocessors.Add(new NamedParametersPreprocessor());
        }

        private void SetUpConverters()
        {
            Converters.Register(new BaseConveerter());
            Converters.Register(new ArrayConverter());
            Converters.Register(new ListConverter());
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

        private static void ExecuteCommandInternal(string commandLine, bool silent)
        {
            try
            {
                commandLine = Preprocessors.Preprocess(commandLine);
                var commandName = ExtractCommandName(commandLine);

                if (!_commands.Commands.ContainsKey(commandName) && !silent)
                {
                    Logger.LogError($"There is no command with name \"{commandName}\". Enter \"help\" to see command usage.");
                    return;
                }

                var command = _commands.Commands[commandName];

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
            catch (Exception ex)
            {
                if (!silent)
                    Logger.LogException(ex);
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
            Commands.RegisterCommand("help", "Print help", () => Logger.Log(
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
Enter ""list"" to print all registered commands
"));
            Commands.RegisterCommand("list", "Print all command names with descriptions", () =>
            {
                var maxLength = _commands.Commands.Values.Max(n => n.Name.Length);
                var nameLength = Mathf.Max(maxLength, maxLength + 5);

                var builder = new StringBuilder();
                builder.AppendLine("Available commands:");
                foreach (var command in _commands.Commands.Values.OrderBy(cmd => cmd.Name))
                {
                    builder.Append("  - ");
                    builder.Append(command.Name);
                    builder.Append(new string('-', nameLength - command.Name.Length));
                    builder.AppendLine(command.Description);
                }

                Logger.Log(builder.ToString());
            });

            Commands.RegisterCommand("clear", "Clear console log", Logs.Clear);
            Commands.RegisterCommand("suggestions-context", "Switch suggestions context", SwitchContext);
        }

        [DebugCommand(Name = "time", Description = "Set time scale")]
        private void SetTimeScale(
            [OptAltNames("s")]
            float scale
        ) => Time.timeScale = Mathf.Max(0, scale);

        [DebugCommand(Name = "console.theme", Description = "Set console theme at runtime. Pass index or name of wanted UITheme listed in DebugConsole Themes list")]
        private void SetConsoleTheme(
            [OptAltNames("i")]
            int index = -1,
            [OptAltNames("n")]
            string name = "",
            [OptAltNames("l"), OptDesc("Print all available themes")]
            bool list = false
        )
        {
            if (list)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"There are {_themes.Length} themes available: ");
                for (int i = 0; i < _themes.Length; i++)
                {
                    var item = _themes[i];
                    sb.AppendLine($"[{i}]: {item.name}");
                }
                Logger.Log(sb.ToString());
                return;
            }


            if (_themes.Length == 0)
            {
                Logger.LogError("Provide at least one item in DebugConsole Themes list");
            }
            else if (index >= 0)
            {
                if (index < 0)
                {
                    Logger.LogError("Index should be greater or equals 0");
                    return;
                }
                else if (index >= _themes.Length)
                {
                    Logger.LogError($"Index out of bounds Themes list. There are {_themes.Length} themes available");
                    return;
                }

                var theme = _themes[index];
                CurrentTheme = theme;
            }
            else if (!string.IsNullOrEmpty(name))
            {
                var theme = _themes.FirstOrDefault(t => t.name == name);
                if (theme == null)
                    Logger.LogError($"Theme \"{name}\" not found. Provide at least one item with name \"{name}\" in DebugConsole Themes list");
                else
                    CurrentTheme = theme;
            }
            else
            {
                Logger.LogError("Provide index or name of wanted UITheme listed in DebugConsole Themes list");
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

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
            if (!ShowLogs)
                return;

            if (Instance == null)
                return;

            var logType = ConsoleLogType.AppMessage;
            var match = _receivedMessageTypeRegex.Match(condition);

            if (match.Success && match.Index == 0)
            {
                condition = condition.Substring(match.Length);
                if (Enum.TryParse(typeof(ConsoleLogType), match.Groups["type"].Value, true, out var parsed))
                    logType = (ConsoleLogType)parsed;
            }

            Logs.Add(new Log(
                logType,
                type,
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

        private class UnityLogger : ILogger
        {
            private readonly ConsoleLogType _consoleLogType;

            public UnityLogger(ConsoleLogType consoleLogType)
                => _consoleLogType = consoleLogType;

            public void Log(string message, UnityEngine.Object context) => Debug.Log($"[console:{_consoleLogType}] {message}", context);
            public void LogWarning(string message, UnityEngine.Object context) => Debug.LogWarning($"[console:{_consoleLogType}] {message}", context);
            public void LogError(string message, UnityEngine.Object context) => Debug.LogError($"[console:{_consoleLogType}] {message}", context);
            public void LogException(Exception exception, UnityEngine.Object context) => Debug.LogException(new Exception($"[console:{_consoleLogType}] {exception.Message}", exception), context);
        }
    }
}