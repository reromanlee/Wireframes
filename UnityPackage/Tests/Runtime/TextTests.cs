using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Texts created in containers, drawn with the glyphs of <see cref="GlyphTestBase.CreateTestGlyphs"/>.</summary>
    public class TextTests : GlyphTestBase
    {
        [Test]
        public void Text_LiesInTheXYPlaneOfItsBone()
        {
            WireframeContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 90f, 0f));

            IText text = CreateTestText(container, "A", bone);

            // A runs from -0.3 to 0.3 across the text, 0.25 below its center, and the bone turns +X to -Z.
            (Vector3 A, Vector3 B) edge = BakeEdges(container, text)[0];
            AssertApproximately(new Vector3(1f, 1.75f, 3.3f), edge.A);
            AssertApproximately(new Vector3(1f, 1.75f, 2.7f), edge.B);
        }

        [Test]
        public void ChangedText_IsRewrittenWhereItIs()
        {
            WireframeContainer container = CreateContainer();
            ILine before = container.CreateLine(Vector3.zero, Vector3.one);
            IText text = CreateTestText(container, "A");
            ILine after = container.CreateLine(Vector3.one, Vector3.one * 2f);
            container.Proxy.Flush();
            int vertexStart = ((Shape)text).VertexStart;

            text.Text = "AB";

            Assert.That(((Shape)text).VertexStart, Is.EqualTo(vertexStart));
            Assert.That(BakeEdges(container, text), Has.Length.EqualTo(1 + 4));
            Assert.That(ChunkOf(text).Mesh.GetIndices(0), Has.Length.EqualTo((1 + 1 + 4 + 1) * 2));
            AssertApproximately(Vector3.one, BakeEdges(container, before)[0].B);
            AssertApproximately(Vector3.one * 2f, BakeEdges(container, after)[0].B);
        }

        [Test]
        public void TextThatOutgrowsItsRoom_MovesAndKeepsDrawing()
        {
            WireframeContainer container = CreateContainer();
            IText text = CreateTestText(container, "A");
            Assert.That(((Shape)text).VertexCount, Is.EqualTo(16));

            text.Text = new string('B', 20);

            Assert.That(((Shape)text).VertexCount, Is.EqualTo(128));
            Assert.That(BakeEdges(container, text), Has.Length.EqualTo(20 * 4));

            text.Text = "A";

            Assert.That(((Shape)text).VertexCount, Is.EqualTo(16));
            Assert.That(BakeEdges(container, text), Has.Length.EqualTo(1));
        }

        [Test]
        public void SettingTheTextEveryFrame_AllocatesNothing()
        {
            WireframeContainer container = CreateContainer();
            IText text = CreateTestText(container, "A");
            char[] characters = { 'A', 'B', 'B', 'A', 'B' };

            void ChangeTheTextBackAndForth()
            {
                text.SetText(characters.AsSpan(0, 5));
                container.Proxy.Flush();
                text.SetText(characters.AsSpan(0, 2));
                container.Proxy.Flush();
            }

            // Warms up every path, and lets each buffer grow once to what the texts need.
            ChangeTheTextBackAndForth();
            ChangeTheTextBackAndForth();

            Assert.That(ChangeTheTextBackAndForth, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void Settings_MoveThePointsAndKeepTheEdges()
        {
            WireframeContainer container = CreateContainer();
            IText text = CreateTestText(container, "A");
            container.Proxy.Flush();
            int firstSlot = ((Shape)text).GetEdgeSlot(0);

            text.HorizontalAlignment = WireframeHorizontalAlignment.Left;
            text.VerticalAlignment = WireframeVerticalAlignment.Top;
            text.CharacterSize = 0.5f;

            (Vector3 A, Vector3 B) edge = BakeEdges(container, text)[0];
            Assert.That(((Shape)text).GetEdgeSlot(0), Is.EqualTo(firstSlot));
            // The default bounds are 4 by 1, so the top left corner is at (-2, 0.5).
            AssertApproximately(new Vector3(-2f, 0.5f - 0.5f + Baseline * 0.5f, 0f), edge.A);
            AssertApproximately(new Vector3(-2f + 0.3f, 0.5f - 0.5f + Baseline * 0.5f, 0f), edge.B);
        }

        [Test]
        public void TextRead_ReturnsWhatWasSetAndCreatesItOnce()
        {
            WireframeContainer container = CreateContainer();
            IText text = CreateTestText(container, "AB");
            Assert.That(text.Text, Is.EqualTo("AB"));

            text.SetText("BA".AsSpan());

            string read = text.Text;
            Assert.That(read, Is.EqualTo("BA"));
            Assert.That(text.Text, Is.SameAs(read));
            text.Text = null;
            Assert.That(text.Text, Is.Empty);
        }

        [Test]
        public void NewBone_KeepsTheSizeOfTheTextInTheWorld()
        {
            WireframeContainer container = CreateContainer();
            IText text = CreateTestText(container, "A");
            Transform scaled = CreateBone(Vector3.zero, Quaternion.identity, 2f);

            text.Bone = scaled;

            AssertApproximately(0.5f, text.CharacterSize);
            AssertApproximately(2f, text.Bounds.x);
            AssertApproximately(new Vector3(0.3f, -0.25f, 0f), BakeEdges(container, text)[0].B);
        }

        [Test]
        public void ValuesOutsideTheirEnums_Throw()
        {
            WireframeContainer container = CreateContainer();
            IText text = CreateTestText(container, "A");

            Assert.Throws<ArgumentOutOfRangeException>(() => text.CharacterWidth = (WireframeCharacterWidth)2);
            Assert.Throws<ArgumentOutOfRangeException>(() => text.HorizontalAlignment = (WireframeHorizontalAlignment)3);
            Assert.Throws<ArgumentOutOfRangeException>(() => text.VerticalAlignment = (WireframeVerticalAlignment)(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => text.Overflow = (WireframeTextOverflow)2);
        }

        [Test]
        public void DisposedText_Throws()
        {
            WireframeContainer container = CreateContainer();
            IText text = CreateTestText(container, "A");
            text.Dispose();

            Assert.Throws<ObjectDisposedException>(() => text.Text = "B");
            Assert.Throws<ObjectDisposedException>(() => text.SetText("B".AsSpan()));
            Assert.Throws<ObjectDisposedException>(() => _ = text.Glyphs);
        }

        private IText CreateTestText(WireframeContainer container, string text, Transform bone = null)
        {
            return container.Proxy.Add(new GlyphText(
                bone, Vector3.zero, Quaternion.identity, text, CreateTestGlyphs(), TextSettings.Default));
        }
    }
}
