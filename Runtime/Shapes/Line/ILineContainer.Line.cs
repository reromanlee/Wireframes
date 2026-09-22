using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>Creates a white line with both endpoints at the world origin.</summary>
        ILine CreateLine();

        /// <summary>Creates a white line between two world positions.</summary>
        ILine CreateLine(Vector3 positionA, Vector3 positionB);
    }
}
