using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class CapsuleTests : WireframesTestBase
    {
        [Test]
        public void Capsule_IsTwoRingsFourSideLinesAndTwoArcsOverEachCap()
        {
            LineContainer container = CreateContainer();
            Vector3 centerA = Vector3.zero;
            Vector3 centerB = new(0f, 0f, 4f);

            ICapsule capsule = container.CreateCapsule(centerA, centerB, 1f, 8);

            Vector3[] vertices = BakeShape(container, capsule);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, capsule);
            Assert.That(vertices, Has.Length.EqualTo(26));
            Assert.That(edges, Has.Length.EqualTo(36));
            foreach (Vector3 vertex in vertices)
            {
                float offSphereA = Mathf.Abs(Vector3.Distance(centerA, vertex) - 1f);
                float offSphereB = Mathf.Abs(Vector3.Distance(centerB, vertex) - 1f);
                Assert.That(Mathf.Min(offSphereA, offSphereB), Is.LessThan(1e-4f), $"{vertex:F3} is on neither sphere");
            }
            // The rings sit around the two centers, and the caps reach the poles.
            AssertApproximately(new Vector3(0f, 1f, 0f), vertices[0]);
            AssertApproximately(new Vector3(0f, 1f, 4f), vertices[8]);
            Assert.That(Array.Exists(vertices, vertex => Vector3.Distance(vertex, new Vector3(0f, 0f, -1f)) < 1e-4f));
            Assert.That(Array.Exists(vertices, vertex => Vector3.Distance(vertex, new Vector3(0f, 0f, 5f)) < 1e-4f));
            // Edges 16 to 19 are the side lines.
            for (int line = 16; line < 20; line++)
            {
                AssertApproximately(4f, Vector3.Distance(edges[line].A, edges[line].B));
            }
        }

        [Test]
        public void TaperedCapsule_SideLinesTouchBothSpheres()
        {
            LineContainer container = CreateContainer();
            Vector3 centerB = new(0f, 0f, 4f);

            ICapsule capsule = container.CreateCapsule(Vector3.zero, centerB, 1f, 0.5f, 8);

            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, capsule);
            for (int line = 16; line < 20; line++)
            {
                (Vector3 a, Vector3 b) = edges[line];
                Vector3 direction = (b - a).normalized;
                AssertApproximately(1f, a.magnitude);
                AssertApproximately(0.5f, Vector3.Distance(centerB, b));
                AssertApproximately(0f, Vector3.Dot(direction, a.normalized));
                AssertApproximately(0f, Vector3.Dot(direction, (b - centerB).normalized));
            }
        }

        [Test]
        public void SphereInsideTheOther_DrawsOnlyTheBiggerSphere()
        {
            LineContainer container = CreateContainer();

            ICapsule capsule = container.CreateCapsule(Vector3.zero, new Vector3(0f, 0f, 1f), 3f, 0.5f, 8);

            foreach (Vector3 vertex in BakeShape(container, capsule))
            {
                AssertApproximately(3f, vertex.magnitude);
            }
        }

        [Test]
        public void NegativeLength_MirrorsTheCapsule()
        {
            LineContainer container = CreateContainer();

            ICapsule capsule = container.CreateCapsule(Vector3.zero, Quaternion.identity, -4f, 1f, 0.5f, 8);

            AssertApproximately(new Vector3(0f, 0f, -4f), capsule.WorldEnd);
            Vector3[] vertices = BakeShape(container, capsule);
            Assert.That(Array.Exists(vertices, vertex => Vector3.Distance(vertex, new Vector3(0f, 0f, -4.5f)) < 1e-4f));
            Assert.That(Array.Exists(vertices, vertex => Vector3.Distance(vertex, new Vector3(0f, 0f, 1f)) < 1e-4f));
        }

        [Test]
        public void CreateCapsule_WithoutArguments_HasHalfUnitSpheresOneUnitApart()
        {
            ICapsule capsule = CreateContainer().CreateCapsule();

            Assert.That(capsule.Length, Is.EqualTo(1f));
            Assert.That(capsule.RadiusA, Is.EqualTo(0.5f));
            Assert.That(capsule.RadiusB, Is.EqualTo(0.5f));
            Assert.That(capsule.Segments, Is.EqualTo(32));
        }

        [Test]
        public void CreateCapsuleOnBone_FollowsTheBone()
        {
            LineContainer container = CreateContainer();
            Transform upperArm = CreateBone(new Vector3(0f, 1.5f, 0f), Quaternion.identity);
            ICapsule arm = container.CreateCapsule(upperArm, Vector3.zero, new Vector3(0f, -0.3f, 0f), 0.1f, 0.08f, 4);

            upperArm.rotation = Quaternion.Euler(0f, 0f, 90f);

            Assert.That(arm.Bone, Is.SameAs(upperArm));
            AssertApproximately(upperArm.TransformPoint(new Vector3(0f, -0.3f, 0f)), arm.WorldEnd);
            foreach (Vector3 vertex in BakeShape(container, arm))
            {
                float offSphereA = Mathf.Abs(Vector3.Distance(upperArm.position, vertex) - 0.1f);
                float offSphereB = Mathf.Abs(Vector3.Distance(arm.WorldEnd, vertex) - 0.08f);
                Assert.That(Mathf.Min(offSphereA, offSphereB), Is.LessThan(1e-4f));
            }
        }

        [Test]
        public void SegmentsThatAreNotAMultipleOfFour_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateCapsule(Vector3.zero, Vector3.up, 1f, 6));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }
    }
}
