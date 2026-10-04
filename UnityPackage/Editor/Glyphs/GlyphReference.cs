using System;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>A glyph of a pack: a character or a symbol, by its position in the pack's list of either.</summary>
    internal readonly struct GlyphReference : IEquatable<GlyphReference>
    {
        internal static readonly GlyphReference None = new(false, -1);

        internal readonly bool IsSymbol;
        internal readonly int Index;

        internal GlyphReference(bool isSymbol, int index)
        {
            IsSymbol = isSymbol;
            Index = index;
        }

        internal bool IsNone
        {
            get => Index < 0;
        }

        public bool Equals(GlyphReference other)
        {
            return IsSymbol == other.IsSymbol && Index == other.Index;
        }

        public override bool Equals(object other)
        {
            return other is GlyphReference glyph && Equals(glyph);
        }

        public override int GetHashCode()
        {
            return IsSymbol ? ~Index : Index;
        }

        /// <summary>The glyph's lines in <paramref name="pack"/>, read straight from the pack.</summary>
        internal GlyphStroke[] StrokesIn(WireframeGlyphPack pack)
        {
            if (IsSymbol)
            {
                return Index < pack.Symbols.Length ? pack.Symbols[Index].Strokes : Array.Empty<GlyphStroke>();
            }
            return Index < pack.Characters.Length ? pack.Characters[Index].Strokes : Array.Empty<GlyphStroke>();
        }

        /// <summary>True when the glyph is still in <paramref name="pack"/>, which edits and undo can change.</summary>
        internal bool IsIn(WireframeGlyphPack pack)
        {
            return Index >= 0 && Index < (IsSymbol ? pack.Symbols.Length : pack.Characters.Length);
        }
    }
}
