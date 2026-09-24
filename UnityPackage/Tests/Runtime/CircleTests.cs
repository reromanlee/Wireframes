using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class CircleTests : WireframesTestBase
    {
        [Test]
        public void Circle_IsARingOfSegmentsFlatInXZ()
        {
            LineContainer container = CreateContainer();
            Vector3 center = new(1f, 2f, 3f);
            ICircle circle = container.CreateCircle(center, 2f, 16);

            Vector3[] vertices = BakeShape(container, circle);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, circle);

            Assert.That(vertices, Has.Length.EqualTo(16));
            foreach (Vector3 vertex in vertices)
            {
                AssertApproximately(2f, Vector3.Distance(center, vertex));
                AssertApproximately(center.y, vertex.y);
            }
            // The ring starts on +Z and passes +X a quarter turn later.
            AssertApproximately(center + new Vector3(0f, 0f, 2f), vertices[0]);
            AssertApproximately(center + new Vector3(2f, 0f, 0f), vertices[4]);

            Assert.That(edges, Has.Length.EqualTo(16));
            for (int i = 0; i < edges.Length; i++)
            {
                AssertApproximately(vertices[i], edges[i].A);
                AssertApproximately(vertices[(i + 1) % 16], edges[i].B);
            }
        }

        [Test]
        public void CreateCircle_WithoutArguments_IsHalfUnitRadiusAtOrigin()
        {
            ICircle circle = CreateContainer().CreateCircle();

            Assert.That(circle.Radius, Is.EqualTo(0.5f));
            Assert.That(circle.Segments, Is.EqualTo(32));
            Assert.That(circle.WorldPosition, Is.EqualTo(Vector3.zero));
            Assert.That(circle.WorldRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(circle.Bone, Is.Null);
        }

        [Test]
        public void CreateCircleWithNormal_FacesTheNormal()
        {
            LineContainer container = CreateContainer();
            Vector3 normal = new Vector3(1f, 1f, 0f).normalized;
            ICircle circle = container.CreateCircle(Vector3.zero, normal * 3f, 1f, 12);

            AssertApproximately(normal, circle.WorldRotation * Vector3.up);
            foreach (Vector3 vertex in BakeShape(container, circle))
            {
                AssertApproximately(0f, Vector3.Dot(vertex, normal));
                AssertApproximately(1f, vertex.magnitude);
            }
        }

        [Test]
        public void CreateCircleOnBone_LiesFlatInTheBonesXZ()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(0f, 1f, 0f), Quaternion.Euler(0f, 0f, 90f));

            ICircle circle = container.CreateCircle(bone, new Vector3(0f, 0.5f, 0f), 2f, 8);

            Assert.That(circle.Bone, Is.SameAs(bone));
            Assert.That(circle.LocalPosition, Is.EqualTo(new Vector3(0f, 0.5f, 0f)));
            Vector3 center = bone.TransformPoint(new Vector3(0f, 0.5f, 0f));
            foreach (Vector3 vertex in BakeShape(container, circle))
            {
                AssertApproximately(0f, Vector3.Dot(vertex - center, bone.up));
                AssertApproximately(2f, Vector3.Distance(center, vertex));
            }
        }

        [Test]
        public void Radius_RewritesTheRing()
        {
            LineContainer container = CreateContainer();
            ICircle circle = container.CreateCircle(Vector3.zero, 1f, 4);

            circle.Radius = 3f;

            AssertApproximately(new Vector3(0f, 0f, 3f), BakeShape(container, circle)[0]);
        }

        [Test]
        public void FewerThanThreeSegments_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateCircle(Vector3.zero, 1f, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateCircle(Vector3.zero, 1f, -1));
            Assert.That(container.CreateCircle(Vector3.zero, 1f, 3).Segments, Is.EqualTo(3));
            Assert.That(ChunkOf(container).ShapeCount, Is.EqualTo(1), "A rejected circle must not stay in the mesh.");
        }
    }
}
