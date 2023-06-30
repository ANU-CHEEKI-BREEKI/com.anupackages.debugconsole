namespace ANU.IngameDebug.Console
{
    public interface IInjectDebugConsoleContext
    {
        public IReadOnlyDebugConsoleProcessor Context { get; set; }
    }
}
