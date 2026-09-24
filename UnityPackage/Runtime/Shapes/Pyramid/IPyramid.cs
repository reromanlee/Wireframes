using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A pyramid with its tip at end A and a rectangular base centered on end B, across the +Z axis of its rotation:
    /// a camera's view without its near plane, or, pointed down, a classic pyramid.
    /// </summary>
    public interface IPyramid : IAxialShape
    {
        /// <summary>
        /// Size of the base in the bone's units: x is its width along the pyramid's X axis and y along its Y axis.
        /// </summary>
        Vector2 BaseSize { get; set; }
    }
}
