using System;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Dense list of edges, each a pair of vertex indices, so the drawn index range never has gaps. Removing an edge
    /// moves the last edge into its slot and tells that edge's owner.
    /// </summary>
    internal sealed class EdgeList
    {
        private const int InitialCapacity = 128;

        private int[] _indices = new int[InitialCapacity * 2];
        private IEdgeOwner[] _owners = new IEdgeOwner[InitialCapacity];
        private int[] _ownerEdges = new int[InitialCapacity];

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
            int newCapacity = _owners.Length;
            while (newCapacity < capacity)
            {
                newCapacity *= 2;
            }
            Array.Resize(ref _indices, newCapacity * 2);
            Array.Resize(ref _owners, newCapacity);
            Array.Resize(ref _ownerEdges, newCapacity);
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
    }
}
