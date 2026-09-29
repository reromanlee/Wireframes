using System.Diagnostics;
#if WIREFRAMES_PROFILING_CORE
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
#endif

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Profiler counters with the totals of every container, to add to a module in the Profiler's Module Editor. They
    /// exist only in projects with the com.unity.profiling.core package, and count only in the Editor and development
    /// builds.
    /// </summary>
    internal static class WireframesCounters
    {
#if WIREFRAMES_PROFILING_CORE
        private static readonly ProfilerCounterValue<int> Shapes =
            new(ProfilerCategory.Render, "Wireframes Shapes", ProfilerMarkerDataUnit.Count);
        private static readonly ProfilerCounterValue<int> Vertices =
            new(ProfilerCategory.Render, "Wireframes Vertices", ProfilerMarkerDataUnit.Count);
        private static readonly ProfilerCounterValue<int> Edges =
            new(ProfilerCategory.Render, "Wireframes Edges", ProfilerMarkerDataUnit.Count);
        private static readonly ProfilerCounterValue<int> Bones =
            new(ProfilerCategory.Render, "Wireframes Bones", ProfilerMarkerDataUnit.Count);
        private static readonly ProfilerCounterValue<int> Chunks =
            new(ProfilerCategory.Render, "Wireframes Chunks", ProfilerMarkerDataUnit.Count);
        private static readonly ProfilerCounterValue<long> GpuMemory =
            new(ProfilerCategory.Render, "Wireframes GPU Memory", ProfilerMarkerDataUnit.Bytes);

        private static int _lastFrame = -1;
#endif

        /// <summary>Sums every container into the counters, at most once per frame.</summary>
        [Conditional("WIREFRAMES_PROFILING_CORE")]
        internal static void Update()
        {
#if WIREFRAMES_PROFILING_CORE && ENABLE_PROFILER
            int frame = Time.frameCount;
            if (frame == _lastFrame)
            {
                return;
            }
            _lastFrame = frame;
            int shapes = 0;
            int vertices = 0;
            int edges = 0;
            int bones = 0;
            int chunks = 0;
            long gpuMemory = 0;
            IReadOnlyList<MeshProxy> proxies = MeshProxy.LiveProxies;
            for (int i = 0; i < proxies.Count; i++)
            {
                WireframeStatistics statistics = proxies[i].GetStatistics();
                shapes += statistics.ShapeCount;
                vertices += statistics.VertexCount;
                edges += statistics.EdgeCount;
                bones += statistics.BoneCount;
                chunks += statistics.ChunkCount;
                gpuMemory += statistics.GpuMemory;
            }
            Shapes.Value = shapes;
            Vertices.Value = vertices;
            Edges.Value = edges;
            Bones.Value = bones;
            Chunks.Value = chunks;
            GpuMemory.Value = gpuMemory;
#endif
        }
    }
}
