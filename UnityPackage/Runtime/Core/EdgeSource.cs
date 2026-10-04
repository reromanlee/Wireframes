namespace reromanlee.Wireframes
{
    /// <summary>
    /// Where a shape's edge pattern comes from: an array of its own, or a <see cref="PatternCache"/> that shares one
    /// between the live shapes of the same resolution. The shape takes the pattern when it is attached and gives it
    /// back when it is disposed.
    /// </summary>
    internal readonly struct EdgeSource
    {
        private const int WholePattern = -1;

        private readonly int[] _pattern;
        private readonly PatternCache _cache;
        private readonly int _resolution;
        private readonly int _edgeCount;

        internal EdgeSource(PatternCache cache, int resolution)
        {
            _pattern = null;
            _cache = cache;
            _resolution = resolution;
            _edgeCount = WholePattern;
        }

        /// <summary>
        /// The first <paramref name="edgeCount"/> pairs of a pattern owned by one shape, which keeps room after them for
        /// edits that add edges, as a text does when its glyphs change.
        /// </summary>
        internal EdgeSource(int[] pattern, int edgeCount)
        {
            _pattern = pattern;
            _cache = null;
            _resolution = 0;
            _edgeCount = edgeCount;
        }

        /// <summary>A pattern owned by one shape: pairs of local vertex indices, one pair per edge.</summary>
        public static implicit operator EdgeSource(int[] pattern)
        {
            return new EdgeSource(pattern, WholePattern);
        }

        internal int[] Acquire()
        {
            return _cache != null ? _cache.Acquire(_resolution) : _pattern;
        }

        internal void Release()
        {
            _cache?.Release(_resolution);
        }

        /// <summary>Number of edges the shape draws from <paramref name="pattern"/>, the array <see cref="Acquire"/> gave.</summary>
        internal int EdgeCountOf(int[] pattern)
        {
            return _edgeCount == WholePattern ? pattern.Length / 2 : _edgeCount;
        }
    }
}
