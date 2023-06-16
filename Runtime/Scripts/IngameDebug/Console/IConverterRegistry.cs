using System;

namespace ANU.IngameDebug.Console
{
    public interface IConverter <T>
    {
        T ConvertFromString(string option);
    }

    public interface IConverterRegistry
    {
        void Register<T>(Func<string, T> converter);
        void Register<T>(IConverter<T> converter);
    }
}