using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class StadiumTests : WireframesTestBase
    {
        [Test]
        public void Stadium_IsTwoArcsJoinedByStraightSides()
        {
            LineContainer container = CreateContainer();

            IStadium stadium = container.CreateStadium(Vector3.zero, new Vector3(0f, 0f, 4f), 1f, 8);

            Vector3[] vertices = BakeShape(container, stadium);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, stadium);
            Assert.That(vertices, Has.Length.EqualTo(10));
            Assert.That(edges, Has.Length.EqualTo(10));
            // Arc A runs from +X around behind end A to -X, arc B from -X around past end B back to +X.
            AssertApproximately(new Vector3(1f, 0f, 0f), vertices[0]);
            AssertApproximately(new Vector3(0f, 0f, -1f), vertices[2]);
            AssertApproximately(new Vector3(-1f, 0f, 0f), vertices[4]);
            AssertApproximately(new Vector3(-1f, 0f, 4f), vertices[5]);
            AssertApproximately(new Vector3(0f, 0f, 5f), vertices[7]);
            AssertApproximately(new Vector3(1f, 0f, 4f), vertices[9]);
            for (int i = 0; i < vertices.Length; i++)
            {
                AssertApproximately(0f, vertices[i].y);
                AssertApproximately(vertices[i], edges[i].A);
                AssertApproximately(vertices[(i + 1) % vertices.Length], edges[i].B);
            }
        }

        [Test]
        public void TaperedStadium_SidesTouchBothCircles()
        {
            LineContainer container = CreateContainer();
            Vector3 centerB = new(0f, 0f, 4f);

            IStadium stadium = container.CreateStadium(Vector3.zero, centerB, 1f, 0.5f, 8);

            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, stadium);
            // Edge 4 is the -X side, edge 9 the +X side.
            foreach (int side in new[] { 4, 9 })
            {
                (Vector3 a, Vector3 b) = edges[side];
                Vector3 onA = a.magnitude < b.magnitude ? a : b;
                Vector3 onB = onA == a ? b : a;
                Vector3 direction = (onB - onA).normalized;
                AssertApproximately(1f, onA.magnitude);
                AssertApproximately(0.5f, Vector3.Distance(centerB, onB));
                AssertApproximately(0f, Vector3.Dot(direction, onA.normalized));
                AssertApproximately(0f, Vector3.Dot(direction, (onB - centerB).normalized));
            }
        }

        [Test]
        public void CreateStadiumWithNormal_FacesTheNormal()
        {
            LineContainer container = CreateContainer();

            IStadium stadium = container.CreateStadium(Vector3.zero, new Vector3(4f, 0f, 0f), 1f, 1f, Vector3.forward, 8);

            AssertApproximately(Vector3.forward, stadium.WorldRotation * Vector3.up);
            foreach (Vector3 vertex in BakeShape(container, stadium))
            {
                AssertApproximately(0f, vertex.z);
            }
        }

        [Test]
        public void CircleInsideTheOther_DrawsOnlyTheBiggerCircle()
        {
            LineContainer container = CreateContainer();

            IStadium stadium = container.CreateStadium(Vector3.zero, new Vector3(0f, 0f, 1f), 0.5f, 3f, 8);

            foreach (Vector3 vertex in BakeShape(container, stadium))
            {
                AssertApproximately(3f, Vector3.Distance(new Vector3(0f, 0f, 1f), vertex));
            }
        }

        [Test]
        public void CreateStadiumOnBone_LiesFlatInTheBonesXZ()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(0f, 1f, 0f), Quaternion.Euler(0f, 0f, 30f));

            IStadium stadium = container.CreateStadium(bone, Vector3.zero, new Vector3(2f, 0f, 0f), 0.5f, 0.5f, 8);

            Assert.That(stadium.Bone, Is.SameAs(bone));
            foreach (Vector3 vertex in BakeShape(container, stadium))
            {
                AssertApproximately(0f, Vector3.Dot(vertex - bone.position, bone.up));
            }
        }

        [Test]
        public void CreateStadium_WithoutArguments_HasHalfUnitCirclesOneUnitApart()
        {
            IStadium stadium = CreateContainer().CreateStadium();

            Assert.That(stadium.Length, Is.EqualTo(1f));
            Assert.That(stadium.RadiusA, Is.EqualTo(0.5f));
            Assert.That(stadium.RadiusB, Is.EqualTo(0.5f));
            Assert.That(stadium.Segments, Is.EqualTo(32));
        }

        [Test]
        public void SegmentsThatAreNotAMultipleOfFour_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateStadium(Vector3.zero, Vector3.right, 1f, 2));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }
    }
}
