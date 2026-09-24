namespace reromanlee.Wireframes
{
    /// <summary>
    /// The shape that wraps two spheres centered on end A and end B, like the capsule of <c>Physics.CapsuleCast</c>,
    /// with a radius for each end. It is drawn as a ring where the outline touches each sphere, four lines along its
    /// sides and two arcs over each cap. When one sphere holds the other, only the bigger sphere is drawn.
    /// </summary>
    public interface ICapsule : IAxialShape
    {
        /// <summary>Radius of the sphere around end A, in the bone's units.</summary>
        float RadiusA { get; set; }

        /// <summary>Radius of the sphere around end B, in the bone's units.</summary>
        float RadiusB { get; set; }

        /// <summary>
        /// Number of straight pieces each ring is drawn with, fixed at creation. Each arc over a cap is half of them.
        /// </summary>
        int Segments { get; }
    }
}
