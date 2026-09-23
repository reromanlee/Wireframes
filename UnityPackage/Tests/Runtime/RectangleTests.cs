using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class RectangleTests : WireframesTestBase
    {
        [Test]
        public void Rectangle_IsFourCornersFlatInXZ()
        {
            LineContainer container = CreateContainer();
            IRectangle rectangle = container.CreateRectangle(new Vector3(0f, 1f, 0f), new Vector3(2f, 1f, 3f));

            Vector3[] corners = BakeShape(container, rectangle);
            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, rectangle);

            AssertApproximately(new Vector3(0f, 1f, 0f), corners[0]);
            AssertApproximately(new Vector3(2f, 1f, 0f), corners[1]);
            AssertApproximately(new Vector3(2f, 1f, 3f), corners[2]);
            AssertApproximately(new Vector3(0f, 1f, 3f), corners[3]);
            Assert.That(edges, Has.Length.EqualTo(4));
            for (int i = 0; i < 4; i++)
            {
                AssertApproximately(corners[i], edges[i].A);
                AssertApproximately(corners[(i + 1) % 4], edges[i].B);
            }
            Assert.That(rectangle.Size, Is.EqualTo(new Vector2(2f, 3f)));
            AssertApproximately(new Vector3(0f, 1f, 0f), rectangle.WorldCornerA);
            AssertApproximately(new Vector3(2f, 1f, 3f), rectangle.WorldCornerB);
        }

        [Test]
        public void CornersAtDifferentHeights_LieHalfwayBetween()
        {
            IRectangle rectangle = CreateContainer().CreateRectangle(Vector3.zero, new Vector3(2f, 4f, 2f));

            AssertApproximately(new Vector3(0f, 2f, 0f), rectangle.WorldCornerA);
            AssertApproximately(new Vector3(2f, 2f, 2f), rectangle.WorldCornerB);
        }

        [Test]
        public void SettingCorner_MovesItOntoThePlaneAndKeepsTheOtherCorner()
        {
            Quaternion rotation = Quaternion.Euler(0f, 30f, 0f);
            IRectangle rectangle = CreateContainer().CreateRectangle(Vector3.zero, rotation, new Vector2(2f, 2f));
            Vector3 cornerA = rectangle.WorldCornerA;

            // 5 above the plane.
            rectangle.WorldCornerB = rotation * new Vector3(3f, 5f, 4f);

            AssertApproximately(cornerA, rectangle.WorldCornerA);
            AssertApproximately(rotation * new Vector3(3f, 0f, 4f), rectangle.WorldCornerB);
            AssertApproximately(new Vector2(4f, 5f), rectangle.Size);
            AssertApproximately(rotation, rectangle.WorldRotation);
        }

        [Test]
        public void CreateRectangleOnBone_LiesFlatInTheBonesXZ()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(0f, 2f, 0f), Quaternion.Euler(90f, 0f, 0f));

            IRectangle rectangle = container.CreateRectangle(bone, new Vector3(-1f, 0f, -1f), new Vector3(1f, 0f, 1f));

            Assert.That(rectangle.Bone, Is.SameAs(bone));
            foreach (Vector3 corner in BakeShape(container, rectangle))
            {
                AssertApproximately(0f, Vector3.Dot(corner - bone.position, bone.up));
            }
            AssertApproximately(bone.TransformPoint(new Vector3(1f, 0f, 1f)), rectangle.WorldCornerB);
        }

        [Test]
        public void CreateRectangle_WithoutArguments_IsAUnitSquareAtOrigin()
        {
            IRectangle rectangle = CreateContainer().CreateRectangle();

            Assert.That(rectangle.Size, Is.EqualTo(Vector2.one));
            AssertApproximately(new Vector3(-0.5f, 0f, -0.5f), rectangle.WorldCornerA);
        }

        [Test]
        public void ChangingBone_ConvertsTheSize()
        {
            IRectangle rectangle = CreateContainer().CreateRectangle(Vector3.zero, new Vector3(4f, 0f, 2f));
            Transform bone = CreateBone(Vector3.one, Quaternion.Euler(0f, 45f, 0f), 2f);

            rectangle.Bone = bone;

            AssertApproximately(new Vector2(2f, 1f), rectangle.Size);
            AssertApproximately(new Vector3(4f, 0f, 2f), rectangle.WorldCornerB);
        }
    }
}
