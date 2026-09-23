using NUnit.Framework;

namespace reromanlee.Wireframes.Tests
{
    public class EdgeListTests
    {
        private sealed class Owner : IEdgeOwner
        {
            public readonly int[] Slots = new int[4];

            public void OnEdgeMoved(int edge, int slot)
            {
                Slots[edge] = slot;
            }
        }

        [Test]
        public void RemoveAt_MovesLastEdgeIntoFreedSlot()
        {
            EdgeList edges = new();
            Owner first = new();
            Owner second = new();
            first.Slots[0] = edges.Add(first, 0);
            edges.Set(first.Slots[0], 0, 1);
            second.Slots[0] = edges.Add(second, 0);
            edges.Set(second.Slots[0], 2, 3);
            second.Slots[1] = edges.Add(second, 1);
            edges.Set(second.Slots[1], 4, 5);

            Assert.That(edges.RemoveAt(first.Slots[0]), Is.True);

            Assert.That(edges.Count, Is.EqualTo(2));
            Assert.That(second.Slots[1], Is.EqualTo(0));
            Assert.That(edges.Indices[0], Is.EqualTo(4));
            Assert.That(edges.Indices[1], Is.EqualTo(5));
        }

        [Test]
        public void RemoveAt_LastSlotMovesNothing()
        {
            EdgeList edges = new();
            Owner owner = new();
            edges.Add(owner, 0);
            int last = edges.Add(owner, 1);

            Assert.That(edges.RemoveAt(last), Is.False);
            Assert.That(edges.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemovingEveryEdgeOfOneOwner_LeavesOtherOwnersIntact()
        {
            // Mirrors MeshChunk.Remove: an owner's later edge can be moved into one of its own earlier slots.
            EdgeList edges = new();
            Owner removed = new();
            Owner kept = new();
            for (int edge = 0; edge < 3; edge++)
            {
                removed.Slots[edge] = edges.Add(removed, edge);
            }
            kept.Slots[0] = edges.Add(kept, 0);
            edges.Set(kept.Slots[0], 7, 8);

            for (int edge = 0; edge < 3; edge++)
            {
                edges.RemoveAt(removed.Slots[edge]);
            }

            Assert.That(edges.Count, Is.EqualTo(1));
            Assert.That(kept.Slots[0], Is.EqualTo(0));
            Assert.That(edges.Indices[0], Is.EqualTo(7));
            Assert.That(edges.Indices[1], Is.EqualTo(8));
        }

        [Test]
        public void EnsureCapacity_DoublesAndKeepsEdges()
        {
            EdgeList edges = new();
            Owner owner = new();
            int slot = edges.Add(owner, 0);
            edges.Set(slot, 3, 4);
            int capacity = edges.Capacity;

            edges.EnsureCapacity(capacity + 1);

            Assert.That(edges.Capacity, Is.EqualTo(capacity * 2));
            Assert.That(edges.Indices.Length, Is.EqualTo(capacity * 4));
            Assert.That(edges.Indices[0], Is.EqualTo(3));
            Assert.That(edges.Indices[1], Is.EqualTo(4));
        }
    }
}
