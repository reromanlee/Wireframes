using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates rectangles in a <see cref="WireframeContainer"/>.</summary>
    public static class RectangleFactory
    {
        /// <summary>Creates a white 1 by 1 rectangle around the world origin, lying flat in the XZ plane.</summary>
        public static IRectangle CreateRectangle(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Rectangle(proxy, null, Vector3.zero, Quaternion.identity, Vector2.one * ShapeDefaults.Size);
        }

        /// <summary>
        /// Creates a white, world-aligned rectangle lying flat in the XZ plane, spanned by two opposite world-space
        /// corners. If the corners differ in height, the rectangle lies halfway between them.
        /// </summary>
        public static IRectangle CreateRectangle(this WireframeContainer container, Vector3 cornerA, Vector3 cornerB)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Vector3 center = (cornerA + cornerB) * 0.5f;
            return new Rectangle(proxy, null, center, Quaternion.identity, RectangleShape.FlatSpan(cornerA, cornerB));
        }

        /// <summary>
        /// Creates a white rectangle that follows <paramref name="bone"/>, lying flat in the bone's XZ plane and spanned
        /// by two opposite corners in the bone's local space.
        /// </summary>
        public static IRectangle CreateRectangle(
            this WireframeContainer container, Transform bone, Vector3 localCornerA, Vector3 localCornerB)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Vector3 center = (localCornerA + localCornerB) * 0.5f;
            Vector2 size = RectangleShape.FlatSpan(localCornerA, localCornerB);
            return new Rectangle(proxy, bone, center, Quaternion.identity, size);
        }

        /// <summary>Creates a white rectangle in world space, lying in the XZ plane of <paramref name="rotation"/>.</summary>
        public static IRectangle CreateRectangle(
            this WireframeContainer container, Vector3 center, Quaternion rotation, Vector2 size)
        {
            return new Rectangle(WireframeContainer.ProxyOf(container), null, center, rotation, size);
        }
    }
}
