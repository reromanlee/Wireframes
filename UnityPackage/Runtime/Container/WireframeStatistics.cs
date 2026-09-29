namespace reromanlee.Wireframes
{
    /// <summary>
    /// What a container holds and the memory it uses, read from <see cref="WireframeContainer.Statistics"/>. Reading it
    /// walks the container's chunks and allocates nothing.
    /// </summary>
    public readonly struct WireframeStatistics
    {
        internal WireframeStatistics(
            int shapeCount,
            int hiddenShapeCount,
            int vertexCount,
            int edgeCount,
            int boneCount,
            int chunkCount,
            long cpuMemory,
            long gpuMemory)
        {
            ShapeCount = shapeCount;
            HiddenShapeCount = hiddenShapeCount;
            VertexCount = vertexCount;
            EdgeCount = edgeCount;
            BoneCount = boneCount;
            ChunkCount = chunkCount;
            CpuMemory = cpuMemory;
            GpuMemory = gpuMemory;
        }

        /// <summary>Shapes in the container, hidden ones included.</summary>
        public int ShapeCount { get; }

        /// <summary>Shapes hidden with <see cref="IShape.IsVisible"/>.</summary>
        public int HiddenShapeCount { get; }

        /// <summary>Vertices the shapes use.</summary>
        public int VertexCount { get; }

        /// <summary>Edges drawn, which leaves out those of hidden shapes.</summary>
        public int EdgeCount { get; }

        /// <summary>Distinct Transforms the shapes follow.</summary>
        public int BoneCount { get; }

        /// <summary>
        /// Meshes the shapes are split into, of up to 65,535 vertices each. Every chunk is one draw call per camera, or
        /// two with <see cref="WireframeOcclusion.Fade"/>.
        /// </summary>
        public int ChunkCount { get; }

        /// <summary>Bytes of CPU memory in the container's buffers, about; the shapes' own objects aren't counted.</summary>
        public long CpuMemory { get; }

        /// <summary>Bytes of GPU memory in the container's vertex, index and bone buffers.</summary>
        public long GpuMemory { get; }

        public override string ToString()
        {
            return $"Shapes: {ShapeCount} ({HiddenShapeCount} hidden), vertices: {VertexCount}, edges: {EdgeCount}, " +
                   $"bones: {BoneCount}, chunks: {ChunkCount}, CPU memory: {CpuMemory / 1024} KB, " +
                   $"GPU memory: {GpuMemory / 1024} KB";
        }
    }
}
