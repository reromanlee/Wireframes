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
            GlyphStroke line = new(new[] { new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.25f) }, false);
            _pack.SetGlyphs(
                new[] { new CharacterGlyph('A', new[] { line }), new CharacterGlyph('B', null) },
                new[] { new SymbolGlyph("Heart", new[] { line }) });
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
        public IEnumerator EditsMadeElsewhere_ShowUpInTheGallery()
        {
            _window = GlyphEditorWindow.Open(_pack);
            yield return null;

            new GlyphPackEditing(_pack).AddCharacter('C');
            yield return new WaitForSecondsRealtime(0.3f);

            Assert.That(_window.rootVisualElement.Query<GlyphTile>().ToList(), Has.Count.EqualTo(4));
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
