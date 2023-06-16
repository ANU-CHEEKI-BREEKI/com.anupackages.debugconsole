using System;
using System.Collections.Generic;

namespace ANU.IngameDebug.Console
{
    internal class ConverterRegistry : IConverterRegistry
    {
        private readonly Dictionary<Type, Func<string, object>> _converters = new();

        public void Register<T>(Func<string, T> converter)
            => _converters[typeof(T)] = option => converter == null ? null : converter.Invoke(option);

        public void Register<T>(IConverter<T> converter)
            => Register(converter.ConvertFromString);

        public bool TryConvert<T>(string option, out object value)
            => TryConvert(typeof(T), option, out value);

        public bool TryConvert(Type type, string option, out object value)
        {
            if (_converters.TryGetValue(type, out var converter))
            {
                value = converter.Invoke(option);
                return true;
            }

            value = null;
            return false;
        }

    }
}