using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// A small popup that asks for a character or a keyword, says what's wrong with it while it's typed, and hands it
    /// over once it's valid, on Enter or its button.
    /// </summary>
    internal sealed class GlyphKeyPopup : PopupWindowContent
    {
        private readonly string _title;
        private readonly string _initialText;
        private readonly string _submitText;
        private readonly Func<string, string> _findProblem;
        private readonly Action<string> _submit;

        private TextField _field;
        private Label _problem;
        private Button _button;

        /// <param name="findProblem">Returns why a text can't be taken, or null when it can.</param>
        internal GlyphKeyPopup(
            string title, string initialText, string submitText, Func<string, string> findProblem, Action<string> submit)
        {
            _title = title;
            _initialText = initialText ?? string.Empty;
            _submitText = submitText;
            _findProblem = findProblem;
            _submit = submit;
        }

        /// <summary>Shows a popup below <paramref name="anchor"/>.</summary>
        internal static void Show(
            VisualElement anchor, string title, string initialText, string submitText, Func<string, string> findProblem,
            Action<string> submit)
        {
            UnityEditor.PopupWindow.Show(anchor.worldBound,
                new GlyphKeyPopup(title, initialText, submitText, findProblem, submit));
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(280f, 104f);
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
            root.Add(new Label(_title) { name = "Title" });
            _field = new TextField { value = _initialText };
            _field.RegisterValueChangedCallback(_ => Validate());
            root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            root.Add(_field);
            _problem = new Label();
            _problem.AddToClassList("glyph-popup__problem");
            root.Add(_problem);
            _button = new Button(Submit) { text = _submitText };
            root.Add(_button);
            Validate();
            _field.schedule.Execute(() => _field.Focus());
        }

        private void OnKeyDown(KeyDownEvent keyEvent)
        {
            if (keyEvent.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
            {
                Submit();
                keyEvent.StopPropagation();
            }
            else if (keyEvent.keyCode == KeyCode.Escape)
            {
                editorWindow.Close();
                keyEvent.StopPropagation();
            }
        }

        private void Validate()
        {
            string problem = _findProblem(_field.value);
            _problem.text = problem ?? string.Empty;
            _button.SetEnabled(problem == null);
        }

        private void Submit()
        {
            string text = _field.value;
            if (_findProblem(text) != null)
            {
                return;
            }
            editorWindow.Close();
            _submit(text);
        }
    }
}
