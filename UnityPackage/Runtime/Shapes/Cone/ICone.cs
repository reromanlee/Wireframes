namespace reromanlee.Wireframes
{
    /// <summary>
    /// A cone with its tip at end A and a round base around end B, drawn as the base ring joined to the tip by four
    /// lines.
    /// </summary>
    public interface ICone : IAxialShape
    {
        /// <summary>Radius of the base in the bone's units.</summary>
        float Radius { get; set; }

        /// <summary>Number of straight pieces the base ring is drawn with, fixed at creation.</summary>
        int Segments { get; }
    }
}
