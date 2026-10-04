using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>
    /// Text and symbol components, drawn with the glyphs of <see cref="GlyphTestBase.CreateTestGlyphs"/>, which they are
    /// given before they are enabled.
    /// </summary>
    public class GlyphComponentTests : GlyphTestBase
    {
        private enum TestSymbols
        {
            None = 0,
            Heart = -1226833789,
            Star = 1365736337
        }

        [Test]
        public void AddedText_DrawsItsTextOnItsGameObject()
        {
            WireframeText text = AddGlyphComponent<WireframeText>(new Vector3(1f, 2f, 3f));

            // A runs from -0.3 to 0.3 across the text, 0.25 below its center.
            Vector3[] vertices = Bake(text);
            AssertApproximately(new Vector3(0.7f, 1.75f, 3f), vertices[0]);
            AssertApproximately(new Vector3(1.3f, 1.75f, 3f), vertices[1]);
        }

        [Test]
        public void TextEdits_ShowUpInTheShape()
        {
            WireframeText text = AddGlyphComponent<WireframeText>();

            text.CharacterSize = 0.5f;
            text.HorizontalAlignment = WireframeHorizontalAlignment.Left;
            text.VerticalAlignment = WireframeVerticalAlignment.Top;
            text.Bounds = new Vector2(2f, 2f);
            text.Center = new Vector3(0f, 0f, 1f);

            Vector3[] vertices = Bake(text);
            AssertApproximately(new Vector3(-1f, 1f - 0.5f + Baseline * 0.5f, 1f), vertices[0]);
            Assert.That(((GlyphText)text.Shape).CharacterSize, Is.EqualTo(0.5f));
        }

        [Test]
        public void SettingTheTextEveryFrame_AllocatesNothing()
        {
            WireframeText text = AddGlyphComponent<WireframeText>();
            WireframeContainer container = text.SharedContainer.Container;
            char[] characters = { 'A', 'B', 'B', 'A', 'B' };

            void ChangeTheTextBackAndForth()
            {
                text.SetText(characters.AsSpan(0, 5));
                container.Proxy.Flush();
                text.SetText(characters.AsSpan(0, 2));
                container.Proxy.Flush();
            }

            ChangeTheTextBackAndForth();
            ChangeTheTextBackAndForth();

            Assert.That(ChangeTheTextBackAndForth, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void TextSetWithASpan_BecomesTheSavedText()
        {
            WireframeText text = AddGlyphComponent<WireframeText>();

            text.SetText("BA".AsSpan());
            ((ISerializationCallbackReceiver)text).OnBeforeSerialize();

            Assert.That(text.Text, Is.EqualTo("BA"));
            Assert.That(((GlyphText)text.Shape).Text, Is.EqualTo("BA"));
        }

        [Test]
        public void TextEditedWhileDisabled_IsDrawnOnceEnabled()
        {
            WireframeText text = AddGlyphComponent<WireframeText>();
            WireframeGlyphPack pack = text.Glyphs.Packs[0];
            text.enabled = false;

            text.Text = "AB";
            pack.SetGlyphs(new[] { Character('A', Open(0f, 0f, 1f, 0f, 1f, 1f)), Character('B') }, null);
            text.enabled = true;

            Assert.That(text.Shape.EdgeCount, Is.EqualTo(2));
        }

        [Test]
        public void InvalidTextValues_ThrowAndChangeNothing()
        {
            WireframeText text = AddGlyphComponent<WireframeText>();

            Assert.Throws<ArgumentOutOfRangeException>(() => text.CharacterWidth = (WireframeCharacterWidth)5);
            Assert.Throws<ArgumentOutOfRangeException>(() => text.Overflow = (WireframeTextOverflow)5);
            Assert.That(text.CharacterWidth, Is.EqualTo(WireframeCharacterWidth.Proportional));
            Assert.That(text.Overflow, Is.EqualTo(WireframeTextOverflow.Overflow));
        }

        [Test]
        public void AddedSymbol_DrawsItsSymbolOnItsGameObject()
        {
            WireframeSymbol symbol = AddGlyphComponent<WireframeSymbol>(new Vector3(0f, 0f, 2f));

            symbol.SetSymbol(TestSymbols.Heart);
            symbol.Size = 2f;

            Vector3[] vertices = Bake(symbol);
            Assert.That(vertices, Has.Length.EqualTo(16));
            AssertApproximately(new Vector3(-1f, -1f, 2f), vertices[0]);
            AssertApproximately(new Vector3(0f, 1f, 2f), vertices[2]);
            Assert.That(symbol.GetSymbol<TestSymbols>(), Is.EqualTo(TestSymbols.Heart));
        }

        [Test]
        public void NewSymbol_DrawsTheStar()
        {
            WireframeSymbol symbol = AddGlyphComponent<WireframeSymbol>();

            Assert.That(symbol.GetSymbol<TestSymbols>(), Is.EqualTo(TestSymbols.Star));
            Assert.That(symbol.Shape.EdgeCount, Is.EqualTo(1));
        }

        /// <summary>
        /// A component on a new GameObject, given the test glyphs before it is enabled, and for a text, the text "A".
        /// </summary>
        private T AddGlyphComponent<T>(Vector3 position = default) where T : WireframeShape
        {
            GameObject owner = Track(new GameObject(typeof(T).Name));
            owner.SetActive(false);
            owner.transform.position = position;
            T component = owner.AddComponent<T>();
            WireframeGlyphs glyphs = CreateTestGlyphs();
            if (component is WireframeText text)
            {
                text.Glyphs = glyphs;
                text.Text = "A";
            }
            else if (component is WireframeSymbol symbol)
            {
                symbol.Glyphs = glyphs;
            }
            owner.SetActive(true);
            return component;
        }

        private static Vector3[] Bake(WireframeShape component)
        {
            return BakeShape(component.SharedContainer.Container, component.Shape);
        }
    }
}
