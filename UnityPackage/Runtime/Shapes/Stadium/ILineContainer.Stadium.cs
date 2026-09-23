using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>
        /// Creates a white stadium lying flat in the XZ plane, around circles of radius 0.5 centered on the world origin
        /// and 1 unit along +Z.
        /// </summary>
        IStadium CreateStadium();

        /// <summary>
        /// Creates a white stadium around two circles of the same radius, centered on two world positions. It lies as
        /// flat as it can: its normal stays as close to world up as the centers allow.
        /// </summary>
        /// <param name="segments">Number of straight pieces a full circle would be drawn with, a positive multiple of 4.</param>
        IStadium CreateStadium(Vector3 centerA, Vector3 centerB, float radius, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white stadium around two circles centered on two world positions, each with its own radius. It lies
        /// as flat as it can: its normal stays as close to world up as the centers allow.
        /// </summary>
        /// <param name="segments">Number of straight pieces a full circle would be drawn with, a positive multiple of 4.</param>
        IStadium CreateStadium(
            Vector3 centerA, Vector3 centerB, float radiusA, float radiusB, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white stadium around two circles centered on two world positions, facing as close to
        /// <paramref name="normal"/> as the centers allow.
        /// </summary>
        /// <param name="segments">Number of straight pieces a full circle would be drawn with, a positive multiple of 4.</param>
        IStadium CreateStadium(
            Vector3 centerA,
            Vector3 centerB,
            float radiusA,
            float radiusB,
            Vector3 normal,
            int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white stadium that follows <paramref name="bone"/>, around two circles centered on positions in the
        /// bone's local space. It lies as flat in the bone's XZ plane as the centers allow.
        /// </summary>
        /// <param name="segments">Number of straight pieces a full circle would be drawn with, a positive multiple of 4.</param>
        IStadium CreateStadium(
            Transform bone,
            Vector3 localCenterA,
            Vector3 localCenterB,
            float radiusA,
            float radiusB,
            int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white stadium in world space, lying in the XZ plane of <paramref name="rotation"/>, whose first
        /// circle is centered on <paramref name="position"/> and whose second is centered <paramref name="length"/>
        /// along the rotation's +Z axis.
        /// </summary>
        /// <param name="segments">Number of straight pieces a full circle would be drawn with, a positive multiple of 4.</param>
        IStadium CreateStadium(
            Vector3 position,
            Quaternion rotation,
            float length,
            float radiusA,
            float radiusB,
            int segments = Ring.DefaultSegments);
    }
}
