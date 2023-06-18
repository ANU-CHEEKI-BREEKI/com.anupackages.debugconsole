using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [ExecuteAlways]
    internal abstract class SectionThemeBase : MonoBehaviour
    {
        [SerializeField] private Graphic[] _background = {};
        [SerializeField] private Graphic[] _foreground = {};
        [SerializeField] private Graphic[] _font = {};
        [SerializeField] private Selectable[] _selectable = {};
        [SerializeField] private Graphic[] _scrollBarBackgroind = {};
        [SerializeField] private Graphic[] _scrollBarForeground = {};

        protected Graphic[] Background => _background;
        protected Graphic[] Foreground => _foreground;
        protected Graphic[] Font => _font;
        public Selectable[] Selectable => _selectable;
        public Graphic[] ScrollBarBackgroind => _scrollBarBackgroind;
        public Graphic[] ScrollBarForeground => _scrollBarForeground;

        private void OnEnable()
        {
            DebugConsole.ThemeChanged += UpdateTheme;
            UpdateTheme(DebugConsole.CurrentTheme);
        }

        private void OnDisable()
        {
            DebugConsole.ThemeChanged -= UpdateTheme;
        }
        protected abstract void UpdateTheme(UITheme obj);
    }
}