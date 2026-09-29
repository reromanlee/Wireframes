using System;
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
        // Past this many collected ranges, they are merged as they come, so the list stays small when no flush clears
        // it, such as while nothing renders.
        private const int MaxCollectedRanges = 256;

        private RangeInt[] _ranges = new RangeInt[16];
        private int _count;
        // Once so many ranges came in that they collapsed into one covering range, later ones only widen it.
        private bool _isCovering;

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
            if (_isCovering)
            {
                RangeInt covering = _ranges[0];
                int coveringStart = Math.Min(covering.start, start);
                _ranges[0] = new RangeInt(coveringStart, Math.Max(covering.end, start + length) - coveringStart);
                return;
            }
            if (_count == _ranges.Length)
            {
                if (_count >= MaxCollectedRanges)
                {
                    _isCovering = Merge() == 1;
                    if (_isCovering)
                    {
                        Add(start, length);
                        return;
                    }
                }
                if (_count == _ranges.Length)
                {
                    Array.Resize(ref _ranges, _count * 2);
                }
            }
            _ranges[_count++] = new RangeInt(start, length);
        }

        internal void Clear()
        {
            _count = 0;
            _isCovering = false;
        }

        /// <summary>Sorts and merges the collected ranges in place and returns how many remain.</summary>
        internal int Merge()
        {
            if (_count <= 1)
            {
                return _count;
            }
            SortByStart();
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

        /// <summary>
        /// Insertion sort by start: there are at most a few hundred ranges, often already in order, and unlike
        /// Array.Sort with a comparer, it allocates nothing.
        /// </summary>
        private void SortByStart()
        {
            for (int i = 1; i < _count; i++)
            {
                RangeInt range = _ranges[i];
                int j = i - 1;
                while (j >= 0 && _ranges[j].start > range.start)
                {
                    _ranges[j + 1] = _ranges[j];
                    j--;
                }
                _ranges[j + 1] = range;
            }
        }
    }
}
