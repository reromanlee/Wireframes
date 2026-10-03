using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of the long shape components: cylinders, cones, capsules, stadiums, frustums and pyramids. The shape runs from
    /// <see cref="EndA"/> to <see cref="EndB"/>, both relative to its GameObject, and turns around that axis by
    /// <see cref="Roll"/>. With no roll, the shape's +Y stays as close to the GameObject's up as the axis allows, as when
    /// a long shape is created from two points.
    /// </summary>
    public abstract class WireframeAxialShape : WireframeShape
    {
        [Tooltip("Where the shape's axis starts, relative to the GameObject: the tip of a cone or pyramid.")]
        [SerializeField] private Vector3 _endA;

        [Tooltip("Where the shape's axis ends, relative to the GameObject: the center of a cone's or pyramid's base.")]
        [SerializeField] private Vector3 _endB = new(0f, 0f, ShapeDefaults.Size);

        [Tooltip("Turn of the shape around its axis, in degrees.")]
        [SerializeField] private float _roll;

        private protected WireframeAxialShape()
        {
        }

        /// <summary>
        /// Where the shape's axis starts, relative to the GameObject, in its local units: the tip of a cone or pyramid,
        /// and the center of a capsule's or stadium's first circle. The GameObject's origin by default.
        /// </summary>
        public Vector3 EndA
        {
            get => _endA;
            set
            {
                _endA = value;
                if (TryGetShape(out AxialShape shape))
                {
                    ApplyAxis(shape);
                }
            }
        }

        /// <summary>
        /// Where the shape's axis ends, relative to the GameObject, in its local units: the center of a cone's or
        /// pyramid's base, and of a capsule's or stadium's second circle. 1 unit along +Z by default.
        /// </summary>
        public Vector3 EndB
        {
            get => _endB;
            set
            {
                _endB = value;
                if (TryGetShape(out AxialShape shape))
                {
                    ApplyAxis(shape);
                }
            }
        }

        /// <summary>
        /// Turn of the shape around its axis, in degrees. With no roll, the shape's +Y stays as close to the GameObject's
        /// up as the axis allows. 0 by default.
        /// </summary>
        public float Roll
        {
            get => _roll;
            set
            {
                _roll = value;
                if (TryGetShape(out AxialShape shape))
                {
                    shape.LocalRotation = AxisRotation;
                }
            }
        }

        /// <summary>Rotation that points the shape's +Z from end A to end B, then turns it by the roll.</summary>
        private protected Quaternion AxisRotation
        {
            get => ShapeRotations.FromAxis(_endB - _endA, Vector3.up) * Quaternion.AngleAxis(_roll, Vector3.forward);
        }

        private protected float AxisLength
        {
            get => Vector3.Distance(_endA, _endB);
        }

        internal sealed override void ApplyTo(Shape shape)
        {
            ApplyAxis((AxialShape)shape);
            ApplySizesTo(shape);
        }

        /// <summary>Writes the shape's own fields, such as its radii, into <paramref name="shape"/>.</summary>
        internal abstract void ApplySizesTo(Shape shape);

        private void ApplyAxis(AxialShape shape)
        {
            shape.LocalPosition = _endA;
            shape.LocalRotation = AxisRotation;
            shape.Length = AxisLength;
        }
    }
}
