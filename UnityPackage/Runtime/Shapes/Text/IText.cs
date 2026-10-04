using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Text drawn with the glyphs of a <see cref="WireframeGlyphs"/>. It lies in the XY plane of its rotation and reads
    /// from the -Z side, so with no rotation it faces a camera looking along +Z. Its lines are laid out in
    /// <see cref="Bounds"/>, centered on its position: each line by <see cref="HorizontalAlignment"/>, and all of them
    /// together by <see cref="VerticalAlignment"/>. Characters the glyphs lack are drawn as '?', with one warning each.
    /// Changing the text allocates nothing once its buffers have grown to fit, so it can change every frame.
    /// </summary>
    public interface IText : IRigidShape
    {
        /// <summary>
        /// The characters drawn, with <c>\n</c> starting a new line, a tab taking 4 spaces, and other control characters
        /// skipped. Reading it after <see cref="SetText"/> creates the string once.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// The glyphs to draw with, or null, the default, for <see cref="WireframeGlyphs.Default"/>.
        /// </summary>
        WireframeGlyphs Glyphs { get; set; }

        /// <summary>
        /// How far each character moves the next one along. <see cref="WireframeCharacterWidth.Proportional"/> by
        /// default.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value isn't a <see cref="WireframeCharacterWidth"/>.</exception>
        WireframeCharacterWidth CharacterWidth { get; set; }

        /// <summary>Height of a glyph box in the bone's units. 1 by default.</summary>
        float CharacterSize { get; set; }

        /// <summary>Extra room after each character, in glyph boxes. 0.1 by default.</summary>
        float CharacterSpacing { get; set; }

        /// <summary>Extra room between lines, in glyph boxes. 0 by default, so lines are one glyph box apart.</summary>
        float LineSpacing { get; set; }

        /// <summary>
        /// Width and height of the rectangle the text is aligned in, centered on its position, in the bone's units. 4 by
        /// 1 by default.
        /// </summary>
        Vector2 Bounds { get; set; }

        /// <summary>Where each line sits across the bounds. Centered by default.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value isn't a <see cref="WireframeHorizontalAlignment"/>.</exception>
        WireframeHorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>Where the lines sit between the top and the bottom of the bounds. Centered by default.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value isn't a <see cref="WireframeVerticalAlignment"/>.</exception>
        WireframeVerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// What lines wider than the bounds do: run past them, the default, or wrap onto more lines.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value isn't a <see cref="WireframeTextOverflow"/>.</exception>
        WireframeTextOverflow Overflow { get; set; }

        /// <summary>
        /// Sets the characters drawn without allocating, such as a number written into a <c>stackalloc</c> buffer with
        /// <c>TryFormat</c>. Text equal to the current one changes nothing.
        /// </summary>
        void SetText(ReadOnlySpan<char> text);
    }
}
