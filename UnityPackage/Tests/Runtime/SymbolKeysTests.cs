using System;
using NUnit.Framework;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>The numbers symbols are found by, which generated enums save as their members' values.</summary>
    public class SymbolKeysTests
    {
        private enum Sample
        {
            None = 0,
            Heart = -1226833789
        }

        private enum NarrowSample : byte
        {
            None
        }

        [Test]
        public void Keys_AreFixedHashesOfTheirKeyword()
        {
            // Saved enum values depend on these, so the hash must never change between versions or platforms.
            Assert.That(SymbolKeys.Of("ArrowUp"), Is.EqualTo(154847355));
            Assert.That(SymbolKeys.Of("Heart"), Is.EqualTo(-1226833789));
            Assert.That(SymbolKeys.Of("Star"), Is.EqualTo(1365736337));
        }

        [Test]
        public void EnumMembers_StandForTheirValue()
        {
            Assert.That(SymbolKeys.Of(Sample.Heart), Is.EqualTo(SymbolKeys.Of("Heart")));
            Assert.That(SymbolKeys.Of(Sample.None), Is.EqualTo(SymbolKeys.None));
        }

        [Test]
        public void EnumsWithoutIntValues_AreRejected()
        {
            Assert.Throws<ArgumentException>(() => SymbolKeys.Of(NarrowSample.None));
        }

        [TestCase("ArrowUp")]
        [TestCase("_Hidden")]
        [TestCase("Arrow2")]
        [TestCase("Class")]
        public void Identifiers_CanBeKeywords(string keyword)
        {
            Assert.That(SymbolKeys.FindProblem(keyword), Is.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("2Up")]
        [TestCase("Arrow Up")]
        [TestCase("Arrow-Up")]
        [TestCase("Fl\u00E8che")]
        [TestCase("class")]
        [TestCase("None")]
        public void OtherNames_CantBeKeywords(string keyword)
        {
            Assert.That(SymbolKeys.FindProblem(keyword), Is.Not.Null);
        }
    }
}
