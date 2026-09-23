using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class PyramidTests : WireframesTestBase
    {
        [Test]
        public void Pyramid_IsARectangularBaseJoinedToTheTip()
        {
            LineContainer container = CreateContainer();

            IPyramid pyramid = container.CreatePyramid(Vector3.zero, new Vector3(0f, 0f, 2f), new Vector2(2f, 1f));

            Vector3[] vertices = BakeShape(container, pyramid);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, pyramid);
            Assert.That(vertices, Has.Length.EqualTo(5));
            Assert.That(edges, Has.Length.EqualTo(8));
            AssertApproximately(Vector3.zero, vertices[0]);
            AssertApproximately(new Vector3(-1f, -0.5f, 2f), vertices[1]);
            AssertApproximately(new Vector3(1f, 0.5f, 2f), vertices[3]);
            for (int line = 4; line < 8; line++)
            {
                AssertApproximately(Vector3.zero, edges[line].A);
            }
        }

        [Test]
        public void PointingDown_KeepsTheBaseAlignedWithTheWorldAxes()
        {
            LineContainer container = CreateContainer();

            IPyramid pyramid = container.CreatePyramid(new Vector3(0f, 3f, 0f), Vector3.zero, new Vector2(2f, 2f));

            Vector3[] vertices = BakeShape(container, pyramid);
            for (int corner = 1; corner <= 4; corner++)
            {
                AssertApproximately(1f, Mathf.Abs(vertices[corner].x));
                AssertApproximately(0f, vertices[corner].y);
                AssertApproximately(1f, Mathf.Abs(vertices[corner].z));
            }
        }

        [Test]
        public void CreatePyramid_WithoutArguments_HasAUnitBaseOneUnitAlongZ()
        {
            IPyramid pyramid = CreateContainer().CreatePyramid();

            Assert.That(pyramid.BaseSize, Is.EqualTo(Vector2.one));
            Assert.That(pyramid.WorldEnd, Is.EqualTo(new Vector3(0f, 0f, 1f)));
        }

        [Test]
        public void CreatePyramidOnBone_PutsTheTipOnTheBone()
        {
            LineContainer container = CreateContainer();
            Transform eye = CreateBone(new Vector3(0f, 2f, 0f), Quaternion.Euler(0f, -90f, 0f));

            IPyramid view = container.CreatePyramid(eye, Vector3.zero, new Vector3(0f, 0f, 5f), new Vector2(4f, 3f));

            Assert.That(view.Bone, Is.SameAs(eye));
            AssertApproximately(eye.position, BakeShape(container, view)[0]);
            AssertApproximately(eye.TransformPoint(new Vector3(0f, 0f, 5f)), view.WorldEnd);
        }

        [Test]
        public void ChangingBone_ConvertsLengthAndBaseSize()
        {
            IPyramid pyramid = CreateContainer().CreatePyramid(Vector3.zero, Quaternion.identity, 2f, new Vector2(4f, 2f));

            pyramid.Bone = CreateBone(Vector3.zero, Quaternion.identity, 2f);

            AssertApproximately(1f, pyramid.Length);
            AssertApproximately(new Vector2(2f, 1f), pyramid.BaseSize);
        }
    }
}
