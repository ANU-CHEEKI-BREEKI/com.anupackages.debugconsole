using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANU.IngameDebug.Console;
using ANU.IngameDebug.Console.CommandLinePreprocessors;
using ANU.IngameDebug.Utils;
using static ANU.IngameDebug.Utils.Extensions;
using static ANU.IngameDebug.Utils.Extensions.ColumnAlignment;

[assembly: RegisterDebugCommandTypes(typeof(CommandInputPreprocessorRegistry))]

namespace ANU.IngameDebug.Console.CommandLinePreprocessors
{
    internal class CommandInputPreprocessorRegistry : ICommandInputPreprocessorRegistry
    {
        private readonly List<ICommandInputPreprocessor> _preprocessors = new();

        public CommandInputPreprocessorRegistry(IReadOnlyDebugConsoleProcessor context)
        {
            Context = context;
            Context.InstanceTargets.Register(this);
        }

        public IReadOnlyDebugConsoleProcessor Context { get; }
        public IReadOnlyList<ICommandInputPreprocessor> Preprocessors => _preprocessors;

        public void Add(ICommandInputPreprocessor preprocessor)
        {
            if (preprocessor is IInjectDebugConsoleContext consoleContext)
                consoleContext.Context = Context;

            _preprocessors.Add(preprocessor);
        }

        public bool Remove(ICommandInputPreprocessor preprocessor)
        {
            if (preprocessor is IInjectDebugConsoleContext consoleContext)
                consoleContext.Context = null;

            return _preprocessors.Remove(preprocessor);
        }

        public string Preprocess(string input)
        {
            foreach (var item in _preprocessors.OrderBy(r => r.Priority))
                input = item.Preprocess(input);

            return input;
        }

        [DebugCommand]
        public void ListRegisteredPreprocessors()
        {
            if (!_preprocessors.Any())
            {
                Context.Logger.LogReturnValue("There are no any registered preprocessors");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine();

            var processors = _preprocessors.OrderBy(r => r.Priority);
            sb.PrintTable(
                processors,
                new string[] { "Priority", "Processor Type" },
                item => new string[] { item.Priority.ToString(), item.GetType().Name },
                new ColumnAlignment[] { Right, Left }
            );

            Context.Logger.LogReturnValue(sb);
        }
    }
}