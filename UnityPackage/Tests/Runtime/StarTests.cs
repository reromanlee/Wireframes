using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class StarTests : WireframesTestBase
    {
        [Test]
        public void Star_AlternatesTipsAndInnerCorners()
        {
            LineContainer container = CreateContainer();
            Vector3 center = new(0f, 1f, 0f);

            IStar star = container.CreateStar(center, 0.4f, 1f, 5);

            Vector3[] vertices = BakeShape(container, star);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, star);
            Assert.That(star.PointCount, Is.EqualTo(5));
            Assert.That(vertices, Has.Length.EqualTo(10));
            Assert.That(edges, Has.Length.EqualTo(10));
            AssertApproximately(center + Vector3.forward, vertices[0]);
            for (int i = 0; i < vertices.Length; i++)
            {
                AssertApproximately(i % 2 == 0 ? 1f : 0.4f, Vector3.Distance(center, vertices[i]));
                AssertApproximately(center.y, vertices[i].y);
                AssertApproximately(vertices[i], edges[i].A);
                AssertApproximately(vertices[(i + 1) % vertices.Length], edges[i].B);
            }
        }

        [Test]
        public void PoseForm_PointsTheFirstTipAlongTheRotationsZ()
        {
            LineContainer container = CreateContainer();
            Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);

            IStar star = container.CreateStar(Vector3.zero, rotation, 0.5f, 2f, 3);

            AssertApproximately(rotation * new Vector3(0f, 0f, 2f), BakeShape(container, star)[0]);
        }

        [Test]
        public void CreateStar_WithoutArguments_IsAFivePointedStar()
        {
            IStar star = CreateContainer().CreateStar();

            Assert.That(star.PointCount, Is.EqualTo(5));
            Assert.That(star.OuterRadius, Is.EqualTo(0.5f));
            Assert.That(star.InnerRadius, Is.EqualTo(0.2f));
        }

        [Test]
        public void ChangingBone_ConvertsBothRadii()
        {
            IStar star = CreateContainer().CreateStar(Vector3.zero, 1f, 2f, 4);

            star.Bone = CreateBone(Vector3.zero, Quaternion.identity, 4f);

            AssertApproximately(0.25f, star.InnerRadius);
            AssertApproximately(0.5f, star.OuterRadius);
        }

        [Test]
        public void FewerThanThreePoints_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateStar(Vector3.zero, 0.5f, 1f, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateStar(Vector3.zero, 0.5f, 1f, -1));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }
    }
}
