using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of the shape components centered on a point: boxes, rectangles, circles, ellipses, stars, spheres,
    /// ellipsoids and spiked spheres. <see cref="Center"/> and <see cref="Rotation"/> place the shape relative to its
    /// GameObject, and flat shapes lie in the XZ plane of their rotation, so with no rotation they lie flat on it.
    /// </summary>
    public abstract class WireframeCenteredShape : WireframeShape
    {
        [Tooltip("Center of the shape relative to the GameObject.")]
        [SerializeField] private Vector3 _center;

        [Tooltip("Rotation of the shape relative to the GameObject, in Euler angles.")]
        [SerializeField] private Vector3 _rotation;

        private protected WireframeCenteredShape()
        {
        }

        /// <summary>Center of the shape relative to the GameObject, in its local units.</summary>
        public Vector3 Center
        {
            get => _center;
            set
            {
                _center = value;
                if (TryGetShape(out RigidShape shape))
                {
                    shape.LocalPosition = value;
                }
            }
        }

        /// <summary>
        /// Rotation of the shape relative to the GameObject. It is kept as the Euler angles of <see cref="EulerAngles"/>,
        /// and a zero quaternion such as <c>default</c> means no rotation.
        /// </summary>
        public Quaternion Rotation
        {
            get => Quaternion.Euler(_rotation);
            set => EulerAngles = Quaternion.Normalize(value).eulerAngles;
        }

        /// <summary>
        /// Rotation of the shape relative to the GameObject in Euler angles, in degrees, as the Inspector shows it.
        /// </summary>
        public Vector3 EulerAngles
        {
            get => _rotation;
            set
            {
                _rotation = value;
                if (TryGetShape(out RigidShape shape))
                {
                    shape.LocalRotation = Quaternion.Euler(value);
                }
            }
        }

        internal sealed override void ApplyTo(Shape shape)
        {
            RigidShape rigid = (RigidShape)shape;
            rigid.LocalPosition = _center;
            rigid.LocalRotation = Quaternion.Euler(_rotation);
            ApplySizesTo(shape);
        }

        /// <summary>Writes the shape's own fields, such as its sizes, into <paramref name="shape"/>.</summary>
        internal abstract void ApplySizesTo(Shape shape);
    }
}
