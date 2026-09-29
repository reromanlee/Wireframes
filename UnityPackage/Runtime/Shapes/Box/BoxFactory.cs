using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates boxes in a <see cref="WireframeContainer"/>.</summary>
    public static class BoxFactory
    {
        /// <summary>Creates a white, world-aligned unit cube centered on the world origin.</summary>
        public static IBox CreateBox(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Box(proxy, null, Vector3.zero, Quaternion.identity, Vector3.one * ShapeDefaults.Size);
        }

        /// <summary>Creates a white, world-aligned box spanned by two opposite world-space corners.</summary>
        public static IBox CreateBox(this WireframeContainer container, Vector3 cornerA, Vector3 cornerB)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Box(proxy, null, (cornerA + cornerB) * 0.5f, Quaternion.identity, cornerB - cornerA);
        }

        /// <summary>
        /// Creates a white box that follows <paramref name="bone"/>, aligned to the bone's axes and spanned by two
        /// opposite corners in the bone's local space.
        /// </summary>
        public static IBox CreateBox(
            this WireframeContainer container, Transform bone, Vector3 localCornerA, Vector3 localCornerB)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Vector3 center = (localCornerA + localCornerB) * 0.5f;
            return new Box(proxy, bone, center, Quaternion.identity, localCornerB - localCornerA);
        }

        /// <summary>Creates a white box in world space from its center, rotation and size.</summary>
        public static IBox CreateBox(this WireframeContainer container, Vector3 center, Quaternion rotation, Vector3 size)
        {
            return new Box(WireframeContainer.ProxyOf(container), null, center, rotation, size);
        }
    }
}
