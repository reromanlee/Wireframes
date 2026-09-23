namespace reromanlee.Wireframes
{
    /// <summary>
    /// A circle around <see cref="IRigidShape.LocalPosition"/>, lying flat in the XZ plane of its rotation, so the
    /// rotation's +Y is its normal.
    /// </summary>
    public interface ICircle : IRigidShape
    {
        /// <summary>Radius in the bone's units.</summary>
        float Radius { get; set; }

        /// <summary>Number of straight pieces the circle is drawn with, fixed at creation.</summary>
        int Segments { get; }
    }
}
