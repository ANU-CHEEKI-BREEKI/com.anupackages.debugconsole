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
    public class MethodCommand : ADebugCommand
    {
        private static ProfilerMarker _getAttribute = new ProfilerMarker("GetAttribute");
        private static ProfilerMarker _getName = new ProfilerMarker("GetNAme");
        private static ProfilerMarker _getDescription = new ProfilerMarker("GetDescription");
        private static ProfilerMarker _createOptions = new ProfilerMarker("CreateOptions");

        private static readonly Dictionary<MethodInfo, DebugCommandAttribute> _cachedAttributes = new();
        private static readonly Dictionary<Type, DebugCommandPrefixAttribute> _cachedAttributes2 = new();

        private readonly object _instance;
        private readonly MethodInfo _method;

        private readonly DebugCommandAttribute _attribute;

        private readonly object[] _parameterValues;
        private readonly bool[] _isParameterValid;
        private readonly ParameterInfo[] _parameters;

        private object[] _dynamicInstances;

        public MethodCommand(MethodInfo method, string prefix = "")
            : this(method, (object)null, prefix) { }

        public MethodCommand(MethodInfo method, object instance, string prefix = "")
            : this(GetName(method, prefix), GetDescription(method), method, instance) { }

        public MethodCommand(string name, string description, MethodInfo method, object instance) : base(name, description)
        {
            _instance = instance;
            _method = method;
            _attribute = GetCachedCommandAttribute(_method);

            _parameters = method.GetParameters();
            _parameterValues = new object[_parameters.Length];
            _isParameterValid = new bool[_parameters.Length];

            ResetParametersValues();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearStatic()
        {
            _cachedAttributes?.Clear();
            _cachedAttributes2?.Clear();
        }

        public static string GetName(MethodInfo method, string prefix = "")
        {
            using (_getName.Auto())
            {
                var attribute = GetCachedCommandAttribute(method);
                var prefixAttribute = GetCachedPrefixAttribute(method);

                var allPrefixes = prefixAttribute?.MorePrefixes?.Prepend(prefixAttribute.Prefix) ?? Array.Empty<string>();
                if (!string.IsNullOrEmpty(prefix))
                    allPrefixes = allPrefixes.Prepend(prefix);

                prefix = allPrefixes.Any()
                    ? string.Join(".", allPrefixes) + "."
                    : "";

                var name = string.IsNullOrEmpty(attribute?.Name)
                    ? string.Join("",
                        method.Name
                            .Select(c =>
                                char.IsUpper(c) ? "-" + char.ToLower(c) : c.ToString()
                            )
                        ).Trim('-')
                    : attribute.Name;

                return prefix + name;
            }
        }

        public static DebugCommandPrefixAttribute GetCachedPrefixAttribute(MethodInfo method)
        {
            using (_getAttribute.Auto())
            {
                if (!_cachedAttributes2.ContainsKey(method.DeclaringType))
                    _cachedAttributes2[method.DeclaringType] = method.DeclaringType.GetCustomAttribute<DebugCommandPrefixAttribute>(false);
                return _cachedAttributes2[method.DeclaringType];
            }
        }

        public static DebugCommandAttribute GetCachedCommandAttribute(MethodInfo method)
        {
            using (_getAttribute.Auto())
            {
                if (!_cachedAttributes.ContainsKey(method))
                    _cachedAttributes[method] = method.GetCustomAttribute<DebugCommandAttribute>(false);
                return _cachedAttributes[method];
            }
        }

        public static string GetDescription(MethodInfo method)
        {
            using (_getDescription.Auto())
            {
                var attribute = GetCachedCommandAttribute(method);
                return attribute?.Description;
            }
        }

        private void ResetParametersValues()
        {
            _dynamicInstances = Array.Empty<object>();

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

        protected override void OnParsed()
        {
            try
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

                var instances = GetInstances();

                if (!instances.Any())
                    throw new Exception("There are no provided targets tor NonStatic method. For more information see InstanceTargetType documentation");

                foreach (var item in instances)
                {
                    try
                    {
                        _method.Invoke(item, _parameterValues);
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
        }

        protected override OptionSet CreateOptions(Dictionary<Option, AvailableValuesHint> valueHints)
        {
            using (_createOptions.Auto())
            {
                var options = new OptionSet();

                for (int i = 0; i < _parameters.Length; i++)
                {
                    var parameterIndex = i;
                    var parameter = _parameters[parameterIndex];

                    var isOptional = parameter.HasDefaultValue;
                    var isFlag = parameter.ParameterType == typeof(bool) && isOptional && ((bool)parameter.DefaultValue == false);

                    var optionName = parameter.Name;

                    var altNames = parameter.GetCustomAttribute<OptAltNamesAttribute>();
                    if (altNames != null)
                        optionName = string.Join("|", altNames.AlternativeNames.Prepend(optionName));

                    if (!isFlag)
                        optionName += isOptional ? ":" : "=";

                    var optionDescription = parameter.GetCustomAttribute<OptDescAttribute>()?.Description;

                    options.Add(
                        optionName,
                        optionDescription,
                        value =>
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
                                    : DebugConsole.Converters.Convert(parameter.ParameterType, value);

                                _isParameterValid[parameterIndex] = true;
                            }
                        }
                    );
                    var valAsKey = optionName.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    var opt = options[valAsKey.Last().TrimEnd('=', ':')];

                    IEnumerable<string> hints = null;

                    var values = parameter.GetCustomAttribute<OptValAttribute>();
                    if (values != null)
                        hints = values.AvailableValues.Select(v => v.ToString());
                    else if (parameter.ParameterType.IsEnum)
                        hints = Enum.GetNames(parameter.ParameterType);
                    else if (parameter.ParameterType == typeof(bool))
                        hints = new string[] { "true", "false" };

                    if (hints != null)
                        valueHints[opt] = new AvailableValuesHint(hints);
                }

                if (!_method.IsStatic && _instance == null)
                {
                    // add optional parameter for dynamic targets
                    options.Add(
                        "targets|t:",
                        "Provide targets for instances command. This has highest priority over any `InstanceTargetType`",
                        inputString => _dynamicInstances = DebugConsole.Converters.Convert(
                            _method.DeclaringType.MakeArrayType(),
                            inputString.Trim('"').Trim('\'')
                        ) as object[]
                    );
                }

                return options;
            }
        }

        private IEnumerable<object> GetInstances()
        {
            if (_instance != null || _method.IsStatic)
                return new object[] { _instance };

            var targets = Array.Empty<object>().AsEnumerable();

            if (_attribute == null)
                return targets;

            if (_dynamicInstances != null && _dynamicInstances.Any())
                return _dynamicInstances;


            var instanceTarget = _attribute.Target;
            if (!typeof(UnityEngine.Component).IsAssignableFrom(_method.DeclaringType))
                instanceTarget = InstanceTargetType.Registry;

            var includeInactive = false
                || instanceTarget == InstanceTargetType.AllIncludingInactive
                || instanceTarget == InstanceTargetType.FirstIncludingInactive;

            switch (instanceTarget)
            {
                case InstanceTargetType.AllActive:
                case InstanceTargetType.AllIncludingInactive:
                    targets = GameObject.FindObjectsByType(
                        _method.DeclaringType,
                        includeInactive
                            ? FindObjectsInactive.Include
                            : FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None
                    );
                    break;
                case InstanceTargetType.FirstActive:
                case InstanceTargetType.FirstIncludingInactive:
                    var target = GameObject.FindFirstObjectByType(
                        _method.DeclaringType,
                        includeInactive
                            ? FindObjectsInactive.Include
                            : FindObjectsInactive.Exclude
                    );
                    targets = new object[] { target };
                    break;
                case InstanceTargetType.Registry:
                    targets = DebugConsole.InstanceTargets.Get(_method.DeclaringType);
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            return targets;
        }
    }
}