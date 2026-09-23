namespace reromanlee.Wireframes
{
    /// <summary>A rectangle whose corners are rounded by <see cref="CornerRadius"/>.</summary>
    public interface IRoundedRectangle : IRectangle
    {
        /// <summary>
        /// Radius of the corners in the bone's units. It is drawn clamped between 0, for sharp corners, and half the
        /// shorter side, where the short sides become half circles.
        /// </summary>
        float CornerRadius { get; set; }

        /// <summary>
        /// Number of straight pieces a full circle of the corners is drawn with, fixed at creation. Each corner is a
        /// quarter of them.
        /// </summary>
        int Segments { get; }
    }
}
