using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>A glyph in the gallery: its preview, its name below, and for a character its code.</summary>
    internal sealed class GlyphTile : VisualElement
    {
        internal const string SelectedClass = "glyph-tile--selected";
        internal const string DraggedClass = "glyph-tile--dragged";

        private readonly GlyphPreview _preview = new();
        private readonly Label _name = new();
        private readonly Label _code = new();

        internal GlyphTile()
        {
            AddToClassList("glyph-tile");
            _name.AddToClassList("glyph-tile__name");
            _code.AddToClassList("glyph-tile__code");
            Add(_preview);
            Add(_name);
            Add(_code);
        }

        internal GlyphReference Glyph { get; private set; }

        /// <summary>The text the gallery's search matches: the character and its code, or the keyword.</summary>
        internal string SearchText { get; private set; }

        internal void Show(GlyphReference glyph, WireframeGlyphPack pack)
        {
            Glyph = glyph;
            _preview.Strokes = glyph.StrokesIn(pack);
            if (glyph.IsSymbol)
            {
                string keyword = pack.Symbols[glyph.Index].Keyword;
                _name.text = keyword;
                _code.style.display = DisplayStyle.None;
                SearchText = keyword;
            }
            else
            {
                int character = pack.Characters[glyph.Index].Character;
                _name.text = GlyphNames.CharacterOf(character);
                _code.text = GlyphNames.CodeOf(character);
                _code.style.display = DisplayStyle.Flex;
                SearchText = $"{_name.text} {_code.text}";
            }
            tooltip = _name.text;
        }

        internal void SetSize(float size)
        {
            style.width = size;
            _preview.style.height = size;
        }
    }
}
