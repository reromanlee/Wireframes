using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// Edits a glyph's points with the mouse and keyboard, over the glyph box, its grid and the pack's guides.
    /// </summary>
    /// <remarks>
    /// Click a point to select it, and drag it to move it, snapped to the grid unless Shift is held. Ctrl+click
    /// (Cmd+click on macOS) adds a point after the end of the selected stroke, or starts a stroke when none is selected.
    /// Double-click a line to insert a point there. Delete removes the selected point, the arrow keys nudge it by a grid
    /// step, the wheel zooms at the pointer, the middle button pans, and F frames the box.
    /// </remarks>
    internal sealed class GlyphCanvas : VisualElement
    {
        private const float Margin = 28f;
        private const float HitDistance = 7f;
        private const float HandleSize = 7f;
        private const float MinimumZoom = 0.25f;
        private const float MaximumZoom = 12f;

        private static readonly float[] UnitValues = { 0f, 0.25f, 0.5f, 0.75f, 1f };

        private readonly List<Label> _unitLabels = new();
        private readonly Label _baselineLabel = new("Baseline");
        private readonly Label _xHeightLabel = new("x-height");
        private readonly Label _capHeightLabel = new("Cap height");

        private GlyphPackEditing _editing;
        private GlyphReference _glyph = GlyphReference.None;
        private bool _isReadOnly;
        private float _zoom = 1f;
        private Vector2 _pan;

        private int _hoverStroke = -1;
        private int _hoverPoint = -1;
        private bool _isDraggingPoint;
        private bool _hasDragMoved;
        private int _dragUndoGroup;
        private bool _isPanning;
        private Vector2 _lastPointer;

        internal GlyphCanvas()
        {
            AddToClassList("glyph-canvas");
            focusable = true;
            generateVisualContent += Draw;
            for (int i = 0; i < UnitValues.Length * 2; i++)
            {
                Label label = new(UnitValues[i % UnitValues.Length].ToString("0.##"));
                label.AddToClassList("glyph-canvas__unit");
                label.pickingMode = PickingMode.Ignore;
                _unitLabels.Add(label);
                Add(label);
            }
            foreach (Label label in new[] { _baselineLabel, _xHeightLabel, _capHeightLabel })
            {
                label.AddToClassList("glyph-canvas__guide");
                label.pickingMode = PickingMode.Ignore;
                Add(label);
            }
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCaptureOutEvent>(_ => EndDrag());
            RegisterCallback<PointerLeaveEvent>(_ => SetHover(-1, -1));
            RegisterCallback<WheelEvent>(OnWheel);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<GeometryChangedEvent>(_ => PlaceLabels());
        }

        /// <summary>Raised after the canvas edits the glyph, so the views of the pack can follow.</summary>
        internal event Action Edited;

        /// <summary>Raised when another stroke or point is selected, or none.</summary>
        internal event Action SelectionChanged;

        internal int SelectedStroke { get; private set; } = -1;

        internal int SelectedPoint { get; private set; } = -1;

        /// <summary>True snaps dragged and added points to the pack's grid.</summary>
        internal bool Snaps { get; set; } = true;

        /// <summary>Where the glyph box is drawn, in the canvas's own coordinates.</summary>
        internal Rect BoxRect
        {
            get
            {
                Rect fitted = GlyphDrawing.FittedBox(contentRect, Margin);
                float size = fitted.width * _zoom;
                Vector2 center = fitted.center + _pan;
                return new Rect(center.x - size * 0.5f, center.y - size * 0.5f, size, size);
            }
        }

        private GlyphStroke[] Strokes
        {
            get => _editing != null && _glyph.IsIn(_editing.Pack) ? _glyph.StrokesIn(_editing.Pack) : Array.Empty<GlyphStroke>();
        }

        private int GridDivisions
        {
            get => _editing != null ? _editing.Pack.GridDivisions : WireframeGlyphPack.DefaultGridDivisions;
        }

        /// <summary>Shows <paramref name="glyph"/>, framed and with nothing selected when it's another glyph.</summary>
        internal void Show(GlyphPackEditing editing, GlyphReference glyph, bool isReadOnly)
        {
            bool isOtherGlyph = editing != _editing || !glyph.Equals(_glyph);
            _editing = editing;
            _glyph = glyph;
            _isReadOnly = isReadOnly;
            if (isOtherGlyph)
            {
                Frame();
                Select(-1, -1);
            }
            Refresh();
        }

        /// <summary>Redraws after the glyph changed, dropping a selection that no longer exists.</summary>
        internal void Refresh()
        {
            GlyphStroke[] strokes = Strokes;
            if (SelectedStroke >= strokes.Length
                || SelectedStroke >= 0 && SelectedPoint >= strokes[SelectedStroke].Points.Length)
            {
                Select(-1, -1);
            }
            MarkDirtyRepaint();
            PlaceLabels();
        }

        internal void Select(int stroke, int point)
        {
            if (stroke == SelectedStroke && point == SelectedPoint)
            {
                return;
            }
            SelectedStroke = stroke;
            SelectedPoint = stroke >= 0 ? point : -1;
            MarkDirtyRepaint();
            SelectionChanged?.Invoke();
        }

        /// <summary>Zooms and pans so the glyph box fills the canvas again.</summary>
        internal void Frame()
        {
            _zoom = 1f;
            _pan = Vector2.zero;
            MarkDirtyRepaint();
            PlaceLabels();
        }

        private void Draw(MeshGenerationContext context)
        {
            Rect box = BoxRect;
            if (box.width <= 0f || _editing == null)
            {
                return;
            }
            Painter2D painter = context.painter2D;
            Color text = resolvedStyle.color;
            WireframeGlyphPack pack = _editing.Pack;

            painter.fillColor = GlyphDrawing.Faded(text, 0.05f);
            painter.BeginPath();
            painter.MoveTo(box.min);
            painter.LineTo(new Vector2(box.xMax, box.yMin));
            painter.LineTo(box.max);
            painter.LineTo(new Vector2(box.xMin, box.yMax));
            painter.ClosePath();
            painter.Fill();

            int divisions = GridDivisions;
            for (int i = 1; i < divisions; i++)
            {
                float t = (float)i / divisions;
                Color grid = GlyphDrawing.Faded(text, i * 4 % divisions == 0 ? 0.18f : 0.09f);
                GlyphDrawing.DrawLine(painter, GlyphDrawing.ToBox(new Vector2(t, 0f), box),
                    GlyphDrawing.ToBox(new Vector2(t, 1f), box), grid, 1f);
                GlyphDrawing.DrawLine(painter, GlyphDrawing.ToBox(new Vector2(0f, t), box),
                    GlyphDrawing.ToBox(new Vector2(1f, t), box), grid, 1f);
            }
            GlyphDrawing.DrawRect(painter, box, GlyphDrawing.Faded(text, 0.45f), 1f);
            DrawGuide(painter, box, WireframeGlyphPack.Baseline, new Color(0.3f, 0.75f, 1f, 0.9f));
            DrawGuide(painter, box, pack.XHeight, new Color(0.45f, 0.85f, 0.45f, 0.6f));
            DrawGuide(painter, box, pack.CapHeight, new Color(1f, 0.6f, 0.3f, 0.6f));

            GlyphStroke[] strokes = Strokes;
            Color highlight = new(0.35f, 0.65f, 1f, 1f);
            for (int stroke = 0; stroke < strokes.Length; stroke++)
            {
                bool isSelected = stroke == SelectedStroke;
                painter.strokeColor = isSelected ? highlight : text;
                painter.lineWidth = isSelected ? 2.5f : 1.75f;
                painter.lineJoin = LineJoin.Round;
                painter.lineCap = LineCap.Round;
                GlyphDrawing.DrawStroke(painter, strokes[stroke], box);
            }
            for (int stroke = 0; stroke < strokes.Length; stroke++)
            {
                Vector2[] points = strokes[stroke].Points;
                for (int point = 0; point < points.Length; point++)
                {
                    DrawHandle(painter, GlyphDrawing.ToBox(points[point], box), point == 0,
                        stroke == SelectedStroke && point == SelectedPoint,
                        stroke == _hoverStroke && point == _hoverPoint, text, highlight);
                }
            }
        }

        private static void DrawGuide(Painter2D painter, Rect box, float height, Color color)
        {
            Vector2 from = GlyphDrawing.ToBox(new Vector2(0f, height), box);
            Vector2 to = GlyphDrawing.ToBox(new Vector2(1f, height), box);
            GlyphDrawing.DrawLine(painter, from, to, color, 1f);
        }

        /// <summary>A square for a point, a diamond for each stroke's first point, filled when selected.</summary>
        private static void DrawHandle(
            Painter2D painter, Vector2 center, bool isFirst, bool isSelected, bool isHovered, Color text, Color highlight)
        {
            float half = (isFirst ? HandleSize * 1.3f : HandleSize) * 0.5f;
            painter.BeginPath();
            if (isFirst)
            {
                painter.MoveTo(center + new Vector2(0f, -half));
                painter.LineTo(center + new Vector2(half, 0f));
                painter.LineTo(center + new Vector2(0f, half));
                painter.LineTo(center + new Vector2(-half, 0f));
            }
            else
            {
                painter.MoveTo(center + new Vector2(-half, -half));
                painter.LineTo(center + new Vector2(half, -half));
                painter.LineTo(center + new Vector2(half, half));
                painter.LineTo(center + new Vector2(-half, half));
            }
            painter.ClosePath();
            painter.fillColor = isSelected ? highlight : new Color(0.12f, 0.12f, 0.12f, 0.9f);
            painter.Fill();
            painter.strokeColor = isSelected || isHovered ? highlight : text;
            painter.lineWidth = isHovered ? 2f : 1f;
            painter.Stroke();
        }

        /// <summary>Puts the unit labels along the box's left and bottom edges, and the guide names at its right.</summary>
        private void PlaceLabels()
        {
            Rect box = BoxRect;
            bool isShown = box.width > 0f && _editing != null;
            for (int i = 0; i < _unitLabels.Count; i++)
            {
                Label label = _unitLabels[i];
                label.style.display = isShown ? DisplayStyle.Flex : DisplayStyle.None;
                float value = UnitValues[i % UnitValues.Length];
                Vector2 position = i < UnitValues.Length
                    ? GlyphDrawing.ToBox(new Vector2(0f, value), box) + new Vector2(-24f, -7f)
                    : GlyphDrawing.ToBox(new Vector2(value, 0f), box) + new Vector2(-10f, 3f);
                label.style.left = position.x;
                label.style.top = position.y;
            }
            if (_editing == null)
            {
                return;
            }
            PlaceGuideLabel(_baselineLabel, box, WireframeGlyphPack.Baseline, isShown);
            PlaceGuideLabel(_xHeightLabel, box, _editing.Pack.XHeight, isShown);
            PlaceGuideLabel(_capHeightLabel, box, _editing.Pack.CapHeight, isShown);
        }

        private static void PlaceGuideLabel(Label label, Rect box, float height, bool isShown)
        {
            label.style.display = isShown ? DisplayStyle.Flex : DisplayStyle.None;
            Vector2 position = GlyphDrawing.ToBox(new Vector2(1f, height), box);
            label.style.left = position.x + 4f;
            label.style.top = position.y - 8f;
        }

        private void OnPointerDown(PointerDownEvent pointerEvent)
        {
            Focus();
            Vector2 pointer = pointerEvent.localPosition;
            if (pointerEvent.button == 2 || pointerEvent.button == 0 && pointerEvent.altKey)
            {
                _isPanning = true;
                _lastPointer = pointer;
                this.CapturePointer(pointerEvent.pointerId);
                pointerEvent.StopPropagation();
                return;
            }
            if (pointerEvent.button != 0 || _editing == null || !_glyph.IsIn(_editing.Pack))
            {
                return;
            }
            pointerEvent.StopPropagation();
            if (TryFindPoint(pointer, out int stroke, out int point))
            {
                Select(stroke, point);
                if (!_isReadOnly)
                {
                    // One undo step for the whole drag, however many moves it makes.
                    Undo.IncrementCurrentGroup();
                    _dragUndoGroup = Undo.GetCurrentGroup();
                    _isDraggingPoint = true;
                    _hasDragMoved = false;
                    this.CapturePointer(pointerEvent.pointerId);
                }
                return;
            }
            if (_isReadOnly)
            {
                Select(-1, -1);
                return;
            }
            Vector2 position = ToGlyph(pointer, !pointerEvent.shiftKey);
            if (pointerEvent.clickCount == 2 && TryFindSegment(pointer, out stroke, out int insertAt))
            {
                _editing.InsertPoint(_glyph, stroke, insertAt, position);
                Select(stroke, insertAt);
                Edited?.Invoke();
                return;
            }
            if (pointerEvent.actionKey)
            {
                AddPoint(position);
                return;
            }
            Select(-1, -1);
        }

        private void OnPointerMove(PointerMoveEvent pointerEvent)
        {
            Vector2 pointer = pointerEvent.localPosition;
            if (_isPanning)
            {
                _pan += pointer - _lastPointer;
                _lastPointer = pointer;
                MarkDirtyRepaint();
                PlaceLabels();
                return;
            }
            if (_isDraggingPoint)
            {
                // Undo or an edit elsewhere can take the point away while it's dragged.
                if (!IsSelectionInGlyph())
                {
                    EndDrag();
                    return;
                }
                Vector2 position = ToGlyph(pointer, !pointerEvent.shiftKey);
                Vector2 current = Strokes[SelectedStroke].Points[SelectedPoint];
                if (position != current)
                {
                    _editing.MovePoint(_glyph, SelectedStroke, SelectedPoint, position, true);
                    _hasDragMoved = true;
                    Edited?.Invoke();
                }
                return;
            }
            if (TryFindPoint(pointer, out int stroke, out int point))
            {
                SetHover(stroke, point);
            }
            else
            {
                SetHover(-1, -1);
            }
        }

        private void OnPointerUp(PointerUpEvent pointerEvent)
        {
            if (this.HasPointerCapture(pointerEvent.pointerId))
            {
                this.ReleasePointer(pointerEvent.pointerId);
            }
            EndDrag();
        }

        private void EndDrag()
        {
            if (_isDraggingPoint && _hasDragMoved)
            {
                Undo.CollapseUndoOperations(_dragUndoGroup);
                Undo.SetCurrentGroupName("Move Point");
            }
            _isDraggingPoint = false;
            _hasDragMoved = false;
            _isPanning = false;
        }

        private void OnWheel(WheelEvent wheelEvent)
        {
            Vector2 pointer = wheelEvent.localMousePosition;
            Vector2 before = GlyphDrawing.FromBox(pointer, BoxRect);
            _zoom = Mathf.Clamp(_zoom * Mathf.Pow(1.1f, -wheelEvent.delta.y), MinimumZoom, MaximumZoom);
            // Keeps the glyph point under the pointer where it was.
            _pan += pointer - GlyphDrawing.ToBox(before, BoxRect);
            MarkDirtyRepaint();
            PlaceLabels();
            wheelEvent.StopPropagation();
        }

        private void OnKeyDown(KeyDownEvent keyEvent)
        {
            bool hasPoint = IsSelectionInGlyph();
            switch (keyEvent.keyCode)
            {
                case KeyCode.F:
                    Frame();
                    break;
                case KeyCode.Escape when hasPoint || SelectedStroke >= 0:
                    Select(-1, -1);
                    break;
                case KeyCode.Delete or KeyCode.Backspace when hasPoint && !_isReadOnly:
                    DeleteSelectedPoint();
                    break;
                case KeyCode.LeftArrow or KeyCode.RightArrow or KeyCode.UpArrow or KeyCode.DownArrow
                    when hasPoint && !_isReadOnly:
                    Nudge(keyEvent.keyCode);
                    break;
                default:
                    return;
            }
            keyEvent.StopPropagation();
        }

        /// <summary>Deletes the selected point and selects its neighbor, or nothing once its stroke is gone.</summary>
        private void DeleteSelectedPoint()
        {
            int stroke = SelectedStroke;
            int point = SelectedPoint;
            _editing.DeletePoint(_glyph, stroke, point);
            GlyphStroke[] strokes = Strokes;
            if (stroke < strokes.Length && strokes[stroke].Points.Length > 0)
            {
                Select(stroke, Mathf.Min(point, strokes[stroke].Points.Length - 1));
            }
            else
            {
                Select(-1, -1);
            }
            Edited?.Invoke();
        }

        /// <summary>Moves the selected point one grid step along an arrow's direction.</summary>
        private void Nudge(KeyCode arrow)
        {
            float step = 1f / GridDivisions;
            Vector2 direction = arrow switch
            {
                KeyCode.LeftArrow => Vector2.left,
                KeyCode.RightArrow => Vector2.right,
                KeyCode.UpArrow => Vector2.up,
                _ => Vector2.down
            };
            Vector2 current = Strokes[SelectedStroke].Points[SelectedPoint];
            Vector2 moved = GlyphGrid.Clamp(current + direction * step);
            if (moved != current)
            {
                _editing.MovePoint(_glyph, SelectedStroke, SelectedPoint, moved);
                Edited?.Invoke();
            }
        }

        /// <summary>Adds a point after the end of the selected stroke, or starts a stroke with it when none is selected.</summary>
        private void AddPoint(Vector2 position)
        {
            GlyphStroke[] strokes = Strokes;
            if (SelectedStroke >= 0 && SelectedStroke < strokes.Length)
            {
                int point = strokes[SelectedStroke].Points.Length;
                _editing.InsertPoint(_glyph, SelectedStroke, point, position);
                Select(SelectedStroke, point);
            }
            else
            {
                int stroke = _editing.AddStroke(_glyph, position);
                Select(stroke, 0);
            }
            Edited?.Invoke();
        }

        /// <summary>The glyph-box point at <paramref name="pointer"/>, in the box, and on the grid when snapping.</summary>
        private Vector2 ToGlyph(Vector2 pointer, bool allowsSnap)
        {
            Vector2 point = GlyphDrawing.FromBox(pointer, BoxRect);
            return Snaps && allowsSnap ? GlyphGrid.Snap(point, GridDivisions) : GlyphGrid.Clamp(point);
        }

        /// <summary>The point nearest to <paramref name="pointer"/> within reach, preferring the selected stroke's.</summary>
        private bool TryFindPoint(Vector2 pointer, out int foundStroke, out int foundPoint)
        {
            foundStroke = -1;
            foundPoint = -1;
            float nearest = HitDistance;
            GlyphStroke[] strokes = Strokes;
            Rect box = BoxRect;
            for (int stroke = 0; stroke < strokes.Length; stroke++)
            {
                Vector2[] points = strokes[stroke].Points;
                for (int point = 0; point < points.Length; point++)
                {
                    float distance = Vector2.Distance(pointer, GlyphDrawing.ToBox(points[point], box));
                    // Equal distances go to the selected stroke, so overlapping points stay reachable.
                    if (distance < nearest || distance <= nearest && stroke == SelectedStroke)
                    {
                        nearest = distance;
                        foundStroke = stroke;
                        foundPoint = point;
                    }
                }
            }
            return foundStroke >= 0;
        }

        /// <summary>The line nearest to <paramref name="pointer"/> within reach, and the index a point inserted there takes.</summary>
        private bool TryFindSegment(Vector2 pointer, out int foundStroke, out int insertAt)
        {
            foundStroke = -1;
            insertAt = -1;
            float nearest = HitDistance;
            GlyphStroke[] strokes = Strokes;
            Rect box = BoxRect;
            for (int stroke = 0; stroke < strokes.Length; stroke++)
            {
                Vector2[] points = strokes[stroke].Points;
                int segmentCount = strokes[stroke].IsClosed && points.Length > 2 ? points.Length : points.Length - 1;
                for (int segment = 0; segment < segmentCount; segment++)
                {
                    Vector2 from = GlyphDrawing.ToBox(points[segment], box);
                    Vector2 to = GlyphDrawing.ToBox(points[(segment + 1) % points.Length], box);
                    float distance = DistanceToSegment(pointer, from, to);
                    if (distance < nearest)
                    {
                        nearest = distance;
                        foundStroke = stroke;
                        insertAt = segment + 1;
                    }
                }
            }
            return foundStroke >= 0;
        }

        private bool IsSelectionInGlyph()
        {
            GlyphStroke[] strokes = Strokes;
            return SelectedStroke >= 0 && SelectedStroke < strokes.Length && SelectedPoint >= 0
                   && SelectedPoint < strokes[SelectedStroke].Points.Length;
        }

        private void SetHover(int stroke, int point)
        {
            if (stroke == _hoverStroke && point == _hoverPoint)
            {
                return;
            }
            _hoverStroke = stroke;
            _hoverPoint = point;
            MarkDirtyRepaint();
        }

        private static float DistanceToSegment(Vector2 point, Vector2 from, Vector2 to)
        {
            Vector2 segment = to - from;
            float lengthSquared = segment.sqrMagnitude;
            float t = lengthSquared > 0f ? Mathf.Clamp01(Vector2.Dot(point - from, segment) / lengthSquared) : 0f;
            return Vector2.Distance(point, from + segment * t);
        }
    }
}
