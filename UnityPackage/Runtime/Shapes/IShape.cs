using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A shape drawn by a <see cref="WireframeContainer"/>. Disposing it removes it from the container. Shape
    /// interfaces are implemented only by the package, so members may be added to them in minor versions.
    /// Every member except <see cref="IsDisposed"/> and <see cref="IDisposable.Dispose"/> throws
    /// <see cref="ObjectDisposedException"/> once the shape is disposed.
    /// </summary>
    public interface IShape : IDisposable
    {
        /// <summary>True once the shape or its container was disposed.</summary>
        bool IsDisposed { get; }

        /// <summary>
        /// False hides the shape without disposing it: it keeps its place, bones and every setting, and draws again as it
        /// is then when shown. Hiding and showing allocate nothing. True by default.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>Sets the color of every vertex of the shape.</summary>
        void SetColor(Color color);
    }
}
