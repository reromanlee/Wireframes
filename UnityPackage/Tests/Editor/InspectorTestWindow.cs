using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Tests
{
    /// <summary>A window that shows one object's Inspector, so its fields bind and follow changes as in the Inspector.</summary>
    internal sealed class InspectorTestWindow : EditorWindow
    {
        /// <summary>Opens a window with the Inspector of <paramref name="target"/> and returns it.</summary>
        internal static InspectorTestWindow Inspect(Object target)
        {
            InspectorTestWindow window = CreateWindow<InspectorTestWindow>("Inspector Test");
            window.position = new Rect(120f, 120f, 480f, 640f);
            window.rootVisualElement.Add(new InspectorElement(target));
            return window;
        }
    }
}
