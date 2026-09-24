using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class FrustumTests : WireframesTestBase
    {
        [Test]
        public void Frustum_IsTwoPolygonsJoinedAtEveryCorner()
        {
            LineContainer container = CreateContainer();

            IFrustum frustum = container.CreateFrustum(Vector3.zero, new Vector3(0f, 0f, 2f), 1f, 2f, 4);

            Vector3[] vertices = BakeShape(container, frustum);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, frustum);
            Assert.That(frustum.Sides, Is.EqualTo(4));
            Assert.That(vertices, Has.Length.EqualTo(8));
            Assert.That(edges, Has.Length.EqualTo(12));
            // A flat side faces down, so four sides make a square aligned with the axes.
            float corner = 1f / Mathf.Sqrt(2f);
            AssertApproximately(new Vector3(corner, -corner, 0f), vertices[0]);
            AssertApproximately(new Vector3(corner, corner, 0f), vertices[1]);
            AssertApproximately(new Vector3(-corner, corner, 0f), vertices[2]);
            AssertApproximately(new Vector3(-corner, -corner, 0f), vertices[3]);
            AssertApproximately(new Vector3(corner * 2f, -corner * 2f, 2f), vertices[4]);
            for (int side = 0; side < 4; side++)
            {
                AssertApproximately(vertices[side], edges[8 + side].A);
                AssertApproximately(vertices[4 + side], edges[8 + side].B);
            }
        }

        [Test]
        public void ThreeSides_HaveALevelBottomSide()
        {
            LineContainer container = CreateContainer();

            IFrustum frustum = container.CreateFrustum(Vector3.zero, Quaternion.identity, 1f, 1f, 1f, 3);

            Vector3[] vertices = BakeShape(container, frustum);
            AssertApproximately(vertices[0].y, vertices[2].y);
            AssertApproximately(new Vector3(0f, 1f, 0f), vertices[1]);
        }

        [Test]
        public void RadiusAOfZero_MakesAPyramidWithARegularBase()
        {
            LineContainer container = CreateContainer();

            IFrustum pyramid = container.CreateFrustum(Vector3.zero, new Vector3(0f, 0f, 1f), 0f, 1f, 5);

            Vector3[] vertices = BakeShape(container, pyramid);
            for (int corner = 0; corner < 5; corner++)
            {
                AssertApproximately(Vector3.zero, vertices[corner]);
                AssertApproximately(1f, Vector3.Distance(new Vector3(0f, 0f, 1f), vertices[5 + corner]));
            }
        }

        [Test]
        public void CreateFrustum_WithoutArguments_IsASquareFrustumAlongZ()
        {
            IFrustum frustum = CreateContainer().CreateFrustum();

            Assert.That(frustum.Sides, Is.EqualTo(4));
            Assert.That(frustum.RadiusA, Is.EqualTo(0.25f));
            Assert.That(frustum.RadiusB, Is.EqualTo(0.5f));
            Assert.That(frustum.Length, Is.EqualTo(1f));
        }

        [Test]
        public void CreateFrustumOnBone_FollowsTheBone()
        {
            Transform bone = CreateBone(new Vector3(1f, 0f, 0f), Quaternion.Euler(0f, 45f, 0f), 2f);

            IFrustum frustum = CreateContainer().CreateFrustum(bone, Vector3.zero, new Vector3(0f, 0f, 1f), 0.5f, 1f, 6);

            Assert.That(frustum.Bone, Is.SameAs(bone));
            AssertApproximately(bone.TransformPoint(new Vector3(0f, 0f, 1f)), frustum.WorldEnd);
        }

        [Test]
        public void FewerThanThreeSides_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateFrustum(Vector3.zero, Vector3.up, 1f, 1f, 2));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }
    }
}
