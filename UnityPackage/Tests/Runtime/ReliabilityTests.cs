using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>What keeps the package safe to misuse, usable without a GPU and steady over long runs.</summary>
    public class ReliabilityTests : WireframesTestBase
    {
        [Test]
        public void WithoutGraphics_ShapesWorkButNothingIsCreatedForTheGpu()
        {
            WireframeContainer container;
            MeshProxy.SimulateNoGraphics = true;
            try
            {
                container = CreateContainer(new WireframeContainerSettings { VertexCapacity = 1000 });
            }
            finally
            {
                MeshProxy.SimulateNoGraphics = false;
            }
            Transform bone = CreateBone(new Vector3(1f, 2f, 3f), Quaternion.identity);

            ICircle circle = container.CreateCircle(bone, Vector3.zero, 1f);
            circle.Radius = 2f;
            container.CreateLine(Vector3.zero, Vector3.one).Dispose();
            container.Proxy.Flush();

            Assert.That(container.Proxy.IsHeadless, Is.True);
            Assert.That(container.Proxy.Chunks, Is.Empty);
            Assert.That(container.Proxy.GetComponentsInChildren<Renderer>(), Is.Empty);
            Assert.That(container.Proxy.HeadlessHost.ShapeCount, Is.EqualTo(1));
            AssertApproximately(new Vector3(1f, 2f, 3f), circle.WorldPosition);
            Assert.That(circle.Radius, Is.EqualTo(2f));

            container.Dispose();
            Assert.That(circle.IsDisposed, Is.True);
        }

        [Test]
        public void EdgePatterns_AreSharedAndDroppedWithTheLastShapeUsingThem()
        {
            // A segment count no other test uses.
            const int segmentCount = 997;
            WireframeContainer container = CreateContainer();
            ICircle first = container.CreateCircle(Vector3.zero, 1f, segmentCount);
            ICircle second = container.CreateCircle(Vector3.one, 1f, segmentCount);
            Assert.That(Ring.Patterns.Contains(segmentCount), Is.True);

            first.Dispose();
            Assert.That(Ring.Patterns.Contains(segmentCount), Is.True);
            second.Dispose();
            Assert.That(Ring.Patterns.Contains(segmentCount), Is.False);

            container.CreateSphere(Vector3.zero, 1f, segmentCount);
            Assert.That(AxisRings.Patterns.Contains(segmentCount), Is.True);
            container.Dispose();
            Assert.That(AxisRings.Patterns.Contains(segmentCount), Is.False);
        }

        [Test]
        public void SettingTheSameBoneAgain_ChangesNothing()
        {
            WireframeContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(1f, 2f, 3f), Quaternion.Euler(10f, 20f, 30f), 1.7f);
            ICircle circle = container.CreateCircle(bone, new Vector3(0.1f, 0.2f, 0.3f), 1.3f);
            ILine line = container.CreateLine(bone, bone);
            line.LocalPositionA = new Vector3(0.4f, 0.5f, 0.6f);
            container.Proxy.Flush();

            for (int i = 0; i < 100; i++)
            {
                circle.Bone = bone;
                line.BoneA = bone;
            }

            // Exactly: the round trip through world space that a different bone takes would round them.
            Assert.That(circle.LocalPosition, Is.EqualTo(new Vector3(0.1f, 0.2f, 0.3f)));
            Assert.That(circle.Radius, Is.EqualTo(1.3f));
            Assert.That(line.LocalPositionA, Is.EqualTo(new Vector3(0.4f, 0.5f, 0.6f)));
            Assert.That(ChunkOf(circle).PendingCount, Is.Zero, "A same-bone set queued an upload.");
        }

        [Test]
        public void ResolutionsPastTheLimit_Throw()
        {
            WireframeContainer container = CreateContainer();
            int tooMany = Ring.MaxSegmentCount + 4;

            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateCircle(Vector3.zero, 1f, tooMany));
            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateSphere(Vector3.zero, 1f, tooMany));
            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateCapsule(Vector3.zero, Vector3.up, 1f, tooMany));
            Assert.Throws<ArgumentOutOfRangeException>(() => container.CreateFrustum(Vector3.zero, Vector3.up, 1f, 1f, tooMany));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => container.CreateStar(Vector3.zero, 0.5f, 1f, Star.MaxPointCount + 1));
            Assert.That(ShapeCountOf(container), Is.Zero);

            Assert.That(container.CreateCircle(Vector3.zero, 1f, Ring.MaxSegmentCount).SegmentCount, Is.EqualTo(1024));
            Assert.That(container.CreateStar(Vector3.zero, 0.5f, 1f, Star.MaxPointCount).PointCount, Is.EqualTo(512));
        }

        [Test]
        public void AShapeThatFailsToUpdate_IsReportedOnceAndTheOthersStillUpdate()
        {
            WireframeContainer container = CreateContainer();
            ILine before = container.CreateLine(Vector3.zero, Vector3.one);
            FaultyShape faulty = container.Proxy.Add(new FaultyShape());
            ILine after = container.CreateLine(Vector3.one, Vector3.one * 2f);

            LogAssert.Expect(LogType.Error, new Regex(@"^\[Wireframes\] A FaultyShape failed to update"));
            AssertApproximately(Vector3.one * 2f, BakeShape(container, after)[1]);
            AssertApproximately(Vector3.one, BakeShape(container, before)[1]);

            // A second failure isn't logged; an unexpected error log would fail the test.
            faulty.MarkDirty(DirtyFlags.Positions);
            container.Proxy.Flush();
        }

        [Test]
        public void PositionsThatArentFinite_AreReportedOncePerShape()
        {
            WireframeContainer container = CreateContainer();
            ILine line = container.CreateLine(Vector3.zero, new Vector3(float.NaN, 0f, 0f));
            int warnings = 0;
            void CountWarnings(string message, string stackTrace, LogType type)
            {
                if (type == LogType.Warning && message.Contains("A Line has a position that isn't a finite number"))
                {
                    warnings++;
                }
            }

            Application.logMessageReceived += CountWarnings;
            try
            {
                container.Proxy.Flush();
                line.LocalPositionA = new Vector3(float.PositiveInfinity, 0f, 0f);
                container.Proxy.Flush();
            }
            finally
            {
                Application.logMessageReceived -= CountWarnings;
            }

            Assert.That(warnings, Is.EqualTo(1));
        }

        [Test]
        public void DisposedShapes_DontPileUpWhileNothingFlushes()
        {
            WireframeContainer container = CreateContainer();
            ILine kept = container.CreateLine();

            for (int i = 0; i < 10000; i++)
            {
                container.CreateLine().Dispose();
            }

            MeshChunk chunk = ChunkOf(kept);
            Assert.That(chunk.PendingCount, Is.LessThan(100));
            Assert.That(chunk.Allocator.End, Is.EqualTo(4), "Freed vertex blocks weren't reused.");
        }

        [Test]
        public void CallsFromAnotherThread_Throw()
        {
            WireframeContainer container = CreateContainer();
            ILine line = container.CreateLine();

            AggregateException fromShape = Assert.Throws<AggregateException>(
                () => Task.Run(() => line.ColorA = Color.red).Wait());
            AggregateException fromFactory = Assert.Throws<AggregateException>(
                () => Task.Run(() => container.CreateLine()).Wait());

            Assert.That(fromShape.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(fromFactory.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ShapeCountOf(container), Is.EqualTo(1));
        }

        /// <summary>A shape whose positions always fail to write.</summary>
        private sealed class FaultyShape : Shape
        {
            internal FaultyShape() : base(2, new[] { 0, 1 })
            {
            }

            public override void SetColor(Color color)
            {
            }

            internal override void WritePositions(Span<Vector3> positions)
            {
                throw new InvalidOperationException("Broken on purpose.");
            }

            internal override void WriteColors(Span<Color32> colors)
            {
            }

            internal override void WriteBoneIndices(Span<float> boneIndices)
            {
            }

            protected override void AcquireBones(BoneRegistry bones)
            {
            }

            protected override void ReleaseBones(BoneRegistry bones)
            {
            }
        }
    }
}
