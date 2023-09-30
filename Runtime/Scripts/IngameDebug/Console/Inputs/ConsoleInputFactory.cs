namespace ANU.IngameDebug.Console
{
    public static class ConsoleInputFactory
    {
        public static IConsoleInput GetInput()
        {
#if ENABLE_INPUT_SYSTEM
            return new ConsoleNewInput();
#else
            return new ConsoleLegacyInput();
#endif
        }
    }
}