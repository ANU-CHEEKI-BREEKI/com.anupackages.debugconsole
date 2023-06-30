using System.Collections.Generic;
using System.Linq;

namespace ANU.IngameDebug.Console.CommandLinePreprocessors
{
    internal class CommandInputPreprocessorRegistry : ICommandInputPreprocessorRegistry
    {
        private readonly List<ICommandInputPreprocessor> _preprocessors = new();

        public CommandInputPreprocessorRegistry(IReadOnlyDebugConsoleProcessor context) => Context = context;

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
    }
}