using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// One skinned mesh holding many shapes. Shapes keep their own state; on each flush the chunk writes the queued
    /// shapes into CPU copies of the vertex and index buffers and uploads only the ranges that changed.
    /// </summary>
    internal sealed class MeshChunk : IDisposable
    {
        private const string ObjectName = "Chunk";
        private const int InitialVertexCapacity = 256;
        private const int InitialShapeCapacity = 64;
        // Index 0xFFFF is left unused because some graphics APIs reserve it for primitive restart.
        private const int MaxUInt16Vertices = ushort.MaxValue;
        // Freed vertex blocks are packed away once they fill half the buffer, but never in small meshes.
        private const int CompactionThreshold = 1024;

        private const int PositionStream = 0;
        private const int ColorStream = 1;
        private const int SkinStream = 2;

        // The layout Unity itself builds for one bone per vertex; skinned meshes must keep this stream arrangement.
        private static readonly VertexAttributeDescriptor[] VertexLayout =
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, PositionStream),
            new(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4, ColorStream),
            new(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt32, 1, SkinStream)
        };

        // Shapes follow arbitrary transforms, so the bounds are fixed and large instead of recomputed every frame.
        private static readonly Bounds FixedBounds = new(Vector3.zero, Vector3.one * 2000000f);

        private const MeshUpdateFlags UploadFlags =
            MeshUpdateFlags.DontValidateIndices |
            MeshUpdateFlags.DontRecalculateBounds |
            MeshUpdateFlags.DontResetBoneBounds;

        private readonly SkinnedMeshRenderer _renderer;
        private readonly Mesh _mesh;

        private readonly VertexAllocator _allocator = new();
        private readonly EdgeList _edges = new();
        private readonly BoneRegistry _bones;

        private readonly DirtyRanges _positionRanges = new();
        private readonly DirtyRanges _colorRanges = new();
        private readonly DirtyRanges _skinRanges = new();
        private readonly DirtyRanges _edgeRanges = new();

        // CPU copies of the three vertex streams, sized to the vertex capacity.
        private Vector3[] _positions = new Vector3[InitialVertexCapacity];
        private Color32[] _colors = new Color32[InitialVertexCapacity];
        private uint[] _skin = new uint[InitialVertexCapacity];
        private ushort[] _shortIndices = Array.Empty<ushort>();
        private Matrix4x4[] _bindposes = Array.Empty<Matrix4x4>();

        private Shape[] _shapes = new Shape[InitialShapeCapacity];
        private int _shapeCount;
        private Shape[] _pending = new Shape[InitialShapeCapacity];
        private int _pendingCount;

        // What the mesh holds right now, so the flush knows when a GPU buffer has to be recreated.
        private int _meshVertexCapacity;
        private int _meshIndexCapacity;
        private IndexFormat _meshIndexFormat;
        private int _meshEdgeCount = -1;

        internal MeshChunk(Transform parent, Material material)
        {
            GameObject chunkObject = new(ObjectName) { hideFlags = HideFlags.NotEditable };
            Transform root = chunkObject.transform;
            root.SetParent(parent, false);

            _bones = new BoneRegistry(root, OnBoneDestroyed);

            _mesh = new Mesh { name = ObjectName };
            _mesh.MarkDynamic();
            _mesh.subMeshCount = 1;
            _mesh.bounds = FixedBounds;

            _renderer = chunkObject.AddComponent<SkinnedMeshRenderer>();
            _renderer.sharedMaterial = material;
            _renderer.rootBone = root;
            _renderer.quality = SkinQuality.Bone1;
            _renderer.updateWhenOffscreen = false;
            _renderer.skinnedMotionVectors = false;
            _renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _renderer.lightProbeUsage = LightProbeUsage.Off;
            _renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            _renderer.allowOcclusionWhenDynamic = false;

            // Creates the buffers and hands the mesh to the renderer.
            Flush();
        }

        internal BoneRegistry Bones
        {
            get => _bones;
        }

        internal Mesh Mesh
        {
            get => _mesh;
        }

        internal SkinnedMeshRenderer Renderer
        {
            get => _renderer;
        }

        internal VertexAllocator Allocator
        {
            get => _allocator;
        }

        internal int ShapeCount
        {
            get => _shapeCount;
        }

        internal int EdgeCount
        {
            get => _edges.Count;
        }

        internal void Add(Shape shape)
        {
            // Capacity is checked before anything changes, so a device limit leaves the chunk untouched.
            EnsureVertexCapacity(_allocator.EndAfterAllocate(shape.VertexCount));
            EnsureEdgeCapacity(_edges.Count + shape.EdgeCount);

            shape.VertexStart = _allocator.Allocate(shape.VertexCount);
            for (int edge = 0; edge < shape.EdgeCount; edge++)
            {
                shape.SetEdgeSlot(edge, _edges.Add(shape, edge));
            }
            if (_shapeCount == _shapes.Length)
            {
                Array.Resize(ref _shapes, _shapeCount * 2);
            }
            shape.ShapeIndex = _shapeCount;
            _shapes[_shapeCount++] = shape;
        }

        internal void Remove(Shape shape)
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
            _allocator.Free(shape.VertexStart, shape.VertexCount);

            int last = --_shapeCount;
            Shape moved = _shapes[last];
            _shapes[shape.ShapeIndex] = moved;
            moved.ShapeIndex = shape.ShapeIndex;
            _shapes[last] = null;
        }

        internal void Enqueue(Shape shape)
        {
            if (_pendingCount == _pending.Length)
            {
                Array.Resize(ref _pending, _pendingCount * 2);
            }
            _pending[_pendingCount++] = shape;
        }

        /// <summary>Writes queued shapes into the buffers and uploads what changed.</summary>
        internal void Flush()
        {
            _bones.PollSleepers();
            if (_allocator.FreeCount >= CompactionThreshold && _allocator.FreeCount * 2 > _allocator.End)
            {
                Compact();
            }
            WritePendingShapes();

            // Bones go first: Unity doesn't range-check bone indices, so skin data must never reference a missing bone.
            UploadBones();
            bool recreated = UploadVertices();
            recreated |= UploadIndices();
            if (recreated)
            {
                // Re-assigning the mesh makes the renderer drop skinning buffers sized for the old one.
                _renderer.sharedMesh = null;
                _renderer.sharedMesh = _mesh;
                _renderer.localBounds = FixedBounds;
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
            Array.Clear(_pending, 0, _pendingCount);
            _pendingCount = 0;
            _bones.Dispose();
            // The chunk's GameObject is a child of the proxy and goes away with it; the mesh is an asset and doesn't.
            UnityObjects.Destroy(_mesh);
        }

        private void OnBoneDestroyed(Transform bone)
        {
            for (int i = 0; i < _shapeCount; i++)
            {
                _shapes[i].OnBoneDestroyed(bone);
            }
        }

        /// <summary>Hands out vertex blocks again from vertex 0, packing live shapes together.</summary>
        private void Compact()
        {
            _allocator.Reset();
            for (int i = 0; i < _shapeCount; i++)
            {
                Shape shape = _shapes[i];
                shape.VertexStart = _allocator.Allocate(shape.VertexCount);
                shape.MarkDirty(DirtyFlags.All);
            }
        }

        private void WritePendingShapes()
        {
            for (int i = 0; i < _pendingCount; i++)
            {
                Shape shape = _pending[i];
                _pending[i] = null;
                if (shape.IsDisposed)
                {
                    continue;
                }

                DirtyFlags dirty = shape.TakeDirty();
                int start = shape.VertexStart;
                int count = shape.VertexCount;
                if ((dirty & DirtyFlags.Positions) != 0)
                {
                    shape.WritePositions(new Span<Vector3>(_positions, start, count));
                    _positionRanges.Add(start, count);
                }
                if ((dirty & DirtyFlags.Colors) != 0)
                {
                    shape.WriteColors(new Span<Color32>(_colors, start, count));
                    _colorRanges.Add(start, count);
                }
                if ((dirty & DirtyFlags.Bones) != 0)
                {
                    shape.WriteBones(new Span<uint>(_skin, start, count));
                    _skinRanges.Add(start, count);
                }
                if ((dirty & DirtyFlags.Edges) != 0)
                {
                    shape.WriteEdges(_edges, _edgeRanges);
                }
            }
            _pendingCount = 0;
        }

        private void UploadBones()
        {
            if (!_bones.IsDirty)
            {
                return;
            }
            Transform[] transforms = _bones.Transforms;
            if (_bindposes.Length != transforms.Length)
            {
                // Endpoints are stored relative to their bone, so every bind pose is identity.
                _bindposes = new Matrix4x4[transforms.Length];
                Array.Fill(_bindposes, Matrix4x4.identity);
                _mesh.bindposes = _bindposes;
            }
            _renderer.bones = transforms;
            _bones.ClearDirty();
        }

        /// <returns>True when the vertex buffer was recreated.</returns>
        private bool UploadVertices()
        {
            int capacity = _positions.Length;
            if (_meshVertexCapacity == capacity)
            {
                UploadRanges(_positionRanges, _positions, PositionStream);
                UploadRanges(_colorRanges, _colors, ColorStream);
                UploadRanges(_skinRanges, _skin, SkinStream);
                return false;
            }

            // A resized buffer starts with garbage, and garbage bone indices crash skinning, so every stream is
            // rewritten in full.
            _mesh.SetVertexBufferParams(capacity, VertexLayout);
            _mesh.SetVertexBufferData(_positions, 0, 0, capacity, PositionStream, UploadFlags);
            _mesh.SetVertexBufferData(_colors, 0, 0, capacity, ColorStream, UploadFlags);
            _mesh.SetVertexBufferData(_skin, 0, 0, capacity, SkinStream, UploadFlags);
            _positionRanges.Clear();
            _colorRanges.Clear();
            _skinRanges.Clear();
            _meshVertexCapacity = capacity;
            // The sub-mesh records the vertex range it uses, so it has to be set again.
            _meshEdgeCount = -1;
            return true;
        }

        private void UploadRanges<T>(DirtyRanges ranges, T[] data, int stream) where T : struct
        {
            int count = ranges.Merge();
            for (int i = 0; i < count; i++)
            {
                RangeInt range = ranges[i];
                _mesh.SetVertexBufferData(data, range.start, range.start, range.length, stream, UploadFlags);
            }
            ranges.Clear();
        }

        /// <returns>True when the index buffer was recreated.</returns>
        private bool UploadIndices()
        {
            int edgeCount = _edges.Count;
            int indexCapacity = _edges.Capacity * 2;
            IndexFormat format = _meshVertexCapacity > MaxUInt16Vertices ? IndexFormat.UInt32 : IndexFormat.UInt16;
            bool recreated = false;

            if (_meshIndexCapacity != indexCapacity || _meshIndexFormat != format)
            {
                _mesh.SetIndexBufferParams(indexCapacity, format);
                _meshIndexCapacity = indexCapacity;
                _meshIndexFormat = format;
                _edgeRanges.Clear();
                UploadIndexRange(0, edgeCount * 2);
                _meshEdgeCount = -1;
                recreated = true;
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
            return recreated;
        }

        private void UploadIndexRange(int start, int count)
        {
            if (count <= 0)
            {
                return;
            }
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
            while (capacity < required)
            {
                capacity *= 2;
            }
            // Positions are the widest stream.
            CheckBufferSize((long)capacity * 12);
            // New elements default to zero, which is bone slot 0 (world space), a valid index for skinning.
            Array.Resize(ref _positions, capacity);
            Array.Resize(ref _colors, capacity);
            Array.Resize(ref _skin, capacity);
        }

        private void EnsureEdgeCapacity(int required)
        {
            int capacity = _edges.Capacity;
            if (required <= capacity)
            {
                return;
            }
            while (capacity < required)
            {
                capacity *= 2;
            }
            // Two 32-bit indices per edge at worst.
            CheckBufferSize((long)capacity * 8);
            _edges.EnsureCapacity(capacity);
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
