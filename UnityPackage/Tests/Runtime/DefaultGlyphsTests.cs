using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>The package's Default Glyphs: its Default Font and Default Symbols.</summary>
    public class DefaultGlyphsTests : WireframesTestBase
    {
        [Test]
        public void Default_DrawsEveryPrintableAsciiCharacter()
        {
            WireframeGlyphs glyphs = WireframeGlyphs.Default;

            Assert.That(glyphs, Is.Not.Null, "The Default Glyphs asset is missing from the package's Resources.");
            for (char character = ' '; character <= '~'; character++)
            {
                Assert.That(glyphs.Contains(character), Is.True, $"'{character}' is missing.");
            }
        }

        [Test]
        public void Default_DrawsEveryDefaultSymbol()
        {
            foreach (DefaultSymbols symbol in Enum.GetValues(typeof(DefaultSymbols)))
            {
                Assert.That(WireframeGlyphs.Default.Contains(symbol), Is.EqualTo(symbol != DefaultSymbols.None), $"{symbol}");
            }
        }

        [Test]
        public void DefaultGlyphs_DrawEveryStrokeInsideTheirBox()
        {
            foreach (WireframeGlyphPack pack in WireframeGlyphs.Default.Packs)
            {
                foreach (CharacterGlyph character in pack.Characters)
                {
                    AssertDrawnInsideTheBox(character.Strokes, $"'{(char)character.Character}' of {pack.name}");
                }
                foreach (SymbolGlyph symbol in pack.Symbols)
                {
                    AssertDrawnInsideTheBox(symbol.Strokes, $"{symbol.Keyword} of {pack.name}");
                }
            }
        }

        [Test]
        public void NewComponents_DrawTheirDefaultsWithTheDefaultGlyphs()
        {
            WireframeSymbol symbol = Track(new GameObject("Symbol")).AddComponent<WireframeSymbol>();
            Track(new GameObject("Text")).AddComponent<WireframeText>();

            Assert.That(symbol.GetSymbol<DefaultSymbols>(), Is.EqualTo(DefaultSymbols.Star));
            LogAssert.NoUnexpectedReceived();
        }

        private static void AssertDrawnInsideTheBox(GlyphStroke[] strokes, string glyph)
        {
            foreach (GlyphStroke stroke in strokes)
            {
                Assert.That(stroke.IsDrawn, Is.True, $"{glyph} has a stroke that draws nothing.");
                foreach (Vector2 point in stroke.Points)
                {
                    bool isInside = point.x is >= 0f and <= 1f && point.y is >= 0f and <= 1f;
                    Assert.That(isInside, Is.True, $"{glyph} has a point outside its box: {point}.");
                }
            }
        }
    }
}
