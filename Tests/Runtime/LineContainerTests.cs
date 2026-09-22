using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace reromanlee.Wireframes.Tests
{
    public class LineContainerTests : WireframesTestBase
    {
        private const string ShaderName = "reromanlee/Wireframes/VertexColors";

        [Test]
        public void Constructor_CreatesProxyInActiveSceneWithPackageShader()
        {
            LineContainer container = CreateContainer();

            Assert.That(container.IsDisposed, Is.False);
            Assert.That(container.Proxy.gameObject.scene, Is.EqualTo(SceneManager.GetActiveScene()));
            Assert.That(ChunkOf(container).Renderer.sharedMaterial.shader.name, Is.EqualTo(ShaderName));
        }

        [Test]
        public void Dispose_DisposesEveryShape()
        {
            LineContainer container = CreateContainer();
            ILine line = container.CreateLine();
            IBox box = container.CreateBox();

            container.Dispose();

            Assert.That(container.IsDisposed, Is.True);
            Assert.That(line.IsDisposed, Is.True);
            Assert.That(box.IsDisposed, Is.True);
            Assert.Throws<ObjectDisposedException>(() => line.ColorA = Color.red);
            Assert.Throws<ObjectDisposedException>(() => box.SetColor(Color.red));
            Assert.Throws<ObjectDisposedException>(() => container.CreateLine());
            Assert.DoesNotThrow(() => line.Dispose());
            Assert.DoesNotThrow(() => container.Dispose());
        }

        [UnityTest]
        public IEnumerator DestroyingProxyObject_DisposesContainer()
        {
            LineContainer container = CreateContainer();
            ILine line = container.CreateLine();

            Object.Destroy(container.Proxy.gameObject);
            yield return null;

            Assert.That(container.IsDisposed, Is.True);
            Assert.That(line.IsDisposed, Is.True);
        }

        [UnityTest]
        public IEnumerator UnloadingScene_DisposesContainer()
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = SceneManager.CreateScene("WireframesUnloadTest");
            SceneManager.SetActiveScene(scene);
            LineContainer container = CreateContainer();
            ILine line = container.CreateLine();
            SceneManager.SetActiveScene(previous);

            yield return SceneManager.UnloadSceneAsync(scene);

            Assert.That(container.IsDisposed, Is.True);
            Assert.That(line.IsDisposed, Is.True);
        }

        [UnityTest]
        public IEnumerator Dispose_LeavesCallerMaterialAlive()
        {
            Material material = Track(new Material(Shader.Find(ShaderName)));
            LineContainer container = CreateContainer(material);

            container.Dispose();
            yield return null;

            Assert.That(material != null, Is.True);
        }

        [UnityTest]
        public IEnumerator Rendering_LogsNoErrorsWhileBuffersGrow()
        {
            // The test framework fails a test on any logged error, e.g. a bone/bind pose mismatch while rendering.
            // The shapes added here outgrow 16-bit indices, so the index format switches while a camera renders.
            Camera camera = Track(new GameObject("Camera")).AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -20f);
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);

            for (int frame = 0; frame < 6; frame++)
            {
                for (int i = 0; i < 1000 * (frame + 1); i++)
                {
                    ILine line = container.CreateLine(Random.insideUnitSphere * 5f, Random.insideUnitSphere * 5f);
                    if (i % 3 == 0)
                    {
                        line.BoneA = bone;
                    }
                    if (i % 7 == 0)
                    {
                        container.CreateBox(Random.insideUnitSphere, Random.insideUnitSphere).Bone = bone;
                    }
                }
                bone.position += Vector3.right;
                yield return null;
            }

            Assert.That(ChunkOf(container).ShapeCount, Is.GreaterThan(21000));
            Assert.That(ChunkOf(container).Mesh.indexFormat, Is.EqualTo(UnityEngine.Rendering.IndexFormat.UInt32));
        }
    }
}
