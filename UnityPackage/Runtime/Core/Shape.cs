using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of every shape. Derived constructors only check and store the shape's state; <see cref="Attach"/> then takes
    /// its bones and edge pattern and hands it to a host, so a shape that fails to build leaves nothing behind. It
    /// queues edits for the next flush and handles disposal, so derived shapes only describe their geometry.
    /// </summary>
    internal abstract class Shape : IShape, IEdgeOwner
    {
        private readonly EdgeSource _edgeSource;
        private int[] _edgePattern;
        private int[] _edgeSlots;
        private IShapeHost _host;
        private DirtyFlags _dirty;

        protected Shape(int vertexCount, EdgeSource edgeSource)
        {
            VertexCount = vertexCount;
            _edgeSource = edgeSource;
        }

        public bool IsDisposed
        {
            get => _host == null;
        }

        /// <summary>The chunk that draws the shape, or null when it is disposed or nothing can draw it.</summary>
        internal MeshChunk Chunk
        {
            get => _host as MeshChunk;
        }

        internal int VertexCount { get; }

        internal int VertexStart { get; set; }

        /// <summary>Position in its host's shape list.</summary>
        internal int ShapeIndex { get; set; }

        internal int EdgeCount
        {
            get => _edgeSlots.Length;
        }

        /// <summary>True once a problem with the shape was logged, so each shape logs at most one.</summary>
        internal bool HasReportedProblem { get; set; }

        /// <summary>The registry of the bones this shape follows.</summary>
        protected BoneRegistry Bones
        {
            get => _host.Bones;
        }

        public void Dispose()
        {
            MainThread.Check();
            if (_host == null)
            {
                return;
            }
            IShapeHost host = _host;
            host.Remove(this);
            ReleaseBones(host.Bones);
            _edgeSource.Release();
            _host = null;
        }

        public abstract void SetColor(Color color);

        /// <summary>
        /// Takes the shape's edge pattern and bones and adds it to <paramref name="proxy"/>. If that fails, everything
        /// taken is given back.
        /// </summary>
        internal void Attach(MeshProxy proxy)
        {
            _edgePattern = _edgeSource.Acquire();
            _edgeSlots = new int[_edgePattern.Length / 2];
            BoneRegistry bones = proxy.Bones;
            AcquireBones(bones);
            try
            {
                _host = proxy.Attach(this);
            }
            catch
            {
                ReleaseBones(bones);
                _edgeSource.Release();
                throw;
            }
            MarkDirty(DirtyFlags.All);
        }

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
                _host.Enqueue(this);
            }
            _dirty |= flags;
        }

        internal DirtyFlags TakeDirty()
        {
            DirtyFlags dirty = _dirty;
            _dirty = DirtyFlags.None;
            return dirty;
        }

        /// <summary>Marks the shape disposed when its whole host goes away, bone registry included.</summary>
        internal void Detach()
        {
            _edgeSource.Release();
            _host = null;
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

        /// <summary>Registers the bones the shape was built with, when it is attached.</summary>
        protected abstract void AcquireBones(BoneRegistry bones);

        protected abstract void ReleaseBones(BoneRegistry bones);

        /// <summary>
        /// Throws off the main thread (in the Editor and development builds) or once the shape is disposed, and
        /// otherwise lets it notice a bone that was destroyed, so every member sees the shape in world space from then on.
        /// </summary>
        protected void EnsureUsable()
        {
            MainThread.Check();
            if (_host == null)
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

        /// <summary>
        /// Points a bone field at <paramref name="value"/>, already checked with <see cref="CheckBone"/>, and moves its
        /// registry slot along.
        /// </summary>
        protected void ReplaceBone(ref Transform bone, ref int slot, Transform value)
        {
            // Acquiring first keeps a bone that is re-assigned from being released and re-registered in between.
            int newSlot = _host.Bones.Acquire(value);
            _host.Bones.Release(slot);
            bone = value;
            slot = newSlot;
        }

        /// <summary>
        /// Returns <paramref name="bone"/> once it is checked to be a Transform in a scene, or null for no bone and for
        /// a destroyed one, which is how bone fields store them so they are never read again.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="bone"/> isn't in a scene, such as a prefab asset.</exception>
        protected static Transform CheckBone(Transform bone, string parameterName)
        {
            if (bone == null)
            {
                return null;
            }
            if (!bone.gameObject.scene.IsValid())
            {
                throw new ArgumentException(
                    $"'{bone.name}' isn't in a scene. Shapes can follow scene objects only, not prefab assets.",
                    parameterName);
            }
            return bone;
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
