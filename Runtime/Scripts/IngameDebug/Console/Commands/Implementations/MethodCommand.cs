using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using NDesk.Options;
using Unity.Profiling;
using UnityEngine;

namespace ANU.IngameDebug.Console.Commands.Implementations
{
    internal static class ReflectionCommandsExtensions
    {
        private static readonly ProfilerMarker _getAttribute = new ProfilerMarker("GetAttribute");
        private static ProfilerMarker _getName = new ProfilerMarker("GetName");
        private static ProfilerMarker _getDescription = new ProfilerMarker("GetDescription");

        public static string GenerateCommandName(this MemberInfo member, string prefix = "")
        {
            using (_getName.Auto())
            {
                DebugCommandAttribute attribute;
                DebugCommandPrefixAttribute prefixAttribute;
                using (_getAttribute.Auto())
                {
                    attribute = member.GetCustomAttribute<DebugCommandAttribute>();
                    prefixAttribute = member.DeclaringType?.GetCustomAttribute<DebugCommandPrefixAttribute>();
                }

                var allPrefixes = prefixAttribute?.MorePrefixes?.Prepend(prefixAttribute.Prefix) ?? Array.Empty<string>();
                if (!string.IsNullOrEmpty(prefix))
                    allPrefixes = allPrefixes.Prepend(prefix);

                prefix = allPrefixes.Any()
                    ? string.Join(".", allPrefixes) + "."
                    : "";

                var name = string.IsNullOrEmpty(attribute?.Name)
                    ? string.Join("",
                        member.Name
                            .Select(c =>
                                char.IsUpper(c) ? "-" + char.ToLower(c) : c.ToString()
                            )
                        ).Trim('-')
                    : attribute.Name;

                return prefix + name;
            }
        }

        public static string GenerateCommandDescription(this MemberInfo member)
        {
            using (_getDescription.Auto())
            {
                DebugCommandAttribute attribute;

                using (_getAttribute.Auto())
                    attribute = member.GetCustomAttribute<DebugCommandAttribute>();

                return attribute?.Description;
            }
        }
    }

    public abstract class MemberCommand<T> : ADebugCommand where T : MemberInfo
    {
        private static ProfilerMarker _createOptions = new ProfilerMarker("CreateOptions");

        protected readonly object _instance;
        protected readonly T _member;

        private readonly DebugCommandAttribute _attribute;

        private object[] _dynamicInstances;

        public MemberCommand(string name, string description, T member, object instance) : base(name, description)
        {
            _instance = instance;
            _member = member;
            _attribute = _member.GetCustomAttribute<DebugCommandAttribute>();

            ResetParametersValues();
        }

        protected virtual void ResetParametersValues() => _dynamicInstances = Array.Empty<object>();

        protected sealed override ExecutionResult OnParsed()
        {
            var returnValues = new List<SingleExecutionResult>();

            try
            {
                ValidateParameters();

                var instances = GetInstances();

                if (!instances.Any())
                    throw new Exception("There are no provided targets tor NonStatic method. For more information see InstanceTargetType documentation");

                foreach (var target in instances)
                {
                    try
                    {
                        returnValues.Add(new SingleExecutionResult(
                            target,
                            Invoke(_member, target)

                        ));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);
                    }
                }
            }
            finally
            {
                ResetParametersValues();
            }

            return new ExecutionResult(
                ReturnValueType,
                returnValues
            );
        }

        protected abstract object Invoke(T member, object item);
        protected abstract void ValidateParameters();
        protected abstract Type ReturnValueType { get; }

        protected sealed override OptionSet CreateOptions(Dictionary<Option, AvailableValuesHint> valueHints)
        {
            using (_createOptions.Auto())
            {
                var options = new OptionSet();
                CreateOptions(valueHints, options);

                if (!IsStatic(_member) && _instance == null)
                {
                    // add optional parameter for dynamic targets
                    options.Add(
                        "targets|t:",
                        "Provide targets for instances command. This has highest priority over any `InstanceTargetType`",
                        inputString => _dynamicInstances = Context.Converters.ConvertFromString(
                            _member.DeclaringType.MakeArrayType(),
                            inputString?.Trim('"')?.Trim('\'') ?? ""
                        ) as object[]
                    );
                }

                return options;
            }
        }

        protected abstract void CreateOptions(Dictionary<Option, AvailableValuesHint> valueHints, OptionSet options);

        protected abstract bool IsStatic(T member);

        private IEnumerable<object> GetInstances()
        {
            if (_instance != null || IsStatic(_member))
                return new object[] { _instance };

            var targets = Array.Empty<object>().AsEnumerable();

            if (_attribute == null)
                return targets;

            if (_dynamicInstances != null && _dynamicInstances.Any())
                return _dynamicInstances;


            var instanceTarget = _attribute.Target;
            if (!typeof(UnityEngine.Component).IsAssignableFrom(_member.DeclaringType))
                instanceTarget = InstanceTargetType.Registry;

            var includeInactive = false
                || instanceTarget == InstanceTargetType.AllIncludingInactive
                || instanceTarget == InstanceTargetType.FirstIncludingInactive;

            switch (instanceTarget)
            {
                case InstanceTargetType.AllActive:
                case InstanceTargetType.AllIncludingInactive:
#if UNITY_2023_0_OR_NEWER
                    targets = GameObject.FindObjectsByType(
                        _member.DeclaringType,
                        includeInactive
                            ? FindObjectsInactive.Include
                            : FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None
                    );
#else
                    targets = GameObject.FindObjectsOfType(
                        _member.DeclaringType,
                        includeInactive
                    );
#endif
                    break;
                case InstanceTargetType.FirstActive:
                case InstanceTargetType.FirstIncludingInactive:
#if UNITY_2023_0_OR_NEWER
                    var target = GameObject.FindFirstObjectByType(
                        _member.DeclaringType,
                        includeInactive
                            ? FindObjectsInactive.Include
                            : FindObjectsInactive.Exclude
                    );
#else
                    var target = GameObject.FindObjectOfType(
                        _member.DeclaringType,
                        includeInactive
                    );
#endif
                    targets = new object[] { target };
                    break;
                case InstanceTargetType.Registry:
                    targets = Context.InstanceTargets.Get(_member.DeclaringType);
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            return targets;
        }
    }

    public class MethodCommand : MemberCommand<MethodInfo>
    {
        private readonly object[] _parameterValues;
        private readonly bool[] _isParameterValid;
        private readonly ParameterInfo[] _parameters;

        public MethodCommand(MethodInfo method, string prefix = "")
            : this(method, (object)null, prefix) { }

        public MethodCommand(MethodInfo method, object instance, string prefix = "")
            : this(method.GenerateCommandName(prefix), method.GenerateCommandDescription(), method, instance) { }

        public MethodCommand(string name, string description, MethodInfo method, object instance) : base(name, description, method, instance)
        {
            _parameters = method.GetParameters();
            _parameterValues = new object[_parameters.Length];
            _isParameterValid = new bool[_parameters.Length];

            ResetParametersValues();
        }

        protected override Type ReturnValueType => _member.ReturnType;

        protected override void ResetParametersValues()
        {
            base.ResetParametersValues();

            if (_parameters == null)
                return;

            for (int i = 0; i < _parameters.Length; i++)
            {
                var parameter = _parameters[i];
                _isParameterValid[i] = parameter.HasDefaultValue || parameter.ParameterType == typeof(bool);
                _parameterValues[i] = parameter.HasDefaultValue
                    ? parameter.DefaultValue
                    : parameter.ParameterType.IsValueType
                        ? Activator.CreateInstance(parameter.ParameterType)
                        : null;
            }
        }

        protected override object Invoke(MethodInfo method, object instance)
        {
            var returnValue = method.Invoke(instance, _parameterValues);
            return returnValue;
        }

        protected override void ValidateParameters()
        {
            var allParametersValide = true;
            var builder = new StringBuilder();
            for (int i = 0; i < _isParameterValid.Length; i++)
            {
                var isValide = _isParameterValid[i];
                if (isValide)
                    continue;
                allParametersValide = false;
                builder.AppendLine(_parameters[i].Name);
            }

            if (!allParametersValide)
            {
                builder.Insert(0, "There are some parameters not set" + Environment.NewLine);
                throw new ArgumentException(builder.ToString());
            }
        }

        protected override void CreateOptions(Dictionary<Option, AvailableValuesHint> valueHints, OptionSet options)
        {
            for (int i = 0; i < _parameters.Length; i++)
            {
                var parameterIndex = i;
                var parameter = _parameters[parameterIndex];

                var isOptional = parameter.HasDefaultValue;
                var isFlag = parameter.ParameterType == typeof(bool) && isOptional && ((bool)parameter.DefaultValue == false);

                var optionName = parameter.Name;
                // var optionKey = optionName;

                var altNames = parameter.GetCustomAttribute<OptAltNamesAttribute>();
                if (altNames != null)
                    optionName = string.Join("|", altNames.AlternativeNames.Prepend(optionName));

                if (!isFlag)
                    optionName += isOptional ? ":" : "=";

                var optionDescription = parameter.GetCustomAttribute<OptDescAttribute>()?.Description;

                Action<string> action = value =>
                {
                    value = value.Trim('"').Trim('\'');

                    if (isOptional && value == null)
                    {
                        // do nothing
                    }
                    else
                    {
                        _parameterValues[parameterIndex] = isFlag
                            ? value != null
                            : Context.Converters.ConvertFromString(parameter.ParameterType, value);

                        _isParameterValid[parameterIndex] = true;
                    }
                };

                options.Add(
                    optionName,
                    optionDescription,
                    action
                );

                var valAsKey = optionName.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                var opt = options[valAsKey.Last().TrimEnd('=', ':')];

                IEnumerable<string> hints = Array.Empty<string>();

                var values = parameter.GetCustomAttribute<OptValAttribute>();
                var valuesDynamic = parameter.GetCustomAttribute<OptValDynamicAttribute>();

                if (values != null)
                    hints = values.AvailableValues.Select(v => v.ToString());
                else if (parameter.ParameterType.IsEnum)
                    hints = Enum.GetNames(parameter.ParameterType);
                else if (parameter.ParameterType == typeof(bool))
                    hints = new string[] { "true", "false" };

                valueHints[opt] = new AvailableValuesHint(hints, valuesDynamic?.DynamicValuesProviderCommandNames);
            }
        }

        protected override bool IsStatic(MethodInfo member) => member.IsStatic;
    }

    public abstract class GetSetCommand<T> : MemberCommand<T> where T : MemberInfo
    {
        private enum InvikeType { Get, Set }

        private readonly Type _parameterType;
        private object _parameterValue;
        private InvikeType _invokeType;

        public GetSetCommand(string name, string description, T member, object instance)
            : base(name, description, member, instance)
        {
            _parameterType = GetMemberType();
            ResetParametersValues();
        }

        protected abstract Type GetMemberType();

        protected override void ResetParametersValues()
        {
            base.ResetParametersValues();
            _parameterValue = null;
            _invokeType = InvikeType.Get;
        }

        protected override void CreateOptions(Dictionary<Option, AvailableValuesHint> valueHints, OptionSet options)
        {
            var optionNames = new string[] { "value", "v" };

            var altNames = _member.GetCustomAttribute<OptAltNamesAttribute>()?.AlternativeNames;
            if (altNames != null)
                optionNames = altNames;

            var optionName = string.Join("|", optionNames);
            optionName += ":";

            var optionDescription = _member.GetCustomAttribute<OptDescAttribute>()?.Description;

            IEnumerable<string> hints = null;

            var values = _member.GetCustomAttribute<OptValAttribute>();
            if (values != null)
                hints = values.AvailableValues.Select(v => v.ToString());
            else if (_parameterType.IsEnum)
                hints = Enum.GetNames(_parameterType);
            else if (_parameterType == typeof(bool))
                hints = new string[] { "true", "false" };

            options.Add(optionName, optionDescription, value =>
            {
                value = value.Trim('"').Trim('\'');

                if (value == null)
                {
                    _invokeType = InvikeType.Get;
                }
                else
                {
                    _invokeType = InvikeType.Set;
                    _parameterValue = Context.Converters.ConvertFromString(_parameterType, value);
                }
            });

            var valAsKey = optionName.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var opt = options[valAsKey.Last().TrimEnd('=', ':')];

            if (hints != null)
                valueHints[opt] = new AvailableValuesHint(hints);
        }

        protected override void ValidateParameters() { }

        protected sealed override object Invoke(T member, object instance)
        {
            if (_invokeType == InvikeType.Get)
            {
                return GetValue(member, instance);
            }
            else
            {
                SetValue(member, instance, _parameterValue);
                return null;
            }
        }

        protected abstract void SetValue(T member, object instance, object value);
        protected abstract object GetValue(T member, object instance);
    }

    public class FieldCommand : GetSetCommand<FieldInfo>
    {
        public FieldCommand(FieldInfo field, string prefix = "")
           : this(field, (object)null, prefix) { }

        public FieldCommand(FieldInfo field, object instance, string prefix = "")
            : this(field.GenerateCommandName(prefix), field.GenerateCommandDescription(), field, instance) { }

        public FieldCommand(string name, string description, FieldInfo field, object instance)
            : base(name, description, field, instance) { }

        protected override Type ReturnValueType => _member.FieldType;

        protected override Type GetMemberType() => _member.FieldType;
        protected override bool IsStatic(FieldInfo member) => member.IsStatic;
        protected override object GetValue(FieldInfo member, object instance) => member.GetValue(instance);
        protected override void SetValue(FieldInfo member, object instance, object value) => member.SetValue(instance, value);
    }

    public class PropertyCommand : GetSetCommand<PropertyInfo>
    {
        public PropertyCommand(PropertyInfo property, string prefix = "")
           : this(property, (object)null, prefix) { }

        public PropertyCommand(PropertyInfo property, object instance, string prefix = "")
            : this(property.GenerateCommandName(prefix), property.GenerateCommandDescription(), property, instance) { }

        public PropertyCommand(string name, string description, PropertyInfo property, object instance)
            : base(name, description, property, instance) { }

        protected override Type ReturnValueType => _member.PropertyType;

        protected override Type GetMemberType() => _member.PropertyType;
        protected override bool IsStatic(PropertyInfo member) => member.GetAccessors(true)[0].IsStatic;
        protected override object GetValue(PropertyInfo member, object instance) => member.GetValue(instance);
        protected override void SetValue(PropertyInfo member, object instance, object value) => member.SetValue(instance, value);
    }
}