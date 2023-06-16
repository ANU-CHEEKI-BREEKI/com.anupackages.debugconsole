using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal abstract class VectorConverterBase
    {
        protected IEnumerable<T> GetComponents<T>(string option)
        {
            if ((!option.StartsWith('[') && !option.StartsWith('('))
                || (!option.EndsWith(']') && !option.EndsWith(')')))
                throw new System.Exception("Use [ ] or ( ) to wrap vector components. For Example [1, 2] or (3 4) or [] for zero or [n] for all components set to n."
                    + "You can use ',' or just whitespace as components delimiters");

            var components = option
                .Trim('[', ']')
                .Trim('(', ')')
                .Split(new char[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(c => (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(c));

            const int dummyCount = 100;

            if (!components.Any())
            {
                var singleItem = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString("0");
                return Enumerable.Range(0, dummyCount).Select(u => singleItem);
            }

            if (!components.Skip(1).Any())
            {
                var singleItem = components.First();
                return Enumerable.Range(0, dummyCount).Select(u => singleItem);
            }

            return components;
        }
    }

    internal class Vector2IntConverter : VectorConverterBase, IConverter<Vector2Int>
    {
        public Vector2Int ConvertFromString(string option)
        {
            var components = GetComponents<int>(option);
            var vector = Vector2Int.zero;
            for (int i = 0; i < 2; i++)
                vector[i] = components.ElementAt(i);
            return vector;
        }
    }

    internal class Vector2Converter : VectorConverterBase, IConverter<Vector2>
    {
        public Vector2 ConvertFromString(string option)
        {
            var components = GetComponents<float>(option);
            var vector = Vector2.zero;
            for (int i = 0; i < 2; i++)
                vector[i] = components.ElementAt(i);
            return vector;
        }
    }

    internal class Vector3IntConverter : VectorConverterBase, IConverter<Vector3Int>
    {
        public Vector3Int ConvertFromString(string option)
        {
            var components = GetComponents<int>(option);
            var vector = Vector3Int.zero;
            for (int i = 0; i < 3; i++)
                vector[i] = components.ElementAt(i);
            return vector;
        }
    }

    internal class Vector3Converter : VectorConverterBase, IConverter<Vector3>
    {
        public Vector3 ConvertFromString(string option)
        {
            var components = GetComponents<float>(option);
            var vector = Vector3.zero;
            for (int i = 0; i < 3; i++)
                vector[i] = components.ElementAt(i);
            return vector;
        }
    }

    internal class Vector4Converter : VectorConverterBase, IConverter<Vector4>
    {
        public Vector4 ConvertFromString(string option)
        {
            var components = GetComponents<float>(option);
            var vector = Vector4.zero;
            for (int i = 0; i < 4; i++)
                vector[i] = components.ElementAt(i);
            return vector;
        }
    }

    internal class QuaternionConverter : IConverter<Quaternion>
    {
        IConverter<Vector3> _vector3Converter = new Vector3Converter();

        public Quaternion ConvertFromString(string option)
        {
            var euler = _vector3Converter.ConvertFromString(option);
            return Quaternion.Euler(euler);
        }
    }

    internal class ColorConverter : IConverter<Color>
    {
        private static readonly IConverter<Vector3> _vector3Converter = new Vector3Converter();
        private static readonly IConverter<Vector4> _vector4Converter = new Vector4Converter();

        private static readonly Dictionary<string, Color> byName = typeof(Color)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(Color))
            .ToDictionary(p => p.Name, p => (Color)p.GetValue(null));

        public Color ConvertFromString(string option)
        {
            try
            {
                var v4 = _vector4Converter.ConvertFromString(option);
                return new Color(v4.x, v4.y, v4.z, v4.w);
            }
            finally { }

            try
            {
                var v3 = _vector3Converter.ConvertFromString(option);
                return new Color(v3.x, v3.y, v3.z);
            }
            finally { }

            if (ColorUtility.TryParseHtmlString(option, out var color))
                return color;

            if (byName.TryGetValue(option.ToLower(), out color))
                return color;

            throw new System.Exception($"Cant convert Color from input: {option}");
        }
    }

    internal class Color32Converter : IConverter<Color32>
    {
        private static readonly IConverter<Color> _colorConverter = new ColorConverter();

        public Color32 ConvertFromString(string option)
        {
            var color = _colorConverter.ConvertFromString(option);
            return new Color32((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(color.a * 255));
        }
    }

    internal class GameObjectConverter : IConverter<GameObject>
    {
        public GameObject ConvertFromString(string option)
            => GameObject.Find(option);
    }

    internal class ComponentConverter : IConverter
    {
        public Type TargetType => typeof(UnityEngine.Component);

        object IConverter.ConvertFromString(string option, System.Type targetType)
            => GameObject.FindObjectsByType(targetType, FindObjectsSortMode.None).FirstOrDefault(t => t.name == option);
    }
}