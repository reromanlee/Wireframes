using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws text on its GameObject with the glyphs of a <see cref="WireframeGlyphs"/>. It stands in the XY plane of its
    /// rotation and reads from the -Z side, laid out in <see cref="Bounds"/> centered on
    /// <see cref="WireframeCenteredShape.Center"/>. Characters the glyphs lack are drawn as '?', with one warning each.
    /// </summary>
    [AddComponentMenu("Wireframes/Text")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeText : WireframeCenteredShape, ISerializationCallbackReceiver
    {
        private const string DefaultText = "Text";

        [Tooltip("Characters drawn. A new line starts a new line of text, and a tab takes 4 spaces.")]
        [TextArea(2, 8)]
        [SerializeField] private string _text = DefaultText;

        [Tooltip("Glyphs to draw with. Empty draws with the package's Default Glyphs.")]
        [SerializeField] private WireframeGlyphs _glyphs;

        [Tooltip("Proportional gives each character the width of its lines; Monospace gives every character a whole glyph "
                 + "box, so columns line up.")]
        [SerializeField] private WireframeCharacterWidth _characterWidth = WireframeCharacterWidth.Proportional;

        [Tooltip("Height of a glyph box, in the GameObject's local units.")]
        [SerializeField] private float _characterSize = ShapeDefaults.Size;

        [Tooltip("Extra room after each character, in glyph boxes.")]
        [SerializeField] private float _characterSpacing = TextSettings.DefaultCharacterSpacing;

        [Tooltip("Extra room between lines, in glyph boxes.")]
        [SerializeField] private float _lineSpacing = TextSettings.DefaultLineSpacing;

        [Tooltip("Width and height of the rectangle the text is aligned in, centered on Center, in the GameObject's local "
                 + "units. Selecting the component outlines it in the Scene view.")]
        [SerializeField] private Vector2 _bounds = TextSettings.DefaultBounds;

        [Tooltip("Where each line sits across the bounds.")]
        [SerializeField] private WireframeHorizontalAlignment _horizontalAlignment = WireframeHorizontalAlignment.Center;

        [Tooltip("Where the lines sit between the top and the bottom of the bounds.")]
        [SerializeField] private WireframeVerticalAlignment _verticalAlignment = WireframeVerticalAlignment.Middle;

        [Tooltip("What lines wider than the bounds do: run past them, or wrap onto more lines.")]
        [SerializeField] private WireframeTextOverflow _overflow = WireframeTextOverflow.Overflow;

        // Text set with SetText, which becomes the saved string only once something reads or saves it.
        [NonSerialized] private char[] _spanText;
        [NonSerialized] private int _spanLength;
        [NonSerialized] private bool _hasSpanText;

        /// <summary>
        /// Characters drawn, with <c>\n</c> starting a new line. "Text" by default. Reading it after
        /// <see cref="SetText"/> creates the string once.
        /// </summary>
        public string Text
        {
            get
            {
                SaveSpanText();
                return _text;
            }
            set
            {
                _text = value ?? string.Empty;
                _hasSpanText = false;
                if (TryGetShape(out GlyphText text))
                {
                    text.Text = _text;
                }
            }
        }

        /// <summary>
        /// Glyphs to draw with. A new component in the Editor gets <see cref="WireframeGlyphs.Default"/>, which null
        /// draws with too.
        /// </summary>
        public WireframeGlyphs Glyphs
        {
            get => _glyphs;
            set
            {
                _glyphs = value;
                if (TryGetShape(out GlyphText text))
                {
                    text.Glyphs = value;
                }
            }
        }

        /// <summary>How far each character moves the next one along. Proportional by default.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value isn't a <see cref="WireframeCharacterWidth"/>.</exception>
        public WireframeCharacterWidth CharacterWidth
        {
            get => _characterWidth;
            set
            {
                if (!TextSettings.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), value, $"{nameof(CharacterWidth)} isn't a {nameof(WireframeCharacterWidth)} value.");
                }
                _characterWidth = value;
                if (TryGetShape(out GlyphText text))
                {
                    text.CharacterWidth = value;
                }
            }
        }

        /// <summary>Height of a glyph box, in the GameObject's local units. 1 by default.</summary>
        public float CharacterSize
        {
            get => _characterSize;
            set
            {
                _characterSize = value;
                if (TryGetShape(out GlyphText text))
                {
                    text.CharacterSize = value;
                }
            }
        }

        /// <summary>Extra room after each character, in glyph boxes. 0.1 by default.</summary>
        public float CharacterSpacing
        {
            get => _characterSpacing;
            set
            {
                _characterSpacing = value;
                if (TryGetShape(out GlyphText text))
                {
                    text.CharacterSpacing = value;
                }
            }
        }

        /// <summary>Extra room between lines, in glyph boxes. 0 by default.</summary>
        public float LineSpacing
        {
            get => _lineSpacing;
            set
            {
                _lineSpacing = value;
                if (TryGetShape(out GlyphText text))
                {
                    text.LineSpacing = value;
                }
            }
        }

        /// <summary>
        /// Width and height of the rectangle the text is aligned in, centered on
        /// <see cref="WireframeCenteredShape.Center"/>, in the GameObject's local units. 4 by 1 by default.
        /// </summary>
        public Vector2 Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;
                if (TryGetShape(out GlyphText text))
                {
                    text.Bounds = value;
                }
            }
        }

        /// <summary>Where each line sits across the bounds. Centered by default.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value isn't a <see cref="WireframeHorizontalAlignment"/>.</exception>
        public WireframeHorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                if (!TextSettings.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), value,
                        $"{nameof(HorizontalAlignment)} isn't a {nameof(WireframeHorizontalAlignment)} value.");
                }
                _horizontalAlignment = value;
                if (TryGetShape(out GlyphText text))
                {
                    text.HorizontalAlignment = value;
                }
            }
        }

        /// <summary>Where the lines sit between the top and the bottom of the bounds. Centered by default.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value isn't a <see cref="WireframeVerticalAlignment"/>.</exception>
        public WireframeVerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                if (!TextSettings.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), value,
                        $"{nameof(VerticalAlignment)} isn't a {nameof(WireframeVerticalAlignment)} value.");
                }
                _verticalAlignment = value;
                if (TryGetShape(out GlyphText text))
                {
                    text.VerticalAlignment = value;
                }
            }
        }

        /// <summary>What lines wider than the bounds do: run past them, the default, or wrap onto more lines.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value isn't a <see cref="WireframeTextOverflow"/>.</exception>
        public WireframeTextOverflow Overflow
        {
            get => _overflow;
            set
            {
                if (!TextSettings.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), value, $"{nameof(Overflow)} isn't a {nameof(WireframeTextOverflow)} value.");
                }
                _overflow = value;
                if (TryGetShape(out GlyphText text))
                {
                    text.Overflow = value;
                }
            }
        }

        private TextSettings Settings
        {
            get => new()
            {
                CharacterWidth = _characterWidth,
                CharacterSize = _characterSize,
                CharacterSpacing = _characterSpacing,
                LineSpacing = _lineSpacing,
                Bounds = _bounds,
                HorizontalAlignment = _horizontalAlignment,
                VerticalAlignment = _verticalAlignment,
                Overflow = _overflow
            };
        }

        private ReadOnlySpan<char> CurrentText
        {
            get => _hasSpanText ? new ReadOnlySpan<char>(_spanText, 0, _spanLength) : (_text ?? string.Empty).AsSpan();
        }

        /// <summary>
        /// Sets the characters drawn without allocating, such as a number written into a <c>stackalloc</c> buffer with
        /// <c>TryFormat</c>. They become the saved <see cref="Text"/> only once something reads or saves it.
        /// </summary>
        public void SetText(ReadOnlySpan<char> text)
        {
            if (text.SequenceEqual(CurrentText))
            {
                return;
            }
            if (_spanText == null || _spanText.Length < text.Length)
            {
                _spanText = new char[Math.Max(text.Length, (_spanText?.Length ?? 8) * 2)];
            }
            text.CopyTo(_spanText);
            _spanLength = text.Length;
            _hasSpanText = true;
            if (TryGetShape(out GlyphText shape))
            {
                shape.SetText(text);
            }
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new GlyphText(bone, Center, Rotation, CurrentText, _glyphs, Settings) { WarningContext = this };
        }

        internal override void ApplySizesTo(Shape shape)
        {
            ((GlyphText)shape).Apply(CurrentText, _glyphs, Settings);
        }

        internal override bool CanApplyInPlace(Shape shape)
        {
            // Other text or glyphs may need more room than the shape has.
            return ((GlyphText)shape).Shows(CurrentText, _glyphs);
        }

        internal override void Sanitize()
        {
            _text ??= string.Empty;
            if (!TextSettings.IsDefined(_characterWidth))
            {
                _characterWidth = WireframeCharacterWidth.Proportional;
            }
            if (!TextSettings.IsDefined(_horizontalAlignment))
            {
                _horizontalAlignment = WireframeHorizontalAlignment.Center;
            }
            if (!TextSettings.IsDefined(_verticalAlignment))
            {
                _verticalAlignment = WireframeVerticalAlignment.Middle;
            }
            if (!TextSettings.IsDefined(_overflow))
            {
                _overflow = WireframeTextOverflow.Overflow;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            SaveSpanText();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // The saved string is the text from now on, as after an edit in the Inspector or an undo.
            _hasSpanText = false;
        }

        private void SaveSpanText()
        {
            if (_hasSpanText)
            {
                _text = new string(_spanText, 0, _spanLength);
                _hasSpanText = false;
            }
        }

        private void Reset()
        {
            _glyphs = WireframeGlyphs.Default;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(Center, Rotation, Vector3.one);
            Gizmos.color = new Color(Color.r, Color.g, Color.b, Color.a * 0.4f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(_bounds.x, _bounds.y, 0f));
        }
#endif
    }
}
