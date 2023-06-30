using System.Collections.Generic;

namespace ANU.IngameDebug.Console.CommandLinePreprocessors
{
    public interface ICommandInputPreprocessor
    {
        int Priority => 0;
        string Preprocess(string input);
    }

    public interface IReadOnlyCommandInputPreprocessorRegistry
    {
        string Preprocess(string input);
    }

    public interface ICommandInputPreprocessorRegistry : IReadOnlyCommandInputPreprocessorRegistry
    {
        IReadOnlyList<ICommandInputPreprocessor> Preprocessors { get; }

        void Add(ICommandInputPreprocessor preprocessor);
        bool Remove(ICommandInputPreprocessor preprocessor);
    }
}
