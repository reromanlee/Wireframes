namespace reromanlee.Wireframes
{
    /// <summary>How far each character of a text moves the next one along.</summary>
    public enum WireframeCharacterWidth
    {
        /// <summary>
        /// Each character takes the width of its lines, so narrow ones sit closer together, and a character without
        /// lines, such as a space, takes its pack's space width.
        /// </summary>
        Proportional,

        /// <summary>Every character takes its whole glyph box, 1 wide, so columns line up.</summary>
        Monospace
    }
}
