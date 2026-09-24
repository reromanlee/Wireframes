using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes.Samples
{
    /// <summary>
    /// Lays out every shape the package draws in three rows: shapes made of points and flat shapes on top, shapes
    /// around a center in the middle and long shapes at the bottom. Each shape follows its own bone, and only the bones
    /// turn each frame, so the package does no work to animate them. Open the ShapeGallery scene and enter Play mode,
    /// or add it to an empty GameObject in any scene with a camera and frame that GameObject.
    /// </summary>
    public sealed class ShapeGallery : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float _spacing = 3f;
        [Tooltip("Degrees per second each shape turns around the world's up axis.")]
        [SerializeField] private float _turnSpeed = 30f;
        [SerializeField] private bool _showNames = true;

        private readonly List<Transform> _bones = new();
        private readonly List<string> _names = new();
        private LineContainer _container;
        private GUIStyle _nameStyle;

        private void Start()
        {
            _container = new LineContainer();
            // Flat shapes lie in their bone's XZ plane, so their bones are tipped over to face the camera.
            Quaternion facing = Quaternion.Euler(-90f, 0f, 0f);
            Quaternion tilted = Quaternion.Euler(-20f, 0f, 0f);

            // Points and flat shapes. Lines and polylines get a bone per point, here children of the exhibit's bone.
            Row row = new(this, 2, 9, facing);
            Transform bone = row.Next("Line");
            _container.CreateLine(Child(bone, -0.8f, -0.8f), Child(bone, 0.8f, 0.8f)).SetColor(row.Color);
            bone = row.Next("Polyline");
            _container.CreatePolyline(
                Child(bone, -0.9f, -0.6f), Child(bone, -0.3f, 0.6f), Child(bone, 0.3f, -0.6f), Child(bone, 0.9f, 0.6f))
                .SetColor(row.Color);
            bone = row.Next("Triangle");
            IPolyline triangle = _container.CreateTriangle(
                Child(bone, -0.9f, -0.7f), Child(bone, 0.9f, -0.7f), Child(bone, 0f, 0.9f));
            // A color per corner, blended along the edges.
            triangle.SetColor(0, Color.red);
            triangle.SetColor(1, Color.green);
            triangle.SetColor(2, Color.blue);
            _container.CreateRectangle(row.Next("Rectangle"), new Vector3(-0.9f, 0f, -0.6f), new Vector3(0.9f, 0f, 0.6f))
                .SetColor(row.Color);
            _container.CreateRoundedRectangle(
                    row.Next("Rounded Rectangle"), new Vector3(-0.9f, 0f, -0.6f), new Vector3(0.9f, 0f, 0.6f), 0.3f)
                .SetColor(row.Color);
            _container.CreateCircle(row.Next("Circle"), Vector3.zero, 0.8f).SetColor(row.Color);
            _container.CreateEllipse(row.Next("Ellipse"), new Vector3(0f, 0f, -0.9f), new Vector3(0f, 0f, 0.9f), 0.45f)
                .SetColor(row.Color);
            _container.CreateStar(row.Next("Star"), Vector3.zero, 0.35f, 0.9f, 5).SetColor(row.Color);
            _container.CreateStadium(
                    row.Next("Stadium"), new Vector3(0f, 0f, -0.5f), new Vector3(0f, 0f, 0.5f), 0.45f, 0.25f)
                .SetColor(row.Color);

            // Shapes around a center.
            row = new Row(this, 1, 8, tilted);
            _container.CreateBox(row.Next("Box"), Vector3.one * -0.6f, Vector3.one * 0.6f).SetColor(row.Color);
            _container.CreateSphere(row.Next("Sphere"), Vector3.zero, 0.8f).SetColor(row.Color);
            _container.CreateEllipsoid(row.Next("Ellipsoid"), new Vector3(0f, -0.9f, 0f), new Vector3(0f, 0.9f, 0f), 0.5f)
                .SetColor(row.Color);
            foreach (int spikes in new[] { 4, 6, 8, 12, 20 })
            {
                _container.CreateSpikedSphere(row.Next($"Spiked Sphere ({spikes})"), Vector3.zero, 0.4f, 0.5f, spikes)
                    .SetColor(row.Color);
            }

            // Long shapes, standing along their bone's +Y.
            row = new Row(this, 0, 7, tilted);
            Vector3 bottom = new(0f, -0.8f, 0f);
            Vector3 top = new(0f, 0.8f, 0f);
            _container.CreateCylinder(row.Next("Cylinder"), bottom, top, 0.5f).SetColor(row.Color);
            _container.CreateCone(row.Next("Cone"), top, bottom, 0.6f).SetColor(row.Color);
            _container.CreateCapsule(row.Next("Capsule"), bottom * 0.5f, top * 0.5f, 0.45f, 0.45f).SetColor(row.Color);
            _container.CreateCapsule(row.Next("Tapered Capsule"), bottom * 0.5f, top * 0.6f, 0.5f, 0.25f).SetColor(row.Color);
            _container.CreateFrustum(row.Next("Frustum"), bottom, top, 0.8f, 0.4f, 4).SetColor(row.Color);
            _container.CreateFrustum(row.Next("Frustum (6 sides)"), bottom, top, 0.5f, 0.8f, 6).SetColor(row.Color);
            _container.CreatePyramid(row.Next("Pyramid"), top, bottom, new Vector2(1.4f, 1.4f)).SetColor(row.Color);
        }

        private void Update()
        {
            // Turning the bones is all it takes to animate every shape; the package does no work for it.
            float angle = _turnSpeed * Time.deltaTime;
            foreach (Transform bone in _bones)
            {
                bone.Rotate(0f, angle, 0f, Space.World);
            }
        }

        private void OnGUI()
        {
            Camera camera = Camera.main;
            if (!_showNames || camera == null)
            {
                return;
            }
            _nameStyle ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter };
            for (int i = 0; i < _bones.Count; i++)
            {
                Vector3 screen = camera.WorldToScreenPoint(_bones[i].position + Vector3.down * (_spacing * 0.4f));
                if (screen.z > 0f)
                {
                    GUI.Label(new Rect(screen.x - 75f, Screen.height - screen.y, 150f, 22f), _names[i], _nameStyle);
                }
            }
        }

        private void OnDestroy()
        {
            _container?.Dispose();
        }

        /// <summary>A point bone under <paramref name="parent"/>, in its XZ plane.</summary>
        private static Transform Child(Transform parent, float x, float z)
        {
            Transform child = new GameObject("Point").transform;
            child.SetParent(parent, false);
            child.localPosition = new Vector3(x, 0f, z);
            return child;
        }

        /// <summary>Hands out the bones of one row of exhibits, centered on the gallery, and a color for each.</summary>
        private sealed class Row
        {
            private readonly ShapeGallery _gallery;
            private readonly float _height;
            private readonly int _count;
            private readonly Quaternion _rotation;
            private int _next;

            internal Row(ShapeGallery gallery, int row, int count, Quaternion rotation)
            {
                _gallery = gallery;
                _height = row * gallery._spacing;
                _count = count;
                _rotation = rotation;
            }

            internal Color Color { get; private set; }

            internal Transform Next(string exhibitName)
            {
                Transform bone = new GameObject(exhibitName).transform;
                bone.SetParent(_gallery.transform, false);
                bone.localPosition = new Vector3((_next - (_count - 1) * 0.5f) * _gallery._spacing, _height, 0f);
                bone.localRotation = _rotation;
                _gallery._bones.Add(bone);
                _gallery._names.Add(exhibitName);
                Color = Color.HSVToRGB(_next / (float)_count, 0.6f, 1f);
                _next++;
                return bone;
            }
        }
    }
}
