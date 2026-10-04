namespace reromanlee.Wireframes
{
    /// <summary>Where the lines of a text sit, all together, between the top and the bottom of its bounds.</summary>
    public enum WireframeVerticalAlignment
    {
        /// <summary>The first line's glyph boxes touch the top of the bounds, and further lines go down.</summary>
        Top,

        /// <summary>The lines are centered between the top and the bottom of the bounds.</summary>
        Middle,

        /// <summary>The last line's glyph boxes touch the bottom of the bounds, and earlier lines go up.</summary>
        Bottom
    }
}
