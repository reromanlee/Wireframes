using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class CylinderTests : WireframesTestBase
    {
        [Test]
        public void Cylinder_IsTwoRingsJoinedByFourSideLines()
        {
            LineContainer container = CreateContainer();
            Vector3 endA = new(1f, 0f, 0f);
            Vector3 endB = new(1f, 0f, 6f);
            ICylinder cylinder = container.CreateCylinder(endA, endB, 2f, 8);

            Vector3[] vertices = BakeShape(container, cylinder);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, cylinder);

            Assert.That(vertices, Has.Length.EqualTo(16));
            for (int i = 0; i < 8; i++)
            {
                AssertApproximately(2f, Vector3.Distance(endA, vertices[i]));
                AssertApproximately(endA.z, vertices[i].z);
                AssertApproximately(2f, Vector3.Distance(endB, vertices[8 + i]));
                AssertApproximately(endB.z, vertices[8 + i].z);
            }
            // Each ring starts on the cylinder's +Y, which stays closest to world up.
            AssertApproximately(endA + new Vector3(0f, 2f, 0f), vertices[0]);

            Assert.That(edges, Has.Length.EqualTo(20));
            for (int line = 0; line < 4; line++)
            {
                (Vector3 a, Vector3 b) = edges[16 + line];
                AssertApproximately(vertices[line * 2], a);
                AssertApproximately(new Vector3(0f, 0f, 6f), b - a);
            }
        }

        [Test]
        public void CreateCylinder_WithoutArguments_RunsOneUnitAlongZ()
        {
            ICylinder cylinder = CreateContainer().CreateCylinder();

            Assert.That(cylinder.Length, Is.EqualTo(1f));
            Assert.That(cylinder.Radius, Is.EqualTo(0.5f));
            Assert.That(cylinder.Segments, Is.EqualTo(32));
            Assert.That(cylinder.WorldEnd, Is.EqualTo(new Vector3(0f, 0f, 1f)));
        }

        [Test]
        public void TwoPointForm_KeepsUpClosestToWorldUp()
        {
            ICylinder cylinder = CreateContainer().CreateCylinder(Vector3.zero, new Vector3(3f, 4f, 0f), 1f);

            AssertApproximately(Vector3.zero, cylinder.WorldPosition);
            AssertApproximately(new Vector3(3f, 4f, 0f), cylinder.WorldEnd);
            AssertApproximately(5f, cylinder.Length);
            // +X stays level, so +Y leans back from the axis toward world up.
            AssertApproximately(0f, (cylinder.WorldRotation * Vector3.right).y);
            Assert.That((cylinder.WorldRotation * Vector3.up).y, Is.GreaterThan(0f));
        }

        [Test]
        public void TwoPointForm_AlongWorldUp_TurnsZUpByTheSmallestRotation()
        {
            ICylinder cylinder = CreateContainer().CreateCylinder(Vector3.zero, new Vector3(0f, 5f, 0f), 1f);

            AssertApproximately(Quaternion.FromToRotation(Vector3.forward, Vector3.up), cylinder.WorldRotation);
            AssertApproximately(new Vector3(0f, 5f, 0f), cylinder.WorldEnd);
        }

        [Test]
        public void SettingEnd_TurnsBySmallestRotationAndSetsLength()
        {
            ICylinder cylinder = CreateContainer().CreateCylinder(new Vector3(1f, 1f, 1f), new Vector3(3f, 1f, 1f), 0.5f);

            cylinder.WorldEnd = new Vector3(1f, 1f, 4f);

            AssertApproximately(new Vector3(1f, 1f, 1f), cylinder.WorldPosition);
            AssertApproximately(new Vector3(1f, 1f, 4f), cylinder.WorldEnd);
            AssertApproximately(3f, cylinder.Length);
            // A turn within the ground plane leaves the cylinder's +Y pointing up.
            AssertApproximately(Vector3.up, cylinder.WorldRotation * Vector3.up);
        }

        [Test]
        public void SettingEndOnTopOfEndA_KeepsRotation()
        {
            ICylinder cylinder = CreateContainer().CreateCylinder(Vector3.zero, new Vector3(0f, 0f, -2f), 0.5f);
            Quaternion rotation = cylinder.WorldRotation;

            cylinder.WorldEnd = Vector3.zero;

            AssertApproximately(0f, cylinder.Length);
            AssertApproximately(rotation, cylinder.WorldRotation);
        }

        [Test]
        public void CreateCylinderOnBone_FollowsTheBone()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(0f, 2f, 0f), Quaternion.Euler(0f, 90f, 0f), 2f);
            Vector3 localEndA = new(0f, 0f, 0.5f);
            Vector3 localEndB = new(0f, 1f, 0.5f);

            ICylinder cylinder = container.CreateCylinder(bone, localEndA, localEndB, 0.25f, 4);

            Assert.That(cylinder.Bone, Is.SameAs(bone));
            AssertApproximately(localEndA, cylinder.LocalPosition);
            AssertApproximately(localEndB, cylinder.LocalEnd);
            bone.rotation = Quaternion.Euler(90f, 0f, 0f);
            Vector3[] vertices = BakeShape(container, cylinder);
            AssertApproximately(bone.TransformPoint(localEndB), (vertices[4] + vertices[6]) * 0.5f);
        }

        [Test]
        public void PoseForm_RunsAlongTheRotationsZ()
        {
            Quaternion rotation = Quaternion.Euler(0f, -90f, 0f);
            ICylinder cylinder = CreateContainer().CreateCylinder(new Vector3(2f, 0f, 0f), rotation, 4f, 1f);

            AssertApproximately(new Vector3(-2f, 0f, 0f), cylinder.WorldEnd);
            AssertApproximately(rotation, cylinder.WorldRotation);
        }

        [Test]
        public void ChangingBone_ConvertsLengthAndRadius()
        {
            ICylinder cylinder = CreateContainer().CreateCylinder(Vector3.zero, new Vector3(0f, 0f, 4f), 1f);
            Transform bone = CreateBone(Vector3.one, Quaternion.Euler(0f, 30f, 0f), 2f);

            cylinder.Bone = bone;

            AssertApproximately(2f, cylinder.Length);
            AssertApproximately(0.5f, cylinder.Radius);
            AssertApproximately(new Vector3(0f, 0f, 4f), cylinder.WorldEnd);
        }

        [Test]
        public void SegmentsThatAreNotAMultipleOfFour_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateCylinder(Vector3.zero, Vector3.up, 1f, 30));
            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateCylinder(Vector3.zero, Vector3.up, 1f, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateCylinder(Vector3.zero, Vector3.up, 1f, -4));
            Assert.That(container.CreateCylinder(Vector3.zero, Vector3.up, 1f, 4).Segments, Is.EqualTo(4));
            Assert.That(ChunkOf(container).ShapeCount, Is.EqualTo(1));
        }
    }
}
