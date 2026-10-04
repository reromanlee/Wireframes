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
        private EdgeSource _edgeSource;
        private int[] _edgePattern;
        private int[] _edgeSlots;
        private int _edgeCount;
        private IShapeHost _host;
        private DirtyFlags _dirty;
        private bool _isHidden;
        private bool _isSuspended;

        protected Shape(int vertexCount, EdgeSource edgeSource)
        {
            VertexCount = vertexCount;
            _edgeSource = edgeSource;
        }

        public bool IsDisposed
        {
            get => _host == null && !_isSuspended;
        }

        /// <summary>
        /// True while the shape is out of every host after <see cref="Suspend"/>: neither disposed nor usable until
        /// <see cref="Resume"/>.
        /// </summary>
        internal bool IsSuspended
        {
            get => _isSuspended;
        }

        public bool IsVisible
        {
            get
            {
                EnsureUsable();
                return !_isHidden;
            }
            set
            {
                EnsureUsable();
                bool isVisible = !_isHidden;
                if (value == isVisible)
                {
                    return;
                }
                // Shown only once the host managed to add the edges, so a failure leaves the shape hidden.
                if (value)
                {
                    _host.Show(this);
                    _isHidden = false;
                }
                else
                {
                    _isHidden = true;
                    _host.Hide(this);
                }
            }
        }

        /// <summary>The chunk that draws the shape, or null when it is disposed or nothing can draw it.</summary>
        internal MeshChunk Chunk
        {
            get => _host as MeshChunk;
        }

        /// <summary>True while the shape is hidden, when its edges aren't in its chunk's drawn edges.</summary>
        internal bool IsHidden
        {
            get => _isHidden;
        }

        internal int VertexCount { get; private set; }

        internal int VertexStart { get; set; }

        /// <summary>Position in its host's shape list.</summary>
        internal int ShapeIndex { get; set; }

        /// <summary>Position in its chunk's queue of shapes to write, or -1 while not queued there.</summary>
        internal int PendingIndex { get; set; } = -1;

        internal int EdgeCount
        {
            get => _edgeCount;
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
            if (_isSuspended)
            {
                // Its host already took everything else back.
                _isSuspended = false;
                _edgeSource.Release();
                return;
            }
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
            _edgeCount = _edgeSource.EdgeCountOf(_edgePattern);
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

        /// <summary>
        /// Takes the shape out of its host without disposing it: the host takes back its vertices and edges and the
        /// bones are released, while the shape keeps its state and edge pattern, so <see cref="Resume"/> allocates
        /// nothing to put it back.
        /// </summary>
        internal void Suspend()
        {
            IShapeHost host = _host;
            host.Remove(this);
            ReleaseBones(host.Bones);
            // Removing it dropped its queue entry, so it has to be queued again wherever it lands.
            _dirty = DirtyFlags.None;
            _host = null;
            _isSuspended = true;
        }

        /// <summary>
        /// Puts a suspended shape into <paramref name="proxy"/>, the one it left or another, and queues all of it to be
        /// written. If that fails, it stays suspended.
        /// </summary>
        internal void Resume(MeshProxy proxy)
        {
            BoneRegistry bones = proxy.Bones;
            AcquireBones(bones);
            try
            {
                _host = proxy.Attach(this);
            }
            catch
            {
                ReleaseBones(bones);
                throw;
            }
            _isSuspended = false;
            MarkDirty(DirtyFlags.All);
        }

        /// <summary>
        /// Changes the shape's vertex count and edge pattern while it stays the same object, for shapes whose number of
        /// points changes. Derived shapes update their own points and bones first. Its host moves it where the new size
        /// fits, and everything is written again on the next flush. If no room can be found, the shape ends up disposed
        /// and the exception is rethrown.
        /// </summary>
        internal void Resize(int vertexCount, EdgeSource edgeSource)
        {
            IShapeHost host = _host;
            host.Remove(this);
            _edgeSource.Release();
            VertexCount = vertexCount;
            _edgeSource = edgeSource;
            _edgePattern = edgeSource.Acquire();
            _edgeCount = edgeSource.EdgeCountOf(_edgePattern);
            if (_edgeSlots.Length != _edgePattern.Length / 2)
            {
                _edgeSlots = new int[_edgePattern.Length / 2];
            }
            // Removing it dropped its queue entry, so it has to be queued again wherever it lands.
            _dirty = DirtyFlags.None;
            try
            {
                _host = host.Reattach(this);
            }
            catch
            {
                ReleaseBones(host.Bones);
                _edgeSource.Release();
                _host = null;
                throw;
            }
            MarkDirty(DirtyFlags.All);
        }

        /// <summary>
        /// Replaces the edges of a shape that owns its pattern while its vertices stay where they are, as a text does when
        /// its glyphs change. Only the first <paramref name="edgeCount"/> pairs of <paramref name="pattern"/> are drawn,
        /// so the array can be the current one rewritten in place, with room to spare. The edges are written again on the
        /// next flush. If they can't be added, the shape ends up hidden and the exception is rethrown.
        /// </summary>
        internal void ReplaceEdges(int[] pattern, int edgeCount)
        {
            bool isDrawn = _host != null && !_isHidden;
            if (isDrawn)
            {
                // Hiding takes the old edges out of the drawn ones, and showing adds the new ones.
                _host.Hide(this);
                _isHidden = true;
            }
            _edgeSource.Release();
            _edgeSource = new EdgeSource(pattern, edgeCount);
            _edgePattern = pattern;
            _edgeCount = edgeCount;
            if (_edgeSlots.Length < pattern.Length / 2)
            {
                _edgeSlots = new int[pattern.Length / 2];
            }
            if (isDrawn)
            {
                _host.Show(this);
                _isHidden = false;
            }
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
            for (int i = 0; i < _edgeCount; i++)
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
