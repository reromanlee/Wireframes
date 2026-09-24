using NUnit.Framework;

namespace reromanlee.Wireframes.Tests
{
    public class VertexAllocatorTests
    {
        [Test]
        public void Allocate_HandsOutConsecutiveBlocks()
        {
            VertexAllocator allocator = new();

            Assert.That(allocator.Allocate(2), Is.EqualTo(0));
            Assert.That(allocator.Allocate(8), Is.EqualTo(2));
            Assert.That(allocator.End, Is.EqualTo(10));
        }

        [Test]
        public void Allocate_ReusesFreedBlockOfSameSize()
        {
            VertexAllocator allocator = new();
            int first = allocator.Allocate(2);
            allocator.Allocate(2);

            allocator.Free(first, 2);

            Assert.That(allocator.FreeCount, Is.EqualTo(2));
            Assert.That(allocator.EndAfterAllocate(2), Is.EqualTo(4));
            Assert.That(allocator.Allocate(2), Is.EqualTo(first));
            Assert.That(allocator.FreeCount, Is.Zero);
            Assert.That(allocator.End, Is.EqualTo(4));
        }

        [Test]
        public void Allocate_IgnoresFreedBlocksOfOtherSizes()
        {
            VertexAllocator allocator = new();
            allocator.Free(allocator.Allocate(8), 8);

            Assert.That(allocator.EndAfterAllocate(2), Is.EqualTo(10));
            Assert.That(allocator.Allocate(2), Is.EqualTo(8));
            Assert.That(allocator.FreeCount, Is.EqualTo(8));
        }

        [Test]
        public void Reset_StartsOverFromVertexZero()
        {
            VertexAllocator allocator = new();
            allocator.Allocate(2);
            allocator.Free(allocator.Allocate(8), 8);

            allocator.Reset();

            Assert.That(allocator.End, Is.Zero);
            Assert.That(allocator.FreeCount, Is.Zero);
            Assert.That(allocator.Allocate(8), Is.EqualTo(0));
        }
    }
}
