using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of the shapes drawn from glyphs: texts and symbols. They lay their glyphs out into a
    /// <see cref="GlyphGeometry"/>, whose capacity is the vertex count they take in their chunk, so most edits rewrite
    /// them where they are, and only outgrowing that capacity moves them. They lie in the XY plane of their rotation, read
    /// from the -Z side.
    /// </summary>
    internal abstract class GlyphShape : RigidShape
    {
        private readonly GlyphGeometry _geometry;
        private WireframeGlyphs _glyphs;
        private int _layoutVersion;

        /// <param name="geometry">The shape's glyphs, already laid out by the derived constructor.</param>
        protected GlyphShape(
            GlyphGeometry geometry,
            WireframeGlyphs glyphs,
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation)
            : base(geometry.VertexCapacity, new EdgeSource(geometry.Pattern, geometry.EdgeCount), bone, localPosition,
                localRotation)
        {
            _geometry = geometry;
            _glyphs = glyphs;
            _layoutVersion = GlyphEdits.Version;
        }

        public WireframeGlyphs Glyphs
        {
            get
            {
                EnsureUsable();
                return _glyphs;
            }
            set
            {
                EnsureUsable();
                if (ReferenceEquals(value, _glyphs))
                {
                    return;
                }
                _glyphs = value;
                Rebuild(true);
            }
        }

        /// <summary>The object that warnings about missing glyphs point to, such as the component that draws the shape.</summary>
        internal Object WarningContext { get; set; }

        /// <summary>Position in the Editor's list of glyph shapes in containers, or -1 while not in it.</summary>
        internal int LiveIndex { get; set; } = -1;

        /// <summary>The glyphs the shape draws with: its own, or the package's default ones when it has none.</summary>
        protected WireframeGlyphs DrawnGlyphs
        {
            get => DrawnGlyphsOf(_glyphs);
        }

        /// <summary>
        /// Lays the glyphs out again if any glyph pack or glyph list changed since, as only happens in the Editor, and
        /// returns whether it did.
        /// </summary>
        internal bool RebuildIfStale()
        {
            if (_layoutVersion == GlyphEdits.Version)
            {
                return false;
            }
            Rebuild(true);
            return true;
        }

        /// <summary>
        /// True while the shape draws <paramref name="glyphs"/> as they are now: they are its glyphs, and no glyph data
        /// changed since its last layout.
        /// </summary>
        protected bool IsLaidOutWith(WireframeGlyphs glyphs)
        {
            return ReferenceEquals(glyphs, _glyphs) && _layoutVersion == GlyphEdits.Version;
        }

        /// <summary>
        /// Replaces the glyphs without laying them out, for derived shapes that set several fields at once before one
        /// <see cref="Rebuild"/>.
        /// </summary>
        protected void ReplaceGlyphs(WireframeGlyphs glyphs)
        {
            _glyphs = glyphs;
        }

        /// <summary>
        /// Lays the glyphs out again and queues what changed. Pass true for <paramref name="glyphsChanged"/> when the
        /// glyphs drawn may differ, such as after the text or the glyph list changed, so the edges are replaced; other
        /// changes, such as alignment, only move the points.
        /// </summary>
        protected void Rebuild(bool glyphsChanged)
        {
            _layoutVersion = GlyphEdits.Version;
            int[] pattern = _geometry.Pattern;
            Layout(_geometry, DrawnGlyphs);
            if (_geometry.VertexCapacity != VertexCount)
            {
                // Outgrown or mostly empty: the host moves the shape to a block of the new size.
                Resize(_geometry.VertexCapacity, new EdgeSource(_geometry.Pattern, _geometry.EdgeCount));
                return;
            }
            if (glyphsChanged || _geometry.EdgeCount != EdgeCount || !ReferenceEquals(pattern, _geometry.Pattern))
            {
                ReplaceEdges(_geometry.Pattern, _geometry.EdgeCount);
            }
            MarkDirty(DirtyFlags.Positions);
        }

        /// <summary>Writes the shape's glyphs into <paramref name="geometry"/>, in its own space.</summary>
        protected abstract void Layout(GlyphGeometry geometry, WireframeGlyphs glyphs);

        protected sealed override void OnAttached()
        {
            GlyphShapes.Add(this);
        }

        protected sealed override void OnDetached()
        {
            GlyphShapes.Remove(this);
        }

        protected sealed override void WriteShape(Span<Vector3> positions)
        {
            int count = _geometry.VertexCount;
            _geometry.Points.AsSpan(0, count).CopyTo(positions);
            // Points past the drawn ones belong to no edge, and zero keeps them finite.
            positions.Slice(count).Clear();
        }

        /// <summary>The glyphs a shape given <paramref name="glyphs"/> draws with: those, or the package's default ones.</summary>
        protected static WireframeGlyphs DrawnGlyphsOf(WireframeGlyphs glyphs)
        {
            return glyphs != null ? glyphs : WireframeGlyphs.Default;
        }
    }
}
