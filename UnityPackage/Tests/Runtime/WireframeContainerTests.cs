using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace reromanlee.Wireframes.Tests
{
    public class WireframeContainerTests : WireframesTestBase
    {
        private const string ShaderName = "reromanlee/Wireframes/Unlit";

        [Test]
        public void Constructor_CreatesProxyInActiveSceneWithPackageShader()
        {
            WireframeContainer container = CreateContainer();

            Assert.That(container.IsDisposed, Is.False);
            Assert.That(container.Proxy.gameObject.scene, Is.EqualTo(SceneManager.GetActiveScene()));
            Assert.That(container.Proxy.Materials[0].shader.name, Is.EqualTo(ShaderName));
        }

        [Test]
        public void Constructor_CreatesNoChunkUntilTheFirstShape()
        {
            WireframeContainer container = CreateContainer();
            Assert.That(container.Proxy.Chunks, Is.Empty);

            ILine line = container.CreateLine();

            Assert.That(container.Proxy.Chunks, Has.Count.EqualTo(1));
            Assert.That(ChunkOf(line).Renderer.sharedMaterials, Is.EqualTo(container.Proxy.Materials));
        }

        [Test]
        public void Dispose_DisposesEveryShape()
        {
            WireframeContainer container = CreateContainer();
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
            WireframeContainer container = CreateContainer();
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
            WireframeContainer container = CreateContainer();
            ILine line = container.CreateLine();
            SceneManager.SetActiveScene(previous);

            yield return SceneManager.UnloadSceneAsync(scene);

            Assert.That(container.IsDisposed, Is.True);
            Assert.That(line.IsDisposed, Is.True);
        }

        [UnityTest]
        public IEnumerator PersistAcrossScenes_KeepsContainerWhenItsSceneUnloads()
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = SceneManager.CreateScene("WireframesPersistTest");
            SceneManager.SetActiveScene(scene);
            WireframeContainer container = CreateContainer(new WireframeContainerSettings { PersistAcrossScenes = true });
            ILine line = container.CreateLine();
            SceneManager.SetActiveScene(previous);

            yield return SceneManager.UnloadSceneAsync(scene);

            Assert.That(container.IsDisposed, Is.False);
            Assert.That(line.IsDisposed, Is.False);
            Assert.That(container.Proxy.gameObject.scene.name, Is.EqualTo("DontDestroyOnLoad"));
        }

        [UnityTest]
        public IEnumerator Dispose_LeavesCallerMaterialAlive()
        {
            Material material = Track(new Material(Shader.Find(ShaderName)));
            WireframeContainer container = CreateContainer(new WireframeContainerSettings { Material = material });

            Assert.That(ChunkOf(container.CreateLine()).Renderer.sharedMaterial, Is.SameAs(material));
            container.Dispose();
            yield return null;

            Assert.That(material != null, Is.True);
        }

        [Test]
        public void Settings_NameAndLayer_ApplyToTheContainersGameObjects()
        {
            WireframeContainer container = CreateContainer(new WireframeContainerSettings { Name = "Debug Lines", Layer = 5 });

            Assert.That(container.Proxy.gameObject.name, Is.EqualTo("Debug Lines"));
            Assert.That(container.Proxy.gameObject.layer, Is.EqualTo(5));
            Assert.That(ChunkOf(container.CreateLine()).Renderer.gameObject.layer, Is.EqualTo(5));
        }

        [Test]
        public void Settings_EmptyName_UsesTheDefaultName()
        {
            WireframeContainer container = CreateContainer(new WireframeContainerSettings { Name = "" });

            Assert.That(container.Proxy.gameObject.name, Is.EqualTo("Wireframes"));
        }

        [Test]
        public void Settings_OutOfRange_ThrowBeforeAnythingIsCreated()
        {
            int objectsBefore = Resources.FindObjectsOfTypeAll<MeshProxy>().Length;

            Assert.Throws<ArgumentOutOfRangeException>(() => CreateContainer(new WireframeContainerSettings { Layer = 32 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateContainer(new WireframeContainerSettings { Layer = -1 }));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateContainer(new WireframeContainerSettings { VertexCapacity = -1 }));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateContainer(new WireframeContainerSettings { EdgeCapacity = -1 }));
            Assert.That(Resources.FindObjectsOfTypeAll<MeshProxy>(), Has.Length.EqualTo(objectsBefore));
        }

        [Test]
        public void Settings_Capacity_IsReservedSoAKnownLoadNeverGrowsTheBuffers()
        {
            WireframeContainer container = CreateContainer(
                new WireframeContainerSettings { VertexCapacity = 10000, EdgeCapacity = 5000 });
            MeshChunk chunk = ChunkOf(container);
            int vertexCapacity = chunk.VertexCapacity;
            int edgeCapacity = chunk.EdgeCapacity;
            int meshVertexCount = chunk.Mesh.vertexCount;

            for (int i = 0; i < 5000; i++)
            {
                container.CreateLine(new Vector3(i, 0f, 0f), new Vector3(i, 1f, 0f));
            }
            container.Proxy.Flush();

            Assert.That(vertexCapacity, Is.GreaterThanOrEqualTo(10000));
            Assert.That(edgeCapacity, Is.GreaterThanOrEqualTo(5000));
            Assert.That(chunk.VertexCapacity, Is.EqualTo(vertexCapacity));
            Assert.That(chunk.EdgeCapacity, Is.EqualTo(edgeCapacity));
            Assert.That(chunk.Mesh.vertexCount, Is.EqualTo(meshVertexCount));
        }

        [Test]
        public void EveryFactoryMethod_RejectsANullContainer()
        {
            foreach (MethodInfo method in FactoryMethods())
            {
                TargetInvocationException exception =
                    Assert.Throws<TargetInvocationException>(() => method.Invoke(null, Arguments(method, null)));
                Assert.That(exception.InnerException, Is.TypeOf<ArgumentNullException>(), Describe(method));
                Assert.That(((ArgumentNullException)exception.InnerException).ParamName, Is.EqualTo("container"));
            }
        }

        [Test]
        public void EveryFactoryMethod_RejectsADisposedContainer()
        {
            WireframeContainer container = CreateContainer();
            container.Dispose();

            foreach (MethodInfo method in FactoryMethods())
            {
                TargetInvocationException exception =
                    Assert.Throws<TargetInvocationException>(() => method.Invoke(null, Arguments(method, container)));
                Assert.That(exception.InnerException, Is.TypeOf<ObjectDisposedException>(), Describe(method));
            }
        }

        /// <summary>Every public extension method on <see cref="WireframeContainer"/> in the package.</summary>
        private static List<MethodInfo> FactoryMethods()
        {
            List<MethodInfo> methods = new();
            foreach (Type type in typeof(WireframeContainer).Assembly.GetExportedTypes())
            {
                if (!type.IsAbstract || !type.IsSealed || !type.Name.EndsWith("Factory", StringComparison.Ordinal))
                {
                    continue;
                }
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length > 0 && parameters[0].ParameterType == typeof(WireframeContainer))
                    {
                        methods.Add(method);
                    }
                }
            }
            Assert.That(methods, Has.Count.GreaterThan(70), "The factory classes weren't found.");
            return methods;
        }

        /// <summary>The container, then a default value for every other parameter; the container is checked first.</summary>
        private static object[] Arguments(MethodInfo method, WireframeContainer container)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] arguments = new object[parameters.Length];
            arguments[0] = container;
            for (int i = 1; i < parameters.Length; i++)
            {
                Type type = parameters[i].ParameterType;
                arguments[i] = type.IsValueType ? Activator.CreateInstance(type) : null;
            }
            return arguments;
        }

        private static string Describe(MethodInfo method)
        {
            return $"{method.DeclaringType?.Name}.{method.Name}({method.GetParameters().Length} parameters)";
        }

        [UnityTest]
        public IEnumerator Rendering_LogsNoErrorsWhileBuffersGrow()
        {
            // The test framework fails a test on any logged error while rendering. The shapes added here grow the first
            // chunk to its limit and start a second one between renders. Batch mode draws no screen cameras, so this
            // one renders into a texture, which is also what applies the edits.
            RenderTexture target = Track(new RenderTexture(64, 64, 24));
            Camera camera = Track(new GameObject("Camera")).AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -20f);
            camera.targetTexture = target;
            WireframeContainer container = CreateContainer();
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
                camera.Render();
                yield return null;
            }

            Assert.That(ShapeCountOf(container), Is.GreaterThan(21000));
            Assert.That(container.Proxy.Chunks, Has.Count.GreaterThanOrEqualTo(2));
            foreach (MeshChunk chunk in container.Proxy.Chunks)
            {
                Assert.That(chunk.Mesh.indexFormat, Is.EqualTo(UnityEngine.Rendering.IndexFormat.UInt16));
            }
        }
    }
}
