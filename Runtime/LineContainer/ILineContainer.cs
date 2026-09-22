using System;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Creates shapes and draws all of them with one GPU-skinned mesh. Disposing the container, or unloading the
    /// scene it was created in, disposes every shape it created.
    /// </summary>
    /// <remarks>Each shape type declares its factory methods in a partial file next to the shape.</remarks>
    public partial interface ILineContainer : IDisposable
    {
        /// <summary>True once the container was disposed or its scene was unloaded.</summary>
        bool IsDisposed { get; }
    }
}
