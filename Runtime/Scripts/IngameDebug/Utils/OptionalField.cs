using System;
using UnityEngine;

namespace ANU.IngameDebug.Utils
{
    [System.Serializable]
    public struct OptionalField<T>
    {
        [SerializeField] private bool _isEnabled;
        [SerializeField] private T _value;

        public OptionalField(T value, bool isEnabled = true)
        {
            _isEnabled = isEnabled;
            _value = value;
        }

        public bool IsEnabled { get => _isEnabled; set => _isEnabled = value; }
        public T Value => IsEnabled ? _value : default(T);

        public static implicit operator OptionalField<T>(T value)
            => new OptionalField<T>(value, false);

        public static implicit operator OptionalField<T>((T value, bool isEnabled) pair)
            => new OptionalField<T>(pair.value, pair.isEnabled);

        public static implicit operator T(OptionalField<T> optField)
            => optField.Value;
    }

    public static class OptionalFieldExtensions
    {
        public static Nullable<T> AsNullable<T>(this OptionalField<T> field) where T : struct
            => !field.IsEnabled ? null : field.Value;
    }
}