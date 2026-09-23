using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class EllipsoidTests : WireframesTestBase
    {
        [Test]
        public void Ellipsoid_IsThreeEllipsesOnItsSurface()
        {
            LineContainer container = CreateContainer();

            IEllipsoid ellipsoid = container.CreateEllipsoid(Vector3.zero, Quaternion.identity, new Vector3(1f, 2f, 3f), 8);

            Vector3[] vertices = BakeShape(container, ellipsoid);
            Assert.That(vertices, Has.Length.EqualTo(24));
            Assert.That(BakeEdges(container, ellipsoid), Has.Length.EqualTo(24));
            foreach (Vector3 vertex in vertices)
            {
                AssertApproximately(1f, vertex.x * vertex.x + vertex.y * vertex.y / 4f + vertex.z * vertex.z / 9f);
            }
            // One ellipse in each of the XZ, XY and YZ planes.
            for (int i = 0; i < 8; i++)
            {
                AssertApproximately(0f, vertices[i].y);
                AssertApproximately(0f, vertices[8 + i].z);
                AssertApproximately(0f, vertices[16 + i].x);
            }
        }

        [Test]
        public void CreateEllipsoidFromTips_IsARoundSpheroidBetweenThem()
        {
            IEllipsoid ellipsoid = CreateContainer().CreateEllipsoid(Vector3.zero, new Vector3(0f, 6f, 0f), 1f);

            Assert.That(ellipsoid.Radii, Is.EqualTo(new Vector3(1f, 1f, 3f)));
            AssertApproximately(new Vector3(0f, 3f, 0f), ellipsoid.WorldPosition);
            AssertApproximately(Vector3.up, ellipsoid.WorldRotation * Vector3.forward);
        }

        [Test]
        public void CreateEllipsoidOnBone_FollowsTheBone()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            IEllipsoid ellipsoid = container.CreateEllipsoid(bone, new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, 1f), 0.5f, 4);

            bone.position = new Vector3(3f, 0f, 0f);

            Assert.That(ellipsoid.Bone, Is.SameAs(bone));
            AssertApproximately(new Vector3(3f, 0f, 1f), BakeShape(container, ellipsoid)[0]);
        }

        [Test]
        public void CreateEllipsoid_WithoutArguments_IsOneUnitLongAlongZ()
        {
            IEllipsoid ellipsoid = CreateContainer().CreateEllipsoid();

            Assert.That(ellipsoid.Radii, Is.EqualTo(new Vector3(0.25f, 0.25f, 0.5f)));
            Assert.That(ellipsoid.Segments, Is.EqualTo(32));
        }

        [Test]
        public void FewerThanThreeSegments_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => container.CreateEllipsoid(Vector3.zero, Vector3.up, 1f, 1));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }
    }
}
