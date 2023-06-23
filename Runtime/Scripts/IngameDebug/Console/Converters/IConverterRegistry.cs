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

    public static class IConverterExtensions
    {
        private static readonly Dictionary<IConverter, IReadOnlyConverterRegistry> _converterRegistry = new();
        private static readonly Dictionary<IConverter, ILogger> _loggers = new();

        public static void SetRegistry(this IConverter converter, IReadOnlyConverterRegistry registry)
            => _converterRegistry[converter] = registry;

        public static IReadOnlyConverterRegistry GetRegistry(this IConverter converter)
            => _converterRegistry.TryGetValue(converter, out var registry) ? registry : null;

        public static void SetLogger(this IConverter converter, ILogger registry)
            => _loggers[converter] = registry;

        public static ILogger GetLogger(this IConverter converter)
            => _loggers.TryGetValue(converter, out var registry) ? registry : null;
    }
}