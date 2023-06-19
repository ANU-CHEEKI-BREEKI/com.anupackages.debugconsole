using System;
using System.Linq;

namespace ANU.IngameDebug.Console
{
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
    /// - When using <see cref="AllActive"/>, <see cref="AllIncludingInactive"/>, <see cref="FirstActive"/>, <see cref="FirstIncludingInactive"/> - you can execute DebugCommand as instanced command for target of any type derrived from Component. Because GameObject.Find.. used to find targets.
    /// - For all instanced commands option names [--targets|t] are reserved. You can pass targets as optional parameter array. Argument targets has highest priority over any <see cref="InstanceTargetType"/>
    /// - When using <see cref="Registry"/> or passing target as an argument - you can execute DebugCommand for target of any Type, since you manually provide the targets.
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

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DebugCommandAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public InstanceTargetType Target { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptDescAttribute : Attribute
    {
        public string Description { get; }
        public OptDescAttribute(string description) => Description = description;
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptValAttribute : Attribute
    {
        public object[] AvailableValues { get; }

        public OptValAttribute(object firstAvailableValue, params object[] allOtherAvailableValues)
            => AvailableValues = allOtherAvailableValues.Prepend(firstAvailableValue).ToArray();
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptAltNamesAttribute : Attribute
    {
        public string[] AlternativeNames { get; }

        public OptAltNamesAttribute(string firstAlternativeName, params string[] otherAlternativeNames)
            => AlternativeNames = otherAlternativeNames.Prepend(firstAlternativeName).ToArray();
    }

}