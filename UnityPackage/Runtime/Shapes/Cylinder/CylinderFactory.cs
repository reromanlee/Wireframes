using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates cylinders in a <see cref="WireframeContainer"/>.</summary>
    public static class CylinderFactory
    {
        /// <summary>Creates a white cylinder of radius 0.5 from the world origin, 1 unit along +Z.</summary>
        public static ICylinder CreateCylinder(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return proxy.Add(new Cylinder(
                null,
                Vector3.zero,
                Quaternion.identity,
                ShapeDefaults.Size,
                ShapeDefaults.Radius,
                Ring.DefaultSegmentCount));
        }

        /// <summary>
        /// Creates a white cylinder between two world positions. Its spin around the axis keeps the cylinder's +Y as
        /// close to world up as it can.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces each ring is drawn with, a multiple of 4 from 4 to 1024.</param>
        public static ICylinder CreateCylinder(
            this WireframeContainer container,
            Vector3 endA,
            Vector3 endB,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(endB - endA, Vector3.up);
            return proxy.Add(new Cylinder(null, endA, rotation, Vector3.Distance(endA, endB), radius, segmentCount));
        }

        /// <summary>
        /// Creates a white cylinder that follows <paramref name="bone"/>, between two positions in the bone's local
        /// space. Its spin around the axis keeps the cylinder's +Y as close to the bone's up as it can.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces each ring is drawn with, a multiple of 4 from 4 to 1024.</param>
        public static ICylinder CreateCylinder(
            this WireframeContainer container,
            Transform bone,
            Vector3 localEndA,
            Vector3 localEndB,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(localEndB - localEndA, Vector3.up);
            float length = Vector3.Distance(localEndA, localEndB);
            return proxy.Add(new Cylinder(bone, localEndA, rotation, length, radius, segmentCount));
        }

        /// <summary>
        /// Creates a white cylinder in world space that starts at <paramref name="position"/> and runs
        /// <paramref name="length"/> along the +Z axis of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces each ring is drawn with, a multiple of 4 from 4 to 1024.</param>
        public static ICylinder CreateCylinder(
            this WireframeContainer container,
            Vector3 position,
            Quaternion rotation,
            float length,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return proxy.Add(new Cylinder(null, position, rotation, length, radius, segmentCount));
        }
    }
}
