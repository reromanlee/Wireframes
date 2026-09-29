using System;
using System.Collections;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>What a container reports about itself: its statistics, Profiler markers and Profiler counters.</summary>
    public class ProfilingTests : WireframesTestBase
    {
        [Test]
        public void Statistics_CountWhatTheContainerHolds()
        {
            WireframeContainer container = CreateContainer();
            Transform first = CreateBone(Vector3.zero, Quaternion.identity);
            Transform second = CreateBone(Vector3.one, Quaternion.identity);
            ICircle circle = container.CreateCircle(first, Vector3.zero, 1f, 16);
            ILine line = container.CreateLine(first, second);
            container.CreateLine(Vector3.zero, Vector3.one);
            container.Proxy.Flush();

            WireframeStatistics statistics = container.Statistics;
            Assert.That(statistics.ShapeCount, Is.EqualTo(3));
            Assert.That(statistics.HiddenShapeCount, Is.Zero);
            Assert.That(statistics.VertexCount, Is.EqualTo(20));
            Assert.That(statistics.EdgeCount, Is.EqualTo(18));
            Assert.That(statistics.BoneCount, Is.EqualTo(2));
            Assert.That(statistics.ChunkCount, Is.EqualTo(1));
            Assert.That(statistics.CpuMemory, Is.Positive);
            Assert.That(statistics.GpuMemory, Is.Positive);

            circle.IsVisible = false;
            line.Dispose();
            statistics = container.Statistics;

            Assert.That(statistics.ShapeCount, Is.EqualTo(2));
            Assert.That(statistics.HiddenShapeCount, Is.EqualTo(1));
            Assert.That(statistics.VertexCount, Is.EqualTo(18));
            Assert.That(statistics.EdgeCount, Is.EqualTo(1));
            Assert.That(statistics.BoneCount, Is.EqualTo(1));
        }

        [Test]
        public void Statistics_AddUpEveryChunk()
        {
            WireframeContainer container = CreateContainer();
            int lineCount = MeshChunk.MaxVertexCount / 2 + 1;
            for (int i = 0; i < lineCount; i++)
            {
                container.CreateLine();
            }
            container.Proxy.Flush();

            WireframeStatistics statistics = container.Statistics;

            Assert.That(statistics.ChunkCount, Is.EqualTo(2));
            Assert.That(statistics.ShapeCount, Is.EqualTo(lineCount));
            Assert.That(statistics.VertexCount, Is.EqualTo(lineCount * 2));
            Assert.That(statistics.EdgeCount, Is.EqualTo(lineCount));
            Assert.That(statistics.GpuMemory, Is.GreaterThan(ChunkOf(container).GpuMemory));
        }

        [Test]
        public void Statistics_WithoutGraphics_CountShapesButNoGpuMemory()
        {
            WireframeContainer container;
            MeshProxy.SimulateNoGraphics = true;
            try
            {
                container = CreateContainer();
            }
            finally
            {
                MeshProxy.SimulateNoGraphics = false;
            }
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            container.CreateCircle(bone, Vector3.zero, 1f, 16).IsVisible = false;
            container.CreateLine();

            WireframeStatistics statistics = container.Statistics;

            Assert.That(statistics.ShapeCount, Is.EqualTo(2));
            Assert.That(statistics.HiddenShapeCount, Is.EqualTo(1));
            Assert.That(statistics.VertexCount, Is.EqualTo(18));
            Assert.That(statistics.EdgeCount, Is.Zero);
            Assert.That(statistics.BoneCount, Is.EqualTo(1));
            Assert.That(statistics.ChunkCount, Is.Zero);
            Assert.That(statistics.GpuMemory, Is.Zero);
        }

        [Test]
        public void ReadingStatistics_AllocatesNothing()
        {
            WireframeContainer container = CreateContainer();
            container.CreateSphere(Vector3.zero, 1f);
            int shapeCount = 0;
            Read();

            Assert.That(Read, Is.Not.AllocatingGCMemory());
            Assert.That(shapeCount, Is.EqualTo(1));

            void Read()
            {
                shapeCount = container.Statistics.ShapeCount;
            }
        }

        [Test]
        public void StatisticsOfADisposedContainer_Throw()
        {
            WireframeContainer container = CreateContainer();
            container.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = container.Statistics);
        }

        [Test]
        public void Flush_RecordsEachPhaseUnderItsProfilerMarker()
        {
            WireframeContainer container = CreateContainer();
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            ILine line = container.CreateLine(bone, bone);
            container.Proxy.Flush();
            string[] names =
            {
                "Wireframes.Flush", "Wireframes.ReadBones", "Wireframes.UploadBones", "Wireframes.WriteShapes",
                "Wireframes.UploadMesh"
            };
            Recorder[] recorders = Array.ConvertAll(names, Recorder.Get);

            // As the Test Framework counts allocations: on this thread only, which makes the counts readable right away.
            foreach (Recorder recorder in recorders)
            {
                recorder.enabled = false;
                recorder.FilterToCurrentThread();
                recorder.enabled = true;
            }
            bone.position = Vector3.one;
            line.ColorA = Color.red;
            container.Proxy.Flush();
            foreach (Recorder recorder in recorders)
            {
                recorder.enabled = false;
                recorder.CollectFromAllThreads();
            }

            for (int i = 0; i < names.Length; i++)
            {
                Assert.That(recorders[i].sampleBlockCount, Is.EqualTo(1), names[i]);
            }
        }

#if WIREFRAMES_PROFILING_CORE
        [UnityTest]
        public IEnumerator Counters_ReportTheTotalsOfEveryContainer()
        {
            // The counters are updated once per frame, so another test's render earlier in this frame would count.
            yield return null;
            WireframeContainer first = CreateContainer();
            WireframeContainer second = CreateContainer();
            first.CreateLine();
            second.CreateLine();
            second.CreateCircle(Vector3.zero, 1f, 16).IsVisible = false;
            RenderTexture target = Track(new RenderTexture(4, 4, 0));
            Camera camera = Track(new GameObject("Counters Test Camera")).AddComponent<Camera>();
            camera.enabled = false;
            camera.targetTexture = target;
            // Creates the counters, which the recorders below then find.
            camera.Render();

            using ProfilerRecorder shapes = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Wireframes Shapes");
            using ProfilerRecorder edges = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Wireframes Edges");
            using ProfilerRecorder chunks = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Wireframes Chunks");
            yield return null;
            camera.Render();
            yield return null;

            Assert.That(shapes.LastValue, Is.EqualTo(3));
            Assert.That(edges.LastValue, Is.EqualTo(2));
            Assert.That(chunks.LastValue, Is.EqualTo(2));
        }
#endif
    }
}
