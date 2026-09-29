using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class MeshBufferTests : WireframesTestBase
    {
        [Test]
        public void DisposingShapes_KeepsRemainingEdgesOnTheirOwnVertices()
        {
            // Mixing sizes makes removals move edges between different owners.
            WireframeContainer container = CreateContainer();
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
        public void Edits_UploadOnlyWhatTheyChange()
        {
            WireframeContainer container = CreateContainer();
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            ILine[] lines = new ILine[100];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = container.CreateLine(bone, bone);
            }
            ISphere sphere = container.CreateSphere(bone, Vector3.zero, 1f);
            MeshChunk chunk = ChunkOf(container);
            container.Proxy.Flush();

            lines[50].ColorA = Color.red;
            container.Proxy.Flush();
            Assert.That(chunk.UploadedVertexCount, Is.EqualTo(2), "A color edit uploaded more than the line's colors.");
            Assert.That(chunk.UploadedIndexCount, Is.Zero);

            sphere.Radius = 2f;
            container.Proxy.Flush();
            Assert.That(chunk.UploadedVertexCount, Is.EqualTo(((Shape)sphere).VertexCount),
                "A radius edit uploaded more than the sphere's positions.");

            bone.position = Vector3.one;
            container.Proxy.Flush();
            Assert.That(chunk.UploadedVertexCount, Is.Zero, "Moving a bone uploaded vertices.");

            lines[20].IsVisible = false;
            container.Proxy.Flush();
            Assert.That(chunk.UploadedVertexCount, Is.Zero);
            Assert.That(chunk.UploadedIndexCount, Is.EqualTo(2), "Hiding a line uploaded more than the edge moved into its slot.");
        }

        [Test]
        public void FreedBlock_IsReusedByShapeOfSameSize()
        {
            WireframeContainer container = CreateContainer();
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
            WireframeContainer container = CreateContainer();
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
            MeshChunk chunk = ChunkOf(container);
            VertexAllocator allocator = chunk.Allocator;
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
            Assert.That(chunk.Mesh.GetIndices(0), Has.Length.EqualTo(1000));
        }

        [Test]
        public void Compaction_ShrinksBuffersToTwiceWhatLiveShapesUse()
        {
            WireframeContainer container = CreateContainer();
            List<ILine> lines = new();
            for (int i = 0; i < 20000; i++)
            {
                lines.Add(container.CreateLine(new Vector3(i, 0f, 0f), new Vector3(i, 1f, 0f)));
            }
            container.Proxy.Flush();
            MeshChunk chunk = ChunkOf(container);
            Assert.That(chunk.VertexCapacity, Is.EqualTo(65535));

            // A fifth of the lines stay, scattered through the buffer: 8000 vertices and 4000 edges.
            for (int i = 0; i < lines.Count; i++)
            {
                if (i % 5 != 0)
                {
                    lines[i].Dispose();
                }
            }
            Vector3[] baked = FlushAndBake(container);

            Assert.That(chunk.VertexCapacity, Is.EqualTo(16000));
            Assert.That(chunk.EdgeCapacity, Is.EqualTo(8000));
            Assert.That(chunk.Mesh.vertexCount, Is.EqualTo(16000));
            Assert.That(chunk.Mesh.GetIndices(0), Has.Length.EqualTo(8000));
            for (int i = 0; i < lines.Count; i += 5)
            {
                int start = ((Line)lines[i]).VertexStart;
                AssertApproximately(new Vector3(i, 0f, 0f), baked[start]);
                AssertApproximately(new Vector3(i, 1f, 0f), baked[start + 1]);
            }
        }

        [Test]
        public void ManyBones_GrowTheBoneTextureWithEveryMatrix()
        {
            // More bones than one texture row holds, so the texture grows while the matrices stay in their slots.
            WireframeContainer container = CreateContainer();
            List<ILine> lines = new();
            for (int i = 0; i < BoneTexture.BonesPerRow + 44; i++)
            {
                ILine line = container.CreateLine();
                line.BoneA = CreateBone(new Vector3(i, 0f, 0f), Quaternion.identity);
                line.LocalPositionA = Vector3.zero;
                lines.Add(line);
            }

            Vector3[] baked = FlushAndBake(container);

            Texture2D texture = container.Proxy.BoneTexture.Texture;
            Assert.That(container.Proxy.Bones.Count, Is.EqualTo(lines.Count));
            Assert.That(texture.height, Is.GreaterThanOrEqualTo(2));
            for (int i = 0; i < lines.Count; i++)
            {
                AssertApproximately(new Vector3(i, 0f, 0f), baked[((Line)lines[i]).VertexStart]);
            }
            // The translation of the last bone, read back from where the shader finds it.
            int slot = lines.Count;
            int texel = slot / BoneTexture.BonesPerRow * BoneTexture.BonesPerRow * 3 + slot % BoneTexture.BonesPerRow * 3;
            Unity.Collections.NativeArray<Vector4> texels = texture.GetPixelData<Vector4>(0);
            AssertApproximately(lines.Count - 1f, texels[texel].w);
        }
    }
}
