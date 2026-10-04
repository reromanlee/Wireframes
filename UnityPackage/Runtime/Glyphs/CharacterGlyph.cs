using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>A glyph that draws a character of text, found by the character's Unicode code point.</summary>
    [Serializable]
    internal struct CharacterGlyph
    {
        internal const string CharacterField = nameof(_character);
        internal const string StrokesField = nameof(_strokes);

        [SerializeField] private int _character;
        [SerializeField] private GlyphStroke[] _strokes;

        internal CharacterGlyph(int character, GlyphStroke[] strokes)
        {
            _character = character;
            _strokes = strokes;
        }

        /// <summary>Unicode code point of the character, such as 65 for A.</summary>
        internal int Character
        {
            get => _character;
        }

        /// <summary>The glyph's lines; never null.</summary>
        internal GlyphStroke[] Strokes
        {
            get => _strokes ?? Array.Empty<GlyphStroke>();
        }
    }
}
