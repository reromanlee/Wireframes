using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>How a text lays out its characters, shared by <see cref="IText"/> and <see cref="WireframeText"/>.</summary>
    internal struct TextSettings : IEquatable<TextSettings>
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

        public bool Equals(TextSettings other)
        {
            return CharacterWidth == other.CharacterWidth && CharacterSize.Equals(other.CharacterSize)
                   && CharacterSpacing.Equals(other.CharacterSpacing) && LineSpacing.Equals(other.LineSpacing)
                   && Bounds.Equals(other.Bounds) && HorizontalAlignment == other.HorizontalAlignment
                   && VerticalAlignment == other.VerticalAlignment && Overflow == other.Overflow;
        }

        public override bool Equals(object other)
        {
            return other is TextSettings settings && Equals(settings);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CharacterSize, CharacterSpacing, LineSpacing, Bounds, (int)CharacterWidth,
                (int)HorizontalAlignment, (int)VerticalAlignment, (int)Overflow);
        }

        internal static bool IsDefined(WireframeCharacterWidth width)
        {
            return width is WireframeCharacterWidth.Proportional or WireframeCharacterWidth.Monospace;
        }

        internal static bool IsDefined(WireframeHorizontalAlignment alignment)
        {
            return alignment is >= WireframeHorizontalAlignment.Left and <= WireframeHorizontalAlignment.Right;
        }

        internal static bool IsDefined(WireframeVerticalAlignment alignment)
        {
            return alignment is >= WireframeVerticalAlignment.Top and <= WireframeVerticalAlignment.Bottom;
        }

        internal static bool IsDefined(WireframeTextOverflow overflow)
        {
            return overflow is WireframeTextOverflow.Overflow or WireframeTextOverflow.Wrap;
        }
    }
}
