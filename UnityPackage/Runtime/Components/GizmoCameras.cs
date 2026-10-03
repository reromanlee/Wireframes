using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Decides which cameras draw the shapes of components drawn as gizmos: the ones Unity draws gizmos for.
    /// </summary>
    internal static class GizmoCameras
    {
        /// <summary>The check behind <see cref="ShouldDraw"/>, which tests replace.</summary>
        internal static Func<Camera, bool> Filter { get; set; } = DrawsGizmos;

        /// <summary>The camera filter of the containers that hold gizmo shapes.</summary>
        internal static bool ShouldDraw(Camera camera)
        {
            return Filter(camera);
        }

        /// <summary>
        /// True for a Scene view camera while the Scene view's Gizmos button is on, and for a game camera while the Game
        /// view that renders it has its Gizmos button on. Other cameras, such as previews and reflection probes, never
        /// draw gizmos, and outside the Editor no camera does.
        /// </summary>
        internal static bool DrawsGizmos(Camera camera)
        {
#if UNITY_EDITOR
            switch (camera.cameraType)
            {
                case CameraType.SceneView:
                    return UnityEditor.Handles.ShouldRenderGizmos();
                case CameraType.Game:
                    // A Scene view renders game cameras too, for its camera preview, which shows no gizmos.
                    return UnityEditor.SceneView.currentDrawingSceneView == null
                           && UnityEditor.Handles.ShouldRenderGizmos();
                default:
                    return false;
            }
#else
            return false;
#endif
        }
    }
}
