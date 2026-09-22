using UnityEngine;

namespace reromanlee.Wireframes
{
    internal static class UnityObjects
    {
        /// <summary>Destroys <paramref name="target"/> with the call Unity allows in the current mode.</summary>
        public static void Destroy(Object target)
        {
            if (target == null)
            {
                return;
            }
            if (Application.isPlaying)
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
