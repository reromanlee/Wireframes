using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class SphereTests : WireframesTestBase
    {
        [Test]
        public void Sphere_IsThreeGreatCirclesTurnedWithTheShape()
        {
            LineContainer container = CreateContainer();
            Vector3 center = new(0f, 5f, 0f);
            Quaternion rotation = Quaternion.Euler(0f, 0f, 30f);
            ISphere sphere = container.CreateSphere(center, rotation, 2f, 8);

            Vector3[] vertices = BakeShape(container, sphere);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, sphere);

            Assert.That(vertices, Has.Length.EqualTo(24));
            Assert.That(edges, Has.Length.EqualTo(24));
            // One circle in each plane of the sphere's axes: XZ, XY and YZ.
            Vector3[] normals = { rotation * Vector3.up, rotation * Vector3.forward, rotation * Vector3.right };
            for (int i = 0; i < vertices.Length; i++)
            {
                AssertApproximately(2f, Vector3.Distance(center, vertices[i]));
                AssertApproximately(0f, Vector3.Dot(vertices[i] - center, normals[i / 8]));
            }
            foreach ((Vector3 a, Vector3 b) in edges)
            {
                // Consecutive points of an 8-segment circle of radius 2.
                AssertApproximately(2f * 2f * Mathf.Sin(Mathf.PI / 8f), Vector3.Distance(a, b));
            }
        }

        [Test]
        public void CreateSphere_WithoutArguments_IsHalfUnitRadiusAtOrigin()
        {
            ISphere sphere = CreateContainer().CreateSphere();

            Assert.That(sphere.Radius, Is.EqualTo(0.5f));
            Assert.That(sphere.Segments, Is.EqualTo(32));
            Assert.That(((Sphere)sphere).VertexCount, Is.EqualTo(96));
        }

        [Test]
        public void CreateSphereOnBone_FollowsTheBone()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(-2f, 0f, 0f), Quaternion.Euler(45f, 0f, 0f));
            ISphere sphere = container.CreateSphere(bone, new Vector3(0f, 0f, 1f), 0.5f, 4);

            bone.position = new Vector3(-2f, 3f, 0f);
            Vector3[] vertices = BakeShape(container, sphere);

            AssertApproximately(bone.TransformPoint(new Vector3(0f, 0f, 1.5f)), vertices[0]);
            AssertApproximately(bone.TransformPoint(new Vector3(0f, 0.5f, 1f)), vertices[4]);
        }

        [Test]
        public void FewerThanThreeSegments_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateSphere(Vector3.zero, 1f, 0));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }
    }
}
