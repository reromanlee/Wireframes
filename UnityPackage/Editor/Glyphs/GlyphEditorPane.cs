using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// The open glyph: its name, snapping and grid settings and a button that closes it, above the canvas that edits it.
    /// </summary>
    internal sealed class GlyphEditorPane : VisualElement
    {
        private readonly Label _title = new();
        private readonly Label _code = new();
        private readonly Toggle _snap = new("Snap") { value = true, tooltip = "Snap points to the grid. Hold Shift to move them freely." };
        private readonly SliderInt _grid = new("Grid", WireframeGlyphPack.MinimumGridDivisions, WireframeGlyphPack.MaximumGridDivisions)
        {
            showInputField = true, tooltip = "Divisions of the grid across the glyph box, saved with the pack."
        };
        private readonly GlyphCanvas _canvas = new();

        internal GlyphEditorPane()
        {
            AddToClassList("glyph-editor-pane");
            VisualElement header = new();
            header.AddToClassList("glyph-editor-pane__header");
            _title.AddToClassList("glyph-editor-pane__title");
            _code.AddToClassList("glyph-editor-pane__code");
            _snap.AddToClassList("glyph-editor-pane__snap");
            _snap.RegisterValueChangedCallback(change => _canvas.Snaps = change.newValue);
            _grid.AddToClassList("glyph-editor-pane__grid");
            _grid.bindingPath = WireframeGlyphPack.GridDivisionsField;
            Button frame = new(_canvas.Frame) { text = "Frame", tooltip = "Fit the glyph box to the canvas (F)." };
            Button close = new(() => CloseClicked?.Invoke()) { text = "×", tooltip = "Close the glyph (Esc)." };
            close.AddToClassList("glyph-editor-pane__close");
            header.Add(_title);
            header.Add(_code);
            header.Add(new VisualElement { style = { flexGrow = 1f } });
            header.Add(_snap);
            header.Add(_grid);
            header.Add(frame);
            header.Add(close);
            Add(header);

            Body = new VisualElement();
            Body.AddToClassList("glyph-editor-pane__body");
            Body.Add(_canvas);
            Add(Body);
            _canvas.Edited += () => Edited?.Invoke();
        }

        internal event Action CloseClicked;

        /// <summary>Raised after the pane edits the glyph.</summary>
        internal event Action Edited;

        /// <summary>Where the canvas and the glyph's details go, below the header.</summary>
        internal VisualElement Body { get; }

        internal GlyphCanvas Canvas
        {
            get => _canvas;
        }

        internal GlyphPackEditing Editing { get; private set; }

        internal GlyphReference Glyph { get; private set; } = GlyphReference.None;

        internal bool IsReadOnly { get; private set; }

        /// <summary>Shows <paramref name="glyph"/> of the pack that <paramref name="editing"/> edits.</summary>
        internal void Show(GlyphPackEditing editing, GlyphReference glyph, bool isReadOnly)
        {
            if (editing != Editing)
            {
                _grid.Bind(editing.Serialized);
            }
            Editing = editing;
            Glyph = glyph;
            IsReadOnly = isReadOnly;
            _grid.SetEnabled(!isReadOnly);
            _canvas.Show(editing, glyph, isReadOnly);
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
            _canvas.Refresh();
        }
    }
}
