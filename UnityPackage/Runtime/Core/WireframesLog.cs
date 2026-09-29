using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes
{
    /// <summary>The package's console messages, all starting with [Wireframes] so they are easy to find and filter.</summary>
    internal static class WireframesLog
    {
        private const string Prefix = "[Wireframes] ";

        internal static void Warning(string message, Object context = null)
        {
            Debug.LogWarning(Prefix + message, context);
        }

        /// <summary>Logs <paramref name="message"/> as an error, followed by <paramref name="exception"/> and its stack trace.</summary>
        internal static void Error(string message, Exception exception, Object context = null)
        {
            Debug.LogError($"{Prefix}{message}\n{exception}", context);
        }
    }
}
