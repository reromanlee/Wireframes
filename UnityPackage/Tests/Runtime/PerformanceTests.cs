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
        private const int BoneCount = 100;
        private const int EditCount = 1000;
        private const int IdleFlushes = 100;

        [Test]
        public void LogTimings()
        {
            Transform[] bones = new Transform[BoneCount];
            for (int i = 0; i < BoneCount; i++)
            {
                bones[i] = CreateBone(Random.insideUnitSphere * 10f, Random.rotation);
            }

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

            for (int i = 0; i < IdleFlushes; i++)
            {
                container.Proxy.Flush();
            }
            double idleFlush = Lap(stopwatch) / IdleFlushes;

            foreach (Transform bone in bones)
            {
                bone.position += Vector3.up;
            }
            stopwatch.Restart();
            container.Proxy.Flush();
            double flushAfterBonesMoved = Lap(stopwatch);

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

        private static double Lap(Stopwatch stopwatch)
        {
            double elapsed = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();
            return elapsed;
        }
    }
}
