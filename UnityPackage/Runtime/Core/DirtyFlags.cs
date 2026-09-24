using System;

namespace reromanlee.Wireframes
{
    /// <summary>Which parts of a shape must be written to the mesh on the next flush.</summary>
    [Flags]
    internal enum DirtyFlags : byte
    {
        None = 0,
        Positions = 1 << 0,
        Colors = 1 << 1,
        Bones = 1 << 2,
        Edges = 1 << 3,
        All = Positions | Colors | Bones | Edges
    }
}
