using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Laying text out into points and edges, with the glyphs of <see cref="GlyphTestBase.CreateTestGlyphs"/>.</summary>
    public class TextLayoutTests : GlyphTestBase
    {
        private readonly GlyphGeometry _geometry = new();

        [Test]
        public void Proportional_AdvancesByInkWidthAndSpacing()
        {
            TextSettings settings = TopLeft(WireframeCharacterWidth.Proportional, 0.1f);

            TextLayout.Layout("AB", CreateTestGlyphs(), settings, _geometry, null);

            Assert.That(_geometry.VertexCount, Is.EqualTo(2 + 4));
            Assert.That(_geometry.EdgeCount, Is.EqualTo(1 + 4));
            // Each glyph's ink starts at the pen: A at the left edge, B after A's 0.6 of ink and 0.1 of spacing.
            AssertApproximately(new Vector3(-5f, 1f - 1f + Baseline, 0f), _geometry.Points[0]);
            AssertApproximately(new Vector3(-5f + 0.7f, Baseline, 0f), _geometry.Points[2]);
            AssertApproximately(new Vector3(-5f + 0.7f + 0.5f, Baseline + 0.5f, 0f), _geometry.Points[4]);
        }

        [Test]
        public void Monospace_AdvancesByWholeBoxes()
        {
            TextSettings settings = TopLeft(WireframeCharacterWidth.Monospace, 0f);

            TextLayout.Layout("AB", CreateTestGlyphs(), settings, _geometry, null);

            AssertApproximately(new Vector3(-5f + 0.2f, Baseline, 0f), _geometry.Points[0]);
            AssertApproximately(new Vector3(-5f + 1f + 0.25f, Baseline, 0f), _geometry.Points[2]);
        }

        [Test]
        public void Edges_JoinEachStrokeInOrderAndCloseClosedOnes()
        {
            TextLayout.Layout("AB", CreateTestGlyphs(), TopLeft(WireframeCharacterWidth.Proportional, 0f), _geometry, null);

            int[] expected = { 0, 1, 2, 3, 3, 4, 4, 5, 5, 2 };
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(_geometry.Pattern[i], Is.EqualTo(expected[i]), $"Index {i}");
            }
        }

        [Test]
        public void Alignments_PlaceTheLinesInTheBounds()
        {
            TextSettings settings = TopLeft(WireframeCharacterWidth.Proportional, 0f);
            WireframeGlyphs glyphs = CreateTestGlyphs();

            settings.HorizontalAlignment = WireframeHorizontalAlignment.Center;
            settings.VerticalAlignment = WireframeVerticalAlignment.Middle;
            TextLayout.Layout("A", glyphs, settings, _geometry, null);
            AssertApproximately(new Vector3(-0.3f, -0.5f + Baseline, 0f), _geometry.Points[0]);

            settings.HorizontalAlignment = WireframeHorizontalAlignment.Right;
            settings.VerticalAlignment = WireframeVerticalAlignment.Bottom;
            TextLayout.Layout("A", glyphs, settings, _geometry, null);
            AssertApproximately(new Vector3(5f - 0.6f, -1f + Baseline, 0f), _geometry.Points[0]);
        }

        [Test]
        public void CharacterSize_ScalesEverything()
        {
            TextSettings settings = TopLeft(WireframeCharacterWidth.Proportional, 0.1f);
            settings.CharacterSize = 2f;

            TextLayout.Layout("AB", CreateTestGlyphs(), settings, _geometry, null);

            AssertApproximately(new Vector3(-5f, 1f - 2f + Baseline * 2f, 0f), _geometry.Points[0]);
            AssertApproximately(new Vector3(-5f + 0.7f * 2f, -1f + Baseline * 2f, 0f), _geometry.Points[2]);
        }

        [Test]
        public void LineBreaks_StartLinesFurtherDown()
        {
            TextSettings settings = TopLeft(WireframeCharacterWidth.Proportional, 0f);
            settings.LineSpacing = 0.5f;

            TextLayout.Layout("A\nA\r\nA", CreateTestGlyphs(), settings, _geometry, null);

            Assert.That(_geometry.VertexCount, Is.EqualTo(6));
            AssertApproximately(new Vector3(-5f, Baseline, 0f), _geometry.Points[0]);
            AssertApproximately(new Vector3(-5f, Baseline - 1.5f, 0f), _geometry.Points[2]);
            AssertApproximately(new Vector3(-5f, Baseline - 3f, 0f), _geometry.Points[4]);
        }

        [Test]
        public void Wrap_BreaksAtTheLastSpaceThatFits()
        {
            TextSettings settings = TopLeft(WireframeCharacterWidth.Proportional, 0f);
            settings.Overflow = WireframeTextOverflow.Wrap;
            // AB is 1.1 wide and a space 0.5, so "AB AB" takes 2.7 and a third word needs a line of its own.
            settings.Bounds = new Vector2(3f, 2f);

            TextLayout.Layout("AB AB AB", CreateTestGlyphs(), settings, _geometry, null);

            Assert.That(_geometry.VertexCount, Is.EqualTo(3 * 6));
            AssertApproximately(new Vector3(-1.5f + 1.6f, 1f - 1f + Baseline, 0f), _geometry.Points[6]);
            AssertApproximately(new Vector3(-1.5f, 1f - 2f + Baseline, 0f), _geometry.Points[12]);
        }

        [Test]
        public void Wrap_BreaksInsideAWordOnlyWhenItIsWiderThanTheBounds()
        {
            TextSettings settings = TopLeft(WireframeCharacterWidth.Proportional, 0f);
            settings.Overflow = WireframeTextOverflow.Wrap;
            settings.Bounds = new Vector2(2f, 2f);

            TextLayout.Layout("ABABAB", CreateTestGlyphs(), settings, _geometry, null);

            // A, B and A take 1.7, so the second B starts the second line.
            AssertApproximately(new Vector3(-1f + 1.1f, Baseline, 0f), _geometry.Points[6]);
            AssertApproximately(new Vector3(-1f, Baseline - 1f, 0f), _geometry.Points[8]);
        }

        [Test]
        public void Overflow_KeepsLinesAsWritten()
        {
            TextSettings settings = TopLeft(WireframeCharacterWidth.Proportional, 0f);
            settings.Bounds = new Vector2(1f, 2f);

            TextLayout.Layout("AB AB", CreateTestGlyphs(), settings, _geometry, null);

            AssertApproximately(new Vector3(-0.5f + 1.6f, Baseline, 0f), _geometry.Points[6]);
        }

        [Test]
        public void TabsAndControlCharacters_AdvanceOrDrawNothing()
        {
            TextSettings settings = TopLeft(WireframeCharacterWidth.Proportional, 0f);

            TextLayout.Layout("A\t\u0007B", CreateTestGlyphs(), settings, _geometry, null);

            // A tab advances 4 spaces of 0.5.
            AssertApproximately(new Vector3(-5f + 0.6f + 2f, Baseline, 0f), _geometry.Points[2]);
        }

        [Test]
        public void UnsupportedCharacters_AreDrawnAsQuestionMarksWithOneWarning()
        {
            TextSettings settings = TopLeft(WireframeCharacterWidth.Proportional, 0f);
            WireframeGlyphs glyphs = CreateTestGlyphs();
            LogAssert.Expect(LogType.Warning, new Regex(@"'Z' \(U\+005A\) isn't in the glyphs"));

            TextLayout.Layout("AZZ", glyphs, settings, _geometry, null);
            TextLayout.Layout("AZZ", glyphs, settings, _geometry, null);

            LogAssert.NoUnexpectedReceived();
            // Both drawn as ?, 0.2 wide and starting at the pen.
            Assert.That(_geometry.VertexCount, Is.EqualTo(2 + 2 + 2));
            AssertApproximately(new Vector3(-5f + 0.6f, Baseline, 0f), _geometry.Points[2]);
            AssertApproximately(new Vector3(-5f + 0.8f, Baseline, 0f), _geometry.Points[4]);
        }

        [Test]
        public void FallbackRanges_TellWhereQuestionMarksAreDrawnInsteadOfWarning()
        {
            List<RangeInt> fallbacks = new();

            TextLayout.Layout("AZB", CreateTestGlyphs(), TopLeft(WireframeCharacterWidth.Proportional, 0f), _geometry,
                null, fallbacks);

            Assert.That(fallbacks, Is.EqualTo(new[] { new RangeInt(2, 2) }));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void SpaceMissingFromTheGlyphs_StillSeparatesWords()
        {
            WireframeGlyphs glyphs = CreateGlyphs(CreatePack(new[] { Character('A', Open(0.2f, Baseline, 0.8f, Baseline)) }));

            TextLayout.Layout("A A", glyphs, TopLeft(WireframeCharacterWidth.Proportional, 0f), _geometry, null);

            AssertApproximately(new Vector3(-5f + 0.6f + WireframeGlyphPack.DefaultSpaceWidth, Baseline, 0f), _geometry.Points[2]);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void EmptyText_DrawsNothing()
        {
            TextLayout.Layout("", CreateTestGlyphs(), TopLeft(WireframeCharacterWidth.Proportional, 0f), _geometry, null);

            Assert.That(_geometry.VertexCount, Is.Zero);
            Assert.That(_geometry.EdgeCount, Is.Zero);
        }

        [Test]
        public void Buffers_GrowByDoublingAndShrinkOnceMostlyEmpty()
        {
            GlyphGeometry geometry = new();
            Assert.That(geometry.VertexCapacity, Is.EqualTo(16));

            geometry.Begin(100, 10);
            Assert.That(geometry.VertexCapacity, Is.EqualTo(128));
            geometry.Begin(40, 10);
            Assert.That(geometry.VertexCapacity, Is.EqualTo(128));
            geometry.Begin(20, 10);
            Assert.That(geometry.VertexCapacity, Is.EqualTo(64));
        }

        /// <summary>Characters 1 unit tall at the top left of 10 by 2 bounds, kept on their lines.</summary>
        private static TextSettings TopLeft(WireframeCharacterWidth width, float spacing)
        {
            TextSettings settings = TextSettings.Default;
            settings.CharacterWidth = width;
            settings.CharacterSpacing = spacing;
            settings.Bounds = new Vector2(10f, 2f);
            settings.HorizontalAlignment = WireframeHorizontalAlignment.Left;
            settings.VerticalAlignment = WireframeVerticalAlignment.Top;
            return settings;
        }
    }
}
