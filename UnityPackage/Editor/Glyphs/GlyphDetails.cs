using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// The open glyph in numbers: its character or keyword, the extent of its ink, buttons that center it, and each stroke
    /// with its points as X and Y fields, kept within the glyph box. Selecting a point here selects it on the canvas, and
    /// the other way around.
    /// </summary>
    internal sealed class GlyphDetails : ScrollView
    {
        private const string SelectedRowClass = "glyph-details__point--selected";

        private readonly TextField _key = new() { isDelayed = true };
        private readonly Label _keyProblem = new();
        private readonly Label _ink = new();
        private readonly Button _centerHorizontally;
        private readonly Button _centerVertically;
        private readonly Button _addStroke;
        private readonly VisualElement _strokes = new();
        private readonly List<List<PointRow>> _pointRows = new();
        private readonly StringBuilder _structure = new();

        private GlyphPackEditing _editing;
        private GlyphReference _glyph = GlyphReference.None;
        private GlyphCanvas _canvas;
        private bool _isReadOnly;
        private string _shownStructure;

        internal GlyphDetails()
        {
            AddToClassList("glyph-details");
            _key.AddToClassList("glyph-details__key");
            _key.RegisterValueChangedCallback(change => ChangeKey(change.newValue));
            _keyProblem.AddToClassList("glyph-details__problem");
            _ink.AddToClassList("glyph-details__ink");
            _centerHorizontally = new Button(() => Center(true)) { text = "Center Horizontally" };
            _centerVertically = new Button(() => Center(false)) { text = "Center Vertically", tooltip = "Suits symbols; characters should stay on the baseline." };
            _addStroke = new Button(AddStroke) { text = "Add Stroke", tooltip = "Starts a stroke with one point in the middle of the box." };
            VisualElement centering = new();
            centering.AddToClassList("glyph-details__row");
            centering.Add(_centerHorizontally);
            centering.Add(_centerVertically);
            Add(_key);
            Add(_keyProblem);
            Add(_ink);
            Add(centering);
            Add(_strokes);
            Add(_addStroke);
        }

        /// <summary>Raised after the panel edits the glyph.</summary>
        internal event Action Edited;

        /// <summary>Raised with the glyph's new place after its character changed or its symbol was renamed.</summary>
        internal event Action<GlyphReference> GlyphMoved;

        internal void Show(GlyphPackEditing editing, GlyphReference glyph, bool isReadOnly, GlyphCanvas canvas)
        {
            if (_canvas != canvas)
            {
                if (_canvas != null)
                {
                    _canvas.SelectionChanged -= ShowSelection;
                }
                canvas.SelectionChanged += ShowSelection;
            }
            bool isOtherGlyph = editing != _editing || !glyph.Equals(_glyph);
            _editing = editing;
            _glyph = glyph;
            _canvas = canvas;
            _isReadOnly = isReadOnly;
            if (isOtherGlyph)
            {
                _shownStructure = null;
                _keyProblem.text = string.Empty;
            }
            Refresh();
        }

        /// <summary>Shows the glyph as it is now, rebuilding the rows only when strokes or points were added or removed.</summary>
        internal void Refresh()
        {
            if (_editing == null || !_glyph.IsIn(_editing.Pack))
            {
                return;
            }
            WireframeGlyphPack pack = _editing.Pack;
            _key.SetValueWithoutNotify(_glyph.IsSymbol
                ? pack.Symbols[_glyph.Index].Keyword
                : CharacterText(pack.Characters[_glyph.Index].Character));
            _key.label = _glyph.IsSymbol ? "Keyword" : "Character";
            _key.tooltip = _glyph.IsSymbol
                ? "The symbol's name, which its enum member takes. Renaming it renames the member when the enum is generated again."
                : "The character the glyph draws, typed as itself or as its code, such as U+0041.";
            GlyphStroke[] strokes = _glyph.StrokesIn(pack);
            GlyphMetrics metrics = GlyphMetrics.Measure(strokes);
            _ink.text = metrics.HasInk
                ? $"Ink from {metrics.InkLeft:0.###} to {metrics.InkRight:0.###}, {metrics.InkWidth:0.###} wide"
                : "No lines are drawn yet.";
            SetEnabled(!_isReadOnly);
            string structure = StructureOf(strokes);
            if (structure != _shownStructure)
            {
                _shownStructure = structure;
                Rebuild(strokes);
            }
            else
            {
                UpdateValues(strokes);
            }
            ShowSelection();
        }

        private void Rebuild(GlyphStroke[] strokes)
        {
            _strokes.Clear();
            _pointRows.Clear();
            for (int stroke = 0; stroke < strokes.Length; stroke++)
            {
                _strokes.Add(CreateStroke(strokes, stroke));
            }
        }

        private VisualElement CreateStroke(GlyphStroke[] strokes, int stroke)
        {
            GlyphStroke data = strokes[stroke];
            Foldout foldout = new() { value = true, text = StrokeTitle(stroke, data) };
            foldout.AddToClassList("glyph-details__stroke");

            VisualElement controls = new();
            controls.AddToClassList("glyph-details__row");
            Toggle closed = new("Closed") { value = data.IsClosed, tooltip = "Join the last point back to the first." };
            closed.RegisterValueChangedCallback(change => Run(() => _editing.SetClosed(_glyph, stroke, change.newValue)));
            controls.Add(closed);
            controls.Add(new VisualElement { style = { flexGrow = 1f } });
            controls.Add(SmallButton("↑", "Draw this stroke earlier.", () => Run(() => _editing.MoveStroke(_glyph, stroke, stroke - 1)), stroke > 0));
            controls.Add(SmallButton("↓", "Draw this stroke later.", () => Run(() => _editing.MoveStroke(_glyph, stroke, stroke + 1)), stroke < strokes.Length - 1));
            controls.Add(SmallButton("×", "Delete the stroke.", () =>
            {
                _canvas.Select(-1, -1);
                Run(() => _editing.DeleteStroke(_glyph, stroke));
            }, true));
            foldout.Add(controls);

            List<PointRow> rows = new();
            Vector2[] points = data.Points;
            for (int point = 0; point < points.Length; point++)
            {
                PointRow row = CreatePointRow(stroke, point, points.Length);
                row.X.SetValueWithoutNotify(points[point].x);
                row.Y.SetValueWithoutNotify(points[point].y);
                rows.Add(row);
                foldout.Add(row.Element);
            }
            _pointRows.Add(rows);
            Button addPoint = new(() => AddPoint(stroke)) { text = "Add Point", tooltip = "Adds a point after the last one." };
            addPoint.AddToClassList("glyph-details__add-point");
            foldout.Add(addPoint);
            return foldout;
        }

        private PointRow CreatePointRow(int stroke, int point, int pointCount)
        {
            VisualElement row = new();
            row.AddToClassList("glyph-details__point");
            Label index = new((point + 1).ToString());
            index.AddToClassList("glyph-details__index");
            FloatField x = new("X") { isDelayed = true };
            FloatField y = new("Y") { isDelayed = true };
            x.AddToClassList("glyph-details__coordinate");
            y.AddToClassList("glyph-details__coordinate");
            x.RegisterValueChangedCallback(change => MovePoint(stroke, point, change.newValue, null));
            y.RegisterValueChangedCallback(change => MovePoint(stroke, point, null, change.newValue));
            row.Add(index);
            row.Add(x);
            row.Add(y);
            row.Add(SmallButton("↑", "Move the point earlier in the stroke.",
                () => Run(() => _editing.MovePointInOrder(_glyph, stroke, point, point - 1)), point > 0));
            row.Add(SmallButton("↓", "Move the point later in the stroke.",
                () => Run(() => _editing.MovePointInOrder(_glyph, stroke, point, point + 1)), point < pointCount - 1));
            row.Add(SmallButton("×", "Delete the point.", () =>
            {
                _canvas.Select(-1, -1);
                Run(() => _editing.DeletePoint(_glyph, stroke, point));
            }, true));
            row.RegisterCallback<PointerDownEvent>(_ => _canvas.Select(stroke, point));
            return new PointRow(row, x, y);
        }

        private void UpdateValues(GlyphStroke[] strokes)
        {
            for (int stroke = 0; stroke < strokes.Length && stroke < _pointRows.Count; stroke++)
            {
                Foldout foldout = _strokes[stroke] as Foldout;
                if (foldout != null)
                {
                    foldout.text = StrokeTitle(stroke, strokes[stroke]);
                }
                Vector2[] points = strokes[stroke].Points;
                List<PointRow> rows = _pointRows[stroke];
                for (int point = 0; point < points.Length && point < rows.Count; point++)
                {
                    rows[point].X.SetValueWithoutNotify(points[point].x);
                    rows[point].Y.SetValueWithoutNotify(points[point].y);
                }
            }
        }

        /// <summary>Highlights the row of the point selected on the canvas.</summary>
        private void ShowSelection()
        {
            for (int stroke = 0; stroke < _pointRows.Count; stroke++)
            {
                List<PointRow> rows = _pointRows[stroke];
                for (int point = 0; point < rows.Count; point++)
                {
                    rows[point].Element.EnableInClassList(SelectedRowClass,
                        _canvas != null && stroke == _canvas.SelectedStroke && point == _canvas.SelectedPoint);
                }
            }
        }

        private void MovePoint(int stroke, int point, float? x, float? y)
        {
            GlyphStroke[] strokes = _glyph.StrokesIn(_editing.Pack);
            if (stroke >= strokes.Length || point >= strokes[stroke].Points.Length)
            {
                return;
            }
            Vector2 current = strokes[stroke].Points[point];
            Vector2 moved = GlyphGrid.Clamp(new Vector2(x ?? current.x, y ?? current.y));
            Run(() => _editing.MovePoint(_glyph, stroke, point, moved));
            _canvas.Select(stroke, point);
        }

        private void AddPoint(int stroke)
        {
            Vector2[] points = _glyph.StrokesIn(_editing.Pack)[stroke].Points;
            Vector2 position = points.Length > 0 ? points[points.Length - 1] : new Vector2(0.5f, 0.5f);
            Run(() => _editing.InsertPoint(_glyph, stroke, points.Length, position));
            _canvas.Select(stroke, points.Length);
        }

        private void AddStroke()
        {
            int stroke = -1;
            Run(() => stroke = _editing.AddStroke(_glyph, new Vector2(0.5f, 0.5f)));
            _canvas.Select(stroke, 0);
        }

        private void Center(bool isHorizontal)
        {
            Run(() => _editing.Center(_glyph, isHorizontal));
        }

        /// <summary>Takes a new character or keyword for the glyph, or says what's wrong with it and keeps the old one.</summary>
        private void ChangeKey(string text)
        {
            WireframeGlyphPack pack = _editing.Pack;
            string problem;
            GlyphReference moved = _glyph;
            if (_glyph.IsSymbol)
            {
                string keyword = text.Trim();
                if (keyword == pack.Symbols[_glyph.Index].Keyword)
                {
                    return;
                }
                problem = SymbolKeys.FindProblem(keyword) ?? (_editing.FindSymbol(keyword).IsNone ? null : $"The pack already has {keyword}.");
                if (problem == null)
                {
                    _editing.Rename(_glyph, keyword);
                }
            }
            else
            {
                problem = GlyphNames.Parse(text, out int character);
                if (problem == null && character == pack.Characters[_glyph.Index].Character)
                {
                    return;
                }
                if (problem == null && !_editing.FindCharacter(character).IsNone)
                {
                    problem = $"The pack already has {GlyphNames.CharacterOf(character)}.";
                }
                if (problem == null)
                {
                    moved = _editing.ChangeCharacter(_glyph, character);
                }
            }
            _keyProblem.text = problem ?? string.Empty;
            if (problem != null)
            {
                Refresh();
                return;
            }
            _glyph = moved;
            GlyphMoved?.Invoke(moved);
        }

        private void Run(Action edit)
        {
            if (_isReadOnly)
            {
                return;
            }
            edit();
            Refresh();
            Edited?.Invoke();
        }

        /// <summary>A signature of the strokes' point counts and closed flags, which changes when rows must be rebuilt.</summary>
        private string StructureOf(GlyphStroke[] strokes)
        {
            _structure.Clear();
            foreach (GlyphStroke stroke in strokes)
            {
                _structure.Append(stroke.Points.Length).Append(stroke.IsClosed ? 'c' : 'o');
            }
            return _structure.ToString();
        }

        private static string StrokeTitle(int stroke, GlyphStroke data)
        {
            int count = data.Points.Length;
            string points = count == 1 ? "1 point" : $"{count} points";
            string closed = data.IsClosed ? ", closed" : string.Empty;
            string warning = data.IsDrawn ? string.Empty : $"  ⚠ needs {(data.IsClosed ? 3 : 2)} points to draw";
            return $"Stroke {stroke + 1}: {points}{closed}{warning}";
        }

        private static string CharacterText(int character)
        {
            return character == ' ' ? "U+0020" : GlyphNames.CharacterOf(character);
        }

        /// <summary>A point's row and its coordinate fields.</summary>
        private readonly struct PointRow
        {
            internal readonly VisualElement Element;
            internal readonly FloatField X;
            internal readonly FloatField Y;

            internal PointRow(VisualElement element, FloatField x, FloatField y)
            {
                Element = element;
                X = x;
                Y = y;
            }
        }

        private static Button SmallButton(string text, string tooltip, Action clicked, bool isEnabled)
        {
            Button button = new(clicked) { text = text, tooltip = tooltip };
            button.AddToClassList("glyph-details__small-button");
            button.SetEnabled(isEnabled);
            return button;
        }
    }
}
