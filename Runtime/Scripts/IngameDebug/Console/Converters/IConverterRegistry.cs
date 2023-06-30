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

        object ConvertFromString(string option, Type targetType);
    }

    public interface IConverter<T> : IConverter
    {
        Type IConverter.TargetType => typeof(T);
        object IConverter.ConvertFromString(string option, Type targetType) => ConvertFromString(option);

        T ConvertFromString(string option);
    }

    public interface IReadOnlyConverterRegistry
    {
        T Convert<T>(string option);
        object Convert(Type type, string option);
    }

    public interface IConverterRegistry : IReadOnlyConverterRegistry
    {
        void Register<T>(Func<string, T> converter);
        void Register<T>(IConverter<T> converter);
        void Register(IConverter converter);
    }
}