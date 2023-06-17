using System;
using System.Collections.Generic;
using System.Linq;

namespace ANU.IngameDebug.Console.Converters
{
    internal class ConverterRegistry : IConverterRegistry
    {
        private readonly Dictionary<Type, IConverter> _converters = new();

        public void Register<T>(Func<string, T> converter)
            => Register<T>(new LambdaConverter<T>(converter));

        public void Register<T>(IConverter<T> converter)
            => _converters[typeof(T)] = converter;

        public void Register(IConverter converter)
            => _converters[converter.TargetType] = converter;

        public bool TryConvert<T>(string option, out object value)
            => TryConvert(typeof(T), option, out value);

        public bool TryConvert(Type type, string option, out object value)
        {
            if (_converters.TryGetValue(type, out var converter) && converter.CanConvert(type))
            {
                value = converter.ConvertFromString(option, type);
                return true;
            }

            converter = _converters.Values.FirstOrDefault(w => w.CanConvert(type));
            if (converter != null)
            {
                value = converter.ConvertFromString(option, type);
                return true;
            }

            value = null;
            return false;
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