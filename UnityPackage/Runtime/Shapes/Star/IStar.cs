namespace reromanlee.Wireframes
{
    /// <summary>
    /// A star around <see cref="IRigidShape.LocalPosition"/>, lying flat in the XZ plane of its rotation, with its first
    /// point along +Z.
    /// </summary>
    public interface IStar : IRigidShape
    {
        /// <summary>Distance from the center to the corners between the points, in the bone's units.</summary>
        float InnerRadius { get; set; }

        /// <summary>Distance from the center to the tips of the points, in the bone's units.</summary>
        float OuterRadius { get; set; }

        /// <summary>Number of points, fixed at creation.</summary>
        int PointCount { get; }
    }
}
