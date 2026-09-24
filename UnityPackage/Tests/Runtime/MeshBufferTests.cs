using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace reromanlee.Wireframes.Tests
{
    public class MeshBufferTests : WireframesTestBase
    {
        [Test]
        public void DisposingShapes_KeepsRemainingEdgesOnTheirOwnVertices()
        {
            // Mixing sizes makes removals move edges between different owners.
            LineContainer container = CreateContainer();
            List<IShape> shapes = new();
            for (int i = 0; i < 30; i++)
            {
                Vector3 origin = new(i * 3f, 0f, 0f);
                shapes.Add(i % 3 == 0
                    ? container.CreateBox(origin, origin + Vector3.one)
                    : container.CreateLine(origin, origin + Vector3.up));
            }
            container.Proxy.Flush();
            for (int i = 0; i < shapes.Count; i += 2)
            {
                shapes[i].Dispose();
            }

            Vector3[] baked = FlushAndBake(container);
            int[] indices = ChunkOf(container).Mesh.GetIndices(0);

            int expectedEdges = 0;
            for (int i = 1; i < shapes.Count; i += 2)
            {
                Shape shape = (Shape)shapes[i];
                expectedEdges += shape.EdgeCount;
                for (int edge = 0; edge < shape.EdgeCount; edge++)
                {
                    int slot = shape.GetEdgeSlot(edge);
                    Assert.That(indices[slot * 2], Is.InRange(shape.VertexStart, shape.VertexStart + shape.VertexCount - 1));
                    Assert.That(indices[slot * 2 + 1], Is.InRange(shape.VertexStart, shape.VertexStart + shape.VertexCount - 1));
                }
                if (shape is ILine line)
                {
                    int slot = shape.GetEdgeSlot(0);
                    AssertApproximately(line.WorldPositionA, baked[indices[slot * 2]]);
                    AssertApproximately(line.WorldPositionB, baked[indices[slot * 2 + 1]]);
                }
            }
            Assert.That(indices, Has.Length.EqualTo(expectedEdges * 2));
        }

        [Test]
        public void FreedBlock_IsReusedByShapeOfSameSize()
        {
            LineContainer container = CreateContainer();
            ILine first = container.CreateLine();
            container.CreateLine();
            int start = ((Line)first).VertexStart;

            first.Dispose();
            ILine replacement = container.CreateLine(Vector3.one, Vector3.one * 2f);

            Assert.That(((Line)replacement).VertexStart, Is.EqualTo(start));
            Vector3[] baked = FlushAndBake(container);
            AssertApproximately(Vector3.one, baked[start]);
        }

        [Test]
        public void FreedBlocks_AreCompactedOnceTheyFillHalfTheBuffer()
        {
            LineContainer container = CreateContainer();
            List<ILine> lines = new();
            for (int i = 0; i < 2000; i++)
            {
                lines.Add(container.CreateLine(new Vector3(i, 0f, 0f), new Vector3(i, 1f, 0f)));
            }
            container.Proxy.Flush();
            for (int i = 0; i < 1500; i++)
            {
                lines[i].Dispose();
            }
            VertexAllocator allocator = ChunkOf(container).Allocator;
            Assert.That(allocator.FreeCount, Is.EqualTo(3000));

            Vector3[] baked = FlushAndBake(container);

            Assert.That(allocator.FreeCount, Is.Zero);
            Assert.That(allocator.End, Is.EqualTo(1000));
            for (int i = 1500; i < 2000; i++)
            {
                int start = ((Line)lines[i]).VertexStart;
                AssertApproximately(new Vector3(i, 0f, 0f), baked[start]);
                AssertApproximately(new Vector3(i, 1f, 0f), baked[start + 1]);
            }
            Assert.That(ChunkOf(container).Mesh.GetIndices(0), Has.Length.EqualTo(1000));
        }

        [Test]
        public void ManyVertices_SwitchTheIndexBufferTo32Bit()
        {
            LineContainer container = CreateContainer();
            container.Proxy.Flush();
            Assert.That(ChunkOf(container).Mesh.indexFormat, Is.EqualTo(IndexFormat.UInt16));

            ILine last = null;
            for (int i = 0; i < 40000; i++)
            {
                last = container.CreateLine(new Vector3(i, 0f, 0f), new Vector3(i, 1f, 0f));
            }
            Vector3[] baked = FlushAndBake(container);

            Mesh mesh = ChunkOf(container).Mesh;
            Assert.That(mesh.indexFormat, Is.EqualTo(IndexFormat.UInt32));
            Assert.That(mesh.GetIndexCount(0), Is.EqualTo(80000u));
            int start = ((Line)last).VertexStart;
            AssertApproximately(new Vector3(39999f, 0f, 0f), baked[start]);
            AssertApproximately(new Vector3(39999f, 1f, 0f), baked[start + 1]);
        }

        [Test]
        public void ManyBones_GrowTheBoneArrayWithMatchingBindposes()
        {
            LineContainer container = CreateContainer();
            List<ILine> lines = new();
            for (int i = 0; i < 100; i++)
            {
                ILine line = container.CreateLine();
                line.BoneA = CreateBone(new Vector3(i, 0f, 0f), Quaternion.identity);
                line.LocalPositionA = Vector3.zero;
                lines.Add(line);
            }

            Vector3[] baked = FlushAndBake(container);

            MeshChunk chunk = ChunkOf(container);
            Assert.That(chunk.Renderer.bones, Has.Length.EqualTo(chunk.Mesh.bindposes.Length));
            Assert.That(chunk.Bones.Count, Is.EqualTo(100));
            for (int i = 0; i < lines.Count; i++)
            {
                AssertApproximately(new Vector3(i, 0f, 0f), baked[((Line)lines[i]).VertexStart]);
            }
        }
    }
}
