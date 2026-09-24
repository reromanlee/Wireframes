namespace reromanlee.Wireframes
{
    /// <summary>
    /// The flat outline that wraps two circles centered on end A and end B, lying in the XZ plane of its rotation, with
    /// a radius for each end: a capsule's silhouette. When one circle holds the other, only the bigger circle is drawn.
    /// </summary>
    public interface IStadium : IAxialShape
    {
        /// <summary>Radius of the circle around end A, in the bone's units.</summary>
        float RadiusA { get; set; }

        /// <summary>Radius of the circle around end B, in the bone's units.</summary>
        float RadiusB { get; set; }

        /// <summary>
        /// Number of straight pieces a full circle would be drawn with, fixed at creation. Each end is half of them.
        /// </summary>
        int Segments { get; }
    }
}
