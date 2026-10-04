using System;
using UnityEditor;
using UnityEngine;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// Every edit the Glyph Editor makes to a pack, each one undo step, made through a <see cref="SerializedObject"/>, so
    /// undo, the Inspector and the texts using the pack all follow. Characters stay sorted by code, and symbols keep the
    /// order they are given, which is the order of their enum's members.
    /// </summary>
    internal sealed class GlyphPackEditing
    {
        private const int FirstPrintableAscii = 0x20;
        private const int LastPrintableAscii = 0x7E;

        private readonly SerializedObject _serialized;

        internal GlyphPackEditing(WireframeGlyphPack pack)
        {
            Pack = pack;
            _serialized = new SerializedObject(pack);
        }

        internal WireframeGlyphPack Pack { get; }

        internal SerializedObject Serialized
        {
            get => _serialized;
        }

        /// <summary>The glyph's entry in the pack's list of characters or symbols.</summary>
        internal SerializedProperty GlyphProperty(GlyphReference glyph)
        {
            return ListOf(glyph.IsSymbol).GetArrayElementAtIndex(glyph.Index);
        }

        internal SerializedProperty StrokesProperty(GlyphReference glyph)
        {
            string field = glyph.IsSymbol ? SymbolGlyph.StrokesField : CharacterGlyph.StrokesField;
            return GlyphProperty(glyph).FindPropertyRelative(field);
        }

        internal SerializedProperty PointsProperty(GlyphReference glyph, int stroke)
        {
            return StrokesProperty(glyph).GetArrayElementAtIndex(stroke).FindPropertyRelative(GlyphStroke.PointsField);
        }

        /// <summary>The character glyph of <paramref name="character"/>, or <see cref="GlyphReference.None"/>.</summary>
        internal GlyphReference FindCharacter(int character)
        {
            CharacterGlyph[] characters = Pack.Characters;
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i].Character == character)
                {
                    return new GlyphReference(false, i);
                }
            }
            return GlyphReference.None;
        }

        /// <summary>The symbol named <paramref name="keyword"/>, or <see cref="GlyphReference.None"/>.</summary>
        internal GlyphReference FindSymbol(string keyword)
        {
            SymbolGlyph[] symbols = Pack.Symbols;
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i].Keyword == keyword)
                {
                    return new GlyphReference(true, i);
                }
            }
            return GlyphReference.None;
        }

        /// <summary>Adds an empty glyph for <paramref name="character"/> where its code sorts it.</summary>
        internal GlyphReference AddCharacter(int character)
        {
            int index = SortedIndexOf(character);
            Edit("Add Glyph", () =>
            {
                SerializedProperty characters = ListOf(false);
                characters.InsertArrayElementAtIndex(index);
                SerializedProperty glyph = characters.GetArrayElementAtIndex(index);
                glyph.FindPropertyRelative(CharacterGlyph.CharacterField).intValue = character;
                glyph.FindPropertyRelative(CharacterGlyph.StrokesField).arraySize = 0;
            });
            return new GlyphReference(false, index);
        }

        /// <summary>Adds an empty symbol named <paramref name="keyword"/> after the others.</summary>
        internal GlyphReference AddSymbol(string keyword)
        {
            int index = Pack.Symbols.Length;
            Edit("Add Symbol", () =>
            {
                SerializedProperty symbols = ListOf(true);
                symbols.arraySize = index + 1;
                SerializedProperty glyph = symbols.GetArrayElementAtIndex(index);
                glyph.FindPropertyRelative(SymbolGlyph.KeywordField).stringValue = keyword;
                glyph.FindPropertyRelative(SymbolGlyph.StrokesField).arraySize = 0;
            });
            return new GlyphReference(true, index);
        }

        /// <summary>Adds an empty glyph for every printable ASCII character the pack lacks, and returns how many.</summary>
        internal int AddMissingAscii()
        {
            int added = 0;
            Edit("Add Missing ASCII", () =>
            {
                SerializedProperty characters = ListOf(false);
                for (int character = FirstPrintableAscii; character <= LastPrintableAscii; character++)
                {
                    if (!FindCharacterIn(characters, character, out int index))
                    {
                        characters.InsertArrayElementAtIndex(index);
                        SerializedProperty glyph = characters.GetArrayElementAtIndex(index);
                        glyph.FindPropertyRelative(CharacterGlyph.CharacterField).intValue = character;
                        glyph.FindPropertyRelative(CharacterGlyph.StrokesField).arraySize = 0;
                        added++;
                    }
                }
            });
            return added;
        }

        internal void Delete(GlyphReference glyph)
        {
            Edit(glyph.IsSymbol ? "Delete Symbol" : "Delete Glyph", () => ListOf(glyph.IsSymbol).DeleteArrayElementAtIndex(glyph.Index));
        }

        /// <summary>
        /// Copies a character's glyph to <paramref name="character"/>, which the pack lacks, and returns the copy.
        /// </summary>
        internal GlyphReference DuplicateCharacter(GlyphReference glyph, int character)
        {
            GlyphReference copy = GlyphReference.None;
            Edit("Duplicate Glyph", () =>
            {
                SerializedProperty characters = ListOf(false);
                characters.InsertArrayElementAtIndex(glyph.Index);
                characters.GetArrayElementAtIndex(glyph.Index + 1)
                    .FindPropertyRelative(CharacterGlyph.CharacterField).intValue = character;
                copy = new GlyphReference(false, Resort(characters, glyph.Index + 1));
            });
            return copy;
        }

        /// <summary>Copies a symbol right after itself, named <paramref name="keyword"/>, and returns the copy.</summary>
        internal GlyphReference DuplicateSymbol(GlyphReference glyph, string keyword)
        {
            Edit("Duplicate Symbol", () =>
            {
                SerializedProperty symbols = ListOf(true);
                symbols.InsertArrayElementAtIndex(glyph.Index);
                symbols.GetArrayElementAtIndex(glyph.Index + 1)
                    .FindPropertyRelative(SymbolGlyph.KeywordField).stringValue = keyword;
            });
            return new GlyphReference(true, glyph.Index + 1);
        }

        /// <summary>Gives a character's glyph to another character, which the pack lacks, and returns where it went.</summary>
        internal GlyphReference ChangeCharacter(GlyphReference glyph, int character)
        {
            GlyphReference moved = glyph;
            Edit("Change Character", () =>
            {
                SerializedProperty characters = ListOf(false);
                characters.GetArrayElementAtIndex(glyph.Index)
                    .FindPropertyRelative(CharacterGlyph.CharacterField).intValue = character;
                moved = new GlyphReference(false, Resort(characters, glyph.Index));
            });
            return moved;
        }

        internal void Rename(GlyphReference symbol, string keyword)
        {
            Edit("Rename Symbol", () => GlyphProperty(symbol).FindPropertyRelative(SymbolGlyph.KeywordField).stringValue = keyword);
        }

        /// <summary>Moves a symbol to <paramref name="index"/> in the pack's order, and with it its enum member.</summary>
        internal void MoveSymbol(int from, int index)
        {
            Edit("Move Symbol", () => ListOf(true).MoveArrayElement(from, index));
        }

        /// <summary>Adds a stroke at the end of the glyph, with one point at <paramref name="firstPoint"/>.</summary>
        internal int AddStroke(GlyphReference glyph, Vector2 firstPoint)
        {
            int stroke = glyph.StrokesIn(Pack).Length;
            Edit("Add Stroke", () =>
            {
                SerializedProperty strokes = StrokesProperty(glyph);
                strokes.arraySize = stroke + 1;
                SerializedProperty added = strokes.GetArrayElementAtIndex(stroke);
                added.FindPropertyRelative(GlyphStroke.IsClosedField).boolValue = false;
                SerializedProperty points = added.FindPropertyRelative(GlyphStroke.PointsField);
                points.arraySize = 1;
                points.GetArrayElementAtIndex(0).vector2Value = firstPoint;
            });
            return stroke;
        }

        internal void DeleteStroke(GlyphReference glyph, int stroke)
        {
            Edit("Delete Stroke", () => StrokesProperty(glyph).DeleteArrayElementAtIndex(stroke));
        }

        internal void MoveStroke(GlyphReference glyph, int from, int index)
        {
            Edit("Move Stroke", () => StrokesProperty(glyph).MoveArrayElement(from, index));
        }

        internal void SetClosed(GlyphReference glyph, int stroke, bool isClosed)
        {
            Edit(isClosed ? "Close Stroke" : "Open Stroke", () => StrokesProperty(glyph).GetArrayElementAtIndex(stroke)
                .FindPropertyRelative(GlyphStroke.IsClosedField).boolValue = isClosed);
        }

        /// <summary>Inserts a point into a stroke at <paramref name="point"/>, the index it takes, which may be the end.</summary>
        internal void InsertPoint(GlyphReference glyph, int stroke, int point, Vector2 position)
        {
            Edit("Add Point", () =>
            {
                SerializedProperty points = PointsProperty(glyph, stroke);
                if (point >= points.arraySize)
                {
                    points.arraySize = point + 1;
                }
                else
                {
                    points.InsertArrayElementAtIndex(point);
                }
                points.GetArrayElementAtIndex(point).vector2Value = position;
            });
        }

        /// <summary>
        /// Moves a point to <paramref name="position"/>. With <paramref name="continuesStep"/>, it joins the undo step
        /// of the previous move, as while dragging.
        /// </summary>
        internal void MovePoint(GlyphReference glyph, int stroke, int point, Vector2 position, bool continuesStep = false)
        {
            Edit("Move Point", () => PointsProperty(glyph, stroke).GetArrayElementAtIndex(point).vector2Value = position,
                !continuesStep);
        }

        internal void MovePointInOrder(GlyphReference glyph, int stroke, int from, int index)
        {
            Edit("Reorder Point", () => PointsProperty(glyph, stroke).MoveArrayElement(from, index));
        }

        /// <summary>Deletes a point, and its stroke with it when it was the last one.</summary>
        internal void DeletePoint(GlyphReference glyph, int stroke, int point)
        {
            Edit("Delete Point", () =>
            {
                SerializedProperty points = PointsProperty(glyph, stroke);
                if (points.arraySize <= 1)
                {
                    StrokesProperty(glyph).DeleteArrayElementAtIndex(stroke);
                }
                else
                {
                    points.DeleteArrayElementAtIndex(point);
                }
            });
        }

        /// <summary>
        /// Moves every point of the glyph so the box of its points is centered horizontally or vertically in the glyph
        /// box. A glyph without points stays as it is.
        /// </summary>
        internal void Center(GlyphReference glyph, bool isHorizontal)
        {
            if (!TryGetPointBounds(glyph.StrokesIn(Pack), out Vector2 minimum, out Vector2 maximum))
            {
                return;
            }
            float offset = isHorizontal ? 0.5f - (minimum.x + maximum.x) * 0.5f : 0.5f - (minimum.y + maximum.y) * 0.5f;
            Vector2 shift = isHorizontal ? new Vector2(offset, 0f) : new Vector2(0f, offset);
            Edit(isHorizontal ? "Center Glyph Horizontally" : "Center Glyph Vertically", () =>
            {
                SerializedProperty strokes = StrokesProperty(glyph);
                for (int stroke = 0; stroke < strokes.arraySize; stroke++)
                {
                    SerializedProperty points = strokes.GetArrayElementAtIndex(stroke).FindPropertyRelative(GlyphStroke.PointsField);
                    for (int point = 0; point < points.arraySize; point++)
                    {
                        SerializedProperty position = points.GetArrayElementAtIndex(point);
                        position.vector2Value = GlyphGrid.Clamp(position.vector2Value + shift);
                    }
                }
            });
        }

        /// <summary>Adds copies of <paramref name="strokes"/> after the glyph's own, as when pasting.</summary>
        internal void AddStrokes(GlyphReference glyph, GlyphStroke[] strokes)
        {
            Edit("Paste Strokes", () =>
            {
                SerializedProperty list = StrokesProperty(glyph);
                foreach (GlyphStroke stroke in strokes)
                {
                    list.arraySize++;
                    SerializedProperty added = list.GetArrayElementAtIndex(list.arraySize - 1);
                    added.FindPropertyRelative(GlyphStroke.IsClosedField).boolValue = stroke.IsClosed;
                    SerializedProperty points = added.FindPropertyRelative(GlyphStroke.PointsField);
                    points.arraySize = stroke.Points.Length;
                    for (int i = 0; i < stroke.Points.Length; i++)
                    {
                        points.GetArrayElementAtIndex(i).vector2Value = GlyphGrid.Clamp(stroke.Points[i]);
                    }
                }
            });
        }

        /// <summary>The smallest box around every finite point of <paramref name="strokes"/>; false without any.</summary>
        internal static bool TryGetPointBounds(GlyphStroke[] strokes, out Vector2 minimum, out Vector2 maximum)
        {
            minimum = Vector2.positiveInfinity;
            maximum = Vector2.negativeInfinity;
            foreach (GlyphStroke stroke in strokes)
            {
                foreach (Vector2 point in stroke.Points)
                {
                    if (float.IsFinite(point.x) && float.IsFinite(point.y))
                    {
                        minimum = Vector2.Min(minimum, point);
                        maximum = Vector2.Max(maximum, point);
                    }
                }
            }
            return minimum.x <= maximum.x;
        }

        /// <summary>
        /// Runs <paramref name="edit"/> on the serialized pack and applies it. By default it's an undo step of its own,
        /// named <paramref name="name"/>; otherwise it joins the current one.
        /// </summary>
        private void Edit(string name, Action edit, bool isNewStep = true)
        {
            if (isNewStep)
            {
                Undo.IncrementCurrentGroup();
            }
            _serialized.Update();
            edit();
            _serialized.ApplyModifiedProperties();
            Undo.SetCurrentGroupName(name);
        }

        private SerializedProperty ListOf(bool isSymbol)
        {
            return _serialized.FindProperty(isSymbol ? WireframeGlyphPack.SymbolsField : WireframeGlyphPack.CharactersField);
        }

        private int SortedIndexOf(int character)
        {
            CharacterGlyph[] characters = Pack.Characters;
            int index = 0;
            while (index < characters.Length && characters[index].Character < character)
            {
                index++;
            }
            return index;
        }

        /// <summary>Finds <paramref name="character"/> in the sorted list, or where it would go.</summary>
        private static bool FindCharacterIn(SerializedProperty characters, int character, out int index)
        {
            for (index = 0; index < characters.arraySize; index++)
            {
                int code = characters.GetArrayElementAtIndex(index).FindPropertyRelative(CharacterGlyph.CharacterField).intValue;
                if (code >= character)
                {
                    return code == character;
                }
            }
            return false;
        }

        /// <summary>Moves the character at <paramref name="index"/> to where its code sorts it, and returns that index.</summary>
        private static int Resort(SerializedProperty characters, int index)
        {
            int character = characters.GetArrayElementAtIndex(index).FindPropertyRelative(CharacterGlyph.CharacterField).intValue;
            int target = 0;
            for (int i = 0; i < characters.arraySize; i++)
            {
                if (i != index
                    && characters.GetArrayElementAtIndex(i).FindPropertyRelative(CharacterGlyph.CharacterField).intValue < character)
                {
                    target++;
                }
            }
            characters.MoveArrayElement(index, target);
            return target;
        }
    }
}
