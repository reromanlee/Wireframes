using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using reromanlee.Wireframes.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Containers in Edit Mode: drawn, never saved, and disposed with their scene or before scripts reload.</summary>
    public class EditModeTests
    {
        private const string ScenePath = "Assets/WireframesEditModeTest.unity";

        private readonly List<WireframeContainer> _containers = new();
        private readonly List<Object> _objects = new();

        [SetUp]
        public void OpenEmptyScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void DestroyEverything()
        {
            foreach (WireframeContainer container in _containers)
            {
                container.Dispose();
            }
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                if (_objects[i] != null)
                {
                    Object.DestroyImmediate(_objects[i]);
                }
            }
            _containers.Clear();
            _objects.Clear();
            AssetDatabase.DeleteAsset(ScenePath);
        }

        [Test]
        public void Container_IsNeverSavedAndLeavesTheSceneUnchanged()
        {
            Scene scene = SceneManager.GetActiveScene();

            WireframeContainer container = CreateContainer();
            container.CreateSphere(Vector3.zero, 1f);
            container.Proxy.Flush();

            Assert.That(scene.isDirty, Is.False);
            Assert.That(container.Proxy.IsEditMode, Is.True);
            Assert.That(container.Proxy.gameObject.hideFlags & HideFlags.DontSave, Is.EqualTo(HideFlags.DontSave));
            foreach (MeshChunk chunk in container.Proxy.Chunks)
            {
                Assert.That(chunk.Renderer.gameObject.hideFlags & HideFlags.DontSave, Is.EqualTo(HideFlags.DontSave));
            }
        }

        [Test]
        public void Container_DrawsWhenACameraRendersAndItsBoneTextureGrows()
        {
            WireframeContainer container = CreateContainer();
            container.CreateLine(new Vector3(-2f, 0f, 0f), new Vector3(2f, 0f, 0f)).SetColor(Color.red);
            Assert.That(CenterProbe.RedAtCenter(), Is.GreaterThan(0.5f));

            // More bones than one texture row holds, so the texture is replaced during the next render.
            for (int i = 0; i < BoneTexture.BonesPerRow + 1; i++)
            {
                Transform bone = Track(new GameObject("Bone")).transform;
                bone.position = new Vector3(50f, i, 0f);
                container.CreateLine(bone, bone);
            }

            Assert.That(CenterProbe.RedAtCenter(), Is.GreaterThan(0.5f));
            Assert.That(container.Proxy.BoneTexture.Texture.height, Is.EqualTo(2));
        }

        [Test]
        public void Container_IsDisposedWhenItsSceneCloses()
        {
            WireframeContainer container = CreateContainer();
            ILine line = container.CreateLine();
            GameObject proxyObject = container.Proxy.gameObject;

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Assert.That(container.IsDisposed, Is.True);
            Assert.That(line.IsDisposed, Is.True);
            Assert.That(proxyObject == null, Is.True);
        }

        [Test]
        public void PersistentContainer_SurvivesItsSceneClosingAndKeepsDrawing()
        {
            WireframeContainer container = CreateContainer(new WireframeContainerSettings { PersistAcrossScenes = true });
            container.CreateLine(new Vector3(-2f, 0f, 0f), new Vector3(2f, 0f, 0f)).SetColor(Color.red);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Assert.That(container.IsDisposed, Is.False);
            Assert.That(CenterProbe.RedAtCenter(), Is.GreaterThan(0.5f));
        }

        [Test]
        public void PersistentContainer_MovesOutOfAnAdditiveSceneThatCloses()
        {
            Scene first = SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(first, ScenePath);
            Scene additive = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(additive);
            WireframeContainer persistent = CreateContainer(new WireframeContainerSettings { PersistAcrossScenes = true });
            WireframeContainer plain = CreateContainer();
            SceneManager.SetActiveScene(first);

            EditorSceneManager.CloseScene(additive, true);

            Assert.That(persistent.IsDisposed, Is.False);
            Assert.That(persistent.Proxy.gameObject.scene, Is.EqualTo(first));
            Assert.That(plain.IsDisposed, Is.True);
        }

        [Test]
        public void ScriptReload_DisposesEveryContainerRightAway()
        {
            WireframeContainer plain = CreateContainer();
            WireframeContainer persistent = CreateContainer(new WireframeContainerSettings { PersistAcrossScenes = true });
            GameObject proxyObject = persistent.Proxy.gameObject;

            EditModeLifecycle.DisposeEveryContainer();

            Assert.That(plain.IsDisposed, Is.True);
            Assert.That(persistent.IsDisposed, Is.True);
            Assert.That(proxyObject == null, Is.True);
            Assert.That(MeshProxy.LiveProxies, Is.Empty);
        }

        [Test]
        public void IdleChunks_AreReleasedFromTheEditorLoop()
        {
            WireframeContainer container = CreateContainer();
            List<ILine> lines = new();
            for (int i = 0; i < MeshChunk.MaxVertexCount / 2 + 10; i++)
            {
                lines.Add(container.CreateLine());
            }
            MeshChunk second = container.Proxy.Chunks[1];
            foreach (ILine line in lines)
            {
                if (((Shape)line).Chunk == second)
                {
                    line.Dispose();
                }
            }

            container.Proxy.ReleaseIdleChunks(1000f);
            container.Proxy.ReleaseIdleChunks(1000f + ChunkAllocator.ReleaseDelay);
            EditModeLifecycle.ReleaseIdleChunks();

            Assert.That(container.Proxy.Chunks, Has.Count.EqualTo(1));
            Assert.That(second.Renderer == null, Is.True, "Edit Mode destroys a released chunk right away.");
        }

        [UnityTest]
        public IEnumerator EnteringAndLeavingPlayMode_KeepsTheContainer()
        {
            // Without a domain reload; with one, the reload disposes every container before Play Mode starts.
            bool optionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            EnterPlayModeOptions options = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            WireframeContainer container = CreateContainer();
            ILine line = container.CreateLine();
            try
            {
                yield return new EnterPlayMode();
                Assert.That(container.IsDisposed, Is.False);
                line.ColorA = Color.red;

                yield return new ExitPlayMode();
                Assert.That(container.IsDisposed, Is.False);
                Assert.That(line.ColorA, Is.EqualTo(Color.red));
            }
            finally
            {
                EditorSettings.enterPlayModeOptionsEnabled = optionsEnabled;
                EditorSettings.enterPlayModeOptions = options;
            }
        }

        private WireframeContainer CreateContainer(WireframeContainerSettings settings = null)
        {
            WireframeContainer container = new(settings);
            _containers.Add(container);
            return container;
        }

        private T Track<T>(T target) where T : Object
        {
            _objects.Add(target);
            return target;
        }
    }
}
