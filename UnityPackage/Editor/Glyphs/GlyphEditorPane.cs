using System;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>The open glyph: its name and a button that closes it, above the canvas that edits it.</summary>
    internal sealed class GlyphEditorPane : VisualElement
    {
        private readonly Label _title = new();
        private readonly Label _code = new();

        internal GlyphEditorPane()
        {
            AddToClassList("glyph-editor-pane");
            VisualElement header = new();
            header.AddToClassList("glyph-editor-pane__header");
            _title.AddToClassList("glyph-editor-pane__title");
            _code.AddToClassList("glyph-editor-pane__code");
            Button close = new(() => CloseClicked?.Invoke()) { text = "×", tooltip = "Close the glyph (Esc)." };
            close.AddToClassList("glyph-editor-pane__close");
            header.Add(_title);
            header.Add(_code);
            header.Add(new VisualElement { style = { flexGrow = 1f } });
            header.Add(close);
            Add(header);
            Body = new VisualElement();
            Body.AddToClassList("glyph-editor-pane__body");
            Add(Body);
        }

        internal event Action CloseClicked;

        /// <summary>Where the canvas and the glyph's details go, below the header.</summary>
        internal VisualElement Body { get; }

        internal GlyphPackEditing Editing { get; private set; }

        internal GlyphReference Glyph { get; private set; } = GlyphReference.None;

        internal bool IsReadOnly { get; private set; }

        /// <summary>Shows <paramref name="glyph"/> of the pack that <paramref name="editing"/> edits.</summary>
        internal void Show(GlyphPackEditing editing, GlyphReference glyph, bool isReadOnly)
        {
            Editing = editing;
            Glyph = glyph;
            IsReadOnly = isReadOnly;
            Refresh();
        }

        /// <summary>Brings the pane in line with the pack after an edit or undo.</summary>
        internal void Refresh()
        {
            if (Editing == null || !Glyph.IsIn(Editing.Pack))
            {
                return;
            }
            WireframeGlyphPack pack = Editing.Pack;
            if (Glyph.IsSymbol)
            {
                _title.text = pack.Symbols[Glyph.Index].Keyword;
                _code.text = "Symbol";
            }
            else
            {
                int character = pack.Characters[Glyph.Index].Character;
                _title.text = GlyphNames.CharacterOf(character);
                _code.text = GlyphNames.CodeOf(character);
            }
        }
    }
}
