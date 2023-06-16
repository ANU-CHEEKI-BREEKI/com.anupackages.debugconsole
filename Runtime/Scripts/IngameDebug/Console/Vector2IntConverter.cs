using System.Linq;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class Vector2IntConverter : IConverter<Vector2Int>
    {
        public Vector2Int ConvertFromString(string option)
        {
            // Debug.Log($"Vector2IntConverter: {option}");
            var components = option.Trim('[', ']').Split(new char[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries).Take(2);
            var vector = Vector2Int.zero;
            vector.x = int.Parse(components.First());
            vector.y = int.Parse(components.Skip(1).First());
            return vector;
        }
    }
}