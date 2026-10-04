using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// The glyph pack's Inspector: how many glyphs it has, a button that opens it in the Glyph Editor, where its glyphs
    /// are edited, and its settings.
    /// </summary>
    [CustomEditor(typeof(WireframeGlyphPack))]
    internal sealed class WireframeGlyphPackEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            WireframeGlyphPack pack = (WireframeGlyphPack)target;
            VisualElement root = new();
            Label summary = new();
            summary.style.marginBottom = 4f;
            root.Add(summary);
            void ShowSummary(SerializedObject _)
            {
                summary.text = $"{Count(pack.Characters.Length, "character")} and {Count(pack.Symbols.Length, "symbol")}";
            }
            ShowSummary(serializedObject);
            root.TrackSerializedObjectValue(serializedObject, ShowSummary);

            Button open = new(() => GlyphEditorWindow.Open(pack)) { text = "Open in Glyph Editor" };
            open.style.height = 24f;
            open.style.marginBottom = 6f;
            root.Add(open);

            root.Add(new PropertyField(serializedObject.FindProperty(WireframeGlyphPack.SpaceWidthField)));
            root.Add(new PropertyField(serializedObject.FindProperty(WireframeGlyphPack.XHeightField)));
            root.Add(new PropertyField(serializedObject.FindProperty(WireframeGlyphPack.CapHeightField)));
            root.Add(new PropertyField(serializedObject.FindProperty(WireframeGlyphPack.GridDivisionsField)));
            root.Add(new PropertyField(serializedObject.FindProperty(WireframeGlyphPack.EnumSettingsField), "Enum"));
            return root;
        }

        private static string Count(int count, string noun)
        {
            return count == 1 ? $"1 {noun}" : $"{count} {noun}s";
        }
    }
}
