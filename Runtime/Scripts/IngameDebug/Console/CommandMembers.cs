using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ANU.IngameDebug.Console.Commands;
using ANU.IngameDebug.Console.Commands.Implementations;
using ANU.IngameDebug.Console.Dashboard;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    public static class CommandMembers
    {
        public const string ListResource = "DebugConsoleCommandMembers";

        private const BindingFlags MemberFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const char MethodKind = 'M';
        private const char PropertyKind = 'P';
        private const char FieldKind = 'F';

        public static Assembly[] CommandAssemblies(IEnumerable<Assembly> assemblies)
        {
            var console = typeof(DebugCommandAttribute).Assembly.GetName().Name;
            return assemblies
                .Where(assembly => assembly.GetName().Name == console
                    || assembly.GetReferencedAssemblies().Any(reference => reference.Name == console))
                .ToArray();
        }

        public static IReadOnlyList<MemberInfo> Scan(IReadOnlyCollection<Assembly> assemblies)
        {
            var types = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(MonoBehaviour).IsAssignableFrom(type))
                .Concat(assemblies
                    .SelectMany(assembly => assembly.GetCustomAttributes<RegisterDebugCommandTypesAttribute>())
                    .Where(attribute => attribute != null)
                    .SelectMany(attribute => attribute.DeclaredTypes))
                .Distinct()
                .ToArray();

            var inherited = new InheritedFromOtherAssemblies(assemblies);
            return Members(types, inherited, type => type.GetMethods(MemberFlags))
                .Concat(Members(types, inherited, type => type.GetProperties(MemberFlags)))
                .Concat(Members(types, inherited, type => type.GetFields(MemberFlags)))
                .ToArray();
        }

        public static IReadOnlyList<ADebugCommand> Create(IReadOnlyCollection<MemberInfo> members)
        {
            var byName = new Dictionary<string, ADebugCommand>();
            foreach (var command in Commands(members.OfType<MethodInfo>(), (method, prefix) => new MethodCommand(method, null, prefix))
                .Concat(Commands(members.OfType<PropertyInfo>(), (property, prefix) => new PropertyCommand(property, null, prefix)))
                .Concat(Commands(members.OfType<FieldInfo>(), (field, prefix) => new FieldCommand(field, null, prefix))))
                byName[command.Name] = command;

            return byName.Values.ToArray();
        }

        public static string LoadBuildList()
        {
            if (Application.isEditor)
                return null;

            var asset = Resources.Load<TextAsset>(ListResource);
            if (!asset)
                return null;

            var text = asset.text;
            Resources.UnloadAsset(asset);
            return text;
        }

        public static string Write(IEnumerable<MemberInfo> members) => string.Join("\n", members.Select(Line));

        public static (IReadOnlyList<MemberInfo> Members, IReadOnlyList<string> Missing) Read(string list)
        {
            var types = new Dictionary<string, Type>();
            var members = new List<MemberInfo>();
            var missing = new List<string>();

            foreach (var line in list.Split('\n'))
            {
                var parts = line.TrimEnd('\r').Split('\t');
                if (parts.Length != 5)
                    continue;

                var member = Find(TypeOf(parts[1]), parts[0][0], parts[2], parts[3], parts[4]);
                if (member == null)
                    missing.Add($"{parts[2]}.{parts[3]} on {parts[1]}");
                else
                    members.Add(member);
            }

            return (members, missing);

            Type TypeOf(string name)
            {
                if (!types.TryGetValue(name, out var type))
                    types[name] = type = Type.GetType(name, false);

                return type;
            }
        }

        public static IEnumerable<(char Kind, string Assembly, string DeclaringType, string Member)> ReadDeclarations(string list)
            => list.Split('\n')
                .Select(line => line.TrimEnd('\r').Split('\t'))
                .Where(parts => parts.Length == 5)
                .Select(parts => (parts[0][0], AssemblyOf(parts[2]), TypeNameOf(parts[2]), parts[3]));

        private static string Line(MemberInfo member)
            => string.Join("\t",
                member switch { MethodInfo => MethodKind, PropertyInfo => PropertyKind, _ => FieldKind },
                Qualified(member.ReflectedType),
                Qualified(member.DeclaringType),
                member.Name,
                member is MethodInfo method ? Signature(method) : "");

        private static string Qualified(Type type) => type.FullName + ", " + type.Assembly.GetName().Name;

        private static string TypeNameOf(string qualified) => qualified.Substring(0, qualified.LastIndexOf(", ", StringComparison.Ordinal));

        private static string AssemblyOf(string qualified) => qualified.Substring(qualified.LastIndexOf(", ", StringComparison.Ordinal) + 2);

        private static string Signature(MethodInfo method) => string.Join(",", method.GetParameters().Select(parameter => parameter.ParameterType.ToString()));

        private static MemberInfo Find(Type type, char kind, string declaringType, string name, string signature)
        {
            if (type == null)
                return null;

            var memberType = kind switch { MethodKind => MemberTypes.Method, PropertyKind => MemberTypes.Property, _ => MemberTypes.Field };
            return type.GetMember(name, memberType, MemberFlags)
                .FirstOrDefault(member => member.DeclaringType != null
                    && Qualified(member.DeclaringType) == declaringType
                    && (member is not MethodInfo method || Signature(method) == signature));
        }

        private static IEnumerable<MemberInfo> Members<T>(Type[] types, InheritedFromOtherAssemblies inherited, Func<Type, T[]> members)
            where T : MemberInfo
            => types
                .SelectMany(type => members(type).Where(member => !inherited.Contains(type, member)))
                .Where(member => Attribute.IsDefined(member, typeof(DebugCommandAttribute), MayInheritAttributes(member)));

        private static IEnumerable<ADebugCommand> Commands<T>(IEnumerable<T> members, Func<T, string, ADebugCommand> create)
            where T : MemberInfo
            => members
                .Select(member => (member, attribute: member.GetCustomAttribute<DebugCommandAttribute>()))
                .Where(entry => entry.attribute != null && entry.attribute.Platforms.HasCurrentPlatform())
                .Select(entry => (entry.member, command: create(entry.member, "")))
                .GroupBy(entry => entry.command.Name)
                .SelectMany(group => group.Skip(1).Any()
                    ? group.Select(entry => create(entry.member, entry.member.DeclaringType.Name))
                    : group.Select(entry => entry.command))
                .ToArray();

        private static bool MayInheritAttributes(MemberInfo member)
            => member is PropertyInfo || member is MethodInfo { IsVirtual: true };

        // Matched by handles, not by DeclaringType.Assembly: DeclaringType is slow on Mono, and over the ~160k members a
        // big game's MonoBehaviours inherit from UnityEngine it cost more than everything else in the scan together.
        private sealed class InheritedFromOtherAssemblies
        {
            private static readonly (HashSet<RuntimeMethodHandle> methods, HashSet<RuntimeFieldHandle> fields) None = (new(), new());

            private readonly HashSet<Assembly> _scanned;
            private readonly Dictionary<Type, Type> _firstOtherBase = new();
            private readonly Dictionary<Type, (HashSet<RuntimeMethodHandle> methods, HashSet<RuntimeFieldHandle> fields)> _members = new();

            public InheritedFromOtherAssemblies(IEnumerable<Assembly> scanned) => _scanned = new HashSet<Assembly>(scanned);

            public bool Contains(Type type, MemberInfo member)
            {
                var members = MembersOf(FirstOtherBase(type));
                return member switch
                {
                    MethodInfo method => members.methods.Contains(method.MethodHandle),
                    FieldInfo field => members.fields.Contains(field.FieldHandle),
                    PropertyInfo property => (property.GetMethod ?? property.SetMethod) is { } accessor
                        && members.methods.Contains(accessor.MethodHandle),
                    _ => false,
                };
            }

            private Type FirstOtherBase(Type type)
            {
                if (_firstOtherBase.TryGetValue(type, out var other))
                    return other;

                other = type.BaseType;
                while (other != null && _scanned.Contains(other.Assembly))
                    other = other.BaseType;

                return _firstOtherBase[type] = other;
            }

            private (HashSet<RuntimeMethodHandle> methods, HashSet<RuntimeFieldHandle> fields) MembersOf(Type type)
            {
                if (type == null)
                    return None;

                if (_members.TryGetValue(type, out var members))
                    return members;

                members = (
                    new HashSet<RuntimeMethodHandle>(type.GetMethods(MemberFlags).Select(method => method.MethodHandle)),
                    new HashSet<RuntimeFieldHandle>(type.GetFields(MemberFlags).Select(field => field.FieldHandle)));
                return _members[type] = members;
            }
        }
    }
}
