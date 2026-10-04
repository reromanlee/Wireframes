namespace reromanlee.Wireframes
{
    /// <summary>What a text does with lines wider than its bounds.</summary>
    public enum WireframeTextOverflow
    {
        /// <summary>Lines stay as written and run past the bounds; only line breaks in the text start new lines.</summary>
        Overflow,

        /// <summary>
        /// Lines break at spaces to fit the width of the bounds, and inside a word only when the word alone is wider.
        /// </summary>
        Wrap
    }
}
