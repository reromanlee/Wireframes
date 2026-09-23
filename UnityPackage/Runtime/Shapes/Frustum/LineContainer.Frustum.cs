using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        private const int UnitFrustumSides = 4;

        public IFrustum CreateFrustum()
        {
            return new Frustum(
                Proxy, null, Vector3.zero, Quaternion.identity, UnitSize, UnitRadius * 0.5f, UnitRadius, UnitFrustumSides);
        }

        public IFrustum CreateFrustum(Vector3 endA, Vector3 endB, float radiusA, float radiusB, int sides)
        {
            Quaternion rotation = ShapeRotations.FromAxis(endB - endA, Vector3.up);
            return new Frustum(Proxy, null, endA, rotation, Vector3.Distance(endA, endB), radiusA, radiusB, sides);
        }

        public IFrustum CreateFrustum(
            Transform bone, Vector3 localEndA, Vector3 localEndB, float radiusA, float radiusB, int sides)
        {
            Quaternion rotation = ShapeRotations.FromAxis(localEndB - localEndA, Vector3.up);
            float length = Vector3.Distance(localEndA, localEndB);
            return new Frustum(Proxy, bone, localEndA, rotation, length, radiusA, radiusB, sides);
        }

        public IFrustum CreateFrustum(
            Vector3 position, Quaternion rotation, float length, float radiusA, float radiusB, int sides)
        {
            return new Frustum(Proxy, null, position, rotation, length, radiusA, radiusB, sides);
        }
    }
}
