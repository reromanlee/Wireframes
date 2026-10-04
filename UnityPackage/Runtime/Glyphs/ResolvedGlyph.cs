namespace reromanlee.Wireframes
{
    /// <summary>A glyph found in a <see cref="WireframeGlyphs"/>: its lines, their metrics and the pack they come from.</summary>
    internal readonly struct ResolvedGlyph
    {
        internal readonly GlyphStroke[] Strokes;
        internal readonly GlyphMetrics Metrics;
        internal readonly WireframeGlyphPack Pack;

        internal ResolvedGlyph(GlyphStroke[] strokes, GlyphMetrics metrics, WireframeGlyphPack pack)
        {
            Strokes = strokes;
            Metrics = metrics;
            Pack = pack;
        }

        /// <summary>
        /// Width proportional text gives the glyph: the width of its ink, or its pack's space width when it has none.
        /// </summary>
        internal float ProportionalWidth
        {
            get => Metrics.HasInk ? Metrics.InkWidth : Pack.SpaceWidth;
        }
    }
}
