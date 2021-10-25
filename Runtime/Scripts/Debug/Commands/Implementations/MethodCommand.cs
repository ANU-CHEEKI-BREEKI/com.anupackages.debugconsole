using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using NDesk.Options;

namespace IngameDebug.Commands.Implementations
{
    public class MethodCommand : ADebugCommand
    {
        private readonly MethodInfo _method;
        private readonly object[] _parameterValues;
        private readonly bool[] _isParameterValid;
        private ParameterInfo[] _parameters;

        public MethodCommand(string name, string description, MethodInfo method) : base(name, description)
        {
            _method = method;

            _parameters = method.GetParameters();
            _parameterValues = new object[_parameters.Length];
            _isParameterValid = new bool[_parameters.Length];

            ResetParametersValues();
        }

        private void ResetParametersValues()
        {
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

            _method.Invoke(null, _parameterValues);
            ResetParametersValues();
        }

        protected override OptionSet CreateOptions()
        {
            var options = new OptionSet();

            for (int i = 0; i < _parameters.Length; i++)
            {
                var parameterIndex = i;
                var parameter = _parameters[parameterIndex];

                var isOptional = parameter.HasDefaultValue;
                var isFlag = parameter.ParameterType == typeof(bool);

                var optionName = parameter.Name;
                if (!isFlag)
                    optionName += isOptional ? ":" : "=";

                var optionDescription = parameter.GetCustomAttribute<OptionDescriptionAttribute>()?.Description;

                options.Add(
                    optionName,
                    optionDescription,
                    value =>
                    {
                        if (isOptional && value == null)
                        {
                            // do nothing
                        }
                        else
                        {
                            if (isFlag)
                            {
                                _parameterValues[parameterIndex] = value != null;
                            }
                            else
                            {
                                _parameterValues[parameterIndex] = TypeDescriptor
                                    .GetConverter(parameter.ParameterType)
                                    .ConvertFromString(value);
                            }
                            _isParameterValid[parameterIndex] = true;
                        }
                    }
                );
            }

            if (!options.Any())
                options.Add("<>", v => { });

            return options;
        }
    }
}