namespace ANU.IngameDebug.Console
{
    /// <summary>
    /// Implement this interface by ICommandInputPreprocessor
    /// and ILogger will be injected before ICommandInputPreprocessor.Preprocess called
    /// 
    /// ---
    /// 
    /// Implement this interface by IConverter
    /// and ILogger will be injected before IConverterConvertFromString called
    /// </summary>
    public interface IInjectLogger
    {
        ILogger Logger { get; set; }
    }
}