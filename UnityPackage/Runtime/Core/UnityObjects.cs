using UnityEngine;

namespace reromanlee.Wireframes
{
    internal static class UnityObjects
    {
        /// <summary>
        /// True makes <see cref="Destroy"/> destroy right away in Play Mode too, for work that runs where no later frame
        /// would do it, such as right before scripts reload.
        /// </summary>
        internal static bool DestroysImmediately { get; set; }

        /// <summary>Destroys <paramref name="target"/> with the call Unity allows in the current mode.</summary>
        public static void Destroy(Object target)
        {
            if (target == null)
            {
                return;
            }
            if (Application.isPlaying && !DestroysImmediately)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
