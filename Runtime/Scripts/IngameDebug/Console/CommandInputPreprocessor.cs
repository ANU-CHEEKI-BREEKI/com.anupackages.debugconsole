namespace ANU.IngameDebug.Console.CommandLinePreprocessors
{
    public struct PreprocessorExtraArgs
    {
        public ILogger Logger { get; set; }
    }

    public interface ICommandInputPreprocessor
    {
        string Preprocess(string input, PreprocessorExtraArgs args);
    }

    public interface ICommandInputPreprocessorRegistry
    {
        void Add(ICommandInputPreprocessor preprocessor);
        string Preprocess(string input);
    }
}