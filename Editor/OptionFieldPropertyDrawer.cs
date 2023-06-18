using UnityEngine;
using UnityEditor;
using ANU.IngameDebug.Utils;


namespace ANU.Editor.IngameDebug.Utils
{
    [CustomPropertyDrawer(typeof(OptionalField<>), true)]
    public class OptionalFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var isSingleLine = IsSingleRowProperty(property);

            GUI.Box(position, "");

            var isEnabled = property.FindPropertyRelative("_isEnabled");
            property.isExpanded = isEnabled.boolValue;

            var toggleSpace = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var headerPosition = position;
            var pos = headerPosition.position;
            pos.x += toggleSpace;
            headerPosition.position = pos;
            var size = headerPosition.size;

            if (isSingleLine)
            {
                size.x = GUI.skin.label.CalcSize(label).x;
                size.x += toggleSpace * 2;
            }

            size.x -= toggleSpace;
            size.y = EditorGUIUtility.singleLineHeight;

            headerPosition.size = size;

            var togglePosition = position;
            togglePosition.size = Vector2.one * EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(togglePosition, isEnabled, new GUIContent(""), false);

            EditorGUI.LabelField(headerPosition, label);

            if (isEnabled.boolValue)
            {
                var value = property.FindPropertyRelative("_value");
                var valuePosition = headerPosition;
                pos = valuePosition.position;
                pos.y += valuePosition.size.y;
                size = valuePosition.size;
                size.y = EditorGUI.GetPropertyHeight(value, new GUIContent(value.displayName), value.isExpanded);
                valuePosition.size = size;
                valuePosition.position = pos;

                if (isSingleLine)
                {
                    valuePosition = headerPosition;
                    var s = headerPosition.size;
                    s.x = Mathf.Clamp(position.size.x - headerPosition.size.x, 100, 200);
                    valuePosition.size = s;

                    var p = valuePosition.position;
                    p.x = position.position.x + position.size.x - valuePosition.size.x;
                    valuePosition.position = p;
                }

                EditorGUI.PropertyField(
                    valuePosition,
                    value,
                    isSingleLine
                        ? GUIContent.none
                        : new GUIContent(value.displayName),
                    value.isExpanded
                );
            }

            property.isExpanded = isEnabled.boolValue;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var isSingleLine = IsSingleRowProperty(property);

            var isEnabled = property.FindPropertyRelative("_isEnabled").boolValue;
            property.isExpanded = isEnabled;

            if (isEnabled)
            {
                var value = property.FindPropertyRelative("_value");
                if (value.isArray)
                    value.isExpanded = true;
            }

            var height = isSingleLine
                ? EditorGUIUtility.singleLineHeight
                : EditorGUI.GetPropertyHeight(property, label, isEnabled);

            if (isEnabled && !isSingleLine)
                height -= EditorGUIUtility.singleLineHeight;

            return height;
        }

        private bool IsSingleRowProperty(SerializedProperty property)
        {
            property = property.Copy();
            property.isExpanded = true;
            var count = property.CountInProperty();
            return count <= 3;
        }
    }
}