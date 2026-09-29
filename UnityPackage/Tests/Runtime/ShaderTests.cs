using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>
    /// Renders shapes with a camera and reads the pixels back, to check that the shader puts every vertex where its
    /// bone's matrix says: the same place the CPU-side tests expect.
    /// </summary>
    public class ShaderTests : WireframesTestBase
    {
        private const int Size = 64;
        // The camera sees world X and Y from -HalfExtent to +HalfExtent.
        private const float HalfExtent = 4f;

        private Camera _camera;
        private RenderTexture _target;
        private Texture2D _readback;

        [SetUp]
        public void CreateCamera()
        {
            _target = Track(new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32));
            _readback = Track(new Texture2D(Size, Size, TextureFormat.RGBA32, false));
            _camera = Track(new GameObject("Shader Test Camera")).AddComponent<Camera>();
            _camera.enabled = false;
            // Without multisampling, a one-pixel line lands on one row at full brightness.
            _camera.allowMSAA = false;
            _camera.orthographic = true;
            _camera.orthographicSize = HalfExtent;
            _camera.transform.position = new Vector3(0f, 0f, -10f);
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = Color.black;
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 100f;
            _camera.targetTexture = _target;
        }

        [Test]
        public void BonePose_PlacesVerticesWhereTheCpuReferenceDoes()
        {
            WireframeContainer container = CreateContainer();
            // Turned a quarter around Z and scaled 2, so local +Y runs toward world -X, twice as long.
            Transform bone = CreateBone(new Vector3(2f, 0f, 0f), Quaternion.Euler(0f, 0f, 90f), 2f);
            ILine skinned = container.CreateLine(bone, bone);
            skinned.LocalPositionB = new Vector3(0f, 1f, 0f);
            skinned.SetColor(Color.red);
            ILine world = container.CreateLine(new Vector3(-3f, 2f, 0f), new Vector3(-1f, 2f, 0f));
            world.SetColor(Color.red);

            Vector3[] vertices = BakeShape(container, skinned);
            AssertApproximately(new Vector3(2f, 0f, 0f), vertices[0]);
            AssertApproximately(Vector3.zero, vertices[1]);
            Render();

            Assert.That(IsLit(Vector3.Lerp(vertices[0], vertices[1], 0.5f)), Is.True, "The skinned line isn't drawn.");
            Assert.That(IsLit(new Vector3(-2f, 2f, 0f)), Is.True, "The world-space line isn't drawn.");
            Assert.That(IsLit(new Vector3(-2f, 0f, 0f)), Is.False, "Something is drawn where nothing should be.");
            Assert.That(IsLit(new Vector3(1f, -2f, 0f)), Is.False, "Something is drawn where nothing should be.");
        }

        [Test]
        public void MovingABone_MovesItsShapesWithoutAnyEdit()
        {
            WireframeContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(0f, 2f, 0f), Quaternion.identity);
            ILine line = container.CreateLine(bone, bone);
            line.LocalPositionA = new Vector3(-1f, 0f, 0f);
            line.LocalPositionB = new Vector3(1f, 0f, 0f);
            line.SetColor(Color.red);
            Render();
            Assert.That(IsLit(new Vector3(0f, 2f, 0f)), Is.True);

            bone.position = new Vector3(0f, -2f, 0f);
            Render();

            Assert.That(IsLit(new Vector3(0f, -2f, 0f)), Is.True);
            Assert.That(IsLit(new Vector3(0f, 2f, 0f)), Is.False);
        }

        [UnityTest]
        public IEnumerator DestroyedBone_KeepsDrawingItsShapesInPlace()
        {
            WireframeContainer container = CreateContainer();
            Transform bone = CreateBone(new Vector3(0f, -2f, 0f), Quaternion.identity);
            ICircle circle = container.CreateCircle(bone, Vector3.zero, 1f);
            circle.LocalRotation = Quaternion.Euler(-90f, 0f, 0f);
            circle.Color = Color.red;
            Render();

            Object.Destroy(bone.gameObject);
            yield return null;
            Render();

            // The circle faces the camera, so its rim passes through these points.
            Assert.That(IsLit(new Vector3(1f, -2f, 0f)), Is.True);
            Assert.That(IsLit(new Vector3(-1f, -2f, 0f)), Is.True);
            Assert.That(IsLit(new Vector3(0f, 2f, 0f)), Is.False);
        }

        private void Render()
        {
            _camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = _target;
            _readback.ReadPixels(new Rect(0f, 0f, Size, Size), 0, 0);
            _readback.Apply();
            RenderTexture.active = previous;
        }

        /// <summary>True when any pixel around <paramref name="world"/> is red, allowing a pixel for line rasterization.</summary>
        private bool IsLit(Vector3 world)
        {
            int x = Mathf.RoundToInt((world.x + HalfExtent) / (HalfExtent * 2f) * Size);
            int y = Mathf.RoundToInt((world.y + HalfExtent) / (HalfExtent * 2f) * Size);
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int px = Mathf.Clamp(x + dx, 0, Size - 1);
                    int py = Mathf.Clamp(y + dy, 0, Size - 1);
                    if (_readback.GetPixel(px, py).r > 0.25f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
