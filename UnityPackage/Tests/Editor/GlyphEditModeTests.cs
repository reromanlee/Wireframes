using System.Collections.Generic;
using NUnit.Framework;
using reromanlee.Wireframes.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>Texts and symbols in Edit Mode, kept in step with the Inspector.</summary>
    public class GlyphEditModeTests
    {
        private readonly List<Object> _objects = new();

        [SetUp]
        public void OpenEmptyScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void DestroyEverything()
        {
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
