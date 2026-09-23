using UnityEngine;
using UnityEngine.Rendering;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws everything the attached camera renders as wireframe. Works under the Built-in Render Pipeline and any
    /// Scriptable Render Pipeline (URP, HDRP or custom). Only the active pipeline's callbacks are subscribed, and they
    /// are swapped when the render pipeline asset changes at runtime.
    /// </summary>
    /// <remarks>
    /// Unity reports a pipeline asset change during the first render that already uses the new pipeline, so the frame
    /// in which a switch between Built-in and a Scriptable Render Pipeline happens is drawn without wireframe.
    /// </remarks>
    [RequireComponent(typeof(Camera))]
    public sealed class WireframeCamera : MonoBehaviour
    {
        private enum Pipeline
        {
            None,
            BuiltIn,
            Scriptable
        }

        private Camera _camera;
        private Pipeline _subscribed;
        private bool _wireframeSet;

        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
            RenderPipelineManager.activeRenderPipelineAssetChanged += OnRenderPipelineAssetChanged;
            Subscribe(GraphicsSettings.currentRenderPipeline);
        }

        private void OnDisable()
        {
            RenderPipelineManager.activeRenderPipelineAssetChanged -= OnRenderPipelineAssetChanged;
            Unsubscribe();
        }

        // activeRenderPipelineTypeChanged is not used: it is not raised when switching back to Built-in.
        private void OnRenderPipelineAssetChanged(RenderPipelineAsset previous, RenderPipelineAsset current) =>
            Subscribe(current);

        private void Subscribe(RenderPipelineAsset asset)
        {
            // The asset is checked instead of RenderPipelineManager.currentPipeline, which stays null until the
            // pipeline first renders and is not cleared when switching back to Built-in.
            Pipeline active = asset != null ? Pipeline.Scriptable : Pipeline.BuiltIn;
            if (active == _subscribed)
            {
                return;
            }

            Unsubscribe();
            if (active == Pipeline.Scriptable)
            {
                RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
                RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            }
            else
            {
                Camera.onPreRender += OnPreRenderCamera;
                Camera.onPostRender += OnPostRenderCamera;
            }
            _subscribed = active;
        }

        private void Unsubscribe()
        {
            if (_subscribed == Pipeline.Scriptable)
            {
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
                RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            }
            else if (_subscribed == Pipeline.BuiltIn)
            {
                Camera.onPreRender -= OnPreRenderCamera;
                Camera.onPostRender -= OnPostRenderCamera;
            }
            _subscribed = Pipeline.None;

            // Covers a switch or disable between the begin and end callbacks, so wireframe never leaks to other cameras.
            if (_wireframeSet)
            {
                GL.wireframe = false;
                _wireframeSet = false;
            }
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera) => OnPreRenderCamera(camera);

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera) => OnPostRenderCamera(camera);

        private void OnPreRenderCamera(Camera camera)
        {
            if (camera == _camera)
            {
                GL.wireframe = true;
                _wireframeSet = true;
            }
        }

        private void OnPostRenderCamera(Camera camera)
        {
            if (camera == _camera)
            {
                GL.wireframe = false;
                _wireframeSet = false;
            }
        }
    }
}
