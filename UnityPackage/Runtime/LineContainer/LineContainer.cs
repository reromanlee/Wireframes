using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <inheritdoc cref="ILineContainer"/>
    /// <remarks>
    /// Main thread only. The container creates a GameObject in the active scene; edits made to its shapes are
    /// uploaded once per frame at the end of LateUpdate.
    /// </remarks>
    public sealed partial class LineContainer : ILineContainer
    {
        private const string ProxyName = "Wireframes";

        // Shapes created without arguments are sized like Unity's primitives: 1 unit across and 1 unit long.
        private const float UnitSize = 1f;
        private const float UnitRadius = 0.5f;

        private readonly MeshProxy _proxy;
        private bool _isDisposed;

        /// <summary>Creates a container that draws with the package's vertex color material.</summary>
        public LineContainer() : this(null)
        {
        }

        /// <summary>Creates a container that draws with <paramref name="material"/>, which stays owned by the caller.</summary>
        public LineContainer(Material material)
        {
            // The proxy lives in the active scene, so unloading that scene disposes the container.
            GameObject proxyObject = new(ProxyName) { hideFlags = HideFlags.NotEditable };
            try
            {
                _proxy = proxyObject.AddComponent<MeshProxy>();
                _proxy.Initialize(this, material);
            }
            catch
            {
                UnityObjects.Destroy(proxyObject);
                throw;
            }
        }

        public bool IsDisposed
        {
            get => _isDisposed;
        }

        internal MeshProxy Proxy
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(LineContainer));
                }
                return _proxy;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _proxy.Shutdown();
            UnityObjects.Destroy(_proxy.gameObject);
        }

        internal void OnProxyShutdown()
        {
            _isDisposed = true;
        }
    }
}
