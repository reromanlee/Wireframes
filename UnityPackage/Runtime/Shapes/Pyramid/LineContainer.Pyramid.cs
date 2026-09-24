using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public IPyramid CreatePyramid()
        {
            return new Pyramid(Proxy, null, Vector3.zero, Quaternion.identity, UnitSize, Vector2.one * UnitSize);
        }

        public IPyramid CreatePyramid(Vector3 tip, Vector3 baseCenter, Vector2 baseSize)
        {
            Quaternion rotation = ShapeRotations.FromAxis(baseCenter - tip, Vector3.up);
            return new Pyramid(Proxy, null, tip, rotation, Vector3.Distance(tip, baseCenter), baseSize);
        }

        public IPyramid CreatePyramid(Transform bone, Vector3 localTip, Vector3 localBaseCenter, Vector2 baseSize)
        {
            Quaternion rotation = ShapeRotations.FromAxis(localBaseCenter - localTip, Vector3.up);
            return new Pyramid(Proxy, bone, localTip, rotation, Vector3.Distance(localTip, localBaseCenter), baseSize);
        }

        public IPyramid CreatePyramid(Vector3 position, Quaternion rotation, float length, Vector2 baseSize)
        {
            return new Pyramid(Proxy, null, position, rotation, length, baseSize);
        }
    }
}
