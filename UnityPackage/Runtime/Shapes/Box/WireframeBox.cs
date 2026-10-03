using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a box on its GameObject, centered on <see cref="WireframeCenteredShape.Center"/>, like a BoxCollider.
    /// </summary>
    [AddComponentMenu("Wireframes/Box")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeBox : WireframeCenteredShape
    {
        [Tooltip("Size along the box's own axes, in the GameObject's local units.")]
        [SerializeField] private Vector3 _size = Vector3.one * ShapeDefaults.Size;

        /// <summary>Size along the box's own axes, in the GameObject's local units. 1 by 1 by 1 by default.</summary>
        public Vector3 Size
        {
            get => _size;
            set
            {
                _size = value;
                if (TryGetShape(out Box box))
                {
                    box.Size = value;
                }
            }
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new Box(bone, Center, Rotation, _size);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            ((Box)shape).Size = _size;
        }
    }
}
