using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Renders the world around the origin with a camera of its own and reads back what was drawn there.</summary>
    internal static class CenterProbe
    {
        private const int Size = 32;
        private const float HalfExtent = 4f;

        /// <summary>
        /// Renders with an orthographic camera that looks along +Z at the origin, 8 units across, and returns the red at
        /// the center. Everything the probe creates is destroyed before it returns.
        /// </summary>
        internal static float RedAtCenter()
        {
            RenderTexture target = new(Size, Size, 24);
            Texture2D readback = new(Size, Size, TextureFormat.RGBA32, false);
            GameObject cameraObject = new("Probe Camera");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                camera.allowMSAA = false;
                camera.orthographic = true;
                camera.orthographicSize = HalfExtent;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.targetTexture = target;
                camera.Render();
                camera.targetTexture = null;

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = target;
                readback.ReadPixels(new Rect(0f, 0f, Size, Size), 0, 0);
                readback.Apply();
                RenderTexture.active = previous;
                float red = 0f;
                for (int y = Size / 2 - 1; y <= Size / 2; y++)
                {
                    for (int x = Size / 2 - 1; x <= Size / 2; x++)
                    {
                        red = Mathf.Max(red, readback.GetPixel(x, y).r);
                    }
                }
                return red;
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(readback);
                Object.DestroyImmediate(target);
            }
        }
    }
}
