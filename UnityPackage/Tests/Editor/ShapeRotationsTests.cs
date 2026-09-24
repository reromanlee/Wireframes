using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class ShapeRotationsTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void FromAxis_PointsZAlongTheAxisAndKeepsYClosestToUp()
        {
            Vector3 axis = new(1f, 1f, 0f);

            Quaternion rotation = ShapeRotations.FromAxis(axis, Vector3.up);

            AssertDirection(axis.normalized, rotation * Vector3.forward);
            Assert.That((rotation * Vector3.right).y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That((rotation * Vector3.up).y, Is.GreaterThan(0f));
        }

        [Test]
        public void FromAxis_AlongUp_TurnsZByTheSmallestRotation()
        {
            Quaternion rotation = ShapeRotations.FromAxis(new Vector3(0f, 3f, 0f), Vector3.up);

            AssertDirection(Vector3.up, rotation * Vector3.forward);
            AssertDirection(Vector3.back, rotation * Vector3.up);
        }

        [Test]
        public void FromAxis_WithZeroAxis_IsNoRotation()
        {
            Assert.That(ShapeRotations.FromAxis(Vector3.zero, Vector3.up), Is.EqualTo(Quaternion.identity));
        }

        [Test]
        public void FromNormal_TurnsYOntoTheNormal()
        {
            AssertDirection(Vector3.forward, ShapeRotations.FromNormal(new Vector3(0f, 0f, 2f)) * Vector3.up);
            Assert.That(Quaternion.Angle(Quaternion.identity, ShapeRotations.FromNormal(Vector3.up)), Is.LessThan(1e-3f));
            Assert.That(ShapeRotations.FromNormal(Vector3.zero), Is.EqualTo(Quaternion.identity));
        }

        private static void AssertDirection(Vector3 expected, Vector3 actual)
        {
            Assert.That(Vector3.Distance(expected, actual), Is.LessThan(Tolerance), $"Expected {expected:F4}, got {actual:F4}");
        }
    }
}
