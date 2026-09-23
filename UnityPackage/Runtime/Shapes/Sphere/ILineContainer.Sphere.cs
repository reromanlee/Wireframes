using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>Creates a white sphere of radius 0.5 around the world origin.</summary>
        ISphere CreateSphere();

        /// <summary>Creates a white, world-aligned sphere around a world position.</summary>
        /// <param name="segments">Number of straight pieces each great circle is drawn with, at least 3.</param>
        ISphere CreateSphere(Vector3 center, float radius, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white sphere that follows <paramref name="bone"/>, around a position in the bone's local space and
        /// aligned to the bone's axes.
        /// </summary>
        /// <param name="segments">Number of straight pieces each great circle is drawn with, at least 3.</param>
        ISphere CreateSphere(Transform bone, Vector3 localCenter, float radius, int segments = Ring.DefaultSegments);

        /// <summary>Creates a white sphere in world space, with its great circles turned by <paramref name="rotation"/>.</summary>
        /// <param name="segments">Number of straight pieces each great circle is drawn with, at least 3.</param>
        ISphere CreateSphere(Vector3 center, Quaternion rotation, float radius, int segments = Ring.DefaultSegments);
    }
}
