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
        public void CreateBox_WithoutArguments_IsUnitCubeAtOrigin()
        {
            IBox box = CreateContainer().CreateBox();

            Assert.That(box.Size, Is.EqualTo(Vector3.one));
            Assert.That(box.LocalPosition, Is.EqualTo(Vector3.zero));
            Assert.That(box.LocalRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(box.Color, Is.EqualTo(Color.white));
            AssertApproximately(new Vector3(-0.5f, -0.5f, -0.5f), box.WorldCornerA);
            AssertApproximately(new Vector3(0.5f, 0.5f, 0.5f), box.WorldCornerB);
        }

        [Test]
        public void Corners_ComeBackAsTheyWereSet()
        {
            // Corner B is below corner A on y, so the size is negative there.
            IBox box = CreateContainer().CreateBox(new Vector3(1f, 2f, 3f), new Vector3(4f, -5f, 6f));

            AssertApproximately(new Vector3(1f, 2f, 3f), box.WorldCornerA);
            AssertApproximately(new Vector3(4f, -5f, 6f), box.WorldCornerB);
            AssertApproximately(new Vector3(3f, -7f, 3f), box.Size);
            AssertApproximately(new Vector3(2.5f, -1.5f, 4.5f), box.WorldPosition);
        }

        [Test]
        public void PoseForm_TurnsTheBox()
        {
            LineContainer container = CreateContainer();
            Quaternion rotation = Quaternion.Euler(0f, 45f, 0f);
            IBox box = container.CreateBox(new Vector3(0f, 1f, 0f), rotation, new Vector3(2f, 4f, 6f));

            Vector3[] corners = BakeShape(container, box);

            AssertApproximately(new Vector3(0f, 1f, 0f) + rotation * new Vector3(-1f, -2f, -3f), corners[0]);
            AssertApproximately(new Vector3(0f, 1f, 0f) + rotation * new Vector3(1f, 2f, 3f), corners[7]);
            AssertApproximately(corners[0], box.WorldCornerA);
            AssertApproximately(corners[7], box.WorldCornerB);
        }

        [Test]
        public void SettingCorner_MovesOnlyThatCorner()
        {
            LineContainer container = CreateContainer();
            Quaternion rotation = Quaternion.Euler(20f, 30f, 40f);
            IBox box = container.CreateBox(Vector3.zero, rotation, Vector3.one);
            Vector3 cornerB = box.WorldCornerB;
            Vector3 target = rotation * new Vector3(-2f, -1f, -3f);

            box.WorldCornerA = target;

            AssertApproximately(target, box.WorldCornerA);
            AssertApproximately(cornerB, box.WorldCornerB);
            AssertApproximately(rotation, box.WorldRotation);
            Vector3[] corners = BakeShape(container, box);
            AssertApproximately(target, corners[0]);
            AssertApproximately(cornerB, corners[7]);
        }

        [Test]
        public void ChangingBone_KeepsWorldCornersAndRotation()
        {
            Transform bone = CreateBone(new Vector3(4f, 0f, 0f), Quaternion.Euler(0f, 30f, 0f), 2f);
            IBox box = CreateContainer().CreateBox(new Vector3(-1f, -1f, -1f), new Vector3(1f, 1f, 1f));

            box.Bone = bone;

            AssertApproximately(new Vector3(-1f, -1f, -1f), box.WorldCornerA);
            AssertApproximately(new Vector3(1f, 1f, 1f), box.WorldCornerB);
            AssertApproximately(Quaternion.identity, box.WorldRotation);
            AssertApproximately(bone.InverseTransformPoint(new Vector3(-1f, -1f, -1f)), box.LocalCornerA);
            AssertApproximately(Vector3.one, box.Size);
        }

        [Test]
        public void CreateBoxOnBone_IsAlignedToTheBone()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(0f, 2f, 0f), Quaternion.Euler(10f, 20f, 30f), 1.5f);
            Vector3 extents = new(0.5f, 1f, 1.5f);

            IBox box = container.CreateBox(bone, -extents, extents);

            Assert.That(box.Bone, Is.SameAs(bone));
            Assert.That(box.LocalRotation, Is.EqualTo(Quaternion.identity));
            Vector3[] corners = BakeShape(container, box);
            AssertApproximately(bone.TransformPoint(-extents), corners[0]);
            AssertApproximately(bone.TransformPoint(extents), corners[7]);
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

            Assert.That(box.Color, Is.EqualTo(Color.magenta));
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
