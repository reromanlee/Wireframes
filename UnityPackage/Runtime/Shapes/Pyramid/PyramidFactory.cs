using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates pyramids with rectangular bases in a <see cref="WireframeContainer"/>.</summary>
    public static class PyramidFactory
    {
        /// <summary>Creates a white pyramid with its tip at the world origin and a 1 by 1 base, 1 unit along +Z.</summary>
        public static IPyramid CreatePyramid(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Vector2 baseSize = Vector2.one * ShapeDefaults.Size;
            return new Pyramid(proxy, null, Vector3.zero, Quaternion.identity, ShapeDefaults.Size, baseSize);
        }

        /// <summary>
        /// Creates a white pyramid from a tip to the center of its base, both world positions. Its spin around the axis
        /// keeps the base's Y side as close to world up as it can; pointing straight up or down, the base is aligned
        /// with the world's X and Z axes.
        /// </summary>
        /// <param name="baseSize">Width of the base along the pyramid's X axis and height along its Y axis.</param>
        public static IPyramid CreatePyramid(
            this WireframeContainer container, Vector3 tip, Vector3 baseCenter, Vector2 baseSize)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(baseCenter - tip, Vector3.up);
            return new Pyramid(proxy, null, tip, rotation, Vector3.Distance(tip, baseCenter), baseSize);
        }

        /// <summary>
        /// Creates a white pyramid that follows <paramref name="bone"/>, from a tip to the center of its base, both in
        /// the bone's local space. Its spin around the axis keeps the base's Y side as close to the bone's up as it can.
        /// </summary>
        /// <param name="baseSize">Width of the base along the pyramid's X axis and height along its Y axis.</param>
        public static IPyramid CreatePyramid(
            this WireframeContainer container,
            Transform bone,
            Vector3 localTip,
            Vector3 localBaseCenter,
            Vector2 baseSize)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(localBaseCenter - localTip, Vector3.up);
            float length = Vector3.Distance(localTip, localBaseCenter);
            return new Pyramid(proxy, bone, localTip, rotation, length, baseSize);
        }

        /// <summary>
        /// Creates a white pyramid in world space with its tip at <paramref name="position"/> and its base
        /// <paramref name="length"/> along the +Z axis of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="baseSize">Width of the base along the rotation's X axis and height along its Y axis.</param>
        public static IPyramid CreatePyramid(
            this WireframeContainer container,
            Vector3 position,
            Quaternion rotation,
            float length,
            Vector2 baseSize)
        {
            return new Pyramid(WireframeContainer.ProxyOf(container), null, position, rotation, length, baseSize);
        }
    }
}
