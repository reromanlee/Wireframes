using System.Collections.Generic;
using NUnit.Framework;
using reromanlee.Wireframes.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Texts and symbols in Edit Mode, kept in step with the Inspector and with edits to their glyph packs.</summary>
    public class GlyphEditModeTests
    {
        private readonly List<Object> _objects = new();
        private readonly List<WireframeContainer> _containers = new();

        [SetUp]
        public void OpenEmptyScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void DestroyEverything()
        {
            foreach (WireframeContainer container in _containers)
            {
                container.Dispose();
            }
            _containers.Clear();
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                if (_objects[i] != null)
                {
                    Object.DestroyImmediate(_objects[i]);
                }
            }
            _objects.Clear();
            ShapeComponents.TakeRepaintRequest();
        }

        [Test]
        public void InspectorTextEdit_IsAppliedOnTheEditorsNextUpdate()
        {
            WireframeText text = AddText();
            SerializedObject serialized = new(text);

            serialized.FindProperty("_text").stringValue = "AAA";
            serialized.ApplyModifiedProperties();

            // More text may need more room than the shape has, and finding it may create objects, which OnValidate forbids.
            Assert.That(text.Shape.EdgeCount, Is.EqualTo(1));
            Assert.That(text.IsDeferred, Is.True);

            ComponentRefresh.Update();

            Assert.That(text.Shape.EdgeCount, Is.EqualTo(3));
            Assert.That(text.IsDeferred, Is.False);
        }

        [Test]
        public void InspectorLayoutEdit_ShowsUpRightAway()
        {
            WireframeText text = AddText();
            SerializedObject serialized = new(text);

            serialized.FindProperty("_characterSize").floatValue = 2f;
            serialized.ApplyModifiedProperties();

            Assert.That(((GlyphText)text.Shape).CharacterSize, Is.EqualTo(2f));
            Assert.That(text.IsDeferred, Is.False);
        }

        [Test]
        public void EditedPack_RedrawsItsTextsOnTheEditorsNextUpdateAndUndoToo()
        {
            WireframeText component = AddText();
            WireframeContainer container = new();
            _containers.Add(container);
            IText code = container.CreateText("A");
            code.Glyphs = component.Glyphs;
            WireframeGlyphPack pack = component.Glyphs.Packs[0];
            Undo.IncrementCurrentGroup();

            // As the Glyph Editor does: a third point on A's stroke.
            SerializedObject serialized = new(pack);
            SerializedProperty points = serialized.FindProperty(WireframeGlyphPack.CharactersField)
                .GetArrayElementAtIndex(0).FindPropertyRelative(CharacterGlyph.StrokesField)
                .GetArrayElementAtIndex(0).FindPropertyRelative(GlyphStroke.PointsField);
            points.arraySize = 3;
            points.GetArrayElementAtIndex(2).vector2Value = new Vector2(0.8f, 0.75f);
            serialized.ApplyModifiedProperties();
            GlyphRefresh.Update();

            Assert.That(component.Shape.EdgeCount, Is.EqualTo(2));
            Assert.That(((Shape)code).EdgeCount, Is.EqualTo(2));

            Undo.PerformUndo();
            GlyphRefresh.Update();

            Assert.That(component.Shape.EdgeCount, Is.EqualTo(1));
            Assert.That(((Shape)code).EdgeCount, Is.EqualTo(1));
        }

        /// <summary>A text component that draws "A", a single line, with glyphs made for the test.</summary>
        private WireframeText AddText()
        {
            WireframeGlyphPack pack = Track(ScriptableObject.CreateInstance<WireframeGlyphPack>());
            GlyphStroke line = new(new[] { new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.25f) }, false);
            pack.SetGlyphs(new[] { new CharacterGlyph('A', new[] { line }) }, null);
            WireframeGlyphs glyphs = Track(ScriptableObject.CreateInstance<WireframeGlyphs>());
            glyphs.SetPacks(pack);

            GameObject owner = Track(new GameObject("Text"));
            owner.SetActive(false);
            WireframeText text = owner.AddComponent<WireframeText>();
            text.Glyphs = glyphs;
            text.Text = "A";
            owner.SetActive(true);
            return text;
        }

        private T Track<T>(T target) where T : Object
        {
            _objects.Add(target);
            return target;
        }
    }
}
