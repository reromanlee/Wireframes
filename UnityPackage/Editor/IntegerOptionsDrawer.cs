using System;
using UnityEditor;
using UnityEngine;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>Draws an int field marked with <see cref="IntegerOptionsAttribute"/> as a popup of its options.</summary>
    [CustomPropertyDrawer(typeof(IntegerOptionsAttribute))]
    internal sealed class IntegerOptionsDrawer : PropertyDrawer
    {
        private GUIContent[] _labels;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }
            int[] options = ((IntegerOptionsAttribute)attribute).Options;
            _labels ??= Array.ConvertAll(options, option => new GUIContent(option.ToString()));

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            int value = EditorGUI.IntPopup(position, label, property.intValue, _labels, options);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = value;
            }
            EditorGUI.EndProperty();
        }
    }
}
