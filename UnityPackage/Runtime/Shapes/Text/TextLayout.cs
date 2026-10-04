using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Lays text out into glyph geometry: finds each character's glyph, breaks the characters into lines, and places the
    /// lines in the text's bounds. Text lies in the XY plane, read from the -Z side, with +Y up. Its buffers are shared
    /// and only grow, so laying out text allocates nothing once they are big enough. Main thread only.
    /// </summary>
    internal static class TextLayout
    {
        private const int SpacesPerTab = 4;
        private const char Fallback = '?';
        private const char Space = ' ';

        private static Item[] _items = new Item[64];
        private static Line[] _lines = new Line[8];

        private enum ItemKind : byte
        {
            Glyph,
            Space,
            LineBreak
        }

        /// <summary>
        /// Lays out <paramref name="text"/> with <paramref name="glyphs"/> into <paramref name="geometry"/>. Characters
        /// the glyphs lack are drawn as '?', with one warning for each, shown with <paramref name="context"/>.
        /// </summary>
        internal static void Layout(
            ReadOnlySpan<char> text, WireframeGlyphs glyphs, in TextSettings settings, GlyphGeometry geometry,
            Object context)
        {
            int itemCount = FindGlyphs(text, glyphs, settings, context, out int vertexCount, out int edgeCount);
            int lineCount = BreakLines(itemCount, settings);
            geometry.Begin(vertexCount, edgeCount);
            PlaceLines(lineCount, settings, geometry);
            // The shared items would otherwise keep the glyphs' packs referenced.
            Array.Clear(_items, 0, itemCount);
        }

        /// <summary>Turns the characters into items that know their glyph and how far they move the next one along.</summary>
        private static int FindGlyphs(
            ReadOnlySpan<char> text, WireframeGlyphs glyphs, in TextSettings settings, Object context,
            out int vertexCount, out int edgeCount)
        {
            bool isMonospace = settings.CharacterWidth == WireframeCharacterWidth.Monospace;
            float spacing = settings.CharacterSpacing;
            if (glyphs == null)
            {
                GlyphWarnings.ReportMissingDefault(context);
            }
            float spaceAdvance = SpaceWidth(glyphs, isMonospace) + spacing;
            EnsureItemCapacity(text.Length);
            int count = 0;
            vertexCount = 0;
            edgeCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                if (character == '\n')
                {
                    _items[count++] = new Item(default, 0f, 0f, ItemKind.LineBreak);
                    continue;
                }
                if (character == '\t')
                {
                    _items[count++] = new Item(default, spaceAdvance * SpacesPerTab, 0f, ItemKind.Space);
                    continue;
                }
                // Carriage returns too, so Windows line endings break lines once.
                if (char.IsControl(character))
                {
                    continue;
                }
                int codePoint = character;
                if (char.IsHighSurrogate(character) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    codePoint = char.ConvertToUtf32(character, text[++i]);
                }
                ItemKind kind = codePoint == Space ? ItemKind.Space : ItemKind.Glyph;
                if (glyphs == null || !glyphs.TryResolveCharacter(codePoint, out ResolvedGlyph glyph))
                {
                    // A space draws nothing anyway, so one missing from the glyphs isn't worth a warning.
                    if (codePoint == Space)
                    {
                        _items[count++] = new Item(default, spaceAdvance, 0f, ItemKind.Space);
                        continue;
                    }
                    if (glyphs != null)
                    {
                        GlyphWarnings.ReportMissingCharacter(glyphs, codePoint, context);
                    }
                    if (glyphs == null || !glyphs.TryResolveCharacter(Fallback, out glyph))
                    {
                        _items[count++] = new Item(default, spaceAdvance, 0f, ItemKind.Glyph);
                        continue;
                    }
                }
                GlyphMetrics metrics = glyph.Metrics;
                float advance = (isMonospace ? 1f : glyph.ProportionalWidth) + spacing;
                // Proportional characters move their ink to the pen, so no glyph keeps the empty room of its box.
                float inkOffset = isMonospace || !metrics.HasInk ? 0f : -metrics.InkLeft;
                _items[count++] = new Item(glyph, advance, inkOffset, kind);
                vertexCount += metrics.VertexCount;
                edgeCount += metrics.EdgeCount;
            }
            return count;
        }

        /// <summary>
        /// Splits the items into lines at line breaks and, when the text wraps, at the last space that keeps a line within
        /// the bounds, or before the character that doesn't fit when the line has no space. The space a line breaks at is
        /// dropped, and a line always keeps at least one character.
        /// </summary>
        private static int BreakLines(int itemCount, in TextSettings settings)
        {
            float spacing = settings.CharacterSpacing;
            float size = Math.Abs(settings.CharacterSize);
            bool wraps = settings.Overflow == WireframeTextOverflow.Wrap && size > 0f;
            float limit = wraps ? Math.Abs(settings.Bounds.x) / size : float.PositiveInfinity;
            int lineCount = 0;
            int lineStart = 0;
            float width = 0f;
            int breakIndex = -1;
            float widthBeforeBreak = 0f;
            for (int i = 0; i < itemCount; i++)
            {
                Item item = _items[i];
                if (item.Kind == ItemKind.LineBreak)
                {
                    AddLine(ref lineCount, lineStart, i, width, spacing);
                    lineStart = i + 1;
                    width = 0f;
                    breakIndex = -1;
                    continue;
                }
                if (item.Kind == ItemKind.Space)
                {
                    // Spaces never start a new line themselves: one may hang past the bounds until a character needs it.
                    breakIndex = i;
                    widthBeforeBreak = width;
                    width += item.Advance;
                    continue;
                }
                // Without its spacing, which stays out of the bounds at the end of a line.
                while (wraps && i > lineStart && width + item.Advance - spacing > limit)
                {
                    if (breakIndex >= lineStart)
                    {
                        AddLine(ref lineCount, lineStart, breakIndex, widthBeforeBreak, spacing);
                        width -= widthBeforeBreak + _items[breakIndex].Advance;
                        lineStart = breakIndex + 1;
                        breakIndex = -1;
                    }
                    else
                    {
                        AddLine(ref lineCount, lineStart, i, width, spacing);
                        lineStart = i;
                        width = 0f;
                    }
                }
                width += item.Advance;
            }
            AddLine(ref lineCount, lineStart, itemCount, width, spacing);
            return lineCount;
        }

        /// <summary>Places each line by the alignments, from the top line down, and writes the glyphs of its items.</summary>
        private static void PlaceLines(int lineCount, in TextSettings settings, GlyphGeometry geometry)
        {
            float size = settings.CharacterSize;
            float lineStep = (1f + settings.LineSpacing) * size;
            float blockHeight = (lineCount + (lineCount - 1) * settings.LineSpacing) * size;
            float halfWidth = settings.Bounds.x * 0.5f;
            float halfHeight = settings.Bounds.y * 0.5f;
            float top = settings.VerticalAlignment switch
            {
                WireframeVerticalAlignment.Top => halfHeight,
                WireframeVerticalAlignment.Bottom => blockHeight - halfHeight,
                _ => blockHeight * 0.5f
            };
            for (int line = 0; line < lineCount; line++)
            {
                Line record = _lines[line];
                float width = record.Width * size;
                float left = settings.HorizontalAlignment switch
                {
                    WireframeHorizontalAlignment.Left => -halfWidth,
                    WireframeHorizontalAlignment.Right => halfWidth - width,
                    _ => -width * 0.5f
                };
                float bottom = top - line * lineStep - size;
                float pen = 0f;
                for (int i = record.Start; i < record.End; i++)
                {
                    Item item = _items[i];
                    if (item.Glyph.Strokes != null)
                    {
                        geometry.AddGlyph(item.Glyph.Strokes, new Vector2(left + (pen + item.InkOffset) * size, bottom), size);
                    }
                    pen += item.Advance;
                }
            }
        }

        /// <summary>Width a space takes: the glyphs' own space, or the default when they have none.</summary>
        private static float SpaceWidth(WireframeGlyphs glyphs, bool isMonospace)
        {
            if (isMonospace)
            {
                return 1f;
            }
            return glyphs != null && glyphs.TryResolveCharacter(Space, out ResolvedGlyph space)
                ? space.ProportionalWidth
                : WireframeGlyphPack.DefaultSpaceWidth;
        }

        private static void AddLine(ref int lineCount, int start, int end, float width, float spacing)
        {
            if (lineCount == _lines.Length)
            {
                Array.Resize(ref _lines, lineCount * 2);
            }
            // The last character's spacing is room after the line, not part of it.
            _lines[lineCount++] = new Line(start, end, end > start ? width - spacing : 0f);
        }

        private static void EnsureItemCapacity(int count)
        {
            if (count <= _items.Length)
            {
                return;
            }
            int capacity = _items.Length;
            while (capacity < count)
            {
                capacity *= 2;
            }
            _items = new Item[capacity];
        }

        private readonly struct Item
        {
            internal readonly ResolvedGlyph Glyph;
            internal readonly float Advance;
            internal readonly float InkOffset;
            internal readonly ItemKind Kind;

            internal Item(ResolvedGlyph glyph, float advance, float inkOffset, ItemKind kind)
            {
                Glyph = glyph;
                Advance = advance;
                InkOffset = inkOffset;
                Kind = kind;
            }
        }

        private readonly struct Line
        {
            internal readonly int Start;
            internal readonly int End;
            internal readonly float Width;

            internal Line(int start, int end, float width)
            {
                Start = start;
                End = end;
                Width = width;
            }
        }
    }
}
