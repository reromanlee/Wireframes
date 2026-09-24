using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>Creates a white line with both endpoints at the world origin.</summary>
        ILine CreateLine();

        /// <summary>Creates a white line between two world positions.</summary>
        ILine CreateLine(Vector3 positionA, Vector3 positionB);

        /// <summary>
        /// Creates a white line from the origin of <paramref name="boneA"/> to the origin of <paramref name="boneB"/>,
        /// with each endpoint following its bone. A null bone puts that endpoint at the world origin.
        /// </summary>
        ILine CreateLine(Transform boneA, Transform boneB);
    }
}
