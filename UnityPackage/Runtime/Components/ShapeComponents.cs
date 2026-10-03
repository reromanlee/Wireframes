using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Keeps track of shape components for the events that refresh many of them at once: the enabled components, the
    /// refreshes that OnValidate has to put off until the editor's next update, and whether the editor's views need a
    /// repaint to show a change made in Edit Mode.
    /// </summary>
    internal static class ShapeComponents
    {
        private static readonly List<WireframeShape> Enabled = new();
        private static readonly List<WireframeShape> Deferred = new();
        private static bool _isRepaintRequested;

        /// <summary>Every enabled shape component, in no particular order.</summary>
        internal static IReadOnlyList<WireframeShape> EnabledComponents
        {
            get => Enabled;
        }

        internal static void Add(WireframeShape component)
        {
            component.EnabledIndex = Enabled.Count;
            Enabled.Add(component);
        }

        internal static void Remove(WireframeShape component)
        {
            int index = component.EnabledIndex;
            if (index < 0)
            {
                return;
            }
            int last = Enabled.Count - 1;
            WireframeShape moved = Enabled[last];
            Enabled[index] = moved;
            moved.EnabledIndex = index;
            Enabled.RemoveAt(last);
            component.EnabledIndex = -1;
        }

        /// <summary>
        /// Puts off refreshing <paramref name="component"/> until <see cref="RefreshDeferred"/>, once however often it
        /// is asked.
        /// </summary>
        internal static void Defer(WireframeShape component)
        {
            if (component.IsDeferred)
            {
                return;
            }
            component.IsDeferred = true;
            Deferred.Add(component);
        }

        /// <summary>Refreshes the components whose refresh was put off, for the editor's update loop.</summary>
        internal static void RefreshDeferred()
        {
            // Refreshes put off while these run wait for the next update.
            int count = Deferred.Count;
            if (count == 0)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                WireframeShape component = Deferred[i];
                component.IsDeferred = false;
                // A component destroyed meanwhile has nothing left to refresh.
                if (component != null)
                {
                    component.Refresh();
                }
            }
            Deferred.RemoveRange(0, count);
        }

        /// <summary>Refreshes every enabled component, as after undo and redo, which can change any of them.</summary>
        internal static void RefreshAll()
        {
            for (int i = Enabled.Count - 1; i >= 0; i--)
            {
                Enabled[i].Refresh();
            }
        }

        /// <summary>
        /// Refreshes the enabled components whose GameObject moved to another layer, which no callback reports.
        /// </summary>
        internal static void RefreshLayers()
        {
            for (int i = Enabled.Count - 1; i >= 0; i--)
            {
                WireframeShape component = Enabled[i];
                if (component.IsOnStaleLayer)
                {
                    component.Refresh();
                }
            }
        }

        /// <summary>
        /// Writes the fields of the point shapes in <paramref name="proxy"/> into their shapes again, after its flush found
        /// bones destroyed, so points that followed one follow their GameObject instead in the same frame.
        /// </summary>
        internal static void RefreshAfterBonesDestroyed(MeshProxy proxy)
        {
            for (int i = Enabled.Count - 1; i >= 0; i--)
            {
                if (Enabled[i] is WireframePointShape component && component.SharedContainer != null
                    && component.SharedContainer.Container == proxy.Container)
                {
                    component.RefreshInPlace();
                }
            }
        }

        /// <summary>
        /// Asks for the editor's views to be repainted on its next update, so a change made outside the Inspector shows up
        /// in Edit Mode too. Play Mode renders every frame anyway.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        internal static void RequestRepaint()
        {
            if (!Application.isPlaying)
            {
                _isRepaintRequested = true;
            }
        }

        /// <summary>Returns whether a repaint was asked for since the last call, and clears the request.</summary>
        internal static bool TakeRepaintRequest()
        {
            bool isRequested = _isRepaintRequested;
            _isRepaintRequested = false;
            return isRequested;
        }
    }
}
