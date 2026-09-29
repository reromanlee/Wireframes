using System;
using NUnit.Framework;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    public class CirclePointsTests
    {
        [Test]
        public void EveryPoint_MatchesItsCosineAndSine([Values(3, 4, 32, 1000, 1024)] int count)
        {
            CirclePoints circle = new(count);

            for (int i = 0; i < count; i++)
            {
                double angle = 2.0 * Math.PI * i / count;
                Vector2 point = circle.Next();
                Assert.That(point.x, Is.EqualTo((float)Math.Cos(angle)).Within(1e-6f), $"Point {i} of {count}");
                Assert.That(point.y, Is.EqualTo((float)Math.Sin(angle)).Within(1e-6f), $"Point {i} of {count}");
            }
        }

        [Test]
        public void First_StartsPartWayAround()
        {
            CirclePoints circle = new(8, 2);

            Vector2 point = circle.Next();

            Assert.That(point.x, Is.EqualTo(0f).Within(1e-6f));
            Assert.That(point.y, Is.EqualTo(1f).Within(1e-6f));
        }
    }
}
