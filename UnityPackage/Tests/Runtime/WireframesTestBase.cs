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

        private readonly List<LineContainer> _containers = new();
        private readonly List<Object> _objects = new();

        [TearDown]
        public void DestroyTestObjects()
        {
            foreach (LineContainer container in _containers)
            {
                container.Dispose();
            }
            foreach (Object target in _objects)
            {
                if (target != null)
                {
                    Object.Destroy(target);
                }
            }
            _containers.Clear();
            _objects.Clear();
        }

        protected LineContainer CreateContainer(Material material = null)
        {
            LineContainer container = material == null ? new LineContainer() : new LineContainer(material);
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

        private protected static MeshChunk ChunkOf(LineContainer container)
        {
            return container.Proxy.Chunks[0];
        }

        /// <summary>Applies pending edits, then skins the mesh on the CPU and returns world-space vertices.</summary>
        protected static Vector3[] FlushAndBake(LineContainer container)
        {
            container.Proxy.Flush();
            Mesh baked = new();
            ChunkOf(container).Renderer.BakeMesh(baked);
            Vector3[] vertices = baked.vertices;
            Object.Destroy(baked);
            return vertices;
        }

        /// <summary>Applies pending edits, then skins the mesh on the CPU and returns the shape's world-space vertices.</summary>
        protected static Vector3[] BakeShape(LineContainer container, IShape shape)
        {
            Vector3[] baked = FlushAndBake(container);
            Shape target = (Shape)shape;
            Vector3[] vertices = new Vector3[target.VertexCount];
            Array.Copy(baked, target.VertexStart, vertices, 0, vertices.Length);
            return vertices;
        }

        /// <summary>Applies pending edits, then skins the mesh on the CPU and returns the world-space ends of the shape's edges.</summary>
        protected static (Vector3 A, Vector3 B)[] BakeEdges(LineContainer container, IShape shape)
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
