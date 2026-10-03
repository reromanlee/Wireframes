using UnityEditor;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>The polyline's Inspector, which also says when it has too few points to draw anything.</summary>
    [CustomEditor(typeof(WireframePolyline))]
    [CanEditMultipleObjects]
    internal sealed class WireframePolylineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            foreach (Object selected in targets)
            {
                if (selected is WireframePolyline polyline && !polyline.CanCreateShape)
                {
                    EditorGUILayout.HelpBox(
                        "A polyline needs at least 2 points, or 3 when closed, so nothing is drawn.", MessageType.Warning);
                    return;
                }
            }
        }
    }
}
