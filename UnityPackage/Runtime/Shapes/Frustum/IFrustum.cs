namespace reromanlee.Wireframes
{
    /// <summary>
    /// A regular polygon around each end, with every pair of matching corners joined. A flat side faces the -Y of its
    /// rotation, so four sides make a square aligned with the shape's axes. A radius of 0 at one end makes a pyramid
    /// with a regular base, and equal radii make a prism.
    /// </summary>
    public interface IFrustum : IAxialShape
    {
        /// <summary>Distance from end A to the corners of its polygon, in the bone's units.</summary>
        float RadiusA { get; set; }

        /// <summary>Distance from end B to the corners of its polygon, in the bone's units.</summary>
        float RadiusB { get; set; }

        /// <summary>Number of sides of each polygon, fixed at creation.</summary>
        int Sides { get; }
    }
}
