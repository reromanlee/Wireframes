using System;
using System.Collections.Generic;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Edge patterns that depend only on a shape's resolution. Shapes only read their pattern, so each one is built
    /// on first use and then shared by every shape with the same resolution.
    /// </summary>
    internal sealed class PatternCache
    {
        private readonly Dictionary<int, int[]> _patterns = new();
        private readonly Func<int, int[]> _build;

        internal PatternCache(Func<int, int[]> build)
        {
            _build = build;
        }

        internal int[] Get(int resolution)
        {
            if (!_patterns.TryGetValue(resolution, out int[] pattern))
            {
                pattern = _build(resolution);
                _patterns.Add(resolution, pattern);
            }
            return pattern;
        }
    }
}
