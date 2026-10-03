using UnityEditor;
using UnityEditorInternal;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// Keeps shape components in step with what happens in the editor without a callback on the components: it runs the
    /// refreshes that OnValidate had to put off, refreshes every component after undo and redo and the ones whose
    /// GameObject changed layer, and repaints the views after a change in Edit Mode, which doesn't render on its own.
    /// </summary>
    [InitializeOnLoad]
    internal static class ComponentRefresh
    {
        static ComponentRefresh()
        {
            EditorApplication.update += Update;
            Undo.undoRedoPerformed += ShapeComponents.RefreshAll;
            ObjectChangeEvents.changesPublished += OnChangesPublished;
        }

        /// <summary>Runs the refreshes that were put off, then repaints the views if a change asked for it.</summary>
        internal static void Update()
        {
            ShapeComponents.RefreshDeferred();
            if (ShapeComponents.TakeRepaintRequest())
            {
                InternalEditorUtility.RepaintAllViews();
            }
        }

        private static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; i++)
            {
                // A layer change is one of a GameObject's properties, or of a whole hierarchy's when it spreads to children.
                ObjectChangeKind kind = stream.GetEventType(i);
                if (kind == ObjectChangeKind.ChangeGameObjectOrComponentProperties
                    || kind == ObjectChangeKind.ChangeGameObjectStructureHierarchy)
                {
                    ShapeComponents.RefreshLayers();
                    return;
                }
            }
        }
    }
}
