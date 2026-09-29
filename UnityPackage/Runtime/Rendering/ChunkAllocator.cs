using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A container's chunks. Every new shape goes where room already exists, else into a chunk that can grow, else
    /// into a new chunk, so a buffer only grows or appears when the others are full; a shape too big for any chunk
    /// gets a large chunk of its own. Chunks reserved by the capacity settings stay for the container's lifetime, and
    /// the others are released after staying empty for <see cref="ReleaseDelay"/> seconds.
    /// </summary>
    internal sealed class ChunkAllocator : IDisposable
    {
        /// <summary>Seconds an unreserved chunk stays empty before it is released, so passing gaps don't churn chunks.</summary>
        internal const float ReleaseDelay = 5f;

        private readonly List<MeshChunk> _chunks = new();
        private readonly Transform _parent;
        private readonly Material[] _materials;
        private readonly int _layer;
        private readonly BoneRegistry _bones;
        private readonly MaterialPropertyBlock _propertyBlock = new();
        // The first chunks are the reserved ones; releasing others keeps them in front.
        private int _reservedCount;
        private int _lastCompactionFrame = -1;

        /// <param name="parent">Transform the chunks' GameObjects are created under; destroying it destroys them.</param>
        /// <param name="materials">Materials every chunk draws with; the allocator leaves them to their owner.</param>
        internal ChunkAllocator(Transform parent, Material[] materials, int layer, BoneRegistry bones)
        {
            _parent = parent;
            _materials = materials;
            _layer = layer;
            _bones = bones;
        }

        internal IReadOnlyList<MeshChunk> Chunks
        {
            get => _chunks;
        }

        /// <summary>
        /// Creates chunks with room for <paramref name="vertexCount"/> vertices and <paramref name="edgeCount"/> edges,
        /// split evenly between as few chunks as that takes. They are never released.
        /// </summary>
        internal void Reserve(int vertexCount, int edgeCount)
        {
            if (vertexCount <= 0 && edgeCount <= 0)
            {
                return;
            }
            int chunkCount = Math.Max(1, CeilingDivide(vertexCount, MeshChunk.MaxVertexCount));
            int vertices = Math.Max(MeshChunk.InitialVertexCapacity, CeilingDivide(vertexCount, chunkCount));
            int edges = Math.Max(MeshChunk.InitialEdgeCapacity, CeilingDivide(edgeCount, chunkCount));
            for (int i = 0; i < chunkCount; i++)
            {
                CreateChunk(vertices, edges);
                _reservedCount++;
            }
        }

        /// <summary>Adds <paramref name="shape"/> to a chunk and returns that chunk.</summary>
        internal MeshChunk Attach(Shape shape)
        {
            int vertexCount = shape.VertexCount;
            MeshChunk chunk = FindChunk(vertexCount);
            if (chunk == null)
            {
                chunk = vertexCount > MeshChunk.MaxVertexCount
                    ? CreateChunk(vertexCount, Math.Max(MeshChunk.InitialEdgeCapacity, shape.EdgeCount))
                    : CreateChunk(GrownCapacity(MeshChunk.InitialVertexCapacity, vertexCount), MeshChunk.InitialEdgeCapacity);
            }
            chunk.Add(shape);
            return chunk;
        }

        /// <summary>Hands the bone texture to every chunk, including the ones created later.</summary>
        internal void SetBoneTexture(Texture texture)
        {
            _propertyBlock.SetTexture(BoneTexture.PropertyId, texture);
            for (int i = 0; i < _chunks.Count; i++)
            {
                _chunks[i].Renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        /// <summary>Compacts at most one chunk per frame, then writes and uploads every chunk's queued shapes.</summary>
        internal void Flush()
        {
            CompactOneChunk();
            for (int i = 0; i < _chunks.Count; i++)
            {
                _chunks[i].Flush();
            }
        }

        /// <summary>
        /// Releases the unreserved chunks that stayed empty for <see cref="ReleaseDelay"/> seconds up to
        /// <paramref name="time"/>. Their GameObjects are destroyed with <see cref="UnityObjects.Destroy"/>, so this must
        /// not run where Unity forbids destroying objects right away, such as in a render callback in Edit Mode.
        /// </summary>
        /// <param name="time">Seconds on a clock that keeps running while the game is paused.</param>
        internal void ReleaseIdleChunks(float time)
        {
            for (int i = _chunks.Count - 1; i >= _reservedCount; i--)
            {
                MeshChunk chunk = _chunks[i];
                if (chunk.ShapeCount > 0)
                {
                    continue;
                }
                if (float.IsNaN(chunk.EmptySince))
                {
                    chunk.EmptySince = time;
                    continue;
                }
                if (time - chunk.EmptySince >= ReleaseDelay)
                {
                    _chunks.RemoveAt(i);
                    chunk.Dispose();
                    UnityObjects.Destroy(chunk.Renderer.gameObject);
                }
            }
        }

        /// <summary>Disposes every chunk and its shapes. The chunks' GameObjects go away with the parent.</summary>
        public void Dispose()
        {
            for (int i = 0; i < _chunks.Count; i++)
            {
                _chunks[i].Dispose();
            }
            _chunks.Clear();
            _reservedCount = 0;
        }

        private MeshChunk FindChunk(int vertexCount)
        {
            // Oldest chunks first, so newer ones are the ones that empty out and get released.
            bool isLarge = vertexCount > MeshChunk.MaxVertexCount;
            for (int i = 0; i < _chunks.Count; i++)
            {
                MeshChunk chunk = _chunks[i];
                // An idle large chunk is reused only when the shape fills at least half of it.
                if (chunk.IsLarge == isLarge && chunk.HasRoomFor(vertexCount)
                    && (!isLarge || chunk.VertexCapacity <= vertexCount * 2L))
                {
                    return chunk;
                }
            }
            if (isLarge)
            {
                return null;
            }
            for (int i = 0; i < _chunks.Count; i++)
            {
                MeshChunk chunk = _chunks[i];
                if (!chunk.IsLarge && chunk.CanGrowToFit(vertexCount))
                {
                    return chunk;
                }
            }
            return null;
        }

        private MeshChunk CreateChunk(int vertexCapacity, int edgeCapacity)
        {
            MeshChunk chunk = new(_parent, _materials, _layer, _bones, vertexCapacity, edgeCapacity);
            chunk.Renderer.SetPropertyBlock(_propertyBlock);
            _chunks.Add(chunk);
            return chunk;
        }

        private void CompactOneChunk()
        {
            // Compacting rewrites a whole chunk, so spreading it over frames keeps each frame's cost bounded.
            int frame = Time.frameCount;
            if (frame == _lastCompactionFrame)
            {
                return;
            }
            for (int i = 0; i < _chunks.Count; i++)
            {
                if (_chunks[i].NeedsCompaction)
                {
                    _chunks[i].Compact();
                    _lastCompactionFrame = frame;
                    return;
                }
            }
        }

        /// <summary><paramref name="capacity"/> doubled until it holds <paramref name="required"/>, up to a chunk's limit.</summary>
        private static int GrownCapacity(int capacity, int required)
        {
            while (capacity < required)
            {
                capacity *= 2;
            }
            return Math.Min(capacity, MeshChunk.MaxVertexCount);
        }

        private static int CeilingDivide(int value, int divisor)
        {
            return (int)((value + (long)divisor - 1) / divisor);
        }
    }
}
