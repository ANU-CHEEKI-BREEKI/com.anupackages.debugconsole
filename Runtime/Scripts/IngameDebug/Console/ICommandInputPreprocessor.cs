namespace ANU.IngameDebug.Console
{
    internal interface ICommandInputPreprocessor
    {
        string Preprocess(string input);
    }
}