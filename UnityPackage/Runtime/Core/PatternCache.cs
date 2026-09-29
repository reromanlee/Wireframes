using System;
using System.Collections.Generic;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Edge patterns of one shape type by resolution, built on first use and shared by every live shape with that
    /// resolution. A pattern is dropped when the last shape using it is disposed, so the cache only ever holds what
    /// live shapes need.
    /// </summary>
    internal sealed class PatternCache
    {
        private readonly Dictionary<int, Entry> _entries = new();
        private readonly Func<int, int[]> _build;

        internal PatternCache(Func<int, int[]> build)
        {
            _build = build;
        }

        /// <summary>The pattern for <paramref name="resolution"/>, taken from the cache once the shape is attached.</summary>
        internal EdgeSource Get(int resolution)
        {
            return new EdgeSource(this, resolution);
        }

        internal bool Contains(int resolution)
        {
            return _entries.ContainsKey(resolution);
        }

        internal int[] Acquire(int resolution)
        {
            if (!_entries.TryGetValue(resolution, out Entry entry))
            {
                entry.Pattern = _build(resolution);
            }
            entry.UserCount++;
            _entries[resolution] = entry;
            return entry.Pattern;
        }

        internal void Release(int resolution)
        {
            if (!_entries.TryGetValue(resolution, out Entry entry))
            {
                return;
            }
            if (--entry.UserCount == 0)
            {
                _entries.Remove(resolution);
            }
            else
            {
                _entries[resolution] = entry;
            }
        }

        private struct Entry
        {
            public int[] Pattern;
            public int UserCount;
        }
    }
}
