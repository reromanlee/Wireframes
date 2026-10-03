using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Shapes taken out of their container without being disposed, and put back there or into another one.</summary>
    public class ShapeTransferTests : WireframesTestBase
    {
        [Test]
        public void SuspendedShape_GivesBackItsRoomAndResumesAsItWas()
        {
            WireframeContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(1f, 2f, 3f), Quaternion.identity);
            ISphere sphere = container.CreateSphere(bone, Vector3.zero, 1f, 8);
            ILine kept = container.CreateLine(Vector3.zero, Vector3.one);
            container.Proxy.Flush();
            Shape shape = (Shape)sphere;

            shape.Suspend();
            container.Proxy.Flush();

            Assert.That(shape.IsSuspended, Is.True);
            Assert.That(sphere.IsDisposed, Is.False);
            Assert.That(ShapeCountOf(container), Is.EqualTo(1));
            Assert.That(container.Proxy.Bones.Count, Is.Zero);
            Assert.That(ChunkOf(kept).Mesh.GetIndices(0), Has.Length.EqualTo(2));

            shape.Resume(container.Proxy);

            Assert.That(shape.IsSuspended, Is.False);
            Assert.That(sphere.Radius, Is.EqualTo(1f));
            // The first circle starts on the sphere's +Z.
            AssertApproximately(new Vector3(1f, 2f, 4f), BakeShape(container, sphere)[0]);
            Assert.That(ChunkOf(kept).Mesh.GetIndices(0), Has.Length.EqualTo(2 + 24 * 2));
        }

        [Test]
        public void ResumingInAnotherContainer_MovesTheShapeThere()
        {
            WireframeContainer first = CreateContainer();
            WireframeContainer second = CreateContainer();
            Transform bone = CreateBone(new Vector3(0f, 1f, 0f), Quaternion.identity);
            ICircle circle = first.CreateCircle(bone, Vector3.zero, 2f, 16);
            Shape shape = (Shape)circle;

            shape.Suspend();
            shape.Resume(second.Proxy);

            Assert.That(ShapeCountOf(first), Is.Zero);
            Assert.That(ShapeCountOf(second), Is.EqualTo(1));
            Assert.That(first.Proxy.Bones.Count, Is.Zero);
            Assert.That(second.Proxy.Bones.Count, Is.EqualTo(1));
            AssertApproximately(new Vector3(0f, 1f, 2f), BakeShape(second, circle)[0]);
        }

        [Test]
        public void SuspendingAndResuming_AllocatesNothing()
        {
            WireframeContainer first = CreateContainer();
            WireframeContainer second = CreateContainer();
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            Shape shape = (Shape)first.CreateSphere(bone, Vector3.zero, 1f);
            first.Proxy.Flush();

            void MoveThereAndBack()
            {
                shape.Suspend();
                shape.Resume(second.Proxy);
                second.Proxy.Flush();
                shape.Suspend();
                shape.Resume(first.Proxy);
                first.Proxy.Flush();
            }

            // Warms up every path, and lets each list grow once to what a round trip needs between flushes.
            MoveThereAndBack();
            MoveThereAndBack();

            Assert.That(MoveThereAndBack, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void DisposingASuspendedShape_ReleasesItsEdgePattern()
        {
            WireframeContainer container = CreateContainer();
            // A segment count no other test uses, so no other shape shares the pattern.
            ICircle circle = container.CreateCircle(Vector3.zero, 1f, 7);
            Shape shape = (Shape)circle;
            shape.Suspend();

            Assert.That(Ring.Patterns.Contains(7), Is.True);

            circle.Dispose();

            Assert.That(circle.IsDisposed, Is.True);
            Assert.That(shape.IsSuspended, Is.False);
            Assert.That(Ring.Patterns.Contains(7), Is.False);
        }

        [Test]
        public void BoneDestroyedWhileSuspended_LeavesTheShapeInWorldSpaceOnceResumed()
        {
            WireframeContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(5f, 0f, 0f), Quaternion.identity);
            ICircle circle = container.CreateCircle(bone, Vector3.zero, 1f);
            Shape shape = (Shape)circle;
            shape.Suspend();

            Object.DestroyImmediate(bone.gameObject);
            shape.Resume(container.Proxy);

            Assert.That(circle.Bone, Is.Null);
            Assert.That(container.Proxy.Bones.Count, Is.Zero);
        }
    }
}
