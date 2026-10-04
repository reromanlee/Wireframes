using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// Lays texts and symbols out again on the editor's update after a glyph pack or glyph list changed, by an edit, undo
    /// or reimport, so every scene shows the change at once, in Edit Mode and Play Mode alike. It does no work while
    /// nothing changes.
    /// </summary>
    [InitializeOnLoad]
    internal static class GlyphRefresh
    {
        static GlyphRefresh()
        {
            EditorApplication.update += Update;
        }

        internal static void Update()
        {
            if (!GlyphEdits.TakePendingChanges())
            {
                return;
            }
            // Play Mode renders every frame anyway; Edit Mode needs the views repainted to show the change.
            if (GlyphShapes.RebuildStale() > 0 && !Application.isPlaying)
            {
                InternalEditorUtility.RepaintAllViews();
            }
        }
    }
}
