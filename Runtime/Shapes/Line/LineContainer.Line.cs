using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public ILine CreateLine()
        {
            return new Line(Proxy, Vector3.zero, Vector3.zero);
        }

        public ILine CreateLine(Vector3 positionA, Vector3 positionB)
        {
            return new Line(Proxy, positionA, positionB);
        }
    }
}
