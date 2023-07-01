using System;
using System.Collections.Generic;

namespace ANU.IngameDebug.Console.Converters
{
    public interface IConverter
    {
        int Priority => 0;
        Type TargetType { get; }
        bool CanConvert<TFrom>() => CanConvert(typeof(TFrom));
        bool CanConvert(Type type) => TargetType.IsAssignableFrom(type);

        string ConvertToString(object obj, Type targetType) => obj?.ToString();
        object ConvertFromString(string option, Type targetType);
    }

    public interface IConverter<T> : IConverter
    {
        Type IConverter.TargetType => typeof(T);
        object IConverter.ConvertFromString(string option, Type targetType) => ConvertFromString(option);

        string ConvertToString(T value) => ConvertToString(value as object, typeof(T));
        T ConvertFromString(string option);
    }

    public interface IReadOnlyConverterRegistry
    {
        string ConvertToString<T>(T value);
        string ConvertToString(Type type, object value);

        T ConvertFromString<T>(string option);
        object ConvertFromString(Type type, string option);
    }

    public interface IConverterRegistry : IReadOnlyConverterRegistry
    {
        void Register<T>(Func<string, T> converter);
        void Register<T>(IConverter<T> converter);
        void Register(IConverter converter);
    }
}