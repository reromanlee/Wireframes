using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>An ellipse around <see cref="IRigidShape.LocalPosition"/>, lying flat in the XZ plane of its rotation.</summary>
    public interface IEllipse : IRigidShape
    {
        /// <summary>Semi-axes in the bone's units: x is the radius along the ellipse's X axis and y along its Z axis.</summary>
        Vector2 Radii { get; set; }

        /// <summary>Number of straight pieces the ellipse is drawn with, fixed at creation.</summary>
        int Segments { get; }
    }
}
