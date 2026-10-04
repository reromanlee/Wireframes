using System;
using UnityEditor;
using UnityEngine;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// Copies glyph strokes as text on the system clipboard, so they can be pasted into another glyph, another pack or
    /// another Unity session.
    /// </summary>
    internal static class GlyphClipboard
    {
        private const string Prefix = "Wireframes glyph strokes:";

        internal static void Copy(GlyphStroke[] strokes)
        {
            EditorGUIUtility.systemCopyBuffer = Prefix + JsonUtility.ToJson(new Strokes { Items = strokes });
        }

        /// <summary>Reads strokes copied with <see cref="Copy"/>; false when the clipboard holds anything else.</summary>
        internal static bool TryPaste(out GlyphStroke[] strokes)
        {
            strokes = null;
            string text = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(text) || !text.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return false;
            }
            try
            {
                strokes = JsonUtility.FromJson<Strokes>(text.Substring(Prefix.Length))?.Items;
            }
            catch (ArgumentException)
            {
                return false;
            }
            return strokes != null && strokes.Length > 0;
        }

        [Serializable]
        private sealed class Strokes
        {
            public GlyphStroke[] Items;
        }
    }
}
