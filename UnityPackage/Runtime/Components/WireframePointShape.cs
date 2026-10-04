using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of the shape components made of points: lines and polylines. Each point follows a bone of its own at a
    /// position relative to it, or the GameObject when it has none. A bone that is destroyed counts as none, so its point
    /// follows the GameObject from the next render on.
    /// </summary>
    public abstract class WireframePointShape : WireframeShape
    {
        private bool _hasReportedBone;

        private protected WireframePointShape()
        {
        }

        /// <summary>
        /// The Transform a point follows: <paramref name="bone"/>, or the GameObject's own when there is none, when it was
        /// destroyed, or when it isn't in a scene, such as a prefab asset, which is reported once.
        /// </summary>
        private protected Transform ResolveBone(Transform bone)
        {
            if (bone == null)
            {
                return transform;
            }
            if (!bone.gameObject.scene.IsValid())
            {
                if (!_hasReportedBone)
                {
                    _hasReportedBone = true;
                    WireframesLog.Warning(
                        $"'{bone.name}' isn't in a scene, so a point of the {GetType().Name} on '{name}' follows its "
                        + "GameObject instead. Points can follow scene objects only, not prefab assets.",
                        this);
                }
                return transform;
            }
            return bone;
        }
    }
}
