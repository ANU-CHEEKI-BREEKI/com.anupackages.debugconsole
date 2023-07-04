using System;
using System.Collections.Generic;
using System.Linq;

namespace ANU.IngameDebug.Console
{
    /// <summary>
    /// Declare names of NON MonoBehaviour types you want declare debug commands inside.
    /// This will let DebugConsole know where to find DebugCommands except types derrived of MonoBehaviour, which 
    /// gonna boost performance a lot compare to searchgin all assembly types
    /// example: [assembly: RegisterDebugCommandTypes(typeof(TestNonMonoBehaviourClass), typeof(OtherNonMonoBehaviourClass))]
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterDebugCommandTypesAttribute : Attribute
    {
        public RegisterDebugCommandTypesAttribute(Type prefix, params Type[] morePrefixes)
        {
            FirstType = prefix;
            OtherTypes = morePrefixes;
        }
        private Type FirstType { get; }
        private Type[] OtherTypes { get; }

        public IEnumerable<Type> DeclaredTypes => OtherTypes.Prepend(FirstType);
    }

    // /// <summary>
    // /// Mask static method, declared in type registered by <see cref="RegisterDebugCommandTypesAttribute"/> to call it BEFORE any commands are registered
    // /// </summary>
    // [AttributeUsage(AttributeTargets.Method)]
    // public class ConsoleInitializationCallbackAttribute : Attribute
    // {

    // }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class DebugCommandPrefixAttribute : Attribute
    {
        public DebugCommandPrefixAttribute(string prefix, params string[] morePrefixes)
        {
            Prefix = prefix;
            MorePrefixes = morePrefixes;
        }
        public string Prefix { get; }
        public string[] MorePrefixes { get; }
    }

    /// <summary>
    /// - When using <see cref="AllActive"/>, <see cref="AllIncludingInactive"/>, <see cref="FirstActive"/>, <see cref="FirstIncludingInactive"/> - you can register DebugCommand as instanced command for target of any type derrived from MonoBehaviour. Because GameObject.Find.. used to find targets.
    /// - For all instanced commands option names [--targets|t] are reserved. You can pass targets as optional parameter array. Argument targets has highest priority over any <see cref="InstanceTargetType"/>
    /// - When using <see cref="Registry"/> or passing target as an argument - you can register DebugCommand for target of any type derrived from MonoBehaviour and ScriptableObject, since you manually provide the targets.  Although technically we could register methods in ALY type but then the searching all these types takes too long time.
    /// </summary>
    public enum InstanceTargetType
    {
        AllActive,
        AllIncludingInactive,
        FirstActive,
        FirstIncludingInactive,

        /// <summary>
        /// Use targets registered manually in code by <see cref="DebugConsole.InstanceTargets"/>. It target Type is not derrived from Component, Registry will be used as default <see cref="InstanceTargetType"/>
        /// </summary>
        Registry,
    }

    /// <summary>
    /// All methods, properties and fields declared directly inside any MonoBehaviour automatically registered.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DebugCommandAttribute : Attribute
    {
        public DebugCommandAttribute() { }
        
        public DebugCommandAttribute(string name) => Name = name;

        public DebugCommandAttribute(string name, string description)
            : this(name) => Description = description;

        public DebugCommandAttribute(string name, string description, InstanceTargetType target)
            : this(name, description) => Target = target;

        public string Name { get; set; }
        public string Description { get; set; }
        public InstanceTargetType Target { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class OptDescAttribute : Attribute
    {
        public string Description { get; }
        public OptDescAttribute(string description) => Description = description;
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class OptValAttribute : Attribute
    {
        public object[] AvailableValues { get; } = Array.Empty<object>();

        public OptValAttribute(object firstAvailableValue, params object[] allOtherAvailableValues)
            => AvailableValues = allOtherAvailableValues.Prepend(firstAvailableValue).ToArray();
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class OptValDynamicAttribute : Attribute
    {
        public string[] DynamicValuesProviderCommandNames { get; }

        public OptValDynamicAttribute(string dynamicValuesCommand, params string[] allOtherDynamicValuesCommand)
            => DynamicValuesProviderCommandNames = allOtherDynamicValuesCommand.Prepend(dynamicValuesCommand).ToArray();
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class OptAltNamesAttribute : Attribute
    {
        public string[] AlternativeNames { get; }

        public OptAltNamesAttribute(string firstAlternativeName, params string[] otherAlternativeNames)
            => AlternativeNames = otherAlternativeNames.Prepend(firstAlternativeName).ToArray();
    }

}