using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class GlyphText : GlyphShape, IText
    {
        private char[] _characters;
        private int _length;
        // Created on first read after SetText, and kept until the text changes.
        private string _text;
        private TextSettings _settings;

        internal GlyphText(
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation,
            ReadOnlySpan<char> text,
            WireframeGlyphs glyphs,
            in TextSettings settings)
            : base(LaidOut(text, glyphs, settings), glyphs, bone, localPosition, localRotation)
        {
            _settings = settings;
            _characters = new char[Math.Max(16, text.Length)];
            text.CopyTo(_characters);
            _length = text.Length;
        }

        public string Text
        {
            get
            {
                EnsureUsable();
                return _text ??= new string(_characters, 0, _length);
            }
            set
            {
                value ??= string.Empty;
                SetText(value);
                _text = value;
            }
        }

        public WireframeCharacterWidth CharacterWidth
        {
            get
            {
                EnsureUsable();
                return _settings.CharacterWidth;
            }
            set
            {
                EnsureUsable();
                if (!TextSettings.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), value, $"{nameof(CharacterWidth)} isn't a {nameof(WireframeCharacterWidth)} value.");
                }
                if (value != _settings.CharacterWidth)
                {
                    _settings.CharacterWidth = value;
                    Rebuild(false);
                }
            }
        }

        public float CharacterSize
        {
            get
            {
                EnsureUsable();
                return _settings.CharacterSize;
            }
            set
            {
                EnsureUsable();
                if (value != _settings.CharacterSize)
                {
                    _settings.CharacterSize = value;
                    Rebuild(false);
                }
            }
        }

        public float CharacterSpacing
        {
            get
            {
                EnsureUsable();
                return _settings.CharacterSpacing;
            }
            set
            {
                EnsureUsable();
                if (value != _settings.CharacterSpacing)
                {
                    _settings.CharacterSpacing = value;
                    Rebuild(false);
                }
            }
        }

        public float LineSpacing
        {
            get
            {
                EnsureUsable();
                return _settings.LineSpacing;
            }
            set
            {
                EnsureUsable();
                if (value != _settings.LineSpacing)
                {
                    _settings.LineSpacing = value;
                    Rebuild(false);
                }
            }
        }

        public Vector2 Bounds
        {
            get
            {
                EnsureUsable();
                return _settings.Bounds;
            }
            set
            {
                EnsureUsable();
                if (value != _settings.Bounds)
                {
                    _settings.Bounds = value;
                    Rebuild(false);
                }
            }
        }

        public WireframeHorizontalAlignment HorizontalAlignment
        {
            get
            {
                EnsureUsable();
                return _settings.HorizontalAlignment;
            }
            set
            {
                EnsureUsable();
                if (!TextSettings.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), value,
                        $"{nameof(HorizontalAlignment)} isn't a {nameof(WireframeHorizontalAlignment)} value.");
                }
                if (value != _settings.HorizontalAlignment)
                {
                    _settings.HorizontalAlignment = value;
                    Rebuild(false);
                }
            }
        }

        public WireframeVerticalAlignment VerticalAlignment
        {
            get
            {
                EnsureUsable();
                return _settings.VerticalAlignment;
            }
            set
            {
                EnsureUsable();
                if (!TextSettings.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), value,
                        $"{nameof(VerticalAlignment)} isn't a {nameof(WireframeVerticalAlignment)} value.");
                }
                if (value != _settings.VerticalAlignment)
                {
                    _settings.VerticalAlignment = value;
                    Rebuild(false);
                }
            }
        }

        public WireframeTextOverflow Overflow
        {
            get
            {
                EnsureUsable();
                return _settings.Overflow;
            }
            set
            {
                EnsureUsable();
                if (!TextSettings.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value), value, $"{nameof(Overflow)} isn't a {nameof(WireframeTextOverflow)} value.");
                }
                if (value != _settings.Overflow)
                {
                    _settings.Overflow = value;
                    Rebuild(false);
                }
            }
        }

        public void SetText(ReadOnlySpan<char> text)
        {
            EnsureUsable();
            if (text.SequenceEqual(new ReadOnlySpan<char>(_characters, 0, _length)))
            {
                return;
            }
            CopyText(text);
            Rebuild(true);
        }

        /// <summary>
        /// Sets the text, the glyphs and every setting at once and lays them out once, for components, which write all
        /// their fields together. Settings are taken as they are, already checked.
        /// </summary>
        internal void Apply(ReadOnlySpan<char> text, WireframeGlyphs glyphs, in TextSettings settings)
        {
            EnsureUsable();
            bool glyphsChanged = ReplaceGlyphs(glyphs);
            if (!text.SequenceEqual(new ReadOnlySpan<char>(_characters, 0, _length)))
            {
                CopyText(text);
                glyphsChanged = true;
            }
            _settings = settings;
            Rebuild(glyphsChanged);
        }

        protected override void Layout(GlyphGeometry geometry, WireframeGlyphs glyphs)
        {
            TextLayout.Layout(new ReadOnlySpan<char>(_characters, 0, _length), glyphs, _settings, geometry, WarningContext);
        }

        protected override void ScaleSizes(float factor)
        {
            _settings.CharacterSize *= factor;
            _settings.Bounds *= factor;
            Rebuild(false);
        }

        private void CopyText(ReadOnlySpan<char> text)
        {
            if (text.Length > _characters.Length)
            {
                _characters = new char[Math.Max(text.Length, _characters.Length * 2)];
            }
            text.CopyTo(_characters);
            _length = text.Length;
            _text = null;
        }

        private static GlyphGeometry LaidOut(ReadOnlySpan<char> text, WireframeGlyphs glyphs, in TextSettings settings)
        {
            GlyphGeometry geometry = new();
            TextLayout.Layout(text, DrawnGlyphsOf(glyphs), settings, geometry, null);
            return geometry;
        }
    }
}
