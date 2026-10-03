using System;
using UnityEngine.SceneManagement;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// What decides which container a shape component draws with: the stage it is drawn in, the settings that a container
    /// applies to all of its shapes, and whether they draw as gizmos. Components with equal keys share one container.
    /// </summary>
    internal readonly struct SharedContainerKey : IEquatable<SharedContainerKey>
    {
        /// <param name="stage">
        /// Scene of a stage of its own, such as a prefab open in Prefab Mode, or an invalid scene for the main stage.
        /// </param>
        internal SharedContainerKey(Scene stage, WireframeOcclusion occlusion, bool usesAlpha, int layer, bool drawsAsGizmo)
        {
            Stage = stage;
            Occlusion = occlusion;
            UsesAlpha = usesAlpha;
            Layer = layer;
            DrawsAsGizmo = drawsAsGizmo;
        }

        internal Scene Stage { get; }

        internal WireframeOcclusion Occlusion { get; }

        internal bool UsesAlpha { get; }

        internal int Layer { get; }

        internal bool DrawsAsGizmo { get; }

        public bool Equals(SharedContainerKey other)
        {
            return Stage == other.Stage && Occlusion == other.Occlusion && UsesAlpha == other.UsesAlpha
                   && Layer == other.Layer && DrawsAsGizmo == other.DrawsAsGizmo;
        }

        public override bool Equals(object obj)
        {
            return obj is SharedContainerKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Scene handles stopped being ints in Unity 6, while Scene hashes its handle in every version.
                int hash = Stage.GetHashCode();
                hash = hash * 31 + (int)Occlusion;
                hash = hash * 31 + (UsesAlpha ? 1 : 0);
                hash = hash * 31 + Layer;
                return hash * 31 + (DrawsAsGizmo ? 1 : 0);
            }
        }
    }
}
