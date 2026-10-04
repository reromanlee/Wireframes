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
        public IEnumerator EditsMadeElsewhere_ShowUpInTheGallery()
        {
            _window = GlyphEditorWindow.Open(_pack);
            yield return null;

            new GlyphPackEditing(_pack).AddCharacter('C');
            yield return new WaitForSecondsRealtime(0.3f);

            Assert.That(_window.rootVisualElement.Query<GlyphTile>().ToList(), Has.Count.EqualTo(4));
        }
    }
}
