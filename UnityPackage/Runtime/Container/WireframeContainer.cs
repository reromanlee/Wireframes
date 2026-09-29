using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Creates shapes and draws them with GPU-skinned meshes of up to 65,535 vertices each, plus one of its own for any
    /// bigger shape. Disposing the container, or unloading the scene it was created in, disposes every shape it created.
    /// </summary>
    /// <remarks>
    /// Main thread only. Each shape type adds its Create methods as extension methods, declared in a factory class next
    /// to the shape, such as <see cref="CircleFactory"/>. The container creates a GameObject in the active scene; edits
    /// made to its shapes are uploaded right before a camera renders them. Without a graphics device, as in server
    /// builds, or without a usable shader, shapes keep working but nothing is drawn or uploaded. Bones must be scene
    /// objects: Create methods and bone setters throw <see cref="ArgumentException"/> for a prefab asset.
    /// <para>
    /// Containers work the same in Edit Mode, drawing in the Scene and Game views. There, a container is never saved
    /// into its scene and never marks it as changed; it is disposed before scripts reload and when its scene closes,
    /// unless it persists across scenes. Switching between Edit and Play Mode alone never disposes a container, though
    /// the script reload that Play Mode starts with by default does.
    /// </para>
    /// </remarks>
    public sealed class WireframeContainer : IDisposable
    {
        private readonly MeshProxy _proxy;
        private bool _isDisposed;

        /// <summary>Creates a container with the default settings.</summary>
        public WireframeContainer() : this(null)
        {
        }

        /// <summary>
        /// Creates a container from <paramref name="settings"/>, or from the default settings when it is null. The
        /// settings are read once, so changing them later doesn't affect the container.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">A setting is outside its valid range.</exception>
        public WireframeContainer(WireframeContainerSettings settings)
        {
            MainThread.Check();
            settings ??= new WireframeContainerSettings();
            settings.Validate(nameof(settings));

            // In Edit Mode it is kept out of the saved scene, which also keeps the scene from being marked as changed.
            HideFlags hideFlags = HideFlags.NotEditable | (Application.isPlaying ? HideFlags.None : HideFlags.DontSave);
            GameObject proxyObject = new(settings.ResolvedName) { hideFlags = hideFlags, layer = settings.Layer };
            try
            {
                // Otherwise the proxy lives in the active scene, and unloading that scene disposes the container.
                if (settings.PersistAcrossScenes && Application.isPlaying)
                {
                    Object.DontDestroyOnLoad(proxyObject);
                }
                _proxy = proxyObject.AddComponent<MeshProxy>();
                _proxy.Initialize(this, settings);
            }
            catch
            {
                UnityObjects.Destroy(proxyObject);
                throw;
            }
        }

        /// <summary>True once the container was disposed or its scene was unloaded.</summary>
        public bool IsDisposed
        {
            get => _isDisposed;
        }

        /// <summary>
        /// False hides every shape of the container without disposing anything, and costs nothing while it lasts: edits
        /// made meanwhile are applied when it is shown again. Each shape's own <see cref="IShape.IsVisible"/> still
        /// counts. True by default.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The container is disposed.</exception>
        public bool IsVisible
        {
            get
            {
                MainThread.Check();
                return Proxy.IsVisible;
            }
            set
            {
                MainThread.Check();
                Proxy.IsVisible = value;
            }
        }

        internal MeshProxy Proxy
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(WireframeContainer));
                }
                return _proxy;
            }
        }

        /// <summary>Removes the container and disposes every shape it created. Calling it again does nothing.</summary>
        public void Dispose()
        {
            MainThread.Check();
            if (_isDisposed)
            {
                return;
            }
            try
            {
                _proxy.Shutdown();
            }
            finally
            {
                UnityObjects.Destroy(_proxy.gameObject);
            }
        }

        /// <summary>The proxy that a factory adds a new shape to, after checking <paramref name="container"/>.</summary>
        internal static MeshProxy ProxyOf(WireframeContainer container)
        {
            MainThread.Check();
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            return container.Proxy;
        }

        internal void OnProxyShutdown()
        {
            _isDisposed = true;
        }
    }
}
