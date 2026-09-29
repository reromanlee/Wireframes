using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Points of a unit circle at equal angles, starting at angle 0 and turning from +X toward +Y, worked out one after
    /// another instead of kept in a table. Each step rotates the last point in double precision, so even a full turn of
    /// thousands of steps stays far closer than float precision can tell.
    /// </summary>
    internal struct CirclePoints
    {
        private readonly double _stepCosine;
        private readonly double _stepSine;
        private double _x;
        private double _y;

        /// <param name="count">Points in a full turn.</param>
        /// <param name="first">Index of the first point to return.</param>
        internal CirclePoints(int count, int first = 0)
        {
            double step = 2.0 * Math.PI / count;
            _stepCosine = Math.Cos(step);
            _stepSine = Math.Sin(step);
            _x = Math.Cos(step * first);
            _y = Math.Sin(step * first);
        }

        /// <summary>Returns the current point and moves on to the next one: (cosine, sine) of its angle.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Vector2 Next()
        {
            Vector2 point = new((float)_x, (float)_y);
            double x = _x * _stepCosine - _y * _stepSine;
            _y = _x * _stepSine + _y * _stepCosine;
            _x = x;
            return point;
        }
    }
}
