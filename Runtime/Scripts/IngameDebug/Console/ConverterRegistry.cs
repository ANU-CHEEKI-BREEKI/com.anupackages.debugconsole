using System;
using System.Collections.Generic;
using System.Linq;

namespace ANU.IngameDebug.Console.Converters
{
    internal class ConverterRegistry : IConverterRegistry
    {
        private readonly Dictionary<Type, IConverter> _converters = new();

        public ConverterRegistry(ILogger logger)
            => Logger = logger;

        public ILogger Logger { get; }

        public void Register<T>(Func<string, T> converter)
            => Register<T>(new LambdaConverter<T>(converter));

        public void Register<T>(IConverter<T> converter)
            => _converters[typeof(T)] = converter;

        public void Register(IConverter converter)
            => _converters[converter.TargetType] = converter;

        public T Convert<T>(string option)
            => (T)Convert(typeof(T), option);

        public object Convert(Type type, string option)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (option is null)
                throw new ArgumentNullException(nameof(option));

            if (_converters.TryGetValue(type, out var converter) && converter.CanConvert(type))
            {
                converter.SetRegistry(this);
                converter.SetLogger(Logger);
                return converter.ConvertFromString(option, type);
            }

            converter = _converters.Values.OrderBy(v => v.Priority).FirstOrDefault(w => w.CanConvert(type));
            if (converter != null)
            {
                converter.SetLogger(Logger);
                converter.SetRegistry(this);
                return converter.ConvertFromString(option, type);
            }

            throw new NotSupportedException($"There are no registered converter which can convert {type}");
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
    }
}