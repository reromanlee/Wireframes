using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Builds glyph packs and glyph lists in code, with glyphs simple enough to check by hand.</summary>
    public abstract class GlyphTestBase : WireframesTestBase
    {
        private protected WireframeGlyphPack CreatePack(CharacterGlyph[] characters, SymbolGlyph[] symbols = null)
        {
            WireframeGlyphPack pack = Track(ScriptableObject.CreateInstance<WireframeGlyphPack>());
            pack.SetGlyphs(characters, symbols);
            return pack;
        }

        private protected WireframeGlyphs CreateGlyphs(params WireframeGlyphPack[] packs)
        {
            WireframeGlyphs glyphs = Track(ScriptableObject.CreateInstance<WireframeGlyphs>());
            glyphs.SetPacks(packs);
            return glyphs;
        }

        private protected static CharacterGlyph Character(char character, params GlyphStroke[] strokes)
        {
            return new CharacterGlyph(character, strokes);
        }

        private protected static SymbolGlyph Symbol(string keyword, params GlyphStroke[] strokes)
        {
            return new SymbolGlyph(keyword, strokes);
        }

        /// <summary>An open stroke through points given as x, y pairs.</summary>
        private protected static GlyphStroke Open(params float[] coordinates)
        {
            return new GlyphStroke(Points(coordinates), false);
        }

        /// <summary>A closed stroke through points given as x, y pairs.</summary>
        private protected static GlyphStroke Closed(params float[] coordinates)
        {
            return new GlyphStroke(Points(coordinates), true);
        }

        private static Vector2[] Points(float[] coordinates)
        {
            Vector2[] points = new Vector2[coordinates.Length / 2];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector2(coordinates[i * 2], coordinates[i * 2 + 1]);
            }
            return points;
        }
    }
}
