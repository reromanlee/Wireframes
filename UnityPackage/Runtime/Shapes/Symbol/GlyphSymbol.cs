using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes
{
    internal sealed class GlyphSymbol : GlyphShape, ISymbol
    {
        private const char Fallback = '?';

        private int _key;
        private float _size;

        internal GlyphSymbol(
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation,
            int key,
            WireframeGlyphs glyphs,
            float size)
            : base(LaidOut(key, glyphs, size), glyphs, bone, localPosition, localRotation)
        {
            _key = key;
            _size = size;
        }

        public float Size
        {
            get
            {
                EnsureUsable();
                return _size;
            }
            set
            {
                EnsureUsable();
                if (value != _size)
                {
                    _size = value;
                    Rebuild(false);
                }
            }
        }

        /// <summary>The hash of the drawn symbol's keyword, the value of its member in generated enums.</summary>
        internal int SymbolKey
        {
            get
            {
                EnsureUsable();
                return _key;
            }
            set
            {
                EnsureUsable();
                if (value != _key)
                {
                    _key = value;
                    Rebuild(true);
                }
            }
        }

        public void SetSymbol<TSymbol>(TSymbol symbol) where TSymbol : unmanaged, Enum
        {
            SymbolKey = SymbolKeys.Of(symbol);
        }

        public TSymbol GetSymbol<TSymbol>() where TSymbol : unmanaged, Enum
        {
            EnsureUsable();
            return SymbolKeys.ToSymbol<TSymbol>(_key);
        }

        /// <summary>Sets the symbol, the glyphs and the size at once and lays them out once, for components.</summary>
        internal void Apply(int key, WireframeGlyphs glyphs, float size)
        {
            EnsureUsable();
            bool glyphsChanged = ReplaceGlyphs(glyphs) | key != _key;
            _key = key;
            _size = size;
            Rebuild(glyphsChanged);
        }

        protected override void Layout(GlyphGeometry geometry, WireframeGlyphs glyphs)
        {
            WriteGlyph(_key, glyphs, _size, geometry, WarningContext);
        }

        protected override void ScaleSizes(float factor)
        {
            _size *= factor;
            Rebuild(false);
        }

        private static GlyphGeometry LaidOut(int key, WireframeGlyphs glyphs, float size)
        {
            GlyphGeometry geometry = new();
            WriteGlyph(key, DrawnGlyphsOf(glyphs), size, geometry, null);
            return geometry;
        }

        /// <summary>Writes the symbol's glyph with its box centered on the origin, or '?' when the glyphs lack it.</summary>
        private static void WriteGlyph(int key, WireframeGlyphs glyphs, float size, GlyphGeometry geometry, Object context)
        {
            if (key == SymbolKeys.None || !TryFindGlyph(key, glyphs, context, out ResolvedGlyph glyph))
            {
                geometry.Begin(0, 0);
                return;
            }
            geometry.Begin(glyph.Metrics.VertexCount, glyph.Metrics.EdgeCount);
            float half = size * 0.5f;
            geometry.AddGlyph(glyph.Strokes, new Vector2(-half, -half), size);
        }

        private static bool TryFindGlyph(int key, WireframeGlyphs glyphs, Object context, out ResolvedGlyph glyph)
        {
            if (glyphs == null)
            {
                GlyphWarnings.ReportMissingDefault(context);
                glyph = default;
                return false;
            }
            if (glyphs.TryResolveSymbol(key, out glyph))
            {
                return true;
            }
            GlyphWarnings.ReportMissingSymbol(glyphs, key, context);
            return glyphs.TryResolveCharacter(Fallback, out glyph);
        }
    }
}
