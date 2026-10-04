using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Builds glyph packs and glyph lists in code, with glyphs simple enough to check by hand.</summary>
    public abstract class GlyphTestBase : WireframesTestBase
    {
        private protected const float Baseline = WireframeGlyphPack.Baseline;

        /// <summary>
        /// Glyphs simple enough to check by hand: A is a line from 0.2 to 0.8 on the baseline, B a closed square from
        /// 0.25 to 0.75, ? a line from 0.4 to 0.6, and the space has no lines. The Heart symbol is a closed triangle.
        /// </summary>
        private protected WireframeGlyphs CreateTestGlyphs()
        {
            return CreateGlyphs(CreatePack(
                new[]
                {
                    Character('A', Open(0.2f, Baseline, 0.8f, Baseline)),
                    Character('B', Closed(0.25f, Baseline, 0.75f, Baseline, 0.75f, Baseline + 0.5f, 0.25f, Baseline + 0.5f)),
                    Character('?', Open(0.4f, Baseline, 0.6f, Baseline)),
                    Character(' ')
                },
                new[] { Symbol("Heart", Closed(0f, 0f, 1f, 0f, 0.5f, 1f)) }));
        }

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
