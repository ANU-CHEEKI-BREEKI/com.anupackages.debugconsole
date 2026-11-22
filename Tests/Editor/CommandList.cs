using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using ANU.Editor.IngameDebug;
using ANU.IngameDebug.Console.Commands;
using ANU.IngameDebug.Console.Commands.Implementations;
using ANU.IngameDebug.Console.Dashboard;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ANU.IngameDebug.Console.Editor.Tests
{
    public class CommandList
    {
        private const BindingFlags MemberFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [Test]
        public void CommandAssembliesScan_GivesTheSameCommandsAsTheFullScan()
        {
            var expected = FullScan().Select(Describe).ToArray();
            var actual = CommandMembers.Create(CommandMembers.Scan(CommandMembers.CommandAssemblies(AppDomain.CurrentDomain.GetAssemblies())))
                .Select(Describe)
                .ToArray();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.Select(Name), Is.SupersetOf(new[]
            {
                "command-list-test.method",
                "command-list-test.overload-int",
                "command-list-test.overload-string",
                "command-list-test.field",
                "command-list-test.property",
                "CommandListFixture.command-list-test.virtual",
                "CommandListFixtureChild.command-list-test.virtual",
                "command-list-test.nested",
                "CommandListFixture.command-list-test.same-name",
                "CommandListFixtureOther.command-list-test.same-name",
            }));
        }

        [Test]
        public void CommandAssemblies_KeepTheConsoleAndTheAssembliesThatReferenceIt()
        {
            var assemblies = CommandMembers.CommandAssemblies(AppDomain.CurrentDomain.GetAssemblies());

            Assert.That(assemblies, Has.Member(typeof(DebugCommandAttribute).Assembly));
            Assert.That(assemblies, Has.Member(typeof(CommandList).Assembly));
            Assert.That(assemblies, Has.No.Member(typeof(MonoBehaviour).Assembly));
            Assert.That(assemblies, Has.No.Member(typeof(object).Assembly));
        }

        [Test]
        public void BuildList_ReadsBackToTheSameCommands()
        {
            var scanned = CommandMembers.Scan(CommandMembers.CommandAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
            var (members, missing) = CommandMembers.Read(CommandMembers.Write(scanned));

            Assert.That(missing, Is.Empty);
            Assert.That(CommandMembers.Create(members).Select(Describe), Is.EqualTo(CommandMembers.Create(scanned).Select(Describe)));
        }

        [Test]
        public void BuildList_ReportsMembersThatAreNotInTheBuild()
        {
            var list = CommandMembers.Write(new[] { typeof(CommandListFixture).GetMethod("Method", MemberFlags) })
                       + "\nM\t" + QualifiedName(typeof(CommandListFixture)) + "\t" + QualifiedName(typeof(CommandListFixture)) + "\tGone\t"
                       + "\nM\tNo.Such.Type, No.Such.Assembly\tNo.Such.Type, No.Such.Assembly\tRun\t";

            var (members, missing) = CommandMembers.Read(list);

            Assert.That(members.Select(member => member.Name), Is.EqualTo(new[] { "Method" }));
            Assert.That(missing.Count, Is.EqualTo(2));
        }

        [Test]
        public void BuildList_FindsTheRightOverload()
        {
            var overloads = typeof(CommandListFixture).GetMethods(MemberFlags).Where(method => method.Name == "Overload").ToArray();
            var (members, _) = CommandMembers.Read(CommandMembers.Write(overloads));

            Assert.That(overloads.Length, Is.EqualTo(2));
            Assert.That(members, Is.EqualTo(overloads));
        }

        [Test]
        public void PlayerMembers_LeaveEditorAndTestAssembliesOut()
        {
            var members = CommandListBuildStep.PlayerMembers(EditorUserBuildSettings.activeBuildTarget);
            var assemblies = members.Select(member => member.ReflectedType.Assembly).Distinct().ToArray();
            var editorOnly = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Editor)
                .Select(assembly => assembly.name)
                .Except(UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.PlayerWithoutTestAssemblies)
                    .Select(assembly => assembly.name))
                .ToHashSet();

            Assert.That(assemblies, Has.Member(typeof(DebugCommandAttribute).Assembly));
            Assert.That(assemblies, Has.No.Member(typeof(CommandList).Assembly));
            Assert.That(assemblies.Select(assembly => assembly.GetName().Name).Where(editorOnly.Contains), Is.Empty);
            Assert.That(CommandListBuildStep.RoundTripProblems(CommandMembers.Write(members), members), Is.Empty);
        }

        [Test]
        public void LinkXml_KeepsEveryListedMember()
        {
            var members = CommandMembers.Scan(CommandMembers.CommandAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
            var xml = XDocument.Parse(CommandListBuildStep.LinkXml(CommandMembers.Write(members)));

            var kept = xml.Root!.Elements("assembly")
                .SelectMany(assembly => assembly.Elements("type").SelectMany(type => type.Elements().Select(member =>
                    $"{assembly.Attribute("fullname")!.Value}|{type.Attribute("fullname")!.Value}|{member.Name.LocalName}|{member.Attribute("name")!.Value}")))
                .ToHashSet();

            var expected = members
                .Select(member => $"{member.DeclaringType!.Assembly.GetName().Name}|{member.DeclaringType.FullName!.Replace('+', '/')}|"
                                  + $"{(member is MethodInfo ? "method" : member is PropertyInfo ? "property" : "field")}|{member.Name}")
                .Distinct()
                .ToArray();

            Assert.That(expected, Has.Some.Contains("CommandListFixture/Nested|method|Run"));
            Assert.That(kept, Is.EquivalentTo(expected));
        }

        [Test]
        public void LinkXml_OfNoListKeepsNothing()
            => Assert.That(XDocument.Parse(CommandListBuildStep.LinkXml("")).Root!.Elements(), Is.Empty);

        [Test]
        public void WriteList_PutsTheListInResourcesAndDeleteListRemovesIt()
        {
            try
            {
                var list = CommandListBuildStep.WriteList(EditorUserBuildSettings.activeBuildTarget);

                Assert.That(Resources.Load<TextAsset>(CommandMembers.ListResource)?.text, Is.EqualTo(list));
            }
            finally
            {
                CommandListBuildStep.DeleteList();
            }

            Assert.That(AssetDatabase.IsValidFolder(CommandListBuildStep.ListFolder), Is.False);
            Assert.That(System.IO.File.Exists(CommandListBuildStep.ListFolder + ".meta"), Is.False);
        }

        [TestCase(CommandListBuilds.DevelopmentBuilds, "", true, "", ExpectedResult = true)]
        [TestCase(CommandListBuilds.DevelopmentBuilds, "", false, "", ExpectedResult = false)]
        [TestCase(CommandListBuilds.BuildsWithDefineSymbols, "DEBUG_BUILD", false, "A;DEBUG_BUILD", ExpectedResult = true)]
        [TestCase(CommandListBuilds.BuildsWithDefineSymbols, "QA ; DEBUG_BUILD", false, "DEBUG_BUILD", ExpectedResult = true)]
        [TestCase(CommandListBuilds.BuildsWithDefineSymbols, "DEBUG_BUILD", true, "A;B", ExpectedResult = false)]
        [TestCase(CommandListBuilds.BuildsWithDefineSymbols, "", true, "A", ExpectedResult = false)]
        [TestCase(CommandListBuilds.AllBuilds, "", false, "", ExpectedResult = true)]
        [TestCase(CommandListBuilds.Never, "DEBUG_BUILD", true, "DEBUG_BUILD", ExpectedResult = false)]
        public bool WritesList_FollowsTheSetting(CommandListBuilds builds, string symbols, bool development, string buildDefines)
            => CommandListBuildStep.WritesList(builds, symbols, development, buildDefines.Split(';').Where(d => d.Length > 0));

        [Test]
        public void NotInEditor_NamesTheBuildDefinesTheEditorWasCompiledWithout()
            => Assert.That(CommandListBuildStep.NotInEditor(new[] { "B", "A", "C" }, new[] { "A", "UNITY_EDITOR" }), Is.EqualTo(new[] { "B", "C" }));

        private static IReadOnlyList<ADebugCommand> FullScan()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(MonoBehaviour).IsAssignableFrom(type))
                .Concat(assemblies.SelectMany(assembly => assembly.GetCustomAttributes<RegisterDebugCommandTypesAttribute>()).SelectMany(attribute => attribute.DeclaredTypes))
                .Distinct()
                .ToArray();

            var byName = new Dictionary<string, ADebugCommand>();
            Add(types.SelectMany(type => type.GetMethods(MemberFlags)), (method, prefix) => new MethodCommand(method, null, prefix));
            Add(types.SelectMany(type => type.GetProperties(MemberFlags)), (property, prefix) => new PropertyCommand(property, null, prefix));
            Add(types.SelectMany(type => type.GetFields(MemberFlags)), (field, prefix) => new FieldCommand(field, null, prefix));
            return byName.Values.ToArray();

            void Add<T>(IEnumerable<T> members, Func<T, string, ADebugCommand> create) where T : MemberInfo
            {
                var groups = members
                    .Where(member => member.GetCustomAttribute<DebugCommandAttribute>() is { } attribute && attribute.Platforms.HasCurrentPlatform())
                    .GroupBy(member => create(member, "").Name)
                    .ToArray();

                foreach (var group in groups)
                foreach (var member in group)
                {
                    var command = create(member, group.Skip(1).Any() ? member.DeclaringType.Name : "");
                    byName[command.Name] = command;
                }
            }
        }

        private static string QualifiedName(Type type) => type.FullName + ", " + type.Assembly.GetName().Name;

        private static string Name(string described) => described.Substring(0, described.IndexOf(" | ", StringComparison.Ordinal));

        private static string Describe(ADebugCommand command)
        {
            var member = FindMember(command);
            return $"{command.Name} | {command.GetType().Name} | {member?.DeclaringType?.FullName}.{member?.Name} | {member?.ReflectedType?.FullName}";
        }

        private static MemberInfo FindMember(ADebugCommand command)
        {
            for (var type = command.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField("_member", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                    return field.GetValue(command) as MemberInfo;
            }

            return null;
        }
    }

    public class CommandListFixture : MonoBehaviour
    {
        [DebugCommand("command-list-test.field")] public static int Field;

        [DebugCommand("command-list-test.property")] private static float Property { get; set; }

        [DebugCommand("command-list-test.method")]
        private static void Method(int value) { }

        [DebugCommand("command-list-test.overload-int")]
        private static void Overload(int value) { }

        [DebugCommand("command-list-test.overload-string")]
        private static void Overload(string value) { }

        [DebugCommand("command-list-test.same-name")]
        private static void SameName() { }

        [DebugCommand("command-list-test.virtual")]
        protected virtual void Virtual() { }

        public class Nested : MonoBehaviour
        {
            [DebugCommand("command-list-test.nested")]
            public static void Run() { }
        }
    }

    public class CommandListFixtureChild : CommandListFixture
    {
        protected override void Virtual() { }
    }

    public class CommandListFixtureOther : MonoBehaviour
    {
        [DebugCommand("command-list-test.same-name")]
        private static void SameName() { }
    }
}
