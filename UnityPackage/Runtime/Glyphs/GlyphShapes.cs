using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// The glyph shapes that are in containers, so the Editor can lay them out again after a glyph pack or a glyph list
    /// changes, for components and shapes created from code alike. Only the Editor changes glyph data, so builds keep no
    /// list.
    /// </summary>
    internal static class GlyphShapes
    {
#if UNITY_EDITOR
        private static readonly List<GlyphShape> Live = new();
#endif

        [Conditional("UNITY_EDITOR")]
        internal static void Add(GlyphShape shape)
        {
#if UNITY_EDITOR
            shape.LiveIndex = Live.Count;
            Live.Add(shape);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        internal static void Remove(GlyphShape shape)
        {
#if UNITY_EDITOR
            int index = shape.LiveIndex;
            if (index < 0)
            {
                return;
            }
            int last = Live.Count - 1;
            GlyphShape moved = Live[last];
            Live[index] = moved;
            moved.LiveIndex = index;
            Live.RemoveAt(last);
            shape.LiveIndex = -1;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Lays out again every shape laid out before the last change to glyph data, and returns how many it did. A shape
        /// that fails is logged once and left as it was.
        /// </summary>
        internal static int RebuildStale()
        {
            int count = 0;
            // Backwards, so a shape that a failed rebuild disposes takes the place of one already visited.
            for (int i = Live.Count - 1; i >= 0; i--)
            {
                GlyphShape shape = Live[i];
                try
                {
                    if (shape.RebuildIfStale())
                    {
                        count++;
                    }
                }
                catch (Exception exception)
                {
                    if (!shape.HasReportedProblem)
                    {
                        shape.HasReportedProblem = true;
                        WireframesLog.Error(
                            $"A {shape.GetType().Name} failed to update after its glyphs changed.", exception,
                            shape.WarningContext);
                    }
                }
            }
            return count;
        }
#endif
    }
}
