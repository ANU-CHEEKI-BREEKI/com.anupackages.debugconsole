namespace ANU.IngameDebug.Console
{
    public interface IConsoleInput
    {
        public bool GetOpen();
        public bool GetControl();
        public bool GetDot();
        public bool GetUp();
        public bool GetDown();
        public bool GetTab();
        public bool GetEscape();
    }
}