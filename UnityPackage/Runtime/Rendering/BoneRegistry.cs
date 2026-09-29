using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Maps the transforms that shapes follow to bone slots, counting the uses of each, and keeps every slot's latest
    /// local-to-world matrix for the shader. Slot 0 is the identity and stands for world space. When a transform is
    /// destroyed, its slot stops being read and keeps its last matrix, which holds the shapes on it in place until each
    /// of them switches to world space and releases the slot. No component is ever added to a bone.
    /// </summary>
    internal sealed class BoneRegistry
    {
        internal const int WorldSlot = 0;
        private const int InitialCapacity = 16;

        private readonly Dictionary<Transform, int> _slots = new();
        private readonly Stack<int> _freeSlots = new();

        private Transform[] _transforms = new Transform[InitialCapacity];
        private Matrix4x4[] _matrices = new Matrix4x4[InitialCapacity];
        private int[] _referenceCounts = new int[InitialCapacity];

        // Slots whose transform is still alive, read every frame, and each slot's position among them (-1 when not).
        private int[] _liveSlots = new int[InitialCapacity];
        private int[] _livePositions = new int[InitialCapacity];
        private int _liveCount;
        private int _end = WorldSlot + 1;

        internal BoneRegistry()
        {
            _matrices[WorldSlot] = Matrix4x4.identity;
            Array.Fill(_livePositions, -1);
            HasChanges = true;
        }

        /// <summary>One past the highest slot handed out so far.</summary>
        internal int End
        {
            get => _end;
        }

        /// <summary>Number of distinct transforms in use, not counting world space.</summary>
        internal int Count
        {
            get => _slots.Count;
        }

        /// <summary>Bytes of CPU memory the registry takes, about.</summary>
        internal long CpuMemory
        {
            get
            {
                // Per slot: its transform, matrix, reference count and two live-list positions; per bone, a map entry.
                long slotSize = IntPtr.Size + 16 * sizeof(float) + 3 * sizeof(int);
                long entrySize = IntPtr.Size + 3 * sizeof(int);
                return _transforms.Length * slotSize + (long)_slots.Count * entrySize;
            }
        }

        /// <summary>Each slot's latest matrix, sized to the capacity.</summary>
        internal Matrix4x4[] Matrices
        {
            get => _matrices;
        }

        /// <summary>True when a matrix changed since <see cref="ClearChanges"/>.</summary>
        internal bool HasChanges { get; private set; }

        internal Matrix4x4 MatrixOf(int slot)
        {
            return _matrices[slot];
        }

        internal void ClearChanges()
        {
            HasChanges = false;
        }

        /// <summary>
        /// Returns the slot for <paramref name="bone"/>, registering it on first use. Null means world space; callers
        /// pass a destroyed transform as null.
        /// </summary>
        internal int Acquire(Transform bone)
        {
            if (bone is null)
            {
                return WorldSlot;
            }
            if (_slots.TryGetValue(bone, out int slot))
            {
                _referenceCounts[slot]++;
                return slot;
            }
            slot = _freeSlots.Count > 0 ? _freeSlots.Pop() : _end++;
            EnsureCapacity(slot + 1);
            _slots.Add(bone, slot);
            _transforms[slot] = bone;
            _referenceCounts[slot] = 1;
            // Read right away, so a bone destroyed before the next frame still leaves a pose to hold its shapes.
            _matrices[slot] = bone.localToWorldMatrix;
            _livePositions[slot] = _liveCount;
            _liveSlots[_liveCount++] = slot;
            HasChanges = true;
            return slot;
        }

        internal void Release(int slot)
        {
            if (slot == WorldSlot || --_referenceCounts[slot] > 0)
            {
                return;
            }
            // Dictionary lookups compare instance IDs, so this also works for a transform that was destroyed.
            _slots.Remove(_transforms[slot]);
            _transforms[slot] = null;
            StopReading(slot);
            _freeSlots.Push(slot);
        }

        /// <summary>
        /// Reads the matrix of every live bone and notes whether any changed. A destroyed bone's slot stops being read
        /// and keeps its last matrix.
        /// </summary>
        internal void ReadMatrices()
        {
            using (WireframesMarkers.ReadBones.Auto())
            {
                // Backwards, so a slot that stops being read swaps in one that was already read.
                for (int i = _liveCount - 1; i >= 0; i--)
                {
                    int slot = _liveSlots[i];
                    Transform bone = _transforms[slot];
                    if (bone == null)
                    {
                        StopReading(slot);
                        continue;
                    }
                    Matrix4x4 matrix = bone.localToWorldMatrix;
                    if (!matrix.Equals(_matrices[slot]))
                    {
                        _matrices[slot] = matrix;
                        HasChanges = true;
                    }
                }
            }
        }

        private void StopReading(int slot)
        {
            int position = _livePositions[slot];
            if (position < 0)
            {
                return;
            }
            int last = _liveSlots[--_liveCount];
            _liveSlots[position] = last;
            _livePositions[last] = position;
            _livePositions[slot] = -1;
        }

        private void EnsureCapacity(int capacity)
        {
            int oldCapacity = _transforms.Length;
            if (capacity <= oldCapacity)
            {
                return;
            }
            int newCapacity = oldCapacity;
            while (newCapacity < capacity)
            {
                newCapacity *= 2;
            }
            Array.Resize(ref _transforms, newCapacity);
            Array.Resize(ref _matrices, newCapacity);
            Array.Resize(ref _referenceCounts, newCapacity);
            Array.Resize(ref _liveSlots, newCapacity);
            Array.Resize(ref _livePositions, newCapacity);
            Array.Fill(_livePositions, -1, oldCapacity, newCapacity - oldCapacity);
        }
    }
}
