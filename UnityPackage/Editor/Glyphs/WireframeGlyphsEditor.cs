using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// The glyph list's Inspector: its packs, highest priority first, and which glyphs each pack overrides of the packs
    /// below it, so an override is never silent.
    /// </summary>
    [CustomEditor(typeof(WireframeGlyphs))]
    internal sealed class WireframeGlyphsEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            WireframeGlyphs glyphs = (WireframeGlyphs)target;
            VisualElement root = new();
            SerializedProperty packs = serializedObject.FindProperty(WireframeGlyphs.PacksField);
            root.Add(new PropertyField(packs, "Packs (highest priority first)"));
            HelpBox overrides = new(string.Empty, HelpBoxMessageType.Info);
            root.Add(overrides);
            void ShowOverrides(SerializedProperty _)
            {
                List<string> sentences = GlyphOverrides.Describe(glyphs);
                overrides.text = string.Join("\n", sentences);
                overrides.style.display = sentences.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }
            ShowOverrides(packs);
            root.TrackPropertyValue(packs, ShowOverrides);
            return root;
        }
    }
}
