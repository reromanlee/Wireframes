using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Creates symbols in a <see cref="WireframeContainer"/>, named by members of the enums the Glyph Editor generates
    /// for glyph packs, and drawn with <see cref="WireframeGlyphs.Default"/>.
    /// </summary>
    public static class SymbolFactory
    {
        /// <summary>
        /// Creates a white symbol 1 unit across, centered on the world origin, in the XY plane and read from the -Z side.
        /// </summary>
        /// <exception cref="ArgumentException"><typeparamref name="TSymbol"/> doesn't have int values.</exception>
        public static ISymbol CreateSymbol<TSymbol>(this WireframeContainer container, TSymbol symbol)
            where TSymbol : unmanaged, Enum
        {
            return CreateSymbol(container, null, Vector3.zero, Quaternion.identity, symbol);
        }

        /// <summary>
        /// Creates a white symbol 1 unit across, centered on a world position, in the XY plane of
        /// <paramref name="rotation"/> and read from its -Z side.
        /// </summary>
        /// <exception cref="ArgumentException"><typeparamref name="TSymbol"/> doesn't have int values.</exception>
        public static ISymbol CreateSymbol<TSymbol>(
            this WireframeContainer container, Vector3 position, Quaternion rotation, TSymbol symbol)
            where TSymbol : unmanaged, Enum
        {
            return CreateSymbol(container, null, position, rotation, symbol);
        }

        /// <summary>
        /// Creates a white symbol 1 bone unit across that follows <paramref name="bone"/>, centered on a position in the
        /// bone's local space, in the XY plane of <paramref name="localRotation"/> and read from its -Z side.
        /// </summary>
        /// <exception cref="ArgumentException"><typeparamref name="TSymbol"/> doesn't have int values.</exception>
        public static ISymbol CreateSymbol<TSymbol>(
            this WireframeContainer container, Transform bone, Vector3 localPosition, Quaternion localRotation,
            TSymbol symbol)
            where TSymbol : unmanaged, Enum
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            int key = SymbolKeys.Of(symbol);
            return proxy.Add(new GlyphSymbol(bone, localPosition, localRotation, key, null, ShapeDefaults.Size));
        }
    }
}
