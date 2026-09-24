using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// An ellipsoid around <see cref="IRigidShape.LocalPosition"/>, drawn as three ellipses in the XZ, XY and YZ planes
    /// of its rotation.
    /// </summary>
    public interface IEllipsoid : IRigidShape
    {
        /// <summary>Semi-axes along the ellipsoid's X, Y and Z axes, in the bone's units.</summary>
        Vector3 Radii { get; set; }

        /// <summary>Number of straight pieces each ellipse is drawn with, fixed at creation.</summary>
        int Segments { get; }
    }
}
