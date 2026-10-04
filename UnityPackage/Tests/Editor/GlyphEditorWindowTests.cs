using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using reromanlee.Wireframes.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>The Glyph Editor window, opened on a pack made for the test.</summary>
    public class GlyphEditorWindowTests
    {
        private const string Folder = "Assets/WireframesGlyphEditorTest";

        private WireframeGlyphPack _pack;
        private GlyphEditorWindow _window;

        [SetUp]
        public void CreatePack()
        {
            AssetDatabase.CreateFolder("Assets", "WireframesGlyphEditorTest");
            _pack = ScriptableObject.CreateInstance<WireframeGlyphPack>();
            _pack.SetGlyphs(
                new[] { new CharacterGlyph('A', new[] { Line() }), new CharacterGlyph('B', null) },
                new[] { new SymbolGlyph("Heart", new[] { Line() }) });
            AssetDatabase.CreateAsset(_pack, $"{Folder}/Test Pack.asset");
        }

        [TearDown]
        public void DeletePack()
        {
            if (_window != null)
            {
                _window.Close();
            }
            Undo.ClearUndo(_pack);
            AssetDatabase.DeleteAsset(Folder);
        }

        [UnityTest]
        public IEnumerator Window_ShowsEveryGlyphInItsSectionAndOpensOne()
        {
            _window = GlyphEditorWindow.Open(_pack);
            yield return null;

            List<GlyphTile> tiles = _window.rootVisualElement.Query<GlyphTile>().ToList();
            Assert.That(tiles, Has.Count.EqualTo(3));
            Assert.That(tiles.FindAll(tile => tile.Glyph.IsSymbol), Has.Count.EqualTo(1));

            _window.Open(new GlyphReference(true, 0));

            Assert.That(_window.OpenGlyph, Is.EqualTo(new GlyphReference(true, 0)));
            Assert.That(tiles.Find(tile => tile.Glyph.IsSymbol).ClassListContains(GlyphTile.SelectedClass), Is.True);
        }

        [UnityTest]
        public IEnumerator Canvas_AddsMovesAndDeletesPointsWithTheMouseAndKeyboard()
        {
            _window = GlyphEditorWindow.Open(_pack);
            _window.position = new Rect(100f, 100f, 1100f, 700f);
            yield return null;
            GlyphReference b = new(false, 1);
            _window.Open(b);
            yield return null;
            GlyphCanvas canvas = _window.rootVisualElement.Q<GlyphCanvas>();
            Assert.That(canvas.BoxRect.width, Is.GreaterThan(50f), "The canvas wasn't laid out.");

            // Ctrl+click starts a stroke, and again adds a point after its end; both snap to the grid.
            Click(canvas, new Vector2(0.26f, 0.24f), EventModifiers.Control | EventModifiers.Command);
            Click(canvas, new Vector2(0.74f, 0.26f), EventModifiers.Control | EventModifiers.Command);
            Assert.That(b.StrokesIn(_pack)[0].Points, Is.EqualTo(new[] { new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.25f) }));

            Send(canvas, EventType.MouseDown, new Vector2(0.75f, 0.25f), EventModifiers.None);
            Send(canvas, EventType.MouseDrag, new Vector2(0.75f, 0.76f), EventModifiers.None);
            Send(canvas, EventType.MouseUp, new Vector2(0.75f, 0.76f), EventModifiers.None);
            Assert.That(b.StrokesIn(_pack)[0].Points[1], Is.EqualTo(new Vector2(0.75f, 0.75f)));

            using (KeyDownEvent delete = KeyDownEvent.GetPooled('\0', KeyCode.Delete, EventModifiers.None))
            {
                canvas.SendEvent(delete);
            }
            Assert.That(b.StrokesIn(_pack)[0].Points, Has.Length.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Details_EditPointsCenterTheGlyphAndChangeItsCharacter()
        {
            _window = GlyphEditorWindow.Open(_pack);
            yield return null;
            GlyphReference a = new(false, 0);
            _window.Open(a);
            yield return null;
            GlyphDetails details = _window.rootVisualElement.Q<GlyphDetails>();

            // Typed values stay in the glyph box.
            details.Query<FloatField>(className: "glyph-details__coordinate").First().value = -1f;
            Assert.That(a.StrokesIn(_pack)[0].Points[0], Is.EqualTo(new Vector2(0f, 0.25f)));

            Press(details.Query<Button>().Where(button => button.text == "Center Horizontally").First());
            Assert.That(a.StrokesIn(_pack)[0].Points[0].x, Is.EqualTo(0.1f).Within(1e-6f));
            Assert.That(a.StrokesIn(_pack)[0].Points[1].x, Is.EqualTo(0.9f).Within(1e-6f));

            details.Q<TextField>(className: "glyph-details__key").value = "C";
            Assert.That(_pack.Characters[1].Character, Is.EqualTo('C'));
            Assert.That(_window.OpenGlyph, Is.EqualTo(new GlyphReference(false, 1)));
        }

        [UnityTest]
        public IEnumerator PreviewStrip_ListsTheCharactersThePackLacks()
        {
            _window = GlyphEditorWindow.Open(_pack);
            yield return null;
            GlyphPreviewStrip strip = _window.rootVisualElement.Q<GlyphPreviewStrip>();

            strip.Q<TextField>().value = "ABZ? Z";

            Label missing = strip.Q<Label>(className: "glyph-preview-strip__missing");
            Assert.That(missing.text, Is.EqualTo("Missing from this pack: Z ?"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator EditsMadeElsewhere_ShowUpInTheGallery()
        {
            _window = GlyphEditorWindow.Open(_pack);
            yield return null;

            new GlyphPackEditing(_pack).AddCharacter('C');
            yield return new WaitForSecondsRealtime(0.3f);

            Assert.That(_window.rootVisualElement.Query<GlyphTile>().ToList(), Has.Count.EqualTo(4));
        }

        /// <summary>A line from 0.2 to 0.8 on the baseline, with points of its own, since glyphs never share arrays.</summary>
        private static GlyphStroke Line()
        {
            return new GlyphStroke(new[] { new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.25f) }, false);
        }

        /// <summary>Presses a button as the keyboard does, which runs its click action.</summary>
        private static void Press(Button button)
        {
            using (NavigationSubmitEvent submit = NavigationSubmitEvent.GetPooled())
            {
                submit.target = button;
                button.SendEvent(submit);
            }
        }

        private static void Click(GlyphCanvas canvas, Vector2 glyphPoint, EventModifiers modifiers)
        {
            Send(canvas, EventType.MouseDown, glyphPoint, modifiers);
            Send(canvas, EventType.MouseUp, glyphPoint, modifiers);
        }

        /// <summary>Sends a mouse event to the canvas at a point of the glyph box, as the editor would.</summary>
        private static void Send(GlyphCanvas canvas, EventType type, Vector2 glyphPoint, EventModifiers modifiers)
        {
            Vector2 position = canvas.LocalToWorld(GlyphDrawing.ToBox(glyphPoint, canvas.BoxRect));
            Event mouseEvent = new()
            {
                type = type, mousePosition = position, button = 0, clickCount = 1, modifiers = modifiers
            };
            EventBase pointerEvent = type switch
            {
                EventType.MouseDown => PointerDownEvent.GetPooled(mouseEvent),
                EventType.MouseDrag => PointerMoveEvent.GetPooled(mouseEvent),
                _ => PointerUpEvent.GetPooled(mouseEvent)
            };
            using (pointerEvent)
            {
                canvas.SendEvent(pointerEvent);
            }
        }
    }
}
