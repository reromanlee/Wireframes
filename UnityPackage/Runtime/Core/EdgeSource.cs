namespace reromanlee.Wireframes
{
    /// <summary>
    /// Where a shape's edge pattern comes from: an array of its own, or a <see cref="PatternCache"/> that shares one
    /// between the live shapes of the same resolution. The shape takes the pattern when it is attached and gives it
    /// back when it is disposed.
    /// </summary>
    internal readonly struct EdgeSource
    {
        private readonly int[] _pattern;
        private readonly PatternCache _cache;
        private readonly int _resolution;

        internal EdgeSource(PatternCache cache, int resolution)
        {
            _pattern = null;
            _cache = cache;
            _resolution = resolution;
        }

        private EdgeSource(int[] pattern)
        {
            _pattern = pattern;
            _cache = null;
            _resolution = 0;
        }

        /// <summary>A pattern owned by one shape: pairs of local vertex indices, one pair per edge.</summary>
        public static implicit operator EdgeSource(int[] pattern)
        {
            return new EdgeSource(pattern);
        }

        internal int[] Acquire()
        {
            return _cache != null ? _cache.Acquire(_resolution) : _pattern;
        }

        internal void Release()
        {
            _cache?.Release(_resolution);
        }
    }
}
