using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A shape drawn by an <see cref="ILineContainer"/>. Disposing it removes it from the container.
    /// Every member except <see cref="IsDisposed"/> and <see cref="IDisposable.Dispose"/> throws
    /// <see cref="ObjectDisposedException"/> once the shape is disposed.
    /// </summary>
    public interface IShape : IDisposable
    {
        /// <summary>True once the shape or its container was disposed.</summary>
        bool IsDisposed { get; }

        /// <summary>Sets the color of every vertex of the shape.</summary>
        void SetColor(Color color);
    }
}
