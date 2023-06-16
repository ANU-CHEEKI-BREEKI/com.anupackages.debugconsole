using System;

namespace ANU.IngameDebug.Console
{
    public interface IConverter
    {
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

    public interface IConverterRegistry
    {
        void Register<T>(Func<string, T> converter);
        void Register<T>(IConverter<T> converter);
        void Register(IConverter converter);
    }
}