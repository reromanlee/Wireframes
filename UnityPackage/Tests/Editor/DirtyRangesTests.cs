using NUnit.Framework;

namespace reromanlee.Wireframes.Tests
{
    public class DirtyRangesTests
    {
        [Test]
        public void Merge_SortsAndJoinsOverlappingOrNearbyRanges()
        {
            DirtyRanges ranges = new();
            ranges.Add(1000, 10);
            ranges.Add(0, 2);
            ranges.Add(1005, 10);
            ranges.Add(1030, 1);

            Assert.That(ranges.Merge(), Is.EqualTo(2));
            Assert.That(ranges[0].start, Is.EqualTo(0));
            Assert.That(ranges[0].length, Is.EqualTo(2));
            Assert.That(ranges[1].start, Is.EqualTo(1000));
            Assert.That(ranges[1].length, Is.EqualTo(31));
        }

        [Test]
        public void Merge_CollapsesManyScatteredRangesIntoOne()
        {
            DirtyRanges ranges = new();
            for (int i = 0; i < 40; i++)
            {
                ranges.Add(i * 1000, 2);
            }

            Assert.That(ranges.Merge(), Is.EqualTo(1));
            Assert.That(ranges[0].start, Is.EqualTo(0));
            Assert.That(ranges[0].length, Is.EqualTo(39002));
        }

        [Test]
        public void Add_IgnoresEmptyRanges()
        {
            DirtyRanges ranges = new();

            ranges.Add(5, 0);

            Assert.That(ranges.Count, Is.Zero);
        }
    }
}
