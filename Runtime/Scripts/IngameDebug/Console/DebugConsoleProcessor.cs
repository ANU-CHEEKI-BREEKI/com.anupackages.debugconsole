using System;
using ANU.IngameDebug.Console.Converters;
using ANU.IngameDebug.Console.CommandLinePreprocessors;
using System.Text;
using System.Linq;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    public interface IReadOnlyDebugConsoleProcessor
    {
        IConverterRegistry Converters { get; }
        ICommandInputPreprocessorRegistry Preprocessors { get; }
        ICommandsRegistry Commands { get; }

        IInstancesTargetRegistry InstanceTargets { get; }
        IDefinesRegistry Defines { get; }
        ILogger Logger { get; }

        ExecutionResult ExecuteCommand(string commandLine, bool silent = false);
    }

    public class DebugConsoleProcessor : IReadOnlyDebugConsoleProcessor
    {
        public ICommandsRouter Router { get; set; } = null;
        public bool ShowLogs { get; set; } = true;
        public ILogger Logger => _logger;

        public IConverterRegistry Converters { get; }
        public ICommandInputPreprocessorRegistry Preprocessors { get; }
        public IInstancesTargetRegistry InstanceTargets { get; } = new InstancesTargetRegistry();
        public ICommandsRegistry Commands { get; }
        public IDefinesRegistry Defines { get; } = new DefinesRegistry();

        internal CommandLineHistory CommandsHistory { get; } = new CommandLineHistory();
        internal ILogger InputLogger => _inputLogger;

        internal readonly UnityLogger _logger = new UnityLogger(ConsoleLogType.Output);
        internal readonly UnityLogger _inputLogger = new UnityLogger(ConsoleLogType.Input);

        public DebugConsoleProcessor()
        {
            Commands = new CommandsRegistry(this);
            Converters = new ConverterRegistry(this);
            Preprocessors = new CommandInputPreprocessorRegistry(this);
        }

        public void Initialize()
        {
            SetUpPreprocessors();
            SetUpConverters();
            SetupConsoleCommands();
        }

        /// <summary>
        /// if void - returns null
        /// if have one target - returns result as object
        /// if have multiple targets - returns IEnumerable<object> as object
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="silent"></param>
        /// <returns></returns>
        public ExecutionResult ExecuteCommand(string commandLine, bool silent = false)
        {
            try
            {
                if (!silent)
                {
                    CommandsHistory.Record(commandLine);
                    InputLogger.LogInfo(commandLine);
                }

                if (Router != null)
                    Router.SendCommand(commandLine);
                else
                    return ExecuteCommandInternal(commandLine, silent);
            }
            finally { }

            return default;
        }

        private ExecutionResult ExecuteCommandInternal(string commandLine, bool silent)
        {
            try
            {
                _inputLogger.SilenceStack.Push(silent);
                _logger.SilenceStack.Push(silent);

                commandLine = Preprocessors.Preprocess(commandLine);
                var commandName = ExtractCommandName(commandLine);

                if (!Commands.Commands.ContainsKey(commandName) && !silent)
                {
                    Logger.LogError($"There is no command with name \"{commandName}\". Enter \"help\" to see command usage.");
                    return default;
                }

                var command = Commands.Commands[commandName];

                // remove command name from command line input
                if (commandLine != null)
                {
                    var nameIndex = commandLine.IndexOf(command.Name);
                    commandLine = commandLine
                        .Remove(0, command.Name.Length)
                        .Trim();
                }

                var result = command.Execute(commandLine);

                if (!silent && result.ReturnType != typeof(void) && result.ReturnValues != null)
                {
                    foreach (var item in result.ReturnValues)
                        Logger.LogReturnValue(item.ReturnValue, item.Target as UnityEngine.Object);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (!silent)
                    Logger.LogException(ex);
            }
            finally
            {
                _inputLogger.SilenceStack.TryPop(out _);
                _logger.SilenceStack.TryPop(out _);
            }

            return default;
        }

        private string ExtractCommandName(string commandLine)
        {
            var commandName = commandLine;
            var spaceindex = commandLine.IndexOf(" ");
            if (spaceindex >= 0)
                commandName = commandLine.Substring(0, spaceindex);
            return commandName;
        }

        private void SetupConsoleCommands()
        {
            Commands.RegisterCommand(new Action(PrintHelpCommand));
            Commands.RegisterCommand(new Action(ListCommandsCommand));
        }

        private void SetUpPreprocessors()
        {
            Preprocessors.Add(new BracketsToStringPreprocessor());
            Preprocessors.Add(new NamedParametersPreprocessor());
            Preprocessors.Add(new DefinesPreprocessor());
            Preprocessors.Add(new NestedCommandsPreprocessor());
        }
        
        private void SetUpConverters()
        {
            Converters.Register(new BaseConverter());
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

        [DebugCommand("help", "Print help", DisplayOptions = CommandDisplayOptions.All & ~CommandDisplayOptions.Dashboard)]
        private void PrintHelpCommand()
        {
            Logger.LogInfo(
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
");
        }

        [DebugCommand("list", "Print all command names with descriptions", DisplayOptions = CommandDisplayOptions.All & ~CommandDisplayOptions.Dashboard)]
        private void ListCommandsCommand()
        {
            var maxLength = Commands.Commands.Values.Max(n => n.Name.Length);
            var nameLength = Mathf.Max(maxLength, maxLength + 5);

            var builder = new StringBuilder();
            builder.AppendLine("Available commands:");
            foreach (var command in Commands.Commands.Values.OrderBy(cmd => cmd.Name))
            {
                builder.Append("  - ");
                builder.Append(command.Name);
                builder.Append(new string('-', nameLength - command.Name.Length));
                builder.AppendLine(command.Description);
            }

            Logger.LogInfo(builder.ToString());
        }
    }
}