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
}