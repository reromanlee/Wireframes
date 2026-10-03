using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a flat rectangle on its GameObject, centered on <see cref="WireframeCenteredShape.Center"/> in the XZ plane of
    /// its rotation.
    /// </summary>
    [AddComponentMenu("Wireframes/Rectangle")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeRectangle : WireframeCenteredShape
    {
        [Tooltip("Width along the rectangle's X axis and depth along its Z axis, in the GameObject's local units.")]
        [SerializeField] private Vector2 _size = Vector2.one * ShapeDefaults.Size;

        /// <summary>
        /// Width along the rectangle's X axis and depth along its Z axis, in the GameObject's local units. 1 by 1 by
        /// default.
        /// </summary>
        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                if (TryGetShape(out Rectangle rectangle))
                {
                    rectangle.Size = value;
                }
            }
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new Rectangle(bone, Center, Rotation, _size);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            ((Rectangle)shape).Size = _size;
        }
    }
}
