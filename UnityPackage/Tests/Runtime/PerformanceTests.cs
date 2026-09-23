using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Logs rough CPU timings of the package's own work; it asserts nothing about speed.</summary>
    public class PerformanceTests : WireframesTestBase
    {
        private const int LineCount = 10000;
        private const int SphereCount = 1000;
        private const int BoneCount = 100;
        private const int EditCount = 1000;
        private const int ResizeCount = 100;
        private const int IdleFlushes = 100;

        [Test]
        public void LogLineTimings()
        {
            Transform[] bones = CreateBones();

            // Warm-up so JIT compilation isn't counted.
            LineContainer warmUp = CreateContainer();
            for (int i = 0; i < 100; i++)
            {
                warmUp.CreateLine().BoneA = bones[i % BoneCount];
            }
            warmUp.Proxy.Flush();
            warmUp.Dispose();

            LineContainer container = CreateContainer();
            ILine[] lines = new ILine[LineCount];
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < LineCount; i++)
            {
                lines[i] = container.CreateLine(Random.insideUnitSphere, Random.insideUnitSphere);
                lines[i].BoneA = bones[i % BoneCount];
            }
            double create = Lap(stopwatch);
            container.Proxy.Flush();
            double firstFlush = Lap(stopwatch);

            double idleFlush = TimeIdleFlushes(container, stopwatch);
            double flushAfterBonesMoved = TimeFlushAfterMoving(bones, container, stopwatch);

            for (int i = 0; i < EditCount; i++)
            {
                lines[i * 7919 % LineCount].ColorA = Color.red;
            }
            double edits = Lap(stopwatch);
            container.Proxy.Flush();
            double editFlush = Lap(stopwatch);

            for (int i = 0; i < LineCount; i += 2)
            {
                lines[i].Dispose();
            }
            double dispose = Lap(stopwatch);
            container.Proxy.Flush();
            double disposeFlush = Lap(stopwatch);

            Debug.Log(
                $"[Wireframes timings] {LineCount} lines on {BoneCount} bones: create {create:F2} ms, first flush {firstFlush:F2} ms, " +
                $"idle flush {idleFlush * 1000.0:F1} us, flush after moving every bone {flushAfterBonesMoved * 1000.0:F1} us, " +
                $"{EditCount} scattered color edits {edits:F2} ms + flush {editFlush:F2} ms, " +
                $"dispose {LineCount / 2} lines {dispose:F2} ms + flush {disposeFlush:F2} ms");
            Assert.That(container.Proxy.Chunks[0].ShapeCount, Is.EqualTo(LineCount / 2));
        }

        [Test]
        public void LogSphereTimings()
        {
            Transform[] bones = CreateBones();

            // Warm-up so JIT compilation isn't counted.
            LineContainer warmUp = CreateContainer();
            for (int i = 0; i < 100; i++)
            {
                warmUp.CreateSphere(bones[i % BoneCount], Vector3.zero, 0.5f).Radius = 1f;
            }
            warmUp.Proxy.Flush();
            warmUp.Dispose();

            LineContainer container = CreateContainer();
            ISphere[] spheres = new ISphere[SphereCount];
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < SphereCount; i++)
            {
                spheres[i] = container.CreateSphere(bones[i % BoneCount], Random.insideUnitSphere, 0.5f);
            }
            double create = Lap(stopwatch);
            container.Proxy.Flush();
            double firstFlush = Lap(stopwatch);

            double idleFlush = TimeIdleFlushes(container, stopwatch);
            double flushAfterBonesMoved = TimeFlushAfterMoving(bones, container, stopwatch);

            // Resizing rewrites every vertex of a sphere.
            for (int i = 0; i < ResizeCount; i++)
            {
                spheres[i * 7919 % SphereCount].Radius = 1f;
            }
            double resizes = Lap(stopwatch);
            container.Proxy.Flush();
            double resizeFlush = Lap(stopwatch);

            for (int i = 0; i < SphereCount; i += 2)
            {
                spheres[i].Dispose();
            }
            double dispose = Lap(stopwatch);
            container.Proxy.Flush();
            double disposeFlush = Lap(stopwatch);

            int vertices = SphereCount * ((Sphere)spheres[1]).VertexCount;
            Debug.Log(
                $"[Wireframes timings] {SphereCount} spheres ({vertices} vertices) on {BoneCount} bones: create {create:F2} ms, " +
                $"first flush {firstFlush:F2} ms, idle flush {idleFlush * 1000.0:F1} us, " +
                $"flush after moving every bone {flushAfterBonesMoved * 1000.0:F1} us, " +
                $"{ResizeCount} scattered radius edits {resizes:F2} ms + flush {resizeFlush:F2} ms, " +
                $"dispose {SphereCount / 2} spheres {dispose:F2} ms + flush {disposeFlush:F2} ms");
            Assert.That(container.Proxy.Chunks[0].ShapeCount, Is.EqualTo(SphereCount / 2));
        }

        private Transform[] CreateBones()
        {
            Transform[] bones = new Transform[BoneCount];
            for (int i = 0; i < BoneCount; i++)
            {
                bones[i] = CreateBone(Random.insideUnitSphere * 10f, Random.rotation);
            }
            return bones;
        }

        /// <returns>Milliseconds per flush when nothing changed.</returns>
        private static double TimeIdleFlushes(LineContainer container, Stopwatch stopwatch)
        {
            stopwatch.Restart();
            for (int i = 0; i < IdleFlushes; i++)
            {
                container.Proxy.Flush();
            }
            return Lap(stopwatch) / IdleFlushes;
        }

        /// <returns>Milliseconds for the flush after every bone moved.</returns>
        private static double TimeFlushAfterMoving(Transform[] bones, LineContainer container, Stopwatch stopwatch)
        {
            foreach (Transform bone in bones)
            {
                bone.position += Vector3.up;
            }
            stopwatch.Restart();
            container.Proxy.Flush();
            return Lap(stopwatch);
        }

        private static double Lap(Stopwatch stopwatch)
        {
            double elapsed = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();
            return elapsed;
        }
    }
}
