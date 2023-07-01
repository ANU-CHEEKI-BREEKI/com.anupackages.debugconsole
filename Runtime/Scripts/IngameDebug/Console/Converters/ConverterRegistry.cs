using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANU.IngameDebug.Console;
using ANU.IngameDebug.Console.Converters;
using ANU.IngameDebug.Utils;
using static ANU.IngameDebug.Utils.Extensions;
using static ANU.IngameDebug.Utils.Extensions.ColumnAlignment;

[assembly: RegisterDebugCommandTypes(typeof(ConverterRegistry))]

namespace ANU.IngameDebug.Console.Converters
{
    internal class ConverterRegistry : IConverterRegistry
    {
        private readonly Dictionary<Type, IConverter> _converters = new();

        public ConverterRegistry(DebugConsoleProcessor context)
        {
            Context = context;
            Context.InstanceTargets.Register(this);
        }

        public DebugConsoleProcessor Context { get; }

        public void Register<T>(Func<string, T> converter)
            => Register<T>(new LambdaConverter<T>(converter));

        public void Register<T>(IConverter<T> converter)
        {
            if (converter is IInjectDebugConsoleContext consoleContext)
                consoleContext.Context = Context;

            _converters[typeof(T)] = converter;
        }

        public void Register(IConverter converter)
        {
            if (converter is IInjectDebugConsoleContext consoleContext)
                consoleContext.Context = Context;

            _converters[converter.TargetType] = converter;
        }

        public T ConvertFromString<T>(string option)
            => (T)ConvertFromString(typeof(T), option);

        public object ConvertFromString(Type type, string option)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (option is null)
                throw new ArgumentNullException(nameof(option));

            if (_converters.TryGetValue(type, out var converter) && converter.CanConvert(type))
                return converter.ConvertFromString(option, type);

            converter = _converters.Values.OrderBy(v => v.Priority).FirstOrDefault(w => w.CanConvert(type));
            if (converter != null)
                return converter.ConvertFromString(option, type);

            throw new NotSupportedException($"There are no registered converter which can convert {type} from strgin");
        }

        public string ConvertToString<T>(T value)
            => ConvertToString(typeof(T), value);

        public string ConvertToString(Type type, object value)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (_converters.TryGetValue(type, out var converter) && converter.CanConvert(type))
                return converter.ConvertToString(value, type);

            converter = _converters.Values.OrderBy(v => v.Priority).FirstOrDefault(w => w.CanConvert(type));
            if (converter != null)
                return converter.ConvertToString(value, type);

            throw new NotSupportedException($"There are no registered converter which can convert {type} to string");
        }

        private class LambdaConverter<T> : IConverter<T>
        {
            private readonly Func<string, T> _lambda;

            public LambdaConverter(Func<string, T> lambda)
            {
                if (lambda is null)
                    throw new ArgumentNullException(nameof(lambda));

                _lambda = lambda;
            }

            public T ConvertFromString(string option) => _lambda.Invoke(option);
        }

        [DebugCommand]
        public void ListRegisteredConverters()
        {
            if (!_converters.Any())
            {
                Context.Logger.LogReturnValue("There are no any registered converters");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine();

            sb.PrintTable(
                _converters,
                new string[] { "Type", "Converter Type" },
                item => new string[] { item.Key.Name, item.Value.GetType().Name },
                new ColumnAlignment[] { Right, Left }
            );

            Context.Logger.LogReturnValue(sb);
        }
    }
}