using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Symbols created in containers, drawn with the glyphs of <see cref="GlyphTestBase.CreateTestGlyphs"/>.</summary>
    public class SymbolTests : GlyphTestBase
    {
        private enum TestSymbols
        {
            None = 0,
            Heart = -1226833789,
            Star = 1365736337,
            Moon = 42
        }

        private enum NarrowSymbols : byte
        {
            None
        }

        [Test]
        public void Symbol_IsCenteredOnItsPositionInItsXYPlane()
        {
            WireframeContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(1f, 0f, 0f), Quaternion.identity);

            ISymbol symbol = CreateTestSymbol(container, TestSymbols.Heart, bone);
            symbol.Size = 2f;

            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, symbol);
            Assert.That(edges, Has.Length.EqualTo(3));
            AssertApproximately(new Vector3(0f, -1f, 0f), edges[0].A);
            AssertApproximately(new Vector3(2f, -1f, 0f), edges[1].A);
            AssertApproximately(new Vector3(1f, 1f, 0f), edges[2].A);
            AssertApproximately(new Vector3(0f, -1f, 0f), edges[2].B);
        }

        [Test]
        public void ChangedSymbol_IsDrawnInstead()
        {
            WireframeContainer container = CreateContainer();
            ISymbol symbol = CreateTestSymbol(container, TestSymbols.Heart);

            symbol.SetSymbol(TestSymbols.Star);

            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, symbol);
            Assert.That(edges, Has.Length.EqualTo(1));
            AssertApproximately(new Vector3(-0.5f, 0f, 0f), edges[0].A);
            AssertApproximately(new Vector3(0.5f, 0f, 0f), edges[0].B);
            Assert.That(symbol.GetSymbol<TestSymbols>(), Is.EqualTo(TestSymbols.Star));
        }

        [Test]
        public void MissingSymbol_IsDrawnAsAQuestionMarkWithOneWarning()
        {
            WireframeContainer container = CreateContainer();
            LogAssert.Expect(LogType.Warning, new Regex("A symbol isn't in the glyphs"));

            ISymbol symbol = CreateTestSymbol(container, TestSymbols.Heart);
            symbol.SetSymbol(TestSymbols.Moon);
            symbol.Size = 2f;

            LogAssert.NoUnexpectedReceived();
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, symbol);
            Assert.That(edges, Has.Length.EqualTo(1));
            AssertApproximately(new Vector3(-0.2f, -0.5f, 0f), edges[0].A);
        }

        [Test]
        public void NoneSymbol_DrawsNothingWithoutWarning()
        {
            WireframeContainer container = CreateContainer();

            ISymbol symbol = CreateTestSymbol(container, TestSymbols.None);

            Assert.That(BakeEdges(container, symbol), Is.Empty);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void NewBone_KeepsTheSizeOfTheSymbolInTheWorld()
        {
            WireframeContainer container = CreateContainer();
            ISymbol symbol = CreateTestSymbol(container, TestSymbols.Star);
            Transform scaled = CreateBone(Vector3.zero, Quaternion.identity, 2f);

            symbol.Bone = scaled;

            AssertApproximately(0.5f, symbol.Size);
            AssertApproximately(new Vector3(0.5f, 0f, 0f), BakeEdges(container, symbol)[0].B);
        }

        [Test]
        public void EnumsWithoutIntValues_AreRejected()
        {
            WireframeContainer container = CreateContainer();
            ISymbol symbol = CreateTestSymbol(container, TestSymbols.Heart);

            Assert.Throws<ArgumentException>(() => symbol.SetSymbol(NarrowSymbols.None));
            Assert.Throws<ArgumentException>(() => container.CreateSymbol(NarrowSymbols.None));
        }

        private ISymbol CreateTestSymbol(WireframeContainer container, TestSymbols symbol, Transform bone = null)
        {
            return container.Proxy.Add(new GlyphSymbol(
                bone, Vector3.zero, Quaternion.identity, SymbolKeys.Of(symbol), CreateTestGlyphs(), ShapeDefaults.Size));
        }
    }
}
