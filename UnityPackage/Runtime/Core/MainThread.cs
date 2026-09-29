using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Catches calls from other threads in the Editor and development builds, where they would race the render-time
    /// flush and corrupt shapes without a word. Release builds leave the check out.
    /// </summary>
    internal static class MainThread
    {
        private const int Unknown = -1;

        private static int _id = Unknown;

        /// <exception cref="InvalidOperationException">The caller isn't on Unity's main thread.</exception>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        internal static void Check()
        {
            if (_id != Unknown && Thread.CurrentThread.ManagedThreadId != _id)
            {
                throw new InvalidOperationException("Wireframes can only be used from Unity's main thread.");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void Capture()
        {
            _id = Thread.CurrentThread.ManagedThreadId;
        }
    }
}
