#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;

namespace ANU.IngameDebug.Console
{
    public sealed class ConsoleNewInput : IConsoleInput
    {
        private readonly Keyboard keyboard;

        public ConsoleNewInput() => keyboard = Keyboard.current;

        public bool GetControl() => IsKey(Key.LeftCtrl) || IsKey(Key.RightCtrl);
        public bool GetOpen() => IsKeyDown(Key.Backquote);
        public bool GetDot() => keyboard[Key.Period].wasPressedThisFrame;
        public bool GetUp() => keyboard[Key.UpArrow].wasPressedThisFrame;
        public bool GetDown() => keyboard[Key.DownArrow].wasPressedThisFrame;
        public bool GetTab() => keyboard[Key.Tab].wasPressedThisFrame;
        public bool GetEscape() => keyboard[Key.Escape].wasPressedThisFrame;

        private bool IsKey(Key key) => keyboard[key].isPressed;
        private bool IsKeyDown(Key key) => keyboard[key].wasPressedThisFrame;
    }
}
#endif