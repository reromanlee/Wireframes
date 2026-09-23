using UnityEngine;
using UnityEngine.Rendering;

namespace reromanlee.Wireframes
{
    [RequireComponent(typeof(Camera))]
    public class URPWireframeCamera : MonoBehaviour
    {
        private Camera camera;

        private void OnEnable()
        {
            camera = GetComponent<Camera>();
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == this.camera)
            {
                GL.wireframe = true;
            }
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == this.camera)
            {
                GL.wireframe = false;
            }
        }
    }
}