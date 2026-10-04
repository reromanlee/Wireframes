using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>How a text lays out its characters, shared by <see cref="IText"/> and <see cref="WireframeText"/>.</summary>
    internal struct TextSettings
    {
        internal const float DefaultCharacterSpacing = 0.1f;
        internal const float DefaultLineSpacing = 0f;

        internal static readonly Vector2 DefaultBounds = new(4f, 1f);

        internal WireframeCharacterWidth CharacterWidth;

        /// <summary>Height of a glyph box in local units.</summary>
        internal float CharacterSize;

        /// <summary>Extra room after each character, in glyph boxes.</summary>
        internal float CharacterSpacing;

        /// <summary>Extra room between lines, in glyph boxes.</summary>
        internal float LineSpacing;

        /// <summary>Width and height of the rectangle the text is aligned in, centered on the shape, in local units.</summary>
        internal Vector2 Bounds;

        internal WireframeHorizontalAlignment HorizontalAlignment;
        internal WireframeVerticalAlignment VerticalAlignment;
        internal WireframeTextOverflow Overflow;

        /// <summary>
        /// Proportional characters 1 unit tall, centered on the shape in 4 by 1 bounds, with lines kept as written.
        /// </summary>
        internal static TextSettings Default
        {
            get => new()
            {
                CharacterWidth = WireframeCharacterWidth.Proportional,
                CharacterSize = ShapeDefaults.Size,
                CharacterSpacing = DefaultCharacterSpacing,
                LineSpacing = DefaultLineSpacing,
                Bounds = DefaultBounds,
                HorizontalAlignment = WireframeHorizontalAlignment.Center,
                VerticalAlignment = WireframeVerticalAlignment.Middle,
                Overflow = WireframeTextOverflow.Overflow
            };
        }
    }
}
