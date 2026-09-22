using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

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

        protected static void AssertApproximately(Vector3 expected, Vector3 actual)
        {
            Assert.That(Vector3.Distance(expected, actual), Is.LessThan(Tolerance), $"Expected {expected:F4}, got {actual:F4}");
        }
    }
}
