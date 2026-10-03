using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a line on its GameObject between two points, each following its own bone, or the GameObject when it has none.
    /// By default it runs from the GameObject's origin to 1 unit along its +Z.
    /// </summary>
    [AddComponentMenu("Wireframes/Line")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeLine : WireframePointShape
    {
        [Tooltip("Transform that point A follows. None follows the GameObject.")]
        [SerializeField] private Transform _boneA;

        [Tooltip("Point A relative to its bone, or to the GameObject without one.")]
        [SerializeField] private Vector3 _positionA;

        [Tooltip("Transform that point B follows. None follows the GameObject.")]
        [SerializeField] private Transform _boneB;

        [Tooltip("Point B relative to its bone, or to the GameObject without one.")]
        [SerializeField] private Vector3 _positionB = new(0f, 0f, ShapeDefaults.Size);

        /// <summary>
        /// Transform that point A follows, or null to follow the GameObject. A destroyed bone counts as none, and so does
        /// one that isn't in a scene, such as a prefab asset.
        /// </summary>
        public Transform BoneA
        {
            get => _boneA;
            set
            {
                _boneA = value;
                if (TryGetShape(out Line line))
                {
                    line.BoneA = ResolveBone(value);
                    line.LocalPositionA = _positionA;
                }
            }
        }

        /// <summary>
        /// Point A relative to <see cref="BoneA"/>, or to the GameObject without one. The origin by default.
        /// </summary>
        public Vector3 PositionA
        {
            get => _positionA;
            set
            {
                _positionA = value;
                if (TryGetShape(out Line line))
                {
                    line.LocalPositionA = value;
                }
            }
        }

        /// <summary>
        /// Transform that point B follows, or null to follow the GameObject. A destroyed bone counts as none, and so does
        /// one that isn't in a scene, such as a prefab asset.
        /// </summary>
        public Transform BoneB
        {
            get => _boneB;
            set
            {
                _boneB = value;
                if (TryGetShape(out Line line))
                {
                    line.BoneB = ResolveBone(value);
                    line.LocalPositionB = _positionB;
                }
            }
        }

        /// <summary>
        /// Point B relative to <see cref="BoneB"/>, or to the GameObject without one. 1 unit along +Z by default.
        /// </summary>
        public Vector3 PositionB
        {
            get => _positionB;
            set
            {
                _positionB = value;
                if (TryGetShape(out Line line))
                {
                    line.LocalPositionB = value;
                }
            }
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new Line(ResolveBone(_boneA), ResolveBone(_boneB));
        }

        internal override void ApplyTo(Shape shape)
        {
            // Each bone goes first, since changing it keeps the point's world position rather than its local one.
            Line line = (Line)shape;
            line.BoneA = ResolveBone(_boneA);
            line.LocalPositionA = _positionA;
            line.BoneB = ResolveBone(_boneB);
            line.LocalPositionB = _positionB;
        }
    }
}
