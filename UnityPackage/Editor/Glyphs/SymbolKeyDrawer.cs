using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// Shows a symbol key as a choice between the symbols of the component's glyphs, by keyword, while saving only the
    /// key. A key the glyphs lack shows as missing until another symbol is picked.
    /// </summary>
    [CustomPropertyDrawer(typeof(SymbolKeyAttribute))]
    internal sealed class SymbolKeyDrawer : PropertyDrawer
    {
        private const string GlyphsField = "_glyphs";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty glyphsProperty = property.serializedObject.FindProperty(GlyphsField);
            Dictionary<int, string> names = new();
            List<int> keys = KeysOf(glyphsProperty, property.intValue, names);
            PopupField<int> field = new(property.displayName, keys, property.intValue, key => NameOf(key, names),
                key => NameOf(key, names)) { tooltip = property.tooltip };
            field.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            field.BindProperty(property);
            if (glyphsProperty != null)
            {
                field.TrackPropertyValue(glyphsProperty, _ => field.choices = KeysOf(glyphsProperty, property.intValue, names));
            }
            return field;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Dictionary<int, string> names = new();
            List<int> keys = KeysOf(property.serializedObject.FindProperty(GlyphsField), property.intValue, names);
            GUIContent[] labels = new GUIContent[keys.Count];
            for (int i = 0; i < keys.Count; i++)
            {
                labels[i] = new GUIContent(NameOf(keys[i], names));
            }
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            int key = EditorGUI.IntPopup(position, label, property.intValue, labels, keys.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = key;
            }
            EditorGUI.EndProperty();
        }

        /// <summary>None, then every symbol of the glyphs in pack order, then <paramref name="current"/> if they lack it.</summary>
        private static List<int> KeysOf(SerializedProperty glyphsProperty, int current, Dictionary<int, string> names)
        {
            names.Clear();
            List<int> keys = new() { SymbolKeys.None };
            WireframeGlyphs glyphs = glyphsProperty?.objectReferenceValue as WireframeGlyphs;
            if (glyphs == null)
            {
                glyphs = WireframeGlyphs.Default;
            }
            if (glyphs != null)
            {
                foreach (WireframeGlyphPack pack in glyphs.Packs)
                {
                    if (pack == null)
                    {
                        continue;
                    }
                    foreach (SymbolGlyph symbol in pack.Symbols)
                    {
                        int key = SymbolKeys.Of(symbol.Keyword);
                        if (SymbolKeys.FindProblem(symbol.Keyword) == null && names.TryAdd(key, symbol.Keyword))
                        {
                            keys.Add(key);
                        }
                    }
                }
            }
            if (!keys.Contains(current))
            {
                keys.Add(current);
            }
            return keys;
        }

        private static string NameOf(int key, Dictionary<int, string> names)
        {
            if (key == SymbolKeys.None)
            {
                return SymbolKeys.NoneName;
            }
            return names.TryGetValue(key, out string name) ? name : $"Missing symbol ({key})";
        }
    }
}
