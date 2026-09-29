using UnityEngine;
using UnityEngine.Rendering;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Builds the package's materials for a container's settings. Every look is the same shader with a different depth
    /// test and blending; with <see cref="WireframeOcclusion.Fade"/>, a second material draws the same mesh again, only
    /// where other geometry hides it, and dimmer.
    /// </summary>
    internal static class WireframeMaterials
    {
        internal const string ShaderName = "reromanlee/Wireframes/Unlit";

        // Share of a color's alpha that hidden parts keep with Fade.
        private const float FadedAlpha = 0.3f;

        private static readonly int DepthTestId = Shader.PropertyToID("_ZTest");
        private static readonly int DepthWriteId = Shader.PropertyToID("_ZWrite");
        private static readonly int SourceBlendId = Shader.PropertyToID("_SrcBlend");
        private static readonly int DestinationBlendId = Shader.PropertyToID("_DstBlend");
        private static readonly int VertexAlphaId = Shader.PropertyToID("_WireframesVertexAlpha");
        private static readonly int AlphaScaleId = Shader.PropertyToID("_WireframesAlphaScale");

        /// <summary>The renderer's materials: the lines, then with Fade the dimmed hidden parts. The caller owns them.</summary>
        internal static Material[] Create(Shader shader, WireframeOcclusion occlusion, bool useAlpha)
        {
            Material lines = CreateMaterial(shader, "Wireframes");
            if (occlusion == WireframeOcclusion.Show)
            {
                // Drawn after the scene's opaque geometry, over all of it.
                SetState(lines, CompareFunction.Always, false, useAlpha);
                SetQueue(lines, RenderQueue.Transparent);
            }
            else
            {
                SetState(lines, CompareFunction.LessEqual, !useAlpha, useAlpha);
                SetQueue(lines, useAlpha ? RenderQueue.Transparent : RenderQueue.Geometry);
            }
            SetAlpha(lines, useAlpha, 1f);
            if (occlusion != WireframeOcclusion.Fade)
            {
                return new[] { lines };
            }

            // Only where the depth buffer already holds something closer: the parts that geometry hides.
            Material hidden = CreateMaterial(shader, "Wireframes Hidden");
            SetState(hidden, CompareFunction.Greater, false, true);
            SetQueue(hidden, RenderQueue.Transparent);
            SetAlpha(hidden, useAlpha, FadedAlpha);
            return new[] { lines, hidden };
        }

        private static Material CreateMaterial(Shader shader, string name)
        {
            return new Material(shader) { name = name, hideFlags = HideFlags.DontSave };
        }

        private static void SetState(Material material, CompareFunction depthTest, bool depthWrite, bool blend)
        {
            material.SetFloat(DepthTestId, (float)depthTest);
            material.SetFloat(DepthWriteId, depthWrite ? 1f : 0f);
            material.SetFloat(SourceBlendId, (float)(blend ? BlendMode.SrcAlpha : BlendMode.One));
            material.SetFloat(DestinationBlendId, (float)(blend ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
        }

        private static void SetQueue(Material material, RenderQueue queue)
        {
            material.renderQueue = (int)queue;
            material.SetOverrideTag("RenderType", queue == RenderQueue.Geometry ? "Opaque" : "Transparent");
        }

        private static void SetAlpha(Material material, bool useVertexAlpha, float scale)
        {
            material.SetFloat(VertexAlphaId, useVertexAlpha ? 1f : 0f);
            material.SetFloat(AlphaScaleId, scale);
        }
    }
}
