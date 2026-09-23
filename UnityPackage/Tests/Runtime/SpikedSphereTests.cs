using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class SpikedSphereTests : WireframesTestBase
    {
        [TestCase(4, 4, 6, 3, TestName = "Tetrahedron")]
        [TestCase(6, 8, 12, 4, TestName = "Cube")]
        [TestCase(8, 6, 12, 3, TestName = "Octahedron")]
        [TestCase(12, 20, 30, 5, TestName = "Dodecahedron")]
        [TestCase(20, 12, 30, 3, TestName = "Icosahedron")]
        public void SpikedSphere_PutsASpikeOnEveryFaceOfItsSolid(int spikes, int corners, int solidEdges, int faceSize)
        {
            LineContainer container = CreateContainer();

            ISpikedSphere sphere = container.CreateSpikedSphere(Vector3.zero, 1f, 0.5f, spikes);

            Vector3[] vertices = BakeShape(container, sphere);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, sphere);
            Assert.That(sphere.SpikeCount, Is.EqualTo(spikes));
            Assert.That(vertices, Has.Length.EqualTo(corners + spikes));
            Assert.That(edges, Has.Length.EqualTo(solidEdges + spikes * faceSize));
            for (int i = 0; i < vertices.Length; i++)
            {
                AssertApproximately(i < corners ? 1f : 1.5f, vertices[i].magnitude);
            }

            // The solid's edges all have one length, and each spike tip is joined to the corners of one face, all at
            // the same distance.
            float solidEdge = Vector3.Distance(edges[0].A, edges[0].B);
            Dictionary<Vector3, List<float>> spikeEdges = new();
            for (int i = 0; i < edges.Length; i++)
            {
                (Vector3 a, Vector3 b) = edges[i];
                if (i < solidEdges)
                {
                    AssertApproximately(1f, a.magnitude);
                    AssertApproximately(1f, b.magnitude);
                    AssertApproximately(solidEdge, Vector3.Distance(a, b));
                    continue;
                }
                AssertApproximately(1.5f, a.magnitude);
                AssertApproximately(1f, b.magnitude);
                if (!spikeEdges.TryGetValue(a, out List<float> lengths))
                {
                    lengths = new List<float>();
                    spikeEdges.Add(a, lengths);
                }
                lengths.Add(Vector3.Distance(a, b));
            }
            Assert.That(spikeEdges, Has.Count.EqualTo(spikes));
            foreach (List<float> lengths in spikeEdges.Values)
            {
                Assert.That(lengths, Has.Count.EqualTo(faceSize));
                Assert.That(lengths, Is.All.EqualTo(lengths[0]).Within(1e-4f));
            }
        }

        [Test]
        public void CubeSpikes_PointAlongTheAxes()
        {
            LineContainer container = CreateContainer();

            ISpikedSphere sphere = container.CreateSpikedSphere(Vector3.zero, 1f, 1f, 6);

            Vector3[] vertices = BakeShape(container, sphere);
            for (int i = 8; i < vertices.Length; i++)
            {
                Vector3 tip = vertices[i];
                float largest = Mathf.Max(Mathf.Abs(tip.x), Mathf.Abs(tip.y), Mathf.Abs(tip.z));
                AssertApproximately(2f, largest);
                AssertApproximately(2f, tip.magnitude);
            }
        }

        [Test]
        public void CreateSpikedSphere_WithoutArguments_IsATwelvePointStar()
        {
            ISpikedSphere sphere = CreateContainer().CreateSpikedSphere();

            Assert.That(sphere.SpikeCount, Is.EqualTo(12));
            Assert.That(sphere.BaseRadius, Is.EqualTo(0.25f));
            Assert.That(sphere.SpikeLength, Is.EqualTo(0.25f));
        }

        [Test]
        public void ChangingBone_ConvertsBaseRadiusAndSpikeLength()
        {
            ISpikedSphere sphere = CreateContainer().CreateSpikedSphere(Vector3.zero, 1f, 2f, 8);

            sphere.Bone = CreateBone(Vector3.zero, Quaternion.identity, 2f);

            AssertApproximately(0.5f, sphere.BaseRadius);
            AssertApproximately(1f, sphere.SpikeLength);
        }

        [TestCase(0)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(24)]
        public void SpikeCountThatIsNotAPlatonicSolid_Throws(int spikes)
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateSpikedSphere(Vector3.zero, 1f, 1f, spikes));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }
    }
}
