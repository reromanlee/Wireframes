using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Bones must be scene objects: a prefab asset never moves, so following one is always a mistake.</summary>
    public class BoneValidationTests
    {
        private const string PrefabPath = "Assets/WireframesBoneValidationTest.prefab";

        private WireframeContainer _container;
        private Transform _prefabBone;
        private GameObject _sceneBone;

        [SetUp]
        public void CreateBones()
        {
            GameObject source = new("Prefab Bone");
            _prefabBone = PrefabUtility.SaveAsPrefabAsset(source, PrefabPath).transform;
            Object.DestroyImmediate(source);
            _sceneBone = new GameObject("Scene Bone");
            _container = new WireframeContainer();
        }

        [TearDown]
        public void DestroyBones()
        {
            _container.Dispose();
            Object.DestroyImmediate(_sceneBone);
            AssetDatabase.DeleteAsset(PrefabPath);
        }

        [Test]
        public void CreatingAShapeOnAPrefabAsset_ThrowsAndLeavesNothingBehind()
        {
            Assert.Throws<ArgumentException>(() => _container.CreateCircle(_prefabBone, Vector3.zero, 1f));
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => _container.CreateLine(_sceneBone.transform, _prefabBone));
            Assert.Throws<ArgumentException>(() => _container.CreatePolyline(_sceneBone.transform, _prefabBone));

            Assert.That(exception.ParamName, Is.EqualTo("boneB"));
            Assert.That(_container.Proxy.Chunks, Is.Empty);
            Assert.That(_container.Proxy.Bones.Count, Is.Zero);
        }

        [Test]
        public void SettingAPrefabAssetAsBone_ThrowsAndKeepsTheBone()
        {
            ICircle circle = _container.CreateCircle(_sceneBone.transform, Vector3.zero, 1f);
            ILine line = _container.CreateLine();

            Assert.Throws<ArgumentException>(() => circle.Bone = _prefabBone);
            Assert.Throws<ArgumentException>(() => line.BoneA = _prefabBone);

            Assert.That(circle.Bone, Is.SameAs(_sceneBone.transform));
            Assert.That(line.BoneA, Is.Null);
        }
    }
}
