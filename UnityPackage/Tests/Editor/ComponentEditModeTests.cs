using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using reromanlee.Wireframes.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>
    /// Shape components in Edit Mode: drawn without entering Play Mode, kept in step with the Inspector, undo and layer
    /// changes, drawn as gizmos when asked, and drawn in Prefab Mode's own scene.
    /// </summary>
    public class ComponentEditModeTests
    {
        private const string PrefabPath = "Assets/WireframesComponentTest.prefab";

        private readonly List<Object> _objects = new();

        [SetUp]
        public void OpenEmptyScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void DestroyEverything()
        {
            GizmoCameras.Filter = GizmoCameras.DrawsGizmos;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                StageUtility.GoToMainStage();
            }
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                if (_objects[i] != null)
                {
                    Object.DestroyImmediate(_objects[i]);
                }
            }
            _objects.Clear();
            AssetDatabase.DeleteAsset(PrefabPath);
            ShapeComponents.TakeRepaintRequest();
        }

        [Test]
        public void Component_DrawsInEditModeWithAContainerThatIsNeverSaved()
        {
            WireframeLine line = AddRedLineAcrossTheCenter();

            Assert.That(CenterProbe.RedAtCenter(), Is.GreaterThan(0.5f));
            GameObject proxyObject = line.SharedContainer.Container.Proxy.gameObject;
            HideFlags hidden = HideFlags.DontSave | HideFlags.HideInHierarchy;
            Assert.That(proxyObject.hideFlags & hidden, Is.EqualTo(hidden));
            Assert.That(proxyObject.transform.GetChild(0).gameObject.hideFlags & hidden, Is.EqualTo(hidden));
        }

        [Test]
        public void InspectorEdit_ShowsUpRightAway()
        {
            WireframeSphere sphere = Add<WireframeSphere>();
            SerializedObject serialized = new(sphere);

            serialized.FindProperty("_radius").floatValue = 2f;
            serialized.ApplyModifiedProperties();

            Assert.That(((Sphere)sphere.Shape).Radius, Is.EqualTo(2f));
            Assert.That(sphere.IsDeferred, Is.False);
        }

        [Test]
        public void InspectorCountEdit_IsAppliedOnTheEditorsNextUpdate()
        {
            WireframeSphere sphere = Add<WireframeSphere>();
            SerializedObject serialized = new(sphere);

            serialized.FindProperty("_segmentCount").intValue = 8;
            serialized.ApplyModifiedProperties();

            // Creating the shape again may create objects, which OnValidate forbids.
            Assert.That(sphere.Shape.VertexCount, Is.EqualTo(96));
            Assert.That(sphere.IsDeferred, Is.True);

            ComponentRefresh.Update();

            Assert.That(sphere.Shape.VertexCount, Is.EqualTo(24));
            Assert.That(sphere.IsDeferred, Is.False);
        }

        [Test]
        public void InspectorCountEdit_IsCorrectedToTheNearestValidCount()
        {
            WireframeCylinder cylinder = Add<WireframeCylinder>();
            SerializedObject serialized = new(cylinder);

            serialized.FindProperty("_segmentCount").intValue = 18;
            serialized.ApplyModifiedProperties();
            ComponentRefresh.Update();

            Assert.That(cylinder.SegmentCount, Is.EqualTo(20));
            Assert.That(((Cylinder)cylinder.Shape).SegmentCount, Is.EqualTo(20));
        }

        [Test]
        public void Undo_BringsTheShapeBack()
        {
            WireframeSphere sphere = Add<WireframeSphere>();
            Undo.RecordObject(sphere, "Change Radius");
            sphere.Radius = 3f;
            Undo.FlushUndoRecordObjects();

            Undo.PerformUndo();

            Assert.That(sphere.Radius, Is.EqualTo(0.5f));
            Assert.That(((Sphere)sphere.Shape).Radius, Is.EqualTo(0.5f));
        }

        [Test]
        public void EditInEditMode_AsksForARepaint()
        {
            WireframeSphere sphere = Add<WireframeSphere>();
            ShapeComponents.TakeRepaintRequest();

            sphere.Radius = 2f;

            Assert.That(ShapeComponents.TakeRepaintRequest(), Is.True);
            Assert.That(ShapeComponents.TakeRepaintRequest(), Is.False);
        }

        [Test]
        public void GizmoShape_IsLeftOutOfCamerasThatDrawNoGizmos()
        {
            WireframeLine line = AddRedLineAcrossTheCenter();
            line.DrawAsGizmo = true;

            // A camera rendered on its own, outside any Scene or Game view, draws no gizmos.
            Assert.That(CenterProbe.RedAtCenter(), Is.LessThan(0.1f));
            Assert.That(line.SharedContainer.Container.Proxy.Chunks[0].Renderer.forceRenderingOff, Is.True);

            GizmoCameras.Filter = camera => true;

            Assert.That(CenterProbe.RedAtCenter(), Is.GreaterThan(0.5f));
            Assert.That(line.SharedContainer.Container.Proxy.Chunks[0].Renderer.forceRenderingOff, Is.True);
        }

        [Test]
        public void LayerChange_MovesTheShapeToAContainerOnThatLayer()
        {
            WireframeBox box = Add<WireframeBox>();
            WireframeContainer before = box.SharedContainer.Container;

            box.gameObject.layer = 5;
            ShapeComponents.RefreshLayers();

            Assert.That(box.SharedContainer.Key.Layer, Is.EqualTo(5));
            Assert.That(box.SharedContainer.Container.Proxy.Chunks[0].Renderer.gameObject.layer, Is.EqualTo(5));
            Assert.That(before.IsDisposed, Is.True);
        }

        [Test]
        public void ScriptReload_LetsComponentsCreateTheirShapesAgain()
        {
            WireframeSphere sphere = Add<WireframeSphere>();
            WireframeContainer before = sphere.SharedContainer.Container;

            EditModeLifecycle.DisposeEveryContainer();
            Assert.That(sphere.Shape.IsDisposed, Is.True);
            sphere.Radius = 1f;

            Assert.That(sphere.Shape.IsDisposed, Is.False);
            Assert.That(sphere.SharedContainer.Container, Is.Not.SameAs(before));
            Assert.That(((Sphere)sphere.Shape).Radius, Is.EqualTo(1f));
        }

        [Test]
        public void DestroyingTheLastComponent_DisposesItsContainerRightAway()
        {
            WireframeSphere sphere = Add<WireframeSphere>();
            WireframeContainer container = sphere.SharedContainer.Container;
            GameObject proxyObject = container.Proxy.gameObject;

            Object.DestroyImmediate(sphere.gameObject);

            Assert.That(container.IsDisposed, Is.True);
            Assert.That(proxyObject == null, Is.True);
        }

        [Test]
        public void ClosingTheScene_DisposesTheContainerOfItsComponents()
        {
            WireframeSphere sphere = Add<WireframeSphere>();
            WireframeContainer container = sphere.SharedContainer.Container;

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Assert.That(container.IsDisposed, Is.True);
        }

        [Test]
        public void BoneOutsideAScene_IsReportedOnceAndTheGameObjectFollowedInstead()
        {
            GameObject source = Track(new GameObject("Bone Asset"));
            GameObject asset = PrefabUtility.SaveAsPrefabAsset(source, PrefabPath);
            WireframeLine line = Add<WireframeLine>();

            LogAssert.Expect(LogType.Warning, new Regex("isn't in a scene"));
            line.BoneB = asset.transform;
            line.PositionB = Vector3.up;

            Line shape = (Line)line.Shape;
            Assert.That(shape.BoneB, Is.SameAs(line.transform));
            Assert.That(shape.LocalPositionB, Is.EqualTo(Vector3.up));
        }

        [Test]
        public void PrefabMode_DrawsComponentsInTheStagesOwnScene()
        {
            GameObject source = Track(new GameObject("Prefab Source"));
            source.AddComponent<WireframeSphere>();
            PrefabUtility.SaveAsPrefabAsset(source, PrefabPath);
            Object.DestroyImmediate(source);

            PrefabStage stage = PrefabStageUtility.OpenPrefab(PrefabPath);
            WireframeSphere sphere = stage.prefabContentsRoot.GetComponent<WireframeSphere>();
            WireframeContainer container = sphere.SharedContainer.Container;

            Assert.That(sphere.SharedContainer.Key.Stage, Is.EqualTo(stage.scene));
            Assert.That(container.Proxy.gameObject.scene, Is.EqualTo(stage.scene));

            StageUtility.GoToMainStage();

            Assert.That(container.IsDisposed, Is.True);
        }

        private T Add<T>() where T : WireframeShape
        {
            return Track(new GameObject(typeof(T).Name)).AddComponent<T>();
        }

        /// <summary>A red line from (-2, 0, 0) to (2, 0, 0), across the center that <see cref="CenterProbe"/> reads.</summary>
        private WireframeLine AddRedLineAcrossTheCenter()
        {
            WireframeLine line = Add<WireframeLine>();
            line.Color = Color.red;
            line.PositionA = new Vector3(-2f, 0f, 0f);
            line.PositionB = new Vector3(2f, 0f, 0f);
            return line;
        }

        private T Track<T>(T target) where T : Object
        {
            _objects.Add(target);
            return target;
        }
    }
}
