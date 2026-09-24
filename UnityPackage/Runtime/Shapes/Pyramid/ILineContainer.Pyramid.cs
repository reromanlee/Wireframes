using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>Creates a white pyramid with its tip at the world origin and a 1 by 1 base, 1 unit along +Z.</summary>
        IPyramid CreatePyramid();

        /// <summary>
        /// Creates a white pyramid from a tip to the center of its base, both world positions. Its spin around the axis
        /// keeps the base's Y side as close to world up as it can; pointing straight up or down, the base is aligned
        /// with the world's X and Z axes.
        /// </summary>
        /// <param name="baseSize">Width of the base along the pyramid's X axis and height along its Y axis.</param>
        IPyramid CreatePyramid(Vector3 tip, Vector3 baseCenter, Vector2 baseSize);

        /// <summary>
        /// Creates a white pyramid that follows <paramref name="bone"/>, from a tip to the center of its base, both in
        /// the bone's local space. Its spin around the axis keeps the base's Y side as close to the bone's up as it can.
        /// </summary>
        /// <param name="baseSize">Width of the base along the pyramid's X axis and height along its Y axis.</param>
        IPyramid CreatePyramid(Transform bone, Vector3 localTip, Vector3 localBaseCenter, Vector2 baseSize);

        /// <summary>
        /// Creates a white pyramid in world space with its tip at <paramref name="position"/> and its base
        /// <paramref name="length"/> along the +Z axis of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="baseSize">Width of the base along the rotation's X axis and height along its Y axis.</param>
        IPyramid CreatePyramid(Vector3 position, Quaternion rotation, float length, Vector2 baseSize);
    }
}
