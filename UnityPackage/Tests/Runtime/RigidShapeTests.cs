using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Behavior every rigid shape shares, checked through a circle.</summary>
    public class RigidShapeTests : WireframesTestBase
    {
        [Test]
        public void LocalRotation_IsNormalized_AndZeroMeansNoRotation()
        {
            ICircle circle = CreateContainer().CreateCircle();
            Quaternion rotation = Quaternion.Euler(10f, 20f, 30f);

            circle.LocalRotation = new Quaternion(rotation.x * 3f, rotation.y * 3f, rotation.z * 3f, rotation.w * 3f);
            AssertApproximately(rotation, circle.LocalRotation);
            AssertApproximately(1f, Quaternion.Dot(circle.LocalRotation, circle.LocalRotation));

            circle.LocalRotation = default;
            Assert.That(circle.LocalRotation, Is.EqualTo(Quaternion.identity));
        }

        [Test]
        public void WorldPose_IsConvertedThroughBone()
        {
            Transform bone = CreateBone(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 90f, 0f), 2f);
            ICircle circle = CreateContainer().CreateCircle(bone, Vector3.zero, 1f);
            Quaternion worldRotation = Quaternion.Euler(30f, 0f, 0f);

            circle.WorldPosition = new Vector3(5f, 5f, 5f);
            circle.WorldRotation = worldRotation;

            AssertApproximately(bone.InverseTransformPoint(new Vector3(5f, 5f, 5f)), circle.LocalPosition);
            AssertApproximately(Quaternion.Inverse(bone.rotation) * worldRotation, circle.LocalRotation);
            AssertApproximately(new Vector3(5f, 5f, 5f), circle.WorldPosition);
            AssertApproximately(worldRotation, circle.WorldRotation);
        }

        [Test]
        public void ChangingBone_KeepsWorldPositionRotationAndSize()
        {
            LineContainer container = CreateContainer();
            Quaternion rotation = Quaternion.Euler(10f, 20f, 30f);
            ICircle circle = container.CreateCircle(new Vector3(1f, 2f, 3f), rotation, 5f);
            Vector3[] before = BakeShape(container, circle);
            Transform bone = CreateBone(new Vector3(4f, 0f, 0f), Quaternion.Euler(0f, 30f, 0f), 2f);

            circle.Bone = bone;

            AssertApproximately(new Vector3(1f, 2f, 3f), circle.WorldPosition);
            AssertApproximately(rotation, circle.WorldRotation);
            AssertApproximately(2.5f, circle.Radius);
            Vector3[] after = BakeShape(container, circle);
            for (int i = 0; i < before.Length; i++)
            {
                AssertApproximately(before[i], after[i]);
            }
        }

        [Test]
        public void ChangingBone_ConvertsSizeByTheBonesAverageScale()
        {
            ICircle circle = CreateContainer().CreateCircle(Vector3.zero, 8f);
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            bone.localScale = new Vector3(1f, 2f, 4f);

            circle.Bone = bone;
            AssertApproximately(4f, circle.Radius);

            circle.Bone = null;
            AssertApproximately(8f, circle.Radius);
        }

        [Test]
        public void Shape_MovesTurnsAndScalesWithItsBone()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            ICircle circle = container.CreateCircle(bone, new Vector3(0f, 1f, 0f), 2f, 8);

            bone.SetPositionAndRotation(new Vector3(3f, 0f, -2f), Quaternion.Euler(0f, 0f, 90f));
            bone.localScale = Vector3.one * 3f;
            Vector3[] vertices = BakeShape(container, circle);

            for (int i = 0; i < vertices.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / vertices.Length;
                Vector3 local = new Vector3(0f, 1f, 0f) + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * 2f;
                AssertApproximately(bone.TransformPoint(local), vertices[i]);
            }
        }

        [UnityTest]
        public IEnumerator DestroyedBone_LeavesShapeInPlaceAtItsSize()
        {
            Transform bone = CreateBone(new Vector3(2f, 0f, 0f), Quaternion.Euler(0f, 45f, 0f), 2f);
            ICircle circle = CreateContainer().CreateCircle(bone, new Vector3(0f, 0f, 1f), 1f);
            Vector3 position = circle.WorldPosition;
            Quaternion rotation = circle.WorldRotation;

            Object.Destroy(bone.gameObject);
            yield return null;

            Assert.That(circle.Bone, Is.Null);
            AssertApproximately(position, circle.WorldPosition);
            AssertApproximately(rotation, circle.WorldRotation);
            AssertApproximately(2f, circle.Radius);
        }

        [Test]
        public void Color_IsUploadedToEveryVertex()
        {
            LineContainer container = CreateContainer();
            ICircle circle = container.CreateCircle(Vector3.zero, 1f, 6);
            int start = ((Circle)circle).VertexStart;

            circle.Color = Color.red;
            container.Proxy.Flush();
            Color32[] colors = ChunkOf(container).Mesh.colors32;
            for (int i = 0; i < 6; i++)
            {
                Assert.That(colors[start + i], Is.EqualTo((Color32)Color.red));
            }

            circle.SetColor(Color.green);
            container.Proxy.Flush();
            colors = ChunkOf(container).Mesh.colors32;
            Assert.That(circle.Color, Is.EqualTo(Color.green));
            Assert.That(colors[start + 5], Is.EqualTo((Color32)Color.green));
        }

        [Test]
        public void DisposedShape_ThrowsFromEveryMember()
        {
            ICircle circle = CreateContainer().CreateCircle();

            circle.Dispose();

            Assert.That(circle.IsDisposed, Is.True);
            Assert.Throws<ObjectDisposedException>(() => _ = circle.LocalPosition);
            Assert.Throws<ObjectDisposedException>(() => circle.WorldRotation = Quaternion.identity);
            Assert.Throws<ObjectDisposedException>(() => circle.Bone = null);
            Assert.Throws<ObjectDisposedException>(() => circle.Color = Color.red);
            Assert.Throws<ObjectDisposedException>(() => _ = circle.Segments);
            Assert.Throws<ObjectDisposedException>(() => circle.Radius = 1f);
            Assert.DoesNotThrow(() => circle.Dispose());
        }
    }
}
