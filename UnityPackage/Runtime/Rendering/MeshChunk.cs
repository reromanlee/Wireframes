using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// One mesh holding many shapes, drawn with its own draw call. Shapes keep their own state; on each flush the chunk
    /// writes the queued shapes into CPU copies of the vertex and index buffers and uploads only the ranges that
    /// changed. Each vertex is a position relative to its bone plus that bone's slot, which the shader turns into a
    /// world position.
    /// </summary>
    /// <remarks>
    /// A chunk grows up to <see cref="MaxVertexCount"/> vertices, so its indices stay 16-bit. A chunk created with more
    /// is a large chunk: it keeps 32-bit indices for a shape bigger than that and never grows.
    /// </remarks>
    internal sealed class MeshChunk : IShapeHost, IDisposable
    {
        /// <summary>
        /// Most vertices a chunk with 16-bit indices holds. Index 0xFFFF stays unused because some graphics APIs reserve
        /// it for primitive restart.
        /// </summary>
        internal const int MaxVertexCount = ushort.MaxValue;

        internal const int InitialVertexCapacity = 256;
        internal const int InitialEdgeCapacity = 128;

        private const string ObjectName = "Chunk";
        private const int InitialShapeCapacity = 64;
        // Freed vertex blocks are packed away once they fill half the buffer, but never in small meshes.
        private const int CompactionThreshold = 1024;
        private const int PositionSize = 12;
        // Position, color and bone slot.
        private const int VertexSize = PositionSize + 4 + 4;

        private const int PositionStream = 0;
        private const int ColorStream = 1;
        private const int BoneStream = 2;

        private static readonly VertexAttributeDescriptor[] VertexLayout =
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, PositionStream),
            new(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4, ColorStream),
            new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 1, BoneStream)
        };

        // Shapes follow arbitrary transforms, so the bounds are fixed and large instead of recomputed every frame.
        private static readonly Bounds FixedBounds = new(Vector3.zero, Vector3.one * 2000000f);

        private const MeshUpdateFlags UploadFlags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds;

        private readonly ChunkAllocator _owner;
        private readonly MeshRenderer _renderer;
        private readonly Mesh _mesh;
        private readonly BoneRegistry _bones;

        private readonly VertexAllocator _allocator = new();
        private readonly EdgeList _edges;

        private readonly DirtyRanges _positionRanges = new();
        private readonly DirtyRanges _colorRanges = new();
        private readonly DirtyRanges _boneRanges = new();
        private readonly DirtyRanges _edgeRanges = new();

        // Growth stops here; shrinking stops at the capacities the chunk was created with.
        private readonly int _vertexLimit;
        private readonly int _minimumVertexCapacity;
        private readonly int _minimumEdgeCapacity;

        // CPU copies of the three vertex streams, sized to the vertex capacity.
        private Vector3[] _positions;
        private Color32[] _colors;
        private float[] _boneIndices;
        private ushort[] _shortIndices = Array.Empty<ushort>();

        private Shape[] _shapes = new Shape[InitialShapeCapacity];
        private int _shapeCount;
        private int _hiddenShapeCount;
        private Shape[] _pending = new Shape[InitialShapeCapacity];
        private int _pendingCount;

        // What the mesh holds right now, so the flush knows when a GPU buffer has to be recreated.
        private int _meshVertexCapacity;
        private int _meshIndexCapacity;
        private IndexFormat _meshIndexFormat;
        private int _meshEdgeCount = -1;

        /// <param name="owner">
        /// The allocator the chunk belongs to, which gives it its parent, layer, bones and materials. Every material draws
        /// all of the mesh; the chunk leaves them to their owner.
        /// </param>
        /// <param name="vertexCapacity">
        /// Vertices the chunk starts with. Past <see cref="MaxVertexCount"/>, it is a large chunk.
        /// </param>
        /// <param name="edgeCapacity">Edges the chunk starts with.</param>
        internal MeshChunk(ChunkAllocator owner, int vertexCapacity, int edgeCapacity)
        {
            _owner = owner;
            _vertexLimit = Math.Max(vertexCapacity, MaxVertexCount);
            // Checked before anything is created, so a device limit leaves nothing behind.
            CheckBufferSize((long)vertexCapacity * PositionSize);
            CheckBufferSize((long)edgeCapacity * 2 * IndexSize);
            _minimumVertexCapacity = vertexCapacity;
            _minimumEdgeCapacity = edgeCapacity;
            _positions = new Vector3[vertexCapacity];
            _colors = new Color32[vertexCapacity];
            _boneIndices = new float[vertexCapacity];
            _edges = new EdgeList(edgeCapacity);
            _bones = owner.Bones;

            // Same flags as the container's GameObject, so an Edit Mode chunk is never saved either.
            Transform parent = owner.Parent;
            GameObject chunkObject = new(ObjectName) { hideFlags = parent.gameObject.hideFlags, layer = owner.Layer };
            chunkObject.transform.SetParent(parent, false);

            // Owned and destroyed by the chunk, so it is kept from saving and from unloading as an unused asset.
            _mesh = new Mesh { name = ObjectName, hideFlags = HideFlags.DontSave };
            _mesh.MarkDynamic();
            _mesh.subMeshCount = 1;
            _mesh.bounds = FixedBounds;
            chunkObject.AddComponent<MeshFilter>().sharedMesh = _mesh;

            _renderer = chunkObject.AddComponent<MeshRenderer>();
            // A renderer with more materials than sub-meshes draws its last sub-mesh once per extra material.
            _renderer.sharedMaterials = owner.Materials;
            _renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _renderer.lightProbeUsage = LightProbeUsage.Off;
            _renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            _renderer.allowOcclusionWhenDynamic = false;

            // Creates the buffers.
            Flush();
        }

        public BoneRegistry Bones
        {
            get => _bones;
        }

        internal Mesh Mesh
        {
            get => _mesh;
        }

        internal MeshRenderer Renderer
        {
            get => _renderer;
        }

        internal VertexAllocator Allocator
        {
            get => _allocator;
        }

        /// <summary>True for a chunk made for a shape bigger than <see cref="MaxVertexCount"/>.</summary>
        internal bool IsLarge
        {
            get => _vertexLimit > MaxVertexCount;
        }

        internal int ShapeCount
        {
            get => _shapeCount;
        }

        internal int HiddenShapeCount
        {
            get => _hiddenShapeCount;
        }

        /// <summary>Vertices that live shapes use, leaving out freed blocks that aren't reused yet.</summary>
        internal int UsedVertexCount
        {
            get => _allocator.End - _allocator.FreeCount;
        }

        /// <summary>Bytes of CPU memory in the chunk's buffers and lists, about.</summary>
        internal long CpuMemory
        {
            get
            {
                // An edge slot holds two indices, its owner and the owner's edge number, then its 16-bit copy.
                long edgeSize = 3 * sizeof(int) + IntPtr.Size + 2 * sizeof(ushort);
                return (long)_positions.Length * VertexSize
                       + _edges.Capacity * edgeSize
                       + (long)(_shapes.Length + _pending.Length) * IntPtr.Size;
            }
        }

        /// <summary>Bytes of GPU memory in the chunk's vertex and index buffers.</summary>
        internal long GpuMemory
        {
            get => (long)_meshVertexCapacity * VertexSize + (long)_meshIndexCapacity * IndexSize;
        }

        /// <summary>Entries in the queue of shapes to write on the next flush, including those emptied by removals.</summary>
        internal int PendingCount
        {
            get => _pendingCount;
        }

        /// <summary>Entries the queue has room for before it grows.</summary>
        internal int PendingCapacity
        {
            get => _pending.Length;
        }

        /// <summary>Vertices the last flush uploaded, counted once for each stream they were uploaded to.</summary>
        internal int UploadedVertexCount { get; private set; }

        /// <summary>Indices the last flush uploaded.</summary>
        internal int UploadedIndexCount { get; private set; }

        internal int EdgeCount
        {
            get => _edges.Count;
        }

        internal int VertexCapacity
        {
            get => _positions.Length;
        }

        internal int EdgeCapacity
        {
            get => _edges.Capacity;
        }

        /// <summary>True once freed vertex blocks take up enough of the chunk to be worth packing away.</summary>
        internal bool NeedsCompaction
        {
            get => _allocator.FreeCount >= CompactionThreshold && _allocator.FreeCount * 2 > _allocator.End;
        }

        /// <summary>
        /// Time the chunk was first seen empty, kept by its <see cref="ChunkAllocator"/>; NaN while it holds shapes.
        /// </summary>
        internal float EmptySince { get; set; } = float.NaN;

        /// <summary>CPU copy of every vertex position, relative to its bone.</summary>
        internal Vector3[] Positions
        {
            get => _positions;
        }

        /// <summary>CPU copy of every vertex's bone slot.</summary>
        internal float[] BoneIndices
        {
            get => _boneIndices;
        }

        private int IndexSize
        {
            get => IsLarge ? sizeof(uint) : sizeof(ushort);
        }

        /// <summary>True when <paramref name="vertexCount"/> vertices fit in the buffers as they are.</summary>
        internal bool HasRoomFor(int vertexCount)
        {
            return _allocator.EndAfterAllocate(vertexCount) <= _positions.Length;
        }

        /// <summary>True when <paramref name="vertexCount"/> vertices fit once the buffers grow to their limit.</summary>
        internal bool CanGrowToFit(int vertexCount)
        {
            return _allocator.EndAfterAllocate(vertexCount) <= _vertexLimit;
        }

        internal void Add(Shape shape)
        {
            // Capacity is checked before anything changes, so a device limit leaves the chunk untouched.
            EnsureVertexCapacity(_allocator.EndAfterAllocate(shape.VertexCount));
            if (!shape.IsHidden)
            {
                EnsureEdgeCapacity(_edges.Count + shape.EdgeCount);
            }

            shape.VertexStart = _allocator.Allocate(shape.VertexCount);
            if (shape.IsHidden)
            {
                _hiddenShapeCount++;
            }
            else
            {
                AddEdges(shape);
            }
            if (_shapeCount == _shapes.Length)
            {
                Array.Resize(ref _shapes, _shapeCount * 2);
            }
            shape.ShapeIndex = _shapeCount;
            _shapes[_shapeCount++] = shape;
            EmptySince = float.NaN;
        }

        public void Remove(Shape shape)
        {
            if (shape.IsHidden)
            {
                _hiddenShapeCount--;
            }
            else
            {
                RemoveEdges(shape);
            }
            _allocator.Free(shape.VertexStart, shape.VertexCount);
            // Its queue entry is emptied, so a shape that comes back is never queued twice.
            int pending = shape.PendingIndex;
            if (pending >= 0 && pending < _pendingCount && _pending[pending] == shape)
            {
                _pending[pending] = null;
            }
            shape.PendingIndex = -1;

            int last = --_shapeCount;
            Shape moved = _shapes[last];
            _shapes[shape.ShapeIndex] = moved;
            moved.ShapeIndex = shape.ShapeIndex;
            _shapes[last] = null;
        }

        public void Show(Shape shape)
        {
            EnsureEdgeCapacity(_edges.Count + shape.EdgeCount);
            AddEdges(shape);
            _hiddenShapeCount--;
            shape.MarkDirty(DirtyFlags.Edges);
        }

        public void Hide(Shape shape)
        {
            RemoveEdges(shape);
            _hiddenShapeCount++;
        }

        public IShapeHost Reattach(Shape shape)
        {
            return _owner.Attach(shape);
        }

        public void Enqueue(Shape shape)
        {
            if (_pendingCount == _pending.Length)
            {
                // Without renders to flush the queue, the entries of removed shapes would pile up in it; each shape of
                // the chunk is queued at most once, so dropping them keeps the queue within the chunk's shape count.
                CompactPending();
                if (_pendingCount == _pending.Length)
                {
                    Array.Resize(ref _pending, _pendingCount * 2);
                }
            }
            shape.PendingIndex = _pendingCount;
            _pending[_pendingCount++] = shape;
        }

        /// <summary>Writes queued shapes into the buffers and uploads what changed.</summary>
        internal void Flush()
        {
            using (WireframesMarkers.WriteShapes.Auto())
            {
                WritePendingShapes();
            }
            using (WireframesMarkers.UploadMesh.Auto())
            {
                UploadedVertexCount = 0;
                UploadedIndexCount = 0;
                UploadVertices();
                UploadIndices();
            }
        }

        /// <summary>
        /// Hands out vertex blocks again from vertex 0, packing live shapes together, and gives back memory that the
        /// packed shapes leave unused. Every shape is written and uploaded again on the next flush.
        /// </summary>
        internal void Compact()
        {
            using (WireframesMarkers.Compact.Auto())
            {
                _allocator.Reset();
                for (int i = 0; i < _shapeCount; i++)
                {
                    Shape shape = _shapes[i];
                    shape.VertexStart = _allocator.Allocate(shape.VertexCount);
                    shape.MarkDirty(DirtyFlags.All);
                }

                int vertexCapacity = ShrunkCapacity(_positions.Length, _allocator.End, _minimumVertexCapacity);
                if (vertexCapacity < _positions.Length)
                {
                    ResizeVertexStreams(vertexCapacity);
                }
                int edgeCapacity = ShrunkCapacity(_edges.Capacity, _edges.Count, _minimumEdgeCapacity);
                if (edgeCapacity < _edges.Capacity)
                {
                    using (WireframesMarkers.ResizeBuffers.Auto())
                    {
                        _edges.Shrink(edgeCapacity);
                    }
                }
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _shapeCount; i++)
            {
                _shapes[i].Detach();
                _shapes[i] = null;
            }
            _shapeCount = 0;
            _hiddenShapeCount = 0;
            Array.Clear(_pending, 0, _pendingCount);
            _pendingCount = 0;
            // Play Mode destroys objects at the end of the frame, and a render before that must not draw the chunk.
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
            // The chunk's GameObject belongs to its owner, which destroys it; the mesh is an asset and goes here.
            UnityObjects.Destroy(_mesh);
        }

        private void WritePendingShapes()
        {
            for (int i = 0; i < _pendingCount; i++)
            {
                Shape shape = _pending[i];
                // Removed shapes leave an empty entry behind.
                if (shape == null)
                {
                    continue;
                }
                _pending[i] = null;
                shape.PendingIndex = -1;
                try
                {
                    WriteShape(shape, shape.TakeDirty());
                }
                catch (Exception exception)
                {
                    // One broken shape must not stop the others from drawing; it keeps what it last wrote.
                    if (!shape.HasReportedProblem)
                    {
                        shape.HasReportedProblem = true;
                        WireframesLog.Error(
                            $"A {shape.GetType().Name} failed to update and was skipped.", exception, _renderer);
                    }
                }
            }
            _pendingCount = 0;
        }

        private void WriteShape(Shape shape, DirtyFlags dirty)
        {
            int start = shape.VertexStart;
            int count = shape.VertexCount;
            if ((dirty & DirtyFlags.Positions) != 0)
            {
                shape.WritePositions(new Span<Vector3>(_positions, start, count));
                _positionRanges.Add(start, count);
                CheckPositions(shape);
            }
            if ((dirty & DirtyFlags.Colors) != 0)
            {
                shape.WriteColors(new Span<Color32>(_colors, start, count));
                _colorRanges.Add(start, count);
            }
            if ((dirty & DirtyFlags.Bones) != 0)
            {
                shape.WriteBoneIndices(new Span<float>(_boneIndices, start, count));
                _boneRanges.Add(start, count);
            }
            // A hidden shape has no edge slots; showing it marks its edges again.
            if ((dirty & DirtyFlags.Edges) != 0 && !shape.IsHidden)
            {
                shape.WriteEdges(_edges, _edgeRanges);
            }
        }

        private void AddEdges(Shape shape)
        {
            for (int edge = 0; edge < shape.EdgeCount; edge++)
            {
                shape.SetEdgeSlot(edge, _edges.Add(shape, edge));
            }
        }

        private void RemoveEdges(Shape shape)
        {
            // The last edge moves into each freed slot; later edges of this same shape may be among them, and
            // their owner callback keeps the shape's slots current while the loop runs.
            for (int edge = 0; edge < shape.EdgeCount; edge++)
            {
                int slot = shape.GetEdgeSlot(edge);
                if (_edges.RemoveAt(slot))
                {
                    _edgeRanges.Add(slot, 1);
                }
            }
        }

        /// <summary>Warns once per shape about positions that aren't finite, in the Editor and development builds.</summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void CheckPositions(Shape shape)
        {
            if (shape.HasReportedProblem)
            {
                return;
            }
            int end = shape.VertexStart + shape.VertexCount;
            for (int i = shape.VertexStart; i < end; i++)
            {
                Vector3 position = _positions[i];
                if (!float.IsFinite(position.x) || !float.IsFinite(position.y) || !float.IsFinite(position.z))
                {
                    shape.HasReportedProblem = true;
                    WireframesLog.Warning(
                        $"A {shape.GetType().Name} has a position that isn't a finite number, so it can't be drawn "
                        + "correctly. Check the positions, sizes and rotations set on it.",
                        _renderer);
                    return;
                }
            }
        }

        /// <summary>Drops the empty entries that removed shapes left in the queue.</summary>
        private void CompactPending()
        {
            int kept = 0;
            for (int i = 0; i < _pendingCount; i++)
            {
                Shape shape = _pending[i];
                if (shape != null)
                {
                    shape.PendingIndex = kept;
                    _pending[kept++] = shape;
                }
            }
            Array.Clear(_pending, kept, _pendingCount - kept);
            _pendingCount = kept;
        }

        private void UploadVertices()
        {
            int capacity = _positions.Length;
            if (_meshVertexCapacity == capacity)
            {
                UploadRanges(_positionRanges, _positions, PositionStream);
                UploadRanges(_colorRanges, _colors, ColorStream);
                UploadRanges(_boneRanges, _boneIndices, BoneStream);
                return;
            }

            // A resized buffer starts with garbage, so every stream is rewritten in full.
            using (WireframesMarkers.ResizeBuffers.Auto())
            {
                _mesh.SetVertexBufferParams(capacity, VertexLayout);
                _mesh.SetVertexBufferData(_positions, 0, 0, capacity, PositionStream, UploadFlags);
                _mesh.SetVertexBufferData(_colors, 0, 0, capacity, ColorStream, UploadFlags);
                _mesh.SetVertexBufferData(_boneIndices, 0, 0, capacity, BoneStream, UploadFlags);
            }
            UploadedVertexCount += capacity * 3;
            _positionRanges.Clear();
            _colorRanges.Clear();
            _boneRanges.Clear();
            _meshVertexCapacity = capacity;
            // The sub-mesh records the vertex range it uses, so it has to be set again.
            _meshEdgeCount = -1;
        }

        private void UploadRanges<T>(DirtyRanges ranges, T[] data, int stream) where T : struct
        {
            int count = ranges.Merge();
            for (int i = 0; i < count; i++)
            {
                RangeInt range = ranges[i];
                _mesh.SetVertexBufferData(data, range.start, range.start, range.length, stream, UploadFlags);
                UploadedVertexCount += range.length;
            }
            ranges.Clear();
        }

        private void UploadIndices()
        {
            int edgeCount = _edges.Count;
            int indexCapacity = _edges.Capacity * 2;
            IndexFormat format = IsLarge ? IndexFormat.UInt32 : IndexFormat.UInt16;

            if (_meshIndexCapacity != indexCapacity || _meshIndexFormat != format)
            {
                using (WireframesMarkers.ResizeBuffers.Auto())
                {
                    _mesh.SetIndexBufferParams(indexCapacity, format);
                    _meshIndexCapacity = indexCapacity;
                    _meshIndexFormat = format;
                    _edgeRanges.Clear();
                    UploadIndexRange(0, edgeCount * 2);
                    _meshEdgeCount = -1;
                }
            }
            else
            {
                int count = _edgeRanges.Merge();
                for (int i = 0; i < count; i++)
                {
                    // Slots freed after they were marked may now lie past the last edge; those aren't drawn.
                    RangeInt range = _edgeRanges[i];
                    int end = Math.Min(range.end, edgeCount);
                    UploadIndexRange(range.start * 2, (end - range.start) * 2);
                }
                _edgeRanges.Clear();
            }

            if (_meshEdgeCount != edgeCount)
            {
                SubMeshDescriptor subMesh = new(0, edgeCount * 2, MeshTopology.Lines)
                {
                    firstVertex = 0,
                    vertexCount = _meshVertexCapacity,
                    bounds = FixedBounds
                };
                _mesh.SetSubMesh(0, subMesh, UploadFlags);
                _meshEdgeCount = edgeCount;
            }
        }

        private void UploadIndexRange(int start, int count)
        {
            if (count <= 0)
            {
                return;
            }
            UploadedIndexCount += count;
            int[] indices = _edges.Indices;
            if (_meshIndexFormat == IndexFormat.UInt32)
            {
                _mesh.SetIndexBufferData(indices, start, start, count, UploadFlags);
                return;
            }
            if (_shortIndices.Length != _meshIndexCapacity)
            {
                _shortIndices = new ushort[_meshIndexCapacity];
            }
            for (int i = start; i < start + count; i++)
            {
                _shortIndices[i] = (ushort)indices[i];
            }
            _mesh.SetIndexBufferData(_shortIndices, start, start, count, UploadFlags);
        }

        private void EnsureVertexCapacity(int required)
        {
            int capacity = _positions.Length;
            if (required <= capacity)
            {
                return;
            }
            if (required > _vertexLimit)
            {
                throw new InvalidOperationException(
                    $"A chunk holds at most {_vertexLimit} vertices, but {required} were requested.");
            }
            capacity = Math.Max(capacity, 1);
            while (capacity < required)
            {
                capacity *= 2;
            }
            ResizeVertexStreams(Math.Min(capacity, _vertexLimit));
        }

        private void ResizeVertexStreams(int capacity)
        {
            using (WireframesMarkers.ResizeBuffers.Auto())
            {
                // New elements default to zero, which is bone slot 0 (world space), a valid index for the shader.
                Array.Resize(ref _positions, capacity);
                Array.Resize(ref _colors, capacity);
                Array.Resize(ref _boneIndices, capacity);
            }
        }

        private void EnsureEdgeCapacity(int required)
        {
            int capacity = _edges.Capacity;
            if (required <= capacity)
            {
                return;
            }
            capacity = Math.Max(capacity, 1);
            while (capacity < required)
            {
                capacity *= 2;
            }
            CheckBufferSize((long)capacity * 2 * IndexSize);
            using (WireframesMarkers.ResizeBuffers.Auto())
            {
                _edges.EnsureCapacity(capacity);
            }
        }

        /// <summary>
        /// The capacity to shrink to: twice what is used once that is at most a quarter, so it takes doubling the load
        /// to grow again, and never below <paramref name="minimum"/>.
        /// </summary>
        private static int ShrunkCapacity(int capacity, int used, int minimum)
        {
            return used <= capacity / 4 ? Math.Min(capacity, Math.Max(minimum, used * 2)) : capacity;
        }

        private static void CheckBufferSize(long bytes)
        {
            // Without a graphics device (batch mode with -nographics) the limit reads as zero.
            long limit = SystemInfo.maxGraphicsBufferSize;
            if (limit > 0 && bytes > limit)
            {
                throw new InvalidOperationException(
                    $"Wireframes needs a {bytes}-byte GPU buffer, but this device allows at most {limit} bytes.");
            }
        }
    }
}
