using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>Creates a white 1 by 1 rectangle around the world origin, lying flat in the XZ plane.</summary>
        IRectangle CreateRectangle();

        /// <summary>
        /// Creates a white, world-aligned rectangle lying flat in the XZ plane, spanned by two opposite world-space
        /// corners. If the corners differ in height, the rectangle lies halfway between them.
        /// </summary>
        IRectangle CreateRectangle(Vector3 cornerA, Vector3 cornerB);

        /// <summary>
        /// Creates a white rectangle that follows <paramref name="bone"/>, lying flat in the bone's XZ plane and spanned
        /// by two opposite corners in the bone's local space.
        /// </summary>
        IRectangle CreateRectangle(Transform bone, Vector3 localCornerA, Vector3 localCornerB);

        /// <summary>Creates a white rectangle in world space, lying in the XZ plane of <paramref name="rotation"/>.</summary>
        IRectangle CreateRectangle(Vector3 center, Quaternion rotation, Vector2 size);
    }
}
