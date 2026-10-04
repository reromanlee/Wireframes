using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// The pack's settings: how its enum is generated, the width proportional text gives a space, and the guides and
    /// grid the glyph canvas draws. Every field is bound to the pack, so changes apply at once and undo.
    /// </summary>
    internal sealed class GlyphPackSettingsPopup : PopupWindowContent
    {
        private readonly WireframeGlyphPack _pack;
        private readonly bool _isReadOnly;

        private GlyphPackSettingsPopup(WireframeGlyphPack pack, bool isReadOnly)
        {
            _pack = pack;
            _isReadOnly = isReadOnly;
        }

        internal static void Show(VisualElement anchor, WireframeGlyphPack pack, bool isReadOnly)
        {
            UnityEditor.PopupWindow.Show(anchor.worldBound, new GlyphPackSettingsPopup(pack, isReadOnly));
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(340f, 236f);
        }

        public override void OnGUI(Rect rect)
        {
        }

        public override void OnOpen()
        {
            VisualElement root = editorWindow.rootVisualElement;
            if (GlyphAssets.StyleSheet != null)
            {
                root.styleSheets.Add(GlyphAssets.StyleSheet);
            }
            root.AddToClassList("glyph-popup");
            root.Add(new Label("Pack Settings") { name = "Title" });

            string settings = WireframeGlyphPack.EnumSettingsField;
            TextField enumName = AddField(root, new TextField("Enum Name"), $"{settings}.{GlyphEnumSettings.NameField}",
                "Name of the generated enum. Empty uses the pack's name.");
            TextField enumNamespace = AddField(root, new TextField("Enum Namespace"),
                $"{settings}.{GlyphEnumSettings.NamespaceField}",
                "Namespace of the generated enum. Empty uses the project's root namespace, if it has one.");
            Label enumHint = new();
            enumHint.AddToClassList("glyph-popup__hint");
            root.Add(enumHint);
            void ShowEnumHint()
            {
                string name = enumName.value.Length > 0 ? enumName.value : GlyphEnumGenerator.IdentifierOf(_pack.name);
                string namespaceName = enumNamespace.value.Length > 0
                    ? enumNamespace.value
                    : EditorSettings.projectGenerationRootNamespace;
                enumHint.text = string.IsNullOrEmpty(namespaceName) ? $"Generates {name}" : $"Generates {namespaceName}.{name}";
            }
            enumName.RegisterValueChangedCallback(_ => ShowEnumHint());
            enumNamespace.RegisterValueChangedCallback(_ => ShowEnumHint());

            AddField(root, new FloatField("Space Width"), WireframeGlyphPack.SpaceWidthField,
                "Width proportional text gives a character without lines, such as a space, in glyph boxes.");
            AddField(root, new Slider("x-Height Guide", 0f, 1f) { showInputField = true }, WireframeGlyphPack.XHeightField,
                "Height of the x-height guide the glyph canvas draws. Text layout doesn't use it.");
            AddField(root, new Slider("Cap Height Guide", 0f, 1f) { showInputField = true },
                WireframeGlyphPack.CapHeightField,
                "Height of the cap-height guide the glyph canvas draws. Text layout doesn't use it.");
            AddField(root,
                new SliderInt("Grid Divisions", WireframeGlyphPack.MinimumGridDivisions,
                    WireframeGlyphPack.MaximumGridDivisions) { showInputField = true },
                WireframeGlyphPack.GridDivisionsField, "Divisions of the canvas grid across the glyph box, which points snap to.");

            root.Bind(new SerializedObject(_pack));
            root.SetEnabled(!_isReadOnly);
            root.schedule.Execute(ShowEnumHint);
        }

        private static T AddField<T>(VisualElement root, T field, string bindingPath, string tooltip)
            where T : VisualElement, IBindable
        {
            field.bindingPath = bindingPath;
            field.tooltip = tooltip;
            field.AddToClassList("glyph-popup__field");
            root.Add(field);
            return field;
        }
    }
}
