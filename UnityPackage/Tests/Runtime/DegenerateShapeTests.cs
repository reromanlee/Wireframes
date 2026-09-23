using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class DegenerateShapeTests : WireframesTestBase
    {
        [Test]
        public void ZeroAndNegativeSizes_NeverProduceInvalidVertices()
        {
            LineContainer container = CreateContainer();
            List<IShape> shapes = new()
            {
                container.CreateBox(Vector3.zero, Vector3.zero),
                container.CreatePolygon(Vector3.zero, Vector3.zero, Vector3.zero),
                container.CreateCircle(Vector3.zero, 0f),
                container.CreateEllipse(Vector3.zero, Vector3.zero, 0f),
                container.CreateStar(Vector3.zero, -1f, 0f, 3),
                container.CreateRectangle(Vector3.zero, Vector3.zero),
                container.CreateRoundedRectangle(Vector3.zero, Vector3.zero, 1f),
                container.CreateSphere(Vector3.zero, 0f),
                container.CreateEllipsoid(Vector3.zero, Vector3.zero, 0f),
                container.CreateSpikedSphere(Vector3.zero, 0f, -1f, 20),
                container.CreateCylinder(Vector3.zero, Vector3.zero, 0f),
                container.CreateCone(Vector3.zero, Vector3.zero, 0f),
                container.CreateCapsule(Vector3.zero, Vector3.zero, 0f),
                container.CreateCapsule(Vector3.zero, Vector3.zero, 1f, 2f),
                container.CreateCapsule(Vector3.zero, Quaternion.identity, -1f, -1f, 3f),
                container.CreateStadium(Vector3.zero, Vector3.zero, 0f),
                container.CreateStadium(Vector3.zero, Vector3.zero, 2f, 1f),
                container.CreateStadium(Vector3.zero, Quaternion.identity, -2f, 1f, -1f),
                container.CreateFrustum(Vector3.zero, Vector3.zero, 0f, 0f, 3),
                container.CreatePyramid(Vector3.zero, Vector3.zero, Vector2.zero)
            };

            Vector3[] baked = FlushAndBake(container);

            foreach (IShape shape in shapes)
            {
                Shape target = (Shape)shape;
                for (int i = target.VertexStart; i < target.VertexStart + target.VertexCount; i++)
                {
                    Assert.That(IsFinite(baked[i]), Is.True, $"{target.GetType().Name} has vertex {baked[i]}");
                }
            }
        }

        [Test]
        public void SettingEndOnTopOfEndA_KeepsEveryLongShapeValid()
        {
            LineContainer container = CreateContainer();
            IAxialShape[] shapes =
            {
                container.CreateCylinder(), container.CreateCone(), container.CreateCapsule(),
                container.CreateStadium(), container.CreateFrustum(), container.CreatePyramid()
            };

            foreach (IAxialShape shape in shapes)
            {
                shape.WorldEnd = shape.WorldPosition;

                AssertApproximately(0f, shape.Length);
                Assert.That(shape.WorldRotation, Is.EqualTo(Quaternion.identity));
            }
            foreach (Vector3 vertex in FlushAndBake(container))
            {
                Assert.That(IsFinite(vertex), Is.True, $"Invalid vertex {vertex}");
            }
        }

        private static bool IsFinite(Vector3 vertex)
        {
            for (int axis = 0; axis < 3; axis++)
            {
                if (float.IsNaN(vertex[axis]) || float.IsInfinity(vertex[axis]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
