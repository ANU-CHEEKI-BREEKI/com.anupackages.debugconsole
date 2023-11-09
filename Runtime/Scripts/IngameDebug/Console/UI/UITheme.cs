using UnityEngine;

namespace ANU.IngameDebug.Console
{
    using System;
    using ANU.IngameDebug.Utils;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ConsoleUITheme", menuName = "ANU/IngameDebug/Console/UITheme", order = 0)]
    public class UITheme : ScriptableObject
    {
        [field: SerializeField] public Color Input { get; private set; }

        [field: Space, Header("Messages - Default")]
        [field: SerializeField] public Color Log { get; private set; }
        [field: SerializeField] public Color Warnings { get; private set; }
        [field: SerializeField] public Color Errors { get; private set; }
        [field: SerializeField] public Color Exceptions { get; private set; }
        [field: SerializeField] public Color Assert { get; private set; }
        [field: SerializeField] public Color Stacktrace { get; private set; }

        [Space, Header("Messages -  Overrides")]
        [SerializeField] private OptionalField<Color> _message_Log;
        [SerializeField] private OptionalField<Color> _message_Warnings;
        [SerializeField] private OptionalField<Color> _message_Errors;
        [SerializeField] private OptionalField<Color> _message_Exceptions;
        [SerializeField] private OptionalField<Color> _message_Assert;
        [Space]
        [SerializeField] private OptionalField<Color> _output_Log;
        [SerializeField] private OptionalField<Color> _output_Warnings;
        [SerializeField] private OptionalField<Color> _output_Errors;
        [SerializeField] private OptionalField<Color> _output_Exceptions;
        [SerializeField] private OptionalField<Color> _output_Assert;

        [field: Space, Header("UI - Default")]
        [field: SerializeField] public Color Header { get; private set; }
        [field: SerializeField] public Color Background { get; private set; }
        [field: SerializeField] public Color Foreground { get; private set; }
        [field: SerializeField] public Color Font { get; private set; }
        [field: SerializeField] public Color FontSelected { get; private set; }
        [field: SerializeField] public Color FontPressed { get; private set; }
        [field: SerializeField] public Color ScrollBar_Background { get; private set; }
        [field: SerializeField] public Color ScrollBar_Foreground { get; private set; }

        [Space, Header("UI - Overrides - Main")]
        [SerializeField] private OptionalField<Color> _main_Background;

        [Space, Header("UI - Overrides - Console List")]
        [SerializeField] private OptionalField<Color> _consoleList_Background;
        [SerializeField] private OptionalField<Color> _consoleList_Foreground;
        [SerializeField] private OptionalField<Color> _consoleList_ScrollBar_Background;
        [SerializeField] private OptionalField<Color> _consoleList_ScrollBar_Foreground;

        [field: Space, Header("UI - Overrides - Suggestions")]
        [SerializeField] private OptionalField<Color> _suggestions_Background;
        [SerializeField] private OptionalField<Color> _suggestions_Font;
        [SerializeField] private OptionalField<Color> _suggestions_FontSelected;
        [SerializeField] private OptionalField<Color> _suggestions_FontPressed;
        [SerializeField] private OptionalField<Color> _suggestions_ScrollBar_Background;
        [SerializeField] private OptionalField<Color> _suggestions_ScrollBar_Foreground;

        [Space, Header("UI - Overrides - CommandLine")]
        [SerializeField] private OptionalField<Color> _commandLine_Background;
        [SerializeField] private OptionalField<Color> _commandLine_Foreground;
        [SerializeField] private OptionalField<Color> _commandLine_Font;

        [Space, Header("UI - Overrides - CommandLine Controls")]
        [SerializeField] private OptionalField<Color> _commandLineControls_Background;
        [SerializeField] private OptionalField<Color> _commandLineControls_Font;

        [Space, Header("UI - Overrides - Resize")]
        [SerializeField] private OptionalField<Color> _resize_Background;
        [SerializeField] private OptionalField<Color> _resize_Foreground;
        [SerializeField] private OptionalField<Color> _resize_Font;

        [Space, Header("UI - Overrides - Filter")]
        [SerializeField] private OptionalField<Color> filter_Background;
        [SerializeField] private OptionalField<Color> filter_Foreground;
        [SerializeField] private OptionalField<Color> filter_Font;

        [Space, Header("UI - Overrides - Dashboard")]
        [SerializeField] private OptionalField<Color> _dashboard_Header;
        [SerializeField] private OptionalField<Color> _dashboard_Background;
        [SerializeField] private OptionalField<Color> _dashboard_Foreground;
        [SerializeField] private OptionalField<Color> _dashboard_Font;
        [SerializeField] private OptionalField<Color> _dashboard_ScrollBar_Background;
        [SerializeField] private OptionalField<Color> _dashboard_ScrollBar_Foreground;
        [Space]
        [SerializeField] private OptionalField<Color> _dashboardInfo_Background;
        [SerializeField] private OptionalField<Color> _dashboardInfo_Foreground;
        [SerializeField] private OptionalField<Color> _dashboardInfo_Font;
        [SerializeField] private OptionalField<Color> _dashboardInfo_ScrollBar_Background;
        [SerializeField] private OptionalField<Color> _dashboardInfo_ScrollBar_Foreground;
        [Space, Header("UI - Overrides - Dashboard - Items")]
        [SerializeField] private OptionalField<Color> _dashboardItem_Background;
        [SerializeField] private OptionalField<Color> _dashboardItem_Foreground;
        [SerializeField] private OptionalField<Color> _dashboardItem_Font;
        [SerializeField] private OptionalField<Color> _dashboardItem_FontSelected;
        [SerializeField] private OptionalField<Color> _dashboardItem_FontPressed;
        [SerializeField] private OptionalField<Color> _dashboardItem_ScrollBar_Background;
        [SerializeField] private OptionalField<Color> _dashboardItem_ScrollBar_Foreground;
        [SerializeField] private Color _dashboardItem_NonRequired;
        [SerializeField] private Color _dashboardItem_Required;
        [SerializeField] private Color _dashboardItem_Toggle;


        public Color Message_Log => _message_Log.AsNullable() ?? Log;
        public Color Message_Warnings => _message_Warnings.AsNullable() ?? Warnings;
        public Color Message_Errors => _message_Errors.AsNullable() ?? Errors;
        public Color Message_Exceptions => _message_Exceptions.AsNullable() ?? Exceptions;
        public Color Message_Assert => _message_Assert.AsNullable() ?? Assert;

        public Color Output_Log => _output_Log.AsNullable() ?? Log;
        public Color Output_Warnings => _output_Warnings.AsNullable() ?? Warnings;
        public Color Output_Errors => _output_Errors.AsNullable() ?? Errors;
        public Color Output_Exceptions => _output_Exceptions.AsNullable() ?? Exceptions;
        public Color Output_Assert => _output_Assert.AsNullable() ?? Assert;

        public Color Main_Background => _main_Background.AsNullable() ?? Background;

        public Color ConsoleList_Background => _consoleList_Background.AsNullable() ?? Background;
        public Color ConsoleList_Foreground => _consoleList_Foreground.AsNullable() ?? Foreground;
        public Color ConsoleList_ScrollBar_Background => _consoleList_ScrollBar_Background.AsNullable() ?? ScrollBar_Background;
        public Color ConsoleList_ScrollBar_Foreground => _consoleList_ScrollBar_Foreground.AsNullable() ?? ScrollBar_Foreground;

        public Color Suggestions_Background => _suggestions_Background.AsNullable() ?? Background;
        public Color Suggestions_Font => _suggestions_Font.AsNullable() ?? Font;
        public Color Suggestions_FontSelected => _suggestions_FontSelected.AsNullable() ?? FontSelected;
        public Color Suggestions_FontPressed => _suggestions_FontPressed.AsNullable() ?? FontPressed;
        public Color Suggestions_ScrollBar_Background => _suggestions_ScrollBar_Background.AsNullable() ?? ScrollBar_Background;
        public Color Suggestions_ScrollBar_Foreground => _suggestions_ScrollBar_Foreground.AsNullable() ?? ScrollBar_Foreground;

        public Color CommandLine_Background => _commandLine_Background.AsNullable() ?? Background;
        public Color CommandLine_Foreground => _commandLine_Foreground.AsNullable() ?? Foreground;
        public Color CommandLine_Font => _commandLine_Font.AsNullable() ?? Font;

        public Color CommandLineControls_Background => _commandLineControls_Background.AsNullable() ?? Background;
        public Color CommandLineControls_Font => _commandLineControls_Font.AsNullable() ?? Font;

        public Color Resize_Background => _resize_Background.AsNullable() ?? Background;
        public Color Resize_Foreground => _resize_Foreground.AsNullable() ?? Foreground;
        public Color Resize_Font => _resize_Font.AsNullable() ?? Font;

        public Color Filter_Background => filter_Background.AsNullable() ?? Background;
        public Color Filter_Foreground => filter_Foreground.AsNullable() ?? Foreground;
        public Color Filter_Font => filter_Font.AsNullable() ?? Font;

        public Color Dashboard_Header => _dashboard_Header.AsNullable() ?? Header;
        public Color Dashboard_Background => _dashboard_Background.AsNullable() ?? Background;
        public Color Dashboard_Foreground => _dashboard_Foreground.AsNullable() ?? Foreground;
        public Color Dashboard_Font => _dashboard_Font.AsNullable() ?? Font;
        public Color Dashboard_ScrollBar_Background => _dashboard_ScrollBar_Background.AsNullable() ?? ScrollBar_Background;
        public Color Dashboard_ScrollBar_Foreground => _dashboard_ScrollBar_Foreground.AsNullable() ?? ScrollBar_Foreground;

        public Color DashboardInfo_Background => _dashboardInfo_Background.AsNullable() ?? Background;
        public Color DashboardInfo_Foreground => _dashboardInfo_Foreground.AsNullable() ?? Foreground;
        public Color DashboardInfo_Font => _dashboardInfo_Font.AsNullable() ?? Font;
        public Color DashboardInfo_ScrollBar_Background => _dashboardInfo_ScrollBar_Background.AsNullable() ?? ScrollBar_Background;
        public Color DashboardInfo_ScrollBar_Foreground => _dashboardInfo_ScrollBar_Foreground.AsNullable() ?? ScrollBar_Foreground;

        public Color DashboardItem_Background => _dashboardItem_Background.AsNullable() ?? Background;
        public Color DashboardItem_Foreground => _dashboardItem_Foreground.AsNullable() ?? Foreground;
        public Color DashboardItem_Font => _dashboardItem_Font.AsNullable() ?? Font;
        public Color DashboardItem_FontSelected => _dashboardItem_FontSelected.AsNullable() ?? FontSelected;
        public Color DashboardItem_FontPressed => _dashboardItem_FontPressed.AsNullable() ?? FontPressed;
        public Color DashboardItem_ScrollBar_Background => _dashboardItem_ScrollBar_Background.AsNullable() ?? ScrollBar_Background;
        public Color DashboardItem_ScrollBar_Foreground => _dashboardItem_ScrollBar_Foreground.AsNullable() ?? ScrollBar_Foreground;

        public Color DashboardItem_NonRequired => _dashboardItem_NonRequired;
        public Color DashboardItem_Required => _dashboardItem_Required;
        public Color DashboardItem_Toggle => _dashboardItem_Toggle;

        public event Action Changed;

        private void OnValidate() => Changed?.Invoke();
    }
}