using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>The materials a container builds for its occlusion and alpha settings. ShaderTests checks the pixels.</summary>
    public class OcclusionTests : WireframesTestBase
    {
        [Test]
        public void Default_IsOneOpaqueDepthTestedMaterial()
        {
            Material[] materials = MaterialsOf(new WireframeContainerSettings());

            Assert.That(materials, Has.Length.EqualTo(1));
            AssertState(materials[0], CompareFunction.LessEqual, true, BlendMode.One, BlendMode.Zero);
            Assert.That(materials[0].renderQueue, Is.EqualTo((int)RenderQueue.Geometry));
            Assert.That(materials[0].GetFloat("_WireframesVertexAlpha"), Is.Zero);
        }

        [Test]
        public void UseAlpha_BlendsByVertexAlphaWithoutWritingDepth()
        {
            Material[] materials = MaterialsOf(new WireframeContainerSettings { UseAlpha = true });

            AssertState(materials[0], CompareFunction.LessEqual, false, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha);
            Assert.That(materials[0].renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
            Assert.That(materials[0].GetFloat("_WireframesVertexAlpha"), Is.EqualTo(1f));
        }

        [Test]
        public void Show_DrawsOverEverythingAfterOpaqueGeometry()
        {
            Material[] materials = MaterialsOf(new WireframeContainerSettings { Occlusion = WireframeOcclusion.Show });

            Assert.That(materials, Has.Length.EqualTo(1));
            AssertState(materials[0], CompareFunction.Always, false, BlendMode.One, BlendMode.Zero);
            Assert.That(materials[0].renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
        }

        [Test]
        public void Fade_AddsASecondDimmedMaterialForHiddenParts()
        {
            Material[] materials = MaterialsOf(new WireframeContainerSettings { Occlusion = WireframeOcclusion.Fade });

            Assert.That(materials, Has.Length.EqualTo(2));
            AssertState(materials[0], CompareFunction.LessEqual, true, BlendMode.One, BlendMode.Zero);
            AssertState(materials[1], CompareFunction.Greater, false, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha);
            Assert.That(materials[1].GetFloat("_WireframesAlphaScale"), Is.LessThan(1f));
            Assert.That(materials[1].renderQueue, Is.GreaterThan(materials[0].renderQueue));
        }

        [Test]
        public void CustomMaterial_IsUsedAsIsWhateverTheOcclusion()
        {
            Material custom = Track(new Material(Shader.Find(WireframeMaterials.ShaderName)));

            Material[] materials = MaterialsOf(
                new WireframeContainerSettings { Material = custom, Occlusion = WireframeOcclusion.Fade, UseAlpha = true });

            Assert.That(materials, Has.Length.EqualTo(1));
            Assert.That(materials[0], Is.SameAs(custom));
        }

        [Test]
        public void UndefinedOcclusion_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateContainer(new WireframeContainerSettings { Occlusion = (WireframeOcclusion)7 }));
        }

        private Material[] MaterialsOf(WireframeContainerSettings settings)
        {
            return ChunkOf(CreateContainer(settings)).Renderer.sharedMaterials;
        }

        private static void AssertState(
            Material material, CompareFunction depthTest, bool depthWrite, BlendMode source, BlendMode destination)
        {
            Assert.That(material.GetFloat("_ZTest"), Is.EqualTo((float)depthTest), "Depth test");
            Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(depthWrite ? 1f : 0f), "Depth write");
            Assert.That(material.GetFloat("_SrcBlend"), Is.EqualTo((float)source), "Source blend");
            Assert.That(material.GetFloat("_DstBlend"), Is.EqualTo((float)destination), "Destination blend");
        }
    }
}
