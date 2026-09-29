using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates lines in a <see cref="WireframeContainer"/>.</summary>
    public static class LineFactory
    {
        /// <summary>Creates a white line with both endpoints at the world origin.</summary>
        public static ILine CreateLine(this WireframeContainer container)
        {
            return new Line(WireframeContainer.ProxyOf(container), Vector3.zero, Vector3.zero);
        }

        /// <summary>Creates a white line between two world positions.</summary>
        public static ILine CreateLine(this WireframeContainer container, Vector3 positionA, Vector3 positionB)
        {
            return new Line(WireframeContainer.ProxyOf(container), positionA, positionB);
        }

        /// <summary>
        /// Creates a white line from the origin of <paramref name="boneA"/> to the origin of <paramref name="boneB"/>,
        /// with each endpoint following its bone. A null bone puts that endpoint at the world origin.
        /// </summary>
        public static ILine CreateLine(this WireframeContainer container, Transform boneA, Transform boneB)
        {
            return new Line(WireframeContainer.ProxyOf(container), boneA, boneB);
        }
    }
}
