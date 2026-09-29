using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Tests
{
    public abstract class WireframesTestBase
    {
        private const float Tolerance = 1e-4f;

        private readonly List<WireframeContainer> _containers = new();
        private readonly List<Object> _objects = new();

        [TearDown]
        public void DestroyTestObjects()
        {
            foreach (WireframeContainer container in _containers)
            {
                container.Dispose();
            }
            // Right away, because tests without a yield share one frame and would otherwise see each other's objects,
            // and newest first, so nothing is destroyed while an object created after it still uses it.
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                if (_objects[i] != null)
                {
                    Object.DestroyImmediate(_objects[i]);
                }
            }
            _containers.Clear();
            _objects.Clear();
        }

        protected WireframeContainer CreateContainer(WireframeContainerSettings settings = null)
        {
            WireframeContainer container = new(settings);
            _containers.Add(container);
            return container;
        }

        protected Transform CreateBone(Vector3 position, Quaternion rotation, float scale = 1f, Transform parent = null)
        {
            GameObject bone = new("Bone");
            _objects.Add(bone);
            bone.transform.SetParent(parent, false);
            bone.transform.SetPositionAndRotation(position, rotation);
            bone.transform.localScale = Vector3.one * scale;
            return bone.transform;
        }

        protected T Track<T>(T target) where T : Object
        {
            _objects.Add(target);
            return target;
        }

        private protected static MeshChunk ChunkOf(WireframeContainer container)
        {
            return container.Proxy.Chunks[0];
        }

        /// <summary>
        /// Applies pending edits and reads the bones, then skins the uploaded vertices on the CPU with the matrices the
        /// shader receives, and returns world-space vertices. ShaderTests checks that the GPU draws the same.
        /// </summary>
        protected static Vector3[] FlushAndBake(WireframeContainer container)
        {
            container.Proxy.Flush();
            MeshChunk chunk = ChunkOf(container);
            BoneRegistry bones = container.Proxy.Bones;
            Vector3[] positions = chunk.Positions;
            float[] boneIndices = chunk.BoneIndices;
            Vector3[] vertices = new Vector3[positions.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = bones.MatrixOf((int)boneIndices[i]).MultiplyPoint3x4(positions[i]);
            }
            return vertices;
        }

        /// <summary>Applies pending edits, then skins the mesh on the CPU and returns the shape's world-space vertices.</summary>
        protected static Vector3[] BakeShape(WireframeContainer container, IShape shape)
        {
            Vector3[] baked = FlushAndBake(container);
            Shape target = (Shape)shape;
            Vector3[] vertices = new Vector3[target.VertexCount];
            Array.Copy(baked, target.VertexStart, vertices, 0, vertices.Length);
            return vertices;
        }

        /// <summary>Applies pending edits, then skins the mesh on the CPU and returns the world-space ends of the shape's edges.</summary>
        protected static (Vector3 A, Vector3 B)[] BakeEdges(WireframeContainer container, IShape shape)
        {
            Vector3[] baked = FlushAndBake(container);
            int[] indices = ChunkOf(container).Mesh.GetIndices(0);
            Shape target = (Shape)shape;
            (Vector3 A, Vector3 B)[] edges = new (Vector3, Vector3)[target.EdgeCount];
            for (int edge = 0; edge < edges.Length; edge++)
            {
                int slot = target.GetEdgeSlot(edge);
                edges[edge] = (baked[indices[slot * 2]], baked[indices[slot * 2 + 1]]);
            }
            return edges;
        }

        protected static void AssertApproximately(Vector3 expected, Vector3 actual)
        {
            Assert.That(Vector3.Distance(expected, actual), Is.LessThan(Tolerance), $"Expected {expected:F4}, got {actual:F4}");
        }

        protected static void AssertApproximately(float expected, float actual)
        {
            Assert.That(actual, Is.EqualTo(expected).Within(Tolerance));
        }

        protected static void AssertApproximately(Quaternion expected, Quaternion actual)
        {
            // q and -q are the same rotation. |dot| is the cosine of half the angle between them, so it needs a
            // tighter tolerance than positions do.
            float alignment = Mathf.Abs(Quaternion.Dot(expected, actual));
            Assert.That(alignment, Is.EqualTo(1f).Within(1e-6f), $"Expected {expected:F4}, got {actual:F4}");
        }
    }
}
