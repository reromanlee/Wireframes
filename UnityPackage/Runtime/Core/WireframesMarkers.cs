using Unity.Profiling;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// The package's Profiler markers, all named Wireframes.* in the Render category, so a capture shows where its time
    /// goes. Outside the Editor and development builds they cost next to nothing.
    /// </summary>
    internal static class WireframesMarkers
    {
        /// <summary>Everything a container does right before a render.</summary>
        internal static readonly ProfilerMarker Flush = new(ProfilerCategory.Render, "Wireframes.Flush");

        /// <summary>Reading the local-to-world matrix of every bone in use.</summary>
        internal static readonly ProfilerMarker ReadBones = new(ProfilerCategory.Render, "Wireframes.ReadBones");

        /// <summary>Writing the bone matrices into the bone texture and uploading it.</summary>
        internal static readonly ProfilerMarker UploadBones = new(ProfilerCategory.Render, "Wireframes.UploadBones");

        /// <summary>Writing the queued shapes into the CPU copies of the buffers.</summary>
        internal static readonly ProfilerMarker WriteShapes = new(ProfilerCategory.Render, "Wireframes.WriteShapes");

        /// <summary>Uploading the changed ranges of the vertex and index buffers.</summary>
        internal static readonly ProfilerMarker UploadMesh = new(ProfilerCategory.Render, "Wireframes.UploadMesh");

        /// <summary>Packing a chunk's live shapes together, which rewrites all of them.</summary>
        internal static readonly ProfilerMarker Compact = new(ProfilerCategory.Render, "Wireframes.Compact");

        /// <summary>Growing or shrinking buffers, which copies them and recreates the GPU ones.</summary>
        internal static readonly ProfilerMarker ResizeBuffers = new(ProfilerCategory.Render, "Wireframes.ResizeBuffers");

        /// <summary>Creating a chunk's mesh and GameObject.</summary>
        internal static readonly ProfilerMarker CreateChunk = new(ProfilerCategory.Render, "Wireframes.CreateChunk");

        /// <summary>Releasing chunks that stayed empty.</summary>
        internal static readonly ProfilerMarker ReleaseChunks = new(ProfilerCategory.Render, "Wireframes.ReleaseChunks");
    }
}
