using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a symbol on its GameObject with a glyph of a <see cref="WireframeGlyphs"/>. Its glyph box, <see cref="Size"/>
    /// across, is centered on <see cref="WireframeCenteredShape.Center"/> and stands in the XY plane of its rotation, read
    /// from the -Z side. A symbol the glyphs lack is drawn as '?', with one warning.
    /// </summary>
    [AddComponentMenu("Wireframes/Symbol")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeSymbol : WireframeCenteredShape
    {
        // The key of the Star symbol, which new components draw.
        private const int DefaultSymbolKey = 1365736337;

        [Tooltip("Glyphs to draw with. Empty draws with the package's Default Glyphs.")]
        [SerializeField] private WireframeGlyphs _glyphs;

        [Tooltip("Symbol drawn, from the symbols of the glyphs.")]
        [SymbolKey]
        [SerializeField] private int _symbol = DefaultSymbolKey;

        [Tooltip("Width and height of the glyph box, in the GameObject's local units.")]
        [SerializeField] private float _size = ShapeDefaults.Size;

        /// <summary>
        /// Glyphs to draw with. A new component in the Editor gets <see cref="WireframeGlyphs.Default"/>, which null
        /// draws with too.
        /// </summary>
        public WireframeGlyphs Glyphs
        {
            get => _glyphs;
            set
            {
                _glyphs = value;
                if (TryGetShape(out GlyphSymbol symbol))
                {
                    symbol.Glyphs = value;
                }
            }
        }

        /// <summary>Width and height of the glyph box, in the GameObject's local units. 1 by default.</summary>
        public float Size
        {
            get => _size;
            set
            {
                _size = value;
                if (TryGetShape(out GlyphSymbol symbol))
                {
                    symbol.Size = value;
                }
            }
        }

        /// <summary>The hash of the drawn symbol's keyword, the value of its member in generated enums.</summary>
        internal int SymbolKey
        {
            get => _symbol;
            set
            {
                _symbol = value;
                if (TryGetShape(out GlyphSymbol symbol))
                {
                    symbol.SymbolKey = value;
                }
            }
        }

        /// <summary>
        /// Draws <paramref name="symbol"/>, a member of a generated symbol enum, such as <c>DefaultSymbols.Heart</c>. A
        /// symbol of the same name from any pack of the glyphs counts, and the enum's None member draws nothing.
        /// </summary>
        /// <exception cref="ArgumentException"><typeparamref name="TSymbol"/> doesn't have int values.</exception>
        public void SetSymbol<TSymbol>(TSymbol symbol) where TSymbol : unmanaged, Enum
        {
            SymbolKey = SymbolKeys.Of(symbol);
        }

        /// <summary>
        /// Returns the symbol drawn as a member of <typeparamref name="TSymbol"/>. A symbol set from another pack's enum
        /// comes back as the member of the same name, or as an undefined value when this enum has none.
        /// </summary>
        /// <exception cref="ArgumentException"><typeparamref name="TSymbol"/> doesn't have int values.</exception>
        public TSymbol GetSymbol<TSymbol>() where TSymbol : unmanaged, Enum
        {
            return SymbolKeys.ToSymbol<TSymbol>(_symbol);
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new GlyphSymbol(bone, Center, Rotation, _symbol, _glyphs, _size) { WarningContext = this };
        }

        internal override void ApplySizesTo(Shape shape)
        {
            ((GlyphSymbol)shape).Apply(_symbol, _glyphs, _size);
        }

        internal override bool CanApplyInPlace(Shape shape)
        {
            // Another symbol or other glyphs may need more room than the shape has.
            return ((GlyphSymbol)shape).Shows(_symbol, _glyphs);
        }

        private void Reset()
        {
            _glyphs = WireframeGlyphs.Default;
        }
    }
}
