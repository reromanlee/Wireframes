using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace reromanlee.Wireframes.Tests
{
    public class BoxTests : WireframesTestBase
    {
        [Test]
        public void Box_IsEightCornersJoinedByTwelveEdges()
        {
            LineContainer container = CreateContainer();
            IBox box = container.CreateBox(new Vector3(-1f, -2f, -3f), new Vector3(1f, 2f, 3f));
            int start = ((Box)box).VertexStart;

            Vector3[] baked = FlushAndBake(container);
            int[] indices = ChunkOf(container).Mesh.GetIndices(0);

            HashSet<Vector3> corners = new();
            for (int i = 0; i < 8; i++)
            {
                corners.Add(baked[start + i]);
            }
            Assert.That(corners, Has.Count.EqualTo(8));
            foreach (Vector3 corner in corners)
            {
                Assert.That(Mathf.Abs(corner.x), Is.EqualTo(1f));
                Assert.That(Mathf.Abs(corner.y), Is.EqualTo(2f));
                Assert.That(Mathf.Abs(corner.z), Is.EqualTo(3f));
            }

            Assert.That(indices, Has.Length.EqualTo(24));
            HashSet<(int, int)> edges = new();
            for (int i = 0; i < indices.Length; i += 2)
            {
                Vector3 a = baked[indices[i]];
                Vector3 b = baked[indices[i + 1]];
                int changedAxes = (a.x != b.x ? 1 : 0) + (a.y != b.y ? 1 : 0) + (a.z != b.z ? 1 : 0);
                Assert.That(changedAxes, Is.EqualTo(1), "Every box edge runs along exactly one axis.");
                edges.Add((Mathf.Min(indices[i], indices[i + 1]), Mathf.Max(indices[i], indices[i + 1])));
            }
            Assert.That(edges, Has.Count.EqualTo(12));
        }

        [Test]
        public void ChangingBone_KeepsCornersAndAlignsBoxToBone()
        {
            Transform bone = CreateBone(new Vector3(4f, 0f, 0f), Quaternion.Euler(0f, 30f, 0f), 2f);
            IBox box = CreateContainer().CreateBox(new Vector3(-1f, -1f, -1f), new Vector3(1f, 1f, 1f));

            box.Bone = bone;

            AssertApproximately(new Vector3(-1f, -1f, -1f), box.WorldCornerA);
            AssertApproximately(new Vector3(1f, 1f, 1f), box.WorldCornerB);
            AssertApproximately(bone.InverseTransformPoint(new Vector3(-1f, -1f, -1f)), box.LocalCornerA);
        }

        [Test]
        public void Box_FollowsItsBone()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            IBox box = container.CreateBox();
            box.Bone = bone;
            box.LocalCornerA = new Vector3(-0.5f, -0.5f, -0.5f);
            box.LocalCornerB = new Vector3(0.5f, 0.5f, 0.5f);
            int start = ((Box)box).VertexStart;

            bone.SetPositionAndRotation(new Vector3(0f, 10f, 0f), Quaternion.Euler(0f, 0f, 90f));
            Vector3[] baked = FlushAndBake(container);

            AssertApproximately(bone.TransformPoint(new Vector3(-0.5f, -0.5f, -0.5f)), baked[start]);
            AssertApproximately(bone.TransformPoint(new Vector3(0.5f, 0.5f, 0.5f)), baked[start + 7]);
        }

        [Test]
        public void SetColor_ColorsEveryCorner()
        {
            LineContainer container = CreateContainer();
            IBox box = container.CreateBox();
            int start = ((Box)box).VertexStart;

            box.SetColor(Color.magenta);
            container.Proxy.Flush();

            Color32[] colors = ChunkOf(container).Mesh.colors32;
            for (int i = 0; i < 8; i++)
            {
                Assert.That(colors[start + i], Is.EqualTo((Color32)Color.magenta));
            }
        }

        [UnityTest]
        public IEnumerator DestroyedBone_LeavesBoxInPlace()
        {
            Transform bone = CreateBone(new Vector3(3f, 0f, 0f), Quaternion.Euler(0f, 60f, 0f));
            IBox box = CreateContainer().CreateBox();
            box.Bone = bone;
            box.LocalCornerA = Vector3.zero;
            box.LocalCornerB = Vector3.one;
            Vector3 expectedA = bone.TransformPoint(Vector3.zero);
            Vector3 expectedB = bone.TransformPoint(Vector3.one);

            Object.Destroy(bone.gameObject);
            yield return null;

            Assert.That(box.Bone, Is.Null);
            AssertApproximately(expectedA, box.WorldCornerA);
            AssertApproximately(expectedB, box.WorldCornerB);
        }
    }
}
