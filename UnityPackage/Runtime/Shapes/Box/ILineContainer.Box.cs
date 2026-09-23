using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>Creates a white, world-aligned unit cube centered on the world origin.</summary>
        IBox CreateBox();

        /// <summary>Creates a white, world-aligned box spanned by two opposite world-space corners.</summary>
        IBox CreateBox(Vector3 cornerA, Vector3 cornerB);

        /// <summary>
        /// Creates a white box that follows <paramref name="bone"/>, aligned to the bone's axes and spanned by two
        /// opposite corners in the bone's local space.
        /// </summary>
        IBox CreateBox(Transform bone, Vector3 localCornerA, Vector3 localCornerB);

        /// <summary>Creates a white box in world space from its center, rotation and size.</summary>
        IBox CreateBox(Vector3 center, Quaternion rotation, Vector3 size);
    }
}
