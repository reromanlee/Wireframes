using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Polyline : PointShape, IPolyline
    {
        private readonly ShapePoint[] _points;
        private readonly bool _isClosed;

        internal Polyline(MeshProxy proxy, IReadOnlyList<Vector3> points, bool isClosed)
            : base(proxy, CheckCount(points, isClosed, nameof(points)), BuildEdges(points.Count, isClosed))
        {
            _isClosed = isClosed;
            _points = new ShapePoint[points.Count];
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = new ShapePoint(points[i]);
            }
        }

        /// <summary>Creates a polyline whose points sit at the origins of their bones.</summary>
        internal Polyline(MeshProxy proxy, IReadOnlyList<Transform> bones, bool isClosed)
            : base(proxy, CheckCount(bones, isClosed, nameof(bones)), BuildEdges(bones.Count, isClosed))
        {
            _isClosed = isClosed;
            _points = new ShapePoint[bones.Count];
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = new ShapePoint(Vector3.zero);
                AttachPoint(i, bones[i]);
            }
        }

        public int PointCount
        {
            get
            {
                ThrowIfDisposed();
                return _points.Length;
            }
        }

        public bool IsClosed
        {
            get
            {
                ThrowIfDisposed();
                return _isClosed;
            }
        }

        protected override ref ShapePoint Point(int index)
        {
            if ((uint)index >= (uint)_points.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The polyline has {_points.Length} points.");
            }
            return ref _points[index];
        }

        private static int CheckCount<T>(IReadOnlyList<T> items, bool isClosed, string parameterName)
        {
            if (items == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            int minimum = isClosed ? 3 : 2;
            if (items.Count < minimum)
            {
                throw new ArgumentException(
                    $"{(isClosed ? "A closed" : "An open")} polyline needs at least {minimum} points.", parameterName);
            }
            return items.Count;
        }

        private static int[] BuildEdges(int pointCount, bool isClosed)
        {
            int edgeCount = isClosed ? pointCount : pointCount - 1;
            int[] pattern = new int[edgeCount * 2];
            for (int edge = 0; edge < edgeCount; edge++)
            {
                pattern[edge * 2] = edge;
                pattern[edge * 2 + 1] = (edge + 1) % pointCount;
            }
            return pattern;
        }
    }
}
