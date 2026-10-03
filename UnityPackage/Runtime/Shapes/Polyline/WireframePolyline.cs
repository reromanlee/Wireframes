using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a polyline on its GameObject: points joined in order, each following its own bone, or the GameObject when it
    /// has none. Closed, it joins the last point back to the first, as in a polygon or a triangle. It draws nothing with
    /// fewer than 2 points, or 3 when closed. By default it is a small triangle lying flat on the GameObject.
    /// </summary>
    [AddComponentMenu("Wireframes/Polyline")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframePolyline : WireframePointShape
    {
        private const float DefaultRadius = 0.5f;

        [Tooltip("Points in order, each relative to its bone, or to the GameObject without one.")]
        [SerializeField] private PolylinePoint[] _points = TrianglePoints();

        [Tooltip("Joins the last point back to the first, as in a polygon.")]
        [SerializeField] private bool _isClosed = true;

        /// <summary>
        /// Number of points. Raising it adds points at the GameObject's origin, and lowering it drops the last ones. With
        /// fewer than 2, or 3 when closed, nothing is drawn. Changing it creates the shape again.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The count is negative.</exception>
        public int PointCount
        {
            get => _points.Length;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), value, "A polyline can't have a negative number of points.");
                }
                Array.Resize(ref _points, value);
                Refresh();
            }
        }

        /// <summary>
        /// True joins the last point back to the first, as in a polygon. True by default. Changing it creates the shape
        /// again.
        /// </summary>
        public bool IsClosed
        {
            get => _isClosed;
            set
            {
                _isClosed = value;
                Refresh();
            }
        }

        internal override int BuildSignature
        {
            get => _points.Length * 2 + (_isClosed ? 1 : 0);
        }

        internal override bool CanCreateShape
        {
            get => _points.Length >= (_isClosed ? 3 : 2);
        }

        /// <summary>
        /// Returns the Transform that point <paramref name="index"/> follows, or null when it follows the GameObject.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the points.</exception>
        public Transform GetBone(int index)
        {
            return Point(index).Bone;
        }

        /// <summary>
        /// Sets the Transform that point <paramref name="index"/> follows, or null to follow the GameObject. A destroyed bone
        /// counts as none, and so does one that isn't in a scene, such as a prefab asset.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the points.</exception>
        public void SetBone(int index, Transform bone)
        {
            ref PolylinePoint point = ref Point(index);
            point.Bone = bone;
            if (TryGetShape(out Polyline polyline))
            {
                polyline.SetBone(index, ResolveBone(bone));
                polyline.SetLocalPosition(index, point.Position);
            }
        }

        /// <summary>
        /// Returns point <paramref name="index"/> relative to its bone, or to the GameObject without one.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the points.</exception>
        public Vector3 GetPosition(int index)
        {
            return Point(index).Position;
        }

        /// <summary>
        /// Sets point <paramref name="index"/> relative to its bone, or to the GameObject without one.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the points.</exception>
        public void SetPosition(int index, Vector3 position)
        {
            Point(index).Position = position;
            if (TryGetShape(out Polyline polyline))
            {
                polyline.SetLocalPosition(index, position);
            }
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new Polyline(_points.Length, _isClosed);
        }

        internal override void ApplyTo(Shape shape)
        {
            // Each bone goes first, since changing it keeps the point's world position rather than its local one.
            Polyline polyline = (Polyline)shape;
            for (int i = 0; i < _points.Length; i++)
            {
                polyline.SetBone(i, ResolveBone(_points[i].Bone));
                polyline.SetLocalPosition(i, _points[i].Position);
            }
        }

        internal override void Sanitize()
        {
            _points ??= Array.Empty<PolylinePoint>();
        }

        private ref PolylinePoint Point(int index)
        {
            if ((uint)index >= (uint)_points.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The polyline has {_points.Length} points.");
            }
            return ref _points[index];
        }

        /// <summary>
        /// An equilateral triangle lying flat around the GameObject's origin, with a corner on its +Z axis.
        /// </summary>
        private static PolylinePoint[] TrianglePoints()
        {
            PolylinePoint[] points = new PolylinePoint[3];
            CirclePoints circle = new(points.Length);
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 point = circle.Next();
                points[i].Position = new Vector3(point.y, 0f, point.x) * DefaultRadius;
            }
            return points;
        }

        [Serializable]
        private struct PolylinePoint
        {
            [Tooltip("Transform the point follows. None follows the GameObject.")]
            public Transform Bone;

            [Tooltip("Position relative to the bone, or to the GameObject without one.")]
            public Vector3 Position;
        }
    }
}
