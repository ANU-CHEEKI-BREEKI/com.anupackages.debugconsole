using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using ANU.IngameDebug.Console;
using ANU.IngameDebug.Console.Commands;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.UnityLinker;
using UnityEngine;
#if UNITY_6000_0_OR_NEWER
using UnityEditor.Build.Profile;
#endif

namespace ANU.Editor.IngameDebug
{
    public sealed class CommandListBuildStep : IPreprocessBuildWithReport, IPostprocessBuildWithReport, IUnityLinkerProcessor
    {
        public const string ListFolder = "Assets/DebugConsoleCommandList";
        public const string ListPath = ListFolder + "/Resources/" + CommandMembers.ListResource + ".txt";

        private static string _writtenList = "";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            DeleteList();
            _writtenList = "";

            var settings = DebugConsoleSettings.instance;
            var buildDefines = BuildDefines(report.summary.platformGroup);
            var development = (report.summary.options & BuildOptions.Development) != 0;
            if (!WritesList(settings.CommandListBuilds, settings.CommandListDefineSymbols, development, buildDefines))
                return;

            var notInEditor = NotInEditor(buildDefines, EditorUserBuildSettings.activeScriptCompilationDefines);
            if (notInEditor.Count > 0)
            {
                Debug.LogWarning($"Debug console: the build defines {string.Join(", ", notInEditor)} but the editor scripts were compiled without "
                                 + "them, so the command list is not written and the player finds the commands by reflection at start. "
                                 + "Make the build profile active before building.");
                return;
            }

            WriteList(report.summary.platform);
            EditorApplication.delayCall += DeleteListAfterBuild;
        }

        public static string WriteList(BuildTarget target)
        {
            var members = PlayerMembers(target);
            var list = CommandMembers.Write(members);
            var problems = RoundTripProblems(list, members);
            if (problems.Count > 0)
                throw new BuildFailedException("Debug console: the command list does not give the same commands as the scan:\n- "
                                               + string.Join("\n- ", problems.Take(20)));

            Directory.CreateDirectory(Path.GetDirectoryName(ListPath)!);
            File.WriteAllText(ListPath, list);
            AssetDatabase.ImportAsset(ListPath, ImportAssetOptions.ForceSynchronousImport);
            _writtenList = list;

            Debug.Log($"Debug console: {members.Count} command members listed in {ListPath}");
            return list;
        }

        public void OnPostprocessBuild(BuildReport report) => DeleteList();

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            var path = Path.Combine(Path.GetFullPath("Temp"), "DebugConsoleCommandMembers.link.xml");
            File.WriteAllText(path, LinkXml(_writtenList));
            return path;
        }

#if !UNITY_6000_0_OR_NEWER
        public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data) { }

        public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data) { }
#endif

        public static bool WritesList(CommandListBuilds builds, string defineSymbols, bool development, IEnumerable<string> buildDefines)
            => builds switch
            {
                CommandListBuilds.DevelopmentBuilds => development,
                CommandListBuilds.BuildsWithDefineSymbols => Split(defineSymbols).Intersect(buildDefines, StringComparer.Ordinal).Any(),
                CommandListBuilds.AllBuilds => true,
                _ => false,
            };

        public static IReadOnlyList<string> NotInEditor(IEnumerable<string> buildDefines, IEnumerable<string> editorDefines)
            => buildDefines.Except(editorDefines ?? Array.Empty<string>(), StringComparer.Ordinal).OrderBy(d => d, StringComparer.Ordinal).ToArray();

        public static IReadOnlyList<MemberInfo> PlayerMembers(BuildTarget target)
        {
            var player = new HashSet<string>(
                CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies).Select(assembly => assembly.name)
                    .Concat(PluginImporter.GetImporters(target)
                        .Where(importer => !importer.isNativePlugin && importer.assetPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        .Select(importer => Path.GetFileNameWithoutExtension(importer.assetPath))),
                StringComparer.Ordinal);

            var assemblies = CommandMembers.CommandAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .Where(assembly => player.Contains(assembly.GetName().Name))
                .ToArray();

            return CommandMembers.Scan(assemblies);
        }

        public static List<string> RoundTripProblems(string list, IReadOnlyCollection<MemberInfo> scanned)
        {
            var (members, missing) = CommandMembers.Read(list);
            var problems = missing.Select(member => "not found again: " + member).ToList();

            var expected = CommandMembers.Create(scanned).Select(Describe).ToArray();
            var actual = CommandMembers.Create(members).Select(Describe).ToArray();
            if (!expected.SequenceEqual(actual))
                problems.AddRange(expected.Except(actual).Select(c => "lost: " + c)
                    .Concat(actual.Except(expected).Select(c => "extra: " + c))
                    .DefaultIfEmpty("the same commands in another order"));

            return problems;
        }

        public static string LinkXml(string list)
        {
            var assemblies = CommandMembers.ReadDeclarations(list)
                .Distinct()
                .GroupBy(member => member.Assembly)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(assembly => $"  <assembly fullname=\"{Escape(assembly.Key)}\">\n"
                                    + string.Concat(assembly
                                        .GroupBy(member => member.DeclaringType)
                                        .OrderBy(type => type.Key, StringComparer.Ordinal)
                                        .Select(type => $"    <type fullname=\"{Escape(type.Key.Replace('+', '/'))}\" preserve=\"nothing\">\n"
                                                        + string.Concat(type
                                                            .OrderBy(member => member.Member, StringComparer.Ordinal)
                                                            .Select(member => $"      <{Element(member.Kind)} name=\"{Escape(member.Member)}\"/>\n"))
                                                        + "    </type>\n"))
                                    + "  </assembly>\n");

            return "<linker>\n" + string.Concat(assemblies) + "</linker>\n";
        }

        public static void DeleteList()
        {
            if (AssetDatabase.IsValidFolder(ListFolder))
                AssetDatabase.DeleteAsset(ListFolder);
        }

        [InitializeOnLoadMethod]
        private static void DeleteListLeftByFailedBuild()
        {
            if (!BuildPipeline.isBuildingPlayer && File.Exists(ListPath))
                EditorApplication.delayCall += DeleteList;
        }

        // A failed build never calls OnPostprocessBuild, and the list must not stay in Assets.
        private static void DeleteListAfterBuild()
        {
            if (BuildPipeline.isBuildingPlayer)
                EditorApplication.delayCall += DeleteListAfterBuild;
            else
                DeleteList();
        }

        private static IReadOnlyList<string> BuildDefines(BuildTargetGroup group)
            => Split(PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group)))
#if UNITY_6000_0_OR_NEWER
                .Concat(BuildProfile.GetActiveBuildProfile() is { } profile && profile.scriptingDefines != null
                    ? profile.scriptingDefines
                    : Array.Empty<string>())
#endif
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        private static IEnumerable<string> Split(string symbols)
            => (symbols ?? "").Split(';', ',').Select(symbol => symbol.Trim()).Where(symbol => symbol.Length > 0);

        private static string Element(char kind) => kind switch { 'M' => "method", 'P' => "property", _ => "field" };

        private static string Escape(string text) => SecurityElement.Escape(text);

        private static string Describe(ADebugCommand command) => command.Name + " | " + command.GetType().Name;
    }
}
