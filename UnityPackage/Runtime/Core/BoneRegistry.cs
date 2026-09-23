using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Maps the transforms that shapes follow to bone slots of one skinned mesh, counting the endpoints using each.
    /// Slot 0 is the chunk's own transform and stands for world space. Freed slots are reused, so no other vertex
    /// ever needs its bone index rewritten.
    /// </summary>
    internal sealed class BoneRegistry : IDisposable
    {
        private const int InitialCapacity = 16;

        private readonly Transform _root;
        private readonly Action<Transform> _onBoneDestroyed;
        private readonly Dictionary<Transform, int> _slots = new();
        private readonly Stack<int> _freeSlots = new();
        // Watchers whose Awake hasn't run: Unity skips OnDestroy for them, so only these are checked every flush.
        private readonly List<BoneWatcher> _sleepers = new();

        private Transform[] _transforms = new Transform[InitialCapacity];
        private BoneWatcher[] _watchers = new BoneWatcher[InitialCapacity];
        private int[] _referenceCounts = new int[InitialCapacity];
        private int _end = 1;

        internal BoneRegistry(Transform root, Action<Transform> onBoneDestroyed)
        {
            _root = root;
            _onBoneDestroyed = onBoneDestroyed;
            Array.Fill(_transforms, root);
            IsDirty = true;
        }

        /// <summary>Bones for the renderer, sized to the capacity. Unused slots hold the root.</summary>
        internal Transform[] Transforms
        {
            get => _transforms;
        }

        /// <summary>True when <see cref="Transforms"/> changed since the renderer last received it.</summary>
        internal bool IsDirty { get; private set; }

        /// <summary>Number of distinct transforms in use, not counting the root.</summary>
        internal int Count
        {
            get => _slots.Count;
        }

        internal void ClearDirty()
        {
            IsDirty = false;
        }

        /// <summary>Returns the slot for <paramref name="bone"/>, registering it on first use. Null means world space.</summary>
        internal int Acquire(Transform bone)
        {
            if (bone == null)
            {
                return 0;
            }
            if (_slots.TryGetValue(bone, out int slot))
            {
                _referenceCounts[slot]++;
                return slot;
            }
            slot = _freeSlots.Count > 0 ? _freeSlots.Pop() : _end++;
            EnsureCapacity(slot + 1);
            BoneWatcher watcher = BoneWatcher.Watch(bone, this);
            _slots.Add(bone, slot);
            _transforms[slot] = bone;
            _watchers[slot] = watcher;
            _referenceCounts[slot] = 1;
            if (!watcher.IsAwake)
            {
                _sleepers.Add(watcher);
            }
            IsDirty = true;
            return slot;
        }

        internal void Release(int slot)
        {
            if (slot == 0 || --_referenceCounts[slot] > 0)
            {
                return;
            }
            BoneWatcher watcher = _watchers[slot];
            // Dictionary lookups compare instance IDs, so this also works for a transform that was just destroyed.
            _slots.Remove(_transforms[slot]);
            _transforms[slot] = _root;
            _watchers[slot] = null;
            _freeSlots.Push(slot);
            _sleepers.Remove(watcher);
            watcher.RemoveListener(this);
            IsDirty = true;
        }

        /// <summary>Called by a watcher whose bone is being destroyed.</summary>
        internal void OnBoneDestroyed(Transform bone)
        {
            _onBoneDestroyed(bone);
        }

        /// <summary>Catches bones that were destroyed before their GameObject was ever active.</summary>
        internal void PollSleepers()
        {
            for (int i = _sleepers.Count - 1; i >= 0; i--)
            {
                BoneWatcher watcher = _sleepers[i];
                if (watcher.IsAwake)
                {
                    _sleepers.RemoveAt(i);
                }
                else if (watcher == null)
                {
                    _sleepers.RemoveAt(i);
                    Debug.LogWarning(
                        "Wireframes: a bone was destroyed before its GameObject was ever active, so its last pose is " +
                        "unknown. The endpoints attached to it now use their local offsets as world positions.");
                    _onBoneDestroyed(watcher.Bone);
                }
            }
        }

        public void Dispose()
        {
            for (int slot = 1; slot < _end; slot++)
            {
                BoneWatcher watcher = _watchers[slot];
                if (!ReferenceEquals(watcher, null))
                {
                    watcher.RemoveListener(this);
                    _watchers[slot] = null;
                }
            }
            _slots.Clear();
            _sleepers.Clear();
            _freeSlots.Clear();
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
            Array.Fill(_transforms, _root, oldCapacity, newCapacity - oldCapacity);
            Array.Resize(ref _watchers, newCapacity);
            Array.Resize(ref _referenceCounts, newCapacity);
        }
    }
}
