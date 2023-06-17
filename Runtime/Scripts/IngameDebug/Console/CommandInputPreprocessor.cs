namespace ANU.IngameDebug.Console.CommandLinePreprocessors
{
    public interface ICommandInputPreprocessor
    {
        string Preprocess(string input);
    }

    public interface ICommandInputPreprocessorRegistry
    {
        void Add(ICommandInputPreprocessor preprocessor);
        string Preprocess(string input);
    }
}