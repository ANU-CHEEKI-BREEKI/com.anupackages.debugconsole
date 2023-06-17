using System.Collections.Generic;

namespace ANU.IngameDebug.Console.CommandLinePreprocessors
{
    internal class CommandInputPreprocessorRegistry : ICommandInputPreprocessorRegistry
    {
        private readonly List<ICommandInputPreprocessor> _preprocessors = new();
        public void Add(ICommandInputPreprocessor preprocessor) => _preprocessors.Add(preprocessor);

        public string Preprocess(string input)
        {
            foreach (var item in _preprocessors)
                input = item.Preprocess(input);

            return input;
        }
    }
}