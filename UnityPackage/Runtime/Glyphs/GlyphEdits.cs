using System.Threading;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Counts changes to glyph packs and glyph lists: edits, undo and reimports. Caches built from them compare
    /// <see cref="Version"/> to know when they are out of date, so a lookup never has to check each pack it reads.
    /// </summary>
    internal static class GlyphEdits
    {
        private static int _version;
        private static int _hasPendingChanges;

        /// <summary>Changes on every recorded edit; nothing changes it in builds once the assets are loaded.</summary>
        internal static int Version
        {
            get => Volatile.Read(ref _version);
        }

        /// <summary>
        /// Records that glyph data changed. Safe from any thread, since Unity can deserialize assets off the main
        /// thread while loading them.
        /// </summary>
        internal static void Record()
        {
            Interlocked.Increment(ref _version);
            Volatile.Write(ref _hasPendingChanges, 1);
        }

        /// <summary>
        /// Returns whether anything was recorded since the last call, and clears it, for the editor's update loop that
        /// redraws the shapes using the changed glyphs.
        /// </summary>
        internal static bool TakePendingChanges()
        {
            return Interlocked.Exchange(ref _hasPendingChanges, 0) != 0;
        }
    }
}
