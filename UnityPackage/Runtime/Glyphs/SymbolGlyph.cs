using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A glyph that draws a symbol, found by its keyword. The keyword names the symbol's member in the pack's generated
    /// enum, and lookups go by its hash, the member's value, so they never compare strings.
    /// </summary>
    [Serializable]
    internal struct SymbolGlyph
    {
        internal const string KeywordField = nameof(_keyword);
        internal const string StrokesField = nameof(_strokes);

        [SerializeField] private string _keyword;
        [SerializeField] private GlyphStroke[] _strokes;

        internal SymbolGlyph(string keyword, GlyphStroke[] strokes)
        {
            _keyword = keyword;
            _strokes = strokes;
        }

        /// <summary>Name of the symbol, a C# identifier such as ArrowUp; never null.</summary>
        internal string Keyword
        {
            get => _keyword ?? string.Empty;
        }

        /// <summary>The glyph's lines; never null.</summary>
        internal GlyphStroke[] Strokes
        {
            get => _strokes ?? Array.Empty<GlyphStroke>();
        }
    }
}
