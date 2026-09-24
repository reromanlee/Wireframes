namespace reromanlee.Wireframes
{
    /// <summary>
    /// A sphere around <see cref="IRigidShape.LocalPosition"/>, drawn as three great circles in the XZ, XY and YZ
    /// planes of its rotation, so the rotation shows.
    /// </summary>
    public interface ISphere : IRigidShape
    {
        /// <summary>Radius in the bone's units.</summary>
        float Radius { get; set; }

        /// <summary>Number of straight pieces each great circle is drawn with, fixed at creation.</summary>
        int Segments { get; }
    }
}
