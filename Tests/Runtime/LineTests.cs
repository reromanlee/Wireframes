using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace reromanlee.Wireframes.Tests
{
    public class LineTests : WireframesTestBase
    {
        [Test]
        public void CreateLine_StartsWhiteInWorldSpace()
        {
            ILine line = CreateContainer().CreateLine(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f));

            Assert.That(line.LocalPositionA, Is.EqualTo(new Vector3(1f, 2f, 3f)));
            Assert.That(line.WorldPositionB, Is.EqualTo(new Vector3(4f, 5f, 6f)));
            Assert.That(line.ColorA, Is.EqualTo(Color.white));
            Assert.That(line.ColorB, Is.EqualTo(Color.white));
            Assert.That(line.BoneA, Is.Null);
            Assert.That(line.BoneB, Is.Null);
        }

        [Test]
        public void WorldPosition_IsConvertedThroughBone()
        {
            Transform bone = CreateBone(new Vector3(10f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), 2f);
            ILine line = CreateContainer().CreateLine();
            line.BoneA = bone;

            line.WorldPositionA = new Vector3(3f, 4f, 5f);

            AssertApproximately(bone.InverseTransformPoint(new Vector3(3f, 4f, 5f)), line.LocalPositionA);
            AssertApproximately(new Vector3(3f, 4f, 5f), line.WorldPositionA);
        }

        [Test]
        public void ChangingBone_KeepsWorldPosition()
        {
            Transform bone = CreateBone(new Vector3(0f, 5f, 0f), Quaternion.Euler(30f, 45f, 0f), 0.5f);
            ILine line = CreateContainer().CreateLine(new Vector3(1f, 2f, 3f), Vector3.zero);

            line.BoneA = bone;
            AssertApproximately(new Vector3(1f, 2f, 3f), line.WorldPositionA);

            bone.position += new Vector3(0f, 0f, 10f);
            AssertApproximately(new Vector3(1f, 2f, 13f), line.WorldPositionA);

            line.BoneA = null;
            AssertApproximately(new Vector3(1f, 2f, 13f), line.WorldPositionA);
            AssertApproximately(new Vector3(1f, 2f, 13f), line.LocalPositionA);
        }

        [Test]
        public void Skinning_MovesEndpointsWithTheirBones()
        {
            LineContainer container = CreateContainer();
            Transform boneA = CreateBone(new Vector3(-3f, 0f, 0f), Quaternion.identity);
            Transform boneB = CreateBone(new Vector3(3f, 0f, 0f), Quaternion.identity);
            ILine line = container.CreateLine();
            line.BoneA = boneA;
            line.LocalPositionA = Vector3.zero;
            line.BoneB = boneB;
            line.LocalPositionB = Vector3.up;
            int start = ((Line)line).VertexStart;

            Vector3[] baked = FlushAndBake(container);
            AssertApproximately(boneA.position, baked[start]);
            AssertApproximately(boneB.TransformPoint(Vector3.up), baked[start + 1]);

            // No edits: moving the bones alone moves the endpoints.
            boneA.position = new Vector3(0f, 7f, 0f);
            boneB.rotation = Quaternion.Euler(0f, 0f, 90f);
            baked = FlushAndBake(container);
            AssertApproximately(new Vector3(0f, 7f, 0f), baked[start]);
            AssertApproximately(boneB.TransformPoint(Vector3.up), baked[start + 1]);
        }

        [Test]
        public void Colors_AreUploadedPerEndpoint()
        {
            LineContainer container = CreateContainer();
            ILine line = container.CreateLine();
            line.ColorA = Color.red;
            line.ColorB = new Color(0f, 0.5f, 1f, 1f);
            int start = ((Line)line).VertexStart;

            container.Proxy.Flush();
            Color32[] colors = ChunkOf(container).Mesh.colors32;

            Assert.That(colors[start], Is.EqualTo((Color32)Color.red));
            Assert.That(colors[start + 1], Is.EqualTo((Color32)new Color(0f, 0.5f, 1f, 1f)));

            line.SetColor(Color.green);
            container.Proxy.Flush();
            colors = ChunkOf(container).Mesh.colors32;
            Assert.That(colors[start], Is.EqualTo((Color32)Color.green));
            Assert.That(colors[start + 1], Is.EqualTo((Color32)Color.green));
        }

        [Test]
        public void SharedBone_IsRegisteredOnceAndReleasedWithLastUse()
        {
            LineContainer container = CreateContainer();
            BoneRegistry bones = ChunkOf(container).Bones;
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            ILine first = container.CreateLine();
            ILine second = container.CreateLine();

            first.BoneA = bone;
            first.BoneA = bone;
            first.BoneB = bone;
            second.BoneA = bone;
            Assert.That(bones.Count, Is.EqualTo(1));

            first.BoneA = null;
            first.BoneB = null;
            Assert.That(bones.Count, Is.EqualTo(1));

            second.Dispose();
            Assert.That(bones.Count, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ReleasingLastUse_RemovesHiddenWatcher()
        {
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            ILine line = CreateContainer().CreateLine();

            line.BoneA = bone;
            Assert.That(bone.GetComponent<BoneWatcher>() != null, Is.True);

            line.BoneA = null;
            yield return null;

            Assert.That(bone.GetComponent<BoneWatcher>() == null, Is.True);
        }

        [UnityTest]
        public IEnumerator DestroyedBone_LeavesEndpointInPlace()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(2f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
            ILine line = container.CreateLine();
            line.BoneA = bone;
            line.LocalPositionA = new Vector3(0f, 0f, 1f);
            bone.position = new Vector3(5f, 1f, 0f);
            Vector3 expected = bone.TransformPoint(new Vector3(0f, 0f, 1f));

            Object.Destroy(bone.gameObject);
            yield return null;

            Assert.That(line.BoneA, Is.Null);
            AssertApproximately(expected, line.WorldPositionA);
            Assert.That(ChunkOf(container).Bones.Count, Is.Zero);
            Vector3[] baked = FlushAndBake(container);
            AssertApproximately(expected, baked[((Line)line).VertexStart]);
        }

        [UnityTest]
        public IEnumerator DestroyedParentOfBone_LeavesEndpointInPlace()
        {
            Transform parent = CreateBone(new Vector3(0f, 3f, 0f), Quaternion.Euler(0f, 45f, 0f));
            Transform bone = CreateBone(new Vector3(1f, 0f, 0f), Quaternion.identity, 1f, parent);
            ILine line = CreateContainer().CreateLine();
            line.BoneB = bone;
            line.LocalPositionB = Vector3.forward;
            Vector3 expected = bone.TransformPoint(Vector3.forward);

            Object.Destroy(parent.gameObject);
            yield return null;

            Assert.That(line.BoneB, Is.Null);
            AssertApproximately(expected, line.WorldPositionB);
        }

        [UnityTest]
        public IEnumerator ReattachingInTheSameFrame_KeepsWatchingTheBone()
        {
            // Releasing schedules the watcher's destruction for the end of the frame, so a fresh one has to take over.
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            ILine line = CreateContainer().CreateLine();
            line.BoneA = bone;
            line.BoneA = null;
            line.BoneA = bone;

            yield return null;
            Assert.That(line.BoneA, Is.SameAs(bone));

            Object.Destroy(bone.gameObject);
            yield return null;
            Assert.That(line.BoneA, Is.Null);
        }

        [UnityTest]
        public IEnumerator BoneDestroyedBeforeItWasActive_FallsBackToLocalOffset()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(9f, 9f, 9f), Quaternion.identity);
            bone.gameObject.SetActive(false);
            ILine line = container.CreateLine();
            line.BoneA = bone;
            line.LocalPositionA = new Vector3(1f, 0f, 0f);
            LogAssert.Expect(LogType.Warning, new Regex("was destroyed before its GameObject was ever active"));

            Object.Destroy(bone.gameObject);
            yield return null;
            container.Proxy.Flush();

            Assert.That(line.BoneA, Is.Null);
            AssertApproximately(new Vector3(1f, 0f, 0f), line.WorldPositionA);
            Assert.That(ChunkOf(container).Bones.Count, Is.Zero);
        }
    }
}
