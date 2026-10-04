using NUnit.Framework;
using reromanlee.Wireframes.Editor;
using UnityEditor;
using UnityEngine;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>The edits the Glyph Editor makes to a pack, and how it names and reads characters.</summary>
    public class GlyphPackEditingTests
    {
        private WireframeGlyphPack _pack;
        private GlyphPackEditing _editing;

        [SetUp]
        public void CreatePack()
        {
            _pack = ScriptableObject.CreateInstance<WireframeGlyphPack>();
            _editing = new GlyphPackEditing(_pack);
        }

        [TearDown]
        public void DestroyPack()
        {
            Undo.ClearUndo(_pack);
            Object.DestroyImmediate(_pack);
        }

        [Test]
        public void AddedCharacters_StaySortedByCode()
        {
            _editing.AddCharacter('C');
            GlyphReference a = _editing.AddCharacter('A');
            _editing.AddCharacter('B');

            Assert.That(a.Index, Is.Zero);
            Assert.That(CharacterCodes(), Is.EqualTo(new[] { 'A', 'B', 'C' }));
        }

        [Test]
        public void AddMissingAscii_AddsEveryPrintableCharacterThePackLacks()
        {
            _editing.AddCharacter('A');

            Assert.That(_editing.AddMissingAscii(), Is.EqualTo(94));
            Assert.That(_editing.AddMissingAscii(), Is.Zero);
            Assert.That(_pack.Characters[0].Character, Is.EqualTo(' '));
            Assert.That(_pack.Characters[94].Character, Is.EqualTo('~'));
        }

        [Test]
        public void ChangedCharacter_MovesWhereItsCodeSortsIt()
        {
            GlyphReference a = _editing.AddCharacter('A');
            _editing.AddCharacter('B');
            _editing.AddCharacter('C');

            GlyphReference moved = _editing.ChangeCharacter(a, 'D');

            Assert.That(moved.Index, Is.EqualTo(2));
            Assert.That(CharacterCodes(), Is.EqualTo(new[] { 'B', 'C', 'D' }));
        }

        [Test]
        public void Points_AreAddedMovedReorderedAndDeleted()
        {
            GlyphReference glyph = _editing.AddCharacter('A');
            int stroke = _editing.AddStroke(glyph, new Vector2(0f, 0f));
            _editing.InsertPoint(glyph, stroke, 1, new Vector2(1f, 1f));
            _editing.InsertPoint(glyph, stroke, 1, new Vector2(0.5f, 0.5f));

            Assert.That(PointsOf(glyph), Is.EqualTo(new[] { Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.one }));

            _editing.MovePoint(glyph, stroke, 1, new Vector2(0.5f, 0.25f));
            _editing.MovePointInOrder(glyph, stroke, 2, 0);
            Assert.That(PointsOf(glyph), Is.EqualTo(new[] { Vector2.one, Vector2.zero, new Vector2(0.5f, 0.25f) }));

            _editing.SetClosed(glyph, stroke, true);
            _editing.DeletePoint(glyph, stroke, 0);
            Assert.That(PointsOf(glyph), Is.EqualTo(new[] { Vector2.zero, new Vector2(0.5f, 0.25f) }));
            Assert.That(glyph.StrokesIn(_pack)[0].IsClosed, Is.True);
        }

        [Test]
        public void DeletingTheLastPoint_DeletesItsStroke()
        {
            GlyphReference glyph = _editing.AddCharacter('A');
            int stroke = _editing.AddStroke(glyph, new Vector2(0.5f, 0.5f));

            _editing.DeletePoint(glyph, stroke, 0);

            Assert.That(glyph.StrokesIn(_pack), Is.Empty);
        }

        [Test]
        public void Center_MovesTheGlyphToTheMiddleOfItsBox()
        {
            GlyphReference glyph = _editing.AddCharacter('A');
            int stroke = _editing.AddStroke(glyph, new Vector2(0f, 0.25f));
            _editing.InsertPoint(glyph, stroke, 1, new Vector2(0.4f, 0.55f));

            _editing.Center(glyph, true);
            _editing.Center(glyph, false);

            Vector2[] points = PointsOf(glyph);
            Assert.That(points[0].x, Is.EqualTo(0.3f).Within(1e-6f));
            Assert.That(points[1].x, Is.EqualTo(0.7f).Within(1e-6f));
            Assert.That(points[0].y, Is.EqualTo(0.35f).Within(1e-6f));
            Assert.That(points[1].y, Is.EqualTo(0.65f).Within(1e-6f));
        }

        [Test]
        public void DuplicatedSymbol_FollowsItsOriginalWithTheSameStrokes()
        {
            GlyphReference heart = _editing.AddSymbol("Heart");
            _editing.AddSymbol("Star");
            _editing.AddStroke(heart, new Vector2(0.5f, 0.5f));

            GlyphReference copy = _editing.DuplicateSymbol(heart, "HeartCopy");

            Assert.That(copy.Index, Is.EqualTo(1));
            Assert.That(_pack.Symbols[1].Keyword, Is.EqualTo("HeartCopy"));
            Assert.That(_pack.Symbols[1].Strokes[0].Points[0], Is.EqualTo(new Vector2(0.5f, 0.5f)));
            Assert.That(_pack.Symbols[2].Keyword, Is.EqualTo("Star"));
        }

        [Test]
        public void Undo_TakesBackOneEditAtATime()
        {
            GlyphReference glyph = _editing.AddCharacter('A');
            _editing.AddStroke(glyph, new Vector2(0.5f, 0.5f));

            Undo.PerformUndo();
            Assert.That(_pack.Characters, Has.Length.EqualTo(1));
            Assert.That(glyph.StrokesIn(_pack), Is.Empty);

            Undo.PerformUndo();
            Assert.That(_pack.Characters, Is.Empty);
        }

        [Test]
        public void CopiedStrokes_PasteIntoAnotherGlyph()
        {
            string clipboard = EditorGUIUtility.systemCopyBuffer;
            try
            {
                GlyphReference source = _editing.AddCharacter('A');
                int stroke = _editing.AddStroke(source, new Vector2(0.25f, 0.25f));
                _editing.InsertPoint(source, stroke, 1, new Vector2(0.75f, 0.25f));
                GlyphClipboard.Copy(source.StrokesIn(_pack));

                GlyphReference target = _editing.AddCharacter('B');
                Assert.That(GlyphClipboard.TryPaste(out GlyphStroke[] strokes), Is.True);
                _editing.AddStrokes(target, strokes);

                Assert.That(PointsOf(target), Is.EqualTo(new[] { new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.25f) }));
                EditorGUIUtility.systemCopyBuffer = "Something else";
                Assert.That(GlyphClipboard.TryPaste(out _), Is.False);
            }
            finally
            {
                EditorGUIUtility.systemCopyBuffer = clipboard;
            }
        }

        [TestCase("A", 'A')]
        [TestCase(" ", ' ')]
        [TestCase("U+0416", 0x416)]
        [TestCase(" u+7e ", '~')]
        public void TypedCharacters_AreReadAsThemselvesOrTheirCode(string text, int character)
        {
            Assert.That(GlyphNames.Parse(text, out int parsed), Is.Null);
            Assert.That(parsed, Is.EqualTo(character));
        }

        [TestCase("")]
        [TestCase("AB")]
        [TestCase("U+0007")]
        [TestCase("U+D800")]
        public void OtherInput_IsRejected(string text)
        {
            Assert.That(GlyphNames.Parse(text, out _), Is.Not.Null);
        }

        [Test]
        public void Characters_AreNamedAsTheyLook()
        {
            Assert.That(GlyphNames.CharacterOf('A'), Is.EqualTo("A"));
            Assert.That(GlyphNames.CharacterOf(' '), Is.EqualTo("Space"));
            Assert.That(GlyphNames.CodeOf('A'), Is.EqualTo("U+0041"));
        }

        private char[] CharacterCodes()
        {
            char[] codes = new char[_pack.Characters.Length];
            for (int i = 0; i < codes.Length; i++)
            {
                codes[i] = (char)_pack.Characters[i].Character;
            }
            return codes;
        }

        private Vector2[] PointsOf(GlyphReference glyph)
        {
            return glyph.StrokesIn(_pack)[0].Points;
        }
    }
}
