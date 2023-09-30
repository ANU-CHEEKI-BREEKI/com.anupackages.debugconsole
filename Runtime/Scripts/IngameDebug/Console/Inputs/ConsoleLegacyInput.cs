using UnityEngine;

namespace ANU.IngameDebug.Console
{
    public sealed class ConsoleLegacyInput : IConsoleInput
    {
        public bool GetControl() => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public bool GetOpen() => Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote);
        public bool GetDot() => Input.GetKeyDown(KeyCode.Period);
        public bool GetUp() => Input.GetKeyDown(KeyCode.UpArrow);
        public bool GetDown() => Input.GetKeyDown(KeyCode.DownArrow);
        public bool GetTab() => Input.GetKeyDown(KeyCode.Tab);
        public bool GetEscape() => Input.GetKeyDown(KeyCode.Escape);
    }
}