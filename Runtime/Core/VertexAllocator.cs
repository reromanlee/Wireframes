using System.Collections.Generic;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Hands out contiguous blocks of vertices. A freed block is reused by the next request of the same size, so
    /// lines reuse line blocks and boxes reuse box blocks.
    /// </summary>
    internal sealed class VertexAllocator
    {
        private readonly Dictionary<int, Stack<int>> _freeBlocks = new();

        /// <summary>One past the highest vertex handed out so far.</summary>
        internal int End { get; private set; }

        /// <summary>Vertices sitting in freed blocks.</summary>
        internal int FreeCount { get; private set; }

        /// <summary><see cref="End"/> after allocating <paramref name="count"/> vertices, without allocating them.</summary>
        internal int EndAfterAllocate(int count)
        {
            return HasFreeBlock(count) ? End : End + count;
        }

        internal int Allocate(int count)
        {
            if (HasFreeBlock(count))
            {
                FreeCount -= count;
                return _freeBlocks[count].Pop();
            }
            int start = End;
            End += count;
            return start;
        }

        internal void Free(int start, int count)
        {
            if (!_freeBlocks.TryGetValue(count, out Stack<int> blocks))
            {
                blocks = new Stack<int>();
                _freeBlocks.Add(count, blocks);
            }
            blocks.Push(start);
            FreeCount += count;
        }

        /// <summary>Forgets every block, so the caller can hand them out again from vertex 0.</summary>
        internal void Reset()
        {
            foreach (Stack<int> blocks in _freeBlocks.Values)
            {
                blocks.Clear();
            }
            End = 0;
            FreeCount = 0;
        }

        private bool HasFreeBlock(int count)
        {
            return _freeBlocks.TryGetValue(count, out Stack<int> blocks) && blocks.Count > 0;
        }
    }
}
