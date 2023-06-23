using System.Collections.Generic;
using System.Linq;

namespace ANU.IngameDebug.Console.CommandLinePreprocessors
{
    internal class CommandInputPreprocessorRegistry : ICommandInputPreprocessorRegistry
    {
        private readonly List<ICommandInputPreprocessor> _preprocessors = new();

        public CommandInputPreprocessorRegistry(ILogger logger) => Logger = logger;

        public ILogger Logger { get; }
        public IReadOnlyList<ICommandInputPreprocessor> Preprocessors => _preprocessors;

        public void Add(ICommandInputPreprocessor preprocessor) => _preprocessors.Add(preprocessor);
        public bool Remove(ICommandInputPreprocessor preprocessor) => _preprocessors.Remove(preprocessor);

        public string Preprocess(string input)
        {
            foreach (var item in _preprocessors.OrderBy(r => r.Priority))
            {
                if (item is IInjectLogger injectLogger)
                    injectLogger.Logger = Logger;

                input = item.Preprocess(input);
            }

            return input;
        }
    }
}