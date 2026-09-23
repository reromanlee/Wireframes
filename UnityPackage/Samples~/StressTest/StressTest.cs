using UnityEngine;

namespace reromanlee.Wireframes.Samples
{
    /// <summary>
    /// Spawns thousands of shapes on orbiting bones. Only the bones move each frame; lines are edited only when
    /// <see cref="_recolorPerFrame"/> is above zero, so the Profiler shows what the package itself costs.
    /// Add it to an empty GameObject in a scene with a camera and enter Play mode.
    /// </summary>
    public sealed class StressTest : MonoBehaviour
    {
        [SerializeField, Min(0)] private int _lineCount = 10000;
        [SerializeField, Min(0)] private int _boxCount = 1000;
        [Tooltip("Circles, ellipses, stars, rectangles, rounded rectangles and stadiums, in turn.")]
        [SerializeField, Min(0)] private int _flatShapeCount = 1000;
        [Tooltip("Spheres, ellipsoids and spiked spheres, in turn.")]
        [SerializeField, Min(0)] private int _centeredShapeCount = 500;
        [Tooltip("Cylinders, cones, capsules, frustums and pyramids, in turn.")]
        [SerializeField, Min(0)] private int _longShapeCount = 500;
        [SerializeField, Min(1)] private int _boneCount = 100;
        [SerializeField, Min(0f)] private float _radius = 25f;
        [Tooltip("Degrees per second each bone orbits the center.")]
        [SerializeField, Min(0f)] private float _orbitSpeed = 20f;
        [Tooltip("Lines recolored every frame, to measure the cost of edits.")]
        [SerializeField, Min(0)] private int _recolorPerFrame;

        private LineContainer _container;
        private Transform[] _bones;
        private Vector3[] _orbitAxes;
        private ILine[] _lines;
        private int _recolorCursor;
        private float _smoothedFrameTime;

        private void Start()
        {
            _container = new LineContainer();

            _bones = new Transform[_boneCount];
            _orbitAxes = new Vector3[_boneCount];
            for (int i = 0; i < _boneCount; i++)
            {
                Transform bone = new GameObject($"Bone {i}").transform;
                bone.SetParent(transform, false);
                bone.localPosition = Random.insideUnitSphere * _radius;
                bone.localRotation = Random.rotation;
                _bones[i] = bone;
                _orbitAxes[i] = Random.onUnitSphere;
            }

            // Set the bone first, then the position in the space you mean: here, offsets around each bone.
            _lines = new ILine[_lineCount];
            for (int i = 0; i < _lineCount; i++)
            {
                ILine line = _container.CreateLine();
                line.BoneA = RandomBone();
                line.LocalPositionA = Random.insideUnitSphere;
                line.BoneB = RandomBone();
                line.LocalPositionB = Random.insideUnitSphere;
                line.ColorA = Color.HSVToRGB(Random.value, 0.8f, 1f);
                line.ColorB = Color.HSVToRGB(Random.value, 0.8f, 1f);
                _lines[i] = line;
            }

            for (int i = 0; i < _boxCount; i++)
            {
                IBox box = _container.CreateBox();
                box.Bone = RandomBone();
                Vector3 center = Random.insideUnitSphere * 2f;
                Vector3 extents = Vector3.one * Random.Range(0.1f, 0.5f);
                box.LocalCornerA = center - extents;
                box.LocalCornerB = center + extents;
                box.SetColor(Color.HSVToRGB(Random.value, 0.5f, 1f));
            }

            // The other shapes are created on their bone directly, at a random offset, size and turn.
            for (int i = 0; i < _flatShapeCount; i++)
            {
                Randomize(CreateFlatShape(i, RandomBone(), Random.insideUnitSphere * 2f, Random.Range(0.1f, 0.5f)));
            }
            for (int i = 0; i < _centeredShapeCount; i++)
            {
                Randomize(CreateCenteredShape(i, RandomBone(), Random.insideUnitSphere * 2f, Random.Range(0.1f, 0.5f)));
            }
            for (int i = 0; i < _longShapeCount; i++)
            {
                Randomize(CreateLongShape(i, RandomBone(), Random.insideUnitSphere * 2f, Random.Range(0.1f, 0.5f)));
            }
        }

        private void Update()
        {
            // Moving the bones is all it takes to animate every shape; the package does no work for it.
            float angle = _orbitSpeed * Time.deltaTime;
            for (int i = 0; i < _bones.Length; i++)
            {
                _bones[i].RotateAround(transform.position, _orbitAxes[i], angle);
            }

            if (_lines.Length > 0)
            {
                Color color = Color.HSVToRGB(Time.time * 0.1f % 1f, 0.8f, 1f);
                for (int i = 0; i < _recolorPerFrame; i++)
                {
                    _lines[_recolorCursor].SetColor(color);
                    _recolorCursor = (_recolorCursor + 1) % _lines.Length;
                }
            }

            _smoothedFrameTime = Mathf.Lerp(_smoothedFrameTime, Time.unscaledDeltaTime, 0.05f);
        }

        private void OnGUI()
        {
            int otherShapes = _flatShapeCount + _centeredShapeCount + _longShapeCount;
            GUILayout.Label(
                $"{_lineCount} lines, {_boxCount} boxes, {otherShapes} other shapes, {_boneCount} bones | " +
                $"{_smoothedFrameTime * 1000f:F2} ms per frame");
        }

        private void OnDestroy()
        {
            _container?.Dispose();
        }

        private IRigidShape CreateFlatShape(int index, Transform bone, Vector3 center, float size)
        {
            Vector3 corner = new(size, 0f, size * 0.6f);
            Vector3 axis = new(0f, 0f, size);
            switch (index % 6)
            {
                case 0:
                    return _container.CreateCircle(bone, center, size);
                case 1:
                    return _container.CreateEllipse(bone, center - axis, center + axis, size * 0.5f);
                case 2:
                    return _container.CreateStar(bone, center, size * 0.4f, size, 5);
                case 3:
                    return _container.CreateRectangle(bone, center - corner, center + corner);
                case 4:
                    return _container.CreateRoundedRectangle(bone, center - corner, center + corner, size * 0.3f);
                default:
                    return _container.CreateStadium(bone, center - axis, center + axis, size * 0.5f, size * 0.3f);
            }
        }

        private IRigidShape CreateCenteredShape(int index, Transform bone, Vector3 center, float size)
        {
            Vector3 axis = new(0f, 0f, size);
            switch (index % 3)
            {
                case 0:
                    return _container.CreateSphere(bone, center, size);
                case 1:
                    return _container.CreateEllipsoid(bone, center - axis, center + axis, size * 0.5f);
                default:
                    return _container.CreateSpikedSphere(bone, center, size * 0.5f, size * 0.5f, 12);
            }
        }

        private IRigidShape CreateLongShape(int index, Transform bone, Vector3 endA, float size)
        {
            Vector3 endB = endA + new Vector3(0f, 0f, size * 3f);
            switch (index % 5)
            {
                case 0:
                    return _container.CreateCylinder(bone, endA, endB, size);
                case 1:
                    return _container.CreateCone(bone, endA, endB, size);
                case 2:
                    return _container.CreateCapsule(bone, endA, endB, size, size * 0.6f);
                case 3:
                    return _container.CreateFrustum(bone, endA, endB, size * 0.5f, size, 6);
                default:
                    return _container.CreatePyramid(bone, endA, endB, new Vector2(size * 2f, size * 1.5f));
            }
        }

        private Transform RandomBone()
        {
            return _bones[Random.Range(0, _bones.Length)];
        }

        private static void Randomize(IRigidShape shape)
        {
            shape.LocalRotation = Random.rotation;
            shape.Color = Color.HSVToRGB(Random.value, 0.6f, 1f);
        }
    }
}
