using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates texts in a <see cref="WireframeContainer"/>, drawn with <see cref="WireframeGlyphs.Default"/>.</summary>
    public static class TextFactory
    {
        /// <summary>
        /// Creates white text centered on the world origin, in the XY plane and read from the -Z side, with characters 1
        /// unit tall.
        /// </summary>
        public static IText CreateText(this WireframeContainer container, string text)
        {
            return CreateText(container, null, Vector3.zero, Quaternion.identity, text);
        }

        /// <summary>
        /// Creates white text centered on a world position, in the XY plane of <paramref name="rotation"/> and read from
        /// its -Z side, with characters 1 unit tall.
        /// </summary>
        public static IText CreateText(this WireframeContainer container, Vector3 position, Quaternion rotation, string text)
        {
            return CreateText(container, null, position, rotation, text);
        }

        /// <summary>
        /// Creates white text that follows <paramref name="bone"/>, centered on a position in the bone's local space, in
        /// the XY plane of <paramref name="localRotation"/> and read from its -Z side, with characters 1 bone unit tall.
        /// </summary>
        public static IText CreateText(
            this WireframeContainer container, Transform bone, Vector3 localPosition, Quaternion localRotation, string text)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return proxy.Add(new GlyphText(bone, localPosition, localRotation, text, null, TextSettings.Default));
        }
    }
}
