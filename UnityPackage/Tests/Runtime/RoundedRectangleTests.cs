using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class RoundedRectangleTests : WireframesTestBase
    {
        [Test]
        public void RoundedRectangle_IsFourQuarterCirclesJoinedBySides()
        {
            LineContainer container = CreateContainer();
            IRoundedRectangle rectangle =
                container.CreateRoundedRectangle(new Vector3(-2f, 0f, -1f), new Vector3(2f, 0f, 1f), 0.5f, 8);

            Vector3[] vertices = BakeShape(container, rectangle);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, rectangle);

            Assert.That(vertices, Has.Length.EqualTo(12));
            Assert.That(edges, Has.Length.EqualTo(12));
            // Going around from +Z toward +X, the corners are (+X, +Z), (+X, -Z), (-X, -Z) and (-X, +Z).
            Vector3[] centers =
            {
                new(1.5f, 0f, 0.5f), new(1.5f, 0f, -0.5f), new(-1.5f, 0f, -0.5f), new(-1.5f, 0f, 0.5f)
            };
            for (int i = 0; i < vertices.Length; i++)
            {
                AssertApproximately(0.5f, Vector3.Distance(centers[i / 3], vertices[i]));
                AssertApproximately(0f, vertices[i].y);
                AssertApproximately(vertices[i], edges[i].A);
                AssertApproximately(vertices[(i + 1) % vertices.Length], edges[i].B);
            }
            AssertApproximately(new Vector3(1.5f, 0f, 1f), vertices[0]);
            AssertApproximately(new Vector3(2f, 0f, 0.5f), vertices[2]);
        }

        [Test]
        public void CornerRadius_IsDrawnClampedToHalfTheShorterSide()
        {
            LineContainer container = CreateContainer();
            IRoundedRectangle rectangle =
                container.CreateRoundedRectangle(Vector3.zero, Quaternion.identity, new Vector2(4f, 2f), 5f, 8);

            Vector3[] vertices = BakeShape(container, rectangle);

            // Radius 1: the short sides are half circles around (+1, 0, 0) and (-1, 0, 0).
            for (int i = 0; i < vertices.Length; i++)
            {
                AssertApproximately(1f, Vector3.Distance(new Vector3(i < 6 ? 1f : -1f, 0f, 0f), vertices[i]));
            }
            Assert.That(rectangle.CornerRadius, Is.EqualTo(5f));
        }

        [Test]
        public void NegativeCornerRadius_DrawsSharpCorners()
        {
            LineContainer container = CreateContainer();
            IRoundedRectangle rectangle =
                container.CreateRoundedRectangle(Vector3.zero, Quaternion.identity, new Vector2(2f, 2f), -1f, 4);

            Vector3[] vertices = BakeShape(container, rectangle);

            AssertApproximately(new Vector3(1f, 0f, 1f), vertices[0]);
            AssertApproximately(new Vector3(1f, 0f, 1f), vertices[1]);
            AssertApproximately(new Vector3(1f, 0f, -1f), vertices[2]);
        }

        [Test]
        public void RoundedRectangle_IsARectangle()
        {
            IRectangle rectangle = CreateContainer().CreateRoundedRectangle(Vector3.zero, new Vector3(2f, 0f, 2f), 0.5f);

            rectangle.WorldCornerB = new Vector3(4f, 0f, 3f);

            Assert.That(rectangle.Size, Is.EqualTo(new Vector2(4f, 3f)));
            AssertApproximately(Vector3.zero, rectangle.WorldCornerA);
        }

        [Test]
        public void CreateRoundedRectangle_WithoutArguments_IsAUnitSquareWithQuarterRadiusCorners()
        {
            IRoundedRectangle rectangle = CreateContainer().CreateRoundedRectangle();

            Assert.That(rectangle.Size, Is.EqualTo(Vector2.one));
            Assert.That(rectangle.CornerRadius, Is.EqualTo(0.25f));
            Assert.That(rectangle.Segments, Is.EqualTo(32));
        }

        [Test]
        public void ChangingBone_ConvertsSizeAndCornerRadius()
        {
            IRoundedRectangle rectangle = CreateContainer().CreateRoundedRectangle(Vector3.zero, new Vector3(4f, 0f, 2f), 1f);

            rectangle.Bone = CreateBone(Vector3.zero, Quaternion.identity, 2f);

            AssertApproximately(new Vector2(2f, 1f), rectangle.Size);
            AssertApproximately(0.5f, rectangle.CornerRadius);
        }

        [Test]
        public void SegmentsThatAreNotAMultipleOfFour_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => container.CreateRoundedRectangle(Vector3.zero, Vector3.one, 0.1f, 6));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }
    }
}
