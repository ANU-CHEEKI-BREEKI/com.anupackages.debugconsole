using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ANU.IngameDebug.Console.CommandLinePreprocessors;
using UnityEngine;

namespace ANU.IngameDebug.Console.Converters
{
    public class BaseConverter : IConverter
    {
        int IConverter.Priority => int.MaxValue;
        public Type TargetType => typeof(object);

        public object ConvertFromString(string option, Type targetType)
            => TypeDescriptor.GetConverter(targetType).ConvertFromString(option);

        bool IConverter.CanConvert(System.Type type) => !type.IsArray;
    }

    public abstract class VectorConverterBase : IInjectDebugConsoleContext
    {
        public IReadOnlyConverterRegistry Converters => Context?.Converters;
        public IReadOnlyDebugConsoleProcessor Context { get; set; }

        /// <summary>
        /// pass count as null to take all available components
        /// pass value to consider arrray aa fixed length array.
        /// </summary>
        protected IEnumerable<T> GetComponents<T>(string option, int? count)
            => GetComponents(typeof(T), option, count).Select(t => (T)t);

        /// <summary>
        /// pass count as null to take all available components
        /// pass value to consider arrray aa fixed length array.
        /// </summary>
        protected IEnumerable<object> GetComponents(Type type, string option, int? count)
        {
            // no need to force user to type brackets 
            // we can pare it like C# params T[]
            // kind of
            // so when user enter only one value N - it equivalent to [N]
            //
            // if ((!option.StartsWith('[') && !option.StartsWith('('))
            //     || (!option.EndsWith(']') && !option.EndsWith(')')))
            //     throw new System.Exception("Use [ ] or ( ) to wrap vector components. For Example [1, 2] or (3 4) or [] for zero or [n] for all components set to n."
            //         + "You can use ',' or just whitespace as components delimiters");

            if (option.StartsWith("[") && option.EndsWith("]"))
                option = option.Trim('[', ']');
            else if (option.StartsWith("(") && option.EndsWith(")"))
                option = option.Trim('(', ')');

            var components = option
                //TODO: prevent splitting array of string values
                .Split(new char[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(c => Converters.ConvertFromString(type, c));

            if (count == null)
                return components;

            // if count provided - parse array as fixed size array
            // so we can add some syntax sugar to fill all items wit same values

            if (!components.Any())
            {
                var singleItem = GetDefault(type);
                return Enumerable.Range(0, count.Value).Select(u => singleItem);
            }

            if (!components.Skip(1).Any())
            {
                var singleItem = components.First();
                return Enumerable.Range(0, count.Value).Select(u => singleItem);
            }

            return components;
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }
    }

    public class Vector2IntConverter : VectorConverterBase, IConverter<Vector2Int>
    {
        public Vector2Int ConvertFromString(string option)
        {
            var components = GetComponents<int>(option, 2);
            var vector = Vector2Int.zero;
            for (int i = 0; i < 2; i++)
                vector[i] = components.ElementAt(i);
            return vector;
        }
    }

    public class Vector2Converter : VectorConverterBase, IConverter<Vector2>
    {
        public Vector2 ConvertFromString(string option)
        {
            var components = GetComponents<float>(option, 2);
            var vector = Vector2.zero;
            for (int i = 0; i < 2; i++)
                vector[i] = components.ElementAt(i);
            return vector;
        }
    }

    public class Vector3IntConverter : VectorConverterBase, IConverter<Vector3Int>
    {
        public Vector3Int ConvertFromString(string option)
        {
            var components = GetComponents<int>(option, 3);
            var vector = Vector3Int.zero;
            for (int i = 0; i < 3; i++)
                vector[i] = components.ElementAt(i);
            return vector;
        }
    }

    public class Vector3Converter : VectorConverterBase, IConverter<Vector3>
    {
        public Vector3 ConvertFromString(string option)
        {
            var components = GetComponents<float>(option, 3);
            var vector = Vector3.zero;
            for (int i = 0; i < 3; i++)
                vector[i] = components.ElementAt(i);
            return vector;
        }
    }

    public class Vector4Converter : VectorConverterBase, IConverter<Vector4>
    {
        public Vector4 ConvertFromString(string option)
        {
            var components = GetComponents<float>(option, 4);
            var vector = Vector4.zero;
            for (int i = 0; i < 4; i++)
                vector[i] = components.ElementAt(i);
            return vector;
        }
    }

    public class QuaternionConverter : IConverter<Quaternion>, IInjectDebugConsoleContext
    {
        public IReadOnlyConverterRegistry Converters => Context?.Converters;
        public IReadOnlyDebugConsoleProcessor Context { get; set; }

        public Quaternion ConvertFromString(string option)
        {
            var euler = Converters.ConvertFromString<Vector3>(option);
            return Quaternion.Euler(euler);
        }
    }

    public class ColorConverter : IConverter<Color>, IInjectDebugConsoleContext
    {
        public IReadOnlyConverterRegistry Converters => Context?.Converters;
        public IReadOnlyDebugConsoleProcessor Context { get; set; }

        private static readonly Dictionary<string, Color> byName = typeof(Color)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(Color))
            .ToDictionary(p => p.Name, p => (Color)p.GetValue(null));

        public Color ConvertFromString(string option)
        {
            try
            {
                var v4 = Converters.ConvertFromString<Vector4>(option);
                return new Color(v4.x, v4.y, v4.z, v4.w);
            }
            catch { }

            try
            {
                var v3 = Converters.ConvertFromString<Vector3>(option);
                return new Color(v3.x, v3.y, v3.z);
            }
            catch { }

            if (byName.TryGetValue(option.ToLower(), out var color))
                return color;

            if (ColorUtility.TryParseHtmlString(option, out color))
                return color;

            throw new System.Exception($"Cant convert Color from input: {option}");
        }
    }

    public class Color32Converter : IConverter<Color32>, IInjectDebugConsoleContext
    {
        public IReadOnlyConverterRegistry Converters => Context?.Converters;
        public IReadOnlyDebugConsoleProcessor Context { get; set; }

        public Color32 ConvertFromString(string option)
        {
            var color = Converters.ConvertFromString<Color>(option);
            return new Color32((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(color.a * 255));
        }
    }

    public class GameObjectConverter : IConverter<GameObject>
    {
        public GameObject ConvertFromString(string option)
            => option.ToLower() == "null"
                ? null
                : GameObject.Find(option);
    }

    public class ComponentConverter : IConverter
    {
        public Type TargetType => typeof(UnityEngine.Component);

        object IConverter.ConvertFromString(string option, System.Type targetType)
        {
            if (option.ToLower() == "null")
                return null;

#if UNITY_2023_0_OR_NEWER
            return  GameObject.FindObjectsByType(targetType, FindObjectsSortMode.None).FirstOrDefault(t => t.name == option);
#else
            return GameObject.FindObjectsOfType(targetType).FirstOrDefault(t => t.name == option);
#endif
        }
    }

    public class BoolConverter : IConverter<bool>
    {
        public bool ConvertFromString(string option)
        {
            switch (option.ToLower())
            {
                case "0":
                case "-":
                case "false":
                case "no":
                case "n":
                case "discard":
                case "cancel":
                case "off":
                    return false;
                case "1":
                case "+":
                case "true":
                case "yes":
                case "y":
                case "approve":
                case "apply":
                case "on":
                    return true;
                default:
                    throw new Exception($"Not a valid input for Boolean: {option}");
            }
        }
    }

    public class ArrayConverter : VectorConverterBase, IConverter
    {
        Type IConverter.TargetType => typeof(Array);
        object IConverter.ConvertFromString(string option, System.Type targetType)
        {
            var components = GetComponents(targetType.GetElementType(), option, null).ToArray();
            var array = Array.CreateInstance(targetType.GetElementType(), components.Length);
            Array.Copy(components, array, array.Length);
            return array;
        }
    }

    public class ListConverter : IConverter, IInjectDebugConsoleContext
    {
        public IReadOnlyConverterRegistry Converters => Context?.Converters;
        public IReadOnlyDebugConsoleProcessor Context { get; set; }

        Type IConverter.TargetType => typeof(List<>);

        bool IConverter.CanConvert(System.Type type)
            => type.IsGenericType
            && (this as IConverter).TargetType.IsAssignableFrom(type.GetGenericTypeDefinition());

        object IConverter.ConvertFromString(string option, System.Type targetType)
        {
            var array = Converters
                .ConvertFromString(
                    targetType.GenericTypeArguments[0].MakeArrayType(),
                    option
                ) as Array;

            var list = Activator.CreateInstance(targetType) as IList;
            for (int i = 0; i < array.Length; i++)
            {
                var item = array.GetValue(i);
                list.Add(item);
            }

            return list;
        }
    }
}