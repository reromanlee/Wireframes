using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>
    /// Times the package's own CPU work one frame at a time, the edits of the frame plus the flush that uploads them,
    /// and counts the frame's allocations. The Performance Testing package records the results; nothing here asserts a
    /// speed, since that depends on the machine.
    /// </summary>
    public class PerformanceTests : WireframesTestBase
    {
        private const int LineCount = 10000;
        private const int SphereCount = 1000;
        private const int BoneCount = 100;
        private const int EditCount = 1000;
        private const int ResizeCount = 100;
        private const int MeasurementCount = 10;

        private readonly ILine[] _lines = new ILine[LineCount];
        private readonly ISphere[] _spheres = new ISphere[SphereCount];
        private Transform[] _bones;
        private WireframeContainer _container;

        [SetUp]
        public void CreateBones()
        {
            _bones = new Transform[BoneCount];
            for (int i = 0; i < BoneCount; i++)
            {
                _bones[i] = CreateBone(Random.insideUnitSphere * 10f, Random.rotation);
            }
        }

        [Test, Performance]
        public void CreateLines()
        {
            MeasureFrame(null, AddLines, SampleUnit.Millisecond);
        }

        [Test, Performance]
        public void FlushUnchangedLines()
        {
            MeasureFrame(AddLines, () => { }, SampleUnit.Microsecond);
        }

        [Test, Performance]
        public void MoveEveryBone()
        {
            MeasureFrame(AddLines, MoveBones, SampleUnit.Microsecond);
        }

        [Test, Performance]
        public void EditLineColors()
        {
            MeasureFrame(AddLines, () =>
            {
                for (int i = 0; i < EditCount; i++)
                {
                    _lines[i * 7919 % LineCount].ColorA = Color.red;
                }
            }, SampleUnit.Millisecond);
        }

        [Test, Performance]
        public void DisposeHalfTheLines()
        {
            MeasureFrame(AddLines, () =>
            {
                for (int i = 0; i < LineCount; i += 2)
                {
                    _lines[i].Dispose();
                }
            }, SampleUnit.Millisecond);
        }

        [Test, Performance]
        public void CreateSpheres()
        {
            MeasureFrame(null, AddSpheres, SampleUnit.Millisecond);
        }

        [Test, Performance]
        public void ResizeSpheres()
        {
            // Resizing rewrites every vertex of a sphere.
            MeasureFrame(AddSpheres, () =>
            {
                for (int i = 0; i < ResizeCount; i++)
                {
                    _spheres[i * 7919 % SphereCount].Radius += 0.1f;
                }
            }, SampleUnit.Millisecond);
        }

        [Test, Performance]
        public void DisposeHalfTheSpheres()
        {
            MeasureFrame(AddSpheres, () =>
            {
                for (int i = 0; i < SphereCount; i += 2)
                {
                    _spheres[i].Dispose();
                }
            }, SampleUnit.Millisecond);
        }

        /// <summary>
        /// Measures <paramref name="frame"/> and the flush after it, each time in a new container that
        /// <paramref name="fill"/> filled and flushed beforehand.
        /// </summary>
        private void MeasureFrame(Action fill, Action frame, SampleUnit unit)
        {
            Measure.Method(() =>
                {
                    frame();
                    _container.Proxy.Flush();
                })
                .SetUp(() =>
                {
                    _container = CreateContainer();
                    fill?.Invoke();
                    _container.Proxy.Flush();
                })
                .CleanUp(() => _container.Dispose())
                .WarmupCount(1)
                .MeasurementCount(MeasurementCount)
                .SampleGroup(new SampleGroup("Frame", unit))
                .GC()
                .Run();
        }

        private void AddLines()
        {
            for (int i = 0; i < LineCount; i++)
            {
                _lines[i] = _container.CreateLine(_bones[i % BoneCount], _bones[(i + 1) % BoneCount]);
            }
        }

        private void AddSpheres()
        {
            for (int i = 0; i < SphereCount; i++)
            {
                _spheres[i] = _container.CreateSphere(_bones[i % BoneCount], Random.insideUnitSphere, 0.5f);
            }
        }

        private void MoveBones()
        {
            foreach (Transform bone in _bones)
            {
                bone.position += Vector3.up;
            }
        }
    }
}
