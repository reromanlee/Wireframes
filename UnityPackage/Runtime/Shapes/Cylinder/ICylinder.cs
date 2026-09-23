namespace reromanlee.Wireframes
{
    /// <summary>
    /// A cylinder from end A to end B, drawn as a ring around each end joined by four lines along its sides.
    /// </summary>
    public interface ICylinder : IAxialShape
    {
        /// <summary>Radius in the bone's units.</summary>
        float Radius { get; set; }

        /// <summary>Number of straight pieces each ring is drawn with, fixed at creation.</summary>
        int Segments { get; }
    }
}
