using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class ConeTests : WireframesTestBase
    {
        [Test]
        public void Cone_IsABaseRingJoinedToTheTipByFourLines()
        {
            LineContainer container = CreateContainer();
            Vector3 tip = new(0f, 3f, 0f);
            Vector3 baseCenter = new(0f, 1f, 0f);

            ICone cone = container.CreateCone(tip, baseCenter, 1f, 8);

            Vector3[] vertices = BakeShape(container, cone);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, cone);
            Assert.That(vertices, Has.Length.EqualTo(9));
            Assert.That(edges, Has.Length.EqualTo(12));
            for (int i = 0; i < 8; i++)
            {
                AssertApproximately(1f, Vector3.Distance(baseCenter, vertices[i]));
                AssertApproximately(1f, vertices[i].y);
            }
            AssertApproximately(tip, vertices[8]);
            for (int line = 0; line < 4; line++)
            {
                AssertApproximately(tip, edges[8 + line].A);
                AssertApproximately(vertices[line * 2], edges[8 + line].B);
            }
            AssertApproximately(2f, cone.Length);
            AssertApproximately(baseCenter, cone.WorldEnd);
        }

        [Test]
        public void CreateCone_WithoutArguments_PointsItsBaseAlongZ()
        {
            ICone cone = CreateContainer().CreateCone();

            Assert.That(cone.Length, Is.EqualTo(1f));
            Assert.That(cone.Radius, Is.EqualTo(0.5f));
            Assert.That(cone.Segments, Is.EqualTo(32));
            Assert.That(cone.WorldEnd, Is.EqualTo(new Vector3(0f, 0f, 1f)));
        }

        [Test]
        public void CreateConeOnBone_PutsTheTipOnTheBone()
        {
            LineContainer container = CreateContainer();
            Transform head = CreateBone(new Vector3(0f, 1.7f, 0f), Quaternion.Euler(0f, 90f, 0f));

            ICone sight = container.CreateCone(head, Vector3.zero, new Vector3(0f, 0f, 10f), 3f, 4);

            Assert.That(sight.Bone, Is.SameAs(head));
            AssertApproximately(head.position, BakeShape(container, sight)[4]);
            AssertApproximately(head.TransformPoint(new Vector3(0f, 0f, 10f)), sight.WorldEnd);
        }

        [Test]
        public void PoseForm_RunsAlongTheRotationsZ()
        {
            Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

            ICone cone = CreateContainer().CreateCone(Vector3.zero, rotation, 5f, 1f);

            AssertApproximately(new Vector3(5f, 0f, 0f), cone.WorldEnd);
        }

        [Test]
        public void SegmentsThatAreNotAMultipleOfFour_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateCone(Vector3.zero, Vector3.forward, 1f, 10));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }
    }
}
