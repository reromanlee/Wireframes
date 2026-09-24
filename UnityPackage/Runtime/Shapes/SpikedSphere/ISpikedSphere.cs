namespace reromanlee.Wireframes
{
    /// <summary>
    /// A 3D star around <see cref="IRigidShape.LocalPosition"/>: a Platonic solid whose corners lie on
    /// <see cref="BaseRadius"/>, with a pyramid spike on every face whose tip reaches <see cref="BaseRadius"/> +
    /// <see cref="SpikeLength"/> from the center. The solid sits in its usual orientation along the shape's axes.
    /// </summary>
    public interface ISpikedSphere : IRigidShape
    {
        /// <summary>Distance from the center to the corners of the solid, in the bone's units.</summary>
        float BaseRadius { get; set; }

        /// <summary>How far the spike tips reach beyond <see cref="BaseRadius"/>, in the bone's units.</summary>
        float SpikeLength { get; set; }

        /// <summary>
        /// Number of spikes, fixed at creation: 4, 6, 8, 12 or 20, one on each face of a tetrahedron, cube, octahedron,
        /// dodecahedron or icosahedron.
        /// </summary>
        int SpikeCount { get; }
    }
}
