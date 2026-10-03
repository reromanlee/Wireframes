using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>
    /// Shape components: drawn as soon as they are added, kept in step with their fields and GameObjects, hidden when
    /// disabled, and sharing containers by their settings.
    /// </summary>
    public class ShapeComponentTests : WireframesTestBase
    {
        [Test]
        public void AddedComponent_DrawsItsShapeRightAway()
        {
            WireframeSphere sphere = AddShape<WireframeSphere>(new Vector3(1f, 2f, 3f));

            Assert.That(sphere.Shape, Is.Not.Null);
            Assert.That(sphere.Shape.IsSuspended, Is.False);
            Assert.That(ShapeCountOf(sphere.SharedContainer.Container), Is.EqualTo(1));
            // The first circle starts on +Z, half a unit from the center.
            AssertApproximately(new Vector3(1f, 2f, 3.5f), Bake(sphere)[0]);
        }

        [Test]
        public void ComponentsWithTheSameSettings_ShareAContainer()
        {
            int containerCount = SharedContainers.Count;
            WireframeSphere first = AddShape<WireframeSphere>();
            WireframeBox second = AddShape<WireframeBox>();
            WireframeLine onTop = AddShape<WireframeLine>();
            onTop.Occlusion = WireframeOcclusion.Show;
            WireframeCircle transparent = AddShape<WireframeCircle>();
            transparent.Color = new Color(1f, 1f, 1f, 0.5f);
            WireframeStar layered = AddShape<WireframeStar>(Vector3.zero, 5);

            Assert.That(second.SharedContainer, Is.SameAs(first.SharedContainer));
            Assert.That(onTop.SharedContainer.Key.Occlusion, Is.EqualTo(WireframeOcclusion.Show));
            Assert.That(transparent.SharedContainer.Key.UsesAlpha, Is.True);
            Assert.That(layered.SharedContainer.Key.Layer, Is.EqualTo(5));
            Assert.That(SharedContainers.Count, Is.EqualTo(containerCount + 4));

            WireframeContainer shared = first.SharedContainer.Container;
            Object.DestroyImmediate(first.gameObject);
            Assert.That(shared.IsDisposed, Is.False);
            Object.DestroyImmediate(second.gameObject);
            Assert.That(shared.IsDisposed, Is.True);
            Assert.That(SharedContainers.Count, Is.EqualTo(containerCount + 3));
        }

        [Test]
        public void PropertyEdits_ShowUpInTheShape()
        {
            WireframeSphere sphere = AddShape<WireframeSphere>();

            sphere.Radius = 2f;
            sphere.Center = new Vector3(0f, 1f, 0f);
            sphere.EulerAngles = new Vector3(90f, 0f, 0f);

            // Turning 90 degrees around X takes the first circle's start from +Z to -Y.
            AssertApproximately(new Vector3(0f, -1f, 0f), Bake(sphere)[0]);
            AssertApproximately(Quaternion.Euler(90f, 0f, 0f), sphere.Rotation);
        }

        [Test]
        public void Shape_FollowsItsGameObject()
        {
            WireframeCircle circle = AddShape<WireframeCircle>();

            circle.transform.SetPositionAndRotation(new Vector3(0f, 0f, 5f), Quaternion.Euler(0f, 90f, 0f));
            circle.transform.localScale = Vector3.one * 2f;

            // The circle starts on its +Z, half a unit out, which the GameObject scales to 1 and turns to +X.
            AssertApproximately(new Vector3(1f, 0f, 5f), Bake(circle)[0]);
        }

        [Test]
        public void DisabledComponent_GivesBackItsShapeAndComesBackAsItIsThen()
        {
            WireframeSphere sphere = AddShape<WireframeSphere>();
            WireframeContainer container = sphere.SharedContainer.Container;

            sphere.enabled = false;
            sphere.Radius = 3f;

            Assert.That(sphere.Shape.IsSuspended, Is.True);
            Assert.That(ShapeCountOf(container), Is.Zero);
            Assert.That(container.Proxy.Bones.Count, Is.Zero);

            sphere.enabled = true;

            Assert.That(ShapeCountOf(container), Is.EqualTo(1));
            AssertApproximately(new Vector3(0f, 0f, 3f), Bake(sphere)[0]);
        }

        [Test]
        public void DeactivatedGameObject_HidesTheShapeToo()
        {
            WireframeBox box = AddShape<WireframeBox>();

            box.gameObject.SetActive(false);
            Assert.That(box.Shape.IsSuspended, Is.True);

            box.gameObject.SetActive(true);
            Assert.That(box.Shape.IsSuspended, Is.False);
        }

        [Test]
        public void TogglingAComponent_AllocatesNothing()
        {
            WireframeSphere sphere = AddShape<WireframeSphere>();
            WireframeContainer container = sphere.SharedContainer.Container;
            // Warms up every path once.
            sphere.enabled = false;
            sphere.enabled = true;
            container.Proxy.Flush();

            Assert.That(() =>
            {
                sphere.enabled = false;
                container.Proxy.Flush();
                sphere.enabled = true;
                container.Proxy.Flush();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void PropertyEdits_AllocateNothing()
        {
            WireframeCapsule capsule = AddShape<WireframeCapsule>();
            WireframeContainer container = capsule.SharedContainer.Container;
            capsule.RadiusA = 0.2f;
            container.Proxy.Flush();

            Assert.That(() =>
            {
                capsule.RadiusA = 0.3f;
                capsule.EndB = new Vector3(0f, 2f, 0f);
                capsule.Roll = 45f;
                capsule.Color = Color.red;
                container.Proxy.Flush();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void SteadyFramesWithMovingComponents_AllocateNothing()
        {
            WireframeShape[] shapes =
            {
                AddShape<WireframeSphere>(), AddShape<WireframeLine>(), AddShape<WireframePolyline>(),
                AddShape<WireframeCone>()
            };
            WireframeContainer container = shapes[0].SharedContainer.Container;
            container.Proxy.Flush();

            Assert.That(() =>
            {
                foreach (WireframeShape shape in shapes)
                {
                    shape.transform.Translate(0.1f, 0f, 0f);
                }
                container.Proxy.Flush();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void TransparentColor_MovesTheShapeToATransparentContainerAndBack()
        {
            WireframeCircle circle = AddShape<WireframeCircle>();
            WireframeContainer opaque = circle.SharedContainer.Container;

            circle.Color = new Color(0f, 1f, 0f, 0.5f);

            Assert.That(circle.SharedContainer.Key.UsesAlpha, Is.True);
            Assert.That(ShapeCountOf(circle.SharedContainer.Container), Is.EqualTo(1));
            Assert.That(opaque.IsDisposed, Is.True);

            circle.Color = Color.green;

            Assert.That(circle.SharedContainer.Key.UsesAlpha, Is.False);
            Assert.That(ShapeCountOf(circle.SharedContainer.Container), Is.EqualTo(1));
        }

        [Test]
        public void ChangingACount_CreatesTheShapeAgain()
        {
            WireframeSphere sphere = AddShape<WireframeSphere>();
            Shape before = sphere.Shape;

            sphere.SegmentCount = 8;

            Assert.That(sphere.Shape, Is.Not.SameAs(before));
            Assert.That(before.IsDisposed, Is.True);
            Assert.That(sphere.Shape.VertexCount, Is.EqualTo(24));
            Assert.That(ShapeCountOf(sphere.SharedContainer.Container), Is.EqualTo(1));
        }

        [Test]
        public void InvalidValues_ThrowAndChangeNothing()
        {
            WireframeCylinder cylinder = AddShape<WireframeCylinder>();
            WireframeSpikedSphere spikedSphere = AddShape<WireframeSpikedSphere>();
            WireframeStar star = AddShape<WireframeStar>();
            WireframeFrustum frustum = AddShape<WireframeFrustum>();
            WireframeCircle circle = AddShape<WireframeCircle>();
            WireframePolyline polyline = AddShape<WireframePolyline>();

            Assert.Throws<ArgumentOutOfRangeException>(() => cylinder.SegmentCount = 30);
            Assert.Throws<ArgumentOutOfRangeException>(() => spikedSphere.SpikeCount = 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => star.PointCount = 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => frustum.SideCount = 1025);
            Assert.Throws<ArgumentOutOfRangeException>(() => circle.SegmentCount = 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => circle.Occlusion = (WireframeOcclusion)7);
            Assert.Throws<ArgumentOutOfRangeException>(() => polyline.PointCount = -1);

            Assert.That(cylinder.SegmentCount, Is.EqualTo(32));
            Assert.That(spikedSphere.SpikeCount, Is.EqualTo(12));
            Assert.That(circle.Occlusion, Is.EqualTo(WireframeOcclusion.Hide));
            Assert.That(polyline.PointCount, Is.EqualTo(3));
        }

        [Test]
        public void SerializedCounts_AreCorrectedToTheNearestValidOne()
        {
            Assert.That(ShapeCounts.ClampSegmentCount(2), Is.EqualTo(3));
            Assert.That(ShapeCounts.ClampSegmentCount(5000), Is.EqualTo(1024));
            Assert.That(ShapeCounts.ClampQuarterSegmentCount(0), Is.EqualTo(4));
            Assert.That(ShapeCounts.ClampQuarterSegmentCount(33), Is.EqualTo(32));
            Assert.That(ShapeCounts.ClampQuarterSegmentCount(34), Is.EqualTo(36));
            Assert.That(ShapeCounts.ClampQuarterSegmentCount(5000), Is.EqualTo(1024));
            Assert.That(ShapeCounts.ClampPointCount(600), Is.EqualTo(512));
            Assert.That(ShapeCounts.ClampSideCount(0), Is.EqualTo(3));
            Assert.That(ShapeCounts.ClampSpikeCount(1), Is.EqualTo(4));
            Assert.That(ShapeCounts.ClampSpikeCount(7), Is.EqualTo(6));
            Assert.That(ShapeCounts.ClampSpikeCount(11), Is.EqualTo(12));
            Assert.That(ShapeCounts.ClampSpikeCount(100), Is.EqualTo(20));
        }

        [Test]
        public void CenteredComponents_DrawWhatTheirCreateMethodsDraw()
        {
            WireframeContainer container = CreateContainer();
            Vector3 center = new(1f, 2f, 3f);
            Vector3 eulerAngles = new(10f, 20f, 30f);
            Quaternion rotation = Quaternion.Euler(eulerAngles);

            WireframeBox box = Centered<WireframeBox>(center, eulerAngles);
            box.Size = new Vector3(1f, 2f, 3f);
            AssertSameVertices(container, container.CreateBox(center, rotation, box.Size), box);

            WireframeRectangle rectangle = Centered<WireframeRectangle>(center, eulerAngles);
            rectangle.Size = new Vector2(2f, 1f);
            AssertSameVertices(container, container.CreateRectangle(center, rotation, rectangle.Size), rectangle);

            WireframeRoundedRectangle rounded = Centered<WireframeRoundedRectangle>(center, eulerAngles);
            rounded.Size = new Vector2(2f, 1f);
            rounded.CornerRadius = 0.3f;
            rounded.SegmentCount = 16;
            AssertSameVertices(
                container, container.CreateRoundedRectangle(center, rotation, rounded.Size, 0.3f, 16), rounded);

            WireframeCircle circle = Centered<WireframeCircle>(center, eulerAngles);
            circle.Radius = 1.5f;
            circle.SegmentCount = 12;
            AssertSameVertices(container, container.CreateCircle(center, rotation, 1.5f, 12), circle);

            WireframeEllipse ellipse = Centered<WireframeEllipse>(center, eulerAngles);
            ellipse.Radii = new Vector2(0.5f, 1f);
            ellipse.SegmentCount = 10;
            AssertSameVertices(container, container.CreateEllipse(center, rotation, ellipse.Radii, 10), ellipse);

            WireframeStar star = Centered<WireframeStar>(center, eulerAngles);
            star.InnerRadius = 0.3f;
            star.OuterRadius = 1f;
            star.PointCount = 6;
            AssertSameVertices(container, container.CreateStar(center, rotation, 0.3f, 1f, 6), star);

            WireframeSphere sphere = Centered<WireframeSphere>(center, eulerAngles);
            sphere.Radius = 1.2f;
            sphere.SegmentCount = 9;
            AssertSameVertices(container, container.CreateSphere(center, rotation, 1.2f, 9), sphere);

            WireframeEllipsoid ellipsoid = Centered<WireframeEllipsoid>(center, eulerAngles);
            ellipsoid.Radii = new Vector3(0.5f, 1f, 1.5f);
            ellipsoid.SegmentCount = 8;
            AssertSameVertices(container, container.CreateEllipsoid(center, rotation, ellipsoid.Radii, 8), ellipsoid);

            WireframeSpikedSphere spikedSphere = Centered<WireframeSpikedSphere>(center, eulerAngles);
            spikedSphere.BaseRadius = 0.4f;
            spikedSphere.SpikeLength = 0.6f;
            spikedSphere.SpikeCount = 8;
            AssertSameVertices(container, container.CreateSpikedSphere(center, rotation, 0.4f, 0.6f, 8), spikedSphere);
        }

        [Test]
        public void AxialComponents_DrawWhatTheirCreateMethodsDraw()
        {
            WireframeContainer container = CreateContainer();
            Vector3 endA = new(1f, 0f, 2f);
            Vector3 endB = new(2f, 3f, 1f);

            WireframeCylinder cylinder = Axial<WireframeCylinder>(endA, endB);
            cylinder.Radius = 0.7f;
            cylinder.SegmentCount = 12;
            AssertSameVertices(container, container.CreateCylinder(endA, endB, 0.7f, 12), cylinder);

            WireframeCone cone = Axial<WireframeCone>(endA, endB);
            cone.Radius = 0.7f;
            cone.SegmentCount = 8;
            AssertSameVertices(container, container.CreateCone(endA, endB, 0.7f, 8), cone);

            WireframeCapsule capsule = Axial<WireframeCapsule>(endA, endB);
            capsule.RadiusA = 0.4f;
            capsule.RadiusB = 0.6f;
            capsule.SegmentCount = 16;
            AssertSameVertices(container, container.CreateCapsule(endA, endB, 0.4f, 0.6f, 16), capsule);

            WireframeStadium stadium = Axial<WireframeStadium>(endA, endB);
            stadium.RadiusA = 0.4f;
            stadium.RadiusB = 0.6f;
            stadium.SegmentCount = 16;
            AssertSameVertices(container, container.CreateStadium(endA, endB, 0.4f, 0.6f, 16), stadium);

            WireframeFrustum frustum = Axial<WireframeFrustum>(endA, endB);
            frustum.RadiusA = 0.4f;
            frustum.RadiusB = 0.8f;
            frustum.SideCount = 5;
            AssertSameVertices(container, container.CreateFrustum(endA, endB, 0.4f, 0.8f, 5), frustum);

            WireframePyramid pyramid = Axial<WireframePyramid>(endA, endB);
            pyramid.BaseSize = new Vector2(1f, 0.5f);
            AssertSameVertices(container, container.CreatePyramid(endA, endB, pyramid.BaseSize), pyramid);
        }

        [Test]
        public void Roll_TurnsTheShapeAroundItsAxis()
        {
            Vector3 endA = new(1f, 0f, 2f);
            Vector3 endB = new(2f, 3f, 1f);
            WireframeFrustum unrolled = Axial<WireframeFrustum>(endA, endB);
            WireframeFrustum rolled = Axial<WireframeFrustum>(endA, endB);

            rolled.Roll = 90f;

            Quaternion roll = Quaternion.AngleAxis(90f, endB - endA);
            Vector3[] expected = Bake(unrolled);
            Vector3[] actual = Bake(rolled);
            for (int i = 0; i < expected.Length; i++)
            {
                AssertApproximately(endA + roll * (expected[i] - endA), actual[i]);
            }
        }

        [Test]
        public void Line_JoinsPointsOnTheirBonesOrItsGameObject()
        {
            WireframeLine line = AddShape<WireframeLine>(new Vector3(1f, 0f, 0f));
            Transform target = CreateBone(new Vector3(0f, 5f, 0f), Quaternion.identity);

            line.PositionA = new Vector3(0f, 1f, 0f);
            line.BoneB = target;
            line.PositionB = new Vector3(0f, 0f, 1f);

            Vector3[] vertices = Bake(line);
            AssertApproximately(new Vector3(1f, 1f, 0f), vertices[0]);
            AssertApproximately(new Vector3(0f, 5f, 1f), vertices[1]);

            target.position = new Vector3(0f, 6f, 0f);

            AssertApproximately(new Vector3(0f, 6f, 1f), Bake(line)[1]);
        }

        [Test]
        public void DestroyedBone_LeavesItsPointOnTheGameObjectInTheSameFlush()
        {
            WireframeLine line = AddShape<WireframeLine>(new Vector3(1f, 0f, 0f));
            Transform target = CreateBone(new Vector3(0f, 5f, 0f), Quaternion.identity);
            line.BoneB = target;
            line.PositionB = new Vector3(0f, 0f, 1f);
            WireframeContainer container = line.SharedContainer.Container;
            container.Proxy.Flush();

            Object.DestroyImmediate(target.gameObject);

            // The flush that finds the bone destroyed moves the point before uploading anything.
            AssertApproximately(new Vector3(1f, 0f, 1f), Bake(line)[1]);
            Assert.That(container.Proxy.Bones.Count, Is.EqualTo(1));
        }

        [Test]
        public void Polyline_DrawsNothingWithTooFewPoints()
        {
            WireframePolyline polyline = AddShape<WireframePolyline>();
            Assert.That(polyline.Shape, Is.Not.Null);

            polyline.PointCount = 2;

            Assert.That(polyline.Shape, Is.Null);

            polyline.IsClosed = false;

            Assert.That(polyline.Shape, Is.Not.Null);
            Assert.That(BakeEdges(polyline.SharedContainer.Container, polyline.Shape), Has.Length.EqualTo(1));
        }

        [Test]
        public void Polyline_PointsFollowTheirBonesOrItsGameObject()
        {
            WireframePolyline polyline = AddShape<WireframePolyline>(new Vector3(0f, 1f, 0f));
            Transform bone = CreateBone(new Vector3(3f, 0f, 0f), Quaternion.identity);

            polyline.PointCount = 4;
            polyline.SetBone(3, bone);
            polyline.SetPosition(3, new Vector3(0f, 0f, 1f));

            Vector3[] vertices = Bake(polyline);
            Assert.That(vertices, Has.Length.EqualTo(4));
            // The default triangle has a corner on +Z, half a unit from the GameObject.
            AssertApproximately(new Vector3(0f, 1f, 0.5f), vertices[0]);
            AssertApproximately(new Vector3(3f, 0f, 1f), vertices[3]);
            Assert.That(polyline.GetBone(3), Is.SameAs(bone));
            Assert.Throws<ArgumentOutOfRangeException>(() => polyline.SetPosition(4, Vector3.zero));
        }

        [Test]
        public void GizmoShape_GoesIntoAContainerThatCamerasFilter()
        {
            WireframeSphere gizmo = AddShape<WireframeSphere>();
            WireframeSphere plain = AddShape<WireframeSphere>();

            gizmo.DrawAsGizmo = true;

            Assert.That(gizmo.SharedContainer, Is.Not.SameAs(plain.SharedContainer));
            Assert.That(gizmo.SharedContainer.Container.Proxy.CameraFilter, Is.Not.Null);
            Assert.That(ChunkOf(gizmo.SharedContainer.Container).Renderer.forceRenderingOff, Is.True);
            Assert.That(ChunkOf(plain.SharedContainer.Container).Renderer.forceRenderingOff, Is.False);
        }

        private T AddShape<T>(Vector3 position = default, int layer = 0) where T : WireframeShape
        {
            GameObject owner = Track(new GameObject(typeof(T).Name) { layer = layer });
            owner.transform.position = position;
            return owner.AddComponent<T>();
        }

        private T Centered<T>(Vector3 center, Vector3 eulerAngles) where T : WireframeCenteredShape
        {
            T shape = AddShape<T>();
            shape.Center = center;
            shape.EulerAngles = eulerAngles;
            return shape;
        }

        private T Axial<T>(Vector3 endA, Vector3 endB) where T : WireframeAxialShape
        {
            T shape = AddShape<T>();
            shape.EndA = endA;
            shape.EndB = endB;
            return shape;
        }

        private static Vector3[] Bake(WireframeShape component)
        {
            return BakeShape(component.SharedContainer.Container, component.Shape);
        }

        private static void AssertSameVertices(WireframeContainer container, IShape expected, WireframeShape actual)
        {
            Vector3[] expectedVertices = BakeShape(container, expected);
            Vector3[] actualVertices = Bake(actual);
            Assert.That(actualVertices, Has.Length.EqualTo(expectedVertices.Length), actual.GetType().Name);
            for (int i = 0; i < expectedVertices.Length; i++)
            {
                AssertApproximately(expectedVertices[i], actualVertices[i]);
            }
        }
    }
}
