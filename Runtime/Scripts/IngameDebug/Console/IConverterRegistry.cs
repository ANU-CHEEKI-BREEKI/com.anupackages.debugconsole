using System;

namespace ANU.IngameDebug.Console.Converters
{
    public struct ConverterExtraArgs
    {
        public ILogger Logger { get; set; }
    }

    public interface IConverter
    {
        Type TargetType { get; }
        bool CanConvert<TFrom>() => CanConvert(typeof(TFrom));
        bool CanConvert(Type type) => TargetType.IsAssignableFrom(type);

        object ConvertFromString(string option, Type targetType, ConverterExtraArgs args);
    }

    public interface IConverter<T> : IConverter
    {
        Type IConverter.TargetType => typeof(T);
        object IConverter.ConvertFromString(string option, Type targetType, ConverterExtraArgs args) => ConvertFromString(option, args);

        T ConvertFromString(string option, ConverterExtraArgs args);
    }

    public interface IConverterRegistry
    {
        void Register<T>(Func<string, ConverterExtraArgs, T> converter);
        void Register<T>(IConverter<T> converter);
        void Register(IConverter converter);

        bool TryConvert<T>(string option, out object value);
        bool TryConvert(Type type, string option, out object value);
    }
}