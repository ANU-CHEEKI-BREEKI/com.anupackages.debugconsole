using System.Collections.Generic;
using System.Linq;

namespace ANU.IngameDebug.Console.CommandLinePreprocessors
{
    internal class CommandInputPreprocessorRegistry : ICommandInputPreprocessorRegistry
    {
        private readonly List<ICommandInputPreprocessor> _preprocessors = new();

        public CommandInputPreprocessorRegistry(ILogger logger)
            => Logger = logger;

        public ILogger Logger { get; }

        public void Add(ICommandInputPreprocessor preprocessor) => _preprocessors.Add(preprocessor);

        public string Preprocess(string input)
        {
            var args = new PreprocessorExtraArgs() { Logger = Logger };
            foreach (var item in _preprocessors.OrderBy(r => r.Priority))
                input = item.Preprocess(input, args);

            return input;
        }
    }
}