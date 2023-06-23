using System.Collections.Generic;

namespace ANU.IngameDebug.Console.CommandLinePreprocessors
{


    public interface ICommandInputPreprocessor
    {
        int Priority => 0;
        string Preprocess(string input);
    }

    public interface ICommandInputPreprocessorRegistry
    {
        IReadOnlyList<ICommandInputPreprocessor> Preprocessors { get; }

        void Add(ICommandInputPreprocessor preprocessor);
        bool Remove(ICommandInputPreprocessor preprocessor);

        string Preprocess(string input);
    }
}
