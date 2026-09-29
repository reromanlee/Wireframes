using System;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Dense list of edges, each a pair of vertex indices, so the drawn index range never has gaps. Removing an edge
    /// moves the last edge into its slot and tells that edge's owner.
    /// </summary>
    internal sealed class EdgeList
    {
        private int[] _indices;
        private IEdgeOwner[] _owners;
        private int[] _ownerEdges;

        internal EdgeList(int capacity)
        {
            _indices = new int[capacity * 2];
            _owners = new IEdgeOwner[capacity];
            _ownerEdges = new int[capacity];
        }

        internal int Count { get; private set; }

        internal int Capacity
        {
            get => _owners.Length;
        }

        /// <summary>Two vertex indices per edge slot, sized to the capacity.</summary>
        internal int[] Indices
        {
            get => _indices;
        }

        internal void EnsureCapacity(int capacity)
        {
            if (capacity <= _owners.Length)
            {
                return;
            }
            int newCapacity = Math.Max(_owners.Length, 1);
            while (newCapacity < capacity)
            {
                newCapacity *= 2;
            }
            Resize(newCapacity);
        }

        /// <summary>Lowers the capacity to <paramref name="capacity"/>, but never below the edges it holds.</summary>
        internal void Shrink(int capacity)
        {
            capacity = Math.Max(capacity, Count);
            if (capacity < _owners.Length)
            {
                Resize(capacity);
            }
        }

        /// <summary>Appends an edge of <paramref name="owner"/> and returns its slot. Its indices are set later.</summary>
        internal int Add(IEdgeOwner owner, int edge)
        {
            EnsureCapacity(Count + 1);
            int slot = Count++;
            _owners[slot] = owner;
            _ownerEdges[slot] = edge;
            return slot;
        }

        internal void Set(int slot, int vertexA, int vertexB)
        {
            _indices[slot * 2] = vertexA;
            _indices[slot * 2 + 1] = vertexB;
        }

        /// <returns>True when another edge moved into <paramref name="slot"/>, which then has to be uploaded again.</returns>
        internal bool RemoveAt(int slot)
        {
            int last = --Count;
            bool moved = slot != last;
            if (moved)
            {
                _indices[slot * 2] = _indices[last * 2];
                _indices[slot * 2 + 1] = _indices[last * 2 + 1];
                IEdgeOwner owner = _owners[last];
                int edge = _ownerEdges[last];
                _owners[slot] = owner;
                _ownerEdges[slot] = edge;
                owner.OnEdgeMoved(edge, slot);
            }
            _owners[last] = null;
            return moved;
        }

        private void Resize(int capacity)
        {
            Array.Resize(ref _indices, capacity * 2);
            Array.Resize(ref _owners, capacity);
            Array.Resize(ref _ownerEdges, capacity);
        }
    }
}
