using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Hiding shapes and containers without disposing them, and shapes that change their vertex count.</summary>
    public class VisibilityTests : WireframesTestBase
    {
        [Test]
        public void HiddenShape_LeavesTheDrawnEdgesAndComesBackAsItIsThen()
        {
            WireframeContainer container = CreateContainer();
            ILine hidden = container.CreateLine(Vector3.zero, Vector3.right);
            ILine kept = container.CreateLine(Vector3.up, Vector3.one);
            container.Proxy.Flush();

            hidden.IsVisible = false;
            // Edited while hidden, drawn that way once shown.
            hidden.WorldPositionB = new Vector3(0f, 0f, 5f);
            container.Proxy.Flush();

            Assert.That(hidden.IsVisible, Is.False);
            Assert.That(ChunkOf(kept).Mesh.GetIndices(0), Has.Length.EqualTo(2));
            (Vector3 A, Vector3 B) keptEdge = BakeEdges(container, kept)[0];
            AssertApproximately(Vector3.up, keptEdge.A);
            AssertApproximately(Vector3.one, keptEdge.B);

            hidden.IsVisible = true;

            Assert.That(hidden.IsVisible, Is.True);
            (Vector3 A, Vector3 B) shownEdge = BakeEdges(container, hidden)[0];
            AssertApproximately(Vector3.zero, shownEdge.A);
            AssertApproximately(new Vector3(0f, 0f, 5f), shownEdge.B);
            Assert.That(ChunkOf(kept).Mesh.GetIndices(0), Has.Length.EqualTo(4));
        }

        [Test]
        public void HidingAndShowing_AllocatesNothing()
        {
            WireframeContainer container = CreateContainer();
            ISphere sphere = container.CreateSphere(Vector3.zero, 1f);
            // Warms up every path once.
            sphere.IsVisible = false;
            sphere.IsVisible = true;
            container.Proxy.Flush();

            Assert.That(() =>
            {
                sphere.IsVisible = false;
                container.Proxy.Flush();
                sphere.IsVisible = true;
                container.Proxy.Flush();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void HiddenShapes_SurviveCompactionAndDisposal()
        {
            WireframeContainer container = CreateContainer();
            ILine[] lines = new ILine[3000];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = container.CreateLine(new Vector3(i, 0f, 0f), new Vector3(i, 1f, 0f));
            }
            container.Proxy.Flush();
            ILine hiddenKept = lines[2999];
            ILine hiddenDisposed = lines[2998];
            hiddenKept.IsVisible = false;
            hiddenDisposed.IsVisible = false;

            // Freeing most of the chunk compacts it, which moves the hidden line's vertices.
            for (int i = 0; i < 2998; i++)
            {
                lines[i].Dispose();
            }
            hiddenDisposed.Dispose();
            container.Proxy.Flush();
            Assert.That(ChunkOf(hiddenKept).Allocator.FreeCount, Is.Zero, "The chunk wasn't compacted.");
            Assert.That(ChunkOf(hiddenKept).Mesh.GetIndices(0), Is.Empty);

            hiddenKept.IsVisible = true;

            (Vector3 A, Vector3 B) edge = BakeEdges(container, hiddenKept)[0];
            AssertApproximately(new Vector3(2999f, 0f, 0f), edge.A);
            AssertApproximately(new Vector3(2999f, 1f, 0f), edge.B);
        }

        [Test]
        public void Visibility_ThrowsOnceDisposed()
        {
            WireframeContainer container = CreateContainer();
            ILine line = container.CreateLine();
            line.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = line.IsVisible);
            Assert.Throws<ObjectDisposedException>(() => line.IsVisible = false);
            container.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _ = container.IsVisible);
            Assert.Throws<ObjectDisposedException>(() => container.IsVisible = false);
        }

        [Test]
        public void ResizedShape_KeepsItsIdentityAndItsNeighbors()
        {
            WireframeContainer container = CreateContainer();
            ILine before = container.CreateLine(Vector3.zero, Vector3.one);
            ChainShape chain = container.Proxy.Add(new ChainShape(Points(2, 0f)));
            ILine after = container.CreateLine(Vector3.one, Vector3.one * 2f);
            container.Proxy.Flush();

            chain.SetPoints(Points(10, 5f));
            AssertChain(container, chain, 10, 5f);
            chain.SetPoints(Points(3, 7f));
            AssertChain(container, chain, 3, 7f);

            AssertApproximately(Vector3.one, BakeEdges(container, before)[0].B);
            AssertApproximately(Vector3.one * 2f, BakeEdges(container, after)[0].B);
            Assert.That(ShapeCountOf(container), Is.EqualTo(3));
        }

        [Test]
        public void ResizedShape_MovesToAChunkWithRoomAndBack()
        {
            WireframeContainer container = CreateContainer();
            ChainShape chain = container.Proxy.Add(new ChainShape(Points(4, 0f)));
            MeshChunk first = ChunkOf(chain);

            chain.SetPoints(Points(MeshChunk.MaxVertexCount + 100, 1f));
            Assert.That(ChunkOf(chain).IsLarge, Is.True);
            AssertChain(container, chain, MeshChunk.MaxVertexCount + 100, 1f);

            chain.SetPoints(Points(4, 2f));
            Assert.That(ChunkOf(chain), Is.SameAs(first));
            AssertChain(container, chain, 4, 2f);
        }

        [Test]
        public void ResizingWithoutFlushes_KeepsTheQueuesFromGrowing()
        {
            WireframeContainer container = CreateContainer();
            ChainShape chain = container.Proxy.Add(new ChainShape(Points(4, 0f)));
            MeshChunk first = ChunkOf(chain);
            int capacity = first.PendingCapacity;
            Vector3[] large = Points(MeshChunk.MaxVertexCount + 10, 1f);
            Vector3[] small = Points(4, 2f);

            // Back and forth between the chunk and a large one, with no render to empty the queues.
            for (int i = 0; i < 200; i++)
            {
                chain.SetPoints(i % 2 == 0 ? large : small);
            }

            Assert.That(first.PendingCapacity, Is.EqualTo(capacity));
            Assert.That(container.Proxy.Chunks, Has.Count.EqualTo(2));
            AssertChain(container, chain, 4, 2f);
        }

        [Test]
        public void ResizedHiddenShape_StaysHiddenUntilShown()
        {
            WireframeContainer container = CreateContainer();
            ChainShape chain = container.Proxy.Add(new ChainShape(Points(3, 0f)));
            chain.IsVisible = false;

            chain.SetPoints(Points(6, 1f));
            container.Proxy.Flush();
            Assert.That(ChunkOf(chain).Mesh.GetIndices(0), Is.Empty);

            chain.IsVisible = true;
            AssertChain(container, chain, 6, 1f);
        }

        private static Vector3[] Points(int count, float height)
        {
            Vector3[] points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                points[i] = new Vector3(i, height, 0f);
            }
            return points;
        }

        private static void AssertChain(WireframeContainer container, ChainShape chain, int count, float height)
        {
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, chain);
            Assert.That(edges, Has.Length.EqualTo(count - 1));
            AssertApproximately(new Vector3(0f, height, 0f), edges[0].A);
            AssertApproximately(new Vector3(count - 1, height, 0f), edges[count - 2].B);
        }

        /// <summary>World-space points joined in a chain, whose number of points can change like a text's glyphs.</summary>
        private sealed class ChainShape : Shape
        {
            private Vector3[] _points;

            internal ChainShape(Vector3[] points) : base(points.Length, ChainEdges(points.Length))
            {
                _points = points;
            }

            internal void SetPoints(Vector3[] points)
            {
                EnsureUsable();
                _points = points;
                Resize(points.Length, ChainEdges(points.Length));
            }

            public override void SetColor(Color color)
            {
            }

            internal override void WritePositions(Span<Vector3> positions)
            {
                _points.AsSpan().CopyTo(positions);
            }

            internal override void WriteColors(Span<Color32> colors)
            {
                colors.Fill(new Color32(255, 255, 255, 255));
            }

            internal override void WriteBoneIndices(Span<float> boneIndices)
            {
                boneIndices.Fill(BoneRegistry.WorldSlot);
            }

            protected override void AcquireBones(BoneRegistry bones)
            {
            }

            protected override void ReleaseBones(BoneRegistry bones)
            {
            }

            private static int[] ChainEdges(int count)
            {
                int[] pattern = new int[(count - 1) * 2];
                for (int edge = 0; edge < count - 1; edge++)
                {
                    pattern[edge * 2] = edge;
                    pattern[edge * 2 + 1] = edge + 1;
                }
                return pattern;
            }
        }
    }
}
