using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// Gives containers created in Edit Mode the lifetime a scene object would have. They are disposed before scripts
    /// reload, which would otherwise leave their GameObjects behind without their state, and when the scene they were
    /// created in closes, unless they persist across scenes. Their idle chunks are released from the editor loop, where
    /// objects may be destroyed right away. Entering or leaving Play Mode alone disposes nothing.
    /// </summary>
    [InitializeOnLoad]
    internal static class EditModeLifecycle
    {
        static EditModeLifecycle()
        {
            AssemblyReloadEvents.beforeAssemblyReload += DisposeEveryContainer;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorApplication.update += ReleaseIdleChunks;
        }

        /// <summary>Disposes every container, Play Mode ones included, destroying their objects right away.</summary>
        internal static void DisposeEveryContainer()
        {
            IReadOnlyList<MeshProxy> proxies = MeshProxy.LiveProxies;
            // No frame comes after a reload starts, so nothing may be left for the end of one.
            UnityObjects.DestroysImmediately = true;
            try
            {
                for (int i = proxies.Count - 1; i >= 0; i--)
                {
                    proxies[i].Container.Dispose();
                }
            }
            finally
            {
                UnityObjects.DestroysImmediately = false;
            }
        }

        /// <summary>
        /// Disposes the Edit Mode containers of a scene that closes, and moves the persistent ones into another open
        /// scene, since closing a scene destroys even the objects it never saves.
        /// </summary>
        internal static void OnSceneClosing(Scene scene, bool removingScene)
        {
            // Entering Play Mode closes the edited scene, and leaving it closes the played ones; neither disposes anything.
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            IReadOnlyList<MeshProxy> proxies = MeshProxy.LiveProxies;
            for (int i = proxies.Count - 1; i >= 0; i--)
            {
                MeshProxy proxy = proxies[i];
                if (!proxy.IsEditMode || proxy.HomeScene != scene)
                {
                    continue;
                }
                if (proxy.PersistsAcrossScenes)
                {
                    MoveToAnotherScene(proxy, scene);
                }
                else
                {
                    proxy.Container.Dispose();
                }
            }
        }

        /// <summary>Releases the idle chunks of every container while not in Play Mode, which releases its own.</summary>
        internal static void ReleaseIdleChunks()
        {
            if (Application.isPlaying)
            {
                return;
            }
            IReadOnlyList<MeshProxy> proxies = MeshProxy.LiveProxies;
            float time = Time.realtimeSinceStartup;
            for (int i = proxies.Count - 1; i >= 0; i--)
            {
                proxies[i].ReleaseIdleChunks(time);
            }
        }

        /// <summary>
        /// Moves <paramref name="proxy"/> into another loaded scene. When none is left, as when a scene replaces all
        /// open ones, the proxy stays out of any scene, where it keeps drawing.
        /// </summary>
        private static void MoveToAnotherScene(MeshProxy proxy, Scene closing)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene != closing)
                {
                    SceneManager.MoveGameObjectToScene(proxy.gameObject, scene);
                    proxy.HomeScene = scene;
                    return;
                }
            }
        }
    }
}
