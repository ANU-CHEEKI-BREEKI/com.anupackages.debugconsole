using System;

namespace IngameDebug.Commands
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DebugCommandAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptionDescriptionAttribute : Attribute
    {
        public string Description { get; }
        public OptionDescriptionAttribute(string description) => Description = description;
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptDescAttribute : OptionDescriptionAttribute
    {
        public OptDescAttribute(string description) : base(description) { }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptionValuesAttribute : Attribute
    {
        public object[] AvailableValues { get; }

        public OptionValuesAttribute(params object[] availableValues)
        {
            AvailableValues = availableValues;
        }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptValAttribute : OptionValuesAttribute
    {
        public OptValAttribute(params object[] availableValues) : base(availableValues) { }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptionAlternativeNamesAttribute : Attribute
    {
        public string[] AlternativeNames { get; }

        public OptionAlternativeNamesAttribute(params string[] alternativeNames)
        {
            AlternativeNames = alternativeNames;
        }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptAltNamesAttribute : OptionAlternativeNamesAttribute
    {
        public OptAltNamesAttribute(params string[] alternativeNames) : base(alternativeNames) { }
    }
}