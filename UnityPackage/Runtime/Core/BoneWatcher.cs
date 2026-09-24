using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Hidden component added to GameObjects used as bones. Its OnDestroy runs while the transform can still be read,
    /// so attached endpoints can detach without moving. It removes itself once no registry uses the bone.
    /// </summary>
    [AddComponentMenu("")]
    internal sealed class BoneWatcher : MonoBehaviour
    {
        private static readonly List<BoneWatcher> Candidates = new();

        private readonly List<BoneRegistry> _registries = new();
        private bool _destroyRequested;

        internal Transform Bone { get; private set; }

        internal bool IsAwake { get; private set; }

        internal static BoneWatcher Watch(Transform bone, BoneRegistry registry)
        {
            BoneWatcher watcher = null;
            bone.GetComponents(Candidates);
            foreach (BoneWatcher candidate in Candidates)
            {
                // A watcher already scheduled for destruction can't be revived, so a fresh one is added instead.
                if (!candidate._destroyRequested)
                {
                    watcher = candidate;
                    break;
                }
            }
            Candidates.Clear();

            if (watcher == null)
            {
                watcher = bone.gameObject.AddComponent<BoneWatcher>();
                watcher.hideFlags = HideFlags.HideInInspector;
                watcher.Bone = bone;
            }
            watcher._registries.Add(registry);
            return watcher;
        }

        internal void RemoveListener(BoneRegistry registry)
        {
            _registries.Remove(registry);
            // The null check covers a watcher that is already gone together with its bone.
            if (_registries.Count == 0 && !_destroyRequested && this != null)
            {
                _destroyRequested = true;
                UnityObjects.Destroy(this);
            }
        }

        private void Awake()
        {
            IsAwake = true;
        }

        private void OnDestroy()
        {
            if (_destroyRequested || _registries.Count == 0)
            {
                return;
            }
            _destroyRequested = true;
            // Registries release the bone while being notified, which edits the list, so they are notified from a copy.
            BoneRegistry[] registries = _registries.ToArray();
            _registries.Clear();
            foreach (BoneRegistry registry in registries)
            {
                registry.OnBoneDestroyed(Bone);
            }
        }
    }
}
