using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of every shape. It owns the shape's vertex block and edges inside a chunk, queues edits for the next
    /// flush and handles disposal, so derived shapes only describe their geometry.
    /// </summary>
    internal abstract class Shape : IShape, IEdgeOwner
    {
        private readonly int[] _edgePattern;
        private readonly int[] _edgeSlots;
        private MeshChunk _chunk;
        private DirtyFlags _dirty;

        /// <param name="edgePattern">Pairs of local vertex indices, one pair per edge.</param>
        protected Shape(MeshProxy proxy, int vertexCount, int[] edgePattern)
        {
            VertexCount = vertexCount;
            _edgePattern = edgePattern;
            _edgeSlots = new int[edgePattern.Length / 2];
            _chunk = proxy.Attach(this);
            MarkDirty(DirtyFlags.All);
        }

        public bool IsDisposed
        {
            get => _chunk == null;
        }

        /// <summary>The chunk holding the shape, or null once it is disposed.</summary>
        internal MeshChunk Chunk
        {
            get => _chunk;
        }

        internal int VertexCount { get; }

        internal int VertexStart { get; set; }

        /// <summary>Position in the chunk's shape list.</summary>
        internal int ShapeIndex { get; set; }

        internal int EdgeCount
        {
            get => _edgeSlots.Length;
        }

        /// <summary>The registry of the bones this shape follows.</summary>
        protected BoneRegistry Bones
        {
            get => _chunk.Bones;
        }

        public void Dispose()
        {
            if (_chunk == null)
            {
                return;
            }
            MeshChunk chunk = _chunk;
            chunk.Remove(this);
            ReleaseBones(chunk.Bones);
            _chunk = null;
        }

        public abstract void SetColor(Color color);

        internal int GetEdgeSlot(int edge)
        {
            return _edgeSlots[edge];
        }

        internal void SetEdgeSlot(int edge, int slot)
        {
            _edgeSlots[edge] = slot;
        }

        void IEdgeOwner.OnEdgeMoved(int edge, int slot)
        {
            _edgeSlots[edge] = slot;
        }

        /// <summary>Queues the shape for the next flush; it is queued once no matter how many edits it gets.</summary>
        internal void MarkDirty(DirtyFlags flags)
        {
            if (_dirty == DirtyFlags.None)
            {
                _chunk.Enqueue(this);
            }
            _dirty |= flags;
        }

        internal DirtyFlags TakeDirty()
        {
            DirtyFlags dirty = _dirty;
            _dirty = DirtyFlags.None;
            return dirty;
        }

        /// <summary>Marks the shape disposed when its whole chunk goes away.</summary>
        internal void Detach()
        {
            _chunk = null;
        }

        internal void WriteEdges(EdgeList edges, DirtyRanges ranges)
        {
            for (int i = 0; i < _edgeSlots.Length; i++)
            {
                int slot = _edgeSlots[i];
                edges.Set(slot, VertexStart + _edgePattern[i * 2], VertexStart + _edgePattern[i * 2 + 1]);
                ranges.Add(slot, 1);
            }
        }

        internal abstract void WritePositions(Span<Vector3> positions);

        internal abstract void WriteColors(Span<Color32> colors);

        internal abstract void WriteBoneIndices(Span<float> boneIndices);

        protected abstract void ReleaseBones(BoneRegistry bones);

        /// <summary>
        /// Throws once the shape is disposed, and otherwise lets it notice a bone that was destroyed, so every member
        /// sees the shape in world space from then on.
        /// </summary>
        protected void EnsureUsable()
        {
            if (_chunk == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            DetachFromDestroyedBones();
        }

        /// <summary>
        /// Switches whatever follows a destroyed bone to world space, where its bone slot's last matrix put it. Until
        /// then that frozen matrix keeps drawing it in place.
        /// </summary>
        protected virtual void DetachFromDestroyedBones()
        {
        }

        /// <summary>Points a bone field at <paramref name="value"/> and moves its registry slot along.</summary>
        protected void ReplaceBone(ref Transform bone, ref int slot, Transform value)
        {
            // Destroyed transforms compare equal to null; they are stored as null so they are never read again.
            if (value == null)
            {
                value = null;
            }
            // Acquiring first keeps a bone that is re-assigned from being released and re-registered in between.
            int newSlot = _chunk.Bones.Acquire(value);
            _chunk.Bones.Release(slot);
            bone = value;
            slot = newSlot;
        }

        /// <summary>True when <paramref name="bone"/> was a transform that has since been destroyed.</summary>
        protected static bool IsDestroyed(Transform bone)
        {
            return !ReferenceEquals(bone, null) && bone == null;
        }

        protected static Vector3 ToWorld(Transform bone, Vector3 local)
        {
            return bone != null ? bone.TransformPoint(local) : local;
        }

        protected static Vector3 ToLocal(Transform bone, Vector3 world)
        {
            return bone != null ? bone.InverseTransformPoint(world) : world;
        }
    }
}
