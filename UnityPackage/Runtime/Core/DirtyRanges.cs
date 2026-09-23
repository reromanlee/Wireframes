using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Collects the element ranges changed during a frame and merges them into as few uploads as possible.
    /// </summary>
    internal sealed class DirtyRanges
    {
        // Ranges closer than this are uploaded together: copying a few extra elements is cheaper than another native call.
        private const int MergeGap = 64;
        // Past this many separate ranges, a single covering upload is cheaper.
        private const int MaxRanges = 16;

        private static readonly Comparer<RangeInt> ByStart = Comparer<RangeInt>.Create((a, b) => a.start.CompareTo(b.start));

        private RangeInt[] _ranges = new RangeInt[16];
        private int _count;

        internal int Count
        {
            get => _count;
        }

        internal RangeInt this[int index]
        {
            get => _ranges[index];
        }

        internal void Add(int start, int length)
        {
            if (length <= 0)
            {
                return;
            }
            if (_count == _ranges.Length)
            {
                Array.Resize(ref _ranges, _count * 2);
            }
            _ranges[_count++] = new RangeInt(start, length);
        }

        internal void Clear()
        {
            _count = 0;
        }

        /// <summary>Sorts and merges the collected ranges in place and returns how many remain.</summary>
        internal int Merge()
        {
            if (_count <= 1)
            {
                return _count;
            }
            Array.Sort(_ranges, 0, _count, ByStart);
            int merged = 0;
            RangeInt current = _ranges[0];
            for (int i = 1; i < _count; i++)
            {
                RangeInt next = _ranges[i];
                if (next.start <= current.end + MergeGap)
                {
                    current.length = Math.Max(current.end, next.end) - current.start;
                }
                else
                {
                    _ranges[merged++] = current;
                    current = next;
                }
            }
            _ranges[merged++] = current;
            if (merged > MaxRanges)
            {
                _ranges[0] = new RangeInt(_ranges[0].start, _ranges[merged - 1].end - _ranges[0].start);
                merged = 1;
            }
            _count = merged;
            return merged;
        }
    }
}
