using System;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A symbol drawn with a glyph of a <see cref="WireframeGlyphs"/>, named by a member of the enum the Glyph Editor
    /// generates for a glyph pack. Its glyph box, <see cref="Size"/> across, is centered on its position and lies in the
    /// XY plane of its rotation, read from the -Z side. A symbol the glyphs lack is drawn as '?', with one warning.
    /// </summary>
    public interface ISymbol : IRigidShape
    {
        /// <summary>
        /// The glyphs to draw with, or null, the default, for <see cref="WireframeGlyphs.Default"/>.
        /// </summary>
        WireframeGlyphs Glyphs { get; set; }

        /// <summary>Width and height of the glyph box in the bone's units. 1 by default.</summary>
        float Size { get; set; }

        /// <summary>
        /// Draws <paramref name="symbol"/>, a member of a generated symbol enum, such as
        /// <see cref="DefaultSymbols.Heart"/>. A symbol of the same name from any pack of the glyphs counts, and the
        /// enum's None member draws nothing.
        /// </summary>
        /// <exception cref="ArgumentException"><typeparamref name="TSymbol"/> doesn't have int values.</exception>
        void SetSymbol<TSymbol>(TSymbol symbol) where TSymbol : unmanaged, Enum;

        /// <summary>
        /// Returns the symbol drawn as a member of <typeparamref name="TSymbol"/>. A symbol set from another pack's enum
        /// comes back as the member of the same name, or as an undefined value when this enum has none.
        /// </summary>
        /// <exception cref="ArgumentException"><typeparamref name="TSymbol"/> doesn't have int values.</exception>
        TSymbol GetSymbol<TSymbol>() where TSymbol : unmanaged, Enum;
    }
}
