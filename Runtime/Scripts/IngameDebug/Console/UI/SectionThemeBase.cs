using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [ExecuteAlways]
    internal abstract class SectionThemeBase : MonoBehaviour
    {
        [SerializeField] private Graphic[] _header = { };
        [SerializeField] private Graphic[] _background = { };
        [SerializeField] private Graphic[] _foreground = { };
        [SerializeField] private Graphic[] _font = { };
        [SerializeField] private Selectable[] _selectable = { };
        [SerializeField] private  Graphic[] _selection = { };
        [SerializeField] private Graphic[] _scrollBarBackgroind = { };
        [SerializeField] private Graphic[] _scrollBarForeground = { };
        [SerializeField] private Graphic[] _nonRequired = { };
        [SerializeField] private Graphic[] _required = { };
        [SerializeField] private Graphic[] _toggle = { };
        [SerializeField] private Graphic[] _info = { };
        [SerializeField] private Graphic[] _warning = { };
        [SerializeField] private Graphic[] _error = { };

        public Graphic[] Header => _header;
        public Graphic[] Background => _background;
        public Graphic[] Foreground => _foreground;
        public Graphic[] Font => _font;
        public Selectable[] Selectable => _selectable;
        public Graphic[] ScrollBarBackgroind => _scrollBarBackgroind;
        public Graphic[] ScrollBarForeground => _scrollBarForeground;
        public Graphic[] Selection => _selection;

        public Graphic[] NonRequired => _nonRequired;
        public Graphic[] Required => _required;
        public Graphic[] Toggle => _toggle;
        
        public Graphic[] Info => _info;
        public Graphic[] Warning => _warning;
        public Graphic[] Error => _error;

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