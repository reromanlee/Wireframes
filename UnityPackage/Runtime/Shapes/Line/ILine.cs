using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A line segment whose endpoints can each follow their own bone.
    /// </summary>
    public interface ILine : IShape
    {
        /// <summary>Endpoint A relative to <see cref="BoneA"/>, or in world space without a bone.</summary>
        Vector3 LocalPositionA { get; set; }

        /// <summary>Endpoint B relative to <see cref="BoneB"/>, or in world space without a bone.</summary>
        Vector3 LocalPositionB { get; set; }

        /// <summary>Endpoint A in world space, converted through <see cref="BoneA"/>'s current pose.</summary>
        Vector3 WorldPositionA { get; set; }

        /// <summary>Endpoint B in world space, converted through <see cref="BoneB"/>'s current pose.</summary>
        Vector3 WorldPositionB { get; set; }

        /// <summary>Color at endpoint A. Colors are interpolated along the line.</summary>
        Color ColorA { get; set; }

        /// <summary>Color at endpoint B. Colors are interpolated along the line.</summary>
        Color ColorB { get; set; }

        /// <summary>
        /// Transform that endpoint A follows, or null for world space. Changing it keeps the endpoint's world position,
        /// like reparenting a Transform. If the bone is destroyed, the endpoint stays where it was and becomes world space.
        /// </summary>
        Transform BoneA { get; set; }

        /// <summary>
        /// Transform that endpoint B follows, or null for world space. Changing it keeps the endpoint's world position,
        /// like reparenting a Transform. If the bone is destroyed, the endpoint stays where it was and becomes world space.
        /// </summary>
        Transform BoneB { get; set; }
    }
}
