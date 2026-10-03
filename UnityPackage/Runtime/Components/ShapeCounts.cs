using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// The counts that shape components pass on to their runtime shapes, which fix them at creation: checks that throw
    /// for scripts, and corrections to the nearest valid count for serialized fields.
    /// </summary>
    internal static class ShapeCounts
    {
        private const int MinSegmentCount = 3;
        private const int MinQuarterSegmentCount = 4;
        private const int MinPointCount = 3;
        private const int MinSideCount = 3;

        private static readonly int[] SpikeCounts = { 4, 6, 8, 12, 20 };

        /// <exception cref="ArgumentOutOfRangeException">The count is outside 3 to 1024.</exception>
        internal static int CheckSegmentCount(int value)
        {
            if (value < MinSegmentCount || value > Ring.MaxSegmentCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, $"A ring needs {MinSegmentCount} to {Ring.MaxSegmentCount} segments.");
            }
            return value;
        }

        /// <exception cref="ArgumentOutOfRangeException">The count isn't a multiple of 4 from 4 to 1024.</exception>
        internal static int CheckQuarterSegmentCount(int value)
        {
            if (value < MinQuarterSegmentCount || value % 4 != 0 || value > Ring.MaxSegmentCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $"This shape needs a multiple of 4 segments, from {MinQuarterSegmentCount} to {Ring.MaxSegmentCount}.");
            }
            return value;
        }

        /// <exception cref="ArgumentOutOfRangeException">The count is outside 3 to 512.</exception>
        internal static int CheckPointCount(int value)
        {
            if (value < MinPointCount || value > Star.MaxPointCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, $"A star needs {MinPointCount} to {Star.MaxPointCount} points.");
            }
            return value;
        }

        /// <exception cref="ArgumentOutOfRangeException">The count is outside 3 to 1024.</exception>
        internal static int CheckSideCount(int value)
        {
            if (value < MinSideCount || value > Ring.MaxSegmentCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, $"A frustum needs {MinSideCount} to {Ring.MaxSegmentCount} sides.");
            }
            return value;
        }

        /// <exception cref="ArgumentOutOfRangeException">The count isn't 4, 6, 8, 12 or 20.</exception>
        internal static int CheckSpikeCount(int value)
        {
            if (!PlatonicSolid.HasFaceCount(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, "A spiked sphere needs 4, 6, 8, 12 or 20 spikes.");
            }
            return value;
        }

        internal static int ClampSegmentCount(int value)
        {
            return Mathf.Clamp(value, MinSegmentCount, Ring.MaxSegmentCount);
        }

        /// <summary>The nearest multiple of 4 from 4 to 1024, rounding halfway counts up.</summary>
        internal static int ClampQuarterSegmentCount(int value)
        {
            int clamped = Mathf.Clamp(value, MinQuarterSegmentCount, Ring.MaxSegmentCount);
            return (clamped + 2) / 4 * 4;
        }

        internal static int ClampPointCount(int value)
        {
            return Mathf.Clamp(value, MinPointCount, Star.MaxPointCount);
        }

        internal static int ClampSideCount(int value)
        {
            return Mathf.Clamp(value, MinSideCount, Ring.MaxSegmentCount);
        }

        /// <summary>The nearest spike count a spiked sphere can have, the smaller one when two are as near.</summary>
        internal static int ClampSpikeCount(int value)
        {
            int nearest = SpikeCounts[0];
            for (int i = 1; i < SpikeCounts.Length; i++)
            {
                if (Math.Abs((long)SpikeCounts[i] - value) < Math.Abs((long)nearest - value))
                {
                    nearest = SpikeCounts[i];
                }
            }
            return nearest;
        }
    }
}
