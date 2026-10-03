using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Polyline : PointShape, IPolyline
    {
        private readonly ShapePoint[] _points;
        private readonly bool _isClosed;

        internal Polyline(IReadOnlyList<Vector3> points, bool isClosed)
            : base(CheckCount(points, isClosed, nameof(points)), BuildEdges(points.Count, isClosed))
        {
            _isClosed = isClosed;
            _points = new ShapePoint[points.Count];
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = new ShapePoint(points[i]);
            }
        }

        /// <summary>Creates a polyline of <paramref name="pointCount"/> white points at the world origin.</summary>
        internal Polyline(int pointCount, bool isClosed)
            : base(CheckCount(pointCount, isClosed, nameof(pointCount)), BuildEdges(pointCount, isClosed))
        {
            _isClosed = isClosed;
            _points = new ShapePoint[pointCount];
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = new ShapePoint(Vector3.zero);
            }
        }

        /// <summary>Creates a polyline whose points sit at the origins of their bones.</summary>
        internal Polyline(IReadOnlyList<Transform> bones, bool isClosed)
            : base(CheckCount(bones, isClosed, nameof(bones)), BuildEdges(bones.Count, isClosed))
        {
            _isClosed = isClosed;
            _points = new ShapePoint[bones.Count];
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = new ShapePoint(Vector3.zero);
                InitializeBone(i, bones[i], nameof(bones));
            }
        }

        public int PointCount
        {
            get
            {
                EnsureUsable();
                return _points.Length;
            }
        }

        public bool IsClosed
        {
            get
            {
                EnsureUsable();
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
            return CheckCount(items.Count, isClosed, parameterName);
        }

        private static int CheckCount(int pointCount, bool isClosed, string parameterName)
        {
            int minimum = isClosed ? 3 : 2;
            if (pointCount < minimum)
            {
                throw new ArgumentException(
                    $"{(isClosed ? "A closed" : "An open")} polyline needs at least {minimum} points.", parameterName);
            }
            return pointCount;
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
