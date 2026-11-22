using UnityEditor;
using UnityEngine;

namespace ANU.Editor.IngameDebug
{
    public enum CommandListBuilds
    {
        DevelopmentBuilds,
        BuildsWithDefineSymbols,
        AllBuilds,
        Never,
    }

    [FilePath("ProjectSettings/ANUDebugConsoleSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class DebugConsoleSettings : ScriptableSingleton<DebugConsoleSettings>
    {
        [SerializeField] private CommandListBuilds _commandListBuilds = CommandListBuilds.DevelopmentBuilds;
        [SerializeField] private string _commandListDefineSymbols = "";

        public CommandListBuilds CommandListBuilds => _commandListBuilds;
        public string CommandListDefineSymbols => _commandListDefineSymbols;

        public void SetCommandList(CommandListBuilds builds, string defineSymbols)
        {
            _commandListBuilds = builds;
            _commandListDefineSymbols = defineSymbols ?? "";
            Save(true);
        }
    }

    internal static class DebugConsoleSettingsProvider
    {
        private static readonly GUIContent BuildsLabel = new GUIContent(
            "Command list in builds",
            "Which player builds get the [DebugCommand] member list written at build time. "
            + "A build without the list finds the commands by reflection at start, which is slow on IL2CPP.");

        private static readonly GUIContent SymbolsLabel = new GUIContent(
            "Define symbols",
            "Builds that have any of these scripting define symbols get the list. Separate symbols with ';'.");

        [SettingsProvider]
        public static SettingsProvider Create() => new SettingsProvider("Project/ANU Debug Console", SettingsScope.Project)
        {
            guiHandler = _ =>
            {
                var settings = DebugConsoleSettings.instance;

                EditorGUI.BeginChangeCheck();
                var builds = (CommandListBuilds)EditorGUILayout.EnumPopup(BuildsLabel, settings.CommandListBuilds);
                string symbols;
                using (new EditorGUI.DisabledScope(builds != CommandListBuilds.BuildsWithDefineSymbols))
                    symbols = EditorGUILayout.TextField(SymbolsLabel, settings.CommandListDefineSymbols);
                if (EditorGUI.EndChangeCheck())
                    settings.SetCommandList(builds, symbols);

                EditorGUILayout.HelpBox(
                    "The list is made in the editor before the build, from the assemblies the player gets, "
                    + $"and is deleted after the build ({CommandListBuildStep.ListFolder}). "
                    + "The player then registers the listed members without scanning any assembly.",
                    MessageType.None);
            },
            keywords = new[] { "debug", "console", "command", "build", "list", "reflection" },
        };
    }
}
