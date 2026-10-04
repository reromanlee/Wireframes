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
    /// <summary>The Inspectors of glyph packs, glyph lists and symbol components, and the overrides they describe.</summary>
    public class GlyphInspectorTests
    {
        private readonly List<Object> _objects = new();
        private InspectorTestWindow _inspector;

        [TearDown]
        public void DestroyObjects()
        {
            if (_inspector != null)
            {
                _inspector.Close();
            }
            if (EditorWindow.HasOpenInstances<GlyphEditorWindow>())
            {
                EditorWindow.GetWindow<GlyphEditorWindow>().Close();
            }
            foreach (Object target in _objects)
            {
                if (target != null)
                {
                    Object.DestroyImmediate(target);
                }
            }
            _objects.Clear();
        }

        [Test]
        public void Overrides_NameTheGlyphsAPackTakesFromThePacksBelowIt()
        {
            WireframeGlyphPack mine = Pack("Mine", "AB", "Heart");
            WireframeGlyphPack fallback = Pack("Fallback", "ABC", "Heart", "Star");

            // An empty entry, and a pack listed again, override nothing.
            List<string> overrides = GlyphOverrides.Describe(Glyphs(mine, null, fallback, mine));

            Assert.That(overrides, Is.EqualTo(new[] { "Mine overrides A, B and Heart from Fallback." }));
        }

        [Test]
        public void Overrides_NameSixGlyphsAtMost()
        {
            List<string> overrides = GlyphOverrides.Describe(Glyphs(Pack("Mine", "ABCDEFG"), Pack("Fallback", "ABCDEFG")));

            Assert.That(overrides, Is.EqualTo(new[] { "Mine overrides A, B, C, D, E and 2 more from Fallback." }));
        }

        [Test]
        public void Overrides_AreNoneWhenNoPacksShareGlyphs()
        {
            Assert.That(GlyphOverrides.Describe(Glyphs(Pack("Letters", "AB"), Pack("Digits", "12"))), Is.Empty);
        }

        [UnityTest]
        public IEnumerator GlyphsInspector_ShowsTheOverridesAsThePacksChange()
        {
            WireframeGlyphPack mine = Pack("Mine", "A");
            WireframeGlyphPack fallback = Pack("Fallback", "A");
            WireframeGlyphs glyphs = Glyphs(mine, fallback);
            _inspector = InspectorTestWindow.Inspect(glyphs);
            yield return null;
            HelpBox overrides = _inspector.rootVisualElement.Q<HelpBox>();
            Assert.That(overrides.text, Is.EqualTo("Mine overrides A from Fallback."));

            SerializedObject serialized = new(glyphs);
            SerializedProperty packs = serialized.FindProperty(WireframeGlyphs.PacksField);
            packs.GetArrayElementAtIndex(0).objectReferenceValue = fallback;
            packs.GetArrayElementAtIndex(1).objectReferenceValue = mine;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            yield return new WaitForSecondsRealtime(0.3f);

            Assert.That(overrides.text, Is.EqualTo("Fallback overrides A from Mine."));
        }

        [UnityTest]
        public IEnumerator PackInspector_CountsTheGlyphsAndOpensThemInTheGlyphEditor()
        {
            WireframeGlyphPack pack = Pack("Mine", "AB", "Heart");
            _inspector = InspectorTestWindow.Inspect(pack);
            yield return null;

            VisualElement root = _inspector.rootVisualElement;
            Assert.That(root.Query<Label>().Where(label => label.text == "2 characters and 1 symbol").First(), Is.Not.Null);
            Press(root.Query<Button>().Where(button => button.text == "Open in Glyph Editor").First());

            Assert.That(EditorWindow.HasOpenInstances<GlyphEditorWindow>(), Is.True);
            Assert.That(EditorWindow.GetWindow<GlyphEditorWindow>().rootVisualElement.Query<GlyphTile>().ToList(),
                Has.Count.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator SymbolField_OffersTheSymbolsOfTheGlyphsByKeyword()
        {
            WireframeSymbol symbol = Track(new GameObject("Symbol")).AddComponent<WireframeSymbol>();
            _inspector = InspectorTestWindow.Inspect(symbol);
            yield return null;

            PopupField<int> field = _inspector.rootVisualElement.Q<PopupField<int>>();
            Assert.That(field.value, Is.EqualTo((int)DefaultSymbols.Star));
            Assert.That(field.text, Is.EqualTo("Star"));
            Assert.That(field.choices, Has.Count.EqualTo(System.Enum.GetValues(typeof(DefaultSymbols)).Length));

            SerializedObject serialized = new(symbol);
            serialized.FindProperty("_glyphs").objectReferenceValue = Glyphs(Pack("Hearts", "", "Heart"));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            yield return new WaitForSecondsRealtime(0.3f);

            Assert.That(field.choices, Is.EqualTo(new[]
            {
                (int)DefaultSymbols.None, (int)DefaultSymbols.Heart, (int)DefaultSymbols.Star
            }));
            Assert.That(field.text, Is.EqualTo($"Missing symbol ({(int)DefaultSymbols.Star})"));
        }

        private WireframeGlyphPack Pack(string packName, string characters, params string[] keywords)
        {
            CharacterGlyph[] characterGlyphs = new CharacterGlyph[characters.Length];
            for (int i = 0; i < characters.Length; i++)
            {
                characterGlyphs[i] = new CharacterGlyph(characters[i], new[] { Line() });
            }
            SymbolGlyph[] symbolGlyphs = new SymbolGlyph[keywords.Length];
            for (int i = 0; i < keywords.Length; i++)
            {
                symbolGlyphs[i] = new SymbolGlyph(keywords[i], new[] { Line() });
            }
            WireframeGlyphPack pack = Track(ScriptableObject.CreateInstance<WireframeGlyphPack>());
            pack.name = packName;
            pack.SetGlyphs(characterGlyphs, symbolGlyphs);
            return pack;
        }

        private WireframeGlyphs Glyphs(params WireframeGlyphPack[] packs)
        {
            WireframeGlyphs glyphs = Track(ScriptableObject.CreateInstance<WireframeGlyphs>());
            glyphs.SetPacks(packs);
            return glyphs;
        }

        private T Track<T>(T target) where T : Object
        {
            _objects.Add(target);
            return target;
        }

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
    }
}
