using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// The containers that shape components share: one per <see cref="SharedContainerKey"/>, created by the first
    /// component that needs it and disposed once the last one lets go, and kept out of the Hierarchy. Containers of the
    /// main stage persist across scenes, since their components can be in any open scene, while a stage of its own,
    /// such as Prefab Mode, gets containers in its own scene, the only one its cameras draw.
    /// </summary>
    internal static class SharedContainers
    {
        private const string ContainerName = "Wireframe Components";

        private static readonly Dictionary<SharedContainerKey, SharedContainer> Containers = new();

        /// <summary>Number of containers that components share right now.</summary>
        internal static int Count
        {
            get => Containers.Count;
        }

        /// <summary>
        /// Returns the container for <paramref name="key"/> and counts the caller among its users, creating it first
        /// when there is none or when something else disposed it.
        /// </summary>
        internal static SharedContainer Acquire(SharedContainerKey key)
        {
            if (!Containers.TryGetValue(key, out SharedContainer shared))
            {
                shared = new SharedContainer(key, Create(key));
                Containers.Add(key, shared);
            }
            else if (shared.Container.IsDisposed)
            {
                // The other users notice their lost shapes on their next refresh and build them again.
                shared.Container = Create(key);
            }
            shared.UserCount++;
            return shared;
        }

        /// <summary>
        /// Stops counting a user of <paramref name="shared"/>, and disposes it when that was the last one.
        /// </summary>
        internal static void Release(SharedContainer shared)
        {
            if (--shared.UserCount > 0)
            {
                return;
            }
            Containers.Remove(shared.Key);
            shared.Container.Dispose();
        }

        /// <summary>
        /// The stage that <paramref name="gameObject"/> is drawn in: the scene of a stage of its own, such as a prefab open
        /// in Prefab Mode, or an invalid scene for the main stage.
        /// </summary>
        internal static Scene StageOf(GameObject gameObject)
        {
#if UNITY_EDITOR
            Scene scene = gameObject.scene;
            if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(scene))
            {
                return scene;
            }
#endif
            return default;
        }

        private static WireframeContainer Create(SharedContainerKey key)
        {
            bool isMainStage = !key.Stage.IsValid();
            WireframeContainerSettings settings = new()
            {
                Name = ContainerName,
                Occlusion = key.Occlusion,
                UseAlpha = key.UsesAlpha,
                Layer = key.Layer,
                PersistAcrossScenes = isMainStage
            };
            return new WireframeContainer(settings, key.Stage, true);
        }
    }
}
