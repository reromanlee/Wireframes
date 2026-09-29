using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>How shapes spread over chunks, and how chunks are reserved, compacted and released.</summary>
    public class ChunkAllocatorTests : WireframesTestBase
    {
        // Two-vertex lines that fill a chunk up to its 16-bit limit.
        private const int LinesPerChunk = MeshChunk.MaxVertexCount / 2;
        // Any time works for the release clock; only differences count.
        private const float Now = 1000f;

        [Test]
        public void Shapes_FillAChunkToItsLimitBeforeStartingAnother()
        {
            WireframeContainer container = CreateContainer();
            List<ILine> lines = CreateLines(container, LinesPerChunk + 10);

            IReadOnlyList<MeshChunk> chunks = container.Proxy.Chunks;
            Assert.That(chunks, Has.Count.EqualTo(2));
            Assert.That(chunks[0].VertexCapacity, Is.EqualTo(MeshChunk.MaxVertexCount));
            Assert.That(chunks[0].ShapeCount, Is.EqualTo(LinesPerChunk));
            Assert.That(chunks[1].ShapeCount, Is.EqualTo(10));
            ILine last = lines[lines.Count - 1];
            Assert.That(ChunkOf(last), Is.SameAs(chunks[1]));
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, last);
            AssertApproximately(new Vector3(lines.Count - 1, 0f, 0f), edges[0].A);
            AssertApproximately(new Vector3(lines.Count - 1, 1f, 0f), edges[0].B);
            foreach (MeshChunk chunk in chunks)
            {
                Assert.That(chunk.Mesh.indexFormat, Is.EqualTo(IndexFormat.UInt16));
            }
        }

        [Test]
        public void ShapeBiggerThanAChunk_GetsALargeChunkOfItsOwn()
        {
            WireframeContainer container = CreateContainer();
            Vector3[] points = new Vector3[MeshChunk.MaxVertexCount + 1000];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector3(i, 0f, 0f);
            }

            IPolyline polyline = container.CreatePolyline(points);
            ILine line = container.CreateLine();

            MeshChunk large = ChunkOf(polyline);
            Assert.That(large.IsLarge, Is.True);
            Assert.That(large.VertexCapacity, Is.EqualTo(points.Length));
            Assert.That(ChunkOf(line), Is.Not.SameAs(large));
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, polyline);
            AssertApproximately(points[points.Length - 2], edges[edges.Length - 1].A);
            AssertApproximately(points[points.Length - 1], edges[edges.Length - 1].B);
            Assert.That(large.Mesh.indexFormat, Is.EqualTo(IndexFormat.UInt32));
            Assert.That(ChunkOf(line).Mesh.indexFormat, Is.EqualTo(IndexFormat.UInt16));
        }

        [Test]
        public void FreedRoomInAnOlderChunk_IsUsedBeforeANewerOneGrows()
        {
            WireframeContainer container = CreateContainer();
            List<ILine> lines = CreateLines(container, LinesPerChunk + 1);
            lines[100].Dispose();

            ILine replacement = container.CreateLine();

            Assert.That(ChunkOf(replacement), Is.SameAs(container.Proxy.Chunks[0]));
        }

        [UnityTest]
        public IEnumerator EmptyChunk_IsReleasedOnceItStayedEmptyForTheDelay()
        {
            WireframeContainer container = CreateContainer();
            List<ILine> lines = CreateLines(container, LinesPerChunk + 10);
            MeshChunk second = container.Proxy.Chunks[1];
            for (int i = LinesPerChunk; i < lines.Count; i++)
            {
                lines[i].Dispose();
            }
            ChunkAllocator allocator = container.Proxy.ChunkAllocator;

            allocator.ReleaseIdleChunks(Now);
            allocator.ReleaseIdleChunks(Now + ChunkAllocator.ReleaseDelay - 0.5f);
            Assert.That(container.Proxy.Chunks, Has.Count.EqualTo(2));

            allocator.ReleaseIdleChunks(Now + ChunkAllocator.ReleaseDelay);

            Assert.That(container.Proxy.Chunks, Has.Count.EqualTo(1));
            Assert.That(second.Renderer.enabled, Is.False);
            yield return null;
            Assert.That(second.Renderer == null, Is.True, "The released chunk's GameObject still exists.");
            Assert.That(second.Mesh == null, Is.True, "The released chunk's mesh still exists.");
        }

        [Test]
        public void EmptyChunk_ThatGetsAShapeInTime_IsKept()
        {
            WireframeContainer container = CreateContainer();
            List<ILine> lines = CreateLines(container, LinesPerChunk + 10);
            for (int i = LinesPerChunk; i < lines.Count; i++)
            {
                lines[i].Dispose();
            }
            ChunkAllocator allocator = container.Proxy.ChunkAllocator;
            allocator.ReleaseIdleChunks(Now);

            // The first chunk is full, so the new line goes into the empty one.
            ILine line = container.CreateLine();
            allocator.ReleaseIdleChunks(Now + ChunkAllocator.ReleaseDelay * 2f);

            Assert.That(container.Proxy.Chunks, Has.Count.EqualTo(2));
            Assert.That(ChunkOf(line), Is.SameAs(container.Proxy.Chunks[1]));
        }

        [Test]
        public void ReservedCapacity_IsSplitEvenlyAndNeverReleased()
        {
            WireframeContainer container = CreateContainer(
                new WireframeContainerSettings { VertexCapacity = 100000, EdgeCapacity = 60000 });
            IReadOnlyList<MeshChunk> chunks = container.Proxy.Chunks;

            Assert.That(chunks, Has.Count.EqualTo(2));
            foreach (MeshChunk chunk in chunks)
            {
                Assert.That(chunk.VertexCapacity, Is.EqualTo(50000));
                Assert.That(chunk.EdgeCapacity, Is.EqualTo(30000));
            }
            container.Proxy.ChunkAllocator.ReleaseIdleChunks(Now);
            container.Proxy.ChunkAllocator.ReleaseIdleChunks(Now + ChunkAllocator.ReleaseDelay * 2f);
            Assert.That(chunks, Has.Count.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator Compaction_RunsForOneChunkPerFrame()
        {
            WireframeContainer container = CreateContainer();
            List<ILine> lines = CreateLines(container, LinesPerChunk + 4000);
            container.Proxy.Flush();
            // Three lines in five go, in both chunks.
            for (int i = 0; i < lines.Count; i++)
            {
                if (i % 5 < 3)
                {
                    lines[i].Dispose();
                }
            }
            MeshChunk first = container.Proxy.Chunks[0];
            MeshChunk second = container.Proxy.Chunks[1];
            Assert.That(first.NeedsCompaction, Is.True);
            Assert.That(second.NeedsCompaction, Is.True);

            container.Proxy.Flush();
            container.Proxy.Flush();
            Assert.That(first.NeedsCompaction, Is.False);
            Assert.That(second.NeedsCompaction, Is.True);

            yield return null;
            container.Proxy.Flush();
            Assert.That(second.NeedsCompaction, Is.False);
            Vector3[] firstVertices = FlushAndBake(container, first);
            Vector3[] secondVertices = FlushAndBake(container, second);
            for (int i = 3; i < lines.Count; i += 5)
            {
                Line line = (Line)lines[i];
                Vector3[] vertices = line.Chunk == first ? firstVertices : secondVertices;
                AssertApproximately(new Vector3(i, 0f, 0f), vertices[line.VertexStart]);
            }
        }

        [Test]
        public void Dispose_DisposesTheShapesOfEveryChunk()
        {
            WireframeContainer container = CreateContainer();
            List<ILine> lines = CreateLines(container, LinesPerChunk + 1);

            container.Dispose();

            Assert.That(lines[0].IsDisposed, Is.True);
            Assert.That(lines[lines.Count - 1].IsDisposed, Is.True);
        }

        private static List<ILine> CreateLines(WireframeContainer container, int count)
        {
            List<ILine> lines = new(count);
            for (int i = 0; i < count; i++)
            {
                lines.Add(container.CreateLine(new Vector3(i, 0f, 0f), new Vector3(i, 1f, 0f)));
            }
            return lines;
        }
    }
}
