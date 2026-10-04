using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Finding glyphs in packs and glyph lists, and measuring them.</summary>
    public class GlyphLookupTests : GlyphTestBase
    {
        [Test]
        public void FirstPackInTheList_DrawsACharacterOrSymbolBothHave()
        {
            WireframeGlyphPack mine = CreatePack(
                new[] { Character('A', Open(0f, 0f, 1f, 1f)) },
                new[] { Symbol("Heart", Open(0f, 0f, 1f, 1f)) });
            WireframeGlyphPack fallback = CreatePack(
                new[] { Character('A', Open(0f, 0f, 0.5f, 0.5f)), Character('B', Open(0f, 0f, 0.5f, 0.5f)) },
                new[] { Symbol("Heart", Open(0f, 0f, 0.5f, 0.5f)), Symbol("Star", Open(0f, 0f, 0.5f, 0.5f)) });
            WireframeGlyphs glyphs = CreateGlyphs(mine, fallback);

            Assert.That(glyphs.TryResolveCharacter('A', out ResolvedGlyph a), Is.True);
            Assert.That(a.Pack, Is.SameAs(mine));
            Assert.That(glyphs.TryResolveCharacter('B', out ResolvedGlyph b), Is.True);
            Assert.That(b.Pack, Is.SameAs(fallback));
            Assert.That(glyphs.TryResolveSymbol(SymbolKeys.Of("Heart"), out ResolvedGlyph heart), Is.True);
            Assert.That(heart.Pack, Is.SameAs(mine));
            Assert.That(glyphs.TryResolveSymbol(SymbolKeys.Of("Star"), out ResolvedGlyph star), Is.True);
            Assert.That(star.Pack, Is.SameAs(fallback));

            glyphs.SetPacks(fallback, mine);

            Assert.That(glyphs.TryResolveCharacter('A', out a) && a.Pack == fallback, Is.True);
            Assert.That(glyphs.TryResolveSymbol(SymbolKeys.Of("Heart"), out heart) && heart.Pack == fallback, Is.True);
        }

        [Test]
        public void PackThatRepeatsAGlyph_KeepsItsFirst()
        {
            GlyphStroke first = Open(0f, 0f, 1f, 0f);
            WireframeGlyphPack pack = CreatePack(
                new[] { Character('A', first), Character('A', Open(0f, 0f, 0f, 1f)) },
                new[] { Symbol("Heart", first), Symbol("Heart", Open(0f, 0f, 0f, 1f)) });
            WireframeGlyphs glyphs = CreateGlyphs(pack);

            Assert.That(glyphs.TryResolveCharacter('A', out ResolvedGlyph a), Is.True);
            Assert.That(a.Strokes[0].Points[1], Is.EqualTo(new Vector2(1f, 0f)));
            Assert.That(glyphs.TryResolveSymbol(SymbolKeys.Of("Heart"), out ResolvedGlyph heart), Is.True);
            Assert.That(heart.Strokes[0].Points[1], Is.EqualTo(new Vector2(1f, 0f)));
        }

        [Test]
        public void Lookups_SkipEmptyEntriesAndInvalidKeywords()
        {
            WireframeGlyphPack pack = CreatePack(
                new[] { Character('A', Open(0f, 0f, 1f, 1f)) },
                new[] { Symbol("class", Open(0f, 0f, 1f, 1f)), Symbol("", Open(0f, 0f, 1f, 1f)) });
            WireframeGlyphs glyphs = CreateGlyphs(null, pack, null);

            Assert.That(glyphs.Contains('A'), Is.True);
            Assert.That(glyphs.Contains('B'), Is.False);
            Assert.That(glyphs.TryResolveSymbol(SymbolKeys.Of("class"), out _), Is.False);
            Assert.That(glyphs.TryResolveSymbol(SymbolKeys.Of(""), out _), Is.False);
            Assert.That(glyphs.TryResolveSymbol(SymbolKeys.None, out _), Is.False);
        }

        [Test]
        public void CharactersPastAscii_AreFoundToo()
        {
            const int grinningFace = 0x1F600;
            WireframeGlyphPack pack = CreatePack(new[]
            {
                Character('\u0416', Open(0f, 0f, 1f, 1f)),
                new CharacterGlyph(grinningFace, new[] { Open(0f, 0f, 1f, 1f) })
            });
            WireframeGlyphs glyphs = CreateGlyphs(pack);

            Assert.That(glyphs.Contains('\u0416'), Is.True);
            Assert.That(glyphs.TryResolveCharacter(grinningFace, out _), Is.True);
            Assert.That(glyphs.Contains('\u0401'), Is.False);
        }

        [Test]
        public void EditedPack_IsReadAgainByEveryList()
        {
            WireframeGlyphPack pack = CreatePack(new[] { Character('A', Open(0f, 0f, 1f, 1f)) });
            WireframeGlyphs glyphs = CreateGlyphs(pack);
            Assert.That(glyphs.Contains('A'), Is.True);

            pack.SetGlyphs(new[] { Character('B', Open(0f, 0f, 1f, 1f)) }, null);

            Assert.That(glyphs.Contains('A'), Is.False);
            Assert.That(glyphs.Contains('B'), Is.True);
        }

        [Test]
        public void Metrics_MeasureOnlyTheStrokesThatDraw()
        {
            GlyphStroke[] strokes =
            {
                Open(0.25f, 0.25f, 0.5f, 0.75f, 0.75f, 0.25f),
                Closed(0.4f, 0.4f, 0.6f, 0.4f, 0.5f, 0.5f),
                // Too few points, or a point that isn't a number, draw nothing.
                Open(0f, 1f),
                Closed(0.9f, 0.9f, 1f, 1f),
                Open(0.1f, 0.1f, float.NaN, 0.1f)
            };

            GlyphMetrics metrics = GlyphMetrics.Measure(strokes);

            Assert.That(metrics.HasInk, Is.True);
            Assert.That(metrics.InkLeft, Is.EqualTo(0.25f));
            Assert.That(metrics.InkRight, Is.EqualTo(0.75f));
            Assert.That(metrics.InkBottom, Is.EqualTo(0.25f));
            Assert.That(metrics.InkTop, Is.EqualTo(0.75f));
            Assert.That(metrics.VertexCount, Is.EqualTo(3 + 3));
            Assert.That(metrics.EdgeCount, Is.EqualTo(2 + 3));
        }

        [Test]
        public void ProportionalWidth_IsTheInkWidthOrThePacksSpaceWidth()
        {
            WireframeGlyphPack pack = CreatePack(new[]
            {
                Character('I', Open(0.4f, 0.25f, 0.6f, 0.75f)),
                Character(' ')
            });
            WireframeGlyphs glyphs = CreateGlyphs(pack);

            Assert.That(glyphs.TryResolveCharacter('I', out ResolvedGlyph letter), Is.True);
            AssertApproximately(0.2f, letter.ProportionalWidth);
            Assert.That(glyphs.TryResolveCharacter(' ', out ResolvedGlyph space), Is.True);
            Assert.That(space.Metrics.HasInk, Is.False);
            Assert.That(space.ProportionalWidth, Is.EqualTo(WireframeGlyphPack.DefaultSpaceWidth));
        }
    }
}
