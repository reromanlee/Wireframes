using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a pyramid on its GameObject, with its tip at <see cref="WireframeAxialShape.EndA"/> and the center of its
    /// rectangular base at <see cref="WireframeAxialShape.EndB"/>, like a camera's view without its near plane.
    /// </summary>
    [AddComponentMenu("Wireframes/Pyramid")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframePyramid : WireframeAxialShape
    {
        [Tooltip("Width of the base along the pyramid's X axis and height along its Y axis, in local units.")]
        [SerializeField] private Vector2 _baseSize = Vector2.one * ShapeDefaults.Size;

        /// <summary>
        /// Width of the base along the pyramid's X axis and height along its Y axis, in the GameObject's local units. 1 by
        /// 1 by default.
        /// </summary>
        public Vector2 BaseSize
        {
            get => _baseSize;
            set
            {
                _baseSize = value;
                if (TryGetShape(out Pyramid pyramid))
                {
                    pyramid.BaseSize = value;
                }
            }
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new Pyramid(bone, EndA, AxisRotation, AxisLength, _baseSize);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            ((Pyramid)shape).BaseSize = _baseSize;
        }
    }
}
