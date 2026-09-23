using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>Creates a white box with both corners at the world origin.</summary>
        IBox CreateBox();

        /// <summary>Creates a white, world-aligned box spanned by two opposite world-space corners.</summary>
        IBox CreateBox(Vector3 cornerA, Vector3 cornerB);
    }
}
