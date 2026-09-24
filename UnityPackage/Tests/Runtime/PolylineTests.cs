using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Tests
{
    public class PolylineTests : WireframesTestBase
    {
        private static readonly Vector3[] Square =
        {
            new(0f, 0f, 0f), new(1f, 0f, 0f), new(1f, 0f, 1f), new(0f, 0f, 1f)
        };

        [Test]
        public void OpenPolyline_JoinsPointsInOrder()
        {
            LineContainer container = CreateContainer();
            IPolyline polyline = container.CreatePolyline(Square);

            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, polyline);

            Assert.That(polyline.PointCount, Is.EqualTo(4));
            Assert.That(polyline.IsClosed, Is.False);
            Assert.That(edges, Has.Length.EqualTo(3));
            for (int i = 0; i < 3; i++)
            {
                AssertApproximately(Square[i], edges[i].A);
                AssertApproximately(Square[i + 1], edges[i].B);
            }
        }

        [Test]
        public void Polygon_ClosesBackToTheFirstPoint()
        {
            LineContainer container = CreateContainer();
            IPolyline polygon = container.CreatePolygon(Square);

            (Vector3 A, Vector3 B)[] edges = BakeEdges(container, polygon);

            Assert.That(polygon.IsClosed, Is.True);
            Assert.That(edges, Has.Length.EqualTo(4));
            AssertApproximately(Square[3], edges[3].A);
            AssertApproximately(Square[0], edges[3].B);
        }

        [Test]
        public void Triangle_IsAClosedPolylineOfThreePoints()
        {
            LineContainer container = CreateContainer();
            IPolyline triangle = container.CreateTriangle(Vector3.zero, Vector3.right, Vector3.up);

            Assert.That(triangle.PointCount, Is.EqualTo(3));
            Assert.That(triangle.IsClosed, Is.True);
            Assert.That(BakeEdges(container, triangle), Has.Length.EqualTo(3));
            Assert.That(triangle.GetWorldPosition(2), Is.EqualTo(Vector3.up));
        }

        [Test]
        public void CreatePolyline_CopiesTheList()
        {
            List<Vector3> points = new(Square);
            IPolyline polyline = CreateContainer().CreatePolyline(points);

            points[0] = new Vector3(9f, 9f, 9f);

            Assert.That(polyline.GetLocalPosition(0), Is.EqualTo(Vector3.zero));
            Assert.That(polyline.GetColor(0), Is.EqualTo(Color.white));
        }

        [Test]
        public void TooFewPoints_Throw()
        {
            LineContainer container = CreateContainer();

            Assert.Throws<ArgumentException>(() => container.CreatePolyline(Vector3.zero));
            Assert.Throws<ArgumentException>(() => container.CreatePolygon(Vector3.zero, Vector3.one));
            Assert.Throws<ArgumentNullException>(() => container.CreatePolyline((Vector3[])null));
            Assert.Throws<ArgumentNullException>(() => container.CreatePolygon((Transform[])null));
            Assert.That(ChunkOf(container).ShapeCount, Is.Zero);
        }

        [Test]
        public void PointIndexOutOfRange_Throws()
        {
            IPolyline polyline = CreateContainer().CreatePolyline(Square);

            Assert.Throws<ArgumentOutOfRangeException>(() => polyline.GetLocalPosition(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => polyline.SetBone(4, null));
        }

        [Test]
        public void CreatePolylineFromBones_PutsPointsOnTheBones()
        {
            LineContainer container = CreateContainer();
            Transform hips = CreateBone(new Vector3(0f, 1f, 0f), Quaternion.identity);
            Transform chest = CreateBone(new Vector3(0f, 1.5f, 0f), Quaternion.identity);
            Transform head = CreateBone(new Vector3(0f, 2f, 0f), Quaternion.identity);

            IPolyline spine = container.CreatePolyline(hips, chest, head);

            Assert.That(spine.GetBone(1), Is.SameAs(chest));
            Assert.That(spine.GetLocalPosition(1), Is.EqualTo(Vector3.zero));
            chest.position = new Vector3(0.5f, 1.5f, 0f);
            Vector3[] vertices = BakeShape(container, spine);
            AssertApproximately(hips.position, vertices[0]);
            AssertApproximately(chest.position, vertices[1]);
            AssertApproximately(head.position, vertices[2]);
        }

        [Test]
        public void SetBone_KeepsTheWorldPosition()
        {
            Transform bone = CreateBone(new Vector3(0f, 5f, 0f), Quaternion.Euler(30f, 45f, 0f), 0.5f);
            IPolyline polyline = CreateContainer().CreatePolyline(Square);

            polyline.SetBone(2, bone);

            AssertApproximately(Square[2], polyline.GetWorldPosition(2));
            AssertApproximately(bone.InverseTransformPoint(Square[2]), polyline.GetLocalPosition(2));
            polyline.SetWorldPosition(2, new Vector3(3f, 3f, 3f));
            AssertApproximately(new Vector3(3f, 3f, 3f), polyline.GetWorldPosition(2));
        }

        [Test]
        public void Colors_AreUploadedPerPoint()
        {
            LineContainer container = CreateContainer();
            IPolyline polyline = container.CreatePolyline(Square);
            int start = ((Polyline)polyline).VertexStart;

            polyline.SetColor(1, Color.red);
            container.Proxy.Flush();
            Color32[] colors = ChunkOf(container).Mesh.colors32;
            Assert.That(colors[start], Is.EqualTo((Color32)Color.white));
            Assert.That(colors[start + 1], Is.EqualTo((Color32)Color.red));

            polyline.SetColor(Color.green);
            container.Proxy.Flush();
            colors = ChunkOf(container).Mesh.colors32;
            Assert.That(polyline.GetColor(1), Is.EqualTo(Color.green));
            Assert.That(colors[start + 3], Is.EqualTo((Color32)Color.green));
        }

        [Test]
        public void Dispose_ReleasesEveryBone()
        {
            LineContainer container = CreateContainer();
            Transform bone = CreateBone(Vector3.zero, Quaternion.identity);
            IPolyline polyline = container.CreatePolygon(bone, bone, CreateBone(Vector3.one, Quaternion.identity));
            Assert.That(ChunkOf(container).Bones.Count, Is.EqualTo(2));

            polyline.Dispose();

            Assert.That(ChunkOf(container).Bones.Count, Is.Zero);
            Assert.Throws<ObjectDisposedException>(() => _ = polyline.PointCount);
        }

        [UnityTest]
        public IEnumerator DestroyedBone_LeavesItsPointsInPlace()
        {
            Transform bone = CreateBone(new Vector3(2f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
            IPolyline polyline = CreateContainer().CreatePolyline(Square);
            polyline.SetBone(0, bone);
            polyline.SetBone(3, bone);
            bone.position = new Vector3(5f, 1f, 0f);
            Vector3 expected0 = polyline.GetWorldPosition(0);
            Vector3 expected3 = polyline.GetWorldPosition(3);

            Object.Destroy(bone.gameObject);
            yield return null;

            Assert.That(polyline.GetBone(0), Is.Null);
            Assert.That(polyline.GetBone(3), Is.Null);
            AssertApproximately(expected0, polyline.GetWorldPosition(0));
            AssertApproximately(expected3, polyline.GetWorldPosition(3));
            AssertApproximately(Square[1], polyline.GetWorldPosition(1));
        }
    }
}
