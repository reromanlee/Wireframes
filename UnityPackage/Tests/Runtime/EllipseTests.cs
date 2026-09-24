using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class EllipseTests : WireframesTestBase
    {
        [Test]
        public void Ellipse_HasItsRadiiAlongXAndZ()
        {
            LineContainer container = CreateContainer();
            Vector3 center = new(1f, 2f, 3f);
            IEllipse ellipse = container.CreateEllipse(center, Quaternion.identity, new Vector2(3f, 1f), 8);

            Vector3[] vertices = BakeShape(container, ellipse);

            // Starts on +Z and passes +X a quarter turn later.
            AssertApproximately(center + new Vector3(0f, 0f, 1f), vertices[0]);
            AssertApproximately(center + new Vector3(3f, 0f, 0f), vertices[2]);
            foreach (Vector3 vertex in vertices)
            {
                Vector3 offset = vertex - center;
                AssertApproximately(1f, offset.x * offset.x / 9f + offset.z * offset.z);
                AssertApproximately(0f, offset.y);
            }
            Assert.That(BakeEdges(container, ellipse), Has.Length.EqualTo(8));
        }

        [Test]
        public void CreateEllipseFromTips_RunsBetweenThemLyingFlat()
        {
            LineContainer container = CreateContainer();

            IEllipse ellipse = container.CreateEllipse(new Vector3(-2f, 1f, 0f), new Vector3(2f, 1f, 0f), 0.5f, 8);

            Assert.That(ellipse.Radii, Is.EqualTo(new Vector2(0.5f, 2f)));
            AssertApproximately(new Vector3(0f, 1f, 0f), ellipse.WorldPosition);
            Vector3[] vertices = BakeShape(container, ellipse);
            AssertApproximately(new Vector3(2f, 1f, 0f), vertices[0]);
            AssertApproximately(new Vector3(-2f, 1f, 0f), vertices[4]);
            foreach (Vector3 vertex in vertices)
            {
                AssertApproximately(1f, vertex.y);
            }
        }

        [Test]
        public void CreateEllipseFromTipsWithNormal_FacesTheNormal()
        {
            LineContainer container = CreateContainer();

            IEllipse ellipse = container.CreateEllipse(Vector3.zero, new Vector3(0f, 0f, 4f), 1f, Vector3.right, 8);

            AssertApproximately(Vector3.right, ellipse.WorldRotation * Vector3.up);
            foreach (Vector3 vertex in BakeShape(container, ellipse))
            {
                AssertApproximately(0f, vertex.x);
            }
        }

        [Test]
        public void CreateEllipseOnBone_RunsBetweenLocalTips()
        {
            Transform bone = CreateBone(new Vector3(5f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), 2f);

            IEllipse ellipse = CreateContainer().CreateEllipse(bone, Vector3.zero, new Vector3(0f, 0f, 2f), 0.5f);

            Assert.That(ellipse.Bone, Is.SameAs(bone));
            AssertApproximately(new Vector3(0f, 0f, 1f), ellipse.LocalPosition);
            AssertApproximately(new Vector2(0.5f, 1f), ellipse.Radii);
        }

        [Test]
        public void CreateEllipse_WithoutArguments_IsOneUnitLongAlongZ()
        {
            IEllipse ellipse = CreateContainer().CreateEllipse();

            Assert.That(ellipse.Radii, Is.EqualTo(new Vector2(0.25f, 0.5f)));
            Assert.That(ellipse.Segments, Is.EqualTo(32));
        }

        [Test]
        public void FewerThanThreeSegments_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => container.CreateEllipse(Vector3.zero, Quaternion.identity, Vector2.one, 2));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }
    }
}
