using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Shows an int field in the Inspector as a choice between <see cref="Options"/>, for counts that take only a few
    /// values.
    /// </summary>
    internal sealed class IntegerOptionsAttribute : PropertyAttribute
    {
        internal IntegerOptionsAttribute(params int[] options)
        {
            Options = options;
        }

        internal int[] Options { get; }
    }
}
